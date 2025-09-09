using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Combat
{
    /// <summary>
    /// Represents the type of actor whose turn it is.
    /// </summary>
    public enum ActorType
    {
        Hero,
        Monster
    }

    /// <summary>
    /// Manages the initiative token "bag" for combat turns.
    /// </summary>
    public class InitiativeService
    {
        private List<ActorType> _initiativeTokens = new List<ActorType>();

        // Properties to hold modifications from quest setup
        public int HeroInitiativeModifier { get; set; } = 0;
        public int MonsterInitiativeModifier { get; set; } = 0;
        public ActorType? ForcedFirstActor { get; set; } = null;

        /// <summary>
        /// Resets initiative modifiers to their default state.
        /// This should be called before setting up a new quest.
        /// </summary>
        public void ResetModifiers()
        {
            HeroInitiativeModifier = 0;
            MonsterInitiativeModifier = 0;
            ForcedFirstActor = null;
        }

        /// <summary>
        /// Sets up the initiative tokens for the start of a new combat encounter.
        /// </summary>
        /// <param name="heroes">The list of heroes in the combat.</param>
        /// <param name="monsters">The list of monsters in the combat.</param>
        /// <param name="didBashDoor">True if the heroes bashed down the door to enter combat.</param>
        public void SetupInitiative(List<Hero> heroes, List<Monster> monsters, bool didBashDoor = false, bool firstTurn = false)
        {
            _initiativeTokens.Clear();

            // Add one token per hero, *unless* they are on Overwatch.
            foreach (var hero in heroes)
            {
                if (hero.CombatStance != CombatStance.Overwatch)
                {
                    if (hero.ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.Initiative) != null) _initiativeTokens.Add(ActorType.Hero);
                    _initiativeTokens.Add(ActorType.Hero);
                }
            }

            // Add one token per monster
            foreach (var monster in monsters)
            {
                if (monster.IsUnique) _initiativeTokens.Add(ActorType.Monster); // unique monsters give added chance of activation
                _initiativeTokens.Add(ActorType.Monster);
            }

            // Handle "Bashing Down Doors"
            if (didBashDoor)
            {
                _initiativeTokens.Add(ActorType.Monster);
                _initiativeTokens.Add(ActorType.Monster);
            }

            if (firstTurn)
            {
                // Handle "Perfect Hearing" rule
                bool heroHasPerfectHearing = heroes.Any(h => h.Talents.Any(t => t.Name == TalentName.PerfectHearing));
                bool monsterHasPerfectHearing = monsters.Any(m => m.SpecialRules.Contains("Perfect Hearing"));

                if (heroHasPerfectHearing && !monsterHasPerfectHearing)
                {
                    _initiativeTokens.Add(ActorType.Hero);
                }
                else if (monsterHasPerfectHearing && !heroHasPerfectHearing)
                {
                    _initiativeTokens.Add(ActorType.Monster);
                }

                // Apply initiative modifiers from quest setup
                for (int i = 0; i < HeroInitiativeModifier; i++)
                {
                    _initiativeTokens.Add(ActorType.Hero);
                }
                for (int i = 0; i < MonsterInitiativeModifier; i++)
                {
                    _initiativeTokens.Add(ActorType.Monster);
                }
                if (ForcedFirstActor != null)
                {
                    if (ForcedFirstActor is ActorType.Monster)
                    {
                        _initiativeTokens.RemoveAll(a => a == ActorType.Hero);
                    }
                    else
                    {
                        _initiativeTokens.RemoveAll(a => a == ActorType.Monster);
                    } 
                }
            }

            // Shuffle the tokens to randomize the turn order
            _initiativeTokens.Shuffle();
        }

        /// <summary>
        /// Draws the next actor's token from the bag.
        /// </summary>
        /// <returns>The ActorType of the next actor to take a turn.</returns>
        public ActorType DrawNextToken()
        {
            if (!_initiativeTokens.Any())
            {
                // This should ideally not be reached if IsTurnOver is checked, but it's a safe fallback.
                throw new InvalidOperationException("Cannot draw a token from an empty initiative bag.");
            }
            _initiativeTokens.Shuffle();
            var token = _initiativeTokens[0];
            _initiativeTokens.RemoveAt(0);
            return token;
        }

        /// <summary>
        /// Checks if the current combat turn is over (i.e., all tokens have been drawn).
        /// </summary>
        public bool IsTurnOver()
        {
            return !_initiativeTokens.Any();
        }

        internal void AddToken(ActorType token)
        {
            _initiativeTokens.Add(token);
        }

        internal void RemoveToken(ActorType token)
        {
            _initiativeTokens.Remove(token);
        }

        public bool ForceNextActorType(ActorType actor)
        {
            if (_initiativeTokens.Contains(actor))
            {
                _initiativeTokens.Remove(actor);
                _initiativeTokens.Insert(0, actor);
                return true;
            }
            return false;
        }
    }
}
