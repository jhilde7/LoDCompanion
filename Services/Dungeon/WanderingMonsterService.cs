using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Models;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    public class WanderingMonsterService
    {
        private readonly EncounterService _encounter;
        private readonly DungeonState _dungeonState;

        public WanderingMonsterService(DungeonState dungeonState, EncounterService encounter)
        {
            _dungeonState = dungeonState;
            _encounter = encounter;
        }

        /// <summary>
        /// Spawns a new wandering monster token at the dungeon's entrance.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        public void SpawnWanderingMonster(DungeonState dungeonState)
        {
            if (dungeonState.StartingRoom == null) return;

            var newWanderingMonster = new WanderingMonsterState();
            dungeonState.WanderingMonsters.Add(newWanderingMonster);

            Console.WriteLine("A wandering monster token has been placed at the dungeon entrance.");
        }

        /// <summary>
        /// Moves all active wandering monster tokens according to the rules.
        /// </summary>
        /// <param name="dungeon">The current state of the dungeon.</param>
        /// <returns>True if any monster spotted the party.</returns>
        public bool MoveWanderingMonsters(DungeonState dungeon)
        {
            bool partySpotted = false;
            if (dungeon.HeroParty == null || !dungeon.HeroParty.Heroes.Any() || dungeon.WanderingMonsters == null)
            {
                return false;
            }

            foreach (var monsterState in dungeon.WanderingMonsters)
            {
                if (monsterState.IsRevealed) continue;

                if (monsterState.IsAtClosedDoor)
                {
                    // Monster is waiting at a door. Roll to see if it breaks through.
                    // Rule: 2-6 on a d6 to open a standard door.
                    if (RandomHelper.RollDie(DiceType.D6) >= 2)
                    {
                        monsterState.IsAtClosedDoor = false;
                        // The door is now considered open. The monster will move on its next turn.
                        Console.WriteLine("The wandering monster breaks through the door!");
                    }
                    else
                    {
                        Console.WriteLine("The wandering monster waits at the closed door.");
                    }
                    continue; // End this monster's turn
                }

                if (monsterState.IsAtChasm)
                {
                    // Monster is at a chasm. Roll to see if it crosses.
                    // Rule: Any roll to continue will get it across, but ends its move for the turn.
                    monsterState.IsAtChasm = false; // It will cross or move on next turn
                    // TODO: define chasm as to eliminate a NoEntry from blocking the A* algoritm
                    // TODO: Find the square on the other side of the chasm and update monsterState.CurrentPosition
                    Console.WriteLine("The wandering monster crosses the chasm and waits.");
                    continue; // End this monster's turn
                }

                int moveRoll = RandomHelper.RollDie(DiceType.D6);
                if (moveRoll >= 2)
                {
                    // Find the shortest path to any hero in the same room.
                    List<GridPosition> shortestPath = new List<GridPosition>();

                    foreach (var hero in dungeon.HeroParty?.Heroes ?? Enumerable.Empty<Hero>())
                    {
                        if (hero == null) continue;
                        if (hero.CurrentHP <= 0) continue;

                        if (monsterState.CurrentRoom == null) continue;

                        List<GridPosition> currentPath = GridService.FindShortestPath(monsterState.CurrentPosition, hero.Position, dungeon.DungeonGrid);

                        // If this is the first valid path found, or if it's shorter than the previous shortest path
                        if (currentPath.Any() && (!shortestPath.Any() || currentPath.Count < shortestPath.Count))
                        {
                            shortestPath = currentPath;
                        }
                    }

                    // If a valid path to a hero was found, move the monster
                    if (shortestPath.Any() && shortestPath.Count > 1)
                    {
                        // Move the monster up to 4 squares along the path
                        int squaresToMove = Math.Min(4, shortestPath.Count - 1);
                        monsterState.CurrentPosition = shortestPath[squaresToMove];
                        Console.WriteLine($"Wandering monster moves towards the party, now at ({monsterState.CurrentPosition.X}, {monsterState.CurrentPosition.Y}).");

                        if (CheckForReveal(monsterState, dungeon))
                        {
                            partySpotted = true;
                        }
                    }
                    else
                    {
                        // This handles cases where no heroes are in the same room, or no path exists.
                        Console.WriteLine("Wandering monster has no path to any hero.");
                        continue;
                    }
                }
                else
                {
                    // TODO: Implement logic to move away from the party
                    Console.WriteLine("Wandering monster token moves away from the party.");
                }
            }
            return partySpotted;
        }

        /// <summary>
        /// Checks if a wandering monster token should be revealed.
        /// </summary>
        private bool CheckForReveal(WanderingMonsterState monsterState, DungeonState dungeonState)
        {
            // Simplified reveal logic. Rule: "If it enters a room from where it has line of sight to
            // the characters and they are within 10 squares, roll on the quest-specific Monster Table".
            if (monsterState.CurrentRoom == dungeonState.CurrentRoom)
            {
                Console.WriteLine("The wandering monster has found the party!");

                // TODO: Replace with a roll on the actual quest's encounter table.
                // For now, we'll just grab a random monster.
                var monsterTemplates = new Dictionary<string, Monster>();
                var weaponTemplates = new Dictionary<string, Weapon>();

                List<Monster> monsters = new List<Monster>();

                if (_dungeonState.Quest != null)
                {
                    monsters = _encounter.GetRandomEncounterByType(_dungeonState.Quest.EncounterType);
                }
                else
                {
                    monsters = _encounter.GetRandomEncounterByType(EncounterType.Beasts);
                }

                if (monsters.Count > 0)
                {
                    monsterState.RevealedMonster = monsters[0];
                    // TODO: Add the revealed monster to the current room's encounter list.
                    // dungeonState.CurrentRoom.Monsters.Add(monsterState.RevealedMonster);
                }
                return true;
            }
            return false;
        }
    }
}