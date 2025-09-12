using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Dungeon;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoDCompanion.Code.BackEnd.Services.Player
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
        EnchantItems,
        Gambling,
        HorseRacing,
        IdentifyMagicItem,
        IdentifyPotion,
        LearnPrayer,
        LearnSpell,
        Pray,
        ReadFortune,
        RestRecuperation,
        RangedSkillTraining,
        TendThoseMemories,
        TreatMentalConditions,
        CollectQuestRewards,
        LevelUp,
        StartCrusade,
        RepairEquipment,
        RepairWeapons,
        RepairArmour,
        CheckBounties,
        BlessArmourAndWeapons,
        CombatSkillTraining,
        DodgeTraining,
        PickLocksTraining,
        PerceptionTraining,
        HealTraining,
        ArcaneArtsTraining,
        AlchemyTraining,
        ForagingTraining,
        BattlePrayersTraining,
        AddShieldPadding,
        AddArmourPadding,
        ApplySlayerWeaponTreatment,
        TendFarm,
        CreateAlchemicalRecipe,
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
                    result = await arena.ArenaFighting(hero, result, _userRequest);
                    break;
                case (Bank bank, SettlementActionType.Banking):
                    result = await bank.Banking(hero, result, _userRequest);
                    break;
                case (ServiceLocation, SettlementActionType.BuyingAndSelling):
                    result = service.GetShopInventory(hero, result);
                    break;
                case (SickWard sickWard, SettlementActionType.CureDiseasePoison):
                    result = await sickWard.VisitSickWard(hero, result, _userRequest);
                    break;
                case (Inn inn, SettlementActionType.CreateScroll):
                    result = await Inn.CreateScroll(hero, result, _userRequest);
                    break;
                case (Inn inn, SettlementActionType.EnchantItems): 
                    result = await Inn.EnchantItemAsync(hero, result, _userRequest);
                    break;
                case (FortuneTeller fortuneTeller, SettlementActionType.ReadFortune):
                    result = await fortuneTeller.ReadFortune(hero, result, _userRequest, _powerActivation);
                    break;
                case (Inn inn, SettlementActionType.Gambling):
                    result = await inn.Gamble(hero, result, _userRequest);
                    break;
                case (HorseTrack horseTrack, SettlementActionType.HorseRacing):
                    result = await horseTrack.HorseRacing(hero, result, _userRequest, _treasure);
                    break;
                case (ServiceLocation, SettlementActionType.IdentifyMagicItem):
                    result = await Scryer.IdentifyMagicItem(hero, result, _userRequest);
                    break;
                case (ServiceLocation, SettlementActionType.IdentifyPotion):
                    result = await GeneralStore.IdentifyPotion(hero, result, _userRequest);
                    break;
                case (ServiceLocation, SettlementActionType.LevelUp):
                    result = await LevelupHero(hero, result);
                    break;
                case (Temple temple, SettlementActionType.Pray):
                    result = await Temple.Pray(hero, temple, result, _userRequest, _powerActivation);
                    break;
                case (Inn inn, SettlementActionType.RestRecuperation):
                    result = await inn.RestRecuperation(hero.Party, result, _userRequest);
                    break;
                case (TheAsylum asylum, SettlementActionType.TreatMentalConditions):
                    result = await asylum.TreatMentalConditions(hero, result, _userRequest);
                    break;
                case (TheDarkGuild guild, SettlementActionType.CombatSkillTraining):
                    result = guild.Train(hero, result, Skill.CombatSkill);
                    break;
                case (TheDarkGuild guild, SettlementActionType.RangedSkillTraining):
                    result = guild.Train(hero, result, Skill.RangedSkill);
                    break;
                case (TheDarkGuild guild, SettlementActionType.PickLocksTraining):
                    result = guild.Train(hero, result, Skill.PickLocks);
                    break;
                case (TheDarkGuild guild, SettlementActionType.PerceptionTraining):
                    result = guild.Train(hero, result, Skill.Perception);
                    break;
                case (FightersGuild guild, SettlementActionType.CombatSkillTraining):
                    result = guild.Train(hero, result, Skill.CombatSkill);
                    break;
                case (FightersGuild guild, SettlementActionType.HealTraining):
                    result = guild.Train(hero, result, Skill.Heal);
                    break;
                case (FightersGuild guild, SettlementActionType.DodgeTraining):
                    result = guild.Train(hero, result, Skill.Dodge);
                    break;
                case (FightersGuild guild, SettlementActionType.CheckBounties):
                    result = guild.CheckBounties(hero, result);
                    break;
                case (FightersGuild guild, SettlementActionType.AddShieldPadding):
                    result = await guild.AddShieldPadding(hero, result, _userRequest);
                    break;
                case (FightersGuild guild, SettlementActionType.AddArmourPadding):
                    result = await guild.AddArmourPadding(hero, result, _userRequest);
                    break;
                case (FightersGuild guild, SettlementActionType.ApplySlayerWeaponTreatment):
                    result = await guild.ApplySlayerWeaponTreatment(hero, result, _userRequest);
                    break;
                case (WizardsGuild guild, SettlementActionType.LearnSpell):
                    result = await guild.LearnSpell(hero, result, _userRequest);
                    break;
                case (WizardsGuild guild, SettlementActionType.ChargeMagicItem):
                    result = await guild.ChargeMagicItem(hero, result, _userRequest);
                    break;
                case (WizardsGuild guild, SettlementActionType.ArcaneArtsTraining):
                    result = guild.Train(hero, result, Skill.ArcaneArts);
                    break;
                case (WizardsGuild guild, SettlementActionType.HealTraining):
                    result = guild.Train(hero, result, Skill.Heal);
                    break;
                case (WizardsGuild guild, SettlementActionType.PerceptionTraining):
                    result = guild.Train(hero, result, Skill.Perception);
                    break;
                case (AlchemistsGuild guild, SettlementActionType.AlchemyTraining):
                    result = guild.Train(hero, result, Skill.Alchemy);
                    break;
                case (AlchemistsGuild guild, SettlementActionType.HealTraining):
                    result = guild.Train(hero, result, Skill.Heal);
                    break;
                case (AlchemistsGuild guild, SettlementActionType.PerceptionTraining):
                    result = guild.Train(hero, result, Skill.Perception);
                    break;
                case (RangersGuild guild, SettlementActionType.CombatSkillTraining):
                    result = guild.Train(hero, result, Skill.CombatSkill);
                    break;
                case (RangersGuild guild, SettlementActionType.RangedSkillTraining):
                    result = guild.Train(hero, result, Skill.RangedSkill);
                    break;
                case (RangersGuild guild, SettlementActionType.DodgeTraining):
                    result = guild.Train(hero, result, Skill.Dodge);
                    break;
                case (RangersGuild guild, SettlementActionType.HealTraining):
                    result = guild.Train(hero, result, Skill.Heal);
                    break;
                case (RangersGuild guild, SettlementActionType.ForagingTraining):
                    result = guild.Train(hero, result, Skill.Foraging);
                    break;
                case (TheInnerSanctum guild, SettlementActionType.LearnPrayer):
                    result = await guild.LearnPrayer(hero, result, _userRequest);
                    break;
                case (TheInnerSanctum guild, SettlementActionType.BlessArmourAndWeapons):
                    result = await guild.BlessArmourAndWeapons(hero, result, _userRequest);
                    break;
                case (TheInnerSanctum guild, SettlementActionType.StartCrusade):
                    result = guild.StartCrusade(hero, result);
                    break;
                case (TheInnerSanctum guild, SettlementActionType.CombatSkillTraining):
                    result = guild.Train(hero, result, Skill.CombatSkill);
                    break;
                case (TheInnerSanctum guild, SettlementActionType.DodgeTraining):
                    result = guild.Train(hero, result, Skill.Dodge);
                    break;
                case (TheInnerSanctum guild, SettlementActionType.BattlePrayersTraining):
                    result = guild.Train(hero, result, Skill.BattlePrayers);
                    break;
                case (Estate estate, SettlementActionType.RangedSkillTraining):
                    var archeryRange = (ArcheryRange?)estate.FurnishedRooms.FirstOrDefault(r => r is ArcheryRange);
                    if (archeryRange != null && archeryRange.IsOwned)
                    {
                        result = archeryRange.Train(hero, result);
                    }
                    else
                    {
                        result.Message = "There is no Estate or Archery Range in this settlement.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Estate estate, SettlementActionType.TendFarm):
                    var farm = (Farm?)estate.FurnishedRooms.FirstOrDefault(r => r is Farm);
                    if (farm != null && farm.IsOwned)
                    {
                        result = await farm.TendFarmAsync(hero, result);
                    }
                    else
                    {
                        result.Message = "There is no Estate or Farm in this settlement.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Estate estate, SettlementActionType.CombatSkillTraining):
                    var trainingGrounds = (TrainingGrounds?)estate.FurnishedRooms.FirstOrDefault(r => r is TrainingGrounds);
                    if (trainingGrounds != null && trainingGrounds.IsOwned)
                    {
                        result = trainingGrounds.Train(hero, result, Skill.CombatSkill);
                    }
                    else
                    {
                        result.Message = "There is no Estate or Training Grounds in this settlement.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Estate estate, SettlementActionType.DodgeTraining):
                    trainingGrounds = (TrainingGrounds?)estate.FurnishedRooms.FirstOrDefault(r => r is TrainingGrounds);
                    if (trainingGrounds != null && trainingGrounds.IsOwned)
                    {
                        result = trainingGrounds.Train(hero, result, Skill.Dodge);
                    }
                    else
                    {
                        result.Message = "There is no Estate or Training Grounds in this settlement.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Estate estate, SettlementActionType.CreateScroll):
                    var wizardsStudy = (WizardsStudy?)estate.FurnishedRooms.FirstOrDefault(r => r is WizardsStudy);
                    if (wizardsStudy != null && wizardsStudy.IsOwned)
                    {
                        result = await wizardsStudy.CreateScroll(hero, result, _userRequest);
                    }
                    else
                    {
                        result.Message = "There is no Estate or Wizards Study in this settlement.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Estate estate, SettlementActionType.EnchantItems):
                    wizardsStudy = (WizardsStudy?)estate.FurnishedRooms.FirstOrDefault(r => r is WizardsStudy);
                    if (wizardsStudy != null && wizardsStudy.IsOwned)
                    {
                        result = await wizardsStudy.EnchantItemAsync(hero, result, _userRequest);
                    }
                    else
                    {
                        result.Message = "There is no Estate or Wizards Study in this settlement.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Estate estate, SettlementActionType.Pray):
                    var shrine = (Shrine?)estate.FurnishedRooms.FirstOrDefault(r => r is Shrine);
                    if (shrine != null && shrine.IsOwned)
                    {
                        result = await shrine.Pray(hero, estate.Settlement, result, _userRequest, _powerActivation);
                    }
                    else
                    {
                        result.Message = "There is no Estate or Shrine in this settlement.";
                        result.WasSuccessful = false;
                    }
                    break;
                case (Estate estate, SettlementActionType.CreateAlchemicalRecipe):
                    var alchemistLab = (AlchemistLab?)estate.FurnishedRooms.FirstOrDefault(r => r is AlchemistLab);
                    if (alchemistLab != null && alchemistLab.IsOwned)
                    {
                        result = await alchemistLab.CreateAlchemicalRecipeAsync(hero, result, _userRequest);
                    }
                    else
                    {
                        result.Message = "There is no Estate or AlchemistLab in this settlement.";
                        result.WasSuccessful = false;
                    }
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

                var availablePerkList = _levelUp.GetPerkCategoryAtLevelup(hero.ProfessionName, hero.Level)?.Where(p => !hero.Perks.Contains(p)).ToList();
                if (availablePerkList != null && availablePerkList.Any())
                {
                    var perkChoiceRequest = await _userRequest.RequestChoiceAsync("Choose a perk.", availablePerkList, p => $"{p.Name.ToString()}, Effect: {p.Effect}");
                    await Task.Yield();
                    var selectedPerk = perkChoiceRequest.SelectedOption;

                    if (selectedPerk != null)
                    {
                        _levelUp.AttemptToSelectPerk(hero, hero.Levelup, selectedPerk, out string error);
                        result.Message += $"\n{hero.Name} can now use: {selectedPerk.ToString()}";
                    }
                }

                var availableTalentsList = _levelUp.GetTalentCategoryAtLevelup(hero.ProfessionName, hero.Level);
                if (availableTalentsList != null && availableTalentsList.Any())
                {
                    var talentChoiceRequest = await _userRequest.RequestChoiceAsync("Choose a talent.", availableTalentsList, t => $"{t.Name.ToString()}, Description: {t.Description}");
                    await Task.Yield();
                    var selectedTalent = talentChoiceRequest.SelectedOption;

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
