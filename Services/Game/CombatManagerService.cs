using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Player;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Utilities;
using System.Threading;

namespace LoDCompanion.Services.Game
{
    public class CombatManagerService
    {
        private readonly GameDataService _gameData;
        private readonly InitiativeService _initiative;
        private readonly HeroCombatService _heroCombat;
        private readonly MonsterCombatService _monsterCombat;
        private readonly PlayerActionService _playerAction;
        private readonly MonsterAIService _monsterAI;
        private readonly DefenseService _defense;
        private readonly StatusEffectService _statusEffect;
        private readonly GridService _grid;

        private List<Hero> HeroesInCombat = new List<Hero>();
        private List<Monster> MonstersInCombat = new List<Monster>();
        private List<Monster> MonstersThatHaveActedThisTurn = new List<Monster>();

        public event Action? OnCombatStateChanged;
        public Hero? ActiveHero { get; private set; }
        private HashSet<string> UnwieldlyBonusUsed = new HashSet<string>();        

        public CombatManagerService(
            InitiativeService initiativeService,
            HeroCombatService heroCombatService,
            MonsterCombatService monsterCombatService,
            PlayerActionService playerActionService,
            DefenseService defenseService,
            MonsterAIService monsterAIService,
            StatusEffectService statusEffectService,
            GridService grid,
            GameDataService gameData)
        {
            _initiative = initiativeService;
            _heroCombat = heroCombatService;
            _monsterCombat = monsterCombatService;
            _playerAction = playerActionService;
            _monsterAI = monsterAIService;
            _defense = defenseService;
            _statusEffect = statusEffectService;
            _grid = grid;
            _gameData = gameData;
        }


        public void StartCombat(List<Hero> heroes, List<Monster> monsters, bool didBashDoor = false)
        {
            HeroesInCombat = heroes;
            MonstersInCombat = monsters;
            UnwieldlyBonusUsed.Clear();

            foreach (var hero in heroes)
            {
                hero.HasDodgedThisBattle = false;
            }

            // Setup the initiative for the first turn.
            _initiative.SetupInitiative(HeroesInCombat, MonstersInCombat, didBashDoor);

            Console.WriteLine("Combat has started!");
            ProcessNextInInitiative();

            OnCombatStateChanged?.Invoke();
        }

        /// <summary>
        /// Sets up a new turn by resetting AP and preparing the initiative bag.
        /// </summary>
        private void StartNewTurn()
        {
            Console.WriteLine("--- New Turn ---");
            MonstersThatHaveActedThisTurn.Clear();

            foreach (var hero in HeroesInCombat)
            {
                hero.IsVulnerableAfterPowerAttack = false;
                if (hero.Stance != CombatStance.Overwatch)
                {
                    hero.CurrentAP = hero.MaxAP;
                }
            }
            foreach (var monster in MonstersInCombat)
            {
                monster.CurrentAP = monster.MaxAP;
            }

            _initiative.SetupInitiative(HeroesInCombat, MonstersInCombat);
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
                OnCombatStateChanged?.Invoke();
                return;
            }

            if (_initiative.IsTurnOver())
            {
                StartNewTurn();
                OnCombatStateChanged?.Invoke();
                return;
            }

            var nextActorType = _initiative.DrawNextToken();

