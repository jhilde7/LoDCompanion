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
}
