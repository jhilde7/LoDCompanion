using LoDCompanion.Services.GameData;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    public class DungeonBuilderService
    {
        private readonly GameDataService _gameData;

        public DungeonBuilderService(GameDataService gameData)
        {
            _gameData = gameData;
        }

        public List<RoomInfo> CreateDungeonDeck(int roomCount, int corridorCount, string objectiveRoom, List<string> roomsToExclude, List<string> corridorsToExclude)
        {
            var deck = new List<RoomInfo>();

            // 1. Build the lists of rooms and corridors
            var rooms = BuildRoomList(roomCount, roomsToExclude);
            var corridors = BuildCorridorList(corridorCount, corridorsToExclude);

            deck.AddRange(rooms);
            deck.AddRange(corridors);

            // 2. Shuffle the deck
            deck.Shuffle();

            // 3. Place the objective room in the second half of the deck
            var objectiveRoomInfo = _gameData.RoomInfo.FirstOrDefault(r => r.Name == objectiveRoom);
            if (objectiveRoomInfo != null)
            {
                var halfDeckSize = deck.Count / 2;
                deck.Insert(RandomHelper.GetRandomNumber(halfDeckSize, deck.Count), objectiveRoomInfo);
            }

            return deck;
        }

        private List<RoomInfo> BuildRoomList(int count, List<string> excludedRooms)
        {
            var rooms = new List<RoomInfo>();
            var availableRooms = _gameData.RoomInfo
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
            var availableCorridors = _gameData.RoomInfo
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
