namespace LoDCompanion.Models.Combat
{
    public class CombatContext
    {
        // General Modifiers
        public bool IsAttackingFromBehind { get; set; }
        public bool HasHeightAdvantage { get; set; }
        public bool IsTargetProne { get; set; }
        public bool IsFireDamage { get; set; } = false;
        public bool IsAcidicDamage { get; set; } = false;
        public bool IsFrostDamage { get; set; } = false;
        public bool IsPoisonousAttack { get; set; } = false;
        public bool CausesDisease { get; set; } = false;
        public int ArmourPiercingValue { get; set; } = 0;

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
