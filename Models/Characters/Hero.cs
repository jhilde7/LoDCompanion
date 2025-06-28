using LoDCompanion.Utilities;
using LoDCompanion.Services.GameData;
using System.Text.Json.Serialization;

namespace LoDCompanion.Models.Characters
{
    public class Hero : Character
    {
        // Basic Hero Information
        public string Race { get; set; }
        public string ProfessionName { get; set; }
        public int Experience { get; set; }
        public int Luck { get; set; }
        public int MaxEnergy { get; set; }
        public int CurrentEnergy { get; set; }
        public int? MaxMana { get; set; }
        public int? CurrentMana { get; set; }
        public int MaxSanity { get; set; }
        public int CurrentSanity { get; set; }
        public List<string> Status { get; set; } // e.g., "Normal", "Poisoned", "Diseased"

        // Skills (could be part of a separate Skill collection if complex, but keeping here for now)
        public int PickLocksSkill { get; set; }
        public int BarterSkill { get; set; }
        public int HealSkill { get; set; }
        public int AlchemySkill { get; set; }
        public int PerceptionSkill { get; set; }
        public int ArcaneArtsSkill { get; set; }
        public int ForagingSkill { get; set; }
        public int BattlePrayersSkill { get; set; }

        // Hero-specific States and Flags
        public int MaxArmour { get; set; }
        public bool IsThief { get; set; } // Indicates if profession is Thief
        public bool HasLantern { get; set; }
        public bool HasTorch { get; set; }
        public bool IsWeShaltNotFalter { get; set; } // Specific buff/debuff

        // Collections of Hero-specific items/abilities
        public List<Talent> Talents { get; set; } = new List<Talent>();
        public List<Perk> Perks { get; set; } = new List<Perk>();
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public List<Armour> Armours { get; set; } = new List<Armour>();
        public Shield? Shield { get; set; }
        public List<Equipment> QuickSlots { get; set; } = new List<Equipment>();
        public List<Equipment> Backpack { get; set; } = new List<Equipment>();

        public List<Spell> Spells { get; set; } = new List<Spell>();
        public List<Prayer> Prayers { get; set; } = new List<Prayer>();

        // Constructor
        public Hero()
        {

            // Set default values (can be overridden by character creation)
            Race = "Human";
            ProfessionName = "Fighter";
            Experience = 0;
            Luck = 0;
            MaxEnergy = 1;
            CurrentEnergy = MaxEnergy;
            if(MaxMana != null)
            {
                MaxMana = Wisdom;
                CurrentMana = MaxMana;
            }
            MaxSanity = 100; // Default max sanity, can be adjusted by profession/species
            CurrentSanity = MaxSanity;
            Status = new List<string>();
            MaxArmour = 0; // Will be calculated based on equipped armour

            // Default skill values (adjusted by profession/species during creation)
            PickLocksSkill = 0;
            BarterSkill = 0;
            HealSkill = 0;
            AlchemySkill = 0;
            PerceptionSkill = 0;
            ArcaneArtsSkill = 0;
            ForagingSkill = 0;
            BattlePrayersSkill = 0;
        }

        public bool ResistDisease(int? roll = null)
        {
            // This method would use a RandomHelper service or static method now
            if (roll == null)
            {
                roll = RandomHelper.RollDie("D100");
            }
            int con = Constitution;

            // Apply talent bonuses
            foreach (Talent talent in Talents)
            {
                if (talent.IsResistDisease)
                {
                    con += 10;
                }
            }

            return (roll <= con);
        }

        public bool ResistPoison(int? roll = null)
        {
            if (roll == null)
            {
                roll = RandomHelper.RollDie("D100"); 
            }
            int con = Constitution;

            foreach (var talent in Talents)
            {
                if (talent.IsResistPoison)
                {
                    con += 10;
                }
            }

            return (roll <= con);
        }

        /// <summary>
        /// Gets the current total armour class from equipped armours and shields.
        /// </summary>
        /// <returns>The total armour class.</returns>
        public int GetTotalArmourClass()
        {
            int totalAC = 0;
            foreach (var armour in Armours)
            {
                totalAC += armour.ArmourClass;
            }
            if (Shield != null)
            {
                totalAC += Shield.ArmourClass;
            }
            return totalAC;
        }

        // Method to get current weapon for combat. HeroWeapon.cs had complex logic
        // This simplified approach assumes the first weapon in the list is the "active" one
        // or a dedicated 'EquippedWeapon' property would be better
        public Weapon? GetEquippedWeapon()
        {
            if (Weapons.Count > 0)
            {
                return Weapons[0]; // Simple, assumes the primary equipped weapon is at index 0
            }
            return null; // No weapon equipped
        }
    }
}