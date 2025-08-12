using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Game
{
    public class IdentificationService
    {
        public IdentificationService() { }

        /// <summary>
        /// Attempts to identify an item using the hero's relevant skill.
        /// </summary>
        public string IdentifyItem(Hero hero, Equipment item)
        {
            int skillValue;
            string skillUsed;

            if (item is Potion)
            {
                skillValue = hero.GetSkill(Skill.Alchemy);
                skillUsed = "Alchemy";
            }
            else if (!string.IsNullOrEmpty(item.MagicEffect))
            {
                skillValue = hero.GetSkill(Skill.ArcaneArts);
                skillUsed = "Arcane Arts";
            }
            else
            {
                return $"{item.Name} does not appear to be magical and does not need to be identified.";
            }

            // Example difficulty - this could be based on the item's level or rarity.
            int difficulty = 50;
            int roll = RandomHelper.RollDie(DiceType.D100);

            if (roll <= skillValue - difficulty)
            {
                // In a real implementation, you would set an "IsIdentified = true" flag on the item.
                return $"{hero.Name} successfully identified the {item.Name} using {skillUsed}!";
            }
            else
            {
                return $"{hero.Name} failed to discern the properties of the {item.Name}.";
            }
        }
    }
}
