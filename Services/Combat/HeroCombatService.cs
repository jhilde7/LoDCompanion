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
        public int CalculateDamageRoll(Weapon heroWeapon) 
        {
            return RandomHelper.GetRandomNumber(heroWeapon.MinDamage, heroWeapon.MaxDamage); ;
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

            // Check if hero has the "Mighty Blow" talent
            if (heroTalents != null && heroTalents.Exists(t => t.IsMightyBlow))
            {
                weaponBonus += 1;
            }

            // Note: The original code had `!meleeWeapon.isFirstHit` which seems like a combat state.
            // This logic should perhaps be moved to the actual attack calculation method.
            // For now, including as is, assuming 'isFirstHit' is a property of the weapon itself.
            if (meleeWeapon.HasProperty(WeaponProperty.Unwieldly) /*&& !meleeWeapon.IsFirstHit*/) //move this to Combat state manager
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

            if (rangedWeapon.Ammo != null && (rangedWeapon.Ammo.HasProperty(AmmoProperty.Barbed) || rangedWeapon.Ammo.HasProperty(AmmoProperty.SupuriorSlingStone)))
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