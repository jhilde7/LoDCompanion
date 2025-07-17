using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Models.Dungeon
{
    public class DungeonState
    {
        public Party? HeroParty { get; set; }
        public Quest? Quest { get; set; }
        public int RoomsWithoutEncounters { get; set; } = 0;
        public int MinThreatLevel { get; set; }
        public int MaxThreatLevel { get; set; }
        public int ThreatLevel { get; set; }
        public int WhenSpawnWanderingMonster { get; set; }
        public Room? StartingRoom { get; set; }
        public Room? CurrentRoom { get; set; }
        public Queue<Room> ExplorationDeck { get; set; } = new Queue<Room>();
        public List<Room> RoomsInDungeon { get; set; } = new List<Room>();
        public List<LeverColor> AvailableLevers { get; set; } = new List<LeverColor>();
        public List<WanderingMonsterState> WanderingMonsters { get; set; } = new List<WanderingMonsterState>();
        public List<Monster> RevealedMonsters { get; set; } = new List<Monster>();
        public Dictionary<GridPosition, GridSquare> DungeonGrid { get; private set; } = new Dictionary<GridPosition, GridSquare>();
    }
}
