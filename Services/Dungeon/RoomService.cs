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
        public GridPosition GridOffset { get; set; } = new GridPosition(0, 0, 0);

        public List<RoomInfo> Rooms => GetRooms();
        public List<Furniture> Furniture => GetFurniture();

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

        public List<RoomInfo> GetRooms()
        {
            return new List<RoomInfo> {
                new RoomInfo(){
                    Name = "Start Tile",
                    Category = RoomCategory.Corridor,
                    Description = "This is the standard tile when entering a dungeon.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C1",
                    Category = RoomCategory.Corridor,
                    Description = "The party enters a long corridor with a door at the other end.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C2",
                    Category = RoomCategory.Corridor,
                    Description = "In the middle of the corridor lies an old backpack. Maybe there is something useful inside?",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Backpack", new List<GridPosition>() { new GridPosition(3, 0, 0) }),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C3",
                    Category = RoomCategory.Corridor,
                    Description = "The walls in this corridor are engraved with large skulls. They give an eerie feeling of vigilance, as if they are watching every step the adventurers take.",
                    ThreatLevelModifier = 1,
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C4",
                    Category = RoomCategory.Corridor,
                    Description = "Just another long-stretched corridor. Better hurry on forward!",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C5",
                    Category = RoomCategory.Corridor,
                    Description = "The floor in this corridor is made of metal grates. By the look of it, the metal has seen better days and it's very rusty. The first step makes the metal creak and the tormented sound echoes through the corridor. Moving through this corridor is bound to attract some attention.",
                    SpecialRules = "See card for special rules.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C6",
                    Category = RoomCategory.Corridor,
                    Description = "This dank corridor seems to end with two doorways, each lit by a torch on the wall.",
                    Size = [ 2, 6 ],
                    DoorCount = 2,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C7",
                    Category = RoomCategory.Corridor,
                    Description = "The blood spilt in this corridor has dried, but appears to only be one or two days old at most.",
                    ThreatLevelModifier = 1,
                    Size = [ 2, 6 ],
                    DoorCount = 2,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C8",
                    Category = RoomCategory.Corridor,
                    Description = "A long corridor with three more doors to explore. It's completely empty apart from cobweb's covering the floor.",
                    Size = [ 2, 6 ],
                    DoorCount = 3,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C9",
                    Category = RoomCategory.Corridor,
                    Description = "The party suddenly halts as they discover that the floor has given way in the middle of this corridor. Where the floor should be, there is now a large gaping hole with unknown depth.",
                    SpecialRules = "See card for special rules.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>(){ new GridPosition(2, 0, 0),
                            new GridPosition(2, 1, 0), new GridPosition(3, 0, 0), new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C10",
                    Category = RoomCategory.Corridor,
                    Description = "The corridor splits in two at a crossway, leaving three possible ways to go. There is nothing special to observe here.",
                    Size = [ 6, 6 ],
                    DoorCount = 3,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C11",
                    Category = RoomCategory.Corridor,
                    Description = "The corridor splits in two.From the left there is a foul smell, and there seems to be far away noises coming from the right.",
                    Size = [ 4, 6 ],
                    DoorCount = 2,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C12",
                    Category = RoomCategory.Corridor,
                    Description = "The corridor makes a sharp turn. In the corner, up against the wall sits a dead adventurer. By the look of it, they must have been there for a while since most of the flesh has been eaten away.",
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "C13",
                    Category = RoomCategory.Corridor,
                    Description = "A short distance ahead is a bend in the corridor. Everything seems quiet, apart from the sound of water droplets hitting the floor.",
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "C14",
                    Category = RoomCategory.Corridor,
                    Description = "The corridor makes a sharp turn. As the party enters the corridor there is a sudden sound of something dragging across the floor and shadows dancing on the wall before everything goes silent.",
                    ThreatLevelModifier = 1,
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "C15",
                    Category = RoomCategory.Corridor,
                    Description = "The corridor in front of you has collapsed and there is no way you can get through without moving the debris blocking the path.",
                    SpecialRules = "See card for special rules.",
                    Size = [ 2, 4 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>(){ new GridPosition(2, 0, 0),
                            new GridPosition(2, 1, 0), new GridPosition(3, 0, 0), new GridPosition(3, 1, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "C16",
                    Category = RoomCategory.Corridor,
                    Description = "The party comes across some stairs that lead upwards into darkness. The steps are slippery and worn. This must be a widely used passage.",
                    SpecialRules = "See card for special rules.",
                    Size = [ 2, 4 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Stairs",
                            new List<GridPosition>() { new GridPosition(0, 0, 2), new GridPosition(0, 1, 2)}),
                        GetFurnitureByNameSetPosition("Stairs",
                            new List<GridPosition>() { new GridPosition(1, 0, 1), new GridPosition(1, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C17",
                    Category = RoomCategory.Corridor,
                    Description = "There are four different openings in this corridor, each covered by a heavy wooden door.",
                    Size = [ 2, 6 ],
                    DoorCount = 3,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C18",
                    Category = RoomCategory.Corridor,
                    Description = "There are two different openings in this corridor, each covered by a heavy wooden door.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C20",
                    Category = RoomCategory.Corridor,
                    Description = "A long corridor with nothing but sand and stone. A closed door can be seen in the darkness ahead.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C21",
                    Category = RoomCategory.Corridor,
                    Description = "An old wooden bridge spans the canal passing through this corridor.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Bridge", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Bridge", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Bridge", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Bridge", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C22",
                    Category = RoomCategory.Corridor,
                    Description = "Two more doors can be seen further down this corridor. Even though they appear closed, there is a draft causing the sand on the floor to shift.",
                    Size = [ 2, 6 ],
                    DoorCount = 2,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C23",
                    Category = RoomCategory.Corridor,
                    Description = "There is a dead adventurer in exotic clothing lying on the floor in a pool of blood. Their demise appears to have happened in the past few hours.",
                    ThreatLevelModifier = 2,
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(2, 0, 0), new GridPosition(2, 1, 0) })
                        ]
                  },
                  new RoomInfo(){
                    Name = "C24",
                    Category = RoomCategory.Corridor,
                    Description = "The wall on one side of this corridor has collapsed, making the passageway even narrower",
                    SpecialRules = "The 2 center squares with stones cannot be entered.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C25",
                    Category = RoomCategory.Corridor,
                    Description = "In the darkness ahead, you see that this corridor makes a sharp turn.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(0, 0, 0) }),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "C26",
                    Category = RoomCategory.Corridor,
                    Description = "In the center of this corridor is a huge plant, with brown green vines. The vines seem to sway slowly even though there is no wind.",
                    SpecialRules = "See card for special rules.",
                    Size = [ 4, 6 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    RandomEncounter = false,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C27",
                    Category = RoomCategory.Corridor,
                    Description = "The party enters an empty crossway with a foul smell. It's hard to tell from which passageway the smell is coming from.",
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C28",
                    Category = RoomCategory.Corridor,
                    Description = "A pole with a large number of blades is spinning around it's own axle as it moves back and forth across the hallway. Judging by the remains around the trap, this will require skill and timing to bypass.",
                    Size = [ 2, 6 ],
                    DoorCount = 1,
                    RandomEncounter = false,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Trap",
                            new List<GridPosition>() { new GridPosition(2, 0, 0), new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "C29",
                    Category = RoomCategory.Corridor,
                    Description = "A large swarm of bats is disturbed as the adventurers enter this corridor. The air is filled with flapping black wings.",
                    SpecialRules = "See card for special rules.",
                    Size = [ 4, 6 ],
                    DoorCount = 2,
                    EncounterModifier = 100,
                    EncounterType = "C29",
                    HasSpecial = true,
                    RandomEncounter = false,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R1",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    Size = [ 5, 5 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Bridge", new List<GridPosition>() {
                            new GridPosition(0, 2, 0), new GridPosition(1, 2, 0), new GridPosition(2, 2, 0),
                            new GridPosition(3, 2, 0), new GridPosition(4, 2, 0) }),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() {
                            new GridPosition(0, 0, 0), new GridPosition(1, 0, 0), new GridPosition(2, 0, 0),
                            new GridPosition(3, 0, 0), new GridPosition(4, 0, 0) }),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() {
                            new GridPosition(0, 1, 0), new GridPosition(1, 1, 0), new GridPosition(2, 1, 0),
                            new GridPosition(3, 1, 0), new GridPosition(4, 1, 0) }),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() {
                            new GridPosition(0, 3, 0), new GridPosition(1, 3, 0), new GridPosition(2, 3, 0),
                            new GridPosition(3, 3, 0), new GridPosition(4, 3, 0) }),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() {
                            new GridPosition(0, 4, 0), new GridPosition(1, 4, 0), new GridPosition(2, 4, 0),
                            new GridPosition(3, 4, 0), new GridPosition(4, 4, 0) })
                        ]
                  },
                  new RoomInfo(){
                    Name = "R2",
                    Category = RoomCategory.Room,
                    ThreatLevelModifier = 1,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    EncounterModifier = 15,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Bed", new List<GridPosition>() {
                            new GridPosition(0, 0, 0), new GridPosition(0, 1, 0), new GridPosition(1, 0, 0), new GridPosition(1, 1, 0),
                            new GridPosition(0, 0, 1), new GridPosition(0, 1, 1), new GridPosition(1, 0, 1), new GridPosition(1, 1, 1)}),
                        GetFurnitureByNameSetPosition("Bed", new List<GridPosition>() {
                            new GridPosition(0, 4, 0), new GridPosition(1, 4, 0), new GridPosition(0, 5, 0), new GridPosition(1, 5, 0),
                            new GridPosition(0, 4, 1), new GridPosition(1, 4, 1), new GridPosition(0, 5, 1), new GridPosition(1, 5, 1) }),
                        GetFurnitureByNameSetPosition("Bed", new List<GridPosition>() {
                            new GridPosition(4, 4, 0), new GridPosition(4, 5, 0), new GridPosition(5, 4, 0), new GridPosition(5, 5, 0),
                            new GridPosition(4, 4, 1), new GridPosition(4, 5, 1), new GridPosition(5, 4, 1), new GridPosition(5, 5, 1) }),
                        GetFurnitureByNameSetPosition("Weapon Rack", new List<GridPosition>() {
                            new GridPosition(4, 0, 0), new GridPosition(5, 0, 0), new GridPosition(4, 0, 1), new GridPosition(5, 0, 1) }),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R3",
                    Category = RoomCategory.Room,
                    PartyMoraleModifier = -4,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Torture Tools", new List<GridPosition>() {
                            new GridPosition(2, 2, 0),  new GridPosition(3, 2, 0), new GridPosition(2, 2, 1),  new GridPosition(3, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Torture Tools", new List<GridPosition>() {
                            new GridPosition(0, 4, 0), new GridPosition(1, 4, 0), new GridPosition(1, 5, 0),
                            new GridPosition(0, 4, 1), new GridPosition(1, 4, 1), new GridPosition(1, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R4",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() {
                            new GridPosition(1, 2, 0), new GridPosition(2, 2, 0), new GridPosition(1, 2, 1), new GridPosition(2, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Barrels",
                            new List<GridPosition>() { new GridPosition(1, 4, 0), new GridPosition(1, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Barrels",
                            new List<GridPosition>() { new GridPosition(0, 5, 0), new GridPosition(0, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() {
                            new GridPosition(4, 5, 0), new GridPosition(5, 5, 0), new GridPosition(4, 5, 1), new GridPosition(5, 5, 1)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R5",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Water Basin",
                            new List<GridPosition>() { new GridPosition(2, 0, 0), new GridPosition(2, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(9, 0, 0), new GridPosition(9, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(1, 1, 0), new GridPosition(1, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(4, 1, 0), new GridPosition(4, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(7, 1, 0), new GridPosition(7, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(10, 1, 0), new GridPosition(10, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Armour Rack",
                            new List<GridPosition>() { new GridPosition(11, 2, 0), new GridPosition(11, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Armour Rack",
                            new List<GridPosition>() { new GridPosition(11, 3, 0), new GridPosition(11, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(1, 4, 0), new GridPosition(1, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(4, 4, 0), new GridPosition(4, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(7, 4, 0), new GridPosition(7, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(10, 4, 0), new GridPosition(10, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(9, 5, 0), new GridPosition(9, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "R6",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(1, 0, 0), new GridPosition(1, 0, 1)}),
                        GetFurnitureByNameSetPosition("Altar", new List<GridPosition>() {
                            new GridPosition(2, 0, 0), new GridPosition(3, 0, 0), new GridPosition(2, 0, 1), new GridPosition(3, 0, 1)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(4, 0, 0), new GridPosition(4, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        new Furniture(){ Name = "Statue", IsSearchable = true, NoEntry = true, OccupiedSquares = new List<GridPosition>() {
                            new GridPosition(2, 5, 0), new GridPosition(3, 5, 0), new GridPosition(2, 5, 1), new GridPosition(3, 5, 1)} },
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R7",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Bookshelf", new List<GridPosition>() {
                            new GridPosition(5, 0, 0), new GridPosition(5, 0, 1), new GridPosition(5, 0, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Study Table", new List<GridPosition>() { 
                            new GridPosition(2, 2, 0), new GridPosition(3, 2, 0), new GridPosition(2, 3, 0), new GridPosition(3, 3, 0),
                            new GridPosition(2, 2, 1), new GridPosition(3, 2, 1), new GridPosition(2, 3, 1), new GridPosition(3, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Bookshelf", new List<GridPosition>() { 
                            new GridPosition(0, 5, 0), new GridPosition(1, 5, 0), new GridPosition(0, 5, 1), new GridPosition(1, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R8",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", 
                            new List<GridPosition>() { new GridPosition(1, 0, 0), new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Grate (over a hole)", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R9",
                    Category = RoomCategory.Room,
                    ThreatLevelModifier = -2,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Fountain", new List<GridPosition>() { 
                            new GridPosition(2, 3, 0), new GridPosition(3, 3, 0), new GridPosition(2, 4, 0), new GridPosition(3, 4, 0),
                            new GridPosition(2, 3, 1), new GridPosition(3, 3, 1), new GridPosition(2, 4, 1), new GridPosition(3, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R10",
                    Category = RoomCategory.Room,
                    Description = "Finally, a room worthy of your presence. Even though the stones are as dark and dank as the rest of the dungeon, there is a silver lining here. Alongside one wall there are three chests to be plundered.",
                    Size = [ 6, 6 ],
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Objective Chest",
                            new List<GridPosition>() { new GridPosition(2, 5, 0), new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R11",
                    Category = RoomCategory.Room,
                    SpecialRules = "Levers can be pulled, check card of instructions.",
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    HasLevers = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Levers", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R12",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Study Table", 
                            new List<GridPosition>() { new GridPosition(1, 0, 0), new GridPosition(1, 0, 1)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Bookshelf", new List<GridPosition>() { 
                            new GridPosition(1, 3, 0), new GridPosition(2, 3, 0), new GridPosition(1, 3, 1), new GridPosition(2, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R13",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() { 
                            new GridPosition(2, 0, 0), new GridPosition(3, 0, 0), new GridPosition(2, 0, 1), new GridPosition(3, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() { 
                            new GridPosition(0, 3, 0), new GridPosition(1, 3, 0), new GridPosition(0, 3, 1), new GridPosition(1, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R14",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Well",
                            new List<GridPosition>() { new GridPosition(1, 1, 0), new GridPosition(2, 1, 0), new GridPosition(1, 3, 0),
                            new GridPosition(1, 1, 1), new GridPosition(2, 1, 1), new GridPosition(1, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R15",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Drawer", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Bed", new List<GridPosition>() { 
                            new GridPosition(3, 0, 0), new GridPosition(3, 1, 0), new GridPosition(3, 0, 1), new GridPosition(3, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R16",
                    Category = RoomCategory.Room,
                    Size = [ 6, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Bed", new List<GridPosition>() { 
                            new GridPosition(0, 2, 0), new GridPosition(0, 3, 0), new GridPosition(0, 2, 1), new GridPosition(0, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Table", new List<GridPosition>() { 
                            new GridPosition(3, 3, 0), new GridPosition(4, 3, 0), new GridPosition(3, 3, 1), new GridPosition(4, 3, 1)}),
                        GetFurnitureByNameSetPosition("Hearth", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R17",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    EncounterModifier = 10,
                    EncounterType = "R17",
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Treasure Pile", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                      Name = "R18",
                      Category = RoomCategory.Room,
                      Size = [ 12, 6 ],
                      DoorCount = 1,
                      FurnitureList = [
                        GetFurnitureByNameSetPosition("Hearth", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Dining Table", new List<GridPosition>() { 
                            new GridPosition(4, 0, 0), new GridPosition(5, 0, 0), new GridPosition(4, 1, 0), new GridPosition(5, 1, 0),
                            new GridPosition(4, 0, 1), new GridPosition(5, 0, 1), new GridPosition(4, 1, 1), new GridPosition(5, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dining Table", new List<GridPosition>() { 
                            new GridPosition(8, 2, 0), new GridPosition(9, 2, 0), new GridPosition(8, 3, 0), new GridPosition(9, 3, 0),
                            new GridPosition(8, 2, 1), new GridPosition(9, 2, 1), new GridPosition(8, 3, 1), new GridPosition(9, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Dining Table", new List<GridPosition>() { 
                            new GridPosition(2, 3, 0), new GridPosition(3, 3, 0), new GridPosition(2, 4, 0), new GridPosition(3, 4, 0),
                            new GridPosition(2, 3, 1), new GridPosition(3, 3, 1), new GridPosition(2, 4, 1), new GridPosition(3, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Armour Rack", 
                            new List<GridPosition>() { new GridPosition(6, 5, 0), new GridPosition(6, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Armour Rack", 
                            new List<GridPosition>() { new GridPosition(8, 5, 0), new GridPosition(8, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "R19",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    EncounterType = "R19",
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Hearth", new List<GridPosition>() { 
                            new GridPosition(2, 0, 0), new GridPosition(3, 0, 0), new GridPosition(2, 0, 1), new GridPosition(3, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Chair", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Bookshelf", new List<GridPosition>() { 
                            new GridPosition(0, 4, 0), new GridPosition(1, 5, 0), new GridPosition(0, 4, 1), new GridPosition(1, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Cage", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R20",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    EncounterType = "R20",
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Lava", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Lava", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Lava", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Lava", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", 
                            new List<GridPosition>() { new GridPosition(10, 2, 0), new GridPosition(10, 2, 1)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "R21",
                    Category = RoomCategory.Room,
                    SpecialRules = "Walls cannot be passed through and block LOS.",
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Wall", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chair", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Stairs", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Stairs", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        new Furniture(){Name = "Chair", OccupiedSquares =
                            new List<GridPosition>() { new GridPosition(11, 2, 0), new GridPosition(11, 3, 0)}},
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Stairs", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Weapon Rack", new List<GridPosition>() { 
                            new GridPosition(5, 4, 0), new GridPosition(5, 5, 0), new GridPosition(5, 4, 1), new GridPosition(5, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Stairs", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Stairs", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Chair",
                            new List<GridPosition>() { new GridPosition(6, 5, 0), new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "R22",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(0, 0, 0), new GridPosition(0, 0, 1)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(1, 0, 0), new GridPosition(1, 0, 1)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(4, 0, 0), new GridPosition(4, 0, 1)}),
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(5, 0, 0), new GridPosition(5, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() { 
                            new GridPosition(2, 2, 0), new GridPosition(3, 2, 0), new GridPosition(2, 3, 0), new GridPosition(3, 3, 0),
                            new GridPosition(2, 2, 1), new GridPosition(3, 2, 1), new GridPosition(2, 3, 1), new GridPosition(3, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(0, 5, 0), new GridPosition(0, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(5, 5, 0), new GridPosition(5, 5, 1)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R23",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    HasSpecial = true,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(2, 0, 0), new GridPosition(2, 0, 1)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(3, 0, 0), new GridPosition(3, 0, 1)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(0, 1, 0), new GridPosition(0, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(5, 1, 0), new GridPosition(5, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Stairs", 
                            new List<GridPosition>() { new GridPosition(2, 2, 0), new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(0, 5, 0), new GridPosition(0, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R24",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R25",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(5, 1, 0), new GridPosition(5, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Pit", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Treasure Pile", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(5, 4, 0), new GridPosition(5, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R26",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Treasure Pile", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R27",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    RandomEncounter = true,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Bridge",
                            new List<GridPosition>() { new GridPosition(2, 1, 0), new GridPosition(2, 2, 0),
                                new GridPosition(2, 3, 0), new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Chasm", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R28",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    HasSpecial = true,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        new Furniture{Name = "Table", CanBeClimbed = true, OccupiedSquares = new List<GridPosition>() { 
                            new GridPosition(2, 2, 0), new GridPosition(3, 2, 0), new GridPosition(2, 3, 0), new GridPosition(3, 3, 0), 
                            new GridPosition(3, 4, 0), new GridPosition(3, 5, 0), new GridPosition(5, 4, 0), new GridPosition(5, 5, 0), 
                            new GridPosition(2, 2, 1), new GridPosition(3, 2, 1), new GridPosition(2, 3, 1), new GridPosition(3, 3, 1), 
                            new GridPosition(3, 4, 1),  new GridPosition(3, 5, 1), new GridPosition(5, 4, 1), new GridPosition(5, 5, 1)}},
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Barrels", 
                            new List<GridPosition>() { new GridPosition(5, 2, 0), new GridPosition(5, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R29",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        new Furniture(){ Name = "Table", CanBeClimbed = true, OccupiedSquares = new List<GridPosition>() {
                            new GridPosition(1, 1, 0), new GridPosition(2, 1, 0), new GridPosition(2, 2, 0),
                            new GridPosition(1, 1, 1), new GridPosition(2, 1, 1), new GridPosition(2, 2, 1)} },
                        GetFurnitureByNameSetPosition("Weapon Rack",
                            new List<GridPosition>() { new GridPosition(3, 1, 0), new GridPosition(3, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(0, 3, 0), new GridPosition(0, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R30",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    EncounterModifier = 100,
                    EncounterType = "R30",
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(0, 0, 0), new GridPosition(0, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(5, 0, 0), new GridPosition(5, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(0, 5, 0), new GridPosition(0, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Statue", 
                            new List<GridPosition>() { new GridPosition(5, 5, 0), new GridPosition(5, 5, 1)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R31",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(1, 1, 0), new GridPosition(1, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(4, 3, 0), new GridPosition(4, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Piller", 
                            new List<GridPosition>() { new GridPosition(1, 4, 0), new GridPosition(1, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R32",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 0,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", 
                            new List<GridPosition>() { new GridPosition(5, 0, 0), new GridPosition(5, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() { 
                            new GridPosition(5, 2, 0), new GridPosition(5, 3, 0), new GridPosition(5, 2, 1), new GridPosition(5, 3, 1)}),
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R33",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        new Furniture(){ Name = "Statue", IsObstacle = true, NoEntry = true, OccupiedSquares = new List<GridPosition>() { 
                            new GridPosition(1, 0, 0), new GridPosition(1, 1, 0), new GridPosition(1, 0, 1), new GridPosition(1, 1, 1) } },
                        GetFurnitureByNameSetPosition("Alchemist Table", new List<GridPosition>() { 
                            new GridPosition(2, 0, 0), new GridPosition(3, 0, 0), new GridPosition(2, 0, 1), new GridPosition(3, 0, 1)}),
                        new Furniture(){ Name = "Statue", IsObstacle = true, NoEntry = true, OccupiedSquares = new List<GridPosition>() { 
                            new GridPosition(4, 0, 0), new GridPosition(4, 1, 0), new GridPosition(4, 0, 1), new GridPosition(4, 1, 1) } },
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Bookshelf", 
                            new List<GridPosition>() { new GridPosition(5, 5, 0), new GridPosition(5, 5, 1)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R1B",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    ThreatLevelModifier = 2,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus",
                            new List<GridPosition>() { new GridPosition(3, 2, 0), new GridPosition(2, 3, 0), new GridPosition(3, 3, 0),
                                new GridPosition(3, 2, 1), new GridPosition(2, 3, 1), new GridPosition(3, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R2B",
                    Category = RoomCategory.Room,
                    SpecialRules = "Threat level decreased by 2.",
                    ThreatLevelModifier = -2,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Fountain", new List<GridPosition>() { 
                            new GridPosition(2, 2, 0), new GridPosition(3, 2, 0), new GridPosition(2, 3, 0), new GridPosition(3, 3, 0),
                            new GridPosition(2, 2, 1), new GridPosition(3, 2, 1), new GridPosition(2, 3, 1), new GridPosition(3, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R3B",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    HasSpecial = true,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Bed", new List<GridPosition>() {
                            new GridPosition(1, 0, 0), new GridPosition(2, 0, 0), new GridPosition(1, 0, 1), new GridPosition(2, 0, 1)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Bed",
                            new List<GridPosition>() { new GridPosition(5, 1, 0), new GridPosition(5, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer",
                            new List<GridPosition>() { new GridPosition(3, 2, 0), new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() {
                            new GridPosition(0, 4, 0), new GridPosition(5, 4, 0), new GridPosition(0, 4, 1), new GridPosition(5, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R4B",
                    Category = RoomCategory.Room,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Booksehlf", new List<GridPosition>() {
                            new GridPosition(3, 0, 0), new GridPosition(4, 0, 0), new GridPosition(3, 0, 1), new GridPosition(4, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Table", new List<GridPosition>() {
                            new GridPosition(2, 2, 0), new GridPosition(3, 2, 0), new GridPosition(2, 3, 0), new GridPosition(3, 3, 0),
                            new GridPosition(2, 2, 1), new GridPosition(3, 2, 1), new GridPosition(2, 3, 1), new GridPosition(3, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R5B",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() {
                            new GridPosition(1, 0, 0), new GridPosition(2, 0, 0), new GridPosition(1, 0, 1), new GridPosition(2, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Barrels",
                            new List<GridPosition>() { new GridPosition(6, 1, 0), new GridPosition(6, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Barrels",
                            new List<GridPosition>() { new GridPosition(10, 2, 0), new GridPosition(10, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() {
                            new GridPosition(0, 3, 0), new GridPosition(0, 4, 0), new GridPosition(0, 3, 1), new GridPosition(0, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() {
                            new GridPosition(3, 3, 0), new GridPosition(4, 3, 0), new GridPosition(3, 3, 1), new GridPosition(4, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Boxes", new List<GridPosition>() {
                            new GridPosition(9, 4, 0), new GridPosition(8, 5, 0), new GridPosition(9, 5, 0),
                            new GridPosition(9, 4, 1), new GridPosition(8, 5, 1), new GridPosition(9, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Barrels",
                            new List<GridPosition>() { new GridPosition(3, 5, 0), new GridPosition(3, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "R6B",
                    Category = RoomCategory.Room,
                    SpecialRules = "Levers can be pulled, check card of instructions.",
                    HasLevers = true,
                    HasSpecial = true,
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Levers", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R7B",
                    Category = RoomCategory.Room,
                    SpecialRules = "See card for special rules.",
                    Size = [ 6, 6 ],
                    DoorCount = 1,
                    HasSpecial = true,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() {
                            new GridPosition(0, 0, 0), new GridPosition(1, 0, 0), new GridPosition(0, 0, 1), new GridPosition(1, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() {
                            new GridPosition(0, 1, 0), new GridPosition(0, 2, 0), new GridPosition(0, 1, 1), new GridPosition(0, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() {
                            new GridPosition(5, 1, 0), new GridPosition(5, 2, 0), new GridPosition(5, 1, 1), new GridPosition(5, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer",
                            new List<GridPosition>() { new GridPosition(3, 2, 0), new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() {
                            new GridPosition(5, 4, 0), new GridPosition(5, 5, 0), new GridPosition(5, 4, 1), new GridPosition(5, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "R8B",
                    Category = RoomCategory.Room,
                    Size = [ 4, 4 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() {
                            new GridPosition(0, 0, 0), new GridPosition(0, 1, 0), new GridPosition(0, 0, 1), new GridPosition(0, 1, 1) }),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Coffin", new List<GridPosition>() {
                            new GridPosition(3, 0, 0), new GridPosition(3, 1, 0), new GridPosition(3, 0, 1), new GridPosition(3, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Great Crypt",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() {
                            new GridPosition(2, 0, 0), new GridPosition(2, 1, 0), new GridPosition(2, 0, 1), new GridPosition(2, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() {
                            new GridPosition(5, 0, 0), new GridPosition(5, 1, 0), new GridPosition(5, 0, 1), new GridPosition(5, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() {
                            new GridPosition(8, 0, 0), new GridPosition(8, 1, 0), new GridPosition(8, 0, 1), new GridPosition(8, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() {
                            new GridPosition(2, 4, 0), new GridPosition(2, 5, 0), new GridPosition(2, 4, 1), new GridPosition(2, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() {
                            new GridPosition(5, 4, 0), new GridPosition(5, 5, 0), new GridPosition(5, 4, 1), new GridPosition(5, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() {
                            new GridPosition(8, 4, 0), new GridPosition(8, 5, 0), new GridPosition(8, 4, 1), new GridPosition(8, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Fountain Room",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Fountain", new List<GridPosition>() {
                            new GridPosition(0, 2, 0), new GridPosition(1, 2, 0), new GridPosition(2, 2, 0),
                            new GridPosition(0, 3, 0), new GridPosition(1, 3, 0), new GridPosition(2, 3, 0),
                            new GridPosition(0, 2, 1), new GridPosition(1, 2, 1), new GridPosition(2, 2, 1),
                            new GridPosition(0, 3, 1), new GridPosition(1, 3, 1), new GridPosition(2, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Water Basin",
                            new List<GridPosition>() { new GridPosition(9, 5, 0), new GridPosition(9, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Throne Room",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(0, 0, 0), new GridPosition(0, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue", new List<GridPosition>() {
                                new GridPosition(2, 0, 0), new GridPosition(3, 0, 0), new GridPosition(2, 1, 0), new GridPosition(3, 1, 0),
                                new GridPosition(2, 0, 1), new GridPosition(3, 0, 1), new GridPosition(2, 1, 1), new GridPosition(3, 1, 1),
                                new GridPosition(2, 0, 2), new GridPosition(3, 0, 2), new GridPosition(2, 1, 2), new GridPosition(3, 1, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 2)}),
                        GetFurnitureByNameSetPosition("Throne", new List<GridPosition>() {
                            new GridPosition(5, 0, 2), new GridPosition(6, 0, 2), new GridPosition(5, 0, 3), new GridPosition(6, 0, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 2)}),
                        GetFurnitureByNameSetPosition("Stairs", new List<GridPosition>() {
                            new GridPosition(4, 1, 2), new GridPosition(5, 1, 2), new GridPosition(6, 1, 2), new GridPosition(7, 1, 2)}),
                        GetFurnitureByNameSetPosition("Stairs", new List<GridPosition>() {
                            new GridPosition(4, 2, 1), new GridPosition(5, 2, 1), new GridPosition(6, 2, 1), new GridPosition(7, 2, 1)}),
                        GetFurnitureByNameSetPosition("Statue", new List<GridPosition>() {
                                new GridPosition(8, 0, 0), new GridPosition(9, 0, 0), new GridPosition(8, 1, 0), new GridPosition(9, 1, 0),
                                new GridPosition(8, 0, 1), new GridPosition(9, 0, 1), new GridPosition(8, 1, 1), new GridPosition(9, 1, 1),
                                new GridPosition(8, 0, 2), new GridPosition(9, 0, 2), new GridPosition(8, 1, 2), new GridPosition(9, 1, 2)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(11, 0, 0), new GridPosition(11, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(0, 3, 0), new GridPosition(0, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(11, 3, 0), new GridPosition(11, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(0, 5, 0), new GridPosition(0, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(11, 5, 0), new GridPosition(11, 5, 1)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Lava River",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(0, 0, 0), new GridPosition(0, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(2, 0, 0), new GridPosition(2, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Lava", new List<GridPosition>() {
                            new GridPosition(5, 0, 0), new GridPosition(6, 0, 0), new GridPosition(5, 1, 0), new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Pillar", new List<GridPosition>() {
                            new GridPosition(9, 1, 0), new GridPosition(9, 1, 1), new GridPosition(9, 1, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Alchemist Table", new List<GridPosition>() {
                            new GridPosition(0, 2, 1), new GridPosition(0, 3, 1), new GridPosition(0, 2, 2), new GridPosition(0, 3, 2)}),
                        GetFurnitureByNameSetPosition("Stairs",
                            new List<GridPosition>() { new GridPosition(1, 2, 1), new GridPosition(1, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Bridge",
                            new List<GridPosition>() { new GridPosition(5, 2, 0), new GridPosition(6, 2, 0),
                            new GridPosition(5, 3, 0), new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Lava", new List<GridPosition>() {
                            new GridPosition(5, 4, 0), new GridPosition(6, 4, 0), new GridPosition(5, 5, 0), new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Pillar", new List<GridPosition>() {
                            new GridPosition(9, 4, 0), new GridPosition(9, 4, 1), new GridPosition(9, 4, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(0, 5, 0), new GridPosition(0, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(2, 5, 0), new GridPosition(2, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Chamber of Reverence",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(1, 0, 0), new GridPosition(1, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(3, 0, 0), new GridPosition(3, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Alchemist Table", new List<GridPosition>() {
                            new GridPosition(5, 0, 0), new GridPosition(6, 0, 0), new GridPosition(5, 0, 1), new GridPosition(6, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(10, 0, 0), new GridPosition(10, 0, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Statue", new List<GridPosition>() {
                            new GridPosition(0, 2, 0), new GridPosition(1, 2, 0), new GridPosition(2, 2, 0),
                            new GridPosition(0, 3, 0), new GridPosition(1, 3, 0), new GridPosition(2, 3, 0),
                            new GridPosition(0, 2, 1), new GridPosition(1, 2, 1), new GridPosition(2, 2, 1),
                            new GridPosition(0, 3, 1), new GridPosition(1, 3, 1), new GridPosition(2, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Dead Adventurer",
                            new List<GridPosition>() { new GridPosition(5, 3, 0), new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(1, 5, 0), new GridPosition(1, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(3, 5, 0), new GridPosition(3, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Pillar",
                            new List<GridPosition>() { new GridPosition(10, 5, 0), new GridPosition(10, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Lone Tomb",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 2)}),
                        GetFurnitureByNameSetPosition("Pottery", new List<GridPosition>() { new GridPosition(3, 0, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(10, 0, 0), new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(0, 1, 2), new GridPosition(0, 1, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 2)}),
                        GetFurnitureByNameSetPosition("Stairs",
                            new List<GridPosition>() { new GridPosition(4, 1, 1), new GridPosition(4, 2, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 2)}),
                        GetFurnitureByNameSetPosition("Sarcophagus", new List<GridPosition>() {
                            new GridPosition(1, 2, 2), new GridPosition(2, 2, 2), new GridPosition(1, 3, 2), new GridPosition(2, 3, 2),
                            new GridPosition(1, 2, 3), new GridPosition(2, 2, 3), new GridPosition(1, 3, 3), new GridPosition(2, 3, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(0, 4, 0), new GridPosition(0, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(10, 5, 0), new GridPosition(10, 5, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Large Tomb",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Debris", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(0, 1, 0), new GridPosition(0, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(11, 1, 0), new GridPosition(11, 1, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Sarcophagus",
                            new List<GridPosition>() { new GridPosition(5, 2, 0), new GridPosition(5, 3, 0),
                            new GridPosition(5, 2, 1), new GridPosition(5, 3, 1)}),
                        GetFurnitureByNameSetPosition("Sarcophagus",
                            new List<GridPosition>() { new GridPosition(6, 2, 0), new GridPosition(6, 3, 0),
                            new GridPosition(6, 2, 1), new GridPosition(6, 3, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 3, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(0, 4, 1), new GridPosition(0, 4, 1)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 0)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(11, 4, 1), new GridPosition(11, 4, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  },
                  new RoomInfo(){
                    Name = "The Ancient Throne Room",
                    Category = RoomCategory.Room,
                    Size = [ 12, 6 ],
                    DoorCount = 1,
                    FurnitureList = [
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 0, 0)}),
                        GetFurnitureByNameSetPosition("Treasure Pile", new List<GridPosition>() { new GridPosition(6, 0, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(7, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 0, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 1, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 1, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(8, 1, 3), new GridPosition(8, 1, 4)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 1, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 1, 3)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(11, 1, 3), new GridPosition(11, 1, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 2, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 2, 0)}),
                        GetFurnitureByNameSetPosition("Stairs",
                            new List<GridPosition>() { new GridPosition(6, 2, 1), new GridPosition(6, 3, 1)}),
                        GetFurnitureByNameSetPosition("Stairs",
                            new List<GridPosition>() { new GridPosition(7, 2, 2), new GridPosition(7, 3, 2)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 2, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 2, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 2, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 3, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 3, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 3, 3)}),
                        GetFurnitureByNameSetPosition("Throne",
                            new List<GridPosition>() { new GridPosition(11, 2, 3), new GridPosition(11, 3, 3),
                            new GridPosition(11, 2, 4), new GridPosition(11, 3, 4)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 3, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 4, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(7, 4, 0)}),
                        GetFurnitureByNameSetPosition("Brazier",
                            new List<GridPosition>() { new GridPosition(8, 4, 3), new GridPosition(8, 4, 4)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 4, 3)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 4, 3)}),
                        GetFurnitureByNameSetPosition("Statue",
                            new List<GridPosition>() { new GridPosition(11, 4, 3), new GridPosition(11, 4, 4)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(0, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(1, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(2, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(3, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(4, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(5, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(6, 5, 0)}),
                        GetFurnitureByNameSetPosition("Chest", new List<GridPosition>() { new GridPosition(7, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(8, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(9, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(10, 5, 0)}),
                        GetFurnitureByNameSetPosition("Floor", new List<GridPosition>() { new GridPosition(11, 5, 0)})
                        ]
                  }
            };
        }

        public RoomInfo GetRoomByName(string name)
        {
            return Rooms.First(r => r.Name == name);
        }

        public List<Furniture> GetFurniture()
        {
            return new List<Furniture>()
        {
            new Furniture()
            {
                Name = "Altar",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Archery Target",
                IsObstacle = true,
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Armour Rack",
                IsObstacle = true,
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Backpack",
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Barrels",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Bed",
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Bedroll",
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Bookshelf",
                IsObstacle = true,
                IsSearchable = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Boxes",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Brazier",
                IsObstacle = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Bridge"
            },
            new Furniture()
            {
                Name = "Chasm",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Chair"
            },
            new Furniture()
            {
                Name = "Cage",
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Chest",
                IsSearchable= true
            },
            new Furniture()
            {
                Name = "Objective Chest",
                IsSearchable= true
            },
            new Furniture()
            {
                Name = "Coffin",
                IsSearchable= true
            },
            new Furniture()
            {
                Name = "Dead Adventurer",
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Debris",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Drawer",
                IsObstacle = true,
                IsSearchable = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Floor"
            },
            new Furniture()
            {
                Name = "Fountain",
                IsObstacle = true,
                IsSearchable = true,
                IsDrinkable = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Grate (over a hole)",
                IsSearchable= true
            },
            new Furniture()
            {
                Name = "Hearth",
                IsObstacle = true,
                IsSearchable = true,
                NoEntry= true
            },
            new Furniture()
            {
                Name = "Lava",
                SpecialRules = "Instant death for any character"
            },
            new Furniture()
            {
                Name = "Levers",
                IsLevers = true
            },
            new Furniture()
            {
                Name = "Pillar",
                IsObstacle= true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Pit",
                SpecialRules = "See exploration card for rules"
            },
            new Furniture()
            {
                Name = "Pottery",
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Rubble",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Sarcophagus",
                IsSearchable= true,
                IsObstacle = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Stairs"
            },
            new Furniture()
            {
                Name = "Statue",
                IsObstacle = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Alchemist Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Dining Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Study Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Throne",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true
            },
            new Furniture()
            {
                Name = "Torture Tools",
                NoEntry = true,
                IsObstacle = true
            },
            new Furniture()
            {
                Name = "Trap"
            },
            new Furniture()
            {
                Name = "Treasure Pile",
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Water Basin",
                IsObstacle = true,
                IsDrinkable = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Water",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Weapon Rack",
                IsSearchable = true
            },
            new Furniture()
            {
                Name = "Wall",
                NoEntry = true,
                BlocksLoS = true,
            },
            new Furniture()
            {
                Name = "Well",
                IsSearchable = true,
                IsObstacle = true,
                NoEntry = true
            }
        };
        }

        public Furniture GetFurnitureByName(string name)
        {
            return Furniture.First(x => x.Name == name);
        }

        private Furniture GetFurnitureByNameSetPosition(string name, List<GridPosition> gridPosition)
        {
            Furniture furniture = GetFurnitureByName(name);
            furniture.OccupiedSquares = gridPosition;
            return furniture;
        }
    }
}