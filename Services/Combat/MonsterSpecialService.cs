using LoDCompanion.Utilities;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Services.GameData;
using System;

namespace LoDCompanion.Services.Combat
{
    public enum SpecialActiveAbility
    {
        Bellow,
        Camouflage,
        Entangle,
        FireBreath,
        GhostlyHowl,
        MasterOfTheDead,
        MultipleAttack,
        Petrify,
        PoisonSpit,
        Seduction,
        SummonChildren,
        TongueAttack,
        Swallow,
        SweepingStrike
    }

    public class MonsterSpecialService
    {

        public MonsterSpecialService()
        {
            
        }

        /// <summary>
        /// Executes a specific monster special ability.
        /// </summary>
        /// <param name="monster">The monster performing the special action.</param>
        /// <param name="heroes">The list of heroes targeted by the action.</param>
        /// <param name="abilityType">The type of special ability to execute (e.g., "Bellow", "FireBreath").</param>
        /// <returns>A string describing the outcome of the special action.</returns>
        public string ExecuteSpecialAbility(Monster monster, List<Hero> heroes, Hero target, SpecialActiveAbility abilityType)
        {
            switch (abilityType)
            {
                case SpecialActiveAbility.Bellow:
                    return Bellow(monster, heroes);
                case SpecialActiveAbility.Camouflage:
                    return Camouflage(monster, heroes);
                case SpecialActiveAbility.Entangle:
                    return Entangle(monster, heroes);
                case SpecialActiveAbility.FireBreath:
                    return FireBreath(monster, heroes);
                case SpecialActiveAbility.GhostlyHowl:
                    return GhostlyHowl(monster, heroes);
                case SpecialActiveAbility.MasterOfTheDead:
                    return MasterOfTheDead(monster, heroes);
                case SpecialActiveAbility.MultipleAttack:
                    // This one needs a specific count, assume it's passed with the monster's state or handled externally
                    // For now, let's use a default or an assumed property on monster.
                    int attackCount = 1; // Default
                    if (monster.SpecialRules.Contains("Multiple Attack x2")) attackCount = 2;
                    else if (monster.SpecialRules.Contains("Multiple Attack x3")) attackCount = 3;
                    // ... and so on for other counts
                    return MultipleAttack(monster, heroes, attackCount);
                case SpecialActiveAbility.Petrify:
                    return Petrify(monster, heroes);
                case SpecialActiveAbility.PoisonSpit:
                    return PoisonSpit(monster, heroes);
                case SpecialActiveAbility.Seduction:
                    return Seduction(monster, heroes);
                case SpecialActiveAbility.SummonChildren:
                    return SummonChildren(monster, heroes);
                case SpecialActiveAbility.TongueAttack:
                    return TongueAttack(monster, heroes);
                case SpecialActiveAbility.Swallow:
                    return Swallow(monster, heroes);
                case SpecialActiveAbility.SweepingStrike:
                    return SweepingStrike(monster, heroes);
                default:
                    return $"{monster.Name} attempts an unknown special ability: {abilityType}. Nothing happens.";
            }
        }

        // --- Individual Special Ability Implementations ---

