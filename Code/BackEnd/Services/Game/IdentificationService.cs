using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Player;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Game
{
    public class IdentificationService
    {
        private readonly UserRequestService _diceRoll;
        private readonly PowerActivationService _powerActivation;
        private readonly PartyManagerService _partyManager;
        public IdentificationService(UserRequestService userRequestService, PowerActivationService powerActivationService, PartyManagerService partyManagerService) 
        { 
            _diceRoll = userRequestService;
            _powerActivation = powerActivationService;
            _partyManager = partyManagerService;

            BackpackHelper.OnIdentifyItemAsync += HandleIdentifyItemAsync;
        }

        public void Dispose()
        {
            BackpackHelper.OnIdentifyItemAsync -= HandleIdentifyItemAsync;
        }

        private async Task HandleIdentifyItemAsync(Equipment item)
        {
            if (_partyManager.Party != null) await IdentifyItemAsync(_partyManager.Party.Heroes[0], item);
        }

        /// <summary>
        /// Attempts to identify an item using the parties relevant skill.
        /// </summary>
        public async Task<string> IdentifyItemAsync(Hero hero, Equipment item)
        {
            int skillValue;
            string skillUsed;
            var highestSkilled = hero;

            if (!item.Identified && hero.Party != null)
            {
                if (item is Potion)
                {
                    highestSkilled = hero.Party.Heroes
                        .OrderBy(h => h.GetSkill(Skill.Alchemy))
                        .FirstOrDefault();
                    if (highestSkilled != null)
                    {
                        skillValue = highestSkilled.GetSkill(Skill.Alchemy);
                        skillUsed = Skill.Alchemy.ToString();
                        if (await _powerActivation.RequestPerkActivationAsync(hero, PerkName.Connoisseur))
                        {
                            skillValue += 10;
                        }
                    }
                    else
                    {
                        skillValue = hero.GetSkill(Skill.Alchemy);
                        skillUsed = Skill.Alchemy.ToString();
                    }
                }
                else
                {
                    highestSkilled = hero.Party.Heroes
                        .OrderBy(h => h.GetSkill(Skill.ArcaneArts))
                        .FirstOrDefault();
                    if (highestSkilled != null)
                    {
                        skillValue = highestSkilled.GetSkill(Skill.ArcaneArts);
                        skillUsed = Skill.ArcaneArts.ToString();
                        var inTunePerk = hero.Perks.FirstOrDefault(p => p.Name == PerkName.InTuneWithTheMagic);
                        if (inTunePerk != null)
                        {
                            int focusPoints = await _powerActivation.RequestInTuneWithTheMagicActivationAsync(hero, inTunePerk);
                            skillValue += focusPoints * 10;

                            var miscastResult = await _diceRoll.RequestRollAsync("Roll for miscast check.", "1d100");
                            await Task.Yield();
                            // Check for miscast if InTuneWithTheMagic was used
                            if (focusPoints > 0)
                            {
                                int miscastThreshold = 95 - focusPoints * 5;
                                if (miscastResult.Roll >= miscastThreshold)
                                {
                                    int sanityLoss = (int)Math.Ceiling((double)RandomHelper.RollDie(DiceType.D6) / 2);
                                    await hero.TakeSanityDamage(sanityLoss, (new FloatingTextService(), hero.Position), _powerActivation);
                                    return $"Miscast! While trying to identify the item, {hero.Name} loses {sanityLoss} sanity!";
                                }
                            }
                        } 
                    }
                    else
                    {
                        skillValue = hero.GetSkill(Skill.ArcaneArts);
                        skillUsed = Skill.ArcaneArts.ToString();
                    }
                } 
            }
            else
            {
                return $"{item.Name} does not appear to be magical and does not need to be identified.";
            }

            var rollResult = await _diceRoll.RequestRollAsync($"Roll for {skillUsed} skill check.", "1d100");
            await Task.Yield();

            if (rollResult.Roll <= skillValue)
            {
                item.Identified = true;
                if (item is Potion potion)
                {
                    return $"item successfully identified: {potion.ToString()}!";
                }
                else
                {
                    return $"item successfully identified: {item.Name}!";
                }
            }
            else
            {
                item.IdentifyAttempted = true;
                return $"{hero.Name} failed to discern the properties of the {item.Name}.";
            }
        }
    }

}
