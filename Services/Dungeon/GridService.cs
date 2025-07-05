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
        public void PlaceRoomOnGrid(RoomService room, GridPosition roomOffset)
        {
            room.GridOffset = roomOffset; // Store the room's global position

            for (int y = 0; y < room.Height; y++)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    var globalPos = new GridPosition(roomOffset.X + x, roomOffset.Y + y);
                    if (!DungeonGrid.ContainsKey(globalPos))
                    {
                        var square = new GridSquare(globalPos.X, globalPos.Y);

                        // --- NEW: Simplified wall/floor logic ---
                        // We assume a square is NOT a wall unless specified by room layout.
                        // Your room generation logic would set `IsWall = true` for border squares.
                        square.IsWall = false; // Default to floor

                        // TODO: Add logic to place furniture on the square from room.FurnitureList
                        // square.Furniture = ...

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
            return Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
        }

        public LineOfSightResult HasLineOfSight(GridPosition start, GridPosition end)
        {
            var result = new LineOfSightResult();

            int x0 = start.X; int y0 = start.Y;
            int x1 = end.X; int y1 = end.Y;
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            bool isStraightLine = (start.X == end.X || start.Y == end.Y);

            while (true)
            {
                var currentPos = new GridPosition(x0, y0);

                // --- Check the current square in the line ---
                // We always skip the start and end squares themselves.
                if (!currentPos.Equals(start) && !currentPos.Equals(end))
                {
                    var square = GetSquareAt(currentPos);

                    // If the square doesn't exist, it's a wall. LOS is blocked.
                    if (square == null || square.LoSBlocked)
                    {
                        result.IsBlocked = true;
                        return result;
                    }

                    // Check for furniture that obstructs LOS
                    if (square.IsObstacle || square.IsOccupied)
                    {
                        result.ObstructionPenalty -= 10;
                        return result;
                    }

                    result.ClearShot = true;

                }

                if (x0 == x1 && y0 == y1)
                {
                    // We have reached the end of the line.
                    break;
                }

                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }

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
            if (shover.Position == null || target.Position == null) return "Cannot shove character with no position.";

            int dx = target.Position.X - shover.Position.X;
            int dy = target.Position.Y - shover.Position.Y;

            var pushbackPosition = new GridPosition(target.Position.X + dx, target.Position.Y + dy);

            // Check if the pushback square is valid and not occupied.
            var pushbackSquare = room.Grid.FirstOrDefault(s => s.Position.X == pushbackPosition.X && s.Position.Y == pushbackPosition.Y);

            if (pushbackSquare != null && !pushbackSquare.IsOccupied && 
                !(pushbackSquare.Furniture != null && 
                (pushbackSquare.Furniture.CanBeClimbed || pushbackSquare.Furniture.NoEntry)))
            {
                MoveCharacter(target, pushbackPosition);
                return $"{shover.Name} successfully shoves {target.Name} back!";
            }
            else
            {
                // Pushback is blocked, target falls over.
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
            int x0 = start.X; int y0 = start.Y;
            int x1 = end.X; int y1 = end.Y;
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            while (true)
            {
                var currentPos = new GridPosition(x0, y0);
                var currentSquare = GetSquareAt(currentPos);

                // Path is blocked if the square doesn't exist (it's the void between rooms)
                // or if it's an obstacle. Ignore start/end points.
                if ((currentSquare == null || currentSquare.IsWall || currentSquare.DoubleMoveCost) && !currentPos.Equals(start) && !currentPos.Equals(end))
                {
                    return false;
                }

                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
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
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                var newPos = new GridPosition(position.X + dx[i], position.Y + dy[i]);
                var square = GetSquareAt(newPos);

                // A square is a valid neighbor if it exists and is not marked as "No Entry".
                // The movement cost for climbable obstacles is handled by the A* algorithm itself.
                if (square != null && (square.Furniture != null && !square.Furniture.NoEntry))
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
    }

    public class LineOfSightResult
    {
        public bool IsBlocked { get; set; }
        public int ObstructionPenalty { get; set; }
        public bool ClearShot { get; set; }
        public bool CanShoot => !IsBlocked;
    }
}
