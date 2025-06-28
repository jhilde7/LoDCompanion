using LoDCompanion.Models;
using LoDCompanion.Models.Characters;
using LoDCompanion.Utilities;
using System.Text.Json.Serialization;

namespace LoDCompanion.Services.GameData
{
    public class GameDataRegistryService
    {
        private readonly GameDataConfiguration _gameData;

        public GameDataRegistryService(GameDataConfiguration gameData)
        {
            _gameData = gameData;
        }

        public GameDataConfiguration GetGameData()
        {
            return _gameData;
        }
        public Species? GetSpeciesByName(string speciesName)
        {
            return _gameData.Species?.FirstOrDefault(s => s.Name == speciesName);
        }

        public Profession? GetProfessionByName(string professionName)
        {
            return _gameData.Profession?.FirstOrDefault(s => s.Name == professionName);
        }

        public RoomInfo? GetRoomInfoByName(string roomName)
        {
            return _gameData.RoomInfo?.FirstOrDefault(s => s.Name == roomName);
        }

        public Perk? GetPerkByName(string perkName)
        {
            return _gameData.Perk?.FirstOrDefault(s => s.Name == perkName);
        }

        public Talent? GetTalentByName(string talentName)
        {
            return _gameData.Talent?.FirstOrDefault(s => s.Name == talentName);
        }

        public Spell? GetSpellByName(string spellName)
        {
            return _gameData.Spell?.FirstOrDefault(s => s.Name == spellName);
        }

        public Prayer? GetPrayerByName(string name)
        {
            return _gameData.Prayer?.FirstOrDefault(s => s.Name == name);
        }

        public List<Spell>? GetSpellsByLevel(int level)
        {
            return _gameData.SpellsByLevel?[level.ToString()];
        }

        public List<Prayer>? GetPrayersByLevel(int level)
        {
            return _gameData.PrayersByLevel?[level.ToString()];
        }

        public Equipment? GetEquipmentByName(string equipmentName)
        {
            return _gameData.Equipment?.FirstOrDefault(s => s.Name == equipmentName);
        }
    }

    public class GameDataConfiguration
    {
        public List<Species>? Species { get; set; }
        public List<Profession>? Profession { get; set; }
        public List<RoomInfo>? RoomInfo { get; set; }
        public List<Perk>? Perk { get; set; }
        public List<Talent>? Talent { get; set; }
        public List<Spell>? Spell { get; set; }
        public List<Prayer>? Prayer { get; set; }
        public List<Equipment>? Equipment { get; set; }
        public Dictionary<string, List<Spell>>? SpellsByLevel { get; set; }
        public Dictionary<string, List<Prayer>>? PrayersByLevel { get; set; }
    }

    public class Species
    {
        private readonly GameDataRegistryService _gameData;

