using LoDCompanion.Services.GameData;
using LoDCompanion.Models;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Combat
{
    public class HeroCombatService
    {
        // This service should perform calculations, not hold the hero's state.
        // Hero's state (HP, Energy, Skills, Weapons, etc.) should be passed in from the Hero object.

        public HeroCombatService()
        {
            // Constructor for any initial setup, e.g., dependency injection for a random number generator
        }

        /// <summary>
        /// Calculates the damage roll for a hero's attack.
        /// </summary>
        /// <param name="heroWeapon">The weapon being used by the hero (MeleeWeapon or RangedWeapon).</param>
        /// <returns>The calculated raw damage roll.</returns>
        public int CalculateDamageRoll(object heroWeapon) // Using 'object' to allow either MeleeWeapon or RangedWeapon
        {
            int damageRoll = 0;

            if (heroWeapon is MeleeWeapon meleeWeapon)
            {
                damageRoll = RandomHelper.GetRandomNumber(meleeWeapon.DamageRange[0], meleeWeapon.DamageRange[1]);
            }
            else if (heroWeapon is RangedWeapon rangedWeapon)
            {
                damageRoll = RandomHelper.GetRandomNumber(rangedWeapon.DamageRange[0], rangedWeapon.DamageRange[1]);
            }
            // If neither, damageRoll remains 0

            return damageRoll;
        }

        /// <summary>
        /// Calculates the weapon bonus for a melee attack.
        /// </summary>
        /// <param name="meleeWeapon">The melee weapon being used.</param>
        /// <param name="heroTalents">The list of talents the hero possesses.</param>
        /// <returns>The calculated bonus to the weapon's damage or to-hit.</returns>
        public int CalculateMeleeWeaponBonus(MeleeWeapon meleeWeapon, List<Talent> heroTalents)
        {
            int weaponBonus = 0;

            if (meleeWeapon == null)
            {
                return weaponBonus;
            }

            if (meleeWeapon.IsMithril)
            {
                weaponBonus += 1;
            }
            if (meleeWeapon.IsSlayerTreated && meleeWeapon.IsEdged)
            {
                weaponBonus += 1;
            }

            // Check if hero has the "Mighty Blow" talent
            if (heroTalents != null && heroTalents.Exists(t => t.IsMightyBlow))
            {
                weaponBonus += 1;
            }

            // Note: The original code had `!meleeWeapon.isFirstHit` which seems like a combat state.
            // This logic should perhaps be moved to the actual attack calculation method.
            // For now, including as is, assuming 'isFirstHit' is a property of the weapon itself.
            if (meleeWeapon.IsUnwieldly && !meleeWeapon.IsFirstHit)
            {
                weaponBonus -= 4;
            }

            return weaponBonus;
        }

        /// <summary>
        /// Calculates the weapon bonus for a ranged attack.
        /// </summary>
        /// <param name="rangedWeapon">The ranged weapon being used.</param>
        /// <returns>The calculated bonus to the weapon's damage or to-hit.</returns>
        public int CalculateRangedWeaponBonus(RangedWeapon rangedWeapon)
        {
            int weaponBonus = 0;

            if (rangedWeapon == null)
            {
                return weaponBonus;
            }

            if (rangedWeapon.Ammo != null && (rangedWeapon.Ammo.IsBarbed || rangedWeapon.Ammo.IsSupSlingstone))
            {
                weaponBonus = 1;
            }

            return weaponBonus;
        }

        // Add methods here to calculate total damage (DamageRoll + DamageBonus + WeaponBonus),
        // to-hit rolls (CombatSkill/RangedSkill, modifiers like isBehindTarget, isCharge, isAiming),
        // and other combat-related calculations.
        // These methods would take the Hero object and specific combat context (e.g., target, range, active stance flags) as parameters.

        // Example (conceptual, depends on full combat system):
        // public int CalculateMeleeToHit(Hero hero, bool isBehindTarget, bool isCharge, bool isPowerAttack, bool isDualWield, bool hasHeightAdvantage, bool isParryStance)
        // {
        //     int toHit = hero.CombatSkill;
        //     // Apply modifiers based on flags
        //     if (isBehindTarget) toHit += 5; // Example bonus
        //     // ... other modifiers
        //     return toHit;
        // }
    }
}