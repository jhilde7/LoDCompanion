
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        HarvestParts,
        ThrowPotion,
        DragonBreath,
        Taunt,
        BreakDownDoor,
        StandUp,
        PickupWeapon
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
        private readonly PotionActivationService _potionActivation;
        private readonly LockService _lock;

        public event Func<Monster, List<GridPosition>, Task<bool>>? OnMonsterMovement;

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
            AlchemyService alchemyService,
            PotionActivationService potionActivation,
            LockService lockService)
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
            _potionActivation = potionActivation;
            _lock = lockService;
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
                        (resultMessage, apCost) = await StandardAttack(dungeon, character, (Character)primaryTarget, combatContext, weapon);                        
                    }
                    else
                    {
                        resultMessage = "Invalid target for attack.";
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
                case (Character, ActionType.Move):
                    if (primaryTarget is GridPosition && character.Position != null && character.Room != null)
                    {
                        (resultMessage, actionWasSuccessful) = await Move(character, (GridPosition)primaryTarget, dungeon, weapon is RangedWeapon ? (RangedWeapon)weapon : null);                        
                    }
                    else
                    {
                        resultMessage = "Invalid destination for move action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.OpenDoor):
                    if (primaryTarget is Door)
                    {
                        await _dungeonManager.InteractWithDoorAsync((Door)primaryTarget, character);
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
                        resultMessage = await _identification.IdentifyItemAsync(hero, itemToIdentify);
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
                        (resultMessage, apCost, actionWasSuccessful) = await CastSpell(primaryTarget, resultMessage, apCost, actionWasSuccessful, hero, spellToCast);
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
                            $"{hero.Name} activated {perkToUse.ToString()}" : $"{perkToUse.ToString()} activation was unsuccessful";
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
                    if (hero.Inventory.Backpack.Any(e => e != null && e.Name == "Alchemist Tool"))
                    {
                        var avaialbleCorpses = hero.Room.CorpsesInRoom?.Where(c => !c.HasBeenHarvested).ToList();
                        if (avaialbleCorpses != null && avaialbleCorpses.Any())
                        {
                            resultMessage = await HarvestPartsAsync(hero, avaialbleCorpses);
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
                case (Hero hero, ActionType.ThrowPotion):

                    if (primaryTarget is GridPosition position && secondaryTarget is Potion potion)
                    {
                        resultMessage = await ThrowPotionAsync(hero, position, potion, dungeon);
                    }
                    else
                    {
                        resultMessage = "Invalid target for ThrowPotion action.";
                        actionWasSuccessful = false;
                    }                    
                    break;
                case (Hero hero, ActionType.DragonBreath):
                    (resultMessage, actionWasSuccessful) = await DragonBreath(dungeon, resultMessage, actionWasSuccessful, hero);
                    break;
                case (Hero hero, ActionType.Taunt):
                    if (primaryTarget is Monster targetMonster && dungeon.HeroParty != null && hero.CurrentEnergy > 0)
                    {
                        (resultMessage, actionWasSuccessful) = await Taunt(dungeon.HeroParty.Heroes, hero, targetMonster);
                    }
                    else
                    {
                        resultMessage = "Invalid target for Taunt action.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.BreakDownDoor):
                    if (primaryTarget is Door)
                    {
                        var door = (Door)primaryTarget;
                        if (door.Lock.IsLocked && weapon is MeleeWeapon)
                        {
                            if (await _lock.BashLock(character, door.Lock, (MeleeWeapon)weapon)) 
                            { 
                                door.State = DoorState.BashedDown;
                            }
                        }
                        else
                        {
                            resultMessage = "Target door is not locked.";
                            resultMessage += await PerformActionAsync(dungeon, character, ActionType.OpenDoor, primaryTarget);
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Target is not a door to break down.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.StandUp):
                    if (character.CombatStance == CombatStance.Prone)
                    {
                        character.CombatStance = CombatStance.Normal;
                        resultMessage = "Character is now standing.";
                    }
                    else
                    {
                        resultMessage = "Character is already standing.";
                        actionWasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.PickupWeapon):
                    if (character.DroppedWeapon != null)
                    {
                        var enemies = GetEnemiesForZOC(character);
                        bool adjacentEnemy = false;
                        if (character.Position != null)
                        {
                            foreach (var enemy in enemies)
                            {
                                if (enemy.Position != null && GridService.IsAdjacent(character.Position, enemy.Position))
                                {
                                    adjacentEnemy = true;
                                    break;
                                }
                            }
                        }

                        if (adjacentEnemy)
                        {
                            bool pickedUpWeapon = false;
                            if (character is Monster)
                            {
                                Monster monster = (Monster)character;
                                var dexRoll = RandomHelper.RollDie(DiceType.D100);
                                if (dexRoll <= character.GetStat(BasicStat.Dexterity))
                                {
                                    monster.ActiveWeapon = monster.DroppedWeapon;
                                    pickedUpWeapon = true;
                                }
                                else
                                {
                                    pickedUpWeapon = false;
                                } 
                            }
                            else
                            {
                                Hero hero = (Hero)character;
                                var dexRoll = await _diceRoll.RequestRollAsync("Roll dexterity check.", "1d100", stat: (hero, BasicStat.Dexterity));
                                await Task.Yield();
                                if (dexRoll.Roll <= character.GetStat(BasicStat.Dexterity))
                                {
                                    if (hero.Inventory.EquippedWeapon == null)
                                    {
                                        hero.Inventory.EquippedWeapon = hero.DroppedWeapon; 
                                    }
                                    else
                                    {
                                        if (hero.DroppedWeapon != null)
                                        {
                                            BackpackHelper.AddItem(hero.Inventory.Backpack, hero.DroppedWeapon); 
                                        }
                                    }
                                    pickedUpWeapon = true;
                                }
                                else
                                {
                                    pickedUpWeapon = false;
                                }
                            }

                            if (pickedUpWeapon)
                            {
                                character.DroppedWeapon = null;
                                resultMessage = $"{character.Name} successfully picked up their weapon.";
                            }
                            else
                            {
                                resultMessage = $"{character.Name} failed to pick up their weapon.";
                            }
                        }
                        else
                        {
                            if (character is Monster)
                            {
                                Monster monster = (Monster)character;
                                monster.ActiveWeapon = character.DroppedWeapon; 
                            }
                            else
                            {
                                Hero hero = (Hero)character;
                                if (hero.Inventory.EquippedWeapon == null)
                                {
                                    hero.Inventory.EquippedWeapon = hero.DroppedWeapon;
                                }
                                else
                                {
                                    if (hero.DroppedWeapon != null)
                                    {
                                        BackpackHelper.AddItem(hero.Inventory.Backpack, hero.DroppedWeapon);
                                    }
                                }
                            }
                            character.DroppedWeapon = null;
                            resultMessage = $"{character.Name} picked up their weapon.";
                        }
                    }
                    else
                    {
                        resultMessage = "No weapon to pick up.";
                        actionWasSuccessful = false;
                    }
                    break;

            }


            if (actionWasSuccessful)
            {
                character.CurrentAP -= apCost;
                if(actionType != ActionType.PowerAttack) character.IsVulnerableAfterPowerAttack = false;
                if (character is Hero hero && hero.ProfessionName == "Wizard" && hero.CurrentAP <= 0) hero.CanCastSpell = true;
            }

            return $"{character.Name} performed {actionType}, {resultMessage}.";
        }

        private async Task<(string resultMessage, bool actionWasSuccessful)> Taunt(List<Hero> heroParty, Hero hero, Monster targetMonster)
        {
            string resultMessage = string.Empty;
            bool actionWasSuccessful = true;
            var taunt = hero.Perks.FirstOrDefault(p => p.Name == PerkName.Taunt);
            if (taunt == null)
            {
                resultMessage = $"{hero.Name} doesn't have the taunt ability.";
                actionWasSuccessful = false;
                return (resultMessage, actionWasSuccessful);
            }

            // Rule: "not locked in close combat"
            bool isAdjacentToAnyHero = heroParty.Any(h => h.Position != null && targetMonster.Position != null && GridService.IsAdjacent(targetMonster.Position, h.Position));

            if (isAdjacentToAnyHero)
            {
                resultMessage = $"{targetMonster.Name} is already locked in close combat and cannot be taunted.";
                actionWasSuccessful = false;
                return (resultMessage, actionWasSuccessful);
            }

            if (await _powerActivation.ActivatePerkAsync(hero, taunt))
            {
                var tauntEffect = taunt.ActiveStatusEffect ?? new ActiveStatusEffect(StatusEffectType.Taunt, 1);
                await StatusEffectService.AttemptToApplyStatusAsync(targetMonster, tauntEffect, _powerActivation);
                targetMonster.TauntedBy = hero;
                resultMessage = $"{hero.Name} taunts {targetMonster.Name}, forcing it to attack them!";
                return (resultMessage, actionWasSuccessful); 
            }
            else
            {
                resultMessage = $"{taunt.Name.ToString()} failed to activate";
                actionWasSuccessful = false;
                return (resultMessage, actionWasSuccessful);
            }
        }

        private async Task<(string resultMessage, bool actionWasSuccessful)> DragonBreath(DungeonState dungeon, string resultMessage, bool actionWasSuccessful, Hero hero)
        {
            var dragonBreathEffect = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.DragonBreath);
            if (dragonBreathEffect == null)
            {
                resultMessage = $"{hero.Name} has not consumed a Potion of Dragon's Breath.";
                actionWasSuccessful = false;
                return (resultMessage, actionWasSuccessful);
            }

            if (hero.Position == null)
            {
                resultMessage = "Cannot use Dragon Breath without a position.";
                actionWasSuccessful = false;
                return (resultMessage, actionWasSuccessful);
            }

            // Generate all possible attack options
            var adjacentSquares = GridService.GetNeighbors(hero.Position, dungeon.DungeonGrid)
                .Where(sq => !(dungeon.DungeonGrid.GetValueOrDefault(sq)?.MovementBlocked ?? true))
                .ToList();

            var spotAttackOptions = new List<GridPosition>(adjacentSquares);
            var jetAttackOptions = new List<(GridPosition, GridPosition)>();

            // Find all pairs of adjacent squares that are also adjacent to each other
            for (int i = 0; i < adjacentSquares.Count; i++)
            {
                for (int j = i + 1; j < adjacentSquares.Count; j++)
                {
                    if (GridService.IsAdjacent(adjacentSquares[i], adjacentSquares[j]))
                    {
                        jetAttackOptions.Add((adjacentSquares[i], adjacentSquares[j]));
                    }
                }
            }

            if (!spotAttackOptions.Any())
            {
                resultMessage = "There are no valid adjacent squares to attack.";
                actionWasSuccessful = false;
                return (resultMessage, actionWasSuccessful);
            }

            bool actionTaken = false;
            var outcome = new StringBuilder();

            // Present Jet Attack options to the player first
            foreach (var (pos1, pos2) in jetAttackOptions)
            {
                var occupants1 = string.Join(", ", dungeon.AllCharactersInDungeon.Where(c => c.Position != null && c.Position.Equals(pos1)).Select(c => c.Name));
                var occupants2 = string.Join(", ", dungeon.AllCharactersInDungeon.Where(c => c.Position != null && c.Position.Equals(pos2)).Select(c => c.Name));
                var prompt1 = !string.IsNullOrEmpty(occupants1) ? $" (hitting {occupants1})" : "";
                var prompt2 = !string.IsNullOrEmpty(occupants2) ? $" (hitting {occupants2})" : "";

                if (await _diceRoll.RequestYesNoChoiceAsync($"Perform a jet attack hitting squares {pos1}{prompt1} and {pos2}{prompt2}?"))
                {
                    outcome.AppendLine($"{hero.Name} breathes a jet of fire!");

                    var charactersOnPos1 = dungeon.AllCharactersInDungeon.Where(c => c.Position != null && c.Position.Equals(pos1)).ToList();
                    int damage1 = RandomHelper.RollDie(DiceType.D4);
                    foreach (var characterInTarget in charactersOnPos1)
                    {
                        await characterInTarget.TakeDamageAsync(damage1, (new FloatingTextService(), characterInTarget.Position), _powerActivation, damageType: DamageType.Fire);
                        outcome.AppendLine($"{characterInTarget.Name} is hit for {damage1} fire damage.");
                    }

                    var charactersOnPos2 = dungeon.AllCharactersInDungeon.Where(c => c.Position != null && c.Position.Equals(pos2)).ToList();
                    int damage2 = RandomHelper.RollDie(DiceType.D4);
                    foreach (var characterInTarget in charactersOnPos2)
                    {
                        await characterInTarget.TakeDamageAsync(damage2, (new FloatingTextService(), characterInTarget.Position), _powerActivation, damageType: DamageType.Fire);
                        outcome.AppendLine($"{characterInTarget.Name} is hit for {damage2} fire damage.");
                    }

                    actionTaken = true;
                    break;
                }
            }

            // If no jet attack was chosen, present Spot Attack options
            if (!actionTaken)
            {
                foreach (var spotOption in spotAttackOptions)
                {
                    var occupants = string.Join(", ", dungeon.AllCharactersInDungeon.Where(c => c.Position != null && c.Position.Equals(spotOption)).Select(c => c.Name));
                    var prompt = !string.IsNullOrEmpty(occupants) ? $" (hitting {occupants})" : "";

                    if (await _diceRoll.RequestYesNoChoiceAsync($"Perform a spot attack on square {spotOption}{prompt}?"))
                    {
                        outcome.AppendLine($"{hero.Name} breathes a gout of flame!");

                        var charactersOnPos = dungeon.AllCharactersInDungeon.Where(c => c.Position != null && c.Position.Equals(spotOption)).ToList();
                        int damage = RandomHelper.RollDie(DiceType.D8);
                        foreach (var characterInTarget in charactersOnPos)
                        {
                            await characterInTarget.TakeDamageAsync(damage, (new FloatingTextService(), characterInTarget.Position), _powerActivation, damageType: DamageType.Fire);
                            outcome.AppendLine($"{characterInTarget.Name} is hit for {damage} fire damage.");
                        }

                        actionTaken = true;
                        break;
                    }
                }
            }

            // Finalize the action based on player choice
            if (actionTaken)
            {
                hero.ActiveStatusEffects.Remove(dragonBreathEffect);
                resultMessage = outcome.ToString();
            }
            else
            {
                resultMessage = $"{hero.Name} decides not to use Dragon Breath.";
                actionWasSuccessful = false; // No AP is consumed if the action is cancelled
            }

            return (resultMessage, actionWasSuccessful);
        }

        private async Task<(string resultMessage, int apCost)> StandardAttack(DungeonState dungeon, Character character, Character target, CombatContext? combatContext, Weapon? weapon)
        {
            int startingAP = character.CurrentAP;
            int apCost = 1;
            string resultMessage = string.Empty;

            if (weapon != null && weapon is RangedWeapon)
            {
                resultMessage = await PerformActionAsync(dungeon, character, ActionType.Reload);
                if (character.CurrentAP <= 0)
                {
                    resultMessage += $"\n {character.Name} is reloading...";
                    return (resultMessage, 0);
                } 
            }

            if (character is Hero hero && weapon != null && hero.CurrentEnergy >= 1
                    && weapon is RangedWeapon bowSling && (bowSling.AmmoType == AmmoType.Arrow || bowSling.AmmoType == AmmoType.SlingStone))
            {
                if (await _powerActivation.RequestPerkActivationAsync(hero, PerkName.HuntersEye))
                {
                    await _attack.PerformStandardAttackAsync(character, weapon, target, dungeon, combatContext);
                            await PerformActionAsync(dungeon, character, ActionType.ReloadWhileMoving);
                }
            }

            AttackResult attackResult = await _attack.PerformStandardAttackAsync(character, weapon, target, dungeon, combatContext);
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

            return (resultMessage, apCost);
        }

        private async Task<(string resultMessage, bool actionWasSuccessful)> Move(
            Character character, GridPosition position, DungeonState dungeon, RangedWeapon? rangedWeapon = null)
        {
            string resultMessage = string.Empty;
            bool actionWasSuccessful = true;

            if (character.Position != null)
            {
                // Determine available movement points for this action
                int availableMovement = character.CurrentMovePoints;
                if (character.HasMadeFirstMoveAction) // Rule: Second move is half distance
                {
                    availableMovement /= 2;
                }

                var enemies = GetEnemiesForZOC(character);
                
                List<GridPosition> path = GridService.FindShortestPath(character.Position, position, dungeon.DungeonGrid, enemies);

                if (character is Monster movingMonster && OnMonsterMovement != null)
                {
                    if (await OnMonsterMovement.Invoke(movingMonster, path))
                    {
                        return ("Movement interrupted by Overwatch!", false);
                    }
                }

                MovementResult moveResult = GridService.MoveCharacter(character, path, dungeon.DungeonGrid, enemies, availableMovement);

                if (moveResult.WasSuccessful)
                {
                    character.SpendMovementPoints(moveResult.MovementPointsSpent); // A new method you'll add to Character
                    availableMovement = character.CurrentMovePoints;
                    resultMessage = moveResult.Message;

                    if (rangedWeapon != null)
                    {
                        if (!rangedWeapon.IsLoaded)
                        {
                            resultMessage += await PerformActionAsync(dungeon, character, ActionType.ReloadWhileMoving);
                        }
                    }

                    if (availableMovement <= 0)
                    {
                        character.HasMadeFirstMoveAction = true;
                        character.ResetMovementPoints();
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
                resultMessage += "Error in finding Hero Party";
                actionWasSuccessful = false; // Don't deduct AP if no move was made
            }

            return (resultMessage, actionWasSuccessful);
        }

        private List<Character> GetEnemiesForZOC(Character character)
        {
            var enemies = new List<Character> ();
            // Determine enemies for ZOC calculation
            if (_dungeonManager.Dungeon != null && _dungeonManager.Dungeon.HeroParty != null)
            {
                if (character is Monster)
                {
                    enemies = _dungeonManager.Dungeon.HeroParty.Heroes.Cast<Character>().ToList();
                    if (enemies.Count <= 0 && character.Room.HeroesInRoom != null)
                    {
                        enemies = character.Room.HeroesInRoom.Cast<Character>().ToList();
                    }
                }
                else if (character is Hero)
                {
                    enemies = _dungeonManager.Dungeon.RevealedMonsters.Cast<Character>().ToList();
                    if (enemies.Count <= 0 && character.Room.MonstersInRoom != null)
                    {
                        enemies = character.Room.MonstersInRoom.Cast<Character>().ToList();
                    }
                } 
            }

            return enemies;
        }

        private async Task<(string resultMessage, int apCost, bool actionWasSuccessful)> CastSpell(object? primaryTarget, string resultMessage, int apCost, bool actionWasSuccessful, Hero hero, Spell spellToCast)
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
                    resultMessage = spellCastResult.OutcomeMessage;
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
                            spellCastResult = await _spellResolution.ResolveSpellAsync(hero, spellToCast, primaryTarget, options);
                            resultMessage = spellCastResult.OutcomeMessage;
                        }
                    }
                }
            }

            return (resultMessage, apCost, actionWasSuccessful);
        }

        private async Task<string> HarvestPartsAsync(Hero hero, List<Corpse> avaialbleCorpses)
        {
            string resultMessage = string.Empty;
            avaialbleCorpses.Shuffle();
            var skillTarget = hero.GetSkill(Skill.Alchemy);
            var resultRoll = await _diceRoll.RequestRollAsync("Roll for alchemy skill test.", "1d100", skill: (hero, Skill.Alchemy));
            await Task.Yield();

            var equisiteRange = 10;

            if (await _powerActivation.RequestPerkActivationAsync(hero, PerkName.CarefulTouch))
            {
                equisiteRange = 20;
            }

            if (resultRoll.Roll <= skillTarget)
            {
                var parts = new List<Part>();
                for (int i = 0; i < Math.Min(avaialbleCorpses.Count, 3); i++)
                {
                    if (i == 0)
                    {
                        var part = (await _alchemy.GetPartsAsync(1, avaialbleCorpses[i].OriginMonster.Species))[0];
                        if (await _powerActivation.RequestPerkActivationAsync(hero, PerkName.Surgeon))
                        {
                            string choiceResult = await _diceRoll.RequestChoiceAsync("Choose part to harvest", Enum.GetNames(typeof(PartName)).ToList());
                            await Task.Yield();
                            Enum.TryParse(choiceResult, out PartName selectedName);
                            part.Name = selectedName;
                        }
                        if (resultRoll.Roll <= equisiteRange)
                        {
                            part.Exquisite = true;
                        }
                        parts.Add(part);
                    }
                    else
                    {
                        parts.AddRange(await _alchemy.GetPartsAsync(1, avaialbleCorpses[i].OriginMonster.Species));
                    }
                    avaialbleCorpses[i].HasBeenHarvested = true;
                }

                foreach (var part in parts)
                {
                    BackpackHelper.AddItem(hero.Inventory.Backpack, part);
                    resultMessage += $"{hero.Name} harvested {part.ToString()}";
                }
            }            
            else
            {
                resultMessage += $"{hero.Name} failed to harvest any parts.";
            }

            return resultMessage;
        }

        private async Task<string> ThrowPotionAsync(Hero hero, GridPosition position, Potion potion, DungeonState dungeon)
        {
            string resultMessage = string.Empty;
            var rsRoll = await _diceRoll.RequestRollAsync($"Roll ranged skill check", "1d100", skill: (hero, Skill.RangedSkill));
            await Task.Yield();

            var pitcherActive = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.Pitcher);
            if (pitcherActive == null)
            {
                await _powerActivation.RequestPerkActivationAsync(hero, PerkName.Pitcher);
            }

            int rsSkill = hero.GetSkill(Skill.RangedSkill);

            if (hero.Position != null)
            {
                var los = GridService.HasLineOfSight(hero.Position, position, dungeon.DungeonGrid);
                if (los.ObstructionPenalty < 0)
                {
                    rsSkill -= 10;
                }
            }

            // Check if throwing through a door
            var throwThroughDoor = hero.Room.Doors.FirstOrDefault(d => d.Position.Contains(position));
            if (throwThroughDoor != null && hero.Position != null)
            {
                bool isAdjacentToDoor = throwThroughDoor.Position.Any(p => GridService.IsAdjacent(hero.Position, p));
                if (!isAdjacentToDoor)
                {
                    rsSkill -= 10;
                }

                if (rsRoll.Roll > rsSkill)
                {
                    // Missed throw through a door, hits a square in front of the door
                    var doorSquares = throwThroughDoor.Position.ToList();
                    position = doorSquares[RandomHelper.GetRandomNumber(0, doorSquares.Count - 1)];
                    resultMessage = $"{hero.Name} misses! The potion hits the doorway at {position}.";
                }
            }
            else if (rsRoll.Roll > rsSkill)
            {
                var neighbors = GridService.GetNeighbors(position, dungeon.DungeonGrid).ToList();
                neighbors.Shuffle();
                position = neighbors.FirstOrDefault() ?? position;
                resultMessage = $"{hero.Name} misses! The potion lands at {position}.";
            }

            pitcherActive = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.Pitcher);
            if (pitcherActive != null)
            {
                hero.ActiveStatusEffects.Remove(pitcherActive);
            }

            resultMessage += await _potionActivation.BreakPotionAsync(hero, potion, position, dungeon);
            return resultMessage;
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
                ActionType.ThrowPotion => 1,
                ActionType.EquipGear => 1,
                ActionType.AddItemToQuickSlot => 1,
                ActionType.BreakDownDoor => 1,
                ActionType.StandUp => 1,
                ActionType.PowerAttack => 2,
                ActionType.ChargeAttack => 2,
                ActionType.PickLock => 2,
                ActionType.DisarmTrap => 2,
                ActionType.SearchRoom => 2,
                ActionType.HealSelf => 2,
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
