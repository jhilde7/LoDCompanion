using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Threading.Tasks;

namespace LoDCompanion.BackEnd.Services.Player
{
    public enum SettlementName
    {
        Birnheim,
        Caelkirk,
        Coalfell,
        Durburim,
        Freyfell,
        Irondale,
        Rochdale,
        SilverCity,
        Whiteport,
        TheOutpost,
        Windfair
    }

    public enum SettlementServiceName
    {
        AlbertasMagnificentAnimals,
        Arena,
        Banks,
        Kennel,
        Blacksmith,
        GeneralStore,
        MagicBrewery,
        MervinsMagicalOddities,
        Herbalist,
        SickWard,
        Inn,
        Temple,
        FortuneTeller,
        HorseTrack,
        Scryer,
        TheDarkGuild,
        FightersGuild,
        WizardsGuild,
        AlchemistGuild,
        RangersGuild,
        InnerSanctum,
        TheAsylum,
    }

    public enum QuestColor
    {
        Red,
        Green,
        Pink,
        Blue,
        Purple,
        White,
        Yellow,
        Black,
        Turquoise
    }

    public class ServiceLocation
    {
        public SettlementServiceName Name { get; set; }
        public List<SettlementActionType> AvailableActions { get; set; } = new List<SettlementActionType>();
        public int AvailabilityModifier { get; set; }
        public float PriceModifier { get; set; }
        public string? SpecialRules { get; set; }
    }

    public class Settlement
    {
        public List<HexTile> HexTiles { get; set; } = new List<HexTile>();
        public SettlementName Name { get; set; }
        public List<ServiceLocation> AvailableServices { get; set; } = new List<ServiceLocation>();
        public int EventOn { get; set; }
        public string QuestDice { get; set; } = string.Empty;
        public QuestColor QuestColor { get; set; }
        public int RejectedQuests { get; set; }
        public int InnPrice { get; set; }
    }

    public class SettlementService
    {
        private readonly UserRequestService _userRequest;
        private readonly QuestService _quest;

        public List<Settlement> Settlements => GetSettlements();

        public SettlementService(UserRequestService userRequestService, QuestService questService)
        {
            _userRequest = userRequestService;
            _quest = questService;
        }

        public async Task<HexTile?> GetRandomQuestLocation(Settlement settlement)
        {
            var rollResult = await _userRequest.RequestRollAsync("Roll for random quest", settlement.QuestDice);
            await Task.Yield();
            return _quest.GetQuestHexLocationByColorNumber(rollResult.Roll, settlement.QuestColor);
        }

