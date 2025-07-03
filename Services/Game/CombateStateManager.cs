using LoDCompanion.Models.Character;
using LoDCompanion.Models;

namespace LoDCompanion.Services.Game
{
    public class CombateStateManager
    {
        // This set will store the unique ID of each character who has used their Unwieldly bonus in this combat.
        private HashSet<string> _unwieldlyBonusUsed = new HashSet<string>();

        public void StartCombat()
        {
            // At the start of every fight, clear the set. This is the only reset you need!
            _unwieldlyBonusUsed.Clear();
        }

        public int CalculateDamage(Hero attacker, MeleeWeapon weapon)
        {
            int totalDamage = 0; // Roll your base damage...

            // --- Unwieldly Bonus Logic ---
            // 1. Check if the weapon has the Unwieldly property.
            if (weapon.HasProperty(WeaponProperty.Unwieldly))
            {
                // 2. Check if the attacker has ALREADY used their bonus in this combat.
                if (!_unwieldlyBonusUsed.Contains(attacker.Id)) // Assuming Hero has a unique Id
                {
                    // 3. If not, apply the bonus and record that it has been used.
                    int bonus = weapon.GetPropertyValue(WeaponProperty.Unwieldly);
                    totalDamage += bonus;

                    // 4. Add the attacker's ID to the set so they can't get the bonus again this fight.
                    _unwieldlyBonusUsed.Add(attacker.Id);
                }
            }

            return totalDamage;
        }
    }
}
