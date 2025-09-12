using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Dungeon
{
    public class WanderingMonsterState
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public DungeonState Dungeon { get; set; }
        public GridPosition CurrentPosition { get; set; } = new GridPosition(0, 0, 0);
        public bool IsAtChasm { get; set; } = false;
        public int RemainingMovement { get; set; } = 4;
        public bool NewRoom { get; set; } = false;
        public bool IsRevealed { get; set; } = false;
        public Room? CurrentRoom => Dungeon.FindRoomAtPosition(CurrentPosition);

        public WanderingMonsterState(DungeonState dungeonState)
        {
            Dungeon = dungeonState;
        }
    }

    public class WanderingMonsterService
    {
        public event Action<Room>? OnSpawnRandomEncounter;
        public WanderingMonsterService()
        {
            
        }

        /// <summary>
        /// Spawns a new wandering monster token at the dungeon's entrance.
        /// </summary>
        public void SpawnWanderingMonster(DungeonState dungeon)
        {
            var newWanderingMonster = new WanderingMonsterState(dungeon);
            dungeon.WanderingMonsters.Add(newWanderingMonster);

            Console.WriteLine("A wandering monster token has been placed at the dungeon entrance.");
        }

        /// <summary>
        /// Moves all active wandering monster tokens according to the rules.
        /// </summary>
        /// <param name="dungeon">The current state of the dungeon.</param>
        /// <returns>True if any monster spotted the party.</returns>
        public async Task<bool> ProcessWanderingMonstersAsync(List<WanderingMonsterState> wanderingMonsters)
        {
            bool partySpotted = false;

            foreach (var monsterState in wanderingMonsters)
            {
                int roll = RandomHelper.RollDie(DiceType.D6);
                var adjacentDoor = GetDoorAdjacentToMonsterLocation(monsterState);
                if (adjacentDoor != null && adjacentDoor.State == DoorState.Closed)
                {
                    if (roll >= 5)
                    {
                        await adjacentDoor.OpenAsync();
                    }
                    else if (adjacentDoor.State != DoorState.MagicallySealed && roll >= 2)
                    {
                        await adjacentDoor.OpenAsync();
                    }
                    else
                    {
                        Console.WriteLine("The wandering monster waits at the closed door.");
                    }
                    continue; // End this monster's turn
                }
                else
                {
                    (bool useContinue, partySpotted) = await MoveWanderingMonsterAsync(partySpotted, monsterState);
                    if (useContinue)
                    {
                        continue;
                    }
                }
            }
            return partySpotted;
        }

        private async Task<(bool useContinue, bool partySpotted)> MoveWanderingMonsterAsync(bool partySpotted, WanderingMonsterState monsterState)
        {
            if (monsterState.Dungeon == null) return (useContinue: true, partySpotted);
            // Find the shortest path to any hero in the same room.
            List<GridPosition> shortestPath = new List<GridPosition>();
            var closestHero = new Hero();
            foreach (var hero in monsterState.Dungeon.HeroParty.Heroes)
            {
                if (hero == null || hero.Position == null || hero.CurrentHP <= 0) continue;

                if (monsterState.CurrentRoom == null || monsterState.CurrentPosition == null) continue;

                List<GridPosition> currentPath = GridService.FindShortestPath(
                    new Monster() 
                    {
                        Position = monsterState.CurrentPosition,
                        CurrentMovePoints = monsterState.RemainingMovement
                    },
                    hero.Position,
                    monsterState.Dungeon.DungeonGrid,
                    monsterState.Dungeon.HeroParty.Heroes.Cast<Character>().ToList()
                    );

                // If this is the first valid path found, or if it's shorter than the previous shortest path
                if (currentPath.Any() && (!shortestPath.Any() || currentPath.Count < shortestPath.Count))
                {
                    shortestPath = currentPath;
                    closestHero = hero; // Update the closest hero reference
                }
            }

            int roll = RandomHelper.RollDie(DiceType.D6);

            if (roll >= 2)
            {
                

                // If a valid path to a hero was found, move the monster
                if (shortestPath.Any() && shortestPath.Count > 1)
                {
                    var roomBeforeMove = monsterState.CurrentRoom;
                    for (int i = 0; i < Math.Min(monsterState.RemainingMovement, shortestPath.Count - 1); i++)
                    {
                        // Move the monster up to 4 squares along the path
                        monsterState.CurrentPosition = shortestPath[i];
                        monsterState.RemainingMovement--;
                        Console.WriteLine($"Wandering monster moves towards the party, now at ({monsterState.CurrentPosition.X}, {monsterState.CurrentPosition.Y}).");

                        if (CheckForReveal(monsterState))
                        {
                            partySpotted = true;
                        }
                        else if (roomBeforeMove != monsterState.CurrentRoom)
                        {
                            return await MoveWanderingMonsterAsync(partySpotted, monsterState);
                        }
                    }
                }
                else
                {
                    // This handles cases where no path exists.
                    Console.WriteLine("Wandering monster has no path to any hero.");
                    return (useContinue: false, partySpotted);
                }
            }
            else 
            {
                // monster moves away form the party
                if (closestHero == null || closestHero.Position == null || monsterState.CurrentPosition == null)
                {
                    // No heroes to move away from, so the monster stays put this turn.
                    Console.WriteLine("Wandering monster has no heroes to move away from.");
                    return (useContinue: false, partySpotted);
                }

                // Get all squares the monster can reach within its movement range.
                var allReachableSquares = GridService.GetAllWalkableSquares(
                    new Monster { Position = monsterState.CurrentPosition, CurrentMovePoints = monsterState.RemainingMovement }, // A temporary monster for pathfinding
                    monsterState.Dungeon.DungeonGrid,
                    monsterState.Dungeon.HeroParty.Heroes.Cast<Character>().ToList()
                );

                if (!allReachableSquares.Any())
                {
                    Console.WriteLine("Wandering monster has nowhere to move.");
                    return (useContinue: false, partySpotted);
                }

                // Find the square that is farthest away from the closest hero.
                var farthestSquare = allReachableSquares.Keys
                    .OrderByDescending(pos => GridService.GetDistance(pos, closestHero.Position))
                    .FirstOrDefault();

                if (farthestSquare == null)
                {
                    Console.WriteLine("Wandering monster could not determine a square to move away to.");
                    return (useContinue: false, partySpotted);
                }

                // Find the path to the farthest square.
                List<GridPosition> retreatPath = GridService.FindShortestPath(
                    new Monster()
                    {
                        Position = monsterState.CurrentPosition,
                        CurrentMovePoints = monsterState.RemainingMovement
                    },
                    farthestSquare,
                    monsterState.Dungeon.DungeonGrid,
                    monsterState.Dungeon.HeroParty.Heroes.Cast<Character>().ToList()
                );

                // Move the monster along the retreat path.
                if (retreatPath.Any() && retreatPath.Count > 1)
                {
                    for (int i = 0; i < Math.Min(monsterState.RemainingMovement, retreatPath.Count - 1); i++)
                    {
                        monsterState.CurrentPosition = retreatPath[i + 1];
                        monsterState.RemainingMovement--;
                        Console.WriteLine($"Wandering monster moves away from the party, now at ({monsterState.CurrentPosition.X}, {monsterState.CurrentPosition.Y}).");
                    }
                }
                else
                {
                    Console.WriteLine("Wandering monster is cornered and cannot move away.");
                }
            }

            return (useContinue: false, partySpotted);
        }

        /// <summary>
        /// Checks if a wandering monster token should be revealed.
        /// </summary>
        private bool CheckForReveal(WanderingMonsterState monsterState)
        {
            // Rule: "If it enters a room from where it has line of sight to
            // the characters and they are within 10 squares, roll on the quest-specific Monster Table".
            if (monsterState.Dungeon != null && monsterState.CurrentRoom != null)
            {
                bool hasLineOfSight = false;
                foreach (var hero in monsterState.Dungeon.HeroParty.Heroes)
                {
                    if (hero.Position != null)
                    {
                        hasLineOfSight = GridService.GetDistance(monsterState.CurrentPosition, hero.Position) <= 10 &&
                                    GridService.HasLineOfSight(monsterState.CurrentPosition, hero.Position, monsterState.Dungeon.DungeonGrid).CanShoot; 
                    }
                }

                if (hasLineOfSight)
                {
                    Console.WriteLine("The wandering monster has found the party!");
                    monsterState.IsRevealed = true;
                    if (OnSpawnRandomEncounter != null)
                    {
                        OnSpawnRandomEncounter(monsterState.CurrentRoom); 
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the specific door a wandering monster is currently waiting at.
        /// </summary>
        /// <param name="monsterState">The state of the wandering monster.</param>
        /// <returns>The Door object the monster is at, or null if it is not at any door.</returns>
        public Door? GetDoorAdjacentToMonsterLocation(WanderingMonsterState monsterState)
        {
            if (monsterState.CurrentRoom == null)
            {
                return null;
            }

            // Iterate through all doors in the monster's current room.
            foreach (var door in monsterState.CurrentRoom.Doors)
            {
                // A door can occupy multiple grid positions (e.g., for double doors).
                // Check if the monster's position is adjacent to any of these positions.
                foreach (var doorPosition in door.PassagewaySquares)
                {
                    if (GridService.IsAdjacent(monsterState.CurrentPosition, doorPosition))
                    {
                        return door; // Found the door the monster is at.
                    }
                }
            }

            return null; // The monster is not currently at a door.
        }
    }
}