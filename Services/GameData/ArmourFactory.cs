using LoDCompanion.Models;

namespace LoDCompanion.Services.GameData
{
    public class ArmourFactory
    {
        public Armour CreateModifiedArmour(
            string baseArmourName,
            string newName,
            Action<Armour> modifications)
        {
            // 1. Find the base template.
            Armour template = EquipmentService.GetArmourByName(baseArmourName) ?? new Armour();

            Armour newArmour = template.Clone();
            newArmour.Name = newName;

            modifications(newArmour);

            return newArmour;
        }

        public Shield CreateModifiedShield(
            string baseShieldName,
            string newName,
            Action<Shield> modifications)
        {
            // 1. Find the base template.
            Shield template = EquipmentService.GetShieldByName(baseShieldName) ?? new Shield();

            Shield newShield = template.Clone();
            newShield.Name = newName;

            modifications(newShield);

            return newShield;
        }
    }
}
