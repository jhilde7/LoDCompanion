using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
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
                int roll = RandomHelper.RollDie("D20");
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
