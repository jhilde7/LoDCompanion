using LoDCompanion.Models.Character;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Utilities;

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
            monster.CurrentAP = monster.MaxAP;
            int ap = monster.CurrentAP;
            while (monster.CurrentAP > 0)
            {
                switch (monster.Behavior)
                {
                    case MonsterBehaviorType.HumanoidMelee:
                        ExecuteHumanoidWithCloseCombatWeapon(monster, heroes, room, ref ap);
                        break;
                    case MonsterBehaviorType.Ranged:
                        ExecuteHumanoidWithMissileWeapon(monster, heroes, room, ref ap);
                        break;
                    case MonsterBehaviorType.Beast:
                        ExecuteBeast(monster, heroes, room, ref ap);
                        break;
                    case MonsterBehaviorType.LowerUndead:
                        ExecuteLowerUndead(monster, heroes, room, ref ap); 
                        break;
                    case MonsterBehaviorType.HigherUndead:
                        ExecuteHigherUndead(monster, heroes, room, ref ap);
                        break;
                    case MonsterBehaviorType.MagicUser:
                        ExecuteMagicUser(monster, heroes, room, ref ap);
                        break;
                }
                // Decide on the action based on the monster's behavior type.
                // For now, we'll use the HumanoidMelee logic from the PDF as a template.
                // Add other behaviors (Ranged, MagicUser) here later.
            }
        }

        private void ExecuteMagicUser(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            //TODO
            throw new NotImplementedException();
        }

        private void ExecuteHigherUndead(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            //TODO
            throw new NotImplementedException();
        }

        private void ExecuteLowerUndead(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            //TODO
            throw new NotImplementedException();
        }

        private void ExecuteBeast(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            //TODO
            throw new NotImplementedException();
        }

        private void ExecuteHumanoidWithCloseCombatWeapon(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            var target = ChooseTarget(monster, heroes);
            if (target == null || monster.Position == null || target.Position == null)
            {
                ap = 0;
                return;
            }

            int distance = _gridService.GetDistance(monster.Position, target.Position);

            if (distance <= 1)
            {
                // "make room for more enemies if possible. Shove if necessary."
                // TODO: Add logic to determine if a shove would be tactically advantageous.
                // For now, we proceed to the attack table.

                // Rule 4: Attack according to the table
                int actionRoll = RandomHelper.RollDie("D6");
                if (actionRoll <= 4) // Attack
                {
                    int attackTypeRoll = RandomHelper.RollDie("D6");
                    switch (attackTypeRoll)
                    {
                        case 1: // Parry Stance
                            Console.WriteLine($"{monster.Name} takes a Parry Stance.");
                            monster.CurrentAP = 0; // Ends turn
                            break;
                        case <= 5: // Standard Attack
                            _monsterCombatService.PerformStandardAttack(monster, target);
                            monster.CurrentAP--;
                            break;
                        case 6: // Power Attack
                            _monsterCombatService.PerformPowerAttack(monster, target);
                            monster.IsVulnerableAfterPowerAttack = true;
                            monster.CurrentAP -= 2; // Power Attack costs 2 AP
                            break;
                    }
                }
                else // Use Special Skill/Talent
                {
                    Console.WriteLine($"{monster.Name} uses a special ability!");
                    // TODO: Trigger a random special ability from MonsterSpecialService
                    monster.CurrentAP--;
                }
                return;
            }

            if (distance <= monster.Move)
            {
                int roll = RandomHelper.RollDie("D6");
                if (roll == 1) // Parry Stance
                {
                    Console.WriteLine($"{monster.Name} takes a Parry Stance.");
                    monster.CurrentAP = 0;
                }
                else if (roll <= 4) // Move into CC
                {
                    Console.WriteLine($"{monster.Name} moves to engage {target.Name}.");
                    // _gridService.MoveCharacter(...);
                    monster.CurrentAP--;
                }
                else // Charge Attack
                {
                    _monsterCombatService.PerformChargeAttack(monster, target);
                    monster.CurrentAP = 0; // Charge is a 2 AP action
                }
                return;
            }
            else
            {
                Console.WriteLine($"{monster.Name} moves towards {target.Name}.");
                ap--;
                return;
            }
        }

        private void ExecuteHumanoidWithMissileWeapon(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            //TODO
        }

        private Hero? ChooseTarget(Monster monster, List<Hero> heroes)
        {
            // "target one that has not been targeted by another enemy."
            // This is complex state to track. For now, we'll use a simpler priority:
            // 1. Closest hero.
            if (!heroes.Any()) return null;

            return heroes
                .Where(h => monster.Position != null && h.Position != null)
                .OrderBy(h => _gridService.GetDistance(monster.Position!, h.Position!))
                .First();
        }
    }
}
