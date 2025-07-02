using LoDCompanion.Services.Dungeon;
using LoDCompanion.Utilities;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;
using System.Text;

namespace LoDCompanion.Models.Dungeon
{
    // Represents a single room or corridor tile in the dungeon.
    public class RoomCorridor
    {
        private readonly GameDataService _gameData;
        // Public properties to hold the room's data and state.
        // These will be populated by a RoomFactoryService or DungeonManagerService.
        public string RoomName { get; set; } = string.Empty; // Default to empty string for safety
        public bool IsStartingTile { get; set; }
        public bool IsObjectiveRoom { get; set; }
        public bool HasLevers { get; set; } // Flag, actual lever logic in a service
        public string Type { get; set; } = "Room"; // Default type, can be "Room" or "Corridor"
        public string Description { get; set; } = string.Empty;
        public string? Special { get; set; }
        public bool HasSpecial { get; set; }
        public bool ActivateSpecial { get; set; } // Trigger for special room effects, handled by a service
        public int ThreatLevelModifier { get; set; } = 0;
        public int PartyMoraleModifier { get; set; } = 0;
        public int[][] Size { get; set; } = new int[2][]; // Represents width/length or dimensions
        public List<DoorChest> Doors { get; set; } = new List<DoorChest>();
        public bool IsDeadEnd { get; set; }
        public List<Furniture> FurnitureList { get; set; } = new List<Furniture>(); // List of furniture types in the room
        public bool RandomEncounter { get; set; } // Flag for whether a random encounter can occur
        public bool RollEncounter { get; set; } // Trigger for rolling an encounter, handled by a service
        public int EncounterRoll { get; set; } = 0;
        public int EncounterModifier { get; set; } = 0;
        public bool IsEncounter { get; set; } // Indicates if an encounter is present
        public bool HasBeenSearched { get; set; }
        public bool PartySearch { get; set; }
        public bool SearchRoomTrigger { get; set; } // Trigger for room search, handled by a service
        public int SearchRoll { get; set; } = 0;
        public int TreasureRoll { get; set; } = 0;
        public Trap CurrentTrap { get; set; } = new Trap("No Trap", 0, 0, ""); // Default trap, no trap present initially
        public List<string> SearchResults { get; set; } = new List<string>();
        public List<RoomCorridor> ConnectedRooms { get; set; } = new List<RoomCorridor>(); // Represents connected dungeon segments
        public int DoorCount { get; set; }

        // Constructor for creating a RoomCorridor instance
        public RoomCorridor(GameDataService gameData)
        {
            _gameData = gameData;
        }

        /// <summary>
        /// Initializes the room's data based on a RoomInfo definition.
        /// This method would be called by a RoomFactoryService.
        /// </summary>
        /// <param name="roomInfo">The data object containing room definitions.</param>
        public void InitializeRoomData(RoomCorridor roomInfo)
        {
            // Basic Information
            RoomName = roomInfo.RoomName;
            Type = roomInfo.Type;
            Description = roomInfo.Description;
            Special = roomInfo.Special;
            HasSpecial = roomInfo.HasSpecial; // Assuming RoomInfo also has this flag

            // Stats
            ThreatLevelModifier = roomInfo.ThreatLevelModifier;
            PartyMoraleModifier = roomInfo.PartyMoraleModifier;
            Size = roomInfo.Size ?? new int[2][];

            // Furniture
            FurnitureList = roomInfo.FurnitureList ?? new List<Furniture>();
            // Note: Actual furniture *objects* or their creation are handled by a service
            // The original `furniture.Create(f, furniture, transform);` is removed.

            // Encounters
            // roomEncounter (original Unity field) would be replaced by logic in EncounterService
            // Here, we just store the encounter type name if available
            // This RoomCorridor no longer instantiates Encounters directly.
            // EncounterType = roomInfo.EncounterType; // If RoomInfo has this field
            EncounterModifier = roomInfo.EncounterModifier;
            RandomEncounter = roomInfo.RandomEncounter;

            // Doors and Dead Ends
            DoorCount = roomInfo.DoorCount;
            Doors.Clear(); // Clear any existing doors
            // Note: Creation of DoorChest *instances* will be handled by DungeonManagerService
            // or RoomFactoryService, not directly here via `Instantiate`.
            // The DoorCount is a definition for how many doors *should* be created externally.

            if (DoorCount == 0)
            {
                IsDeadEnd = true;
            }

            // Levers (If Applicable)
            HasLevers = roomInfo.HasLevers;
            // Note: `gameObject.AddComponent<Levers>();` is removed.
            // A separate `LeverService` would handle lever interactions.

            // Set initial state for various triggers
            RollEncounter = false;
            SearchRoomTrigger = false;
            ActivateSpecial = false; // Reset special activation on initialization
            HasBeenSearched = false;
            IsEncounter = false; // Start with no active encounter
        }


