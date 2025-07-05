using System.Text;

namespace LoDCompanion.Models.Dungeon
{

    public enum RoomCategory
    {
        None,
        Room,
        Corridor
    }

    public class RoomInfo
    {
        public string? Name { get; set; }
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
        public bool RandomEncounter { get; set; }
        public bool HasSpecial { get; set; }


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
    }

    /// <summary>
    /// Represents a simple X, Y coordinate on the game grid.
    /// </summary>
    public class GridPosition
    {
        public int X { get; set; }
        public int Y { get; set; }

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridPosition? other)
        {
            if (other is null) return false;
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as GridPosition);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
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

        public GridSquare(int x, int y)
        {
            Position = new GridPosition(x, y);
        }
    }
}
