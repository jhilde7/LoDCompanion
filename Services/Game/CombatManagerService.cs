using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Player;

namespace LoDCompanion.Services.Game
{
    public class CombatManagerService
    {
        private readonly InitiativeService _initiative;
        private readonly HeroCombatService _heroCombat;
        private readonly MonsterCombatService _monsterCombat;
        private readonly PlayerActionService _playerAction;

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
            _initiative = initiativeService;
            _heroCombat = heroCombatService;
            _monsterCombat = monsterCombatService;
            _playerAction = playerActionService;
        }


        public void StartCombat(List<Hero> heroes, List<Monster> monsters)
        {
            HeroesInCombat = heroes;
            MonstersInCombat = monsters;
            UnwieldlyBonusUsed.Clear();

            // Setup the initiative for the first turn.
            _initiative.SetupInitiative(HeroesInCombat, MonstersInCombat);

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

            if (_initiative.IsTurnOver())
            {
                Console.WriteLine("--- New Turn ---");
                _initiative.SetupInitiative(HeroesInCombat, MonstersInCombat);
            }

            var nextActorType = _initiative.DrawNextToken();

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
                // Before the monster acts, check if any hero on Overwatch can interrupt.
                // This is a simplified check. A full implementation would need monster and hero positions.
                var monsterTarget = MonstersInCombat.First(m => m.CurrentHP > 0); // Simplified: first monster acts
                var interruptingHero = CheckForOverwatchInterrupt(monsterTarget);

                if (interruptingHero != null)
                {
                    Console.WriteLine($"{interruptingHero.Name} on Overwatch interrupts {monsterTarget.Name}'s action!");
                    // _heroCombatService.ExecuteOverwatchAttack(interruptingHero, monsterTarget);

                    // After the interrupt, the monster might be dead or its action cancelled.
                    // For now, we'll assume the monster's turn is consumed by the interruption.
                }
                else
                {
                    // No interruption, the monster performs its action as normal.
                    // Monster AI logic would go here.
                }

                // After the monster acts (or is interrupted), process the next actor.
                ProcessNextInInitiative();
            }
        }

        /// <summary>
        /// Checks if any hero on Overwatch can interrupt a moving monster.
        /// </summary>
        /// <param name="movingMonster">The monster that is taking its turn.</param>
        /// <returns>The hero that can interrupt, or null if none can.</returns>
        private Hero? CheckForOverwatchInterrupt(Monster movingMonster)
        {
            // This requires game state knowledge (LOS, ZOC) which isn't available here yet.
            // This is a placeholder for that future logic.
            foreach (var hero in HeroesInCombat.Where(h => h.Stance == CombatStance.Overwatch))
            {
                // TODO: Check if movingMonster is in LOS for ranged or ZOC for melee.
                bool canInterrupt = true; // Assume true for this example
                if (canInterrupt)
                {
                    return hero;
                }
            }
            return null;
        }

        // This method would be called by the UI when the player selects an action.
        public void HeroPerformsAction(PlayerActionType action, object? target = null)
        {
            if (ActiveHero != null && ActiveHero.CurrentAP > 0)
            {
                _playerAction.PerformAction(ActiveHero, action, target);

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
