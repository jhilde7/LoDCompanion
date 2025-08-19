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
        public int PartyMaxMorale => GetMaxMorale();
        public PartyManagerService? PartyManager { get; set; }

        public Party()
        {
            Id = Guid.NewGuid().ToString();
        }

        private int GetMaxMorale()
        {
            return Heroes.Sum(hero => (int)Math.Floor(hero.GetStat(BasicStat.Resolve) / 10d));
        }
    }

    public enum MoraleChangeEvent
    {
        HeroDies,
        HeroDown,
        CombatWithDemons,
        HeroTerror,
        GoingHungry,
        HeroFear,
        HeroPoisoned,
        HeroDiseased,
        SprungTrap,
        Miscast,
        Trapped,
        Rest,
        FineTreasure,
        DefeatLargeMonster,
        DwarvenAle,
        WonderfulTreasure
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
        public bool PartyRetreat => Morale < 1;
        public bool PartyWavering => Morale < Math.Floor(MoraleMax / 2d);


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
            if (Party != null)
            {
                Party.PartyManager = this; // Set the PartyManager reference 
            }
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

        public void SetMaxMorale()
        {
            MoraleMax = Party?.PartyMaxMorale ?? 0;
            Morale = MoraleMax;
        }

        internal int UpdateMorale(int? v = null, MoraleChangeEvent? changeEvent = null)
        {
            int missingMorale = MoraleMax - Morale;
            if (v != null)
            {
                Morale += Math.Min((int)v, missingMorale);
                return Morale;
            }
            else if (changeEvent != null)
            {
                switch (changeEvent)
                {
                    case MoraleChangeEvent.HeroDies:
                        Morale -= 6;
                        break;
                    case MoraleChangeEvent.HeroDown:
                        Morale -= 4;
                        break;
                    case MoraleChangeEvent.CombatWithDemons:
                    case MoraleChangeEvent.HeroTerror:
                    case MoraleChangeEvent.GoingHungry:
                        Morale -= 2;
                        break;
                    case MoraleChangeEvent.HeroFear:
                    case MoraleChangeEvent.HeroPoisoned:
                    case MoraleChangeEvent.HeroDiseased:
                    case MoraleChangeEvent.SprungTrap:
                    case MoraleChangeEvent.Miscast:
                    case MoraleChangeEvent.Trapped:
                        Morale -= 1;
                        break;
                    case MoraleChangeEvent.Rest:
                    case MoraleChangeEvent.FineTreasure:
                        Morale = UpdateMorale(1);
                        break;
                    case MoraleChangeEvent.DefeatLargeMonster:
                        Morale = UpdateMorale(2);
                        break;
                    case MoraleChangeEvent.DwarvenAle:
                    case MoraleChangeEvent.WonderfulTreasure:
                        Morale = UpdateMorale(3);
                        break;
                }
            }
            return Morale;
        }

        public DungeonState SetCurrentDungeon(DungeonState dungeon)
        {
            _gameStateManager.GameState.CurrentDungeon = dungeon;
            return _gameStateManager.GameState.CurrentDungeon;
        }
    }
}