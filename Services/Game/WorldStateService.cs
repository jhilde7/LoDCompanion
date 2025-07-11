using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Player;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Game
{
    public interface IGameEntity
    {
        string Name { get; set; }
        Room Room { get; set; }
        GridPosition Position { get; set; } 
        List<GridPosition> OccupiedSquares { get; set; }
    }

    public interface IWorldStateService
    {
        /// <summary>
        /// Finds any IGameEntity (Monster, Hero, Furniture, etc.) by its unique name.
        /// </summary>
        /// <returns>The entity if found; otherwise, null.</returns>
        IGameEntity? FindEntityInRoomByName(Room room, string name);
    }

    public class WorldStateService : IWorldStateService
    {
        private readonly RoomService _room;
        private readonly EncounterService _encounter;
        private readonly PartyManagerService _party;

        public WorldStateService(RoomService fm, EncounterService mm, PartyManagerService hm)
        {
            _room = fm;
            _encounter = mm;
            _party = hm;
        }

        public IGameEntity? FindEntityInRoomByName(Room room, string name)
        {
            IGameEntity? entity = null;

            // Search each manager in order until the entity is found.
            entity = _room.GetFurnitureInRoomByName(room, name);
            if (entity != null) return entity;

            entity = _room.GetMonsterInRoomByName(room, name);
            if (entity != null) return entity;

            entity = _room.GetHeroInRoomByName(room, name);
            if (entity != null) return entity;

            // Return null if no entity with that name exists.
            return null;
        }
    }
}
