namespace LoDCompanion.Models
{
    public class CombatContext
    {
        // General Modifiers
        public bool IsAttackingFromBehind { get; set; }
        public bool HasHeightAdvantage { get; set; }
        public bool IsTargetProne { get; set; }

        // Melee Specific Modifiers
        public bool IsChargeAttack { get; set; }
        public bool IsPowerAttack { get; set; }
        public bool ApplyUnwieldlyBonus { get; set; }

        // Ranged Specific Modifiers
        public bool HasAimed { get; set; }
        public int ObstaclesInLineOfSight { get; set; }

        // Target's State Modifiers
        public bool IsTargetInParryStance { get; set; }
        public bool DidTargetUsePowerAttackLastTurn { get; set; }
    }
}
