using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class HealResult
    {
        public string Message { get; set; } = string.Empty;
        public Character Healer { get; set; } = new Character();
        public Character HealTarget { get; set; } = new Character();
        public Equipment? HealItem { get; set; }
        public int HealRoll { get; set; }
        public int TargetRoll { get; set; }
        public bool WasSuccessful => HealRoll <= TargetRoll && HealRoll != 0;
        public int AmountHealed { get; set; }
    }

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
        public async Task<HealResult> ApplyBandageAsync(Hero healer, Hero target, UserRequestService userRequest, PowerActivationService activation)
        {
            var result = new HealResult();
            result.Healer = healer;
            result.HealTarget = target;

            result.HealItem = null;
            if (!healer.Inventory.QuickSlots.Any())
            {
                // Consume one use of the bandage.
                result.HealItem = BackpackHelper.TakeOneItem(
                    healer.Inventory.QuickSlots, 
                    healer.Inventory.QuickSlots.FirstOrDefault(i => i != null && i.Name.Contains("Bandage"))?? new Equipment()
                    );
            }
            
            if (result.HealItem == null)
            {
                result.Message = $"{healer.Name} has no bandages in their quick slots.";
                return result;
            }

            // Perform a Heal skill check.
            var rollResult = await _userRequest.RequestRollAsync("Roll heal skill check.", "1d100", skill: (healer, Skill.Heal));
            result.HealRoll = rollResult.Roll;
            result.TargetRoll = healer.GetSkill(Skill.Heal);
            if (!result.WasSuccessful)
            {
                result.Message = $"{healer.Name}'s attempt to heal {target.Name} failed, and the bandage was wasted.";
                return result;
            }

            // Determine HP restored based on bandage type.
            if (result.HealItem.Name.Contains("old rags")) result.AmountHealed = RandomHelper.RollDie(DiceType.D4);
            else if (result.HealItem.Name.Contains("linen")) result.AmountHealed = RandomHelper.RollDie(DiceType.D8);
            else if (result.HealItem.Name.Contains("Herbal wrap")) result.AmountHealed = RandomHelper.RollDie(DiceType.D10);

            if (await activation.RequestPerkActivationAsync(healer, PerkName.Healer))
            {
                result.AmountHealed += 3;
            }

            // Apply healing to the target.
            result.AmountHealed = target.Heal(result.AmountHealed);
            target.CurrentAP--;

            result.Message = $"{result.Healer.Name} successfully heals {result.HealTarget.Name} for {result.AmountHealed} HP.";
            return result;
        }
    }
}
