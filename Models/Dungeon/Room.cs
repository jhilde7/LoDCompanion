using LoDCompanion.Models.Character;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Models.Dungeon
{
    public class Room
    {
        public string RoomName { get; set; } = string.Empty; // Default to empty string for safety
        public string ImagePath { get; set; } = string.Empty;
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
        public List<Room> ConnectedRooms { get; set; } = new List<Room>(); // Represents connected dungeon segments
        public int DoorCount { get; set; }
        public List<GridSquare> Grid { get; set; } = new List<GridSquare>();
        public int Width { get; set; }
        public int Height { get; set; }
        public GridPosition GridOffset { get; set; } = new GridPosition(0, 0, 0);
        public List<Hero>? HeroesInRoom { get; set; }
        public List<Monster>? MonstersInRoom { get; set; }
    }
}
