using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Services.Dungeon
{
    public class DungeonManagerService
    {
        private readonly GameDataService _gameData;
        public List<Hero> HeroParty { get; private set; } // Use properties for better encapsulation
        public int RoomsWithoutEncounters { get; set; } = 0;
        public int PartyMorale { get; private set; }
        public int MaxPartyMorale { get; private set; }
        public int MinThreatLevel { get; set; }
        public int MaxThreatLevel { get; set; }
        public int ThreatLevel { get; set; }
        public int WhenSpawnWanderingMonster { get; set; }

        // Dependencies, injected via constructor
        private readonly WanderingMonsterService _wanderingMonsterService;
        private readonly EncounterService _encounterService;
        private readonly QuestEncounterService _questEncounterService;
        private readonly RoomFactoryService _roomFactoryService;

        // Represents the current active room
        public RoomCorridor StartingRoom { get; private set; }
        public RoomCorridor CurrentRoom { get; private set; }

        public DungeonManagerService( GameDataService gameData,
            WanderingMonsterService wanderingMonsterService,
            EncounterService encounterService,
            QuestEncounterService questEncounterService,
            RoomFactoryService roomFactoryService)
        {
            _gameData = gameData;
            _wanderingMonsterService = wanderingMonsterService ?? throw new ArgumentNullException(nameof(wanderingMonsterService));
            _encounterService = encounterService ?? throw new ArgumentNullException(nameof(encounterService));
            _questEncounterService = questEncounterService ?? throw new ArgumentNullException(nameof(questEncounterService));
            _roomFactoryService = roomFactoryService ?? throw new ArgumentNullException(nameof(roomFactoryService));

            StartingRoom = new RoomCorridor(_gameData);
            CurrentRoom = StartingRoom;
            HeroParty = new List<Hero>(); // Initialize the list
        }

        // Replaces the Start() method logic for initial setup
        public void InitializeDungeon(List<Hero> initialHeroes, string startingRoomName = "StartingRoom")
        {
            if (initialHeroes == null || initialHeroes.Count == 0)
            {
                throw new ArgumentException("Initial hero party cannot be null or empty.", nameof(initialHeroes));
            }

            HeroParty.Clear();
            PartyMorale = 0;
            foreach (Hero hero in initialHeroes)
            {
                HeroParty.Add(hero);
                // Replaced Mathf.Ceil with Math.Ceiling
                PartyMorale += (int)Math.Ceiling((double)hero.Resolve / 10);
            }
            MaxPartyMorale = PartyMorale;

            // Initialize threat levels and wandering monster trigger
            MinThreatLevel = 1; // Example default
            MaxThreatLevel = 10; // Example default
            ThreatLevel = 0; // Starting low
            WhenSpawnWanderingMonster = 5; // Example default

            // Create the first room using the RoomFactoryService
            // Note: CurrentRoom represents the player's current location, not necessarily the 'firstRoom' concept from original
            CurrentRoom = _roomFactoryService.CreateRoom(startingRoomName) ?? new RoomCorridor(_gameData);
            // Any other initial dungeon setup logic here, e.g., connecting rooms
        }

        // Replaces the Update() method logic for game progression (e.g., called per turn or timer tick)
        public void AdvanceTurn()
        {
            // Logic for increasing threat level or other time-based events
            ThreatLevel++;

            // Wandering monster spawn logic
            if (ThreatLevel >= WhenSpawnWanderingMonster)
            {
                // The original code passed 'encounter' to 'wanderingMonster'.
                // In a service structure, WanderingMonsterService would likely directly use EncounterService.
                // We just tell the WanderingMonsterService to spawn a monster, and it handles the details.
                // It should return the monster to be added to the current room.
                _wanderingMonsterService.TriggerWanderingMonster(StartingRoom);

                ThreatLevel -= 5; // Reset or reduce threat after spawn
            }
            // Add other per-turn logic here (e.g., character status effects, resource regeneration)
        }

        // Example method for moving between rooms
        public bool MoveToRoom(RoomCorridor nextRoom)
        {
            if (nextRoom != null)
            {
                CurrentRoom = nextRoom;
                Console.WriteLine($"Party moved to {CurrentRoom.RoomName}.");
                return true;
            }
            return false;
        }

        // Method to change party morale
        public void AdjustPartyMorale(int adjustment)
        {
            PartyMorale = Math.Min(MaxPartyMorale, PartyMorale + adjustment);
            if (PartyMorale < 0) PartyMorale = 0; // Morale cannot go below zero
        }
    }
}