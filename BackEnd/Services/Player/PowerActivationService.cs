using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
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
        private readonly DungeonManagerService _dungeonManager;

        public PowerActivationService(InitiativeService initiativeService, PartyManagerService partyManagerService, DungeonManagerService dungeonManagerService)
        {
            _initiative = initiativeService;
            _partyManager = partyManagerService;
            _dungeonManager = dungeonManagerService;
        }

        public async Task<bool> ActivatePerkAsync(Hero hero, Perk perk, Character? target = null)
        {
            await Task.Yield();
            bool success = false;
            if (hero.Perks.Any(p => p.Name == perk.Name))
            {
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
                    case PerkName.LuckyGit:
                        success = _dungeonManager.UpdateThreat(-2);
                        break;
                    default:
                        success = true;
                        break;
                }
            }

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
