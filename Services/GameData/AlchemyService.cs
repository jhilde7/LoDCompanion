using LoDCompanion.Utilities;
using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace LoDCompanion.Services.GameData
{
    public class AlchemyService
    {
        private readonly UserRequestService _diceRoll;

        public AlchemyService( UserRequestService diceRollService)
        {
            _diceRoll = diceRollService;
        }

        public static List<Part> Parts => GetPartsList();
        public static List<Ingredient> Ingredients => GetIngredientsList();
        public static List<Potion> Potions => GetAllPotions();
        public static List<Potion> StandardPotions => GetStandardPotions();

        public async Task<string> GetStandardPotionAsync()
        {
            int roll = RandomHelper.GetRandomNumber(1, 3);
            switch (roll)
            {
                case 1:
                    roll = await _diceRoll.RequestRollAsync(
                        $"Roll for standard potion", "1d10");
                    await Task.Yield();
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
                    roll = await _diceRoll.RequestRollAsync(
                        $"Roll for standard potion", "1d10" );
                    await Task.Yield();
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
                    roll = await _diceRoll.RequestRollAsync(
                        $"Roll for standard potion", "1d8");
                    await Task.Yield();
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

        public async Task<string> GetNonStandardPotionAsync()
        {
            int roll = await _diceRoll.RequestRollAsync(
                $"Roll for standard potion", "1d12");
            await Task.Yield();
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

        public static Ingredient[] GetIngredients(int count)
        {
            Ingredient[] ingredients = new Ingredient[count];
            for (int i = 0; i < count; i++)
            {
                ingredients[i] = new Ingredient() { Name = GetIngredient(), IsIngredient = true };
            }
            return ingredients;
        }

        private static string GetIngredient()
        {
            int roll = RandomHelper.GetRandomNumber(0, Ingredients.Count - 1);
            return Ingredients[roll].Name;
        }

        public async Task<Part[]> GetPartsAsync(int count, string? origin = null)
        {
            Part[] parts = new Part[count];
            for (int i = 0; i < count; i++)
            {
                if (origin != null)
                {
                    parts[i] = new Part() { Origin = origin, Name = GetPart(), IsPart = true };
                }
                else
                {
                    parts[i] = new Part() { Origin = await GetOriginAsync(), Name = GetPart(), IsPart = true };
                }
            }
            return parts;
        }

        private string GetPart()
        {
            int roll = RandomHelper.GetRandomNumber(0, Parts.Count - 1);
            return Parts[roll].Name;
        }

        private async Task<string> GetOriginAsync()
        {
            int roll = await _diceRoll.RequestRollAsync(
                $"Roll for standard potion", "1d100");
            await Task.Yield();
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

        private static List<Part> GetPartsList()
        {
            return new List<Part>()
            {
                new Part() { Name = "Brain" },
                new Part() { Name = "Kidney" },
                new Part() { Name = "Saliva" },
                new Part() { Name = "Blood" },
                new Part() { Name = "Skin" },
                new Part() { Name = "Nails" },
                new Part() { Name = "Hair" },
                new Part() { Name = "Eye" },
                new Part() { Name = "Tongue" },
                new Part() { Name = "Heart" }
            };
        }

        private static List<Ingredient> GetIngredientsList()
        {
            return new List<Ingredient> {
                new Ingredient() { Name = "Lunarberry" },
                new Ingredient() { Name = "Dragon Stalk" },
                new Ingredient() { Name = "Ember Bark" },
                new Ingredient() { Name = "Mountain Barberry" },
                new Ingredient() { Name = "Salty Wyrmwood" },
                new Ingredient() { Name = "Ashen Ginger" },
                new Ingredient() { Name = "Spicy Windroot" },
                new Ingredient() { Name = "Wintercress" },
                new Ingredient() { Name = "Sweet Ivy" },
                new Ingredient() { Name = "Monk's Laurel" },
                new Ingredient() { Name = "Nightshade" },
                new Ingredient() { Name = "Weeping Clover" },
                new Ingredient() { Name = "Snakeberry" },
                new Ingredient() { Name = "Bitterweed" },
                new Ingredient() { Name = "Arching Pokeroot" },
                new Ingredient() { Name = "Toxic Hogweed" },
                new Ingredient() { Name = "Blue Coneflower" },
                new Ingredient() { Name = "Giant Raspberry" },
                new Ingredient() { Name = "Bright Gallberry" },
                new Ingredient() { Name = "Barbed Wormwood" },
            };
        }

        private static List<Potion> GetAllPotions()
        {
            return new List<Potion>()
            {
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Health",
                    Durability = 1,
                    Value = 75,
                    Availability = 4,
                    IsPotion = true,
                    Strength = PotionStrength.Weak,
                    EffectDescription = "Heals 1d4 Hit Points."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Health",
                    Durability = 1,
                    Value = 100,
                    Availability = 4,
                    IsPotion = true,
                    Strength = PotionStrength.Standard,
                    EffectDescription = "Heals 1d6 Hit Points."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Health",
                    Durability = 1,
                    Value = 200,
                    Availability = 3,
                    IsPotion = true,
                    Strength = PotionStrength.Supreme,
                    EffectDescription = "Heals 1d10 Hit Points."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Restoration",
                    Durability = 1,
                    Value = 200,
                    Availability = 1,
                    IsPotion = true,
                    Strength = PotionStrength.Standard,
                    EffectDescription = "Restores a hero to full health and removes any disease or poison."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Cure Disease",
                    Durability = 1,
                    Value = 75,
                    Availability = 3,
                    IsPotion = true,
                    Strength = PotionStrength.Weak,
                    EffectDescription = "75% chance to remove all effects of disease."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Cure Disease",
                    Durability = 1,
                    Value = 100,
                    Availability = 3,
                    IsPotion = true,
                    Strength = PotionStrength.Standard,
                    EffectDescription = "Removes all effects of disease."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Cure Disease",
                    Durability = 1,
                    Value = 200,
                    Availability = 2,
                    IsPotion = true,
                    Strength = PotionStrength.Supreme,
                    EffectDescription = "Removes all effects of disease and heals 1d3 HP."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Cure Poison",
                    Durability = 1,
                    Value = 75,
                    Availability = 3,
                    IsPotion = true,
                    Strength = PotionStrength.Weak,
                    EffectDescription = "75% chance to remove all effects of poison."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Cure Poison",
                    Durability = 1,
                    Value = 100,
                    Availability = 3,
                    IsPotion = true,
                    Strength = PotionStrength.Standard,
                    EffectDescription = "Removes all effects of poison."
                },
                new Potion(){
                    Category = "Common",
                    Shop = ShopCategory.Potions,
                    Name = "Potion of Cure Poison",
                    Durability = 1,
                    Value = 200,
                    Availability = 2,
                    IsPotion = true,
                    Strength = PotionStrength.Supreme,
                    EffectDescription = "Removes all effects of poison and heals 1d3 HP."
                },

                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Strength", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Strength until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Strength", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Strength until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Strength", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Strength until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Constitution", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Constitution until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Constitution", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Constitution until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Constitution", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Constitution until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Dexterity", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +5 Dexterity until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Dexterity", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +10 Dexterity until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Dexterity", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +15 Dexterity until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Wisdom", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Wisdom until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Wisdom", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Wisdom until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Wisdom", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Wisdom until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Courage", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Resolve until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Courage", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Resolve until the end of the next battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Courage", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Resolve until the end of the next battle." },
                
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Energy", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +1 Energy until the end of the dungeon." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Energy", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +2 Energy until the end of the dungeon." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Energy", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +3 Energy until the end of the dungeon." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Mana", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Restores 1d20 Mana." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Mana", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Restores 2d20 Mana." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Mana", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Restores 3d20 Mana." },
                
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Acidic Bomb (Tr)", Strength = PotionStrength.Weak, Value = 60, EffectDescription = "Explodes for 1d6 Acidic damage in the target square and half to adjacent squares." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Acidic Bomb (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Explodes for 1d10 Acidic damage in the target square and half to adjacent squares." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Acidic Bomb (Tr)", Strength = PotionStrength.Supreme, Value = 180, EffectDescription = "Explodes for 1d12 Acidic damage in the target square and half to adjacent squares." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Firebomb (Tr)", Strength = PotionStrength.Weak, Value = 60, EffectDescription = "Explodes for 1d6 Fire damage in the target square and half to adjacent squares." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Firebomb (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Explodes for 1d10 Fire damage in the target square and half to adjacent squares." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Firebomb (Tr)", Strength = PotionStrength.Supreme, Value = 180, EffectDescription = "Explodes for 1d12 Fire damage in the target square and half to adjacent squares." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Smoke (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Creates a thick smoke in a 3x3 area, obscuring LOS and giving -20 CS to fights within. Lasts 4 turns." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Disorientation (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Target must pass a RES test or forfeit their next turn. Adjacent models test at +20 RES." },
                
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Alchemical Dust", Strength = PotionStrength.Standard, Value = 60, EffectDescription = "Allows a reroll on a search check for one room or corridor." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Bottle of Experience", Strength = PotionStrength.Weak, Value = 250, EffectDescription = "Instantly grants +100 XP. Can only be used once between dungeons." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Bottle of Experience", Strength = PotionStrength.Standard, Value = 350, EffectDescription = "Instantly grants +200 XP. Can only be used once between dungeons." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Bottle of Experience", Strength = PotionStrength.Supreme, Value = 500, EffectDescription = "Instantly grants +300 XP. Can only be used once between dungeons." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Bottle of the Void", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Any spell cast during the battle suffers a -20 modifier." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Elixir of Speed", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Grants +1 Movement for the rest of the dungeon." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Elixir of the Archer", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Grants +1 DMG to one ranged weapon until you leave the dungeon." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Liquid Fire", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Coats one melee weapon, causing it to deal Fire Damage for one battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Poison", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Coats one weapon or 5 arrows. Enemies hit lose 1 HP per turn for the rest of the battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Dragon's Breath", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants a single-use fire breath attack (1d8 or 2x1d4 damage)." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Dragon Skin", Strength = PotionStrength.Standard, Value = 150, EffectDescription = "The drinker ignores all HP damage for 3 turns." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Fire Protection", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Ignores the secondary damage effect from being on fire for one battle." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Potion of Rage", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants the Frenzy Perk for one battle without spending energy." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Vial of Corrosion", Strength = PotionStrength.Standard, Value = 60, EffectDescription = "Automatically opens one lock." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Vial of Invisibility", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Become invisible for one battle, but cannot fight." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", IsPotion = true, Name = "Weapon Oil", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Grants a +1 DMG modifier to one edged weapon until you leave the dungeon." },
            };
        }

        public static List<Potion> GetAllDistinctPotions()
        {
            // Uses LINQ's DistinctBy to get only the first entry for each unique potion name.
            return GetAllPotions().DistinctBy(p => p.Name).ToList();
        }

        public static List<Potion> GetStandardPotions()
        {
            List<Potion> list = new List<Potion>();
            list.AddRange(Potions.Where(x => x.Strength == PotionStrength.Standard));
            return list;
        }

        public static Potion GetPotionByName(string name)
        {
            return Potions.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }

        public static Potion GetPotionByNameStrength(string name, PotionStrength strength)
        {
            List<Potion> list = new List<Potion>();
            list.AddRange(Potions.Where(x => x.Name == name));
            return list.FirstOrDefault(x => x.Strength == strength) ?? throw new NullReferenceException();
        }

        public async Task<List<Potion>> GetRandomPotions(int count, PotionStrength quality)
        {
            List<Potion> potions = new List<Potion>();
            for (int i = 0; i < count; i++)
            {
                switch (quality)
                {
                    case PotionStrength.Weak:
                    case PotionStrength.Supreme:
                        potions.Add(GetPotionByNameStrength(await GetNonStandardPotionAsync(), quality));
                        break;
                    case PotionStrength.Standard:
                        potions.Add(GetPotionByNameStrength(await GetStandardPotionAsync(), quality));
                        break;
                }
            }
            return potions;
        }

        public async Task<Potion> GetPotionByStrengthAsync(PotionStrength strength)
        {
            var potions = await GetRandomPotions(1, strength);
            return potions[0];
        }

        public static List<PotionStrength> GetPotionStrengths(Potion potion)
        {
            List<Potion> potions = (List<Potion>)Potions.Where(x => x.Name == potion.Name);
            List<PotionStrength> strengths = new List<PotionStrength>();
            foreach(Potion p in  potions)
            {
                strengths.Add(p.Strength);
            }
            return strengths;
        }

        /// <summary>
        /// Attempts to brew a potion from a given recipe.
        /// </summary>
        /// <param name="alchemist">The hero attempting to brew the potion.</param>
        /// <param name="recipe">The alchemical recipe to be brewed.</param>
        /// <returns>A string message indicating the success or failure of the brewing attempt.</returns>
        public string BrewPotion(Hero alchemist, AlchemicalRecipe recipe)
        {
            // 1. Check if the alchemist has the required components in their backpack
            foreach (var component in recipe.Components)
            {
                var requiredItem = alchemist.Inventory.Backpack.FirstOrDefault(item => item.Name == component.Name && item.Quantity > 0);
                if (requiredItem == null)
                {
                    return $"Brewing failed: Missing component - {component.Name}.";
                }
            }

            // 2. Consume the components
            foreach (var component in recipe.Components)
            {
                var itemInBackpack = alchemist.Inventory.Backpack.First(item => item.Name == component.Name);
                itemInBackpack.Quantity--;
                if (itemInBackpack.Quantity <= 0)
                {
                    alchemist.Inventory.Backpack.Remove(itemInBackpack);
                }
            }

            // 3. Create the new potion
            var newPotion = new Potion
            {
                Name = recipe.Name,
                Strength = recipe.Strength,
                EffectDescription = recipe.EffectDescription,
                Value = recipe.Value,
                // ... copy other relevant properties from the recipe
            };

            // 4. Add the new potion to the alchemist's backpack
            BackpackHelper.AddItem(alchemist.Inventory.Backpack, newPotion);

            return $"Successfully brewed: {newPotion.Strength} {newPotion.Name}.";
        }
    }

    public class AlchemyItem : Equipment
    {
        public bool IsPotion { get; set; }
        public bool IsIngredient { get; set; }
        public bool IsPart { get; set; }
        public bool IsRecipe { get; set; }

        public AlchemyItem()
        {
            
        }
    }

    public class Ingredient : AlchemyItem
    {
        public Ingredient()
        {
            IsIngredient = true;
        }
    }

    public class Part : AlchemyItem
    {
        public string Origin { get; set; } = "Unknown";

        public Part()
        {
            IsPart = true;
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

    public class Potion : AlchemyItem
    {
        public PotionStrength Strength { get; set; } = PotionStrength.None;
        public string EffectDescription { get; set; } = string.Empty;

        public Potion()
        {
            IsPotion = true;
        }

        public override string ToString()
        {
            return $"{Strength} {Name}: {EffectDescription}";
        }
    }

    public class  AlchemicalRecipe : Potion
    {
        public List<AlchemyItem> Components { get; set; } = new List<AlchemyItem>();

        public override string ToString()
        {
            List<string> components = new List<string>();
            foreach (var component in Components)
            {
                components.Add(component.Name);
            }
            return $"Recipe: {Strength} {Name} (Requires: {string.Join(", ", components.ToArray())})";
        }
    }
}