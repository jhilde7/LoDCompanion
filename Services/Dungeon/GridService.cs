using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    public class GridService
    {
        public GridService() { }

        /// <summary>
        /// Creates a grid for a room based on its dimensions and furniture.
        /// </summary>
        public void GenerateGridForRoom(RoomService room)
        {
            room.Grid.Clear();
            room.Width = room.Size[0];
            room.Height = room.Size[1];

            for (int y = 0; y < room.Height; y++)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    var square = new GridSquare(x, y);
                    // TODO: Add logic to check if furniture from room.FurnitureList occupies this square
                    // and set IsObstacle and BlocksLineOfSight accordingly.
                    room.Grid.Add(square);
                }
            }
        }

        /// <summary>
        /// Moves a character to a new position on the grid.
        /// </summary>
        public bool MoveCharacter(Models.Character.Character character, GridPosition newPosition, RoomService room)
        {
            var targetSquare = room.Grid.FirstOrDefault(s => s.Position.X == newPosition.X && s.Position.Y == newPosition.Y);
            if (targetSquare == null || targetSquare.IsOccupied || targetSquare.IsObstacle)
            {
                return false; // Invalid move
            }

            // Vacate the old square
            if (character.Position != null)
            {
                var oldSquare = room.Grid.FirstOrDefault(s => s.Position.X == character.Position.X && s.Position.Y == character.Position.Y);
                if (oldSquare != null)
                {
                    oldSquare.OccupyingCharacterId = null;
                }
            }

            // Occupy the new square
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

        /// <summary>
        /// A placeholder for a Line of Sight algorithm (e.g., Bresenham's line algorithm).
        /// </summary>
        public bool HasLineOfSight(GridPosition start, GridPosition end, RoomService room)
        {
            // TODO: Implement a line-drawing algorithm to check for intervening squares
            // with BlocksLineOfSight == true.
            return true; // Assume true for now
        }

        /// <summary>
        /// A placeholder for a pathfinding algorithm (e.g., A*).
        /// </summary>
        public List<GridPosition> FindShortestPath(GridPosition start, GridPosition end, RoomService room)
        {
            // TODO: Implement A* or another pathfinding algorithm to navigate the grid,
            // avoiding squares where IsObstacle && !CanBeClimbed or if the shorstest route requires climbing over an object which is twice the movement cost.
            return new List<GridPosition> { end }; // Return a direct path for now
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

            if (pushbackSquare != null && !pushbackSquare.IsOccupied && !pushbackSquare.IsObstacle)
            {
                MoveCharacter(target, pushbackPosition, room);
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
        /// Finds the shortest path between two points using the A* algorithm.
        /// </summary>
        /// <param name="start">The starting position.</param>
        /// <param name="end">The destination position.</param>
        /// <param name="room">The room grid to navigate.</param>
        /// <returns>A list of GridPosition objects representing the path, or an empty list if no path is found.</returns>
        public List<GridPosition> FindShortestPath(GridPosition start, GridPosition end, RoomService room)
        {
            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();

            var startNode = new Node(start, null, 0, GetDistance(start, end));
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.OrderBy(node => node.FScore).First();

                if (currentNode.Position.Equals(end))
                {
                    return ReconstructPath(currentNode);
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                foreach (var neighborPos in GetNeighbors(currentNode.Position, room))
                {
                    var neighborSquare = room.Grid.First(s => s.Position.Equals(neighborPos));
                    if (closedSet.Any(n => n.Position.Equals(neighborPos)))
                    {
                        continue;
                    }

                    // Calculate the cost to move to the neighbor
                    // Climbing costs double movement.
                    int movementCost = neighborSquare.IsObstacle ? 2 : 1;
                    int gScore = currentNode.GScore + movementCost;

                    var neighborNode = openSet.FirstOrDefault(n => n.Position.Equals(neighborPos));

                    if (neighborNode == null)
                    {
                        neighborNode = new Node(neighborPos, currentNode, gScore, GetDistance(neighborPos, end));
                        openSet.Add(neighborNode);
                    }
                    else if (gScore < neighborNode.GScore)
                    {
                        neighborNode.Parent = currentNode;
                        neighborNode.GScore = gScore;
                    }
                }
            }

            return new List<GridPosition>(); // Return an empty list if no path is found
        }

        /// <summary>
        /// Reconstructs the path from the end node back to the start.
        /// </summary>
        private List<GridPosition> ReconstructPath(Node node)
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
        private IEnumerable<GridPosition> GetNeighbors(GridPosition position, RoomService room)
        {
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                var newPos = new GridPosition(position.X + dx[i], position.Y + dy[i]);
                var square = room.Grid.FirstOrDefault(s => s.Position.Equals(newPos));

                // A square is a valid neighbor if it exists and is not an obstacle that cannot be climbed.
                if (square != null && (!square.IsObstacle || square.CanBeClimbed))
                {
                    yield return newPos;
                }
            }
        }

        // --- Helper Class for A* ---
        private class Node
        {
            public GridPosition Position { get; }
            public Node Parent { get; set; }
            public int GScore { get; set; } // Cost from start to current node
            public int HScore { get; set; } // Heuristic cost from current node to end
            public int FScore => GScore + HScore; // Total cost

            public Node(GridPosition position, Node parent, int gScore, int hScore)
            {
                Position = position;
                Parent = parent;
                GScore = gScore;
                HScore = hScore;
            }
        }
    }
}
