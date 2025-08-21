using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.Player;

namespace LoDCompanion.BackEnd.Services.Combat
{
    /// <summary>
    /// Defines the different status effects that can afflict a character.
    /// </summary>
    public enum StatusEffectType
    {
        NeedRest,
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
        Bellow,
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
        CharusWalkWithUs,
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
        Depression,
        Taunt,
        TasteForBlood,
        Encouragement,
        ShieldWall,
        StrikeToInjure,
        PowerfulBlow,
        StunningStrike,
        DispelMaster,
        InnerPower,
        Strength,
        Constitution,
        Dexterity,
        Wisdom,
        Resolve,
        Energy,
        DragonBreath,
        DragonSkin,
        FireProtection,
        Invisible,
        Pitcher,
        ForgedUnderPressure,
        PoisonGas,
        DetectedMimic
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
        public string? DiceToRoll { get; set; } // Optional dice notation for effects that require rolling dice.
        public bool RemoveAfterCombat { get; set; }
        public bool RemoveAfterNextBattle { get; set; }

        public ActiveStatusEffect(
            StatusEffectType type,
            int duration,
            (BasicStat, int)? statBonus = null,
            (Skill, int)? skillBonus = null,
            int? damage = null,
            string? diceToRoll = null,
            bool removeAfterCombat = false,
            bool removeAfterNextBattle = false)
        {
            Category = type;
            Duration = duration;
            StatBonus = statBonus;
            SkillBonus = skillBonus;
            Damage = damage;
            DiceToRoll = diceToRoll;
            RemoveAfterCombat = removeAfterCombat;
            RemoveAfterNextBattle = removeAfterNextBattle;
        }
    }

    public static class StatusEffectService
    {
        /// <summary>
        /// Attempts to apply a status to a target, performing a CON test first.
        /// </summary>
        public static async Task<string> AttemptToApplyStatusAsync(Character target, ActiveStatusEffect effect, PowerActivationService activation, int? resistRoll = null, Monster? monster = null)
        {
            if (target.ActiveStatusEffects.Any(e => e.Category == effect.Category)) return "Already affected";

            bool resisted = false;
            if (target is Hero hero)
            {
                // Perform the CON test based on the effect type
                if (effect.Category == StatusEffectType.Poisoned) resisted = hero.ResistPoison(resistRoll);
                if (effect.Category == StatusEffectType.Diseased) resisted = hero.ResistDisease(resistRoll);
                if (effect.Category == StatusEffectType.Fear && monster != null) resisted = await hero.ResistFearAsync(monster, activation, resistRoll);
                if (effect.Category == StatusEffectType.Terror && monster != null) resisted = await hero.ResistTerrorAsync(monster, activation, resistRoll);
            }

            if (target is Hero && (effect.Category == StatusEffectType.Incapacitated 
                || effect.Category == StatusEffectType.Petrified
                || effect.Category == StatusEffectType.Bellow)) 
                resisted = target.TestResolve(resistRoll ?? RandomHelper.RollDie(DiceType.D100));

            if (effect.Category == StatusEffectType.Prone && resistRoll != null)
                resisted = target.TestDexterity((int)resistRoll);

            if (!resisted)
            {
                if (effect.Category == StatusEffectType.Prone) target.CombatStance = CombatStance.Prone;
                if (effect.Category == StatusEffectType.Bellow) effect = new ActiveStatusEffect(StatusEffectType.Stunned, effect.Duration);
                return await ApplyStatusAsync(target, effect, activation);
            }
            else
            {
                return $"{target.Name} resisted the {effect.Category} effect!";
            }
        }

        /// <summary>
        /// Applies a new status effect to a target character.
        /// </summary>
        private static async Task<string> ApplyStatusAsync(Character target, ActiveStatusEffect effect, PowerActivationService activation)
        {
            target.ActiveStatusEffects.Add(effect);
            if (target is Hero hero)
            {
                switch (effect.Category)
                {
                    case StatusEffectType.ThePowerOfIphy:
                        foreach (var monster in hero.AfraidOfTheseMonsters)
                        {
                            bool resisted = false;
                            resisted = await hero.ResistFearAsync(monster, activation);
                            if (resisted) 
                            {
                                hero.AfraidOfTheseMonsters.Remove(monster);
                                Console.WriteLine($"{hero.Name} is no longer afraid of {monster.Name}");
                            }
                        }
                        break;
                    case StatusEffectType.PowerOfFaith: hero.AfraidOfTheseMonsters.Clear(); break;
                    case StatusEffectType.WeShallNotFalter: hero.Heal(5); break;
                };
            } 

            return $"{target.Name} is now has effect of {effect.Category}!";
        }

