using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Game;
using LoDCompanion.Services.Player;
using LoDCompanion.Utilities;
using System;
using System.Drawing;
using System.Linq;

namespace LoDCompanion.Services.Dungeon
{

    /// <summary>
    /// Represents the outcome of a movement action.
    /// </summary>
    public class MovementResult
    {
        public bool WasSuccessful { get; set; }
        public int MovementPointsSpent { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a 3D coordinate on the game grid.
    /// </summary>
    public class GridPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public GridPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public GridPosition Add(GridPosition other)
        {
            return new GridPosition(X + other.X, Y + other.Y, Z + other.Z);
        }

        public override string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z}";
        }

        public bool Equals(GridPosition? other)
        {
            if (other is null) return false;
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as GridPosition);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }

    /// <summary>
    /// Represents a single square on the room's grid.
    /// </summary>
    public class GridSquare
    {
        public GridPosition Position { get; set; }
        public string? OccupyingCharacterId { get; set; }
        public Furniture? Furniture { get; set; }
        public bool IsWall { get; set; }

        public bool LoSBlocked => IsWall || Furniture != null && Furniture.BlocksLoS;
        public bool MovementBlocked => IsWall || Furniture != null && Furniture.NoEntry;
        public bool DoubleMoveCost => Furniture != null && Furniture.CanBeClimbed; //moving through cost 2x movement
        public bool IsObstacle => Furniture != null && Furniture.IsObstacle; //Affects ranged attacks passing through this square
        public bool IsOccupied => OccupyingCharacterId != null;

        public GridSquare(int x, int y, int z)
        {
            Position = new GridPosition(x, y, z);
        }
    }

    public static class GridService
    {
        /// <summary>
        /// Places a new room onto the global grid at a specific offset.
        /// </summary>
        /// <param name="room">The room to place.</param>
        /// <param name="roomOffset">The global grid position of the room's top-left corner.</param>
        public static void PlaceRoomOnGrid(Room room, GridPosition roomOffset, Dictionary<GridPosition, GridSquare> grid)
        {
            room.GridOffset = roomOffset; // Store the room's global position

            // Assuming the room itself is a flat 2D layout being placed at a specific Z level
            for (int y = 0; y < room.Height; y++)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    // UPDATED: The Z coordinate from the offset is now used.
                    var globalPos = new GridPosition(roomOffset.X + x, roomOffset.Y + y, roomOffset.Z);

                    if (!grid.ContainsKey(globalPos))
                    {
                        // UPDATED: The GridSquare is initialized with its full 3D position.
                        var square = new GridSquare(globalPos.X, globalPos.Y, globalPos.Z);

                        // Your existing logic for placing walls and furniture...
                        grid[globalPos] = square;
                    }
                }
            }
        }

        public static GridSquare? GetSquareAt(GridPosition position, Dictionary<GridPosition, GridSquare> grid)
        {
            grid.TryGetValue(position, out var square);
            return square;
        }

        /// <summary>
        /// Helper to check if two positions are adjacent (not diagonal).
        /// </summary>
        public static bool IsAdjacent(GridPosition pos1, GridPosition pos2)
        {
            int dist = Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y) + Math.Abs(pos1.Z - pos2.Z);
            return dist == 1;
        }

        /// <summary>
        /// Moves a character along a given path, one square at a time, consuming movement points.
        /// The character will stop if they run out of movement, the path is blocked, or an event interrupts them.
        /// </summary>
        /// <returns>A MovementResult detailing how far the character moved and how many points were spent.</returns>
        public static MovementResult MoveCharacter(Character character, List<GridPosition> path, Dictionary<GridPosition, GridSquare> grid, List<Character> enemies, int maxMovementPoints)
        {
            var result = new MovementResult();
            if (path == null || path.Count <= 1)
            {
                result.Message = "No valid path to move along.";
                return result;
            }
            if (character.Position == null) return result;

            var originalPosition = character.Position;

            // Start from the second square in the path, as the first is the character's current location.
            foreach (var nextPos in path.Skip(1))
            {
                var nextSquare = GetSquareAt(nextPos, grid);

                if (nextSquare == null || nextSquare.MovementBlocked || nextSquare.IsOccupied)
                {
                    result.Message = $"{character.Name}'s path is blocked at {nextPos}.";
                    break; // End movement here.
                }

                int costForThisSquare = nextSquare.DoubleMoveCost ? 2 : 1;
                foreach (var enemy in enemies)
                {
                    if (DirectionService.IsInZoneOfControl(nextPos, enemy))
                    {
                        costForThisSquare = 2;
                        break;
                    }
                }

                if (result.MovementPointsSpent + costForThisSquare > maxMovementPoints)
                {
                    result.Message = $"{character.Name} does not have enough movement points to enter {nextPos}.";
                    break;
                }

                // --- Commit the single step ---
                result.MovementPointsSpent += costForThisSquare;

                // Vacate the old square(s)
                foreach (var oldSquarePos in character.OccupiedSquares)
                {
                    var oldSquare = GetSquareAt(oldSquarePos, grid);
                    if (oldSquare != null) oldSquare.OccupyingCharacterId = null;
                }

                // Update character's position and occupy the new square(s)
                character.Position = nextPos;
                character.UpdateOccupiedSquares();
                foreach (var newSquarePos in character.OccupiedSquares)
                {
                    var newSquare = GetSquareAt(newSquarePos, grid);
                    if (newSquare != null) newSquare.OccupyingCharacterId = character.Id;
                }
            }

            if (!character.Position.Equals(originalPosition))
            {
                result.WasSuccessful = true;
                result.Message += $" {character.Name} moved to {character.Position}, spending {result.MovementPointsSpent} movement points.";
            }

            return result;
        }

        /// <summary>
        /// Moves a character to a new position on the global dungeon grid.
        /// </summary>
        public static bool MoveCharacterToPosition(Character character, GridPosition newPosition, Dictionary<GridPosition, GridSquare> grid)
        {
            var targetSquare = GetSquareAt(newPosition, grid);
            // The check is now much cleaner!
            if (targetSquare == null || targetSquare.MovementBlocked || targetSquare.IsOccupied)
            {
                return false;
            }

            if (character.Position == null) return false;

            var oldSquare = GetSquareAt(character.Position, grid);
            if (oldSquare != null) oldSquare.OccupyingCharacterId = null;

            character.Position = newPosition;
            character.UpdateOccupiedSquares();
            foreach (var newSquarePos in character.OccupiedSquares)
            {
                var newSquare = GetSquareAt(newSquarePos, grid);
                if (newSquare == null || newSquare.IsOccupied)
                {
                    // This is a safety check in case a multi-tile unit's new footprint is invalid.
                    // A more robust implementation would check the whole footprint before committing the move.
                    return false;
                }
                newSquare.OccupyingCharacterId = character.Id;
            }

            return true;
        }

        /// <summary>
        /// Calculates the distance between two points (Manhattan distance).
        /// </summary>
        public static int GetDistance(GridPosition start, GridPosition end)
        {
            return Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) + Math.Abs(start.Z - end.Z);
        }

        public static LineOfSightResult HasLineOfSight(GridPosition start, GridPosition end, Dictionary<GridPosition, GridSquare> grid)
        {
            var result = new LineOfSightResult();
            var line = GetLine(start, end);

            // Skip the first (start) and last (end) squares in the line.
            foreach (var pos in line.Skip(1).SkipLast(1))
            {
                var square = GetSquareAt(pos, grid);

                // If the square doesn't exist or has LoS-blocking furniture/walls.
                if (square == null || square.LoSBlocked)
                {
                    result.IsBlocked = true;
                    return result; // The first thing that blocks LoS stops the check.
                }

                // Check for obstructions that apply a penalty but don't fully block.
                if (square.IsObstacle || square.IsOccupied)
                {
                    result.ObstructionPenalty -= 10;
                }
            }

            // If we checked all squares and none fully blocked the line.
            result.ClearShot = result.ObstructionPenalty == 0;
            return result;
        }

        /// <summary>
        /// Finds the shortest path between two points using the A* algorithm,
        /// now correctly using your existing Node class and helper methods.
        /// </summary>
        public static List<GridPosition> FindShortestPath(GridPosition start, GridPosition end, Dictionary<GridPosition, GridSquare> grid)
        {

            // The PriorityQueue stores nodes to visit, prioritized by their F-Score.
            var openSet = new PriorityQueue<Node, int>();

            // This dictionary tracks the lowest G-Score found so far for each position.
            // This is our replacement for searching the openSet.
            var gScores = new Dictionary<GridPosition, int>();

            // The set of positions that have already been evaluated.
            var closedSet = new HashSet<GridPosition>();

            // Initialize with the starting node.
            var startNode = new Node(start, null, 0, GetDistance(start, end));
            openSet.Enqueue(startNode, startNode.FScore);
            gScores[start] = 0;

            while (openSet.Count > 0)
            {
                var currentNode = openSet.Dequeue();

                // If we've already processed this node, skip it.
                // This handles cases where we've added a duplicate node to the queue.
                if (!closedSet.Add(currentNode.Position))
                {
                    continue;
                }

                // If we've reached the end, reconstruct and return the path.
                if (currentNode.Position.Equals(end))
                {
                    return ReconstructPath(currentNode);
                }

                foreach (var neighborPos in GetNeighbors(currentNode.Position, grid))
                {
                    var neighborSquare = GetSquareAt(neighborPos, grid);
                    if (neighborSquare == null) continue; // Should not happen if GetNeighbors is correct

                    // Calculate the cost to move to this neighbor.
                    int movementCost = neighborSquare.DoubleMoveCost ? 2 : 1;
                    int tentativeGScore = currentNode.GScore + movementCost;

                    // Check if this new path to the neighbor is better than any previous one.
                    // gScores.GetValueOrDefault returns 0 if the key doesn't exist, which is fine,
                    // but checking for the key explicitly or setting a high default is safer.
                    int existingGScore = gScores.GetValueOrDefault(neighborPos, int.MaxValue);

                    if (tentativeGScore < existingGScore)
                    {
                        // This path is the best one found so far. Record it.
                        gScores[neighborPos] = tentativeGScore;

                        var neighborNode = new Node(neighborPos, currentNode, tentativeGScore, GetDistance(neighborPos, end));
                        openSet.Enqueue(neighborNode, neighborNode.FScore);
                    }
                }
            }

            // No path could be found.
            return new List<GridPosition>();
        }

        /// <summary>
        /// Checks for a clear path, using the `IsObstacle` property for ranged attacks.
        /// </summary>
        public static bool HasClearPath(GridPosition start, GridPosition end, Dictionary<GridPosition, GridSquare> grid)
        {
            var line = GetLine(start, end);

            // Skip the first (start) and last (end) squares in the line.
            foreach (var pos in line.Skip(1).SkipLast(1))
            {
                var square = GetSquareAt(pos, grid);

                // Path is blocked if a square is missing, a wall, or has movement-blocking furniture.
                if (square == null || square.MovementBlocked)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Reconstructs the path from the end node back to the start.
        /// </summary>
        private static List<GridPosition> ReconstructPath(Node? node)
        {
            var path = new List<GridPosition>();
            while (node != null)
            {
                path.Add(node.Position);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Gets the walkable neighbors of a given position.
        /// </summary>
        public static IEnumerable<GridPosition> GetNeighbors(GridPosition position, Dictionary<GridPosition, GridSquare> grid)
        {
            // Define potential neighbors in 6 directions (4 horizontal, 2 vertical)
            var directions = new GridPosition[]
            {
                new GridPosition(1, 0, 0),  // East
                new GridPosition(-1, 0, 0), // West
                new GridPosition(0, 1, 0),  // North
                new GridPosition(0, -1, 0), // South
                new GridPosition(0, 0, 1),  // Up
                new GridPosition(0, 0, -1)  // Down
            };

            foreach (var dir in directions)
            {
                var newPos = new GridPosition(position.X + dir.X, position.Y + dir.Y, position.Z + dir.Z);
                var square = GetSquareAt(newPos, grid);

                // A square is a valid neighbor if it exists and is not blocked.
                // You could add more complex rules here for vertical movement,
                // e.g., requiring "Stairs" or a "Ladder" to move up or down.
                if (square != null && !square.MovementBlocked)
                {
                    yield return newPos;
                }
            }
        }

        // --- Helper Class for A* ---
        private class Node
        {
            public GridPosition Position { get; }
            public Node? Parent { get; set; }
            public int GScore { get; set; } // Cost from start to current node
            public int HScore { get; set; } // Heuristic cost from current node to end
            public int FScore => GScore + HScore; // Total cost

            public Node(GridPosition position, Node? parent, int gScore, int hScore)
            {
                Position = position;
                Parent = parent;
                GScore = gScore;
                HScore = hScore;
            }
        }

        /// <summary>
        /// Gets all grid points along a 3D line using Bresenham's 3D algorithm.
        /// </summary>
        /// <returns>An enumerable list of GridPositions forming the line.</returns>
        private static IEnumerable<GridPosition> GetLine(GridPosition start, GridPosition end)
        {
            int x1 = start.X, y1 = start.Y, z1 = start.Z;
            int x2 = end.X, y2 = end.Y, z2 = end.Z;

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int dz = Math.Abs(z2 - z1);

            int sx = (x1 < x2) ? 1 : -1;
            int sy = (y1 < y2) ? 1 : -1;
            int sz = (z1 < z2) ? 1 : -1;

            yield return new GridPosition(x1, y1, z1);

            if (dx >= dy && dx >= dz) // X-dominant
            {
                int err1 = 2 * dy - dx;
                int err2 = 2 * dz - dx;
                while (x1 != x2)
                {
                    if (err1 >= 0) { y1 += sy; err1 -= 2 * dx; }
                    if (err2 >= 0) { z1 += sz; err2 -= 2 * dx; }
                    err1 += 2 * dy;
                    err2 += 2 * dz;
                    x1 += sx;
                    yield return new GridPosition(x1, y1, z1);
                }
            }
            else if (dy >= dx && dy >= dz) // Y-dominant
            {
                int err1 = 2 * dx - dy;
                int err2 = 2 * dz - dy;
                while (y1 != y2)
                {
                    if (err1 >= 0) { x1 += sx; err1 -= 2 * dy; }
                    if (err2 >= 0) { z1 += sz; err2 -= 2 * dy; }
                    err1 += 2 * dx;
                    err2 += 2 * dz;
                    y1 += sy;
                    yield return new GridPosition(x1, y1, z1);
                }
            }
            else // Z-dominant
            {
                int err1 = 2 * dy - dz;
                int err2 = 2 * dx - dz;
                while (z1 != z2)
                {
                    if (err1 >= 0) { y1 += sy; err1 -= 2 * dz; }
                    if (err2 >= 0) { x1 += sx; err2 -= 2 * dz; }
                    err1 += 2 * dy;
                    err2 += 2 * dx;
                    z1 += sz;
                    yield return new GridPosition(x1, y1, z1);
                }
            }
        }

        public static void GenerateGridForRoom(Room room)
        {
            if (room.Size == null || room.Size.Length != 2)
            {
                return;
            }

            int width = room.Size[0];
            int height = room.Size[1];

            var grid = new Furniture[width, height, 10];

            foreach (var furniture in room.FurnitureList)
            {
                foreach (var position in furniture.OccupiedSquares)
                {
                    if (position.X >= 0 && position.X < width && position.Y >= 0 && position.Y < height)
                    {
                        grid[position.X, position.Y, position.Z] = furniture;
                    }
                }
            }
        }

        public static List<GridPosition> GetAllWalkableSquares(Room room, IGameEntity entity, Dictionary<GridPosition, GridSquare> grid)
        {
            if(entity.Position == null) return new List<GridPosition>();
            var reachableSquares = new List<GridPosition>();
            // 'visited' tracks positions we've seen and the cost to reach them.
            var visited = new Dictionary<GridPosition, int>();
            var queue = new Queue<GridPosition>();

            // Start at the entity's current position with a cost of 0.
            queue.Enqueue(entity.Position);
            visited[entity.Position] = 0;

            while (queue.Count > 0)
            {
                var currentPos = queue.Dequeue();
                var currentCost = visited[currentPos];

                // Get all valid, walkable neighbors from the current position.
                foreach (var neighborPos in GetNeighbors(currentPos, grid))
                {
                    // Calculate the cost to move into this neighbor square.
                    var neighborSquare = GetSquareAt(neighborPos, grid);
                    if (neighborSquare == null) continue;

                    // This is the base cost to enter the square.
                    int movementCost = neighborSquare.DoubleMoveCost ? 2 : 1;

                    // Check if the neighbor square is in the ZOC of any enemy in the room.
                    if (room.MonstersInRoom != null)
                    {
                        foreach (var monster in room.MonstersInRoom)
                        {
                            if (DirectionService.IsInZoneOfControl(neighborPos, monster))
                            {
                                // Moving through ZOC costs 2 Movement Points.
                                movementCost = 2;
                                break; // The penalty is applied once.
                            }
                        }
                    }

                    int newCost = currentCost + movementCost;

                    if (entity is Character character && newCost <= character.GetStat(BasicStat.Move))
                    {
                        // ...and we haven't found a cheaper path to this square already...
                        if (!visited.ContainsKey(neighborPos) || newCost < visited[neighborPos])
                        {
                            // ...then this is a valid square to move to.
                            visited[neighborPos] = newCost;
                            queue.Enqueue(neighborPos);
                            reachableSquares.Add(neighborPos);
                        }
                    }
                }
            }

            // Return all the unique positions found within the movement range.
            return reachableSquares.Distinct().ToList();
        }

        internal static List<GridPosition> GetAllSquaresInRadius(GridPosition currentCenter, int areaOfEffectRadius, Dictionary<GridPosition, GridSquare> grid)
        {
            HashSet<GridPosition> affectedSquares = new HashSet<GridPosition>();
            affectedSquares.Add(currentCenter); // Always include the center

            if (areaOfEffectRadius <= 0) return affectedSquares.ToList();

            Queue<Tuple<GridPosition, int>> queue = new Queue<Tuple<GridPosition, int>>();
            queue.Enqueue(Tuple.Create(currentCenter, 0)); // Position, current_distance

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                GridPosition currentPos = current.Item1;
                int currentDistance = current.Item2;

                if (currentDistance >= areaOfEffectRadius) continue;

                foreach (var neighbor in GetNeighbors(currentPos, grid)) // Use your actual GetNeighbors logic
                {
                    if (affectedSquares.Add(neighbor)) // Add returns true if it's a new element
                    {
                        queue.Enqueue(Tuple.Create(neighbor, currentDistance + 1));
                    }
                }
            }
            return affectedSquares.ToList();
        }
    }


    public class LineOfSightResult
    {
        public bool IsBlocked { get; set; }
        public int ObstructionPenalty { get; set; }
        public bool ClearShot { get; set; }
        public bool CanShoot => !IsBlocked;
    }
}
