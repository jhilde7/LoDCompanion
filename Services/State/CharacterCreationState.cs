using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Services.State
{
    public class CharacterCreationState
    {
        public string Name { get; set; } = string.Empty;
        public Species? SelectedSpecies { get; set; }
        public Profession? SelectedProfession { get; set; }
        public Background? SelectedBackground { get; set; }
        public int[] BaseStatRolls { get; set; } = new int[6];
        public int AddedStrength { get; set; } = 0;
        public int AddedConstitution { get; set; } = 0;
        public int AddedDexterity { get; set; } = 0;
        public int AddedWisdom { get; set; } = 0;
        public int AddedResolve { get; set; } = 0;
        public int SpecializationBonus { get; set; } = 15; // Total points to spend
        public int SpecializeAmount { get; set; } = 0; // Points allocated in current specialization turn

        public int Strength { get; set; }
        public int Constitution { get; set; }
        public int Dexterity { get; set; }
        public int Wisdom { get; set; }
        public int Resolve { get; set; }
        public int BaseHP { get; set; }
        public int DamageBonus { get; set; }
        public int NatrualArmour { get; set; }

        public int CombatSkillModifier { get; set; }
        public int RangedSkillModifier { get; set; }
        public int DodgeSkillModifier { get; set; }
        public int PickLocksSkillModifier { get; set; }
        public int BarterSkillModifier { get; set; }
        public int HealSkillModifier { get; set; }
        public int AlchemySkillModifier { get; set; }
        public int PerceptionSkillModifier { get; set; }
        public int? ArcaneArtsSkillModifier { get; set; }
        public int ForagingSkillModifier { get; set; }
        public int? BattlePrayersSkillModifier { get; set; }
        public int HpModifier { get; set; }

        public List<string> FreeSkills { get; set; } = new List<string>();
        public int CombatSkill { get; set; }
        public int RangedSkill { get; set; }
        public int Dodge { get; set; }
        public int PickLocks { get; set; }
        public int Barter { get; set; }
        public int Heal { get; set; }
        public int Alchemy { get; set; }
        public int Perception { get; set; }
        public int Foraging { get; set; }
        public int ArcaneArts { get; set; } // Specific to Wizard
        public int BattlePrayers { get; set; } // Specific to Warrior Priest

        public int MaxHP { get; set; }
        public int MaxArmour { get; set; }

        public List<Equipment>? WeaponChoices { get; set; }
        public List<Equipment>? SpecificWeaponChoices { get; set; }
        public List<Equipment>? RelicChoices { get; set; }
        public List<Potion>? PotionChoices { get;  set; }
        public List<Part>? PartChoices { get;  set; }
        public bool HasRecipe { get; set; }
        public string? SelectedWeapon { get; set; }
        public string? SelectedRelic { get; set; }

        public List<Talent>? TalentChoices { get; set; }
        public List<Talent> TalentList { get; set; } = new List<Talent>();
        public List<string>? HumanTalentCategoryList { get; set; }
        public string? HumanTalentCategorySelection { get; set; }
        public List<Perk> PerkList { get; set; } = new List<Perk>();
        public List<Equipment> StartingEquipment { get; set; } = new List<Equipment>(); // Changed from string to Equipment objects
        public List<Spell> SpellList { get; set; } = new List<Spell>();
        public List<Prayer> PrayerList { get; set; } = new List<Prayer>();

        public Hero? Hero { get; set; }

        public CharacterCreationState()
        {

        }
    }
}