        /// <summary>
        /// This method encapsulates the original `SearchRoom` logic.
        /// The actual search roll and treasure generation would be performed by a service,
        /// which would then update the `SearchResults` of this room.
        /// The TreasureService and RandomHelper (formerly Utilities) are injected/passed.
        /// </summary>
        /// <param name="hero">The hero performing the search.</param>
        /// <param name="treasureService">The service for generating treasure.</param>
        /// <param name="randomHelper">A utility for random number generation.</param>
        public void PerformSearch(Hero hero)
        {
            if (HasBeenSearched)
            {
                SearchResults.Add("This room has already been searched.");
                return;
            }

            int searchTarget = hero.PerceptionSkill; // Assuming Perception is a property on Hero
            if (PartySearch) // PartySearch would be a flag set by game state/UI
            {
                searchTarget += 20;
            }
            if (hero.HasTorch) // Assuming HasTorch is a property on Hero
            {
                searchTarget += 5;
            }
            if (hero.HasLantern) // Assuming HasLantern is a property on Hero
            {
                searchTarget += 10;
            }

            SearchRoll = RandomHelper.GetRandomNumber(1, 100);

            if (Type == "Corridor")
            {
                SearchRoll += 10; // Corridor search bonus
            }

            SearchResults.Clear(); // Clear previous search results

            if (SearchRoll <= searchTarget)
            {
                TreasureRoll = RandomHelper.GetRandomNumber(1, 100);
                TreasureService treasure = new TreasureService(_gameData);
                // Original logic from SearchRoom(string type, bool isThief, int roll)
                int count = hero.IsThief ? 2 : 1; // Assuming IsThief is a property on Hero

                switch (TreasureRoll)
                {
                    case int r when r >= 1 && r <= 15:
                        SearchResults.Add("You found a secret door leading to a small treasure chamber. Place tile R10 adjacent to the current tile and add a door as usual. Re-roll if tile is in use. Once the heroes leave the treasure chamber, the door closes up and the tile can be removed.");
                        // Note: The logic for creating a new room/door (GetRoom, Instantiate)
                        // must be handled by DungeonManagerService. This just adds the text result.
                        // You'd have to signal back to the DungeonManagerService to create this room.
                        break;
                    case int r when r >= 16 && r <= 25:
                        SearchResults.AddRange(treasure.FoundTreasure("Fine", count));
                        break;
                    case int r when r >= 26 && r <= 40:
                        SearchResults.AddRange(treasure.FoundTreasure("Mundane", count));
                        break;
                    case int r when r >= 41 && r <= 45:
                        SearchResults.Add("You found a set of levers. (Interaction handled by a LeverService)");
                        HasLevers = true; // Update room state
                        break;
                    case int r when r >= 46 && r <= 50:
                        SearchResults.Add(treasure.GetTreasure("Coin", 0, 1, RandomHelper.GetRandomNumber(4, 40)));
                        break;
                    case int r when r >= 91 && r <= 100:
                        SearchResults.Add("You've sprung a trap!");
                        // A TrapService or DungeonManagerService would handle the trap instantiation/effect.
                        // You might set a flag here or return a Trap object.
                        // CurrentTrap = newTrap; // Example: if Trap is a simple data class.
                        break;
                    default:
                        SearchResults.Add("You found Nothing");
                        break;
                }
            }
            else
            {
                SearchResults.Add("Search Failed");
            }
            SearchRoomTrigger = false; // Reset trigger
            HasBeenSearched = true;
        }

