using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Game;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class SettlementEventContext
    {
        public PartyManagerService PartyManager { get; set; }
        public UserRequestService UserRequest { get; set; }
        public TreasureService Treasure { get; set; }
        public InventoryService Inventory { get; set; }
        public QuestService Quest { get; set; }
        public PowerActivationService PowerActivation { get; set; }
        public string EventDescription { get; set; } = string.Empty;

        public SettlementEventContext(
            PartyManagerService partyManager,
            UserRequestService userRequest,
            TreasureService treasureService,
            InventoryService inventory,
            QuestService questService,
            PowerActivationService powerActivationService) 
        { 
            PartyManager = partyManager;
            UserRequest = userRequest;
            Treasure = treasureService;
            Inventory = inventory;
            Quest = questService;
            PowerActivation = powerActivationService;
        }
    }

    public class SettlementEvent
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Func<SettlementEventContext, Task<SettlementEventResult>>? Execute { get; set; }
    }

    public class SettlementEventResult
    {
        public string Message {  get; set; } = string.Empty;
        public ActiveStatusEffect? ActiveStatusEffect { get; set; }

    }

    public class SettlementEventService
    {
        private readonly PartyManagerService _partyManager;
        private readonly UserRequestService _userRequest;
        private readonly TreasureService _treasure;
        private readonly InventoryService _inventory;
        private readonly QuestService _quest;
        private readonly PowerActivationService _powerActivation;

        public SettlementEventService(
            PartyManagerService partyManager, 
            UserRequestService userRequest, 
            TreasureService treasureService,
            InventoryService inventory,
            QuestService questService,
            PowerActivationService powerActivationService)
        {
            _partyManager = partyManager;
            _userRequest = userRequest;
            _treasure = treasureService;
            _inventory = inventory;
            _quest = questService;
            _powerActivation = powerActivationService;
        }

        public List<SettlementEvent> GetSettlementEvents()
        {
            var sideQuest = _quest.GetRandomSideQuest();
            var result = new SettlementEventResult();
            return new List<SettlementEvent>
            {
                new SettlementEvent
                {
                    Name = "Stray dog",
                    Description = "Nothing special happens",
                        //"The party is followed through the streets by a stray dog after receiving a small treat from one of the heroes. It seems that you now are the proud owners of a dog. Randomise what kind of dog, using the Companions' Compendium. If you do not own this compendium, or if you already have a dog, treat this result as: 'Nothing special happens'.",
                    //Execute = (context.PartyManager, context.UserRequest) => { /* Add logic for stray dog event */ }
                },
                new SettlementEvent
                {
                    Name = "Scrolls Salesman",
                    Description = "The heroes are approached by a man who sells magic scrolls.",
                    Execute = async (context) => 
                    {
                        var scrollsForSale = new List<Equipment>();
                        for (int i = 0; i < 3; i++)
                        {
                            scrollsForSale.Add(await context.Treasure.CreateItemAsync("Scroll", value: 100)); 
                        }
                        //TODO: activate shop modal in UI populated with the list of scrolls
                        result.Message = context.EventDescription;
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Potion Salesman",
                    Description = "A man clad in purple robes informs the party that he sells premium potions.",
                    Execute = async (context) => 
                    {
                        var potionsForSale = new List<Potion>();
                        foreach(var potion in AlchemyService.Potions)
                        {
                            if(RandomHelper.RollDie(DiceType.D6) <= 4)
                            {
                                var newPotion = potion.Clone();
                                newPotion.Value -= 20;
                                potionsForSale.Add(newPotion);
                            }
                        }
                        await Task.Yield();
                        //TODO: activate shop modal in UI populated with the list of potions
                        result.Message = context.EventDescription;
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Trinket Salesman",
                    Description = "This old man claims to be selling magic trinkets. " +
                    "Most of the things he carries around seem quite plain and ordinary, but some of them do have a special feel to them. " +
                    "He charges 100 c for each trinket, but you are only allowed to buy 1 per hero. ",
                    Execute = async (context) =>
                    {
                        result.Message = context.EventDescription;
                        if (context.PartyManager.Party == null) return result;
                        foreach (var hero in context.PartyManager.Party.Heroes)
                        {
                            if (context.PartyManager.Party.Coins >= 100 || hero.Coins >= 100)
                            {
                                if (await context.UserRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to purchase a trinket?"))
                                {
                                    await Task.Yield();
                                    if (hero.Coins >= 100) hero.Coins -= 100;
                                    else context.PartyManager.Party.Coins -= 100;

                                    var choiceResult = await context.UserRequest.RequestChoiceAsync("Choose a trinket.", new List<string>() { "Ring", "Amulet" });
                                    await Task.Yield();
                                    var trinket = await context.Treasure.CreateItemAsync(choiceResult);

                                    var rollResult = await context.UserRequest.RequestRollAsync("Roll for random result", "1d12");
                                    await Task.Yield();
                                    switch (rollResult.Roll)
                                    {
                                        case <= 5:
                                            trinket = await context.Treasure.GetMagicItemAsync(trinket, includeCurseRoll: false);
                                            break;
                                        case <= 11: 
                                            trinket.Value = 0; 
                                            break;
                                        case >=12:
                                            trinket = context.Treasure.ApplyCurseToItem(trinket);
                                            break;
                                    }
                                    await BackpackHelper.AddItem(hero.Inventory.Backpack, trinket);
                                    await context.Inventory.EquipItemAsync(hero, trinket);
                                }
	                        }
                        }
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Sale!",
                    Description = "There seems to be a settlement-wide sale going on.",
                    Execute = async (context) => 
                    {
                        await Task.Yield();
                        //TODO: apply to shop modal, "All stores sell their items at a 20% discount."
                        result.Message = context.EventDescription;
                        result.ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Sale, -1, removeEndDay: true);
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Fresh Stocks",
                    Description = "All stores have just received their refill and there is plenty to choose from.",
                    Execute = async (context) => 
                    {
                        await Task.Yield();
                        //TODO: apply to shop modal, "All availabilities are modified by +2. If this results in 6, the item is automatically in stock."
                        result.Message = context.EventDescription;
                        result.ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.FreshStocks, -1, removeEndDay: true);
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Settlement Feast",
                    Description = "There is a celebration in the settlement and there are people everywhere." +
                    " If you stay the night, the good mood of the citizens will boost Party Morale.",
                    Execute = async (context) =>
                    {
                        var rollResult = await context.UserRequest.RequestRollAsync("Roll for bed availability", "1d12");
                        await Task.Yield();

                        if (rollResult.Roll >= 9)
                        {
                            // TODO: Logic to force party to leave, no actions can be taken except turning in quests and leveling up
                        }
                        else context.PartyManager.UpdateMorale(2);

                        result.Message = context.EventDescription;
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Side Quest",
                    Description = $"{sideQuest.NarrativeSetup}",
                    Execute = async (context) => 
                    {                        
                        if (context.Quest.ActiveQuest != null 
                            && await context.UserRequest.RequestYesNoChoiceAsync("Does the party want to add this side quest to their current quest?"))
                        {
                            context.Quest.ActiveQuest.SideQuests ??= new List<Quest>();
                            context.Quest.ActiveQuest.SideQuests.Add(sideQuest);
                        }
                        await Task.Yield();
                        result.Message = context.EventDescription;
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Shortage of Goods",
                    Description = "The settlement has not had any trade caravans passing by for weeks, and the stores are nearly empty.",
                    Execute = async (context) =>
                    {
                        await Task.Yield();
                        //TODO: apply to shop modal, " All item availability are modified by -2. If this results in 0, the item is automatically out of stock. As a result, prices have also gone up, resulting in a +10% price modifier on all items."
                        result.Message = context.EventDescription;
                        result.ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.ShortageOfGoods, -1, removeEndDay: true);
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Thief",
                    Description = "The party realizes that a pickpocket has managed to get too close. Your collective purses feel lighter.",
                    Execute = async (context) =>
                    {
                        var rollResult = await context.UserRequest.RequestRollAsync("Roll for stolen coins", "1d100");
                        if (context.PartyManager.Party != null)
                        {
                            var remainingCoinToSteal = rollResult.Roll;
                            if (context.PartyManager.Party.Coins > 0)
                            {
                                int stolenFromParty = Math.Min(remainingCoinToSteal, context.PartyManager.Party.Coins);
                                context.PartyManager.Party.Coins -= stolenFromParty;
                                remainingCoinToSteal -= stolenFromParty;
                            }

                            if (remainingCoinToSteal > 0)
                            {
                                foreach (var hero in context.PartyManager.Party.Heroes.OrderBy(h => h.Coins))
                                {
                                    if (remainingCoinToSteal <= 0) break;
                                    int stolenFromHero = Math.Min(remainingCoinToSteal, hero.Coins);
                                    hero.Coins -= stolenFromHero;
                                    remainingCoinToSteal -= stolenFromHero;
                                }
                            }
                        }
                        await Task.Yield();
                        result.Message = $"{rollResult.Roll} coins have been stolen from the party!";
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Assassination attempt",
                    Description = "It seems someone is holding a grudge against the heroes.",
                    //TODO:"All other heroes that has has done the same activity that day may join the fight (i.e. been to the same guild, been to the same store etc). " +
                    //TODO:"The heroes will not die, but instead be nursed back to 1 HP by the locals. The bandits may be searched.",
                    Execute = async (context) => 
                    {  
                        var heroesList = context.PartyManager.Party.Heroes;
                        heroesList.Shuffle();
                        var heroAttacked = heroesList[0];

                        var newQuest = new Quest() 
                        { 
                            SetupActions = new List<QuestSetupAction>()
                            {
                                new QuestSetupAction
                                {
                                    ActionType = QuestSetupActionType.SetRoom,
                                    Parameters = new Dictionary<string, string>()
                                    {
                                        { "RoomName", "City Street" }
                                    }
                                },new QuestSetupAction
                                {
                                    ActionType = QuestSetupActionType.PlaceHeroes,
                                    Parameters = new Dictionary<string, string>()
                                    {
                                        { "PlacementRule", "RandomEdge" },
                                        { "Arrangement", "Adjacent" }
                                    }
                                },
                                new QuestSetupAction
                                {
                                    ActionType = QuestSetupActionType.SpawnMonster,
                                    Parameters = new Dictionary<string, string>()
                                    {
                                        { "Name", "Bandit" },
                                        { "Count", RandomHelper.RollDie(DiceType.D4).ToString() },
                                        { "Weapons", "Shortsword" },
                                        { "PlacementRule", "RandomEdge" },
                                        { "PlacementArgs", "Adjacent" }
                                    }
                                },
                            }
                        };

                        context.Quest.StartIndividualQuest(heroAttacked, newQuest);
                        await Task.Yield();
                        result.Message = context.EventDescription;
                        return result;
                    }
                },
                new SettlementEvent
                {
                    Name = "Curse!",
                    Description = "While walking down the street, a gnarly old woman suddenly points her finger at the party and screams: \"Heretics! I curse thee!\".",
                    Execute = async (context) => 
                    {
                        var curseEffect = StatusEffectService.GetRandomCurseEffect();
                        curseEffect.RemoveEndOfDungeon = true;
                        foreach (var hero in context.PartyManager.Party.Heroes)
                        {
                            await StatusEffectService.AttemptToApplyStatusAsync(hero, curseEffect, context.PowerActivation);
                        }
                        result.Message = context.EventDescription;
                        return result;
                    }
                }
            };
        }

        public async Task ProcessSettlementEventAsync()
        {
            var events = GetSettlementEvents();
            var randomEvent = events[RandomHelper.GetRandomNumber(0, events.Count - 1)];
            if(randomEvent.Execute != null)
            {
                var eventContext = new SettlementEventContext( _partyManager, _userRequest, _treasure, _inventory, _quest, _powerActivation)
                {
                    EventDescription = randomEvent.Description
                };
                await randomEvent.Execute(eventContext); 
            }
        }
    }
}
