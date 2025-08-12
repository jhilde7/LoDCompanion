using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Dungeon;

namespace LoDCompanion.BackEnd.Services.Game
{
    public interface IGameEntity
    {
        internal string Id { get; }
        string Name { get; set; }
        Room Room { get; set; }
        GridPosition? Position { get; set; } 
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
        private readonly GameStateManagerService _gameStateManager;

        public Party? HeroParty => _gameStateManager.GameState.CurrentParty;

        public WorldStateService(RoomService roomService, GameStateManagerService gameStateManagerService)
        {
            _room = roomService;
            _gameStateManager = gameStateManagerService;
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