        /// <summary>
        /// This method encapsulates the original SplitDungeonBetweenDoors logic.
        /// It modifies the current room's `Doors` and `ConnectedRooms` properties.
        /// The actual creation of new `DoorChest` instances (which were `Instantiate` calls)
        /// will be handled by the calling service (e.g., `DungeonManagerService` or `RoomFactoryService`).
        /// </summary>
        public void SplitDungeonBetweenDoors(List<RoomCorridor> availableDungeonCards, Func<DoorChest> doorFactory, Func<string, RoomCorridor> roomFactory)
        {
            if (IsObjectiveRoom || availableDungeonCards.Count == 0)
            {
                return;
            }

            // Copy to mutable lists for manipulation  
            List<RoomCorridor> workingDungeonCards = new List<RoomCorridor>(availableDungeonCards);
            List<DoorChest> workingDoors = new List<DoorChest>(Doors); // Assuming 'Doors' is already populated  

            // 1. Handle dead ends (repeatedly if necessary) - Logic to create secret doors and redistribute cards  
            while (workingDoors.Any(d => d.ConnectedRooms != null && d.ConnectedRooms.Count > 0 && d.ConnectedRooms[0].IsDeadEnd))
            {
                // Find the dead-end door and its remaining cards  
                DoorChest deadEndDoor = workingDoors.First(d => d.ConnectedRooms != null && d.ConnectedRooms.Count > 0 && d.ConnectedRooms[0].IsDeadEnd);
                List<RoomCorridor> remainingCards = new List<RoomCorridor>(deadEndDoor.ConnectedRooms ?? throw new NullReferenceException());

                // Clear the dead-end door's cards (making it inaccessible for its original purpose)  
                deadEndDoor.ConnectedRooms.Clear();

                // Distribute remaining cards to other doors or create a new secret door  
                int doorIndex = 0;
                while (remainingCards.Count > 0)
                {
                    // Find the next available door to add cards to.  
                    // This loop cycles through existing doors, skipping the 'deadEndDoor' and any others that are themselves dead ends or already full.  
                    int initialDoorIndex = doorIndex; // To prevent infinite loop if no suitable door is found  
                    bool suitableDoorFound = false;
                    for (int i = 0; i < workingDoors.Count; i++)
                    {
                        var connectedRooms = workingDoors[doorIndex].ConnectedRooms;
                        bool isDeadEnd = connectedRooms != null && connectedRooms.Count > 0 && connectedRooms[0].IsDeadEnd;
                        if (workingDoors[doorIndex] != deadEndDoor && (!isDeadEnd))
                        {
                            suitableDoorFound = true;
                            break;
                        }
                        doorIndex = (doorIndex + 1) % workingDoors.Count; // Cycle through doors  
                        if (doorIndex == initialDoorIndex) // Checked all doors, no suitable found  
                            break;
                    }

                    if (!suitableDoorFound)
                    {
                        // If no suitable door is found among existing ones, create a new "secret" door.  
                        // This new door needs to be instantiated by an external factory and added to the list.  
                        DoorChest newSecretDoor = doorFactory(); // Use the injected factory  
                        workingDoors.Add(newSecretDoor);
                        newSecretDoor.ConnectedRooms = new List<RoomCorridor>(); // Initialize its dungeon  
                        doorIndex = workingDoors.Count - 1; // Point to the newly added door  
                    }

                    // Add a card to the chosen door (or the new secret door)  
                    if (workingDoors[doorIndex].ConnectedRooms == null)
                    {
                        workingDoors[doorIndex].ConnectedRooms = new List<RoomCorridor>();
                    }
                    workingDoors[doorIndex].ConnectedRooms.Insert(0, remainingCards[0]);
                    remainingCards.RemoveAt(0);
                    doorIndex = (doorIndex + 1) % workingDoors.Count; // Move to the next door for fair distribution  
                }
            }

            // After handling dead ends, proceed with distributing remaining dungeon cards  
            if (workingDoors.Count > 1)
            {
                // 2. Distribute cards to multiple doors (near equal amounts)  
                List<RoomCorridor>[] roomSplits = new List<RoomCorridor>[workingDoors.Count];
                for (int i = 0; i < roomSplits.Length; i++)
                {
                    roomSplits[i] = new List<RoomCorridor>();
                }

                int index = 0;
                while (workingDungeonCards.Count > 0)
                {
                    roomSplits[index].Add(workingDungeonCards[0]);
                    workingDungeonCards.RemoveAt(0);
                    index = (index + 1) % roomSplits.Length;
                }

                // 3. Assign room splits to doors  
                for (int i = 0; i < workingDoors.Count; i++)
                {
                    workingDoors[i].ConnectedRooms = roomSplits[i];
                    // Note: No Instantiate calls here. The DoorChest objects are already in the list.  
                }
            }
            else if (workingDoors.Count == 1)
            {
                // 4. Handle single-door case  
                workingDoors[0].ConnectedRooms = workingDungeonCards;
                // Note: No Instantiate calls here.  
            }
            else
            {
                // No doors, or no cards to distribute  
                // This scenario might need specific handling depending on game rules.  
            }

            // Update the actual Doors list of this RoomCorridor instance  
            Doors = workingDoors;
            ConnectedRooms.Clear(); // Clear the main dungeon list as cards are now distributed to doors  
        }
    }

