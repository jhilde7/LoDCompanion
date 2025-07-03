using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Player;

namespace LoDCompanion.Services.Game
{
    public class CombatManagerService
    {
        private readonly InitiativeService _initiativeService;
        private readonly HeroCombatService _heroCombatService;
        private readonly MonsterCombatService _monsterCombatService;
        private readonly PlayerActionService _playerActionService;

        private List<Hero> HeroesInCombat = new List<Hero>();
        private List<Monster> MonstersInCombat = new List<Monster>();
        private Hero? ActiveHero;
        // This set will store the unique ID of each character who has used their Unwieldly bonus in this combat.
        private HashSet<string> UnwieldlyBonusUsed = new HashSet<string>();

        public CombatManagerService(
            InitiativeService initiativeService,
            HeroCombatService heroCombatService,
            MonsterCombatService monsterCombatService,
            PlayerActionService playerActionService)
        {
            _initiativeService = initiativeService;
            _heroCombatService = heroCombatService;
            _monsterCombatService = monsterCombatService;
            _playerActionService = playerActionService;
        }


        public void StartCombat(List<Hero> heroes, List<Monster> monsters)
        {
            HeroesInCombat = heroes;
            MonstersInCombat = monsters;
            UnwieldlyBonusUsed.Clear();

            // Setup the initiative for the first turn.
            _initiativeService.SetupInitiative(HeroesInCombat, MonstersInCombat);

            Console.WriteLine("Combat has started!");
            ProcessNextInInitiative();
        }

        /// <summary>
        /// Processes the next actor in the initiative order.
        /// </summary>
        public void ProcessNextInInitiative()
        {
            if (IsCombatOver())
            {
                Console.WriteLine("Combat is over!");
                return;
            }

            if (_initiativeService.IsTurnOver())
            {
                Console.WriteLine("--- New Turn ---");
                _initiativeService.SetupInitiative(HeroesInCombat, MonstersInCombat);
            }

            var nextActorType = _initiativeService.DrawNextToken();

            if (nextActorType == ActorType.Hero)
            {
                // In a real UI, you would prompt the player to choose which hero acts.
                // For now, we'll pick the first available hero who hasn't acted this turn.
                ActiveHero = HeroesInCombat.FirstOrDefault(h => h.CurrentAP > 0); // Simplified logic
                if (ActiveHero != null)
                {
                    ActiveHero.CurrentAP = ActiveHero.MaxAP; // Reset AP at the start of their turn
                    Console.WriteLine($"It's {ActiveHero.Name}'s turn. They have {ActiveHero.CurrentAP} AP.");
                    // The game would now wait for player input to call _playerActionService.PerformAction(...)
                }
            }
            else // It's a Monster's turn
            {
                ActiveHero = null; // No hero is active
                Console.WriteLine("A monster acts!");
                // Monster AI logic would go here.
                // After the monster acts, we automatically process the next in initiative.
                ProcessNextInInitiative();
            }
        }

        // This method would be called by the UI when the player selects an action.
        public void HeroPerformsAction(PlayerActionType action, object? target = null)
        {
            if (ActiveHero != null && ActiveHero.CurrentAP > 0)
            {
                _playerActionService.PerformAction(ActiveHero, action, target);

                if (ActiveHero.CurrentAP <= 0)
                {
                    Console.WriteLine($"{ActiveHero.Name}'s turn is over.");
                    ProcessNextInInitiative();
                }
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
                if (!UnwieldlyBonusUsed.Contains(attacker.Id)) // Assuming Hero has a unique Id
                {
                    // 3. If not, apply the bonus and record that it has been used.
                    int bonus = weapon.GetPropertyValue(WeaponProperty.Unwieldly);
                    totalDamage += bonus;

                    // 4. Add the attacker's ID to the set so they can't get the bonus again this fight.
                    UnwieldlyBonusUsed.Add(attacker.Id);
                }
            }

            return totalDamage;
        }

        public bool IsCombatOver()
        {
            return !HeroesInCombat.Any(h => h.CurrentHP > 0) || !MonstersInCombat.Any(m => m.CurrentHP > 0);
        }
    }
}
