using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Utilities;

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
        public static DefenseResult AttemptDodge(Hero hero)
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
            if (hero.Stance == CombatStance.Parry)
            {
                dodgeSkill += 15; // Bonus for dodging from a Parry Stance
            }

            int roll = RandomHelper.RollDie("D100");
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
        public static DefenseResult AttemptWeaponParry(Hero hero, Weapon weapon)
        {
            var result = new DefenseResult();
            if (hero.Stance != CombatStance.Parry)
            {
                result.OutcomeMessage = "Cannot parry with a weapon unless in a Parry Stance.";
                return result;
            }

            if (hero.IsVulnerableAfterPowerAttack)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} is vulnerable and cannot parry!" };
            }

            int roll = RandomHelper.RollDie("D100");
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
        public static DefenseResult AttemptShieldParry(Hero hero, Shield shield, int incomingDamage)
        {
            var result = new DefenseResult();

            if (hero.IsVulnerableAfterPowerAttack)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} is vulnerable and cannot parry!" };
            }

            int parrySkill = hero.CombatSkill;

            if (hero.Stance == CombatStance.Parry)
            {
                parrySkill += 15;
            }
            else
            {
                parrySkill -= 15; // Penalty for parrying with a shield from a normal stance
            }

            int roll = RandomHelper.RollDie("D100");
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
