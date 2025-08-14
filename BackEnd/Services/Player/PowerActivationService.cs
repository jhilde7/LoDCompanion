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
            if (perk.ActiveStatusEffect != null)
            {
                var effect = perk.ActiveStatusEffect;
                StatusEffectService.AttemptToApplyStatus(target ?? hero, effect); 
            }
            hero.CurrentEnergy--;
            return $"{hero.Name} used {perk.Name}!";
        }

        public string ActivatePrayer(Hero hero, Prayer prayer, Character? target = null)
        {
            var effect = prayer.ActiveStatusEffect;
            StatusEffectService.AttemptToApplyStatus(target ?? hero, effect);
            hero.CurrentEnergy--;
            return $"{hero.Name} prayed for {prayer.Name}!";
        }
    }
}
