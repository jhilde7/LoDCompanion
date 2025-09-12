using LoDCompanion.Code.BackEnd.Services.Game;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Dungeon
{
    public class DungeonBuilderService
    {
        private readonly RoomService _room;

        public DungeonBuilderService(RoomService roomService)
        {
            _room = roomService;
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

            var halfDeckSize = initialDeck.Count / 2;
            var firstHalf = initialDeck.Take(halfDeckSize).ToList();
            var secondHalf = initialDeck.Skip(halfDeckSize).ToList();

            if (quest.SideQuests != null)
            {
                foreach (var sideQuest in quest.SideQuests)
                {
                    var sideQuestCardInfo = sideQuest.ObjectiveRoom;
                    if (quest.SideQuests.Any(sq => sq.Name == "The Hidden Treasure"))
                    {
                        sideQuestCardInfo = _room.GetRoomByName("R10");
                    }

                    if (sideQuestCardInfo != null)
                    {
                        Room sideQuestCard = new Room();
                        _room.InitializeRoomData(sideQuestCardInfo, sideQuestCard);
                        firstHalf.Insert(RandomHelper.GetRandomNumber(0, firstHalf.Count), sideQuestCard);
                    } 
                }
            }

            if (quest.ObjectiveRoom != null)
            {
                var objectiveRoomInfo = _room.GetRoomByName(quest.ObjectiveRoom.Name);
                Room objectiveRoom = new Room();
                _room.InitializeRoomData(objectiveRoomInfo, objectiveRoom);
                if (objectiveRoomInfo != null)
                {
                    secondHalf.Add(objectiveRoom);
                    secondHalf.Shuffle();
                } 
            }

            var finalDeck = new List<Room>();
            finalDeck.AddRange(firstHalf);
            finalDeck.AddRange(secondHalf);

            return finalDeck;
        }

        private List<Room> BuildRoomList(int count, List<RoomInfo>? excluded = null, List<RoomInfo>? included = null)
        {
            var rooms = new List<Room>();
            List<RoomInfo> availableRooms;

            // If an inclusion list is provided, use ONLY that list.
            if (included != null && included.Any())
            {
                availableRooms = included;
            }
            // Otherwise, use all rooms, applying the exclusion list if it exists.
            else
            {
                availableRooms = _room.Rooms.Where(r => r.Category == RoomCategory.Room).ToList();
                if (excluded != null && excluded.Any())
                {
                    availableRooms = availableRooms.Where(r => !excluded.Contains(r)).ToList();
                }
            }

            availableRooms.Shuffle();

            int numberToTake = Math.Min(count, availableRooms.Count);
            if (numberToTake > 0)
            {
                foreach (RoomInfo roomInfo in availableRooms.GetRange(0, numberToTake))
                {
                    rooms.Add(_room.InitializeRoomData(roomInfo, new Room()));
                }
            }

            return rooms;
        }

        private List<Room> BuildCorridorList(int count, List<RoomInfo>? excluded)
        {
            var corridors = new List<Room>();
            var available = _room.Rooms
                .Where(r => r.Category == RoomCategory.Corridor && (excluded == null || !excluded.Contains(r)))
                .ToList();

            available.Shuffle();

            int numberToTake = Math.Min(count, available.Count);
            if (numberToTake > 0)
            {
                foreach (RoomInfo roomInfo in available.GetRange(0, numberToTake))
                {
                    corridors.Add(_room.InitializeRoomData(roomInfo, new Room()));
                } 
            }

            return corridors;
        }
    }
}
