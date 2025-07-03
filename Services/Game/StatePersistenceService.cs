using Blazored.LocalStorage;
using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoDCompanion.Services.Game
{
    public interface IStatePersistenceService
    {
        Task ClearSavedGameAsync();
        Task SaveGameStateAsync(GameState gameState);
        Task<GameState?> LoadGameStateAsync();
    }

    public class StatePersistenceService : IStatePersistenceService
    {
        private readonly ILocalStorageService _localStorage;
        private const string GameStateKey = "LOD_GAME_STATE";
        private readonly JsonSerializerOptions _serializerOptions;

        public StatePersistenceService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;

            // This tells the serializer how to handle loops in your object graph.
            _serializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true // Makes the JSON in local storage readable for debugging
            };
        }

        public async Task ClearSavedGameAsync()
        {
            await _localStorage.RemoveItemAsync(GameStateKey);
        }

        public async Task<GameState?> LoadGameStateAsync()
        {
            var gameStateJson = await _localStorage.GetItemAsStringAsync(GameStateKey);
            if (string.IsNullOrEmpty(gameStateJson)) return null;
            return JsonSerializer.Deserialize<GameState>(gameStateJson, _serializerOptions);
        }

        public async Task SaveGameStateAsync(GameState gameState)
        {
            var gameStateJson = JsonSerializer.Serialize(gameState, _serializerOptions);
            await _localStorage.SetItemAsStringAsync(GameStateKey, gameStateJson);
        }
    }
}