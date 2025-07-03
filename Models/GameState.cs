using LoDCompanion.Models.Character;

namespace LoDCompanion.Models
{
    public class GameState
    {
        // This holds the party, their inventory, stats, etc.
        public Party? CurrentParty { get; set; }

        // This will hold the current dungeon, including the map, monster positions, etc.
        public Dungeon? CurrentDungeon { get; set; }

        // You can add more states as your game grows
        // public WorldMapState WorldMap { get; set; }
        // public QuestLog Quests { get; set; }

        // This helps manage game flow
        public string CurrentLocation { get; set; } = "Town"; // e.g., "Town", "Dungeon", "WorldMap"
    }
}