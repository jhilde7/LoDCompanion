using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Utilities;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace LoDCompanion.Services.Combat
{
    public class DefenseResult
    {
        public bool WasSuccessful { get; set; }
        public int DamageNegated { get; set; }
        public bool WeaponDamaged { get; set; }
        public bool ShieldDamaged { get; set; }
        public string OutcomeMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles hero defensive actions like dodging and parrying.
    /// </summary>
    public static class DefenseService
    {

        /// <summary>
        /// Resolves a hero's dodge attempt against an incoming attack.
        /// </summary>
        public static async Task<DefenseResult> AttemptDodge(Hero hero, DiceRollService diceRoll)
        {
            var result = new DefenseResult();
            if (hero.HasDodgedThisBattle)
            {
                result.OutcomeMessage = $"{hero.Name} has already dodged this battle.";
                return result;
            }

            if (hero.IsVulnerableAfterPowerAttack)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} is vulnerable and cannot dodge!" };
            }

            int dodgeSkill = hero.Dodge;
            if (hero.CombatStance == CombatStance.Parry)
            {
                dodgeSkill += 15; // Bonus for dodging from a Parry CombatStance
            }

            int roll = await diceRoll.RequestRollAsync("Attempt to dodge the attack.", "1d100");
            if (roll <= dodgeSkill)
            {
                result.WasSuccessful = true;
                result.OutcomeMessage = $"{hero.Name} successfully dodges the attack!";
                hero.HasDodgedThisBattle = true; // Mark the dodge as used
            }
            else
            {
                result.OutcomeMessage = $"{hero.Name} fails to dodge.";
            }
            return result;
        }

        /// <summary>
        /// Resolves a hero's parry attempt using a weapon.
        /// </summary>
        public static async Task<DefenseResult> AttemptWeaponParry(Hero hero, Weapon? weapon, DiceRollService diceRoll)
        {
            var result = new DefenseResult();
            if (hero.CombatStance != CombatStance.Parry)
            {
                new DefenseResult { OutcomeMessage = "Cannot parry with a weapon unless in a Parry CombatStance." };
            }

            if (hero.IsVulnerableAfterPowerAttack)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} is vulnerable and cannot parry!" };
            }

            if(weapon == null)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} does not have a melee weapon equipped." };
            }

            int roll = await diceRoll.RequestRollAsync("Attempt to parry the with your weapon.", "1d100");
            if (roll >= 95) // Fumble on 95-100
            {
                result.WeaponDamaged = true;
                result.OutcomeMessage = $"{hero.Name}'s parry fails and their {weapon.Name} is damaged!";
            }
            else if (roll <= hero.CombatSkill)
            {
                result.WasSuccessful = true;
                result.OutcomeMessage = $"{hero.Name} masterfully parries the blow with their {weapon.Name}!";
            }
            else
            {
                result.OutcomeMessage = $"{hero.Name} fails to parry with their {weapon.Name}.";
            }
            return result;
        }

        /// <summary>
        /// Resolves a hero's parry attempt using a shield.
        /// </summary>
        public static async Task<DefenseResult> AttemptShieldParry(Hero hero, Shield shield, int incomingDamage, DiceRollService diceRoll)
        {
            var result = new DefenseResult();

            if (hero.IsVulnerableAfterPowerAttack)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} is vulnerable and cannot parry!" };
            }

            int parrySkill = hero.CombatSkill;

            if (hero.CombatStance == CombatStance.Parry)
            {
                parrySkill += 15;
            }
            else
            {
                parrySkill -= 15; // Penalty for parrying with a shield from a normal stance
            }

            int roll = await diceRoll.RequestRollAsync("Attempt to parry the blow with your shield", "1d100");
            if (roll <= parrySkill)
            {
                result.WasSuccessful = true;
                result.DamageNegated = shield.DefValue;
                result.OutcomeMessage = $"{hero.Name} blocks with their shield, negating {shield.DefValue} damage.";

                // Check if the shield takes damage (spillover)
                if (incomingDamage > shield.DefValue)
                {
                    result.ShieldDamaged = true;
                    result.OutcomeMessage += " The shield takes the brunt of the force and is damaged!";
                }
            }
            else
            {
                result.OutcomeMessage = $"{hero.Name} fails to block with their shield.";
            }
            return result;
        }
    }
}
