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
        private readonly DungeonBuilderService _dungeonBuilder;
        private readonly ThreatService _threatService;

        private readonly DungeonState DungeonState;
        public Party? HeroParty => DungeonState.HeroParty;
        public RoomService? StartingRoom => DungeonState.StartingRoom;
        public RoomService? CurrentRoom => DungeonState.CurrentRoom;


        public DungeonManagerService(
            GameDataService gameData,
            WanderingMonsterService wanderingMonsterService,
            EncounterService encounterService,
            QuestEncounterService questEncounterService,
            RoomFactoryService roomFactoryService,
            GameStateManagerService gameStateManagerService,
            DungeonState dungeonState,
            DungeonBuilderService dungeonBuilder,
            ThreatService threatService) // Add this
        {
            _gameData = gameData;
            _wanderingMonster = wanderingMonsterService;
            _encounter = encounterService;
            _questEncounter = questEncounterService;
            _roomFactory = roomFactoryService;
            _gameManager = gameStateManagerService;
            _dungeonBuilder = dungeonBuilder;
            _threatService = threatService;

            DungeonState = dungeonState;
        }

        // Create a new method to start a quest
        public void StartQuest(Party heroParty, int roomCount, int corridorCount, string objectiveRoom, string startingRoomName = "Start Tile")
        {
            // 1. Initialize the basic dungeon state
            DungeonState.HeroParty = heroParty;
            DungeonState.MinThreatLevel = 1;
            DungeonState.MaxThreatLevel = 10; // This can be overridden by quest specifics
            DungeonState.ThreatLevel = 0;
            DungeonState.WhenSpawnWanderingMonster = 5; // This can also be overridden

            // 2. Generate the exploration deck using the DungeonBuilderService
            var explorationDeck = _dungeonBuilder.CreateDungeonDeck(roomCount, corridorCount, objectiveRoom, new List<string>(), new List<string>());
            DungeonState.ExplorationDeck = new Queue<RoomInfo>(explorationDeck);

            // 3. Create and set the starting room
            DungeonState.StartingRoom = _roomFactory.CreateRoom(startingRoomName);
            DungeonState.CurrentRoom = DungeonState.StartingRoom;
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
        public void ProcessTurn()
        {
            // For now, we'll assume the party is not in battle for the scenario roll.
            // This would be determined by checking if there are active monsters in the room.
            bool isInBattle = false;

            var threatResult = _threatService.ProcessScenarioRoll(DungeonState, isInBattle);

            if (threatResult != null)
            {
                // A threat event was triggered. We can now handle the result.
                Console.WriteLine($"Threat Event: {threatResult.Description}");

                if (threatResult.SpawnWanderingMonster)
                {
                    // Call your wandering monster logic here
                    _wanderingMonster.TriggerWanderingMonster(DungeonState.StartingRoom);
                }
                if (threatResult.SpawnTrap)
                {
                    // Call your trap logic here
                }
                // Handle other complex events here...
            }
        }

        public bool MoveToRoom(RoomService nextRoom)
        {
            if (nextRoom != null)
            {
                DungeonState.CurrentRoom = nextRoom;
                return true;
            }
            return false;
        }

        public void OpenDoor(DoorChest door)
        {
            // ... logic for opening the door ...

            IncreaseThreat(1); // Increase threat when a door is opened

            // ... logic for checking for traps and encounters ...
        }

        public void IncreaseThreat(int amount)
        {
            DungeonState.ThreatLevel += amount;
            if (DungeonState.ThreatLevel >= DungeonState.WhenSpawnWanderingMonster)
            {
                // Trigger wandering monster logic...
                DungeonState.ThreatLevel -= 5;
            }
        }
    }
}