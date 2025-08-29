using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoDCompanion.BackEnd.Services.Player
{
    public enum SettlementActionType
    {
        VisitArena,
        VisitBanks,
        VisitKennel,
        VisitAlbertasMagnificentAnimals,
        VisitBlacksmith,
        VisitGeneralStore,
        VisitHerbalist,
        VisitMagicBrewery,
        VisitTheDarkGuild,
        VisitFightersGuild,
        VisitWizardsGuild,
        VisitAlchemistGuild,
        VisitRangersGuild,
        VisistInnerSanctum,
        ChargeMagicItem,
        CreateScroll,
        VisitSickWard,
        CurePoison,
        EnchantObjects,
        Gamble,
        HorseRacing,
        IdentifyMagicItem,
        IdentifyPotion,
        LearnPrayer,
        LearnSpell,
        Pray,
        ReadFortune,
        RestRecuperation,
        SkillTraining,
        TendThoseMemories,
        TreatMentalConditions,
        CollectQuestRewards,
        LevelUp,
    }

    public class SettlementActionResult
    {
        public SettlementActionType Action { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool WasSuccessful { get; set; } = true;
        public int ActionCost { get; set; }
        public int ArenaWinnings { get; set; }
        public List<Equipment>? FoundItems { get; set; }
        public List<Equipment> ShopInventory { get; set; } = new List<Equipment>();
        public int BarterRollResult { get; internal set; }
        public int BarterTarget { get; internal set; }
        public double BuyPriceModifications { get; internal set; }
        public double SellPriceModification { get; internal set; }
        public int AvailableCoins { get; internal set; }

        public SettlementActionResult (SettlementActionType action)
        {
            Action = action;
        }
    }

    public class SettlementActionService
    {
        private readonly UserRequestService _userRequest;
        private readonly TreasureService _treasure;

        public SettlementActionService(
            UserRequestService userRequestService,
            TreasureService treasureService)
        {
            _userRequest = userRequestService;
            _treasure = treasureService;
        }

        public async Task<SettlementActionResult> PerformSettlementActionAsync(Hero hero, SettlementActionType action, Settlement settlement)
        {
            var result = new SettlementActionResult(action);
            result.ActionCost = SettlementActionCost(action);

            if (settlement.State.BusyHeroes.ContainsKey(hero))
            {
                result.Message = $"{hero.Name} is busy with a multi-day task.";
                result.WasSuccessful = false;
                return result;
            }
            else if (settlement.State.HeroActionPoints[hero] <= 0)
            {
                result.Message = $"{hero.Name} does not have any action points left for today.";
                result.WasSuccessful = false;
                return result;
            }
            result.AvailableCoins = hero.Coins + hero.Party.Coins;
            hero.Coins = 0;
            hero.Party.Coins = 0;

            switch (action)
            {
                case SettlementActionType.VisitArena:                    
                    result = await ArenaFighting(hero, result);
                    break;
                case SettlementActionType.VisitBanks:
                    result = await Banking(hero, settlement, result);
                    break;
                case SettlementActionType.VisitKennel: 
                    // TODO: on implementation of the companions expansion
                    break;
                case SettlementActionType.VisitAlbertasMagnificentAnimals:
                    // TODO: on implementation of the companions expansion
                    break;
                case SettlementActionType.VisitBlacksmith:
                    result = VisitBlacksmith(hero, settlement, result);
                    break;
                case SettlementActionType.VisitGeneralStore:
                    result = VisitGeneralStore(hero, settlement, result);
                    break;
                case SettlementActionType.VisitHerbalist:
                    result = VisitHerbalist(hero, settlement, result);
                    break;
                case SettlementActionType.VisitMagicBrewery:
                    result = VisitMagicBrewery(hero, settlement, result);
                    break;
                case SettlementActionType.CollectQuestRewards:
                    result = await CollectQuestRewardsAsync(hero, settlement, result);
                    break;
                case SettlementActionType.VisitSickWard:
                    result = await VisitSickWard(hero, settlement, result);
                    break;
                case SettlementActionType.CreateScroll:
                    result = await CreateScroll(hero, settlement, result);
                    break;
                case SettlementActionType.EnchantObjects: 
                    result = await EnchantItemAsync(hero, settlement, result);
                    break;
                case SettlementActionType.Gamble: 
                    break;
                case SettlementActionType.VisitRangersGuild: 
                    break;
                case SettlementActionType.HorseRacing: 
                    break;
                case SettlementActionType.IdentifyMagicItem: 
                    break;
                case SettlementActionType.IdentifyPotion: 
                    break;
                case SettlementActionType.VisistInnerSanctum: 
                    break;
                case SettlementActionType.LearnSpell: 
                    break;
                case SettlementActionType.LevelUp: 
                    break;
                case SettlementActionType.Pray: 
                    break;
                case SettlementActionType.ReadFortune: 
                    break;
                case SettlementActionType.RestRecuperation: 
                    break;
                case SettlementActionType.SkillTraining: 
                    break;
                case SettlementActionType.TendThoseMemories: 
                    break;
                case SettlementActionType.TreatMentalConditions: 
                    break;
            }

            if (result.WasSuccessful)
            {
                settlement.State.HeroActionPoints[hero] -= result.ActionCost;
                if (settlement.State.HeroActionPoints[hero] < 0)
                {
                    settlement.State.BusyHeroes.TryAdd(hero, (result.Action, Math.Abs(settlement.State.HeroActionPoints[hero])));
                } 

                if (result.ShopInventory.Any())
                {
                    var rollResult = await _userRequest.RequestRollAsync($"{hero.Name} barters with the shop owner, roll result", "1d100", skill: (hero, Skill.Barter));
                    result.BarterRollResult = rollResult.Roll;
                    result.BarterTarget = hero.GetSkill(Skill.Barter);
                    if (result.BarterRollResult <= result.BarterTarget)
                    {
                        result.BuyPriceModifications = 0.9d;
                        result.SellPriceModification = 1.1d;
                    }
                }

                hero.Party.Coins = result.AvailableCoins;
                result.AvailableCoins = 0;
            }

            return result;
        }

        private async Task<SettlementActionResult> ArenaFighting(Hero hero, SettlementActionResult result)
        {
            if (hero.Coins < 50 || hero.Party.Coins < 50)
            {
                result.Message = $"{hero.Name} does not have enough coin to participate";
                result.WasSuccessful = false;
                return result;
            }

            var inputResult = await _userRequest.RequestNumberInputAsync("How much fo you want to bet", min: 50, max: 200);
            var bet = inputResult.Amount;
            var arena = new ArenaFight(bet, _treasure);
            while (!arena.IsComplete)
            {
                var rollRequest = await _userRequest.RequestRollAsync($"Roll combat skill to compete in bout: {arena.Bout}", "1d100");
                await arena.StartBoutAsync(rollRequest.Roll, hero);
            }
            result.ArenaWinnings = arena.Winnings;
            result.FoundItems = arena.ExtraAward;
            result.Message = arena.Message;
            hero.Party.Coins += arena.Winnings;
            hero.GainExperience(arena.Experience);
            return result;
        }

        private class ArenaFight
        {
            public enum ArenaBout
            {
                Group,
                SemiFinal,
                Final
            }

            private readonly TreasureService _treasure;
            public string Message { get; set; } = string.Empty;
            public int EntryFee { get; set; }
            public ArenaBout Bout { get; set; } = ArenaBout.Group;
            public int Winnings { get; set; }
            public int Experience { get; set; }
            public bool IsComplete { get; set; }
            public List<Equipment>? ExtraAward { get; set; }

            public ArenaFight(int entryFee, TreasureService treasureService) 
            { 
                EntryFee = entryFee;
                _treasure = treasureService;
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
                            var roll = RandomHelper.RollDie(DiceType.D10);
                            switch (roll)
                            {
                                case 1: ExtraAward = await _treasure.FoundTreasureAsync(TreasureType.Wonderful, 1); break;
                                case <= 4: ExtraAward = await _treasure.FoundTreasureAsync(TreasureType.Fine, 1); break;
                                default: break;
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
                        var extraAwardString = string.Empty;
                        foreach (var extraAward in ExtraAward)
                        {
                            if(!string.IsNullOrEmpty(extraAwardString)) extraAwardString += ", ";
                            extraAwardString += $"{extraAward.Name}";
                        }
                        Message += $"The hero also received an extra award of {extraAwardString}";
                    }
                }
            }

            private int ArenaExperience(ArenaBout bout)
            {
                switch(bout)
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
                return (int)Math.Floor(EntryFee * multiplier);
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
                    case ArenaFight.ArenaBout.Group: modifier -= 10; break;
                    case ArenaFight.ArenaBout.SemiFinal: modifier -= 15; break;
                    case ArenaFight.ArenaBout.Final: modifier -= 20; break;
                }
                return modifier;
            }
        }

        private async Task<SettlementActionResult> Banking(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            if (settlement.State.Banks == null)
            {
                result.Message = $"There are no banks at {settlement.Name}.";
                result.WasSuccessful = false;
                return result;
            }
            bool isBanking = true;
            while (isBanking)
            {
                var bankChoice = await _userRequest.RequestChoiceAsync("Which bank are you visiting?", settlement.State.Banks.Select(bank => bank.Name.ToString()).ToList(), canCancel: true);
                await Task.Yield();
                if (bankChoice.WasCancelled) isBanking = false;
                if (!bankChoice.WasCancelled)
                {
                    Enum.TryParse<Bank.BankName>(bankChoice.SelectedOption, out var selectedBankName);
                    var currentBank = settlement.State.Banks.FirstOrDefault(b => b.Name == selectedBankName);
                    if (currentBank != null)
                    {
                        var actionChoice = await _userRequest.RequestChoiceAsync($"Your account has {await currentBank.CheckBalanceAsync()} available coins. What would you like to do?", new List<string> { "Deposit coins.", "Withdraw coins." }, canCancel: true);
                        await Task.Yield();
                        switch (actionChoice.SelectedOption)
                        {
                            case "Deposit coins.":
                                var depositResult = await _userRequest.RequestNumberInputAsync("How much would you like to deposit?", min: 0);
                                if (!depositResult.WasCancelled)
                                {
                                    if (result.AvailableCoins >= depositResult.Amount)
                                    {
                                        result.Message += $"{depositResult.Amount} was deposited at {currentBank.Name.ToString()}. The new balance is {await currentBank.DepositAsync(depositResult.Amount)}";
                                        result.AvailableCoins -= depositResult.Amount;
                                    }
                                }
                                break;
                            case "Withdraw coins.":
                                var withdrawResult = await _userRequest.RequestNumberInputAsync("How much would you like to withdraw?", min: 0, max: currentBank.AccountBalance);
                                if (!withdrawResult.WasCancelled)
                                {
                                    var amountWithdrawn = await currentBank.WithdrawAsync(withdrawResult.Amount);
                                    result.Message += $"{amountWithdrawn} was withdrawn from {currentBank.Name.ToString()}. The new balance is {await currentBank.CheckBalanceAsync()}";
                                    result.AvailableCoins += amountWithdrawn;
                                }
                                break;
                        }
                    }
                }
            }
            return result;
        }

        private SettlementActionResult VisitBlacksmith(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var blackSmith = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.Blacksmith);
            if (blackSmith == null)
            {
                result.Message = "There is no blacksmith at this settlement.";
                result.WasSuccessful = false;
                return result;
            }

            // Initialize the shop inventory
            var inventory = new List<Equipment>();

            // Get Armor and Shields stock
            var armourMaxDurabilityModifier = blackSmith.ArmourMaxDurabilityModifier;
            var armourPriceModifier = blackSmith.ArmourPriceModifier;
            var armourAvailabilityModifier = blackSmith.ArmourAvailabilityModifier;
            inventory.AddRange(GetStock(ShopCategory.Armour, settlement, armourAvailabilityModifier, armourPriceModifier, armourMaxDurabilityModifier));
            inventory.AddRange(GetStock(ShopCategory.Shields, settlement, armourAvailabilityModifier, armourPriceModifier, armourMaxDurabilityModifier));

            // Get Weapons stock
            var weaponMaxDurabilityModifier = blackSmith.WeaponMaxDurabilityModifier;
            var weaponPriceModifier = blackSmith.WeaponPriceModifier;
            var weaponAvailabilityModifier = blackSmith.WeaponAvailabilityModifier;
            inventory.AddRange(GetStock(ShopCategory.Weapons, settlement, weaponAvailabilityModifier, weaponPriceModifier, weaponMaxDurabilityModifier));

            // Apply any shop specials to the combined inventory
            if (blackSmith.ShopSpecials != null)
            {
                foreach (var special in blackSmith.ShopSpecials)
                {
                    var item = inventory.FirstOrDefault(i => i.Name == special.ItemName);
                    if (item != null)
                    {
                        if (special.Price.HasValue)
                        {
                            item.Value = special.Price.Value;
                        }
                        if (special.Availability.HasValue)
                        {
                            item.Availability = special.Availability.Value;
                        }
                    }
                }
            }

            result.ShopInventory = inventory;
            return result;
        }

        private SettlementActionResult VisitGeneralStore(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var generalStore = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.GeneralStore);
            if (generalStore != null)
            {
                var priceModifier = generalStore.EquipmentPriceModifier;
                var availabilityModifier = generalStore.EquipmentAvailabilityModifier;
                result.ShopInventory =
                    [.. GetStock(ShopCategory.General, settlement, availabilityModifier, priceModifier),
                    .. GetStock(ShopCategory.Potions, settlement, availabilityModifier, priceModifier)];
                if (generalStore.ShopSpecials != null)
                {
                    foreach (var special in generalStore.ShopSpecials)
                    {
                        var item = result.ShopInventory.FirstOrDefault(i => i.Name == special.ItemName);
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
                result.Message = "There is no blacksmith at this settlement";
                result.WasSuccessful = false;
            }
            return result;
        }

        private List<Equipment> GetStock(ShopCategory category, Settlement settlement, int availabilityModifier = 0, double priceModifier = 1d, int maxDurabilityModifier = 0)
        {
            var freshStocks = settlement.State.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.FreshStocks);
            var shortageOfGoods = settlement.State.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.ShortageOfGoods);
            if (freshStocks != null) availabilityModifier += 2;
            if (shortageOfGoods != null) availabilityModifier -= 2;

            var list = EquipmentService.GetShopInventoryByCategory(category, availabilityModifier);

            var sale = settlement.State.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.Sale);
            if (sale != null) priceModifier -= 0.2;
            if (shortageOfGoods != null) priceModifier += 0.1;

            list.ForEach(item => item.Value = (int)Math.Floor(item.Value * priceModifier));
            list.ForEach(item => item.MaxDurability += maxDurabilityModifier);
            list.ForEach(item => item.Durability += item.MaxDurability);
            return list;
        }

        private SettlementActionResult VisitHerbalist(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var herbalist = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.Herbalist);
            if (herbalist == null)
            {
                result.Message = "There is no herbalist at this settlement";
                result.WasSuccessful = false;
                return result;
            }
            result.ShopInventory = AlchemyService.GetShopIngredients().Cast<Equipment>().ToList();
            return result;
        }

        private SettlementActionResult VisitMagicBrewery(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var magicBrewery = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.MagicBrewery);
            if (magicBrewery == null)
            {
                result.Message = "There is no magic brewery at this settlement";
                result.WasSuccessful = false;
                return result;
            }
            result.ShopInventory = AlchemyService.GetShopPotions().Cast<Equipment>().ToList();
            return result;
        }

        private async Task<SettlementActionResult> CollectQuestRewardsAsync(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var completedQuestsForThisSettlement = hero.Party.Quests
                .Where(q => q.IsComplete && q.QuestOrigin == settlement.Name)
                .ToList();
            if (!completedQuestsForThisSettlement.Any())
            {
                result.Message = "There are no completed quest to turn in here.";
                result.WasSuccessful = false;
                return result;
            }

            foreach (var quest in completedQuestsForThisSettlement)
            {
                result.Message += $"Quest: {quest.Name} completed.\n";
                hero.Party.Coins += quest.RewardCoin;
                result.Message += $"Coin Reward: {quest.RewardCoin}.\n";
                if (quest.RewardItems != null)
                {
                    result.Message += $"Reward Items: {string.Join(", ", quest.RewardItems.Select(i => i != null ? i.Name : string.Empty))}.\n";
                    foreach (var item in quest.RewardItems)
                    {
                        if (item != null)
                        {
                            await BackpackHelper.AddItem(hero.Inventory.Backpack, item);
                        }
                    }
                }
            }
            hero.Party.Quests.RemoveAll(q => q.IsComplete && q.QuestOrigin == settlement.Name);
            return result;
        }

        private async Task<SettlementActionResult> VisitSickWard(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var sickWard = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.SickWard);
            if (sickWard == null)
            {
                result.Message = "There is no sickward at this settlement.";
                result.WasSuccessful = false;
                return result;
            }
            var poison = hero.ActiveStatusEffects.FirstOrDefault(a => a.Category == Combat.StatusEffectType.Poisoned);
            var disease = hero.ActiveStatusEffects.FirstOrDefault(a => a.Category == Combat.StatusEffectType.Diseased);
            if (poison == null &&  disease == null)
            {
                result.Message = $"{hero.Name} is neither poisoned nor diseased.";
                result.WasSuccessful = false;
                return result;
            }
            if (poison != null && result.AvailableCoins >= 100 && await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wnat to be cured of poison for 100c?"))
            {
                StatusEffectService.RemoveActiveStatusEffect(hero, poison);
                result.AvailableCoins -= 100;
                result.Message += $"{hero.Name} was cured of poison!";
            }
            await Task.Yield();
            if (disease != null && result.AvailableCoins >= 100 && await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wnat to be cured of disease for 100c?"))
            {
                StatusEffectService.RemoveActiveStatusEffect(hero, disease);
                result.AvailableCoins -= 100;
                result.Message += $"{hero.Name} was cured of disease!";
            }
            await Task.Yield();

            return result;
        }

        private async Task<SettlementActionResult> EnchantItemAsync(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var inn = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.Inn);
            if (inn == null)
            {
                result.Message = "There is no inn at this settlement.";
                result.WasSuccessful = false; 
                return result;
            }

            if (hero.ProfessionName != "Wizard")
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
                var list = powerStones.Select(i => i.Name).ToList();
                var stoneChoiceRequest = await _userRequest.RequestChoiceAsync("Choose a power stone to enchant with.", list);
                await Task.Yield();
                selectedPowerStone = powerStones.FirstOrDefault(i => i.Name == stoneChoiceRequest.SelectedOption);
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
                .Select(i => i != null ? i.Name : string.Empty)
                .ToList();

            if (!equipmentList.Any())
            {
                result.Message = $"There are no valid items to enchant with {selectedPowerStone.Name}.";
                result.WasSuccessful = false;
                return result;
            }

            var choiceRequest = await _userRequest.RequestChoiceAsync("Choose the item to enchant.", equipmentList);
            await Task.Yield();
            var selectedEquipment = hero.Inventory.Backpack.FirstOrDefault(i => i != null && i.Name == choiceRequest.SelectedOption);

            if (selectedEquipment == null)
            {
                result.Message = "Invalid item selection.";
                result.WasSuccessful = false;
                return result;
            }

            selectedEquipment = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, selectedEquipment);

            var rollResult = await _userRequest.RequestRollAsync("Roll arcane arts skill check.", "1d100", skill: (hero, Skill.ArcaneArts));
            var skillTarget = hero.GetSkill(Skill.ArcaneArts);

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

        private async Task<SettlementActionResult> CreateScroll(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var inn = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.Inn);
            if (inn == null)
            {
                result.Message = "There is no inn at this settlement.";
                result.WasSuccessful = false;
                return result;
            }

            if (hero.ProfessionName != "Wizard")
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
                var rollResult = await _userRequest.RequestRollAsync("Roll arcane arts skill check.", "1d100", skill: (hero, Skill.ArcaneArts));
                await Task.Yield();
                var skillTarget = hero.GetSkill(Skill.ArcaneArts);
                if (rollResult.Roll > skillTarget)
                {
                    result.Message += $"{hero.Name} fails to create a scroll.";
                }
                else
                {
                    var spellList = hero.Spells?.Select(s => s.Name).ToList() ?? new();
                    var choiceRequest = await _userRequest.RequestChoiceAsync("Choose the scroll you wish to create.", spellList);
                    await Task.Yield();
                    var newScroll = new Equipment { Name = $"Scroll of {SpellService.GetSpellByName(choiceRequest.SelectedOption)}", Quantity = 1, Value = 100};
                    result.Message += $"{newScroll.Name} was created!";
                    await BackpackHelper.AddItem(hero.Inventory.Backpack, newScroll);
                }
            }
            hero.HasCreatedScrolls = true;

            return result;
        }

        public int SettlementActionCost(SettlementActionType action)
        {
            return action switch
            {
                SettlementActionType.CollectQuestRewards => 0,
                SettlementActionType.Gamble => 0,
                SettlementActionType.LevelUp => 0,
                SettlementActionType.RestRecuperation => 0,
                SettlementActionType.TendThoseMemories => 0,
                SettlementActionType.LearnPrayer => 2,
                SettlementActionType.LearnSpell => 3,
                SettlementActionType.TreatMentalConditions => 5,
                _ => 1
            };
        }
    }

    public class Bank
    {
        public enum BankName
        {
            ChamberlingsReserve,
            SmartfallBank,
            TheVault
        }

        public BankName Name { get; set; } = BankName.ChamberlingsReserve;
        public string Description { get; set; } = string.Empty;
        public int AccountBalance { get; set; }
        public bool HasCheckedBankAccount { get; set; }
        public double ProfitLoss { get; set; }

        public Bank() { }

        public async Task<int> DepositAsync (int amount)
        {
            if (amount <= 0) return AccountBalance;

            await CheckBalanceAsync();
            AccountBalance += amount;
            return AccountBalance;
        }

        public async Task<int> WithdrawAsync (int amount)
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
}
