﻿using LoDCompanion.Services.GameData;
using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    public class TreasureService
    {
        private readonly GameDataService _gameData;
        private const int DefaultArmourDurability = 6;
        private const int DefaultWeaponDurability = 6;

        public TreasureService(GameDataService gameData)
        {
            _gameData = gameData;
        }

        public List<string> GetTreasures(List<Equipment> itemsFound)
        {
            List<string> rewards = new List<string>();
            foreach (Equipment item in itemsFound)
            {
                rewards.Add(item.Name);
                // In a web project, you would not "BuildEquipmentWithParent" as there's no visual hierarchy
                // You would simply add the item to the player's inventory or dungeon loot list
            }
            return rewards;
        }

        public string GetTreasure(string itemName, int durability = 0, int value = 0, int amount = 1, string description = "")
        {
            Equipment item = CreateItem(itemName, durability, value, amount, description);
            // Again, no "BuildEquipmentWithParent" as there's no Unity game object to build
            return $"{item.Quantity} {item.Name}";
        }
        public List<string> SearchCorpse(string type, Hero hero, int searchRoll)
        {
            List<string> rewards = new List<string>();

            int count = 1;
            if (hero.IsThief)
            {
                count = 2;
            }

            switch (type)
            {
                case "T1":
                    if (searchRoll == 0 || searchRoll > 10)
                    {
                        searchRoll = RandomHelper.GetRandomNumber(1, 10);
                    }
                    switch (searchRoll)
                    {
                        case 1:
                            Equipment equipmentFound = GetRandomWeapon(DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4)));
                            rewards.Add(GetTreasure(equipmentFound.Name, equipmentFound.Durability));
                            break;
                        case 2:
                            rewards.Add(GetTreasure("Coin", 0, 1, 20));
                            break;
                        case 3:
                            rewards.Add(GetTreasure("Coin", 0, 1, 10));
                            break;
                        case 4:
                            rewards.Add(GetTreasure("Bandage (old rags)", 1));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case "T2":
                    if (searchRoll == 0 || searchRoll > 10)
                    {
                        searchRoll = RandomHelper.GetRandomNumber(1, 10);
                    }
                    switch (searchRoll)
                    {
                        case 1:
                            rewards.AddRange(FoundTreasure("Fine", count));
                            break;
                        case 2:
                            rewards.AddRange(FoundTreasure("Mundane", count));
                            break;
                        case 3:
                            rewards.Add(GetTreasure("Coin", 0, 1, 50));
                            break;
                        case 4:
                            rewards.Add(GetTreasure("Coin", 0, 1, 40));
                            break;
                        case 5:
                            rewards.Add(GetTreasure("Coin", 0, 1, 20));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case "T3":
                    if (searchRoll == 0 || searchRoll > 10)
                    {
                        searchRoll = RandomHelper.GetRandomNumber(1, 10);
                    }
                    switch (searchRoll)
                    {
                        case 1:
                        case 2:
                            rewards.AddRange(FoundTreasure("Fine", count));
                            break;
                        case 3:
                        case 4:
                            rewards.Add(GetTreasure("Coin", 0, 1, 100));
                            break;
                        case 5:
                            rewards.AddRange(FoundTreasure("Mundane", count));
                            break;
                        case 6:
                            rewards.Add(GetTreasure("Coin", 0, 1, 80));
                            break;
                        case 7:
                            rewards.Add(GetTreasure("Coin", 0, 1, 60));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case "T4":
                    if (searchRoll == 0 || searchRoll > 10)
                    {
                        searchRoll = RandomHelper.GetRandomNumber(1, 10);
                    }
                    switch (searchRoll)
                    {
                        case 1:
                            rewards.Add(GetTreasure("Grimoire"));
                            break;
                        case 2:
                        case 3:
                            rewards.Add(GetTreasure("Scroll"));
                            break;
                        case 4:
                            for (int i = 0; i < RandomHelper.GetRandomNumber(1, 2); i++)
                            {
                                rewards.Add(AlchemyService.GetRandomPotions(1, RandomHelper.GetRandomEnumValue<PotionStrength>(1, 3))[0].Name);
                            }
                            break;
                        case 5:
                            rewards.Add(GetTreasure("Coin", 0, 1, 150));
                            break;
                        case 6:
                            rewards.Add(GetTreasure("Coin", 0, 1, 100));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case "T5":
                    if (searchRoll == 0 || searchRoll > 10)
                    {
                        searchRoll = RandomHelper.GetRandomNumber(1, 10);
                    }
                    switch (searchRoll)
                    {
                        case 1:
                        case 2:
                            rewards.AddRange(FoundTreasure("Wonderful", count));
                            count *= 2;
                            rewards.AddRange(FoundTreasure("Fine", count));
                            break;
                        case 3:
                        case 4:
                            count *= 2;
                            rewards.AddRange(FoundTreasure("Fine", count));
                            rewards.Add(GetTreasure("Grimoire"));
                            break;
                        case 5:
                        case 6:
                        case 7:
                            count *= 3;
                            rewards.AddRange(FoundTreasure("Fine", count));
                            break;
                        case 8:
                        case 9:
                        case 10:
                            rewards.Add(GetTreasure("Coin", 0, 1, 500));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case "Part":
                    if (searchRoll == 0)
                    {
                        searchRoll = RandomHelper.GetRandomNumber(1, 100);
                    }
                    if (searchRoll <= hero.AlchemySkill)
                    {
                        rewards.Add(GetTreasure("Part", 0, 0, 1, GetAlchemicalTreasure("Part", 1, false)));
                    }
                    else
                    {
                        rewards.Add("You found nothing.");
                    }
                    break;
                default:
                    break;
            }
            return rewards;
        }

        public List<string> FoundTreasure(string type, int count)
        {
            List<string> rewards = new List<string>();
            for (int i = 0; i < count; i++)
            {
                Equipment item;
                switch (type)
                {
                    case "Mundane":
                        item = GetMundaneTreasure();
                        rewards.Add(item.Name);
                        break;
                    case "Fine":
                        item = GetFineTreasure();
                        rewards.Add(item.Name);
                        break;
                    case "Wonderful":
                        item = GetWonderfulTreasure();
                        rewards.Add(item.Name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }
            return rewards;
        }

        public Equipment GetMundaneTreasure()
        {
            int roll = RandomHelper.GetRandomNumber(1, 54);
            int defaultDurabilityDamageRoll = RandomHelper.GetRandomNumber(1, 4) + 1;
            int armourDurability = DefaultArmourDurability - defaultDurabilityDamageRoll;
            int weaponDurability = DefaultWeaponDurability - defaultDurabilityDamageRoll;
            // Console.WriteLine($"Treasure roll {roll}"); // For debugging, replace with proper logging

            string itemName = "";
            Equipment treasure;

            switch (roll)
            {
                case 1: treasure = CreateItem("Amulet"); break;
                case 2: treasure = EquipmentService.GetAmmoByNameSetQuantity(_gameData, "Arrow", 5); break;
                case 3: treasure = EquipmentService.GetAmmoByNameSetQuantity(_gameData, "Arrow", 10); break;
                case 4:
                case 5: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Bandage (old rags)", RandomHelper.GetRandomNumber(1, 3)); break;
                case 6: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Bandage (linen)", RandomHelper.GetRandomNumber(1, 2)); break;
                case 7: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Bear Trap", (2 - (RandomHelper.GetRandomNumber(1, 2) - 1)), RandomHelper.GetRandomNumber(1, 3)); break;
                case 8:
                case 9: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Bedroll", RandomHelper.GetRandomNumber(1, 3)); break;
                case 10: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Beef Jerky", RandomHelper.GetRandomNumber(1, 4)); break;
                case 11: treasure = EquipmentService.GetAmmoByNameSetQuantity(_gameData, "Bolt", 5); break;
                case 12: treasure = EquipmentService.GetShieldByNameSetDurability(_gameData, "Buckler", armourDurability); break;
                case 13:
                case 14: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(1, 20)); break;
                case 15: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(2, 40)); break;
                case 16:
                case 17: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(3, 60)); break;
                case 18: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(4, 80)); break;
                case 19: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(1, 100)); break;
                case 20: treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, "Cloak", armourDurability); break;
                case 21: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Crowbar", (6 - defaultDurabilityDamageRoll), RandomHelper.GetRandomNumber(1, 6)); break;
                case 22: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Dagger", weaponDurability); break;
                case 23: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Empty Bottle", RandomHelper.GetRandomNumber(1, 6)); break;
                case 24: treasure = GetFineTreasure(); break;
                case 25: treasure = CreateItem("Ingredient", 0, 0, 1, GetAlchemicalTreasure("Ingredient", RandomHelper.GetRandomNumber(1, 3))); break;
                case 26: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Javelin", weaponDurability); break;
                case 27: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Lantern", RandomHelper.GetRandomNumber(1, 3)); break;
                case 28:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1: itemName = "Leather Cap"; break;
                        case 2:
                        case 3: itemName = "Leather Vest"; break;
                        case 4: itemName = "Leather Jacket"; break;
                        case 5: itemName = "Leather Leggings"; break;
                        case 6: itemName = "Leather Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, armourDurability);
                    break;
                case 29: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Lock Picks", RandomHelper.GetRandomNumber(1, 4)); break;
                case 30: treasure = EquipmentService.GetEquipmentByName(_gameData, "Backpack - Medium"); break;
                case 31: treasure = EquipmentService.GetEquipmentByName(_gameData, "Rope (old)"); break;
                case 32:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1: itemName = "Padded Cap"; break;
                        case 2:
                        case 3: itemName = "Padded Vest"; break;
                        case 4: itemName = "Padded Jacket"; break;
                        case 5: itemName = "Padded Pants"; break;
                        case 6: itemName = "Padded Coat"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, armourDurability);
                    break;
                case 33: treasure = CreateItem("Part", 0, 0, 1, GetAlchemicalTreasure("Ingredient", RandomHelper.GetRandomNumber(1, 3))); break;
                case 34: treasure = CreateItem("Part", 0, 0, 1, GetAlchemicalTreasure("Part", 1)); break;
                case 35: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Rapier", weaponDurability); break;
                case 36: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Ration", RandomHelper.GetRandomNumber(1, 4)); break;
                case 37: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Ration", RandomHelper.GetRandomNumber(1, 6)); break;
                case 38: treasure = CreateItem("Potion Recipe - Weak"); break;
                case 39:
                case 40: treasure = EquipmentService.GetEquipmentByName(_gameData, "Ring"); break;
                case 41: treasure = EquipmentService.GetEquipmentByName(_gameData, "Rope"); break;
                case 42: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Shortbow", weaponDurability); break;
                case 43: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Shortsword", weaponDurability); break;
                case 44: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Skinning Knife", RandomHelper.GetRandomNumber(1, 3)); break;
                case 45: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Sling", weaponDurability); break;
                case 46: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Staff", weaponDurability); break;
                case 47:
                case 48:
                case 49: treasure = EquipmentService.GetEquipmentByName(_gameData, "Torch"); break;
                case 50: treasure = EquipmentService.GetEquipmentByName(_gameData, "Whetstone"); break;
                case 51: treasure = EquipmentService.GetEquipmentByName(_gameData, "Wild game traps"); break;
                case 52: treasure = CreateItem("Wolf Pelt", 0, 50, RandomHelper.GetRandomNumber(1, 3)); break;
                case 53: treasure = GetWonderfulTreasure(); break;
                case 54: treasure = CreateItem("Ingredient", 0, 0, 1, GetAlchemicalTreasure("Ingredient", RandomHelper.GetRandomNumber(1, 3))); break;
                default:
                    treasure = CreateItem("Unknown Mundane Item"); // Fallback for unexpected rolls
                    break;
            }

            return treasure;
        }

        public Equipment GetFineTreasure()
        {
            string itemName = "";
            int roll = RandomHelper.GetRandomNumber(1, 54);
            // Console.WriteLine($"Treasure roll {roll}");

            Equipment treasure = new Equipment();

            switch (roll)
            {
                case 1: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Alchemist Tool", 6 - RandomHelper.GetRandomNumber(1, 4)); break;
                case 2: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Alchemist Belt", 6 - RandomHelper.GetRandomNumber(1, 4)); break;
                case 3: treasure = EquipmentService.GetEquipmentByName(_gameData, "Armour Repair Kit"); break;
                case 4:
                    roll = RandomHelper.GetRandomNumber(1, 3);
                    switch (roll)
                    {
                        case 1: itemName = "Battleaxe"; break;
                        case 2: itemName = "Greataxe"; break;
                        case 3: itemName = "Halberd"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 5:
                    treasure = EquipmentService.GetAmmoByName(_gameData, "Barbed Arrow");
                    treasure.Quantity = 5;
                    break;
                case 6:
                    treasure = EquipmentService.GetAmmoByName(_gameData, "Barbed Bolt");
                    treasure.Quantity = 5;
                    break;
                case 7: treasure = EquipmentService.GetEquipmentByName(_gameData, "Bedroll"); break;
                case 8:
                    roll = RandomHelper.GetRandomNumber(1, 2);
                    switch (roll)
                    {
                        case 1: itemName = "Morning Star"; break;
                        case 2: itemName = "Flail"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 9: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(1, 100) + 40); break;
                case 10: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(2, 200) + 20); break;
                case 11: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(3, 300)); break;
                case 12: treasure = EquipmentService.GetEquipmentByName(_gameData, "Door Mirror"); break;
                case 13: treasure = CreateItem("Lock Picks - Dwarven", 1, 0, RandomHelper.GetRandomNumber(1, 6)); break;
                case 14: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Elven Bow", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 2)))); break;
                case 15: treasure = EquipmentService.GetEquipmentByName(_gameData, "Elven Skinning Knife"); break;
                case 16: treasure = CreateItem("Ingredient - Exquisite", 0, 0, 1, GetAlchemicalTreasure("Ingredient", 1)); break;
                case 17: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Extended Battle Belt", (6 - (RandomHelper.GetRandomNumber(1, 4)))); break;
                case 18: treasure = EquipmentService.GetEquipmentByName(_gameData, "Fishing Gear"); break;
                case 19: treasure = CreateItem("Gemstone", 0, RandomHelper.GetRandomNumber(3, 300)); break;
                case 20: treasure = CreateItem("Gemstone", 0, 100, RandomHelper.GetRandomNumber(1, 6)); break;
                case 21:
                    roll = RandomHelper.GetRandomNumber(1, 2);
                    switch (roll)
                    {
                        case 1: itemName = "Battle Hammer"; break;
                        case 2: itemName = "Warhammer"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 22: treasure = EquipmentService.GetShieldByNameSetDurability(_gameData, "Heater Shield", (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4)))); break;
                case 23: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Iron Wedge", 6, (RandomHelper.GetRandomNumber(1, 3))); break;
                case 24: treasure = EquipmentService.GetEquipmentByName(_gameData, "Backpack - Large"); break;
                case 25:
                case 26:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1: itemName = "Leather Cap"; break;
                        case 2:
                        case 3: itemName = "Leather Vest"; break;
                        case 4: itemName = "Leather Jacket"; break;
                        case 5: itemName = "Leather Leggings"; break;
                        case 6: itemName = "Leather Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4)));
                    break;
                case 27: treasure = CreateItem("Lute"); break;
                case 28:
                case 29:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1: itemName = "Mail Coif"; break;
                        case 2: itemName = "Mail Shirt"; break;
                        case 3: itemName = "Sleeved Mail Shirt"; break;
                        case 4: itemName = "Mail Coat"; break;
                        case 5: itemName = "Mail Leggings"; break;
                        case 6: itemName = "Sleeved Mail Coat"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4)));
                    break;
                case 30: treasure = EquipmentService.GetEquipmentByName(_gameData, "Necklace"); treasure.Value = RandomHelper.GetRandomNumber(3, 300); break;
                case 31: treasure = CreateItem("Part - Exquisite", 0, 0, 1, GetAlchemicalTreasure("Part", 1)); break;
                case 32: treasure = EquipmentService.GetEquipmentByName(_gameData, "Partial Map"); break;
                case 33:
                case 34: treasure = AlchemyService.GetPotionByStrength(PotionStrength.Standard); break;
                case 35: treasure = AlchemyService.GetPotionByName("Potion of Health"); break;
                case 36:
                    roll = RandomHelper.GetRandomNumber(1, 4);
                    switch (roll)
                    {
                        case 1: itemName = "Crossbow"; break;
                        case 2: itemName = "Longbow"; break;
                        case 3: itemName = "Crossbow Pistol"; break;
                        case 4: itemName = "Arbalest"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1));
                    break;
                case 37: treasure = CreateItem("Relic"); break;
                case 38: treasure = EquipmentService.GetEquipmentByName(_gameData, "Ring"); break;
                case 39: treasure = CreateItem("Scroll"); break;
                case 40: treasure = CreateItem("Scroll", 0, 100, RandomHelper.GetRandomNumber(1, 3)); break;
                case 41:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity(_gameData, "Silver Arrow", RandomHelper.GetRandomNumber(1, 10));
                    break;
                case 42:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity(_gameData, "Silver Bolt", RandomHelper.GetRandomNumber(1, 10));
                    break;
                case 43:
                case 44:
                    roll = RandomHelper.GetRandomNumber(1, 8);
                    int durabilityDamageRoll = RandomHelper.GetRandomNumber(1, 3) - 1;
                    switch (roll)
                    {
                        case 1: itemName = "Greatsword"; break;
                        case 2: itemName = "Greataxe"; break;
                        case 3: itemName = "Flail"; break;
                        case 4: itemName = "Halberd"; break;
                        case 5: itemName = "Dagger"; break;
                        case 6: itemName = "Shortsword"; break;
                        case 7: itemName = "Longsword"; break;
                        case 8: itemName = "Rapier"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, DefaultWeaponDurability - durabilityDamageRoll);
                    break;
                case 45: treasure = EquipmentService.GetAmmoByNameSetQuantity(_gameData, "Superior Sling Stone", RandomHelper.GetRandomNumber(1, 10)); break;
                case 46: treasure = AlchemyService.GetPotionByStrength(PotionStrength.Supreme); break;
                case 47:
                    roll = RandomHelper.GetRandomNumber(1, 4);
                    switch (roll)
                    {
                        case 1: itemName = "Shortsword"; break;
                        case 2: itemName = "Broadsword"; break;
                        case 3: itemName = "Longsword"; break;
                        case 4: itemName = "Greatsword"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1));
                    break;
                case 48: treasure = EquipmentService.GetEquipmentByName(_gameData, "Tobacco"); break;
                case 49: treasure = EquipmentService.GetEquipmentByName(_gameData, "Trap Disarming Kit"); break;
                case 50:
                case 51:
                case 52: treasure = GetWonderfulTreasure(); break;
                case 53: treasure = GetRandomWizardStaff(DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)); break;
                case 54: treasure = EquipmentService.GetEquipmentByName(_gameData, "Dwarven Ale"); break;
                default:
                    treasure = CreateItem("Unknown Fine Item");
                    break;
            }
            return treasure;
        }

        public Equipment GetWonderfulTreasure()
        {
            string itemName = "";
            int roll = RandomHelper.GetRandomNumber(1, 54);
            int defaultDurabilityDamageRoll = RandomHelper.GetRandomNumber(1, 3) - 1;
            int armourDurability = DefaultArmourDurability - defaultDurabilityDamageRoll;
            int weaponDurability = DefaultWeaponDurability - defaultDurabilityDamageRoll;
            // Console.WriteLine($"Treasure roll {roll}");

            Equipment treasure;

            switch (roll)
            {
                case 1: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Aim Attachment", RandomHelper.GetRandomNumber(1, 3)); break;
                case 2: treasure = CreateItem("Talent Training Manual"); break;
                case 3: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Combat Harness", 6 - defaultDurabilityDamageRoll); break;
                case 4:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1:
                        case 2:
                        case 3: itemName = "Dragon Scale Cap"; break;
                        case 4: itemName = "Dragon Scale Breastplate"; break;
                        case 5: itemName = "Dragon Scale Pants"; break;
                        case 6: itemName = "Dragon Scale Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, (10 - defaultDurabilityDamageRoll));
                    break;
                case 5: treasure = EquipmentService.GetEquipmentByNameSetQuantity(_gameData, "Superior Lock Picks", RandomHelper.GetRandomNumber(1, 6)); break;
                case 6: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Dwarven Pickaxe", (6 - defaultDurabilityDamageRoll)); break;
                case 7: treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Elven Bow", DefaultWeaponDurability); break;
                case 8: treasure = CreateItem("Elven Bowstring"); break;
                case 9: treasure = AlchemyService.GetPotionByName("Potion of Restoration"); break;
                case 10: treasure = CreateItem("Relic - Epic"); break;
                case 11: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity(_gameData, "Extended Battle Belt", 6); break;
                case 12: treasure = CreateItem("Set of Fine Clothes"); break;
                case 13: treasure = CreateItem("Flute"); break;
                case 14:
                case 15: treasure = CreateItem("Gemstone", 0, 100, RandomHelper.GetRandomNumber(1, 10)); break;
                case 16: treasure = CreateItem("Grimoire"); break;
                case 17: treasure = CreateItem("Harp"); break;
                case 18: treasure = CreateItem("Huge Backpack"); break;
                case 19: treasure = CreateItem("Ingredient - Exquisite", 0, 0, 1, GetAlchemicalTreasure("Ingredient", RandomHelper.GetRandomNumber(1, 3))); break;
                case 20:
                case 21: treasure = CreateItem("Legendary"); break;
                case 22:
                    string[] itemArray = GetMagicItem("Item");
                    treasure = CreateItem("Amulet", 0, 700, 1, itemArray[1]);
                    treasure.Name = "Magic amulet of " + itemArray[0];
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.Description += " Cursed: " + itemArray[2];
                    }
                    break;
                case 23:
                    itemArray = GetMagicItem("Armour");
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, "Cloak", armourDurability);
                    treasure.Name = "Magic Cloak of " + itemArray[0];
                    treasure.Value = 300;
                    treasure.MagicEffect += itemArray[1];
                    ((Armour)treasure).Properties.Add(ArmourProperty.Magic, 0);
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.Description += " Cursed: " + itemArray[2];
                    }
                    break;
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                    itemArray = GetMagicItem("Armour");
                    roll = RandomHelper.GetRandomNumber(1, 25);
                    int value = 0;
                    switch (roll)
                    {
                        case 1: itemName = "Leather Cap"; value = 150; break;
                        case 2:
                        case 3: itemName = "Leather Vest"; value = 240; break;
                        case 4: itemName = "Leather Jacket"; value = 420; break;
                        case 5: itemName = "Leather Leggings"; value = 360; break;
                        case 6: itemName = "Leather Bracers"; value = 360; break;
                        case 7: itemName = "Mail Coif"; value = 600; break;
                        case 8:
                        case 9: itemName = "Mail Shirt"; value = 1800; break;
                        case 10: itemName = "Mail Coat"; value = 2250; break;
                        case 11: itemName = "Sleeved Mail Coat"; value = 3900; break;
                        case 12: itemName = "Mail Leggings"; value = 600; break;
                        case 13: itemName = "Padded Cap"; value = 90; break;
                        case 14:
                        case 15: itemName = "Padded Vest"; value = 180; break;
                        case 16: itemName = "Padded Jacket"; value = 360; break;
                        case 17: itemName = "Padded Pants"; value = 300; break;
                        case 18: itemName = "Padded Coat"; value = 150; break;
                        case 19: itemName = "Plate Helmet"; value = 900; break;
                        case 20: itemName = "Breastplate"; value = 2100; break;
                        case 21: itemName = "Plate Leggings"; value = 1800; break;
                        case 22: itemName = "Plate Bracers"; value = 2100; break;
                        case 23: itemName = "Buckler"; value = 120; break;
                        case 24: itemName = "Heater Shield"; value = 300; break;
                        case 25: itemName = "Tower Shield"; value = 600; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, armourDurability);
                    treasure.Name = "Magic " + itemName + " of " + itemArray[0];
                    treasure.Value = value;
                    treasure.MagicEffect = itemArray[1];
                    ((Armour)treasure).Properties.Add(ArmourProperty.Magic, 0); ;
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.MagicEffect += " Cursed: " + itemArray[2];
                    }
                    break;
                case 29:
                    itemArray = GetMagicItem("Item");
                    treasure = EquipmentService.GetEquipmentByName(_gameData, "Ring");
                    treasure.Value = 700;
                    treasure.Name = "Magic ring of " + itemArray[0];
                    treasure.Description = itemArray[1];
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.Description += " Cursed: " + itemArray[2];
                    }
                    break;
                case 30:
                case 31:
                case 32:
                case 33:
                    itemArray = GetMagicItem("Weapon");
                    roll = RandomHelper.GetRandomNumber(1, 15);
                    value = 0;
                    switch (roll)
                    {
                        case 1: itemName = "Staff"; value = 120; break;
                        case 2: itemName = "Javelin"; value = 300; break;
                        case 3: itemName = "Shortbow"; value = 600; break;
                        case 4: itemName = "Greatsword"; value = 600; break;
                        case 5: itemName = "Greataxe"; value = 600; break;
                        case 6: itemName = "Flail"; value = 450; break;
                        case 7: itemName = "Halberd"; value = 450; break;
                        case 8: itemName = "Battleaxe"; value = 300; break;
                        case 9: itemName = "Dagger"; value = 100; break;
                        case 10: itemName = "Shortsword"; value = 210; break;
                        case 11: itemName = "Rapier"; value = 390; break;
                        case 12: itemName = "Crossbow"; value = 750; break;
                        case 13: itemName = "Crossbow Pistol"; value = 1050; break;
                        case 14: itemName = "Elven Bow"; value = 1500; break;
                        case 15: itemName = "Longbow"; value = 300; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, armourDurability);
                    treasure.Name = "Magic " + itemName + " of " + itemArray[0];
                    treasure.MagicEffect = itemArray[1];
                    treasure.Value = value;
                    ((Weapon)treasure).Properties.Add(WeaponProperty.Magic, 0); ;
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.MagicEffect += " Cursed: " + itemArray[2];
                    }
                    break;
                case 34:
                case 35:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1: itemName = "Mail Coif"; break;
                        case 2: itemName = "Mail Shirt"; break;
                        case 3: itemName = "Sleeved Mail Shirt"; break;
                        case 4: itemName = "Mail Coat"; break;
                        case 5: itemName = "Mail Leggings"; break;
                        case 6: itemName = "Sleeved Mail Coat"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, armourDurability);
                    break;
                case 36:
                case 37:
                case 38:
                    roll = RandomHelper.GetRandomNumber(1, 10);
                    switch (roll)
                    {
                        case 1: itemName = "Mithril Mail Coif"; break;
                        case 2: itemName = "Mithril Mail Shirt"; break;
                        case 3: itemName = "Mithril Sleeved Mail Shirt"; break;
                        case 4: itemName = "Mithril Mail Coat"; break;
                        case 5: itemName = "Mithril Mail Leggings"; break;
                        case 6: itemName = "Mithril Sleeved Mail Coat"; break;
                        case 7: itemName = "Mithril Plate Helmet"; break;
                        case 8: itemName = "Mithril Breastplate"; break;
                        case 9: itemName = "Mithril Plate Leggings"; break;
                        case 10: itemName = "Mithril Plate Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, armourDurability);
                    break;
                case 39:
                    treasure = EquipmentService.GetShieldByNameSetDurability(_gameData, "Mithril Heater Shield", armourDurability);
                    break;
                case 40:
                case 41:
                case 42:
                    roll = RandomHelper.GetRandomNumber(1, 12);
                    switch (roll)
                    {
                        case 1: itemName = "Mithril Longsword"; break;
                        case 2: itemName = "Mithril Warhammer"; break;
                        case 3: itemName = "Mithril Battle Hammer"; break;
                        case 4: itemName = "Mithril Morning Star"; break;
                        case 5: itemName = "Mithril Battleaxe"; break;
                        case 6: itemName = "Mithril Dagger"; break;
                        case 7: itemName = "Mithril Shortsword"; break;
                        case 8: itemName = "Mithril Rapier"; break;
                        case 9: itemName = "Mithril Greatsword"; break;
                        case 10: itemName = "Mithril Greataxe"; break;
                        case 11: itemName = "Mithril Flail"; break;
                        case 12: itemName = "Mithril Halberd"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, weaponDurability);
                    break;
                case 43:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1: itemName = "Night Stalker Cap"; break;
                        case 2:
                        case 3: itemName = "Night Stalker Vest"; break;
                        case 4: itemName = "Night Stalker Jacket"; break;
                        case 5: itemName = "Night Stalker Pants"; break;
                        case 6: itemName = "Night Stalker Bracers"; break;
                        default:
                            break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, (8 - defaultDurabilityDamageRoll));
                    break;
                case 44: treasure = CreateItem("Part - Exquisite", 0, 0, 1, GetAlchemicalTreasure("Part", RandomHelper.GetRandomNumber(1, 3))); break;
                case 45:
                    roll = RandomHelper.GetRandomNumber(1, 4);
                    switch (roll)
                    {
                        case 1: itemName = "Plate Helmet"; break;
                        case 2: itemName = "Breastplate"; break;
                        case 3: itemName = "Plate Leggings"; break;
                        case 4: itemName = "Plate Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, itemName, armourDurability);
                    break;
                case 46:
                case 47: treasure = AlchemyService.GetPotionByStrength(PotionStrength.Supreme); break;
                case 48:
                case 49:
                case 50: treasure = CreateItem("Power Stone", 0, 1000, RandomHelper.GetRandomNumber(1, 3)); break;
                case 51:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity(_gameData, "Silver Arrow", RandomHelper.GetRandomNumber(1, 10));
                    break;
                case 52:
                    roll = RandomHelper.GetRandomNumber(1, 4);
                    switch (roll)
                    {
                        case 1: itemName = "Silver Greatsword"; break;
                        case 2: itemName = "Silver Greataxe"; break;
                        case 3: itemName = "Silver Flail"; break;
                        case 4: itemName = "Silver Halberd"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(_gameData, itemName, weaponDurability);
                    break;
                case 53: treasure = EquipmentService.GetShieldByNameSetDurability(_gameData, "Tower Shield", armourDurability); break;
                case 54: treasure = EquipmentService.GetArmourByNameSetDurability(_gameData, "Wyvern Cloak", DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 2) - 1)); break;
                default:
                    treasure = CreateItem("Unknown Wonderful Item");
                    break;
            }
            return treasure;
        }

        public string GetAlchemicalTreasure(string type, int amount, bool getOrigin = true)
        {
            List<string> items = new List<string>();
            if (type == "Ingredient")
            {
                foreach (AlchemyItem itemName in AlchemyService.GetIngredients(amount))
                {
                    items.Add(itemName.Name);
                }
            }
            else if (type == "Part")
            {
                foreach (AlchemyItem itemName in AlchemyService.GetParts(amount))
                {
                    items.Add(itemName.Name);
                }
            }

            return string.Join(", ", items);
        }

        public MagicStaff GetRandomWizardStaff(int durability)
        {
            int roll = RandomHelper.GetRandomNumber(0, _gameData.MagicStaves.Count - 1); // Adjust for 0-indexed list
            MagicStaff newStaff = _gameData.MagicStaves[roll];
            newStaff.Durability = durability; // Set the specific durability for this instance

            return newStaff;
        }

        public string[] GetRelic(string type = "Standard") // as it refers to fixed data
        {
            string[] relic = new string[2];
            int roll = RandomHelper.GetRandomNumber(1, 6); // Use RandomHelper
            switch (type)
            {
                case "Standard":
                    switch (roll)
                    {
                        case 1: relic[0] = "Relic of Charus"; relic[1] = "Grants +1 energy point"; break;
                        case 2: relic[0] = "Relic of Metheia"; relic[1] = "Adds +1d3 to any form of healing done by the priest"; break;
                        case 3: relic[0] = "Relic of Iphy"; relic[1] = "Grants +5 RES"; break;
                        case 4: relic[0] = "Relic of Rhidnir"; relic[1] = "Grants +1 luck during each quest"; break;
                        case 5: relic[0] = "Relic of Ohlnir"; relic[1] = "Grants +5 STR"; break;
                        case 6: relic[0] = "Relic of Ramos"; relic[1] = "Grants +5 CS"; break;
                        default: break; // Should not happen with valid roll range
                    }
                    break;
                case "Epic":
                    switch (roll)
                    {
                        case 1: relic[0] = "Epic Relic of Charus"; relic[1] = "Grants +2 energy point"; break;
                        case 2: relic[0] = "Epic Relic of Metheia"; relic[1] = "Adds +1d6 to any form of healing done by the priest"; break;
                        case 3: relic[0] = "Epic Relic of Iphy"; relic[1] = "Grants +7 RES"; break;
                        case 4: relic[0] = "Epic Relic of Rhidnir"; relic[1] = "Grants +2 luck during each quest"; break;
                        case 5: relic[0] = "Epic Relic of Ohlnir"; relic[1] = "Grants +7 STR"; break;
                        case 6: relic[0] = "Epic Relic of Ramos"; relic[1] = "Grants +7 CS"; break;
                        default: break; // Should not happen with valid roll range
                    }
                    break;
                default:
                    relic[0] = "Unknown Relic"; relic[1] = "No description found."; // Fallback for unknown type
                    break;
            }
            return relic;
        }

        public string[] GetPowerStone() // as it refers to fixed data
        {
            string[] stone = new string[2];
            int roll = RandomHelper.GetRandomNumber(1, 20);
            switch (roll)
            {
                case 1: stone[0] = "DMG +2"; stone[1] = "This stone can be used to improve any weapon."; break;
                case 2: stone[0] = "DMG +1"; stone[1] = "This stone can be used to improve any weapon."; break;
                case 3: stone[0] = "Poisonous"; stone[1] = "This stone gives any weapon a permanent poison ability. If a monster is wounded by this weapon it will lose 1HP every turn until the end of the battle."; break;
                case 4: stone[0] = "Fire Damage"; stone[1] = "This stone makes the weapon cause fire damage. Follows the fire damage rules"; break;
                case 5: stone[0] = "ToHit +10"; stone[1] = "This stone will make a weapon perfectly balanced, increasing chance to hit."; break;
                case 6: stone[0] = "Strength +5"; stone[1] = "This stone can be used on rings or amulets, enhancing the bearers stat."; break;
                case 7: stone[0] = "Constitution +5"; stone[1] = "This stone can be used on rings or amulets, enhancing the bearers stat."; break;
                case 8: stone[0] = "Wisdom +5"; stone[1] = "This stone can be used on rings or amulets, enhancing the bearers stat."; break;
                case 9: stone[0] = "Resolve +5"; stone[1] = "This stone can be used on rings or amulets, enhancing the bearers stat."; break;
                case 10: stone[0] = "Dexterity +5"; stone[1] = "This stone can be used on rings or amulets, enhancing the bearers stat."; break;
                case 11: stone[0] = "Fast Reload"; stone[1] = "Reduces reload time by 1 AP. If reload is 0 AP then the hero may only attack twice the first action, but only once the second action."; break;
                case 12: stone[0] = "Def +2"; stone[1] = "This stone can be used to enhance a piece of armour or a shield."; break;
                case 13: stone[0] = "Energy +1 per quest"; stone[1] = "This stone can be used on rings or amulets, increasing the bearers energy by 1 for each quest."; break;
                case 14: stone[0] = "Luck +1"; stone[1] = "This stone can be used on rings or amulets, increasing the bearers luck by 1."; break;
                case 15: stone[0] = "Detect trap +10"; stone[1] = "This stone can be used on rings or amulets, enhancing the chance to detect traps when prompted."; break;
                case 16: stone[0] = "Surprise +5 on open door"; stone[1] = "This stone can be used on rings or amulets. Subtract 5 from the enemies DEX."; break;
                case 17: stone[0] = "Reroll Fear/Terror test"; stone[1] = "This stone can be used on rings or amulets."; break;
                case 18: stone[0] = "HP +2"; stone[1] = "This stone can be used on rings or amulets, enhancing the bearers max HP."; break;
                case 19: stone[0] = "Party Morale +2"; stone[1] = "This stone can be used on rings or amulets, enhancing the party's starting morale on each quest."; break;
                case 20: stone[0] = "Sanity +2"; stone[1] = "This stone can be used on rings or amulets, enhancing the bearers stat."; break;
                default: stone[0] = "Unknown Power Stone"; stone[1] = "No description found."; break; // Should not happen
            }
            return stone;
        }

        public string[] GetMagicItem(string type)
        {
            string[] magic = new string[3];
            int roll = RandomHelper.GetRandomNumber(1, 10);
            if (roll == 10) // 10% chance to be cursed
            {
                roll = RandomHelper.GetRandomNumber(1, 10);
                switch (roll)
                {
                    case 1: magic[2] = "HP -2"; break;
                    case 2: magic[2] = "WIS -5"; break;
                    case 3: magic[2] = "CON -5"; break;
                    case 4: magic[2] = "STR -5"; break;
                    case 5: magic[2] = "DEX -5"; break;
                    case 6: magic[2] = "HP -3"; break;
                    case 7: magic[2] = "RES -10"; break;
                    case 8:
                        roll = RandomHelper.GetRandomNumber(1, 9);
                        switch (roll)
                        {
                            case 1: magic[2] = "CS -5"; break;
                            case 2: magic[2] = "RS -5"; break;
                            case 3: magic[2] = "Dodge -5"; break;
                            case 4: magic[2] = "Pick Locks -5"; break;
                            case 5: magic[2] = "Barter -5"; break;
                            case 6: magic[2] = "Heal -5"; break;
                            case 7: magic[2] = "Alchemy -5"; break;
                            case 8: magic[2] = "Perception -5"; break;
                            case 9: magic[2] = "Foraging -5"; break;
                            default: break; // Should not happen
                        }
                        break;
                    case 9: magic[2] = "Luck -1"; break;
                    case 10: magic[2] = "Energy -1"; break;
                    default: break; // Should not happen
                }
                roll = RandomHelper.GetRandomNumber(1, 9); // Re-roll for positive effect if cursed, to fill magic[0] and magic[1]
            }
            else
            {
                magic[2] = ""; // Not cursed
            }

            switch (type)
            {
                case "Weapon":
                    switch (roll)
                    {
                        case 1: magic[0] = "Fire DMG"; magic[1] = "This weapon has a slight glow to it, as if radiating heat."; break;
                        case 2: magic[0] = "DMG +2"; magic[1] = "When wielding this weapon, is is as if the hero can feel its taste for blood."; break;
                        case 3: magic[0] = "DMG +1"; magic[1] = "A slight humming sound can be heard from this weapon, as if it powered up and ready to kill."; break;
                        case 4: magic[0] = "CS/RS +10"; magic[1] = "This weapon feels as if it is one with your body, making every move as simple and perfect as they can be."; break;
                        case 5: magic[0] = "CS/RS +5"; magic[1] = "The magic infused into this weapon causes every strike or shot to be all but perfect."; break;
                        case 6: magic[0] = "ENC -5 and Class -1 (min 1)"; magic[1] = "The low weight of this weapon goes beyond your heroes' understanding. A weapon half the size would not even weigh this little."; break;
                        case 7: magic[0] = "DUR +2 (total 10)"; magic[1] = "It is as if the magic in this weapon is unusually strong, binding it together in a way that your  hero has never experienced before."; break;
                        case 8: magic[0] = "Fear/Terror test +10"; magic[1] = "One look at this weapon reassures your hero that nothing can stand between them and victory."; break;
                        case 9: magic[0] = "+5 Parry for CC, +5 Dodge for ranged weapon"; magic[1] = "Imbued with powerful spells, this weapon is designed to protect its owner."; break;
                        default: break; // Should not happen
                    }
                    break;
                case "Armour":
                    switch (roll)
                    {
                        case 1: magic[0] = "DEF +2"; magic[1] = "This piece of armor simply radiates strength."; break;
                        case 2: magic[0] = "DEF +1"; magic[1] = "This piece of armor can deflect any blow."; break;
                        case 3: magic[0] = "-50% ENC"; magic[1] = "This armour seems lighter than even Mithril."; break;
                        case 4: magic[0] = "DUR +1 (total 9)"; magic[1] = "Enchanted by a master enchanter, it will take tremendous effort to break this armor."; break;
                        case 5: magic[0] = "Fire Immunity"; magic[1] = "From time to time, itr almost looks as if this armour is on fire even though it is cool to the touch. Maybe this is the reason that it is not susceptible to fire."; break;
                        case 6: magic[0] = "CON +10 for poison roll tests"; magic[1] = "While forging this armour, powerful enchantments were imbued into it to thwart anyone trying to poison its wearer."; break;
                        case 7: magic[0] = "CON +5"; magic[1] = "The enchantment in this armour reaches out to its wearer and strengthens their resilience."; break;
                        case 8: magic[0] = "STR +5"; magic[1] = "As if the armour lends its strength, the wearer of this piece of armour becomes as strong as an ox"; break;
                        case 9: magic[0] = "Dodge +5"; magic[1] = "Imbued with powerful spells, the armour gives a tingling sensation to its wearer when danger is near, giving them time to dodge the threat."; break;
                        default: break; // Should not happen
                    }
                    break;
                case "Item":
                    switch (roll)
                    {
                        case 1: magic[0] = "HP +1"; magic[1] = "This item has been imbued by some of the life essence of the enchanter and lends it to the one wearing it."; break;
                        case 2: magic[0] = "CON +5"; magic[1] = "A spell of resilience has been cast upon this item, giving the wearer the power to resist damage and illness alike."; break;
                        case 3: magic[0] = "STR +5"; magic[1] = "Using this item grants the wearer the strength of several men."; break;
                        case 4: magic[0] = "RES +5"; magic[1] = "Bolstering the mind against the horrors, this item lends a bit of spine to its user."; break;
                        case 5: magic[0] = "DEX +5"; magic[1] = "This makes the user as nimble as a cat...almost."; break;
                        case 6: magic[0] = "Energy +1"; magic[1] = "TRhis is an extraordinary item for sure. Used as a vessel for energy, it gives the user the endurance to perform that which a normal man could not."; break;
                        case 7: magic[0] = "Luck +1"; magic[1] = "Clearly enchanted by a follower of Rhidnir, this item bestows a little bit of luck to its user."; break;
                        case 8:
                            roll = RandomHelper.GetRandomNumber(1, 9);
                            switch (roll)
                            {
                                case 1: magic[0] = "CS +5"; break;
                                case 2: magic[0] = "RS +5"; break;
                                case 3: magic[0] = "Dodge +5"; break;
                                case 4: magic[0] = "Pick Locks +5"; break;
                                case 5: magic[0] = "Barter +5"; break;
                                case 6: magic[0] = "Heal +5"; break;
                                case 7: magic[0] = "Alchemy +5"; break;
                                case 8: magic[0] = "Perception +5"; break;
                                case 9: magic[0] = "Foraging +5"; break;
                                default: break; // Should not happen
                            }
                            magic[1] = "Through this enchantment, the wizards who created this lends some of his skill to its user.";
                            break;
                        case 9: magic[0] = "Init +5 rolls / Add 1 hero token"; magic[1] = "A simple attempt to copy the Rings of Awareness, this item still has its benefits."; break;
                        default: break; // Should not happen
                    }
                    break;
                default:
                    magic[0] = "Unknown Magic Item"; magic[1] = "No description found."; // Fallback for unknown type
                    break;
            }
            return magic;
        }

        public string GetLegendary() // as it refers to fixed data
        {
            string item = "";
            int roll = RandomHelper.GetRandomNumber(1, 6);
            switch (roll)
            {
                case 1:
                case 2:
                    roll = RandomHelper.GetRandomNumber(1, 10);
                    switch (roll)
                    {
                        case 1: item = "Horn of Alfheim"; break;
                        case 2: item = "Bow of Divine Twilight"; break;
                        case 3: item = "Stone of Valheir"; break;
                        case 4: item = "Gauntlets of Hrafneir"; break;
                        case 5: item = "Belt of Oakenshield"; break;
                        case 6: item = "Vial of Never Ending"; break;
                        case 7: item = "Ring of the Heirophant"; break;
                        case 8: item = "Amulet of Haamile"; break;
                        case 9: item = "Boots of Stability"; break;
                        case 10: item = "Ring of Awareness"; break;
                        default: break; // Should not happen
                    }
                    break;
                case 3:
                case 4:
                    roll = RandomHelper.GetRandomNumber(1, 10);
                    switch (roll)
                    {
                        case 1: item = "Cloak of Elswhyr"; break;
                        case 2: item = "Priestly Dice"; break;
                        case 3: item = "Legendary Elixir"; break;
                        case 4: item = "Dagger of Vrunoir"; break;
                        case 5: item = "The Summoner's Staff"; break;
                        case 6: item = "The Headman's Axe"; break;
                        case 7: item = "Sword of Lightning"; break;
                        case 8: item = "Ohlnir's Hammer"; break;
                        case 9: item = "The Vampire's Brooch"; break;
                        case 10: item = "The Helmet of Golgorosh the Ram"; break;
                        default: break; // Should not happen
                    }
                    break;
                case 5:
                case 6:
                    roll = RandomHelper.GetRandomNumber(1, 10);
                    switch (roll)
                    {
                        case 1: item = "Teh Breastplate of Rannulf"; break;
                        case 2: item = "The Golden Kopesh"; break;
                        case 3: item = "Ring of Regeneration"; break;
                        case 4: item = "Crown of Resolve"; break;
                        case 5: item = "Boots of Energy"; break;
                        case 6: item = "The Goblin Scimitar"; break;
                        case 7: item = "The Halfling Backpack"; break;
                        case 8: item = "Trap-sensing Ring"; break;
                        case 9: item = "Necklace of Flight"; break;
                        case 10: item = "Necklace of Deflection"; break;
                        default: break; // Should not happen
                    }
                    break;
                default:
                    item = "Unknown Legendary Item"; // Fallback for unexpected roll
                    break;
            }
            return item;
        }

        public List<Equipment> GetArmourPieces(int count, int durabilityWear = 0)
        {
            List<Equipment> items = new List<Equipment>();
            if (durabilityWear == 0)
            {
                durabilityWear = RandomHelper.GetRandomNumber(1, 5);
            }
            for (int i = 0; i < count; i++)
            {
                items.Add(GetRandomArmour(DefaultArmourDurability - durabilityWear));
            }
            return items;
        }

        public Equipment GetRandomArmour(int durability = DefaultArmourDurability)
        {
            Equipment armour;
            int roll = RandomHelper.GetRandomNumber(1, 20);
            switch (roll)
            {
                case 1: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Padded Cap", durability); break;
                case 2: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Padded Vest", durability); break;
                case 3: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Padded Jacket", durability); break;
                case 4: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Padded Pants", durability); break;
                case 5: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Padded Coat", durability); break;
                case 6: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Leather Cap", durability); break;
                case 7: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Leather Vest", durability); break;
                case 8: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Leather Jacket", durability); break;
                case 9: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Leather Leggings", durability); break;
                case 10: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Leather Bracers", durability); break;
                case 11: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Mail Coif", durability); break;
                case 12: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Mail Shirt", durability); break;
                case 13: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Sleeved Mail Shirt", durability); break;
                case 14: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Mail Coat", durability); break;
                case 15: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Sleeved Mail Coat", durability); break;
                case 16: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Mail Leggings", durability); break;
                case 17: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Plate Helmet", durability); break;
                case 18: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Breastplate", durability); break;
                case 19: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Plate Bracers", durability); break;
                case 20: armour = EquipmentService.GetArmourByNameSetDurability(_gameData, "Plate Leggings", durability); break;
                default:
                    armour = CreateItem("Unknown Random Armour", durability); // Should not happen
                    break;
            }
            return armour;
        }

        public Equipment GetRandomWeapon(int durability)
        {
            Equipment weapon;
            int roll = RandomHelper.GetRandomNumber(1, 22);
            switch (roll)
            {
                case 1: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Dagger", durability); break;
                case 2: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Shortsword", durability); break;
                case 3: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Rapier", durability); break;
                case 4: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Broadsword", durability); break;
                case 5: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Longsword", durability); break;
                case 6: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Battleaxe", durability); break;
                case 7: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Battle Hammer", durability); break;
                case 8: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Morning Star", durability); break;
                case 9: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Flail", durability); break;
                case 10: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Staff", durability); break;
                case 11: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Javelin", durability); break;
                case 12: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Greatsword", durability); break;
                case 13: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Greataxe", durability); break;
                case 14: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Warhammer", durability); break;
                case 15: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Halberd", durability); break;
                case 16: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Shortbow", durability); break;
                case 17: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Longbow", durability); break;
                case 18: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Elven Bow", durability); break;
                case 19: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Crossbow Pistol", durability); break;
                case 20: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Crossbow", durability); break;
                case 21: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Arbalest", durability); break;
                case 22: weapon = EquipmentService.GetWeaponByNameSetDurability(_gameData, "Sling", durability); break;
                default:
                    weapon = CreateItem("Unknown Random Weapon", durability); // Should not happen
                    break;
            }
            return weapon;
        }

        /// <summary>
        /// Factory method to create a specific _Equipment item by name.
        /// </summary>
        /// <param name="itemName">The name of the item to create.</param>
        /// <param name="durability">The current durability of the item. If 0, MaxDurability will be used.</param>
        /// <param name="value">The value of the item.</param>
        /// <param name="quantity">The quantity of the item.</param>
        /// <param name="itemDescription">An optional description for the item.</param>
        /// <returns>A newly created _Equipment object of the appropriate derived type.</returns>
        public Equipment CreateItem(string itemName, int durability = 0, int value = 0, int quantity = 0, string itemDescription = "")
        {
            Equipment newItem;

            switch (itemName)
            {
                case "Legendary":
                    newItem = new Equipment()
                    {
                        Name = "Legendary item: " + GetLegendary(),
                        Encumbrance = 0,
                        Durability = 0, // Legendary items might not have durability
                        Value = 0,
                        Description = "" // Legendary items often have their description from the name
                    };
                    break;
                case "Relic":
                case "Relic - Epic":
                    string[] relicData = itemName == "Relic" ? GetRelic() : GetRelic("Epic");
                    newItem = new Equipment
                    {
                        Name = relicData[0],
                        Encumbrance = 0,
                        Durability = 0,
                        Value = itemName == "Relic" ? 350 : 450,
                        Description = relicData[1]
                    };
                    break;
                case "Coin":
                    newItem = new Equipment { Name = "Coin", Encumbrance = 0, Durability = 0, Value = 1 };
                    break;
                case "Potion Recipe - Weak":
                    newItem = new Equipment { Name = $"Weak Potion Recipe: {AlchemyService.GetNonStandardPotion()}", Encumbrance = 0, Durability = 0, Value = 0, Description = "The actual components involved shall be chosen by the player", MaxDurability = 0 };
                    break;
                case "Ingredient":
                    newItem = new Ingredient { Name = "Ingredient", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Part":
                    newItem = new Part { Name = "Part", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Ingredient - Exquisite":
                    newItem = new Ingredient { Name = "Ingredient - Exquisite", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Part - Exquisite":
                    newItem = new Part { Name = "Part - Exquisite", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Amulet":
                    newItem = new Equipment { Name = "Amulet", Encumbrance = 0, Durability = 0, Value = 100, Description = "Can be enchanted", MaxDurability = 0 };
                    break;
                case "Backpack - Huge":
                    newItem = new Equipment { Name = "Backpack - Huge", Encumbrance = 1, Durability = 0, Value = 850, Description = "Increase ENC +35, while decreasing DEX -20", MaxDurability = 0 };
                    break;
                case "Elven Bowstring":
                    newItem = new Equipment { Name = "Elven Bowstring", Encumbrance = 0, Durability = 0, Value = 0, Description = "After adding to any bow during a rest, it adds RS +5 to the weapon", MaxDurability = 0 };
                    break;
                case "Flute":
                    newItem = new Equipment { Name = "Flute", Encumbrance = 1, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, to lower the threat level by 1d3.", MaxDurability = 0 };
                    break;
                case "Gemstone":
                    newItem = new Equipment { Name = "Gemstone", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 }; // Value will be set by the calling method
                    break;
                case "Grimoire":
                    newItem = new Equipment { Name = $"Grimoire of {new GameDataService().GetRandomSpellName()}", Encumbrance = 1, Durability = 0, Value = 0, Description = "This spell can be learned back at the Wizards' Guild as long as you have the proper level", MaxDurability = 0 };
                    break;
                case "Harp":
                    newItem = new Equipment { Name = "Harp", Encumbrance = 2, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, all heroes regain an extra 1d3HP.", MaxDurability = 0 };
                    break;
                case "Lute":
                    newItem = new Equipment { Name = "Lute", Encumbrance = 5, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, to recover 1 sanity for all heroes.", MaxDurability = 0 };
                    break;
                case "Power Stone":
                    string[] stoneData = GetPowerStone();
                    newItem = new Equipment { Name = stoneData[0], Encumbrance = 0, Durability = 0, Value = 1000, Description = stoneData[1], MaxDurability = 0 };
                    break;
                case "Set of Fine Clothes":
                    newItem = new Equipment { Name = "Set of Fine Clothes", Encumbrance = 0, Durability = 0, Value = 0, Description = "Increases Barter +5", MaxDurability = 0 };
                    break;
                case "Scroll":
                    newItem = new Equipment { Name = $"Scroll of {new GameDataService().GetRandomSpellName()}", Encumbrance = 0, Durability = 0, Value = 100, MaxDurability = 0 };
                    break;
                case "Talent Training Manual":
                    newItem = new Equipment { Name = $"{new GameDataService().GetRandomTalent()} Training Manual", Encumbrance = 1, Durability = 0, Value = 0, Description = "Grants the talent named on the book, when read at an inn", MaxDurability = 0 };
                    break;
                case "Wolf Pelt":
                    newItem = new Equipment { Name = "Wolf Pelt", Encumbrance = 2, Durability = 0, Value = 50, MaxDurability = 0 };
                    break;
                default:
                    newItem = new Equipment { Name = itemName }; // Fallback for unknown items
                    // TODO: Log an error or handle unknown item names appropriately
                    break;
            }

            // Apply override values if provided
            if (!string.IsNullOrEmpty(itemDescription))
            {
                newItem.Description = itemDescription;
            }

            // Set quantity
            if (quantity > 0)
            {
                newItem.Quantity = quantity;
            }
            else if (newItem.Quantity < 1 && newItem.MaxDurability == 0) // For items without MaxDurability, ensure quantity is at least 1
            {
                newItem.Quantity = 1;
            }
            else if (newItem.Quantity < 1 && newItem.MaxDurability > 0) // For items with MaxDurability, default quantity to 1 if not specified
            {
                newItem.Quantity = 1;
            }


            // Set durability (if applicable and within bounds)
            if (durability > 0 && durability <= newItem.MaxDurability)
            {
                newItem.Durability = durability;
            }
            else if (newItem.MaxDurability > 0) // If durability not specified, set to max if item has durability
            {
                newItem.Durability = newItem.MaxDurability;
            }


            // Set value override
            if (value > 0)
            {
                newItem.Value = value;
            }

            return newItem;
        }
    }
}