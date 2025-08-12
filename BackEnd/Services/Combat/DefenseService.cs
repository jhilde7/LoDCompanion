
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.GameData;

namespace LoDCompanion.BackEnd.Services.Combat
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
        public static async Task<DefenseResult> AttemptDodge(Hero hero, UserRequestService diceRoll)
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

            int dodgeSkill = hero.GetSkill(Skill.Dodge);
            if (hero.CombatStance == CombatStance.Parry)
            {
                dodgeSkill += 15; // Bonus for dodging from a Parry CombatStance
            }

            var rollResult = await diceRoll.RequestRollAsync("Attempt to dodge the attack.", "1d100", canCancel: true); await Task.Yield();
            if (!rollResult.WasCancelled)
            {
                int roll = rollResult.Roll;
                if (roll <= 80 && roll <= dodgeSkill)
                {
                    result.WasSuccessful = true;
                    result.OutcomeMessage = $"{hero.Name} successfully dodges the attack!";
                }
                else
                {
                    result.OutcomeMessage = $"{hero.Name} fails to dodge.";
                    hero.HasDodgedThisBattle = true; // Mark the dodge as used
                }
            }
            else
            {
                result.OutcomeMessage = "Dodge attempt canceled.";
            }
            return result;
        }

        /// <summary>
        /// Resolves a hero's parry attempt using a weapon.
        /// </summary>
        public static async Task<DefenseResult> AttemptWeaponParry(Hero hero, Weapon? weapon, UserRequestService diceRoll)
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

            if (weapon == null)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} does not have a melee weapon equipped." };
            }

            var rollResult = await diceRoll.RequestRollAsync("Attempt to parry the with your weapon.", "1d100"); await Task.Yield();
            int roll = rollResult.Roll;
            if (roll >= 95) // Fumble on 95-100
            {
                result.WeaponDamaged = true;
                result.OutcomeMessage = $"{hero.Name}'s parry fails and their {weapon.Name} is damaged!";
            }
            else if (roll <= 80 && roll <= hero.GetSkill(Skill.CombatSkill))
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
        public static async Task<DefenseResult> AttemptShieldParry(Hero hero, Shield shield, int incomingDamage, UserRequestService diceRoll)
        {
            var result = new DefenseResult();

            if (hero.IsVulnerableAfterPowerAttack)
            {
                return new DefenseResult { OutcomeMessage = $"{hero.Name} is vulnerable and cannot parry!" };
            }

            int parrySkill = hero.GetSkill(Skill.CombatSkill);

            if (hero.CombatStance == CombatStance.Parry)
            {
                parrySkill += 15;
            }
            else
            {
                parrySkill -= 15; // Penalty for parrying with a shield from a normal stance
            }

            var rollResult = await diceRoll.RequestRollAsync("Attempt to parry the blow with your shield", "1d100"); await Task.Yield();
            int roll = rollResult.Roll;
            if (roll <= 80 && roll <= parrySkill)
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
