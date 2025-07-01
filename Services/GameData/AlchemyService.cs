using LoDCompanion.Utilities;
using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Xml.Linq;
using System.Text;

namespace LoDCompanion.Services.GameData
{
    public class AlchemyService
    {
        public static List<AlchemyItem> Parts => GetPartsList();
        public static List<AlchemyItem> Ingredients => GetIngredientsList();

        public AlchemyService()
        {

        }

        public static string GetStandardPotion()
        {
            int roll = RandomHelper.GetRandomNumber(1, 3);
            switch (roll)
            {
                case 1:
                    roll = RandomHelper.RollDie("D10");
                    switch (roll)
                    {
                        case 1:
                             return "Bottle of Experience";
                        case 2:
                             return "Potion of Constitution";
                        case 3:
                             return "Potion of Courage";
                        case 4:
                             return "Potion of Dexterity";
                        case 5:
                             return "Potion of Energy";
                        case 6:
                             return "Potion of Health";
                        case 7:
                             return "Potion of Mana";
                        case 8:
                             return "Potion of Strength";
                        case 9:
                             return "Potion of Wisdom";
                        case 10:
                             return "Acidic Bomb";
                        default:
                            return "Acidic Bomb";
                    }
                case 2:
                    roll = RandomHelper.RollDie("D10");
                    switch (roll)
                    {
                        case 1:
                             return "Potion of Distortion";
                        case 2:
                             return "Firebomb";
                        case 3:
                             return "Potion of Invisibility";
                        case 4:
                             return "Vial of Corrosion";
                        case 5:
                             return "Potion of Cure Disease";
                        case 6:
                             return "Potion of Cure Poison";
                        case 7:
                             return "Poison";
                        case 8:
                             return "Liquid Fire";
                        case 9:
                             return "Bottle of the Void";
                        case 10:
                             return "Weapons Oil";
                        default:
                            return "Weapons Oil";
                    }
                case 3:
                    roll = RandomHelper.RollDie("D8");
                    switch (roll)
                    {
                        case 1:
                             return "Elixir of Speed";
                        case 2:
                             return "Alchemical Dust";
                        case 3:
                             return "Elixir of the Archer";
                        case 4:
                             return "Potion of Rage";
                        case 5:
                             return "Potion of Fire Protection";
                        case 6:
                             return "Potion of Dragon Skin";
                        case 7:
                             return "Potion of Restoration";
                        case 8:
                             return "Potion of Dragon Breath";
                        default:
                            return "Potion of Dragon Breath";
                    }
                default: return "";
            }
        }

        public static string GetNonStandardPotion()
        {
            int roll = RandomHelper.RollDie("D12");
            switch (roll)
            {
                case 1:
                     return "Bottle of Experience";
                case 2:
                     return "Potion of Constitution";
                case 3:
                     return "Potion of Courage";
                case 4:
                     return "Potion of Dexterity";
                case 5:
                     return "Potion of Energy";
                case 6:
                     return "Potion of Health";
                case 7:
                     return "Potion of Mana";
                case 8:
                     return "Potion of Strength";
                case 9:
                     return "Potion of Wisdom";
                case 10:
                     return "Acidic Bomb";
                case 11:
                     return "Potion of Cure Disease";
                case 12:
                     return "Potion of Cure Poison";
                default:
                     return "Potion of Cure Poison";
            }
        }

        public static AlchemyItem[] GetIngredients(int count)
        {
            AlchemyItem[] ingredients = new AlchemyItem[count];
            for (int i = 0; i < count; i++)
            {
                ingredients[i] = new AlchemyItem() { Name = GetIngredient(), IsIngredient = true };
            }
            return ingredients;
        }

        public static string GetIngredient()
        {
            int roll = RandomHelper.GetRandomNumber(0, Ingredients.Count - 1);
            return Ingredients[roll].Name;
        }

        public static AlchemyItem[] GetParts(int count, string? origin = null)
        {
            AlchemyItem[] parts = new AlchemyItem[count];
            for (int i = 0; i < count; i++)
            {
                if (origin != null)
                {
                    parts[i] = new AlchemyItem() { Origin = origin, Name = GetPart(), IsPart = true };
                }
                else
                {
                    parts[i] = new AlchemyItem() { Origin = GetOrigin(), Name = GetPart(), IsPart = true };
                }
            }
            return parts;
        }

        public static string GetPart()
        {
            int roll = RandomHelper.GetRandomNumber(0, Parts.Count - 1);
            return Parts[roll].Name;
        }

