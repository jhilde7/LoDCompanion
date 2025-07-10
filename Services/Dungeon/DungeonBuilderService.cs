using LoDCompanion.Services.GameData;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    public class DungeonBuilderService
    {
        private readonly RoomService _rooms;

        public DungeonBuilderService(RoomService roomService)
        {
            _rooms = roomService;
        }

        public List<Room> CreateDungeonDeck(int roomCount, int corridorCount, Room objectiveRoom, List<string> roomsToExclude, List<string> corridorsToExclude)
        {
            var deck = new List<Room>();

            // 1. Build the lists of rooms and corridors
            var rooms = BuildRoomList(roomCount, roomsToExclude);
            var corridors = BuildCorridorList(corridorCount, corridorsToExclude);

            var initialDeck = new List<Room>();
            initialDeck.AddRange(rooms);
            initialDeck.AddRange(corridors);
            initialDeck.Shuffle();

            // 2. Divide the deck into two equal (or near-equal) piles.
            var halfDeckSize = initialDeck.Count / 2;
            var firstHalf = initialDeck.Take(halfDeckSize).ToList();
            var secondHalf = initialDeck.Skip(halfDeckSize).ToList();

            // 3. Add the objective room to one of the piles and shuffle that pile.
            var objectiveRoomInfo = _rooms.GetRoomByName(objectiveRoom.RoomName);
            _rooms.InitializeRoomData(objectiveRoomInfo, objectiveRoom);
            if (objectiveRoomInfo != null)
            {
                secondHalf.Add(objectiveRoom);
                secondHalf.Shuffle();
            }

            // 4. Combine the piles, placing the pile with the objective at the bottom. 
            var finalDeck = new List<Room>();
            finalDeck.AddRange(firstHalf);
            finalDeck.AddRange(secondHalf);

            return finalDeck;
        }

        private List<Room> BuildRoomList(int count, List<string> excludedRooms)
        {
            var rooms = new List<Room>();
            var availableRooms = _rooms.Rooms
                .Where(r => r.Category == RoomCategory.Room && !excludedRooms.Contains(r.Name ?? string.Empty))
                .ToList();

            availableRooms.Shuffle();
            foreach (RoomInfo roomInfo in availableRooms.GetRange(0, count))
            {
                rooms.Add(_rooms.InitializeRoomData(roomInfo, new Room()));
            }

            return rooms;
        }

        private List<Room> BuildCorridorList(int count, List<string> excludedCorridors)
        {
            var corridors = new List<Room>();
            var availableCorridors = _rooms.Rooms
                .Where(r => r.Category == RoomCategory.Corridor && !excludedCorridors.Contains(r.Name ?? string.Empty))
                .ToList();

            availableCorridors.Shuffle();
            foreach (RoomInfo roomInfo in availableCorridors.GetRange(0, count))
            {
                corridors.Add(_rooms.InitializeRoomData(roomInfo, new Room()));
            }

            return corridors;
        }
    }
}
