

using LoDCompanion.Utilities;

namespace LoDCompanion.Models.Dungeon
{
    public class Trap
    {
        public string Name { get; set; }
        public int SkillModifier { get; set; } // Modifier for detecting/avoiding the trap
        public int DisarmModifier { get; set; } // Modifier for disarming the trap
        public string Description { get; set; }
        public string SpecialDescription { get; set; } // Description of the special effect if triggered

        // Properties to track trap state (can be managed by a service)
        public bool IsTrapped { get; set; } = true; // A trap starts as active
        public bool IsTriggered { get; set; }
        public bool IsAvoided { get; set; }
        public bool IsDisarmed { get; set; }

        public Trap(string name, int skillModifier, int disarmModifier, string description, string specialDescription = "")
        {
            Name = name;
            SkillModifier = skillModifier;
            DisarmModifier = disarmModifier;
            Description = description;
            SpecialDescription = specialDescription;
        }

        // Static factory method to create common trap types
        public static Trap GetRandomTrap()
        {
            
            int roll = RandomHelper.RollDie(DiceType.D100);
            return roll switch
            {
                <= 20 => new Trap("Pit Trap", 0, 10, "A deep pit trap.", "Causes fall damage."),
                <= 40 => new Trap("Poison Dart Trap", 20, 15, "A trap that shoots poisoned darts.", "Causes poison damage."),
                <= 60 => new Trap("Spear Trap", 10, 10, "Hidden spears spring out from the walls.", "Causes piercing damage."),
                <= 80 => new Trap("Tripping Wire", 5, 5, "A nearly invisible wire across the floor.", "Causes hero to fall, potentially losing turn or taking minor damage."),
                _ => new Trap("Magic Rune Trap", 25, 20, "A glowing rune on the floor.", "Triggers a magical effect, e.g., a spell."),
            };
            
        }
    }

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
            int rollForRed = Utilities.RandomHelper.GetRandomNumber(2, 5); // Assuming Utilities.RandomNumber is available
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
            Utilities.IListExtensions.Shuffle(_levers);
        }

        // Public method to simulate pulling a lever and get the event description
        public string PullLever()
        {
            CreateDeck();
            string pulledLever = _levers.Any() ? _levers[0] : "None"; // Ensure list is not empty

            int roll = 0;
            if (pulledLever == "Black")
            {
                roll = Utilities.RandomHelper.GetRandomNumber(1, 8); // Assuming Utilities.RandomNumber is available
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
                roll = Utilities.RandomHelper.GetRandomNumber(1, 20); // Assuming Utilities.RandomNumber is available
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
}