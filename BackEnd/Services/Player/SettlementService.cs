using BlazorContextMenu;
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using RogueSharp;
using System.Threading.Tasks;
using System.Xml.Linq;
using static LoDCompanion.BackEnd.Services.Player.EstateFurnishing;
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
        public int DungeonsBetweenVisits { get; set; }
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

        public SettlementActionResult GetShopInventory(Hero hero, SettlementActionResult result)
        {
            if (!AvailableActions.Contains(SettlementActionType.BuyingAndSelling))
            {
                result.Message = "This service does not have anything to buy or sell";
                result.WasSuccessful = false;
                return result;
            }

            result.ShopInventory = CurrentAvailableStock;
            return result;
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

        public async Task<SettlementActionResult> Gamble(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            var minBet = 50;
            var maxBet = 500;
            if (result.AvailableCoins < minBet)
            {
                result.Message = $"{hero.Name} does not have enough coin to gamble with.";
                result.WasSuccessful = false;
                return result;
            }

            var inputResult = await userRequest.RequestNumberInputAsync("How much do you want to bet?", min: minBet, max: Math.Min(maxBet, result.AvailableCoins), canCancel: true);
            await Task.Yield();
            if (!inputResult.WasCancelled)
            {
                var bet = inputResult.Amount;
                result.AvailableCoins -= bet;
                var luck = hero.GetStat(BasicStat.Luck);
                var rollResult = await userRequest.RequestRollAsync("Roll gambling result.", "1d10");
                await Task.Yield();
                if (rollResult.Roll < 10) rollResult.Roll -= luck;
                switch (rollResult.Roll)
                {
                    case <= 1:
                        bet *= 2;
                        result.Message = $"Jackpot! You won {bet}";
                        result.AvailableCoins += bet;
                        break;
                    case <= 3:
                        bet = (int)Math.Ceiling(bet * 1.5);
                        result.Message = $"Win! You won {bet}";
                        result.AvailableCoins += bet;
                        break;
                    case 10:
                        result.Message = $"The others around the table are certain {hero.Name} has cheated, and they end up getting a good beting and are robbed of 100c on top of their bet.";
                        result.AvailableCoins -= Math.Min(100, result.AvailableCoins);
                        break;
                    default: result.Message = "You lose all your bets."; break;
                }
            }
            return result;
        }

        public static async Task<SettlementActionResult> EnchantItemAsync(Hero hero, SettlementActionResult result, UserRequestService userRequest, int bonusModifier = 0)
        {
            if (hero.ProfessionName != ProfessionName.Wizard)
            {
                result.Message = $"{hero.Name} is not proficient enough in the arcane arts.";
                result.WasSuccessful = false;
                return result;
            }

            if (!hero.CanCreateScrollEnchantItem)
            {
                result.Message = $"{hero.Name} has already attempted to enchanted an item or attempted creation of two scrolls this visit.";
                result.WasSuccessful = false;
                return result;
            }

            if (hero.Spells != null)
            {
                var magicScribbles = hero.Spells.FirstOrDefault(s => s.Name == "Enchant Item");
                if (magicScribbles == null)
                {
                    result.Message = $"{hero.Name} does not know the spell Enchant Item.";
                    result.WasSuccessful = false;
                    return result;
                }
            }
            var powerStones = hero.Inventory.Backpack.OfType<PowerStone>().ToList();
            if (!powerStones.Any())
            {
                result.Message = $"{hero.Name} does not have a Power Stone to enchant items with.";
                result.WasSuccessful = false;
                return result;
            }

            var selectedPowerStone = powerStones.First();
            if (powerStones.Count > 1)
            {
                var stoneChoiceRequest = await userRequest.RequestChoiceAsync("Choose a power stone to enchant with.", powerStones, p => p.Name);
                await Task.Yield();
                selectedPowerStone = stoneChoiceRequest.SelectedOption;
            }

            if (selectedPowerStone == null)
            {
                result.Message = "Invalid power stone selection.";
                result.WasSuccessful = false;
                return result;
            }

            var equipmentList = hero.Inventory.Backpack
                .Where(i => i != null &&
                            ((selectedPowerStone.ItemToEnchant == PowerStoneEffectItem.Weapon && i is Weapon) ||
                             (selectedPowerStone.ItemToEnchant == PowerStoneEffectItem.ArmourShield && (i is Armour || i is Shield)) ||
                             (selectedPowerStone.ItemToEnchant == PowerStoneEffectItem.RingAmulet && (i.Name == "Ring" || i.Name == "Amulet"))) &&
                            string.IsNullOrEmpty(i.MagicEffect))
                .Cast<Equipment>()
                .ToList();

            if (!equipmentList.Any())
            {
                result.Message = $"There are no valid items to enchant with {selectedPowerStone.Name}.";
                result.WasSuccessful = false;
                return result;
            }

            var choiceRequest = await userRequest.RequestChoiceAsync("Choose the item to enchant.", equipmentList, e => e.Name);
            await Task.Yield();
            var selectedEquipment = choiceRequest.SelectedOption;

            if (selectedEquipment == null)
            {
                result.Message = "Invalid item selection.";
                result.WasSuccessful = false;
                return result;
            }

            selectedEquipment = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, selectedEquipment);

            var rollResult = await userRequest.RequestRollAsync("Roll arcane arts skill check.", "1d100", skill: (hero, Skill.ArcaneArts));
            var skillTarget = hero.GetSkill(Skill.ArcaneArts) + bonusModifier;

            if (rollResult.Roll > skillTarget)
            {
                result.Message += $"{hero.Name} fails and {selectedEquipment?.Name} is destroyed. However, the power stone is still intact.";
            }
            else
            {
                BackpackHelper.TakeOneItem(hero.Inventory.Backpack, selectedPowerStone);

                if (selectedEquipment != null)
                {
                    selectedEquipment.Name += $"{selectedPowerStone.Name.Replace("Power stone", "")}";
                    selectedEquipment.MagicEffect = $"{selectedPowerStone.Name.Replace("Power stone of ", "")}";
                    selectedEquipment.Value *= selectedPowerStone.ValueModifier;
                    if (selectedPowerStone.ActiveStatusEffects != null)
                    {
                        selectedEquipment.ActiveStatusEffects ??= new();
                        selectedEquipment.ActiveStatusEffects.AddRange(selectedPowerStone.ActiveStatusEffects);
                    }

                    if (selectedEquipment is Weapon weapon && selectedPowerStone.WeaponProperties != null)
                    {
                        foreach (var prop in selectedPowerStone.WeaponProperties)
                        {
                            if (prop.Key == WeaponProperty.DamageBonus && weapon.Properties.ContainsKey(WeaponProperty.DamageBonus))
                            {
                                weapon.Properties[WeaponProperty.DamageBonus] += prop.Value;
                            }
                            else weapon.Properties.TryAdd(prop.Key, prop.Value);
                        }
                    }
                    else if (selectedEquipment is Armour || selectedEquipment is Shield && selectedPowerStone.DefenseBonus > 0)
                    {
                        if (selectedEquipment is Armour armour) armour.DefValue += selectedPowerStone.DefenseBonus;
                        if (selectedEquipment is Shield shield) shield.DefValue += selectedPowerStone.DefenseBonus;
                    }

                    result.Message += $"{selectedEquipment.Name} was created!";
                    await BackpackHelper.AddItem(hero.Inventory.Backpack, selectedEquipment);
                }
            }

            return result;
        }

        public static async Task<SettlementActionResult> CreateScroll(Hero hero, SettlementActionResult result, UserRequestService userRequest, int bonusModifier = 0)
        {
            if (hero.ProfessionName != ProfessionName.Wizard)
            {
                result.Message = $"{hero.Name} is not proficient enough in the arcane arts.";
                result.WasSuccessful = false;
                return result;
            }

            if (!hero.CanCreateScrollEnchantItem)
            {
                result.Message = $"{hero.Name} has already attempted to enchanted an item or attempted creation of two scrolls this visit.";
                result.WasSuccessful = false;
                return result;
            }

            if (hero.Spells != null)
            {
                var magicScribbles = hero.Spells.FirstOrDefault(s => s.Name == "Magic Scribbles");
                if (magicScribbles == null)
                {
                    result.Message = $"{hero.Name} does not know the spell Magic Scribbles.";
                    result.WasSuccessful = false;
                    return result;
                }
            }
            var scroll = hero.Inventory.Backpack.FirstOrDefault(i => i != null && i.Name == "Parchment");
            if (scroll == null)
            {
                result.Message = $"{hero.Name} does not have a Parchment to create the scroll with.";
                result.WasSuccessful = false;
                return result;
            }

            for (int i = 0; i < Math.Min(2, scroll.Quantity); i++)
            {
                BackpackHelper.TakeOneItem(hero.Inventory.Backpack, scroll);
                var rollResult = await userRequest.RequestRollAsync("Roll arcane arts skill check.", "1d100", skill: (hero, Skill.ArcaneArts));
                await Task.Yield();
                var skillTarget = hero.GetSkill(Skill.ArcaneArts) + bonusModifier;
                if (rollResult.Roll > skillTarget || hero.Spells == null)
                {
                    result.Message += $"{hero.Name} fails to create a scroll.";
                }
                else
                {
                    var spellList = hero.Spells.Cast<Spell>().ToList();
                    var choiceRequest = await userRequest.RequestChoiceAsync("Choose the scroll you wish to create.", spellList, s => $"{s.Name} Description: {s.SpellEffect}");
                    await Task.Yield();
                    var spell = choiceRequest.SelectedOption != null ? choiceRequest.SelectedOption : new Spell();
                    var newScroll = new Scroll(spell) { Quantity = 1, Value = 100 };
                    result.Message += $"{newScroll.Name} was created!";
                    await BackpackHelper.AddItem(hero.Inventory.Backpack, newScroll);
                }
            }
            hero.HasCreatedScrolls = true;

            return result;
        }

        public async Task<SettlementActionResult> RestRecuperation(Party party, SettlementActionResult result, UserRequestService userRequest)
        {
            var estate = Settlement.State.Estate;
            //The party owns an estate in this settlement (free stay)
            if (estate != null && estate.IsOwned)
            {
                result.Message = "The party rests comfortably in their estate.";
                result.AvailableCoins = await PerformRest(party, 0, result.AvailableCoins, false, userRequest); // Free rest, not in stables
                return result;
            }

            // The party can afford the inn
            if (result.AvailableCoins >= Price)
            {
                var stayAtInn = await userRequest.RequestYesNoChoiceAsync($"A room at the inn costs {Price} coins. Would you like to stay the night?");
                if (stayAtInn)
                {
                    result.Message = "The party enjoys a comfortable night at the ";
                    result.AvailableCoins = await PerformRest(party, Price, result.AvailableCoins, false, userRequest);
                    return result;
                }
                else
                {
                    result.Message = "The party decides not to rest at the ";
                    result.WasSuccessful = false;
                    return result;
                }
            }

            // The party cannot afford the inn, but might afford the stables
            if (result.AvailableCoins < SleepInStablesPrice)
            {
                result.Message = "You do not have enough coin to stay at the Inn, not even in the stables.";
                result.WasSuccessful = false;
                return result;
            }
            else
            {
                var sleepInStables = await userRequest.RequestYesNoChoiceAsync($"You cannot afford a room ({Price}c), but the stables are available for {SleepInStablesPrice} coin. Sleep in the stables?");
                if (sleepInStables)
                {
                    result.Message = "The party rests in the stables.";
                    result.AvailableCoins = await PerformRest(party, SleepInStablesPrice, result.AvailableCoins, true, userRequest);
                }
                else
                {
                    result.Message = "The party chooses not to sleep in the stables.";
                    result.WasSuccessful = false;
                }
                return result;
            }
        }

        private async Task<int> PerformRest(Party party, int cost, int availableCoin, bool isStables, UserRequestService userRequest)
        {
            // Deduct cost from party funds
            availableCoin -= cost;

            foreach (var hero in party.Heroes)
            {
                if (isStables)
                {
                    // Rules for sleeping in the stables
                    hero.Heal(RandomHelper.RollDie(DiceType.D6));
                    int missingEnergy = hero.GetStat(BasicStat.Energy) - hero.CurrentEnergy;
                    int missingluck = hero.GetStat(BasicStat.Luck) - hero.CurrentLuck;
                    hero.CurrentEnergy = Math.Min((int)Math.Floor(hero.GetStat(BasicStat.Energy) / 2.0), missingEnergy);
                    hero.CurrentLuck = Math.Min((int)Math.Floor(hero.GetStat(BasicStat.Luck) / 2.0), missingluck);

                    if (hero.CurrentMana.HasValue)
                    {
                        int missingMana = hero.GetStat(BasicStat.Wisdom) - hero.CurrentMana.Value;
                        hero.CurrentMana = Math.Min((int)Math.Floor(hero.GetStat(BasicStat.Wisdom) / 2.0), missingMana);
                    }
                }
                else
                {
                    // Rules for sleeping in the inn
                    hero.Heal(RandomHelper.RollDice("2d6"));
                    hero.CurrentEnergy = hero.GetStat(BasicStat.Energy);
                    hero.CurrentLuck = hero.GetStat(BasicStat.Luck);
                    if (hero.CurrentMana.HasValue)
                    {
                        hero.CurrentMana = hero.GetStat(BasicStat.Wisdom);
                    }

                    // Tending to memories (Sanity)
                    var missingSanity = hero.GetStat(BasicStat.Sanity) - hero.CurrentSanity;
                    hero.CurrentSanity += Math.Min(RandomHelper.RollDie(DiceType.D3), missingSanity);

                    missingSanity = hero.GetStat(BasicStat.Sanity) - hero.CurrentSanity;
                    int sanityCost = RandomHelper.RollDie(DiceType.D3) * 100;
                    if (missingSanity > 0 && await userRequest.RequestYesNoChoiceAsync($"{hero.Name} has {missingSanity} sanity left to heal. Do they wish to drown their memories in ale or other pleasures at the cost of {sanityCost}, in an attempt to regain some sanity?"))
                    {
                        if (availableCoin >= sanityCost)
                        {
                            availableCoin -= sanityCost;
                            hero.CurrentSanity += Math.Min(RandomHelper.RollDie(DiceType.D6), missingSanity);
                        }
                    }
                }
            }
            return availableCoin;
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
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.RepairWeapons,
                        SettlementActionType.RepairArmour,
                    };
        }

        public async Task<SettlementActionResult> RepairWeaponsArmour(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            bool isRepairing = true;
            while (isRepairing)
            {
                var weaponsArmourList = hero.Inventory.GetAllWeaponsArmour();
                var repairableList = weaponsArmourList.Where(item => item.RepairCost <= result.AvailableCoins && item.RepairCost >= 2).ToList();
                if (repairableList.Count < 1)
                {
                    result.Message = "No more items can be repaired";
                    isRepairing = false; 
                    break;
                }

                var choiceResult = await userRequest.RequestChoiceAsync<Equipment>(
                    "Choose an item to repair.",
                    repairableList,
                    item => $"{item.Name}, Repair: {item.Durability}>{item.MaxDurability} {item.RepairCost}c",
                    canCancel: true);
                if (choiceResult.WasCancelled)
                {
                    isRepairing = false;
                    break;
                }

                var item = choiceResult.SelectedOption;
                if (!choiceResult.WasCancelled && item != null)
                {
                    result.AvailableCoins -= item.RepairCost;
                    item.Repair();
                    result.Message += $"{item.Name} was repaired.";
                } 
            }
            return result;
        }
    }

    public class GeneralStore : ServiceLocation
    {
        public int EquipmentAvailabilityModifier { get; set; }
        public double EquipmentPriceModifier { get; set; } = 1d;
        public List<ShopSpecial>? ShopSpecials { get; set; }
        public static int IdentifyPotionPrice { get; set; } = 25; 

        public GeneralStore(Settlement settlement) : base(SettlementServiceName.GeneralStore, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.IdentifyPotion,
                        SettlementActionType.RepairEquipment
                    };
        }

        public async Task<SettlementActionResult> RepairEquipment(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            bool isRepairing = true;
            while (isRepairing)
            {
                var weaponsArmourList = hero.Inventory.GetAllNonWeaponsArmour();
                var repairableList = weaponsArmourList.Where(item => item.RepairCost <= result.AvailableCoins && item.RepairCost >= 2).ToList();
                if (repairableList.Count < 1)
                {
                    result.Message = "No more items can be repaired";
                    isRepairing = false;
                    break;
                }

                var choiceResult = await userRequest.RequestChoiceAsync<Equipment>(
                    "Choose an item to repair.",
                    repairableList,
                    item => $"{item.Name}, Repair: {item.Durability}>{item.MaxDurability} {item.RepairCost}c",
                    canCancel: true);
                if (choiceResult.WasCancelled)
                {
                    isRepairing = false;
                    break;
                }

                var item = choiceResult.SelectedOption;
                if (!choiceResult.WasCancelled && item != null)
                {
                    result.AvailableCoins -= item.RepairCost;
                    item.Repair();
                    result.Message += $"{item.Name} was repaired.";
                }
            }
            return result;
        }

        public static async Task<SettlementActionResult> IdentifyPotion(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            if (result.AvailableCoins < IdentifyPotionPrice)
            {
                result.Message = $"{hero.Name} deos not have enough available coins for this action.";
                result.WasSuccessful = false;
                return result;
            }

            var unidentifiedItems = hero.Inventory.Backpack.Where(i => i != null && i is Potion && !i.Identified).ToList();
            if (!unidentifiedItems.Any())
            {
                result.Message = $"{hero.Name} deos not have any potions in need of identification.";
                result.WasSuccessful = false;
                return result;
            }

            var selectedItem = unidentifiedItems.First();
            if (unidentifiedItems.Count > 1)
            {
                var choiceResult = await userRequest.RequestChoiceAsync("Choose potion to identify.", unidentifiedItems, item => item != null ? item.Name : "Unknown");
                await Task.Yield();
                selectedItem = choiceResult.SelectedOption;
            }

            if (selectedItem != null)
            {
                selectedItem.Identified = true;
                result.AvailableCoins -= IdentifyPotionPrice;
                result.Message = $"Potion identified: {selectedItem.ToString()}";
            }
            return result;
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

        public async Task<SettlementActionResult> IdentifyPotion(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            return await GeneralStore.IdentifyPotion(hero, result, userRequest);
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

        public async Task<SettlementActionResult> VisitSickWard(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            var poison = hero.ActiveStatusEffects.FirstOrDefault(a => a.Category == Combat.StatusEffectType.Poisoned);
            var disease = hero.ActiveStatusEffects.FirstOrDefault(a => a.Category == Combat.StatusEffectType.Diseased);
            if (poison == null && disease == null)
            {
                result.Message = $"{hero.Name} is neither poisoned nor diseased.";
                result.WasSuccessful = false;
                return result;
            }
            if (poison != null && result.AvailableCoins >= 100 && await userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wnat to be cured of poison for 100c?"))
            {
                StatusEffectService.RemoveActiveStatusEffect(hero, poison);
                result.AvailableCoins -= 100;
                result.Message += $"{hero.Name} was cured of poison!";
            }
            await Task.Yield();
            if (disease != null && result.AvailableCoins >= 100 && await userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wnat to be cured of disease for 100c?"))
            {
                StatusEffectService.RemoveActiveStatusEffect(hero, disease);
                result.AvailableCoins -= 100;
                result.Message += $"{hero.Name} was cured of disease!";
            }
            await Task.Yield();

            return result;
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
        public int Price { get; set; } = 50;
        public string FortuneDice { get; set; } = "1d6";
        public FortuneTeller(Settlement settlement) : base(SettlementServiceName.FortuneTeller, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.ReadFortune,
                    };
        }

        public async Task<SettlementActionResult> ReadFortune(Hero hero, SettlementActionResult result, UserRequestService userRequest, PowerActivationService powerActivation)
        {
            if (result.AvailableCoins < Price)
            {
                result.Message = $"{hero.Name} does not have enough available coins for this action.";
                result.WasSuccessful = false;
                return result;
            }

            result.AvailableCoins -= Price;
            var rollResult = await userRequest.RequestRollAsync("Roll for your fortune", FortuneDice);
            await Task.Yield();
            switch (rollResult.Roll)
            {
                case 1:
                    result.Message = "The Fortune Teller describes an upcoming battle in such detail that during the next quest, the hero recognizes the situation and manages to avoid harm." +
                        " The hero may treat one successful attack against them as a miss during the next quest.";
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, new ActiveStatusEffect(StatusEffectType.Precognition, -1), powerActivation);
                    break;
                case 2:
                    result.Message = "The Fortune Teller talks about great fortune being made through gambling. The hero has enhanced luck at a gambling dice roll during this stay in the city.";
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, new ActiveStatusEffect(StatusEffectType.GamblingLuck, -1), powerActivation);
                    break;
                case 6:
                    result.Message = "You are cursed! The Fortune Teller staggers back in shock after reading the hero's palm. The hero will suffer a curse during the next quest.";
                    var curseEffect = StatusEffectService.GetRandomCurseEffect();
                    curseEffect.RemoveEndOfDungeon = true;
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, curseEffect, powerActivation);
                    break;
                default: result.Message = "The Fortune Teller talks about lots of things, nut nothing that is of any importance."; break;
            }
            return result;
        }
    }

    public class Scryer : ServiceLocation
    {
        public static int PriceToIdentify { get; set; } = 300;
        public Scryer(Settlement settlement) : base(SettlementServiceName.Scryer, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.IdentifyMagicItem,
                    };
        }

        public static async Task<SettlementActionResult> IdentifyMagicItem(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            if (result.AvailableCoins < PriceToIdentify)
            {
                result.Message = $"{hero.Name} deos not have enough available coins for this action.";
                result.WasSuccessful = false;
                return result;
            }

            var unidentifiedItems = hero.Inventory.Backpack.Where(i => i != null && i is not Potion && !i.Identified).ToList();
            if (!unidentifiedItems.Any())
            {
                result.Message = $"{hero.Name} deos not have any non-potion items in need of identification.";
                result.WasSuccessful = false;
                return result;
            }

            var selectedItem = unidentifiedItems.First();
            if (unidentifiedItems.Count > 1)
            {
                var choiceResult = await userRequest.RequestChoiceAsync("Choose potion to identify.", unidentifiedItems, item => item != null ? item.Name : "Unknown");
                await Task.Yield();
                selectedItem = choiceResult.SelectedOption;
            }

            if (selectedItem != null)
            {
                selectedItem.Identified = true;
                result.AvailableCoins -= PriceToIdentify;
                result.Message = $"Item identified: {selectedItem.ToString()}";
            }
            return result;
        }
    }

    public class TheAsylum : ServiceLocation
    {
        public int Price { get; set; } = 1000;

        public TheAsylum(Settlement settlement) : base(SettlementServiceName.TheAsylum, settlement)
        {
            Settlement = settlement;

            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.TreatMentalConditions,
                    };
        }

        public async Task<SettlementActionResult> TreatMentalConditions(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            var curableConditions = hero.ActiveStatusEffects.Where(e =>
                e.Category == StatusEffectType.PostTraumaticStressDisorder ||
                e.Category == StatusEffectType.FearDark ||
                e.Category == StatusEffectType.Arachnophobia ||
                e.Category == StatusEffectType.Jumpy ||
                e.Category == StatusEffectType.IrrationalFear ||
                e.Category == StatusEffectType.Claustrophobia ||
                e.Category == StatusEffectType.Depression).ToList();

            if (!curableConditions.Any())
            {
                result.Message = $"{hero.Name} has no mental conditions that can be treated at the Asylum.";
                result.WasSuccessful = false;
                return result;
            }

            if (result.AvailableCoins < Price)
            {
                result.Message = "You do not have enough coins for treatment.";
                result.WasSuccessful = false;
                return result;
            }

            var choiceResult = await userRequest.RequestChoiceAsync("Choose a condition to treat:", curableConditions, condition => condition.Category.ToString(), canCancel: true);

            if (choiceResult.WasCancelled)
            {
                result.Message = "Treatment cancelled.";
                result.WasSuccessful = false;
                return result;
            }

            var chosenCondition = choiceResult.SelectedOption;
            if (chosenCondition == null)
            {
                result.Message = "Invalid selection.";
                result.WasSuccessful = false;
                return result;
            }

            var confirmation = await userRequest.RequestYesNoChoiceAsync($"Treating {chosenCondition.Category} will cost 1000 coins and take 5 days. Continue?");
            if (!confirmation)
            {
                result.Message = "Treatment cancelled.";
                result.WasSuccessful = false;
                return result;
            }

            result.AvailableCoins -= Price;

            var rollResult = await userRequest.RequestRollAsync("Roll to determine treatment success (1-5 succeeds)", "1d6");
            if (rollResult.Roll <= 5)
            {
                hero.ActiveStatusEffects.Remove(chosenCondition);
                result.Message = $"Treatment was successful! {hero.Name} is no longer suffering from {chosenCondition.Category}.";
                result.WasSuccessful = true;
            }
            else
            {
                result.Message = "The treatment was unsuccessful, and the condition remains.";
                result.WasSuccessful = false;
            }

            return result;
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

        public async Task<SettlementActionResult> ArenaFighting(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            if (result.AvailableCoins < MinimumEntryFee)
            {
                result.Message = $"{hero.Name} does not have enough coin to participate";
                result.WasSuccessful = false;
                return result;
            }

            var inputResult = await userRequest.RequestNumberInputAsync("How much fo you want to bet", min: MinimumEntryFee, max: Math.Min(MaxBet, result.AvailableCoins), canCancel: true);
            if (!inputResult.WasCancelled)
            {
                var bet = inputResult.Amount;
                Bet = bet;
                while (!IsComplete)
                {
                    var rollRequest = await userRequest.RequestRollAsync($"Roll combat skill to compete in bout: {Bout}", "1d100");
                    await StartBoutAsync(rollRequest.Roll, hero);
                }
                result.ArenaWinnings = Winnings;
                result.FoundItems = ExtraAward;
                result.Message = Message;
                hero.Party.Coins += Winnings;
                hero.GainExperience(Experience);
            }
            return result;
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

        public async Task<SettlementActionResult> Banking(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            bool isBanking = true;
            while (isBanking)
            {
                var actionChoice = await userRequest.RequestChoiceAsync(
                    $"Your account has {await CheckBalanceAsync()} available coins. What would you like to do?", 
                    new List<string> { "Deposit coins.", "Withdraw coins." },
                    action => action,
                    canCancel: true);
                await Task.Yield();
                if (!actionChoice.WasCancelled)
                {
                    switch (actionChoice.SelectedOption)
                    {
                        case "Deposit coins.":
                            var depositResult = await userRequest.RequestNumberInputAsync("How much would you like to deposit?", min: 0, canCancel: true);
                            if (!depositResult.WasCancelled)
                            {
                                if (result.AvailableCoins >= depositResult.Amount)
                                {
                                    result.Message += $"{depositResult.Amount} was deposited at {Name.ToString()}. The new balance is {await DepositAsync(depositResult.Amount)}";
                                    result.AvailableCoins -= depositResult.Amount;
                                }
                            }
                            break;
                        case "Withdraw coins.":
                            var withdrawResult = await userRequest.RequestNumberInputAsync("How much would you like to withdraw?", min: 0, max: AccountBalance, canCancel: true);
                            if (!withdrawResult.WasCancelled)
                            {
                                var amountWithdrawn = await WithdrawAsync(withdrawResult.Amount);
                                result.Message += $"{amountWithdrawn} was withdrawn from {Name.ToString()}. The new balance is {await CheckBalanceAsync()}";
                                result.AvailableCoins += amountWithdrawn;
                            }
                            break;
                    }
                }
                else isBanking = false;
            }
            return result;
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
        public int RollForBoon { get; set; } = 3;
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

        public async Task<SettlementActionResult> Pray(Hero hero, SettlementActionResult result, UserRequestService userRequest, PowerActivationService powerActivation, bool isShrine = false)
        {
            if (isShrine) RollForBoon = 4;
            if (hero.ProfessionName == ProfessionName.WarriorPriest) RollForBoon = 5;
            if (hero.ActiveStatusEffects.Where(e => e.Category.ToString().Contains("Blessing")).Any())
            {
                result.Message = $"{hero.Name} has already offered prayers during this visit.";
                result.WasSuccessful = false;
                return result;
            }

            if (result.AvailableCoins >= CostToPray)
            {
                var rollResult = await userRequest.RequestRollAsync($"Roll to see if {GodName.ToString()} listens.", DiceToPray);
                await Task.Yield();
                if (rollResult.Roll <= RollForBoon && GrantedEffect != null)
                {
                    result.Message = $"{GodName.ToString()} hears your prayer and decides to grant you a boon.";
                    if (GodName == GodName.Ohlnir)
                    {
                        var skillChoiceRequest = await userRequest.RequestChoiceAsync(
                            "Which skill do you want Ohlnir to enhance?", 
                            new List<string>() { "Combat", "Ranged" },
                            x => x);
                        await Task.Yield();
                        switch (skillChoiceRequest.SelectedOption)
                        {
                            case "Combat": GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true); break;
                            case "Ranged": GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.RangedSkill, 5), removeEndOfDungeon: true); break;
                        }
                    }
                    result.AvailableCoins -= CostToPray;
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, GrantedEffect, powerActivation);
                }
                else
                {
                    result.Message = $"You pray, but {GodName.ToString()} remains silent.";
                }
            }
            else
            {
                result.Message = "You do not have enough coins to make an offering.";
                result.WasSuccessful = false;
            }
            return result;
        }
    }

    public class Guild : ServiceLocation
    {
        public List<ProfessionName> AllowedToEnter { get; set; } = new();
        public List<(Skill, int)> AvailableSkillTraining { get; set; } = new();
        public int SkillTrainingFee { get; set; }
        public Dictionary<Hero, int> QuestsBeforeNextTraining { get; set; } = new();

        public Guild(SettlementServiceName name, Settlement settlement) : base(name, settlement)
        {
            Settlement = settlement;
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
                if(skillToTrain.Item1 == skill)
                {
                    hero.SetSkill(skill, hero.GetSkill(skill) + 3);
                }
                else return false;
                
                QuestsBeforeNextTraining[hero] = 2;
            }
            return false;
        }

        public SettlementActionResult Train(Hero hero, SettlementActionResult result, Skill skillToTrain)
        {
            if (result.AvailableCoins >= SkillTrainingFee)
            {
                if (AttemptToTrainHeroSkill(hero, skillToTrain))
                {
                    result.Message = $"{hero.Name} successfully trained {skillToTrain.ToString()} adding 3 to its value.";
                    result.AvailableCoins -= SkillTrainingFee;
                }
                else
                {
                    result.Message = $"{hero.Name} failed to train {skillToTrain.ToString()}.";
                    result.WasSuccessful = false;
                }
            }
            else
            {
                result.Message = $"{hero.Name} does not have enough coin to train {skillToTrain.ToString()}.";
                result.WasSuccessful = false;
            }
            return result;
        }
    }

    public class TheDarkGuild : Guild
    {
        public TheDarkGuild(Settlement settlement) : base(SettlementServiceName.TheDarkGuild, settlement)
        {
            Settlement = settlement;
            AllowedToEnter = new() { ProfessionName.Thief, ProfessionName.Rogue };
            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.CombatSkillTraining,
                        SettlementActionType.RangedSkillTraining,
                        SettlementActionType.PickLocksTraining,
                        SettlementActionType.PerceptionTraining,
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.CombatSkill, 3), (Skill.RangedSkill, 3), (Skill.PickLocks, 3), (Skill.Perception, 3) };
            SkillTrainingFee = 300;
        }
    }

    public class Bounty
    {
        public string BountyName { get; set; }
        public Monster Monster { get; private set; }
        public int Value { get; private set; }
        public int TargetAmount { get; private set; }
        public int AmountKilled { get; set; }

        public Bounty(Monster monster)
        {
            Monster = monster;
            SetTargetAndValue();
            BountyName = $"Kill {TargetAmount} {Monster.Name}";
        }

        private void SetTargetAndValue()
        {
            Value = Monster.XP;
            while (TargetAmount * Value < 250)
            {
                TargetAmount++;
            }
            Value *= TargetAmount;
        }
    }

    public class FightersGuild : Guild
    {
        public EncounterService Encounter { get; set; }
        public List<Bounty>? WantedBounties { get; set; }
        public bool BountiesHaveBeenChecked { get; set; }
        public int ShieldPaddingPrice { get; set; } = 15;
        public int ArmourPaddingPricePerArea { get; set; } = 50;
        public int SlayerTreatmentPrice { get; set; } = 100;


        public override void RefreshStockForNewVisit()
        {
            base.RefreshStockForNewVisit();
            // Reset the stock, so it will be regenerated on the next access.
            BountiesHaveBeenChecked = false;
        }
        public FightersGuild(Settlement settlement, EncounterService encounter) : base(SettlementServiceName.FightersGuild, settlement)
        {
            Settlement = settlement;
            Encounter = encounter;
            AllowedToEnter = new() { ProfessionName.Warrior, ProfessionName.Barbarian };
            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.CheckBounties,
                        SettlementActionType.CombatSkillTraining,
                        SettlementActionType.HealTraining,
                        SettlementActionType.DodgeTraining,
                        SettlementActionType.AddShieldPadding,
                        SettlementActionType.AddArmourPadding,
                        SettlementActionType.ApplySlayerWeaponTreatment,
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.CombatSkill, 3), (Skill.Heal, 3), (Skill.Dodge, 3) };
            SkillTrainingFee = 300;
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

        public SettlementActionResult CheckBounties(Hero hero, SettlementActionResult result)
        {
            foreach (var bounty in hero.Party.FightersGuildBounties)
            {
                if (bounty.TargetAmount <= bounty.AmountKilled)
                {
                    result.AvailableCoins += bounty.Value;
                    result.Message = $"The heroes completed the bounty hunt: {bounty.BountyName} for {bounty.Value}c.";
                    WantedBounties = null;
                }
            }

            if (!BountiesHaveBeenChecked || WantedBounties == null) WantedBounties = GetFightersGuildBountyHuntList(Encounter);
            hero.Party.FightersGuildBounties = WantedBounties;
            BountiesHaveBeenChecked = true;

            return result;
        }

        public async Task<SettlementActionResult> AddShieldPadding(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            bool isShopping = true;
            while (isShopping)
            {
                var shields = hero.Inventory.GetAllWeaponsArmour()
                    .OfType<Shield>()
                    .Where(item => item is Shield shield && !shield.HasShieldPadding && ShieldPaddingPrice < result.AvailableCoins)
                    .ToList();

                if (!shields.Any())
                {
                    result.Message += "You have no shields eligible for padding.";
                    break;
                }

                var choiceResult = await userRequest.RequestChoiceAsync(
                    "Which shield will be padded?",
                    shields,
                    shield => $"{shield.Name} MaxDurability: {shield.MaxDurability} > {shield.MaxDurability + 1} Encumbrance: {shield.Encumbrance} > {shield.Encumbrance + 1} Cost: {ShieldPaddingPrice}c",
                    canCancel: true);
                await Task.Yield();

                if (choiceResult.WasCancelled || choiceResult.SelectedOption == null)
                {
                    result.Message += "Shield padding was cancelled.";
                    break;
                }

                var shield = choiceResult.SelectedOption;
                if (shield.AttemptToApplyShieldPadding())
                {
                    result.Message += $"Shield padding was successfully applied to {shield.Name}.";
                    result.AvailableCoins -= ShieldPaddingPrice;
                }
                else
                {
                    result.Message += $"Padding {shield.Name} was unsuccessful.";
                    result.WasSuccessful = false;
                } 
            }
            return result;
        }

        public async Task<SettlementActionResult> AddArmourPadding(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            bool isShopping = true;
            while (isShopping)
            {
                var armourList = hero.Inventory.GetAllWeaponsArmour()
                    .OfType<Armour>()
                    .Where(item => item is Armour armour && !armour.HasArmourPadding && armour.ArmourClass < 4 && armour.CoverageCount * ArmourPaddingPricePerArea < result.AvailableCoins)
                    .ToList();

                if (!armourList.Any())
                {
                    result.Message += "You have no armour eligible for padding.";
                    break;
                }

                var choiceResult = await userRequest.RequestChoiceAsync(
                    "Which armour will be padded?",
                    armourList,
                    armour => $"{armour.Name} DefValue: {armour.DefValue} > {armour.DefValue + 1} Encumbrance: {armour.Encumbrance} > {armour.Encumbrance + armour.CoverageCount} Cost: {armour.CoverageCount * ArmourPaddingPricePerArea}c",
                    canCancel: true);
                await Task.Yield();

                if (choiceResult.WasCancelled || choiceResult.SelectedOption == null)
                {
                    result.Message += "Armour padding was cancelled.";
                    break;
                }

                var selectedArmour = choiceResult.SelectedOption;
                if (selectedArmour.AttemptToApplyArmourPadding())
                {
                    result.Message += $"Armour padding was successfully applied to {selectedArmour.Name}.";
                    result.AvailableCoins -= selectedArmour.CoverageCount * ArmourPaddingPricePerArea;
                }
                else
                {
                    result.Message += $"Padding {selectedArmour.Name} was unsuccessful.";
                    result.WasSuccessful = false;
                } 
            }
            return result;
        }

        public async Task<SettlementActionResult> ApplySlayerWeaponTreatment(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            bool isShopping = true;
            while (isShopping)
            {
                var eligibleWeapons = hero.Inventory.GetAllWeaponsArmour()
                    .OfType<MeleeWeapon>()
                    .Where(i => i is MeleeWeapon weapon && !weapon.HasSlayerModifier && 
                                     weapon.HasProperty(WeaponProperty.Edged) &&
                                     weapon.Durability == weapon.MaxDurability &&
                                     SlayerTreatmentPrice <= result.AvailableCoins)
                    .ToList();

                if (!eligibleWeapons.Any())
                {
                    result.Message += "You have no edged weapons at full durability eligible for the Slayer Treatment.";
                    break;
                }

                var choiceResult = await userRequest.RequestChoiceAsync(
                    "Which weapon will you treat?",
                    eligibleWeapons,
                    weapon => $"{weapon.Name} | Damage Bonus: +1 | Cost: {SlayerTreatmentPrice}c",
                    canCancel: true);
                await Task.Yield();

                if (choiceResult.WasCancelled || choiceResult.SelectedOption == null)
                {
                    result.Message += "Slayer Weapon Treatment was cancelled.";
                    break;
                }

                var weaponToTreat = choiceResult.SelectedOption;
                if (weaponToTreat.AttemptApplySlayerTreatment())
                {
                    result.Message += $"Slayer treatment was successfully applied to {weaponToTreat.Name}.";
                    result.AvailableCoins -= SlayerTreatmentPrice;
                }
                else
                {
                    result.Message += $"Applying slayer treatment to {weaponToTreat.Name} was unsuccessful.";
                    result.WasSuccessful = false;
                }
            }
            return result;
        }
    }

    public class WizardsGuild : Guild
    {
        public int LearnSpellBasePrice { get; set; } = 400;
        public int PricePerSpellLevel { get; set; } = 100;

        public WizardsGuild(Settlement settlement) : base(SettlementServiceName.WizardsGuild, settlement)
        {
            Settlement = settlement;
            AllowedToEnter = new() { ProfessionName.Wizard };
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

        public async Task<SettlementActionResult> IdentifyMagicItem(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            return await Scryer.IdentifyMagicItem(hero, result, userRequest);
        }

        public async Task<SettlementActionResult> ChargeMagicItem(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            var magicStaves = hero.Inventory.GetAllWeaponsArmour().Where(i => i is MagicStaff staff && staff.ContainedSpell != null).ToList();
            if (!magicStaves.Any())
            {
                result.Message = $"{hero.Name} does not have any magic staves that need charging";
                result.WasSuccessful = false;
                return result;
            }

            var affordableCharging = magicStaves.Where(i => i is MagicStaff staff && staff.RechargeCost < result.AvailableCoins).ToList();
            if (!affordableCharging.Any())
            {
                result.Message = $"{hero.Name} does not have enough coin to charge any staves";
                result.WasSuccessful = false;
                return result;
            }

            var choiceResult = await userRequest.RequestChoiceAsync(
                "Choose a staff to charge", 
                affordableCharging, 
                staff =>  $"{staff.Name} {((MagicStaff)staff).RechargeCost}c", 
                canCancel: true);

            if (!choiceResult.WasCancelled && choiceResult.SelectedOption != null)
            {
                MagicStaff selectedStaff = (MagicStaff)choiceResult.SelectedOption;
                result.AvailableCoins -= selectedStaff.RechargeCost;
                selectedStaff.CurrentSpellCharges = selectedStaff.MaxSpellCharges;
                result.Message = $"{selectedStaff.Name} was recharged.";
            }

            return result;
        }

        public async Task<SettlementActionResult> LearnSpell(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            var grimoires = hero.Inventory.Backpack.Where(i => i != null && i.Name.Contains("Grimoire")).ToList();
            bool learnFromGrimoire = false;
            if (grimoires.Any())
            {
                var yesNoResult = await userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to learn a spell from an owned Grimoire?");
                await Task.Yield();
                learnFromGrimoire = yesNoResult;
                if (learnFromGrimoire)
                {
                    var selectedGrimoire = grimoires.FirstOrDefault();
                    if (grimoires.Count > 1)
                    {
                        var choiceResult = await userRequest.RequestChoiceAsync("Choose which grimoire to learn from.", grimoires, g => g != null ? g.Name : string.Empty);
                        await Task.Yield();
                        selectedGrimoire = choiceResult.SelectedOption;
                    }
                    if (selectedGrimoire != null)
                    {
                        var spellName = selectedGrimoire.Name.Replace("Grimoire of ", "");
                        var spell = SpellService.GetSpellByName(spellName);
                        if (hero.Spells != null && !hero.Spells.Contains(spell))
                        {
                            hero.Spells.Add(spell);
                            result.Message = $"{hero.Name} now knows the spell: {spell.ToString()}.";
                            return result;
                        }
                        else
                        {
                            result.Message = $"{hero.Name} already knows that spell.";
                            result.WasSuccessful = false;
                            return result;
                        }
                    }
                    else
                    {
                        learnFromGrimoire = false;
                    }
                }
            }

            if (hero.Spells != null)
            {
                var knownSpells = hero.Spells;
                var affordableSpells = SpellService.Spells
                    .Where(spell => spell.Level <= hero.Level && !knownSpells.Contains(spell) && (spell.Level * PricePerSpellLevel + LearnSpellBasePrice) <= result.AvailableCoins)
                    .OrderBy(spell => spell.Level)
                    .ToList();

                var choiceSpellResult = await userRequest.RequestChoiceAsync(
                    "Choose as spell to learn.", 
                    affordableSpells, 
                    spell => $"{(spell.Level * PricePerSpellLevel + LearnSpellBasePrice)}c {spell.Name}, Effect: {spell.SpellEffect}", 
                    canCancel: true);
                await Task.Yield();
                var spellChoice = choiceSpellResult.SelectedOption;
                if (!choiceSpellResult.WasCancelled && spellChoice != null)
                {
                    result.AvailableCoins -= (spellChoice.Level * PricePerSpellLevel + LearnSpellBasePrice);
                    hero.Spells.Add(spellChoice);
                    result.Message = $"{hero.Name} now knows the spell: {spellChoice.ToString()}.";
                    return result;
                }
            }
            return result;
        }
    }

    public class AlchemistsGuild : Guild
    {
        public AlchemistsGuild(Settlement settlement) : base(SettlementServiceName.AlchemistGuild, settlement)
        {
            Settlement = settlement;
            AllowedToEnter = new() { ProfessionName.Alchemist };
            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.IdentifyPotion
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.Alchemy, 3), (Skill.Heal, 3), (Skill.Perception, 3) };
            SkillTrainingFee = 300;
        }

        public async Task<SettlementActionResult> IdentifyPotion(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            return await GeneralStore.IdentifyPotion(hero, result, userRequest);
        }
    }

    public class RangersGuild : Guild
    {
        public RangersGuild(Settlement settlement) : base(SettlementServiceName.RangersGuild, settlement)
        {
            Settlement = settlement;
            AllowedToEnter = new() { ProfessionName.Ranger };
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
        public EncounterType EncounterType { get; set; }
        public int AwardPerKill { get; set; } = 25;
        public int AmountKilled { get; set; }
        public int TotalAward => AmountKilled * AwardPerKill;
        
        public Crusade(EncounterType encounter)
        {
            EncounterType = encounter;
        }
    }

    public class TheInnerSanctum : Guild
    {
        public int LearnPrayerBasePrice { get; set; } = 400;
        public int PricePerPrayerLevel { get; set; } = 100;
        public int BlessArmourPrice { get; set; } = 25;
        public int BlessWeaponPrice { get; set; } = 75;
        public EncounterService Encounter { get; set; }
        public EncounterType? Crusade { get; set; }

        public TheInnerSanctum(Settlement settlement, EncounterService encounter) : base(SettlementServiceName.TheInnerSanctum, settlement)
        {
            Settlement = settlement;
            Encounter = encounter;
            AllowedToEnter = new() { ProfessionName.WarriorPriest };
            AvailableActions = new List<SettlementActionType>
                    {
                        SettlementActionType.BuyingAndSelling,
                        SettlementActionType.LearnPrayer,
                        SettlementActionType.BlessArmourAndWeapons,
                        SettlementActionType.StartCrusade
                    };
            AvailableSkillTraining = new List<(Skill, int)> { (Skill.CombatSkill, 3), (Skill.Dodge, 3), (Skill.BattlePrayers, 3) };
            SkillTrainingFee = 300;
        }

        public async Task<SettlementActionResult> LearnPrayer(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            if (hero.Prayers != null)
            {
                var knownPrayers = hero.Prayers;
                var prayerList = PrayerService.Prayers
                        .Where(prayer => !knownPrayers.Contains(prayer) && prayer.Level <= hero.Level && (prayer.Level * PricePerPrayerLevel + LearnPrayerBasePrice) <= result.AvailableCoins)
                        .OrderBy(p => p.Level)
                    .ToList();

                var choicePrayerResult = await userRequest.RequestChoiceAsync(
                    "Choose as prayer to learn.", 
                    prayerList, 
                    prayer => $"{prayer.Name}, Effect: {prayer.PrayerEffect}", 
                    canCancel: true);
                await Task.Yield();
                var prayerChoice = choicePrayerResult.SelectedOption;
                if (!choicePrayerResult.WasCancelled && prayerChoice != null)
                {
                    result.AvailableCoins -= prayerChoice.Level * PricePerPrayerLevel + LearnPrayerBasePrice;
                    hero.Prayers.Add(prayerChoice);
                    result.Message = $"{hero.Name} now knows the prayer: {prayerChoice.ToString()}.";
                    return result;
                }
            }
            return result;
        }

        public async Task<SettlementActionResult> BlessArmourAndWeapons(Hero hero, SettlementActionResult result, UserRequestService userRequest)
        {
            bool isBlessing = true;
            while (isBlessing)
            {
                var weaponArmourList = hero.Inventory.GetAllWeaponsArmour()
                    .Where(item => item is Weapon && BlessWeaponPrice <= result.AvailableCoins || !(item is Weapon) && BlessArmourPrice <= result.AvailableCoins)
                    .Where(item => item.ActiveStatusEffects?.FirstOrDefault(e => e.Category == StatusEffectType.BlessedWeapon || e.Category == StatusEffectType.BlessedArmour) == null)
                    .ToList();

                var choiceResult = await userRequest.RequestChoiceAsync(
                    "Choose an item to be blessed until end of next dungeon. Armour and shields will negate 1 point of durability damage, weapons will receive +2 damage bonus.",
                    weaponArmourList,
                    item => item is Weapon ? $"{item.Name} {BlessWeaponPrice}c" : $"{item.Name} {BlessArmourPrice}c", 
                    canCancel: true);
                await Task.Yield();

                if (choiceResult.WasCancelled)
                {
                    isBlessing = false;
                }
                else
                {
                    var itemToBless = choiceResult.SelectedOption;
                    if (itemToBless != null)
                    {
                        if (itemToBless is Weapon weapon)
                        {
                            weapon?.ActiveStatusEffects?.Add(new ActiveStatusEffect(StatusEffectType.BlessedWeapon, -1, removeEndOfDungeon: true));
                            result.AvailableCoins -= BlessWeaponPrice;
                        }
                        else
                        {
                            itemToBless.ActiveStatusEffects?.Add(new ActiveStatusEffect(StatusEffectType.BlessedArmour, -1, removeEndOfDungeon: true));
                            result.AvailableCoins -= BlessArmourPrice;
                        }
                        result.Message += $"{itemToBless.Name} has been blessed!";
                    }
                }
            }
            return result;
        }

        public SettlementActionResult StartCrusade(Hero hero, SettlementActionResult result)
        {
            if (hero.Party.InnerSenctumCrusade != null)
            {
                result.AvailableCoins += hero.Party.InnerSenctumCrusade.TotalAward;
                hero.Party.InnerSenctumCrusade = null;
            }

            var roll = RandomHelper.RollDie(DiceType.D6);
            var crusade = new Crusade(EncounterType.Undead);
            switch (roll)
            {
                case 1: crusade = new Crusade(EncounterType.Undead); break;
                case 2: crusade = new Crusade(EncounterType.Bandits_Brigands); break;
                case 3: crusade = new Crusade(EncounterType.Orcs_Goblins); break;
                case 4: crusade = new Crusade(EncounterType.Beasts); break;
                case 5: crusade = new Crusade(EncounterType.DarkElves); break;
                case 6: crusade = new Crusade(EncounterType.Reptiles); break;
            }
            hero.Party.InnerSenctumCrusade = crusade;

            result.Message = $"{hero.Name} started a new crusade against {crusade.EncounterType.ToString()}";
            return result;
        }
    }

    public enum EstateRoomName
    {
        AlchemistLab,
        ArcheryRange,
        CropsHenHouseAndPigsty,
        Kennel,
        TrainingGrounds,
        WizardsStudy,
        Shrine,
        Smithy
    }

    public class EstateFurnishing
    {
        public EstateRoomName Name { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsOwned { get; set; }
        public int DungeonsBetweeUses { get; set; }
        public int DungeonsUntilUsable { get; set; }
    }

    public class AlchemistLab : EstateFurnishing
    {
        public AlchemistLab()
        {
            Name = EstateRoomName.AlchemistLab;
            Cost = 500;
            Description = "This room is specially tailored to fit every need of an alchemist. A single Recipe can be made here between dungeons.";
            DungeonsBetweeUses = 1;
        }
    }

    public class ArcheryRange : EstateFurnishing
    {
        public ArcheryRange()
        {
            Name = EstateRoomName.ArcheryRange;
            Cost = 500;
            Description = "With a fully-fledged archery range, heroes staying at the manor may increase their Ranged Skill.";
            DungeonsBetweeUses = 1;
        }

        public SettlementActionResult Train(Hero hero, SettlementActionResult result)
        {
            int trainedAmount = RandomHelper.RollDie(DiceType.D2);
            hero.SetSkill(Skill.RangedSkill, trainedAmount);
            result.Message = $"{hero.Name} trainedthe whole day and improved their RangedSkill by {trainedAmount}.";
            DungeonsUntilUsable = DungeonsBetweeUses;
            return result;
        }
    }

    public class Farm : EstateFurnishing
    {
        public Farm()
        {
            Name = EstateRoomName.CropsHenHouseAndPigsty;
            Cost = 200;
            Description = "As long as one of the heroes spends at least a full day tending to the farm, the party will receive a number of rations for free.";
        }

        public async Task<SettlementActionResult> TendFarmAsync(Hero hero, SettlementActionResult result)
        {
            var rations = RandomHelper.RollDie(DiceType.D8);
            var ration = EquipmentService.GetEquipmentByNameSetQuantity("Ration", rations);
            await BackpackHelper.AddItem(hero.Inventory.Backpack, ration);

            result.Message = $"{hero.Name} tended the farm and received {rations} rations.";
            return result;
        }
    }

    public class TrainingGrounds : EstateFurnishing
    {
        public TrainingGrounds()
        {
            Name = EstateRoomName.TrainingGrounds;
            Cost = 500;
            Description = "Any hero who spends time at the training grounds may choose to increase either their Combat Skill or Dodge.";
            DungeonsBetweeUses = 1;
        }

        public SettlementActionResult Train(Hero hero, SettlementActionResult result, Skill skillToTrain)
        {
            int trainedAmount = RandomHelper.RollDie(DiceType.D2);
            hero.SetSkill(skillToTrain, trainedAmount);
            result.Message = $"{hero.Name} trainedthe whole day and improved their {skillToTrain} by {trainedAmount}.";
            DungeonsUntilUsable = DungeonsBetweeUses;
            return result;
        }
    }

    public class Estate : ServiceLocation
    {
        public bool IsOwned { get; set; }
        public List<EstateFurnishing> FurnishedRooms { get; set; }

        public Estate(Settlement settlement) : base(SettlementServiceName.Estate, settlement)
        {
            FurnishedRooms = GetFurnishings();
        }

        public List<EstateFurnishing> GetFurnishings()
        {
            return new()
            {
                new AlchemistLab(),
                new ArcheryRange(),
                new Farm(),
                new EstateFurnishing()
                {
                    Name = EstateRoomName.Kennel,
                    Cost = 75,
                    Description = "With a kennel, you can leave any dogs you own between quests, if you decide not to bring them."
                },
                new TrainingGrounds(),
                new EstateFurnishing()
                {
                    Name = EstateRoomName.WizardsStudy,
                    Cost = 500,
                    Description = "This room is equipped with everything a wizard could possibly need."
                },
                new EstateFurnishing()
                {
                    Name = EstateRoomName.Shrine,
                    Cost = 350,
                    Description = "A room can be turned into a dedicated shrine for a god of the hero's choice. Only one shrine can be built."
                },
                new EstateFurnishing()
                {
                    Name = EstateRoomName.Smithy,
                    Cost = 350,
                    Description = "An old smithy on the estate can be turned into a fully functioning workshop."
                }
            };
        }

        public void GhostlyEvent()
        {
            var roll = RandomHelper.RollDie(DiceType.D10);

            switch (roll)
            {

            };
        }
    }

    public class SettlementService
    {
        private readonly UserRequestService _userRequest;
        private readonly QuestService _quest;
        private readonly PartyManagerService _partyManager;
        private readonly GameDataService _gameData;
        private readonly EncounterService _encounter;
        private readonly GameState _gameState;

        public List<Settlement> Settlements => _gameState.Settlements == null ? _gameState.Settlements = GetSettlements() : _gameState.Settlements;

        public SettlementService(
            UserRequestService userRequestService, 
            QuestService questService,
            PartyManagerService partyManager,
            GameDataService gameData,
            EncounterService encounter,
            GameState gameState)
        {
            _userRequest = userRequestService;
            _quest = questService;
            _partyManager = partyManager;
            _gameData = gameData;
            _encounter = encounter;

            _gameState = gameState;
            _gameState.Settlements = GetSettlements();
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
            settlement.State.Estate = new(settlement);
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
            settlement.State.TheDarkGuild = new(settlement);
            settlement.State.FightersGuild = new (settlement, _encounter);
            settlement.State.WizardsGuild = new (settlement);
            settlement.State.AlchemistsGuild = new (settlement);
            settlement.State.RangersGuild = new (settlement);
            settlement.State.TheInnerSanctum = new (settlement, _encounter);
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
