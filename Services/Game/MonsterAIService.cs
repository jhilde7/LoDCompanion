using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LoDCompanion.Services.Game
{
    public class MonsterAIService
    {
        private readonly MonsterSpecialService _monsterSpecial;
        private readonly DungeonState _dungeon;

        public MonsterAIService(
            MonsterSpecialService monsterSpecialService,
            DungeonState dungeon)
        {
            _monsterSpecial = monsterSpecialService;
            _dungeon = dungeon;
        }

        /// <summary>
        /// Executes a monster's turn by repeatedly choosing the best action until AP is depleted.
        /// </summary>
        public void ExecuteMonsterTurn(Monster monster, List<Hero> heroes, Room room)
        {
            while (monster.CurrentAP > 0)
            {
                // Store AP before acting to check if an action was taken.
                int apBeforeAction = monster.CurrentAP;

                // The main decision-making hub.
                DecideAndPerformAction(monster, heroes, room);
            }

            // After all actions, ensure the monster faces the best direction.
            EndTurnFacing(monster, heroes);
        }

        /// <summary>
        /// The core AI decision tree that routes to the correct behavior.
        /// </summary>
        private void DecideAndPerformAction(Monster monster, List<Hero> heroes, Room room)
        {
            Hero? target = ChooseTarget(monster, heroes);
            if (target == null) return; // No valid targets, do nothing.

            // Delegate to the appropriate consolidated behavior handler.
            switch (monster.Behavior)
            {
                case MonsterBehaviorType.Beast:
                case MonsterBehaviorType.HigherUndead:
                    ExecuteAggressiveMeleeBehavior(monster, target, heroes);
                    break;
                case MonsterBehaviorType.HumanoidMelee:
                    ExecuteHumanoidMeleeBehavior(monster, target, heroes);
                    break;
                case MonsterBehaviorType.HumanoidRanged:
                    ExecuteRangedBehavior(monster, target, heroes);
                    break;
                case MonsterBehaviorType.MagicUser:
                    ExecuteMagicUserBehavior(monster, target, heroes, room);
                    break;
                case MonsterBehaviorType.LowerUndead:
                    RangedWeapon? missile = (RangedWeapon?)monster.Weapons.First(x => x.IsRanged);
                    if (missile != null) ExecuteRangedBehavior(monster, target, heroes);
                    else ExecuteLowerUndeadBehavior(monster, target, heroes);
                    break;
            }
        }

        private void ExecuteAggressiveMeleeBehavior(Monster monster, Hero target, List<Hero> heroes)
        {
            int distance = GridService.GetDistance(monster.Position, target.Position);

            if (distance > 1)
            {
                if (distance < monster.Move && monster.CurrentAP >=2)
                {                    
                    MonsterCombatService.PerformChargeAttack(monster, monster.Weapons.FirstOrDefault(w => w.IsMelee), target);
                }
                else
                {
                    MoveTowards(monster, target);
                } 
            }
            else
            {
                HandleAdjacentMeleeAttack(monster, monster.Weapons.FirstOrDefault(w => w.IsMelee), target, heroes);
            }
        }

        private void ExecuteLowerUndeadBehavior(Monster monster, Hero target, List<Hero> heroes)
        {
            int distance = GridService.GetDistance(monster.Position, target.Position);

            if (distance >= monster.Move)
            {
                MoveTowards(monster, target);
            }
            else
            {
                HandleAdjacentMeleeAttack(monster, monster.Weapons.FirstOrDefault(w => w.IsMelee), target, heroes);
            }
        }

        private void ExecuteHumanoidMeleeBehavior(Monster monster, Hero target, List<Hero> heroes)
        {
            int distance = GridService.GetDistance(monster.Position, target.Position);

            if (distance > monster.Move)
            {
                MoveTowards(monster, target); 
            }
            else if (distance > 1 && distance <= monster.Move) 
            {
                int roll = RandomHelper.RollDie("D6");
                switch (roll)
                {
                    case 1: EnterParryStance(monster); break;
                    case <= 4: MoveTowards(monster, target); break;
                    default:
                        if (monster.CurrentAP >= 2) MonsterCombatService.PerformChargeAttack(monster, monster.Weapons.FirstOrDefault(w => w.IsMelee), target);
                        break;
                }
            }
            else
            {
                HandleAdjacentMeleeAttack(monster, monster.Weapons.FirstOrDefault(w => w.IsMelee), target, heroes); 
            }
        }

        private void ExecuteRangedBehavior(Monster monster, Hero target, List<Hero> heroes)
        {
            int distance = GridService.GetDistance(monster.Position, target.Position);
            Dictionary<Hero, int> distances = new Dictionary<Hero, int>();
            RangedWeapon? missile = (RangedWeapon?)monster.Weapons.First(x => x.IsRanged);
            MeleeWeapon? melee = (MeleeWeapon?)monster.Weapons.First(x => x.IsMelee);

            foreach (Hero hero in heroes)
            {
                distances.Add(hero, GridService.GetDistance(monster.Position, hero.Position)); 
            }

            if (monster.Weapons.FirstOrDefault(w => w.IsRanged) is RangedWeapon weapon)
            {
                if (distances.Values.FirstOrDefault(i => i <= 2) > 0)
                {
                    target = distances.FirstOrDefault(h => h.Value <= 2).Key;
                    if(MoveAwayFrom(monster, target))
                    {
                        if (missile != null && !missile.IsLoaded)
                        {
                            missile.reloadAmmo();
                            Console.Write(" and reloads");
                        }
                    }
                    else
                    {
                        if (GridService.GetDistance(monster.Position, target.Position) <= 1)
                        {
                            HandleAdjacentMeleeAttack(monster, monster.Weapons.FirstOrDefault(w => w.IsMelee), target, heroes);
                        }
                        else
                        {
                            MonsterCombatService.PerformStandardAttack(monster, weapon, target);
                        }
                    }
                }
                else if (missile != null && !missile.IsLoaded)
                {
                    missile.reloadAmmo();
                    monster.CurrentAP--;
                }
                else if(!GridService.HasLineOfSight(monster.Position, target.Position, _dungeon.DungeonGrid).CanShoot)
                {
                    MoveToGetLineOfSight(monster, target);
                }
                else if(distance <= 1)
                {
                    HandleAdjacentMeleeAttack(monster, monster.Weapons.FirstOrDefault(w => w.IsMelee), target, heroes);
                }
                else
                {
                int actionRoll = RandomHelper.RollDie("D6");
                    if (actionRoll <= 4)
                    {
                        int attackTypeRoll = RandomHelper.RollDie("D6");
                        switch (attackTypeRoll)
                        {
                            case <= 2:
                                if (monster.CombatStance != CombatStance.Aiming)
                                {
                                    monster.CombatStance = CombatStance.Aiming;
                                    Console.WriteLine($"{monster.Name} starts aiming");
                                    monster.CurrentAP--;
                                }
                                else MonsterCombatService.PerformStandardAttack(monster, weapon, target);
                                break;
                            default:
                                MonsterCombatService.PerformStandardAttack(monster, weapon, target); break;
                        }
                    }
                    else
                    {
                        List<SpecialActiveAbility> specialAttacks = _monsterSpecial.GetSpecialAttacks(monster.SpecialRules);
                        if (specialAttacks.Count > 0)
                        {
                            specialAttacks.Shuffle();
                            _monsterSpecial.ExecuteSpecialAbility(monster, heroes, target, specialAttacks[0]);
                            monster.CurrentAP--;
                        }
                        else
                        {
                            MonsterCombatService.PerformStandardAttack(monster, weapon, target);
                        }
                    }
                }
            }
        }

        private void ExecuteMagicUserBehavior(Monster monster, Hero? target, List<Hero> heroes, Room room)
        {
            var adjacentHeroes = heroes.Where(h => GridService.GetDistance(monster.Position, h.Position) <= 1).ToList();
            var losHeroes = heroes.Where(h => GridService.HasLineOfSight(monster.Position, h.Position, _dungeon.DungeonGrid).CanShoot).ToList();

            int roll = RandomHelper.RollDie("D6");
            if (adjacentHeroes.Any())
            {
                target = ChooseTarget(monster, adjacentHeroes); // Choose from adjacent heroes
                if (target == null) { return; }

                switch (roll)
                {
                    case <= 2: MoveAwayFrom(monster, target); break;
                    case <= 4:
                        var spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.CloseCombat);
                        if (spellChoice != null)
                        {
                            MonsterSpell spell = spellChoice.FirstOrDefault().Key;
                            GridPosition targetPosition = spellChoice.FirstOrDefault().Value;
                            spell.CastSpell(monster, targetPosition, _dungeon);
                        }
                        break;
                    default: HandleAdjacentMeleeAttack(monster, monster.Weapons.FirstOrDefault(), target, heroes); break;
                }
            }
            else if(losHeroes.Any())
            {
                switch (roll)
                {
                    case <= 4:
                        var spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.Ranged);
                        if (spellChoice != null)
                        {
                            MonsterSpell spell = spellChoice.FirstOrDefault().Key;
                            GridPosition targetPosition = spellChoice.FirstOrDefault().Value;
                            spell.CastSpell(monster, targetPosition, _dungeon);
                        }
                        break;
                    default:
                        spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.Support);
                        if (spellChoice != null)
                        {
                            MonsterSpell spell = spellChoice.FirstOrDefault().Key;
                            GridPosition targetPosition = spellChoice.FirstOrDefault().Value;
                            spell.CastSpell(monster, targetPosition, _dungeon);
                        }
                        break;
                }
            }
            else if(!losHeroes.Any())
            {
                switch (roll)
                {
                    case <= 4:
                        Hero? closestHero = ChooseTarget(monster, heroes);
                        if (closestHero != null) MoveToGetLineOfSight(monster, closestHero); break;
                    default:
                        var spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.Support);
                        if (spellChoice != null)
                        {
                            MonsterSpell spell = spellChoice.FirstOrDefault().Key;
                            GridPosition targetPosition = spellChoice.FirstOrDefault().Value;
                            spell.CastSpell(monster, targetPosition, _dungeon);
                        }
                        break;
                }
            }
            else
            {
                EnterParryStance(monster);
            }
        }

        private void HandleAdjacentMeleeAttack(Monster monster, Weapon? weapon, Hero target, List<Hero> heroes)
        {                
            bool isWounded = (monster.CurrentHP <= monster.MaxHP / 2);
            int actionRoll = RandomHelper.RollDie("D6");
            if (actionRoll <= 4)
            {
                int attackTypeRoll = RandomHelper.RollDie("D6");
                switch (monster.Behavior)
                {
                    case MonsterBehaviorType.HumanoidMelee:
                        switch (attackTypeRoll)
                        {
                            case 1: EnterParryStance(monster); return;
                            case <= 5: MonsterCombatService.PerformStandardAttack(monster, weapon, target); break;
                            default:
                                if (isWounded) EnterParryStance(monster);
                                else MonsterCombatService.PerformPowerAttack(monster, weapon, target); break;
                        }
                        break;
                    case MonsterBehaviorType.Beast:
                        switch (attackTypeRoll)
                        {
                            case <= 4:
                                if (isWounded) MonsterCombatService.PerformStandardAttack(monster, weapon, target);
                                else MonsterCombatService.PerformPowerAttack(monster, weapon, target); break;
                            default: MonsterCombatService.PerformStandardAttack(monster, weapon, target); break;
                        }
                        break;
                    case MonsterBehaviorType.LowerUndead:
                        switch (attackTypeRoll)
                        {
                            case <= 4:
                                MonsterCombatService.PerformStandardAttack(monster, weapon, target); break;
                            default: MonsterCombatService.PerformPowerAttack(monster, weapon, target); break;
                        }
                        break;
                    case MonsterBehaviorType.HigherUndead:
                        switch (attackTypeRoll)
                        {
                            case <= 2: MonsterCombatService.PerformPowerAttack(monster, weapon, target); break;
                            default: MonsterCombatService.PerformStandardAttack(monster, weapon, target); break;
                        }
                        break;
                }
            }
            else
            {
                List<SpecialActiveAbility> specialAttacks = _monsterSpecial.GetSpecialAttacks(monster.SpecialRules);
                if (specialAttacks.Count > 0)
                {
                    specialAttacks.Shuffle();
                    _monsterSpecial.ExecuteSpecialAbility(monster, heroes, target, specialAttacks[0]);
                }
                else
                {
                    MonsterCombatService.PerformStandardAttack(monster, weapon, target);
                }
            }
        }

        private void EnterParryStance(Monster monster)
        {
            monster.CombatStance = CombatStance.Parry;
            monster.CurrentAP = 0;
            Console.WriteLine($"{monster.Name} entered parry stance");
        }

        private void EndTurnFacing(Monster monster, List<Hero> heroes)
        {
            if (!heroes.Any(h => h.CurrentHP > 0)) return;

            var facingScores = new Dictionary<FacingDirection, int>
            {
                { FacingDirection.North, 0 },
                { FacingDirection.South, 0 },
                { FacingDirection.East, 0 },
                { FacingDirection.West, 0 }
            };

            var aliveHeroes = heroes.Where(h => h.CurrentHP > 0).ToList();

            foreach (FacingDirection potentialFacing in Enum.GetValues(typeof(FacingDirection)))
            {
                int currentScore = 0;
                foreach (var hero in aliveHeroes)
                {
                    var relativeDir = DirectionService.GetRelativeDirection(potentialFacing, monster.Position, hero.Position);

                    int threat = relativeDir switch
                    {
                        RelativeDirection.Back => 10,
                        RelativeDirection.BackLeft or RelativeDirection.BackRight => 5,

                        RelativeDirection.Left or RelativeDirection.Right => 2,

                        _ => 0
                    };

                    if (GridService.GetDistance(monster.Position, hero.Position) <= 1)
                    {
                        threat *= 2;
                    }

                    currentScore += threat;
                }
                facingScores[potentialFacing] = currentScore;
            }

            FacingDirection bestFacing = facingScores.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;

            monster.Facing = bestFacing;

            Console.WriteLine($"{monster.Name} ends their turn facing {monster.Facing.ToString()}");
        }

        private bool MoveTowards(Monster monster, Hero target)
        {
            if (monster.Position == null || target.Position == null || monster.CurrentAP < 1)
            {
                return false;
            }

            List<GridPosition> path = GridService.FindShortestPath(monster.Position, target.Position, _dungeon.DungeonGrid);

            if (path == null || path.Count <= 1)
            {
                Console.WriteLine($"{monster.Name} has no valid path to {target.Name}.");
                return false;
            }

            int stepsToTake = Math.Min(monster.Move, path.Count() - 1);

            GridPosition finalDestination = path[stepsToTake];
            GridService.MoveCharacter(monster, finalDestination, _dungeon.DungeonGrid);

            Console.WriteLine($"{monster.Name} moves {stepsToTake} squares towards {target.Name}.");

            monster.CurrentAP--;
            return true;
        }

        private bool MoveAwayFrom(Monster monster, Hero target)
        {
            var allReachableSquares = GridService.GetAllWalkableSquares(monster.Room, monster, _dungeon.DungeonGrid);

            if (!allReachableSquares.Any())
            {
                Console.WriteLine($"{monster.Name} is trapped and cannot move away!");
                return false;
            }

            var idealSquares = allReachableSquares
                .Where(pos => GridService.HasLineOfSight(pos, target.Position, _dungeon.DungeonGrid).CanShoot)
                .OrderByDescending(pos => GridService.GetDistance(pos, target.Position))
                .ToList();

            GridPosition bestRetreatSpot;

            if (idealSquares.Any())
            {
                bestRetreatSpot = idealSquares.FirstOrDefault();
            }
            else
            {
                bestRetreatSpot = allReachableSquares
                    .OrderByDescending(pos => GridService.GetDistance(pos, target.Position))
                    .FirstOrDefault();
            }

            var pathToRetreat = GridService.FindShortestPath(monster.Position, bestRetreatSpot, _dungeon.DungeonGrid);

            if (pathToRetreat.Any())
            {
                int stepsToTake = Math.Min(monster.Move, pathToRetreat.Count() - 1);
                GridPosition finalDestination = pathToRetreat[stepsToTake];

                GridService.MoveCharacter(monster, finalDestination, _dungeon.DungeonGrid);
                Console.WriteLine($"{monster.Name} retreats to {finalDestination} to keep its distance from {target.Name}.");
            }

            monster.CurrentAP--;
            return true;
        }

        private void MoveToGetLineOfSight(Monster monster, Hero target)
        {
            var allReachableSquares = GridService.GetAllWalkableSquares(monster.Room, monster, _dungeon.DungeonGrid);

            var squaresWithLOS = allReachableSquares
                .Where(pos => GridService.HasLineOfSight(pos, target.Position, _dungeon.DungeonGrid).CanShoot)
                .ToList();

            if (!squaresWithLOS.Any())
            {
                MoveTowards(monster, target);
                return;
            }

            var vantagePoints = squaresWithLOS
                .Where(pos => {
                    var square = GridService.GetSquareAt(pos, _dungeon.DungeonGrid);
                    return square?.Furniture != null && (square.Furniture.CanBeClimbed || square.Furniture.HeightAdvantage);
                })
                .ToList();

            GridPosition bestSpot;
            if (vantagePoints.Any())
            {
                bestSpot = vantagePoints
                    .OrderBy(pos => GridService.FindShortestPath(monster.Position, pos, _dungeon.DungeonGrid).Count)
                    .FirstOrDefault();
                Console.WriteLine($"{monster.Name} moves to a high position for a better shot!");
            }
            else
            {
                bestSpot = squaresWithLOS
                    .OrderBy(pos => GridService.FindShortestPath(monster.Position, pos, _dungeon.DungeonGrid).Count)
                    .FirstOrDefault();
            }

            var path = GridService.FindShortestPath(monster.Position, bestSpot, _dungeon.DungeonGrid);
            GridService.MoveCharacter(monster, bestSpot, _dungeon.DungeonGrid);
            monster.CurrentAP--;
        }

        private Hero? ChooseTarget(Monster monster, List<Hero> heroes, bool isRanged = false)
        {
            if (!heroes.Any(h => h.CurrentHP > 0) || monster.Position == null)
            {
                return null;
            }

            var targetableHeroes = heroes.Where(h => h.CurrentHP > 0 && h.Position != null).ToList();
            if (!targetableHeroes.Any()) return null;

            // --- Prioritize Untargeted Adjacent Heroes ---
            // "If the enemy has a choice between adjacent targets, target one that has not been targeted by another enemy."
            var adjacentHeroes = targetableHeroes
                .Where(h => GridService.GetDistance(monster.Position, h.Position!) <= 1)
                .ToList();

            if (adjacentHeroes.Any())
            {
                List<Hero> untargetedAdjacent = adjacentHeroes.Where(h => h.HasBeenTargetedThisTurn).ToList();
                if (untargetedAdjacent.Any())
                {
                    // If there's an untargeted hero adjacent, pick one randomly.
                    untargetedAdjacent.Shuffle();
                    return untargetedAdjacent[0];
                }
                // If all adjacent heroes have been targeted, fall through to default logic for the closest one.
            }

            // --- Behavior-Specific Targeting Logic ---
            // If no priority target is found, use the monster's AI behavior.
            switch (monster.Behavior)
            {
                case MonsterBehaviorType.HumanoidRanged:
                    // "Target: 1-4: closest enemy, 5-6: easiest to hit (Lowest HP first)"
                    int roll = RandomHelper.RollDie("D6");
                    if (roll <= 4)
                    {
                        return targetableHeroes.OrderBy(h => GridService.GetDistance(monster.Position, h.Position!)).FirstOrDefault();
                    }
                    else
                    {
                        return targetableHeroes
                        .OrderByDescending(h => MonsterCombatService.CalculateHitChanceModifier(monster, h)) // Highest modifier is easiest
                        .ThenBy(h => h.CurrentHP) // Then by lowest HP
                        .FirstOrDefault();
                    }

                case MonsterBehaviorType.MagicUser:
                    // "1-3: Closest Hero. 4-5: Least remaining hit points, 6: Opposing Magic User"
                    int magicRoll = RandomHelper.RollDie("D6");
                    if (magicRoll <= 3)
                    {
                        return targetableHeroes.OrderBy(h => GridService.GetDistance(monster.Position, h.Position!)).FirstOrDefault();
                    }
                    else if (magicRoll <= 5)
                    {
                        return targetableHeroes.OrderBy(h => h.CurrentHP).FirstOrDefault();
                    }
                    else
                    {
                        // Find a hero who is a magic user (e.g., has Arcane Arts skill > 0)
                        var magicHero = targetableHeroes.FirstOrDefault(h => h.ArcaneArtsSkill > 0);
                        // If a magic hero exists, target them. Otherwise, fall back to the closest.
                        return magicHero ?? targetableHeroes.OrderBy(h => GridService.GetDistance(monster.Position, h.Position!)).FirstOrDefault();
                    }

                // Default for Beast, HumanoidMelee, Undead, etc.
                default:
                    // The default behavior is to target the closest hero.
                    return targetableHeroes
                        .OrderBy(h => GridService.GetDistance(monster.Position, h.Position!))
                        .FirstOrDefault();
            }
        }

        /// <summary>
        /// Evaluates all available spells and returns the best one to cast and the target location to cast it.
        /// </summary>
        public Dictionary<MonsterSpell, GridPosition>? ChooseBestSpellAndTarget(Monster caster, List<Hero> heroes, List<Monster> allies, MonsterSpellType spellType)
        {
            var choices = new List<SpellChoice>();
            var adjacentHeroes = heroes.Where(h => GridService.GetDistance(caster.Position, h.Position) <= 1).ToList();
            var adjacentAllies = _dungeon.RevealedMonsters.Where(h => GridService.GetDistance(caster.Position, h.Position) <= 1).ToList();
            var losHeroes = heroes.Where(h => GridService.HasLineOfSight(caster.Position, h.Position, _dungeon.DungeonGrid).CanShoot).ToList();

            foreach (var spell in caster.Spells.Where(s => s.Type == spellType))
            {
                // Use the hint to determine the best target and score for this spell.
                switch (spell.AITargetingHint)
                {
                    // Use the hint to determine how to score this spell
                    // --- OFFENSIVE SPELLS ---
                    case AiTargetHints.MaximizeHeroTargets:
                    // This requires a helper that returns the best position and the number of targets hit.
                    (GridPosition? bestPos, double targetsHit) = EvaluateAoESpell(caster, heroes, spell);
                    if (targetsHit > 0)
                    {
                        // The target is the square itself, not a specific character.
                        choices.Add(new SpellChoice { Spell = spell, Target = bestPos, Score = targetsHit * 10});
                    }
                    break;

                    case AiTargetHints.TargetHighestCombatSkillHero:
                        var strongestHero = losHeroes.OrderByDescending(h => h.CombatSkill).FirstOrDefault();
                        if (strongestHero != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = strongestHero.Position, Score = 15 + (strongestHero.CombatSkill / 10.0) });
                        }
                        break;

                    case AiTargetHints.TargetAdjacentHero:
                        var adjacentHero = adjacentHeroes
                            .OrderBy(h => h.CurrentHP) // Prioritize weaker adjacent heroes
                           .FirstOrDefault();
                        if (adjacentHero != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = adjacentHero.Position, Score = 18 });
                        }
                        break;

                    case AiTargetHints.TargetRandomHero: // AI interprets "Random" as "General Purpose Attack"
                        var weakestHero = losHeroes.OrderBy(h => h.CurrentHP).FirstOrDefault();
                        if (weakestHero != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = weakestHero.Position, Score = 10 }); // Baseline score for a standard attack
                        }
                        break;

                    // --- SUPPORT & HEALING SPELLS ---
                    case AiTargetHints.HealLowestHealthAlly:
                        var mostWoundedAlly = allies.OrderBy(a => (double)a.CurrentHP / a.MaxHP).FirstOrDefault();
                        if (mostWoundedAlly != null && mostWoundedAlly.CurrentHP < mostWoundedAlly.MaxHP)
                        {
                            double missingHealthPercent = 1.0 - ((double)mostWoundedAlly.CurrentHP / mostWoundedAlly.MaxHP);
                            choices.Add(new SpellChoice { Spell = spell, Target = mostWoundedAlly.Position, Score = 15 + (missingHealthPercent * 20) });
                        }
                        break;

                    case AiTargetHints.HealLowestHealthAdjacentAlly:
                        var mostWoundedAdjacent = adjacentAllies.OrderBy(a => (double)a.CurrentHP / a.MaxHP).FirstOrDefault();
                        if (mostWoundedAdjacent != null && mostWoundedAdjacent.CurrentHP < mostWoundedAdjacent.MaxHP)
                        {
                            double missingHealthPercent = 1.0 - ((double)mostWoundedAdjacent.CurrentHP / mostWoundedAdjacent.MaxHP);
                            choices.Add(new SpellChoice { Spell = spell, Target = mostWoundedAdjacent.Position, Score = 16 + (missingHealthPercent * 20) });
                        }
                        break;

                    case AiTargetHints.ResurrectOrHealUndeadAllies:
                        // This assumes a method to get fallen allies exists.
                        var undeadAllies = _dungeon.RevealedMonsters.Where(m => m.Type == EncounterType.Undead);
                        var fallenUndead = undeadAllies.Where(m => m.CurrentHP <= 0);
                        if (fallenUndead.Any())
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = null, Score = 30 }); // Resurrection is top priority
                        }
                        else
                        {
                            var woundedUndead = undeadAllies.Where(a => a.CurrentHP < a.MaxHP).OrderBy(a => a.MaxHP - a.CurrentHP);
                            if (woundedUndead != null)
                            {
                                choices.Add(new SpellChoice { Spell = spell, Target = woundedUndead.FirstOrDefault().Position, Score = 15 });
                            }
                        }
                        break;

                    // --- BUFF & DEBUFF SPELLS ---
                    case AiTargetHints.BuffHighestCombatSkillAlly:
                        var allyToBuff = allies.Where(a => !a.ActiveStatusEffects.Contains(StatusEffectService.GetStatusEffectByType(StatusEffectType.Frenzy)))
                                               .OrderByDescending(a => a.CombatSkill).FirstOrDefault();
                        if (allyToBuff != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = allyToBuff.Position, Score = 12 });
                        }
                        break;

                    case AiTargetHints.BuffLowestArmourAlly:
                        var allyToShield = allies.Where(a => !a.ActiveStatusEffects.Contains(StatusEffectService.GetStatusEffectByType(StatusEffectType.Shield)))
                                                 .OrderBy(a => a.ArmourValue).FirstOrDefault();
                        if (allyToShield != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = allyToShield.Position, Score = 11 });
                        }
                        break;

                    case AiTargetHints.DebuffEnemyCaster:
                        var enemyCaster = losHeroes.FirstOrDefault(h => h.ProfessionName == "Wizard" || h.ProfessionName == "Warrior Priest");
                        if (enemyCaster != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = enemyCaster.Position, Score = 16 });
                        }
                        break;

                    case AiTargetHints.DebuffHeroRanged:
                        int rangedHeroCount = losHeroes.Count(h => h.Weapons.Any(w => w is RangedWeapon));
                        if (rangedHeroCount > 0)
                        {
                            // No specific target, but it's a valuable spell.
                            choices.Add(new SpellChoice { Spell = spell, Target = null, Score = 8 * rangedHeroCount });
                        }
                        break;

                    // --- UTILITY & SELF SPELLS ---
                    case AiTargetHints.SelfPreservation:
                        bool isLowHealth = (double)caster.CurrentHP / caster.MaxHP < 0.4;
                        if (isLowHealth)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = caster.Position, Score = 25 }); // High priority to escape danger
                        }
                        break;

                    case AiTargetHints.SummonReinforcements:
                        // No specific target, just needs a valid location. The high score reflects its power.
                        choices.Add(new SpellChoice { Spell = spell, Target = null, Score = 22 });
                        break;
                }
            }

            if (!choices.Any())
            {
                return null; // No valid action
            }

            // Find the best choice from our list.
            var bestChoice = choices.OrderByDescending(c => c.Score).FirstOrDefault();

            if (bestChoice.Spell != null && bestChoice.Target != null) return new Dictionary<MonsterSpell, GridPosition> { { bestChoice.Spell, bestChoice.Target } };
            return null; // No valid action
        }

        /// <summary>
        /// Evaluates the best GridPosition to cast an AoE spell to maximize heroes hit.
        /// </summary>
        /// <param name="caster">The monster casting the spell.</param>
        /// <param name="heroes">A list of all active heroes on the map.</param>
        /// <param name="spell">The AoE spell being evaluated (e.g., Fireball).</param>
        /// <returns>The GridPosition that maximizes heroes hit, or null if no valid target found.</returns>
        private (GridPosition?, double) EvaluateAoESpell(Monster caster, List<Hero> heroes, MonsterSpell spell)
        {
            GridPosition? bestAoECenter = null;
            double maxHeroesHit = -1; // Initialize to -1 to ensure any valid hit count is better

            // Collect all potential target squares.
            HashSet<GridPosition> potentialCenterSquares = new HashSet<GridPosition>();
            foreach (var hero in heroes)
            {
                potentialCenterSquares.Add(hero.Position);
                foreach (var occupiedSquare in hero.OccupiedSquares)
                {
                    potentialCenterSquares.Add(occupiedSquare);
                }
                // Also consider squares around heroes that might be good centers
                potentialCenterSquares.UnionWith(GridService.GetNeighbors(hero.Position, _dungeon.DungeonGrid));
            }


            foreach (var currentCenter in potentialCenterSquares)
            {
                // It should return all GridPositions within the specified radius from the currentCenter.
                // Radius 0: only currentCenter
                // Radius 1: currentCenter + all 8 (or 4) adjacent squares
                // Radius N: currentCenter + all squares within N steps
                List<GridPosition> currentAoESquares = GridService.GetAllSquaresInRadius(currentCenter, spell.AreaOfEffectRadius, _dungeon.DungeonGrid);

                int currentHeroesHit = 0;
                // Use a HashSet to ensure each hero is counted only once for the current AoE
                HashSet<Hero> heroesAlreadyCountedInThisAoE = new HashSet<Hero>();

                foreach (var hero in heroes)
                {
                    // Check if any of the squares occupied by the hero are within the current spell's AoE
                    if (hero.OccupiedSquares.Any(heroSquare => currentAoESquares.Contains(heroSquare)))
                    {
                        // If the hero hasn't been counted yet for this specific AoE, add them
                        if (heroesAlreadyCountedInThisAoE.Add(hero))
                        {
                            currentHeroesHit++;
                        }
                    }
                }

                // Update the best target if this center hits more heroes
                if (currentHeroesHit > maxHeroesHit)
                {
                    maxHeroesHit = currentHeroesHit;
                    bestAoECenter = currentCenter;
                }
            }

            return (bestAoECenter, maxHeroesHit);
        }
    }
}
