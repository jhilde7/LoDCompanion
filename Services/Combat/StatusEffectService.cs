using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Combat
{
    public class StatusEffectService
    {
        public StatusEffectService() { }

        /// <summary>
        /// Attempts to apply a status to a target, performing a CON test first.
        /// </summary>
        public void AttemptToApplyStatus(Character target, StatusEffectType type)
        {
            if (target.ActiveStatusEffects.Any(e => e.Type == type)) return; // Already affected

            bool resisted = false;
            if (target is Hero hero)
            {
                // Perform the CON test based on the effect type
                if (type == StatusEffectType.Poisoned) resisted = hero.ResistPoison();
                if (type == StatusEffectType.Diseased) resisted = hero.ResistDisease();
            }
            else
            {
                // Monsters might have a simpler resistance check
                resisted = RandomHelper.RollDie("D100") <= target.Constitution;
            }

            if (!resisted)
            {
                int duration = (type == StatusEffectType.Poisoned) ? RandomHelper.RollDie("D10") : -1; // -1 for permanent until cured
                ApplyStatus(target, type, duration);
            }
            else
            {
                Console.WriteLine($"{target.Name} resisted the {type} effect!");
            }
        }

        /// <summary>
        /// Applies a new status effect to a target character.
        /// </summary>
        public void ApplyStatus(Character target, StatusEffectType type, int duration)
        {
            // Prevent stacking the same effect.
            if (!target.ActiveStatusEffects.Any(e => e.Type == type))
            {
                target.ActiveStatusEffects.Add(new ActiveStatusEffect(type, duration));
                Console.WriteLine($"{target.Name} is now {type}!");
            }
        }

        /// <summary>
        /// Processes all active status effects for a character at the start of their turn.
        /// </summary>
        /// <param name="character">The character whose effects are to be processed.</param>
        public void ProcessStatusEffects(Character character)
        {
            // Use a copy of the list to avoid issues with modifying it while iterating.
            var effectsToProcess = character.ActiveStatusEffects.ToList();

            foreach (var effect in effectsToProcess)
            {
                switch (effect.Type)
                {
                    case StatusEffectType.Poisoned:
                        // As per PDF, make a CON test. On fail, lose 1 HP.
                        if (character is Hero hero && !hero.ResistPoison())
                        {
                            character.TakeDamage(1);
                            Console.WriteLine($"{character.Name} takes 1 damage from poison.");
                        }
                        break;

                    case StatusEffectType.Burning:
                        // As per PDF, Fire damage over time.
                        int fireDamage = RandomHelper.RollDie("D6") / 2;
                        character.TakeDamage(fireDamage);
                        Console.WriteLine($"{character.Name} takes {fireDamage} damage from burning.");
                        break;

                    case StatusEffectType.Stunned:
                        // Logic to reduce AP would be in CombatManagerService when the turn starts.
                        Console.WriteLine($"{character.Name} is stunned and loses an action.");
                        break;
                }

                // Decrease duration and remove if expired.
                if (effect.Duration > 0)
                {
                    effect.Duration--;
                    if (effect.Duration <= 0)
                    {
                        character.ActiveStatusEffects.Remove(effect);
                        Console.WriteLine($"{character.Name} is no longer {effect.Type}.");
                    }
                }
            }
        }
    }
}
