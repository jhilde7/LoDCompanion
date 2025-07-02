using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.State;

namespace LoDCompanion.Services.Player
{
    public class PartyManagerService
    {
        private readonly PartyState _partyState;
        public Party Party => GetCurrentParty();

        // Inject the state into the service's constructor
        public PartyManagerService(PartyState partyState)
        {
            _partyState = partyState;
        }

        public void CreateParty()
        {
            _partyState.CreateParty();
        }

        public Party GetCurrentParty()
        {
            if (_partyState.CurrentParty != null)
            {
                return _partyState.CurrentParty!;
            }
            else
            {
                CreateParty();
                // Ensure that CreateParty actually sets CurrentParty, otherwise throw
                if (_partyState.CurrentParty == null)
                {
                    throw new InvalidOperationException("Failed to create a new party.");
                }
                return _partyState.CurrentParty;
            }
        }

        public void AddHeroToParty(Hero newHero)
        {
            // Ensure the party exists before adding a hero
            var party = GetCurrentParty();
            party.Heroes ??= new List<Hero>();
            party.Heroes.Add(newHero);
        }

        // Other methods will now modify the _partyState.CurrentParty
        public void AddCoins(Hero hero)
        {
            if (_partyState.CurrentParty != null)
            {
                _partyState.CurrentParty.Coins += hero.Coins;
                hero.Coins = 0;
            }
        }
    }
}