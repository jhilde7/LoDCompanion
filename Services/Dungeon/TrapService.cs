using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    public class TrapService
    {

        /// <summary>
        /// Checks if a hero successfully detects a trap based on their Perception.
        /// </summary>
        /// <param name="hero">The hero attempting to detect the trap.</param>
        /// <param name="trap">The trap to be detected.</param>
        /// <returns>True if the trap is detected, false otherwise.</returns>
        public bool DetectTrap(Hero hero, Trap trap)
        {
            int perceptionRoll = RandomHelper.RollDie("D100");
            // The PDF mentions a modifier on the card next to the eye; we use the trap's SkillModifier for this.
            return perceptionRoll <= 80 && perceptionRoll <= (hero.GetSkill(Skill.Perception) + trap.SkillModifier);
        }

        /// <summary>
        /// Attempts to disarm a detected trap using the hero's Pick Lock skill.
        /// </summary>
        /// <param name="hero">The hero attempting to disarm the trap.</param>
        /// <param name="trap">The trap to be disarmed.</param>
        /// <returns>True if the trap is successfully disarmed.</returns>
        public bool DisarmTrap(Hero hero, Trap trap)
        {
            // Disarming uses the Pick Lock Skill, as per the PDF.
            // The modifier next to the cogs on the card corresponds to the trap's DisarmModifier.
            int disarmRoll = RandomHelper.RollDie("D100");
            if (disarmRoll <= 80 && disarmRoll <= (hero.GetSkill(Skill.PickLocks) + trap.DisarmModifier))
            {
                trap.IsDisarmed = true;
                trap.IsTrapped = false;
                return true;
            }
            else
            {
                // Failure to disarm sets off the trap.
                TriggerTrap(hero, trap);
                return false;
            }
        }

        /// <summary>
        /// Triggers the trap's effect on a hero. In a full implementation, this would apply damage or status effects.
        /// </summary>
        /// <param name="hero">The hero who triggered the trap.</param>
        /// <param name="trap">The trap that was triggered.</param>
        /// <returns>A string describing the outcome of the trap.</returns>
        public string TriggerTrap(Hero hero, Trap trap)
        {
            trap.IsTriggered = true;
            trap.IsTrapped = false;

            // In a real game loop, you would apply damage or status effects to the hero here.
            // For example: hero.TakeDamage(trap.Damage);

            return $"{hero.Name} triggered the {trap.Name}! {trap.SpecialDescription}";
        }
    }
}