            if (nextActorType == ActorType.Hero)
            {
                // A hero gets to act. The UI would allow the player to choose an available hero.
                // For now, we'll assume the player is now in control.
                ActiveHero = HeroesInCombat.First(h => h.CurrentAP > 0 && h.Stance != CombatStance.Overwatch);
                if (ActiveHero != null)
                {
                    ActiveHero.IsVulnerableAfterPowerAttack = false;

                    // Process status effects at the start of the hero's turn
                    _statusEffect.ProcessStatusEffects(ActiveHero);
                    Console.WriteLine($"It's {ActiveHero.Name}'s turn. They have {ActiveHero.CurrentAP} AP.");
                    // The game now waits for UI input to call HeroPerformsAction(...).
                }
                else
                {
                    // This can happen if all non-Overwatch heroes have used their AP.
                    ProcessNextInInitiative();
                }
            }
            else // It's a Monster's turn
            {
                ActiveHero = null; 
                Console.WriteLine("A monster acts!");

                var monstersToAct = MonstersInCombat;
                foreach(var monster in MonstersThatHaveActedThisTurn)
                {
                    monstersToAct.Remove(monster);
                }
                var monsterToAct = SelectMonsterToAct(monstersToAct, HeroesInCombat);
                if (monsterToAct != null)
                {
                    monsterToAct.IsVulnerableAfterPowerAttack = false;

                    Console.WriteLine($"A monster ({monsterToAct.Name}) prepares to act...");
                    _statusEffect.ProcessStatusEffects(monsterToAct);
                    MonstersThatHaveActedThisTurn.Add(monsterToAct);
                    var interruptingHero = CheckForOverwatchInterrupt(monsterToAct);

                    if (interruptingHero != null)
                    {
                        Console.WriteLine($"{interruptingHero.Name} on Overwatch interrupts {monsterToAct.Name}'s action!");
                        // _heroCombatService.ExecuteOverwatchAttack(interruptingHero, monsterToAct);
                        interruptingHero.Stance = CombatStance.Normal; // Overwatch is used up
                    }
                    else
                    {
                        // No interruption, so the monster performs its turn using the AI.
                        // The 'new RoomService' is a placeholder for the actual current room state.
                        _monsterAI.ExecuteMonsterTurn(monsterToAct, HeroesInCombat, monsterToAct.Room);
                    }

                    ProcessNextInInitiative(); // No valid monsters left to act
                    return;
                }

                // After the monster's turn is resolved, process the next actor in initiative.
                ProcessNextInInitiative();
            }

            OnCombatStateChanged?.Invoke();
        }

        private Monster? SelectMonsterToAct(List<Monster> availableMonsters, List<Hero> heroes)
        {
            if (!availableMonsters.Any()) return null;

            // 1. Magic User or Ranged Weapon
            var magicOrRanged = availableMonsters
                .FirstOrDefault(m => m.Spells.Any() || m.Weapons.Any(w => w is RangedWeapon));
            if (magicOrRanged != null) return magicOrRanged;

            // 2. Adjacent to a hero and could make room
            var canMakeRoom = availableMonsters
                .FirstOrDefault(m => IsAdjacentToHero(m, heroes) && CanMakeRoom(m, heroes));
            if (canMakeRoom != null) return canMakeRoom;

            // 3. Adjacent to a hero
            var adjacent = availableMonsters
                .FirstOrDefault(m => IsAdjacentToHero(m, heroes));
            if (adjacent != null) return adjacent;

            // 4. Closest to a hero and can charge
            var canCharge = availableMonsters
                .Where(m => CanCharge(m, heroes))
                .OrderBy(m => GetDistanceToClosestHero(m, heroes))
                .FirstOrDefault();
            if (canCharge != null) return canCharge;

            // 5. Can move its full movement
            var canMoveFull = availableMonsters
                .OrderByDescending(m => m.Move) // Prioritize faster monsters
                .FirstOrDefault(m => CanMoveFullPath(m));
            if (canMoveFull != null) return canMoveFull;

            // Default: return the first available monster if no other condition is met
            return availableMonsters.First();
        }

        private bool CanMakeRoom(Monster monster, List<Hero> heroes)
        {
            // This is a more complex grid analysis function.
            // It would check if the monster can move to a position that opens up
            // a path for another monster to attack a hero.
            // For now, we can default to false.
            return false;
        }

        private bool CanCharge(Monster monster, List<Hero> heroes)
        {
            // Checks if a monster has a clear path to charge a hero.
            // A charge is typically a straight line move ending in an attack.
            var target = GetClosestHero(monster, heroes);
            if (target == null || monster.Position == null || target.Position == null) return false;

            int distance = _grid.GetDistance(monster.Position, target.Position);
            return distance > 1 && distance <= monster.Move && _grid.HasClearPath(monster.Position, target.Position);
        }

        private bool CanMoveFullPath(Monster monster)
        {
            // This would check if the monster has enough open space around it
            // to move its full movement distance without being blocked by other units or terrain.
            // This is a complex analysis; a simpler version might just check for a few empty adjacent squares.
            return true; // Placeholder
        }

