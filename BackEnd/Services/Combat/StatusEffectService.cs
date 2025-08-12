using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.Game;

namespace LoDCompanion.BackEnd.Services.Combat
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
        DeadlyStrike,
        Frenzy,
        PerfectAim,
        CleverFingers,
        HideInShadows,
        MyWillBeDone,
        //--Prayers--
        Disciplined,
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
        WarriorsOfRamos,
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
        public (BasicStat, int)? StatBonus { get; set; } // Optional value for effects that change stats.
        public (Skill, int)? SkillBonus { get; set; } // Optional value for effects that change skills.
        public int? Damage { get; set; } // Optional value for effects that deal damage.
        public DiceType? DiceToRoll { get; set; } // Optional dice notation for effects that require rolling dice.

        public ActiveStatusEffect(
            StatusEffectType type,
            int duration,
            (BasicStat, int)? statBonus = null,
            (Skill, int)? skillBonus = null,
            int? damage = null,
            DiceType? diceToRoll = null)
        {
            Category = type;
            Duration = duration;
            StatBonus = statBonus;
            SkillBonus = skillBonus;
            Damage = damage;
            DiceToRoll = diceToRoll;
        }
    }

    public static class StatusEffectService
    {
        /// <summary>
        /// Attempts to apply a status to a target, performing a CON test first.
        /// </summary>
        public static void AttemptToApplyStatus(Character target, ActiveStatusEffect effect, int? resistRoll = null)
        {
            if (target.ActiveStatusEffects.Any(e => e.Category == effect.Category)) return; // Already affected

            bool resisted = false;
            if (target is Hero hero)
            {
                // Perform the CON test based on the effect type
                if (effect.Category == StatusEffectType.Poisoned) resisted = hero.ResistPoison(resistRoll);
                if (effect.Category == StatusEffectType.Diseased) resisted = hero.ResistDisease(resistRoll);
            }

            if (!resisted)
            {
                ApplyStatus(target, effect);
                if (effect.Category == StatusEffectType.Prone) target.CombatStance = CombatStance.Prone;
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
        public static async Task ProcessStatusEffectsAsync(Character character)
        {
            // Use a copy of the list to avoid issues with modifying it while iterating.
            var effectsToProcess = character.ActiveStatusEffects.ToList();
            var _floatingText = new FloatingTextService();

            foreach (var effect in effectsToProcess)
            {
                switch (effect.Category)
                {
                    case StatusEffectType.Poisoned:
                        // make a CON test. On fail, lose 1 HP.
                        if (character is Hero hero && !hero.ResistPoison())
                        {
                            character.TakeDamage(1, (_floatingText, character.Position));
                            Console.WriteLine($"{character.Name} takes 1 damage from poison.");
                        }
                        break;

                    case StatusEffectType.FireBurning:
                        // Fire damage over time.
                        int fireDamage = RandomHelper.RollDie(DiceType.D6) / 2;
                        character.TakeDamage(fireDamage, (_floatingText, character.Position));
                        Console.WriteLine($"{character.Name} takes {fireDamage} damage from burning.");
                        break;

                    case StatusEffectType.Stunned:
                        // Logic to reduce AP would be in CombatManagerService when the turn starts.
                        Console.WriteLine($"{character.Name} is stunned and loses an action.");
                        break;

                    case StatusEffectType.Entangled:
                        // The hero takes escalating damage at the end of each turn they are entangled.
                        int damage = -effect.Duration; // duration controls the damage, e.g., 1 damage for 1 turn, 2 for 2 turns, etc.
                        character.TakeDamage(damage, (_floatingText, character.Position));
                        effect.Duration--;
                        Console.WriteLine($"{character.Name} takes {damage} damage from being entangled.");
                        break;

                    case StatusEffectType.Seduce:
                        if (character is Hero heroToSave)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync("Roll a resolve test to resist the effects", "1d100"); await Task.Yield();
                            int resolveRoll = rollResult.Roll;
                            if (resolveRoll <= heroToSave.GetStat(BasicStat.Resolve))
                            {
                                character.ActiveStatusEffects.Remove(effect);
                                Console.WriteLine($"{character.Name} breaks free from the seduction!");
                            }
                            else
                            {
                                // The hero remains incapacitated. The combat manager will handle skipping their turn.
                                Console.WriteLine($"{character.Name} remains seduced.");
                            }
                        }
                        break;

                    case StatusEffectType.Incapacitated:
                        if (character is Hero)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync("Roll a resolve test to resist the effects", "1d100"); await Task.Yield();
                            int resolveRoll = rollResult.Roll;
                            if (resolveRoll <= character.GetStat(BasicStat.Resolve))
                            {
                                character.ActiveStatusEffects.Remove(effect);
                                Console.WriteLine($"{character.Name} is no longer incapacitated!");
                            }
                            else
                            {
                                // The hero remains incapacitated. The combat manager will handle skipping their turn.
                                Console.WriteLine($"{character.Name} remains incapacitated.");
                            }
                        }
                        break;

                    case StatusEffectType.Swallowed:
                        character.CurrentAP = 0; // No actions while swallowed
                        break;

                    case StatusEffectType.BeingSwallowed:
                        // First STR test
                        if (effect.Duration > 0)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync("Roll a resolve test to resist the effects", "1d100"); await Task.Yield();
                            int strTest1 = rollResult.Roll;
                            if (strTest1 <= character.GetStat(BasicStat.Strength))
                            {
                                character.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.BeingSwallowed);
                                Console.WriteLine($"{character.Name} breaks free from the creature's grasp!");
                                break;
                            }
                            Console.WriteLine($"{character.Name} struggles but can't break free!\n");
                            character.CurrentAP = 0;
                            break;
                        }
                        // Second STR test at half strength
                        else if (effect.Duration == 0)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync("Roll a resolve test to resist the effects", "1d100"); await Task.Yield();
                            int strTest2 = rollResult.Roll;
                            if (strTest2 <= character.GetStat(BasicStat.Strength) / 2)
                            {
                                character.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.BeingSwallowed);
                                Console.WriteLine($"{character.Name} makes a last-ditch effort and escapes!");
                                break;
                            }
                            // Swallowed whole
                            character.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.BeingSwallowed);
                            character.Position = null;
                            character.CurrentAP = 0;
                            AttemptToApplyStatus(character, new ActiveStatusEffect(StatusEffectType.Swallowed, -1));
                            Console.WriteLine($"{character.Name} is swallowed whole!");
                        }
                        break;

                        case StatusEffectType.IgnoreWounds:
                        character.BasicStats[BasicStat.NaturalArmour] += 2; // Temporary bonus to NA

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
