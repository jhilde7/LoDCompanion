using LoDCompanion.Utilities;
using LoDCompanion.Models.Characters;
using System.Text.Json.Serialization;

namespace LoDCompanion.Services.GameData
{
    public class SpellLookupService
    {
        private readonly GameDataRegistryService _gameData;

        public SpellLookupService(GameDataRegistryService gameData)
        {
            _gameData = gameData;
        }

        public string GetRandomSpellNameByCategory(string category = "All", bool isUndead = false)
        {
            string spell = "";
            // Using the new Utilities.RandomHelper.RandomNumber method
            int roll = RandomHelper.GetRandomNumber(1, 100);

            switch (category)
            {
                case "All":
                    return roll switch
                    {
                        <= 4 => "Fake Death",
                        <= 8 => "Flare",
                        <= 12 => "Gust of Wind",
                        <= 16 => "Hand of Death",
                        <= 22 => "Light Healing",
                        <= 29 => "Protective Shield",
                        <= 33 => "Slip",
                        <= 35 => "Blur",
                        <= 37 => "Fist of Iron",
                        <= 39 => "Magic Scribbles",
                        <= 41 => "Open Lock",
                        <= 43 => "Seal Door",
                        <= 45 => "Silence",
                        <= 47 => "Strengthen Body",
                        <= 49 => "Summon Lesser Demon",
                        <= 51 => "Confuse",
                        <= 53 => "Control Undead",
                        <= 55 => "Corruption",
                        <= 57 => "Enchant Item",
                        <= 59 => "Healing",
                        <= 61 => "Ice Pikes",
                        <= 63 => "Lightning Bolt",
                        <= 65 => "Magic Armour",
                        <= 67 => "Magic Bolt",
                        <= 69 => "Slow",
                        <= 71 => "Summon Water Elemental",
                        <= 73 => "Summon Wind Elemental",
                        <= 75 => "Vampiric Touch",
                        76 => "Banish Undead",
                        77 => "Bolster Mind",
                        78 => "Frost Beam",
                        79 => "Hold Creature",
                        80 => "Ice Tomb",
                        81 => "Transpose",
                        82 => "Second Sight",
                        83 => "Summon Demon",
                        84 => "Summon Earth Elemental",
                        85 => "Summon Fire Elemental",
                        86 => "Summon Souls",
                        87 => "Weakness",
                        88 => "Cause Animosity",
                        89 => "Fire Rain",
                        90 => "Fire Wall",
                        91 => "Levitate",
                        92 => "Mirrored Self",
                        93 => "Speed",
                        94 => "Time Freeze",
                        95 => "Fireball",
                        96 => "Into the Void",
                        97 => "Life Force",
                        98 => "Raise Dead",
                        99 => "Summon Greater Demon",
                        100 => "Teleportation",
                        _ => "Invalid"
                    };
                case "Ranged spell":
                    roll = RandomHelper.GetRandomNumber(1, 12);
                    return roll switch
                    {
                        <= 2 => "Blind",
                        <= 4 => "Flare",
                        <= 6 => "Fireball",
                        <= 8 => "Frost Ray",
                        <= 10 => "Gust of Wind",
                        <= 12 => "Slow",
                        _ => "Invalid"
                    };
                case "Touch spell":
                    roll = RandomHelper.GetRandomNumber(1, 12);
                    return roll switch
                    {
                        <= 2 => "Mind Blast",
                        <= 4 => "Mirrored Self",
                        <= 6 => "Seduce",
                        <= 8 => "Stun",
                        <= 10 => "Teleportation",
                        <= 12 => "Vampiric Touch",
                        _ => "Invalid"
                    };
                case "Support spell":
                    roll = RandomHelper.GetRandomNumber(1, 16);
                    return roll switch
                    {
                        <= 2 => "Frenzy",
                        <= 4 => "Healing",
                        <= 6 => "Healing Hand",
                        <= 8 => "Mute",
                        <= 10 => isUndead ? "Raise Dead" : GetRandomSpellNameByCategory("Support spell"),
                        <= 12 => "Shield",
                        <= 14 => "Summon Demon",
                        <= 16 => "Summon Greater Demon",
                        _ => "Invalid"
                    };
            }
            return spell;
        }

        public string GetRandomSpellName()
        {
            return GetRandomSpellNameByCategory();
        }

        public List<Spell> GetStartingSpells()
        {
            var spells = new List<Spell>();
            var possibleSpells = _gameData.GetSpellsByLevel(1);
            if (possibleSpells == null)
            {
                throw new ArgumentException("No spells found for level 1.");
            }

            for (int i = 0; i < 3; i++)
            {
                Spell spell;
                do
                {
                    spell = possibleSpells[RandomHelper.GetRandomNumber(0, possibleSpells.Count - 1)];
                } while (spells.Contains(spell));
                spells.Add(spell);
            }
            return spells;
        }
    }
}