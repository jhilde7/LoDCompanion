using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;

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

    public class SettlementActionService
    {

        public ActionResult PerformSettlementAction(Hero hero, SettlementActionType action, SettlementService serviceLocation)
        {
            var result = new ActionResult();

            switch (action)
            {
                case SettlementActionType.ArenaFighting: 
                    break;
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
