using System.Text.Json.Serialization;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.GameData
{
    public class TalentLookupService
    {
        // This class will handle the logic for retrieving talent names based on categories.
        // It's stateless, so it can be registered as a singleton service if preferred.
        private readonly GameDataRegistryService _gameData;
        public List<Talent> PhysicalTalents => GetTalentsByCategory("Physical");
        public List<Talent> CombatTalents => GetTalentsByCategory("Combat");
        public List<Talent> FaithTalents => GetTalentsByCategory("Faith");
        public List<Talent> AlchemistTalents => GetTalentsByCategory("Alchemist");
        public List<Talent> CommonTalents => GetTalentsByCategory("Common");
        public List<Talent> MagicTalents => GetTalentsByCategory("Magic");
        public List<Talent> SneakyTalents => GetTalentsByCategory("Sneaky");
        public List<Talent> MentalTalents => GetTalentsByCategory("Mental");

        public TalentLookupService(GameDataRegistryService gameData)
        {
            _gameData = gameData;
        }

        public Talent GetTalentForHumanByCategory(string? category = null)
        {
            int roll;

            switch (category)
            {
                case "Physical":
                    roll = RandomHelper.GetRandomNumber(1, 8);
                    break;
                case "Combat":
                    roll = RandomHelper.GetRandomNumber(9, 23);
                    break;
                case "Faith":
                    roll = RandomHelper.GetRandomNumber(24, 29);
                    break;
                case "Alchemist":
                    roll = RandomHelper.GetRandomNumber(30, 36);
                    break;
                case "Common":
                    roll = RandomHelper.GetRandomNumber(37, 46);
                    break;
                case "Magic":
                    roll = RandomHelper.GetRandomNumber(47, 58);
                    break;
                case "Sneaky":
                    roll = RandomHelper.GetRandomNumber(59, 69);
                    break;
                case "Mental":
                    roll = RandomHelper.GetRandomNumber(70, 75);
                    break;
                default:
                    roll = RandomHelper.GetRandomNumber(1, 75);
                    break;
            }

            return roll switch
            {
                1 => PhysicalTalents.Find(talent => talent.IsCatLike) ?? new Talent(),
                2 => PhysicalTalents.Find(talent => talent.IsFast) ?? new Talent(),
                3 => PhysicalTalents.Find(talent => talent.IsResilient) ?? new Talent(),
                4 => PhysicalTalents.Find(talent => talent.IsResistDisease) ?? new Talent(),
                5 => PhysicalTalents.Find(talent => talent.IsResistPoison) ?? new Talent(),
                6 => PhysicalTalents.Find(talent => talent.IsStrong) ?? new Talent(),
                7 => PhysicalTalents.Find(talent => talent.IsStrongBuild) ?? new Talent(),
                8 => PhysicalTalents.Find(talent => talent.IsTank) ?? new Talent(),
                9 => CombatTalents.Find(talent => talent.IsAxeman) ?? new Talent(),
                10 => CombatTalents.Find(talent => talent.IsBruiser) ?? new Talent(),
                11 => CombatTalents.Find(talent => talent.IsDeathLament) ?? new Talent(),
                12 => CombatTalents.Find(talent => talent.IsDisarm) ?? new Talent(),
                13 => CombatTalents.Find(talent => talent.IsDualWield) ?? new Talent(),
                14 => CombatTalents.Find(talent => talent.IsFastReload) ?? new Talent(),
                15 => CombatTalents.Find(talent => talent.IsMarksman) ?? new Talent(),
                16 => CombatTalents.Find(talent => talent.IsMightyBlow) ?? new Talent(),
                17 => CombatTalents.Find(talent => talent.IsParryMaster) ?? new Talent(),
                18 => CombatTalents.Find(talent => talent.IsPerfectShot) ?? new Talent(),
                19 => CombatTalents.Find(talent => talent.IsRiposteMaster) ?? new Talent(),
                20 => CombatTalents.Find(talent => talent.IsSniper) ?? new Talent(),
                21 => CombatTalents.Find(talent => talent.IsSwordsman) ?? new Talent(),
                22 => CombatTalents.Find(talent => talent.IsTightGrip) ?? new Talent(),
                23 => CombatTalents.Find(talent => talent.IsTunnelFighter) ?? new Talent(),
                24 => FaithTalents.Find(talent => talent.IsDevoted) ?? new Talent(),
                25 => FaithTalents.Find(talent => talent.IsGodsChosen) ?? new Talent(),
                26 => FaithTalents.Find(talent => talent.IsHealer) ?? new Talent(),
                27 => FaithTalents.Find(talent => talent.IsMessiah) ?? new Talent(),
                28 => FaithTalents.Find(talent => talent.IsPure) ?? new Talent(),
                29 => FaithTalents.Find(talent => talent.IsReliquary) ?? new Talent(),
                30 => AlchemistTalents.Find(talent => talent.IsGatherer) ?? new Talent(),
                31 => AlchemistTalents.Find(talent => talent.IsHarvester) ?? new Talent(),
                32 => AlchemistTalents.Find(talent => talent.IsKeenEye) ?? new Talent(),
                33 => AlchemistTalents.Find(talent => talent.IsMasterHealer) ?? new Talent(),
                34 => AlchemistTalents.Find(talent => talent.IsPerfectToss) ?? new Talent(),
                35 => AlchemistTalents.Find(talent => talent.IsPoisoner) ?? new Talent(),
                36 => AlchemistTalents.Find(talent => talent.IsPowerfulPotions) ?? new Talent(),
                37 => CommonTalents.Find(talent => talent.IsCharming) ?? new Talent(),
                38 => CommonTalents.Find(talent => talent.IsDisciplined) ?? new Talent(),
                39 => CommonTalents.Find(talent => talent.IsHunter) ?? new Talent(),
                40 => CommonTalents.Find(talent => talent.IsLucky) ?? new Talent(),
                41 => CommonTalents.Find(talent => talent.IsMasterCook) ?? new Talent(),
                42 => CommonTalents.Find(talent => talent.IsNaturalLeader) ?? new Talent(),
                43 => CommonTalents.Find(talent => talent.IsRingBearer) ?? new Talent(),
                44 => CommonTalents.Find(talent => talent.IsSurvivalist) ?? new Talent(),
                45 => CommonTalents.Find(talent => talent.IsSwiftLeader) ?? new Talent(),
                46 => CommonTalents.Find(talent => talent.IsVeteran) ?? new Talent(),
                47 => MagicTalents.Find(talent => talent.IsBloodMagic) ?? new Talent(),
                48 => MagicTalents.Find(talent => talent.IsConjurer) ?? new Talent(),
                49 => MagicTalents.Find(talent => talent.IsDivinator) ?? new Talent(),
                50 => MagicTalents.Find(talent => talent.IsFastReflexes) ?? new Talent(),
                51 => MagicTalents.Find(talent => talent.IsFocused) ?? new Talent(),
                52 => MagicTalents.Find(talent => talent.IsRestorer) ?? new Talent(),
                53 => MagicTalents.Find(talent => talent.IsMystic) ?? new Talent(),
                54 => MagicTalents.Find(talent => talent.IsNecromancer) ?? new Talent(),
                55 => MagicTalents.Find(talent => talent.IsPowerfulMissiles) ?? new Talent(),
                56 => MagicTalents.Find(talent => talent.IsSummoner) ?? new Talent(),
                57 => MagicTalents.Find(talent => talent.IsSustainer) ?? new Talent(),
                58 => MagicTalents.Find(talent => talent.IsThrifty) ?? new Talent(),
                59 => SneakyTalents.Find(talent => talent.IsAssassin) ?? new Talent(),
                60 => SneakyTalents.Find(talent => talent.IsBackstabber) ?? new Talent(),
                61 => SneakyTalents.Find(talent => talent.IsCutpurse) ?? new Talent(),
                62 => SneakyTalents.Find(talent => talent.IsEvaluate) ?? new Talent(),
                63 => SneakyTalents.Find(talent => talent.IsLockPicker) ?? new Talent(),
                64 => SneakyTalents.Find(talent => talent.IsMechanicalGenius) ?? new Talent(),
                65 => SneakyTalents.Find(talent => talent.IsNimble) ?? new Talent(),
                66 => SneakyTalents.Find(talent => talent.IsQuickFingers) ?? new Talent(),
                67 => SneakyTalents.Find(talent => talent.IsSharpEyed) ?? new Talent(),
                68 => SneakyTalents.Find(talent => talent.IsSenseForGold) ?? new Talent(),
                69 => SneakyTalents.Find(talent => talent.IsTrapFinder) ?? new Talent(),
                70 => MentalTalents.Find(talent => talent.IsBraveheart) ?? new Talent(),
                71 => MentalTalents.Find(talent => talent.IsConfident) ?? new Talent(),
                72 => MentalTalents.Find(talent => talent.IsFearless) ?? new Talent(),
                73 => MentalTalents.Find(talent => talent.IsHate) ?? new Talent(),
                74 => MentalTalents.Find(talent => talent.IsStrongMinded) ?? new Talent(),
                75 => MentalTalents.Find(talent => talent.IsWise) ?? new Talent(),
                _ => new Talent()
            };
        }

        public List<Talent> GetTalentCategoryAtLevelup(Profession profession, int level)
        {

            switch (profession.Name)
            {
                case "Alchemist":
                    return level switch
                    {
                        3 => MentalTalents,
                        4 => CommonTalents,
                        6 => CombatTalents,
                        7 => MentalTalents,
                        8 => CommonTalents,
                        _ => AlchemistTalents
                    };
                case "Barbarian":
                    return level switch
                    {
                        2 => PhysicalTalents,
                        4 => MentalTalents,
                        5 => CommonTalents,
                        7 => PhysicalTalents,
                        9 => CommonTalents,
                        _ => CombatTalents
                    };
                case "Ranger":
                    return level switch
                    {
                        2 => PhysicalTalents,
                        4 => CommonTalents,
                        5 => MentalTalents,
                        7 => PhysicalTalents,
                        8 => CommonTalents,
                        9 => MentalTalents,
                        _ => CombatTalents
                    };
                case "Rogue":
                    return level switch
                    {
                        2 => PhysicalTalents,
                        3 => SneakyTalents,
                        5 => MentalTalents,
                        6 => PhysicalTalents,
                        8 => SneakyTalents,
                        9 => CommonTalents,
                        _ => CombatTalents
                    };
                case "Thief":
                    return level switch
                    {
                        3 => CommonTalents,
                        5 => CombatTalents,
                        6 => MentalTalents,
                        7 => PhysicalTalents,
                        8 => CommonTalents,
                        9 => CombatTalents,
                        _ => SneakyTalents
                    };
                case "Warrior":
                    return level switch
                    {
                        2 => MentalTalents,
                        4 => PhysicalTalents,
                        6 => CommonTalents,
                        7 => MentalTalents,
                        9 => CommonTalents,
                        _ => CombatTalents
                    };
                case "Warrior Priest":
                    return level switch
                    {
                        2 => MentalTalents,
                        4 => CombatTalents,
                        5 => PhysicalTalents,
                        7 => CombatTalents,
                        8 => MentalTalents,
                        10 => CombatTalents,
                        _ => FaithTalents
                    };
                case "Wizard":
                    return level switch
                    {
                        3 => CommonTalents,
                        4 => MentalTalents,
                        6 => MentalTalents,
                        7 => PhysicalTalents,
                        9 => CommonTalents,
                        10 => MentalTalents,
                        _ => MagicTalents
                    };
                default: return CommonTalents;
            }
        }

        private List<Talent> GetTalentsByCategory(string category)
        {
            switch (category)
            {
                case "Physical":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Catlike") ?? new Talent(),
                        _gameData.GetTalentByName("Fast") ?? new Talent(),
                        _gameData.GetTalentByName("Resilient") ?? new Talent(),
                        _gameData.GetTalentByName("Resistance to Disease") ?? new Talent(),
                        _gameData.GetTalentByName("Resistance to Poison") ?? new Talent(),
                        _gameData.GetTalentByName("Strong") ?? new Talent(),
                        _gameData.GetTalentByName("Strong Build") ?? new Talent(),
                        _gameData.GetTalentByName("Tank") ?? new Talent()
                    };
                case "Combat":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Axeman") ?? new Talent(),
                        _gameData.GetTalentByName("Bruiser") ?? new Talent(),
                        _gameData.GetTalentByName("Death Lament") ?? new Talent(),
                        _gameData.GetTalentByName("Disarm") ?? new Talent(),
                        _gameData.GetTalentByName("Dual Wield") ?? new Talent(),
                        _gameData.GetTalentByName("Fast Reload") ?? new Talent(),
                        _gameData.GetTalentByName("Marksman") ?? new Talent(),
                        _gameData.GetTalentByName("Mighty Blow") ?? new Talent(),
                        _gameData.GetTalentByName("Parry Master") ?? new Talent(),
                        _gameData.GetTalentByName("Perfect Shot") ?? new Talent(),
                        _gameData.GetTalentByName("Riposte Master") ?? new Talent(),
                        _gameData.GetTalentByName("Sniper") ?? new Talent(),
                        _gameData.GetTalentByName("Swordsman") ?? new Talent(),
                        _gameData.GetTalentByName("Tight Grip") ?? new Talent(),
                        _gameData.GetTalentByName("Tunnel Fighter") ?? new Talent()
                    };
                case "Faith":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Devoted") ?? new Talent(),
                        _gameData.GetTalentByName("Gods Chosen") ?? new Talent(),
                        _gameData.GetTalentByName("Healer") ?? new Talent(),
                        _gameData.GetTalentByName("Messiah") ?? new Talent(),
                        _gameData.GetTalentByName("Pure") ?? new Talent(),
                        _gameData.GetTalentByName("Reliquary") ?? new Talent()
                    };
                case "Alchemist":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Gatherer") ?? new Talent(),
                        _gameData.GetTalentByName("Harvester") ?? new Talent(),
                        _gameData.GetTalentByName("Keen Eye") ?? new Talent(),
                        _gameData.GetTalentByName("Master Healer") ?? new Talent(),
                        _gameData.GetTalentByName("Perfect Toss") ?? new Talent(),
                        _gameData.GetTalentByName("Poisoner") ?? new Talent(),
                        _gameData.GetTalentByName("Powerful Potions") ?? new Talent()
                    };
                case "Common":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Charming") ?? new Talent(),
                        _gameData.GetTalentByName("Disciplined") ?? new Talent(),
                        _gameData.GetTalentByName("Hunter") ?? new Talent(),
                        _gameData.GetTalentByName("Lucky") ?? new Talent(),
                        _gameData.GetTalentByName("Master Cook") ?? new Talent(),
                        _gameData.GetTalentByName("Natural Leader") ?? new Talent(),
                        _gameData.GetTalentByName("Ring Bearer") ?? new Talent(),
                        _gameData.GetTalentByName("Survivalist") ?? new Talent(),
                        _gameData.GetTalentByName("Swift Leader") ?? new Talent(),
                        _gameData.GetTalentByName("Veteran") ?? new Talent()
                    };
                case "Magic":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Blood Magic") ?? new Talent(),
                        _gameData.GetTalentByName("Conjurer") ?? new Talent(),
                        _gameData.GetTalentByName("Divinator") ?? new Talent(),
                        _gameData.GetTalentByName("Fast Reflexes") ?? new Talent(),
                        _gameData.GetTalentByName("Focused") ?? new Talent(),
                        _gameData.GetTalentByName("Restorer") ?? new Talent(),
                        _gameData.GetTalentByName("Mystic") ?? new Talent(),
                        _gameData.GetTalentByName("Necromancer") ?? new Talent(),
                        _gameData.GetTalentByName("Powerful Missiles") ?? new Talent(),
                        _gameData.GetTalentByName("Summoner") ?? new Talent(),
                        _gameData.GetTalentByName("Sustainer") ?? new Talent(),
                        _gameData.GetTalentByName("Thrifty") ?? new Talent()
                    };
                case "Sneaky":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Assassin") ?? new Talent(),
                        _gameData.GetTalentByName("Backstabber") ?? new Talent(),
                        _gameData.GetTalentByName("Cutpurse") ?? new Talent(),
                        _gameData.GetTalentByName("Evaluate") ?? new Talent(),
                        _gameData.GetTalentByName("Lock Picker") ?? new Talent(),
                        _gameData.GetTalentByName("Mechanical Genius") ?? new Talent(),
                        _gameData.GetTalentByName("Nimble") ?? new Talent(),
                        _gameData.GetTalentByName("Quick Fingers") ?? new Talent(),
                        _gameData.GetTalentByName("Sharp-Eyed") ?? new Talent(),
                        _gameData.GetTalentByName("Sense for Gold") ?? new Talent(),
                        _gameData.GetTalentByName("Streetwise") ?? new Talent(),
                        _gameData.GetTalentByName("Trap Finder") ?? new Talent()
                    };
                case "Mental":
                    return new List<Talent>()
                    {
                        _gameData.GetTalentByName("Braveheart") ?? new Talent(),
                        _gameData.GetTalentByName("Confident") ?? new Talent(),
                        _gameData.GetTalentByName("Fearless") ?? new Talent(),
                        _gameData.GetTalentByName("Hate") ?? new Talent(),
                        _gameData.GetTalentByName("Strong-Minded") ?? new Talent(),
                        _gameData.GetTalentByName("Wise") ?? new Talent()
                    };
                default:
                    return new List<Talent>();
            }
        }

        public Talent GetTalentByName(string talentName)
        {
            List<Talent> list = new List<Talent>();
            list.AddRange(PhysicalTalents);
            list.AddRange(CombatTalents);
            list.AddRange(FaithTalents);
            list.AddRange(AlchemistTalents);
            list.AddRange(CommonTalents);
            list.AddRange(MagicTalents);
            list.AddRange(SneakyTalents);
            list.AddRange(MentalTalents);

            return list.FirstOrDefault(t => t.Name == talentName) ?? new Talent();
        }

        internal Talent GetRandomTalent()
        {
            Talent talent;
            do
            {
                talent = GetTalentForHumanByCategory();

            } while (talent == null);

            return talent;
        }
    }

    public class Talent
    {
        [JsonPropertyName("talent_name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        // Physical Talents
        [JsonPropertyName("is_cat_like")]
        public bool IsCatLike { get; set; }
        [JsonPropertyName("is_fast")]
        public bool IsFast { get; set; }
        [JsonPropertyName("is_night_vision")]
        public bool IsNightVision { get; set; }
        [JsonPropertyName("is_perfect_hearing")]
        public bool IsPerfectHearing { get; set; }
        [JsonPropertyName("is_resilient")]
        public bool IsResilient { get; set; }
        [JsonPropertyName("is_resist_disease")]
        public bool IsResistDisease { get; set; }
        [JsonPropertyName("is_resist_poison")]
        public bool IsResistPoison { get; set; }
        [JsonPropertyName("is_strong")]
        public bool IsStrong { get; set; }
        [JsonPropertyName("is_strong_build")]
        public bool IsStrongBuild { get; set; }
        [JsonPropertyName("is_tank")]
        public bool IsTank { get; set; }

        // Combat Talents
        [JsonPropertyName("is_axeman")]
        public bool IsAxeman { get; set; }
        [JsonPropertyName("is_bruiser")]
        public bool IsBruiser { get; set; }
        [JsonPropertyName("is_death_lament")]
        public bool IsDeathLament { get; set; }
        [JsonPropertyName("is_disarm")]
        public bool IsDisarm { get; set; }
        [JsonPropertyName("is_dual_wield")]
        public bool IsDualWield { get; set; }
        [JsonPropertyName("is_fast_reload")]
        public bool IsFastReload { get; set; }
        [JsonPropertyName("is_marksman")]
        public bool IsMarksman { get; set; }
        [JsonPropertyName("is_mighty_blow")]
        public bool IsMightyBlow { get; set; }
        [JsonPropertyName("is_parry_master")]
        public bool IsParryMaster { get; set; }
        [JsonPropertyName("is_perfect_shot")]
        public bool IsPerfectShot { get; set; }
        [JsonPropertyName("is_riposte_master")]
        public bool IsRiposteMaster { get; set; }
        [JsonPropertyName("is_sniper")]
        public bool IsSniper { get; set; }
        [JsonPropertyName("is_swordsman")]
        public bool IsSwordsman { get; set; }
        [JsonPropertyName("is_tight_grip")]
        public bool IsTightGrip { get; set; }
        [JsonPropertyName("is_tunnel_fighter")]
        public bool IsTunnelFighter { get; set; }

        // Faith Talents
        [JsonPropertyName("is_devoted")]
        public bool IsDevoted { get; set; }
        [JsonPropertyName("is_gods_chosen")]
        public bool IsGodsChosen { get; set; }
        [JsonPropertyName("is_healer")]
        public bool IsHealer { get; set; }
        [JsonPropertyName("is_messiah")]
        public bool IsMessiah { get; set; }
        [JsonPropertyName("is_pure")]
        public bool IsPure { get; set; }
        [JsonPropertyName("is_reliquary")]
        public bool IsReliquary { get; set; }

        // Alchemist Talents
        [JsonPropertyName("is_gatherer")]
        public bool IsGatherer { get; set; }
        [JsonPropertyName("is_harvester")]
        public bool IsHarvester { get; set; }
        [JsonPropertyName("is_keen_eye")]
        public bool IsKeenEye { get; set; }
        [JsonPropertyName("is_master_healer")]
        public bool IsMasterHealer { get; set; }
        [JsonPropertyName("is_perfect_toss")]
        public bool IsPerfectToss { get; set; }
        [JsonPropertyName("is_poisoner")]
        public bool IsPoisoner { get; set; }
        [JsonPropertyName("is_powerful_potions")]
        public bool IsPowerfulPotions { get; set; }

        // Common Talents
        [JsonPropertyName("is_charming")]
        public bool IsCharming { get; set; }
        [JsonPropertyName("is_disciplined")]
        public bool IsDisciplined { get; set; }
        [JsonPropertyName("is_hunter")]
        public bool IsHunter { get; set; }
        [JsonPropertyName("is_lucky")]
        public bool IsLucky { get; set; }
        [JsonPropertyName("is_master_cook")]
        public bool IsMasterCook { get; set; }
        [JsonPropertyName("is_natural_leader")]
        public bool IsNaturalLeader { get; set; }
        [JsonPropertyName("is_ring_bearer")]
        public bool IsRingBearer { get; set; }
        [JsonPropertyName("is_survivalist")]
        public bool IsSurvivalist { get; set; }
        [JsonPropertyName("is_swift_leader")]
        public bool IsSwiftLeader { get; set; }
        [JsonPropertyName("is_veteran")]
        public bool IsVeteran { get; set; }

        // Magic Talents
        [JsonPropertyName("is_blood_magic")]
        public bool IsBloodMagic { get; set; }
        [JsonPropertyName("is_conjurer")]
        public bool IsConjurer { get; set; }
        [JsonPropertyName("is_divinator")]
        public bool IsDivinator { get; set; }
        [JsonPropertyName("is_fast_reflexes")]
        public bool IsFastReflexes { get; set; }
        [JsonPropertyName("is_focused")]
        public bool IsFocused { get; set; }
        [JsonPropertyName("is_restorer")]
        public bool IsRestorer { get; set; }
        [JsonPropertyName("is_mystic")]
        public bool IsMystic { get; set; }
        [JsonPropertyName("is_necromancer")]
        public bool IsNecromancer { get; set; }
        [JsonPropertyName("is_powerful_missiles")]
        public bool IsPowerfulMissiles { get; set; }
        [JsonPropertyName("is_summoner")]
        public bool IsSummoner { get; set; }
        [JsonPropertyName("is_sustainer")]
        public bool IsSustainer { get; set; }
        [JsonPropertyName("is_thrifty")]
        public bool IsThrifty { get; set; }

        // Sneaky Talents
        [JsonPropertyName("is_assassin")]
        public bool IsAssassin { get; set; }
        [JsonPropertyName("is_backstabber")]
        public bool IsBackstabber { get; set; }
        [JsonPropertyName("is_cutpurse")]
        public bool IsCutpurse { get; set; }
        [JsonPropertyName("is_evaluate")]
        public bool IsEvaluate { get; set; }
        [JsonPropertyName("is_lock_picker")]
        public bool IsLockPicker { get; set; }
        [JsonPropertyName("is_mechanical_genius")]
        public bool IsMechanicalGenius { get; set; }
        [JsonPropertyName("is_nimble")]
        public bool IsNimble { get; set; }
        [JsonPropertyName("is_quick_fingers")]
        public bool IsQuickFingers { get; set; }
        [JsonPropertyName("is_sharp_eyed")]
        public bool IsSharpEyed { get; set; }
        [JsonPropertyName("is_sense_for_gold")]
        public bool IsSenseForGold { get; set; }
        [JsonPropertyName("is_streetwise")]
        public bool IsStreetwise { get; set; }
        [JsonPropertyName("is_trap_finder")]
        public bool IsTrapFinder { get; set; }

        // Mental Talents
        [JsonPropertyName("is_braveheart")]
        public bool IsBraveheart { get; set; }
        [JsonPropertyName("is_confident")]
        public bool IsConfident { get; set; }
        [JsonPropertyName("is_fearless")]
        public bool IsFearless { get; set; }
        [JsonPropertyName("is_hate")]
        public bool IsHate { get; set; }
        [JsonPropertyName("is_strong_minded")]
        public bool IsStrongMinded { get; set; }
        [JsonPropertyName("is_wise")]
        public bool IsWise { get; set; }

        public Dictionary<string, int> GetInitialTalentBonus()
        {
            Dictionary<string, int> bonus = new Dictionary<string, int>();
            if (IsCatLike)
            {
                bonus.Add("DEX", 5);
            }
            if (IsResilient)
            {
                bonus.Add("CON", 5);
            }
            if (IsNightVision)
            {
                bonus.Add("PS", 10);
            }
            if (IsStrong)
            {
                bonus.Add("STR", 5);
            }
            if (IsStrongBuild)
            {
                bonus.Add("HP", 2);
            }
            if (IsMarksman)
            {
                bonus.Add("RS", 5);
            }
            if (IsSniper)
            {
                bonus.Add("RS", 10);
            }
            if (IsGodsChosen || IsLucky)
            {
                bonus.Add("Luck", 1);
            }
            if (IsHunter)
            {
                bonus.Add("FS", 10);
            }
            if (IsLockPicker)
            {
                bonus.Add("PLS", 5);
            }
            if (IsConfident)
            {
                bonus.Add("RES", 5);
            }
            if (IsStrongMinded)
            {
                bonus.Add("Sanity", 1);
            }
            if (IsWise)
            {
                bonus.Add("WIS", 5);
            }

            return bonus;
        }

    }
}