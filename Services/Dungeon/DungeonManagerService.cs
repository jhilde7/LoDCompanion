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
        private readonly QuestEncounterService _questEncounter;
        private readonly RoomFactoryService _roomFactory;
        private readonly GameStateManagerService _gameManager;
        private readonly DungeonBuilderService _dungeonBuilder;
        private readonly ThreatService _threatService;
        private readonly LockService _lockService;
        private readonly TrapService _trapService;
        private readonly CombatManagerService _combatManager;
        private readonly PartyRestingService _partyRestingService;

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
            ThreatService threatService,
            LockService lockService,
            TrapService trapService,
            PartyRestingService partyRestingService,
            CombatManagerService combatManager)
        {
            _gameData = gameData;
            _wanderingMonster = wanderingMonsterService;
            _encounter = encounterService;
            _questEncounter = questEncounterService;
            _roomFactory = roomFactoryService;
            _gameManager = gameStateManagerService;
            _dungeonBuilder = dungeonBuilder;
            _threatService = threatService;
            _lockService = lockService;
            _trapService = trapService;
            _partyRestingService = partyRestingService;
            _combatManager = combatManager;

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
            _threatService.IncreaseThreat(DungeonState, 1);

            // Step 2: Roll for Trap (d6)
            if (RandomHelper.RollDie("D6") == 6)
            {
                door.IsTrapped = true;
                var trap = Trap.GetRandomTrap(); // A new trap is generated
                DungeonState.CurrentRoom.CurrentTrap = trap;

                // Step 3: Resolve Trap
                if (!_trapService.DetectTrap(hero, trap))
                {
                    // Failed to detect, trap is sprung!
                    door.IsTrapped = false;
                    return _trapService.TriggerTrap(hero, trap);
                }
                else
                {
                    // Trap detected. The UI would ask the player to disarm or trigger it.
                    // For now, we assume they attempt to disarm.
                    if (!_trapService.DisarmTrap(hero, trap))
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
            RevealNextRoom();
            return "The door creaks open...";
        }

        /// <summary>
        /// Reveals the next room after a door has been successfully opened.
        /// </summary>
        private void RevealNextRoom()
        {
            if (DungeonState.ExplorationDeck.TryDequeue(out RoomInfo nextRoomInfo))
            {
                var newRoom = _roomFactory.CreateRoom(nextRoomInfo.Name);
                if (newRoom != null)
                {
                    DungeonState.CurrentRoom = newRoom;
                    CheckForEncounter(newRoom);
                }
            }
            else
            {
                // No more rooms in this path.
                if (DungeonState.CurrentRoom != null) DungeonState.CurrentRoom.IsDeadEnd = true;
            }
        }

        private void CheckForEncounter(RoomService newRoom)
        {
            if (!newRoom.RandomEncounter)
            {
                DungeonState.RoomsWithoutEncounters++;
                return;
            }

            int encounterChance = (newRoom.Type == "Room") ? 50 : 30;

            if (DungeonState.RoomsWithoutEncounters >= 4)
            {
                encounterChance += 10;
            }

            int roll = RandomHelper.RollDie("D100");

            if (roll <= encounterChance)
            {
                // Encounter Triggered!
                Console.WriteLine("Encounter! Monsters appear!");
                DungeonState.RoomsWithoutEncounters = 0;

                // TODO: Replace these placeholders with actual monster/weapon data loading
                var monsterTemplates = new Dictionary<string, Monster>();
                var weaponTemplates = new Dictionary<string, MonsterWeapon>();

                List<Monster> monsters = new List<Monster>();

                if (DungeonState.Quest != null)
                {
                    monsters = _encounter.GetEncounters(DungeonState.Quest.EncounterType, monsterTemplates, weaponTemplates); 
                }
                else
                {
                    monsters = _encounter.GetEncounters(EncounterType.Beasts, monsterTemplates, weaponTemplates);
                }

                if (monsters.Any())
                {
                    PlaceMonsters(monsters, newRoom);
                    // Hand off to the CombatManager to start the battle
                    _combatManager.StartCombat(DungeonState.HeroParty.Heroes, monsters);
                }
            }
            else
            {
                // No encounter
                Console.WriteLine("The room is quiet... for now.");
                DungeonState.RoomsWithoutEncounters++;
            }
        }

        private void PlaceMonsters(List<Monster> monsters, RoomService room)
        {
            // This is a simplified representation of monster placement. A real implementation
            // would require a grid system for the room and hero positions.

            foreach (var monster in monsters)
            {
                if (monster.Type.Contains("Archer") || monster.Spells.Any())
                {
                    // Ranged/Magic users are placed as far away as possible with LOS.
                    Console.WriteLine($"Placing ranged monster: {monster.Name} at the back of the room.");
                    // TODO: Add logic to find the furthest valid grid square.
                }
                else
                {
                    // Melee monsters are placed randomly, at least 1 square away from heroes.
                    Console.WriteLine($"Placing melee monster: {monster.Name} randomly.");
                    // TODO: Add logic to find a random valid grid square away from the party.
                }
            }
            // After placement, the room's state would be updated with monster positions.
            room.IsEncounter = true;
        }

        public void WinBattle()
        {
            _threatService.IncreaseThreat(DungeonState, 1);
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

            if (DungeonState.HeroParty == null)
            {
                return "Cannot rest without a party.";
            }

            // Call the resting service, specifying the Dungeon context
            var restResult = _partyRestingService.AttemptRest(DungeonState.HeroParty, RestingContext.Dungeon, DungeonState);

            if (restResult.WasInterrupted)
            {
                // _combatManager.StartCombat(...);
            }

            return restResult.Message;
        }
    }
}