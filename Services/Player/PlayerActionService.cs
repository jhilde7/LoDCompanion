using LoDCompanion.Models;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Services.Player
{
    /// <summary>
    /// Defines the types of actions a player can take.
    /// </summary>
    public enum PlayerActionType
    {
        StandardAttack,
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
        Reload

    }

    public class PlayerActionInfo
    {
        public PlayerActionType ActionType { get; set; }
        public int ApCost { get; set; }
        public object? Target { get; set; }
    }

    /// <summary>
    /// Handles the execution of actions performed by heroes.
    /// </summary>
    public class PlayerActionService
    {
        private readonly DungeonManagerService _dungeonManager;
        private readonly SearchService _search;
        private readonly HealingService _healing;
        private readonly InventoryService _inventory;
        private readonly IdentificationService _identification;
        private readonly AttackService _attack;

        public PlayerActionService(
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
        /// <param name="hero">The hero performing the action.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <param name="target">The target of the action (e.g., a Monster, DoorChest, or another Hero).</param>
        /// <returns>True if the action was successfully performed, false otherwise.</returns>
        public string PerformAction(DungeonState dungeon, Hero hero, PlayerActionType actionType, object? primaryTarget = null, object? secondaryTarget = null)
        {
            string resultMessage = "";
            int apCost = GetActionCost(actionType);
            if (hero.CurrentAP < apCost)
            {
                resultMessage = $"{hero.Name} does not have enough AP for {actionType}.";
                return resultMessage;
            }

            resultMessage = $"{hero.Name} performed {actionType}.";
            bool actionWasSuccessful = true;
            Weapon? weapon = new Weapon();
            if (hero.Weapons.FirstOrDefault() is Weapon w)
            {
                weapon = w;
            }
            // Execute the action logic
            switch (actionType)
            {
                case PlayerActionType.StandardAttack:
                    if (primaryTarget is Monster monster && weapon != null)
                    {
                        if (weapon is RangedWeapon rangedWeapon && !rangedWeapon.IsLoaded)
                        {
                            rangedWeapon.reloadAmmo();
                            apCost = GetActionCost(actionType);
                            resultMessage = $"{hero.Name} spends a moment to reload their {rangedWeapon.Name}.";
                            break;
                        }

                        var context = new CombatContext();
                        var attackResult = _attack.PerformStandardAttack(hero, weapon, monster, dungeon);
                        resultMessage = attackResult.OutcomeMessage;
                    }
                    else
                    {
                        resultMessage = "Invalid target or no weapon equipped for attack.";
                        actionWasSuccessful = false;
                    }
                    break;

                case PlayerActionType.Move:
                    if (primaryTarget is GridPosition targetPosition)
                    {
                        if (GridService.MoveCharacter(hero, targetPosition, dungeon.DungeonGrid))
                        {
                            resultMessage = $"{hero.Name} moves to {targetPosition}.";
                            if (weapon != null && weapon is RangedWeapon rangedWeapon && !rangedWeapon.IsLoaded)
                            {
                                rangedWeapon.reloadAmmo();
                                resultMessage = $" and reloads their {rangedWeapon.Name}";
                                break;
                            }
                        }
                        else
                        {
                            resultMessage = $"{hero.Name} cannot move there; the path is blocked.";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid destination for move action.";
                        actionWasSuccessful = false;
                    }
                    break;

                case PlayerActionType.OpenDoor:
                    if (primaryTarget is DoorChest door)
                    {
                        _dungeonManager.InteractWithDoor(door, hero);
                    }
                    break;
                case PlayerActionType.HealSelf:
                    resultMessage = _healing.ApplyBandage(hero, hero);
                    break;
                case PlayerActionType.HealOther:
                    if (primaryTarget is Hero targetHero)
                    {
                        resultMessage = _healing.ApplyBandage(hero, targetHero);
                    }
                    break;
                case PlayerActionType.RearrangeGear:
                    if (primaryTarget is Equipment item && secondaryTarget is ValueTuple<ItemSlot, ItemSlot> slots)
                    { 
                        resultMessage = _inventory.RearrangeItem(hero, item, slots.Item1, slots.Item2);
                    }
                    break;
                case PlayerActionType.IdentifyItem:
                    if (primaryTarget is Equipment itemToIdentify)
                    { 
                        resultMessage = _identification.IdentifyItem(hero, itemToIdentify);
                    }
                    break;
                case PlayerActionType.SetOverwatch:
                    var equippedWeapon = hero.Weapons.FirstOrDefault();
                    if (equippedWeapon == null) return $"{hero.Name} does not have a weapon equipped";
                    if (equippedWeapon is RangedWeapon ranged && !ranged.IsLoaded) return $"{hero.Name} needs to reload their weapon";
                    hero.CombatStance = CombatStance.Overwatch;
                    resultMessage = $"{hero.Name} takes an Overwatch stance, ready to react.";
                    break;
                case PlayerActionType.PowerAttack:
                    if (primaryTarget is Monster monster1 && hero.Weapons.FirstOrDefault() is Weapon weapon1)
                    {
                        var attackResult = _attack.PerformPowerAttack(hero, weapon1, monster1, dungeon);
                        hero.IsVulnerableAfterPowerAttack = true; // Set the vulnerability flag
                        resultMessage = attackResult.OutcomeMessage;
                    }
                    break;
                case PlayerActionType.ChargeAttack:
                    if (primaryTarget is Monster monsterTarget && hero.Weapons.FirstOrDefault() is Weapon chargeWeapon)
                    {
                        var attackResult = _attack.PerformChargeAttack(hero, chargeWeapon, monsterTarget, dungeon);
                        resultMessage = attackResult.OutcomeMessage;
                    }
                    break;
                case PlayerActionType.Shove:
                    if (primaryTarget is Character targetToShove && _dungeonManager.DungeonState != null)
                    {
                        resultMessage = GridService.ShoveCharacter(hero, targetToShove, targetToShove.Room, _dungeonManager.DungeonState.DungeonGrid); // Pass current room
                    }
                    break;
                case PlayerActionType.EndTurn:
                    resultMessage = $"{hero.Name} ends their turn.";
                    apCost = hero.CurrentAP; // Ending turn consumes all AP
                    break;
            }

            if (actionWasSuccessful)
            {
                hero.CurrentAP -= apCost;
            }

            return $"{hero.Name} performed {actionType}, {resultMessage}. {hero.CurrentAP} AP remaining.";
        }

        /// <summary>
        /// Gets the AP cost for a specific action type.
        /// </summary>
        private int GetActionCost(PlayerActionType actionType)
        {
            return actionType switch
            {
                PlayerActionType.StandardAttack => 1,
                PlayerActionType.PowerAttack => 2,
                PlayerActionType.ChargeAttack => 2,
                PlayerActionType.Shove => 1,
                PlayerActionType.Move => 1,
                PlayerActionType.OpenDoor => 1,
                PlayerActionType.PickLock => 2,
                PlayerActionType.DisarmTrap => 2,
                PlayerActionType.SearchRoom => 2,
                PlayerActionType.SearchFurniture => 1,
                PlayerActionType.SearchCorpse => 1,
                PlayerActionType.HealOther => 1,
                PlayerActionType.HealSelf => 2,
                PlayerActionType.RearrangeGear => 2,
                PlayerActionType.IdentifyItem => 0,
                PlayerActionType.Reload => 1,
                _ => 1,
            };
        }
    }
}
