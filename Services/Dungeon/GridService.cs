using LoDCompanion.Models.Dungeon;

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
            // avoiding squares where IsObstacle == true.
            return new List<GridPosition> { end }; // Return a direct path for now
        }
    }
}
