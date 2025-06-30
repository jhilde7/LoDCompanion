using LoDCompanion.Utilities;
using LoDCompanion.Models;

namespace LoDCompanion.Services.GameData
{
    public class AlchemyService
    {
        private readonly GameDataService _gameData;

        public AlchemyService(GameDataService gameData)
        {
            _gameData = gameData;
        }

        public static string[] GetPotionNames(int count, string quality)
        {
            string[] potions = new string[count];
            for (int i = 0; i < count; i++)
            {
                switch (quality)
                {
                    case "Weak":
                    case "Supreme":
                        potions[i] = GetNonStandardPotion();
                        break;
                    case "Standard":
                        potions[i] = GetStandardPotion();
                        break;
                    case "Any":
                        int roll = RandomHelper.GetRandomNumber(1, 3); // Using RandomHelper from Utilities
                        switch (roll)
                        {
                            case 1:
                            case 2:
                                potions[i] = GetNonStandardPotion();
                                break;
                            case 3:
                                potions[i] = GetStandardPotion();
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            return potions;
        }

        public static string GetStandardPotion()
        {
            string retPotion = "";
            int roll = RandomHelper.GetRandomNumber(1, 3);
            switch (roll)
            {
                case 1:
                    roll = RandomHelper.GetRandomNumber(1, 10);
                    switch (roll)
                    {
                        case 1:
                            retPotion = "Bottle of Experience";
                            break;
                        case 2:
                            retPotion = "Potion of Constitution";
                            break;
                        case 3:
                            retPotion = "Potion of Courage";
                            break;
                        case 4:
                            retPotion = "Potion of Dexterity";
                            break;
                        case 5:
                            retPotion = "Potion of Energy";
                            break;
                        case 6:
                            retPotion = "Potion of Health";
                            break;
                        case 7:
                            retPotion = "Potion of Mana";
                            break;
                        case 8:
                            retPotion = "Potion of Strength";
                            break;
                        case 9:
                            retPotion = "Potion of Wisdom";
                            break;
                        case 10:
                            retPotion = "Acidic Bomb";
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    roll = RandomHelper.GetRandomNumber(1, 10);
                    switch (roll)
                    {
                        case 1:
                            retPotion = "Potion of Distortion";
                            break;
                        case 2:
                            retPotion = "Firebomb";
                            break;
                        case 3:
                            retPotion = "Potion of Invisibility";
                            break;
                        case 4:
                            retPotion = "Vial of Corrosion";
                            break;
                        case 5:
                            retPotion = "Potion of Cure Disease";
                            break;
                        case 6:
                            retPotion = "Potion of Cure Poison";
                            break;
                        case 7:
                            retPotion = "Poison";
                            break;
                        case 8:
                            retPotion = "Liquid Fire";
                            break;
                        case 9:
                            retPotion = "Bottle of the Void";
                            break;
                        case 10:
                            retPotion = "Weapons Oil";
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    roll = RandomHelper.GetRandomNumber(1, 8);
                    switch (roll)
                    {
                        case 1:
                            retPotion = "Elixir of Speed";
                            break;
                        case 2:
                            retPotion = "Alchemical Dust";
                            break;
                        case 3:
                            retPotion = "Elixir of the Archer";
                            break;
                        case 4:
                            retPotion = "Potion of Rage";
                            break;
                        case 5:
                            retPotion = "Potion of Fire Protection";
                            break;
                        case 6:
                            retPotion = "Potion of Dragon Skin";
                            break;
                        case 7:
                            retPotion = "Potion of Restoration";
                            break;
                        case 8:
                            retPotion = "Potion of Dragon Breath";
                            break;
                        default:
                            break;
                    }
                    break;
            }
            return retPotion;
        }

        public static string GetNonStandardPotion()
        {
            string retPotion = "";
            int roll = RandomHelper.GetRandomNumber(1, 12);
            switch (roll)
            {
                case 1:
                    retPotion = "Bottle of Experience";
                    break;
                case 2:
                    retPotion = "Potion of Constitution";
                    break;
                case 3:
                    retPotion = "Potion of Courage";
                    break;
                case 4:
                    retPotion = "Potion of Dexterity";
                    break;
                case 5:
                    retPotion = "Potion of Energy";
                    break;
                case 6:
                    retPotion = "Potion of Health";
                    break;
                case 7:
                    retPotion = "Potion of Mana";
                    break;
                case 8:
                    retPotion = "Potion of Strength";
                    break;
                case 9:
                    retPotion = "Potion of Wisdom";
                    break;
                case 10:
                    retPotion = "Acidic Bomb";
                    break;
                case 11:
                    retPotion = "Potion of Cure Disease";
                    break;
                case 12:
                    retPotion = "Potion of Cure Poison";
                    break;
                default:
                    break;
            }
            return retPotion;
        }

        public static string[] GetIngredients(int count)
        {
            string[] ingredients = new string[count];
            for (int i = 0; i < count; i++)
            {
                ingredients[i] = GetIngredient();
            }
            return ingredients;
        }

        public static string GetIngredient()
        {
            string retIngredient = "";
            int roll = RandomHelper.GetRandomNumber(1, 20);
            switch (roll)
            {
                case 1:
                    retIngredient = "Lunarberry";
                    break;
                case 2:
                    retIngredient = "Dragon Stalk";
                    break;
                case 3:
                    retIngredient = "Ember Bark";
                    break;
                case 4:
                    retIngredient = "Mountain Barberry";
                    break;
                case 5:
                    retIngredient = "Salty Wyrmwood";
                    break;
                case 6:
                    retIngredient = "Ashen Ginger";
                    break;
                case 7:
                    retIngredient = "Spicy Windroot";
                    break;
                case 8:
                    retIngredient = "Wintercress";
                    break;
                case 9:
                    retIngredient = "Sweet Ivy";
                    break;
                case 10:
                    retIngredient = "Monk's Laurel";
                    break;
                case 11:
                    retIngredient = "Nightshade";
                    break;
                case 12:
                    retIngredient = "Weeping Clover";
                    break;
                case 13:
                    retIngredient = "Snakeberry";
                    break;
                case 14:
                    retIngredient = "Bitterweed";
                    break;
                case 15:
                    retIngredient = "Arching Pokeroot";
                    break;
                case 16:
                    retIngredient = "Toxic Hogweed";
                    break;
                case 17:
                    retIngredient = "Blue Coneflower";
                    break;
                case 18:
                    retIngredient = "Giant Raspberry";
                    break;
                case 19:
                    retIngredient = "Bright Gallberry";
                    break;
                case 20:
                    retIngredient = "Barbed Wormwood";
                    break;
                default:
                    break;
            }
            return retIngredient;
        }

        public static string[] GetParts(int count, bool useOrigin = false)
        {
            string[] parts = new string[count];
            for (int i = 0; i < count; i++)
            {
                if (useOrigin)
                {
                    parts[i] = GetOrigin() + " " + GetPart();
                }
                else
                {
                    parts[i] = GetPart();
                }
            }
            return parts;
        }

        public static string GetOrigin()
        {
            int roll = RandomHelper.GetRandomNumber(1, 100);
            return roll switch
            {
                1 => "Banshee",
                2 => "Basilisk",
                3 => "Beast man",
                4 => "Cave Bear",
                <= 10 => "Cave Goblin",
                11 => "Centaur",
                12 => "Common Troll",
                13 => "Dire Wolf",
                14 => "Dark Elf",
                15 => "Dragon",
                16 => "Dryder",
                17 => "Ettin",
                18 => "Frogling",
                19 => "Gargoyle",
                20 => "Gecko",
                21 => "Ghost",
                22 => "Ghoul",
                23 => "Giant",
                <= 35 => "Giant Bat",
                36 => "Giant Centipede",
                37 => "Giant Leech",
                <= 48 => "Giant Pox Rat",
                <= 55 => "Giant Rat",
                56 => "Giant Scorpion",
                <= 58 => "Giant Snake",
                59 => "Giant Spider",
                60 => "Giant Toad",
                61 => "Giant Wolf",
                62 => "Gigantic Snake",
                63 => "Gigantic Spider",
                64 => "Gnoll",
                65 => "Goblin",
                66 => "Griffon",
                67 => "Harpy",
                68 => "Hydra",
                69 => "Lurker",
                70 => "Medusa",
                71 => "Mimic",
                72 => "Minotaur",
                73 => "Minotaur Skeleton",
                74 => "Mummy",
                75 => "Naga",
                76 => "Ogre",
                77 => "Orc",
                78 => "Raptor",
                79 => "River Troll",
                80 => "Salamander",
                81 => "Satyr",
                82 => "Saurian",
                83 => "Shambler",
                84 => "Skeleton",
                85 => "Slime",
                86 => "Sphinx",
                87 => "Stone Golem",
                88 => "Stone Troll",
                89 => "Tomb Guardian",
                90 => "Vampire",
                91 => "Werewolf",
                92 => "Wight",
                93 => "Wraith",
                94 => "Wyvern",
                <= 99 => "Zombie",
                100 => "Zombie Ogre",
                _ => "Unknown"
            };
        }

        public static string GetPart()
        {
            string retPart = "";
            int roll = RandomHelper.GetRandomNumber(1, 10);
            switch (roll)
            {
                case 1:
                    retPart = "Brain";
                    break;
                case 2:
                    retPart = "Kidney";
                    break;
                case 3:
                    retPart = "Saliva";
                    break;
                case 4:
                    retPart = "Blood";
                    break;
                case 5:
                    retPart = "Skin";
                    break;
                case 6:
                    retPart = "Nails";
                    break;
                case 7:
                    retPart = "Hair";
                    break;
                case 8:
                    retPart = "Eye";
                    break;
                case 9:
                    retPart = "Tongue";
                    break;
                case 10:
                    retPart = "Heart";
                    break;
                default:
                    break;
            }
            return retPart;
        }
    }

    // Enum to represent the strength or type of a potion
    public enum PotionStrength
    {
        None,
        Weak,
        Standard,
        Supreme
    }

    public class AlchemyItem : Equipment
    {
        public bool IsPotion { get; set; }
        public bool IsIngredient { get; set; }
        public bool IsPart { get; set; }
        public PotionStrength Strength { get; set; } = PotionStrength.None;
        public string EffectDescription { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty; // For ingredients/parts, their origin (e.g., "Plant", "Fungus", "Animal")
        public bool CreatePotion { get; set; } //triggers random potion generator

        public AlchemyItem()
        {
            if (CreatePotion)
            {
                AlchemyService.GetPotionNames(1, Strength.ToString());
            }
        }
        // Additional constructor for ingredients/parts
        public AlchemyItem(string name, string origin, bool isIngredient = true, bool isPart = false)
        {
            Name = name;
            Origin = origin;
            IsIngredient = isIngredient;
            IsPart = isPart;
            Description = GetItemDescription();
        }

        // Method to describe the item (can be overridden if needed)
        public string GetItemDescription()
        {
            if (IsPotion)
            {
                return $"{Strength} {Name}: {EffectDescription}";
            }
            else if (IsIngredient || IsPart)
            {
                return $"{Name} (Origin: {Origin})";
            }
            return Description;
        }
    }
}