using LoDCompanion.Services.GameData;
using LoDCompanion.Models.Dungeon;

namespace LoDCompanion.Services.Dungeon
{
    public class RoomFactoryService
    {
        private readonly GameDataService _gameData;
        private readonly GridService _grid;
        private readonly RoomService _roomService;

        public RoomFactoryService(GameDataService gameData, GridService gridService, RoomService roomService)
        {
            _gameData = gameData;
            _grid = gridService;
            _roomService = roomService;
        }

        public Room? CreateRoom(string roomName)
        {
            Room room = new Room();
            RoomInfo roomDefinition = _roomService.GetRoomByName(roomName);
            _roomService.InitializeRoomData(roomDefinition, room);
            room.RoomName = roomName;

            _grid.GenerateGridForRoom(room);

            return room;
        }
    }
}