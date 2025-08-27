using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

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
        public bool WasSuccessful { get; set; }
        public int ActionCost { get; set; }
        public int ArenaWinnings { get; set; }
        public List<Equipment>? FoundItems { get; set; }

        public SettlementActionResult (SettlementActionType action)
        {
            Action = action;
        }
    }

    public class SettlementActionService
    {
        private readonly SettlementState _settlement;
        private readonly UserRequestService _userRequest;
        private readonly TreasureService _treasure;

        public SettlementActionService(
            SettlementState settlement, 
            UserRequestService userRequestService,
            TreasureService treasureService)
        {
            _settlement = settlement;
            _userRequest = userRequestService;
            _treasure = treasureService;
        }

        public async Task<SettlementActionResult> PerformSettlementActionAsync(Hero hero, SettlementActionType action, SettlementService serviceLocation)
        {
            var result = new SettlementActionResult(action);
            result.ActionCost = SettlementActionCost(action);

            if (_settlement.BusyHeroes.ContainsKey(hero.Id))
            {
                result.Message = $"{hero.Name} is busy with a multi-day task.";
                result.WasSuccessful = false;
                return result;
            }

            switch (action)
            {
                case SettlementActionType.ArenaFighting:
                    if (hero.Coins < 50 || hero.Party.Coins < 50)
                    {
                        result.Message = $"{hero.Name} does nto have enough coin to participate";
                        result.WasSuccessful = false;
                        return result;
                    }
                    else return await ArenaFighting(hero, result);
                case SettlementActionType.Banking: 
                    break;
                case SettlementActionType.BuyDog: 
                    break;
                case SettlementActionType.BuyFamiliar: 
                    break;
                case SettlementActionType.BuySellArmour: 
                    break;
                case SettlementActionType.BuySellEquipment: 
                    break;
                case SettlementActionType.BuyIngredients: 
                    break;
                case SettlementActionType.BuySellWeapons: 
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

            return result;
        }

        private async Task<SettlementActionResult> ArenaFighting(Hero hero, SettlementActionResult result)
        {
            var totalCoins = hero.Coins + hero.Party.Coins;
            var choiceString = new List<string>() { "50" };
            if (totalCoins >= 75) { choiceString.Add("75"); }
            if (totalCoins >= 100) { choiceString.Add("100"); }
            if (totalCoins >= 125) { choiceString.Add("125"); }
            if (totalCoins >= 150) { choiceString.Add("150"); }
            if (totalCoins >= 175) { choiceString.Add("175"); }
            if (totalCoins >= 200) { choiceString.Add("200"); }

            var choiceResult = await _userRequest.RequestChoiceAsync("Choose your entry fee level.", choiceString);
            var bet = int.Parse(choiceResult);
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
}