        [JsonPropertyName("species_name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("base_strength")]
        public int BaseStrength { get; set; }
        [JsonPropertyName("base_constitution")]
        public int BaseConstitution { get; set; }
        [JsonPropertyName("base_dexterity")]
        public int BaseDexterity { get; set; }
        [JsonPropertyName("base_wisdom")]
        public int BaseWisdom { get; set; }
        [JsonPropertyName("base_resolve")]
        public int BaseResolve { get; set; }
        [JsonPropertyName("base_hitpoints")]
        public int BaseHP { get; set; }

        public Species(GameDataRegistryService gameData)
        {
            _gameData = gameData;
        }

        public List<Talent> GetTraits()
        {
            List<Talent> traits = new List<Talent>();
            if (Name != null)
            {
                switch (Name)
                {
                    case "Dwarf":
                        traits.Add(_gameData.GetTalentByName("Night Vision") ?? new Talent { Name = "Night Vision", Description = "", IsNightVision = true });
                        traits.Add(new TalentLookupService(_gameData).GetTalentByName("Hate") ?? new Talent());
                        return traits;
                    case "Elf":
                        traits.Add(_gameData.GetTalentByName("Night Vision") ?? new Talent { Name = "Night Vision", Description = "", IsNightVision = true });
                        traits.Add(_gameData.GetTalentByName("Perfect Hearing") ?? new Talent { Name = "Perfect Hearing", Description = "", IsPerfectHearing = true });
                        return traits;
                    case "Halfling":
                        traits.Add(_gameData.GetTalentByName("Lucky") ?? new Talent());
                        return traits;
                    default: return traits;
                }
            }
            else
            {
                return traits;
            }
        }
    }

    public class Profession
    {
        private readonly GameDataRegistryService _gameData;
        [JsonPropertyName("profession_name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("combat_skill_modifier")]
        public int CombatSkillModifier { get; set; }
        [JsonPropertyName("ranged_skill_modifier")]
        public int RangedSkillModifier { get; set; }
        [JsonPropertyName("dodge_skill_modifier")]
        public int DodgeSkillModifier { get; set; }
        [JsonPropertyName("pick_locks_skill_modifier")]
        public int PickLocksSkillModifier { get; set; }
        [JsonPropertyName("barter_skill_modifier")]
        public int BarterSkillModifier { get; set; }
        [JsonPropertyName("heal_skill_modifier")]
        public int HealSkillModifier { get; set; }
        [JsonPropertyName("alchemy_skill_modifier")]
        public int AlchemySkillModifier { get; set; }
        [JsonPropertyName("perception_skill_modifier")]
        public int PerceptionSkillModifier { get; set; }
        [JsonPropertyName("arcane_arts_skill_modifier")]
        public int? ArcaneArtsSkillModifier { get; set; }
        [JsonPropertyName("foraging_skill_modifier")]
        public int ForagingSkillModifier { get; set; }
        [JsonPropertyName("battle_prayers_skill_modifier")]
        public int? BattlePrayersSkillModifier { get; set; }
        [JsonPropertyName("hp_modifier")]
        public int HPModifier { get; set; }
        [JsonPropertyName("max_armour_type")]
        public int MaxArmourType { get; set; }
        [JsonPropertyName("max_melee_weapon_type")]
        public int MaxMeleeWeaponType { get; set; }


        // Lists of names/IDs for starting items, talents, perks, spells, prayers
        [JsonPropertyName("starting_backpack_list")]
        public List<string> StartingBackpackList { get; set; } = new List<string>();
        [JsonPropertyName("starting_talent_list")]
        public List<string> StartingTalentList { get; set; } = new List<string>();
        [JsonPropertyName("level_up_cost")]
        public Dictionary<string, int> LevelUpCost { get; set; } = new Dictionary<string, int>();

        public List<Perk> StartingPerkList { get; set; } = new List<Perk>();

        // Assuming Prayer will be a class, this would be a list of prayer names/IDs
        public List<string> FreeSkills { get; set; } = new List<string>();

        public Profession(GameDataRegistryService gameData)
        {
            _gameData = gameData;
            SetStartingPerks();
            GetFreeSkills();
        }

        private void SetStartingPerks()
        {
            if (Name == "Barbarian")
            {
                StartingPerkList.Add(_gameData.GetPerkByName("Frenzy") ?? new Perk());
            }
        }

        // Method to get the list of free skills, if needed outside directly accessing the list
        public void GetFreeSkills()
        {
            FreeSkills.Clear();
            if (CombatSkillModifier < 0)
            {
                FreeSkills.Add("Combat Skill");
            }
            if (RangedSkillModifier < 0)
            {
                FreeSkills.Add("Ranged Skill");
            }
            if (DodgeSkillModifier < 0)
            {
                FreeSkills.Add("Dodge");
            }
            if (PickLocksSkillModifier < 0)
            {
                FreeSkills.Add("Pick Locks");
            }
            if (BarterSkillModifier < 0)
            {
                FreeSkills.Add("Barter");
            }
            if (HealSkillModifier < 0)
            {
                FreeSkills.Add("Heal");
            }
            if (AlchemySkillModifier < 0)
            {
                FreeSkills.Add("Alchemy");
            }
            if (PerceptionSkillModifier < 0)
            {
                FreeSkills.Add("Perception");
            }
            if (ForagingSkillModifier < 0)
            {
                FreeSkills.Add("Foraging");
            }
        }
    }

    public class RoomInfo
    {
        [JsonPropertyName("roomName")]
        public string? Name { get; set; }
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("special_rules")]
        public string? SpecialRules { get; set; }
        [JsonPropertyName("threat_level_modifier")]
        public int? ThreatLevelModifier { get; set; }
        [JsonPropertyName("party_morale_modifier")]
        public int? PartyMoraleModifier { get; set; }
        [JsonPropertyName("size")]
        public int[]? Size { get; set; }
        [JsonPropertyName("door_count")]
        public int? DoorCount { get; set; }
        [JsonPropertyName("furniture_list")]
        public List<string>? FurnitureList { get; set; }
        [JsonPropertyName("encounter_modifier")]
        public int? EncounterModifier { get; set; }
        [JsonPropertyName("has_levers")]
        public bool HasLevers { get; set; }
        [JsonPropertyName("random_encounter")]
        public bool RandomEncounter { get; set; }
        [JsonPropertyName("has_special")]
        public bool HasSpecial { get; set; }

        public RoomInfo()
        {

        }
    }

    public class Perk
    {
        [JsonPropertyName("perk_name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("effect")]
        public string Effect { get; set; } = string.Empty;
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        public Perk() { }

    }

    public class Spell
    {
        [JsonPropertyName("spell_name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("casting_value")]
        public int CastingValue { get; set; } // The base difficulty or power of the spell
        [JsonPropertyName("mana_cost")]
        public int ManaCost { get; set; }
        [JsonPropertyName("upkeep_cost")]
        public int UpkeepCost { get; set; } // Per turn cost for sustained spells
        [JsonPropertyName("turn_duration")]
        public int TurnDuration { get; set; } // Duration in turns
        [JsonPropertyName("add_caster_lvl_to_duration")]
        public bool AddCasterLvlToDuration { get; set; }

        // Damage properties for direct damage spells
        [JsonPropertyName("is_damage_spell")]
        public bool IsDamageSpell { get; set; }
        [JsonPropertyName("min_damage")]
        public int MinDamage { get; set; }
        [JsonPropertyName("max_damage")]
        public int MaxDamage { get; set; }
        [JsonPropertyName("include_caster_level_in_damage")]
        public bool IncludeCasterLevelInDamage { get; set; }
        [JsonPropertyName("is_armour_piercing")]
        public bool IsArmourPiercing { get; set; }
        [JsonPropertyName("is_water_dmg")]
        public bool IsWaterDmg { get; set; }
        [JsonPropertyName("is_fire_dmg")]
        public bool IsFireDmg { get; set; }
        [JsonPropertyName("is_lightning")]
        public bool IsLightning { get; set; }

        // Spell Type Flags (can be used for filtering or specific effects)
        [JsonPropertyName("is_quick_spell")]
        public bool IsQuickSpell { get; set; }
        [JsonPropertyName("is_incantation")]
        public bool IsIncantation { get; set; }
        [JsonPropertyName("is_magic_missile")]
        public bool IsMagicMissile { get; set; }
        [JsonPropertyName("is_touch")]
        public bool IsTouch { get; set; }
        [JsonPropertyName("is_necromancy")]
        public bool IsNecromancy { get; set; }
        [JsonPropertyName("is_destruction")]
        public bool IsDestruction { get; set; }
        [JsonPropertyName("is_alteration")]
        public bool IsAlteration { get; set; }
        [JsonPropertyName("is_restoration")]
        public bool IsRestoration { get; set; }
        [JsonPropertyName("is_mysticism")]
        public bool IsMysticism { get; set; }
        [JsonPropertyName("is_hex")]
        public bool IsHex { get; set; }
        [JsonPropertyName("is_illusion")]
        public bool IsIllusion { get; set; }
        [JsonPropertyName("is_enchantment")]
        public bool IsEnchantment { get; set; }
        [JsonPropertyName("is_conjuration")]
        public bool IsConjuration { get; set; }
        [JsonPropertyName("is_divination")]
        public bool IsDivination { get; set; }

        // AOE properties
        [JsonPropertyName("is_aoe_spell")]
        public bool IsAOESpell { get; set; }
        [JsonPropertyName("aoe_min_damage")]
        public int AOEMinDamage { get; set; }
        [JsonPropertyName("aoe_max_damage")]
        public int AOEMaxDamage { get; set; }
        [JsonPropertyName("aoe_radius")]
        public int AOERadius { get; set; } // Or target count for EnemiesAOE
        [JsonPropertyName("aoe_includes_caster_level")]
        public bool AOEIncludesCasterLevel { get; set; }

        // Constructor
        public Spell()
        {
        }

        public int GetSpellDamage(int casterLevel)
        {
            if (!IsDamageSpell) return 0;

            int calculatedDamage = RandomHelper.GetRandomNumber(MinDamage, MaxDamage);
            if (IncludeCasterLevelInDamage)
            {
                calculatedDamage += casterLevel;
            }
            return calculatedDamage;
        }

        public int GetSpellDamageAOE(int casterLevel)
        {
            if (!IsAOESpell) return 0;

            int calculatedDamage = RandomHelper.GetRandomNumber(AOEMinDamage, AOEMaxDamage);
            if (AOEIncludesCasterLevel)
            {
                calculatedDamage += casterLevel;
            }
            return calculatedDamage;
        }

        /// <summary>
        /// Represents the casting attempt for the spell.
        /// Actual mana deduction, success/failure handling, and effects on game state
        /// would be managed by a higher-level SpellCastingService or Hero class.
        /// </summary>
        /// <param name="hero">The hero attempting to cast the spell.</param>
        /// <param name="skillRoll">The result of the hero's ArcaneArts skill roll.</param>
        /// <returns>True if the spell was successfully cast, false otherwise.</returns>
        public bool CastSpell(Hero hero, int skillRoll)
        {
            // Simplified logic: Check if hero has enough mana
            if (hero.CurrentEnergy < ManaCost)
            {
                // Optionally log: Console.WriteLine($"{hero.Name} does not have enough mana to cast {SpellName}.");
                return false;
            }

            // Check if skill roll meets or exceeds casting value
            if (skillRoll >= CastingValue)
            {
                hero.CurrentEnergy -= ManaCost; // Deduct mana
                // Spell effect would be handled by a SpellCastingService
                // Console.WriteLine($"{hero.Name} successfully cast {SpellName}!");
                return true;
            }
            else
            {
                // Optionally log: Console.WriteLine($"{hero.Name} failed to cast {SpellName}.");
                return false;
            }
        }
    }

    public class Prayer
    {
        public string Name { get; set; } = string.Empty;
        public int EnergyCost { get; set; } = 1;
        public bool IsActive { get; set; }
        public string PrayerEffect { get; set; } = string.Empty;// This could be an enum or a more complex object if effects become varied.

        // Constructor
        public Prayer()
        {

        }

        // Method to get the prayer effect description
        // This method can be expanded to apply the effect in a game logic service.
        public string GetPrayerEffectDescription()
        {
            return PrayerEffect;
        }

        // Example method for applying the prayer effect (logic would typically be in a service)
        public void ApplyEffect(Hero hero)
        {
            // This is a placeholder. Real logic would depend on the 'PrayerEffect' string
            // or if 'PrayerEffect' was an enum/interface.
            Console.WriteLine($"{hero.Name} is affected by {Name}: {PrayerEffect}");
            // Example: if PrayerName == "Heal" -> hero.HP += amount;
        }
    }

}
