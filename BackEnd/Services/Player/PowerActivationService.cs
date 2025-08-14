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
        public async Task<string> ActivatePerkAsync(Hero hero, Perk perk, Character? target = null)
        {
            if (perk.ActiveStatusEffect != null)
            {
                var effect = perk.ActiveStatusEffect;
                await StatusEffectService.AttemptToApplyStatusAsync(target ?? hero, effect); 
            }
            hero.CurrentEnergy--;
            return $"{hero.Name} used {perk.Name}!";
        }

        public async Task<string> ActivatePrayerAsync(Hero hero, Prayer prayer, Character? target = null)
        {
            var effect = prayer.ActiveStatusEffect;
            await StatusEffectService.AttemptToApplyStatusAsync(target ?? hero, effect);
            hero.CurrentEnergy--;
            return $"{hero.Name} prayed for {prayer.Name}!";
        }
    }
}
