using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class Party
    {
        public string Id { get; private set; }
        public List<Hero> Heroes { get; set; } = new List<Hero>();
        public int Coins { get; set; }
        public int PartyMorale { get; set; }

        public Party()
        {
            Id = Guid.NewGuid().ToString();
        }

    }

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
        public int MoraleMax { get; set; }
        public int Morale {  get; set; }


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
                return gameState.CurrentParty;
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

            newHero.Party = gameState.CurrentParty;
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

        internal bool UpdateMorale(int v)
        {
            if(Morale == MoraleMax) return false;

            Morale += v;
            if(Morale > MoraleMax) Morale = MoraleMax;
            return true;
        }

        public DungeonState SetCurrentDungeon(DungeonState dungeon)
        {
            _gameStateManager.GameState.CurrentDungeon = dungeon;
            return _gameStateManager.GameState.CurrentDungeon;
        }
    }
}