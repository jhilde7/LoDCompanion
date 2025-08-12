using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Game
{
    public class MonsterAIService
    {
        private readonly MonsterSpecialService _monsterSpecial;
        private readonly DungeonState _dungeon;
        private readonly AttackService _attack;
        private readonly ActionService _action;
        private readonly SpellResolutionService _spell;

        public MonsterAIService(
            MonsterSpecialService monsterSpecialService,
            DungeonState dungeon,
            AttackService attackService,
            ActionService actionService,
            SpellResolutionService spell)
        {
            _monsterSpecial = monsterSpecialService;
            _dungeon = dungeon;
            _attack = attackService;
            _action = actionService;
            _spell = spell;
        }

        /// <summary>
        /// Executes a monster's turn by repeatedly choosing the best action until AP is depleted.
        /// </summary>
        public async Task<string> ExecuteMonsterTurnAsync(Monster monster, List<Hero> heroes, Room room)
        {
            string actionResult = "";
            while (monster.CurrentAP > 0)
            {
                // Store AP before acting to check if an action was taken.
                int apBeforeAction = monster.CurrentAP;

                // if has Kick then perform during the first action of the monsters turn as a free action
                if (monster.ActiveAbilities != null && monster.CurrentAP >= 2)
                {
                    var kick = monster.ActiveAbilities.FirstOrDefault(s => s == SpecialActiveAbility.Kick);
                    if (kick == SpecialActiveAbility.Kick)
                    {
                        actionResult += await _monsterSpecial.ExecuteSpecialAbilityAsync(monster, heroes, heroes[0], kick, _dungeon);
                    }
                }

                // The main decision-making hub.
                actionResult += "\n" + await DecideAndPerformAction(monster, heroes, room);

                // Failsafe check
                if (monster.CurrentAP == apBeforeAction)
                {
                    actionResult += $"\n{monster.Name} hesitates and ends its turn.";
                    monster.CurrentAP --; // Force end of turn
                }
            }

            // After all actions, ensure the monster faces the best direction.
            actionResult += "\n" + EndTurnFacing(monster, heroes);
            return actionResult;
        }

        /// <summary>
        /// The core AI decision tree that routes to the correct behavior.
        /// </summary>
        private async Task<string> DecideAndPerformAction(Monster monster, List<Hero> heroes, Room room)
        {
            Hero? target = ChooseTarget(monster, heroes);
            if (target == null)
            {
                return $"{monster.Name} scans the room but finds no targets.";
            }

            // Delegate to the appropriate consolidated behavior handler.
            switch (monster.Behavior)
            {
                case MonsterBehaviorType.Beast:
                case MonsterBehaviorType.HigherUndead:
                    return await ExecuteAggressiveMeleeBehaviorAsync(monster, target, heroes);
                case MonsterBehaviorType.HumanoidMelee:
                    return await ExecuteHumanoidMeleeBehaviorAsync(monster, target, heroes);
                case MonsterBehaviorType.HumanoidRanged:
                    return await ExecuteRangedBehaviorAsync(monster, target, heroes);
                case MonsterBehaviorType.MagicUser:
                    return await ExecuteMagicUserBehaviorAsync(monster, target, heroes, room);
                case MonsterBehaviorType.LowerUndead:
                    RangedWeapon? missile = (RangedWeapon?)monster.GetRangedWeapon();
                    if (missile != null) return await ExecuteRangedBehaviorAsync(monster, target, heroes);
                    else return await ExecuteLowerUndeadBehaviorAsync(monster, target, heroes);
                default:
                    return $"{monster.Name} is confused and does nothing.";
            }
        }

        private async Task<string> ExecuteAggressiveMeleeBehaviorAsync(Monster monster, Hero target, List<Hero> heroes)
        {
            if (monster.Position == null || target.Position == null) return string.Empty;
            AttackResult attackResult = new AttackResult();
            int distance = GridService.GetDistance(monster.Position, target.Position);
            monster.ActiveWeapon = monster.GetMeleeWeapon();

            if (!GridService.IsAdjacent(monster.Position, target.Position))
            {
                if (distance < monster.GetStat(BasicStat.Move) && monster.CurrentAP >=2)
                {
                    return await _action.PerformActionAsync(_dungeon, monster, ActionType.ChargeAttack, target);
                }
                else
                {
                    return await MoveTowardsAsync(monster, target, heroes);
                } 
            }
            else
            {
                attackResult = await HandleAdjacentMeleeAttackAsync(monster, target, heroes);
                return attackResult.OutcomeMessage;
            }
        }

        private async Task<string> ExecuteLowerUndeadBehaviorAsync(Monster monster, Hero target, List<Hero> heroes)
        {
            if (monster.Position == null || target.Position == null) return string.Empty;
            AttackResult attackResult = new AttackResult();
            int distance = GridService.GetDistance(monster.Position, target.Position);
            monster.ActiveWeapon = monster.GetMeleeWeapon();

            if (distance >= monster.GetStat(BasicStat.Move))
            {
                return await MoveTowardsAsync(monster, target, heroes);
            }
            else
            {
                attackResult = await HandleAdjacentMeleeAttackAsync(monster, target, heroes);
                return attackResult.OutcomeMessage;
            }
        }

        private async Task<string> ExecuteHumanoidMeleeBehaviorAsync(Monster monster, Hero target, List<Hero> heroes)
        {
            if (monster.Position == null || target.Position == null) return string.Empty;
            AttackResult attackResult = new AttackResult();
            int distance = GridService.GetDistance(monster.Position, target.Position);
            monster.ActiveWeapon = monster.GetMeleeWeapon();

            if (distance > monster.GetStat(BasicStat.Move))
            {
                return await MoveTowardsAsync(monster, target, heroes); 
            }
            else if (!GridService.IsAdjacent(monster.Position, target.Position) && distance <= monster.GetStat(BasicStat.Move)) 
            {
                int roll = RandomHelper.RollDie(DiceType.D6);
                switch (roll)
                {
                    case 1: return await _action.PerformActionAsync(_dungeon, monster, ActionType.Parry);
                    case <= 4: return await MoveTowardsAsync(monster, target, heroes); 
                    default:
                        if (monster.CurrentAP >= 2) 
                        {
                            return await _action.PerformActionAsync(_dungeon, monster, ActionType.ChargeAttack, target);
                            
                        }
                        break;
                        
                }
            }
            else
            {
                attackResult = await HandleAdjacentMeleeAttackAsync(monster, target, heroes);
                return attackResult.OutcomeMessage;
            }
            return $"{monster.Name} hesitates.";
        }

        private async Task<string> ExecuteRangedBehaviorAsync(Monster monster, Hero target, List<Hero> heroes)
        {
            if (monster.Position == null || target.Position == null) return string.Empty;
            AttackResult attackResult = new AttackResult();
            int distance = GridService.GetDistance(monster.Position, target.Position);
            Dictionary<Hero, int> distances = new Dictionary<Hero, int>();
            RangedWeapon? missile = (RangedWeapon?)monster.GetRangedWeapon();
            MeleeWeapon? melee = (MeleeWeapon?)monster.GetMeleeWeapon();

            foreach (Hero hero in heroes)
            {
                if (hero.Position != null)
                {
                    distances.TryAdd(hero, GridService.GetDistance(monster.Position, hero.Position));  
                }
            }

            if (missile != null)
            {
                monster.ActiveWeapon = missile;
                if (distances.Values.FirstOrDefault(i => i <= 2) > 0)
                {
                    target = distances.FirstOrDefault(h => h.Value <= 2).Key;
                    if(await MoveAwayFromAsync(monster, target, heroes))
                    {
                        return $"{monster.Name} retreats to {monster.Position} to keep its distance from {target.Name}";
                    }
                    else
                    {
                        if (target.Position != null && GridService.IsAdjacent(monster.Position, target.Position))
                        {
                            monster.ActiveWeapon = melee;
                            attackResult = await HandleAdjacentMeleeAttackAsync(monster, target, heroes);
                            return attackResult.OutcomeMessage;
                        }
                        else
                        {
                            return await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target);
                        }
                    }
                }

                if(!GridService.HasLineOfSight(monster.Position, target.Position, _dungeon.DungeonGrid).CanShoot)
                {
                    return await MoveToGetLineOfSightAsync(monster, target, heroes);
                }
                else if(GridService.IsAdjacent(monster.Position, target.Position))
                {
                    monster.ActiveWeapon = melee;
                    attackResult = await HandleAdjacentMeleeAttackAsync(monster, target, heroes);
                    return attackResult.OutcomeMessage;
                }
                else
                {
                    int actionRoll = RandomHelper.RollDie(DiceType.D6);
                    if (actionRoll <= 4)
                    {
                        int attackTypeRoll = RandomHelper.RollDie(DiceType.D6);
                        switch (attackTypeRoll)
                        {
                            case <= 2:
                                if (monster.CombatStance != CombatStance.Aiming)
                                {
                                    return await _action.PerformActionAsync(_dungeon, monster, ActionType.Aim);
                                }
                                else
                                {
                                    return await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttackWhileAiming, target);
                                }
                            default:
                                if (monster.CombatStance == CombatStance.Aiming)
                                {
                                    return await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttackWhileAiming, target);
                                }
                                else
                                {
                                    return await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target);
                                }
                        }
                    }
                    else
                    {
                        List<SpecialActiveAbility>? specialAttacks = monster.ActiveAbilities;
                        string? specialAbilityMessage = null;
                        if (specialAttacks != null && specialAttacks.Count > 0)
                        {
                            specialAbilityMessage = await AttemptSpecialAbility(monster, heroes, target, specialAttacks);

                            if (specialAbilityMessage != null)
                            {
                                return specialAbilityMessage;
                            }
                        }

                        if (monster.CombatStance == CombatStance.Aiming)
                        {
                            return await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttackWhileAiming, target);
                        }
                        else
                        {
                            return await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target);
                        }
                    }
                }
            }
            else
            {
                 return await ExecuteHumanoidMeleeBehaviorAsync(monster, target, heroes);
            }
        }

        private async Task<string> ExecuteMagicUserBehaviorAsync(Monster monster, Hero? target, List<Hero> heroes, Room room)
        {
            if (monster.Position == null || target == null) return string.Empty;
            AttackResult attackResult = new AttackResult();
            var adjacentHeroes = heroes.Where(h => h.Position != null && target.Position != null && GridService.IsAdjacent(monster.Position, target.Position)).ToList();
            var losHeroes = heroes.Where(h => h.Position != null && GridService.HasLineOfSight(monster.Position, h.Position, _dungeon.DungeonGrid).CanShoot).ToList();
            monster.ActiveWeapon = monster.GetMeleeWeapon();

            int roll = RandomHelper.RollDie(DiceType.D6);
            Dictionary<MonsterSpell, GridPosition>? spellChoice = null;

            if (adjacentHeroes.Any())
            {
                target = ChooseTarget(monster, adjacentHeroes); // Choose from adjacent heroes
                if (target == null) { return $"{monster.Name} hesitates."; }

                switch (roll)
                {
                    case <= 2: 
                        if(await MoveAwayFromAsync(monster, target, heroes))
                        {
                            return $"{monster.Name} retreats to {monster.Position} to keep its distance from {target.Name}."; ;
                        }
                        else
                        {
                            if (target.Position != null && GridService.IsAdjacent(monster.Position, target.Position))
                            {
                                attackResult = await HandleAdjacentMeleeAttackAsync(monster, target, heroes);
                                return attackResult.OutcomeMessage;
                            }
                            else
                            {
                                return $"{monster.Name} hesitates.";
                            }
                        }                        
                    case <= 4:
                        spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.CloseCombat);
                        break;
                    default: 
                        attackResult = await HandleAdjacentMeleeAttackAsync(monster, target, heroes);
                        return attackResult.OutcomeMessage;
                }
            }
            else if(losHeroes.Any())
            {
                switch (roll)
                {
                    case <= 4:
                        spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.Ranged);
                        break;
                    default:
                        spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.Support);
                        break;
                }
            }
            else if(!losHeroes.Any())
            {
                switch (roll)
                {
                    case <= 4:
                        Hero? closestHero = ChooseTarget(monster, heroes);
                        if (closestHero != null) return await MoveToGetLineOfSightAsync(monster, closestHero, heroes);
                        return $"{monster.Name} considers its next move."; 
                    default:
                        spellChoice = ChooseBestSpellAndTarget(monster, heroes, _dungeon.RevealedMonsters, MonsterSpellType.Support);
                        break;
                }
            }

            if (spellChoice != null && spellChoice.Any())
            {
                MonsterSpell spell = spellChoice.First().Key;
                GridPosition targetPosition = spellChoice.First().Value;
                var result = _spell.ResolveMonsterSpell(monster, spell, targetPosition);
                return result.OutcomeMessage;
            }

            // Fallback if no other action was taken (e.g., no valid spell target was found)
            if (monster.CurrentAP >= 1)
            {
                return await _action.PerformActionAsync(_dungeon, monster, ActionType.Parry);
            }

            return $"{monster.Name} hesitates.";
        }

        private async Task<AttackResult> HandleAdjacentMeleeAttackAsync(Monster monster, Hero target, List<Hero> heroes)
        {                
            bool isWounded = monster.CurrentHP <= monster.GetStat(BasicStat.HitPoints) / 2;
            int actionRoll = RandomHelper.RollDie(DiceType.D6);
            if (actionRoll <= 4)
            {
                int attackTypeRoll = RandomHelper.RollDie(DiceType.D6);
                switch (monster.Behavior)
                {
                    case MonsterBehaviorType.HumanoidMelee:
                        switch (attackTypeRoll)
                        {
                            case 1: return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.Parry) };
                            case <= 5: return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target) };
                            default:
                                if (isWounded) return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.Parry) };
                                else return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.PowerAttack, target) };
                        }
                    case MonsterBehaviorType.Beast:
                        switch (attackTypeRoll)
                        {
                            case <= 4:
                                if (isWounded) return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target) };
                                else return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.PowerAttack, target) };
                            default: return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target) };
                        }
                    case MonsterBehaviorType.LowerUndead:
                        switch (attackTypeRoll)
                        {
                            case <= 4:
                                return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target) };
                            default: return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.PowerAttack, target) };
                        }
                    case MonsterBehaviorType.HigherUndead:
                        switch (attackTypeRoll)
                        {
                            case <= 2: return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.PowerAttack, target) };
                            default: return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target) };
                        }
                    default: return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target) };
                }
            }
            else
            {
                List<SpecialActiveAbility>? specialAttacks = monster.ActiveAbilities;
                string? specialAbilityMessage = null;
                if (specialAttacks != null && specialAttacks.Count > 0)
                {
                    specialAbilityMessage = await AttemptSpecialAbility(monster, heroes, target, specialAttacks);

                    if (specialAbilityMessage != null)
                    {
                        return new AttackResult()
                        {
                            OutcomeMessage = specialAbilityMessage
                        };
                    }
                }
                // If no special ability was used, perform a standard attack.
                return new AttackResult() { OutcomeMessage = await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, target) };
            }
        }

        private string EndTurnFacing(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return string.Empty;
            var aliveHeroes = heroes.Where(h => h.CurrentHP > 0).ToList();
            if (!aliveHeroes.Any())
            {
                return $"{monster.Name} stands triumphantly.";
            }

            var facingScores = new Dictionary<FacingDirection, int>();

            foreach (FacingDirection potentialFacing in Enum.GetValues(typeof(FacingDirection)))
            {
                int totalThreatScore = 0;
                foreach (var hero in aliveHeroes)
                {
                    if (hero.Position == null) continue;
                    RelativeDirection relativeDir = DirectionService.GetRelativeDirection(potentialFacing, monster.Position, hero.Position);
                    bool isAdjacent = GridService.IsAdjacent(monster.Position, hero.Position);

                    int threat = 0;
                    if (isAdjacent)
                    {
                        threat = relativeDir switch
                        {
                            RelativeDirection.Front => 100,
                            RelativeDirection.FrontLeft or RelativeDirection.FrontRight => 50,
                            RelativeDirection.Left or RelativeDirection.Right => 20,
                            _ => 0,
                        };
                    }
                    else
                    {
                        threat = relativeDir switch
                        {
                            RelativeDirection.Front => 10,
                            RelativeDirection.FrontLeft or RelativeDirection.FrontRight => 5,
                            RelativeDirection.Left or RelativeDirection.Right => 2,
                            _ => 0,
                        };
                    }

                    totalThreatScore += threat;
                }
                facingScores[potentialFacing] = totalThreatScore;
            }

            // Debug: Output threat scores for each direction (optional)
            Console.WriteLine($"{monster.Name} current position: {monster.Position}");
            foreach (var kvp in facingScores) Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            foreach (var hero in aliveHeroes)
            {
                Console.WriteLine($"{hero.Name} at {hero.Position}");
            }

            // Order the directions by their threat score, from highest to lowest.
            var orderedFacings = facingScores.OrderByDescending(kvp => kvp.Value).ToList();

            var topScore = orderedFacings.First().Value;
            var bestDirections = orderedFacings.Where(kvp => kvp.Value == topScore).ToList();

            FacingDirection finalFacing;

            // Analyze the best directions to handle ties.
            if (bestDirections.Count > 1)
            {
                var firstBest = bestDirections[0].Key;
                var secondBest = bestDirections[1].Key;

                // Check if the tie is between two opposite directions.
                if (DirectionService.GetOpposite(firstBest) == secondBest)
                {
                    // The monster is caught between two equal threats.
                    // Find the next best direction that is NOT one of the opposites.
                    var nextBestOption = orderedFacings.FirstOrDefault(kvp => kvp.Key != firstBest && kvp.Key != secondBest);

                    finalFacing = nextBestOption.Key;
                    Console.WriteLine($"Opposite tie detected! Choosing next best: {finalFacing}");
                }
                else
                {
                    // It's a tie, but not between opposites (e.g., North and East).
                    // Pick one randomly from the tied top scores.
                    finalFacing = bestDirections[RandomHelper.GetRandomNumber(0, bestDirections.Count - 1)].Key;
                }
            }
            else
            {
                finalFacing = bestDirections.First().Key;
            }

            monster.Facing = finalFacing;

            return $"{monster.Name} ends their turn facing {monster.Facing}";
        }

        private async Task<string> MoveTowardsAsync(Monster monster, Hero target, List<Hero> heroes)
        {
            if (monster.Position == null || target.Position == null || monster.CurrentAP < 1)
            {
                return $"{monster.Name} hesitates."; ;
            }

            List<GridPosition> path = GridService.FindShortestPath(monster.Position, target.Position, _dungeon.DungeonGrid, heroes.Cast<Character>().ToList());

            if (path == null || path.Count <= 1)
            {                
                return $"{monster.Name} has no valid path to {target.Name}.";
            }

            int stepsToTake = Math.Min(monster.GetStat(BasicStat.Move), path.Count() - 1);

            GridPosition finalDestination = path[stepsToTake];
            return await _action.PerformActionAsync(_dungeon, monster, ActionType.Move, finalDestination);
        }

        private async Task<bool> MoveAwayFromAsync(Monster monster, Hero target, List<Hero> heroes)
        {
            if (monster.Position == null || target.Position == null) return false;
            var allReachableSquares = GridService.GetAllWalkableSquares(monster, _dungeon.DungeonGrid, heroes.Cast<Character>().ToList());

            if (!allReachableSquares.Any())
            {
                return false;
            }

            var idealSquares = allReachableSquares.Keys
                .Where(pos => GridService.HasLineOfSight(pos, target.Position, _dungeon.DungeonGrid).CanShoot)
                .OrderByDescending(pos => GridService.GetDistance(pos, target.Position))
                .ToList();

            GridPosition? bestRetreatSpot;

            if (idealSquares.Any())
            {
                bestRetreatSpot = idealSquares.FirstOrDefault();
            }
            else
            {
                bestRetreatSpot = allReachableSquares.Keys
                    .OrderByDescending(pos => GridService.GetDistance(pos, target.Position))
                    .FirstOrDefault();
            }

            if(bestRetreatSpot != null)
            {
                var pathToRetreat = GridService.FindShortestPath(monster.Position, bestRetreatSpot, _dungeon.DungeonGrid, heroes.Cast<Character>().ToList());

                if (pathToRetreat.Any())
                {
                    int stepsToTake = Math.Min(monster.GetStat(BasicStat.Move), pathToRetreat.Count() - 1);
                    GridPosition finalDestination = pathToRetreat[stepsToTake];

                    await _action.PerformActionAsync(_dungeon, monster, ActionType.Move, finalDestination);
                }
            }
            return true;
        }

        private async Task<string> MoveToGetLineOfSightAsync(Monster monster, Hero target, List<Hero> heroes)
        {
            if (monster.Position == null || target.Position == null) return string.Empty;
            var allReachableSquares = GridService.GetAllWalkableSquares(monster, _dungeon.DungeonGrid, heroes.Cast<Character>().ToList());

            var squaresWithLOS = allReachableSquares.Keys
                .Where(pos => GridService.HasLineOfSight(pos, target.Position, _dungeon.DungeonGrid).CanShoot)
                .ToList();

            if (!squaresWithLOS.Any())
            {
                return await MoveTowardsAsync(monster, target, heroes);
            }

            var vantagePoints = squaresWithLOS
                .Where(pos => {
                    var square = GridService.GetSquareAt(pos, _dungeon.DungeonGrid);
                    return square?.Furniture != null && (square.Furniture.CanBeClimbed || square.Furniture.HeightAdvantage);
                })
                .ToList();

            GridPosition? bestSpot;
            if (vantagePoints.Any())
            {
                bestSpot = vantagePoints
                    .OrderBy(pos => GridService.FindShortestPath(monster.Position, pos, _dungeon.DungeonGrid, heroes.Cast<Character>().ToList()).Count)
                    .FirstOrDefault();
                if (bestSpot != null)
                {
                    return await _action.PerformActionAsync(_dungeon, monster, ActionType.Move, bestSpot);
                }
            }
            else
            {
                bestSpot = squaresWithLOS
                    .OrderBy(pos => GridService.FindShortestPath(monster.Position, pos, _dungeon.DungeonGrid, heroes.Cast<Character>().ToList()).Count)
                    .FirstOrDefault();
                if (bestSpot != null)
                {
                    return await _action.PerformActionAsync(_dungeon, monster, ActionType.Move, bestSpot);                    
                }
            }
            return await MoveTowardsAsync(monster, target, heroes);
        }

        private Hero? ChooseTarget(Monster monster, List<Hero> heroes, int? range = null)
        {
            if (!heroes.Any(h => h.CurrentHP > 0) || monster.Position == null)
            {
                return null;
            }

            var targetableHeroes = heroes.Where(h => h.CurrentHP > 0 && h.Position != null).ToList();
            if (!targetableHeroes.Any()) return null;

            // if range is specified, filter heroes within that range
            if (range.HasValue)
            {
                targetableHeroes = targetableHeroes.Where(h => h.Position != null && GridService.GetDistance(monster.Position, h.Position) == range.Value).ToList();

                if(targetableHeroes.Any())
                {
                    targetableHeroes.Shuffle();
                    return targetableHeroes[0]; // Return a random hero within range
                }
            }

            // --- Prioritize Untargeted Adjacent Heroes ---
            // "If the enemy has a choice between adjacent targets, target one that has not been targeted by another enemy."
            var adjacentHeroes = targetableHeroes
                .Where(h => GridService.IsAdjacent(monster.Position, h.Position!))
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
                    int roll = RandomHelper.RollDie(DiceType.D6);
                    if (roll <= 4)
                    {
                        return targetableHeroes.OrderBy(h => GridService.GetDistance(monster.Position, h.Position!)).FirstOrDefault();
                    }
                    else
                    {
                        return targetableHeroes
                        .OrderByDescending(h => _attack.CalculateHitChanceModifier(monster, monster.GetRangedWeapon(), h, new CombatContext())) // Highest modifier is easiest
                        .ThenBy(h => h.CurrentHP) // Then by lowest HP
                        .FirstOrDefault();
                    }

                case MonsterBehaviorType.MagicUser:
                    // "1-3: Closest Hero. 4-5: Least remaining hit points, 6: Opposing Magic User"
                    int magicRoll = RandomHelper.RollDie(DiceType.D6);
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
                        var magicHero = targetableHeroes.FirstOrDefault(h => h.GetSkill(Skill.ArcaneArts) > 0);
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
            if (caster.Position == null) return null;
            var choices = new List<SpellChoice>();
            var adjacentHeroes = heroes.Where(h => h.Position != null && GridService.IsAdjacent(caster.Position, h.Position)).ToList();
            var adjacentAllies = _dungeon.RevealedMonsters.Where(h => h.Position != null && GridService.IsAdjacent(caster.Position, h.Position)).ToList();
            var losHeroes = heroes.Where(h => h.Position != null && GridService.HasLineOfSight(caster.Position, h.Position, _dungeon.DungeonGrid).CanShoot).ToList();

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
                        var strongestHero = losHeroes.OrderByDescending(h => h.GetSkill(Skill.CombatSkill)).FirstOrDefault();
                        if (strongestHero != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = strongestHero.Position, Score = 15 + strongestHero.GetSkill(Skill.CombatSkill) / 10.0 });
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
                        var mostWoundedAlly = allies.OrderBy(a => (double)a.CurrentHP / a.GetStat(BasicStat.HitPoints)).FirstOrDefault();
                        if (mostWoundedAlly != null && mostWoundedAlly.CurrentHP < mostWoundedAlly.GetStat(BasicStat.HitPoints))
                        {
                            double missingHealthPercent = 1.0 - (double)mostWoundedAlly.CurrentHP / mostWoundedAlly.GetStat(BasicStat.HitPoints);
                            choices.Add(new SpellChoice { Spell = spell, Target = mostWoundedAlly.Position, Score = 15 + missingHealthPercent * 20 });
                        }
                        break;

                    case AiTargetHints.HealLowestHealthAdjacentAlly:
                        var mostWoundedAdjacent = adjacentAllies.OrderBy(a => (double)a.CurrentHP / a.GetStat(BasicStat.HitPoints)).FirstOrDefault();
                        if (mostWoundedAdjacent != null && mostWoundedAdjacent.CurrentHP < mostWoundedAdjacent.GetStat(BasicStat.HitPoints))
                        {
                            double missingHealthPercent = 1.0 - (double)mostWoundedAdjacent.CurrentHP / mostWoundedAdjacent.GetStat(BasicStat.HitPoints);
                            choices.Add(new SpellChoice { Spell = spell, Target = mostWoundedAdjacent.Position, Score = 16 + missingHealthPercent * 20 });
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
                            var woundedUndead = undeadAllies.Where(a => a.CurrentHP < a.GetStat(BasicStat.HitPoints)).OrderBy(a => a.GetStat(BasicStat.HitPoints) - a.CurrentHP);
                            if (woundedUndead != null)
                            {
                                choices.Add(new SpellChoice { Spell = spell, Target = woundedUndead.First().Position, Score = 15 });
                            }
                        }
                        break;

                    // --- BUFF & DEBUFF SPELLS ---
                    case AiTargetHints.BuffHighestCombatSkillAlly:
                        var allyToBuff = allies.Where(a => a.ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.Frenzy) != null)
                                               .OrderByDescending(a => a.GetSkill(Skill.CombatSkill)).FirstOrDefault();
                        if (allyToBuff != null)
                        {
                            choices.Add(new SpellChoice { Spell = spell, Target = allyToBuff.Position, Score = 12 });
                        }
                        break;

                    case AiTargetHints.BuffLowestArmourAlly:
                        var allyToShield = allies.Where(a => a.ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.Shield) != null)
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
                        int rangedHeroCount = losHeroes.Count(h => h.Inventory.EquippedWeapon is RangedWeapon);
                        if (rangedHeroCount > 0)
                        {
                            // No specific target, but it's a valuable spell.
                            choices.Add(new SpellChoice { Spell = spell, Target = null, Score = 8 * rangedHeroCount });
                        }
                        break;

                    // --- UTILITY & SELF SPELLS ---
                    case AiTargetHints.SelfPreservation:
                        bool isLowHealth = (double)caster.CurrentHP / caster.GetStat(BasicStat.HitPoints) < 0.4;
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

            if (bestChoice != null && bestChoice.Spell != null && bestChoice.Target != null) return new Dictionary<MonsterSpell, GridPosition> { { bestChoice.Spell, bestChoice.Target } };
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
                if (hero.Position != null)
                {
                    potentialCenterSquares.Add(hero.Position);
                    foreach (var occupiedSquare in hero.OccupiedSquares)
                    {
                        potentialCenterSquares.Add(occupiedSquare);
                    }
                    // Also consider squares around heroes that might be good centers
                    potentialCenterSquares.UnionWith(GridService.GetNeighbors(hero.Position, _dungeon.DungeonGrid)); 
                }
            }


            foreach (var currentCenter in potentialCenterSquares)
            {
                // It should return all GridPositions within the specified radius from the currentCenter.
                // Radius 0: only currentCenter
                // Radius 1: currentCenter + all 8 (or 4) adjacent squares
                // Radius N: currentCenter + all squares within N steps
                List<GridPosition> currentAoESquares = GridService.GetAllSquaresInRadius(currentCenter, spell.Properties?.GetValueOrDefault(SpellProperty.Radius) ?? 0, _dungeon.DungeonGrid);

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

        /// <summary>
        /// Attempts to use a random special ability if conditions are met. Otherwise, performs a standard or aimed attack.
        /// </summary>
        private async Task<string?> AttemptSpecialAbility(Monster monster, List<Hero> heroes, Hero target, List<SpecialActiveAbility> specialAttacks)
        {
            if(monster.Position == null) return null;
            specialAttacks.Shuffle();

            foreach (var ability in specialAttacks)
            {
                Hero? currentTarget = target; // Use a temporary target for each ability check
                bool isUsable = false;

                switch (ability)
                {
                    case SpecialActiveAbility.PoisonSpit:
                        var availableTargets = heroes.Where(h => h.Position != null && GridService.GetDistance(monster.Position, h.Position) <= 2).ToList();
                        if (availableTargets.Contains(currentTarget) || availableTargets.Any())
                        {
                            if (!availableTargets.Contains(currentTarget))
                            {
                                // If the initial target isn't valid, find a new one
                                currentTarget = ChooseTarget(monster, availableTargets, 2);
                            }

                            if (currentTarget != null)
                            {
                                isUsable = true;
                            }
                        }
                        break;

                    case SpecialActiveAbility.Seduction:
                        var seducibleTargets = heroes.Where(h => !h.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Incapacitated)).ToList();
                        if (seducibleTargets.Any())
                        {
                            currentTarget = ChooseTarget(monster, seducibleTargets);
                            if (currentTarget != null)
                            {
                                isUsable = true;
                            }
                        }
                        break;

                    case SpecialActiveAbility.Swallow:
                        var swallowTargets = heroes.Where(h => h.Position != null && GridService.IsAdjacent(monster.Position, h.Position) && !h.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.BeingSwallowed || e.Category == StatusEffectType.Swallowed)).ToList();
                        if (swallowTargets.Any())
                        {
                            currentTarget = ChooseTarget(monster, swallowTargets);
                            if (currentTarget != null)
                            {
                                isUsable = true;
                            }
                        }
                        break;

                    case SpecialActiveAbility.SweepingStrike:
                        if (heroes.Any(h => h.Position != null && DirectionService.IsInZoneOfControl(h.Position, monster)))
                        {
                            isUsable = true;
                        }
                        break;

                    case SpecialActiveAbility.TongueAttack:
                        var tongueTargets = heroes.Where(h => h.Position != null && GridService.GetDistance(monster.Position, h.Position) == 2).ToList();
                        if (tongueTargets.Any())
                        {
                            currentTarget = ChooseTarget(monster, tongueTargets, 2);
                            if (currentTarget != null)
                            {
                                isUsable = true;
                            }
                        }
                        break;

                    // Default case for abilities that don't have special targeting requirements
                    default:
                        isUsable = true;
                        break;
                }

                if (isUsable && currentTarget != null)
                {
                    // Found a usable special ability and target, execute it and return the result.
                    string abilityResult = await _monsterSpecial.ExecuteSpecialAbilityAsync(monster, heroes, currentTarget, ability, _dungeon);

                    if (abilityResult.ToLower().Contains("performs a standard attack"))
                    {
                        // Handle cases where the special ability defaults to a standard attack
                        return abilityResult + await _action.PerformActionAsync(_dungeon, monster, ActionType.StandardAttack, currentTarget);
                    }
                    return abilityResult;
                }
                else
                {
                    // If the ability is not usable, continue to the next one.
                    continue;
                }
            }
            return null;
        }
    }
}
