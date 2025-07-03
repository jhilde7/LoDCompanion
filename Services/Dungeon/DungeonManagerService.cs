using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Services.Dungeon
{
    public class DungeonManagerService
    {
        private readonly GameDataService _gameData;
        private readonly WanderingMonsterService _wanderingMonster;
        private readonly EncounterService _encounter;
        private readonly QuestEncounterService _questEncounter;
        private readonly RoomFactoryService _roomFactory;
        private readonly GameStateManagerService _gameManager;

        private readonly DungeonState DungeonState;
        public Party? HeroParty => DungeonState.HeroParty;
        public RoomService? StartingRoom => DungeonState.StartingRoom;
        public RoomService? CurrentRoom => DungeonState.CurrentRoom;


        public DungeonManagerService( GameDataService gameData,
            WanderingMonsterService wanderingMonsterService,
            EncounterService encounterService,
            QuestEncounterService questEncounterService,
            RoomFactoryService roomFactoryService,
            GameStateManagerService gameStateManagerService,
            DungeonState dungeonState)
        {
            _gameData = gameData;
            _wanderingMonster = wanderingMonsterService;
            _encounter = encounterService;
            _questEncounter = questEncounterService;
            _roomFactory = roomFactoryService;
            _gameManager = gameStateManagerService;

            DungeonState = dungeonState;
        }

        public void InitializeDungeon(Party initialHeroes, string startingRoomName = "StartingRoom")
        {
            if (initialHeroes == null || initialHeroes.Heroes.Count == 0)
            {
                throw new ArgumentException("Initial hero party cannot be null or empty.", nameof(initialHeroes));
            }

            // Initialize threat levels and wandering monster trigger
            DungeonState.HeroParty = initialHeroes;
            DungeonState.MinThreatLevel = 1; // Example default
            DungeonState.MaxThreatLevel = 10; // Example default
            DungeonState.ThreatLevel = 0; // Starting low
            DungeonState.WhenSpawnWanderingMonster = 5; // Example default

            if (DungeonState.CurrentRoom != null)
            {
                DungeonState.CurrentRoom = _roomFactory.CreateRoom(startingRoomName) ?? new RoomService(_gameData); 
            }
            // Any other initial dungeon setup logic here, e.g., connecting rooms
        }

        // Replaces the Update() method logic for game progression (e.g., called per turn or timer tick)
        public void AdvanceTurn()
        {
            // Logic for increasing threat level or other time-based events
            DungeonState.ThreatLevel++;

            // Wandering monster spawn logic
            if (DungeonState.ThreatLevel >= DungeonState.WhenSpawnWanderingMonster)
            {
                // Only trigger if StartingRoom is not null
                if (StartingRoom != null)
                {
                    _wanderingMonster.TriggerWanderingMonster(StartingRoom);
                }
                DungeonState.ThreatLevel -= 5; // Reset or reduce threat after spawn
            }
            // Add other per-turn logic here (e.g., character status effects, resource regeneration)
        }

        // Example method for moving between rooms
        public bool MoveToRoom(RoomService nextRoom)
        {
            if (nextRoom != null)
            {
                DungeonState.CurrentRoom = nextRoom;
                return true;
            }
            return false;
        }
    }
}