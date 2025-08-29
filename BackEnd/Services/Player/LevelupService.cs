using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class Levelup
    {
        /// <summary>
        /// Points available to spend on improving stats or skills. Starts at 15.
        /// </summary>
        public int ImprovementPoints { get; set; }

        /// <summary>
        /// Tracks how many times each basic stat has been improved during this level-up.
        /// A stat cannot be increased more than 5 times per level (or +2 for Hit Points).
        /// </summary>
        public Dictionary<BasicStat, int> BasicStatsImprovedThisLevel { get; set; } = new Dictionary<BasicStat, int>();

        /// <summary>
        /// Tracks how many times each skill has been improved during this level-up.
        /// A skill cannot be increased more than 5 times per level.
        /// </summary>
        public Dictionary<Skill, int> SkillsImprovedThisLevel { get; set; } = new Dictionary<Skill, int>();

        /// <summary>
        /// The Talent chosen during this level-up.
        /// </summary>
        public Talent? SelectedTalent { get; set; }

        /// <summary>
        /// The Perk chosen during this level-up, if applicable.
        /// </summary>
        public Perk? SelectedPerk { get; set; }

        public Levelup() { }
    }

    public class LevelupService
    {
        private readonly GameDataService _gameData;
        private readonly PassiveAbilityService _passive;

        public LevelupService(GameDataService gameData, PassiveAbilityService passive)
        {
            _gameData = gameData;
            _passive = passive;
        }

        /// <summary>
        /// Captures the hero's saved points from their current level and carries them through to their next level while resetting level caps
        /// </summary>
        public void LevelUp(Hero hero)
        {
            if (hero.Level < hero.MaxLevel)
            {
                int previousLevelPoints = hero.Levelup.ImprovementPoints;
                hero.Levelup = new Levelup() { ImprovementPoints = 15 + previousLevelPoints };
                hero.Level++; 
            }
        }

        /// <summary>
        /// Attempts to improve a hero's basic stat.
        /// </summary>
        /// <param name="hero">The hero who is leveling up.</param>
        /// <param name="session">The current level-up session object.</param>
        /// <param name="statToImprove">The basic stat to increase.</param>
        /// <param name="profession">The hero's profession data, which contains cost info.</param>
        /// <param name="errorMessage">An output message explaining why the improvement failed.</param>
        /// <returns>True if the stat was improved successfully, otherwise false.</returns>
        public bool AttemptToImproveStat(Hero hero, Levelup session, BasicStat statToImprove, Profession profession, out string errorMessage)
        {
            int maxIncrease = statToImprove == BasicStat.HitPoints ? 2 : 5;
            session.BasicStatsImprovedThisLevel.TryGetValue(statToImprove, out int currentIncrease);

            // Rule: A stat cannot be increased more than +5 per level (+2 for HP).
            if (currentIncrease >= maxIncrease)
            {
                errorMessage = $"You cannot increase {statToImprove} more than {maxIncrease} times per level.";
                return false;
            }

            // Rule: Check for race-specific maximums.
            if (IsStatAtRacialMax(hero, statToImprove))
            {
                errorMessage = $"{statToImprove} is already at its maximum for a {hero.SpeciesName}.";
                return false;
            }

            int currentStatValue = hero.GetStat(statToImprove);
            int cost = GetStatImprovementCost(statToImprove, currentStatValue, profession);

            // Check if the hero has enough points.
            if (session.ImprovementPoints < cost)
            {
                errorMessage = $"Not enough improvement points. Needs {cost}, but you only have {session.ImprovementPoints}.";
                return false;
            }

            // All checks passed, apply the improvement.
            session.ImprovementPoints -= cost;
            hero.SetStat(statToImprove, currentStatValue + 1);
            session.BasicStatsImprovedThisLevel[statToImprove] = currentIncrease + 1;

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Attempts to improve a hero's skill.
        /// </summary>
        /// <param name="hero">The hero who is leveling up.</param>
        /// <param name="session">The current level-up session object.</param>
        /// <param name="skillToImprove">The skill to increase.</param>
        /// <param name="profession">The hero's profession data, which contains cost info.</param>
        /// <param name="errorMessage">An output message explaining why the improvement failed.</param>
        /// <returns>True if the skill was improved successfully, otherwise false.</returns>
        public bool AttemptToImproveSkill(Hero hero, Levelup session, Skill skillToImprove, Profession profession, out string errorMessage)
        {
            session.SkillsImprovedThisLevel.TryGetValue(skillToImprove, out int currentIncrease);

            // Rule: A skill cannot be increased by more than +5 per level.
            if (currentIncrease >= 5)
            {
                errorMessage = $"You cannot increase {skillToImprove} more than 5 times per level.";
                return false;
            }

            int currentSkillValue = hero.GetSkill(skillToImprove);

            // Rule: A skill can never exceed 80.
            if (currentSkillValue >= 80)
            {
                errorMessage = $"{skillToImprove} cannot be raised above 80.";
                return false;
            }

            int cost = GetSkillImprovementCost(skillToImprove, currentSkillValue, profession);

            if (session.ImprovementPoints < cost)
            {
                errorMessage = $"Not enough improvement points. Needs {cost}, but you only have {session.ImprovementPoints}.";
                return false;
            }

            // All checks passed, apply the improvement.
            session.ImprovementPoints -= cost;
            hero.SetSkill(skillToImprove, currentSkillValue + 1);
            session.SkillsImprovedThisLevel[skillToImprove] = currentIncrease + 1;

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Sets the chosen Talent for the level-up session.
        /// </summary>
        /// <returns>True if the talent was successfully selected, otherwise false.</returns>
        public bool AttemptToSelectTalent(Hero hero, Levelup session, Talent talent, out string errorMessage)
        {
            if (hero.Talents.Contains(talent))
            {
                errorMessage = "You already have this talent.";
                return false;
            }

            hero.Talents.Add(talent);
            session.SelectedTalent = talent;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Sets the chosen Perk for the level-up session.
        /// </summary>
        /// <returns>True if the perk was successfully selected, otherwise false.</returns>
        public bool AttemptToSelectPerk(Hero hero, Levelup session, Perk perk, out string errorMessage)
        {
            if (hero.Perks.Contains(perk))
            {
                errorMessage = "You already have this perk.";
                return false;
            }

            hero.Perks.Add(perk);
            session.SelectedPerk = perk;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Calculates the cost to improve a basic stat by 1.
        /// </summary>
        private int GetStatImprovementCost(BasicStat stat, int currentStatValue, Profession profession)
        {
            // The cost is determined by the hero's profession.
            profession.LevelUpCost.TryGetValue(GetKeyForStat(stat), out int baseCost);

            // Rule: Once a stat passes 70, the cost is doubled.
            if (currentStatValue >= 70)
            {
                baseCost *= 2;
            }

            return baseCost;
        }

        /// <summary>
        /// Calculates the cost to improve a skill by 1.
        /// </summary>
        private int GetSkillImprovementCost(Skill skill, int currentSkillValue, Profession profession)
        {
            // A mapping might be needed if the names don't align perfectly.
            profession.LevelUpCost.TryGetValue(GetKeyForSkill(skill), out int baseCost);

            // Rule: Once a skill passes 70, the cost is doubled.
            if (currentSkillValue >= 70)
            {
                baseCost *= 2;
            }

            return baseCost;
        }

        /// <summary>
        /// Checks if a stat has reached the maximum value for the hero's species.
        /// </summary>
        private bool IsStatAtRacialMax(Hero hero, BasicStat stat)
        {
            Species species = _gameData.GetSpeciesByName(hero.SpeciesName);
            int currentStatValue = hero.GetStat(stat);

            return stat switch
            {
                BasicStat.Wisdom => currentStatValue >= species.MaxWIS,
                BasicStat.Dexterity => currentStatValue >= species.MaxDEX,
                BasicStat.Strength => currentStatValue >= species.MaxSTR,
                BasicStat.Resolve => currentStatValue >= species.MaxRES,
                BasicStat.Constitution => currentStatValue >= species.MaxCON,
                _ => false,
            };
        }

        private string GetKeyForSkill(Skill skill)
        {
            return skill switch
            {
                Skill.CombatSkill => "CS",
                Skill.RangedSkill => "RS",
                Skill.PickLocks => "PickLocks",
                Skill.Dodge => "Dodge",
                Skill.Perception => "Perception",
                Skill.Heal => "Heal",
                Skill.ArcaneArts => "ArcaneArts",
                Skill.BattlePrayers => "BattlePrayers",
                Skill.Foraging => "Foraging",
                Skill.Barter => "Barter",
                Skill.Alchemy => "Alchemy",
                _ => skill.ToString()
            };
        }

        private string GetKeyForStat(BasicStat stat)
        {
            return stat switch
            {
                BasicStat.Strength => "STR",
                BasicStat.Dexterity => "DEX",
                BasicStat.Constitution => "CON",
                BasicStat.Resolve => "RES",
                BasicStat.Wisdom => "WIS",
                BasicStat.HitPoints => "HitPoints",
                _ => stat.ToString()
            };
        }

        public List<Talent>? GetTalentCategoryAtLevelup(string professionName, int level)
        {
            var profession = _gameData.Professions.FirstOrDefault(p => p.Name == professionName);
            if (profession == null) return null;

            switch (profession.Name)
            {
                case "Alchemist":
                    return level switch
                    {
                        3 => _passive.MentalTalents,
                        4 => _passive.CommonTalents,
                        6 => _passive.CombatTalents,
                        7 => _passive.MentalTalents,
                        8 => _passive.CommonTalents,
                        _ => _passive.AlchemistTalents
                    };
                case "Barbarian":
                    return level switch
                    {
                        2 => _passive.PhysicalTalents,
                        4 => _passive.MentalTalents,
                        5 => _passive.CommonTalents,
                        7 => _passive.PhysicalTalents,
                        9 => _passive.CommonTalents,
                        _ => _passive.CombatTalents
                    };
                case "Ranger":
                    return level switch
                    {
                        2 => _passive.PhysicalTalents,
                        4 => _passive.CommonTalents,
                        5 => _passive.MentalTalents,
                        7 => _passive.PhysicalTalents,
                        8 => _passive.CommonTalents,
                        9 => _passive.MentalTalents,
                        _ => _passive.CombatTalents
                    };
                case "Rogue":
                    return level switch
                    {
                        2 => _passive.PhysicalTalents,
                        3 => _passive.SneakyTalents,
                        5 => _passive.MentalTalents,
                        6 => _passive.PhysicalTalents,
                        8 => _passive.SneakyTalents,
                        9 => _passive.CommonTalents,
                        _ => _passive.CombatTalents
                    };
                case "Thief":
                    return level switch
                    {
                        3 => _passive.CommonTalents,
                        5 => _passive.CombatTalents,
                        6 => _passive.MentalTalents,
                        7 => _passive.PhysicalTalents,
                        8 => _passive.CommonTalents,
                        9 => _passive.CombatTalents,
                        _ => _passive.SneakyTalents
                    };
                case "Warrior":
                    return level switch
                    {
                        2 => _passive.MentalTalents,
                        4 => _passive.PhysicalTalents,
                        6 => _passive.CommonTalents,
                        7 => _passive.MentalTalents,
                        9 => _passive.CommonTalents,
                        _ => _passive.CombatTalents
                    };
                case "Warrior Priest":
                    return level switch
                    {
                        2 => _passive.MentalTalents,
                        4 => _passive.CombatTalents,
                        5 => _passive.PhysicalTalents,
                        7 => _passive.CombatTalents,
                        8 => _passive.MentalTalents,
                        10 => _passive.CombatTalents,
                        _ => _passive.FaithTalents
                    };
                case "Wizard":
                    return level switch
                    {
                        3 => _passive.CommonTalents,
                        4 => _passive.MentalTalents,
                        6 => _passive.MentalTalents,
                        7 => _passive.PhysicalTalents,
                        9 => _passive.CommonTalents,
                        10 => _passive.MentalTalents,
                        _ => _passive.MagicTalents
                    };
                default: return _passive.CommonTalents;
            }
        }

        public List<Perk>? GetPerkCategoryAtLevelup(string professionName, int level)
        {
            var profession = _gameData.Professions.FirstOrDefault(p => p.Name == professionName);
            if (profession == null) return null;
            switch (profession.Name)
            {
                case "Alchemist":
                    return level switch
                    {
                        2 => _gameData.AlchemistPerks,
                        4 => _gameData.LeaderPerks,
                        6 => _gameData.CombatPerks,
                        8 => _gameData.AlchemistPerks,
                        10 => _gameData.CommonPerks,
                        _ => null,
                    };
                case "Barbarian":
                    return level switch
                    {
                        2 => _gameData.CombatPerks,
                        4 => _gameData.CommonPerks,
                        6 => _gameData.CombatPerks,
                        8 => _gameData.CommonPerks,
                        10 => _gameData.CombatPerks,
                        _ => null,
                    };
                case "Ranger":
                    return level switch
                    {
                        2 => _gameData.CombatPerks,
                        4 => _gameData.CommonPerks,
                        6 => _gameData.CombatPerks,
                        8 => _gameData.CommonPerks,
                        10 => _gameData.CommonPerks,
                        _ => null,
                    };
                case "Rogue":
                    return level switch
                    {
                        2 => _gameData.CombatPerks,
                        4 => _gameData.SneakyPerks,
                        6 => _gameData.CommonPerks,
                        8 => _gameData.CombatPerks,
                        10 => _gameData.SneakyPerks,
                        _ => null,
                    };
                case "Thief":
                    return level switch
                    {
                        2 => _gameData.SneakyPerks,
                        4 => _gameData.CommonPerks,
                        6 => _gameData.SneakyPerks,
                        8 => _gameData.CombatPerks,
                        10 => _gameData.SneakyPerks,
                        _ => null,
                    };
                case "Warrior":
                    return level switch
                    {
                        2 => _gameData.LeaderPerks,
                        4 => _gameData.CombatPerks,
                        6 => _gameData.CombatPerks,
                        8 => _gameData.CommonPerks,
                        10 => _gameData.CombatPerks,
                        _ => null,
                    };
                case "Warrior Priest":
                    return level switch
                    {
                        2 => _gameData.FaithPerks,
                        4 => _gameData.LeaderPerks,
                        6 => _gameData.CombatPerks,
                        8 => _gameData.FaithPerks,
                        10 => _gameData.CommonPerks,
                        _ => null,
                    };
                case "Wizard":
                    return level switch
                    {
                        2 => _gameData.ArcanePerks,
                        4 => _gameData.LeaderPerks,
                        6 => _gameData.ArcanePerks,
                        8 => _gameData.CommonPerks,
                        10 => _gameData.ArcanePerks,
                        _ => null,
                    };
                default: return _gameData.CommonPerks;
            }
        }
    }
}