        /// <summary>
        /// Processes all active status effects for a character at the start of their turn.
        /// </summary>
        /// <param name="character">The character whose effects are to be processed.</param>
        public static async Task ProcessActiveStatusEffectsAsync(Character character, PowerActivationService activation)
        {
            // Use a copy of the list to avoid issues with modifying it while iterating.
            var effectsToProcess = character.ActiveStatusEffects.ToList();
            var _floatingText = new FloatingTextService();

            foreach (var effect in effectsToProcess)
            {
                switch (effect.Category)
                {
                    case StatusEffectType.Poisoned:
                        if (character is Hero hero && !hero.ResistPoison())
                        {
                            int poisonDamage = effect.Damage ??= 1; // Default to 1 damage if not specified
                            await character.TakeDamageAsync(poisonDamage, (_floatingText, character.Position), activation, ignoreAllArmour: true);
                            Console.WriteLine($"{character.Name} takes {poisonDamage} damage from poison.");
                        }
                        break;

                    case StatusEffectType.AcidBurning:
                    case StatusEffectType.FireBurning:
                        int burnDamage = effect.Damage ??= 1;
                        await character.TakeDamageAsync(burnDamage, (_floatingText, character.Position), activation, ignoreAllArmour: true);
                        Console.WriteLine($"{character.Name} takes {burnDamage} damage from burning.");
                        break;

                    case StatusEffectType.Stunned:
                        character.CurrentAP -= 1; // Lose an action point
                        Console.WriteLine($"{character.Name} is stunned and loses an action.");
                        break;

                    case StatusEffectType.Entangled:
                        // The hero takes escalating damage at the end of each turn they are entangled.
                        int damage = -effect.Duration; // duration controls the damage, e.g., 1 damage for 1 turn, 2 for 2 turns, etc.
                        await character.TakeDamageAsync(damage, (_floatingText, character.Position), activation);
                        effect.Duration--;
                        Console.WriteLine($"{character.Name} takes {damage} damage from being entangled.");
                        break;

                    case StatusEffectType.Seduce:
                        if (character is Hero heroToSave)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync(
                                "Roll a resolve test to resist the effects", "1d100",
                                stat: (heroToSave, BasicStat.Resolve));
                            await Task.Yield();
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
                        if (character is Hero heroToResist)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync(
                                "Roll a resolve test to resist the effects", "1d100",
                                stat: (heroToResist, BasicStat.Resolve)); 
                            await Task.Yield();
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
                        if (effect.Duration > 0 && character is Hero heroBeingSwallowed1)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync(
                                "Roll a strength test to resist the effects", "1d100",
                                stat: (heroBeingSwallowed1, BasicStat.Strength));
                            await Task.Yield();
                            int strTest1 = rollResult.Roll;
                            if (strTest1 <= heroBeingSwallowed1.GetStat(BasicStat.Strength))
                            {
                                heroBeingSwallowed1.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.BeingSwallowed);
                                Console.WriteLine($"{heroBeingSwallowed1.Name} breaks free from the creature's grasp!");
                                break;
                            }
                            Console.WriteLine($"{heroBeingSwallowed1.Name} struggles but can't break free!\n");
                            heroBeingSwallowed1.CurrentAP = 0;
                            break;
                        }
                        // Second STR test at half strength
                        else if (effect.Duration == 0 && character is Hero heroBeingSwallowed2)
                        {
                            var rollResult = await new UserRequestService().RequestRollAsync(
                                "Roll a strength test to resist the effects", "1d100", 
                                stat: (heroBeingSwallowed2, BasicStat.Strength)); 
                            await Task.Yield();
                            int strTest2 = rollResult.Roll;
                            if (strTest2 <= heroBeingSwallowed2.GetStat(BasicStat.Strength) / 2)
                            {
                                heroBeingSwallowed2.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.BeingSwallowed);
                                Console.WriteLine($"{heroBeingSwallowed2.Name} makes a last-ditch effort and escapes!");
                                break;
                            }
                            // Swallowed whole
                            heroBeingSwallowed2.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.BeingSwallowed);
                            heroBeingSwallowed2.Position = null;
                            heroBeingSwallowed2.CurrentAP = 0;
                            await AttemptToApplyStatusAsync(heroBeingSwallowed2, new ActiveStatusEffect(StatusEffectType.Swallowed, -1), activation);
                            Console.WriteLine($"{heroBeingSwallowed2.Name} is swallowed whole!");
                        }
                        break;

                    case StatusEffectType.MetheiasWard when character.CurrentHP < character.GetStat(BasicStat.HitPoints): character.CurrentHP += 1; 
                        break;
                    case StatusEffectType.LitanyOfMetheia when character.CurrentHP < character.GetStat(BasicStat.HitPoints):
                            var resultRoll = await new UserRequestService().RequestRollAsync("Roll for resolve test", "1d100"); await Task.Yield();
                            if (character.TestResolve(resultRoll.Roll)) character.CurrentHP += 1;
                        break;
                    case StatusEffectType.PoisonGas:
                            await character.TakeDamageAsync(RandomHelper.RollDie(DiceType.D3), (_floatingText, character.Position), activation, ignoreAllArmour: true);
                        break;
                    case StatusEffectType.Pit:
                        int roll = 0;
                        if (character is Hero)
                        {
                            hero = (Hero)character;
                            var rollResult = await new UserRequestService().RequestRollAsync(
                                "Roll a dexterity test in an attempt to climb out of the pit", "1d100",
                                stat: (hero, BasicStat.Dexterity));
                            roll = rollResult.Roll;
                        }
                        else
                        {
                            roll = RandomHelper.RollDie(DiceType.D100);
                        }

                        if (character.TestDexterity(roll))
                        {
                            RemoveActiveStatusEffect(character, effect);
                            character.CurrentAP = 0;
                            Console.WriteLine($"{character.Name} climbs out of the pit!");
                        }
                        else
                        {
                            // The character remains in the pit, no action points this turn.
                            character.CurrentAP = 0;
                            Console.WriteLine($"{character.Name} fails to climb out of the pit and remains trapped.");
                        }
                        break;
                    case StatusEffectType.DetectedMimic:
                        character.CurrentAP = 0; // No actions while Mimic is detected, andhas not taken any damage yet.
                        break;
                }

                // Decrease duration and remove if expired.
                if (effect.Duration > 0)
                {
                    effect.Duration--;
                    if (effect.Duration == 0)
                    {
                        RemoveActiveStatusEffect(character, effect);
                        Console.WriteLine($"{character.Name} is no longer {effect.Category}.");
                    }
                }
            }
        }

        public static void RemoveActiveStatusEffect(Character character, ActiveStatusEffect effect)
        {
            character.ActiveStatusEffects.Remove(effect);
            if (character.CombatStance == CombatStance.Prone) character.CombatStance = CombatStance.Normal;
        }
    }
}
