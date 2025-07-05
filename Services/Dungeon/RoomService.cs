using LoDCompanion.Utilities;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;
using System.Text;
using LoDCompanion.Models.Dungeon;
using System.Collections.Generic;

namespace LoDCompanion.Services.Dungeon
{
    // Represents a single room or corridor tile in the dungeon.
    public class RoomService
    {
        private readonly GameDataService _gameData;
        // Public properties to hold the room's data and state.
        // These will be populated by a RoomFactoryService or DungeonManagerService.
        public string RoomName { get; set; } = string.Empty; // Default to empty string for safety
        public bool IsStartingTile { get; set; }
        public bool IsObjectiveRoom { get; set; }
        public bool HasLevers { get; set; } // Flag, actual lever logic in a service
        public RoomCategory Category { get; set; } = RoomCategory.Room; // Default type, can be "Room" or "Corridor"
        public string Description { get; set; } = string.Empty;
        public string SpecialRules { get; set; } = string.Empty;
        public bool HasSpecial { get; set; }
        public bool ActivateSpecial { get; set; } // Trigger for special room effects, handled by a service
        public int ThreatLevelModifier { get; set; }
        public int PartyMoraleModifier { get; set; }
        public int[] Size { get; set; } = new int[2]; // Represents width/length or dimensions
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
        public Trap CurrentTrap { get; set; } = new Trap("No Trap", 0, 0, string.Empty); // Default trap, no trap present initially
        public List<string> SearchResults { get; set; } = new List<string>();
        public List<RoomService> ConnectedRooms { get; set; } = new List<RoomService>(); // Represents connected dungeon segments
        public int DoorCount { get; set; }
        public List<GridSquare> Grid { get; set; } = new List<GridSquare>();
        public int Width { get; set; }
        public int Height { get; set; }
        public GridPosition GridOffset { get; set; } = new GridPosition(0, 0);

        // Constructor for creating a RoomCorridor instance
        public RoomService(GameDataService gameData)
        {
            _gameData = gameData;
        }

        /// <summary>
        /// Initializes the room's data based on a RoomInfo definition.
        /// This method would be called by a RoomFactoryService.
        /// </summary>
        /// <param name="roomInfo">The data object containing room definitions.</param>
        public void InitializeRoomData(RoomInfo roomInfo)
        {
            // Basic Information
            RoomName = roomInfo.Name ?? string.Empty;
            Category = roomInfo.Category;
            Description = roomInfo.Description ?? string.Empty;
            SpecialRules = roomInfo.SpecialRules ?? string.Empty;
            HasSpecial = roomInfo.HasSpecial;

            // Stats
            ThreatLevelModifier = roomInfo.ThreatLevelModifier;
            PartyMoraleModifier = roomInfo.PartyMoraleModifier;
            Size = roomInfo.Size ?? new int[] { 1, 1 };

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

            if (Category == RoomCategory.Corridor)
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

        public List<DoorChest> HandlePartialDeadends(List<DoorChest> workingDoors)
        {
            while (workingDoors.Any(d => d.ConnectedRooms != null && d.ConnectedRooms.Count > 0 && d.ConnectedRooms[0].IsDeadEnd))
            {
                DoorChest deadEndDoor = workingDoors.First(d => d.ConnectedRooms != null && d.ConnectedRooms.Count > 0 && d.ConnectedRooms[0].IsDeadEnd);
                List<RoomService> remainingCards = new List<RoomService>(deadEndDoor.ConnectedRooms);

                deadEndDoor.ConnectedRooms.Clear();

                // Distribute remaining cards to other doors
                if (workingDoors.Count > 1)
                {
                    List<DoorChest> otherDoors = workingDoors.Where(d => d != deadEndDoor).ToList();
                    if (otherDoors.Any())
                    {
                        int doorIndex = 0;
                        while (remainingCards.Count > 0)
                        {
                            if (otherDoors[doorIndex].ConnectedRooms == null)
                            {
                                otherDoors[doorIndex].ConnectedRooms = new List<RoomService>();
                            }
                            // Add to the bottom of the pile as per the rule
                            otherDoors[doorIndex].ConnectedRooms.Add(remainingCards[0]);
                            remainingCards.RemoveAt(0);
                            doorIndex = (doorIndex + 1) % otherDoors.Count;
                        }
                    }
                }
            }

            return workingDoors;
        }

        /// <summary>
        /// This method encapsulates the original SplitDungeonBetweenDoors logic.
        /// It modifies the current room's `Doors` and `ConnectedRooms` properties.
        /// The actual creation of new `DoorChest` instances (which were `Instantiate` calls)
        /// will be handled by the calling service (e.g., `DungeonManagerService` or `RoomFactoryService`).
        /// </summary>
        public void SplitDungeonBetweenDoors(List<RoomService> availableDungeonCards, Func<DoorChest> doorFactory, Func<string, RoomService> roomFactory)
        {
            if (IsObjectiveRoom || availableDungeonCards.Count == 0)
            {
                return;
            }

            // Copy to mutable lists for manipulation  
            List<RoomService> workingDungeonCards = new List<RoomService>(availableDungeonCards);
            List<DoorChest> workingDoors = new List<DoorChest>(Doors); // Assuming 'Doors' is already populated  

            // 1. Handle dead ends (repeatedly if necessary) - Logic to create secret doors and redistribute cards
            workingDoors = HandlePartialDeadends(workingDoors);
            if (workingDoors.All(d => d.ConnectedRooms == null || d.ConnectedRooms.Count == 0 || d.ConnectedRooms[0].IsDeadEnd))
            {
                if (workingDungeonCards.Any())
                {
                    // Create a new secret door if all paths are dead ends
                    DoorChest newSecretDoor = doorFactory();
                    newSecretDoor.ConnectedRooms = new List<RoomService>();
                    workingDoors.Add(newSecretDoor);

                    // Assign all remaining cards to this new secret door
                    newSecretDoor.ConnectedRooms.AddRange(workingDungeonCards);
                    workingDungeonCards.Clear();
                }
            }

            // After handling dead ends, proceed with distributing remaining dungeon cards  
            if (workingDoors.Count > 1)
            {
                // 2. Distribute cards to multiple doors (near equal amounts)  
                List<RoomService>[] roomSplits = new List<RoomService>[workingDoors.Count];
                for (int i = 0; i < roomSplits.Length; i++)
                {
                    roomSplits[i] = new List<RoomService>();
                }

                int index = 0;
                while (workingDungeonCards.Count > 0)
                {
                    // Get the card from the BOTTOM of the deck (the last element in the list)
                    int lastCardIndex = workingDungeonCards.Count - 1;
                    RoomService cardToDeal = workingDungeonCards[lastCardIndex];

                    // Add the card to the current door's split
                    roomSplits[index].Add(cardToDeal);

                    // Remove the card from the working deck
                    workingDungeonCards.RemoveAt(lastCardIndex);

                    // Move to the next door for the next card
                    index = (index + 1) % roomSplits.Length;
                }

                // 3. Assign room splits to doors  
                for (int i = 0; i < workingDoors.Count; i++)
                {
                    workingDoors[i].ConnectedRooms = roomSplits[i];
                }
            }
            else if (workingDoors.Count == 1)
            {
                // 4. Handle single-door case  
                workingDoors[0].ConnectedRooms = workingDungeonCards;
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
}