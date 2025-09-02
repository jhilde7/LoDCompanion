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
        ArenaFighting,
        Banking,
        VisitKennel,
        VisitAlbertasMagnificentAnimals,
        BuyingAndSelling,
        ChargeMagicItem,
        CreateScroll,
        CureDiseasePoison,
        CurePoison,
        EnchantObjects,
        Gambling,
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
        private readonly PowerActivationService _powerActivation;
        private readonly LevelupService _levelUp;
        
        public event Func<Hero, int, Task<string>>? OnLevelup;

        public SettlementActionService(
            UserRequestService userRequestService,
            TreasureService treasureService,
            PowerActivationService powerActivationService,
            LevelupService levelupService)
        {
            _userRequest = userRequestService;
            _treasure = treasureService;
            _powerActivation = powerActivationService;
            _levelUp = levelupService;
        }

        public async Task<SettlementActionResult> PerformSettlementActionAsync(Hero hero, SettlementActionType action, ServiceLocation service)
        {
            var result = new SettlementActionResult(action);
            result.ActionCost = SettlementActionCost(action);

            if (service.Settlement.State.BusyHeroes.ContainsKey(hero))
            {
                result.Message = $"{hero.Name} is busy with a multi-day task.";
                result.WasSuccessful = false;
                return result;
            }
            else if (service.Settlement.State.HeroActionPoints[hero] <= 0)
            {
                result.Message = $"{hero.Name} does not have any action points left for today.";
                result.WasSuccessful = false;
                return result;
            }

            result.AvailableCoins = hero.Coins + hero.Party.Coins;
            hero.Coins = 0;
            hero.Party.Coins = 0;

            switch (service, action)
            {
                case (ServiceLocation, SettlementActionType.CollectQuestRewards):
                    result = await CollectQuestRewardsAsync(hero, service.Settlement, result);
                    break;
                case (Arena arena, SettlementActionType.ArenaFighting):                    
                    result = await ArenaFighting(hero, arena, result);
                    break;
                case (Bank bank, SettlementActionType.Banking):
                    result = await Banking(hero, bank, result);
                    break;
                case (ServiceLocation, SettlementActionType.BuyingAndSelling):
                    result = GetShopInventory(hero, service, result);
                    break;
                case (SickWard sickWard, SettlementActionType.CureDiseasePoison):
                    result = await VisitSickWard(hero, sickWard, result);
                    break;
                case (Inn inn, SettlementActionType.CreateScroll):
                    result = await CreateScroll(hero, inn, result);
                    break;
                case (Inn inn, SettlementActionType.EnchantObjects): 
                    result = await EnchantItemAsync(hero, inn, result);
                    break;
                case (FortuneTeller fortuneTeller, SettlementActionType.ReadFortune):
                    result = await ReadFortune(hero, fortuneTeller, result);
                    break;
                case (Inn inn, SettlementActionType.Gambling):
                    result = await Gamble(hero, inn, result);
                    break;
                case (HorseTrack horseTrack, SettlementActionType.HorseRacing):
                    result = await horseTrack.HorseRacing(hero, result, _userRequest, _treasure);
                    break;
                case (ServiceLocation, SettlementActionType.IdentifyMagicItem):
                    result = await IdentifyMagicItem(hero, service, result);
                    break;
                case (ServiceLocation, SettlementActionType.IdentifyPotion):
                    result = await IdentifyPotion(hero, service, result);
                    break;
                case (WizardsGuild wizardsGuild, SettlementActionType.LearnSpell):
                    result = await LearnSpell(hero, wizardsGuild, result);
                    break;
                case (TheInnerSanctum theInnerSanctum, SettlementActionType.LearnPrayer):
                    result = await LearnPrayer(hero, theInnerSanctum, result);
                    break;
                case (ServiceLocation, SettlementActionType.LevelUp):
                    result = await LevelupHero(hero, result);
                    break;
                case (Temple temple, SettlementActionType.Pray):
                    result = await Pray(hero, temple, result);
                    break;
                case (Inn inn, SettlementActionType.RestRecuperation):
                    result = await RestRecuperation(hero.Party, inn, result);
                    break;
                case (TheAsylum asylum, SettlementActionType.TreatMentalConditions):
                    result = await TreatMentalConditions(hero, asylum, result);
                    break;
            }

            if (result.WasSuccessful)
            {
                service.Settlement.State.HeroActionPoints[hero] -= result.ActionCost;
                if (service.Settlement.State.HeroActionPoints[hero] < 0)
                {
                    service.Settlement.State.BusyHeroes.TryAdd(hero, (result.Action, Math.Abs(service.Settlement.State.HeroActionPoints[hero])));
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

        private async Task<SettlementActionResult> ArenaFighting(Hero hero, Arena arena, SettlementActionResult result)
        {
            if (result.AvailableCoins < arena.MinimumEntryFee)
            {
                result.Message = $"{hero.Name} does not have enough coin to participate";
                result.WasSuccessful = false;
                return result;
            }

            var inputResult = await _userRequest.RequestNumberInputAsync("How much fo you want to bet", min: arena.MinimumEntryFee, max: Math.Min(arena.MaxBet, result.AvailableCoins), canCancel: true);
            if (!inputResult.WasCancelled)
            {
                var bet = inputResult.Amount;
                arena.Bet = bet;
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
            }
            return result;
        }
        
        private async Task<SettlementActionResult> Banking(Hero hero, Bank bank, SettlementActionResult result)
        {
            bool isBanking = true;
            while (isBanking)
            {
                if (bank != null)
                {
                    var actionChoice = await _userRequest.RequestChoiceAsync($"Your account has {await bank.CheckBalanceAsync()} available coins. What would you like to do?", new List<string> { "Deposit coins.", "Withdraw coins." }, canCancel: true);
                    await Task.Yield();
                    switch (actionChoice.SelectedOption)
                    {
                        case "Deposit coins.":
                            var depositResult = await _userRequest.RequestNumberInputAsync("How much would you like to deposit?", min: 0, canCancel: true);
                            if (!depositResult.WasCancelled)
                            {
                                if (result.AvailableCoins >= depositResult.Amount)
                                {
                                    result.Message += $"{depositResult.Amount} was deposited at {bank.Name.ToString()}. The new balance is {await bank.DepositAsync(depositResult.Amount)}";
                                    result.AvailableCoins -= depositResult.Amount;
                                }
                            }
                            break;
                        case "Withdraw coins.":
                            var withdrawResult = await _userRequest.RequestNumberInputAsync("How much would you like to withdraw?", min: 0, max: bank.AccountBalance, canCancel: true);
                            if (!withdrawResult.WasCancelled)
                            {
                                var amountWithdrawn = await bank.WithdrawAsync(withdrawResult.Amount);
                                result.Message += $"{amountWithdrawn} was withdrawn from {bank.Name.ToString()}. The new balance is {await bank.CheckBalanceAsync()}";
                                result.AvailableCoins += amountWithdrawn;
                            }
                            break;
                    }
                }
            }
            return result;
        }

        private SettlementActionResult GetShopInventory(Hero hero, ServiceLocation service, SettlementActionResult result)
        {
            result.ShopInventory = service.CurrentAvailableStock;
            return result;
        }

        private async Task<SettlementActionResult> VisitSickWard(Hero hero, SickWard sickWard, SettlementActionResult result)
        {
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

        private async Task<SettlementActionResult> EnchantItemAsync(Hero hero, Inn inn, SettlementActionResult result)
        {
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

        private async Task<SettlementActionResult> CreateScroll(Hero hero, Inn inn, SettlementActionResult result)
        {
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

        private async Task<SettlementActionResult> ReadFortune(Hero hero, FortuneTeller fortuneTeller, SettlementActionResult result)
        {
            if (result.AvailableCoins < 50)
            {
                result.Message = $"{hero.Name} does not have enough available coins for this action.";
                result.WasSuccessful = false;
                return result;
            }

            result.AvailableCoins -= 50;
            var rollResult = await _userRequest.RequestRollAsync("Roll for your fortune", "1d6");
            await Task.Yield();
            switch (rollResult.Roll)
            {
                case 1: 
                    result.Message = "The Fortune Teller describes an upcoming battle in such detail that during the next quest, the hero recognizes the situation and manages to avoid harm." +
                        " The hero may treat one successful attack against them as a miss during the next quest.";
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, new ActiveStatusEffect(StatusEffectType.Precognition, -1), _powerActivation); 
                    break;
                case 2: 
                    result.Message = "The Fortune Teller talks about great fortune being made through gambling. The hero has enhanced luck at a gambling dice roll during this stay in the city.";
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, new ActiveStatusEffect(StatusEffectType.GamblingLuck, -1), _powerActivation);
                    break;
                case 6: 
                    result.Message = "You are cursed! The Fortune Teller staggers back in shock after reading the hero's palm. The hero will suffer a curse during the next quest.";
                    var curseEffect = StatusEffectService.GetRandomCurseEffect();
                    curseEffect.RemoveEndOfDungeon = true;
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, curseEffect, _powerActivation);
                    break;
                default: result.Message = "The Fortune Teller talks about lots of things, nut nothing that is of any importance."; break;
            }
            return result;
        }

        private async Task<SettlementActionResult> Gamble(Hero hero, Inn inn, SettlementActionResult result)
        {
            if (result.AvailableCoins < 50)
            {
                result.Message = $"{hero.Name} does not have enough coin to gamble with.";
                result.WasSuccessful = false;
                return result;
            }

            var inputResult = await _userRequest.RequestNumberInputAsync("How much do you want to bet?", min: 50, max: Math.Min(500, result.AvailableCoins), canCancel: true);
            await Task.Yield();
            if (!inputResult.WasCancelled)
            {
                var bet = inputResult.Amount;
                result.AvailableCoins -= bet;
                var luck = hero.GetStat(BasicStat.Luck);
                var rollResult = await _userRequest.RequestRollAsync("Roll gambling result.", "1d10");
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

        

        private async Task<SettlementActionResult> IdentifyMagicItem(Hero hero, ServiceLocation scryer, SettlementActionResult result)
        {
            if (result.AvailableCoins < 300)
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
                var itemsListString = unidentifiedItems.Select(i => i != null ? i.Name : string.Empty).ToList();
                var choiceResult = await _userRequest.RequestChoiceAsync("Choose item to identify.", itemsListString);
                await Task.Yield();
                selectedItem = unidentifiedItems.FirstOrDefault(i => i?.Name == choiceResult.SelectedOption);
            }

            if (selectedItem != null) 
            {
                selectedItem.Identified = true;
                result.AvailableCoins -= 300;
                result.Message = $"Item identified: {selectedItem.ToString()}";
            }
            return result;
        }

        private async Task<SettlementActionResult> IdentifyPotion(Hero hero, ServiceLocation generalStore, SettlementActionResult result)
        {
            if (result.AvailableCoins < 25)
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
                var itemsListString = unidentifiedItems.Select(i => i != null ? i.Name : string.Empty).ToList();
                var choiceResult = await _userRequest.RequestChoiceAsync("Choose potion to identify.", itemsListString);
                await Task.Yield();
                selectedItem = unidentifiedItems.FirstOrDefault(i => i?.Name == choiceResult.SelectedOption);
            }

            if (selectedItem != null)
            {
                selectedItem.Identified = true;
                result.AvailableCoins -= 25;
                result.Message = $"Potion identified: {selectedItem.ToString()}";
            }
            return result;
        }

        private async Task<SettlementActionResult> LearnSpell(Hero hero, WizardsGuild wizardsGuild, SettlementActionResult result)
        {
            if (hero.ProfessionName != "Wizard")
            {
                result.Message = $"{hero.Name} is not a Wizard and can't learn spells.";
                result.WasSuccessful = false;
                return result;
            }

            var grimoires = hero.Inventory.Backpack.Where(i => i != null && i.Name.Contains("Grimoire")).ToList();
            bool learnFromGrimoire = false;
            if (grimoires.Any())
            {
                var yesNoResult = await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to learn a spell from an owned Grimoire?");
                await Task.Yield();
                learnFromGrimoire = yesNoResult;
                if (learnFromGrimoire)
                {
                    var selectedGrimoire = grimoires.FirstOrDefault();
                    if (grimoires.Count > 1)
                    {
                        var grimoiresString = grimoires.Select(g => g != null ? g.Name : string.Empty).ToList();
                        var choiceResult = await _userRequest.RequestChoiceAsync("Choose which grimoire to learn from.", grimoiresString);
                        await Task.Yield();
                        selectedGrimoire = grimoires.FirstOrDefault(g => g != null && g.Name == choiceResult.SelectedOption);
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

            if (!learnFromGrimoire && result.AvailableCoins < 1000)
            {
                result.Message = $"{hero.Name} deos not have enough available coins for this action.";
                result.WasSuccessful = false;
                return result;
            }

            var yesNoSpellResult = await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to learn a spell for 1000c and {result.ActionCost} days of time?");
            await Task.Yield();
            if (!yesNoSpellResult)
            {
                result.Message = "action was cancelled.";
                result.WasSuccessful = false;
                return result;
            }

            if (hero.Spells != null)
            {
                var spellList = new List<Spell>();
                var knownSpells = hero.Spells;
                for (int level = 1; level <= hero.Level; level++)
                {
                    spellList.AddRange(SpellService.GetSpellsByLevel(level).Where(s => !knownSpells.Contains(s)));
                }
                var spellListStrings = spellList.Select(s => s.Name).ToList();
                var choiceSpellResult = await _userRequest.RequestChoiceAsync("Choose as spell to learn.", spellListStrings);
                await Task.Yield();
                var spellChoice = spellList.FirstOrDefault(s => s.Name == choiceSpellResult.SelectedOption);
                if (spellChoice != null)
                {
                    result.AvailableCoins -= 1000;
                    hero.Spells.Add(spellChoice);
                    result.Message = $"{hero.Name} now knows the spell: {spellChoice.ToString()}.";
                    return result; 
                }
            }
            return result;
        }

        private async Task<SettlementActionResult> LearnPrayer(Hero hero, TheInnerSanctum innerSanctum, SettlementActionResult result)
        {
            if (result.AvailableCoins < 1000)
            {
                result.Message = $"{hero.Name} deos not have enough available coins for this action.";
                result.WasSuccessful = false;
                return result;
            }

            if (hero.ProfessionName != "Warrior Priest")
            {
                result.Message = $"{hero.Name} is not a Warrior Priest and can't learn prayers.";
                result.WasSuccessful = false;
                return result;
            }

            var yesNoSpellResult = await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to learn a prayer for 1000c and {result.ActionCost} days of time?");
            await Task.Yield();
            if (!yesNoSpellResult)
            {
                result.Message = "action was cancelled.";
                result.WasSuccessful = false;
                return result;
            }

            if (hero.Prayers != null)
            {
                var prayerList = new List<Prayer>();
                var knownPrayers = hero.Prayers;
                for (int level = 1; level <= hero.Level; level++)
                {
                    prayerList.AddRange(PrayerService.GetPrayersByLevel(level).Where(s => !knownPrayers.Contains(s)));
                }
                var prayerListStrings = prayerList.Select(s => s.Name.ToString()).ToList();
                var choicePrayerResult = await _userRequest.RequestChoiceAsync("Choose as prayer to learn.", prayerListStrings);
                await Task.Yield();
                var prayerChoice = prayerList.FirstOrDefault(s => s.Name.ToString() == choicePrayerResult.SelectedOption);
                if (prayerChoice != null)
                {
                    result.AvailableCoins -= 1000;
                    hero.Prayers.Add(prayerChoice);
                    result.Message = $"{hero.Name} now knows the prayer: {prayerChoice.ToString()}.";
                    return result;
                }
            }
            return result;
        }

        private async Task<SettlementActionResult> LevelupHero(Hero hero, SettlementActionResult result)
        {
            if (hero.Experience < hero.XPtoLVL)
            {
                result.Message = $"{hero.Name} does not have enough experience to level up.";
                result.WasSuccessful = false;
                return result;
            }

            while (hero.Experience >= hero.XPtoLVL)
            {
                _levelUp.LevelUp(hero);
                result.Message += $"\n{hero.Name} gained a level.";

                var availablePerkList = _levelUp.GetPerkCategoryAtLevelup(hero.ProfessionName, hero.Level)?.Where(p => !hero.Perks.Contains(p));
                if (availablePerkList != null && availablePerkList.Any())
                {
                    var perkNameList = availablePerkList.Select(p => p.Name.ToString()).ToList();
                    var perkChoiceRequest = await _userRequest.RequestChoiceAsync("Choose a perk.", perkNameList);
                    await Task.Yield();
                    var selectedPerk = availablePerkList.FirstOrDefault(p => p.Name.ToString() == perkChoiceRequest.SelectedOption);

                    if (selectedPerk != null)
                    {
                        _levelUp.AttemptToSelectPerk(hero, hero.Levelup, selectedPerk, out string error);
                        result.Message += $"\n{hero.Name} can now use: {selectedPerk.ToString()}";
                    }
                }

                var availableTalentsList = _levelUp.GetTalentCategoryAtLevelup(hero.ProfessionName, hero.Level);
                if (availableTalentsList != null && availableTalentsList.Any())
                {
                    var talentNameList = availableTalentsList.Select(t => t.Name.ToString()).ToList();
                    var talentChoiceRequest = await _userRequest.RequestChoiceAsync("Choose a talent.", talentNameList);
                    await Task.Yield();
                    var selectedTalent = availableTalentsList.FirstOrDefault(t => t.Name.ToString() == talentChoiceRequest.SelectedOption);

                    if (selectedTalent != null)
                    {
                        _levelUp.AttemptToSelectTalent(hero, hero.Levelup, selectedTalent, out string error);
                        result.Message += $"\n{hero.Name} can now use: {selectedTalent.ToString()}";
                    }
                }

                var imporvementPoints = hero.Levelup.ImprovementPoints;
                if (OnLevelup != null) result.Message += $"\n{await OnLevelup.Invoke(hero, imporvementPoints)}";
            }
            return result;
        }

        private async Task<SettlementActionResult> Pray(Hero hero, Temple temple, SettlementActionResult result)
        {
            if (hero.ActiveStatusEffects.Where(e => e.Category.ToString().Contains("Blessing")).Any())
            {
                result.Message = $"{hero.Name} has already offered prayers during this visit.";
                result.WasSuccessful = false;
                return result;
            }

            if (result.AvailableCoins >= temple.CostToPray)
            {
                var rollResult = await _userRequest.RequestRollAsync($"Roll to see if {temple.GodName.ToString()} listens.", temple.DiceToPray);
                await Task.Yield();
                if (rollResult.Roll <= 3 && temple.GrantedEffect != null)
                {
                    result.Message = $"{temple.GodName.ToString()} hears your prayer and decides to grant you a boon.";
                    if (temple.GodName == GodName.Ohlnir)
                    {
                        var skillChoiceRequest = await _userRequest.RequestChoiceAsync("Which skill do you want Ohlnir to enhance?", new List<string>() { "Combat", "Ranged" });
                        await Task.Yield();
                        switch (skillChoiceRequest.SelectedOption)
                        {
                            case "Combat": temple.GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true); break;
                            case "Ranged": temple.GrantedEffect = new ActiveStatusEffect(StatusEffectType.OhlnirsBlessing, -1, skillBonus: (Skill.RangedSkill, 5), removeEndOfDungeon: true); break;
                        }
                    }
                    result.AvailableCoins -= temple.CostToPray;
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, temple.GrantedEffect, _powerActivation);
                }
                else
                {
                    result.Message = $"You pray, but {temple.GodName.ToString()} remains silent.";
                }
            }
            else
            {
                result.Message = "You do not have enough coins to make an offering.";
                result.WasSuccessful = false;
            }
            return result;
        }

        private async Task<SettlementActionResult> RestRecuperation(Party party, Inn inn, SettlementActionResult result)
        {
            var estate = inn.Settlement.State.Estate;
            //The party owns an estate in this settlement (free stay)
            if (estate != null)
            {
                result.Message = "The party rests comfortably in their estate.";
                result.AvailableCoins = await PerformRest(party, 0, result.AvailableCoins, false); // Free rest, not in stables
                return result;
            }

            // The party can afford the inn
            if (result.AvailableCoins >= inn.Price)
            {
                var stayAtInn = await _userRequest.RequestYesNoChoiceAsync($"A room at the inn costs {inn.Price} coins. Would you like to stay the night?");
                if (stayAtInn)
                {
                    result.Message = "The party enjoys a comfortable night at the inn.";
                    result.AvailableCoins = await PerformRest(party, inn.Price, result.AvailableCoins, false);
                    return result;
                }
                else
                {
                    result.Message = "The party decides not to rest at the inn.";
                    result.WasSuccessful = false;
                    return result;
                }
            }

            // The party cannot afford the inn, but might afford the stables
            if (result.AvailableCoins < inn.SleepInStablesPrice)
            {
                result.Message = "You do not have enough coin to stay at the Inn, not even in the stables.";
                result.WasSuccessful = false;
                return result;
            }
            else
            {
                var sleepInStables = await _userRequest.RequestYesNoChoiceAsync($"You cannot afford a room ({inn.Price}c), but the stables are available for {inn.SleepInStablesPrice} coin. Sleep in the stables?");
                if (sleepInStables)
                {
                    result.Message = "The party rests in the stables.";
                    result.AvailableCoins = await PerformRest(party, inn.SleepInStablesPrice, result.AvailableCoins, true);
                }
                else
                {
                    result.Message = "The party chooses not to sleep in the stables.";
                    result.WasSuccessful = false;
                }
                return result;
            }
        }

        private async Task<int> PerformRest(Party party, int cost, int availableCoin, bool isStables)
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
                    if (missingSanity > 0 && await _userRequest.RequestYesNoChoiceAsync($"{hero.Name} has {missingSanity} sanity left to heal. Do they wish to drown their memories in ale or other pleasures at the cost of {sanityCost}, in an attempt to regain some sanity?"))
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

        private async Task<SettlementActionResult> TreatMentalConditions(Hero hero, TheAsylum asylum, SettlementActionResult result)
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

            if (result.AvailableCoins < 1000)
            {
                result.Message = "You do not have enough coins for treatment.";
                result.WasSuccessful = false;
                return result;
            }

            var conditionNames = curableConditions.Select(c => c.Category.ToString()).ToList();
            var choiceResult = await _userRequest.RequestChoiceAsync("Choose a condition to treat:", conditionNames, canCancel: true);

            if (choiceResult.WasCancelled)
            {
                result.Message = "Treatment cancelled.";
                result.WasSuccessful = false;
                return result;
            }

            var chosenCondition = curableConditions.FirstOrDefault(c => c.Category.ToString() == choiceResult.SelectedOption);
            if (chosenCondition == null)
            {
                result.Message = "Invalid selection.";
                result.WasSuccessful = false;
                return result;
            }

            var confirmation = await _userRequest.RequestYesNoChoiceAsync($"Treating {chosenCondition.Category} will cost 1000 coins and take 5 days. Continue?");
            if (!confirmation)
            {
                result.Message = "Treatment cancelled.";
                result.WasSuccessful = false;
                return result;
            }

            result.AvailableCoins -= 1000;

            var rollResult = await _userRequest.RequestRollAsync("Roll to determine treatment success (1-5 succeeds)", "1d6");
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

        public int SettlementActionCost(SettlementActionType action)
        {
            return action switch
            {
                SettlementActionType.CollectQuestRewards => 0,
                SettlementActionType.Gambling => 0,
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
}
