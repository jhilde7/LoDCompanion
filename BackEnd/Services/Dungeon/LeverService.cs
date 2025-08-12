using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public class Lever
    {
        private List<string> _levers = new List<string>(); // Internal list for lever types
        public bool HaveClue { get; set; } = false; // Public property for clue status
        public string EventDescription { get; private set; } = string.Empty; // Read-only property for the event description

        public Lever()
        {
            _levers = new List<string>();
        }

        // Method to create the deck of levers based on game rules
        private void CreateDeck()
        {
            _levers.Clear(); // Clear existing levers before creating new ones
            string blackLever = "Black";
            string redLever = "Red";

            _levers.Add(blackLever);
            int rollForRed = RandomHelper.GetRandomNumber(2, 5); // Assuming Utilities.RandomNumber is available
            if (HaveClue)
            {
                rollForRed -= 1;
            }

            for (int i = 0; i < rollForRed; i++)
            {
                _levers.Add(redLever);
            }

            // Assuming a Shuffle extension method is available for List<T>
            // This would likely come from a shared Utilities or RandomHelper class.
            _levers.Shuffle();
        }

        // Public method to simulate pulling a lever and get the event description
        public string PullLever()
        {
            CreateDeck();
            string pulledLever = _levers.Any() ? _levers[0] : "None"; // Ensure list is not empty

            int roll = 0;
            if (pulledLever == "Black")
            {
                roll = RandomHelper.GetRandomNumber(1, 8); // Assuming Utilities.RandomNumber is available
                switch (roll)
                {
                    case 1:
                        return "If the spawn point for wandering monsters is the starting tile, and when a wandering monster is to be spawned, " +
                        "lower the threat level as usual but do not spawn a wandering monster.";
                    case 2:
                        return "A secret passage to the Treasure Chamber has been discovered. If this room is already placed you may move it (temporarily) " +
                        "to an adjacent space next to the current room.";
                    case 3:
                        return "A small compartment opens revealing a wonderful treasure.";
                    case 4:
                        return "The next locked door discovered will be unlocked.";
                    case 5:
                        return "The next trap encountered may be ignored";
                    case 6:
                        return "If there is an unopened door in this room, it can be opened without raising the threat level.";
                    case 7:
                        return "A small compartment opens revealing a 1d3 potions";
                    case 8:
                        return "The party gains a collective luck point to be used in this dungeon.";
                    default:
                        return "Lever pulled, but no specific event occurred (unexpected roll).";
                }
            }
            else if (pulledLever == "Red")
            {
                roll = RandomHelper.GetRandomNumber(1, 20); // Assuming Utilities.RandomNumber is available
                return roll switch
                {
                    12 => "If there is an unopened door in this room, it is now locked at the highest level. If all the doors are open, then randomize close and lock one of them.",
                    13 => "Raise the threat level by 2.",
                    14 => "Add 2 random exploration cards to the top of each exploration pile without looking at them.",
                    15 => "Lower party morale by 4, and each characters sanity by 1.",
                    16 => "All doors to this room have been blocked by portcullis. See pg 89 about lifting them.",
                    17 => "A wandering monster has appeared in a random empty tile at least one tile from this room. " +
                        "The monster comes from the standard encounter for this quest, with a +20 to the die roll, with single highest experience monster " +
                        "becoming the wandering monster.",
                    18 => "Every door in this dungeon is now trapped on a 5-6 roll instead of 6 on trap checks.",
                    19 => "A random hero, not the one pulling the lever, must make a DEX test or fall into a pit trap. " +
                        "If succeeded, the hero moves to an adjacent square next to the pit. If failed, the hero takes 1d10 dmg using NA only as well as " +
                        "lower part morale by 1 and sanity by 2. The hero will then need to make a DEX test or use a rope to get out of the pit. ",
                    20 => "The hero pulling the lever is trapped in a cage, and an encounter must be rolled for each door in the room. The caged hero is unable to act during " +
                        "the battle. Additionally, the caged hero suffers 2 sanity loss and party morale is reduced by 1.",
                    _ => "Nothing Happens."
                };
            }
            else
            {
                EventDescription = "No lever type recognized.";
            }
            return EventDescription;
        }
    }

    /// <summary>
    /// Represents the color of a lever, corresponding to the "cards" in the PDF.
    /// </summary>
    public enum LeverColor
    {
        Red,
        Black
    }

    /// <summary>
    /// Represents the outcome of pulling a lever.
    /// </summary>
    public class LeverPullResult
    {
        public string Description { get; set; } = "Nothing happens.";
        public LeverColor LeverColor { get; set; }
        // Add properties to signal specific game state changes to the manager
        public int ThreatIncrease { get; set; } = 0;
        public int PartyMoraleDecrease { get; set; } = 0;
        public int SanityDecrease { get; set; } = 0;
        public bool ShouldSpawnWanderingMonster { get; set; } = false;
        public bool ShouldAddExplorationCards { get; set; } = false;
        public bool ShouldLockADoor { get; set; } = false;
        public bool ShouldSpawnPortcullis { get; set; } = false;
        public bool ShouldTriggerPitTrap { get; set; } = false;
        public bool ShouldTriggerCageTrap { get; set; } = false;
    }

    /// <summary>
    /// Manages the logic for the lever mini-game within a dungeon.
    /// </summary>
    public class LeverService
    {
        /// <summary>
        /// Prepares the "deck" of levers as described on page 89 of the PDF.
        /// </summary>
        /// <param name="hasClue">If true, one red lever is removed, per the rules.</param>
        /// <returns>A shuffled list of LeverColor representing the available levers.</returns>
        public List<LeverColor> PrepareLeverDeck(bool hasClue)
        {
            var deck = new List<LeverColor> { LeverColor.Black };

            int numberOfRedLevers = RandomHelper.RollDie(DiceType.D4) + 1;
            if (hasClue)
            {
                numberOfRedLevers--;
            }

            for (int i = 0; i < numberOfRedLevers; i++)
            {
                deck.Add(LeverColor.Red);
            }

            deck.Shuffle();
            return deck;
        }

        /// <summary>
        /// Resolves the event for pulling a lever of a specific color.
        /// </summary>
        /// <param name="color">The color of the lever pulled.</param>
        /// <returns>A LeverPullResult object describing the outcome.</returns>
        public LeverPullResult PullLever(LeverColor color)
        {
            var result = new LeverPullResult { LeverColor = color };

            if (color == LeverColor.Black)
            {
                int roll = RandomHelper.RollDie(DiceType.D8);
                switch (roll)
                {
                    case 1: result.Description = "A distant bang is heard! The dungeon entrance is now sealed, preventing new Wandering Monsters from entering."; break;
                    case 2: result.Description = "A section of the wall retracts, revealing a hidden passage to the Treasure Chamber (R10)!"; break;
                    case 3: result.Description = "A small compartment opens, revealing a Wonderful Treasure!"; break;
                    case 4: result.Description = "A metallic sound echoes. The next locked door the heroes encounter will be unlocked."; break;
                    case 5: result.Description = "You feel a sense of protection. The next trap the party encounters may be ignored."; break;
                    case 6: result.Description = "With a screech, a nearby door unlocks and opens slightly. It is no longer trapped or locked."; break;
                    case 7: result.Description = "A hidden compartment opens, revealing 1d3 potions!"; break;
                    case 8: result.Description = "The party is blessed with good fortune, gaining a collective Luck Point for this dungeon."; break;
                }
            }
            else // Red Lever
            {
                int roll = RandomHelper.RollDie(DiceType.D20);
                switch (roll)
                {
                    case int n when n >= 1 && n <= 11:
                        result.Description = "A rumble can be heard far off in the dungeon. Then, silence. Nothing happens.";
                        break;
                    case 12:
                        result.Description = "With a metallic clang, a door in the room slams shut and is now locked!";
                        result.ShouldLockADoor = true;
                        break;
                    case 13:
                        result.Description = "The sound of grinding cogs echoes... The Threat Level increases by 2.";
                        result.ThreatIncrease = 2;
                        break;
                    case 14:
                        result.Description = "Screeching noises and a slight tremor are felt. Two random Exploration Cards are added to the top of each deck!";
                        result.ShouldAddExplorationCards = true;
                        break;
                    case 15:
                        result.Description = "An eerie scream echoes, chilling the party to the bone. Party Morale -4, Sanity -1 for each hero.";
                        result.PartyMoraleDecrease = 4;
                        result.SanityDecrease = 1;
                        break;
                    case 16:
                        result.Description = "With a loud bang, portcullises slam down, blocking all doors in the room!";
                        result.ShouldSpawnPortcullis = true;
                        break;
                    case 17:
                        result.Description = "A foul draft blows through the dungeon, heralding a new arrival. A Wandering Monster has appeared!";
                        result.ShouldSpawnWanderingMonster = true;
                        break;
                    case 18:
                        result.Description = "An imperceptible click is heard. The dungeon's defenses are on high alert! All doors are now trapped on a roll of 5-6.";
                        break;
                    case 19:
                        result.Description = "Suddenly, the floor gives way beneath a random hero, who falls into a pit trap!";
                        result.ShouldTriggerPitTrap = true;
                        break;
                    case 20:
                        result.Description = "A cage descends from the ceiling, trapping the hero who pulled the lever! Enemies appear at all doorways!";
                        result.ShouldTriggerCageTrap = true;
                        break;
                }
            }
            return result;
        }
    }
}
