using LoDCompanion.Models.Character;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Player
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
        public string ApplyBandage(Hero healer, Hero target)
        {
            Models.Equipment? bandage = null;
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
            int healRoll = RandomHelper.RollDie("D100");
            if (healRoll > healer.GetSkill(Skill.Heal))
            {
                return $"{healer.Name}'s attempt to heal {target.Name} failed, and the bandage was wasted.";
            }

            // Determine HP restored based on bandage type.
            int hpGained = 0;
            if (bandage.Name.Contains("old rags")) hpGained = RandomHelper.RollDie("D4");
            else if (bandage.Name.Contains("linen")) hpGained = RandomHelper.RollDie("D8");
            else if (bandage.Name.Contains("Herbal wrap")) hpGained = RandomHelper.RollDie("D10");

            // Apply healing to the target.
            target.CurrentHP = Math.Min(target.GetStat(BasicStat.HitPoints), target.CurrentHP + hpGained);

            return $"{healer.Name} successfully heals {target.Name} for {hpGained} HP.";
        }
    }
}
