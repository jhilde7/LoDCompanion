
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using Microsoft.Extensions.Options;
using System.Collections.Frozen;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace LoDCompanion.BackEnd.Services.Player
{
    /// <summary>
    /// Defines the types of actions a player can take.
    /// </summary>
    public enum ActionType
    {
        StandardAttack,
        StandardAttackWhileAiming,
        Move,
        SearchRoom,
        SearchFurniture,
        SearchCorpse,
        OpenDoor,
        PickLock,
        DisarmTrap,
        HealSelf,
        HealOther,
        EquipGear,
        AddItemToQuickSlot,
        IdentifyItem,
        SetOverwatch,
        PowerAttack,
        ChargeAttack,
        Shove,
        CastSpell,
        EndTurn,
        Reload,
        ReloadWhileMoving,
        Aim,
        Parry,
        Pray,
        Focus,
        BreakFreeFromEntangle,
        Frenzy,
        UsePerk,
        ShieldBash,
        StunningStrike,
        HarvestParts
    }

    public class ActionInfo
    {
        public ActionType ActionType { get; set; }
        public int ApCost { get; set; }
        public object? Target { get; set; }
        public object? SecondaryTarget { get; set; } // For actions that may require a secondary target, like healing or attacking multiple targets
    }

    /// <summary>
    /// Handles the execution of actions performed by heroes.
    /// </summary>
    public class ActionService
    {
        private readonly DungeonManagerService _dungeonManager;
        private readonly SearchService _search;
        private readonly HealingService _healing;
        private readonly InventoryService _inventory;
        private readonly IdentificationService _identification;
        private readonly AttackService _attack;
        private readonly UserRequestService _diceRoll;
        private readonly SpellCastingService _spellCasting;
        private readonly SpellResolutionService _spellResolution;
        private readonly PowerActivationService _powerActivation;
        private readonly PartyManagerService _partyManager;
        private readonly AlchemyService _alchemy;

        public ActionService(
            DungeonManagerService dungeonManagerService,
            SearchService searchService,
            HealingService healingService,
            InventoryService inventoryService,
            IdentificationService identificationService,
            AttackService attackService,
            UserRequestService diceRollService,
            SpellCastingService spellCastingService,
            SpellResolutionService spellResolutionService,
            PowerActivationService powerActivationService,
            PartyManagerService partyManager,
            AlchemyService alchemyService)
        {
            _dungeonManager = dungeonManagerService;
            _search = searchService;
            _healing = healingService;
            _inventory = inventoryService;
            _identification = identificationService;
            _attack = attackService;
            _diceRoll = diceRollService;
            _spellCasting = spellCastingService;
            _spellResolution = spellResolutionService;
            _powerActivation = powerActivationService;
            _partyManager = partyManager;
            _alchemy = alchemyService;
        }

        /// <summary>
        /// Attempts to perform an action for a hero, checking and deducting AP.
        /// </summary>
        /// <param name="character">The hero performing the action.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <param name="target">The target of the action (e.g., a Monster, DoorChest, or another Hero).</param>
        /// <returns>True if the action was successfully performed, false otherwise.</returns>
        public async Task<string> PerformActionAsync(DungeonState dungeon, Character character, ActionType actionType, object? primaryTarget = null, object? secondaryTarget = null, CombatContext? combatContext = null)
        {
            string resultMessage = "";
            int startingAP = character.CurrentAP;
            int apCost = GetActionCost(actionType);
            if (character.CurrentAP < apCost)
            {
                resultMessage = $"{character.Name} does not have enough AP for {actionType}.";
                return resultMessage;
            }
            resultMessage = $"{character.Name} performed {actionType}.";
            bool actionWasSuccessful = true;

            Weapon? weapon = null;
            if (character is Hero h)
            {
                weapon = h.Inventory.EquippedWeapon;
            }
            else if (character is Monster m)
            {
                weapon = m.ActiveWeapon;
            }

            // This restricts the hero to only attacking or moving while in a frenzy.
            if (character.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Frenzy) &&
                actionType != ActionType.StandardAttack && actionType != ActionType.Move && actionType != ActionType.EndTurn)
            {
                return $"{character.Name} is in a frenzy and can only attack or move.";
            }

            // check to see if the character is in the middle of their move and is choosing a different action type.
            // This cancels the remaining move and sets as finishing their move.
            if ( actionType != ActionType.Move &&  character.CurrentMovePoints < character.GetStat(BasicStat.Move) && !character.HasMadeFirstMoveAction)
            {
                character.HasMadeFirstMoveAction = true;

                // Sprint is only in effect for the first move action
                var sprint = character.ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.Sprint);
                if (sprint != null)
                {
                    character.ActiveStatusEffects.Remove(sprint);
                }

                character.ResetMovementPoints(); // Reset movement points if first move action is made
                character.CurrentAP--; // Deduct 1 AP for finishing the move action
                if (character.CurrentAP <= 0)
                {
                    return $"{character.Name} has no AP left to perform {actionType}.";
                }
            }

            // Execute the action logic
            switch (character, actionType)
            {
                case (Character, ActionType.StandardAttack):
                    if (primaryTarget is Character)
                    {
                        resultMessage = await PerformActionAsync(dungeon, character, ActionType.Reload);
                        if (character.CurrentAP <= 0) break;
                        var hero = (Hero)character;
                        var huntersEye = hero.Perks.FirstOrDefault(p => p.Name == PerkName.HuntersEye);
                        if(huntersEye != null && weapon != null 
                            && weapon is RangedWeapon bowSling && (bowSling.AmmoType == AmmoType.Arrow || bowSling.AmmoType == AmmoType.SlingStone)
                            && hero.CurrentEnergy >= 1)
                        {
                            if (await _diceRoll.RequestYesNoChoiceAsync($"Does {hero.Name} want to use {huntersEye.Name.ToString()} against {((Character)primaryTarget).Name}?"))
                            {
                                await Task.Yield();
                                if (await _powerActivation.ActivatePerkAsync(hero, huntersEye))
                                {
                                    await _attack.PerformStandardAttackAsync(character, weapon, (Character)primaryTarget, dungeon, combatContext);
                                    await PerformActionAsync(dungeon, character, ActionType.ReloadWhileMoving);
                                }
                            }
                            await Task.Yield();
                        }

                        AttackResult attackResult = await _attack.PerformStandardAttackAsync(character, weapon, (Character)primaryTarget, dungeon, combatContext);
                        if (startingAP > character.CurrentAP)
                        {
                            resultMessage += "\n" + attackResult.OutcomeMessage;
                        }
                        else
                        {
                            resultMessage = attackResult.OutcomeMessage;
                        }

                        if (attackResult.IsHit && character.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Frenzy))
                        {
                            apCost = 0;
                            resultMessage += $"\n {character.Name} is in a frenzy and can act again";
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid target or no weapon equipped for attack.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.PowerAttack):
                    if ((character.CurrentAP >= GetActionCost(actionType) 
                        || (character.ActiveStatusEffects.Any(a => a.Category == StatusEffectType.BattleFury) && character.CurrentAP > 0)
                        ) && primaryTarget is Character)
                    {
                        AttackResult attackResult = await _attack.PerformPowerAttackAsync(character, weapon, (Character)primaryTarget, dungeon);
                        character.IsVulnerableAfterPowerAttack = true; // Set the vulnerability flag
                        resultMessage = attackResult.OutcomeMessage;

                        if (character.ActiveStatusEffects.Any(a => a.Category == StatusEffectType.BattleFury))
                        {
                            apCost = 1; // Battle Fury reduces the AP cost of Power Attacks to 1
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.ChargeAttack):
                    if (character.Position != null && character.CurrentAP >= GetActionCost(actionType) && primaryTarget is Character)
                    {
                        AttackResult attackResult = await _attack.PerformChargeAttackAsync(character, weapon, (Character)primaryTarget, dungeon);
                        resultMessage = attackResult.OutcomeMessage;
                        Room? room = _dungeonManager.FindRoomAtPosition(character.Position);
                        if (room != null)
                        {
                            character.Room = room;
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.Shove):
                    if (primaryTarget is Character targetToShove && character.Position != null)
                    {
                        AttackResult attackResult = await _attack.PerformShoveAsync(character, targetToShove, dungeon);
                        resultMessage = attackResult.OutcomeMessage;
                        if (attackResult.IsHit && targetToShove.Position != null)
                        {
                            Room? room = _dungeonManager.FindRoomAtPosition(targetToShove.Position);
                            if (room != null)
                            {
                                targetToShove.Room = room;
                            }
                        }
                        else
                        {
                            actionWasSuccessful = false;
                        }

                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero, ActionType.Move):
                    if (primaryTarget is GridPosition && character.Position != null && character.Room != null)
                    {
                        // Determine available movement points for this action
                        int availableMovement = character.CurrentMovePoints;
                        if (character.HasMadeFirstMoveAction) // Rule: Second move is half distance
                        {
                            availableMovement /= 2;
                        }

                        if (_dungeonManager.DungeonState != null)
                        {
                            // Determine enemies for ZOC calculation
                            var enemies = _dungeonManager.DungeonState.RevealedMonsters.Cast<Character>().ToList();
                            if (enemies.Count <= 0 && character.Room.MonstersInRoom != null)
                            {
                                enemies = character.Room.MonstersInRoom.Cast<Character>().ToList();
                            }
                            List<GridPosition> path = GridService.FindShortestPath(character.Position, (GridPosition)primaryTarget, dungeon.DungeonGrid, enemies);

                            MovementResult moveResult = GridService.MoveCharacter(character, path, dungeon.DungeonGrid, enemies, availableMovement);

                            if (moveResult.WasSuccessful)
                            {
                                character.SpendMovementPoints(moveResult.MovementPointsSpent); // A new method you'll add to Character
                                availableMovement = character.CurrentMovePoints;
                                resultMessage = moveResult.Message;
                                if (availableMovement <= 0)
                                {
                                    character.HasMadeFirstMoveAction = true;
                                    character.ResetMovementPoints();
                                }
                                else
                                {
                                    resultMessage = moveResult.Message;
                                    actionWasSuccessful = false; // Don't deduct AP if movement points remain
                                }
                            }
                            else
                            {
                                resultMessage = moveResult.Message;
                                actionWasSuccessful = false; // Don't deduct AP if no move was made
                            }
                        }
                        else
                        {
                            actionWasSuccessful = false; // Don't deduct AP if no move was made
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid destination for move action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Monster, ActionType.Move):
                    if (primaryTarget is GridPosition && character.Position != null && character.Room != null)
                    {
                        // Determine available movement points for this action
                        int availableMovement = character.CurrentMovePoints;
                        if (character.HasMadeFirstMoveAction) // Rule: Second move is half distance
                        {
                            availableMovement /= 2;
                        }

                        if (_dungeonManager.DungeonState != null && _dungeonManager.DungeonState.HeroParty != null)
                        {
                            // Determine enemies for ZOC calculation
                            var enemies = _dungeonManager.DungeonState.HeroParty.Heroes.Cast<Character>().ToList();
                            if (enemies.Count <= 0 && character.Room.HeroesInRoom != null)
                            {
                                enemies = character.Room.HeroesInRoom.Cast<Character>().ToList();
                            }
                            List<GridPosition> path = GridService.FindShortestPath(character.Position, (GridPosition)primaryTarget, dungeon.DungeonGrid, enemies);

                            MovementResult moveResult = GridService.MoveCharacter(character, path, dungeon.DungeonGrid, enemies, availableMovement);

                            if (moveResult.WasSuccessful)
                            {
                                character.SpendMovementPoints(moveResult.MovementPointsSpent); // A new method you'll add to Character
                                availableMovement = character.CurrentMovePoints;
                                resultMessage = moveResult.Message;
                                if (availableMovement <= 0)
                                {
                                    character.HasMadeFirstMoveAction = true;
                                    character.ResetMovementPoints();
                                }
                                else
                                {
                                    resultMessage = moveResult.Message;
                                    actionWasSuccessful = false; // Don't deduct AP if movement points remain
                                }
                            }
                            else
                            {
                                resultMessage = moveResult.Message;
                                actionWasSuccessful = false; // Don't deduct AP if no move was made
                            }
                        }
                        else
                        {
                            actionWasSuccessful = false; // Don't deduct AP if no move was made
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid destination for move action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.OpenDoor):
                    if (primaryTarget is Door door)
                    {
                        await _dungeonManager.InteractWithDoorAsync(door, character);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.HealSelf):
                    resultMessage = await _healing.ApplyBandageAsync(hero, hero, _diceRoll, _powerActivation);
                    break;
                case (Hero hero, ActionType.HealOther):
                    if (primaryTarget is Hero targetHero)
                    {
                        resultMessage = await _healing.ApplyBandageAsync(hero, targetHero, _diceRoll, _powerActivation);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.EquipGear):
                    if (primaryTarget is Equipment item)
                    {
                        if (_inventory.EquipItem(hero, item)) resultMessage = $"{item.Name} was equipped";
                        else
                        {
                            resultMessage = $"{item.Name} could not be equipped";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.AddItemToQuickSlot):
                    if (primaryTarget is Equipment item1)
                    {
                        if (_inventory.EquipItem(hero, item1)) resultMessage = $"{item1.Name} was equipped";
                        else
                        {
                            resultMessage = $"{item1.Name} could not be equipped";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.IdentifyItem):
                    if (primaryTarget is Equipment itemToIdentify)
                    {
                        resultMessage = _identification.IdentifyItem(hero, itemToIdentify);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.SetOverwatch):
                    var equippedWeapon = weapon;
                    if (equippedWeapon == null) return $"{character.Name} does not have a weapon equipped";
                    if (equippedWeapon is RangedWeapon ranged && !ranged.IsLoaded) return $"{hero.Name} needs to reload their weapon";
                    character.CombatStance = CombatStance.Overwatch;
                    apCost = hero.CurrentAP;
                    resultMessage = $"{character.Name} takes an Overwatch stance, ready to react.";
                    break;
                case (Hero hero, ActionType.EndTurn):
                    resultMessage = $"{hero.Name} ends their turn.";
                    apCost = hero.CurrentAP;
                    break;
                case (Character, ActionType.Parry):
                    character.CombatStance = CombatStance.Parry;
                    apCost = character.CurrentAP;
                    resultMessage = $"{character.Name} entered parry stance";
                    break;
                case (Character, ActionType.Aim):
                    character.CombatStance = CombatStance.Aiming;
                    resultMessage = $"{character.Name} takes careful aim.";
                    break;
                case (Character, ActionType.Reload):
                    if (weapon is RangedWeapon rangedWeapon)
                    {
                        if (!rangedWeapon.IsLoaded)
                        {
                            rangedWeapon.reloadAmmo();
                            if (character is Monster) rangedWeapon.IsLoaded = true;
                            resultMessage = $"{character.Name} spends a moment to reload their {rangedWeapon.Name}.";
                            apCost = rangedWeapon.ReloadTime;
                        }
                        else
                        {
                            resultMessage = $"{character.Name} weapon is already loaded";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = $"{character.Name} does not have a ranged weapon equipped";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.ReloadWhileMoving):
                    if (weapon is RangedWeapon rangedWeapon1 && !rangedWeapon1.IsLoaded)
                    {
                        rangedWeapon1.reloadAmmo();
                        rangedWeapon1.IsLoaded = true;
                        resultMessage = $" and reloads their {rangedWeapon1.Name}.";
                    }
                    else
                    {
                        resultMessage = string.Empty;
                    }
                    break;
                case (Hero hero, ActionType.CastSpell):
                    if (secondaryTarget is Spell spellToCast)
                    {
                        SpellCastingResult options = await _spellCasting.RequestCastingOptionsAsync(hero, spellToCast); await Task.Yield();

                        if (options.WasCancelled)
                        {
                            resultMessage = $"{hero.Name} decided not to cast the spell.";
                            actionWasSuccessful = false;
                        }
                        else
                        {
                            SpellCastResult spellCastResult = await spellToCast.CastSpellAsync(hero, _diceRoll, _powerActivation, options.FocusPoints, options.PowerLevels, 
                                monster: (primaryTarget is Monster) ? (Monster)primaryTarget : null);
                            resultMessage = spellCastResult.OutcomeMessage;

                            if (spellCastResult.ManaSpent <= 0)
                            {
                                actionWasSuccessful = false;
                            }
                            else
                            {
                                if (primaryTarget != null)
                                {
                                    if (options.FocusPoints <= 0)
                                    {
                                        if (spellToCast.Properties != null && spellToCast.Properties.ContainsKey(SpellProperty.QuickSpell))
                                        {
                                            apCost = 1; // Quick spells cost 1 AP if there is no focus points added
                                        }
                                        else
                                        {
                                            apCost = 2; // Regular spells cost 2 AP if there is no focus points added
                                        }
                                        await _spellResolution.ResolveSpellAsync(hero, spellToCast, primaryTarget, options);
                                    }
                                    else if (options.FocusPoints >= 1)
                                    {
                                        hero.ChanneledSpell = new ChanneledSpell(hero, spellToCast, primaryTarget, options);
                                        if (spellToCast.Properties != null && spellToCast.Properties.ContainsKey(SpellProperty.QuickSpell))
                                        {
                                            apCost = 2;
                                            hero.ChanneledSpell.FocusActionsRemaining--; // Deduct focus action if used
                                        }
                                    }

                                    if (hero.ChanneledSpell != null && hero.ChanneledSpell.FocusActionsRemaining <= 0)
                                    {
                                        await _spellResolution.ResolveSpellAsync(hero, spellToCast, primaryTarget, options);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid target for CastSpell action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.Focus):
                    if (hero.ChanneledSpell != null)
                    {
                        apCost = 0;
                        for (int i = 0; i < hero.CurrentAP; i++)
                        {
                            if (hero.ChanneledSpell.FocusActionsRemaining > 0)
                            {
                                hero.ChanneledSpell.FocusActionsRemaining--;
                                apCost++;
                                resultMessage = $"{hero.Name} focuses their mind on casting.";

                                if (hero.ChanneledSpell.FocusActionsRemaining <= 0)
                                {
                                    await _spellResolution.ResolveSpellAsync(hero, hero.ChanneledSpell.Spell, hero.ChanneledSpell.Target, hero.ChanneledSpell.CastingOptions);

                                }
                            }
                            else
                            {
                                await _spellResolution.ResolveSpellAsync(hero, hero.ChanneledSpell.Spell, hero.ChanneledSpell.Target, hero.ChanneledSpell.CastingOptions);
                                resultMessage = $"{hero.Name} has no focus actions remaining.";
                                actionWasSuccessful = false;
                            }
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.BreakFreeFromEntangle):
                    if (hero.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Entangled))
                    {
                        var entangledEffect = hero.ActiveStatusEffects.First(e => e.Category == StatusEffectType.Entangled);
                        int strengthTestModifier = -10 * (-entangledEffect.Duration - 1); // -0 on turn 1, -10 on turn 2, etc.

                        // Perform a strength test
                        int strengthRoll = RandomHelper.RollDie(DiceType.D100);
                        if (strengthRoll <= hero.GetStat(BasicStat.Strength) + strengthTestModifier)
                        {
                            hero.ActiveStatusEffects.Remove(entangledEffect);
                            resultMessage = $"{hero.Name} breaks free from the entanglement!";
                        }
                        else
                        {
                            resultMessage = $"{hero.Name} fails to break free.";
                        }
                        apCost = 1; // Breaking free costs 1 AP
                    }
                    else
                    {
                        resultMessage = "There is nothing to break free from.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.Pray):
                    if (secondaryTarget is Prayer prayerToCast
                        && hero.ActiveStatusEffects.Any(a => a.Category == (StatusEffectType)Enum.Parse(typeof(StatusEffectType), prayerToCast.Name.ToString())))
                    {
                        resultMessage = await _powerActivation.ActivatePrayerAsync(hero, prayerToCast, (Character?)primaryTarget);
                    }
                    else
                    {
                        resultMessage = "Invalid target for Pray action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.UsePerk):
                    if (secondaryTarget is Perk perkToUse
                        && hero.ActiveStatusEffects.Any(a => a.Category == (StatusEffectType)Enum.Parse(typeof(StatusEffectType), perkToUse.Name.ToString())))
                    {

                        if (perkToUse.Name == PerkName.Frenzy)
                        {
                            if (hero.CurrentAP <= 0)
                            {
                                resultMessage = "Not enough AP to activate this perk";
                                actionWasSuccessful = false;
                                break;
                            }
                            apCost = 1;
                        }
                        resultMessage = await _powerActivation.ActivatePerkAsync(hero, perkToUse, (Character?)primaryTarget) ? 
                            $"{hero.Name} activated {perkToUse.Name.ToString()}" : $"{perkToUse.Name.ToString()} activation was unsuccessful";
                    }
                    else
                    {
                        resultMessage = "Invalid target for UsePerk action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.ShieldBash):
                    if (primaryTarget is Character targetToBash && character.Position != null && hero.CurrentEnergy > 0
                        && hero.Inventory.OffHand != null && hero.Inventory.OffHand is Shield && ((Shield)hero.Inventory.OffHand).WeaponClass > 1)
                    {
                        AttackResult attackResult = await _attack.PerformShoveAsync(character, targetToBash, dungeon, isShieldBash: true);
                        resultMessage = attackResult.OutcomeMessage;
                        if (attackResult.IsHit && targetToBash.Position != null)
                        {
                            Room? room = _dungeonManager.FindRoomAtPosition(targetToBash.Position);
                            if (room != null)
                            {
                                targetToBash.Room = room;
                            }
                            hero.CurrentEnergy--;
                        }
                        else
                        {
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Cannot perform Shield Bash. Conditions not met (not enough energy, no suitable shield, or invalid target).";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.StunningStrike):
                    if (primaryTarget is Character && character.Position != null && hero.CurrentEnergy > 0 && weapon is MeleeWeapon)
                    {
                        var result = await _attack.PerformStunningStrikeAsync(hero, weapon, (Monster)primaryTarget, new CombatContext());
                        resultMessage = result.OutcomeMessage;
                    }
                    else
                    {
                        resultMessage = "Cannot perform action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.HarvestParts):
                    if (hero.Inventory.Backpack.Any(e => e.Name == "Alchemist Tool"))
                    {
                        var avaialbleCorpses = hero.Room.CorpsesInRoom?.Where(c => !c.HasBeenHarvested).ToList();
                        if (avaialbleCorpses != null && avaialbleCorpses.Any())
                        {
                            avaialbleCorpses.Shuffle();
                            var skillTarget = hero.GetSkill(Skill.Alchemy);
                            var resultRoll = await _diceRoll.RequestRollAsync("Roll for alchemy skill test.", "1d100", skill: Skill.Alchemy);
                            await Task.Yield();

                            var equisiteRange = 10;

                            var carefulTouch = hero.Perks.FirstOrDefault(p => p.Name == PerkName.CarefulTouch);
                            if (carefulTouch != null && hero.CurrentEnergy > 0)
                            {
                                var choiceResult = await _diceRoll.RequestYesNoChoiceAsync($"Does {hero.Name} wish to use their {carefulTouch.Name.ToString()}");
                                await Task.Yield();
                                if(choiceResult)
                                {
                                    if (await _powerActivation.ActivatePerkAsync(hero, carefulTouch)) equisiteRange = 20;
                                }
                            }

                            if (resultRoll.Roll <= skillTarget)
                            {
                                var parts = new List<Part>();
                                for (int i = 0; i < Math.Min(avaialbleCorpses.Count, 3); i++)
                                {
                                    if(i == 0 && resultRoll.Roll <= equisiteRange)
                                    {
                                        var part = (await _alchemy.GetPartsAsync(1, avaialbleCorpses[i].OriginMonster.Species))[0];
                                        part.Exquisite = true;
                                        parts.Add(part);
                                    }
                                    else
                                    {
                                        parts.AddRange(await _alchemy.GetPartsAsync(1, avaialbleCorpses[i].OriginMonster.Species));
                                    }
                                    avaialbleCorpses[i].HasBeenHarvested = true;
                                }

                                foreach(var part in parts)
                                {
                                    BackpackHelper.AddItem(hero.Inventory.Backpack, part);
                                }
                            }
                        }
                        else
                        {
                            resultMessage = "Cannot perform action as there are no harvestable corpses in this room.";
                            actionWasSuccessful = false;
                            break;
                        }
                    }
                    else
                    {
                        resultMessage = "Cannot harvest parts wihtout the appropriate equipment: Alchemist Tool";
                        actionWasSuccessful = false;
                    }
                    break;
            }


            if (actionWasSuccessful)
            {
                character.CurrentAP -= apCost;
                if(actionType != ActionType.PowerAttack) character.IsVulnerableAfterPowerAttack = false;
                if (character is Hero hero && hero.ProfessionName == "Wizard") hero.CanCastSpell = true;
            }

            return $"{character.Name} performed {actionType}, {resultMessage}.";
        }

        /// <summary>
        /// Gets the AP cost for a specific action type.
        /// </summary>
        public int GetActionCost(ActionType actionType, object? context = null)
        {
            return actionType switch
            {
                ActionType.StandardAttack => 1,
                ActionType.Shove => 1,
                ActionType.Move => 1,
                ActionType.OpenDoor => 1,
                ActionType.SearchFurniture => 1,
                ActionType.SearchCorpse => 1,
                ActionType.HealOther => 1,
                ActionType.Aim => 1,
                ActionType.HarvestParts => 1,
                ActionType.PowerAttack => 2,
                ActionType.ChargeAttack => 2,
                ActionType.PickLock => 2,
                ActionType.DisarmTrap => 2,
                ActionType.SearchRoom => 2,
                ActionType.HealSelf => 2,
                ActionType.EquipGear => 2,
                ActionType.AddItemToQuickSlot => 2,
                ActionType.IdentifyItem => 0,
                ActionType.ReloadWhileMoving => 0,
                ActionType.Pray => 0,
                ActionType.UsePerk => 0,
                ActionType.ShieldBash => 0,
                ActionType.StunningStrike => 0,
                _ => 1,
            };
        }
    }
}
