using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Services.Player
{
    public class PartyManagerService
    {
        private readonly GameStateManagerService _gameStateManager;
        public Party? Party => _gameStateManager.GameState.CurrentParty;
        public Action? OnPartyChanged;
        private Hero? _selectedHero;
        public Hero? SelectedHero
        {
            get => _selectedHero;
            set
            {
                if (_selectedHero != value)
                {
                    _selectedHero = value;
                    OnSelectedHeroChanged?.Invoke(_selectedHero);
                }
            }
        }
        public event Action<Hero?>? OnSelectedHeroChanged;


        // Inject the state into the service's constructor
        public PartyManagerService(GameStateManagerService gameStateManagerService)
        {
            _gameStateManager = gameStateManagerService;
        }

        public void StartNewParty()
        {
            CreateParty();
        }

        public void CreateParty()
        {
            var gameState = _gameStateManager.GameState;
            gameState.CurrentParty = new Party();
        }

        public Party GetCurrentParty()
        {
            var gameState = _gameStateManager.GameState;
            if (gameState.CurrentParty != null)
            {
                return gameState.CurrentParty!;
            }
            else
            {
                CreateParty();
                // Ensure that CreateParty actually sets CurrentParty, otherwise throw
                if (gameState.CurrentParty == null)
                {
                    throw new InvalidOperationException("Failed to create a new party.");
                }
                return gameState.CurrentParty;
            }
        }

        public void AddHeroToParty(Hero newHero)
        {
            var gameState = _gameStateManager.GameState;

            if (gameState.CurrentParty == null)
            {
                gameState.CurrentParty = new Party();
            }

            gameState.CurrentParty.Heroes.Add(newHero);
        }

        // Other methods will now modify the _partyState.CurrentParty
        public void AddCoins(Hero hero)
        {
            var gameState = _gameStateManager.GameState;

            if (gameState.CurrentParty != null)
            {
                gameState.CurrentParty.Coins += hero.Coins;
                hero.Coins = 0;
            }
        }
    }
}