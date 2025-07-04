using LoDCompanion.Services.GameData;
using LoDCompanion.Models.Dungeon;

namespace LoDCompanion.Services.Dungeon
{
    public class RoomFactoryService
    {
        private readonly GameDataService _gameData;
        private readonly GridService _gridService;
        private readonly Dictionary<string, RoomInfo> _roomDefinitions;

        public RoomFactoryService(GameDataService gameData, GridService gridService,
            IEnumerable<RoomInfo> roomDefinitions)
        {
            _gameData = gameData;
            _gridService = gridService;

            if (roomDefinitions == null)
            {
                throw new ArgumentNullException(nameof(roomDefinitions), "Room definitions cannot be null.");
            }

            _roomDefinitions = roomDefinitions
                .Where(r => !string.IsNullOrEmpty(r.Name))
                .ToDictionary(r => r.Name!, StringComparer.OrdinalIgnoreCase);

            if (!_roomDefinitions.Any())
            {
                Console.WriteLine("Warning: No room definitions loaded into RoomFactoryService.");
            }
        }

        public RoomService? CreateRoom(string roomName)
        {
            if (_roomDefinitions.TryGetValue(roomName, out RoomInfo? roomDefinition))
            {
                RoomService room = new RoomService(_gameData);
                room.InitializeRoomData(roomDefinition);
                room.RoomName = roomName;

                _gridService.GenerateGridForRoom(room);

                return room;
            }
            return null;
        }
    }
}