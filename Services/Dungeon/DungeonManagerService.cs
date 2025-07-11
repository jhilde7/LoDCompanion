using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Game;
using LoDCompanion.Utilities;
using LoDCompanion.Services.Player;

namespace LoDCompanion.Services.Dungeon
{
    public class DungeonManagerService
    {
        private readonly GameDataService _gameData;
        private readonly WanderingMonsterService _wanderingMonster;
        private readonly EncounterService _encounter;
        private readonly RoomFactoryService _roomFactory;
        private readonly GameStateManagerService _gameManager;
        private readonly DungeonBuilderService _dungeonBuilder;
        private readonly ThreatService _threat;
        private readonly LockService _lock;
        private readonly TrapService _trap;
        private readonly PartyRestingService _partyResting;
        private readonly LeverService _lever;
        private readonly QuestService _quest;
        private readonly GridService _grid;
        private readonly DungeonState _dungeonState;
        
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
            LockService lockService,
            TrapService trapService,
            PartyRestingService partyResting,
            LeverService leverService,
            QuestService questService,
            GridService gridService)
        {
            _gameData = gameData;
            _wanderingMonster = wanderingMonster;
            _encounter = encounterService;
            _roomFactory = roomFactoryService;
            _gameManager = gameStateManager;
            _dungeonBuilder = dungeonBuilder;
            _threat = threatService;
            _lock = lockService;
            _trap = trapService;
            _partyResting = partyResting;
            _lever = leverService;
            _quest = questService;
            _grid = gridService;

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
            _dungeonState.StartingRoom = _roomFactory.CreateRoom(quest.StartingRoom?.Name ?? "Starting Tile");
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

        public void ProcessTurn()
        {
            // For now, we'll assume the party is not in battle for the scenario roll.
            // This would be determined by checking if there are active monsters in the room.
            bool isInBattle = false;

            var threatResult = _threat.ProcessScenarioRoll(_dungeonState, isInBattle);

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
        /// <param name="hero">The hero performing the action.</param>
        /// <returns>A string describing the result of the attempt.</returns>
        public string InteractWithDoor(DoorChest door, Hero hero)
        {
            if (door.IsOpen) return "The door is already open.";

            // Step 1: Increase Threat Level
            _threat.IncreaseThreat(_dungeonState, 1);

            // Step 2: Roll for Trap (d6)
            if (RandomHelper.RollDie("D6") == 6)
            {
                door.IsTrapped = true;
                var trap = Trap.GetRandomTrap(); // A new trap is generated
                if (_dungeonState.CurrentRoom != null)
                {
                    _dungeonState.CurrentRoom.CurrentTrap = trap;
                }

                // Step 3: Resolve Trap
                if (!_trap.DetectTrap(hero, trap))
                {
                    // Failed to detect, trap is sprung!
                    door.IsTrapped = false;
                    return _trap.TriggerTrap(hero, trap);
                }
                else
                {
                    // Trap detected. The UI would ask the player to disarm or trigger it.
                    // For now, we assume they attempt to disarm.
                    if (!_trap.DisarmTrap(hero, trap))
                    {
                        return $"{hero.Name} failed to disarm the {trap.Name} and triggered it!";
                    }
                    // On success, the trap is gone, and we proceed.
                }
            }

            // Step 4: Roll for Lock (d10)
            int lockRoll = RandomHelper.RollDie("D10");
            if (lockRoll > 6)
            {
                door.IsLocked = true;
                switch (lockRoll)
                {
                    case 7: door.SetLockAndTrapState(true, 0, 10, door.IsTrapped); break;
                    case 8: door.SetLockAndTrapState(true, -10, 15, door.IsTrapped); break;
                    case 9: door.SetLockAndTrapState(true, -15, 20, door.IsTrapped); break;
                    case 10: door.SetLockAndTrapState(true, -20, 25, door.IsTrapped); break;
                }
            }

            // Step 5: Resolve Lock
            if (door.IsLocked)
            {
                // The door is locked. The game must now wait for player input
                // (e.g., Pick Lock, Bash, Cast Spell). This method's job is done for now.
                return $"The door is locked (Difficulty: {door.LockModifier}, HP: {door.LockHP}).";
            }

            // Step 6: Open the door and reveal the next room
            door.IsOpen = true;
            RevealNextRoom(door);
            return "The door creaks open...";
        }

        /// <summary>
        /// Reveals the next room after a door has been successfully opened.
        /// </summary>
        private List<Monster>? RevealNextRoom(DoorChest openedDoor)
        {
            if (_dungeonState.ExplorationDeck != null && _dungeonState.ExplorationDeck.TryDequeue(out Room? nextRoomInfo) && nextRoomInfo != null)
            {
                var newRoom = _roomFactory.CreateRoom(nextRoomInfo.RoomName ?? string.Empty);
                if (newRoom != null)
                {
                    GridPosition newRoomOffset = CalculateNewRoomOffset(openedDoor, newRoom);
                    _grid.PlaceRoomOnGrid(newRoom, newRoomOffset);

                    // Link the rooms logically
                    if (_dungeonState.CurrentRoom != null)
                    {
                        openedDoor.ConnectedRooms.Add(newRoom);
                        newRoom.ConnectedRooms.Add(_dungeonState.CurrentRoom);
                    }

                    _dungeonState.CurrentRoom = newRoom;
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
        private GridPosition CalculateNewRoomOffset(DoorChest door, Room newRoom)
        {
            GridPosition primaryDoorPos = door.Position[0];
            int newRoomWidth = newRoom.Width;
            int newRoomHeight = newRoom.Height;

            switch (door.Orientation)
            {
                case Orientation.North:
                    // Place the new room's bottom edge against the door
                    return new GridPosition(primaryDoorPos.X - (newRoomWidth / 2) + 1, primaryDoorPos.Y + 1, primaryDoorPos.Z);
                case Orientation.South:
                    // Place the new room's top edge against the door
                    return new GridPosition(primaryDoorPos.X - (newRoomWidth / 2) + 1, primaryDoorPos.Y - newRoomHeight, primaryDoorPos.Z);
                case Orientation.East:
                    // Place the new room's left edge against the door
                    return new GridPosition(primaryDoorPos.X + 1, primaryDoorPos.Y - (newRoomHeight / 2) + 1, primaryDoorPos.Z);
                case Orientation.West:
                    // Place the new room's right edge against the door
                    return new GridPosition(primaryDoorPos.X - newRoomWidth, primaryDoorPos.Y - (newRoomHeight / 2) + 1, primaryDoorPos.Z);
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

            int encounterChance = (newRoom.Category == RoomCategory.Room) ? 50 : 30;

            if (_dungeonState.RoomsWithoutEncounters >= 4)
            {
                encounterChance += 10;
            }

            int roll = RandomHelper.RollDie("D100");

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

        public void IncreaseThreat(int amount)
        {
            _dungeonState.ThreatLevel += amount;
            if (_dungeonState.ThreatLevel >= _dungeonState.WhenSpawnWanderingMonster)
            {
                // Trigger wandering monster logic...
                _dungeonState.ThreatLevel -= 5;
            }
        }

        /// <summary>
        /// Initiates the resting process for the party within the dungeon.
        /// </summary>
        /// <returns>A string summarizing the outcome of the rest attempt.</returns>
        public string InitiateDungeonRest()
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
            var restResult = _partyResting.AttemptRest(_dungeonState.HeroParty, RestingContext.Dungeon, _dungeonState);

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

        /// <summary>
        /// Called when the party decides to leave the dungeon after completing their objective.
        /// </summary>
        public string FinishQuest()
        {
            if (!_quest.IsObjectiveComplete)
            {
                return "The quest objective is not yet complete. You cannot leave yet.";
            }

            if (_dungeonState.HeroParty == null) return "Error: No active party.";

            // Grant rewards and get the aftermath message.
            var resultMessage = _quest.GrantRewards(_dungeonState.HeroParty);

            // Tell the GameStateManager to handle the post-quest state transition.
            _gameManager.CompleteDungeon();

            return resultMessage;
        }

        /// <summary>
        /// Called when the party decides to leave the dungeon before completing the objective.
        /// </summary>
        public async Task<string> AbandonQuest()
        {
            // Per the rules, we simply save the current state to be resumed later.
            await _gameManager.SaveGameAsync("In-Settlement"); // Or another appropriate location
            _gameManager.LeaveDungeon(); // Clears the active dungeon from the game state for this session

            return "You have abandoned the quest. Your progress has been saved, but the dungeon will be repopulated with new threats upon your return.";
        }
    }
}