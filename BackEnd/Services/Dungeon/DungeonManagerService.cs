using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.Combat;
using System.Threading.Tasks;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public class DungeonState
    {
        public Party HeroParty { get; set; } = new Party();
        public Quest Quest { get; set; } = new Quest();
        public int RoomsWithoutEncounters { get; set; } = 0;
        public int MinThreatLevel { get; set; }
        public int MaxThreatLevel { get; set; }
        public int ThreatLevel { get; set; }
        public int EncounterChanceModifier { get; set; } = 0;
        public int ScenarioRollModifier { get; set; } = 0;
        public int WhenSpawnWanderingMonster { get; set; }
        public bool SpawnWanderingMonster => ThreatLevel >= MaxThreatLevel;
        public Room? StartingRoom { get; set; }
        public Room? CurrentRoom { get; set; }
        public Queue<Room> ExplorationDeck { get; set; } = new Queue<Room>();
        public List<Room> RoomsInDungeon { get; set; } = new List<Room>();
        public List<LeverColor> AvailableLevers { get; set; } = new List<LeverColor>();
        public List<WanderingMonsterState> WanderingMonsters { get; set; } = new List<WanderingMonsterState>();
        public List<Monster> RevealedMonsters { get; set; } = new List<Monster>();
        public Dictionary<GridPosition, GridSquare> DungeonGrid { get; private set; } = new Dictionary<GridPosition, GridSquare>();

        public List<Character> AllCharactersInDungeon => [.. RevealedMonsters, .. HeroParty?.Heroes ?? new List<Hero>()];

        public Party? SetParty(Party? party)
        {
            if (party != null)
            {
                HeroParty = party; 
            }
            return HeroParty;
        }
    }

    public class DungeonManagerService
    {
        private readonly WanderingMonsterService _wanderingMonster;
        private readonly EncounterService _encounter;
        private readonly PartyManagerService _partyManager;
        private readonly DungeonBuilderService _dungeonBuilder;
        private readonly ThreatService _threat;
        private readonly TrapService _trap;
        private readonly PartyRestingService _partyResting;
        private readonly LeverService _lever;
        private readonly DungeonState _dungeon;
        private readonly CombatManagerService _combatManager;
        private readonly RoomService _room;
        private readonly UserRequestService _userRequest;
        private readonly PlacementService _placement;
        private readonly PowerActivationService _powerActivation;

        public DungeonState Dungeon => _partyManager.SetCurrentDungeon(_dungeon);
        public Party? HeroParty => _dungeon.SetParty(_partyManager.Party);
        public Room? StartingRoom => _dungeon.StartingRoom;
        public Room? CurrentRoom => _dungeon.CurrentRoom;
        public PartyManagerService PartyManager => _partyManager;


        public DungeonManagerService(
            DungeonState dungeonState,
            WanderingMonsterService wanderingMonster,
            EncounterService encounterService,
            PartyManagerService partyManagerService,
            DungeonBuilderService dungeonBuilder,
            ThreatService threatService,
            TrapService trapService,
            PartyRestingService partyResting,
            LeverService leverService,
            CombatManagerService combatManagerService,
            RoomService roomService,
            UserRequestService userRequestService,
            PlacementService placement,
            PowerActivationService powerActivationService)
        {
            _dungeon = dungeonState;
            _wanderingMonster = wanderingMonster;
            _encounter = encounterService;
            _partyManager = partyManagerService;
            _dungeonBuilder = dungeonBuilder;
            _threat = threatService;
            _trap = trapService;
            _partyResting = partyResting;
            _lever = leverService;
            _combatManager = combatManagerService;
            _room = roomService;
            _userRequest = userRequestService;
            _placement = placement;
            _powerActivation = powerActivationService;

            _partyManager.SetMaxMorale();
        }

        // Create a new method to start a quest
        public void StartQuest(Party heroParty, Quest quest)
        {
            // 1. Initialize the basic dungeon state
            _dungeon.HeroParty = heroParty;
            _dungeon.MinThreatLevel = 1;
            _dungeon.MaxThreatLevel = 10; // This can be overridden by quest specifics
            _dungeon.ThreatLevel = 0;
            _dungeon.WhenSpawnWanderingMonster = 5; // This can also be overridden

            // 2. Generate the exploration deck using the DungeonBuilderService
            List<Room> explorationDeck = _dungeonBuilder.CreateDungeonDeck(quest);
            _dungeon.ExplorationDeck = new Queue<Room>(explorationDeck);

            // 3. Create and set the starting room
            _dungeon.StartingRoom = _room.CreateRoom(quest.StartingRoom?.Name ?? "Start Tile");
            _dungeon.CurrentRoom = _dungeon.StartingRoom;
        }

        public void InitializeDungeon(Party initialHeroes, Quest quest)
        {
            if (initialHeroes == null || initialHeroes.Heroes.Count == 0)
            {
                throw new ArgumentException("Initial hero party cannot be null or empty.", nameof(initialHeroes));
            }

            // Initialize threat levels and wandering monster trigger
            _dungeon.HeroParty = initialHeroes;
            _dungeon.MinThreatLevel = 1; // Example default
            _dungeon.MaxThreatLevel = 10; // Example default
            _dungeon.ThreatLevel = 0; // Starting low
            _dungeon.WhenSpawnWanderingMonster = 5; // Example default

            if (_dungeon.CurrentRoom != null)
            {
                _dungeon.CurrentRoom = _room.CreateRoom(quest.StartingRoom?.Name ?? "Start Tile") ?? new Room(); 
            }
            // Any other initial dungeon setup logic here, e.g., connecting rooms
        }

        /// <summary>
        /// Finds the room that contains a specific grid position.
        /// </summary>
        public Room? FindRoomAtPosition(GridPosition position)
        {
            // This method iterates through all known rooms to find which one contains the coordinate.
            foreach (Room room in _dungeon.RoomsInDungeon) // Assumes DungeonState can provide all rooms
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
            bool isInBattle = !_combatManager.IsCombatOver;
            await HandleScenarioRoll(isInBattle);

            if (Dungeon != null)
            {
                // After hero actions, move any wandering monsters.
                if (_wanderingMonster.ProcessWanderingMonsters(Dungeon.WanderingMonsters))
                {
                    Dungeon.WanderingMonsters.Where(w => w.IsRevealed).ToList().ForEach(w => Dungeon.WanderingMonsters.Remove(w));
                }
            }

            if (!isInBattle)
            {
                Dungeon?.HeroParty.Heroes.ForEach(async hero => await StatusEffectService.ProcessActiveStatusEffectsAsync(hero, _powerActivation)); 
            }
        }

        public async Task<ThreatEventResult> HandleScenarioRoll(bool isInBattle)
        {
            var threatResult = await _threat.ProcessScenarioRoll(isInBattle, HeroParty);

            if (threatResult != null)
            {
                // A threat event was triggered. We can now handle the result.
                Console.WriteLine($"Threat Event: {threatResult.Description}");

                if (threatResult.SpawnWanderingMonster)
                {
                    _wanderingMonster.SpawnWanderingMonster(this);
                }
                if (threatResult.SpawnTrap)
                {
                    if (_dungeon.HeroParty != null && _dungeon.HeroParty.Heroes.Any())
                    {
                        var heroes = _dungeon.HeroParty.Heroes;
                        heroes.Shuffle();
                        var randomHero = heroes[0];
                        var trap = new Trap();

                        await _trap.TriggerTrapAsync(randomHero, trap);
                    }
                }
                if (threatResult.ShouldAddExplorationCards)
                {
                    AddExplorationCardsToPiles(1);
                }
                if (threatResult.SpawnRandomEncounter != null)
                {
                    SpawnRandomEncounter(threatResult.SpawnRandomEncounter);
                }
            }
            else
            {
                return new ThreatEventResult
                {
                    Description = "No threat event occurred.",
                    SpawnWanderingMonster = false,
                    SpawnTrap = false,
                    ShouldAddExplorationCards = false,
                    SpawnRandomEncounter = null
                };
            }

            if (_dungeon.SpawnWanderingMonster)
            {
                _wanderingMonster.SpawnWanderingMonster(this);
                _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.WanderingMonsterSpawned);
            }

            return threatResult;
        }

        /// <summary>
        /// Adds one new, random Exploration Card to the top of each active door's deck.
        /// This is triggered by a specific threat event.
        /// </summary>
        public void AddExplorationCardsToPiles(int amount)
        {
            if (_dungeon.CurrentRoom == null || _dungeon.ExplorationDeck == null) return;

            var explorationCards = new Queue<Room>();
            var explorationRooms = _room.GetExplorationDeckRooms();
            explorationRooms.Shuffle();

            foreach (var room in explorationRooms)
            {
                explorationCards.Enqueue(_room.CreateRoom(room.Name));
            }

            var roomsExplorationDoors = _dungeon.RoomsInDungeon
                .Where(r => r.Doors.Any(d => d.ExplorationDeck != null))
                .ToList();
            foreach (var room in roomsExplorationDoors)
            {
                foreach (var door in room.Doors.Where(d => d.ExplorationDeck != null).ToList())
                {
                    if (door.ExplorationDeck != null)
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            var roomCard = explorationCards.Dequeue();
                            door.ExplorationDeck.Prepend(roomCard);
                        }
                    }
                }
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
            if (door.State == DoorState.Open) return "The door is already open.";

            // Roll for Trap (d6)
            if (RandomHelper.RollDie(DiceType.D6) == 6)
            {
                door.SetTrapState();
                door.Trap = new Trap(); 

                // Resolve Trap
                if (character is Hero hero)
                {
                    if (door.Trap != null)
                    {                       
                        return await _trap.TriggerTrapAsync(hero, door.Trap);
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
            if (door.Lock.IsLocked)
            {
                // The door is locked. The game must now wait for player input
                // (e.g., Pick Lock, Bash, Cast Spell). This method's job is done for now.
                return $"The door is locked (Difficulty: {door.Lock.LockModifier}, HP: {door.Lock.LockHP}).";
            }

            // Open the door and reveal the next room
            door.Open();
            _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.OpenDoorOrChest);
            await RevealNextRoomAsync(door);
            return "The door creaks open...";
        }

        /// <summary>
        /// Reveals the next room after a door has been successfully opened.
        /// </summary>
        private async Task RevealNextRoomAsync(Door openedDoor)
        {
            // this should only happen on the very first door opened
            if (openedDoor.ExplorationDeck == null)
            {
                return; // No exploration deck to draw from
            }

            if (openedDoor.ExplorationDeck.TryDequeue(out Room? nextRoomInfo) && nextRoomInfo != null && Dungeon !=null)
            {
                var newRoom = _room.CreateRoom(nextRoomInfo.Name ?? string.Empty);
                if (newRoom != null)
                {
                    GridPosition newRoomOffset = CalculateNewRoomOffset(openedDoor, newRoom);
                    GridService.PlaceRoomOnGrid(newRoom, newRoomOffset, Dungeon.DungeonGrid);

                    // Link the rooms logically
                    newRoom.ConnectedRooms.Add(openedDoor.ConnectedRooms[0]);
                    openedDoor.ConnectedRooms.Add(newRoom);
                    newRoom.Doors.Add(openedDoor);
                    openedDoor.PassagewaySquares = GetPassagewaySquares(openedDoor.ConnectedRooms[0], newRoom, Dungeon.DungeonGrid);

                    Dungeon.CurrentRoom = newRoom;
                    Dungeon.RoomsInDungeon.Add(newRoom);
                    await CheckForEncounter(newRoom);

                    // Handle the remaining deck for the new room's exits
                    var remainingCards = openedDoor.ExplorationDeck.ToList();
                    openedDoor.ExplorationDeck = null; // The old door's deck is now processed

                    if (!remainingCards.Any() || newRoom.DoorCount < 1)
                    {
                        // This path is a dead end as it has no more cards.
                        newRoom.IsDeadEnd = true;
                        return;
                    }

                    var newDoors = newRoom.Doors.Where(d => d != openedDoor).ToList();
                    
                    if (newDoors.Count > 1)
                    {
                        // Create a separate queue for each new door.
                        var doorDecks = new List<Queue<Room>>();
                        for (int i = 0; i < newDoors.Count; i++)
                        {
                            doorDecks.Add(new Queue<Room>());
                        }

                        // Deal the remaining cards from the bottom of the deck (as per the rules).
                        var cardList = remainingCards.ToList();
                        int currentDeckIndex = 0;
                        while (cardList.Any())
                        {
                            // Take the last card from the list (simulating dealing from the bottom).
                            var cardToDeal = cardList.Last();
                            cardList.RemoveAt(cardList.Count - 1);

                            // Add it to the current door's deck.
                            doorDecks[currentDeckIndex].Enqueue(cardToDeal);

                            // Move to the next door's deck for the next card.
                            currentDeckIndex = (currentDeckIndex + 1) % newDoors.Count;
                        }

                        // Assign the newly created decks to each door.
                        for (int i = 0; i < newDoors.Count; i++)
                        {
                            newDoors[i].ExplorationDeck = doorDecks[i];
                        }
                    }
                    else
                    {
                        newDoors[0].ExplorationDeck = openedDoor.ExplorationDeck; // Assign the exploration deck to the door
                    }

                    openedDoor.ExplorationDeck = null; // Clear the original door's exploration deck

                    foreach (var exitDoor in newDoors)
                    {
                        // place doors at a random position on a random edge that does not have a door
                        _placement.PlaceExitDoor(exitDoor, newRoom);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates where to place the new room based on the door that was opened.
        /// </summary>
        private GridPosition CalculateNewRoomOffset(Door door, Room newRoom)
        {
            GridPosition doorReferencePos = door.PassagewaySquares[0];
            Room currentRoom = door.ConnectedRooms[0];

            switch (door.Orientation)
            {
                case Orientation.North: // Door is on the top edge of current room
                    return new GridPosition(doorReferencePos.X, currentRoom.GridOffset.Y + currentRoom.Height + 1, currentRoom.GridOffset.Z);
                case Orientation.South: // Door is on the bottom edge of current room
                    return new GridPosition(doorReferencePos.X, currentRoom.GridOffset.Y - newRoom.Height - 1, currentRoom.GridOffset.Z);
                case Orientation.East:  // Door is on the right edge of current room
                    return new GridPosition(currentRoom.GridOffset.X + currentRoom.Width + 1, doorReferencePos.Y, currentRoom.GridOffset.Z);
                case Orientation.West:  // Door is on the left edge of current room
                    return new GridPosition(currentRoom.GridOffset.X - newRoom.Width - 1, doorReferencePos.Y, currentRoom.GridOffset.Z);
                default:
                    // Fallback case, though an orientation should always be set.
                    return new GridPosition(currentRoom.GridOffset.X + currentRoom.Width + 1, currentRoom.GridOffset.Y, currentRoom.GridOffset.Z);
            }
        }

        /// <summary>
        /// Determines the two global grid squares that form the passageway between two rooms.
        /// This assumes the rooms are placed adjacently on the grid.
        /// </summary>
        /// <param name="currentRoom">The first room.</param>
        /// <param name="newRoom">The second room, placed adjacent to the first.</param>
        /// <param name="dungeonGrid">The global grid of the dungeon.</param>
        /// <returns>A list containing the two GridPosition objects for the passageway, or an empty list if no valid opening is found.</returns>
        public List<GridPosition> GetPassagewaySquares(Room currentRoom, Room newRoom, Dictionary<GridPosition, GridSquare> dungeonGrid)
        {
            var passageway = new List<GridPosition>();

            // Iterate through the boundary of currentRoom to find adjacent squares in newRoom
            for (int y = currentRoom.GridOffset.Y; y < currentRoom.GridOffset.Y + currentRoom.Height; y++)
            {
                for (int x = currentRoom.GridOffset.X; x < currentRoom.GridOffset.X + currentRoom.Width; x++)
                {
                    var currentPos = new GridPosition(x, y, currentRoom.GridOffset.Z);
                    var neighbors = GridService.GetNeighbors(currentPos, dungeonGrid)
                                               .Where(n => !n.Equals(currentPos)); // Exclude self

                    foreach (var neighborPos in neighbors)
                    {
                        // Check if the neighbor is inside newRoom
                        if (neighborPos.X >= newRoom.GridOffset.X && neighborPos.X < newRoom.GridOffset.X + newRoom.Width &&
                            neighborPos.Y >= newRoom.GridOffset.Y && neighborPos.Y < newRoom.GridOffset.Y + newRoom.Height)
                        {
                            var squareA = GridService.GetSquareAt(currentPos, dungeonGrid);
                            var squareB = GridService.GetSquareAt(neighborPos, dungeonGrid);

                            // A valid passageway square must not be a wall in either room
                            if (squareA != null && !squareA.IsWall && squareB != null && !squareB.IsWall)
                            {
                                passageway.Add(currentPos);
                            }
                        }
                    }
                }
            }

            // Return the first two valid squares found, ensuring we have a 2-square wide door
            return passageway.Take(2).ToList();
        }

        /// <summary>
        /// Spawns a random encounter in a specified room.
        /// </summary>
        /// <param name="room">The room where the encounter will be spawned.</param>
        /// <returns>A list of the monsters that were spawned.</returns>
        public void SpawnRandomEncounter(Room room, EncounterType? encounterType = null)
        {
            room.MonstersInRoom = new List<Monster>();
            var dungeonEncounterType = _dungeon.Quest.EncounterType;

            if (room.EncounterType.HasValue)
            {
                room.MonstersInRoom = _encounter.GetRandomEncounterByType(room.EncounterType.Value, dungeonEncounterType: dungeonEncounterType);
            }
            else if(encounterType.HasValue)
            {
                room.MonstersInRoom = _encounter.GetRandomEncounterByType(encounterType.Value);
            }
            else
            {
                room.MonstersInRoom = _encounter.GetRandomEncounterByType(dungeonEncounterType);
            }

            if (room.MonstersInRoom.Any())
            {
                room.IsEncounter = true;
                PlaceMonsters(room, room.MonstersInRoom);
            }
        }

        /// <summary>
        /// Places monsters in a room based on their behavior type.
        /// </summary>
        /// <param name="room">The room to place the monsters in.</param>
        /// <param name="monsters">The list of monsters to be placed.</param>
        private void PlaceMonsters(Room room, List<Monster> monsters)
        {
            var heroes = room.HeroesInRoom;
            if (heroes == null || !heroes.Any())
            {
                // If there are no heroes in the room, we can't place monsters relative to them.
                // As a fallback, we can place them randomly.
                foreach (var monster in monsters)
                {
                    _placement.PlaceEntity(monster, room, new Dictionary<string, string>
                    {
                        { "PlacementRule", "RandomEdge" }
                    });
                }
                return;
            }

            foreach (var monster in monsters)
            {
                var placementParams = new Dictionary<string, string>();

                switch (monster.Behavior)
                {
                    case MonsterBehaviorType.HumanoidRanged:
                    case MonsterBehaviorType.MagicUser:
                        // Place as far as possible from the heroes' centroid
                        placementParams["PlacementRule"] = "AsFarAsPossible";
                        placementParams["PlacementTarget"] = heroes.First().Name; // Placeholder for targeting logic
                        break;
                    default: // Melee and other types
                             // Place randomly, but at least 1 square away from heroes
                        placementParams["PlacementRule"] = "RandomEdge";
                        break;
                }

                _placement.PlaceEntity(monster, room, placementParams);
            }
        }

        private async Task CheckForEncounter(Room newRoom)
        {
            if (!newRoom.RandomEncounter)
            {
                _dungeon.RoomsWithoutEncounters++;
            }

            int encounterChance = (newRoom.Category == RoomCategory.Room ? 50 : 30) + _dungeon.EncounterChanceModifier;

            if (_dungeon.RoomsWithoutEncounters >= 4)
            {
                encounterChance += 10;
            }

            var rollResult = await _userRequest.RequestRollAsync("Roll for encounter.", "1d100");
            await Task.Yield();

            if (rollResult.Roll <= encounterChance)
            {
                // Encounter Triggered!
                Console.WriteLine("Encounter! Monsters appear!");
                _dungeon.RoomsWithoutEncounters = 0;

                SpawnRandomEncounter(newRoom);
            }
            else
            {
                // No encounter
                Console.WriteLine("No encounter this time.");
                _dungeon.RoomsWithoutEncounters++;
            }

            // No encounter
            Console.WriteLine("The room is quiet... for now.");
            _dungeon.RoomsWithoutEncounters++;
        }

        public void WinBattle()
        {
            _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.WinBattle);
        }

        public bool UpdateThreat(int amount)
        {
            _dungeon.ThreatLevel += amount;
            if (_dungeon.ThreatLevel >= _dungeon.WhenSpawnWanderingMonster)
            {
                // Trigger wandering monster logic...
                _dungeon.ThreatLevel -= 5;
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

            if (_dungeon.HeroParty == null)
            {
                return "Cannot rest without a party.";
            }

            // Call the resting service, specifying the Dungeon context
            var restResult = await _partyResting.AttemptRest(RestingContext.Dungeon, _dungeon);

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
            _dungeon.AvailableLevers = _lever.PrepareLeverDeck(partyHasClue);

            Console.WriteLine($"Levers activated! {_dungeon.AvailableLevers.Count} levers are available.");
        }

        /// <summary>
        /// Called when a player chooses to pull a specific lever from the available deck.
        /// </summary>
        /// <param name="leverIndex">The index of the lever to pull from the AvailableLevers list.</param>
        public void PullLever(int leverIndex)
        {
            if (_dungeon.AvailableLevers == null || leverIndex < 0 || leverIndex >= _dungeon.AvailableLevers.Count)
            {
                return;
            }

            // Get the color and remove it from the deck
            LeverColor pulledLeverColor = _dungeon.AvailableLevers[leverIndex];
            _dungeon.AvailableLevers.RemoveAt(leverIndex);

            // Resolve the event
            var result = _lever.PullLever(pulledLeverColor);
            Console.WriteLine($"Pulled a {result.LeverColor} lever! Event: {result.Description}");

            //TODO: Handle the result of the lever pull, e.g., update dungeon state, trigger events, etc.
        }

        internal async Task SpawnMimicEncounterAsync(Chest chest, bool detected = false)
        {
            var mimic = _encounter.GetRandomEncounterByType(EncounterType.Mimic)[0];
            mimic.Position = chest.Position;
            mimic.Room = chest.Room;
            if (detected) await StatusEffectService.AttemptToApplyStatusAsync(mimic, new ActiveStatusEffect(StatusEffectType.DetectedMimic, -1), _powerActivation);

            if (chest.Position != null)
            {
                var square = GridService.GetSquareAt(chest.Position, _dungeon.DungeonGrid);
                if ( square != null)
                {
                    square.Furniture = SearchService.GetFurnitureByName("Floor");
                }
            }
        }

        internal void SpawnSkeletonsTrapEncounter(Room room, int amount)
        {
            var paramaters = new Dictionary<string, string>()
            {
                { "Name", "Skeleton" },
                { "Count", amount.ToString() },
                { "Armour", "1" },
                { "Shield", "true" },
                { "Weapons", "Broadsword" }
            };
            var skeletons = _encounter.GetEncounterByParams(paramaters);

            foreach (var skeleton in skeletons)
            {
                var placementParams = new Dictionary<string, string>()
                {
                    { "PlacementRule", "RandomEdge" }
                };

                _placement.PlaceEntity(skeleton, room, placementParams);
            }
        }
    }
}