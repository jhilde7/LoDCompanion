using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Models;
using LoDCompanion.Utilities;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;
using LoDCompanion.Services.Player;
using System.Threading.Tasks;

namespace LoDCompanion.Services.Combat
{

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
            _monsterSpecial.OnSweepingStrikeAttack += HanldeSweepingStrikeAttackAsync;
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
                context =  new CombatContext { IsChargeAttack = true };
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
                int potentialDamage = (weapon != null)
                    ? CalculateWeaponPotentialDamage(weapon)
                    : (attacker is Monster m) ? CalculateMonsterPotentialDamage(m) : 0;
                result = await ResolveAttackAgainstHeroAsync(attacker, heroTarget, potentialDamage, weapon, context, dungeon);
            }
            else if (target is Monster monsterTarget && weapon != null)
            {
                result = await ResolveAttackAgainstMonsterAsync(attacker, monsterTarget, weapon, context, dungeon);
            }

            if(weapon is RangedWeapon rangedWeapon)
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

            int baseSkill = (weapon?.IsRanged ?? false) ? attacker.GetSkill(Skill.RangedSkill) : attacker.GetSkill(Skill.CombatSkill);
            int situationalModifier = CalculateHitChanceModifier(attacker, weapon, target, context);
            result.ToHitChance = baseSkill + situationalModifier;

            result.AttackRoll = await _diceRoll.RequestRollAsync("Roll to-hit.", "1d100");

            if (result.AttackRoll > 80 || result.AttackRoll > result.ToHitChance)
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

            int baseSkill = (weapon?.IsRanged ?? false) ? attacker.GetSkill(Skill.RangedSkill) : attacker.GetSkill(Skill.CombatSkill);
            int situationalModifier = CalculateHitChanceModifier(attacker, weapon, target, context);
            result.ToHitChance = baseSkill + situationalModifier;

            result.AttackRoll = RandomHelper.RollDie(DiceType.D100);

            if (result.AttackRoll > 80 || result.AttackRoll > result.ToHitChance)
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

            if (damageAfterDefense > 0)
            {
                HitLocation location = DetermineHitLocation();
                result.DamageDealt = ApplyArmorToLocation(target, location, damageAfterDefense, weapon);
                target.TakeDamage(result.DamageDealt);

                result.OutcomeMessage += $"\nThe blow hits {target.Name}'s {location} for {result.DamageDealt} damage!";
                if (location == HitLocation.Torso)
                {
                    result.OutcomeMessage += CheckForQuickSlotDamage(target); 
                }
                _floatingText.ShowText($"-{result.DamageDealt}", target.Position, "damage-text");
            }
            else
            {
                _floatingText.ShowText("Blocked!", target.Position, "miss-text");
            }

            if (context.IsChargeAttack && dungeon != null)
            {
                result.OutcomeMessage += "\n" + GridService.ShoveCharacter(attacker, target, dungeon.DungeonGrid);
            }

            return result;
        }

        private async Task<AttackResult> ResolveAttackAgainstMonsterAsync(Character attacker, Monster target, Weapon weapon, CombatContext context, DungeonState? dungeon)
        {
            var result = new AttackResult { IsHit = true };
            int finalDamage = await CalculateHeroDamageAsync(attacker, target, weapon, context);
            target.TakeDamage(finalDamage);
            result.DamageDealt = finalDamage;

            result.OutcomeMessage = $"{attacker.Name}'s attack hits {target.Name} for {finalDamage} damage!";
            _floatingText.ShowText($"-{finalDamage}", target.Position, "damage-text");

            if (context.IsChargeAttack && dungeon != null)
            {
                result.OutcomeMessage += "\n" + GridService.ShoveCharacter(attacker, target, dungeon.DungeonGrid);
            }

            return result;
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
            if(target is Monster monster)
            {
                monsterWeapon = monster.GetMeleeWeapon();
            }


            // Ranged-specific modifiers
            if (weapon != null && weapon is RangedWeapon)
            {
                modifier -= (context.ObstaclesInLineOfSight * 10);
                if (target is Monster targetMonster && 
                    (targetMonster.PassiveSpecials.Any(n => n.Key.Name == MonsterSpecialName.XLarge) 
                    || targetMonster.PassiveSpecials.Any(n => n.Key.Name == MonsterSpecialName.Large))) 
                    modifier += 10;
                if (context.HasAimed) modifier += 10;
            }
            if (DirectionService.IsAttackingFromBehind(attacker, target)) modifier += 20;
            if (attacker.Position.Z > target.Position.Z) modifier += 10;

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
            return weapon.RollDamage();
        }

        public int CalculateMonsterPotentialDamage(Monster monster)
        {
            return RandomHelper.GetRandomNumber(monster.MinDamage, monster.MaxDamage) + monster.GetStat(BasicStat.DamageBonus);
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
        private int ApplyArmorToLocation(Hero target, HitLocation location, int incomingDamage, Weapon? weapon)
        {
            var relevantArmor = target.Inventory.EquippedArmour.Where(a => DoesArmorCoverLocation(a, location)).ToList();
            int totalArmorValue = relevantArmor.Sum(a => a.DefValue);

            int armourPiercing = weapon?.Properties[WeaponProperty.ArmourPiercing] ?? 0;
            int effectiveArmor = Math.Max(0, totalArmorValue - armourPiercing);

            return Math.Max(0, incomingDamage - effectiveArmor);
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
        /// This method now incorporates the logic from the previous bonus calculation methods.
        /// </summary>
        private async Task<int> CalculateHeroDamageAsync(Character attacker, Character target, Weapon weapon, CombatContext context)
        {
            string? dice = weapon.DamageDice;
            int damage = 0;
            if(dice != null)
            {
                damage = await _diceRoll.RequestRollAsync($"You Hit {target.Name}, now roll for damage", dice);
            }
            else
            {
                damage = weapon.RollDamage();
            }
            damage += attacker.GetStat(BasicStat.DamageBonus);

            if (weapon is MeleeWeapon meleeWeapon && attacker is Hero hero)
            {
                if (hero.Talents.Any(t => t.Name == TalentName.MightyBlow))
                {
                    damage += 1;
                }
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
                if (rangedWeapon.Ammo != null && (rangedWeapon.Ammo.HasProperty(AmmoProperty.Barbed) || rangedWeapon.Ammo.HasProperty(AmmoProperty.SupuriorSlingStone)))
                {
                    damage += 1;
                }
            }

            int finalDamage = 0;
            int targetArmor = 0;
            int targetNaturalArmor = 0;
            if (target is Monster monster)
            {
                targetArmor = monster.ArmourValue;
                targetNaturalArmor = monster.GetStat(BasicStat.NaturalArmour);
            }

            // Apply Armour Piercing from the context
            targetArmor = Math.Max(0, targetArmor - context.ArmourPiercingValue);

            if (context.IsFireDamage)
            {
                // Rule: "Fire Damage will ignore both NA and armour."
                finalDamage = damage;
            }
            else if (context.IsAcidicDamage)
            {
                // Rule: "Acidic Damage... will ignore NA."
                finalDamage = Math.Max(0, damage - targetArmor);
            }
            else // This includes Frost and standard Physical damage
            {
                // Rule: Standard damage calculation.
                finalDamage = Math.Max(0, damage - (targetArmor + targetNaturalArmor));
            }

            return finalDamage;
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
                return result; // If the attack missed, return early.
            }
            else // attack can be parried or dodged as normal
            {
                DefenseResult defenseResult = await ResolveHeroDefenseAsync(target, 0);
                if( defenseResult.WasSuccessful)
                {
                    result.IsHit = false; // The attack was successfully defended
                    result.OutcomeMessage = defenseResult.OutcomeMessage; 
                }
            }
            return result;
        }

        public async Task<AttackResult> HanldeSweepingStrikeAttackAsync(Monster attacker, List<Hero> heroes, DungeonState dungeon)
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
                    int potentialDamage = (int)Math.Ceiling(baseDamage/ 2d); // Base damage is halved for sweeping strikes.
                    
                    // Calculate pushback position
                    int dx = hero.Position.X - attacker.Position.X;
                    int dy = hero.Position.Y - attacker.Position.Y;
                    var pushbackPosition = new GridPosition(hero.Position.X + Math.Sign(dx), hero.Position.Y + Math.Sign(dy), hero.Position.Z);
                    var pushbackSquare = GridService.GetSquareAt(pushbackPosition, dungeon.DungeonGrid);
                    bool isBlocked = pushbackSquare == null || pushbackSquare.MovementBlocked || pushbackSquare.IsOccupied;

                    if (isBlocked)
                    {
                        result = await ResolveAttackAgainstHeroAsync(attacker, hero, baseDamage, attacker.GetMeleeWeapon(), new CombatContext());
                    }
                    else
                    {
                        result = await ResolveAttackAgainstHeroAsync(attacker, hero, potentialDamage, attacker.GetMeleeWeapon(), new CombatContext());
                    }

                    // DEX test to avoid falling prone
                    int dexRoll = await _diceRoll.RequestRollAsync($"Roll a DEX test for {hero.Name} to stay standing.", "1d100");
                    if (dexRoll > hero.GetStat(BasicStat.Dexterity))
                    {
                        hero.CombatStance = CombatStance.Prone;
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
                    result.OutcomeMessage += $"{attacker.Name}'s sweeping strike misses {hero.Name}.\n";
                }
            }
        }
    }
}
