using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Player
{
    public enum SettlementActionType
    {
        ArenaFighting,
        Banking,
        BuyDog,
        BuyFamiliar,
        BuySellArmour,
        BuySellEquipment,
        BuyIngredients,
        BuySellWeapons,
        ChargeMagicItem,
        CollectQuestRewards,
        CreateScroll,
        CureDisease,
        CurePoison,
        EnchantObjects,
        Gamble,
        GuildBusiness,
        HorseRacing,
        IdentifyMagicItem,
        IdentifyPotion,
        LearnPrayer,
        LearnSpell,
        LevelUp,
        Pray,
        ReadFortune,
        RepairEquipment,
        RestRecuperation,
        SkillTraining,
        TendThoseMemories,
        TreatMentalConditions
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

            switch (action)
            {
                case SettlementActionType.ArenaFighting:                    
                    result = await ArenaFighting(hero, result);
                    break;
                case SettlementActionType.Banking:
                    result = await Banking(hero, settlement, result);
                    break;
                case SettlementActionType.BuyDog: 
                    // TODO: on implementation of the companions expansion
                    break;
                case SettlementActionType.BuyFamiliar:
                    // TODO: on implementation of the companions expansion
                    break;
                case SettlementActionType.BuySellArmour:
                    result = BuySellArmour(hero, settlement, result);
                    break;
                case SettlementActionType.BuySellWeapons:
                    result = BuySellWeapons(hero, settlement, result);
                    break;
                case SettlementActionType.BuySellEquipment:
                    result = BuySellEquipment(hero, settlement, result);
                    break;
                case SettlementActionType.BuyIngredients: 
                    break;
                case SettlementActionType.ChargeMagicItem: 
                    break;
                case SettlementActionType.CollectQuestRewards:
                    break;
                case SettlementActionType.CreateScroll: 
                    break;
                case SettlementActionType.CureDisease: 
                    break;
                case SettlementActionType.CurePoison: 
                    break;
                case SettlementActionType.EnchantObjects: 
                    break;
                case SettlementActionType.Gamble: 
                    break;
                case SettlementActionType.GuildBusiness: 
                    break;
                case SettlementActionType.HorseRacing: 
                    break;
                case SettlementActionType.IdentifyMagicItem: 
                    break;
                case SettlementActionType.IdentifyPotion: 
                    break;
                case SettlementActionType.LearnPrayer: 
                    break;
                case SettlementActionType.LearnSpell: 
                    break;
                case SettlementActionType.LevelUp: 
                    break;
                case SettlementActionType.Pray: 
                    break;
                case SettlementActionType.ReadFortune: 
                    break;
                case SettlementActionType.RepairEquipment: 
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

            settlement.State.HeroActionPoints[hero] -= result.ActionCost;
            if (settlement.State.HeroActionPoints[hero] < 0)
            {
                settlement.State.BusyHeroes.TryAdd(hero, (result.Action, Math.Abs(settlement.State.HeroActionPoints[hero])));
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
            var totalCoins = hero.Coins + hero.Party.Coins;

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
                                    if (hero.Coins + hero.Party.Coins >= depositResult.Amount)
                                    {
                                        result.Message += $"{depositResult.Amount} was deposited at {currentBank.Name.ToString()}. The new balance is {await currentBank.DepositAsync(depositResult.Amount)}";
                                        var remainingDeposit = depositResult.Amount;
                                        var heroDeposited = Math.Min(hero.Coins, remainingDeposit);
                                        hero.Coins -= heroDeposited;
                                        remainingDeposit -= heroDeposited;
                                        hero.Party.Coins -= remainingDeposit;
                                    }
                                }
                                break;
                            case "Withdraw coins.":
                                var withdrawResult = await _userRequest.RequestNumberInputAsync("How much would you like to withdraw?", min: 0, max: currentBank.AccountBalance);
                                if (!withdrawResult.WasCancelled)
                                {
                                    var amountWithdrawn = await currentBank.WithdrawAsync(withdrawResult.Amount);
                                    result.Message += $"{amountWithdrawn} was withdrawn from {currentBank.Name.ToString()}. The new balance is {await currentBank.CheckBalanceAsync()}";
                                    hero.Coins += amountWithdrawn;
                                }
                                break;
                        }
                    }
                }
            }
            return result;
        }

        private SettlementActionResult BuySellArmour(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var blackSmith = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.Blacksmith);
            if (blackSmith != null)
            {
                var maxDurabilityModifier = blackSmith.ArmourMaxDurabilityModifier;
                var priceModifier = blackSmith.ArmourPriceModifier;
                var availabilityModifier = blackSmith.ArmourAvailabilityModifier;
                List<Equipment> inventory =
                    [.. GetStock(ShopCategory.Armour, settlement, availabilityModifier, priceModifier, maxDurabilityModifier),
                    .. GetStock(ShopCategory.Shields, settlement, availabilityModifier, priceModifier, maxDurabilityModifier)];
                if (blackSmith.ShopSpecials != null)
                {
                    foreach (var special in blackSmith.ShopSpecials)
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

        private SettlementActionResult BuySellWeapons(Hero hero, Settlement settlement, SettlementActionResult result)
        {
            var blackSmith = settlement.AvailableServices.FirstOrDefault(s => s.Name == SettlementServiceName.Blacksmith);
            if (blackSmith != null)
            {
                var maxDurabilityModifier = blackSmith.WeaponMaxDurabilityModifier;
                var priceModifier = blackSmith.WeaponPriceModifier;
                var availabilityModifier = blackSmith.WeaponAvailabilityModifier;
                result.ShopInventory = [.. GetStock(ShopCategory.Weapons, settlement, availabilityModifier, priceModifier, maxDurabilityModifier)];
                if (blackSmith.ShopSpecials != null)
                {
                    foreach (var special in blackSmith.ShopSpecials)
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

        private SettlementActionResult BuySellEquipment(Hero hero, Settlement settlement, SettlementActionResult result)
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


        public int SettlementActionCost(SettlementActionType action)
        {
            return action switch
            {
                SettlementActionType.ArenaFighting => 1,
                SettlementActionType.Banking => 1,
                SettlementActionType.BuyDog => 1,
                SettlementActionType.BuyFamiliar => 1,
                SettlementActionType.BuySellArmour => 1,
                SettlementActionType.BuySellEquipment => 1,
                SettlementActionType.BuyIngredients => 1,
                SettlementActionType.BuySellWeapons => 1,
                SettlementActionType.ChargeMagicItem => 1,
                SettlementActionType.CollectQuestRewards => 0,
                SettlementActionType.CreateScroll => 1,
                SettlementActionType.CureDisease => 1,
                SettlementActionType.CurePoison => 1,
                SettlementActionType.EnchantObjects => 1,
                SettlementActionType.Gamble => 0,
                SettlementActionType.GuildBusiness => 1,
                SettlementActionType.HorseRacing => 1,
                SettlementActionType.IdentifyMagicItem => 1,
                SettlementActionType.IdentifyPotion => 1,
                SettlementActionType.LearnPrayer => 2,
                SettlementActionType.LearnSpell => 3,
                SettlementActionType.LevelUp => 0,
                SettlementActionType.Pray => 1,
                SettlementActionType.ReadFortune => 1,
                SettlementActionType.RepairEquipment => 1,
                SettlementActionType.RestRecuperation => 0,
                SettlementActionType.SkillTraining => 1,
                SettlementActionType.TendThoseMemories => 0,
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
