using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Utilities;
using System.Numerics;

namespace LoDCompanion.Services.Game
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
                    IGameEntity? targetEntity = _worldState.FindEntityInRoomByName(room, targetName);
                    if (targetEntity != null)
                    {
                        potentialPositions = GetPositionsNearTarget(entity, targetEntity, _dungeon);
                    }
                    break;

                case PlacementRule.AsFarAsPossible:
                    string awayFromName = placementParams["PlacementTarget"];
                    IGameEntity? awayFromEntity = _worldState.FindEntityInRoomByName(room, awayFromName);
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
            bool checkHorizontal = (isShortSide == null) || (isShortSide == true && isWidthShort) || (isShortSide == false && !isWidthShort);
            bool checkVertical = (isShortSide == null) || (isShortSide == true && !isWidthShort) || (isShortSide == false && isWidthShort);

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
            var farthestSquare = allValidSquares.OrderByDescending(p => GridService.GetDistance(p, awayFromTarget.Position)).FirstOrDefault();
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
    }
}
