using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Combat
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
        FireBurning,
        AcidBurning,
        Prone,
        Pit,
        Fear,
        Terror,
        Entangled,
        Petrified,
        Incapacitated,
        BeingSwallowed,
        Swallowed,
        Ensnared,
        //--Perks--
        IgnoreWounds,
        Sprint,
        BattleFury,
        Frenzy,
        HideInShadows,
        MyWillBeDone,        
        //--Prayers--
        BringerOfLight,
        PowerOfTheGods,
        ThePowerOfIphy,
        MetheiasWard,
        LitanyOfMetheia,
        SmiteTheHeretics,
        ShieldOfTheGods,
        PowerOfFaith,
        VerseOfTheSane,
        StrengthOfOhlnir,
        StayThyHand,
        ProvidenceOfMetheia,
        WarriorOfRamos,
        BeGone,
        WeShallNotFalter,
        GodsChampion,
        //--Spells--
        ProtectiveShield,
        FakeDeath,
        GustOfWind,
        StrengthenBodyStrength,
        StrengthenBodyConstitution,
        Silence,
        Blur,
        MagicArmour,
        Corruption,
        ControlUndead,
        Confuse,
        HoldCreature,
        IceTomb,
        BolsteredMind,
        CauseAnimosity,
        Levitate,
        Speed,
        //--MonsterSpells--
        Blind,
        Shield,
        GustOfWindAura,
        MuteAura,
        RaiseDead,
        Summoned,
        MirroredSelf,
        Seduce,
        Slow,
        Weakness,
        //--Psychology--
        HateBandits,
        HateBats,
        HateBeastmen,
        HateCentipedes,
        HateDarkElves,
        HateDemons,
        HateDragons,
        HateDwarves,
        HateElementals,
        HateFroglings,
        HateGeckos,
        HateGhosts,
        HateGhouls,
        HateGiants,
        HateGnolls,
        HateGoblins,
        HateGolems,
        HateMinotaurs,
        HateMummies,
        HateOgres,
        HateOrcs,
        HateRats,
        HateSaurians,
        HateScorpions,
        HateSkeletons,
        HateSnakes,
        HateSpiders,
        HateToads,
        HateTrolls,
        HateVampires,
        HateWerewolves,
        HateWights,
        HateWolves,
        HateZombies,
        AcuteStessSyndrome,
        PostTraumaticStressDisorder, // PTSD
        PTSD_Trap,
        PTSD_PortcullisSlam,
        PTSD_AllyDeath,
        PTSD_SpellMiscast,
        PTSD_ShortRest,
        PTSD_OpenChest,
        FearDark,
        Arachnophobia,
        Jumpy,
        IrrationalFear,
        Claustrophobia,
        Depression
    }

    /// <summary>
    /// Represents an active status effect on a character, including its duration.
    /// </summary>
    public class ActiveStatusEffect
    {
        public StatusEffectType Category { get; set; }
        public int Duration { get; set; } // Duration in turns. -1 for permanent until cured.
        public int? StatBonus { get; set; } // Optional value for effects that change stats.
        public int? Damage { get; set; } // Optional value for effects that deal damage.
        public DiceType? DiceToRoll { get; set; } // Optional dice notation for effects that require rolling dice.

        public ActiveStatusEffect(StatusEffectType type, int duration)
        {
            Category = type;
            Duration = duration;
        }
    }

    public static class StatusEffectService
    {
        /// <summary>
        /// Attempts to apply a status to a target, performing a CON test first.
        /// </summary>
        public static void AttemptToApplyStatus(Character target, ActiveStatusEffect effect)
        {
            if (target.ActiveStatusEffects.Any(e => e.Category == effect.Category)) return; // Already affected

            bool resisted = false;
            if (target is Hero hero)
            {
                // Perform the CON test based on the effect type
                if (effect.Category == StatusEffectType.Poisoned) resisted = hero.ResistPoison();
                if (effect.Category == StatusEffectType.Diseased) resisted = hero.ResistDisease();
            }
            else
            {
                // Monsters might have a simpler resistance check
                resisted = RandomHelper.RollDie(DiceType.D100) <= target.GetStat(BasicStat.Constitution);
            }

            if (!resisted)
            {
                int duration = (effect.Category == StatusEffectType.Poisoned) ? RandomHelper.RollDie(DiceType.D10) : -1; // -1 for permanent until cured
                ApplyStatus(target, effect);
            }
            else
            {
                Console.WriteLine($"{target.Name} resisted the {effect.Category} effect!");
            }
        }

        /// <summary>
        /// Applies a new status effect to a target character.
        /// </summary>
        private static void ApplyStatus(Character target, ActiveStatusEffect effect)
        {
            target.ActiveStatusEffects.Add(effect);
            Console.WriteLine($"{target.Name} is now {effect.Category}!");
        }

        /// <summary>
        /// Processes all active status effects for a character at the start of their turn.
        /// </summary>
        /// <param name="character">The character whose effects are to be processed.</param>
        public static void ProcessStatusEffects(Character character)
        {
            // Use a copy of the list to avoid issues with modifying it while iterating.
            var effectsToProcess = character.ActiveStatusEffects.ToList();

            foreach (var effect in effectsToProcess)
            {
                switch (effect.Category)
                {
                    case StatusEffectType.Poisoned:
                        // make a CON test. On fail, lose 1 HP.
                        if (character is Hero hero && !hero.ResistPoison())
                        {
                            character.TakeDamage(1);
                            Console.WriteLine($"{character.Name} takes 1 damage from poison.");
                        }
                        break;

                    case StatusEffectType.FireBurning:
                        // Fire damage over time.
                        int fireDamage = RandomHelper.RollDie(DiceType.D6) / 2;
                        character.TakeDamage(fireDamage);
                        Console.WriteLine($"{character.Name} takes {fireDamage} damage from burning.");
                        break;

                    case StatusEffectType.Stunned:
                        // Logic to reduce AP would be in CombatManagerService when the turn starts.
                        Console.WriteLine($"{character.Name} is stunned and loses an action.");
                        break;

                    case StatusEffectType.Entangled:
                        // The hero takes escalating damage at the end of each turn they are entangled.
                        int damage = -effect.Duration; // duration controls the damage, e.g., 1 damage for 1 turn, 2 for 2 turns, etc.
                        character.TakeDamage(damage);
                        effect.Duration--;
                        Console.WriteLine($"{character.Name} takes {damage} damage from being entangled.");
                        break;
                }

                // Decrease duration and remove if expired.
                if (effect.Duration > 0)
                {
                    effect.Duration--;
                    if (effect.Duration == 0)
                    {
                        character.ActiveStatusEffects.Remove(effect);
                        Console.WriteLine($"{character.Name} is no longer {effect.Category}.");
                    }
                }
            }
        }
    }
}