        public static string GetOrigin()
        {
            int roll = RandomHelper.RollDie("D100");
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

        public static List<AlchemyItem> GetPartsList()
        {
            return new List<AlchemyItem>()
            {
                new AlchemyItem() { Name = "Brain", IsPart = true },
                new AlchemyItem() { Name = "Kidney", IsPart = true },
                new AlchemyItem() { Name = "Saliva", IsPart = true },
                new AlchemyItem() { Name = "Blood", IsPart = true },
                new AlchemyItem() { Name = "Skin", IsPart = true },
                new AlchemyItem() { Name = "Nails", IsPart = true },
                new AlchemyItem() { Name = "Hair", IsPart = true },
                new AlchemyItem() { Name = "Eye", IsPart = true },
                new AlchemyItem() { Name = "Tongue", IsPart = true },
                new AlchemyItem() { Name = "Heart", IsPart = true }
            };
        }

        public static List<AlchemyItem> GetIngredientsList()
        {
            return new List<AlchemyItem> {
                new AlchemyItem() { IsIngredient = true, Name = "Lunarberry" },
                new AlchemyItem() { IsIngredient = true, Name = "Dragon Stalk" },
                new AlchemyItem() { IsIngredient = true, Name = "Ember Bark" },
                new AlchemyItem() { IsIngredient = true, Name = "Mountain Barberry" },
                new AlchemyItem() { IsIngredient = true, Name = "Salty Wyrmwood" },
                new AlchemyItem() { IsIngredient = true, Name = "Ashen Ginger" },
                new AlchemyItem() { IsIngredient = true, Name = "Spicy Windroot" },
                new AlchemyItem() { IsIngredient = true, Name = "Wintercress" },
                new AlchemyItem() { IsIngredient = true, Name = "Sweet Ivy" },
                new AlchemyItem() { IsIngredient = true, Name = "Monk's Laurel" },
                new AlchemyItem() { IsIngredient = true, Name = "Nightshade" },
                new AlchemyItem() { IsIngredient = true, Name = "Weeping Clover" },
                new AlchemyItem() { IsIngredient = true, Name = "Snakeberry" },
                new AlchemyItem() { IsIngredient = true, Name = "Bitterweed" },
                new AlchemyItem() { IsIngredient = true, Name = "Arching Pokeroot" },
                new AlchemyItem() { IsIngredient = true, Name = "Toxic Hogweed" },
                new AlchemyItem() { IsIngredient = true, Name = "Blue Coneflower" },
                new AlchemyItem() { IsIngredient = true, Name = "Giant Raspberry" },
                new AlchemyItem() { IsIngredient = true, Name = "Bright Gallberry" },
                new AlchemyItem() { IsIngredient = true, Name = "Barbed Wormwood" },
            };
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
        public bool IsRecipe { get; set; }
        public PotionStrength Strength { get; set; } = PotionStrength.None;
        public string? EffectDescription { get; set; }
        public string? Origin { get; set; }

        public AlchemyItem()
        {
            
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
                return $"{Strength} {Name}: {EffectDescription ?? "Unknown effect"}";
            }
            else if (IsIngredient || IsPart)
            {
                return $"{Name} (Origin: {Origin ?? "Unknown"})";
            }
            return Description;
        }

        public static AlchemicalRecipe DeserializeRecipe(AlchemyItem recipeItem, GameDataService gameData)
        {
            List<AlchemyItem> components = new List<AlchemyItem>();
            
            recipeItem.Description.Replace("Recipe Components: ", "");
            string[] componentPairs = recipeItem.Description.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in componentPairs)
            {
                string[] itemArray = pair.Split(':', 2);
                if (itemArray.Length != 2) continue; // Skip malformed entries

                string itemType = itemArray[0];
                string itemName = itemArray[1].Trim();

                AlchemyItem? fullItem = gameData.GetAlchemyItemByName(itemName);

                if (fullItem != null)
                {
                    components.Add(fullItem);
                }
            }

            return new AlchemicalRecipe()
            {
                // Remove " Recipe" from the end of the name
                Name = recipeItem.Name.Replace(" Recipe", ""),
                Strength = recipeItem.Strength,
                Components = components
            };
        }

    }

    public class  AlchemicalRecipe
    {
        public string Name { get; set; } = string.Empty;
        public PotionStrength Strength { get; set; }
        public List<AlchemyItem> Components { get; set; } = new List<AlchemyItem>();


        public AlchemyItem SerializeRecipe()
        {
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append("Recipe Components: ");
            foreach (var item in Components)
            {
                if (item.IsIngredient)
                {
                    descriptionBuilder.Append($"Ingredient:{item.Name},");
                }
                else if (item.IsPart)
                {
                    descriptionBuilder.Append($"Part:{item.Name},");
                }
            }

            // Remove the final trailing comma.
            if (descriptionBuilder.Length > 0)
            {
                descriptionBuilder.Length--;
            }

            return new AlchemyItem()
            {
                Name = this.Name + " Recipe",
                Description = descriptionBuilder.ToString(),
                Strength = this.Strength,
                IsRecipe = true,
            };
        }
    }
}