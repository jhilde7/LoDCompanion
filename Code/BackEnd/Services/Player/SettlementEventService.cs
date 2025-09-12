using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Dungeon;
using LoDCompanion.Code.BackEnd.Services.Game;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Player
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
        private readonly CombatManagerService _combatManager;
        private readonly UserRequestService _userRequest = new UserRequestService();
        private readonly TreasureService _treasure = new TreasureService();
        private readonly InventoryService _inventory = new InventoryService();
        private readonly PowerActivationService _powerActivation = new PowerActivationService();
        private readonly QuestService _quest = new QuestService();

        public SettlementEventService(
            PartyManagerService partyManager,
            CombatManagerService combatManager)
        {
            _partyManager = partyManager;
            _combatManager = combatManager;
        }

        public List<SettlementEvent> GetSettlementEventsAsync()
        {
            var sideQuest = _quest.GetRandomSideQuest();
            var result = new SettlementEventResult();
            return new List<SettlementEvent>
            {
                new SettlementEvent
                {
                    Name = "Stray dog",
                    Description = "Nothing special happens",
                    //TODO: add on for companions
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
                        var trinketPrice = 100;
                        result.Message = context.EventDescription;
                        if (context.PartyManager.Party == null) return result;
                        foreach (var hero in context.PartyManager.Party.Heroes)
                        {
                            if (context.PartyManager.Party.Coins >= trinketPrice || hero.Coins >= trinketPrice)
                            {
                                if (await context.UserRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to purchase a trinket?"))
                                {
                                    await Task.Yield();
                                    if (hero.Coins >= trinketPrice) hero.Coins -= trinketPrice;
                                    else context.PartyManager.Party.Coins -= trinketPrice;

                                    var choiceResult = await context.UserRequest.RequestChoiceAsync("Choose a trinket type.", new List<string>() { "Ring", "Amulet" }, t => t);
                                    await Task.Yield();
                                    var trinket = new Equipment();
                                    if (choiceResult.SelectedOption != null) trinket = await context.Treasure.CreateItemAsync(choiceResult.SelectedOption);

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

                        await context.Quest.StartIndividualQuestAsync(heroAttacked, newQuest, _combatManager);
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
            var events = GetSettlementEventsAsync();
            events.Shuffle();
            var randomEvent = events[0];
            if(randomEvent.Execute != null)
            {
                var eventContext = new SettlementEventContext( _partyManager, _userRequest, _treasure, _inventory, _quest, _powerActivation)
                {
                    EventDescription = randomEvent.Description
                };
                await randomEvent.Execute(eventContext); 
            }
        }

        public async Task EstateGhostlyEvent(Party party, Estate estate)
        {
            var roll = RandomHelper.RollDie(DiceType.D10);

            if (roll < 7)
            {
                return; // No event occurs
            }

            switch (roll)
            {
                case 1: // The Family Heirlooms

                    List<Equipment> wonderfulTreasures = [.. await _treasure.GetWonderfulTreasureAsync(), .. await _treasure.GetWonderfulTreasureAsync()];
                    foreach (var treasure in wonderfulTreasures)
                    {
                        if (treasure != null)
                        {
                            await BackpackHelper.AddItem(party.Heroes[0].Inventory.Backpack, treasure);
                        }
                    }
                    foreach (var hero in party.Heroes)
                    {
                        hero.CurrentEnergy -= 1;
                    }
                    break;

                case 2: // Guardian Spirits
                    foreach (var hero in party.Heroes)
                    {
                        hero.CurrentLuck += 1;
                    }
                    break;

                case 3: // The Hidden Treasure
                    var quest = new Quest() { Name = "The Hidden Treasure", IsSideQuest = true }; ;
                    if (_quest.ActiveQuest != null && quest != null)
                    {
                        _quest.ActiveQuest.SideQuests ??= new();
                        _quest.ActiveQuest.SideQuests.Add(quest); 
                    }
                    break;

                case 4: // Spiritual Guides
                    foreach (var hero in party.Heroes.Where(h => h.GetSkill(Skill.CombatSkill) > h.GetSkill(Skill.RangedSkill)))
                    {
                        var ghostlyEffect = new ActiveStatusEffect(StatusEffectType.SpiritualGuides, -1, skillBonus: (Skill.CombatSkill, 5), removeEndOfDungeon: true);
                        hero.ActiveStatusEffects.Add(ghostlyEffect);
                    }
                    foreach (var hero in party.Heroes.Where(h => h.GetSkill(Skill.CombatSkill) <= h.GetSkill(Skill.RangedSkill)))
                    {
                        var ghostlyEffect = new ActiveStatusEffect(StatusEffectType.SpiritualGuides, -1, skillBonus: (Skill.RangedSkill, 5), removeEndOfDungeon: true);
                        hero.ActiveStatusEffects.Add(ghostlyEffect);
                    }
                    break;

                case 5: // Protector
                    foreach (var hero in party.Heroes)
                    {
                        if (hero.ProfessionName == ProfessionName.Wizard)
                        {
                            var protectorEffect = new ActiveStatusEffect(StatusEffectType.GhostlyProtector, -1, removeEndOfDungeon: true);
                            hero.ActiveStatusEffects.Add(protectorEffect);
                        }
                    }
                    break;

                case 6: // The Grieving Mother
                    quest = _quest.GetQuestByName("The Grieving Mother");
                    if (_quest.ActiveQuest != null && quest != null)
                    {
                        _quest.ActiveQuest.SideQuests ??= new();
                        _quest.ActiveQuest.SideQuests.Add(quest);
                    }
                    break;

                case 7: // Angered Ghost
                    var farm = estate.FurnishedRooms.OfType<Farm>().FirstOrDefault();
                    if (farm != null)
                    {
                        farm.DungeonsUntilUsable = 1;
                    }
                    break;

                case 8: // Restless Night
                    foreach (var hero in party.Heroes)
                    {
                        hero.CurrentEnergy = Math.Max(0, hero.CurrentEnergy - 2);
                    }
                    break;

                case 9: // Lost Item
                    var affectedHero = party.Heroes[RandomHelper.GetRandomNumber(0, party.Heroes.Count - 1)];
                    var items = new List<Equipment?>();
                    items.AddRange(affectedHero.Inventory.GetAllWeaponsArmour());
                    items.AddRange(affectedHero.Inventory.Backpack.Where(i => i?.Name == "Ration"));
                    items.Shuffle();

                    if (items.Any())
                    {
                        var lostItem = items[0];
                        if (lostItem != null)
                        {
                            await _inventory.UnequipItemAsync(affectedHero, lostItem);
                            estate.LostItem = (affectedHero, lostItem);
                            affectedHero.Inventory.Backpack.Remove(lostItem);
                        }
                    }
                    break;

                case 10: // The Curse
                    foreach (var hero in party.Heroes)
                    {
                        var curse = StatusEffectService.GetRandomCurseEffect();
                        curse.RemoveEndOfDungeon = true;
                        hero.ActiveStatusEffects.Add(curse);
                    }
                    break;
            }
        }
    }
}
