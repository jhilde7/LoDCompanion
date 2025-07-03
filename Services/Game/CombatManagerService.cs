using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Services.Combat;

namespace LoDCompanion.Services.Game
{
    public class CombatManagerService
    {
        private readonly InitiativeService _initiativeService;
        private readonly HeroCombatService _heroCombatService;
        private readonly MonsterCombatService _monsterCombatService;

        private List<Hero> _heroesInCombat = new List<Hero>();
        private List<Monster> _monstersInCombat = new List<Monster>();
        // This set will store the unique ID of each character who has used their Unwieldly bonus in this combat.
        private HashSet<string> _unwieldlyBonusUsed = new HashSet<string>();

        public CombatManagerService(
            InitiativeService initiativeService,
            HeroCombatService heroCombatService,
            MonsterCombatService monsterCombatService)
        {
            _initiativeService = initiativeService;
            _heroCombatService = heroCombatService;
            _monsterCombatService = monsterCombatService;
        }


        public void StartCombat(List<Hero> heroes, List<Monster> monsters)
        {
            _heroesInCombat = heroes;
            _monstersInCombat = monsters;
            // At the start of every fight, clear the set. This is the only reset you need!
            _unwieldlyBonusUsed.Clear();

            // Setup the initiative for the first turn.
            _initiativeService.SetupInitiative(_heroesInCombat, _monstersInCombat);

            Console.WriteLine("Combat has started!");
        }

        public void ProcessCombatTurn()
        {
            if (_initiativeService.IsTurnOver())
            {
                Console.WriteLine("Turn is over. Setting up for the next turn.");
                _initiativeService.SetupInitiative(_heroesInCombat, _monstersInCombat);
            }

            var nextActor = _initiativeService.DrawNextToken();

            if (nextActor == ActorType.Hero)
            {
                // It's a hero's turn. In a real UI, you would let the player choose which hero acts.
                // For now, we'll just log it.
                Console.WriteLine("A hero acts!");
                // Example: _heroCombatService.PerformAction(chosenHero, target);
            }
            else // It's a Monster's turn
            {
                // It's a monster's turn. You would have AI logic to select a monster and its action.
                Console.WriteLine("A monster acts!");
                // Example: _monsterCombatService.PerformAction(chosenMonster, target);
            }
        }

        public int CalculateDamage(Hero attacker, MeleeWeapon weapon)
        {
            int totalDamage = 0; // Roll your base damage...

            // --- Unwieldly Bonus Logic ---
            // 1. Check if the weapon has the Unwieldly property.
            if (weapon.HasProperty(WeaponProperty.Unwieldly))
            {
                // 2. Check if the attacker has ALREADY used their bonus in this combat.
                if (!_unwieldlyBonusUsed.Contains(attacker.Id)) // Assuming Hero has a unique Id
                {
                    // 3. If not, apply the bonus and record that it has been used.
                    int bonus = weapon.GetPropertyValue(WeaponProperty.Unwieldly);
                    totalDamage += bonus;

                    // 4. Add the attacker's ID to the set so they can't get the bonus again this fight.
                    _unwieldlyBonusUsed.Add(attacker.Id);
                }
            }

            return totalDamage;
        }

        public bool IsCombatOver()
        {
            return !_heroesInCombat.Any(h => h.CurrentHP > 0) || !_monstersInCombat.Any(m => m.CurrentHP > 0);
        }
    }
}
