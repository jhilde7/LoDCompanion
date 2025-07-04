using LoDCompanion.Models;
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
        SetOverwatch
    }

    /// <summary>
    /// Handles the execution of actions performed by heroes.
    /// </summary>
    public class PlayerActionService
    {
        private readonly DungeonManagerService _dungeonManager;
        private readonly HeroCombatService _heroCombat;
        private readonly SearchService _search;
        private readonly HealingService _healing;
        private readonly InventoryService _inventory;
        private readonly IdentificationService _identification;

        public PlayerActionService(
            DungeonManagerService dungeonManagerService, 
            HeroCombatService heroCombatService,
            SearchService searchService,
            HealingService healingService,
            InventoryService inventoryService,
            IdentificationService identificationService)
        {
            _dungeonManager = dungeonManagerService;
            _heroCombat = heroCombatService;
            _search = searchService;
            _healing = healingService;
            _inventory = inventoryService;
            _identification = identificationService;
        }

        /// <summary>
        /// Attempts to perform an action for a hero, checking and deducting AP.
        /// </summary>
        /// <param name="hero">The hero performing the action.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <param name="target">The target of the action (e.g., a Monster, DoorChest, or another Hero).</param>
        /// <returns>True if the action was successfully performed, false otherwise.</returns>
        public bool PerformAction(Hero hero, PlayerActionType actionType, object? primaryTarget = null, object? secondaryTarget = null)
        {
            int apCost = GetActionCost(actionType);
            if (hero.CurrentAP < apCost)
            {
                Console.WriteLine($"{hero.Name} does not have enough AP for {actionType}.");
                return false;
            }

            // Deduct AP before performing the action
            hero.CurrentAP -= apCost;

            string resultMessage = $"{hero.Name} performed {actionType}.";
            // Execute the action logic
            switch (actionType)
            {
                case PlayerActionType.StandardAttack:
                    if (primaryTarget is Monster monster)
                    {
                        // _heroCombatService.PerformAttack(hero, monster);
                        Console.WriteLine($"{hero.Name} attacks {monster.Name}.");
                    }
                    break;

                case PlayerActionType.Move:
                    Console.WriteLine($"{hero.Name} moves.");
                    // Add movement logic here
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
                        resultMessage = _healing.ApplyBandage(hero, targetHero);
                    break;
                case PlayerActionType.RearrangeGear:
                    if (primaryTarget is Equipment item && secondaryTarget is ValueTuple<ItemSlot, ItemSlot> slots)
                        resultMessage = _inventory.RearrangeItem(hero, item, slots.Item1, slots.Item2);
                    break;
                case PlayerActionType.IdentifyItem:
                    if (primaryTarget is Equipment itemToIdentify)
                        resultMessage = _identification.IdentifyItem(hero, itemToIdentify);
                    break;
                case PlayerActionType.SetOverwatch:
                    var equippedWeapon = hero.Weapons.FirstOrDefault();
                    if (equippedWeapon == null) return false;
                    if (equippedWeapon is RangedWeapon ranged && !ranged.IsLoaded) return false;
                    hero.Stance = CombatStance.Overwatch;
                    hero.CurrentAP = 0; // End the hero's turn
                    resultMessage = $"{hero.Name} takes an Overwatch stance, ready to react.";
                    break;
            }

            Console.WriteLine($"{hero.Name} performed {actionType}. {hero.CurrentAP} AP remaining.");
            return true;
        }

        /// <summary>
        /// Gets the AP cost for a specific action type.
        /// </summary>
        private int GetActionCost(PlayerActionType actionType)
        {
            return actionType switch
            {
                PlayerActionType.StandardAttack => 1,
                PlayerActionType.Move => 1,
                PlayerActionType.OpenDoor => 1,
                PlayerActionType.SearchFurniture => 1,
                PlayerActionType.HealOther => 1,
                PlayerActionType.SearchRoom => 2,
                PlayerActionType.PickLock => 2,
                PlayerActionType.DisarmTrap => 2,
                PlayerActionType.HealSelf => 2,
                PlayerActionType.RearrangeGear => 2,
                PlayerActionType.IdentifyItem => 0,
                _ => 1,
            };
        }

        private int GetDefaultActionCost(PlayerActionType actionType)
        {
            return actionType switch
            {
                PlayerActionType.SearchRoom => 2,
                PlayerActionType.SearchFurniture => 1,
                PlayerActionType.SearchCorpse => 1,
                PlayerActionType.PickLock => 2,
                PlayerActionType.DisarmTrap => 2,
                _ => 1,
            };
        }
    }
}
