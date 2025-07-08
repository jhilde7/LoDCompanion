using System.Text;
using SixLabors.ImageSharp;

namespace LoDCompanion.Models.Dungeon
{

    public enum RoomCategory
    {
        None,
        Room,
        Corridor,
        Wilderness,
        AncientLands,
        City,

    }

    public class RoomInfo
    {
        public string Name { get; set; } = string.Empty;
        public RoomCategory Category { get; set; } = RoomCategory.None;
        public string? Description { get; set; }
        public string? SpecialRules { get; set; }
        public int ThreatLevelModifier { get; set; }
        public int PartyMoraleModifier { get; set; }
        public int[] Size { get; set; } = new int[] { 1, 1 };
        public int DoorCount { get; set; }
        public List<Furniture>? FurnitureList { get; set; }
        public int EncounterModifier { get; set; }
        public string? EncounterType { get; set; }
        public bool HasLevers { get; set; }
        public bool RandomEncounter { get; set; } = true;
        public bool HasSpecial { get; set; }
        public Uri? ImageUri { get; set; }
        public int TilePixelWidth { get; set; } = 128;
        public int TilePixelHeight { get; set; } = 128;


        public RoomInfo()
        {

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"--- Room: {Name} [{Category}] ---");
            sb.Append($"Size: {Size[0]}x{Size[1]} | ");
            sb.Append($"Doors: {DoorCount} | ");
            sb.AppendLine($"Random Encounter: {RandomEncounter}");

            if (!string.IsNullOrEmpty(Description))
            {
                sb.AppendLine($"Description: {Description}");
            }
            if (FurnitureList != null && FurnitureList.Any())
            {
                sb.AppendLine($"Furniture: {string.Join(", ", FurnitureList)}");
            }
            if (!string.IsNullOrEmpty(SpecialRules))
            {
                sb.AppendLine($"Special Rules: {SpecialRules}");
            }

            var modifiers = new List<string>();
            if (ThreatLevelModifier > 0) modifiers.Add($"Threat: {ThreatLevelModifier:+#;-#;0}");
            if (PartyMoraleModifier > 0) modifiers.Add($"Morale: {PartyMoraleModifier:+#;-#;0}");
            if (EncounterModifier > 0) modifiers.Add($"Encounter: {EncounterModifier:+#;-#;0}");
            if (modifiers.Any())
            {
                sb.AppendLine($"Modifiers: {string.Join(" | ", modifiers)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a grid position to a top-left pixel coordinate on the room's image.
        /// </summary>
        /// <param name="gridPos">The GridPosition of the furniture or character.</param>
        /// <param name="room">The RoomInfo containing the grid and pixel dimensions.</param>
        /// <returns>A Point representing the top-left (X, Y) pixel of the grid square.</returns>
        public System.Drawing.Point GetPixelCoordinateForGridPosition(GridPosition gridPos, RoomInfo room)
        {
            int pixelX = gridPos.X * room.TilePixelWidth;
            int pixelY = gridPos.Y * room.TilePixelHeight;

            return new System.Drawing.Point(pixelX, pixelY);
        }
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
}
