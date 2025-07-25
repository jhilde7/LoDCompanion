using LoDCompanion.Models.Character;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Services.Player
{
    public class Levelup
    {
        public int ImprovementPoints { get; set; } // Points used for improving stats or skills
        public Dictionary<BasicStat, int> BasicStatsImprovedThisLevel { get; set; } = new Dictionary<BasicStat, int>();
        public Dictionary<Skill, int> SkillsImprovedThisLevel { get; set; } = new Dictionary<Skill, int>();
    }

    public class LevelupService
    {
        private readonly GameDataService _gameData;

        public LevelupService(GameDataService gameData)
        {
            _gameData = gameData;
        }

        public List<Talent> GetTalentCategoryAtLevelup(Profession profession, int level)
        {

            switch (profession.Name)
            {
                case "Alchemist":
                    return level switch
                    {
                        3 => _gameData.MentalTalents,
                        4 => _gameData.CommonTalents,
                        6 => _gameData.CombatTalents,
                        7 => _gameData.MentalTalents,
                        8 => _gameData.CommonTalents,
                        _ => _gameData.AlchemistTalents
                    };
                case "Barbarian":
                    return level switch
                    {
                        2 => _gameData.PhysicalTalents,
                        4 => _gameData.MentalTalents,
                        5 => _gameData.CommonTalents,
                        7 => _gameData.PhysicalTalents,
                        9 => _gameData.CommonTalents,
                        _ => _gameData.CombatTalents
                    };
                case "Ranger":
                    return level switch
                    {
                        2 => _gameData.PhysicalTalents,
                        4 => _gameData.CommonTalents,
                        5 => _gameData.MentalTalents,
                        7 => _gameData.PhysicalTalents,
                        8 => _gameData.CommonTalents,
                        9 => _gameData.MentalTalents,
                        _ => _gameData.CombatTalents
                    };
                case "Rogue":
                    return level switch
                    {
                        2 => _gameData.PhysicalTalents,
                        3 => _gameData.SneakyTalents,
                        5 => _gameData.MentalTalents,
                        6 => _gameData.PhysicalTalents,
                        8 => _gameData.SneakyTalents,
                        9 => _gameData.CommonTalents,
                        _ => _gameData.CombatTalents
                    };
                case "Thief":
                    return level switch
                    {
                        3 => _gameData.CommonTalents,
                        5 => _gameData.CombatTalents,
                        6 => _gameData.MentalTalents,
                        7 => _gameData.PhysicalTalents,
                        8 => _gameData.CommonTalents,
                        9 => _gameData.CombatTalents,
                        _ => _gameData.SneakyTalents
                    };
                case "Warrior":
                    return level switch
                    {
                        2 => _gameData.MentalTalents,
                        4 => _gameData.PhysicalTalents,
                        6 => _gameData.CommonTalents,
                        7 => _gameData.MentalTalents,
                        9 => _gameData.CommonTalents,
                        _ => _gameData.CombatTalents
                    };
                case "Warrior Priest":
                    return level switch
                    {
                        2 => _gameData.MentalTalents,
                        4 => _gameData.CombatTalents,
                        5 => _gameData.PhysicalTalents,
                        7 => _gameData.CombatTalents,
                        8 => _gameData.MentalTalents,
                        10 => _gameData.CombatTalents,
                        _ => _gameData.FaithTalents
                    };
                case "Wizard":
                    return level switch
                    {
                        3 => _gameData.CommonTalents,
                        4 => _gameData.MentalTalents,
                        6 => _gameData.MentalTalents,
                        7 => _gameData.PhysicalTalents,
                        9 => _gameData.CommonTalents,
                        10 => _gameData.MentalTalents,
                        _ => _gameData.MagicTalents
                    };
                default: return _gameData.CommonTalents;
            }
        }

        public List<Perk>? GetPerkCategoryAtLevelup(Profession profession, int level)
        {
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
