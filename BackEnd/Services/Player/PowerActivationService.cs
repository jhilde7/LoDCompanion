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
        private readonly InitiativeService _initiative;
        private readonly PartyManagerService _partyManager;

        public PowerActivationService(InitiativeService initiativeService, PartyManagerService partyManagerService)
        {
            _initiative = initiativeService;
            _partyManager = partyManagerService;
        }

        public async Task<bool> ActivatePerkAsync(Hero hero, Perk perk, Character? target = null)
        {
            bool success = false;
            if (perk.ActiveStatusEffect != null)
            {
                var effect = perk.ActiveStatusEffect;
                success = await StatusEffectService.AttemptToApplyStatusAsync(target ?? hero, effect, this) != "Already affected"; 
            }

            switch (perk.Name)
            {
                case PerkName.CallToAction:
                    success = _initiative.ForceNextActorType(ActorType.Hero);
                    break;
                case PerkName.KeepCalmAndCarryOn:
                    success = _partyManager.UpdateMorale(2);
                    break;
            }
            ;
            if (success)
            {
                hero.CurrentEnergy--;
                return true; 
            }
            else return false;
        }

        public async Task<string> ActivatePrayerAsync(Hero hero, Prayer prayer, Character? target = null)
        {
            var effect = prayer.ActiveStatusEffect;
            await StatusEffectService.AttemptToApplyStatusAsync(target ?? hero, effect, this);
            hero.CurrentEnergy--;
            return $"{hero.Name} prayed for {prayer.Name}!";
        }
    }
}
