
namespace LoDCompanion.Models.Characters
{
    public class Character
    {
        public string Name { get; set; } = string.Empty; // Default to empty string for safety
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Strength { get; set; }
        public int Constitution { get; set; }
        public int Dexterity { get; set; }
        public int Wisdom { get; set; }
        public int Resolve { get; set; }
        public int CombatSkill { get; set; }
        public int RangedSkill { get; set; }
        public int Dodge { get; set; }
        public int Level { get; set; } 
        public int NaturalArmour { get; set; } 
        public int DamageBonus { get; set; } // Bonus damage from Strength or other sources

        // Constructor (optional, but good practice for initialization)
        public Character()
        {
        }

        // Common methods for all characters
        public virtual void TakeDamage(int damage)
        {
            HP -= damage;
            if (HP < 0)
            {
                HP = 0;
            }
        }

        public virtual int CalculateNaturalArmor()
        {
            // Example: Base natural armor plus a bonus from Constitution
            return NaturalArmour + Constitution / 2; // Simple example, adjust as per game rules
        }

        // Add other common methods here as needed
    }


}