using LoDCompanion.Models.Character;

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

    }
}
