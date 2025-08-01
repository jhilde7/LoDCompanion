using LoDCompanion.Services.GameData;
using LoDCompanion.Models.Dungeon;

namespace LoDCompanion.Services.Dungeon
{
    public class RoomFactoryService
    {
        private readonly RoomService _roomService;

        public RoomFactoryService(RoomService roomService)
        {
            _roomService = roomService;
        }

        public Room? CreateRoom(string roomName)
        {
            Room room = new Room();
            RoomInfo roomDefinition = _roomService.GetRoomByName(roomName);
            _roomService.InitializeRoomData(roomDefinition, room);
            room.Name = roomName;

            GridService.GenerateGridForRoom(room);

            return room;
        }
    }
}