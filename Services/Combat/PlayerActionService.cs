using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Combat
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
        OpenDoor,
        PickLock,
        DisarmTrap,
        HealSelf,
        HealOther
    }

    /// <summary>
    /// Handles the execution of actions performed by heroes.
    /// </summary>
    public class PlayerActionService
    {
        private readonly DungeonManagerService _dungeonManager;
        private readonly HeroCombatService _heroCombatService;
        // Inject other services as needed

        public PlayerActionService(DungeonManagerService dungeonManager, HeroCombatService heroCombatService)
        {
            _dungeonManager = dungeonManager;
            _heroCombatService = heroCombatService;
        }

        /// <summary>
        /// Attempts to perform an action for a hero, checking and deducting AP.
        /// </summary>
        /// <param name="hero">The hero performing the action.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <param name="target">The target of the action (e.g., a Monster, DoorChest, or another Hero).</param>
        /// <returns>True if the action was successfully performed, false otherwise.</returns>
        public bool PerformAction(Hero hero, PlayerActionType actionType, object? target = null)
        {
            int apCost = GetActionCost(actionType);
            if (hero.CurrentAP < apCost)
            {
                Console.WriteLine($"{hero.Name} does not have enough AP for {actionType}.");
                return false;
            }

            // Deduct AP before performing the action
            hero.CurrentAP -= apCost;

            // Execute the action logic
            switch (actionType)
            {
                case PlayerActionType.StandardAttack:
                    if (target is Monster monster)
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
                    if (target is DoorChest door)
                    {
                        _dungeonManager.InteractWithDoor(door, hero);
                    }
                    break;

                    // Add cases for other actions here...
                    // case PlayerActionType.SearchRoom:
                    //     _dungeonManager.SearchCurrentRoom(hero);
                    //     break;
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
                _ => 1,
            };
        }
    }
}
