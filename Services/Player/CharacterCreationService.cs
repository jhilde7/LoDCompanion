using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Services.GameData;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.CharacterCreation
{
    public class CharacterCreationService
    {
        private readonly GameDataService _gameData = new GameDataService();
        // Internal state of the character being built
        public CharacterCreationState State { get; private set; } = new CharacterCreationState();



        public CharacterCreationService(GameDataService gameData)
        {
            _gameData = gameData;
            // _alchemyService = alchemyService; // Initialize if passed
            InitializeCreationState();
        }

        public void InitializeCreationState()
        {
            State = new CharacterCreationState();
        }

        public void SetCharacterName(string name)
        {
            State.Name = name;
        }

        public void SetSpecies(Species species, int[]? statRolls = null)
        {
            State.SelectedSpecies = species;
            ResetSpecialization();
            RollBaseStats(statRolls);
            State.TalentList = GetTraits(species.Name);
        }

        public List<Talent> GetTraits(string name)
        {
            List<Talent> traits = new List<Talent>();
            if (name != null)
            {                          
                switch (name)
                {
                    case "Dwarf":
                        traits.Add(new Talent()
                        {
                            Category = TalentCategory.Physical,
                            Name = TalentName.NightVision,
                            Description = "Your hero's species has the natural ability to see in the dark and is not affected by darkness. A hero with Night Vision gets +10 on Perception. This talent can only be given to a newly-created character that has this talent listed in the Species Description.",
                        });
                        traits.Add(_gameData.GetHateTalentByCategory(GameDataService.HateCategory.Goblins));
                        return traits;
                    case "Elf":
                        traits.Add(new Talent()
                        {
                            Category = TalentCategory.Physical,
                            Name = TalentName.NightVision,
                            Description = "Your hero's species has the natural ability to see in the dark and is not affected by darkness. A hero with Night Vision gets +10 on Perception. This talent can only be given to a newly-created character that has this talent listed in the Species Description.",
                        });
                        traits.Add(new Talent()
                        {
                            Category = TalentCategory.Physical,
                            Name = TalentName.PerfectHearing,
                            Description = "Your hero's hearing is exceptionally good, and you gain a +15 bonus when rolling for initiative after opening a door. This cannot be used if the door was broken down. This talent can only be given to a newly created character that has this Talent listed in the Species Description. Alternative activation: Add one extra hero chit to the bag during the first turn. Regardless of activation mechanics, this Talent only works if the hero is on the same tile as the door being opened.",
                        });
                        return traits;
                    case "Halfling":
                        traits.Add(_gameData.GetTalentByName(TalentName.Lucky) ?? new Talent());
                        return traits;
                    case "Human":
                        State.HumanTalentCategoryList = new() { "Physical", "Combat", "Faith", "Alchemist", "Common", "Magic", "Sneaky", "Mental" };
                        return traits;
                    default: return traits;
                }
            }
            else
            {
                return traits;
            }
        }

        public void SetHumanRandomTalent(string selection)
        {
            if (State.SelectedSpecies?.Name != "Human")
            {
                return;
            }
            State.HumanTalentCategorySelection = selection;

            State.TalentList.Clear();
            List<Talent> list = new List<Talent>();
            switch (selection)
            {
                case "Physical": list = _gameData.PhysicalTalents; break;
                case "Combat": list = _gameData.CombatTalents; break;
                case "Faith": list = _gameData.FaithTalents; break;
                case "Alchemist": list = _gameData.AlchemistTalents; break;
                case "Common": list = _gameData.CommonTalents; break;
                case "Magic": list = _gameData.MagicTalents; break;
                case "Sneaky": list = _gameData.SneakyTalents; break;
                case "Mental": list = _gameData.MentalTalents; break;
            }

            if (State.HumanTalentCategoryList != null && State.HumanTalentCategoryList.Any())
            {
                Talent randomTalent = list[RandomHelper.GetRandomNumber(1, list.Count() - 1)];
                State.TalentList.Add(randomTalent);
            }
        }

        public void SetProfession(Profession profession)
        {
            State.SelectedProfession = profession;
            GetSkillModifiers();
            GetSkillStats(); // Update skills based on new profession

            GetTalentChoices();
            if(profession.StartingTalentList != null)
            {
                State.TalentList.AddRange(profession.StartingTalentList);
            }

            State.PerkList = SetStartingPerks(profession.Name);
            State.FreeSkills = profession.FreeSkills;
            GetEquipmentChoices();

            State.SpellList = new List<Spell>();
            State.PrayerList = new List<Prayer>();
        }

        private List<Perk> SetStartingPerks(string name)
        {
            List<Perk> startingPerkList = new List<Perk> { };
            if (name == "Barbarian")
            {
                startingPerkList.Add(_gameData.GetPerkByName("Frenzy") ?? new Perk());
            }
            return startingPerkList;
        }

        public void RollBaseStats(int[]? _baseStatRoll = null)
        {
            if (State.SelectedSpecies == null)
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
            if (State.SelectedSpecies == null) return; // Should not happen if RollBaseStats is called first

            State.Strength = State.SelectedSpecies.BaseStrength + baseStatRolls[0];
            State.Constitution = State.SelectedSpecies.BaseConstitution + baseStatRolls[1];
            State.Dexterity = State.SelectedSpecies.BaseDexterity + baseStatRolls[2];
            State.Wisdom = State.SelectedSpecies.BaseWisdom + baseStatRolls[3];
            State.Resolve = State.SelectedSpecies.BaseResolve + baseStatRolls[4];
            State.BaseHP = State.SelectedSpecies.BaseHitPoints + baseStatRolls[5];
        }

        public int ResetSpecialization()
        {
            State.AddedStrength = 0;
            State.AddedConstitution = 0;
            State.AddedDexterity = 0;
            State.AddedWisdom = 0;
            State.AddedResolve = 0;
            State.SpecializationBonus = 15;
            State.SpecializeAmount = 0;

            return State.SpecializationBonus;
        }

        public bool ApplySpecialization(string statType, int amount)
        {
            if (amount <= 0 || State.SpecializationBonus <= 0) return false;

            int actualAmount = Math.Min(amount, 10); // Cap per stat at 10, as per original logic

            switch (statType)
            {
                case "STR":
                    State.AddedStrength += actualAmount;
                    break;
                case "CON":
                    State.AddedConstitution += actualAmount;
                    break;
                case "DEX":
                    State.AddedDexterity += actualAmount;
                    break;
                case "WIS":
                    State.AddedWisdom += actualAmount;
                    break;
                case "RES":
                    State.AddedResolve += actualAmount;
                    break;
                default:
                    return false; // Invalid stat type
            }

            State.SpecializationBonus -= actualAmount;
            return true;
        }

        private void GetSkillModifiers()
        {
            if (State.SelectedProfession == null) return;

            State.CombatSkillModifier = State.SelectedProfession.CombatSkillModifier;
            State.RangedSkillModifier = State.SelectedProfession.RangedSkillModifier;
            State.DodgeSkillModifier = State.SelectedProfession.DodgeSkillModifier;
            State.PickLocksSkillModifier = State.SelectedProfession.PickLocksSkillModifier;
            State.BarterSkillModifier = State.SelectedProfession.BarterSkillModifier;
            State.HealSkillModifier = State.SelectedProfession.HealSkillModifier;
            State.AlchemySkillModifier = State.SelectedProfession.AlchemySkillModifier;
            State.PerceptionSkillModifier = State.SelectedProfession.PerceptionSkillModifier;
            State.ForagingSkillModifier = State.SelectedProfession.ForagingSkillModifier;
            State.HpModifier = State.SelectedProfession.HPModifier;
            State.ArcaneArtsSkillModifier = State.SelectedProfession.ArcaneArtsSkillModifier ?? 0;
            State.BattlePrayersSkillModifier = State.SelectedProfession.BattlePrayersSkillModifier ?? 0;
        }

        private void GetSkillStats()
        {
            if (State.SelectedSpecies == null || State.SelectedProfession == null) return; // Ensure both are selected

            State.Strength += State.AddedStrength;
            State.Dexterity += State.AddedDexterity;
            State.Constitution += State.AddedConstitution;
            State.Wisdom += State.AddedWisdom;
            State.Resolve += State.AddedResolve;

            State.AddedResolve = 0;
            State.AddedConstitution = 0;
            State.AddedDexterity = 0;
            State.AddedStrength = 0;
            State.AddedWisdom = 0;

            State.CombatSkill = State.Dexterity + State.CombatSkillModifier;
            State.RangedSkill = State.Dexterity + State.RangedSkillModifier;
            State.Dodge = State.Dexterity + State.DodgeSkillModifier;
            State.PickLocks = State.Dexterity + State.PickLocksSkillModifier;
            State.Barter = State.Wisdom + State.BarterSkillModifier;
            State.Heal = State.Wisdom + State.HealSkillModifier;
            State.Alchemy = State.Wisdom + State.AlchemySkillModifier;
            State.Perception = State.Wisdom + State.PerceptionSkillModifier;
            State.Foraging = State.Constitution + State.ForagingSkillModifier;

            State.MaxHP = State.BaseHP + State.HpModifier;
            State.MaxArmour = State.SelectedProfession.MaxArmourType;

            if (State.SelectedProfession.Name == "Wizard" && State.ArcaneArtsSkillModifier.HasValue)
            {
                State.ArcaneArts = State.Wisdom + (int)State.ArcaneArtsSkillModifier;
            }
            else if (State.SelectedProfession.Name == "Warrior Priest" && State.BattlePrayersSkillModifier.HasValue)
            {
                State.BattlePrayers = State.Wisdom + (int)State.BattlePrayersSkillModifier;
            }
        }

        public void GetTalentChoices()
        {            
            if (State.SelectedProfession != null && State.SelectedProfession.TalentChoices != null)
            {
                State.TalentChoices = new();
                State.TalentChoices.AddRange(State.SelectedProfession.TalentChoices);
            }
        }

        public void GetEquipmentChoices()
        {
            // Clear any previous choices
            State.SpecificWeaponChoices = null;
            State.WeaponChoices = null;
            State.RelicChoices = null;
            State.PotionChoices = null;
            State.PartChoices = null;
            State.SelectedWeapon = null;
            State.SelectedRelic = null;
            State.HasRecipe = false;

            if (State.SelectedProfession == null) return;

            if (State.SelectedProfession.EquipmentChoices != null)
            {
                foreach (var item in State.SelectedProfession.EquipmentChoices)
                {
                    if (item.Contains("/", StringComparison.OrdinalIgnoreCase))
                    {
                        State.SpecificWeaponChoices = new List<Weapon>();
                        List<string> list = item.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (var item2 in list)
                        {
                            State.SpecificWeaponChoices.Add(EquipmentService.GetMeleeWeaponByName(item2) ?? new MeleeWeapon());
                        }
                    }
                    else if (item.Contains("Weapon"))
                    {
                        State.WeaponChoices = EquipmentService.GetStartingWeapons();
                    }
                    else if (item.Contains("Relic"))
                    {
                        State.RelicChoices = EquipmentService.Relics;
                    }
                    else if(item.Contains("Potions"))
                    {
                        State.PotionChoices = AlchemyService.StandardPotions;
                    }
                    else if(item.Contains("Parts"))
                    {
                        State.PartChoices = AlchemyService.Parts;
                    }
                    else if (item.Contains("Recipe"))
                    {
                        State.HasRecipe = true;
                    }
                } 
            }
        }

        public void UpdateTalentsWithSelection(TalentName selection)
        {
            State.TalentList.Add(_gameData.GetTalentByName(selection));
        }


        public bool AddFreeSkill(string? skillName)
        {
            if (string.IsNullOrEmpty(skillName) || !State.FreeSkills.Contains(skillName)) return false;

            // Remove skill from available free skills once added
            State.FreeSkills.Remove(skillName);

            switch (skillName)
            {
                case "CombatSkill": State.CombatSkillModifier += 10; break;
                case "RangedSkill": State.RangedSkillModifier += 10; break;
                case "DodgeSkill": State.DodgeSkillModifier += 10; break;
                case "PickLocksSkill": State.PickLocksSkillModifier += 10; break;
                case "BarterSkill": State.BarterSkillModifier += 10; break;
                case "HealSkill": State.HealSkillModifier += 10; break;
                case "AlchemySkill": State.AlchemySkillModifier += 10; break;
                case "PerceptionSkill": State.PerceptionSkillModifier += 10; break;
                default:
                    State.FreeSkills.Add(skillName); // Add back if not a recognized skill
                    return false;
            }

            GetSkillStats(); // Recalculate skills after adding free skill
            return true;
        }

        public void RollBackground()
        {
            // Assuming GameDataRegistryService can provide a list of all possible backgrounds
            State.SelectedBackground = new Background(_gameData).GetRandomBackground();
            // You might also want to automatically apply any trait from the background here
            if (State.SelectedBackground.Trait != null)
            {
                State.TalentList.Add(State.SelectedBackground.Trait);
            }
        }

        public void AddStartingRecipe(AlchemicalRecipe recipe)
        {
            State.StartingEquipment.Add(recipe);
        }

        public Hero BuildPreviewHero()
        {
            if (State.SelectedWeapon != null)
            {
                Weapon? weapon = EquipmentService.GetWeaponByName(State.SelectedWeapon);
                if (weapon != null)
                {
                    State.StartingEquipment.Add(weapon); 
                }
            }

            if (State.SelectedRelic != null)
            {
                var relicChoice = EquipmentService.GetRelicByName(State.SelectedRelic);
                if (State.SelectedRelic != null && relicChoice != null)
                {
                    State.StartingEquipment.Add(relicChoice);
                } 
            }

            if (State.SelectedSpecies == null)
            {
                throw new InvalidOperationException("SelectedSpecies must be set before building the preview hero.");
            }
            if (State.SelectedProfession == null)
            {
                throw new InvalidOperationException("SelectedProfession must be set before building the preview hero.");
            }

            State.StartingEquipment.RemoveAll(item => item.Name == "");

            State.Hero = new Hero
            {
                Name = State.Name,
                SpeciesName = State.SelectedSpecies.Name,
                ProfessionName = State.SelectedProfession.Name,
                Talents = State.TalentList,
                Perks = State.PerkList,
            };
            State.Hero.SetStat(BasicStat.Strength, State.Strength);
            State.Hero.SetStat(BasicStat.Constitution, State.Constitution);
            State.Hero.SetStat(BasicStat.Dexterity, State.Dexterity);
            State.Hero.SetStat(BasicStat.Wisdom, State.Wisdom);
            State.Hero.SetStat(BasicStat.Resolve, State.Resolve);
            State.Hero.SetStat(BasicStat.HitPoints, State.MaxHP);
            State.Hero.SetStat(BasicStat.Level, 1);
            State.Hero.SetStat(BasicStat.Experience, 0);
            State.Hero.SetStat(BasicStat.Energy, 1);
            State.Hero.SetStat(BasicStat.Sanity, 10);
            State.Hero.SetStat(BasicStat.Move, 4);
            State.Hero.SetSkill(Skill.CombatSkill, State.CombatSkill);
            State.Hero.SetSkill(Skill.RangedSkill, State.RangedSkill);
            State.Hero.SetSkill(Skill.Dodge, State.Dodge);
            State.Hero.SetSkill(Skill.PickLocks, State.PickLocks);
            State.Hero.SetSkill(Skill.Barter, State.Barter);
            State.Hero.SetSkill(Skill.Heal, State.Heal);
            State.Hero.SetSkill(Skill.Alchemy, State.Alchemy);
            State.Hero.SetSkill(Skill.Perception, State.Perception);
            State.Hero.SetSkill(Skill.Foraging, State.Foraging);
            State.Hero.SetSkill(Skill.ArcaneArts, State.ArcaneArts);
            State.Hero.SetSkill(Skill.BattlePrayers, State.BattlePrayers);

            // Populate Hero's Talents and Perks lists with actual Talent/Perk objects if they exist
            State.Hero.Talents = State.TalentList;
            State.Hero.Perks = State.PerkList;

            // Add starting spells and prayers
            if (State.Hero.ProfessionName == "Wizard")
            {
                State.Hero.Spells = GetStartingSpells();
            }
            else if (State.Hero.ProfessionName == "Warrior Priest")
            {
                State.Hero.Prayers = GetStartingPrayers();
            }

            // Add starting backpack items
            foreach (Equipment item in State.StartingEquipment)
            {
                BackpackHelper.AddItem(State.Hero.Inventory.Backpack, item);
            }

            if (State.SelectedProfession.StartingBackpackList != null)
            {
                foreach (Equipment item in State.SelectedProfession.StartingBackpackList)
                {
                    BackpackHelper.AddItem(State.Hero.Inventory.Backpack, item);
                } 
            }

            foreach(Equipment equipment in State.Hero.Inventory.Backpack)
            {
                if (equipment is MeleeWeapon || equipment is RangedWeapon || equipment is Armour)
                {
                    equipment.Durability = RandomHelper.RollDie("D4"); 
                }
                else
                {
                    equipment.Durability = 1;
                }
            }

            return State.Hero;
        }

        public void FinalizeCharacter()
        {           
            // Re-initialize service state for next creation process
            InitializeCreationState();
        }

        private List<Spell> GetStartingSpells()
        {
            var spells = new List<Spell>();
            var possibleSpells = SpellService.GetSpellsByLevel(1);
            if (possibleSpells == null)
            {
                throw new ArgumentException("No spells found for level 1.");
            }

            for (int i = 0; i < 3; i++)
            {
                Spell spell;
                do
                {
                    spell = possibleSpells[RandomHelper.GetRandomNumber(0, possibleSpells.Count - 1)];
                } while (spells.Contains(spell));
                spells.Add(spell);
            }
            return spells;
        }

        private List<Prayer> GetStartingPrayers()
        {
            var prayers = new List<Prayer>();
            var possiblePrayers = _gameData.GetPrayersByLevel(1);
            if (possiblePrayers == null)
            {
                throw new ArgumentException("No spells found for level 1.");
            }

            for (int i = 0; i < 2; i++)
            {
                Prayer prayer;
                do
                {
                    prayer = possiblePrayers[RandomHelper.GetRandomNumber(0, possiblePrayers.Count - 1)];
                } while (prayers.Contains(prayer));
                prayers.Add(prayer);
            }
            return prayers;
        }

        // Expose current state for UI binding (read-only properties)
        public bool IsSpeciesPicked => State.SelectedSpecies != null;
        public bool IsProfessionPicked => State.SelectedProfession != null;
        public bool IsHuman => State.SelectedSpecies?.Name == "Human";
    }

    public class Species
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BaseStrength { get; set; }
        public int BaseConstitution { get; set; }
        public int BaseDexterity { get; set; }
        public int BaseWisdom { get; set; }
        public int BaseResolve { get; set; }
        public int BaseHitPoints { get; set; }
        public int MaxSTR { get; set; }
        public int MaxDEX { get; set; }
        public int MaxWIS { get; set; }
        public int MaxRES { get; set; }
        public int MaxCON { get; set; }

        public Species() { }
        
    }

    public class Profession
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CombatSkillModifier { get; set; }
        public int RangedSkillModifier { get; set; }
        public int DodgeSkillModifier { get; set; }
        public int PickLocksSkillModifier { get; set; }
        public int BarterSkillModifier { get; set; }
        public int HealSkillModifier { get; set; }
        public int AlchemySkillModifier { get; set; }
        public int PerceptionSkillModifier { get; set; }
        public int? ArcaneArtsSkillModifier { get; set; } = null;
        public int ForagingSkillModifier { get; set; }
        public int? BattlePrayersSkillModifier { get; set; } = null;
        public int HPModifier { get; set; }
        public int MaxArmourType { get; set; }
        public int MaxMeleeWeaponType { get; set; }
        public List<string>? EquipmentChoices { get; set; }
        public List<Talent>? TalentChoices { get; set; }
        public List<Equipment>? StartingBackpackList { get; set; }
        public List<Talent>? StartingTalentList { get; set; }
        public List<Perk>? StartingPerkList { get; set; }
        public Dictionary<string, int> LevelUpCost { get; set; } = new();
        public List<string> FreeSkills { get; set; } = new();

        public Profession() { }

    }

    public class Background
    {
        private readonly GameDataService _gameData;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PersonalQuest { get; set; }
        public Talent? Trait { get; set; }

        public Background(GameDataService gameData) 
        { 
            _gameData = gameData;
        }

        public Background GetRandomBackground()
        {
            List<Background> backgrounds = GetBackgrounds();
            return backgrounds[RandomHelper.GetRandomNumber(0, backgrounds.Count - 1)];           
        }

        private List<Background> GetBackgrounds() 
        {
           return new List<Background>()
                {
                    new Background(_gameData)
                    {
                        Id = 1,
                        Name = "Wanderlust",
                        Description = "For as long as you can remember, you have felt the urge to see what lies beyond the horizon. Born in a small village far to the north, you helped out on the family farm just like your siblings. As the eldest child, you were expected to take over after your father, just as he had done when your grandfather was too old to work in the fields. The older you got, the stronger the wanderlust grew, until you finally realised that your current world felt too small, and that staying within the confines of the small farm was simply not an option. Afraid to face your father, you sneak out in the middle of the night, bringing only the bare necessities as the family resources are limited. Now, several years later you have managed to make a life for yourself beyond that horizon and you have seen most of the kingdom. The wanderlust is still strong though, and you are determined to visit all of the settlements within the kingdom.",
                        PersonalQuest = "Visit all settlements on the map (A total of 10). Once done, you gain 1500 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 2,
                        Name = "The Well",
                        Description = "As with so many others in the kingdom, you were born in a small village with farmers for parents. During the harvest, all able-bodied people were needed in the fields. When you were young, you had to stay behind with the other children who were not yet strong enough to work, and those who were too old for hard labour. Wanting to help, you decided that you could at least fetch water from the town well to make things easier for your mother on her return. Once at the well, you start mimicking the moves you have seen your mother do countless times. Turning the crank to hoist the bucket is heavy work, but after several minutes the bucket is finally visible. As you lean out to reach the bucket, you suddenly lose grip on the crank and slip, falling into the well. Luckily, the water breaks your fall, and the level is not too high for you to keep your head above water. Once the initial panic settles, you realise that you are stuck deep underground in this cold, dark place. It took several hours before you were finally located and saved, but even now the memory of the cold and darkness won't leave you alone.",
                        Trait = new Talent() {Name = TalentName.Claustrophobia, Description = "The hero is troubled be tight spaces and becomes less effective. All skills and stats are at a -10 in corridors.This is not curable at the asylum. Instead, you must face your fears." },
                        PersonalQuest = "Once you have fought and survived 5 battles in a corridor, your condition is finally cured. Such is the effect of beating this trauma that you actually turn it into a strength. You gain the Tunnel Fighter Talent."
                    },
                    new Background(_gameData)
                    {
                        Id = 3,
                        Name = "Fables",
                        Description = "Your uncle had a knack for telling stories, and they were always stories of brave heroes defying the undead creatures of the Ancient Lands. He told his stories with such skill and rich detail that you almost started to believe that you had seen these ancient tombs with your own eyes. Even now as an adult, you still have a clear vision of the tombs. You have sworn to see them with your own eyes to see if your uncle's fables were true.",
                        PersonalQuest = "Visit 3 Quest Sites in the Ancient Lands. Once you leave the third site, you gain 1500 \u03a7\u03a1."
                    },
                    new Background(_gameData)
                    {
                        Id = 4,
                        Name = "The Heirloom",
                        Description = "Your great grandmother's sister was not like most other girls in her village. At the age of 17, she left the village in search of adventure, and returned not long after, having cleared her first dungeons. She made several such trips and after one of them, she returned with a wonderful longsword. Perfectly balanced and adorned with jewels, this sword was clearly a unique find. Sadly, she would never return from her next quest. Since then, you have sworn to find and retrieve that sword.",
                        PersonalQuest = "Find your Great Aunt's sword. At the start of each quest, roll 1d10. On a roll of 1, the dungeon you are heading to is actually the one that holds the sword. Place a secondary Quest Card in the first half of the Exploration Card pile. The room after the secondary Quest Card will contain two enemies. The enemy with the highest XP will carry the sword (although not use it in battle). The fate of your ancestor will never be known, but at least you will now have a chance to get that sword back. The weapon is a silver shortsword that does +1 DMG and has +2 Durability. You may not sell it."
                    },
                    new Background(_gameData)
                    {
                        Id = 5,
                        Name = "Arachnophobia",
                        Description = "Many people feel uneasy in the presence of spiders. This was the case with you as well as you were growing up, but it has now become something far worse. One late afternoon as you were out picking berries in the forest surrounding your village, you sprained your ankle. Barely able to walk, you fashioned a staff to support your weight using your small knife. You began your trek home, but walking was slow, and the sun began to set. Before long, the forest became steadily darker, until you could barely see your hand in front of you. At that moment you suddenly spotted the gleaming eyes of animals in front of you. You blinked several times to get better focus, as the eyes seemed to be clustered together in a pack. Just as you are wondering what animals would stay so close together, a chill ran down your spine as you counted to eight, and realised that those eyes belonged to one single creature. Then, with a low hissing sound, a spider the size of a wolf slowly advanced towards you. Over what seemed like an eternity you fought off the vile creature using your little knife and the wooden staff. In the end though, the outcome was inevitable. You were trapped in strings of thick cobweb as the spider finally sank its fangs into your flesh. As the venom entered your veins, you slowly lost consciousness. The next thing you remembered was being back in your bed, feverish but alive. Apparently, a local Ranger had spotted you and managed to kill the spider just as you passed out. She brought you back home, and although there were no antidotes available, they managed to nurse you back to health. You made a full physical recovery from the ordeal, but the trauma remains to this day.",
                        Trait = new Talent() {Name = TalentName.Arachnophobia, Description = "The hero finds all kinds of spiders terrifying. Treat all encounters with spiders as Terror. This is not curable at the asylum. Instead, you must face your fears." },
                        PersonalQuest = "Once you have fought and survived 3 battles with spiders, your condition is finally cured. Such is the effect of beating this trauma that you actually turn it into a strength. You gain +10 CS whenever trying to hit a spider."
                    },
                    new Background(_gameData)
                    {
                        Id = 6,
                        Name = "The Lost Brother",
                        Description = "Your older brother always spoke about the adventures he would embark upon once he came of age. Sure enough, it did not take long before he set out on his adventures, leaving you behind with your parents. He never returned and as you grew older, your determination to find him increased. Since then, you have searched for him everywhere you have travelled. From time to time, you have managed to pick up some clues from people who seem to have run into him, even though most leads are now very cold.",
                        PersonalQuest = "Find your lost brother. For this quest, you need to keep track of how many dungeons you have entered. At the start of each quest, roll 1d10. On a roll of 1, the dungeon you are heading to is the one in which you will find your brother. Place a secondary Quest Card in the pile not containing the Quest Room. The tile you enter next will contain your brother. Once you enter the room, roll 1d100, adding the number of dungeons you have entered. If the result is 60 or higher, you are too late and you find the remains of your dear brother on the floor, dead. It seems that he has been dead for some time. Devastated, you must decide if you will leave him where he is, or bring him out of the dungeon and bury him. In both cases you lose 3 Points of Sanity, but gain 250 XP. If you choose to bury him, you must carry him through the dungeon (of course letting go of him when danger approaches). The downside of this is that you cannot carry anything else you find (you may not search for treasures, or carry anything your comrades find). Once outside, you have a short ceremony and lay him to rest in a nearby meadow. You gain +10 RES permanently. If the result is lower than 60, your brother is alive, but badly wounded. Place him on the tile and you may move him just like the other heroes. Use the civilian Monster Card to represent him. If your brother makes it out alive, he will accompany you to the next settlement where you will part ways. You gain 1500 XP once you reach the settlement. If he dies during the dungeon crawl, revert to 'result higher than 60', If he dies in a skirmish, you do not need to carry him further, but the end result is the same."
                    },
                    new Background(_gameData)
                    {
                        Id = 7,
                        Name = "Revenge",
                        Description = "During the early days of adventuring, you were travelling the roads with one of your childhood friends. Having known each other since you could talk, you had experienced your entire childhood together. Thus, it was quite natural that you would leave your village together in search of fame, gold, and glory. You had been travelling for some weeks, still with no gold or glory in sight, when you were ambushed by two Brigands. Within seconds, your friend caught an arrow through the throat, and collapsed almost instantly. The rest of the fight is blurry, but you managed to overcome and kill both attackers. Although it has been some years since that episode, you sometimes still dream of your friend's last seconds, the shock on his face, and the gurgling sound as the last air passed through his windpipe.",
                        Trait = _gameData.GetHateTalentByCategory(GameDataService.HateCategory.Bandits),
                        PersonalQuest = "Furthermore,for every 5 enemies from that section that you deliver the killing blow, you gain an additional 250 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 8,
                        Name = "Bad Tempered",
                        Description = "We are all born differently, and we look upon life in different ways. In this case, your character is a tad bit on the negative side and is, quite frankly, really grumpy. Even on a sunny day, with wind in the hair, there is always something that could have been better. Maybe the sun doesn't have to shine straight in the eyes? That sound of creaking branches from the trees due to the wind is really annoying, isn't it?",
                        Trait = new Talent() {Name = TalentName.BadTempered, Description = "You will give a permanent -2 modifier to Party Morale. But, on the other hand, always expecting the worst can have its benefits as well. Your maximum Sanity is permanently increased by +2." }
                    },
                    new Background(_gameData)
                    {
                        Id = 9,
                        Name = "Poverty",
                        Description = "Life in the kingdom is not easy, and few are those who can spend money on a whim. Your family were, and still are, on the very edge of survival. Growing up, you did what you could to support your family, but decided early on that there was a need for a change. As soon as you were old enough, you left in search of other ways to bring food to the table. Living the life of an adventuring vagabond has sustained you so far, yet you still feel the urge to improve the situation for your family.",
                        Trait = new Talent() {Name = TalentName.Poverty, Description = "You know the value of each coin, and may never make a purchase, or lend out money, that would leave you with less than 10 c." },
                        PersonalQuest = "Furthermore,you must try to accumulate 1000 c for your family. Randomise which village (not Silver City) in which you were born and raised. If you are a dwarf, randomise between the two Dwarven settlements. Once you feel ready to hand over the money to your family, pay them a visit and hand over the money. This can be done by spending one Point of Movement in that village, and it, will grant you 2000 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 10,
                        Name = "Proving Your Worth",
                        Description = "Your father spent most of his career in the Royal Army and has been in retirement for a few years. During his career, he rose from the rank of Soldier to Centurion, a transition that few could make. Somewhere during this time, he was presented with exquisite armour by the Battalion Commander for some obscure reason. Due to the inestimable number of stories he has told you about his army life, you cannot remember the details precisely. Once you grew old enough and decided to take to the road, you were told by your father that if you could prove your worth, you could come back for that piece of armour.",
                        PersonalQuest = "Kill an enemy that gives you 450 XP or more. You do not need to strike the fatal blow, as long as your party makes the kill. Once that is done, return to your father to claim the armour. Randomise in which village (not Silver City) you were born and raised. If you are a dwarf, randomise between the two Dwarven settlements. This can be done by simply spending one Point of Movement in that village. This is the Armour of the Father as described in the 'Legendary Items' chapter."
                    },
                    new Background(_gameData)
                    {
                        Id = 11,
                        Name = "The Fraud",
                        Description = "Not applicable for wizards. Reroll if you are a wizard. You have been struggling most of your life. Food and money have always been scarce, and with no education or family business to inherit, the prospects for the future were not looking too bright. With such a life, you never quite got the attention from others that you craved either. All of this changed the day you stumbled across a wounded adventurer lying on the road. Although you did your best, you could not prevent the inevitable, and the adventurer passed away. You can't really explain why, but there and then you decided to change your fate. You quickly changed clothes with the fallen, and for the first time in your life you gripped a sword in your hand. From that day you vowed to never return home, but instead to keep travelling the kingdom. At the taverns and inns along the way you were often the centre of the attention as you told fabricated stories of your exploits and adventures. One such night of drinks and stories has landed you in your current situation. As the sun rises the morning after, you find yourself in the company of your party, apparently having promised them that you will be a great asset. With a feeling of panic, you realise that the time has now come for you to put your money where your mouth is.",
                        Trait = new Talent() {Name = TalentName.TheFraud, Description = "Deduct -10 from CS, RS, and Dodge since you have neither formal training nor experience with this. Your RES is also reduced with -10." },
                        PersonalQuest = "It is time to go from fraud to the real deal. Once you have improved CS, RS, and Dodge with +10 you can finally believe that you are more than empty words. Once this is achieved, you regain your RES and may increase it with another +10. You also gain an additional 1500 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 12,
                        Name = "The Noble",
                        Description = "Belonging to the lucky few privileged people in the kingdom, you have always gone to bed on a full stomach and, whenever needed, there was always a coin toss to solve any predicament in which you found yourself. For your father, this was not always the case. Although not poor, he was not of noble birth, and it was through marriage that he acquired his title. Realising that his child was growing spoiled, he determined you would have to make a living for yourself for at least a year before returning to the noble life. Now this year is well past, but you have grown fond of your new lifestyle, and in no hurry to return to dinner parties and boring meetings.",
                        Trait = new Talent() {Name = TalentName.TheNoble, Description = "You were not kicked out without means, and you have managed to retain some of the coins your mother secretly handed you before you parted. You start with 400 c instead of the normal 150 c. However, being accustomed to having money makes it extra hard when you have none. If you ever drop below 150 c, you start questioning if this is really what you should do for a living. Your resolve is reduced with -20 until you have enough money again (150 c)." }
                    },
                    new Background(_gameData)
                    {
                        Id = 13,
                        Name = "Sworn Enemy",
                        Description = "You are not the only one in your family who has taken up arms and travelled the world. Your older sister, who left home years ago, has made your family name somewhat famous, at least amongst certain people. To reach this position of fame, she had to put more than one enemy to the blade. Of course, even these rogues have relatives. One such relative has sworn revenge, and vowed to kill all of your bloodline.",
                        PersonalQuest = "Whenever you end up in battle with bandits, roll 1d10. On a result of 10, add one Bandit Leader to the encounter. This bandit has both the Hate special rule against the entire party, as well as Frenzy. Once the bandit is killed, you have rid your family of this sworn enemy, and you gain an extra 500 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 14,
                        Name = "The Family Keep",
                        Description = "Generations ago, your family was considered to be amongst the finer families in the Kingdom. The King granted your great grandfather a Keep as a token of his status. Sadly, his terrible financial sense and his lust for alcohol, gambling, and general decadence slowly reduced the Keep to a shadow of its former glory. Once your great grandfather passed away, the Keep was abandoned, and it has since been occupied by creatures of the night. Randomize one quest location on the map using the white numbers to situate the ruins of your Keep. Although it is far beyond repair, you feel it is your ancestral duty to purge the place of its current occupants.",
                        PersonalQuest = "Clear out the Keep. Whether you go there as a part of another quest, or if you decide to go there for this sole purpose, you must clear the entire dungeon. Every tile must be placed on the table and all enemies must be killed. If you go there specifically for this purpose, use the generic Dungeon Generator to create the dungeon. Once the dungeon is cleared, you gain 1500 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 15,
                        Name = "Troll Slayer",
                        Description = "To kill a troll is a true feat, and brave individuals who accomplish this deed are accorded the title of 'Troll Slayer'. Although it is not in any way a formal title, it is held in high regard amongst commoners and nobles alike. There have been two Troll Slayers in your lineage who preceded you. Since childhood, you have vowed to honour the family tradition by ridding the world of yet another troll.",
                        PersonalQuest = "You must slay a troll. To rightfully claim the title of Troll Slayer, you must land the killing blow on a troll (of any kind). If you achieve this, you have both honoured your lineage and gained a further +1000 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 16,
                        Name = "Revenge",
                        Description = "A little over a decade ago, your village was savagely attacked by a large group of beastmen. Although the villagers bravely tried to counter the attack, many were killed and several houses were lost to fire. Amongst the beastmen was a huge Minotaur, and it was responsible for the worst of the carnage. You hid throughout the fight, but at one point, the beast was close enough for you to make out a strange scar on its chest. The image of that beast has stayed with you ever since, and as you have grown to adulthood your lust for revenge has grown stronger.",
                        Trait = _gameData.GetHateTalentByCategory(GameDataService.HateCategory.Minotaurs),
                        PersonalQuest = "Every time you fight a Minotaur, roll 1d6. On a result of 1, you recognize the scar. If you defeat the beast, you gain an additional +1000 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 17,
                        Name = "A New Home",
                        Description = "Your parents had you rather late in life; nevertheless, you had a very happy childhood. You received all of the love any child could wish for, and there was always food on the table, even if it was far from fancy. Your parents grew older, and eventually passed away within a few weeks of each other. A few days after your mother's funeral, you are visited by a stranger who claims to own the house. He presents you with a contract which shows that your parents were deeply in debt to him, and that the house was given to him as payment. You are given two days to clear out. With nowhere to go, you don't take much with you other than the thought of finding a new home.",
                        PersonalQuest = "Even though the adventuring lifestyle suits you much better than you had expected, you still yearn for a place to call your own. Once you have acquired the Bergmeister Estate, you gain 1500 XP."
                    },
                    new Background(_gameData)
                    {
                        Id = 18,
                        Name = "The Apprentice",
                        Description = "As with most young ones in the Kingdom, the time for being a carefree child is short. Most start working before their 10 birthday, and for you there was no difference. Although most children would be toiling in the fields, you were given the chance to work for the local blacksmith. He had grown fond of you as a youngster, and with no children of his own, he suggested you could help him out with the more menial tasks. At first, your work was to keep the smithy clean, bring water from the well, and run errands to and from customers. As you grew older, you began to learn some of the trade. In the end though, the blacksmith could not afford to pay you a salary that would sustain you. So you decided to find other means of bringing bread to the table.",
                        Trait = new Talent() {Name = TalentName.TheApprentice, Description = "Your blacksmithing skills are truly useful while adventuring. Whenever using an armour repair kit or a whetstone, you automatically regain 3 Points of Durability on your gear." }
                    },
                    new Background(_gameData)
                    {
                        Id = 19,
                        Name = "Weak",
                        Description = "As a child you were prone to catching every cold there was. According to your mother, there isn't a disease around that you haven't suffered from! That's not true of course, but the fact remains that you seem to contract diseases more easily than most. Perhaps your father is right, and you are 'just plain weak', or maybe you are just suffering from an immune system that struggles to keep up. Luckily, this seems to have improved with time. Now when you fall ill, you usually feel better within a few days.",
                        Trait = new Talent() {Name = TalentName.Weak, Description = "Whenever rolling for contracting a disease, you suffer an -10 modifier to your CON. However, once you are cured of your 3rd disease, your immune system kicks into overdrive and you instead get a +10 modifier to your CON when rolling for disease, and you cure yourself on a natural CON roll of 01-10 instead of 01-05." }
                    },
                    new Background(_gameData)
                    {
                        Id = 20,
                        Name = "Afraid of Heights",
                        Description = "There are few things in life you fear, and that is a well-known fact amongst friends and family. You once saved a child in your village who came face to face with a stray wolf. With nothing but a stick in your hand, you charged the beast, screaming and flailing wildly. Though you did get a fair share of cuts, you managed to scare the beast away. Since then, the fearless attribute has stuck with you. However, as with most people, you do have an Achilles heel, and it is a fear that most people don't know that you have. You are deathly scared of heights. Whenever you are only a few feet above the ground your knees begin to shake, and you start to sweat profusely.",
                        Trait = new Talent() {Name = TalentName.AfraidOfHeights, Description = "Whenever you take a Fear Test (but not a Terror Test), you gain a +10 modifier on your RES. However, whenever you are on a bridge your resolve is halved (RDD) and your CS and RS suffer a -20 modifier." }
                    }
                };
        }
    }
}