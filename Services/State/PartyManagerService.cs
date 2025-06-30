using LoDCompanion.Models.Character;

namespace LoDCompanion.Services.State
{
    public class PartyManagerService
    {
        public List<Hero> PartyMembers { get; private set; } = new List<Hero>();

        public int PartyMorale { get; private set; }
        public int MaxPartyMorale { get; private set; }

        public void AddHeroToParty(Hero hero)
        {
            if (hero != null && !PartyMembers.Any(p => p.Name == hero.Name)) // Basic check to prevent duplicates
            {
                PartyMembers.Add(hero);
                RecalculatePartyMorale();
            }
        }

        public void RemoveHeroFromParty(Hero hero)
        {
            PartyMembers.Remove(hero);
            RecalculatePartyMorale();
        }

        public void ClearParty()
        {
            PartyMembers.Clear();
            RecalculatePartyMorale();
        }

        public List<Hero> GetParty()
        {
            return PartyMembers;
        }

        private void RecalculatePartyMorale()
        {
            if (PartyMembers == null || !PartyMembers.Any())
            {
                PartyMorale = 0;
                MaxPartyMorale = 0;
                return;
            }

            int totalResolve = 0;
            foreach (var hero in PartyMembers)
            {
                // Each hero contributes morale equal to their Resolve divided by 10, rounded up.
                totalResolve += (int)Math.Ceiling((double)hero.Resolve / 10);
            }
            MaxPartyMorale = totalResolve;
            PartyMorale = MaxPartyMorale; // Morale starts at max
        }
    }
}