using LoDCompanion.Models.Characters;
using LoDCompanion.Models;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.CharacterCreation
{
    public class CharacterCreationService
    {
        private readonly GameDataRegistryService _gameData;
        // Internal state of the character being built
        public string CharacterName { get; private set; } = string.Empty;
        public Species? SelectedSpecies { get; private set; } = null; // Initially null, set when species is selected
        public Profession? SelectedProfession { get; private set; } = null; // Initially null, set when profession is selected

        public int[] BaseStatRolls { get; private set; } = new int[6];
        public int RerollsRemaining { get; private set; } = 2; // Allows 2 rerolls

        public int AddedStrength { get; private set; } = 0;
        public int AddedConstitution { get; private set; } = 0;
        public int AddedDexterity { get; private set; } = 0;
        public int AddedWisdom { get; private set; } = 0;
        public int AddedResolve { get; private set; } = 0;
        public int SpecializationBonus { get; private set; } = 15; // Total points to spend
        public int SpecializeAmount { get; private set; } = 0; // Points allocated in current specialization turn

        public int Strength { get; private set; }
        public int Constitution { get; private set; }
        public int Dexterity { get; private set; }
        public int Wisdom { get; private set; }
        public int Resolve { get; private set; }
        public int BaseHP { get; private set; }

        public int CombatSkillModifier { get; private set; }
        public int RangedSkillModifier { get; private set; }
        public int DodgeSkillModifier { get; private set; }
        public int PickLocksSkillModifier { get; private set; }
        public int BarterSkillModifier { get; private set; }
        public int HealSkillModifier { get; private set; }
        public int AlchemySkillModifier { get; private set; }
        public int PerceptionSkillModifier { get; private set; }
        public int ArcaneArtsSkillModifier { get; private set; }
        public int ForagingSkillModifier { get; private set; }
        public int BattlePrayersSkillModifier { get; private set; }
        public int HpModifier { get; private set; }

        public List<string> FreeSkills { get; private set; } = new List<string>();
        public int CombatSkill { get; private set; }
        public int RangedSkill { get; private set; }
        public int Dodge { get; private set; }
        public int PickLocks { get; private set; }
        public int Barter { get; private set; }
        public int Heal { get; private set; }
        public int Alchemy { get; private set; }
        public int Perception { get; private set; }
        public int Foraging { get; private set; }
        public int ArcaneArts { get; private set; } // Specific to Wizard
        public int BattlePrayers { get; private set; } // Specific to Warrior Priest

        public int MaxHP { get; private set; }
        public int MaxArmour { get; private set; }

        public List<Talent> TalentList { get; private set; } = new List<Talent>();
        public List<Perk> PerkList { get; private set; } = new List<Perk>();
        public List<Equipment> BackpackList { get; private set; } = new List<Equipment>(); // Changed from string to Equipment objects
        public List<Spell> SpellList { get; private set; } = new List<Spell>();
        public List<Prayer> PrayerList { get; private set; } = new List<Prayer>();


        public CharacterCreationService(GameDataRegistryService gameData)
        {
            _gameData = gameData;
            // _alchemyService = alchemyService; // Initialize if passed
            InitializeCreationState();
        }

        private void InitializeCreationState()
        {
            // Reset all state variables for a new character creation session
            CharacterName = "";
            SelectedSpecies = null;
            SelectedProfession = null;
            RerollsRemaining = 2;
            ResetSpecialization();
            Strength = 0; Constitution = 0; Dexterity = 0; Wisdom = 0; Resolve = 0; BaseHP = 0;
            CombatSkillModifier = 0; RangedSkillModifier = 0; DodgeSkillModifier = 0;
            PickLocksSkillModifier = 0; BarterSkillModifier = 0; HealSkillModifier = 0;
            AlchemySkillModifier = 0; PerceptionSkillModifier = 0; ForagingSkillModifier = 0;
            ArcaneArtsSkillModifier = 0; BattlePrayersSkillModifier = 0; HpModifier = 0;
            FreeSkills.Clear();
            CombatSkill = 0; RangedSkill = 0; Dodge = 0; PickLocks = 0; Barter = 0;
            Heal = 0; Alchemy = 0; Perception = 0; Foraging = 0;
            MaxHP = 0; MaxArmour = 0;
            TalentList.Clear();
            PerkList.Clear();
            BackpackList.Clear();
            SpellList.Clear();
            PrayerList.Clear();
        }

        public void SetCharacterName(string name)
        {
            CharacterName = name;
        }

        public void SetSpecies(Species species, int[]? statRolls = null, string? talentCategory = null)
        {
            TalentLookupService talent = new TalentLookupService(_gameData);
            SelectedSpecies = species;
            RerollsRemaining = 2; // Reset rerolls when new species selected
            ResetSpecialization();
            RollBaseStats(statRolls); // Roll stats immediately after species selection
            TalentList = species.GetTraits(); // Create a new list to avoid modifying original
            if (species.Name == "Human")
            {
                TalentList.Add(talent.GetTalentForHumanByCategory(talentCategory));
            }
        }

        private string RollRandomTalent(string? talentCatergory)
        {
            throw new NotImplementedException();
        }

        public void SetProfession(Profession profession)
        {
            SelectedProfession = profession;
            GetSkillModifiers();
            GetSkillStats(); // Update skills based on new profession

            // Add starting talents from profession to current talent list
            foreach (string talent in profession.StartingTalentList)
            {
                if (_gameData.GetTalentByName(talent) != null)
                { 
                    if (!TalentList.Contains(_gameData.GetTalentByName(talent) ?? new Talent())) // Avoid duplicates
                    {
                        TalentList.Add(_gameData.GetTalentByName(talent) ?? new Talent());
                    } 
                }
            }
            PerkList = profession.StartingPerkList;
            FreeSkills = profession.FreeSkills; 

            SpellList = new List<Spell>();
            PrayerList = new List<Prayer>();
        }

        public void RollBaseStats(int[]? _baseStatRoll = null)
        {
            if (SelectedSpecies == null)
            {
                throw new InvalidOperationException("Species must be selected before rolling base stats.");
            }

            int[] baseStatRolls = new int[6]; // Strength, Constitution, Dexterity, Wisdom, Resolve, BaseHP

            if (_baseStatRoll == null)
            {
                for (int i = 0; i < 5; i++) // Strength, Constitution, Dexterity, Wisdom, Resolve
                {
                    baseStatRolls[i] = GenerateStatRoll("D10");
                }
                baseStatRolls[5] = GenerateStatRoll("D6"); // BaseHP roll
            }
            else
            {
                baseStatRolls = _baseStatRoll; // Use provided rolls if available
            }

            SetCalculatedStats(baseStatRolls); // Apply the rolls to actual stats
        }

        private int GenerateStatRoll(string die)
        {
            // Assuming RollTwiceAndChooseHighest is a method that rolls two dice and returns the highest result
            int highestRoll = 0;
            for (int i = 0; i < 2; i++)
            {
                int roll = RandomHelper.RollDie(die); // Simulating a die roll
                if (roll > highestRoll)
                {
                    highestRoll = roll;
                }
            }
            return highestRoll;
        }

        private void SetCalculatedStats(int[] baseStatRolls)
        {
            if (SelectedSpecies == null) return; // Should not happen if RollBaseStats is called first

            Strength = SelectedSpecies.BaseStrength + baseStatRolls[0];
            Constitution = SelectedSpecies.BaseConstitution + baseStatRolls[1];
            Dexterity = SelectedSpecies.BaseDexterity + baseStatRolls[2];
            Wisdom = SelectedSpecies.BaseWisdom + baseStatRolls[3];
            Resolve = SelectedSpecies.BaseResolve + baseStatRolls[4];
            BaseHP = SelectedSpecies.BaseHP + baseStatRolls[5];

            // Re-calculate skills and MaxHP after stat changes
            GetSkillStats();
        }

        private void ResetSpecialization()
        {
            AddedStrength = 0;
            AddedConstitution = 0;
            AddedDexterity = 0;
            AddedWisdom = 0;
            AddedResolve = 0;
            SpecializationBonus = 15;
            SpecializeAmount = 0;
        }

        public bool ApplySpecialization(string statType, int amount)
        {
            if (amount <= 0 || SpecializationBonus < amount) return false;

            int actualAmount = Math.Min(amount, 10); // Cap per stat at 10, as per original logic

            switch (statType.ToUpperInvariant())
            {
                case "STR":
                    if (AddedStrength + actualAmount > 10) actualAmount = 10 - AddedStrength;
                    Strength += actualAmount;
                    AddedStrength += actualAmount;
                    break;
                case "CON":
                    if (AddedConstitution + actualAmount > 10) actualAmount = 10 - AddedConstitution;
                    Constitution += actualAmount;
                    AddedConstitution += actualAmount;
                    break;
                case "DEX":
                    if (AddedDexterity + actualAmount > 10) actualAmount = 10 - AddedDexterity;
                    Dexterity += actualAmount;
                    AddedDexterity += actualAmount;
                    break;
                case "WIS":
                    if (AddedWisdom + actualAmount > 10) actualAmount = 10 - AddedWisdom;
                    Wisdom += actualAmount;
                    AddedWisdom += actualAmount;
                    break;
                case "RES":
                    if (AddedResolve + actualAmount > 10) actualAmount = 10 - AddedResolve;
                    Resolve += actualAmount;
                    AddedResolve += actualAmount;
                    break;
                default:
                    return false; // Invalid stat type
            }

            SpecializationBonus -= actualAmount;
            GetSkillStats(); // Recalculate skills after stat specialization
            return true;
        }

        private void GetSkillModifiers()
        {
            if (SelectedProfession == null) return;

            CombatSkillModifier = SelectedProfession.CombatSkillModifier;
            RangedSkillModifier = SelectedProfession.RangedSkillModifier;
            DodgeSkillModifier = SelectedProfession.DodgeSkillModifier;
            PickLocksSkillModifier = SelectedProfession.PickLocksSkillModifier;
            BarterSkillModifier = SelectedProfession.BarterSkillModifier;
            HealSkillModifier = SelectedProfession.HealSkillModifier;
            AlchemySkillModifier = SelectedProfession.AlchemySkillModifier;
            PerceptionSkillModifier = SelectedProfession.PerceptionSkillModifier;
            ForagingSkillModifier = SelectedProfession.ForagingSkillModifier;
            HpModifier = SelectedProfession.HPModifier;
            ArcaneArtsSkillModifier = SelectedProfession.ArcaneArtsSkillModifier ?? 0;
            BattlePrayersSkillModifier = SelectedProfession.BattlePrayersSkillModifier ?? 0;
        }

        private void GetSkillStats()
        {
            if (SelectedSpecies == null || SelectedProfession == null) return; // Ensure both are selected

            CombatSkill = Dexterity + CombatSkillModifier;
            RangedSkill = Dexterity + RangedSkillModifier;
            Dodge = Dexterity + DodgeSkillModifier;
            PickLocks = Dexterity + PickLocksSkillModifier;
            Barter = Wisdom + BarterSkillModifier;
            Heal = Wisdom + HealSkillModifier;
            Alchemy = Wisdom + AlchemySkillModifier;
            Perception = Wisdom + PerceptionSkillModifier;
            Foraging = Constitution + ForagingSkillModifier;

            MaxHP = BaseHP + HpModifier;
            MaxArmour = SelectedProfession.MaxArmourType;

            if (SelectedProfession.Name == "Wizard")
            {
                ArcaneArts = Wisdom + ArcaneArtsSkillModifier;
            }
            else if (SelectedProfession.Name == "Warrior Priest")
            {
                BattlePrayers = Wisdom + BattlePrayersSkillModifier;
            }
        }

        public bool AddFreeSkill(string skillName)
        {
            if (string.IsNullOrEmpty(skillName) || !FreeSkills.Contains(skillName)) return false;

            // Remove skill from available free skills once added
            FreeSkills.Remove(skillName);

            switch (skillName)
            {
                case "CS":
                    CombatSkillModifier += 10;
                    break;
                case "RS":
                    RangedSkillModifier += 10;
                    break;
                case "Dodge":
                    DodgeSkillModifier += 10;
                    break;
                case "PL":
                    PickLocksSkillModifier += 10;
                    break;
                case "Barter":
                    BarterSkillModifier += 10;
                    break;
                case "Heal":
                    HealSkillModifier += 10;
                    break;
                case "Alchemy":
                    AlchemySkillModifier += 10;
                    break;
                case "Perception":
                    PerceptionSkillModifier += 10;
                    break;
                case "AA":
                    ArcaneArtsSkillModifier += 10;
                    break;
                case "Foraging":
                    ForagingSkillModifier += 10;
                    break;
                case "BP":
                    BattlePrayersSkillModifier += 10;
                    break;
                default:
                    FreeSkills.Add(skillName); // Add back if not a recognized skill
                    return false;
            }

            GetSkillStats(); // Recalculate skills after adding free skill
            return true;
        }

        public Hero FinalizeCharacter()
        {
            if (string.IsNullOrEmpty(CharacterName) || SelectedSpecies == null || SelectedProfession == null)
            {
                throw new InvalidOperationException("Character name, species, and profession must be selected before finalizing character.");
            }

            // Create the new Hero instance
            Hero newHero = new Hero
            {
                Name = CharacterName,
                Race = SelectedSpecies.Name,
                ProfessionName = SelectedProfession.Name,
                Strength = Strength,
                Constitution = Constitution,
                Dexterity = Dexterity,
                Wisdom = Wisdom,
                Resolve = Resolve,
                MaxHP = MaxHP,
                HP = MaxHP,
                CombatSkill = CombatSkill,
                RangedSkill = RangedSkill,
                Dodge = Dodge,
                PickLocksSkill = PickLocks,
                BarterSkill = Barter,
                HealSkill = Heal,
                AlchemySkill = Alchemy,
                PerceptionSkill = Perception,
                ForagingSkill = Foraging,
                MaxArmour = MaxArmour,
                Talents = TalentList,
                Perks = PerkList, 
                Level = 1,
                Experience = 0,
                MaxMana = SelectedProfession.Name == "Wizard" ? Wisdom : null,
                CurrentMana = SelectedProfession.Name == "Wizard" ? Wisdom : null,
                CurrentEnergy = 1, 
                MaxEnergy = 1, 
                CurrentSanity = 10, 
                MaxSanity = 10,
            };

            // Populate Hero's Talents and Perks lists with actual Talent/Perk objects if they exist
            // For now, these are just string names. If Talent/Perk are rich objects, you'd load them here.
            //newHero.Talents = TalentList.Select(t => new Talent { TalentName = t }).ToList(); // Assuming Talent class has a TalentName property
            //newHero.Perks = PerkList.Select(p => new Perk { PerkName = p }).ToList(); // Assuming Perk class has a PerkName property
            SpellLookupService spell = new SpellLookupService(_gameData);
            PrayerLookupService prayer = new PrayerLookupService(_gameData);
            // Add starting spells and prayers (assuming these are HeroSpell/Prayer objects)
            if(newHero.ProfessionName == "Wizard")
            { 
                newHero.Spells = spell.GetStartingSpells(); 
            }
            else if ( newHero.ProfessionName == "Warrior Priest")
            { 
                newHero.Prayers = prayer.GetStartingPrayers();
            }

            // Add starting backpack items using TreasureService
            // The original `backpackList` was List<string>, so we fetch items by name.
            foreach (string itemName in SelectedProfession.StartingBackpackList)
            {
                int durability = 6 - RandomHelper.GetRandomNumber(1, 4); // Original logic
                Equipment newItem = new TreasureService(_gameData).CreateItem(itemName, durability);
                if (newItem != null)
                {
                    newHero.Backpack.Add(newItem);
                }
            }

            // Re-initialize service state for next creation process
            InitializeCreationState();

            return newHero;
        }

        // Expose current state for UI binding (read-only properties)
        public bool IsSpeciesPicked => SelectedSpecies != null;
        public bool IsProfessionPicked => SelectedProfession != null;
        public bool IsHuman => SelectedSpecies?.Name == "Human";

        // You might add methods to get the current skill values to display in UI
    }
}