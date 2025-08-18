using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Dungeon
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

        public List<Character> AllCharactersInDungeon => [.. RevealedMonsters, .. HeroParty?.Heroes ?? new List<Hero>()];
    }

    public class DungeonManagerService
    {
        private readonly GameDataService _gameData;
        private readonly WanderingMonsterService _wanderingMonster;
        private readonly EncounterService _encounter;
        private readonly RoomFactoryService _roomFactory;
        private readonly GameStateManagerService _gameManager;
        private readonly DungeonBuilderService _dungeonBuilder;
        private readonly ThreatService _threat;
        private readonly TrapService _trap;
        private readonly PartyRestingService _partyResting;
        private readonly LeverService _lever;
        private readonly DungeonState _dungeonState;

        public event Action? OnDungeonStateChanged;

        public Party? HeroParty => _dungeonState.HeroParty;
        public Room? StartingRoom => _dungeonState.StartingRoom;
        public Room? CurrentRoom => _dungeonState.CurrentRoom;
        public DungeonState? DungeonState => _dungeonState;


        public DungeonManagerService(
            DungeonState dungeonState,
            GameDataService gameData,
            WanderingMonsterService wanderingMonster,
            EncounterService encounterService,
            RoomFactoryService roomFactoryService,
            GameStateManagerService gameStateManager,
            DungeonBuilderService dungeonBuilder,
            ThreatService threatService,
            TrapService trapService,
            PartyRestingService partyResting,
            LeverService leverService)
        {
            _gameData = gameData;
            _wanderingMonster = wanderingMonster;
            _encounter = encounterService;
            _roomFactory = roomFactoryService;
            _gameManager = gameStateManager;
            _dungeonBuilder = dungeonBuilder;
            _threat = threatService;
            _trap = trapService;
            _partyResting = partyResting;
            _lever = leverService;

            _dungeonState = dungeonState;
        }

        // Create a new method to start a quest
        public void StartQuest(Party heroParty, Quest quest)
        {
            // 1. Initialize the basic dungeon state
            _dungeonState.HeroParty = heroParty;
            _dungeonState.MinThreatLevel = 1;
            _dungeonState.MaxThreatLevel = 10; // This can be overridden by quest specifics
            _dungeonState.ThreatLevel = 0;
            _dungeonState.WhenSpawnWanderingMonster = 5; // This can also be overridden

            // 2. Generate the exploration deck using the DungeonBuilderService
            List<Room> explorationDeck = _dungeonBuilder.CreateDungeonDeck(quest);
            _dungeonState.ExplorationDeck = new Queue<Room>(explorationDeck);

            // 3. Create and set the starting room
            _dungeonState.StartingRoom = _roomFactory.CreateRoom(quest.StartingRoom?.Name ?? "Start Tile");
            _dungeonState.CurrentRoom = _dungeonState.StartingRoom;
        }

        public void InitializeDungeon(Party initialHeroes, Quest quest)
        {
            if (initialHeroes == null || initialHeroes.Heroes.Count == 0)
            {
                throw new ArgumentException("Initial hero party cannot be null or empty.", nameof(initialHeroes));
            }

            // Initialize threat levels and wandering monster trigger
            _dungeonState.HeroParty = initialHeroes;
            _dungeonState.MinThreatLevel = 1; // Example default
            _dungeonState.MaxThreatLevel = 10; // Example default
            _dungeonState.ThreatLevel = 0; // Starting low
            _dungeonState.WhenSpawnWanderingMonster = 5; // Example default

            if (_dungeonState.CurrentRoom != null)
            {
                _dungeonState.CurrentRoom = _roomFactory.CreateRoom(quest.StartingRoom?.Name ?? "Start Tile") ?? new Room(); 
            }
            // Any other initial dungeon setup logic here, e.g., connecting rooms
        }

        /// <summary>
        /// Finds the room that contains a specific grid position.
        /// </summary>
        public Room? FindRoomAtPosition(GridPosition position)
        {
            // This method iterates through all known rooms to find which one contains the coordinate.
            foreach (Room room in _dungeonState.RoomsInDungeon) // Assumes DungeonState can provide all rooms
            {
                // Check if the position is within the room's bounding box.
                if (position.X >= room.GridOffset.X && position.X < room.GridOffset.X + room.Width &&
                    position.Y >= room.GridOffset.Y && position.Y < room.GridOffset.Y + room.Height)
                {
                    return room;
                }
            }
            return null; // Position is not in any known room.
        }

        public async Task ProcessTurnAsync()
        {
            // For now, we'll assume the party is not in battle for the scenario roll.
            // This would be determined by checking if there are active monsters in the room.
            bool isInBattle = false;

            var threatResult = await _threat.ProcessScenarioRoll(_dungeonState, isInBattle, HeroParty);

            if (threatResult != null)
            {
                // A threat event was triggered. We can now handle the result.
                Console.WriteLine($"Threat Event: {threatResult.Description}");

                if (threatResult.SpawnWanderingMonster)
                {
                    // Call your wandering monster logic here
                    _wanderingMonster.SpawnWanderingMonster(_dungeonState);
                }
                if (threatResult.SpawnTrap)
                {
                    // Call your trap logic here
                }
                // After hero actions, move any wandering monsters.
                _wanderingMonster.MoveWanderingMonsters(_dungeonState);
            }
        }

        /// <summary>
        /// Orchestrates the entire sequence of a hero attempting to open a door or chest.
        /// This method follows the rules on page 87 of the PDF.
        /// </summary>
        /// <param name="door">The door or chest being opened.</param>
        /// <param name="character">The hero performing the action.</param>
        /// <returns>A string describing the result of the attempt.</returns>
        public async Task<string> InteractWithDoorAsync(Door door, Character character)
        {
            if (door.Properties != null && door.Properties.ContainsKey(DoorProperty.Open)) return "The door is already open.";
            
            door.Properties ??= new Dictionary<DoorProperty, int>();

            // Increase Threat Level
            _threat.IncreaseThreat(_dungeonState, 1);

            // Roll for Trap (d6)
            if (RandomHelper.RollDie(DiceType.D6) == 6)
            {
                door.SetTrapState();
                door.Trap = Trap.GetRandomTrap(); 

                // Resolve Trap
                if (character is Hero hero)
                {
                    if (!_trap.DetectTrap(hero, door.Trap))
                    {
                        // Failed to detect, trap is sprung!
                        door.Properties.Remove(DoorProperty.Trapped);
                        return await _trap.TriggerTrapAsync(hero, door.Trap);
                    }
                    else
                    {
                        // TODO: Trap detected. The UI would ask the player to disarm or trigger it.
                        // For now, we assume they attempt to disarm.
                        if (!await _trap.DisarmTrapAsync(hero, door.Trap))
                        {
                            return $"{hero.Name} failed to disarm the {door.Trap.Name} and triggered it!";
                        }
                        // On success, the trap is gone, and we proceed.
                    } 
                }
            }

            // Roll for Lock (d10)
            int lockRoll = RandomHelper.RollDie(DiceType.D10);
            if (lockRoll > 6)
            {
                switch (lockRoll)
                {
                    case 7: door.SetLockState(0, 10); break;
                    case 8: door.SetLockState(-10, 15); break;
                    case 9: door.SetLockState(-15, 20); break;
                    case 10: door.SetLockState(-20, 25); break;
                }
            }

            // Resolve Lock
            if (door.Properties != null && door.Properties.ContainsKey(DoorProperty.Locked))
            {
                // The door is locked. The game must now wait for player input
                // (e.g., Pick Lock, Bash, Cast Spell). This method's job is done for now.
                return $"The door is locked (Difficulty: {door.Properties[DoorProperty.LockModifier]}, HP: {door.Properties[DoorProperty.LockHP]}).";
            }

            // Open the door and reveal the next room
            door.Properties?.TryAdd(DoorProperty.Open, 0);
            RevealNextRoom(door);
            return "The door creaks open...";
        }

        /// <summary>
        /// Reveals the next room after a door has been successfully opened.
        /// </summary>
        private List<Monster>? RevealNextRoom(Door openedDoor)
        {
            if (_dungeonState.ExplorationDeck != null && _dungeonState.ExplorationDeck.TryDequeue(out Room? nextRoomInfo) && nextRoomInfo != null)
            {
                var newRoom = _roomFactory.CreateRoom(nextRoomInfo.Name ?? string.Empty);
                if (newRoom != null)
                {
                    GridPosition newRoomOffset = CalculateNewRoomOffset(openedDoor, newRoom);
                    if(DungeonState != null) GridService.PlaceRoomOnGrid(newRoom, newRoomOffset, DungeonState.DungeonGrid);

                    // Link the rooms logically
                    if (_dungeonState.CurrentRoom != null)
                    {
                        openedDoor.ConnectedRooms.Add(newRoom);
                        newRoom.ConnectedRooms.Add(_dungeonState.CurrentRoom);
                    }

                    _dungeonState.CurrentRoom = newRoom;
                    _dungeonState.RoomsInDungeon.Add(newRoom);
                    return CheckForEncounter(newRoom);
                }
            }

            // No more rooms in this path.
            if (_dungeonState.CurrentRoom != null) _dungeonState.CurrentRoom.IsDeadEnd = true;
            return null;
        }

        /// <summary>
        /// Calculates where to place the new room based on the door that was opened.
        /// </summary>
        private GridPosition CalculateNewRoomOffset(Door door, Room newRoom)
        {
            GridPosition primaryDoorPos = door.Position[0];
            int newRoomWidth = newRoom.Width;
            int newRoomHeight = newRoom.Height;

            switch (door.Orientation)
            {
                case Orientation.North:
                    // Place the new room's bottom edge against the door
                    return new GridPosition(primaryDoorPos.X - newRoomWidth / 2 + 1, primaryDoorPos.Y + 1, primaryDoorPos.Z);
                case Orientation.South:
                    // Place the new room's top edge against the door
                    return new GridPosition(primaryDoorPos.X - newRoomWidth / 2 + 1, primaryDoorPos.Y - newRoomHeight, primaryDoorPos.Z);
                case Orientation.East:
                    // Place the new room's left edge against the door
                    return new GridPosition(primaryDoorPos.X + 1, primaryDoorPos.Y - newRoomHeight / 2 + 1, primaryDoorPos.Z);
                case Orientation.West:
                    // Place the new room's right edge against the door
                    return new GridPosition(primaryDoorPos.X - newRoomWidth, primaryDoorPos.Y - newRoomHeight / 2 + 1, primaryDoorPos.Z);
                default:
                    return new GridPosition(primaryDoorPos.X + 1, primaryDoorPos.Y, primaryDoorPos.Z);
            }
        }

        private List<Monster>? CheckForEncounter(Room newRoom)
        {
            if (!newRoom.RandomEncounter)
            {
                _dungeonState.RoomsWithoutEncounters++;
                return null;
            }

            int encounterChance = newRoom.Category == RoomCategory.Room ? 50 : 30;

            if (_dungeonState.RoomsWithoutEncounters >= 4)
            {
                encounterChance += 10;
            }

            int roll = RandomHelper.RollDie(DiceType.D100);

            if (roll <= encounterChance)
            {
                // Encounter Triggered!
                Console.WriteLine("Encounter! Monsters appear!");
                _dungeonState.RoomsWithoutEncounters = 0;

                newRoom.MonstersInRoom = new List<Monster>();

                if (_dungeonState.Quest != null)
                {
                    newRoom.MonstersInRoom = _encounter.GetRandomEncounterByType(_dungeonState.Quest.EncounterType); 
                }
                else
                {
                    newRoom.MonstersInRoom = _encounter.GetRandomEncounterByType(EncounterType.Beasts);
                }

                if (newRoom.MonstersInRoom.Any())
                {
                    newRoom.IsEncounter = true;

                    // Hand off to the CombatManager to start the battle
                    if (_dungeonState.HeroParty != null && _dungeonState.HeroParty.Heroes != null)
                    {
                        return newRoom.MonstersInRoom;
                    }
                    else
                    {
                        Console.WriteLine("Error: Hero party is not initialized.");
                    }
                }
            }

            // No encounter
            Console.WriteLine("The room is quiet... for now.");
            _dungeonState.RoomsWithoutEncounters++;
            return null;
        }

        public void WinBattle()
        {
            _threat.IncreaseThreat(_dungeonState, 1);
        }

        public bool UpdateThreat(int amount)
        {
            _dungeonState.ThreatLevel += amount;
            if (_dungeonState.ThreatLevel >= _dungeonState.WhenSpawnWanderingMonster)
            {
                // Trigger wandering monster logic...
                _dungeonState.ThreatLevel -= 5;
            }
            return true;
        }

        /// <summary>
        /// Initiates the resting process for the party within the dungeon.
        /// </summary>
        /// <returns>A string summarizing the outcome of the rest attempt.</returns>
        public async Task<string> InitiateDungeonRestAsync()
        {
            if (CurrentRoom != null && CurrentRoom.IsEncounter)
            {
                return "Cannot rest while enemies are present!";
            }

            if (_dungeonState.HeroParty == null)
            {
                return "Cannot rest without a party.";
            }

            // Call the resting service, specifying the Dungeon context
            var restResult = await _partyResting.AttemptRest(_dungeonState.HeroParty, RestingContext.Dungeon, _dungeonState);

            if (restResult.WasInterrupted)
            {
                // _combatManager.StartCombat(...);
            }

            return restResult.Message;
        }
        
        /// <summary>
         /// Initiates the lever mini-game for the current room.
         /// </summary>
        public void ActivateLevers()
        {
            if (CurrentRoom == null || !CurrentRoom.HasLevers)
            {
                return;
            }

            // A "clue" would be an item in the party's inventory.
            bool partyHasClue = false; // party.Inventory.Any(item => item.Name == "Lever Clue");

            // Store the prepared deck in the DungeonState so the UI can interact with it.
            _dungeonState.AvailableLevers = _lever.PrepareLeverDeck(partyHasClue);

            Console.WriteLine($"Levers activated! {_dungeonState.AvailableLevers.Count} levers are available.");
        }

        /// <summary>
        /// Called when a player chooses to pull a specific lever from the available deck.
        /// </summary>
        /// <param name="leverIndex">The index of the lever to pull from the AvailableLevers list.</param>
        public void PullLever(int leverIndex)
        {
            if (_dungeonState.AvailableLevers == null || leverIndex < 0 || leverIndex >= _dungeonState.AvailableLevers.Count)
            {
                return;
            }

            // Get the color and remove it from the deck
            LeverColor pulledLeverColor = _dungeonState.AvailableLevers[leverIndex];
            _dungeonState.AvailableLevers.RemoveAt(leverIndex);

            // Resolve the event
            var result = _lever.PullLever(pulledLeverColor);
            Console.WriteLine($"Pulled a {result.LeverColor} lever! Event: {result.Description}");

            // Process the consequences of the event
            if (result.ThreatIncrease > 0)
            {
                _threat.IncreaseThreat(_dungeonState, result.ThreatIncrease);
            }
            if (result.ShouldSpawnWanderingMonster)
            {
                if (_dungeonState.StartingRoom != null) _wanderingMonster.SpawnWanderingMonster(_dungeonState);
            }
            // ... handle other results like ShouldLockADoor, ShouldSpawnPortcullis, etc.
        }
    }
}