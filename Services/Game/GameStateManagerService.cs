using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Models;

namespace LoDCompanion.Services.Game
{
    public class GameStateManagerService
    {
        private readonly IStatePersistenceService _persistenceService;
        public event Action? OnStateChanged;
        public bool HasSavedGame => GameState.CurrentParty != null && GameState.CurrentParty.Heroes.Any();

        public GameState GameState { get; private set; } = new();

        public GameStateManagerService(IStatePersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
        }

        public async Task InitializeGameAsync()
        {
            var loadedState = await _persistenceService.LoadGameStateAsync();
            if (loadedState != null)
            {
                GameState = loadedState;
            }
            NotifyStateChanged();
        }
        private void NotifyStateChanged() => OnStateChanged?.Invoke();

        public async Task<bool> SaveGameAsync(string locationUrl)
        {
            GameState.CurrentLocationUrl = locationUrl;
            try
            {
                if (GameState != null)
                {
                    Console.WriteLine("GameStateManager: SaveGameAsync called.");
                    await _persistenceService.SaveGameStateAsync(GameState);
                    Console.WriteLine("GameStateManager: Save successful!");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GameStateManager: CRITICAL - Save FAILED. Exception: {ex}");
                return false;
            }
        }

        public async Task StartNewGameAsync()
        {
            GameState = new GameState { CurrentParty = new Party() };
            await Task.CompletedTask;
        }

        // Example of a state-changing action
        public void EnterDungeon(DungeonState dungeon)
        {
            GameState.CurrentDungeon = dungeon;
            // The save will be triggered by another action, like moving rooms.
        }

        /// <summary>
        /// Clears the current dungeon state after a quest is successfully completed.
        /// </summary>
        public void CompleteDungeon()
        {
            GameState.CurrentDungeon = null;
            // Optionally, save the game here to persist the rewards and removed dungeon.
            // await SaveGameAsync("In-Settlement");
            NotifyStateChanged();
        }

        /// <summary>
        /// Temporarily leaves the dungeon, for abandoning a quest.
        /// </summary>
        public void LeaveDungeon()
        {
            // This method is used when abandoning a quest. The saved state in local storage
            // will still contain the dungeon progress. This just clears it for the current session.
            GameState.CurrentDungeon = null;
            NotifyStateChanged();
        }
    }
}
