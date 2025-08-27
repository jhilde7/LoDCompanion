
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Text;

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
        RearrangeGear,
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
        PickupWeapon,
        AssistAllyOutOfPit,
        DrinkFromFurniture,
        SearchRoomWithParty,
        PullLever,
        OpenPortcullis,
        RemoveCobwebs,
    }

    public class ActionInfo
    {
        public ActionType ActionType { get; set; }
        public int ApCost { get; set; }
        public object? Target { get; set; }
        public object? SecondaryTarget { get; set; } // For actions that may require a secondary target, like healing or attacking multiple targets
    }

    public class ActionResult
    {
        public string Message { get; set; } = string.Empty;
        public bool WasSuccessful { get; set; } = true;
        public int ApCost { get; set; }
        public SearchResult? SearchResult { get; set; }
        public LeverResult? LeverResult { get; set; }
        public AttackResult? AttackResult { get; set; }
        public HealResult? HealResult { get; set; }
        public SpellCastResult? SpellResult { get; internal set; }
    }

    /// <summary>
    /// Handles the execution of actions performed by heroes.
    /// </summary>
    public class ActionService
    {
        private readonly HealingService _healing;
        private readonly InventoryService _inventory;
        private readonly IdentificationService _identification;
        private readonly AttackService _attack;
        private readonly UserRequestService _diceRoll;
        private readonly SpellCastingService _spellCasting;
        private readonly SpellResolutionService _spellResolution;
        private readonly PowerActivationService _powerActivation;
        private readonly AlchemyService _alchemy;
        private readonly PotionActivationService _potionActivation;
        private readonly LockService _lock;
        private readonly TrapService _trap;
        private readonly SearchService _search;
        private readonly ThreatService _threat;

        public event Func<Monster, List<GridPosition>, Task<bool>>? OnMonsterMovement;
        public event Func<Door, Task<bool>>? OnOpenDoor;
        public event Func<Hero, Door, Task<bool>>? OnRemoveCobwebs;

        public ActionService(
            SearchService searchService,
            HealingService healingService,
            InventoryService inventoryService,
            IdentificationService identificationService,
            AttackService attackService,
            UserRequestService diceRollService,
            SpellCastingService spellCastingService,
            SpellResolutionService spellResolutionService,
            PowerActivationService powerActivationService,
            AlchemyService alchemyService,
            PotionActivationService potionActivation,
            LockService lockService,
            TrapService trapService,
            ThreatService threatService)
        {
            _healing = healingService;
            _inventory = inventoryService;
            _identification = identificationService;
            _attack = attackService;
            _diceRoll = diceRollService;
            _spellCasting = spellCastingService;
            _spellResolution = spellResolutionService;
            _powerActivation = powerActivationService;
            _alchemy = alchemyService;
            _potionActivation = potionActivation;
            _lock = lockService;
            _trap = trapService;
            _search = searchService;
            _threat = threatService;
        }

        /// <summary>
        /// Attempts to perform an action for a hero, checking and deducting AP.
        /// </summary>
        /// <param name="character">The hero performing the action.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <param name="target">The target of the action (e.g., a Monster, DoorChest, or another Hero).</param>
        /// <returns>True if the action was successfully performed, false otherwise.</returns>
        public async Task<ActionResult> PerformActionAsync(DungeonState dungeon, Character character, ActionType actionType, object? primaryTarget = null, object? secondaryTarget = null, CombatContext? combatContext = null)
        {
            ActionResult result = new ActionResult();
            result.ApCost = GetActionCost(actionType);
            if (character.CurrentAP < result.ApCost)
            {
                result.Message = $"{character.Name} does not have enough AP for {actionType}.";
                return result;
            }
            result.Message = $"{character.Name} performed {actionType}.";
            result.WasSuccessful = true;

            Weapon? weapon = null;
            if (character is Hero h)
            {
                weapon = h.Inventory.EquippedWeapon;
            }
            else if (character is Monster m)
            {
                weapon = m.ActiveWeapon;
            }

            if (primaryTarget is Door)
            {
                ((Door)primaryTarget).OnTrapTriggered += HandleDoorTrapTrigger;
            }

            // This restricts the hero to only attacking or moving while in a frenzy.
            if (character.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Frenzy) &&
                actionType != ActionType.StandardAttack && actionType != ActionType.Move && actionType != ActionType.EndTurn)
            {
                result.Message = $"{character.Name} is in a frenzy and can only attack or move.";
                return result;
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
                    result.Message = $"{character.Name} has no AP left to perform {actionType}.";
                    return result;
                }
            }

            // Execute the action logic
            switch (character, actionType)
            {
                case (Character, ActionType.StandardAttack):
                    if (primaryTarget is Character)
                    {
                        result = await StandardAttack(dungeon, character, (Character)primaryTarget, combatContext, weapon);                        
                    }
                    else
                    {
                        result.Message = "Invalid target for attack.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.PowerAttack):
                    if ((character.CurrentAP >= GetActionCost(actionType) 
                        || (character.ActiveStatusEffects.Any(a => a.Category == StatusEffectType.BattleFury) && character.CurrentAP > 0)
                        ) && primaryTarget is Character)
                    {
                        result.AttackResult = await _attack.PerformPowerAttackAsync(character, weapon, (Character)primaryTarget, dungeon);
                        character.IsVulnerableAfterPowerAttack = true; // Set the vulnerability flag

                        if (character.ActiveStatusEffects.Any(a => a.Category == StatusEffectType.BattleFury))
                        {
                            result.ApCost = 1; // Battle Fury reduces the AP cost of Power Attacks to 1
                        }
                    }
                    else
                    {
                        result.Message = "Action was unsuccessful";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.ChargeAttack):
                    if (character.Position != null && character.CurrentAP >= GetActionCost(actionType) && primaryTarget is Character)
                    {
                        result.AttackResult = await _attack.PerformChargeAttackAsync(character, weapon, (Character)primaryTarget, dungeon);
                        Room? room = dungeon.FindRoomAtPosition(character.Position);
                        if (room != null)
                        {
                            character.Room = room;
                        }
                    }
                    else
                    {
                        result.Message = "Action was unsuccessful";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.Shove):
                    if (primaryTarget is Character targetToShove && character.Position != null)
                    {
                        result.AttackResult = await _attack.PerformShoveAsync(character, targetToShove, dungeon);
                        if (result.AttackResult.IsHit && targetToShove.Position != null)
                        {
                            Room? room = dungeon.FindRoomAtPosition(targetToShove.Position);
                            if (room != null)
                            {
                                targetToShove.Room = room;
                            }
                        }
                        else
                        {
                            result.WasSuccessful = false;
                        }

                    }
                    else
                    {
                        result.Message = "Action was unsuccessful";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.Move):
                    if (primaryTarget is GridPosition && character.Position != null && character.Room != null)
                    {
                        result = await Move(character, (GridPosition)primaryTarget, dungeon, weapon is RangedWeapon ? (RangedWeapon)weapon : null);                        
                    }
                    else
                    {
                        result.Message = "Invalid destination for move action.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.PickLock):
                    var pickLockResult = new ActionResult();
                    if (primaryTarget is Door lockedDoor && lockedDoor.IsLocked)
                    {
                        pickLockResult = await PickLock(lockedDoor.Lock, hero);
                        if (pickLockResult.WasSuccessful)
                        {
                            result.ApCost = 1;
                            await PerformActionAsync(dungeon, hero, ActionType.OpenDoor, lockedDoor);
                        }
                    }
                    else if (primaryTarget is Chest lockedChest && lockedChest.IsLocked)
                    {
                        pickLockResult = await PickLock(lockedChest.Lock, hero);
                        if (pickLockResult.WasSuccessful)
                        {
                            result.ApCost = 1;
                            await PerformActionAsync(dungeon, hero, ActionType.SearchFurniture, lockedChest);
                        }
                    }
                    else
                    {
                        result.Message = "Target is not locked.";
                        result.WasSuccessful = false;
                        break;
                    }
                    result.Message = pickLockResult.Message;
                    break;
                case (Character, ActionType.OpenDoor):
                    if (primaryTarget is Door)
                    {
                        result = await OpenDoor((Door)primaryTarget, character);
                    }
                    else
                    {
                        result.Message = "Target is not a door to open.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.OpenPortcullis):
                    if (primaryTarget is Door)
                    {
                        result = await OpenPortcullis((Door)primaryTarget, hero);
                    }
                    else
                    {
                        result.Message = "Target is not a doorway.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.BreakDownDoor):
                    if (primaryTarget is Door)
                    {
                        result = await BreakDownDoorAsync(dungeon, hero, (Door)primaryTarget, weapon);
                    }
                    else
                    {
                        result.Message = "Target is not a door to break down.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.RemoveCobwebs):
                    if (primaryTarget is Door)
                    {
                        result = await RemoveCobwebsAsync(hero, (Door)primaryTarget);
                    }
                    else
                    {
                        result.Message = "Target is not a door to break down.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.HealSelf):
                    result.HealResult = await _healing.ApplyBandageAsync(hero, hero, _powerActivation);
                    break;
                case (Hero hero, ActionType.HealOther):
                    if (primaryTarget is Hero targetHero)
                    {
                        result.HealResult = await _healing.ApplyBandageAsync(hero, targetHero, _powerActivation);
                    }
                    else
                    {
                        result.Message = "Action was unsuccessful";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.RearrangeGear):
                    if (primaryTarget is Equipment item)
                    {
                        if (await _inventory.EquipItemAsync(hero, item)) result.Message = $"{item.Name} was equipped";
                        else
                        {
                            result.Message = $"{item.Name} could not be equipped";
                            result.WasSuccessful = false;
                        }
                    }
                    else
                    {
                        result.Message = "Action was unsuccessful";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.IdentifyItem):
                    if (primaryTarget is Equipment itemToIdentify)
                    {
                        result.Message = await _identification.IdentifyItemAsync(hero, itemToIdentify);
                    }
                    else
                    {
                        result.Message = "Action was unsuccessful";
                        result.WasSuccessful = false;
                    }
                    break;

                case (Hero hero, ActionType.SetOverwatch):
                    var equippedWeapon = weapon;
                    if (equippedWeapon == null) { result.Message = $"{character.Name} does not have a weapon equipped"; return result; }
                    if (equippedWeapon is RangedWeapon ranged && !ranged.IsLoaded) { result.Message = $"{hero.Name} needs to reload their weapon"; return result; }
                    character.CombatStance = CombatStance.Overwatch;
                    result.ApCost = hero.CurrentAP;
                    result.Message = $"{character.Name} takes an Overwatch stance, ready to react.";
                    break;

                case (Hero hero, ActionType.EndTurn):
                    result.Message = $"{hero.Name} ends their turn.";
                    result.ApCost = hero.CurrentAP;
                    break;

                case (Character, ActionType.Parry):
                    character.CombatStance = CombatStance.Parry;
                    result.ApCost = character.CurrentAP;
                    result.Message = $"{character.Name} entered parry stance";
                    break;

                case (Character, ActionType.Aim):
                    character.CombatStance = CombatStance.Aiming;
                    result.Message = $"{character.Name} takes careful aim.";
                    break;

                case (Character, ActionType.Reload):
                    if (weapon is RangedWeapon rangedWeapon)
                    {
                        if (!rangedWeapon.IsLoaded)
                        {
                            rangedWeapon.reloadAmmo();
                            if (character is Monster) rangedWeapon.IsLoaded = true;
                            result.Message = $"{character.Name} spends a moment to reload their {rangedWeapon.Name}.";
                            result.ApCost = rangedWeapon.ReloadTime;
                        }
                        else
                        {
                            result.Message = $"{character.Name} weapon is already loaded";
                            result.WasSuccessful = false;
                        }
                    }
                    else
                    {
                        result.Message = $"{character.Name} does not have a ranged weapon equipped";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.ReloadWhileMoving):
                    if (weapon is RangedWeapon rangedWeapon1 && !rangedWeapon1.IsLoaded)
                    {
                        rangedWeapon1.reloadAmmo();
                        rangedWeapon1.IsLoaded = true;
                        result.Message = $" and reloads their {rangedWeapon1.Name}.";
                    }
                    else
                    {
                        result.Message = string.Empty;
                    }
                    break;
                case (Hero hero, ActionType.CastSpell):
                    if (secondaryTarget is Spell spellToCast)
                    {
                        result = await CastSpell(primaryTarget, hero, spellToCast);
                    }
                    else
                    {
                        result.Message = "Invalid target for CastSpell action.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.Focus):
                    if (hero.ChanneledSpell != null)
                    {
                        result.ApCost = 0;
                        for (int i = 0; i < hero.CurrentAP; i++)
                        {
                            if (hero.ChanneledSpell.FocusActionsRemaining > 0)
                            {
                                hero.ChanneledSpell.FocusActionsRemaining--;
                                result.ApCost++;
                                result.Message = $"{hero.Name} focuses their mind on casting.";

                                if (hero.ChanneledSpell.FocusActionsRemaining <= 0)
                                {
                                    await _spellResolution.ResolveSpellAsync(hero, hero.ChanneledSpell.Spell, hero.ChanneledSpell.Target, hero.ChanneledSpell.CastingOptions);

                                }
                            }
                            else
                            {
                                await _spellResolution.ResolveSpellAsync(hero, hero.ChanneledSpell.Spell, hero.ChanneledSpell.Target, hero.ChanneledSpell.CastingOptions);
                                result.Message = $"{hero.Name} has no focus actions remaining.";
                                result.WasSuccessful = false;
                            }
                        }
                    }
                    else
                    {
                        result.Message = "Invalid action.";
                        result.WasSuccessful = false;
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
                            result.Message = $"{hero.Name} breaks free from the entanglement!";
                        }
                        else
                        {
                            result.Message = $"{hero.Name} fails to break free.";
                        }
                        result.ApCost = 1; // Breaking free costs 1 AP
                    }
                    else
                    {
                        result.Message = "There is nothing to break free from.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.Pray):
                    if (secondaryTarget is Prayer prayerToCast
                        && hero.ActiveStatusEffects.Any(a => a.Category == (StatusEffectType)Enum.Parse(typeof(StatusEffectType), prayerToCast.Name.ToString())))
                    {
                        result.Message = await _powerActivation.ActivatePrayerAsync(hero, prayerToCast, (Character?)primaryTarget);
                    }
                    else
                    {
                        result.Message = "Invalid target for Pray action.";
                        result.WasSuccessful = false;
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
                                result.Message = "Not enough AP to activate this perk";
                                result.WasSuccessful = false;
                                break;
                            }
                            result.ApCost = 1;
                        }
                        result.Message = await _powerActivation.ActivatePerkAsync(hero, perkToUse, (Character?)primaryTarget) ? 
                            $"{hero.Name} activated {perkToUse.ToString()}" : $"{perkToUse.ToString()} activation was unsuccessful";
                    }
                    else
                    {
                        result.Message = "Invalid target for UsePerk action.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.ShieldBash):
                    if (primaryTarget is Character targetToBash && character.Position != null && hero.CurrentEnergy > 0
                        && hero.Inventory.OffHand != null && hero.Inventory.OffHand is Shield && ((Shield)hero.Inventory.OffHand).WeaponClass > 1)
                    {
                        result.AttackResult = await _attack.PerformShoveAsync(character, targetToBash, dungeon, isShieldBash: true);
                        if (result.AttackResult.IsHit && targetToBash.Position != null)
                        {
                            Room? room = dungeon.FindRoomAtPosition(targetToBash.Position);
                            if (room != null)
                            {
                                targetToBash.Room = room;
                            }
                            hero.CurrentEnergy--;
                        }
                        else
                        {
                            result.WasSuccessful = false;
                        }
                    }
                    else
                    {
                        result.Message = "Cannot perform Shield Bash. Conditions not met (not enough energy, no suitable shield, or invalid target).";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.StunningStrike):
                    if (primaryTarget is Character && character.Position != null && hero.CurrentEnergy > 0 && weapon is MeleeWeapon)
                    {
                        result.AttackResult = await _attack.PerformStunningStrikeAsync(hero, weapon, (Monster)primaryTarget, new CombatContext());
                    }
                    else
                    {
                        result.Message = "Cannot perform action.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.HarvestParts):
                    if (hero.Inventory.Backpack.Any(e => e != null && e.Name == "Alchemist Tool"))
                    {
                        var avaialbleCorpses = hero.Room.CorpsesInRoom?.Where(c => !c.HasBeenHarvested).ToList();
                        if (avaialbleCorpses != null && avaialbleCorpses.Any())
                        {
                            result = await HarvestPartsAsync(hero, avaialbleCorpses);
                        }
                        else
                        {
                            result.Message = "Cannot perform action as there are no harvestable corpses in this room.";
                            result.WasSuccessful = false;
                            break;
                        }
                    }
                    else
                    {
                        result.Message = "Cannot harvest parts wihtout the appropriate equipment: Alchemist Tool";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.ThrowPotion):

                    if (primaryTarget is GridPosition position && secondaryTarget is Potion potion)
                    {
                        result = await ThrowPotionAsync(hero, position, potion, dungeon);
                    }
                    else
                    {
                        result.Message = "Invalid target for ThrowPotion action.";
                        result.WasSuccessful = false;
                    }                    
                    break;
                case (Hero hero, ActionType.DragonBreath):
                    result = await DragonBreath(dungeon, hero);
                    break;
                case (Hero hero, ActionType.Taunt):
                    if (primaryTarget is Monster targetMonster && dungeon.HeroParty != null && hero.CurrentEnergy > 0)
                    {
                        result = await Taunt(dungeon.HeroParty.Heroes, hero, targetMonster);
                    }
                    else
                    {
                        result.Message = "Invalid target for Taunt action.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.StandUp):
                    if (character.CombatStance == CombatStance.Prone)
                    {
                        character.CombatStance = CombatStance.Normal;
                        result.Message = "Character is now standing.";
                    }
                    else
                    {
                        result.Message = "Character is already standing.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Character, ActionType.PickupWeapon):
                    if (character.DroppedWeapon != null)
                    {
                        result = await PickupWeapon(character, dungeon);
                    }
                    else
                    {
                        result.Message = "No weapon to pick up.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.AssistAllyOutOfPit):
                    if (primaryTarget is Hero)
                    {
                        result = await AssistAllyOutOfPitAsync((Hero)primaryTarget, hero);
                    }
                    else
                    {
                        result.Message = "Invalid target for AssistAllyOutOfPit action.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.DisarmTrap):
                    if (primaryTarget is Trap trap)
                    {
                        var trapTriggered = await _trap.DisarmTrapAsync(hero, trap);
                        if (!trapTriggered)
                        {
                            result.Message = $"{hero.Name} successfully disarmed the trap.";
                        }
                        else
                        {
                            result.Message = $"{hero.Name} failed to disarm the trap.";
                            result.Message += await _trap.TriggerTrapAsync(hero, trap, trapTriggered);
                        }
                    }
                    else
                    {
                        result.Message = "Invalid target for DisarmTrap action.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.SearchRoomWithParty):
                    if (primaryTarget is Room roomToSearchWithParty && !roomToSearchWithParty.HasBeenSearched && roomToSearchWithParty.HeroesInRoom != null)
                    {
                        result.SearchResult = new SearchResult();
                        result.SearchResult.HeroSearching = hero;
                        result = await SearchRoom(hero, roomToSearchWithParty, isPartySearch: true);
                        foreach (var member in roomToSearchWithParty.HeroesInRoom)
                        {
                            member.CurrentAP -= 2; // Each party member in the room(implies they participated in the search) loses 2 AP for a party search
                        }
                        result.ApCost = 0;
                    }
                    else
                    {
                        result.Message = "Invalid target, target must be room and there must be heroes in the room.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.SearchRoom):
                    if (primaryTarget is Room roomToSearch && !roomToSearch.HasBeenSearched)
                    {
                        result.SearchResult = new SearchResult();
                        result.SearchResult.HeroSearching = hero;
                        result = await SearchRoom(hero, roomToSearch, isPartySearch: false);
                    }
                    else
                    {
                        result.Message = "Invalid target, target must be room.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.SearchCorpse):
                    if (primaryTarget is Corpse corpse && !corpse.HasBeenSearched)
                    {
                        var rollResult = await _diceRoll.RequestRollAsync($"Roll for treasure from searching {corpse.Name}", "1d10");
                        await Task.Yield();
                        result.SearchResult = new SearchResult();
                        result.SearchResult.SearchRoll = rollResult.Roll;
                        result.SearchResult.HeroSearching = hero;
                        result.SearchResult = await _search.SearchCorpseAsync(corpse, result.SearchResult);
                    }
                    else
                    {
                        result.Message = "Invalid target, target must be a corpse.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.SearchFurniture):
                    if (primaryTarget is Furniture furniture && !furniture.HasBeenSearched)
                    {
                        var rollResult = await _diceRoll.RequestRollAsync($"Roll for treasure from searching {furniture.Name}", "1d10");
                        await Task.Yield();
                        result.SearchResult = new SearchResult();
                        result.SearchResult.SearchRoll = rollResult.Roll;
                        result.SearchResult.HeroSearching = hero;
                        result.SearchResult = await _search.SearchFurnitureAsync(furniture, result.SearchResult);
                    }
                    else
                    {
                        result.Message = "Invalid target, target must be furniture.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.DrinkFromFurniture):
                    if (primaryTarget is Furniture fountain && fountain.IsDrinkable)
                    {
                        var rollResult = await _diceRoll.RequestRollAsync($"Roll for results from drinking from the {fountain.Name}", "1d10");
                        await Task.Yield();
                        result.SearchResult = new SearchResult();
                        result.SearchResult.SearchRoll = rollResult.Roll;
                        result.SearchResult.HeroSearching = hero;
                        result.SearchResult = await _search.DrinkFromFurniture(fountain, result.SearchResult);
                    }
                    else
                    {
                        result.Message = "Invalid target, target must be drinkable.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Hero hero, ActionType.PullLever):
                    if (primaryTarget is Lever lever)
                    {
                        result.LeverResult = await lever.PullLever(hero);
                        result.SearchResult = result.LeverResult.SearchResult;
                    }
                    else
                    {
                        result.Message = "Invalid target, target must be a lever.";
                        result.WasSuccessful = false;
                    }
                    break;

            }

            if (primaryTarget is Door)
            {
                ((Door)primaryTarget).OnTrapTriggered -= HandleDoorTrapTrigger;
            }

            if (result.WasSuccessful)
            {
                character.CurrentAP -= result.ApCost;
                if(actionType != ActionType.PowerAttack) character.IsVulnerableAfterPowerAttack = false;
                if (character is Hero hero && hero.ProfessionName == "Wizard" && hero.CurrentAP <= 0) hero.CanCastSpell = true;

                if (result.SearchResult != null && result.SearchResult.FoundItems != null)
                {
                    foreach (var foundItem in result.SearchResult.FoundItems)
                    {
                        if (foundItem != null)
                        {
                            await BackpackHelper.AddItem(result.SearchResult.HeroSearching.Inventory.Backpack, foundItem);
                        }
                    }
                }
            }

            return result;
        }

        private async Task<ActionResult> PickLock(Lock lockToPick, Hero hero)
        {
            return await _lock.PickLock(hero, lockToPick);
        }

        private async Task HandleDoorTrapTrigger(Trap trap, Character character)
        {
            await _trap.TriggerTrapAsync(character, trap);
        }

        private async Task<ActionResult> RemoveCobwebsAsync(Hero hero, Door door)
        {
            var result = new ActionResult();
            if (OnRemoveCobwebs != null)
            {
                var isEncounter = await OnRemoveCobwebs.Invoke(hero, door);
                if (isEncounter) result.Message = $"{hero.Name} removes the webs, but the spinners are not happy. Giant spiders descend form the ceiling and attack!";
                else result.Message = $"{hero.Name} removes the webs. The doorway is now clear, but you hear faint shreiks which imply's somebody is unhappy with your actions.";
                door.State = DoorState.Open;
            }
            else
            {
                result.Message = "Nothing happens.";
                result.WasSuccessful = false;
            }
            return result;
        }

        private async Task<ActionResult> OpenDoor(Door door, Character character)
        {
            var result = new ActionResult();
            if (door.State == DoorState.Open) return new ActionResult() { Message = "The door is already open.", WasSuccessful = false };

            // Resolve Lock
            if (door.Lock.IsLocked)
            {
                return new ActionResult() { Message = $"The door is locked (Difficulty: {door.Lock.LockModifier}, HP: {door.Lock.LockHP}).", WasSuccessful = false };
            }

            // Open the door and reveal the next room
            await door.OpenAsync(character);

            if (OnOpenDoor != null && await OnOpenDoor.Invoke(door))
            {
                result.Message = result.Message = "The door creaks open...";
            }

            return result;
        }

        private async Task<ActionResult> OpenPortcullis(Door door, Hero hero)
        {
            var result = new ActionResult();
            if (hero.Party == null) return result;
            var adjacentHeroes = hero.Party.Heroes.Where(h => h.Position != null && GridService.IsAdjacent(h.Position, door.PassagewaySquares[0]));
            var rollResult = await _diceRoll.RequestRollAsync("Roll strength test.", "1d100", stat: (hero, BasicStat.Strength));
            await Task.Yield();
            int roll = rollResult.Roll;
            for (int i = 0; i < adjacentHeroes.Count(); i++)
            {
                if (i == 0) continue;
                else roll -= 10;
            }
            if (hero.TestStrength(roll))
            {
                result.Message = $"{hero.Name} lifts the Portcullis and the doorway is not open.";
                _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.OpenDoorOrChest);
                await door.OpenAsync(hero);
            }
            else
            {
                result.Message = $"{hero.Name} fails to lift the portcullis and instead raises the threat level due to the loud bang made when it dropped.";
            }

            return result;
        }

        private async Task<ActionResult> SearchRoom(Hero hero, Room roomToSearch, bool isPartySearch = false)
        {
            var result = new ActionResult();
            result.WasSuccessful = true;
            await _search.SearchRoomAsync(roomToSearch, hero, isPartySearch);
            result.SearchResult = roomToSearch.SearchResults;

            return result;
        }

        private async Task<ActionResult> AssistAllyOutOfPitAsync(Hero heroToAssist, Hero hero)
        {
            var result = new ActionResult();
            result.Message = string.Empty;
            result.WasSuccessful = true;
            var rope = hero.Inventory.Backpack.FirstOrDefault(i => i != null && i.Name.Contains("Rope"));
            if (rope != null)
            {
                if (rope.Name.Contains("old"))
                {
                    var roll = RandomHelper.RollDie(DiceType.D6);
                    if (roll >= 5)
                    {
                        BackpackHelper.TakeOneItem(hero.Inventory.Backpack, rope);
                        roll = RandomHelper.RollDie(DiceType.D6);
                        int damage = await heroToAssist.TakeDamageAsync(roll, (new FloatingTextService(), heroToAssist.Position), _powerActivation);
                        result.Message = $"{hero.Name} assists {heroToAssist.Name} out of the pit using an old rope, but it breaks causing {heroToAssist.Name} to fall back in the pit causing {damage} damage.";
                    }
                    else
                    {
                        result.Message = $"{hero.Name} assists {heroToAssist.Name} out of the pit using an old rope.";
                    }
                }
                else
                {
                    result.Message = $"{hero.Name} assists {heroToAssist.Name} out of the pit using a rope.";
                }
            }
            else
            {
                result.Message = $"{hero.Name} does not have a rope to assist {heroToAssist.Name} out of the pit.";
                result.WasSuccessful = false;
            }

            return result;
        }

        private async Task<ActionResult> BreakDownDoorAsync(
            DungeonState dungeon, Hero hero, Door door, Weapon? weapon)
        {
            var result = new ActionResult();
            result.Message = string.Empty;
            result.WasSuccessful = true;
            if (door.Lock.IsLocked && weapon is MeleeWeapon)
            {
                if (await _lock.BashLock(hero, door.Lock, (MeleeWeapon)weapon))
                {
                    await door.OpenAsync(hero);
                    door.State = DoorState.BashedDown;

                    // if the door that was bashed donw was in relation to the poison gas trap.
                    var poisonGas = hero.ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.PoisonGas);
                    if (poisonGas != null)
                    {
                        foreach (var characterInRoom in hero.Room.CharactersInRoom)
                        {
                            var poisonGasEffect = characterInRoom.ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.PoisonGas);
                            if (poisonGasEffect != null)
                            {
                                StatusEffectService.RemoveActiveStatusEffect(characterInRoom, poisonGasEffect);
                            }
                        }
                        result.Message = $"{hero.Name} bashed down the door, releasing the poison gas!";
                    }
                    else
                    {
                        result.Message = $"{hero.Name} successfully bashed down the door.";
                    }
                }
                else
                {
                    result.Message = $"{hero.Name} failed to bash down the door, the lock has {door.Lock.LockHP} HP left.";
                }
            }
            else
            {
                result.Message = "Target door is not locked.";
                result.Message += await PerformActionAsync(dungeon, hero, ActionType.OpenDoor, door);
                result.WasSuccessful = false;
            }

            return result;
        }

        private async Task<ActionResult> PickupWeapon(Character character, DungeonState dungeon)
        {
            var result = new ActionResult();
            result.Message = string.Empty;
            var enemies = GetEnemiesForZOC(character, dungeon);
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
                                await BackpackHelper.AddItem(hero.Inventory.Backpack, hero.DroppedWeapon);
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
                    result.Message = $"{character.Name} successfully picked up their weapon.";
                }
                else
                {
                    result.Message = $"{character.Name} failed to pick up their weapon.";
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
                            await BackpackHelper.AddItem(hero.Inventory.Backpack, hero.DroppedWeapon);
                        }
                    }
                }
                character.DroppedWeapon = null;
                result.Message = $"{character.Name} picked up their weapon.";
            }

            return result;
        }

        private async Task<ActionResult> Taunt(List<Hero> heroParty, Hero hero, Monster targetMonster)
        {
            var result = new ActionResult();
            result.Message = string.Empty;
            result.WasSuccessful = true;
            var taunt = hero.Perks.FirstOrDefault(p => p.Name == PerkName.Taunt);
            if (taunt == null)
            {
                result.Message = $"{hero.Name} doesn't have the taunt ability.";
                result.WasSuccessful = false;
                return result;
            }

            // Rule: "not locked in close combat"
            bool isAdjacentToAnyHero = heroParty.Any(h => h.Position != null && targetMonster.Position != null && GridService.IsAdjacent(targetMonster.Position, h.Position));

            if (isAdjacentToAnyHero)
            {
                result.Message = $"{targetMonster.Name} is already locked in close combat and cannot be taunted.";
                result.WasSuccessful = false;
            }
            else if (await _powerActivation.ActivatePerkAsync(hero, taunt))
            {
                var tauntEffect = taunt.ActiveStatusEffect ?? new ActiveStatusEffect(StatusEffectType.Taunt, 1);
                await StatusEffectService.AttemptToApplyStatusAsync(targetMonster, tauntEffect, _powerActivation);
                targetMonster.TauntedBy = hero;
                result.Message = $"{hero.Name} taunts {targetMonster.Name}, forcing it to attack them!";
            }
            else
            {
                result.Message = $"{taunt.Name.ToString()} failed to activate";
                result.WasSuccessful = false;
            }
            return result;
        }

        private async Task<ActionResult> DragonBreath(DungeonState dungeon, Hero hero)
        {
            var result = new ActionResult();
            var dragonBreathEffect = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.DragonBreath);
            if (dragonBreathEffect == null)
            {
                result.Message = $"{hero.Name} has not consumed a Potion of Dragon's Breath.";
                result.WasSuccessful = false;
                return result;
            }

            if (hero.Position == null)
            {
                result.Message = "Cannot use Dragon Breath without a position.";
                result.WasSuccessful = false;
                return result;
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
                result.Message = "There are no valid adjacent squares to attack.";
                result.WasSuccessful = false;
                return result;
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
                result.Message = outcome.ToString();
            }
            else
            {
                result.Message = $"{hero.Name} decides not to use Dragon Breath.";
                result.WasSuccessful = false; // No AP is consumed if the action is cancelled
            }

            return result;
        }

        private async Task<ActionResult> StandardAttack(DungeonState dungeon, Character character, Character target, CombatContext? combatContext, Weapon? weapon)
        {
            var result = new ActionResult();
            int startingAP = character.CurrentAP;
            result.ApCost = 1;
            result.Message = string.Empty;

            if (weapon != null && weapon is RangedWeapon)
            {
                result = await PerformActionAsync(dungeon, character, ActionType.Reload);
                if (character.CurrentAP <= 0)
                {
                    result.Message += $"\n {character.Name} is reloading...";
                    result.ApCost = 0;
                    return result;
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
                result.Message += "\n" + attackResult.OutcomeMessage;
            }
            else
            {
                result.Message = attackResult.OutcomeMessage;
            }

            if (attackResult.IsHit && character.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Frenzy))
            {
                result.ApCost = 0;
                result.Message += $"\n {character.Name} is in a frenzy and can act again";
            }

            return result;
        }

        private async Task<ActionResult> Move( Character character, GridPosition position, DungeonState dungeon, RangedWeapon? rangedWeapon = null)
        {
            var result = new ActionResult();
            result.Message = string.Empty;
            result.WasSuccessful = true;

            if (character.Position != null)
            {
                // Determine available movement points for this action
                int availableMovement = character.CurrentMovePoints;
                if (character.HasMadeFirstMoveAction) // Rule: Second move is half distance
                {
                    availableMovement /= 2;
                }

                var enemies = GetEnemiesForZOC(character, dungeon);
                
                List<GridPosition> path = GridService.FindShortestPath(character, position, dungeon.DungeonGrid, enemies);

                if (character is Monster movingMonster && OnMonsterMovement != null)
                {
                    if (await OnMonsterMovement.Invoke(movingMonster, path))
                    {
                        result.Message = "Movement interrupted by Overwatch!";
                        result.WasSuccessful = false;
                        return result;
                    }
                }

                MovementResult moveResult = GridService.MoveCharacter(character, path, dungeon.DungeonGrid, enemies, availableMovement);

                if (moveResult.WasSuccessful)
                {
                    character.SpendMovementPoints(moveResult.MovementPointsSpent); // A new method you'll add to Character
                    availableMovement = character.CurrentMovePoints;
                    result.Message = moveResult.Message;

                    if (rangedWeapon != null)
                    {
                        if (!rangedWeapon.IsLoaded)
                        {
                            result.Message += await PerformActionAsync(dungeon, character, ActionType.ReloadWhileMoving);
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
                    result.Message = moveResult.Message;
                    result.WasSuccessful = false; // Don't deduct AP if no move was made
                }
            }
            else
            {
                result.Message += "Error in finding Hero Party";
                result.WasSuccessful = false; // Don't deduct AP if no move was made
            }

            return result;
        }

        private List<Character> GetEnemiesForZOC(Character character, DungeonState dungeon)
        {
            var enemies = new List<Character> ();
            // Determine enemies for ZOC calculation
            if (dungeon.HeroParty != null)
            {
                if (character is Monster)
                {
                    enemies = dungeon.HeroParty.Heroes.Cast<Character>().ToList();
                    if (enemies.Count <= 0 && character.Room.HeroesInRoom != null)
                    {
                        enemies = character.Room.HeroesInRoom.Cast<Character>().ToList();
                    }
                }
                else if (character is Hero)
                {
                    enemies = dungeon.RevealedMonsters.Cast<Character>().ToList();
                    if (enemies.Count <= 0 && character.Room.MonstersInRoom != null)
                    {
                        enemies = character.Room.MonstersInRoom.Cast<Character>().ToList();
                    }
                } 
            }

            return enemies;
        }

        private async Task<ActionResult> CastSpell(object? primaryTarget, Hero hero, Spell spellToCast)
        {
            var result = new ActionResult();
            SpellCastingResult options = await _spellCasting.RequestCastingOptionsAsync(hero, spellToCast); await Task.Yield();

            if (options.WasCancelled)
            {
                result.Message = $"{hero.Name} decided not to cast the spell.";
                result.WasSuccessful = false;
            }
            else
            {
                SpellCastResult spellCastResult = await spellToCast.CastSpellAsync(hero, _diceRoll, _powerActivation, options.FocusPoints, options.PowerLevels,
                    monster: (primaryTarget is Monster) ? (Monster)primaryTarget : null);
                result.Message = spellCastResult.OutcomeMessage;

                if (spellCastResult.ManaSpent <= 0)
                {
                    result.Message = spellCastResult.OutcomeMessage;
                    result.WasSuccessful = false;
                }
                else
                {
                    if (primaryTarget != null)
                    {
                        if (options.FocusPoints <= 0)
                        {
                            if (spellToCast.Properties != null && spellToCast.Properties.ContainsKey(SpellProperty.QuickSpell))
                            {
                                result.ApCost = 1; // Quick spells cost 1 AP if there is no focus points added
                            }
                            else
                            {
                                result.ApCost = 2; // Regular spells cost 2 AP if there is no focus points added
                            }
                            await _spellResolution.ResolveSpellAsync(hero, spellToCast, primaryTarget, options);
                        }
                        else if (options.FocusPoints >= 1)
                        {
                            hero.ChanneledSpell = new ChanneledSpell(hero, spellToCast, primaryTarget, options);
                            if (spellToCast.Properties != null && spellToCast.Properties.ContainsKey(SpellProperty.QuickSpell))
                            {
                                result.ApCost = 2;
                                hero.ChanneledSpell.FocusActionsRemaining--; // Deduct focus action if used
                            }
                        }

                        if (hero.ChanneledSpell != null && hero.ChanneledSpell.FocusActionsRemaining <= 0)
                        {
                            spellCastResult = await _spellResolution.ResolveSpellAsync(hero, spellToCast, primaryTarget, options);
                            result.Message = spellCastResult.OutcomeMessage;
                        }
                    }
                }
            }

            return result;
        }

        private async Task<ActionResult> HarvestPartsAsync(Hero hero, List<Corpse> avaialbleCorpses)
        {
            var result = new ActionResult();
            result.Message = string.Empty;
            avaialbleCorpses.Shuffle();
            result.SearchResult = new SearchResult();
            result.SearchResult.HeroSearching = hero;
            result.SearchResult.SearchTarget = hero.GetSkill(Skill.Alchemy);
            var resultRoll = await _diceRoll.RequestRollAsync("Roll for alchemy skill test.", "1d100", skill: (hero, Skill.Alchemy));
            await Task.Yield();
            result.SearchResult.SearchRoll = resultRoll.Roll;

            var equisiteRange = 10;

            if (await _powerActivation.RequestPerkActivationAsync(hero, PerkName.CarefulTouch))
            {
                equisiteRange = 20;
            }

            if (result.SearchResult.WasSuccessful)
            {
                var parts = new List<Part>();
                for (int i = 0; i < Math.Min(avaialbleCorpses.Count, 3); i++)
                {
                    if (i == 0)
                    {
                        var part = (await _alchemy.GetPartsAsync(1, avaialbleCorpses[i].OriginMonster.Species))[0];
                        if (await _powerActivation.RequestPerkActivationAsync(hero, PerkName.Surgeon))
                        {
                            var choiceResult = await _diceRoll.RequestChoiceAsync("Choose part to harvest", Enum.GetNames(typeof(PartName)).ToList());
                            await Task.Yield();
                            Enum.TryParse(choiceResult.SelectedOption, out PartName selectedName);
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
                    result.SearchResult.FoundItems = [part];
                    result.SearchResult.Message += $"{hero.Name} harvested {part.ToString()}";
                }
            }            
            else
            {
                result.Message += $"{hero.Name} failed to harvest any parts.";
            }

            return result;
        }

        private async Task<ActionResult> ThrowPotionAsync(Hero hero, GridPosition position, Potion potion, DungeonState dungeon)
        {
            var result = new ActionResult();
            result.Message = string.Empty;
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
            var throwThroughDoor = hero.Room.Doors.FirstOrDefault(d => d.PassagewaySquares.Contains(position));
            if (throwThroughDoor != null && hero.Position != null)
            {
                bool isAdjacentToDoor = throwThroughDoor.PassagewaySquares.Any(p => GridService.IsAdjacent(hero.Position, p));
                if (!isAdjacentToDoor)
                {
                    rsSkill -= 10;
                }

                if (rsRoll.Roll > rsSkill)
                {
                    // Missed throw through a door, hits a square in front of the door
                    var doorSquares = throwThroughDoor.PassagewaySquares.ToList();
                    position = doorSquares[RandomHelper.GetRandomNumber(0, doorSquares.Count - 1)];
                    result.Message = $"{hero.Name} misses! The potion hits the doorway at {position}.";
                }
            }
            else if (rsRoll.Roll > rsSkill)
            {
                var neighbors = GridService.GetNeighbors(position, dungeon.DungeonGrid).ToList();
                neighbors.Shuffle();
                position = neighbors.FirstOrDefault() ?? position;
                result.Message = $"{hero.Name} misses! The potion lands at {position}.";
            }

            pitcherActive = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.Pitcher);
            if (pitcherActive != null)
            {
                hero.ActiveStatusEffects.Remove(pitcherActive);
            }

            result.Message += await _potionActivation.BreakPotionAsync(hero, potion, position, dungeon);
            return result;
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
                ActionType.OpenPortcullis => 1,
                ActionType.SearchFurniture => 1,
                ActionType.SearchCorpse => 1,
                ActionType.HealOther => 1,
                ActionType.Aim => 1,
                ActionType.HarvestParts => 1,
                ActionType.ThrowPotion => 1,
                ActionType.BreakDownDoor => 1,
                ActionType.StandUp => 1,
                ActionType.PowerAttack => 2,
                ActionType.ChargeAttack => 2,
                ActionType.RearrangeGear => 2,
                ActionType.PickLock => 2,
                ActionType.DisarmTrap => 2,
                ActionType.SearchRoom => 2,
                ActionType.HealSelf => 2,
                ActionType.AssistAllyOutOfPit => 2,
                ActionType.RemoveCobwebs => 2,
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
