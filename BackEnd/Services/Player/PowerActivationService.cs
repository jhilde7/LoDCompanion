using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.GameData;

namespace LoDCompanion.BackEnd.Services.Player
{
    public enum PowerType
    {
        Perk,
        Prayer
    }

    public class PowerActivationService
    {
        public string ActivatePerk(Hero hero, Perk perk, Character? target = null)
        {
            var effect = new ActiveStatusEffect((StatusEffectType)Enum.Parse(typeof(StatusEffectType), perk.Name.ToString()), -1);
            StatusEffectService.AttemptToApplyStatus(target ?? hero, effect);
            hero.CurrentEnergy--;
            return $"{hero.Name} used {perk.Name}!";
        }

        public string ActivatePrayer(Hero hero, Prayer prayer, Character? target = null)
        {
            var effect = new ActiveStatusEffect((StatusEffectType)Enum.Parse(typeof(StatusEffectType), prayer.Name.Replace(" ", "")), -1);
            StatusEffectService.AttemptToApplyStatus(target ?? hero, effect);
            hero.CurrentEnergy--;
            return $"{hero.Name} prayed for {prayer.Name}!";
        }
    }
}
