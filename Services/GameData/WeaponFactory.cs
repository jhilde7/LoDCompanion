using LoDCompanion.Models;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.GameData
{
    public class WeaponFactory
    {
        /// <summary>
        /// Creates a new weapon instance from a base template and applies modifications.
        /// </summary>
        /// <param name="baseWeaponName">The name of the weapon to use as a template.</param>
        /// <param name="newName">The new name for the modified weapon.</param>
        /// <param name="modifications">An action to apply custom changes to the new weapon.</param>
        /// <returns>A new, modified MeleeWeapon instance, or null if the base weapon doesn't exist.</returns>
        public MeleeWeapon CreateModifiedMeleeWeapon(
            string baseWeaponName,
            string newName,
            Action<MeleeWeapon> modifications)
        {
            var template = EquipmentService.GetWeaponByName(baseWeaponName) as MeleeWeapon ?? new MeleeWeapon();

            var newWeapon = template.Clone();

            newWeapon.Name = newName;

            modifications(newWeapon);

            return newWeapon;
        }

        /// <summary>
        /// Creates a new, randomized Magic Staff by combining a base staff with magical properties.
        /// </summary>
        /// <returns>A fully constructed MagicStaff object, or null if the base staff is not found.</returns>
        public MagicStaff CreateMagicStaff(MagicStaff magicStaffTemplate)
        {
            Weapon baseStaff = EquipmentService.GetWeaponByName("Staff") ?? new MeleeWeapon();

            MagicStaff newMagicStaff = new MagicStaff
            {
                // --- Properties from the base Staff ---
                MinDamage = baseStaff.MinDamage,
                MaxDamage = baseStaff.MaxDamage,
                DamageDice = baseStaff.DamageDice,
                Encumbrance = baseStaff.Encumbrance,
                Class = baseStaff.Class,
                Properties = new Dictionary<WeaponProperty, int>(baseStaff.Properties), // Copy the dictionary

                // --- Properties from the selected MagicStaff template ---
                Category = magicStaffTemplate.Category,
                Name = magicStaffTemplate.Name,
                Description = magicStaffTemplate.Description,
                Value = magicStaffTemplate.Value,
                Availability = magicStaffTemplate.Availability,
                StaffType = magicStaffTemplate.StaffType,
                ContainedSpell = magicStaffTemplate.ContainedSpell,
                MagicStaffProperties = new Dictionary<MagicStaffProperty, int>(magicStaffTemplate.MagicStaffProperties ?? new Dictionary<MagicStaffProperty, int>())
            };

            return newMagicStaff;
        }
    }
}
