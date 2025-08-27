using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Game;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class SettlementEvent
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Func<PartyManagerService, UserRequestService, string, Task<string>>? Execute { get; set; }
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
            return new List<SettlementEvent>
            {
                new SettlementEvent
                {
                    Name = "Stray dog",
                    Description = "Nothing special happens",
                        //"The party is followed through the streets by a stray dog after receiving a small treat from one of the heroes. It seems that you now are the proud owners of a dog. Randomise what kind of dog, using the Companions' Compendium. If you do not own this compendium, or if you already have a dog, treat this result as: 'Nothing special happens'.",
                    //Execute = (partyManager, userRequest) => { /* Add logic for stray dog event */ }
                },
                new SettlementEvent
                {
                    Name = "Scrolls Salesman",
                    Description = "The heroes are approached by a man who sells magic scrolls.",
                    Execute = async (partyManager, userRequest, eventDescription) => 
                    {
                        var scrollsForSale = new List<Equipment>();
                        for (int i = 0; i < 3; i++)
                        {
                            scrollsForSale.Add(await _treasure.CreateItemAsync("Scroll", value: 100)); 
                        }
                        //TODO: activate shop modal in UI populated with the list of scrolls
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Potion Salesman",
                    Description = "A man clad in purple robes informs the party that he sells premium potions.",
                    Execute = async (partyManager, userRequest, eventDescription) => 
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
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Trinket Salesman",
                    Description = "This old man claims to be selling magic trinkets. " +
                    "Most of the things he carries around seem quite plain and ordinary, but some of them do have a special feel to them. " +
                    "He charges 100 c for each trinket, but you are only allowed to buy 1 per hero. ",
                    Execute = async (partyManager, userRequest, eventDescription) => 
                    {
                        if (partyManager.Party == null) return eventDescription;
                        foreach (var hero in partyManager.Party.Heroes)
                        {
                            if (partyManager.Party.Coins >= 100 || hero.Coins >= 100)
                            {
                                if (await userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to purchase a trinket?"))
                                {
                                    await Task.Yield();
                                    if (hero.Coins >= 100) hero.Coins -= 100;
                                    else partyManager.Party.Coins -= 100;
                                    var choiceResult = await userRequest.RequestChoiceAsync("Choose a trinket.", new List<string>() { "Ring", "Amulet" });
                                    await Task.Yield();
                                    var trinket = new Equipment();
                                    if (choiceResult == "Ring")
                                    {
                                       trinket = await _treasure.CreateItemAsync("Ring");
                                    }
                                    else
                                    {
                                        trinket = await _treasure.CreateItemAsync("Amulet");
                                    }

                                    var rollResult = await userRequest.RequestRollAsync("Roll for random result", "1d12");
                                    await Task.Yield();
                                    switch (rollResult.Roll)
                                    {
                                        case <= 5:
                                            trinket = await _treasure.GetMagicItemAsync(trinket, includeCurseRoll: false);
                                            break;
                                        case <= 11: trinket.Value = 0; break;
                                        case >=12:
                                            trinket = _treasure.ApplyCurseToItem(trinket);
                                            break;
                                    }
                                    await BackpackHelper.AddItem(hero.Inventory.Backpack, trinket);
                                    await _inventory.EquipItemAsync(hero, trinket);
                                }
	                        }
                        }
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Sale!",
                    Description = "There seems to be a settlement-wide sale going on.",
                    Execute = async (partyManager, userRequest, eventDescription) => 
                    {
                        partyManager.Party.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.Sale, -1, removeEndDay: true));
                        await Task.Yield();
                        //TODO: apply to shop modal, "All stores sell their items at a 20% discount."
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Fresh Stocks",
                    Description = "All stores have just received their refill and there is plenty to choose from.",
                    Execute = async (partyManager, userRequest, eventDescription) => 
                    {
                        partyManager.Party.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.FreshStocks, -1, removeEndDay: true));
                        await Task.Yield();
                        //TODO: apply to shop modal, "All availabilities are modified by +2. If this results in 6, the item is automatically in stock."
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Settlement Feast",
                    Description = "There is a celebration in the settlement and there are people everywhere." +
                    " If you stay the night, the good mood of the citizens will boost Party Morale.",
                    Execute = async (partyManager, userRequest, eventDescription) =>
                    {
                        var rollResult = await userRequest.RequestRollAsync("Roll for bed availability", "1d12");
                        await Task.Yield();
                        if (rollResult.Roll >= 9)
                        {
                            // Logic to force party to leave, no actions can be taken except turning in quests and leveling up
                        }
                        else partyManager.UpdateMorale(2);
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Side Quest",
                    Description = $"{sideQuest.NarrativeSetup}",
                    Execute = async (partyManager, userRequest, eventDescription) => 
                    {                        
                        if (_quest.ActiveQuest != null && await userRequest.RequestYesNoChoiceAsync("Does the party want to add this side quest to their current quest?"))
                        {
                            _quest.ActiveQuest.SideQuests ??= new List<Quest>();
                            _quest.ActiveQuest.SideQuests.Add(sideQuest);
                        }
                        await Task.Yield();
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Shortage of Goods",
                    Description = "The settlement has not had any trade caravans passing by for weeks, and the stores are nearly empty.",
                    Execute = async (partyManager, userRequest, eventDescription) =>
                    {
                        partyManager.Party.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ShortageOfGoods, -1, removeEndDay: true));
                        await Task.Yield();
                        //TODO: apply to shop modal, " All item availability are modified by -2. If this results in 0, the item is automatically out of stock. As a result, prices have also gone up, resulting in a +10% price modifier on all items."
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Thief",
                    Description = "The party realizes that a pickpocket has managed to get too close. Your collective purses feel lighter.",
                    Execute = async (partyManager, userRequest, eventDescription) =>
                    {
                        var rollResult = await userRequest.RequestRollAsync("Roll for stolen coins", "1d100");
                        if (partyManager.Party != null)
                        {
                            var remiaingCoin = rollResult.Roll;
                            if (partyManager.Party.Coins >= rollResult.Roll)
                            {
                                partyManager.Party.Coins -= rollResult.Roll; 
                            }
                            else
                            {
                                remiaingCoin -= partyManager.Party.Coins;
                                partyManager.Party.Coins = 0;
                            }
                            if (remiaingCoin > 0)
                            {
                                var heroesByCoin = partyManager.Party.Heroes .OrderBy(h => h.Coins);
                                foreach (var hero in heroesByCoin)
                                {
                                    if (hero.Coins >= remiaingCoin)
                                    {
                                        hero.Coins -= remiaingCoin;
                                    }
                                    else
                                    {
                                        remiaingCoin -= hero.Coins;
                                        hero.Coins = 0;
                                    }
                                } 
	                        }
                        }
                        await Task.Yield();
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Assassination attempt",
                    Description = "It seems someone is holding a grudge against the heroes. " +
                    "All other heroes that has has done the same activity that day may join the fight (i.e. been to the same guild, been to the same store etc). " +
                    "Randomize each bandits' weapon by using the enemy equipment cards. Bandits with ranged weapons will also have daggers. " +
                    "Use the city tile and place the heroes along one board edge and the bandits along the opposite edge. " +
                    "The battle ends when all bandits are dead or all heroes have dropped to 0 HP. " +
                    "The heroes will not die, but instead be nursed back to 1 HP by the locals. The bandits may be searched.",
                    Execute = async (partyManager, userRequest, eventDescription) => 
                    {  
                        var heroesList = partyManager.Party.Heroes;
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

                        _quest.StartIndividualQuest(heroAttacked, newQuest);
                        await Task.Yield();
                        return eventDescription;
                    }
                },
                new SettlementEvent
                {
                    Name = "Curse!",
                    Description = "While walking down the street, a gnarly old woman suddenly points her finger at the party and screams: \"Heretics! I curse thee!\"." +
                    " Roll on the Cursed Items Table once and apply the curse to all heroes until they exit the next dungeon.",
                    Execute = async (partyManager, userRequest, eventDescription) => 
                    {
                        var curseEffect = StatusEffectService.GetRandomCurseEffect();
                        curseEffect.RemoveEndOfDungeon = true;
                        foreach (var hero in partyManager.Party.Heroes)
                        {
                            await StatusEffectService.AttemptToApplyStatusAsync(hero, curseEffect, _powerActivation);
                        }
                        return eventDescription;
                    }
                }
            };
        }

        public async Task ProcessSettlementEventAsync()
        {
            var events = GetSettlementEvents();
            var randomEvent = events[RandomHelper.GetRandomNumber(0, events.Count - 1)];
            if(randomEvent.Execute != null) await randomEvent.Execute(_partyManager, _userRequest, randomEvent.Description);
        }
    }
}
