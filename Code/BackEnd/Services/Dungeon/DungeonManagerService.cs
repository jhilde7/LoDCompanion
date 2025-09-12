using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Game;
using LoDCompanion.Code.BackEnd.Services.Player;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Dungeon
{
    public class DungeonState
    {
        public Party HeroParty { get; set; } = new Party();
        public Quest? Quest { get; set; }
        public int RoomsWithoutEncounters { get; set; } = 0;
        public int MinThreatLevel { get; set; }
        public int MaxThreatLevel { get; set; }
        public int ThreatLevel { get; set; }
        public int EncounterChanceModifier { get; set; } = 0;
        public int ScenarioRollModifier { get; set; } = 0;
        public Dictionary<string, string> DungeonRules { get; set; } = new();
        public bool SpawnWanderingMonster => ThreatLevel >= MaxThreatLevel;
        public Room? StartingRoom { get; set; }
        public Room? CurrentRoom { get; set; }
        public Queue<Room> ExplorationDeck { get; set; } = new Queue<Room>();
        public List<Room> RoomsInDungeon { get; set; } = new List<Room>();
        public List<LeverColor> AvailableLevers { get; set; } = new List<LeverColor>();
        public List<WanderingMonsterState> WanderingMonsters { get; set; } = new List<WanderingMonsterState>();
        public List<Monster> RevealedMonsters { get; set; } = new List<Monster>();
        public List<string> DefeatedUniqueMonsters { get; set; } = new List<string>();
        public Dictionary<GridPosition, GridSquare> DungeonGrid { get; private set; } = new Dictionary<GridPosition, GridSquare>();
        public List<CombatRule> QuestCombatRules { get; set; } = new();

        public List<Character> AllCharactersInDungeon => [.. RevealedMonsters, .. HeroParty?.Heroes ?? new List<Hero>()];

        public bool CanSpawnWanderingMonster { get; set; } = true;
        public int TrapChanceOnDoor { get; set; } = 6;
        public bool NextDoorIsUnlockedDisarmed { get; set; }
        public bool NextLockedDoorIsUnlocked { get; set; }
        public bool NextTrapWillBeDisarmed { get; internal set; }
        public EncounterType EncounterType { get; internal set; }

        public Party? SetParty(Party? party)
        {
            if (party != null)
            {
                HeroParty = party; 
            }
            return HeroParty;
        }

        /// <summary>
        /// Finds the room that contains a specific grid position.
        /// </summary>
        public Room? FindRoomAtPosition(GridPosition position)
        {
            // This method iterates through all known rooms to find which one contains the coordinate.
            foreach (Room room in RoomsInDungeon) // Assumes DungeonState can provide all rooms
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
    }

    public class DungeonManagerService
    {
        private readonly WanderingMonsterService _wanderingMonster = new WanderingMonsterService();
        private readonly Lever _lever = new Lever();
        private readonly EncounterService _encounter = new EncounterService();
        private readonly ThreatService _threat = new ThreatService();
        private readonly TrapService _trap = new TrapService();
        private readonly RoomService _room = new RoomService();
        private readonly PlacementService _placement = new PlacementService();
        private readonly UserRequestService _userRequest = new UserRequestService();
        private readonly PowerActivationService _powerActivation = new PowerActivationService();
        private readonly SearchService _search = new SearchService();
        private readonly QuestSetupService _questSetup = new QuestSetupService();
        private readonly LockService _lock = new LockService();
        private readonly ActionService _action = new ActionService();
        private readonly PartyRestingService _partyResting;
        private readonly DungeonBuilderService DungeonBuilder = new DungeonBuilderService(new RoomService());
        private readonly CombatManagerService _combatManager = new CombatManagerService();
        private readonly PartyManagerService _partyManager;
        private readonly GameState _gameState;

        public DungeonState Dungeon => _partyManager.SetCurrentDungeon(new DungeonState());
        public Party? HeroParty => Dungeon.SetParty(_partyManager.Party);
        public Room? StartingRoom => Dungeon.StartingRoom;
        public Room? CurrentRoom => Dungeon.CurrentRoom;
        public PartyManagerService PartyManager => _partyManager;


        public DungeonManagerService(
            PartyManagerService partyManagerService,
            GameStateManagerService gameState)
        {
            _partyManager = partyManagerService;
            _partyResting = new PartyRestingService(_partyManager);
            _gameState = gameState.GameState;

            _partyManager.SetMaxMorale();

            _action.OnOpenDoor += HandleOpenDoor;
            _action.OnRemoveCobwebs += HandleRemoveCobwebs;
            _wanderingMonster.OnSpawnRandomEncounter += HandleSpawnRandomEncounter;
            _lever.OnLeverResult += HandleLeverResultAsync;
            _trap.OnSpawnSkeletonsTrapEncounter += HandleSkeletonsTrapSpawnEncounter;
            _trap.OnSpawnMimicEncounterAsync += HandleMimicSpawnEncounterAsync;
            _trap.OnAddExplorationCardsToPiles += AddExplorationCardsToPiles;
            _trap.OnSpawnCageTrapEncounter += HandleCageTrapSpawnEncounter;
            _search.OnSpawnTreasureRoom += SpawnTreasureRoom;
            _partyResting.OnDungeonRestAsync += HandleOnDungeonRestAsync;
            _combatManager.OnTriggerSpawnEncounter += HandleTriggerSpawnEncounterAsync;
            _powerActivation.OnUpdateMorale += HandleUpdateMorale;
            _powerActivation.OnUpdateThreat += HandleUpdateThreat;
            _questSetup.OnGetEncounterType += HandleGetEncounterType;
            _questSetup.OnAddQuestCombatRules += HandleAddQuestCombatRules;
            _questSetup.OnSetDungeonRule += HandleSetDungeonRule;
            _lock.OnUpdateThreatLevelByThreatActionType += HandleUpdateThreatLevelByThreatActionType;
            _action.OnUpdateThreatLevelByThreatActionType += HandleUpdateThreatLevelByThreatActionType;
            _combatManager.OnScenarioRoll += HandleScenarioRoll;
        }

        public void Dispose()
        {
            _action.OnOpenDoor -= HandleOpenDoor;
            _action.OnRemoveCobwebs -= HandleRemoveCobwebs;
            _wanderingMonster.OnSpawnRandomEncounter -= HandleSpawnRandomEncounter;
            _lever.OnLeverResult -= HandleLeverResultAsync;
            _trap.OnSpawnSkeletonsTrapEncounter -= HandleSkeletonsTrapSpawnEncounter;
            _trap.OnSpawnMimicEncounterAsync -= HandleMimicSpawnEncounterAsync;
            _trap.OnAddExplorationCardsToPiles -= AddExplorationCardsToPiles;
            _trap.OnSpawnCageTrapEncounter -= HandleCageTrapSpawnEncounter;
            _search.OnSpawnTreasureRoom -= SpawnTreasureRoom;
            _partyResting.OnDungeonRestAsync -= HandleOnDungeonRestAsync;
            _combatManager.OnTriggerSpawnEncounter -= HandleTriggerSpawnEncounterAsync;
            _powerActivation.OnUpdateMorale -= HandleUpdateMorale;
            _powerActivation.OnUpdateThreat -= HandleUpdateThreat;
            _questSetup.OnGetEncounterType -= HandleGetEncounterType;
            _questSetup.OnAddQuestCombatRules -= HandleAddQuestCombatRules;
            _questSetup.OnSetDungeonRule -= HandleSetDungeonRule;
            _lock.OnUpdateThreatLevelByThreatActionType -= HandleUpdateThreatLevelByThreatActionType;
            _action.OnUpdateThreatLevelByThreatActionType -= HandleUpdateThreatLevelByThreatActionType;
            _combatManager.OnScenarioRoll -= HandleScenarioRoll;
        }

        private async Task HandleMimicSpawnEncounterAsync(Chest chest, bool detected = false)
        {
            var mimic = _encounter.GetRandomEncounterByType(EncounterType.Mimic)[0];
            mimic.Position = chest.Position;
            mimic.Room = chest.Room;
            if (detected) await StatusEffectService.AttemptToApplyStatusAsync(mimic, new ActiveStatusEffect(StatusEffectType.DetectedMimic, -1), _powerActivation);

            if (chest.Position != null)
            {
                var square = GridService.GetSquareAt(chest.Position, Dungeon.DungeonGrid);
                if (square != null)
                {
                    square.Furniture = SearchService.GetFurnitureByName("Floor");
                }
            }
        }

        private void HandleSkeletonsTrapSpawnEncounter(Room room, int amount)
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

        private void HandleCageTrapSpawnEncounter(Room room)
        {
            foreach (var door in room.Doors)
            {
                var placementParams = new Dictionary<string, string>
                {
                    { "PlacementRule", "RelativeToPosition" },
                    { "PlacementPosition", $"{door.PassagewaySquares[0].X},{door.PassagewaySquares[0].Y},{door.PassagewaySquares[0].Z}" }
                };
                var connectedRoom = door.ConnectedRooms.FirstOrDefault(r => r != room);
                if (connectedRoom != null) SpawnRandomEncounter(connectedRoom, placementParams: placementParams);
                door.Lock.SetLockState(0, 0);
                door.Trap.IsDisarmed = true;
            }
        }

        private void SpawnTreasureRoom(Room room)
        {
            var treasureRoomDeck = new Queue<Room>();
            var tresureRoom = _room.CreateRoom("R10");
            treasureRoomDeck.Enqueue(tresureRoom);
            Door newDoor = _room.AddDoorToRoom(room, _placement, Dungeon, explorationDeck: treasureRoomDeck);
            newDoor.Trap.IsDisarmed = true;
            newDoor.Lock.SetLockState(0, 0);
        }

        private async Task HandleLeverResultAsync(Hero hero, LeverResult result)
        {
            if (result.AddExplorationCards) AddExplorationCardsToPiles(2);
            if (result.CloseDungeonEntrance) Dungeon.CanSpawnWanderingMonster = false;
            if (result.CreateTreasureRoom) SpawnTreasureRoom(hero.Room);
            if (result.DoorTrapChanceIncrease) Dungeon.TrapChanceOnDoor = 5;
            if (result.NextDoorIsUnlockedDisarmed) Dungeon.NextDoorIsUnlockedDisarmed = true;
            if (result.NextLockedDoorIsUnlocked) Dungeon.NextLockedDoorIsUnlocked = true;
            if (result.NextTrapWillBeDisarmed) Dungeon.NextTrapWillBeDisarmed = true;
            if (result.PartyGainedLuckPoint) PartyManager.PartyLuck += 1;
            if (result.SpawnPortcullis) hero.Room.Doors.ForEach(d => d.State = DoorState.Portcullis);
            if (result.SpawnWanderingMonster) _wanderingMonster.SpawnWanderingMonster(Dungeon);
            if (result.LockADoor)
            {
                var openDoor = hero.Room.Doors.FirstOrDefault(d => d.IsOpen);
                if (openDoor != null)
                {
                    openDoor.State = DoorState.Closed;
                    openDoor.Lock.SetLockState(25, 25);
                }
                else
                {
                    var doors = hero.Room.Doors;
                    doors.Shuffle();
                    doors[0].Lock.SetLockState(25, 25);
                }
            }
            if (result.TriggerCageTrap)
            {
                var trap = new Trap(TrapType.CageTrap, 5, 10, $"A rattling noise makes {hero.Name} look up, only to realise that an iron case is descending from the ceiling.");
                await _trap.TriggerTrapAsync(hero, trap, trapTriggered: true);
            }
            if (result.TriggerPitTrap)
            {
                if (PartyManager.Party != null)
                {
                    var heroes = PartyManager.Party.Heroes.ToList();
                    while (heroes[0] == hero) heroes.Shuffle();
                    var trap = new Trap(TrapType.TrapDoor, 5, 10, $"Suddenly the floor gives way under {heroes[0].Name}", "A random character must pass a DEX test or fall, taking 1d10 DMG (no armour/NA).") { DamageDice = "1d10" };
                    await _trap.TriggerTrapAsync(heroes[0], trap, trapTriggered: true);
                }
            }

            _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.Lever, Dungeon, result.ThreatIncrease);
            PartyManager.UpdateMorale(amount: -result.PartyMoraleDecrease);
            await hero.TakeSanityDamage(result.SanityDecrease, (new FloatingTextService(), hero.Position), _powerActivation);
            if (result.PartySanityDecrease > 0 && PartyManager.Party != null)
            {
                foreach (var h in PartyManager.Party.Heroes)
                {
                    await h.TakeSanityDamage(result.PartySanityDecrease, (new FloatingTextService(), h.Position), _powerActivation);
                }
            }
        }

        private void HandleSpawnRandomEncounter(Room room)
        {
            SpawnRandomEncounter(room);
        }

        private async Task<bool> HandleOpenDoor(Door door)
        {
            return await RevealNextRoomAsync(door);
        }

        private async Task<bool> HandleRemoveCobwebs(Hero hero, Door door)
        {
            var rollResult = await _userRequest.RequestRollAsync("Roll to determine effect.", "1d10");
            await Task.Yield();
            int roll = rollResult.Roll;

            _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.RemoveCobwebs, Dungeon, 1);

            if (roll >= 9)
            {
                SpawnGiantSpidersFromRemoveCobwebs(RandomHelper.RollDie(DiceType.D2), hero.Room);
                return true;
            }
            else return false;
        }

        private async Task<RestResult> HandleOnDungeonRestAsync(PartyManagerService partyManager)
        {
            var result = new RestResult();

            _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.Rest, Dungeon);
            var threatResult = await HandleScenarioRoll(isInBattle: false);
            result.WasInterrupted = threatResult.ThreatEventTriggered;

            for (int i = 0; i < 3; i++)
            {
                if (await _wanderingMonster.ProcessWanderingMonstersAsync(Dungeon.WanderingMonsters)) result.WasInterrupted = true;
            }

            if (!result.WasInterrupted)
            {
                result.WasSuccessful = true;
                PartyManager.UpdateMorale(changeEvent: MoraleChangeEvent.Rest);
                result.Message = "The party rests successfully.";
            }

            return result;
        }

        private async Task HandleTriggerSpawnEncounterAsync(Dictionary<string, string> parameters)
        {
            var monstersToSpawn = new List<Monster>();
            var currentRoom = Dungeon?.CurrentRoom;
            if (currentRoom == null) return;

            // Check if we are spawning from a chart or by name
            if (parameters.TryGetValue("ChartName", out var chartNameStr) && Enum.TryParse<EncounterType>(chartNameStr, out var chartType))
            {
                monstersToSpawn = _encounter.GetRandomEncounterByType(chartType);
            }
            else
            {
                monstersToSpawn = _encounter.GetEncounterByParams(parameters);
            }

            // Place the new monsters
            foreach (var monster in monstersToSpawn)
            {
                _placement.PlaceEntity(monster, currentRoom, parameters);
            }

            // Add the newly spawned monsters to the ongoing combat
            _combatManager.AddMonstersToCombat(monstersToSpawn);

            await Task.CompletedTask;
        }

        private async Task<bool> HandleUpdateMorale(int amount)
        {
            await Task.Yield();
            return PartyManager.UpdateMorale(amount) >= 0;
        }

        private async Task<bool> HandleUpdateThreat(int amount)
        {
            await Task.Yield();
            return UpdateThreat(amount);
        }

        private void HandleSetDungeonRule(string rule, string value)
        {
            Dungeon.DungeonRules[rule] = value;
        }

        private async Task<EncounterType> HandleGetEncounterType()
        {
            await Task.Yield();
            return Dungeon.EncounterType;
        }

        private void HandleAddQuestCombatRules(CombatRule rule)
        {
            Dungeon.QuestCombatRules.Add(rule);
        }

        private void HandleUpdateThreatLevelByThreatActionType(ThreatActionType action)
        {
            _threat.UpdateThreatLevelByThreatActionType(action, Dungeon);
        }

        // Create a new method to start a quest
        public void StartQuest(Party heroParty, Quest quest)
        {
            // 1. Initialize the basic dungeon state
            Dungeon.HeroParty = heroParty;
            Dungeon.MinThreatLevel = 1;
            Dungeon.MaxThreatLevel = 10; // This can be overridden by quest specifics
            Dungeon.ThreatLevel = 0;
            Dungeon.DungeonRules["WanderingMonsterAtThreat"] = "5"; // This can also be overridden

            // 2. Generate the exploration deck using the DungeonBuilderService
            List<Room> explorationDeck = DungeonBuilder.CreateDungeonDeck(quest);
            Dungeon.ExplorationDeck = new Queue<Room>(explorationDeck);

            // 3. Create and set the starting room
            Dungeon.StartingRoom = _room.CreateRoom(quest.StartingRoom?.Name ?? "Start Tile");
            Dungeon.CurrentRoom = Dungeon.StartingRoom;
        }

        public void InitializeDungeon(Party initialHeroes, Quest quest)
        {
            // Reset dungeon-specific state
            Dungeon.Quest = quest;
            Dungeon.RoomsWithoutEncounters = 0;
            Dungeon.EncounterChanceModifier = 0;
            Dungeon.ScenarioRollModifier = 0;
            Dungeon.StartingRoom = null;
            Dungeon.CurrentRoom = null;
            Dungeon.ExplorationDeck.Clear();
            Dungeon.RoomsInDungeon.Clear();
            Dungeon.AvailableLevers.Clear();
            Dungeon.WanderingMonsters.Clear();
            Dungeon.RevealedMonsters.Clear();
            Dungeon.DungeonGrid.Clear();
            Dungeon.CanSpawnWanderingMonster = true;
            Dungeon.TrapChanceOnDoor = 6;
            Dungeon.NextDoorIsUnlockedDisarmed = false;
            Dungeon.NextLockedDoorIsUnlocked = false;
            Dungeon.NextTrapWillBeDisarmed = false;
            if (quest.EncounterType == EncounterType.Random)
            {
                // Define the pool of potential random encounter types
                var rollableTypes = new List<EncounterType>
                {
                    EncounterType.Bandits_Brigands,
                    EncounterType.Orcs_Goblins,
                    EncounterType.Undead,
                    EncounterType.Beasts,
                    EncounterType.DarkElves,
                    EncounterType.Reptiles,
                };
                Dungeon.EncounterType = rollableTypes[RandomHelper.GetRandomNumber(0, rollableTypes.Count - 1)];
            }
            else
            {
                Dungeon.EncounterType = quest.EncounterType;
            }

            // Initialize the new dungeon
            Dungeon.HeroParty = initialHeroes;
            Dungeon.MinThreatLevel = quest.MinThreatLevel;
            Dungeon.MaxThreatLevel = quest.MaxThreatLevel;
            Dungeon.ThreatLevel = quest.StartThreatLevel;
            Dungeon.DungeonRules["WanderingMonsterAtThreat"] = "5";

            List<Room> explorationDeck = DungeonBuilder.CreateDungeonDeck(quest);
            Dungeon.ExplorationDeck = new Queue<Room>(explorationDeck);

            Dungeon.StartingRoom = _room.CreateRoom(quest.StartingRoom?.Name ?? "Start Tile");
            Dungeon.CurrentRoom = Dungeon.StartingRoom;
        }

        public void LeaveDungeon()
        {
            if (_gameState.Settlements == null) return;
            foreach (var settlement in _gameState.Settlements)
            {
                settlement.State.DungeonsBetweenVisits++;
            }
        }

        public async Task ProcessTurnAsync()
        {
            bool isInBattle = !_combatManager.IsCombatOver;
            await HandleScenarioRoll(isInBattle);

            if (Dungeon != null)
            {
                // After hero actions, move any wandering monsters.
                if (await _wanderingMonster.ProcessWanderingMonstersAsync(Dungeon.WanderingMonsters))
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
            var threatResult = await _threat.ProcessScenarioRoll(isInBattle, HeroParty, Dungeon);

            if (threatResult != null)
            {
                // A threat event was triggered. We can now handle the result.
                Console.WriteLine($"Threat Event: {threatResult.Description}");

                if (threatResult.SpawnWanderingMonster)
                {
                    _wanderingMonster.SpawnWanderingMonster(Dungeon);
                }
                if (threatResult.SpawnTrap)
                {
                    if (Dungeon.HeroParty != null && Dungeon.HeroParty.Heroes.Any())
                    {
                        var heroes = Dungeon.HeroParty.Heroes;
                        heroes.Shuffle();
                        var randomHero = heroes[0];
                        var trap = new Trap(guaranteedTrap: true);

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

            if (Dungeon.SpawnWanderingMonster)
            {
                _wanderingMonster.SpawnWanderingMonster(Dungeon);
                _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.WanderingMonsterSpawned, Dungeon);
            }

            return threatResult;
        }

        /// <summary>
        /// Adds one new, random Exploration Card to the top of each active door's deck.
        /// This is triggered by a specific threat event.
        /// </summary>
        public void AddExplorationCardsToPiles(int amount)
        {
            if (Dungeon.CurrentRoom == null || Dungeon.ExplorationDeck == null) return;

            var explorationCards = new Queue<Room>();
            var explorationRooms = _room.GetExplorationDeckRooms();
            explorationRooms.Shuffle();

            foreach (var room in explorationRooms)
            {
                explorationCards.Enqueue(_room.CreateRoom(room.Name));
            }

            var roomsExplorationDoors = Dungeon.RoomsInDungeon
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
        /// Reveals the next room after a door has been successfully opened.
        /// </summary>
        private async Task<bool> RevealNextRoomAsync(Door openedDoor)
        {
            _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.OpenDoorOrChest, Dungeon);
            // this should only happen on the very first door opened
            if (openedDoor.ExplorationDeck == null)
            {
                return false; // No exploration deck to draw from
            }

            if (openedDoor.ExplorationDeck.TryDequeue(out Room? nextRoomInfo) && nextRoomInfo != null && Dungeon !=null)
            {
                var newRoom = _room.CreateRoom(nextRoomInfo.Name ?? string.Empty);
                if (newRoom != null)
                {
                    newRoom.Dungeon = Dungeon;
                    // Determine the room's default entry orientation from its RoomInfo
                    Orientation defaultEntryOrientation = GridService.GetOrientationFromPosition(newRoom.EntryPositions.First(), newRoom.Width, newRoom.Height);

                    // Calculate the rotation needed to match the door
                    int rotationAngle = ((int)openedDoor.Orientation - (int)defaultEntryOrientation + 360) % 360;
                    newRoom.RotationAngle = rotationAngle;

                    // The room's dimensions might swap if rotated 90 or 270 degrees
                    if (rotationAngle == 90 || rotationAngle == 270)
                    {
                        newRoom.Size = [newRoom.Height, newRoom.Width];
                    }

                    GridPosition newRoomOffset = CalculateNewRoomOffset(openedDoor, newRoom);
                    GridService.PlaceRoomOnGrid(newRoom, newRoomOffset, Dungeon.DungeonGrid);

                    // Link the rooms logically
                    newRoom.ConnectedRooms.Add(openedDoor.ConnectedRooms[0]);
                    openedDoor.ConnectedRooms.Add(newRoom);
                    newRoom.Doors.Add(openedDoor);
                    openedDoor.PassagewaySquares = GetPassagewaySquares(openedDoor.ConnectedRooms[0], newRoom, Dungeon.DungeonGrid);

                    Dungeon.CurrentRoom = newRoom;
                    Dungeon.RoomsInDungeon.Add(newRoom);
                    if (Dungeon.Quest != null && newRoom.Name == Dungeon.Quest.ObjectiveRoom?.Name)
                    {
                        // This is the objective room, run its specific setup actions
                        await _questSetup.ExecuteRoomSetupAsync(Dungeon.Quest, newRoom);

                        // Start combat if monsters were spawned
                        if (newRoom.MonstersInRoom != null && newRoom.MonstersInRoom.Any())
                        {
                            _combatManager.SetupCombat(newRoom.HeroesInRoom ?? new List<Hero>(), newRoom.MonstersInRoom);
                        }
                    }
                    else
                    {
                        // It's a normal room, check for a random encounter
                        await CheckForEncounter(newRoom);
                    }

                    // Handle the remaining deck for the new room's exits
                    var remainingCards = openedDoor.ExplorationDeck.ToList();
                    openedDoor.ExplorationDeck = null; // The old door's deck is now processed

                    if (!remainingCards.Any())
                    {
                        // This path is a dead end as it has no more cards.
                        newRoom.DoorCount = 1;
                        return true;
                    }

                    var newDoors = newRoom.Doors.Where(d => d != openedDoor).ToList();

                    if (Dungeon.DungeonRules["DoorType"] == "CobwebCovered")
                    {
                        newDoors.ForEach(d => d.State = DoorState.CobwebCovered);
                    }
                    
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
                        _room.AddDoorToRoom(newRoom, _placement, Dungeon, exitDoor.ExplorationDeck);
                    }
                }
                return true;
            }
            else return false;
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
        public void SpawnRandomEncounter(Room room, EncounterType? encounterType = null, Dictionary<string, string>? placementParams = null)
        {
            room.MonstersInRoom = new List<Monster>();
            var dungeonEncounterType = EncounterType.Beasts;

            dungeonEncounterType = Dungeon.EncounterType;

            if (room.EncounterType.HasValue)
            {
                room.MonstersInRoom = _encounter.GetRandomEncounterByType(room.EncounterType.Value, dungeonEncounterType: dungeonEncounterType);
            }
            else if (encounterType.HasValue)
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
                PlaceMonsters(room, room.MonstersInRoom, placementParams);
            }
        }

        /// <summary>
        /// Places monsters in a room based on their behavior type.
        /// </summary>
        /// <param name="room">The room to place the monsters in.</param>
        /// <param name="monsters">The list of monsters to be placed.</param>
        private void PlaceMonsters(Room room, List<Monster> monsters, Dictionary<string, string>? placementParams = null)
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

            if (placementParams == null)
            {
                foreach (var monster in monsters)
                {
                    placementParams ??= new Dictionary<string, string>();

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
                            placementParams["PlacementRule"] = "RandomSquare";
                            break;
                    }

                    _placement.PlaceEntity(monster, room, placementParams);
                } 
            }
            else
            {
                foreach (var monster in monsters)
                {
                    _placement.PlaceEntity(monster, room, placementParams);
                }
            }
        }

        private async Task CheckForEncounter(Room newRoom)
        {
            if (!newRoom.RandomEncounter)
            {
                Dungeon.RoomsWithoutEncounters++;
            }

            int encounterChance = (newRoom.Category == RoomCategory.Room ? 50 : 30) + Dungeon.EncounterChanceModifier;

            if (Dungeon.RoomsWithoutEncounters >= 4)
            {
                encounterChance += 10;
            }

            var rollResult = await _userRequest.RequestRollAsync("Roll for encounter.", "1d100");
            await Task.Yield();

            if (rollResult.Roll <= encounterChance)
            {
                // Encounter Triggered!
                Console.WriteLine("Encounter! Monsters appear!");
                Dungeon.RoomsWithoutEncounters = 0;

                SpawnRandomEncounter(newRoom);
            }
            else
            {
                // No encounter
                Console.WriteLine("No encounter this time.");
                Dungeon.RoomsWithoutEncounters++;
            }

            // No encounter
            Console.WriteLine("The room is quiet... for now.");
            Dungeon.RoomsWithoutEncounters++;
        }

        public void WinBattle()
        {
            _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.WinBattle, Dungeon);
        }

        public bool UpdateThreat(int amount)
        {
            Dungeon.ThreatLevel += amount;
            if (Dungeon.DungeonRules.TryGetValue("WanderingMonsterAtThreat", out var threatValueStr) &&
                int.TryParse(threatValueStr, out var wanderingMonsterAtThreat))
            {
                if (Dungeon.ThreatLevel >= wanderingMonsterAtThreat)
                {
                    // Trigger wandering monster logic...
                    _wanderingMonster.SpawnWanderingMonster(Dungeon);
                    Dungeon.ThreatLevel -= 5; // Note: This logic can be adjusted based on your game rules
                }
            }
            return true;
        }

        private void SpawnGiantSpidersFromRemoveCobwebs(int amount, Room room)
        {
            var paramaters = new Dictionary<string, string>()
            {
                { "Name", "Giant Spider" },
                { "Count", amount.ToString() }
            };
            var spiders = _encounter.GetEncounterByParams(paramaters);

            foreach (var spider in spiders)
            {
                var placementParams = new Dictionary<string, string>()
                {
                    { "PlacementRule", "RandomSquare" }
                };

                _placement.PlaceEntity(spider, room, placementParams);
            }
        }
    }
}