using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Services.Dungeon
{
    public class RoomFactoryService
    {
        private readonly GameDataService _gameData;
        // This dictionary will hold the definitions for various rooms.
        // It will be populated from a data source (e.g., JSON file) at application startup.
        private readonly Dictionary<string, RoomCorridor> _roomDefinitions;

        // Constructor: Inject room definitions.
        // These definitions would typically be loaded by a data loading service at application startup.
        public RoomFactoryService(GameDataService gameData, IEnumerable<RoomCorridor> roomDefinitions)
        {
            _gameData = gameData;

            if (roomDefinitions == null)
            {
                throw new ArgumentNullException(nameof(roomDefinitions), "Room definitions cannot be null.");
            }

            _roomDefinitions = roomDefinitions.ToDictionary(r => r.RoomName, StringComparer.OrdinalIgnoreCase);

            if (!_roomDefinitions.Any())
            {
                Console.WriteLine("Warning: No room definitions loaded into RoomFactoryService.");
                // In a real application, you might throw an exception or log an error more robustly.
            }
        }

        /// <summary>
        /// Creates and initializes a new RoomCorridor instance based on the provided room name.
        /// </summary>
        /// <param name="roomName">The name of the room to create.</param>
        /// <returns>A new RoomCorridor object, or null if the room data is not found.</returns>
        public RoomCorridor? CreateRoom(string roomName)
        {
            // Check if room data exists
            if (_roomDefinitions.TryGetValue(roomName, out RoomCorridor? roomDefinition))
            {
                // Create a new instance of RoomCorridor (no prefabs or Unity Transforms needed)
                RoomCorridor room = new RoomCorridor(_gameData);

                // Initialize room properties using the RoomInfo
                // The InitializeRoomData method should exist on your RoomCorridor class.
                room.InitializeRoomData(roomDefinition);

                // Set the name for identification
                room.RoomName = roomName;

                return room;
            }

            Console.Error.WriteLine($"Error: Room data not found for: {roomName}");
            return null; // Or return a default room instance if appropriate
        }
    }
}