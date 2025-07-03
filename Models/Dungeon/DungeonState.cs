using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Models.Dungeon
{
    public class DungeonState
    {
        public Party? HeroParty { get; set; }
        public int RoomsWithoutEncounters { get; set; } = 0;
        public int MinThreatLevel { get; set; }
        public int MaxThreatLevel { get; set; }
        public int ThreatLevel { get; set; }
        public int WhenSpawnWanderingMonster { get; set; }
        public RoomService? StartingRoom { get; set; }
        public RoomService? CurrentRoom { get; set; }
        public Queue<RoomInfo> ExplorationDeck { get; set; } = new Queue<RoomInfo>();
    }
}
