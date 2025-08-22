using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Numerics;

namespace LoDCompanion.BackEnd.Services.Game
{
    public enum PlacementRule
    {
        // --- Hero & Monster Rules ---
        Center,
        Door,
        ShortSide,
        LongSide,
        RandomEdge,

        // --- Target-Based Rules ---
        RelativeToTarget,
        AsFarAsPossible
    }

    public class PlacementService
    {
        private readonly WorldStateService _worldState;
        private readonly DungeonState _dungeon;

        public PlacementService( WorldStateService worldStateService, DungeonState dungeon)
        {
            _worldState = worldStateService;
            _dungeon = dungeon;
        }

        /// <summary>
        /// Places an entity in a room according to a set of data-driven placement parameters.
        /// </summary>
        public void PlaceEntity(IGameEntity entity, Room room, Dictionary<string, string> placementParams)
        {
            if (!Enum.TryParse<PlacementRule>(placementParams.GetValueOrDefault("PlacementRule"), out var rule))
            {
                return;
            }

            Console.WriteLine($"Placing {entity.Name} with rule: {rule}");
            List<GridPosition> potentialPositions = new List<GridPosition>();


            switch (rule)
            {
                case PlacementRule.Center:
                    potentialPositions = GetCenterPositions(room, entity);
                    break;

                case PlacementRule.ShortSide:
                    potentialPositions = GetEdgePositions(room, entity, isShortSide: true);
                    potentialPositions.Shuffle();
                    break;

                case PlacementRule.LongSide:
                    potentialPositions = GetEdgePositions(room, entity, isShortSide: false);
                    potentialPositions.Shuffle();
                    break;

                case PlacementRule.RandomEdge:
                    potentialPositions = GetEdgePositions(room, entity, isShortSide: null);
                    potentialPositions.Shuffle();
                    break;

                case PlacementRule.RelativeToTarget:
                    string targetName = placementParams["PlacementTarget"];
                    IGameEntity? targetEntity = _worldState.FindEntityInRoomById(room, targetName);
                    if (targetEntity != null)
                    {
                        potentialPositions = GetPositionsNearTarget(entity, targetEntity, _dungeon);
                    }
                    break;

                case PlacementRule.AsFarAsPossible:
                    string awayFromName = placementParams["PlacementTarget"];
                    IGameEntity? awayFromEntity = _worldState.FindEntityInRoomById(room, awayFromName);
                    if (awayFromEntity != null)
                    {
                        potentialPositions = GetFarAwayPositions(room, entity, awayFromEntity);
                    }
                    break;
            }

            if (potentialPositions.Any())
            {
                foreach (var position in potentialPositions)
                {
                    if (AttemptFinalPlacement(entity, position, _dungeon))
                    {
                        if (entity is Character character)
                        {
                            character.Room = room;
                        }
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Warning: Could not find any valid placement positions for {entity.Name} with rule {rule}. Attempting fallback.");
                // Fallback: Try to place anywhere in the room.
                potentialPositions = FindAllValidSquaresForEntity(room, entity);
                if (potentialPositions.Any())
                {
                    potentialPositions.Shuffle();
                    foreach (var position in potentialPositions)
                    {
                        if (AttemptFinalPlacement(entity, position, _dungeon))
                        {
                            if (entity is Character character)
                            {
                                character.Room = room;
                            }
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"FATAL: No space available in room for entity {entity.Name}.");
                }
            }
        }

        // --- Private Helper Methods for Placement Logic ---

        private List<GridPosition> GetCenterPositions(Room room, IGameEntity entity)
        {
            var potentialPositions = new List<GridPosition>();
            int centerX = room.Width / 2;
            int centerY = room.Height / 2;
            var centerPos = new GridPosition(centerX, centerY, room.GridOffset.Z);

            potentialPositions.Add(centerPos);

            // The loop's limit is half the largest room dimension, ensuring we search the whole room.
            int maxRadius = Math.Max(room.Width, room.Height) / 2 + 1;

            for (int r = 1; r < maxRadius; r++)
            {
                // Top edge of the spiral ring
                for (int x = centerX - r; x <= centerX + r; x++)
                {
                    potentialPositions.Add(new GridPosition(x, centerY - r, centerPos.Z));
                }
                // Bottom edge
                for (int x = centerX - r; x <= centerX + r; x++)
                {
                    potentialPositions.Add(new GridPosition(x, centerY + r, centerPos.Z));
                }
                // Left edge
                for (int y = centerY - r + 1; y < centerY + r; y++)
                {
                    potentialPositions.Add(new GridPosition(centerX - r, y, centerPos.Z));
                }
                // Right edge
                for (int y = centerY - r + 1; y < centerY + r; y++)
                {
                    potentialPositions.Add(new GridPosition(centerX + r, y, centerPos.Z));
                }
            }

            return potentialPositions;
        }


        private List<GridPosition> GetEdgePositions(Room room, IGameEntity entity, bool? isShortSide)
        {
            var edgeSquares = new List<GridPosition>();
            bool isWidthShort = room.Width < room.Height;

            // Determine which edges to check. If isShortSide is null, check all edges.
            bool checkHorizontal = isShortSide == null || isShortSide == true && isWidthShort || isShortSide == false && !isWidthShort;
            bool checkVertical = isShortSide == null || isShortSide == true && !isWidthShort || isShortSide == false && isWidthShort;

            if (checkHorizontal) // Top and Bottom edges
            {
                for (int x = 0; x < room.Width; x++)
                {
                    edgeSquares.Add(new GridPosition(x, 0, room.GridOffset.Z));
                    edgeSquares.Add(new GridPosition(x, room.Height - 1, room.GridOffset.Z));
                }
            }
            if (checkVertical) // Left and Right edges
            {
                for (int y = 0; y < room.Height; y++)
                {
                    edgeSquares.Add(new GridPosition(0, y, room.GridOffset.Z));
                    edgeSquares.Add(new GridPosition(room.Width - 1, y, room.GridOffset.Z));
                }
            }

            return edgeSquares;
        }

        private List<GridPosition> GetPositionsNearTarget(IGameEntity entity, IGameEntity target, DungeonState dungeon)
        {
            var surroundingPositions = new List<GridPosition>();
            foreach (var occupiedSquare in target.OccupiedSquares)
            {
                surroundingPositions.AddRange(GridService.GetNeighbors(occupiedSquare, dungeon.DungeonGrid));
            }
            return surroundingPositions;
        }

        private List<GridPosition> GetFarAwayPositions(Room room, IGameEntity entity, IGameEntity awayFromTarget)
        {
            var allValidSquares = FindAllValidSquaresForEntity(room, entity);
            if (!allValidSquares.Any()) return new List<GridPosition>();

            // Find the single square with the maximum distance to the target.
            var farthestSquare = allValidSquares.OrderByDescending(p => GridService.GetDistance(p ?? new GridPosition(0, 0, 0), awayFromTarget.Position ?? new GridPosition(0, 0, 0))).FirstOrDefault();
            return farthestSquare != null ? new List<GridPosition> { farthestSquare } : new List<GridPosition>();
        }

        private List<GridPosition> FindAllValidSquaresForEntity(Room room, IGameEntity entity)
        {
            var validSquares = new List<GridPosition>();
            for (int y = 0; y < room.Height; y++)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    var position = new GridPosition(room.GridOffset.X + x, room.GridOffset.Y + y, room.GridOffset.Z);
                    validSquares.Add(position);
                }
            }
            return validSquares;
        }

        private bool IsPlacementFootprintValid(IGameEntity entity, GridPosition targetPosition, DungeonState dungeon)
        {
            entity.Position = targetPosition;

            if (entity is Character character) character.UpdateOccupiedSquares();

            foreach (var squareCoords in entity.OccupiedSquares)
            {
                var square = GridService.GetSquareAt(squareCoords, dungeon.DungeonGrid);
                if (square == null || square.IsWall || square.IsOccupied || square.MovementBlocked)
                {
                    return false;
                }
            }
            return true;
        }

        private bool AttemptFinalPlacement(IGameEntity entity, GridPosition targetPosition, DungeonState dungeon)
        {
            if (!IsPlacementFootprintValid(entity, targetPosition, dungeon))
            {
                Console.WriteLine($"Final placement check failed for {entity.Name} id: {entity.Id} at {targetPosition.ToString()}.");
                return false;
            }

            foreach (var squareCoords in entity.OccupiedSquares)
            {
                var square = GridService.GetSquareAt(squareCoords, dungeon.DungeonGrid);
                if (square != null)
                {
                    square.OccupyingCharacterId = entity.Id;
                }
            }

            Console.WriteLine($"{entity.Name} id: {entity.Id} at {targetPosition.ToString()}!");
            return true;
        }

        /// <summary>
        /// Places a new exit door on a random, available edge of a room.
        /// </summary>
        /// <param name="door">The door object to be placed.</param>
        /// <param name="room">The room to place the door on.</param>
        public void PlaceExitDoor(Door door, Room room)
        {
            var existingOrientations = room.Doors.Select(d => d.Orientation).ToHashSet();
            var availableOrientations = Enum.GetValues(typeof(Orientation))
                                            .Cast<Orientation>()
                                            .Where(o => !existingOrientations.Contains(o))
                                            .ToList();

            if (!availableOrientations.Any())
            {
                Console.WriteLine($"Warning: No available edges in {room.Name} to place a new door.");
                return;
            }

            availableOrientations.Shuffle();
            var chosenOrientation = availableOrientations.First();
            door.Orientation = chosenOrientation;

            List<GridPosition> edgeSquares = GridService.GetEdgeSquaresForOrientation(room, chosenOrientation);

            // Find a valid 2-square segment on the chosen edge
            for (int i = 0; i < edgeSquares.Count - 1; i++)
            {
                GridPosition pos1 = edgeSquares[i];
                GridPosition pos2 = edgeSquares[i + 1];

                // The squares immediately inside the room must also be clear
                if (!IsDoorwaySpaceClear(pos1, chosenOrientation) || !IsDoorwaySpaceClear(pos2, chosenOrientation))
                {
                    continue;
                }

                // If all checks pass, this is a valid spot
                door.PassagewaySquares = new List<GridPosition> { pos1, pos2 };
                return;
            }

            // Fallback if no 2-square opening is found (e.g., due to furniture)
            Console.WriteLine($"Warning: Could not find a valid 2-square opening on the {chosenOrientation} edge of {room.Name}.");
        }


        /// <summary>
        /// Checks if an edge square and its immediate interior neighbor are both clear.
        /// </summary>
        private bool IsDoorwaySpaceClear(GridPosition edgePos, Orientation edgeOrientation)
        {
            // Check the edge square itself.
            var edgeSquare = GridService.GetSquareAt(edgePos, _dungeon.DungeonGrid);
            if (edgeSquare == null || edgeSquare.IsWall || edgeSquare.Furniture != null)
            {
                return false; // Edge is blocked by a wall or furniture.
            }

            // Determine the position of the square immediately inside the room.
            GridPosition interiorPos;
            switch (edgeOrientation)
            {
                case Orientation.North: interiorPos = new GridPosition(edgePos.X, edgePos.Y + 1, edgePos.Z); break;
                case Orientation.South: interiorPos = new GridPosition(edgePos.X, edgePos.Y - 1, edgePos.Z); break;
                case Orientation.East: interiorPos = new GridPosition(edgePos.X - 1, edgePos.Y, edgePos.Z); break;
                case Orientation.West: interiorPos = new GridPosition(edgePos.X + 1, edgePos.Y, edgePos.Z); break;
                default: return false;
            }

            // Check if the interior square is blocked.
            var interiorSquare = GridService.GetSquareAt(interiorPos, _dungeon.DungeonGrid);
            if (interiorSquare == null || interiorSquare.MovementBlocked)
            {
                return false; // Interior space is blocked.
            }

            // Both the edge and the space behind it are clear.
            return true;
        }
    }
}
