using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;
using System.Drawing;
using System.Linq;

namespace LoDCompanion.Services.Dungeon
{
    public class GridService
    {
        public Dictionary<GridPosition, GridSquare> DungeonGrid { get; private set; } = new Dictionary<GridPosition, GridSquare>();


        public GridService() { }

        /// <summary>
        /// Places a new room onto the global grid at a specific offset.
        /// </summary>
        /// <param name="room">The room to place.</param>
        /// <param name="roomOffset">The global grid position of the room's top-left corner.</param>
        public void PlaceRoomOnGrid(Room room, GridPosition roomOffset)
        {
            room.GridOffset = roomOffset; // Store the room's global position

            // Assuming the room itself is a flat 2D layout being placed at a specific Z level
            for (int y = 0; y < room.Height; y++)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    // UPDATED: The Z coordinate from the offset is now used.
                    var globalPos = new GridPosition(roomOffset.X + x, roomOffset.Y + y, roomOffset.Z);

                    if (!DungeonGrid.ContainsKey(globalPos))
                    {
                        // UPDATED: The GridSquare is initialized with its full 3D position.
                        var square = new GridSquare(globalPos.X, globalPos.Y, globalPos.Z);

                        // Your existing logic for placing walls and furniture...
                        DungeonGrid[globalPos] = square;
                    }
                }
            }
        }

        public GridSquare? GetSquareAt(GridPosition position)
        {
            DungeonGrid.TryGetValue(position, out var square);
            return square;
        }

        /// <summary>
        /// Moves a character to a new position on the global dungeon grid.
        /// </summary>
        public bool MoveCharacter(Character character, GridPosition newPosition)
        {
            var targetSquare = GetSquareAt(newPosition);
            // The check is now much cleaner!
            if (targetSquare == null || targetSquare.MovementBlocked || targetSquare.IsOccupied)
            {
                return false;
            }

            if (character.Position != null)
            {
                var oldSquare = GetSquareAt(character.Position);
                if (oldSquare != null) oldSquare.OccupyingCharacterId = null;
            }

            targetSquare.OccupyingCharacterId = character.Id;
            character.Position = newPosition;
            return true;
        }

        /// <summary>
        /// Calculates the distance between two points (Manhattan distance).
        /// </summary>
        public int GetDistance(GridPosition start, GridPosition end)
        {
            return Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) + Math.Abs(start.Z - end.Z);
        }

        public LineOfSightResult HasLineOfSight(GridPosition start, GridPosition end)
        {
            var result = new LineOfSightResult();
            var line = GetLine(start, end);

            // Skip the first (start) and last (end) squares in the line.
            foreach (var pos in line.Skip(1).SkipLast(1))
            {
                var square = GetSquareAt(pos);

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
        /// Attempts to shove a target character one square back.
        /// </summary>
        /// <param name="shover">The character performing the shove.</param>
        /// <param name="target">The character being shoved.</param>
        /// <param name="room">The current room grid.</param>
        /// <returns>A string describing the outcome.</returns>
        public string ShoveCharacter(Character shover, Character target, RoomService room)
        {
            if (target.IsLarge) return $"{shover.Name} tries to shove {target.Name}, but they are too large to be moved!";

            int shoveRoll = RandomHelper.RollDie("D100");
            int shoveBonus = shover.DamageBonus * 10;

            if (shoveRoll > target.Dexterity + shoveBonus)
            {
                return $"{shover.Name}'s shove attempt fails.";
            }

            // Shove is successful. Calculate pushback direction.
            int dx = target.Position.X - shover.Position.X;
            int dy = target.Position.Y - shover.Position.Y;
            int dz = target.Position.Z - shover.Position.Z;

            int shoveX = 0, shoveY = 0, shoveZ = 0;

            // Determine the primary axis of the shove
            if (Math.Abs(dx) >= Math.Abs(dy) && Math.Abs(dx) >= Math.Abs(dz))
            {
                shoveX = Math.Sign(dx); // Shove is primarily horizontal (X)
            }
            else if (Math.Abs(dy) >= Math.Abs(dx) && Math.Abs(dy) >= Math.Abs(dz))
            {
                shoveY = Math.Sign(dy); // Shove is primarily horizontal (Y)
            }
            else
            {
                shoveZ = Math.Sign(dz); // Shove is primarily vertical
            }

            var pushbackPosition = new GridPosition(
                target.Position.X + shoveX,
                target.Position.Y + shoveY,
                target.Position.Z + shoveZ
            );

            var pushbackSquare = GetSquareAt(pushbackPosition);

            if (pushbackSquare != null && !pushbackSquare.MovementBlocked && !pushbackSquare.IsOccupied)
            {
                MoveCharacter(target, pushbackPosition);
                return $"{shover.Name} successfully shoves {target.Name} back!";
            }
            else
            {
                // TODO: Add a "Prone" status effect to the target.
                return $"{shover.Name} shoves {target.Name}, but they are blocked and fall over!";
            }
        }

        /// <summary>
        /// Finds the shortest path between two points using the A* algorithm,
        /// now correctly using your existing Node class and helper methods.
        /// </summary>
        public List<GridPosition> FindShortestPath(GridPosition start, GridPosition end)
        {
            var openSet = new List<Node>();
            var closedSet = new HashSet<GridPosition>();

            var startNode = new Node(start, null, 0, GetDistance(start, end));
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.OrderBy(node => node.FScore).First();

                if (currentNode.Position.Equals(end))
                {
                    // Path found, reconstruct and return it.
                    return ReconstructPath(currentNode);
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode.Position);

                foreach (var neighborPos in GetNeighbors(currentNode.Position))
                {
                    if (closedSet.Contains(neighborPos))
                    {
                        continue; // Ignore already evaluated positions.
                    }

                    var neighborSquare = GetSquareAt(neighborPos);
                    // This check is now redundant if GetNeighbors already does it, but it's safe to keep.
                    if (neighborSquare == null || neighborSquare.MovementBlocked)
                    {
                        continue;
                    }

                    // Calculate the cost to move to the neighbor.
                    int movementCost = neighborSquare.DoubleMoveCost ? 2 : 1;
                    int tentativeGScore = currentNode.GScore + movementCost;

                    var neighborNode = openSet.FirstOrDefault(n => n.Position.Equals(neighborPos));

                    if (neighborNode == null)
                    {
                        // This is a new node.
                        neighborNode = new Node(neighborPos, currentNode, tentativeGScore, GetDistance(neighborPos, end));
                        openSet.Add(neighborNode);
                    }
                    else if (tentativeGScore < neighborNode.GScore)
                    {
                        // We found a better path to this existing node.
                        neighborNode.Parent = currentNode;
                        neighborNode.GScore = tentativeGScore;
                    }
                }
            }

            // No path could be found.
            return new List<GridPosition>();
        }

        /// <summary>
        /// Checks for a clear path, now using the `IsObstacle` property for ranged attacks.
        /// </summary>
        public bool HasClearPath(GridPosition start, GridPosition end)
        {
            var line = GetLine(start, end);

            // Skip the first (start) and last (end) squares in the line.
            foreach (var pos in line.Skip(1).SkipLast(1))
            {
                var square = GetSquareAt(pos);

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
        private List<GridPosition> ReconstructPath(Node? node)
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
        private IEnumerable<GridPosition> GetNeighbors(GridPosition position)
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
                var square = GetSquareAt(newPos);

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
        private IEnumerable<GridPosition> GetLine(GridPosition start, GridPosition end)
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

        internal void GenerateGridForRoom(Room room)
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
    }

    public class LineOfSightResult
    {
        public bool IsBlocked { get; set; }
        public int ObstructionPenalty { get; set; }
        public bool ClearShot { get; set; }
        public bool CanShoot => !IsBlocked;
    }
}
