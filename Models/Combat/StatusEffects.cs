namespace LoDCompanion.Models.Combat
{

    /// <summary>
    /// Defines the different status effects that can afflict a character.
    /// </summary>
    public enum StatusEffectType
    {
        Poisoned,
        Diseased,
        Stunned,
        BleedingOut,
        Burning
    }

    /// <summary>
    /// Represents an active status effect on a character, including its duration.
    /// </summary>
    public class ActiveStatusEffect
    {
        public StatusEffectType Type { get; set; }
        public int Duration { get; set; } // Duration in turns. -1 for permanent until cured.

        public ActiveStatusEffect(StatusEffectType type, int duration)
        {
            Type = type;
            Duration = duration;
        }
    }
}
