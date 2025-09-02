using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using RogueSharp;
using System.Threading.Tasks;
using System.Xml.Linq;
using static LoDCompanion.BackEnd.Services.Player.ServiceLocation;

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
        TheInnerSanctum,
        TheAsylum,
        Estate,
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
        public Settlement Settlement { get; set; }
        public SettlementServiceName Name { get; set; }
        public List<SettlementActionType> AvailableActions { get; set; } = new List<SettlementActionType>();
        public string? SpecialRules { get; set; }

        private bool _newVisit;
        public bool NewVisit
        {
            get => _newVisit;
            set
            {
                if (_newVisit)
                {
                    RefreshStockForNewVisit();
                    _newVisit = false;
                }
            }
        }
        private List<Equipment>? _currentAvailableStock;
        public List<Equipment> CurrentAvailableStock
        {
            get
            {
                // If the stock hasn't been generated for this visit yet, generate it.
                if (_currentAvailableStock == null)
                {
                    _currentAvailableStock = GetStock();
                }
                return _currentAvailableStock;
            }
        }

        public ServiceLocation(SettlementServiceName name, Settlement settlement)
        {
            Name = name;
            Settlement = settlement;
        }

        // Call this method when the party first enters the settlement
        public virtual void RefreshStockForNewVisit()
        {
            // Reset the stock, so it will be regenerated on the next access.
            _currentAvailableStock = null;
        }

        private List<Equipment> GetStock()
        {
            var availabilityModifier = 0;
            var priceModifier = 1d;

            var freshStocks = Settlement.State.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.FreshStocks);
            var shortageOfGoods = Settlement.State.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.ShortageOfGoods);
            var sale = Settlement.State.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.Sale);
            if (freshStocks != null) availabilityModifier += 2;
            if (shortageOfGoods != null) availabilityModifier -= 2;

            var list = EquipmentService.GetShopInventoryByServiceLocation(this, availabilityModifier);

            if (this is BlackSmith blackSmith)
            {
                var weaponPriceMod = blackSmith.WeaponPriceModifier;
                var armourPriceMod = blackSmith.ArmourPriceModifier;
                if (sale != null)
                {
                    weaponPriceMod -= 0.2;
                    armourPriceMod -= 0.2;
                }
                if (shortageOfGoods != null)
                {
                    weaponPriceMod += 0.1;
                    armourPriceMod += 0.1;
                }
                // apply weapon price mod
                list
                    .Where(item => item is Weapon || item is Ammo).ToList()
                    .ForEach(item => item.Value = (int)Math.Floor(item.Value * weaponPriceMod));
                // apply weapon max durability mod
                list
                    .Where(item => item is Weapon || item is Ammo).ToList()
                    .ForEach(item => item.MaxDurability += blackSmith.WeaponMaxDurabilityModifier);
                // apply armour price mod
                list
                    .Where(item => !(item is Weapon || item is Ammo)).ToList()
                    .ForEach(item => item.Value = (int)Math.Floor(item.Value * armourPriceMod));
                // apply armour max durability mod
                list
                    .Where(item => !(item is Weapon || item is Ammo)).ToList()
                    .ForEach(item => item.MaxDurability += blackSmith.ArmourMaxDurabilityModifier);
                // update durability to max
                list.ForEach(item => item.Durability = item.MaxDurability);

                if (this is BlackSmith specials && specials.ShopSpecials != null)
                {
                    foreach (var special in specials.ShopSpecials)
                    {
                        var item = list.FirstOrDefault(i => i.Name == special.ItemName);
                        if (item != null)
                        {
                            item.Value = special.Price.HasValue ? special.Price.Value : item.Value;
                            item.Availability = special.Availability.HasValue ? special.Availability.Value : item.Availability;
                        }
                    }
                }
            }
            else
            {
                if (this is GeneralStore generalStore) priceModifier = generalStore.EquipmentPriceModifier;
                if (sale != null) priceModifier -= 0.2;
                if (shortageOfGoods != null) priceModifier += 0.1;
                list.ForEach(item => item.Value = (int)Math.Floor(item.Value * priceModifier));

                if (this is GeneralStore specials && specials.ShopSpecials != null)
                {
                    foreach (var special in specials.ShopSpecials)
                    {
                        var item = list.FirstOrDefault(i => i.Name == special.ItemName);
                        if (item != null)
                        {
                            item.Value = special.Price.HasValue ? special.Price.Value : item.Value;
                            item.Availability = special.Availability.HasValue ? special.Availability.Value : item.Availability;
                        }
                    }
                }
            }

            return list;
        }

    }

    public class Settlement
    {
        public SettlementState State { get; set; }
        public List<HexTile> HexTiles { get; set; } = new List<HexTile>();
        public SettlementName Name { get; set; }
        public int EventOn { get; set; }
        public string? QuestDice { get; set; }
        public QuestColor? QuestColor { get; set; }
        public int RejectedQuests { get; set; }
        public string? SpecialRules { get; internal set; }

        public Settlement(SettlementName name, int eventOn, List<HexTile> hexTiles, string? questDice = null, QuestColor? questColor = null, string? specialRules = null)
        {
            Name = name;
            EventOn = eventOn;
            HexTiles = hexTiles;
            State = new SettlementState(this);

            QuestDice = questDice;
            QuestColor = questColor;
            SpecialRules = specialRules;
        }
    }

    public class SettlementState
    {
        public Settlement Settlement { get; set; }
        public int CurrentDay { get; set; } = 1;
        public Dictionary<Hero, int> HeroActionPoints { get; set; } = new Dictionary<Hero, int>();
        public Dictionary<Hero, (SettlementActionType Action, int DaysRemaining)> BusyHeroes { get; set; } = new Dictionary<Hero, (SettlementActionType, int)>();
        public List<ActiveStatusEffect> ActiveStatusEffects { get; set; } = new List<ActiveStatusEffect>();
        public Inn? Inn { get; set; }
        public GeneralStore? GeneralStore { get; set; }
        public BlackSmith? BlackSmith { get; set; }
        public Herbalist? Herbalist { get; set; }
        public MagicBrewery? MagicBrewery { get; set; }
        public SickWard? SickWard { get; set; }
        public FortuneTeller? FortuneTeller { get; set; }
        public HorseTrack? HorseTrack { get; set; }
        public Scryer? Scryer { get; set; }
        public TheAsylum? TheAsylum { get; set; }
        public Arena? Arena { get; set; }
        public List<Bank>? Banks { get; set; }
        public List<Temple>? Temples { get; set; }
        public TheDarkGuild? TheDarkGuild { get; set; }
        public FightersGuild? FightersGuild { get; set; }
        public WizardsGuild? WizardsGuild { get; set; }
        public AlchemistsGuild? AlchemistsGuild { get; set; }
        public RangersGuild? RangersGuild { get; set; }
        public TheInnerSanctum? TheInnerSanctum { get; set; }
        public Estate? Estate { get; set; }

        public SettlementState (Settlement settlement)
        {
            Settlement = settlement;
        }
    }

    public class Inn : ServiceLocation
    {
        public int Price { get; set; }
        public int SleepInStablesPrice => (int)Math.Floor(Price / 2d);

        public Inn(Settlement settlement) : base(SettlementServiceName.Inn, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.Gambling,
                        SettlementActionType.RestRecuperation,
                        SettlementActionType.TendThoseMemories,
                        SettlementActionType.EnchantObjects,
                        SettlementActionType.CreateScroll
                    };
        }
    }

    public class ShopSpecial
    {
        public string ItemName { get; set; } = string.Empty;
        public int? Price { get; set; }
        public int? Availability { get; set; }

        public ShopSpecial() { }
    }

    public class BlackSmith : ServiceLocation
    {
        public int WeaponAvailabilityModifier { get; set; }
        public int ArmourAvailabilityModifier { get; set; }
        public double WeaponPriceModifier { get; set; } = 1d;
        public double ArmourPriceModifier { get; set; } = 1d;
        public int WeaponMaxDurabilityModifier { get; internal set; }
        public int ArmourMaxDurabilityModifier { get; internal set; }
        public List<ShopSpecial>? ShopSpecials { get; set; }

        public BlackSmith(Settlement settlement) : base(SettlementServiceName.Blacksmith, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling
                    };
        }
    }

    public class GeneralStore : ServiceLocation
    {
        public int EquipmentAvailabilityModifier { get; set; }
        public double EquipmentPriceModifier { get; set; } = 1d;
        public List<ShopSpecial>? ShopSpecials { get; set; }

        public GeneralStore(Settlement settlement) : base(SettlementServiceName.GeneralStore, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.IdentifyPotion
                    };
        }
    }

    public class Herbalist : ServiceLocation
    {
        public Herbalist(Settlement settlement) : base(SettlementServiceName.Herbalist, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling
                    };
        }
    }

    public class MagicBrewery : ServiceLocation
    {
        public MagicBrewery(Settlement settlement) : base(SettlementServiceName.MagicBrewery, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.IdentifyPotion
                    };
        }
    }

    public class SickWard : ServiceLocation
    {
        public SickWard(Settlement settlement) : base(SettlementServiceName.SickWard, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.CureDiseasePoison,
                    };
        }
    }

    public class HorseTrack : ServiceLocation
    {
        public int MinBet { get; set; } = 50;
        public int MaxBet { get; set; } = 300;
        public int Bet { get; set; }
        public HorseTrack(Settlement settlement) : base(SettlementServiceName.HorseTrack, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.HorseRacing,
                    };
        }

        public async Task<SettlementActionResult> HorseRacing(Hero hero, SettlementActionResult result, UserRequestService userRequest, TreasureService treasure)
        {
            if (result.AvailableCoins < MinBet)
            {
                result.Message = $"{hero.Name} does not have enough coin to gamble with.";
                result.WasSuccessful = false;
                return result;
            }

            var horse = hero.Inventory.Mount;
            if (horse == null || !horse.Properties.ContainsKey(EquipmentProperty.Horse))
            {
                result.Message = $"{hero.Name} does not have a mount to race with.";
                result.WasSuccessful = false;
                return result;
            }

            var inputResult = await userRequest.RequestNumberInputAsync("How much do you want to Bet?", min: MinBet, max: Math.Min(MaxBet, result.AvailableCoins), canCancel: true);
            await Task.Yield();
            if (!inputResult.WasCancelled)
            {
                Bet = inputResult.Amount;
                result.AvailableCoins -= Bet;

                var targetStat = hero.GetStat(BasicStat.Dexterity);
                var rollResult = await userRequest.RequestRollAsync("Roll dexterity test.", "1d100");
                await Task.Yield();
                if (rollResult.Roll <= (int)Math.Floor(targetStat / 2d))
                {
                    result.Message = $"{hero.Name} gets 1st place!";
                    switch (hero.Level)
                    {
                        case 1: Bet = (int)Math.Floor(Bet * 3.0d); break;
                        case 2: Bet = (int)Math.Floor(Bet * 2.9d); break;
                        case 3: Bet = (int)Math.Floor(Bet * 2.8d); break;
                        case 4: Bet = (int)Math.Floor(Bet * 2.7d); break;
                        case 5: Bet = (int)Math.Floor(Bet * 2.6d); break;
                        case 6: Bet = (int)Math.Floor(Bet * 2.5d); break;
                        case 7: Bet = (int)Math.Floor(Bet * 2.4d); break;
                        case 8: Bet = (int)Math.Floor(Bet * 2.3d); break;
                        case 9: Bet = (int)Math.Floor(Bet * 2.2d); break;
                        default: Bet = (int)Math.Floor(Bet * 2.1d); break;
                    }
                    result.Message += $" Winning {Bet}";
                    result.AvailableCoins += Bet;
                    rollResult = await userRequest.RequestRollAsync("Roll for a chance for an extra prize.", "1d10");
                    await Task.Yield();
                    switch (rollResult.Roll)
                    {
                        case <= 2:
                            result.FoundItems = [.. await treasure.GetWonderfulTreasureAsync()];
                            if (result.FoundItems != null)
                            {
                                var extraAwardNames = string.Join(", ", result.FoundItems.Select(award => award.Name));
                                result.Message += $"\nThe hero also received an extra award of {extraAwardNames}";
                            }
                            break;
                        case <= 4:
                            result.FoundItems = [.. await treasure.GetFineTreasureAsync()];
                            if (result.FoundItems != null)
                            {
                                var extraAwardNames = string.Join(", ", result.FoundItems.Select(award => award.Name));
                                result.Message += $"\nThe hero also received an extra award of {extraAwardNames}";
                            }
                            break;
                        default:
                            result.Message += "\nNo extra prize awarded.";
                            break;
                    }
                }
                else if (rollResult.Roll <= targetStat - 10)
                {
                    result.Message = $"{hero.Name} gets 2nd place!";
                    switch (hero.Level)
                    {
                        case 1: Bet = (int)Math.Floor(Bet * 2.5d); break;
                        case 2: Bet = (int)Math.Floor(Bet * 2.4d); break;
                        case 3: Bet = (int)Math.Floor(Bet * 2.3d); break;
                        case 4: Bet = (int)Math.Floor(Bet * 2.2d); break;
                        case 5: Bet = (int)Math.Floor(Bet * 2.1d); break;
                        case 6: Bet = (int)Math.Floor(Bet * 2.0d); break;
                        case 7: Bet = (int)Math.Floor(Bet * 1.9d); break;
                        case 8: Bet = (int)Math.Floor(Bet * 1.8d); break;
                        case 9: Bet = (int)Math.Floor(Bet * 1.7d); break;
                        default: Bet = (int)Math.Floor(Bet * 1.6d); break;
                    }
                    result.Message += $" Winning {Bet}";
                    result.AvailableCoins += Bet;
                    rollResult = await userRequest.RequestRollAsync("Roll for a chance for an extra prize.", "1d10");
                    await Task.Yield();
                    switch (rollResult.Roll)
                    {
                        case <= 1:
                            result.FoundItems = [.. await treasure.GetWonderfulTreasureAsync()];
                            if (result.FoundItems != null)
                            {
                                var extraAwardNames = string.Join(", ", result.FoundItems.Select(award => award.Name));
                                result.Message += $"\nThe hero also received an extra award of {extraAwardNames}";
                            }
                            break;
                        case <= 3:
                            result.FoundItems = [.. await treasure.GetFineTreasureAsync()];
                            if (result.FoundItems != null)
                            {
                                var extraAwardNames = string.Join(", ", result.FoundItems.Select(award => award.Name));
                                result.Message += $"\nThe hero also received an extra award of {extraAwardNames}";
                            }
                            break;
                        default:
                            result.Message += "\nYou did not win an extra prize.";
                            break;
                    }
                }
                else if (rollResult.Roll >= 95)
                {
                    result.Message = $"Catastrophe strikes! {hero.Name} and his horse crash, {hero.Name} is minorly injured but their horse has a broken leg and must be put down.";
                    hero.Inventory.Mount = null;
                    hero.CurrentHP -= Math.Min(RandomHelper.RollDie(DiceType.D6), hero.CurrentHP);
                    hero.CurrentSanity -= Math.Min(1, hero.CurrentSanity);
                }
                else
                {
                    result.Message = $"{hero.Name} loses...";
                }
            }
            return result;
        }
    }

    public class FortuneTeller : ServiceLocation
    {
        public FortuneTeller(Settlement settlement) : base(SettlementServiceName.FortuneTeller, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.ReadFortune,
                    };
        }
    }

    public class Scryer : ServiceLocation
    {
        public Scryer(Settlement settlement) : base(SettlementServiceName.Scryer, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.IdentifyMagicItem,
                    };
        }
    }

    public class TheAsylum : ServiceLocation
    {
        public TheAsylum(Settlement settlement) : base(SettlementServiceName.TheAsylum, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.TreatMentalConditions,
                    };
        }
    }

    public class Arena : ServiceLocation
    {
        public enum ArenaBout
        {
            Group,
            SemiFinal,
            Final
        }

        public string Message { get; set; } = string.Empty;
        public int MinimumEntryFee { get; set; } = 50;
        public int MaxBet { get; set; } = 200;
        public int Bet { get; set; }
        public ArenaBout Bout { get; set; } = ArenaBout.Group;
        public int Winnings { get; set; }
        public int Experience { get; set; }
        public bool IsComplete { get; set; }
        public List<Equipment>? ExtraAward { get; set; }

        public event Func<TreasureType, int, Task<List<Equipment>>>? OnGetExtraAward;

        public Arena(Settlement settlement) : base(SettlementServiceName.Arena, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.ArenaFighting,
                    };
        }

        public async Task StartBoutAsync(int rollAttempt, Hero hero)
        {
            int modifier = GetArenaModifier(hero);
            var combatSkill = hero.GetSkill(Skill.CombatSkill);
            combatSkill += modifier;
            if (rollAttempt < combatSkill)
            {
                Winnings = ArenaWinnings(Bout, hero);
                Experience = ArenaExperience(Bout);
                switch (Bout)
                {
                    case ArenaBout.Group: Bout = ArenaBout.SemiFinal; break;
                    case ArenaBout.SemiFinal: Bout = ArenaBout.Final; break;
                    case ArenaBout.Final:
                        IsComplete = true;
                        if (OnGetExtraAward != null)
                        {
                            var roll = RandomHelper.RollDie(DiceType.D10);
                            switch (roll)
                            {
                                case 1: ExtraAward = await OnGetExtraAward.Invoke(TreasureType.Wonderful, 1); break;
                                case <= 4: ExtraAward = await OnGetExtraAward.Invoke(TreasureType.Fine, 1); break;
                                default: break;
                            } 
                        }
                        break;
                }
            }
            else
            {
                IsComplete = true;
                int hpLoss = 0;
                switch (Bout)
                {
                    case ArenaBout.Group: hpLoss = 2; break;
                    case ArenaBout.SemiFinal: hpLoss = 4; break;
                    case ArenaBout.Final: hpLoss = 6; break;
                }
                hpLoss = Math.Min(hpLoss, hero.CurrentHP);
                hero.CurrentHP -= hpLoss;
                var sanityLoss = Math.Min(2, hero.CurrentSanity);
                hero.CurrentSanity -= sanityLoss;
                Message += $"{hero.Name} lost the {Bout} bout. {hero.Name} takes {hpLoss} health damage and loses {sanityLoss} sanity.\n";
            }

            if (IsComplete)
            {
                Message += $"{hero.Name} total winnings {Winnings} coin and {Experience} experience.\n";
                if (ExtraAward != null)
                {
                    var extraAwardNames = string.Join(", ", ExtraAward.Select(award => award.Name));
                    Message += $"The hero also received an extra award of {extraAwardNames}";
                }
            }
        }

        private int ArenaExperience(ArenaBout bout)
        {
            switch (bout)
            {
                case ArenaBout.Group: return 50;
                case ArenaBout.SemiFinal: return 100;
                case ArenaBout.Final: return 150;
                default: return 0;
            }
        }

        private int ArenaWinnings(ArenaBout bout, Hero hero)
        {
            double multiplier = 1;
            switch (bout)
            {
                case ArenaBout.Group:
                    switch (hero.Level)
                    {
                        case 1: multiplier = 2; break;
                        case 2: multiplier = 1.9; break;
                        case 3: multiplier = 1.8; break;
                        case 4: multiplier = 1.7; break;
                        case 5: multiplier = 1.6; break;
                        case 6: multiplier = 1.5; break;
                        case 7: multiplier = 1.4; break;
                        case 8: multiplier = 1.3; break;
                        case 9: multiplier = 1.2; break;
                        default: multiplier = 1.1; break;
                    }
                    break;
                case ArenaBout.SemiFinal:
                    switch (hero.Level)
                    {
                        case 1: multiplier = 2.2; break;
                        case 2: multiplier = 2.1; break;
                        case 3: multiplier = 2; break;
                        case 4: multiplier = 1.9; break;
                        case 5: multiplier = 1.8; break;
                        case 6: multiplier = 1.7; break;
                        case 7: multiplier = 1.6; break;
                        case 8: multiplier = 1.5; break;
                        case 9: multiplier = 1.4; break;
                        default: multiplier = 1.3; break;
                    }
                    break;
                case ArenaBout.Final:
                    switch (hero.Level)
                    {
                        case 1: multiplier = 2.4; break;
                        case 2: multiplier = 2.3; break;
                        case 3: multiplier = 2.2; break;
                        case 4: multiplier = 2.1; break;
                        case 5: multiplier = 2; break;
                        case 6: multiplier = 1.9; break;
                        case 7: multiplier = 1.8; break;
                        case 8: multiplier = 1.7; break;
                        case 9: multiplier = 1.6; break;
                        default: multiplier = 1.5; break;
                    }
                    break;
            }
            return (int)Math.Floor(Bet * multiplier);
        }

        private int GetArenaModifier(Hero hero)
        {
            var modifier = 0;
            switch (hero.GetStat(BasicStat.HitPoints))
            {
                case < 10: modifier -= 5; break;
                case <= 15: modifier += 0; break;
                case > 15: modifier += 5; break;
            }

            switch (hero.GetStat(BasicStat.Strength))
            {
                case < 40: modifier -= 5; break;
                case <= 50: modifier += 0; break;
                case > 50: modifier += 5; break;
            }

            switch (Bout)
            {
                case Arena.ArenaBout.Group: modifier -= 10; break;
                case Arena.ArenaBout.SemiFinal: modifier -= 15; break;
                case Arena.ArenaBout.Final: modifier -= 20; break;
            }
            return modifier;
        }
    }

    public class Bank : ServiceLocation
    {
        public enum BankName
        {
            ChamberlingsReserve,
            SmartfallBank,
            TheVault
        }

        public new BankName Name { get; set; } = BankName.ChamberlingsReserve;
        public string Description { get; set; } = string.Empty;
        public int AccountBalance { get; set; }
        public bool HasCheckedBankAccount { get; set; }
        public double ProfitLoss { get; set; }

        public Bank(Settlement settlement) : base(SettlementServiceName.Banks, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.Banking,
                    };
        }

        public async Task<int> DepositAsync(int amount)
        {
            if (amount <= 0) return AccountBalance;

            await CheckBalanceAsync();
            AccountBalance += amount;
            return AccountBalance;
        }

        public async Task<int> WithdrawAsync(int amount)
        {
            if (amount <= 0) return 0;

            await CheckBalanceAsync();
            var withdrawAmount = Math.Min(amount, AccountBalance);
            AccountBalance -= withdrawAmount;
            return withdrawAmount;
        }

        public async Task<int> CheckBalanceAsync()
        {
            if (!HasCheckedBankAccount)
            {
                HasCheckedBankAccount = true;
                if (AccountBalance > 0)
                {
                    var rollResult = await new UserRequestService().RequestRollAsync("Roll for profit or loss chance.", "1d20");
                    ProfitLoss = GetProfitLoss(rollResult.Roll);
                    return (int)Math.Floor(AccountBalance * ProfitLoss);
                }
            }

            return AccountBalance;
        }

        private double GetProfitLoss(int roll)
        {
            switch (Name)
            {
                case BankName.ChamberlingsReserve:
                    return roll switch
                    {
                        <= 4 => 1.2,
                        <= 7 => 1.15,
                        <= 10 => 1.10,
                        <= 11 => 1.05,
                        <= 12 => 1,
                        <= 14 => 0.95,
                        <= 17 => 0.90,
                        <= 19 => 0.80,
                        >= 20 => 0
                    };
                case BankName.SmartfallBank:
                    return roll switch
                    {
                        <= 2 => 1.15,
                        <= 4 => 1.10,
                        <= 9 => 1.05,
                        <= 14 => 1,
                        <= 16 => 0.95,
                        <= 17 => 0.90,
                        >= 18 => 0

                    };
                case BankName.TheVault:
                    return roll switch
                    {
                        <= 2 => 1.3,
                        <= 4 => 1.2,
                        <= 5 => 1.15,
                        <= 6 => 1.10,
                        <= 7 => 1.05,
                        <= 10 => 1,
                        <= 14 => 0.95,
                        <= 16 => 0.90,
                        <= 17 => 0.80,
                        <= 18 => 0.70,
                        >= 19 => 0

                    };
                default: return 1;
            }
        }
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

    public class Temple : ServiceLocation
    {
        public GodName GodName { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CostToPray { get; set; } = 50;
        public string DiceToPray { get; set; } = "1d6";
        public ActiveStatusEffect? GrantedEffect { get; set; }

        public Temple(Settlement settlement) : base(SettlementServiceName.Temple, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.Pray,
                    };
        }
    }

    public class Bounty
    {
        public Monster Monster { get; private set; }
        public int Value { get; private set; }
        public int TargetAmount { get; private set; }
        public int AmountKilled { get; set; }

        public Bounty(Monster monster)
        {
            Monster = monster;
            SetTargetAndValue();
        }

        private void SetTargetAndValue()
        {
            Value = Monster.XP;
            while (TargetAmount * Value < 250)
            {
                TargetAmount++;
            }
        }
    }

    public class Guild : ServiceLocation
    {
        public List<Profession> AllowedToEnter { get; set; } = new();
        public List<(Skill, int)> AvailableSkillTraining { get; set; } = new();
        public int SkillTrainingFee { get; set; }
        public Dictionary<Hero, int> QuestsBeforeNextTraining { get; set; } = new();

        public Guild(SettlementServiceName name, Settlement settlement) : base(name, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling
                    };
        }

        public bool CanTrain(Hero hero)
        {
            if (!QuestsBeforeNextTraining.ContainsKey(hero))
            {
                return true;
            }
            return QuestsBeforeNextTraining[hero] > 0;
        }

        public bool AttemptToTrainHeroSkill(Hero hero, Skill skill)
        {
            var skillToTrain = AvailableSkillTraining.FirstOrDefault(s => s.Item1 == skill);
            if(CanTrain(hero))
            {
                if(skillToTrain.Item1 == skill && hero.Coins + hero.Party.Coins >= SkillTrainingFee)
                {
                    var remainingTrainingFee = SkillTrainingFee - hero.Coins;
                    hero.Coins -= SkillTrainingFee;
                    if (remainingTrainingFee > 0)
                    {
                        hero.Party.Coins -= remainingTrainingFee;
                    }
                    hero.SetSkill(skill, hero.GetSkill(skill) + 3);
                }
                else return false;
                
                QuestsBeforeNextTraining[hero] = 2;
            }
            return false;
        }
    }

    public class TheDarkGuild : Guild
    {
        public TheDarkGuild(Settlement settlement) : base(SettlementServiceName.TheDarkGuild, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.CombatSkill, 3), (Skill.RangedSkill, 3), (Skill.PickLocks, 3), (Skill.Perception, 3) };
            SkillTrainingFee = 300;
        }
    }

    public class FightersGuild : Guild
    {
        public EncounterService Encounter { get; set; }
        public List<Bounty>? WantedBounties { get; set; }

        public FightersGuild(Settlement settlement, EncounterService encounter) : base(SettlementServiceName.FightersGuild, settlement)
        {
            Settlement = settlement;
            Encounter = encounter;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.CombatSkill, 3), (Skill.Heal, 3), (Skill.Dodge, 3) };
            SkillTrainingFee = 300;
        }

        public override void RefreshStockForNewVisit()
        {
            //Call the base class method to reset the stock
            base.RefreshStockForNewVisit();
            if (Name == SettlementServiceName.FightersGuild && Encounter != null)
            {
                WantedBounties = GetFightersGuildBountyHuntList(Encounter);
            }
        }

        private List<Bounty> GetFightersGuildBountyHuntList(EncounterService encounter)
        {
            var availableBounties = encounter.Monsters.Where(m => !m.IsUnique && m.XP >= 50).ToList();
            var currentBounties = new List<Bounty>();
            availableBounties.Shuffle();
            for (int i = 0; i < 5; i++)
            {
                var monster = availableBounties[i];
                var bounty = new Bounty(monster);
                currentBounties.Add(bounty);
            }

            return currentBounties;
        }
    }

    public class WizardsGuild : Guild
    {
        public WizardsGuild(Settlement settlement) : base(SettlementServiceName.WizardsGuild, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.ChargeMagicItem,
                        SettlementActionType.IdentifyMagicItem,
                        SettlementActionType.LearnSpell
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.ArcaneArts, 3), (Skill.Perception, 3), (Skill.Heal, 3) };
            SkillTrainingFee = 300;
        }
    }

    public class AlchemistsGuild : Guild
    {
        public AlchemistsGuild(Settlement settlement) : base(SettlementServiceName.AlchemistGuild, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.IdentifyPotion
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.Alchemy, 3), (Skill.Heal, 3), (Skill.Perception, 3) };
            SkillTrainingFee = 300;
        }
    }

    public class RangersGuild : Guild
    {
        public RangersGuild(Settlement settlement) : base(SettlementServiceName.RangersGuild, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.CombatSkill, 3), (Skill.RangedSkill, 3), (Skill.Dodge, 3), (Skill.Heal, 3), (Skill.Foraging, 3) };
            SkillTrainingFee = 300;
        }
    }

    public class Crusade
    {
        public EncounterType encounterType { get; set; }
    }

    public class TheInnerSanctum : Guild
    {
        public EncounterService Encounter { get; set; }
        public EncounterType? Crusade { get; set; }

        public TheInnerSanctum(Settlement settlement, EncounterService encounter) : base(SettlementServiceName.TheInnerSanctum, settlement)
        {
            Settlement = settlement;
            Encounter = encounter;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.CombatSkill, 3), (Skill.Dodge, 3), (Skill.BattlePrayers, 3) };
            SkillTrainingFee = 300;
        }
    }

    public class Estate : ServiceLocation
    {

        public Estate(Settlement settlement) : base(SettlementServiceName.Estate, settlement)
        {

        }
    }

    public class SettlementService
    {
        private readonly UserRequestService _userRequest;
        private readonly QuestService _quest;
        private readonly PartyManagerService _partyManager;
        private readonly TreasureService _treasure;
        private readonly GameDataService _gameData;
        private readonly EncounterService _encounter;

        public List<Settlement> Settlements => GetSettlements();

        public SettlementService(
            UserRequestService userRequestService, 
            QuestService questService,
            PartyManagerService partyManager,
            TreasureService treasure,
            GameDataService gameData,
            EncounterService encounter)
        {
            _userRequest = userRequestService;
            _quest = questService;
            _partyManager = partyManager;
            _treasure = treasure;
            _gameData = gameData;
            _encounter = encounter;


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
            if (settlement.QuestColor == null || settlement.QuestDice == null) return null;
            var rollResult = await _userRequest.RequestRollAsync("Roll for random quest", settlement.QuestDice);
            await Task.Yield();
            return _quest.GetQuestHexLocationByColorNumber(rollResult.Roll, (QuestColor)settlement.QuestColor);
        }

        public List<Settlement> GetSettlements()
        {
            var list = new List<Settlement>();
            var settlement = new Settlement
            (
                SettlementName.Caelkirk,
                11,
                new List<HexTile>
                    {
                        new HexTile(new Hex(0, -28, 28)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -29, 29)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -29, 28)) { Terrain = TerrainType.Town }
                    },
                "1d4",
                QuestColor.Red
            );
            settlement.State.Inn = new Inn(settlement)
            {
                Price = 35
            };
            settlement.State.BlackSmith = new BlackSmith(settlement)
            {
                WeaponAvailabilityModifier = -2,
                WeaponPriceModifier = 1.1,
                ArmourAvailabilityModifier = -2,
                ArmourPriceModifier = 1.1
            };
            settlement.State.GeneralStore = new GeneralStore(settlement)
            {
                EquipmentAvailabilityModifier = -2,
                EquipmentPriceModifier = 1.1,
                ShopSpecials = new() { new() { ItemName = "Fishing Gear", Price = 50, Availability = 6 } }
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Coalfell,
                12,
                new List<HexTile>
                {
                    new HexTile(new Hex(0, 15, -15)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(0, 14, -14)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(1, 14, -15)) { Terrain = TerrainType.Town }
                },
                "1d6",
                QuestColor.Green
            );
            settlement.State.Inn = new Inn(settlement)
            {
                Price = 35
            };
            settlement.State.BlackSmith = new BlackSmith(settlement)
            {
                WeaponAvailabilityModifier = 1,
                WeaponPriceModifier = 0.9,
                ArmourAvailabilityModifier = 1,
                ArmourPriceModifier = 0.9
            };
            settlement.State.GeneralStore = new GeneralStore(settlement)
            {
                EquipmentAvailabilityModifier = -2,
                EquipmentPriceModifier = 1.2
            };
            settlement.State.Temples = new()
            {
                new (settlement)
                {
                    GodName = GodName.Ohlnir,
                    Description = "At the temple of Ohlnir, your heroes may pray for guidance of their weapons, so that they will always strike true.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true)
                }
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Freyfell,
                11,
                new List<HexTile>
                    {
                        new HexTile(new Hex(0, -15, 15)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -15, 14)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(1, -16, 15)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -16, 16)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(-1, -14, 15)) { Terrain = TerrainType.Town },
                        new HexTile(new Hex(0, -14, 14)) { Terrain = TerrainType.Town }
                    },
                "1d6",
                QuestColor.Pink
            );
            settlement.State.Inn = new Inn(settlement)
            {
                Price = 25
            };
            settlement.State.BlackSmith = new(settlement);
            settlement.State.GeneralStore = new(settlement)
            {
                EquipmentAvailabilityModifier = 1,
                EquipmentPriceModifier = 0.9
            };
            settlement.State.Arena = new Arena(settlement);
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Irondale,
                12,
                new List<HexTile>
                {
                    new HexTile(new Hex(0, -13, 13)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(1, -13, 12)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(1, -14, 13)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(0, -14, 14)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-1, -12, 13)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(0, -12, 12)) { Terrain = TerrainType.Town }
                },
                "1d6",
                QuestColor.Blue
            );
            settlement.State.Inn = new (settlement)
            {
                Price = 15
            };
            settlement.State.BlackSmith = new (settlement);
            settlement.State.GeneralStore = new (settlement);
            settlement.State.Temples = new ()
            {
                new (settlement)
                {
                    GodName = GodName.Rhidnir,
                    Description = "At the temple of the Great Trickster, your hero may pray for increased luck, but it is risky indeed as the jester may just as easily answer the prayer with what he sees as a funny surprise!",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RhidnirsBlessing, -1, statBonus: (BasicStat.Luck, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Iphy,
                    Description = "At the temple of Iphy, your heroes may pray for increased mental strength, which will grant you the Resolve it takes to face the horrors of the dungeons.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.IphysBlessing, -1, statBonus: (BasicStat.Resolve, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Metheia,
                    Description = "At the temple of Metheia, you may pray for health and a long life. If she answers your hero's prayers, they will get +1 HP until the next time the hero leaves a dungeon.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.MetheiasBlessing, -1, statBonus: (BasicStat.HitPoints, 1), removeEndOfDungeon: true)
                }
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Whiteport,
                10,
                new List<HexTile>
                {
                    new HexTile(new Hex(-16, 0, 16)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-16, -1, 17)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-17, 1, 16)) { Terrain = TerrainType.Town }
                },
                "1d6",
                QuestColor.Black
            );
            settlement.State.Inn = new(settlement)
            {
                Price = 15
            };
            settlement.State.GeneralStore = new(settlement)
            {
                EquipmentAvailabilityModifier = 1,
                EquipmentPriceModifier = 0.9,
                ShopSpecials = new() { new() { ItemName = "Fishing Gear", Price = 50, Availability = 6 } }
            };
            settlement.State.Temples = new ()
            {
                new (settlement)
                {
                    GodName = GodName.Rhidnir,
                    Description = "At the temple of the Great Trickster, your hero may pray for increased luck, but it is risky indeed as the jester may just as easily answer the prayer with what he sees as a funny surprise!",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RhidnirsBlessing, -1, statBonus: (BasicStat.Luck, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Iphy,
                    Description = "At the temple of Iphy, your heroes may pray for increased mental strength, which will grant you the Resolve it takes to face the horrors of the dungeons.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.IphysBlessing, -1, statBonus: (BasicStat.Resolve, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Metheia,
                    Description = "At the temple of Metheia, you may pray for health and a long life. If she answers your hero's prayers, they will get +1 HP until the next time the hero leaves a dungeon.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.MetheiasBlessing, -1, statBonus: (BasicStat.HitPoints, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Ohlnir,
                    Description = "At the temple of Ohlnir, your heroes may pray for guidance of their weapons, so that they will always strike true.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true)
                }
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Windfair,
                12,
                new List<HexTile>
                {
                    new HexTile(new Hex(12, -8, -4)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(12, -9, -3)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(13, -9, -4)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(12, -7, -5)) { Terrain = TerrainType.Town }
                },
                "1d6",
                QuestColor.Blue
            );
            settlement.State.Inn = new (settlement)
            {
                Price = 35
            };
            settlement.State.BlackSmith = new (settlement);
            settlement.State.GeneralStore = new (settlement)
            {
                EquipmentAvailabilityModifier = -1,
                EquipmentPriceModifier = 1.1,
                ShopSpecials = new() { new() { ItemName = "Fishing Gear", Price = 50, Availability = 6 } }
            };
            settlement.State.Temples = new()
            {
                new(settlement)
                {
                    GodName = GodName.Ohlnir,
                    Description = "At the temple of Ohlnir, your heroes may pray for guidance of their weapons, so that they will always strike true.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true)
                },
                new(settlement)
                {
                    GodName = GodName.Charus,
                    Description = "At the temple of Charus, your heroes may pray for increased endurance so that they can endure the physical challenges ahead.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.CharusBlessing, -1, statBonus: (BasicStat.Energy, 1), removeEndOfDungeon: true)
                }
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Rochdale,
                12,
                new List<HexTile>
                {
                    new HexTile(new Hex(-6, 7, -1)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-7, 8, -1)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-6, 8, -2)) { Terrain = TerrainType.Town }
                },
                "1d6",
                QuestColor.Purple
            );
            settlement.State.Inn = new (settlement)
            {
                Price = 20
            };
            settlement.State.GeneralStore = new (settlement);
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.SilverCity,
                8,
                new List<HexTile>
                {
                    new HexTile(new Hex(0, 0, 0)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(1, 0, -1)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(0, 1, -1)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-1, 1, 0)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-1, 0, 1)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(0, -1, 1)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(1, -1, 0)) { Terrain = TerrainType.Town }
                },
                "2d20",
                QuestColor.White
            );
            settlement.State.Inn = new (settlement)
            {
                Price = 25
            };
            settlement.State.BlackSmith = new(settlement);
            settlement.State.GeneralStore = new(settlement);
            settlement.State.Banks = new()
            {
                new (settlement)
                {
                    Name = Bank.BankName.ChamberlingsReserve,
                    Description = "This is the go-to bank for the noble people in the city. They have good security, but rather aggressive investment plans for your deposits. After all, most people who utilise this bank can afford to lose some money now and then. It is acceptable for the long-term win."
                },
                new (settlement)
                {
                    Name = Bank.BankName.SmartfallBank,
                    Description = "Most commoners in the city use this bank, and indeed throughout the kingdom, as it exists in most of the larger cities. Security is somewhat lax; however, they make careful investments. Typically, deposits grow slowly but surely."
                },
                new (settlement)
                {
                    Name = Bank.BankName.TheVault,
                    Description = "This bank invests your money in more dubious areas, with a high risk but also with a good profit, should the investments succeed. The security is moderately good."
                }
            };
            settlement.State.Temples = new()
            {
                new (settlement)
                {
                    GodName = GodName.Rhidnir,
                    Description = "At the temple of the Great Trickster, your hero may pray for increased luck, but it is risky indeed as the jester may just as easily answer the prayer with what he sees as a funny surprise!",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RhidnirsBlessing, -1, statBonus: (BasicStat.Luck, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Iphy,
                    Description = "At the temple of Iphy, your heroes may pray for increased mental strength, which will grant you the Resolve it takes to face the horrors of the dungeons.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.IphysBlessing, -1, statBonus: (BasicStat.Resolve, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Metheia,
                    Description = "At the temple of Metheia, you may pray for health and a long life. If she answers your hero's prayers, they will get +1 HP until the next time the hero leaves a dungeon.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.MetheiasBlessing, -1, statBonus: (BasicStat.HitPoints, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Ohlnir,
                    Description = "At the temple of Ohlnir, your heroes may pray for guidance of their weapons, so that they will always strike true.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Charus,
                    Description = "At the temple of Charus, your heroes may pray for increased endurance so that they can endure the physical challenges ahead.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.CharusBlessing, -1, statBonus: (BasicStat.Energy, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Ramos,
                    Description = "At the temple of Ramos, your hero may pray for increased strength. If he decides to listen to the prayer, the hero will be granted +5 STR.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RamosBlessing, -1, statBonus: (BasicStat.Strength, 5), removeEndOfDungeon: true)
                }
            };
            settlement.State.Arena = new (settlement);
            settlement.State.TheDarkGuild = new(settlement)
            {
                AllowedToEnter = _gameData.Professions.Where(p => p.Name == "Thief" || p.Name == "Rogue").ToList()
            };
            settlement.State.FightersGuild = new (settlement, _encounter)
            {
                AllowedToEnter = _gameData.Professions.Where(p => p.Name == "Warrior" || p.Name == "Barbarian").ToList()                
            };
            settlement.State.WizardsGuild = new (settlement)
            {
                AllowedToEnter = _gameData.Professions.Where(p => p.Name == "Wizard").ToList()
            };
            settlement.State.AlchemistsGuild = new (settlement)
            {
                AllowedToEnter = _gameData.Professions
            };
            settlement.State.RangersGuild = new (settlement)
            {
                AllowedToEnter = _gameData.Professions.Where(p => p.Name == "Ranger").ToList()
            };
            settlement.State.TheInnerSanctum = new (settlement, _encounter)
            {
                AllowedToEnter = _gameData.Professions.Where(p => p.Name == "Warrior Priest").ToList()
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.TheOutpost,
                9,
                new List<HexTile>
                {
                    new HexTile(new Hex(-1, 21, -21)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-1, 20, -20)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(0, 20, -21)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-2, 22, -21)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(-1, 22, -22)) { Terrain = TerrainType.Town }
                },
                "1d12",
                QuestColor.Yellow,
                "Toll. If you are to head out to a Quest Site in the Ancient Lands (yellow 1-12), you will have to pay a toll to the King of 100 c per hero first. You must be a member of the league to be allowed to pass."
            );
            settlement.State.BlackSmith = new (settlement)
            {
                WeaponPriceModifier = 1.2,
                ArmourPriceModifier = 1.2
            };
            settlement.State.GeneralStore = new (settlement)
            {
                EquipmentAvailabilityModifier = -2,
                EquipmentPriceModifier = 1.2
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Durburim,
                12,
                new List<HexTile>
                {
                    new HexTile(new Hex(20, -10, -10)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(20, -11, -9)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(21, -11, -10)) { Terrain = TerrainType.Town }
                }
            );
            settlement.State.Inn = new (settlement)
            {
                Price = 65
            };
            settlement.State.BlackSmith = new (settlement)
            {
                WeaponAvailabilityModifier = 1,
                WeaponPriceModifier = 1.2,
                ArmourPriceModifier = 1.1,
                WeaponMaxDurabilityModifier = 2
            };
            settlement.State.GeneralStore = new (settlement)
            {
                EquipmentAvailabilityModifier = -1,
                EquipmentPriceModifier = 1.2
            };
            settlement.State.Temples = new()
            {
                new (settlement)
                {
                    GodName = GodName.Rhidnir,
                    Description = "At the temple of the Great Trickster, your hero may pray for increased luck, but it is risky indeed as the jester may just as easily answer the prayer with what he sees as a funny surprise!",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RhidnirsBlessing, -1, statBonus: (BasicStat.Luck, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Iphy,
                    Description = "At the temple of Iphy, your heroes may pray for increased mental strength, which will grant you the Resolve it takes to face the horrors of the dungeons.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.IphysBlessing, -1, statBonus: (BasicStat.Resolve, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Metheia,
                    Description = "At the temple of Metheia, you may pray for health and a long life. If she answers your hero's prayers, they will get +1 HP until the next time the hero leaves a dungeon.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.MetheiasBlessing, -1, statBonus: (BasicStat.HitPoints, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Ohlnir,
                    Description = "At the temple of Ohlnir, your heroes may pray for guidance of their weapons, so that they will always strike true.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Charus,
                    Description = "At the temple of Charus, your heroes may pray for increased endurance so that they can endure the physical challenges ahead.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.CharusBlessing, -1, statBonus: (BasicStat.Energy, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Ramos,
                    Description = "At the temple of Ramos, your hero may pray for increased strength. If he decides to listen to the prayer, the hero will be granted +5 STR.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RamosBlessing, -1, statBonus: (BasicStat.Strength, 5), removeEndOfDungeon: true)
                }
            };
            list.Add(settlement);

            settlement = new Settlement
            (
                SettlementName.Birnheim,
                12,
                new List<HexTile>
                {
                    new HexTile(new Hex(14, -23, 9)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(13, -22, 9)) { Terrain = TerrainType.Town },
                    new HexTile(new Hex(14, -22, 8)) { Terrain = TerrainType.Town }
                }
            );
            settlement.State.Inn = new Inn(settlement)
            {
                Price = 65
            };
            settlement.State.BlackSmith = new (settlement)
            {
                WeaponAvailabilityModifier = -1,
                WeaponPriceModifier = 1.2,
                ArmourAvailabilityModifier = 1,
                ArmourPriceModifier = 1.2,
                ArmourMaxDurabilityModifier = 2
            };
            settlement.State.GeneralStore = new (settlement)
            {
                EquipmentAvailabilityModifier = -2,
                EquipmentPriceModifier = 1.2
            };
            settlement.State.Temples = new()
            {
                new (settlement)
                {
                    GodName = GodName.Rhidnir,
                    Description = "At the temple of the Great Trickster, your hero may pray for increased luck, but it is risky indeed as the jester may just as easily answer the prayer with what he sees as a funny surprise!",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RhidnirsBlessing, -1, statBonus: (BasicStat.Luck, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Iphy,
                    Description = "At the temple of Iphy, your heroes may pray for increased mental strength, which will grant you the Resolve it takes to face the horrors of the dungeons.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.IphysBlessing, -1, statBonus: (BasicStat.Resolve, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Metheia,
                    Description = "At the temple of Metheia, you may pray for health and a long life. If she answers your hero's prayers, they will get +1 HP until the next time the hero leaves a dungeon.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.MetheiasBlessing, -1, statBonus: (BasicStat.HitPoints, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Ohlnir,
                    Description = "At the temple of Ohlnir, your heroes may pray for guidance of their weapons, so that they will always strike true.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Charus,
                    Description = "At the temple of Charus, your heroes may pray for increased endurance so that they can endure the physical challenges ahead.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.CharusBlessing, -1, statBonus: (BasicStat.Energy, 1), removeEndOfDungeon: true)
                },
                new (settlement)
                {
                    GodName = GodName.Ramos,
                    Description = "At the temple of Ramos, your hero may pray for increased strength. If he decides to listen to the prayer, the hero will be granted +5 STR.",
                    GrantedEffect = new ActiveStatusEffect(StatusEffectType.RamosBlessing, -1, statBonus: (BasicStat.Strength, 5), removeEndOfDungeon: true)
                }
            };
            list.Add(settlement);

            return list;
        }

        public Settlement GetSettlementByName(SettlementName name)
        {
            return Settlements.First(s => s.Name == name);
        }
    }
}
