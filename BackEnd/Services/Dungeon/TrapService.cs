using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using System;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public class Trap
    {
        public string Name { get; set; }
        public int SkillModifier { get; set; } // Modifier for detecting/avoiding the trap
        public int DisarmModifier { get; set; } // Modifier for disarming the trap
        public string Description { get; set; }
        public string SpecialDescription { get; set; } // Description of the special effect if triggered
        public bool IsTriggered { get; set; }
        public bool IsDisarmed { get; set; }

        public Trap(string name, int skillModifier, int disarmModifier, string description, string specialDescription = "")
        {
            Name = name;
            SkillModifier = skillModifier;
            DisarmModifier = disarmModifier;
            Description = description;
            SpecialDescription = specialDescription;
        }

        // Static factory method to create common trap types
        public static Trap GetRandomTrap()
        {

            int roll = RandomHelper.RollDie(DiceType.D100);
            return roll switch
            {
                <= 20 => new Trap("Pit Trap", 0, 10, "A deep pit trap.", "Causes fall damage."),
                <= 40 => new Trap("Poison Dart Trap", 20, 15, "A trap that shoots poisoned darts.", "Causes poison damage."),
                <= 60 => new Trap("Spear Trap", 10, 10, "Hidden spears spring out from the walls.", "Causes piercing damage."),
                <= 80 => new Trap("Tripping Wire", 5, 5, "A nearly invisible wire across the floor.", "Causes hero to fall, potentially losing turn or taking minor damage."),
                _ => new Trap("Magic Rune Trap", 25, 20, "A glowing rune on the floor.", "Triggers a magical effect, e.g., a spell."),
            };

        }
    }

    public class TrapService
    {
        private readonly UserRequestService _diceRoll;
        private readonly PowerActivationService _powerActivation;

        public TrapService (UserRequestService diceRoll, PowerActivationService powerActivation)
        {
            _diceRoll = diceRoll;
            _powerActivation = powerActivation;
        }


        /// <summary>
        /// Checks if a hero successfully detects a trap based on their Perception.
        /// </summary>
        /// <param name="hero">The hero attempting to detect the trap.</param>
        /// <param name="trap">The trap to be detected.</param>
        /// <returns>True if the trap is detected, false otherwise.</returns>
        public bool DetectTrap(Hero hero, Trap trap)
        {
            int perceptionRoll = RandomHelper.RollDie(DiceType.D100);
            return perceptionRoll <= 80 && perceptionRoll <= hero.GetSkill(Skill.Perception) + trap.SkillModifier;
        }

        /// <summary>
        /// Attempts to disarm a detected trap using the hero's Pick Lock skill.
        /// </summary>
        /// <param name="hero">The hero attempting to disarm the trap.</param>
        /// <param name="trap">The trap to be disarmed.</param>
        /// <returns>True if the trap is successfully disarmed.</returns>
        public async Task<bool> DisarmTrapAsync(Hero hero, Trap trap)
        {
            var rollResult = await _diceRoll.RequestRollAsync("Roll pick locks test", "1d100");
            int trapDisarmTarget = hero.GetSkill(Skill.PickLocks) + trap.DisarmModifier;

            var cleverFingers = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.CleverFingers);
            if (cleverFingers != null) hero.ActiveStatusEffects.Remove(cleverFingers);

            if (rollResult.Roll <= 80 && rollResult.Roll <= trapDisarmTarget)
            {
                trap.IsDisarmed = true;
                return true;
            }
            else
            {
                // Failure to disarm sets off the trap.
                await TriggerTrapAsync(hero, trap);
                return false;
            }
        }

        /// <summary>
        /// Triggers the trap's effect on a hero. In a full implementation, this would apply damage or status effects.
        /// </summary>
        /// <param name="hero">The hero who triggered the trap.</param>
        /// <param name="trap">The trap that was triggered.</param>
        /// <returns>A string describing the outcome of the trap.</returns>
        public async Task<string> TriggerTrapAsync(Hero hero, Trap trap)
        {
            trap.IsTriggered = true;
            var perceptionSkill = hero.GetSkill(Skill.Perception);
            
            var sixthSense = hero.Perks.FirstOrDefault(p => p.Name == PerkName.SixthSense);
            if (sixthSense != null)
            {                
                if (await _diceRoll.RequestYesNoChoiceAsync($"Do you want to use {sixthSense.Name.ToString()} to add +20 to your chance to avoid the trap?") 
                    && (await _powerActivation.ActivatePerkAsync(hero, sixthSense)))
                {
                    perceptionSkill += 20;
                }
                await Task.Yield();
            }

            // In a real game loop, you would apply damage or status effects to the hero here.
            // For example: hero.TakeDamage(trap.Damage, (_floatingText, hero.Position));

            return $"{hero.Name} triggered the {trap.Name}! {trap.SpecialDescription}";
        }
    }
}
