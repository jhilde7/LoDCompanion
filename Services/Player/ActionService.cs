using LoDCompanion.Models;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;
using System.Threading;

namespace LoDCompanion.Services.Player
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
        Parry

    }

    public class ActionInfo
    {
        public ActionType ActionType { get; set; }
        public int ApCost { get; set; }
        public object? Target { get; set; }
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

        public ActionService(
            DungeonManagerService dungeonManagerService, 
            SearchService searchService,
            HealingService healingService,
            InventoryService inventoryService,
            IdentificationService identificationService,
            AttackService attackService)
        {
            _dungeonManager = dungeonManagerService;
            _search = searchService;
            _healing = healingService;
            _inventory = inventoryService;
            _identification = identificationService;
            _attack = attackService;
        }

        /// <summary>
        /// Attempts to perform an action for a hero, checking and deducting AP.
        /// </summary>
        /// <param name="character">The hero performing the action.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <param name="target">The target of the action (e.g., a Monster, DoorChest, or another Hero).</param>
        /// <returns>True if the action was successfully performed, false otherwise.</returns>
        public async Task<string> PerformActionAsync(DungeonState dungeon, Character character, ActionType actionType, object? primaryTarget = null, object? secondaryTarget = null)
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
            Weapon? weapon = new Weapon();
            if (character.Weapons.FirstOrDefault() is Weapon w)
            {
                weapon = w;
            }
            // Execute the action logic
            switch (actionType)
            {
                case ActionType.StandardAttack:
                    if (primaryTarget is Character standardAttackTarget && weapon != null)
                    {
                        resultMessage = await PerformActionAsync(dungeon, character, ActionType.Reload);
                        if (character.CurrentAP <= 0) break;

                        AttackResult attackResult = await _attack.PerformStandardAttackAsync(character, weapon, standardAttackTarget, dungeon);
                        if (startingAP > character.CurrentAP)
                        {
                            resultMessage += "\n" + attackResult.OutcomeMessage; 
                        }
                        else
                        {
                            resultMessage = attackResult.OutcomeMessage;
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid target or no weapon equipped for attack.";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.StandardAttackWhileAiming:
                    if (primaryTarget is Character standardAttackTarget1 && weapon != null)
                    {
                        resultMessage = await PerformActionAsync(dungeon, character, ActionType.Reload);
                        if (character.CurrentAP <= 0) break;

                        AttackResult attackResult = await _attack.PerformStandardAttackAsync(character, weapon, standardAttackTarget1, dungeon, new CombatContext { HasAimed = true });
                        if (startingAP > character.CurrentAP)
                        {
                            resultMessage += "\n" + attackResult.OutcomeMessage;
                        }
                        else
                        {
                            resultMessage = attackResult.OutcomeMessage;
                        }
                        character.CombatStance = CombatStance.Normal;
                    }
                    else
                    {
                        resultMessage = "Invalid target or no weapon equipped for attack.";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.PowerAttack:
                    if (character.CurrentAP >= GetActionCost(actionType) && character.Weapons.FirstOrDefault(w => w.IsMelee) is MeleeWeapon meleeWeapon && primaryTarget is Character powerAttackTarget)
                    {
                        AttackResult attackResult = await _attack.PerformPowerAttackAsync(character, meleeWeapon, powerAttackTarget, dungeon);
                        character.IsVulnerableAfterPowerAttack = true; // Set the vulnerability flag
                        resultMessage = attackResult.OutcomeMessage;
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.ChargeAttack:
                    if (character.CurrentAP >= GetActionCost(actionType) && primaryTarget is Character chargeAttackTarget && character.Weapons.FirstOrDefault(w => w.IsMelee) is MeleeWeapon chargeWeapon)
                    {
                        AttackResult attackResult = await _attack.PerformChargeAttackAsync(character, chargeWeapon, chargeAttackTarget, dungeon);
                        resultMessage = attackResult.OutcomeMessage;
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.Shove:
                    if (primaryTarget is Character targetToShove && _dungeonManager.DungeonState != null)
                    {
                        resultMessage = GridService.ShoveCharacter(character, targetToShove, targetToShove.Room, _dungeonManager.DungeonState.DungeonGrid); // Pass current room
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.Move:
                    if (primaryTarget is GridPosition targetPosition)
                    {
                        if (GridService.MoveCharacter(character, targetPosition, dungeon.DungeonGrid))
                        {
                            resultMessage = $"{character.Name} moves to {targetPosition}.";
                            resultMessage += await PerformActionAsync(dungeon, character, ActionType.ReloadWhileMoving);
                        }
                        else
                        {
                            resultMessage = $"{character.Name} cannot move there; the path is blocked.";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid destination for move action.";
                        actionWasSuccessful = false;
                    }
                    break;

                case ActionType.OpenDoor:
                    if (primaryTarget is DoorChest door)
                    {
                        _dungeonManager.InteractWithDoor(door, character);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.HealSelf:
                    if (character is Hero self)
                    {
                        resultMessage = _healing.ApplyBandage(self, self);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.HealOther:
                    if (character is Hero actingHero && primaryTarget is Hero targetHero)
                    {
                        resultMessage = _healing.ApplyBandage(actingHero, targetHero);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.RearrangeGear:
                    if (character is Hero inventoryHero && primaryTarget is Equipment item && secondaryTarget is ValueTuple<ItemSlot, ItemSlot> slots)
                    { 
                        resultMessage = _inventory.RearrangeItem(inventoryHero, item, slots.Item1, slots.Item2);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.IdentifyItem:
                    if (character is Hero identifyingHero && primaryTarget is Equipment itemToIdentify)
                    { 
                        resultMessage = _identification.IdentifyItem(identifyingHero, itemToIdentify);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.SetOverwatch:
                    var equippedWeapon = character.Weapons.FirstOrDefault();
                    if (equippedWeapon == null) return $"{character.Name} does not have a weapon equipped";
                    if (equippedWeapon is RangedWeapon ranged && !ranged.IsLoaded) return $"{character.Name} needs to reload their weapon";
                    character.CombatStance = CombatStance.Overwatch;
                    apCost = character.CurrentAP;
                    resultMessage = $"{character.Name} takes an Overwatch stance, ready to react.";
                    break;
                case ActionType.EndTurn:
                    resultMessage = $"{character.Name} ends their turn.";
                    apCost = character.CurrentAP;
                    break;
                case ActionType.Parry:
                    character.CombatStance = CombatStance.Parry;
                    apCost = character.CurrentAP;
                    resultMessage = $"{character.Name} entered parry stance";
                    break;
                case ActionType.Aim:
                    character.CombatStance = CombatStance.Aiming;
                    resultMessage = $"{character.Name} takes careful aim.";
                    break;
                case ActionType.Reload:
                    if (weapon is RangedWeapon rangedWeapon)
                    {
                        if (!rangedWeapon.IsLoaded)
                        {
                            rangedWeapon.reloadAmmo();
                            resultMessage = $"{character.Name} spends a moment to reload their {rangedWeapon.Name}."; 
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
                case ActionType.ReloadWhileMoving:
                    if (weapon is RangedWeapon rangedWeapon1 && !rangedWeapon1.IsLoaded)
                    {
                        rangedWeapon1.reloadAmmo();
                        resultMessage = $" and reloads their {rangedWeapon1.Name}.";
                    }
                    else
                    {
                        resultMessage = string.Empty;
                    }
                    break;
            }

            if (actionWasSuccessful)
            {
                character.CurrentAP -= apCost;
            }

            return $"{character.Name} performed {actionType}, {resultMessage}. {character.CurrentAP} AP remaining.";
        }

        /// <summary>
        /// Gets the AP cost for a specific action type.
        /// </summary>
        private int GetActionCost(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.StandardAttack => 1,
                ActionType.PowerAttack => 2,
                ActionType.ChargeAttack => 2,
                ActionType.Shove => 1,
                ActionType.Move => 1,
                ActionType.OpenDoor => 1,
                ActionType.PickLock => 2,
                ActionType.DisarmTrap => 2,
                ActionType.SearchRoom => 2,
                ActionType.SearchFurniture => 1,
                ActionType.SearchCorpse => 1,
                ActionType.HealOther => 1,
                ActionType.HealSelf => 2,
                ActionType.RearrangeGear => 2,
                ActionType.IdentifyItem => 0,
                ActionType.Reload => 1,
                ActionType.Aim => 1,
                ActionType.ReloadWhileMoving => 0,
                _ => 1,
            };
        }
    }
}
