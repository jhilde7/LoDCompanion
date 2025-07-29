

using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Models.Dungeon
{
    public enum Orientation
    {
        North,
        South, 
        East, 
        West
    }

    public enum DoorChestProperty
    {
        None,
        Locked,
        Trapped,
        Open,
        LockModifier,
        LockHP,
        MagicallySealed,
    }

    public class DoorChest
    {
        public string Category { get; set; } = string.Empty;
        public Orientation Orientation { get; set; }
        public Dictionary<DoorChestProperty, int>? Properties { get; set; }

        // List of rooms that this door/chest could lead to (for doors)
        // In a web project, this would represent the connections in your dungeon graph.
        public GridPosition[] Position { get; set; } = {new GridPosition(0, 0, 0), new GridPosition(1, 0, 0)};
        public List<Room> ConnectedRooms { get; set; } = new List<Room>();

        // Constructor
        public DoorChest(string type)
        {
            Category = type;
        }

        // This method would be called by a service (e.g., DungeonManagerService or a specific DoorChestService)
        // which performs the rolls and sets these properties.
        public void SetLockAndTrapState(bool isLocked, int lockModifier, int lockHP, bool isTrapped)
        {
            Properties ??= new Dictionary<DoorChestProperty, int>();
            if (isLocked) 
            {
                Properties.TryAdd(DoorChestProperty.Locked, 0);
                Properties.TryAdd(DoorChestProperty.LockModifier, lockModifier);
                Properties.TryAdd(DoorChestProperty.LockHP, lockHP);
            }
            if (isTrapped) Properties.TryAdd(DoorChestProperty.Trapped, 0);
        }

        // Provides the next connected room (for doors). The actual logic for selecting
        // the room and updating dungeon state will be in a service.
        public Room? GetNextConnectedRoom()
        {
            if (ConnectedRooms != null && ConnectedRooms.Count > 0)
            {
                // In a real application, you might have logic to pick a specific room,
                // e.g., based on player choice, pre-determined path, or specific game rules.
                // For now, simply return the first connected room as an example.
                return ConnectedRooms[0];
            }
            return null;
        }

        // A simple method to toggle the open state. The logic to determine if it *can* be opened
        // (e.g., if unlocked) would reside in a service.
        public void ToggleOpen()
        {
            Properties ??= new Dictionary<DoorChestProperty, int>();
            if (Properties.ContainsKey(DoorChestProperty.Open))
            {
                Properties.Remove(DoorChestProperty.Open);
            }
            else
            {
                Properties.TryAdd(DoorChestProperty.Open, 0); // Default to open state
            }
        }
    }
}