        public string Bellow(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} lets out a thunderous bellow!\n";
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < hero.Resolve) // Assuming lower is better for Resolve check
                {
                    StatusEffectService.AttemptToApplyStatus(hero, StatusEffectService.GetStatusEffectByType(StatusEffectType.Stunned)); // Assuming Status is a List<string> or similar
                    outcome += $"{hero.Name} is stunned by the roar!\n";
                }
                else
                {
                    outcome += $"{hero.Name} resists the bellow.\n";
                }
            }
            return outcome;
        }

        private string Camouflage(Monster monster, List<Hero> heroes)
        {
            //TODO
            throw new NotImplementedException();
        }

        private string Entangle(Monster monster, List<Hero> heroes)
        {
            //TODO
            throw new NotImplementedException();
        }

        public string FireBreath(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} unleashes a cone of fiery breath!\n";
            int damage = RandomHelper.GetRandomNumber(1, 6) + RandomHelper.GetRandomNumber(1, 6); // Example damage
            foreach (var hero in heroes)
            {
                hero.TakeDamage(damage);
                outcome += $"{hero.Name} takes {damage} fire damage.\n";
            }
            return outcome;
        }

        public string GhostlyHowl(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} emits a chilling, ghostly howl!\n";
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < (hero.Resolve * 2)) // Example check for fear/sanity loss
                {
                    hero.CurrentSanity -= 1; // Assuming Sanity exists and can be reduced
                    outcome += $"{hero.Name} loses 1 Sanity.\n";
                }
                else
                {
                    outcome += $"{hero.Name} remains unfazed.\n";
                }
            }
            return outcome;
        }

        public string Kick(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} attempts a powerful kick!\n";
            if (heroes.Count > 0)
            {
                var target = heroes[0]; // Assume single target for now, or pick nearest
                int damage = RandomHelper.GetRandomNumber(1, 6); // Example kick damage
                target.TakeDamage(damage);
                outcome += $"{target.Name} is kicked for {damage} damage.\n";
                // Add a chance to knock down, etc. based on original logic
            }
            return outcome;
        }

        public string MasterOfTheDead(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name}, the Master of the Dead, attempts to animate a corpse!\n";
            // This would involve creating a new Monster instance (e.g., Skeleton, Zombie)
            // and adding it to the current encounter or dungeon.
            // For simplicity, just a message for now.
            outcome += "A new undead minion rises to fight!\n"; // Placeholder for actual minion creation
            return outcome;
        }

        public string MultipleAttack(Monster monster, List<Hero> heroes, int attackCount)
        {
            string outcome = $"{monster.Name} unleashes {attackCount} rapid attacks!\n";
            //var monsterCombatService = new MonsterCombatService(this); // Assuming this service is available or injected
            for (int i = 0; i < attackCount; i++)
            {
                if (heroes.Count > 0)
                {
                    var target = heroes[RandomHelper.GetRandomNumber(0, heroes.Count - 1)]; // Target a random hero
                    // This would call the regular attack logic from MonsterCombatService
                    // For example: monsterCombatService.ProcessPhysicalAttack(monster, target, monster.Weapons[0]);
                    int damage = RandomHelper.GetRandomNumber(1, 6) + monster.DamageBonus; // Simplified damage
                    target.TakeDamage(damage);
                    outcome += $"  Attack {i + 1}: {monster.Name} hits {target.Name} for {damage} damage.\n";
                }
            }
            return outcome;
        }

        public string Petrify(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} attempts to turn its targets to stone!\n";
            foreach (var hero in heroes)
            {
                int conRoll = RandomHelper.GetRandomNumber(1, 20);
                if (conRoll < hero.Constitution) // Example: Constitution check to resist petrification
                {
                    StatusEffectService.AttemptToApplyStatus(hero, StatusEffectService.GetStatusEffectByType(StatusEffectType.Petrified));
                    outcome += $"{hero.Name} is turned to stone!\n";
                }
                else
                {
                    outcome += $"{hero.Name} resists the petrifying gaze.\n";
                }
            }
            return outcome;
        }

        public string PoisonSpit(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} spits corrosive poison!\n";
            if (heroes.Count > 0)
            {
                var target = heroes[0]; // Assume single target
                int damage = RandomHelper.GetRandomNumber(1, 4); // Initial damage
                target.TakeDamage(damage);
                StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Poisoned));
                outcome += $"{target.Name} is hit for {damage} damage and is poisoned!\n";
            }
            return outcome;
        }

        public string Regenerate(Monster monster, List<Hero> heroes)
        {
            int regenAmount = RandomHelper.GetRandomNumber(1, 6); // Example regeneration amount
            monster.CurrentHP += regenAmount;
            if (monster.CurrentHP > monster.MaxHP) monster.CurrentHP = monster.MaxHP;
            return $"{monster.Name} regenerates {regenAmount} HP!\n";
        }

        public string RiddleMaster(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} poses a perplexing riddle!\n";
            // This would typically involve a UI interaction and a wisdom/intellect check.
            // For now, a placeholder.
            outcome += "The heroes must answer correctly or face consequences...\n";
            return outcome;
        }

        public string Seduction(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} attempts to charm a hero!\n";
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < hero.Resolve) // Example: Resolve check to resist charm
                {
                    //hero.ActiveStatusEffect.Add("Charmed"); // Or some other effect
                    outcome += $"{hero.Name} is charmed by {monster.Name}!\n";
                }
                else
                {
                    outcome += $"{hero.Name} resists the charm.\n";
                }
            }
            return outcome;
        }

        public string Stupid(Monster monster, List<Hero> heroes)
        {
            // This typically implies a debuff on the monster, or a special action that fails.
            // If it's a monster's "action", it usually means they do nothing or a simple attack.
            return $"{monster.Name} stares blankly. It's too stupid to do anything complex this turn, simply attacks if able.\n";
        }

        public string SummonChildren(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} calls forth its vile offspring!\n";
            // Similar to MasterOfTheDead, this would involve creating new Monster instances.
            outcome += "Several smaller monsters emerge!\n"; // Placeholder
            return outcome;
        }

        public string TongueAttack(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} lashes out with a sticky tongue!\n";
            if (heroes.Count > 0)
            {
                var target = heroes[0]; // Assume single target
                int damage = RandomHelper.GetRandomNumber(1, 4);
                target.TakeDamage(damage);
                outcome += $"{target.Name} is hit for {damage} damage and ensnared by the tongue!\n";
                StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Ensnared)); // Apply status effect
            }
            return outcome;
        }

        public string Swallow(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} attempts to swallow a hero whole!\n";
            if (heroes.Count > 0)
            {
                var target = heroes[0]; // Assume single target
                // This would involve a grapple/strength check and potentially instant death or heavy damage
                int swallowRoll = RandomHelper.GetRandomNumber(1, 20);
                if (swallowRoll > target.Dexterity) // Example: Dexterity check to avoid being swallowed
                {
                    StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.BeingSwallowed)); // Apply swallowed status (e.g., for ongoing damage)
                    outcome += $"{target.Name} is swallowed by {monster.Name}!\n";
                }
                else
                {
                    outcome += $"{target.Name} narrowly avoids being swallowed.\n";
                }
            }
            return outcome;
        }

        public string SweepingStrike(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} performs a wide sweeping strike!\n";
            int damage = RandomHelper.GetRandomNumber(1, 8); // Example AOE damage
            foreach (var hero in heroes)
            {
                hero.TakeDamage(damage);
                outcome += $"{hero.Name} takes {damage} damage from the sweeping strike.\n";
            }
            return outcome;
        }

        public string ApplyFear(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} emanates an aura of fear!\n";
            int fearLevel = 1; // Default, or get from monster.FearLevel property
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < (hero.Resolve - fearLevel)) // Example: Resolve vs. FearLevel
                {
                    StatusEffectService.AttemptToApplyStatus(hero, StatusEffectService.GetStatusEffectByType(StatusEffectType.Fear));
                    outcome += $"{hero.Name} is gripped by fear!\n";
                }
                else
                {
                    outcome += $"{hero.Name} resists the fear.\n";
                }
            }
            return outcome;
        }

        public string ApplyTerror(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} inspires abject terror!\n";
            int terrorLevel = 2; // Default, or get from monster.TerrorLevel property
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < (hero.Resolve - terrorLevel)) // Example: Resolve vs. TerrorLevel
                {
                    StatusEffectService.AttemptToApplyStatus(hero, StatusEffectService.GetStatusEffectByType(StatusEffectType.Terror));
                    outcome += $"{hero.Name} is terrified and tries to flee!\n";
                }
                else
                {
                    outcome += $"{hero.Name} manages to overcome the terror.\n";
                }
            }
            return outcome;
        }

        internal List<SpecialActiveAbility> GetSpecialAttacks(List<string> specialRules)
        {
            var activeAbilities = new List<SpecialActiveAbility>();

            if (specialRules == null)
            {
                return activeAbilities;
            }

            foreach (string ruleString in specialRules)
            {
                if (Enum.TryParse<SpecialActiveAbility>(ruleString, true, out var ability))
                {
                    activeAbilities.Add(ability);
                }
            }

            return activeAbilities;
        }
    }
}