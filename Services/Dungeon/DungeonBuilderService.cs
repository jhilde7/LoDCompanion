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

        public List<RoomInfo> CreateDungeonDeck(int roomCount, int corridorCount, RoomInfo objectiveRoom, List<string> roomsToExclude, List<string> corridorsToExclude)
        {
            var deck = new List<RoomInfo>();

            // 1. Build the lists of rooms and corridors
            var rooms = BuildRoomList(roomCount, roomsToExclude);
            var corridors = BuildCorridorList(corridorCount, corridorsToExclude);

            var initialDeck = new List<RoomInfo>();
            initialDeck.AddRange(rooms);
            initialDeck.AddRange(corridors);
            initialDeck.Shuffle();

            // 2. Divide the deck into two equal (or near-equal) piles.
            var halfDeckSize = initialDeck.Count / 2;
            var firstHalf = initialDeck.Take(halfDeckSize).ToList();
            var secondHalf = initialDeck.Skip(halfDeckSize).ToList();

            // 3. Add the objective room to one of the piles and shuffle that pile.
            var objectiveRoomInfo = _rooms.Rooms.First(r => r == objectiveRoom);
            if (objectiveRoomInfo != null)
            {
                secondHalf.Add(objectiveRoomInfo);
                secondHalf.Shuffle();
            }

            // 4. Combine the piles, placing the pile with the objective at the bottom. 
            var finalDeck = new List<RoomInfo>();
            finalDeck.AddRange(firstHalf);
            finalDeck.AddRange(secondHalf);

            return finalDeck;
        }

        private List<RoomInfo> BuildRoomList(int count, List<string> excludedRooms)
        {
            var rooms = new List<RoomInfo>();
            var availableRooms = _rooms.Rooms
                .Where(r => r.Category == RoomCategory.Room && !excludedRooms.Contains(r.Name ?? string.Empty))
                .ToList();

            for (int i = 0; i < count; i++)
            {
                if (!availableRooms.Any()) break;

                var room = availableRooms[RandomHelper.GetRandomNumber(0, availableRooms.Count - 1)];
                rooms.Add(room);
                availableRooms.Remove(room);
            }

            return rooms;
        }

        private List<RoomInfo> BuildCorridorList(int count, List<string> excludedCorridors)
        {
            var corridors = new List<RoomInfo>();
            var availableCorridors = _rooms.Rooms
                .Where(r => r.Category == RoomCategory.Corridor && !excludedCorridors.Contains(r.Name ?? string.Empty))
                .ToList();

            for (int i = 0; i < count; i++)
            {
                if (!availableCorridors.Any()) break;

                var corridor = availableCorridors[RandomHelper.GetRandomNumber(0, availableCorridors.Count - 1)];
                corridors.Add(corridor);
                availableCorridors.Remove(corridor);
            }

            return corridors;
        }
    }
}
