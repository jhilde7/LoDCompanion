using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

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
        private readonly UserRequestService _userRequest;

        public PowerActivationService(
            InitiativeService initiativeService, 
            PartyManagerService partyManagerService, 
            DungeonManagerService dungeonManagerService,
            UserRequestService userRequestService)
        {
            _initiative = initiativeService;
            _partyManager = partyManagerService;
            _dungeonManager = dungeonManagerService;
            _userRequest = userRequestService;
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
                    case PerkName.GodsFavorite:
                        var resultRoll = await _userRequest.RequestRollAsync("Roll for threat decrease amount.", "1d6");
                        await Task.Yield();
                        success = _dungeonManager.UpdateThreat(-resultRoll.Roll);
                        break;
                    case PerkName.EnergyToMana:
                        var missingMana = hero.GetStat(BasicStat.Mana) - (hero.CurrentMana ?? 0);
                        hero.CurrentMana += (int)MathF.Min(missingMana, 5);
                        success = true;
                        break;
                    default:
                        success = true;
                        break;
                }
            }

            if (success && hero.CurrentEnergy > 0)
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

        public async Task<bool> RequestPerkActivationAsync(Hero hero, PerkName perkName)
        {
            var perk = hero.Perks.FirstOrDefault(p => p.Name == perkName);
            if (perk != null)
            {
                var choiceResult = await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to use their perk {perk.ToString()}");
                await Task.Yield();
                if (choiceResult)
                {
                    return await ActivatePerkAsync(hero, perk);
                }
            }

            return false;
        }
    }
}
