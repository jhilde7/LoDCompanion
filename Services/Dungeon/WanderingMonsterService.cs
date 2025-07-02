namespace LoDCompanion.Services.Dungeon
{
    public class WanderingMonsterService
    {
        private readonly EncounterService _encounterService;

        // Constructor for dependency injection
        public WanderingMonsterService(EncounterService encounterService)
        {
            _encounterService = encounterService;
        }

        /// <summary>
        /// Triggers a wandering monster encounter in the specified room.
        /// </summary>
        /// <param name="currentRoom">The current room where the wandering monster should appear.</param>
        public void TriggerWanderingMonster(RoomService currentRoom)
        {
            if (currentRoom == null)
            {
                // Log or handle error: Cannot trigger wandering monster in a null room.
                return;
            }
            /*
            // The original logic checked for roomName "R28" to use roomEncounter from RoomCorridor.
            // In the refactored service, we delegate encounter creation to EncounterService.
            // The EncounterService should be smart enough to know if an R28-specific encounter is needed.
            // Or, we can pass a hint to it.

            // Example: Create a new encounter for the room
            // The EncounterService would determine the type of encounter (R28 specific or generic)
            // based on the currentRoom details.
            var newEncounter = _encounterService.GenerateRandomEncounter(currentRoom);

            // Add the new encounter to the dungeon's active encounters or the room itself
            // The DungeonManagerService is responsible for managing active encounters.
            if (newEncounter != null)
            {
                _dungeonManagerService.AddActiveEncounter(newEncounter);
                // Optionally, update the room's state to reflect the new encounter
                currentRoom.RoomEncounter = newEncounter;
                // Log the event, e.g., "A wandering monster appeared in [RoomName]!"
            }
            */
        }

        // Additional methods related to wandering monster logic can be added here,
        // e.g., conditions for when a wandering monster *might* appear (handled by DungeonManagerService),
        // or specific types of wandering monsters.
    }
}