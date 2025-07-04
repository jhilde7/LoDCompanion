using LoDCompanion.Services.GameData;
using LoDCompanion.Models;
using LoDCompanion.Utilities;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;

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

    public class HeroCombatService
    {
        /// <summary>
        /// Resolves a hero's attack against a monster, calculating the final to-hit chance and damage.
        /// </summary>
        public AttackResult ResolveAttack(Hero attacker, Monster target, Weapon weapon, CombatContext context)
        {
            var result = new AttackResult();
            bool isRanged = weapon is RangedWeapon;

            // Step 1: Determine the base skill (CS or RS)
            int baseSkill = isRanged ? attacker.RangedSkill : attacker.CombatSkill;

            // Step 2: Calculate the final To-Hit chance using all modifiers
            result.ToHitChance = CalculateToHitChance(baseSkill, target, weapon, context);

            // Step 3: Roll the dice
            result.AttackRoll = RandomHelper.RollDie("D100");

            // Step 4: Determine if the attack hits
            if (result.AttackRoll <= result.ToHitChance)
            {
                result.IsHit = true;
                // Step 5: If it's a hit, calculate damage
                result.DamageDealt = CalculateFinalDamage(attacker, target, weapon, context);
                target.TakeDamage(result.DamageDealt);
                result.OutcomeMessage = $"{attacker.Name}'s attack hits {target.Name} for {result.DamageDealt} damage!";
            }
            else
            {
                result.IsHit = false;
                result.OutcomeMessage = $"{attacker.Name}'s attack misses {target.Name}.";
            }

            return result;
        }

        /// <summary>
        /// Calculates the final To-Hit chance based on the tables on page 99 of the combat PDF.
        /// </summary>
        private int CalculateToHitChance(int baseSkill, Monster target, Weapon weapon, CombatContext context)
        {
            int finalChance = baseSkill;
            var targetWeapon = target.Weapons.First();

            // --- Apply Modifiers from Tables ---
            if (context.IsTargetProne) finalChance += 30;
            if (context.IsAttackingFromBehind) finalChance += 20;
            if (context.HasHeightAdvantage) finalChance += 10;
            if (context.IsChargeAttack) finalChance += 10;
            if (context.IsPowerAttack) finalChance += 20;
            if (context.HasAimed) finalChance += 10;

            if (target.IsLarge) finalChance += 10;
            if (target.HasShield && !context.DidTargetUsePowerAttackLastTurn) finalChance -= 5;
            if (context.IsTargetInParryStance) finalChance -= 10;

            if (targetWeapon != null)
            {
                if (targetWeapon.Name == "Rapier") finalChance -= 5;
                if (targetWeapon.IsSlow) finalChance += 5;
                if (targetWeapon.IsBFO) finalChance += 5;
                if (targetWeapon.Name == "Staff") finalChance -= 5;
            }

            // Ranged-specific modifiers
            if (weapon is RangedWeapon)
            {
                finalChance -= (context.ObstaclesInLineOfSight * 10);
                // The PDF refers to "Enemy Defence Value", which is the monster's Dodge stat.
                finalChance -= target.Dodge;
            }
            else // Melee-specific modifiers
            {
                // The PDF refers to "Enemy 'To Hit' value", which is also the monster's Dodge stat.
                if (!context.DidTargetUsePowerAttackLastTurn)
                {
                    finalChance -= target.ToHit;
                }
            }

            return Math.Max(0, finalChance); // Chance cannot be negative
        }

        /// <summary>
        /// Calculates the damage roll for a hero's attack.
        /// </summary>
        /// <param name="heroWeapon">The weapon being used by the hero (MeleeWeapon or RangedWeapon).</param>
        /// <returns>The calculated raw damage roll.</returns>
        public int CalculateDamageRoll(Weapon heroWeapon) 
        {
            return RandomHelper.GetRandomNumber(heroWeapon.MinDamage, heroWeapon.MaxDamage); ;
        }
        
        /// <summary>
         /// Calculates the final damage dealt by a successful hit, including all bonuses and armor reduction.
         /// This method now incorporates the logic from the previous bonus calculation methods.
         /// </summary>
        private int CalculateFinalDamage(Hero attacker, Monster target, Weapon weapon, CombatContext context)
        {
            int damage = weapon.RollDamage();
            damage += attacker.DamageBonus;
            
            if (weapon is MeleeWeapon meleeWeapon)
            {
                if (attacker.Talents.Any(t => t.IsMightyBlow))
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

            int finalDamage;
            int targetArmor = target.ArmourValue;
            int targetNaturalArmor = target.NaturalArmour;

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
    }
}