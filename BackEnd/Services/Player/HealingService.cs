using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class HealingService
    {
        private readonly UserRequestService _userRequest;
        public HealingService(UserRequestService userRequestService) 
        { 
            _userRequest = userRequestService;
        }

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
                // Consume one use of the bandage.
                bandage = BackpackHelper.TakeOneItem(
                    healer.Inventory.QuickSlots, 
                    healer.Inventory.QuickSlots.FirstOrDefault(i => i != null && i.Name.Contains("Bandage"))?? new Equipment()
                    );
            }
            
            if (bandage == null)
            {
                return $"{healer.Name} has no bandages in their quick slots.";
            }

            // Perform a Heal skill check.
            var rollResult = await _userRequest.RequestRollAsync("Roll heal skill check.", "1d100", skill: (healer, Skill.Heal));
            if (rollResult.Roll > healer.GetSkill(Skill.Heal))
            {
                return $"{healer.Name}'s attempt to heal {target.Name} failed, and the bandage was wasted.";
            }

            // Determine HP restored based on bandage type.
            int hpGained = 0;
            if (bandage.Name.Contains("old rags")) hpGained = RandomHelper.RollDie(DiceType.D4);
            else if (bandage.Name.Contains("linen")) hpGained = RandomHelper.RollDie(DiceType.D8);
            else if (bandage.Name.Contains("Herbal wrap")) hpGained = RandomHelper.RollDie(DiceType.D10);

            if (await activation.RequestPerkActivationAsync(healer, PerkName.Healer))
            {
                hpGained += 3;
            }

            // Apply healing to the target.
            target.Heal(hpGained);
            target.CurrentAP--;

            return $"{healer.Name} successfully heals {target.Name} for {hpGained} HP.";
        }
    }
}
