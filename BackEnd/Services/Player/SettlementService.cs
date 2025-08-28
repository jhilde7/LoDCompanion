using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Models;
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

    public enum GodName
    {
        Ohlnir,
        Rhidnir,
        Iphy,
        Metheia,
        Charus,
        Ramos
    }

    public class ServiceLocation
    {
        public SettlementServiceName Name { get; set; }
        public List<SettlementActionType> AvailableActions { get; set; } = new List<SettlementActionType>();
        public int WeaponAvailabilityModifier { get; set; }
        public int ArmourAvailabilityModifier { get; set; }
        public int EquipmentAvailabilityModifier { get; set; }
        public float WeaponPriceModifier { get; set; }
        public float ArmourPriceModifier { get; set; }
        public float EquipmentPriceModifier { get; set; }
        public string? SpecialRules { get; set; }
        public GodName TempleDevotion { get; set; }
        public int WeaponMaxDurabilityModifier { get; internal set; }
        public int ArmourMaxDurabilityModifier { get; internal set; }
    }

    public class Settlement
    {
        public SettlementState State { get; set; } = new SettlementState();
        public List<HexTile> HexTiles { get; set; } = new List<HexTile>();
        public SettlementName Name { get; set; }
        public List<ServiceLocation> AvailableServices { get; set; } = new List<ServiceLocation>();
        public int EventOn { get; set; }
        public string QuestDice { get; set; } = string.Empty;
        public QuestColor QuestColor { get; set; }
        public int RejectedQuests { get; set; }
        public int InnPrice { get; set; }
        public string? SpecialRules { get; internal set; }
    }

    public class SettlementState
    {
        public int CurrentDay { get; set; } = 1;
        public Dictionary<Hero, int> HeroActionPoints { get; set; } = new Dictionary<Hero, int>();
        public Dictionary<Hero, (SettlementActionType Action, int DaysRemaining)> BusyHeroes { get; set; } = new Dictionary<Hero, (SettlementActionType, int)>();
        public List<ActiveStatusEffect> ActiveStatusEffects { get; set; } = new List<ActiveStatusEffect>();
        public List<Bank>? Banks { get; set; }
    }

    public class SettlementService
    {
        private readonly UserRequestService _userRequest;
        private readonly QuestService _quest;
        private readonly PartyManagerService _partyManager;

        public List<Settlement> Settlements => GetSettlements();

        public SettlementService(
            UserRequestService userRequestService, 
            QuestService questService,
            PartyManagerService partyManager)
        {
            _userRequest = userRequestService;
            _quest = questService;
            _partyManager = partyManager;
        }

        public void StartNewDay(Settlement settlement)
        {
            // Remove status effects that expire at the end of the day.
            settlement.State.ActiveStatusEffects.RemoveAll(e => e.RemoveEndDay);

            // Update busy heroes and identify those who are now free.
            var heroesFinished = new List<Hero>();
            foreach (var hero in settlement.State.BusyHeroes.Keys)
            {
                var busyInfo = settlement.State.BusyHeroes[hero];
                busyInfo.DaysRemaining--;
                settlement.State.BusyHeroes[hero] = busyInfo;

                if (busyInfo.DaysRemaining <= 0)
                {
                    heroesFinished.Add(hero);
                }
            }

            settlement.State.CurrentDay++;
            settlement.State.HeroActionPoints.Clear();
            if (_partyManager.Party != null)
            {
                foreach (var hero in _partyManager.Party.Heroes)
                {
                    settlement.State.HeroActionPoints[hero] = 1;
                }
            }

            foreach (var hero in heroesFinished)
            {
                settlement.State.HeroActionPoints[hero] = 0;
                settlement.State.BusyHeroes.Remove(hero);
            }
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
                    Name = SettlementName.Caelkirk,
                    EventOn = 11,
                    QuestDice = "1d4",
                    QuestColor = QuestColor.Red,
                    InnPrice = 35,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, WeaponAvailabilityModifier = -2, WeaponPriceModifier = 1.1f, ArmourAvailabilityModifier = -2, ArmourPriceModifier = 1.1f },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = -2, EquipmentPriceModifier = 1.1f, SpecialRules = "Fishing gear 50 c, availability 6" },
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
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, WeaponAvailabilityModifier = 1, WeaponPriceModifier = 0.9f, ArmourAvailabilityModifier = 1, ArmourPriceModifier = 0.9f },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = -2, EquipmentPriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ohlnir }
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
                    Name = SettlementName.Freyfell,
                    EventOn = 11,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Pink,
                    InnPrice = 25,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Arena },
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = 1, EquipmentPriceModifier = 0.9f },
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
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Rhidnir },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Iphy },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Metheia }
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
                    Name = SettlementName.Whiteport,
                    EventOn = 10,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Black,
                    InnPrice = 15,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.AlbertasMagnificentAnimals },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = 1, EquipmentPriceModifier = 0.9f, SpecialRules = "Fishing gear 50 c, availability 6" },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Rhidnir },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Iphy },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ohlnir },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Metheia }
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
                    Name = SettlementName.Windfair,
                    EventOn = 12,
                    QuestDice = "1d6",
                    QuestColor = QuestColor.Blue,
                    InnPrice = 35,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = -1, EquipmentPriceModifier = 1.1f, SpecialRules = "Fishing gear 50 c, availability 6" },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Scryer },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ohlnir },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Charus }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(12, -8, -4)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(12, -9, -3)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(13, -9, -4)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(12, -7, -5)) { Terrain = TerrainType.Town }
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
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Charus },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Iphy },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Metheia },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ohlnir },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ramos },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Rhidnir }
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
                    },
                    State = new SettlementState()
                    {
                        Banks = new List<Bank>
                        {
                            new Bank()
                            {
                                Name = Bank.BankName.ChamberlingsReserve,
                                Description = "This is the go-to bank for the noble people in the city. They have good security, but rather aggressive investment plans for your deposits. After all, most people who utilise this bank can afford to lose some money now and then. It is acceptable for the long-term win."
                            },
                            new Bank()
                            {
                                Name = Bank.BankName.SmartfallBank,
                                Description = "Most commoners in the city use this bank, and indeed throughout the kingdom, as it exists in most of the larger cities. Security is somewhat lax; however, they make careful investments. Typically, deposits grow slowly but surely."
                            },
                            new Bank()
                            {
                                Name = Bank.BankName.TheVault,
                                Description = "This bank invests your money in more dubious areas, with a high risk but also with a good profit, should the investments succeed. The security is moderately good."
                            }
                        }
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
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, WeaponPriceModifier = 1.2f, ArmourPriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = -2, EquipmentPriceModifier = 1.2f }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(-1, 21, -21)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, 20, -20)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, 20, -21)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-2, 22, -21)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, 22, -22)) { Terrain = TerrainType.Town }
                    }, 
                    SpecialRules = "Toll. If you are to head out to a Quest Site in the Ancient Lands (yellow 1-12), you will have to pay a toll to the King of 100 c per hero first. You must be a member of the league to be allowed to pass."
                },
                new Settlement
                {
                    Name = SettlementName.Durburim,
                    EventOn = 12,
                    InnPrice = 65,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, WeaponAvailabilityModifier = 1, WeaponPriceModifier = 1.2f, ArmourPriceModifier = 1.1f, WeaponMaxDurabilityModifier = 2 },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = -1, EquipmentPriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Charus },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Iphy },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Metheia },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ohlnir },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ramos },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Rhidnir }
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
                    Name = SettlementName.Birnheim,
                    EventOn = 12,
                    InnPrice = 65,
                    AvailableServices = new List<ServiceLocation>
                    {
                        new ServiceLocation { Name = SettlementServiceName.Blacksmith, WeaponAvailabilityModifier = -1, WeaponPriceModifier = 1.2f, ArmourAvailabilityModifier = 1, ArmourPriceModifier = 1.2f, ArmourMaxDurabilityModifier = 2 },
                        new ServiceLocation { Name = SettlementServiceName.GeneralStore, EquipmentAvailabilityModifier = -2, EquipmentPriceModifier = 1.2f },
                        new ServiceLocation { Name = SettlementServiceName.Inn },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Charus },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Iphy },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Metheia },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ohlnir },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Ramos },
                        new ServiceLocation { Name = SettlementServiceName.Temple, TempleDevotion = GodName.Rhidnir }
                    },
                    HexTiles = new List<HexTile>
                    {
                        new HexTile(new Hex(14, -23, 9)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(13, -22, 9)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(14, -22, 8)) { Terrain = TerrainType.Town }
                    }
                }
            };
        }

        public Settlement GetSettlementByName(SettlementName name)
        {
            return Settlements.First(s => s.Name == name);
        }

        public List<SettlementActionType> GetSettlementActionByServiceLocation(SettlementServiceName name)
        {
            return name switch
            {
                SettlementServiceName.Arena => new List<SettlementActionType>
                    {
                        SettlementActionType.ArenaFighting,
                    },
                SettlementServiceName.Banks => new List<SettlementActionType>
                    {
                        SettlementActionType.Banking,
                    },
                SettlementServiceName.Kennel => new List<SettlementActionType>
                    {
                        SettlementActionType.BuyDog,
                    },
                SettlementServiceName.AlbertasMagnificentAnimals => new List<SettlementActionType>
                    {
                        SettlementActionType.BuyFamiliar,
                    },
                SettlementServiceName.Blacksmith => new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellArmour,
                        SettlementActionType.BuySellWeapons,
                        SettlementActionType.RepairEquipment
                    },
                SettlementServiceName.GeneralStore => new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellEquipment,
                    },
                SettlementServiceName.MagicBrewery => new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellEquipment,
                        SettlementActionType.IdentifyPotion
                    },
                SettlementServiceName.MervinsMagicalOddities => new List<SettlementActionType>
                    {
                        SettlementActionType.BuySellEquipment,
                    },
                SettlementServiceName.Herbalist => new List<SettlementActionType>
                    {
                        SettlementActionType.BuyIngredients,
                    },
                SettlementServiceName.SickWard => new List<SettlementActionType>
                    {
                        SettlementActionType.CureDisease,
                        SettlementActionType.CurePoison
                    },
                SettlementServiceName.Inn => new List<SettlementActionType>
                    {
                        SettlementActionType.Gamble,
                        SettlementActionType.RestRecuperation,
                        SettlementActionType.TendThoseMemories,
                        SettlementActionType.EnchantObjects,
                        SettlementActionType.CreateScroll
                    },
                SettlementServiceName.Temple => new List<SettlementActionType>
                    {
                        SettlementActionType.Pray,
                    },
                SettlementServiceName.FortuneTeller => new List<SettlementActionType>
                    {
                        SettlementActionType.ReadFortune,
                    },
                SettlementServiceName.HorseTrack => new List<SettlementActionType>
                    {
                        SettlementActionType.HorseRacing,
                    },
                SettlementServiceName.Scryer => new List<SettlementActionType>
                    {
                        SettlementActionType.IdentifyMagicItem,
                    },
                SettlementServiceName.TheDarkGuild => new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                    },
                SettlementServiceName.FightersGuild => new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                    },
                SettlementServiceName.WizardsGuild => new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                        SettlementActionType.ChargeMagicItem,
                        SettlementActionType.IdentifyMagicItem,
                        SettlementActionType.LearnSpell
                    },
                SettlementServiceName.AlchemistGuild => new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                        SettlementActionType.BuyIngredients,
                        SettlementActionType.IdentifyPotion
                    },
                SettlementServiceName.RangersGuild => new List<SettlementActionType>
                    {
                        SettlementActionType.GuildBusiness,
                    },
                SettlementServiceName.InnerSanctum => new List<SettlementActionType>
                    {
                        SettlementActionType.LearnPrayer,
                    },
                SettlementServiceName.TheAsylum => new List<SettlementActionType>
                    {
                        SettlementActionType.TreatMentalConditions,
                    },
                _ => new List<SettlementActionType>
                    {
                        SettlementActionType.LevelUp,
                    }
            };
        }
    }
}
