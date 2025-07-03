namespace LoDCompanion.Utilities
{
    public static class TooltipHelper
    {
        private static readonly Dictionary<string, string> TooltipDictionary = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
        // Basic Stats
        { "STR", "Strength (STR): This is your hero's physical strength. This affects how much your character can carry, the damage they deal, and the weapons they can wield. Effects how many hands a weapon will take to wield, and damage bonuses above 60" },
        { "CON", "Constitution (CON): Symbolises how fit your hero is and how well they can withstand diseases and poison. A high constitution also allows the hero to take more damage. Effects the foraging skill, and provides Natural Armour starting at 50" },
        { "DEX", "Dexterity (DEX): Represents how dexterous your hero is. The more dexterous, the more likely they will dodge an incoming strike or jump across a chasm. Skills affected: Combat Skill, Ranged Skill, Dodge, Pick Locks" },
        { "WIS", "Wisdom (WIS): Governs your abilities to analyse things, and is also important for dealing with Magic and Alchemy. A wizard's Mana is equal to their Wisdom. Skills affected: Arcane Arts (Wizard), Barter, Heal, Alchemy, Perception" },
        { "RES", "Resolve (RES): This is the mental strength of your character and a measure of how well they can handle fear. It also symbolises their faith in the gods. Skills affected: Battle Prayers (Warrior Priest)" },

        // Other Core Stats
        { "DB", "Damage Bonus (DB): Especially strong heroes can cause extra damage to their enemies." },
        { "NA", "Natural Armour (NA): Some heroes may have such a strong constitution that they will shrug off even the most grievous of wounds." },
        { "E", "Energy (E): Used to activate Perks during quests. Lost energy is regained after the quest or by resting." },
        { "L", "Luck (L): Can be used to reroll a dice roll that directly affects the hero. A reroll can never be rerolled." },
        { "M", "Movement (M): How many squares your character can move during one action. All heroes start with a movement of 4." },
        { "Sanity", "Sanity: The mental status of your hero. As the game progresses, Sanity will decrease. If it reaches zero, the hero will contract a disorder." },
        { "HP", "Hit Points (HP): How many Hit Points your hero may lose before they start bleeding out." },

        // Skills
        { "Combat Skill", "Combat Skill (CS): Your skill in using close-combat weapons. Based on Dexterity." },
        { "Ranged Skill", "Ranged Skill (RS): Your hero's ability to hit targets from afar with a bow, a sling, or maybe even a bottle. Based on Dexterity." },
        { "Dodge", "Dodge: The ability to dodge a strike or an incoming arrow. A successful result means you have avoided potential damage. Based on Dexterity." },
        { "Arcane Art", "Arcane Art: The knowledge of everything magical. Only wizards may learn this skill. It is used to cast spells and identify magical items. Based on Wisdom." },
        { "Barter", "Barter: Your skill in making good deals when trading. A successful roll lets you buy at a 10% discount and sell at a 10% higher price. Based on Wisdom." },
        { "Heal", "Heal: Your skill to mend the wounds of your comrades. Requires a bandage. Based on Wisdom." },
        { "Foraging", "Foraging: Allows you to gather food and hunt while traveling. It can also be used to skin animals for fur. Based on Constitution." },
        { "Pick Locks", "Pick Locks: Used to open locked doors, chests, and disarm traps. Requires a lock pick. Based on Dexterity." },
        { "Alchemy", "Alchemy: The skill to identify and mix potions. Based on Wisdom." },
        { "Perception", "Perception: Used to notice important details like traps or clues, and is also used while searching. Based on Wisdom." },
        { "Battle Prayers", "Battle Prayers: A special knowledge perfected by Warrior Priests who call upon the gods to help them in battle. Only a Warrior Priest may learn this skill. Based on Resolve." }
    };

        public static string GetTooltip(string key)
        {
            return TooltipDictionary.TryGetValue(key, out var description) ? description : "No description available.";
        }
    }
}