        public List<Settlement> GetSettlements()
        {
            return new List<Settlement>
            {
                new Settlement
                {
                    Name = SettlementName.Birnheim,
                    EventOn = 12,
                    InnPrice = 65,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, AvailabilityModifier = -1, PriceModifier = 1.2f, SpecialRules = "Armours purchased in Birnheim are Dwarven-made and gain an additional +2 max durability. This is cumulative with any other modifiers." },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = -2, PriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(14, -23, 9)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(13, -22, 9)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(14, -22, 8)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Caelkirk,
                    EventOn = 11,
                    QuestDice = "1d4",
                    QuestColor = QuestColor.Red,
                    InnPrice = 35,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, AvailabilityModifier = -2, PriceModifier = 1.1f },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = -2, PriceModifier = 1.1f, SpecialRules = "Fishing gear 50 c, availability 6" },
                        new ServiceLocation { Name = SettlementServiceName.Inn }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(0, -28, 28)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -29, 29)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -29, 28)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Coalfell,
                    EventOn = 12,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Green,
                    InnPrice = 35,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, AvailabilityModifier = 1, PriceModifier = 0.9f, SpecialRules = "Weapons and weapons availability +1, Price -10%" },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = -2, PriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(0, 15, -15)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, 14, -14)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, 14, -15)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Durburim,
                    EventOn = 12,
                    InnPrice = 65,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, AvailabilityModifier = 1, PriceModifier = 1.2f, SpecialRules = "Weapons purchased in Durburim are Dwarven-made and gain an additional +2 Durability. This is cumulative with any other modifiers." },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = -1, PriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(20, -10, -10)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(20, -11, -9)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(21, -11, -10)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Freyfell,
                    EventOn = 11,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Pink,
                    InnPrice = 25,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Arena },
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = 1, PriceModifier = 0.9f },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.SickWard }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(0, -15, 15)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -15, 14)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -16, 15)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -16, 16)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, -14, 15)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -14, 14)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Irondale,
                    EventOn = 12,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Blue,
                    InnPrice = 15,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(0, -13, 13)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -13, 12)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -14, 13)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -14, 14)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, -12, 13)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -12, 12)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Rochdale,
                    EventOn = 12,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Purple,
                    InnPrice = 20,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore },
                        new ServiceLocation { Name = SettlementServiceName.Herbalist },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.SickWard },
                        new ServiceLocation { Name = SettlementServiceName.MagicBrewery }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(-6, 7, -1)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-7, 8, -1)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-6, 8, -2)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.SilverCity,
                    EventOn = 8,
                    QuestDice = "2d20",
                    QuestColor = QuestColor.White,
                    InnPrice = 25,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Arena },
                        new ServiceLocation { Name = SettlementServiceName.Banks },
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith },
                        new ServiceLocation { Name = SettlementServiceName.FortuneTeller },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore },
                        new ServiceLocation { Name = SettlementServiceName.TheDarkGuild },
                        new ServiceLocation { Name = SettlementServiceName.FightersGuild },
                        new ServiceLocation { Name = SettlementServiceName.AlchemistGuild },
                        new ServiceLocation { Name = SettlementServiceName.WizardsGuild },
                        new ServiceLocation { Name = SettlementServiceName.RangersGuild },
                        new ServiceLocation { Name = SettlementServiceName.InnerSanctum },
                        new ServiceLocation { Name = SettlementServiceName.HorseTrack },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(0, 0, 0)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, 0, -1)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, 1, -1)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, 1, 0)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, 0, 1)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -1, 1)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -1, 0)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Whiteport,
                    EventOn = 10,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Black,
                    InnPrice = 15,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.AlbertasMagnificentAnimals },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = 1, PriceModifier = 0.9f, SpecialRules = "Fishing gear 50 c, availability 6" },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(-16, 0, 16)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-16, -1, 17)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-17, 1, 16)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.TheOutpost,
                    EventOn = 9,
                    QuestDice = "1d12",
                    QuestColor = QuestColor.Yellow,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, PriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = -2, PriceModifier = 1.2f, SpecialRules = "Toll. If you are to head out to a Quest Site in the Ancient Lands (yellow 1-12), you will have to pay a toll to the King of 100 c per hero first. You must be a member of the league to be allowed to pass." }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(-1, 21, -21)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, 20, -20)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, 20, -21)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-2, 22, -21)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, 22, -22)) { Terrain = TerrainType.Town }
                    }
                },
                new Settlement
                {
                    Name = SettlementName.Windfair,
                    EventOn = 12,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Blue,
                    InnPrice = 35,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, AvailabilityModifier = -1, PriceModifier = 1.1f, SpecialRules = "Fishing gear 50 c, availability 6" },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Scryer },
                        new ServiceLocation { Name = SettlementServiceName.Temple }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(12, -8, -4)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(12, -9, -3)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(13, -9, -4)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(12, -7, -5)) { Terrain = TerrainType.Town }
                    }
                }
            };
        }

        public Settlement GetSettlementByName(SettlementName name)
        {
            return Settlements.First(s => s.Name == name);
        }

        public List<ServiceLocation> GetSettlementServices()
        {
            return new List<ServiceLocation>
            {
                new ServiceLocation
                {
                    Name = SettlementServiceName.Arena,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.ArenaFighting,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.Banks,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.Banking,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.Kennel,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyDog,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.AlbertasMagnificentAnimals,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyFamiliar,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.Blacksmith,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellArmour,
                        SettlementActionType.BuySellWeapons,
                        SettlementActionType.RepairEquipment
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.GeneralStore,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellEquipment,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.MagicBrewery,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellEquipment,
                        SettlementActionType.IdentifyPotion
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.MervinsMagicalOddities,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellEquipment,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.Herbalist,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyIngredients,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.SickWard,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.CureDisease,
                        SettlementActionType.CurePoison
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.Inn,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.Gamble,
                        SettlementActionType.RestRecuperation,
                        SettlementActionType.TendThoseMemories,
                        SettlementActionType.EnchantObjects,
                        SettlementActionType.CreateScroll
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.Temple,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.Pray,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.FortuneTeller,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.ReadFortune,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.HorseTrack,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.HorseRacing,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.Scryer,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.IdentifyMagicItem,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.TheDarkGuild,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.FightersGuild,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.WizardsGuild,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                        SettlementActionType.ChargeMagicItem,
                        SettlementActionType.IdentifyMagicItem,
                        SettlementActionType.LearnSpell
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.AlchemistGuild,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                        SettlementActionType.BuyIngredients,
                        SettlementActionType.IdentifyPotion
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.RangersGuild,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.InnerSanctum,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.LearnPrayer,
                    }
                },
                new ServiceLocation
                {
                    Name = SettlementServiceName.TheAsylum,
                    AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.TreatMentalConditions,
                    }
                },
            };
        }
    }
}
