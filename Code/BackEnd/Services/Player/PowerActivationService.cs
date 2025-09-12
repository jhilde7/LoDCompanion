using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Dungeon;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Player
{
    public enum PowerType
    {
        Perk,
        Prayer
    }

    public class PowerActivationService
    {
        public event Func<ActorType, Task<bool>>? OnForceNextActorType;
        public event Func<int, Task<bool>>? OnUpdateMorale;
        public event Func<int, Task<bool>>? OnUpdateThreat;

        public PowerActivationService()
        {

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
                        success = OnForceNextActorType != null ? await OnForceNextActorType.Invoke(ActorType.Hero): false;
                        break;
                    case PerkName.KeepCalmAndCarryOn:
                        success = OnUpdateMorale != null ? await OnUpdateMorale.Invoke(2) : false;
                        break;
                    case PerkName.LuckyGit:
                        success = OnUpdateThreat != null ? await OnUpdateThreat.Invoke(-2) : false;
                        break;
                    case PerkName.GodsFavorite:
                        var resultRoll = await new UserRequestService().RequestRollAsync("Roll for threat decrease amount.", "1d6");
                        await Task.Yield();
                        success = OnUpdateThreat != null ? await OnUpdateThreat.Invoke(-resultRoll.Roll) : false;
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
                var choiceResult = await new UserRequestService().RequestYesNoChoiceAsync($"Does {hero.Name} wish to use their perk {perk.ToString()}");
                await Task.Yield();
                if (choiceResult)
                {
                    return await ActivatePerkAsync(hero, perk);
                }
            }

            return false;
        }

        public async Task<int> RequestInTuneWithTheMagicActivationAsync(Hero hero, Perk perk)
        {
            var choiceResult = await new UserRequestService().RequestYesNoChoiceAsync($"Does {hero.Name} wish to use their perk {perk.Name}?");
            await Task.Yield();
            if (choiceResult)
            {
                // Use SpellCastingService to get focus points
                var castingOptions = await new SpellCastingService().RequestCastingOptionsAsync(hero, new Spell { Name = "Identify Item" });
                if (!castingOptions.WasCancelled)
                {
                    await ActivatePerkAsync(hero, perk);
                    return castingOptions.FocusPoints;
                }
            }
            return 0; // Return 0 focus points if the user cancels or doesn't use the perk
        }
    }
}
