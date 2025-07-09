using LoDCompanion.Models;
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
                    case MonsterBehaviorType.HumanoidRanged:
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
            var target = ChooseTarget(monster, heroes, true);
            if (target == null || monster.Position == null || target.Position == null)
            {
                monster.CurrentAP = 0;
                return;
            }

            int distance = _gridService.GetDistance(monster.Position, target.Position);

            if (distance <= 1)
            {
                int actionRoll = RandomHelper.RollDie("D6");
                switch (actionRoll)
                {
                    case 1:
                    case 2:
                        // Move M away but stay in LOS
                        // TODO: This logic would be complex, involving finding a suitable square
                        Console.WriteLine($"{monster.Name} moves away from {target.Name}.");
                        monster.CurrentAP--;
                        break;
                    case 3:
                    case 4:
                        // Cast close combat spell
                        Console.WriteLine($"{monster.Name} casts a close combat spell on {target.Name}.");
                        //TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, new List<Hero> { target }, "SomeCloseCombatSpell");
                        monster.CurrentAP--;
                        break;
                    case 5:
                    case 6:
                        // Standard attack
                        _monsterCombatService.PerformStandardAttack(monster, target);
                        monster.CurrentAP--;
                        break;
                }
            }
            else if (_gridService.HasLineOfSight(monster.Position, target.Position).CanShoot)
            {
                int spellTypeRoll = RandomHelper.RollDie("D6");
                if (spellTypeRoll <= 4) // Ranged magic
                {
                    var rangedTarget = ChooseTarget(monster, heroes);
                    if (rangedTarget != null)
                    {
                        Console.WriteLine($"{monster.Name} casts a ranged spell on {rangedTarget.Name}.");
                        // TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, new List<Hero> { rangedTarget }, "SomeRangedSpell");
                    }
                }
                else // Support magic
                {
                    Console.WriteLine($"{monster.Name} casts a support spell.");
                    //TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, heroes, "SomeSupportSpell");
                }
                monster.CurrentAP = 0; // Casting a spell takes the whole turn
            }
            else
            {
                int actionRoll = RandomHelper.RollDie("D6");
                if (actionRoll <= 4)
                {
                    // Move to get LOS
                    Console.WriteLine($"{monster.Name} moves to get a clear line of sight.");
                }
                else
                {
                    // Cast support magic
                    Console.WriteLine($"{monster.Name} casts a support spell.");
                    // TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, heroes, "SomeSupportSpell");
                }
                monster.CurrentAP--;
            }
        }

        private void ExecuteHigherUndead(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            var target = ChooseTarget(monster, heroes);
            if (target == null || monster.Position == null || target.Position == null)
            {
                monster.CurrentAP = 0;
                return;
            }

            int distance = _gridService.GetDistance(monster.Position, target.Position);

            if (distance > monster.Move)
            {
                Console.WriteLine($"{monster.Name} moves towards {target.Name}.");
                monster.CurrentAP--;
                return;
            }

            if (distance > 1 && distance <= monster.Move)
            {
                _monsterCombatService.PerformChargeAttack(monster, target);
                monster.CurrentAP = 0;
                return;
            }

            if (distance <= 1)
            {
                int actionRoll = RandomHelper.RollDie("D6");
                if (actionRoll <= 4)
                {
                    int attackTypeRoll = RandomHelper.RollDie("D6");
                    if (attackTypeRoll <= 2)
                    {
                        _monsterCombatService.PerformPowerAttack(monster, target);
                        monster.CurrentAP -= 2;
                    }
                    else
                    {
                        _monsterCombatService.PerformStandardAttack(monster, target);
                        monster.CurrentAP--;
                    }
                }
                else
                {
                    Console.WriteLine($"{monster.Name} uses a special ability on {target.Name}!");
                    //TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, new List<Hero> { target }, "SomeSpecialAbility");
                    monster.CurrentAP--;
                }
            }
        }

        private void ExecuteLowerUndead(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            if (monster.Weapons.Any(w => w.IsRanged))
            {
                ExecuteHumanoidWithMissileWeapon(monster, heroes, room, ref ap);
                return;
            }

            var target = ChooseTarget(monster, heroes);
            if (target == null || monster.Position == null || target.Position == null)
            {
                monster.CurrentAP = 0;
                return;
            }

            int distance = _gridService.GetDistance(monster.Position, target.Position);

            if (distance > 1)
            {
                // Move towards the closest hero
                Console.WriteLine($"{monster.Name} shambles towards {target.Name}.");
                monster.CurrentAP--;
            }
            else
            {
                int attackRoll = RandomHelper.RollDie("D6");
                if (attackRoll <= 4)
                {
                    _monsterCombatService.PerformStandardAttack(monster, target);
                }
                else
                {
                    _monsterCombatService.PerformPowerAttack(monster, target);
                }
                monster.CurrentAP -= 2;
            }
        }

        private void ExecuteBeast(Monster monster, List<Hero> heroes, RoomService room, ref int ap)
        {
            var target = ChooseTarget(monster, heroes);
            if (target == null || monster.Position == null || target.Position == null)
            {
                monster.CurrentAP = 0;
                return;
            }

            int distance = _gridService.GetDistance(monster.Position, target.Position);

            if (distance > monster.Move)
            {
                Console.WriteLine($"{monster.Name} moves towards {target.Name}.");
                monster.CurrentAP--;
                return;
            }

            if (distance > 1 && distance <= monster.Move)
            {
                _monsterCombatService.PerformChargeAttack(monster, target);
                monster.CurrentAP = 0;
                return;
            }

            if (distance <= 1)
            {
                int actionRoll = RandomHelper.RollDie("D6");
                if (actionRoll <= 4)
                {
                    int attackTypeRoll = RandomHelper.RollDie("D6");
                    if (attackTypeRoll <= 4)
                    {
                        if (monster.CurrentHP < monster.MaxHP / 2)
                        {
                            _monsterCombatService.PerformStandardAttack(monster, target);
                        }
                        else
                        {
                            _monsterCombatService.PerformPowerAttack(monster, target);
                        }

                        monster.CurrentAP -= 2;
                    }
                    else
                    {
                        _monsterCombatService.PerformStandardAttack(monster, target);
                        monster.CurrentAP--;
                    }
                }
                else
                {
                    Console.WriteLine($"{monster.Name} uses a special ability on {target.Name}!");
                    // TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, new List<Hero> { target }, "SomeSpecialAbility");
                    monster.CurrentAP--;
                }
            }
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
                    // TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, new List<Hero> { target }, "SomeSpecialAbility");
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
            var target = ChooseTarget(monster, heroes, true);
            if (target == null || monster.Position == null || target.Position == null)
            {
                monster.CurrentAP = 0;
                return;
            }

            int distance = _gridService.GetDistance(monster.Position, target.Position);

            if (distance <= 2)
            {
                Console.WriteLine($"{monster.Name} moves away from {target.Name}.");

                if (monster.Weapons.First(r => r.IsRanged) is RangedWeapon rangedWeapon1 && !rangedWeapon1.IsLoaded)
                {
                    rangedWeapon1.reloadAmmo();
                    Console.WriteLine($"{monster.Name} reloads its {rangedWeapon1.Name}.");
                }
                monster.CurrentAP--;
                return;
            }

            if (monster.Weapons.First() is RangedWeapon rangedWeapon && !rangedWeapon.IsLoaded)
            {
                rangedWeapon.reloadAmmo();
                Console.WriteLine($"{monster.Name} reloads its {rangedWeapon.Name}.");
                monster.CurrentAP--;
                return;
            }

            // The monster will try to move to a better position if it hasn't already moved away.
            if (monster.CurrentAP == monster.MaxAP)
            {
                Console.WriteLine($"{monster.Name} moves to a better vantage point.");
                // Add logic for movement here
                monster.CurrentAP--;
            }

            int attackRoll = RandomHelper.RollDie("D6");
            if (attackRoll <= 4)
            {
                int aimOrShoot = RandomHelper.RollDie("D6");
                if (aimOrShoot <= 2)
                {
                    Console.WriteLine($"{monster.Name} aims carefully.");
                    // Set an "isAming" flag on the monster for the next turn
                }
                else
                {
                    Console.WriteLine($"{monster.Name} shoots at {target.Name}.");
                    _monsterCombatService.PerformStandardAttack(monster, target);
                }
            }
            else
            {
                Console.WriteLine($"{monster.Name} uses a special skill.");
                // TODO: _monsterSpecialService.ExecuteSpecialAbility(monster, new List<Hero> { target }, "SomeSpecialAbility");
            }

            monster.CurrentAP--;
        }

        private Hero? ChooseTarget(Monster monster, List<Hero> heroes, bool isRanged = false)
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
