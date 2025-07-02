

namespace LoDCompanion.Models.Dungeon
{
    public class DoorChest
    {
        public string Type { get; set; } = "Room";
        public bool IsTrapped { get; set; }
        public bool IsLocked { get; set; }
        public bool IsOpen { get; set; } = false; // Default to closed

        // Properties related to lock, if locked
        public int LockModifier { get; set; }
        public int LockHP { get; set; }

        // List of rooms that this door/chest could lead to (for doors)
        // In a web project, this would represent the connections in your dungeon graph.
        public List<RoomCorridor> ConnectedRooms { get; set; } = new List<RoomCorridor>();

        // Constructor
        public DoorChest(string type)
        {
            Type = type;
        }

        // This method would be called by a service (e.g., DungeonManagerService or a specific DoorChestService)
        // which performs the rolls and sets these properties.
        public void SetLockAndTrapState(bool isLocked, int lockModifier, int lockHP, bool isTrapped)
        {
            IsLocked = isLocked;
            LockModifier = lockModifier;
            LockHP = lockHP;
            IsTrapped = isTrapped;
        }

        // Provides the next connected room (for doors). The actual logic for selecting
        // the room and updating dungeon state will be in a service.
        public RoomCorridor? GetNextConnectedRoom()
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
            IsOpen = !IsOpen;
        }
    }
}