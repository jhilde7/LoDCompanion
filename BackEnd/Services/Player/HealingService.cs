using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class HealingService
    {
        public HealingService() { }

        /// <summary>
        /// Attempts to apply a bandage from one hero to a target hero.
        /// </summary>
        /// <param name="healer">The hero applying the bandage.</param>
        /// <param name="target">The hero receiving the healing.</param>
        /// <returns>A string describing the outcome.</returns>
        public async Task<string> ApplyBandageAsync(Hero healer, Hero target, UserRequestService userRequest, PowerActivationService activation)
        {
            Equipment? bandage = null;
            if (!healer.Inventory.QuickSlots.Any())
            {
                bandage = healer.Inventory.Backpack.FirstOrDefault(i => i.Name.Contains("Bandage"));
            }
            
            if (bandage == null)
            {
                return $"{healer.Name} has no bandages in their quick slots.";
            }

            // Consume one use of the bandage.
            bandage.Quantity--;
            if (bandage.Quantity <= 0)
            {
                healer.Inventory.QuickSlots.Remove(bandage);
            }

            // Perform a Heal skill check.
            int healRoll = RandomHelper.RollDie(DiceType.D100);
            if (healRoll > healer.GetSkill(Skill.Heal))
            {
                return $"{healer.Name}'s attempt to heal {target.Name} failed, and the bandage was wasted.";
            }

            // Determine HP restored based on bandage type.
            int hpGained = 0;
            if (bandage.Name.Contains("old rags")) hpGained = RandomHelper.RollDie(DiceType.D4);
            else if (bandage.Name.Contains("linen")) hpGained = RandomHelper.RollDie(DiceType.D8);
            else if (bandage.Name.Contains("Herbal wrap")) hpGained = RandomHelper.RollDie(DiceType.D10);

            var healerPerk = healer.Perks.FirstOrDefault(p => p.Name == PerkName.Healer);
            if(healerPerk != null && healer.CurrentEnergy > 0)
            {
                if(await userRequest.RequestYesNoChoiceAsync($"Does {healer.Name} wish to activate {healerPerk.Name.ToString()} which will add 3 to the healing roll of {hpGained}?"))
                {
                    if(await activation.ActivatePerkAsync(healer, healerPerk))
                    {
                        hpGained += 3;
                    }
                }
            }

            // Apply healing to the target.
            target.Heal(hpGained);

            return $"{healer.Name} successfully heals {target.Name} for {hpGained} HP.";
        }
    }
}
