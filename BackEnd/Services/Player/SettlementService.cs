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

    public enum SettlementServices
    {
        Arena,
        Banks,
        Kennel,
        MagnificentAnimals,
        Blacksmith,
        GeneralStore,
        MagicBrewery,
        MervinsMagicalOddities,
        Herbalist,
        WizardsGuild,
        SickWards,
        Inn,
        Temple,
        FortuneTeller,
        HorseRacing,
        Scryer,
        InnerSanctum,
        Guilds,
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

    public class Settlement
    {
        public List<HexTile> HexTiles { get; set; } = new List<HexTile>();
        public SettlementName Name { get; set; }
        public List<SettlementServices> AvailableServices { get; set; } = new List<SettlementServices>();
        public int EventOn { get; set; }
        public string QuestDice { get; set; } = string.Empty;
        public QuestColor QuestColor { get; set; }
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Blacksmith, SettlementServices.GeneralStore, SettlementServices.Inn, SettlementServices.Temple },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Blacksmith, SettlementServices.GeneralStore, SettlementServices.Inn },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Blacksmith, SettlementServices.GeneralStore, SettlementServices.Inn, SettlementServices.Temple },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Blacksmith, SettlementServices.GeneralStore, SettlementServices.Inn, SettlementServices.Temple },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Arena, SettlementServices.Blacksmith, SettlementServices.GeneralStore, SettlementServices.Inn, SettlementServices.SickWards },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Blacksmith, SettlementServices.GeneralStore, SettlementServices.Inn, SettlementServices.Temple },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.GeneralStore, SettlementServices.Herbalist, SettlementServices.Inn, SettlementServices.SickWards, SettlementServices.MagicBrewery },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Arena, SettlementServices.Banks, SettlementServices.Blacksmith, SettlementServices.FortuneTeller, SettlementServices.GeneralStore, SettlementServices.Guilds, SettlementServices.HorseRacing, SettlementServices.Inn, SettlementServices.InnerSanctum, SettlementServices.Temple },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.MagnificentAnimals, SettlementServices.GeneralStore, SettlementServices.Inn, SettlementServices.Temple },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Blacksmith, SettlementServices.GeneralStore },
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
                    AvailableServices = new List<SettlementServices> { SettlementServices.Blacksmith, SettlementServices.GeneralStore, SettlementServices.Inn, SettlementServices.Scryer, SettlementServices.Temple },
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
    }
}
