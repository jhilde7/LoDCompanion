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
    }

    /// <summary>
    /// Represents a single square on the room's grid.
    /// </summary>
    public class GridSquare
    {
        public GridPosition Position { get; set; }
        public bool IsOccupied => OccupyingCharacterId != null;
        public string? OccupyingCharacterId { get; set; }
        public bool IsObstacle { get; set; } // True if furniture or a wall blocks movement.
        public bool BlocksLineOfSight { get; set; } // True if furniture or a pillar blocks LOS.

        public GridSquare(int x, int y)
        {
            Position = new GridPosition(x, y);
        }
    }
}
