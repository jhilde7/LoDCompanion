using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public class Lever
    {
        public string EventDescription { get; private set; } = string.Empty; // Read-only property for the event description

        public Lever()
        {
            
        }

        /// <summary>
        /// Prepares the "deck" of levers as described on page 89 of the PDF.
        /// </summary>
        /// <param name="hasClue">If true, one red lever is removed, per the rules.</param>
        /// <returns>A shuffled list of LeverColor representing the available levers.</returns>
        public List<LeverColor> PrepareLeverDeck()
        {
            var deck = new List<LeverColor> { LeverColor.Black };

            int numberOfRedLevers = RandomHelper.RollDie(DiceType.D4) + 1;

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
        /// <returns>A LeverResult object describing the outcome.</returns>
        public LeverResult PullLever(Hero hero)
        {
            if (hero.Party == null) throw new ArgumentNullException(nameof(hero.Party), "Hero must be part of a party to pull a lever.");
            var leverColors = PrepareLeverDeck();
            var color = leverColors[0];
            var result = new LeverResult();
            
            var partyMemebersHasClue = hero.Party.Heroes.Any(h => h.Inventory.Backpack.Contains(EquipmentService.GetEquipmentByName("Clue"))); // This should be set based on actual party state
            if (partyMemebersHasClue)
            {
                leverColors.Remove(LeverColor.Red);
            }

            if (color == LeverColor.Black)
            {
                int roll = RandomHelper.RollDie(DiceType.D8);
                switch (roll)
                {
                    case 1: 
                        result.Description = "A distant bang is heard! The dungeon entrance is now sealed, preventing new Wandering Monsters from entering."; 
                        result.CloseDungeonEntrance = true;
                        break;
                    case 2: 
                        result.Description = "A section of the wall retracts, revealing a hidden passage to the Treasure Chamber (R10)!";
                        result.CreateTreasureRoom = true;
                        break;
                    case 3: 
                        result.Description = "A small compartment opens, revealing a Wonderful Treasure!";
                        result.FoundWonderfulTreasure = true;
                        break;
                    case 4: 
                        result.Description = "A metallic sound echoes. The next locked door the heroes encounter will be unlocked.";
                        result.NextLockedDoorIsUnlocked = true;
                        break;
                    case 5: 
                        result.Description = "You feel a sense of protection. The next trap the party encounters may be ignored.";
                        result.NextTrapWillBeDisarmed = true;
                        break;
                    case 6: 
                        result.Description = "With a screech, a nearby door unlocks and opens slightly. It is no longer trapped or locked.";
                        result.NextDoorIsUnlockedDisarmed = true;
                        break;
                    case 7: 
                        result.Description = "A hidden compartment opens, revealing 1d3 potions!"; 
                        result.FoundPotions = true;
                        break;
                    case 8: 
                        result.Description = "The party is blessed with good fortune, gaining a collective Luck Point for this dungeon.";
                        result.PartyGainedLuckPoint = true;
                        break;
                }
            }
            else // Red Lever
            {
                int roll = RandomHelper.RollDie(DiceType.D20);
                switch (roll)
                {
                    case <= 11:
                        result.Description = "A rumble can be heard far off in the dungeon. Then, silence. Nothing happens.";
                        break;
                    case 12:
                        result.Description = "With a metallic clang, a door in the room slams shut and is now locked!";
                        result.LockADoor = true;
                        break;
                    case 13:
                        result.Description = "The sound of grinding cogs echoes... The Threat Level increases by 2.";
                        result.ThreatIncrease = 2;
                        break;
                    case 14:
                        result.Description = "Screeching noises and a slight tremor are felt. Two random Exploration Cards are added to the top of each deck!";
                        result.AddExplorationCards = true;
                        break;
                    case 15:
                        result.Description = "An eerie scream echoes, chilling the party to the bone. Party Morale -4, Sanity -1 for each hero.";
                        result.PartyMoraleDecrease = 4;
                        result.SanityDecrease = 1;
                        break;
                    case 16:
                        result.Description = "With a loud bang, portcullises slam down, blocking all doors in the room!";
                        result.SpawnPortcullis = true;
                        break;
                    case 17:
                        result.Description = "A foul draft blows through the dungeon, heralding a new arrival. A Wandering Monster has appeared!";
                        result.SpawnWanderingMonster = true;
                        break;
                    case 18:
                        result.Description = "An imperceptible click is heard. The dungeon's defenses are on high alert! All doors are now trapped on a roll of 5-6.";
                        result.DoorTrapChanceIncrease = true;
                        break;
                    case 19:
                        result.Description = "Suddenly, the floor gives way beneath a random hero, who falls into a pit trap!";
                        result.TriggerPitTrap = true;
                        break;
                    case 20:
                        result.Description = "A cage descends from the ceiling, trapping the hero who pulled the lever! Enemies appear at all doorways!";
                        result.TriggerCageTrap = true;
                        break;
                }
            }
            return result;
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
    public class LeverResult
    {
        public string Description { get; set; } = "Nothing happens.";
        public int ThreatIncrease { get; set; } = 0;
        public int PartyMoraleDecrease { get; set; } = 0;
        public int SanityDecrease { get; set; } = 0;
        public bool SpawnWanderingMonster { get; set; }
        public bool DoorTrapChanceIncrease { get; internal set; }
        public bool AddExplorationCards { get; set; }
        public bool LockADoor { get; set; }
        public bool SpawnPortcullis { get; set; }
        public bool TriggerPitTrap { get; set; }
        public bool TriggerCageTrap { get; set; }
        public bool CloseDungeonEntrance { get; set; }
        public bool CreateTreasureRoom { get; set; }
        public bool FoundWonderfulTreasure { get; internal set; }
        public bool NextLockedDoorIsUnlocked { get; internal set; }
        public bool NextTrapWillBeDisarmed { get; internal set; }
        public bool NextDoorIsUnlockedDisarmed { get; internal set; }
        public bool FoundPotions { get; internal set; }
        public bool PartyGainedLuckPoint { get; internal set; }
    }

    /// <summary>
    /// Manages the logic for the lever mini-game within a dungeon.
    /// </summary>
    public class LeverService
    {
        
    }
}
