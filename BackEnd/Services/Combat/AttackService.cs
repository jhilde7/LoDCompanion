using System.Text;
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Combat
{
    public enum HitLocation
    {
        Head,
        Torso,
        Arms,
        Legs
    }

    public class CombatContext
    {
        // General Modifiers
        public bool IsFireDamage { get; set; } = false;
        public bool IsAcidicDamage { get; set; } = false;
        public bool IsFrostDamage { get; set; } = false;
        public bool IsPoisonousAttack { get; set; } = false;
        public bool CausesDisease { get; set; } = false;
        public int ArmourValue { get; set; } = 0;
        public int ArmourPiercingValue { get; set; } = 0;

        // Melee Specific Modifiers
        public bool IsChargeAttack { get; set; }
        public bool IsPowerAttack { get; set; }
        public bool ApplyUnwieldlyBonus { get; set; }

        // Ranged Specific Modifiers
        public bool HasAimed { get; set; }
        public int ObstaclesInLineOfSight { get; set; }
    }

    /// <summary>
    /// Represents the result of a single attack action.
    /// </summary>
    public class AttackResult
    {
        public bool IsHit { get; set; }
        public int DamageDealt { get; set; }
        public string OutcomeMessage { get; set; } = string.Empty;
        public int ToHitChance { get; set; }
        public int AttackRoll { get; set; }
    }

    public class AttackService
    {
        private readonly FloatingTextService _floatingText;
        private readonly UserRequestService _diceRoll;
        private readonly MonsterSpecialService _monsterSpecial;

        public AttackService(
            FloatingTextService floatingTextService,
            UserRequestService diceRollService,
            MonsterSpecialService monsterSpecialService)
        {
            _floatingText = floatingTextService;
            _diceRoll = diceRollService;
            _monsterSpecial = monsterSpecialService;

            _monsterSpecial.OnEntangleAttack += HandleEntangleAttempt;
            _monsterSpecial.OnKickAttack += HandleKickAttack;
            _monsterSpecial.OnSpitAttack += HandleSpitAttack;
            _monsterSpecial.OnSweepingStrikeAttack += HandleSweepingStrikeAttack;
            _monsterSpecial.OnTongueAttack += HandleTongueAttack;
            _monsterSpecial.OnWebAttack += HandleWebAttempt;
        }

        /// <summary>
        /// Resolves a monster's standard attack against a hero.
        /// </summary>
        public async Task<AttackResult> PerformStandardAttackAsync(Character attacker, Weapon? weapon, Character target, DungeonState dungeon, CombatContext? context = null)
        {
            if (context == null)
            {
                context = new CombatContext(); // Standard attack has a default context
            }
            return await ResolveAttackAsync(attacker, weapon, target, context);
        }

        /// <summary>
        /// Resolves a monster's Power Attack, which grants +20 CS.
        /// </summary>
        public async Task<AttackResult> PerformPowerAttackAsync(Character attacker, Weapon? weapon, Character target, DungeonState dungeon, CombatContext? context = null)
        {
            if (context == null)
            {
                context = new CombatContext { IsPowerAttack = true };
            }
            return await ResolveAttackAsync(attacker, weapon, target, context);
        }

        /// <summary>
        /// Resolves a Charge Attack, which grants +10 CS.
        /// </summary>
        public async Task<AttackResult> PerformChargeAttackAsync(Character attacker, Weapon? weapon, Character target, DungeonState dungeon, CombatContext? context = null)
        {
            if (context == null)
            {
                context = new CombatContext { IsChargeAttack = true };
            }
            return await ResolveAttackAsync(attacker, weapon, target, context, dungeon);
        }

        public async Task<AttackResult> ResolveAttackAsync(Character attacker, Weapon? weapon, Character target, CombatContext context, DungeonState? dungeon = null)
        {
            var result = new AttackResult();

            if (attacker is Hero hero)
            {
                result = await CalculateHeroHitAttemptAsync(hero, weapon, (Monster)target, context);
            }
            else
            {
                result = CalculateMonsterHitAttempt((Monster)attacker, weapon, (Hero)target, context);
            }

            if (!result.IsHit)
            {
                return result; // If the attack missed, return early.
            }

            if (target is Hero heroTarget)
            {
                int potentialDamage = weapon != null
                    ? CalculateWeaponPotentialDamage(weapon)
                    : attacker is Monster m ? CalculateMonsterPotentialDamage(m) : 0;
                result = await ResolveAttackAgainstHeroAsync(attacker, heroTarget, potentialDamage, weapon, context, dungeon);
            }
            else if (target is Monster monsterTarget && weapon != null)
            {
                result = await ResolveAttackAgainstMonsterAsync(attacker, monsterTarget, weapon, context, dungeon);
            }

            if (weapon is RangedWeapon rangedWeapon)
            {
                rangedWeapon.ConsumeAmmo();
            }
            return result;
        }

        /// <summary>
        /// Calculates if an attack hits its target by performing a to-hit roll.
        /// </summary>
        /// <param name="attacker">The hero initiating the attack.</param>
        /// <param name="weapon">The weapon being used.</param>
        /// <param name="target">The monster being targeted.</param>
        /// <param name="context">The context of the combat (e.g., power attack, charge).</param>
        /// <returns>An AttackResult containing the outcome of the to-hit roll.</returns>
        public async Task<AttackResult> CalculateHeroHitAttemptAsync(Hero attacker, Weapon? weapon, Monster target, CombatContext context)
        {
            var result = new AttackResult();

            int baseSkill = weapon?.IsRanged ?? false ? attacker.GetSkill(Skill.RangedSkill) : attacker.GetSkill(Skill.CombatSkill);
            int situationalModifier = CalculateHitChanceModifier(attacker, weapon, target, context);
            result.ToHitChance = baseSkill + situationalModifier;
            var resultRoll = await _diceRoll.RequestRollAsync("Roll to-hit.", "1d100"); await Task.Yield();
            result.AttackRoll = resultRoll.Roll;

            if (target.Position != null && (result.AttackRoll > 80 || result.AttackRoll > result.ToHitChance))
            {
                result.IsHit = false;
                result.OutcomeMessage = $"{attacker.Name}'s attack misses {target.Name}.";
                _floatingText.ShowText("Miss!", target.Position, "miss-toast");
            }
            else
            {
                result.IsHit = true;
            }

            return result;
        }

        /// <summary>
        /// Calculates if an attack hits its target by performing a to-hit roll.
        /// </summary>
        /// <param name="attacker">The monster initiating the attack.</param>
        /// <param name="weapon">The weapon being used.</param>
        /// <param name="target">The hero being targeted.</param>
        /// <param name="context">The context of the combat (e.g., power attack, charge).</param>
        /// <returns>An AttackResult containing the outcome of the to-hit roll.</returns>
        public AttackResult CalculateMonsterHitAttempt(Monster attacker, Weapon? weapon, Hero target, CombatContext context)
        {
            var result = new AttackResult();

            int baseSkill = weapon?.IsRanged ?? false ? attacker.GetSkill(Skill.RangedSkill) : attacker.GetSkill(Skill.CombatSkill);
            int situationalModifier = CalculateHitChanceModifier(attacker, weapon, target, context);
            result.ToHitChance = baseSkill + situationalModifier;

            result.AttackRoll = RandomHelper.RollDie(DiceType.D100);

            if (target.Position != null && (result.AttackRoll > 80 || result.AttackRoll > result.ToHitChance))
            {
                result.IsHit = false;
                result.OutcomeMessage = $"{attacker.Name}'s attack misses {target.Name}.";
                _floatingText.ShowText("Miss!", target.Position, "miss-toast");
            }
            else
            {
                result.IsHit = true;
            }

            return result;
        }

        private async Task<AttackResult> ResolveAttackAgainstHeroAsync(Character attacker, Hero target, int potentialDamage, Weapon? weapon, CombatContext context, DungeonState? dungeon = null)
        {
            var result = new AttackResult { IsHit = true };

            DefenseResult defenseResult = await ResolveHeroDefenseAsync(target, potentialDamage);
            int damageAfterDefense = Math.Max(0, potentialDamage - defenseResult.DamageNegated);
            result.OutcomeMessage = defenseResult.OutcomeMessage;

            if (target.Position != null && damageAfterDefense > 0)
            {
                HitLocation location = DetermineHitLocation();
                context = ApplyArmorToLocation(target, location, context, weapon);
                result.DamageDealt = target.TakeDamage(result.DamageDealt, (_floatingText, target.Position), context);

                result.OutcomeMessage += $"\nThe blow hits {target.Name}'s {location} for {result.DamageDealt} damage!";
                if (location == HitLocation.Torso)
                {
                    result.OutcomeMessage += CheckForQuickSlotDamage(target);
                }
            }
            else
            {
                if (target.Position != null)
                {
                    _floatingText.ShowText("Blocked!", target.Position, "miss-text");
                }
            }

            if (context.IsChargeAttack && dungeon != null)
            {
                result.OutcomeMessage += ResolvePostChargeAttack(attacker, target, dungeon);
            }

            return result;
        }

        private async Task<AttackResult> ResolveAttackAgainstMonsterAsync(Character attacker, Monster target, Weapon weapon, CombatContext context, DungeonState? dungeon)
        {
            var result = new AttackResult { IsHit = true };
            int finalDamage = 0; 
            (finalDamage, context) = await CalculateHeroDamageAsync(attacker, target, weapon, context);
            
            result.DamageDealt = finalDamage;

            result.OutcomeMessage = $"{attacker.Name}'s attack hits {target.Name} for {finalDamage} damage!";
            if (target.Position != null)
            {
                target.TakeDamage(finalDamage, (_floatingText, target.Position), context);
            }

            if (context.IsChargeAttack && dungeon != null)
            {
                result.OutcomeMessage += ResolvePostChargeAttack(attacker, target, dungeon);
            }

            return result;
        }

        public string ResolvePostChargeAttack(Character attacker, Character target, DungeonState dungeon)
        {
            var chargeMessage = new StringBuilder();

            var originalTargetPosition = target.Position;

            chargeMessage.Append("\n" + $"{attacker.Name} follows through with the charge!");

            // Perform the shove
            AttackResult shoveResult = PerformShove(attacker, target, dungeon, isCharge: true);
            chargeMessage.Append("\n" + shoveResult.OutcomeMessage);

            // If the shove was successful and a valid original position was stored
            if (shoveResult.IsHit && originalTargetPosition != null)
            {
                // Move the attacker into the square the target was pushed FROM
                if(GridService.MoveCharacterToPosition(attacker, originalTargetPosition, dungeon.DungeonGrid))
                {
                    chargeMessage.Append($"\n{attacker.Name} moves into the vacated space.");
                }
            }

            return chargeMessage.ToString();
        }

        public int CalculateHitChanceModifier(Character attacker, Weapon? weapon, Character target, CombatContext context)
        {
            int modifier = 0;
            Weapon? heroWeapon = null;
            Weapon? monsterWeapon = null;
            if (target is Hero hero)
            {
                heroWeapon = hero.Inventory.EquippedWeapon;
            }
            if (target is Monster monster)
            {
                monsterWeapon = monster.GetMeleeWeapon();
            }


            // Ranged-specific modifiers
            if (weapon != null && weapon is RangedWeapon)
            {
                modifier -= context.ObstaclesInLineOfSight * 10;
                if (target is Monster targetMonster &&
                    (targetMonster.PassiveSpecials.Any(n => n.Key.Name == MonsterSpecialName.XLarge)
                    || targetMonster.PassiveSpecials.Any(n => n.Key.Name == MonsterSpecialName.Large)))
                    modifier += 10;
                if (context.HasAimed) modifier += 10;
            }
            if (DirectionService.IsAttackingFromBehind(attacker, target)) modifier += 20;
            if (attacker.Position != null && target.Position != null && attacker.Position.Z > target.Position.Z) modifier += 10;

            if (context.IsChargeAttack) modifier += 10;
            if (context.IsPowerAttack) modifier += 20;
            if (target.CombatStance == CombatStance.Prone) modifier += 30;
            if (monsterWeapon != null)
            {
                if (monsterWeapon.Name == "Rapier") modifier -= 5;
                if (monsterWeapon.Properties.ContainsKey(WeaponProperty.Slow)) modifier += 5;
                if (monsterWeapon.Properties.ContainsKey(WeaponProperty.BFO)) modifier += 5;
                if (monsterWeapon.Name == "Staff") modifier -= 5;
            }
            // If the hero performed a Power Attack, they are vulnerable.
            if (target.IsVulnerableAfterPowerAttack)
            {
                modifier += 10;
            }
            else
            {
                if (!DirectionService.IsAttackingFromBehind(attacker, target))
                {
                    // If not vulnerable, their normal defensive bonuses apply.
                    if (target.HasShield)
                    {
                        modifier -= 5;
                    }
                    // A Parry Stance makes the hero harder to hit.
                    if (target.CombatStance == CombatStance.Parry)
                    {
                        modifier -= 10;
                        if (heroWeapon != null && heroWeapon is MeleeWeapon meleeWeapon)
                        {
                            if (meleeWeapon != null && meleeWeapon.HasProperty(WeaponProperty.BFO)) modifier += 5;
                            if (meleeWeapon != null && meleeWeapon.HasProperty(WeaponProperty.Defensive)) modifier -= 10;
                        }
                    }
                }
            }

            return modifier;
        }

        private int CalculateWeaponPotentialDamage(Weapon weapon)
        {
            int damage = 0;
            while (damage == 0)
            {
                damage = weapon.RollDamage();
            }
            return damage;
        }

        public int CalculateMonsterPotentialDamage(Monster monster)
        {
            int damage = 0;
            while (damage == 0)
            {
                damage = RandomHelper.GetRandomNumber(monster.MinDamage, monster.MaxDamage);
            }
            return damage + monster.GetStat(BasicStat.DamageBonus);
        }

        private async Task<DefenseResult> ResolveHeroDefenseAsync(Hero target, int incomingDamage)
        {
            if (!target.HasDodgedThisBattle)
            {
                return await DefenseService.AttemptDodge(target, _diceRoll);
            }
            if (target.Inventory.OffHand != null && target.Inventory.OffHand is Shield shield)
            {
                return await DefenseService.AttemptShieldParry(target, shield, incomingDamage, _diceRoll);
            }
            if (target.CombatStance == CombatStance.Parry)
            {
                return await DefenseService.AttemptWeaponParry(target, target.Weapons.FirstOrDefault(w => w.IsMelee), _diceRoll);
            }
            return new DefenseResult { WasSuccessful = false, OutcomeMessage = $"{target.Name} is unable to defend!" };
        }

        private HitLocation DetermineHitLocation()
        {
            int roll = RandomHelper.RollDie(DiceType.D6);
            return roll switch
            {
                1 => HitLocation.Head,
                2 => HitLocation.Arms,
                6 => HitLocation.Legs,
                _ => HitLocation.Torso
            };
        }

        /// <summary>
        /// Applies armor reduction based on the specific hit location.
        /// </summary>
        private CombatContext ApplyArmorToLocation(Hero target, HitLocation location, CombatContext combatContext, Weapon? weapon)
        {
            var relevantArmor = target.Inventory.EquippedArmour.Where(a => DoesArmorCoverLocation(a, location)).ToList();
            combatContext.ArmourValue = relevantArmor.Sum(a => a.DefValue);

            int armourPiercing = 0;
            weapon?.Properties.TryGetValue(WeaponProperty.ArmourPiercing, out armourPiercing);
            combatContext.ArmourValue = Math.Max(0, combatContext.ArmourValue - armourPiercing);

            return combatContext;
        }

        /// <summary>
        /// Checks if a piece of armor covers a given hit location.
        /// </summary>
        private bool DoesArmorCoverLocation(Armour armour, HitLocation location)
        {
            return location switch
            {
                HitLocation.Head => armour.HasProperty(ArmourProperty.Head),
                HitLocation.Arms => armour.HasProperty(ArmourProperty.Arms),
                HitLocation.Torso => armour.HasProperty(ArmourProperty.Torso),
                HitLocation.Legs => armour.HasProperty(ArmourProperty.Legs),
                _ => false,
            };
        }

        /// <summary>
        /// On a torso hit, rolls to see if an item in a quick slot is damaged.
        /// </summary>
        private string CheckForQuickSlotDamage(Hero target)
        {
            int slotRoll = RandomHelper.RollDie(DiceType.D10);
            if (slotRoll <= target.Inventory.QuickSlots.Count)
            {
                var item = target.Inventory.QuickSlots[slotRoll - 1]; // -1 for 0-based index
                if (item != null)
                {
                    item.Durability--;
                    return $"The blow also strikes {target.Name}'s gear! Their {item.Name} is damaged.";
                }
            }
            return "The hero's gear was spared from the impact.";
        }

        /// <summary>
        /// Calculates the final damage dealt by a successful hit, including all bonuses and armor reduction.
        /// </summary>
        private async Task<(int, CombatContext)> CalculateHeroDamageAsync(Character attacker, Character target, Weapon weapon, CombatContext context)
        {
            int damage = 0;
            if (weapon.DamageDice != null)
            {
                var rollResult = await _diceRoll.RequestRollAsync($"You Hit {target.Name}, now roll for damage", weapon.DamageDice); await Task.Yield();
                damage = rollResult.Roll;
            }
            else
            {
                damage = weapon.RollDamage();
            }
            damage += attacker.GetStat(BasicStat.DamageBonus);

            if (weapon is MeleeWeapon meleeWeapon && attacker is Hero hero)
            {
                // If the hero has a talent that provides a damage bonus, apply it.
                damage += hero.Talents
                    .Where(t => t.StatBonus != null && t.StatBonus.Value.Item1 == BasicStat.DamageBonus)
                    .Sum(t => t.StatBonus != null ? t.StatBonus.Value.Item2 : 0);
                // Apply the Unwieldly bonus if the context flag is set.
                if (context.ApplyUnwieldlyBonus)
                {
                    // Get the bonus value from the weapon's properties (e.g., 4 for the Morning Star)
                    int bonus = meleeWeapon.GetPropertyValue(WeaponProperty.Unwieldly);
                    damage += bonus;
                    Console.WriteLine($"Unwieldly bonus applied: +{bonus} damage!");
                }
            }
            else if (weapon is RangedWeapon rangedWeapon)
            {
                if (rangedWeapon.Ammo != null)
                {
                    if (rangedWeapon.Ammo.HasProperty(AmmoProperty.Barbed))
                    {
                        damage += rangedWeapon.Ammo.GetPropertyValue(AmmoProperty.Barbed); 
                    }
                    else if (rangedWeapon.Ammo.HasProperty(AmmoProperty.SuperiorSlingStone))
                    {
                        damage += rangedWeapon.Ammo.GetPropertyValue(AmmoProperty.SuperiorSlingStone);
                    }
                }
            }

            int targetArmor = 0;
            if (target is Monster monster)
            {
                targetArmor = monster.ArmourValue;
            }

            // Apply Armour Piercing from the context
            context.ArmourValue = Math.Max(0, targetArmor - context.ArmourPiercingValue);

            return (damage, context);
        }

        /// <summary>
        /// Performs a shove attack, handling all rules and outcomes.
        /// </summary>
        public AttackResult PerformShove(Character shover, Character target, DungeonState dungeon, bool isCharge = false)
        {
            var result = new AttackResult();
            var grid = dungeon.DungeonGrid;
            result.IsHit = true;

            var charactersInArea = new List<Character>();
            if (shover.Room?.HeroesInRoom != null) charactersInArea.AddRange(shover.Room.HeroesInRoom);
            if (shover.Room?.MonstersInRoom != null) charactersInArea.AddRange(shover.Room.MonstersInRoom);

            // === PRE-CHECKS ===
            if (target.Position == null || shover.Position == null)
            {
                result.OutcomeMessage = "Invalid character position.";
                result.IsHit = false;
                return result;
            }

            // Rule: Can only shove adjacent models, only applies to non-charge attacks
            if (!isCharge && !GridService.IsAdjacent(shover.Position, target.Position))
            {
                result.OutcomeMessage = $"{target.Name} is not adjacent.";
                result.IsHit = false;
                return result;
            }

            // Rule: Cannot shove large models
            if (target is Monster monster && monster.PassiveSpecials.Any(s => s.Key.Name == MonsterSpecialName.Large || s.Key.Name == MonsterSpecialName.XLarge))
            {
                result.OutcomeMessage = $"{target.Name} is too large to be moved!";
                result.IsHit = false;
                return result;
            }

            if (!isCharge)
            {
                // === ROLL CHECK ===
                int shoveRoll = RandomHelper.RollDie(DiceType.D100);
                int shoveBonus = shover.GetStat(BasicStat.DamageBonus) * 10;
                int totalShoveValue = shoveRoll + shoveBonus;
                result.ToHitChance = target.GetStat(BasicStat.Dexterity);
                result.AttackRoll = totalShoveValue;

                if (result.AttackRoll <= result.ToHitChance)
                {
                    result.OutcomeMessage = $"attempt fails. (Rolled {result.AttackRoll} vs DEX {result.ToHitChance})";
                    return result;
                }
            }

            // Determine "straight back" vector
            var straightBackVector = new GridPosition(
                Math.Sign(target.Position.X - shover.Position.X),
                Math.Sign(target.Position.Y - shover.Position.Y),
                Math.Sign(target.Position.Z - shover.Position.Z)
            );

            var straightBackPos = target.Position.Add(straightBackVector);
            var straightBackSquare = GridService.GetSquareAt(straightBackPos, grid);

            // --- Attempt Straight Push ---
            if (straightBackSquare != null && !straightBackSquare.MovementBlocked)
            {
                Character? model2 = charactersInArea.FirstOrDefault(c => c.Position != null && c.Position.Equals(straightBackPos));

                if (model2 == null) // Space is free
                {
                    GridService.MoveCharacterToPosition(target, straightBackPos, grid);
                    result.OutcomeMessage = $"successfully shoves {target.Name} straight back!";
                    return result;
                }
                else // Attempt chain reaction
                {
                    if(model2.Position != null)
                    {
                        var posBehindModel2 = model2.Position.Add(straightBackVector);
                        var squareBehindModel2 = GridService.GetSquareAt(posBehindModel2, grid);

                        if (squareBehindModel2 != null && !squareBehindModel2.MovementBlocked && !charactersInArea.Any(c => c.Position != null && c.Position.Equals(posBehindModel2)))
                        {
                            GridService.MoveCharacterToPosition(model2, posBehindModel2, grid);
                            GridService.MoveCharacterToPosition(target, straightBackPos, grid);
                            result.OutcomeMessage = $"shoves {target.Name}, who stumbles into {model2.Name}, pushing them both back!";
                            return result;
                        }
                    }
                }
            }

            // --- Attempt Diagonal Push if straight back is blocked ---
            if (straightBackSquare == null || straightBackSquare.MovementBlocked)
            {
                var diagonalPositions = new List<GridPosition>();
                if (straightBackVector.X != 0) // Pushing along X axis
                {
                    diagonalPositions.Add(target.Position.Add(new GridPosition(straightBackVector.X, 1, 0)));
                    diagonalPositions.Add(target.Position.Add(new GridPosition(straightBackVector.X, -1, 0)));
                }
                else if (straightBackVector.Y != 0) // Pushing along Y axis
                {
                    diagonalPositions.Add(target.Position.Add(new GridPosition(1, straightBackVector.Y, 0)));
                    diagonalPositions.Add(target.Position.Add(new GridPosition(-1, straightBackVector.Y, 0)));
                }

                foreach (var diagPos in diagonalPositions)
                {
                    var diagSquare = GridService.GetSquareAt(diagPos, grid);
                    if (diagSquare != null && !diagSquare.MovementBlocked && !charactersInArea.Any(c => c.Position != null && c.Position.Equals(diagPos)))
                    {
                        GridService.MoveCharacterToPosition(target, diagPos, grid);
                        result.OutcomeMessage = $"successfully shoves {target.Name} diagonally back!";
                        return result;
                    }
                }
            }

            // === FALL OVER (If all else fails) ===
            StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.Prone, 1));
            result.OutcomeMessage = $"shoves {target.Name}, but they are blocked and fall over!";
            return result;
        }

        public async Task<DefenseResult> HandleEntangleAttempt(Monster attacker, Hero target)
        {
            return await ResolveHeroDefenseAsync(target, 0);
        }

        public async Task<AttackResult> HandleKickAttack(Monster attacker, Hero target)
        {
            var result = new AttackResult();
            result = CalculateMonsterHitAttempt(attacker, null, target, new CombatContext());
            if (!result.IsHit)
            {
                if (!result.IsHit && target.Position != null)
                {
                    _floatingText.ShowText("Miss!", target.Position, "miss-toast");
                }
                return result; // If the attack missed, return early.
            }
            int damageRoll = RandomHelper.RollDie(DiceType.D10);
            int totalDamage = damageRoll + 2;

            return await ResolveAttackAgainstHeroAsync(attacker, target, totalDamage, null, new CombatContext());
        }

        public async Task<AttackResult> HandleSpitAttack(Monster attacker, Hero target)
        {
            var result = new AttackResult();
            result = CalculateMonsterHitAttempt(attacker, null, target, new CombatContext());
            if (!result.IsHit)
            {
                if (!result.IsHit && target.Position != null)
                {
                    _floatingText.ShowText("Miss!", target.Position, "miss-toast");
                }
                return result; // If the attack missed, return early.
            }
            else // attack can be parried or dodged as normal
            {
                DefenseResult defenseResult = await ResolveHeroDefenseAsync(target, 0);
                if (defenseResult.WasSuccessful)
                {
                    result.IsHit = false; // The attack was successfully defended
                    result.OutcomeMessage = defenseResult.OutcomeMessage;
                }
            }
            return result;
        }

        public async Task<AttackResult> HandleSweepingStrikeAttack(Monster attacker, List<Hero> heroes, DungeonState dungeon)
        {
            var result = new AttackResult();
            if (attacker.Position == null) return result;

            foreach (var hero in heroes)
            {
                if (hero.Position == null) continue;
                var heroesInZoc = DirectionService.IsInZoneOfControl(attacker.Position, hero);
                result = CalculateMonsterHitAttempt(attacker, attacker.GetMeleeWeapon(), hero, new CombatContext());

                // Heroes can attempt to dodge but not parry.
                var defenseResult = await DefenseService.AttemptDodge(hero, _diceRoll);
                if (defenseResult.WasSuccessful)
                {
                    result.OutcomeMessage += defenseResult.OutcomeMessage + "\n";
                    continue; // Dodge was successful, no further effects.
                }

                if (result.IsHit)
                {
                    int baseDamage = CalculateMonsterPotentialDamage(attacker);
                    int potentialDamage = (int)Math.Ceiling(baseDamage / 2d); // Base damage is halved for sweeping strikes.

                    // Calculate pushback position
                    int dx = hero.Position.X - attacker.Position.X;
                    int dy = hero.Position.Y - attacker.Position.Y;
                    var pushbackPosition = new GridPosition(hero.Position.X + Math.Sign(dx), hero.Position.Y + Math.Sign(dy), hero.Position.Z);
                    var pushbackSquare = GridService.GetSquareAt(pushbackPosition, dungeon.DungeonGrid);
                    bool isBlocked = pushbackSquare == null || pushbackSquare.MovementBlocked || pushbackSquare.IsOccupied;
                    var attackResult = new AttackResult();
                    if (isBlocked)
                    {
                        attackResult = await ResolveAttackAgainstHeroAsync(attacker, hero, baseDamage, attacker.GetMeleeWeapon(), new CombatContext());
                    }
                    else
                    {
                        attackResult = await ResolveAttackAgainstHeroAsync(attacker, hero, potentialDamage, attacker.GetMeleeWeapon(), new CombatContext());
                    }
                    result.OutcomeMessage += attackResult.OutcomeMessage;
                    // DEX test to avoid falling prone
                    var resultRoll = await _diceRoll.RequestRollAsync($"Roll a DEX test for {hero.Name} to stay standing.", "1d100"); await Task.Yield();
                    int dexRoll = resultRoll.Roll;
                    if (dexRoll > hero.GetStat(BasicStat.Dexterity))
                    {
                        StatusEffectService.AttemptToApplyStatus(hero, new ActiveStatusEffect(StatusEffectType.Prone, 1));
                        result.OutcomeMessage += $"{hero.Name} is knocked off their feet!\n";
                    }
                    else
                    {
                        result.OutcomeMessage += $"{hero.Name} manages to stay standing.\n";
                    }

                    if (!isBlocked)
                    {
                        GridService.MoveCharacterToPosition(hero, pushbackPosition, dungeon.DungeonGrid);
                    }
                }
                else
                {
                    if (!result.IsHit && hero.Position != null)
                    {
                        _floatingText.ShowText("Miss!", hero.Position, "miss-toast");
                    }
                    result.OutcomeMessage += $"{attacker.Name}'s sweeping strike misses {hero.Name}.\n";
                }
            }
            return result;
        }

        public async Task<AttackResult> HandleTongueAttack(Monster monster, Hero target, DungeonState dungeon)
        {
            var result = new AttackResult();
            var outcome = result.OutcomeMessage;

            result = CalculateMonsterHitAttempt(monster, null, target, new CombatContext());
            if (result.IsHit)
            {
                // Heroes can attempt to dodge but not parry.
                var defenseResult = await ResolveHeroDefenseAsync(target, 0);
                if (defenseResult.WasSuccessful)
                {
                    result.OutcomeMessage += defenseResult.OutcomeMessage + "\n";
                }
                else
                {
                    outcome += "The tongue hits!\n";
                    if (target.Position == null || monster.Position == null) return result;
                    // Determine the square adjacent to the monster, in the direction of the target
                    var directionX = Math.Sign(target.Position.X - monster.Position.X);
                    var directionY = Math.Sign(target.Position.Y - monster.Position.Y);
                    var destination = new GridPosition(monster.Position.X + directionX, monster.Position.Y + directionY, monster.Position.Z);

                    var destinationSquare = GridService.GetSquareAt(destination, dungeon.DungeonGrid);
                    if (destinationSquare != null)
                    {
                        if (destinationSquare.IsOccupied)
                        {
                            var occupant = dungeon.AllCharactersInDungeon.FirstOrDefault(c => c.Id == destinationSquare.OccupyingCharacterId);
                            if (occupant != null)
                            {
                                // Swap positions
                                GridService.MoveCharacterToPosition(occupant, target.Position, dungeon.DungeonGrid);
                                GridService.MoveCharacterToPosition(target, destination, dungeon.DungeonGrid);
                                outcome += $"{target.Name} is pulled, swapping places with {occupant.Name}!\n";
                            }
                        }
                        else
                        {
                            // Move the target to the destination
                            GridService.MoveCharacterToPosition(target, destination, dungeon.DungeonGrid);
                            outcome += $"{target.Name} is pulled to the square next to {monster.Name}!\n";
                        }
                    }
                }
            }
            else
            {
                if (target.Position != null)
                {
                    _floatingText.ShowText("Miss!", target.Position, "miss-toast");
                }
                result.OutcomeMessage = $"{monster.Name}'s tongue attack misses {target.Name}.";
            }

            return result;
        }

        public Task<AttackResult> HandleWebAttempt(Monster monster, Hero target)
        {
            var result = CalculateMonsterHitAttempt(monster, null, target, new CombatContext());
            if (!result.IsHit && target.Position != null)
            {
                _floatingText.ShowText("Miss!", target.Position, "miss-toast");
            }
            return Task.FromResult(result);
        }
    }
}