    public class RoomInfo
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? SpecialRules { get; set; }
        public int? ThreatLevelModifier { get; set; }
        public int? PartyMoraleModifier { get; set; }
        public int[]? Size { get; set; }
        public int? DoorCount { get; set; }
        public List<string>? FurnitureList { get; set; }
        public int? EncounterModifier { get; set; }
        public bool HasLevers { get; set; }
        public bool RandomEncounter { get; set; }
        public bool HasSpecial { get; set; }

        public RoomInfo()
        {

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"--- Room: {Name} [{Type}] ---");
            if (Size != null && Size.Length == 2)
            {
                sb.Append($"Size: {Size[0]}x{Size[1]} | ");
            }
            if (DoorCount.HasValue)
            {
                sb.Append($"Doors: {DoorCount} | ");
            }
            sb.AppendLine($"Random Encounter: {RandomEncounter}");

            if (!string.IsNullOrEmpty(Description))
            {
                sb.AppendLine($"Description: {Description}");
            }
            if (FurnitureList != null && FurnitureList.Any())
            {
                sb.AppendLine($"Furniture: {string.Join(", ", FurnitureList)}");
            }
            if (!string.IsNullOrEmpty(SpecialRules))
            {
                sb.AppendLine($"Special Rules: {SpecialRules}");
            }

            var modifiers = new List<string>();
            if (ThreatLevelModifier.HasValue) modifiers.Add($"Threat: {ThreatLevelModifier.Value:+#;-#;0}");
            if (PartyMoraleModifier.HasValue) modifiers.Add($"Morale: {PartyMoraleModifier.Value:+#;-#;0}");
            if (EncounterModifier.HasValue) modifiers.Add($"Encounter: {EncounterModifier.Value:+#;-#;0}");
            if (modifiers.Any())
            {
                sb.AppendLine($"Modifiers: {string.Join(" | ", modifiers)}");
            }

            return sb.ToString();
        }
    }
}