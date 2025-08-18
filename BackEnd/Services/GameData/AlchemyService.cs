using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LoDCompanion.BackEnd.Services.GameData
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
            DiceRollResult potionRoll = new DiceRollResult();
            if(roll <= 2)
            {
                potionRoll = await _diceRoll.RequestRollAsync($"Roll for standard potion", "1d10"); await Task.Yield();
            }
            else
            {
                potionRoll = await _diceRoll.RequestRollAsync($"Roll for standard potion", "1d8"); await Task.Yield();
            }
            switch (roll)
            {
                case 1:
                    switch (potionRoll.Roll)
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
                    switch (potionRoll.Roll)
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
                    switch (potionRoll.Roll)
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
            var result = await _diceRoll.RequestRollAsync($"Roll for standard potion", "1d12"); await Task.Yield();
            int roll = result.Roll;
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

        private static IngredientName GetIngredient()
        {
            int roll = RandomHelper.GetRandomNumber(0, Ingredients.Count - 1);
            return Ingredients[roll].Name;
        }

        public async Task<Part[]> GetPartsAsync(int count, SpeciesName? origin = null)
        {
            Part[] parts = new Part[count];
            for (int i = 0; i < count; i++)
            {
                if (origin != null)
                {
                    parts[i] = new Part() { Origin = (SpeciesName)origin, Name = GetPart(), IsPart = true };
                }
                else
                {
                    parts[i] = new Part() { Origin = await GetOriginAsync(), Name = GetPart(), IsPart = true };
                }
            }
            return parts;
        }

        private PartName GetPart()
        {
            int roll = RandomHelper.GetRandomNumber(0, Parts.Count - 1);
            return Parts[roll].Name;
        }

        private async Task<SpeciesName> GetOriginAsync()
        {
            var result = await _diceRoll.RequestRollAsync($"Roll for standard potion", "1d100"); await Task.Yield();
            int roll = result.Roll;
            return roll switch
            {
                1 => SpeciesName.Banshee,
                2 => SpeciesName.Basilisk,
                3 => SpeciesName.Beastman,
                4 => SpeciesName.CaveBear,
                <= 10 => SpeciesName.CaveGoblin,
                11 => SpeciesName.Centaur,
                12 => SpeciesName.CommonTroll,
                13 => SpeciesName.DireWolf,
                14 => SpeciesName.DarkElf,
                15 => SpeciesName.Dragon,
                16 => SpeciesName.Drider,
                17 => SpeciesName.Ettin,
                18 => SpeciesName.Frogling,
                19 => SpeciesName.Gargoyle,
                20 => SpeciesName.Gecko,
                21 => SpeciesName.Ghost,
                22 => SpeciesName.Ghoul,
                23 => SpeciesName.Giant,
                <= 35 => SpeciesName.GiantBat,
                36 => SpeciesName.GiantCentipede,
                37 => SpeciesName.GiantLeech,
                <= 48 => SpeciesName.GiantPoxRat,
                <= 55 => SpeciesName.GiantRat,
                56 => SpeciesName.GiantScorpion,
                <= 58 => SpeciesName.GiantSnake,
                59 => SpeciesName.GiantSpider,
                60 => SpeciesName.GiantToad,
                61 => SpeciesName.GiantWolf,
                62 => SpeciesName.GiganticSnake,
                63 => SpeciesName.GiganticSpider,
                64 => SpeciesName.Gnoll,
                65 => SpeciesName.Goblin,
                66 => SpeciesName.Griffon,
                67 => SpeciesName.Harpy,
                68 => SpeciesName.Hydra,
                69 => SpeciesName.Lurker,
                70 => SpeciesName.Medusa,
                71 => SpeciesName.Mimic,
                72 => SpeciesName.Minotaur,
                73 => SpeciesName.MinotaurSkeleton,
                74 => SpeciesName.Mummy,
                75 => SpeciesName.Naga,
                76 => SpeciesName.Ogre,
                77 => SpeciesName.Orc,
                78 => SpeciesName.Raptor,
                79 => SpeciesName.RiverTroll,
                80 => SpeciesName.Salamander,
                81 => SpeciesName.Satyr,
                82 => SpeciesName.Saurian,
                83 => SpeciesName.Shambler,
                84 => SpeciesName.Skeleton,
                85 => SpeciesName.Slime,
                86 => SpeciesName.Sphinx,
                87 => SpeciesName.StoneGolem,
                88 => SpeciesName.StoneTroll,
                89 => SpeciesName.TombGuardian,
                90 => SpeciesName.Vampire,
                91 => SpeciesName.Werewolf,
                92 => SpeciesName.Wight,
                93 => SpeciesName.Wraith,
                94 => SpeciesName.Wyvern,
                <= 99 => SpeciesName.Zombie,
                100 => SpeciesName.ZombieOgre,
                _ => SpeciesName.Unknown
            };
        }

        private static List<Part> GetPartsList()
        {
            return new List<Part>()
            {
                new Part() { Name = PartName.Brain },
                new Part() { Name = PartName.Kidney },
                new Part() { Name = PartName.Saliva},
                new Part() { Name = PartName.Blood },
                new Part() { Name = PartName.Skin },
                new Part() { Name = PartName.Nails },
                new Part() { Name = PartName.Hair },
                new Part() { Name = PartName.Eye },
                new Part() { Name = PartName.Tongue },
                new Part() { Name = PartName.Heart }
            };
        }

        private static List<Ingredient> GetIngredientsList()
        {
            return new List<Ingredient> {
                new Ingredient() { Name = IngredientName.Lunarberry },
                new Ingredient() { Name = IngredientName.DragonStalk },
                new Ingredient() { Name = IngredientName.EmberBark },
                new Ingredient() { Name = IngredientName.MountainBarberry },
                new Ingredient() { Name = IngredientName.SaltyWyrmwood },
                new Ingredient() { Name = IngredientName.AshenGinger },
                new Ingredient() { Name = IngredientName.SpicyWindroot },
                new Ingredient() { Name = IngredientName.Wintercress },
                new Ingredient() { Name = IngredientName.SweetIvy },
                new Ingredient() { Name = IngredientName.MonksLaurel },
                new Ingredient() { Name = IngredientName.Nightshade },
                new Ingredient() { Name = IngredientName.WeepingClover },
                new Ingredient() { Name = IngredientName.Snakeberry },
                new Ingredient() { Name = IngredientName.Bitterweed },
                new Ingredient() { Name = IngredientName.ArchingPokeroot },
                new Ingredient() { Name = IngredientName.ToxicHogweed },
                new Ingredient() { Name = IngredientName.BlueConeflower },
                new Ingredient() { Name = IngredientName.GiantRaspberry },
                new Ingredient() { Name = IngredientName.BrightGallberry },
                new Ingredient() { Name = IngredientName.BarbedWormwood },
            };
        }

        private static List<Potion> GetAllPotions()
        {
            return new List<Potion>()
            {
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Health", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Heals 1d4 Hit Points.", Availability = 4,
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.HealHP, 4 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Health", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Heals 1d6 Hit Points.", Availability = 4,
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.HealHP, 6 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Health", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Heals 1d10 Hit Points.", Availability = 3,
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.HealHP, 10 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Restoration", Strength = PotionStrength.Standard, Value = 200, EffectDescription = "Restores a hero to full health and removes any disease or poison.", Availability = 1, 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CureDisease, 100 }, { PotionProperty.CurePoison, 100 }, { PotionProperty.HealHP, 999 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Cure Disease", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "75% chance to remove all effects of disease.", Availability = 3, 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CureDisease, 75 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Cure Disease", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Removes all effects of disease.", Availability = 3, 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CureDisease, 100 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Cure Disease", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Removes all effects of disease and heals 1d3 HP.", Availability = 2, 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CureDisease, 100 }, { PotionProperty.HealHP, 3 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Cure Poison", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "75% chance to remove all effects of poison.", Availability = 3, 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CurePoison, 75 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Cure Poison", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Removes all effects of poison.", Availability = 3, 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CurePoison, 100 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Common", Name = "Potion of Cure Poison", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Removes all effects of poison and heals 1d3 HP.", Availability = 2, 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CurePoison, 100 }, { PotionProperty.HealHP, 3 } } },

                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Strength", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Strength until the end of the next battle.", 
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Strength, -1, statBonus: (BasicStat.Strength, 10), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Strength", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Strength until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Strength, -1, statBonus: (BasicStat.Strength, 15), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Strength", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Strength until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Strength, -1, statBonus: (BasicStat.Strength, 20), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Constitution", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Constitution until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Constitution, -1, statBonus: (BasicStat.Constitution, 10), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Constitution", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Constitution until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Constitution, -1, statBonus: (BasicStat.Constitution, 15), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Constitution", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Constitution until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Constitution, -1, statBonus: (BasicStat.Constitution, 20), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Dexterity", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +5 Dexterity until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Dexterity, -1, statBonus: (BasicStat.Dexterity, 5), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Dexterity", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +10 Dexterity until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Dexterity, -1, statBonus: (BasicStat.Dexterity, 10), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Dexterity", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +15 Dexterity until the end of the next battle.", 
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Dexterity, -1, statBonus :(BasicStat.Dexterity, 15), removeAfterNextBattle : true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Wisdom", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Wisdom until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Wisdom, -1, statBonus: (BasicStat.Wisdom, 10), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Wisdom", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Wisdom until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Wisdom, -1, statBonus: (BasicStat.Wisdom, 15), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Wisdom", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Wisdom until the end of the next battle.", 
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Wisdom, -1, statBonus :(BasicStat.Wisdom, 20), removeAfterNextBattle : true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Courage", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +10 Resolve until the end of the next battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Resolve, -1, statBonus: (BasicStat.Resolve, 10), removeAfterNextBattle: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Courage", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +15 Resolve until the end of the next battle.", 
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Resolve, -1, statBonus :(BasicStat.Resolve, 15), removeAfterNextBattle : true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Courage", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +20 Resolve until the end of the next battle.", 
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Resolve, -1, statBonus :(BasicStat.Resolve, 20), removeAfterNextBattle : true) },
                                                                       
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Energy", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Grants +1 Energy until the end of the dungeon.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Energy, 1 } }, ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Energy, -1, statBonus: (BasicStat.Energy, 1)) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Energy", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants +2 Energy until the end of the dungeon.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Energy, 2 } }, ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Energy, -1, statBonus: (BasicStat.Energy, 2)) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Energy", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Grants +3 Energy until the end of the dungeon.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Energy, 3 } }, ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Energy, -1, statBonus: (BasicStat.Energy, 3)) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Mana", Strength = PotionStrength.Weak, Value = 75, EffectDescription = "Restores 1d20 Mana.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Mana, 20 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Mana", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Restores 2d20 Mana.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Mana, 40 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Mana", Strength = PotionStrength.Supreme, Value = 200, EffectDescription = "Restores 3d20 Mana.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Mana, 60 } } },
                                                                       
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Acidic Bomb (Tr)", Strength = PotionStrength.Weak, Value = 60, EffectDescription = "Explodes for 1d6 Acidic damage in the target square and half to adjacent squares.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 }, { PotionProperty.AcidDamage, 6 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Acidic Bomb (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Explodes for 1d8 Acidic damage in the target square and half to adjacent squares.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 }, { PotionProperty.AcidDamage, 8 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Acidic Bomb (Tr)", Strength = PotionStrength.Supreme, Value = 180, EffectDescription = "Explodes for 1d12 Acidic damage in the target square and half to adjacent squares.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 }, { PotionProperty.AcidDamage, 12 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Firebomb (Tr)", Strength = PotionStrength.Weak, Value = 60, EffectDescription = "Explodes for 1d6 Fire damage in the target square and half to adjacent squares.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 }, { PotionProperty.FireDamage, 6 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Firebomb (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Explodes for 1d8 Fire damage in the target square and half to adjacent squares.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 }, { PotionProperty.FireDamage, 8 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Firebomb (Tr)", Strength = PotionStrength.Supreme, Value = 180, EffectDescription = "Explodes for 1d12 Fire damage in the target square and half to adjacent squares.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 }, { PotionProperty.FireDamage, 12 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Smoke (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Creates a thick smoke in a 3x3 area, obscuring LOS and giving -20 CS to fights within. Lasts 4 turns.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Disorientation (Tr)", Strength = PotionStrength.Standard, Value = 90, EffectDescription = "Target must pass a RES test or forfeit their next turn. Adjacent models test at +20 RES.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Throwable, 1 } } },
                                                                       
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Alchemical Dust", Strength = PotionStrength.Standard, Value = 60, EffectDescription = "Allows a reroll on a search check for one room or corridor." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Bottle of Experience", Strength = PotionStrength.Weak, Value = 250, EffectDescription = "Instantly grants +100 XP. Can only be used once between dungeons.",
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Experience, 100 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Bottle of Experience", Strength = PotionStrength.Standard, Value = 350, EffectDescription = "Instantly grants +200 XP. Can only be used once between dungeons.",
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Experience, 200 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Bottle of Experience", Strength = PotionStrength.Supreme, Value = 500, EffectDescription = "Instantly grants +300 XP. Can only be used once between dungeons.",
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.Experience, 300 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Bottle of the Void", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Any spell cast during the battle suffers a -20 modifier." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Elixir of Speed", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Grants +1 Movement for the rest of the dungeon.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Speed, -1, statBonus: (BasicStat.Move, 1))},
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Elixir of the Archer", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Grants +1 DMG to one ranged weapon until you leave the dungeon.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.WeaponCoating, -1 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Liquid Fire", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Coats one melee weapon, causing it to deal Fire Damage for one battle.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.WeaponCoating, 1 }, { PotionProperty.FireDamage, 0 } } },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Poison", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Coats one weapon or 5 arrows. Enemies hit lose 1 HP per turn for the rest of the battle.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.AmmoCoating, 5 }, { PotionProperty.WeaponCoating, 1 } }, ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Poisoned, -1) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Dragon's Breath", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants a single-use fire breath attack (1d8 or 2x1d4 damage).",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.DragonBreath, -1)},
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Dragon Skin", Strength = PotionStrength.Standard, Value = 150, EffectDescription = "The drinker ignores all HP damage for 3 turns.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.DragonSkin, 3) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Fire Protection", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Ignores the secondary damage effect from being on fire for one battle.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.FireProtection, -1, removeAfterCombat: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Potion of Rage", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Grants the Frenzy Perk for one battle without spending energy.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Frenzy, -1, removeAfterCombat: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Vial of Corrosion", Strength = PotionStrength.Standard, Value = 60, EffectDescription = "Automatically opens one lock." },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Vial of Invisibility", Strength = PotionStrength.Standard, Value = 100, EffectDescription = "Become invisible for one battle, but cannot fight.",
                    ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.Invisible, -1, removeAfterCombat: true) },
                new Potion(){ Shop = ShopCategory.Potions, Category = "Uncommon", Name = "Weapon Oil", Strength = PotionStrength.Standard, Value = 80, EffectDescription = "Grants a +1 DMG modifier to one edged weapon until you leave the dungeon.", 
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.WeaponCoating, -1 } } },
            };
        }

        public static List<Potion> GetAllDistinctPotions()
        {
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

        private bool ValidateComponents(PotionStrength strength, List<AlchemyItem> components, AlchemicalRecipe? recipe, out string errorMessage)
        {
            var parts = components.Where(c => c is Part);
            var ingredients = components.Where(c => c is Ingredient);

            int partCount = parts.Count();
            int ingredientCount = ingredients.Count();

            if (recipe != null)
            {
                var requiredComponents = recipe.Components.GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.Count());
                var providedComponents = components.GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.Count());

                if (requiredComponents.Count != providedComponents.Count)
                {
                    errorMessage = "The provided components do not match the recipe.";
                    return false;
                }

                foreach (var required in requiredComponents)
                {
                    if (!providedComponents.ContainsKey(required.Key) || providedComponents[required.Key] != required.Value)
                    {
                        errorMessage = $"Missing or incorrect quantity of {required.Key}.";
                        return false;
                    }
                }
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                switch (strength)
                {
                    case PotionStrength.Weak:
                        if (partCount == 1 && ingredientCount == 1)
                        {
                            errorMessage = string.Empty;
                            return true;
                        }
                        errorMessage = "Weak potions require 1 part and 1 ingredient.";
                        return false;
                    case PotionStrength.Standard:
                        if ((partCount == 1 && ingredientCount == 2) || (partCount == 2 && ingredientCount == 1))
                        {
                            errorMessage = string.Empty;
                            return true;
                        }
                        errorMessage = "Standard potions require either 1 part and 2 ingredients, or 2 parts and 1 ingredient.";
                        return false;
                    case PotionStrength.Supreme:
                        if (partCount + ingredientCount == 4 && partCount > 0 && ingredientCount > 0)
                        {
                            errorMessage = string.Empty;
                            return true;
                        }
                        errorMessage = "Supreme potions require a total of 4 components, with at least one part and one ingredient.";
                        return false;
                    default:
                        errorMessage = "Invalid potion strength selected.";
                        return false;
                }
            }
        }

        /// <summary>
        /// Attempts to brew a potion from a given recipe.
        /// </summary>
        /// <param name="alchemist">The hero attempting to brew the potion.</param>
        /// <param name="recipe">The alchemical recipe to be brewed.</param>
        /// <returns>A string message indicating the success or failure of the brewing attempt.</returns>
        public async Task<string> BrewPotion(Hero alchemist, PotionStrength strength, List<AlchemyItem> components, PowerActivationService activation, AlchemicalRecipe? recipe = null, bool secondAttempt = false)
        {
            // Validate Components
            if (!ValidateComponents(strength, components, recipe, out string validationError))
            {
                return $"Brewing failed: {validationError}";
            }

            // Check for Empty Bottle
            var emptyBottle = alchemist.Inventory.Backpack.FirstOrDefault(item => item.Name == "Empty Bottle" && item.Quantity > 0);
            if (emptyBottle == null)
            {
                return "Brewing failed: No empty bottle available.";
            }

            // Perform Alchemy Skill Roll
            int alchemySkill = alchemist.GetSkill(Skill.Alchemy);
            if (recipe != null)
            {
                alchemySkill += 10; // +10 modifier for using a recipe
            }

            var resultRoll = await _diceRoll.RequestRollAsync("Attempting to brew potion...", "1d100");
            await Task.Yield();
            int skillRoll = resultRoll.Roll;

            if (skillRoll > alchemySkill)
            {
                // Consume Components and Bottle
                foreach (var component in components)
                {
                    BackpackHelper.TakeOneItem(alchemist.Inventory.Backpack, component);
                }
                BackpackHelper.TakeOneItem(alchemist.Inventory.Backpack, emptyBottle);
                return "Brewing failed: The alchemical process was unsuccessful.";
            }

            // Create the Potion
            Potion newPotion;
            if (recipe != null)
            {
                newPotion = GetPotionByName(recipe.Name).Clone();
            }
            else
            {
                newPotion = (await GetPotionByStrengthAsync(strength)).Clone();
            }

            var preciseMixing = alchemist.Perks.FirstOrDefault(p => p.Name == PerkName.PreciseMixing);
            if (preciseMixing != null && !secondAttempt)
            {
                var choiceResult = await new UserRequestService().RequestYesNoChoiceAsync($"You brewed a {newPotion.ToString()}, does {alchemist.Name} wish to attempt to use his perk {preciseMixing.ToString()}");
                await Task.Yield();
                if (choiceResult)
                {
                    if (await activation.ActivatePerkAsync(alchemist, preciseMixing))
                    {
                        return await BrewPotion(alchemist, strength, components, activation, secondAttempt: true);
                    }
                }
            }

            var perfectHealer = alchemist.Perks.FirstOrDefault(p => p.Name == PerkName.PerfectHealer);
            if (newPotion.PotionProperties != null && newPotion.PotionProperties.Any(p => p.Key == PotionProperty.HealHP) && perfectHealer != null)
            {
                var choiceResult = await new UserRequestService().RequestYesNoChoiceAsync($"Does {alchemist.Name} wish to use {perfectHealer.ToString()}");
                await Task.Yield();
                if(choiceResult)
                {
                    if(await activation.ActivatePerkAsync(alchemist, perfectHealer))
                    {
                        newPotion.PotionProperties.TryAdd(PotionProperty.HealHPBonus, 3);
                    }
                }
            }

            // Consume Components and Bottle
            foreach (var component in components)
            {
                BackpackHelper.TakeOneItem(alchemist.Inventory.Backpack, component);
            }
            BackpackHelper.TakeOneItem(alchemist.Inventory.Backpack, emptyBottle);

            // Add Potion to Inventory
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

    public enum IngredientName
    {
        Lunarberry,
        DragonStalk,
        EmberBark,
        MountainBarberry,
        SaltyWyrmwood,
        AshenGinger,
        SpicyWindroot,
        Wintercress,
        SweetIvy,
        MonksLaurel,
        Nightshade,
        WeepingClover,
        Snakeberry,
        Bitterweed,
        ArchingPokeroot,
        ToxicHogweed,
        BlueConeflower,
        GiantRaspberry,
        BrightGallberry,
        BarbedWormwood,
        BlackAcathusLeaf,
    }

    public class Ingredient : AlchemyItem
    {
        public new IngredientName Name { get; set; }
        public bool Exquisite { get; set; }
        public Ingredient()
        {
            IsIngredient = true;
        }

        public override string ToString()
        {
            if (Exquisite)
            {
                return $"Exquisite {Name.ToString()}";
            }
            else
            {
                return $"{Name.ToString()}";
            }
        }
    }

    public enum PartName
    {
        Brain,
        Kidney,
        Saliva,
        Blood,
        Skin,
        Nails, 
        Hair, 
        Eye,
        Tongue,
        Heart
    }

    public class Part : AlchemyItem
    {
        public new PartName Name { get; set; }
        public SpeciesName Origin { get; set; } = SpeciesName.Unknown;
        public bool Exquisite {  set; get; }

        public Part()
        {
            IsPart = true;
        }

        public override string ToString()
        {
            if(Exquisite)
            {
                return $"Exquisite {Origin} {Name.ToString()}";
            }
            else
            {
                return $"{Origin} {Name.ToString()}"; 
            }
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

    public enum PotionProperty
    {
        HealHP,
        Throwable,
        AmmoCoating,
        WeaponCoating,
        FireDamage,
        AcidDamage,
        Poison,
        Energy,
        Mana,
        CurePoison,
        CureDisease,
        HealHPBonus,
        Experience,
        HolyDamage
    }

    public class Potion : AlchemyItem
    {
        public PotionStrength Strength { get; set; } = PotionStrength.None;
        public string EffectDescription { get; set; } = string.Empty;
        public Dictionary<PotionProperty, int>? PotionProperties { get; set; }

        public Potion()
        {
            IsPotion = true;
            Durability = 1;
        }

        public override Potion Clone()
        {
            var newPotion = new Potion();
            newPotion.Strength = Strength;
            newPotion.EffectDescription = EffectDescription;
            newPotion.Category = Category;
            newPotion.Shop = Shop;
            newPotion.Name = Name;
            newPotion.Encumbrance = Encumbrance;
            newPotion.Value = Value;
            newPotion.Availability = Availability;
            newPotion.MaxDurability = MaxDurability;
            newPotion.Durability = Durability;
            newPotion.Quantity = Quantity;
            newPotion.Description = Description;
            newPotion.MagicEffect = MagicEffect;
            newPotion.Storage = Storage;
            newPotion.Properties = new Dictionary<EquipmentProperty, int>(Properties);
            newPotion.Identified = Identified;
            return newPotion;
        }

        public override string ToString()
        {
            if (Identified)
            {
                return $"{Strength} {Name}: {EffectDescription}";
            }
            else
            {
                return "Unidentified Potion";
            }
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