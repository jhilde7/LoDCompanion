using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Combat
{
    public static class StatusEffectService
    {
        public static List<ActiveStatusEffect> StatusEffects => GetStatusEffects();

        /// <summary>
        /// Attempts to apply a status to a target, performing a CON test first.
        /// </summary>
        public static void AttemptToApplyStatus(Character target, StatusEffectType type)
        {
            if (target.ActiveStatusEffects.Any(e => e.Category == type)) return; // Already affected

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
        public static void ApplyStatus(Character target, StatusEffectType type, int duration)
        {
            // Prevent stacking the same effect.
            if (!target.ActiveStatusEffects.Any(e => e.Category == type))
            {
                target.ActiveStatusEffects.Add(new ActiveStatusEffect(type, duration));
                Console.WriteLine($"{target.Name} is now {type}!");
            }
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
                        // As per PDF, make a CON test. On fail, lose 1 HP.
                        if (character is Hero hero && !hero.ResistPoison())
                        {
                            character.TakeDamage(1);
                            Console.WriteLine($"{character.Name} takes 1 damage from poison.");
                        }
                        break;

                    case StatusEffectType.FireBurning:
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
                        Console.WriteLine($"{character.Name} is no longer {effect.Category}.");
                    }
                }
            }
        }


        public static List<ActiveStatusEffect> GetStatusEffects()
        {
            return new List<ActiveStatusEffect>()
            {
                new ActiveStatusEffect(StatusEffectType.FireBurning, 1),
                new ActiveStatusEffect(StatusEffectType.AcidBurning, 1),
                new ActiveStatusEffect(StatusEffectType.Poisoned, RandomHelper.GetRandomNumber(1, RandomHelper.RollDie("D10"))),
                new ActiveStatusEffect(StatusEffectType.Diseased, 0),
                new ActiveStatusEffect(StatusEffectType.Stunned, 1),
                new ActiveStatusEffect(StatusEffectType.Prone, 1),
                new ActiveStatusEffect(StatusEffectType.Pit, 0),
                // TODO: Update duration of below effects
                new ActiveStatusEffect(StatusEffectType.BleedingOut, 0),
                new ActiveStatusEffect(StatusEffectType.Fear, 0),
                new ActiveStatusEffect(StatusEffectType.Terror, 0),
                new ActiveStatusEffect(StatusEffectType.Entangled, 0),
                new ActiveStatusEffect(StatusEffectType.Petrified, 0),
                new ActiveStatusEffect(StatusEffectType.Incapacitated, 0),
                new ActiveStatusEffect(StatusEffectType.BeingSwallowed, 0),
                new ActiveStatusEffect(StatusEffectType.Swallowed, 0),
                new ActiveStatusEffect(StatusEffectType.Ensnared, 0),
                new ActiveStatusEffect(StatusEffectType.IgnoreWounds, 0),
                new ActiveStatusEffect(StatusEffectType.Sprint, 0),
                new ActiveStatusEffect(StatusEffectType.BattleFury, 0),
                new ActiveStatusEffect(StatusEffectType.Frenzy, 0),
                new ActiveStatusEffect(StatusEffectType.HideInShadows, 0),
                new ActiveStatusEffect(StatusEffectType.MyWillBeDone, 0),
                new ActiveStatusEffect(StatusEffectType.NightVision, 0),
                new ActiveStatusEffect(StatusEffectType.PerfectHearing, 0),
                new ActiveStatusEffect(StatusEffectType.ResistanceToDisease, 0),
                new ActiveStatusEffect(StatusEffectType.ResistanceToPoison, 0),
                new ActiveStatusEffect(StatusEffectType.Tank, 0),
                new ActiveStatusEffect(StatusEffectType.Axeman, 0),
                new ActiveStatusEffect(StatusEffectType.Bruiser, 0),
                new ActiveStatusEffect(StatusEffectType.DeathLament, 0),
                new ActiveStatusEffect(StatusEffectType.DualWield, 0),
                new ActiveStatusEffect(StatusEffectType.FastReload, 0),
                new ActiveStatusEffect(StatusEffectType.Marksman, 0),
                new ActiveStatusEffect(StatusEffectType.MightyBlow, 0),
                new ActiveStatusEffect(StatusEffectType.ParryMaster, 0),
                new ActiveStatusEffect(StatusEffectType.PerfectShot, 0),
                new ActiveStatusEffect(StatusEffectType.RiposteMaster, 0),
                new ActiveStatusEffect(StatusEffectType.Sniper, 0),
                new ActiveStatusEffect(StatusEffectType.TunnelFighter, 0),
                new ActiveStatusEffect(StatusEffectType.Healer, 0),
                new ActiveStatusEffect(StatusEffectType.Messiah, 0),
                new ActiveStatusEffect(StatusEffectType.Pure, 0),
                new ActiveStatusEffect(StatusEffectType.Gatherer, 0),
                new ActiveStatusEffect(StatusEffectType.Harvester, 0),
                new ActiveStatusEffect(StatusEffectType.PerfectToss, 0),
                new ActiveStatusEffect(StatusEffectType.Disciplined, 0),
                new ActiveStatusEffect(StatusEffectType.Conjurer, 0),
                new ActiveStatusEffect(StatusEffectType.Divinator, 0),
                new ActiveStatusEffect(StatusEffectType.FastReflexes, 0),
                new ActiveStatusEffect(StatusEffectType.Focused, 0),
                new ActiveStatusEffect(StatusEffectType.Restorer, 0),
                new ActiveStatusEffect(StatusEffectType.Mystic, 0),
                new ActiveStatusEffect(StatusEffectType.PowerfulMissiles, 0),
                new ActiveStatusEffect(StatusEffectType.Summoner, 0),
                new ActiveStatusEffect(StatusEffectType.Sustainer, 0),
                new ActiveStatusEffect(StatusEffectType.Thrifty, 0),
                new ActiveStatusEffect(StatusEffectType.Assassin, 0),
                new ActiveStatusEffect(StatusEffectType.Backstabber, 0),
                new ActiveStatusEffect(StatusEffectType.LockPicker, 0),
                new ActiveStatusEffect(StatusEffectType.MechanicalGenius, 0),
                new ActiveStatusEffect(StatusEffectType.QuickFingers, 0),
                new ActiveStatusEffect(StatusEffectType.SharpEyed, 0),
                new ActiveStatusEffect(StatusEffectType.SenseForGold, 0),
                new ActiveStatusEffect(StatusEffectType.TrapFinder, 0),
                new ActiveStatusEffect(StatusEffectType.BraveHeart, 0),
                new ActiveStatusEffect(StatusEffectType.Fearless, 0),
                new ActiveStatusEffect(StatusEffectType.HateBandits, 0),
                new ActiveStatusEffect(StatusEffectType.HateBats, 0),
                new ActiveStatusEffect(StatusEffectType.HateBeastmen, 0),
                new ActiveStatusEffect(StatusEffectType.HateCentipedes, 0),
                new ActiveStatusEffect(StatusEffectType.HateDarkElves, 0),
                new ActiveStatusEffect(StatusEffectType.HateDemons, 0),
                new ActiveStatusEffect(StatusEffectType.HateDragons, 0),
                new ActiveStatusEffect(StatusEffectType.HateElementals, 0),
                new ActiveStatusEffect(StatusEffectType.HateFroglings, 0),
                new ActiveStatusEffect(StatusEffectType.HateGeckos, 0),
                new ActiveStatusEffect(StatusEffectType.HateGhosts, 0),
                new ActiveStatusEffect(StatusEffectType.HateGhouls, 0),
                new ActiveStatusEffect(StatusEffectType.HateGiants, 0),
                new ActiveStatusEffect(StatusEffectType.HateGnolls, 0),
                new ActiveStatusEffect(StatusEffectType.HateGoblins, 0),
                new ActiveStatusEffect(StatusEffectType.HateGolems, 0),
                new ActiveStatusEffect(StatusEffectType.HateMinotaurs, 0),
                new ActiveStatusEffect(StatusEffectType.HateMummies, 0),
                new ActiveStatusEffect(StatusEffectType.HateOgres, 0),
                new ActiveStatusEffect(StatusEffectType.HateOrcs, 0),
                new ActiveStatusEffect(StatusEffectType.HateRats, 0),
                new ActiveStatusEffect(StatusEffectType.HateSaurians, 0),
                new ActiveStatusEffect(StatusEffectType.HateScorpions, 0),
                new ActiveStatusEffect(StatusEffectType.HateSkeletons, 0),
                new ActiveStatusEffect(StatusEffectType.HateSnakes, 0),
                new ActiveStatusEffect(StatusEffectType.HateSpiders, 0),
                new ActiveStatusEffect(StatusEffectType.HateToads, 0),
                new ActiveStatusEffect(StatusEffectType.HateTrolls, 0),
                new ActiveStatusEffect(StatusEffectType.HateVampires, 0),
                new ActiveStatusEffect(StatusEffectType.HateWerewolves, 0),
                new ActiveStatusEffect(StatusEffectType.HateWights, 0),
                new ActiveStatusEffect(StatusEffectType.HateWolves, 0),
                new ActiveStatusEffect(StatusEffectType.HateZombies, 0),
                new ActiveStatusEffect(StatusEffectType.Claustrophobia, 0),
                new ActiveStatusEffect(StatusEffectType.Arachnophobia, 0),
                new ActiveStatusEffect(StatusEffectType.BadTempered, 0),
                new ActiveStatusEffect(StatusEffectType.Poverty, 0),
                new ActiveStatusEffect(StatusEffectType.TheFraud, 0),
                new ActiveStatusEffect(StatusEffectType.TheRealDeal, 0),
                new ActiveStatusEffect(StatusEffectType.TheNoble, 0),
                new ActiveStatusEffect(StatusEffectType.TheApprentice, 0),
                new ActiveStatusEffect(StatusEffectType.BringerOfLight, 0),
                new ActiveStatusEffect(StatusEffectType.PowerOfTheGods, 0),
                new ActiveStatusEffect(StatusEffectType.ThePowerOfIphy, 0),
                new ActiveStatusEffect(StatusEffectType.MetheiasWard, 0),
                new ActiveStatusEffect(StatusEffectType.LitanyOfMetheia, 0),
                new ActiveStatusEffect(StatusEffectType.SmiteTheHeretics, 0),
                new ActiveStatusEffect(StatusEffectType.ShieldOfTheGods, 0),
                new ActiveStatusEffect(StatusEffectType.PowerOfFaith, 0),
                new ActiveStatusEffect(StatusEffectType.VerseOfTheSane, 0),
                new ActiveStatusEffect(StatusEffectType.StrengthOfOhlnir, 0),
                new ActiveStatusEffect(StatusEffectType.StayThyHand, 0),
                new ActiveStatusEffect(StatusEffectType.ProvidenceOfMetheia, 0),
                new ActiveStatusEffect(StatusEffectType.WarriorOfRamos, 0),
                new ActiveStatusEffect(StatusEffectType.BeGone, 0),
                new ActiveStatusEffect(StatusEffectType.WeShallNotFalter, 0),
                new ActiveStatusEffect(StatusEffectType.GodsChampion, 0),
                new ActiveStatusEffect(StatusEffectType.ProtectiveShield, 0),
                new ActiveStatusEffect(StatusEffectType.FakeDeath, 0),
                new ActiveStatusEffect(StatusEffectType.GustOfWind, 0),
                new ActiveStatusEffect(StatusEffectType.StrengthenBody, 0),
                new ActiveStatusEffect(StatusEffectType.Silence, 0),
                new ActiveStatusEffect(StatusEffectType.Blur, 0),
                new ActiveStatusEffect(StatusEffectType.MagicArmour, 0),
                new ActiveStatusEffect(StatusEffectType.Slow, 0),
                new ActiveStatusEffect(StatusEffectType.Corruption, 0),
                new ActiveStatusEffect(StatusEffectType.ControlUndead, 0),
                new ActiveStatusEffect(StatusEffectType.Confuse, 0),
                new ActiveStatusEffect(StatusEffectType.HoldCreature, 0),
                new ActiveStatusEffect(StatusEffectType.IceTomb, 0),
                new ActiveStatusEffect(StatusEffectType.Weakness, 0),
                new ActiveStatusEffect(StatusEffectType.BolsteredMind, 0),
                new ActiveStatusEffect(StatusEffectType.CauseAnimosity, 0),
                new ActiveStatusEffect(StatusEffectType.Levitate, 0),
                new ActiveStatusEffect(StatusEffectType.Speed, 0),
                new ActiveStatusEffect(StatusEffectType.CauseFear, 0),
                new ActiveStatusEffect(StatusEffectType.CauseTerror, 0),
                new ActiveStatusEffect(StatusEffectType.Corrosive, 0),
                new ActiveStatusEffect(StatusEffectType.CursedWeapon, 0),
                new ActiveStatusEffect(StatusEffectType.Demon, 0),
                new ActiveStatusEffect(StatusEffectType.DiseaseRidden, 0),
                new ActiveStatusEffect(StatusEffectType.Ethereal, 0),
                new ActiveStatusEffect(StatusEffectType.WeakToFire, 0),
                new ActiveStatusEffect(StatusEffectType.WeakToWater, 0),
                new ActiveStatusEffect(StatusEffectType.WeakToSilver, 0),
                new ActiveStatusEffect(StatusEffectType.FearElves, 0),
                new ActiveStatusEffect(StatusEffectType.FerociousCharge, 0),
                new ActiveStatusEffect(StatusEffectType.FireDamage, 0),
                new ActiveStatusEffect(StatusEffectType.FrostDamage, 0),
                new ActiveStatusEffect(StatusEffectType.Floater, 0),
                new ActiveStatusEffect(StatusEffectType.Flyer, 0),
                new ActiveStatusEffect(StatusEffectType.GhostlyTouch, 0),
                new ActiveStatusEffect(StatusEffectType.Gust, 0),
                new ActiveStatusEffect(StatusEffectType.HardAsRock, 0),
                new ActiveStatusEffect(StatusEffectType.HateHero, 0),
                new ActiveStatusEffect(StatusEffectType.JustBones, 0),
                new ActiveStatusEffect(StatusEffectType.Large, 0),
                new ActiveStatusEffect(StatusEffectType.XLarge, 0),
                new ActiveStatusEffect(StatusEffectType.Leech, 0),
                new ActiveStatusEffect(StatusEffectType.MagicBeing, 0),
                new ActiveStatusEffect(StatusEffectType.MagicUser, 0),
                new ActiveStatusEffect(StatusEffectType.MultipleAttacksX, 0),
                new ActiveStatusEffect(StatusEffectType.MultipleAttacksHydra, 0),
                new ActiveStatusEffect(StatusEffectType.Poisonous, 0),
                new ActiveStatusEffect(StatusEffectType.Psychic, 0),
                new ActiveStatusEffect(StatusEffectType.Regeneration, 0),
                new ActiveStatusEffect(StatusEffectType.Rend, 0),
                new ActiveStatusEffect(StatusEffectType.RiddleMaster, 0),
                new ActiveStatusEffect(StatusEffectType.Scurry, 0),
                new ActiveStatusEffect(StatusEffectType.Silent, 0),
                new ActiveStatusEffect(StatusEffectType.SimpleWeapons, 0),
                new ActiveStatusEffect(StatusEffectType.Sneaky, 0),
                new ActiveStatusEffect(StatusEffectType.Stench, 0),
                new ActiveStatusEffect(StatusEffectType.Stupid, 0),
                new ActiveStatusEffect(StatusEffectType.WallCrawler, 0)
            };
        }

        public static ActiveStatusEffect GetStatusEffectByType(StatusEffectType type)
        {
            return StatusEffects.First(x => x.Category == type);
        }
    }
}