        /// <summary>
        /// Helper method to check if a monster is adjacent to any hero.
        /// </summary>
        private bool IsAdjacentToHero(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return false;
            foreach (var hero in heroes.Where(h => h.CurrentHP > 0 && h.Position != null))
            {
                if (Math.Abs(monster.Position.X - hero.Position.X) <= 1 &&
                    Math.Abs(monster.Position.Y - hero.Position.Y) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private int GetDistanceToClosestHero(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return int.MaxValue;
            return heroes
                .Where(h => h.Position != null)
                .Min(h => _grid.GetDistance(monster.Position, h.Position!));
        }

        private Hero? GetClosestHero(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return null;
            return heroes
               .Where(h => h.Position != null && h.CurrentHP > 0 && !h.ActiveStatusEffects.Contains(_gameData.GetStatusEffectByType(StatusEffectType.Pit)))
               .OrderBy(h => _grid.GetDistance(monster.Position, h.Position!))
               .FirstOrDefault();
        }

        public void ResolveMonsterAttack(Monster attacker, Hero target, int incomingDamage)
        {
            Console.WriteLine($"{attacker.Name} attacks {target.Name}!");

            // In a real UI, you would now ask the player if they want to Dodge or Parry.
            // For this example, let's assume they try to dodge if they can.

            DefenseResult defenseResult;

            if (!target.HasDodgedThisBattle)
            {
                defenseResult = _defense.AttemptDodge(target);
            }
            else if (target.Shield != null)
            {
                defenseResult = _defense.AttemptShieldParry(target, target.Shield, incomingDamage);
            }
            else
            {
                // No defense options available.
                target.TakeDamage(incomingDamage);
                Console.WriteLine($"The attack hits for {incomingDamage} damage!");
                return;
            }

            Console.WriteLine(defenseResult.OutcomeMessage);

            if (defenseResult.WasSuccessful)
            {
                int remainingDamage = incomingDamage - defenseResult.DamageNegated;
                if (remainingDamage > 0)
                {
                    target.TakeDamage(remainingDamage);
                    Console.WriteLine($"{target.Name} still takes {remainingDamage} damage!");
                }
            }
            else
            {
                // The defense failed, so the hero takes full damage.
                target.TakeDamage(incomingDamage);
                Console.WriteLine($"The attack hits for {incomingDamage} damage!");
            }

            OnCombatStateChanged?.Invoke();
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

                OnCombatStateChanged?.Invoke();

                if (ActiveHero.CurrentAP <= 0)
                {
                    Console.WriteLine($"{ActiveHero.Name}'s turn is over.");
                    ProcessNextInInitiative();
                }
            }
        }

        // This would be called by the UI when a hero performs an attack
        public void HeroPerformsAttack(Hero hero, Monster target, Weapon weapon)
        {
            var context = new CombatContext();
            context.ArmourPiercingValue = weapon.ArmourPiercing;

            if (weapon is MeleeWeapon meleeWeapon && meleeWeapon.HasProperty(WeaponProperty.Unwieldly))
            {
                if (!UnwieldlyBonusUsed.Contains(hero.Id))
                {
                    context.ApplyUnwieldlyBonus = true;
                }
            }

            var attackResult = _heroCombat.ResolveAttack(hero, target, weapon, context);
            Console.WriteLine(attackResult.OutcomeMessage);

            // --- Apply secondary status effects based on the context ---
            if (attackResult.IsHit)
            {
                if (context.IsFireDamage)
                {
                    // Rule: Fire causes ongoing damage.
                    _statusEffect.ApplyStatus(target, StatusEffectType.FireBurning, 1);
                }
                if (context.IsFrostDamage && RandomHelper.RollDie("D100") <= 50)
                {
                    // Rule: Frost has a 50% chance to stun.
                    _statusEffect.ApplyStatus(target, StatusEffectType.Stunned, 1);
                }
                if (context.IsPoisonousAttack)
                {
                    _statusEffect.AttemptToApplyStatus(target, StatusEffectType.Poisoned);
                }
                if (context.CausesDisease)
                {
                    _statusEffect.AttemptToApplyStatus(target, StatusEffectType.Diseased);
                }
            }

            // 4. If the bonus was applied and the attack was a hit, record it.
            if (context.ApplyUnwieldlyBonus && attackResult.IsHit)
            {
                UnwieldlyBonusUsed.Add(hero.Id);
            }

            OnCombatStateChanged?.Invoke();
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
