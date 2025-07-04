using LoDCompanion.Models.Character;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Game
{
    public class MonsterAIService
    {
        private readonly GridService _gridService;
        private readonly MonsterCombatService _monsterCombatService;

        public MonsterAIService(GridService gridService, MonsterCombatService monsterCombatService)
        {
            _gridService = gridService;
            _monsterCombatService = monsterCombatService;
        }

        /// <summary>
        /// Executes a full turn for a given monster.
        /// </summary>
        /// <param name="monster">The monster taking its turn.</param>
        /// <param name="heroes">The list of all heroes in the combat.</param>
        /// <param name="room">The current room where combat is taking place.</param>
        public void ExecuteMonsterTurn(Monster monster, List<Hero> heroes, RoomService room)
        {
            // The monster gets 2 Action Points.
            int ap = 2;

            while (ap > 0)
            {
                // Decide on the action based on the monster's behavior type.
                // For now, we'll use the HumanoidMelee logic from the PDF as a template.
                if (monster.Behavior == MonsterBehaviorType.HumanoidMelee)
                {
                    ExecuteHumanoidMeleeAction(monster, heroes, room, ref ap);
                }
                // Add other behaviors (Ranged, MagicUser) here later.
            }
        }

        private void ExecuteHumanoidMeleeAction(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            var target = ChooseTarget(monster, heroes);
            if (target == null || monster.Position == null || target.Position == null)
            {
                ap = 0; // No valid target, end turn.
                return;
            }

            int distance = _gridService.GetDistance(monster.Position, target.Position);

            // Rule 1: If more than M spaces away, move.
            if (distance > monster.Move)
            {
                // _gridService.MoveCharacter(...) would go here.
                Console.WriteLine($"{monster.Name} moves towards {target.Name}.");
                ap--;
                return;
            }

            // Rule 2: If within M spaces but not adjacent, roll 1d6.
            if (distance > 1)
            {
                int roll = Utilities.RandomHelper.RollDie("D6");
                if (roll == 1) // Parry Stance
                {
                    Console.WriteLine($"{monster.Name} takes a Parry Stance.");
                    ap = 0; // Forfeits second action
                }
                else if (roll <= 4) // Move into CC
                {
                    // _gridService.MoveCharacter(...)
                    Console.WriteLine($"{monster.Name} moves to engage {target.Name}.");
                    ap--;
                }
                else // Charge Attack
                {
                    // _monsterCombatService.PerformChargeAttack(monster, target);
                    Console.WriteLine($"{monster.Name} charges {target.Name}!");
                    ap = 0; // Charge is a 2 AP action
                }
                return;
            }

            // Rule 3 & 4: If adjacent, attack.
            if (distance <= 1)
            {
                // For simplicity, we'll just do a standard attack.
                // A full implementation would use the attack table from the PDF.
                // _monsterCombatService.PerformStandardAttack(monster, target);
                Console.WriteLine($"{monster.Name} attacks {target.Name}.");
                ap--;
            }
        }

        /// <summary>
        /// Chooses a target based on the rules on page 103 of the PDF.
        /// </summary>
        private Hero? ChooseTarget(Monster monster, List<Hero> heroes)
        {
            // "target one that has not been targeted by another enemy."
            // This is complex state to track. For now, we'll use a simpler priority:
            // 1. Closest hero.
            if (!heroes.Any()) return null;

            return heroes
                .Where(h => monster.Position != null && h.Position != null)
                .OrderBy(h => _gridService.GetDistance(monster.Position!, h.Position!))
                .FirstOrDefault();
        }
    }
}
