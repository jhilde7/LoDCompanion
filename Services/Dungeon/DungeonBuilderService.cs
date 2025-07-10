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

        public List<Room> CreateDungeonDeck(Quest quest)
        {
            var deck = new List<Room>();

            // 1. Build the lists of rooms and corridors
            var rooms = BuildRoomList(quest.RoomCount, quest.RoomsToExclude);
            var corridors = BuildCorridorList(quest.CorridorCount, quest.CorridorsToExclude);

            var initialDeck = new List<Room>();
            initialDeck.AddRange(rooms);
            initialDeck.AddRange(corridors);
            initialDeck.Shuffle();

            // 2. Divide the deck into two equal (or near-equal) piles.
            var halfDeckSize = initialDeck.Count / 2;
            var firstHalf = initialDeck.Take(halfDeckSize).ToList();
            var secondHalf = initialDeck.Skip(halfDeckSize).ToList();

            // 3. Add the objective room to one of the piles and shuffle that pile.
            if (quest.ObjectiveRoom != null)
            {
                var objectiveRoomInfo = _rooms.GetRoomByName(quest.ObjectiveRoom.Name);
                Room objectiveRoom = new Room();
                _rooms.InitializeRoomData(objectiveRoomInfo, objectiveRoom);
                if (objectiveRoomInfo != null)
                {
                    secondHalf.Add(objectiveRoom);
                    secondHalf.Shuffle();
                } 
            }

            // 4. Combine the piles, placing the pile with the objective at the bottom. 
            var finalDeck = new List<Room>();
            finalDeck.AddRange(firstHalf);
            finalDeck.AddRange(secondHalf);

            return finalDeck;
        }

        private List<Room> BuildRoomList(int count, List<RoomInfo>? excluded)
        {
            var rooms = new List<Room>();
            var available = _rooms.Rooms
                .Where(r => r.Category == RoomCategory.Room && (excluded == null || !excluded.Contains(r)))
                .ToList();

            available.Shuffle();

            int numberToTake = Math.Min(count, available.Count);
            if (numberToTake > 0)
            {
                foreach (RoomInfo roomInfo in available.GetRange(0, numberToTake))
                {
                    rooms.Add(_rooms.InitializeRoomData(roomInfo, new Room()));
                }
            }

            return rooms;
        }

        private List<Room> BuildCorridorList(int count, List<RoomInfo>? excluded)
        {
            var corridors = new List<Room>();
            var available = _rooms.Rooms
                .Where(r => r.Category == RoomCategory.Corridor && (excluded == null || !excluded.Contains(r)))
                .ToList();

            available.Shuffle();

            int numberToTake = Math.Min(count, available.Count);
            if (numberToTake > 0)
            {
                foreach (RoomInfo roomInfo in available.GetRange(0, numberToTake))
                {
                    corridors.Add(_rooms.InitializeRoomData(roomInfo, new Room()));
                } 
            }

            return corridors;
        }
    }
}
