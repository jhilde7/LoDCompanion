using LoDCompanion.Services.GameData;
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

        public List<MagicStaff> _wizardStaves = new List<MagicStaff>
            {
                new MagicStaff() {Name = "Basic Staff", Encumbrance = 5, Value = 30, WeaponClass = 2, DamageRange = [1, 8], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability },
                new MagicStaff() {Name = "Fire Staff", Encumbrance = 5, Value = 150, WeaponClass = 2, DamageRange = [2, 10], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "Deals fire damage." },
                new MagicStaff() {Name = "Ice Staff", Encumbrance = 5, Value = 150, WeaponClass = 2, DamageRange = [2, 10], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "Deals ice damage." },
                new MagicStaff() {Name = "Lightning Staff", Encumbrance = 5, Value = 150, WeaponClass = 2, DamageRange = [2, 10], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "Deals lightning damage." },
                new MagicStaff() {Name = "Dark Staff", Encumbrance = 5, Value = 150, WeaponClass = 2, DamageRange = [2, 10], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "Deals dark damage." },
                new MagicStaff() {Name = "Holy Staff", Encumbrance = 5, Value = 150, WeaponClass = 2, DamageRange = [2, 10], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "Deals holy damage." },
                new MagicStaff() {Name = "Nature Staff", Encumbrance = 5, Value = 150, WeaponClass = 2, DamageRange = [2, 10], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "Deals nature damage." },
                new MagicStaff() {Name = "Arcane Staff", Encumbrance = 5, Value = 200, WeaponClass = 2, DamageRange = [3, 12], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "Deals arcane damage." },
                new MagicStaff() {Name = "Elder Staff", Encumbrance = 5, Value = 250, WeaponClass = 2, DamageRange = [4, 14], IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability, Description = "A staff of ancient power." }
            };

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
                            rewards.Add(GetTreasure("Bandage (Old Rag)", 1));
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
                            foreach (string item in AlchemyService.GetPotionNames(RandomHelper.GetRandomNumber(1, 2), "Any"))
                            {
                                rewards.Add(item);
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
                        // TODO: Log or throw an error for unsupported treasure type
                        break;
                }
            }
            return rewards;
        }

        public Equipment GetMundaneTreasure()
        {
            int roll = RandomHelper.GetRandomNumber(1, 54);
            // Console.WriteLine($"Treasure roll {roll}"); // For debugging, replace with proper logging

            string itemName = "";
            Equipment treasure;

            switch (roll)
            {
                case 1: treasure = CreateItem("Amulet"); break;
                case 2: treasure = CreateItem("Arrow", 0, 0, 5); break;
                case 3: treasure = CreateItem("Arrow", 0, 0, 10); break;
                case 4:
                case 5: treasure = CreateItem("Bandage (Old Rag)", 0, 0, RandomHelper.GetRandomNumber(1, 3)); break;
                case 6: treasure = CreateItem("Bandage (Linen)", 0, 0, RandomHelper.GetRandomNumber(1, 2)); break;
                case 7: treasure = CreateItem("Bear Trap", (2 - (RandomHelper.GetRandomNumber(1, 2) - 1)), 200, RandomHelper.GetRandomNumber(1, 3)); break;
                case 8:
                case 9: treasure = CreateItem("Bedroll", 0, 140, RandomHelper.GetRandomNumber(1, 3)); break;
                case 10: treasure = CreateItem("Beef Jerky", 0, 0, RandomHelper.GetRandomNumber(1, 4)); break;
                case 11: treasure = CreateItem("Bolt", 0, 0, 5); break;
                case 12: treasure = CreateItem("Buckler", (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 13:
                case 14: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(1, 20)); break;
                case 15: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(2, 40)); break;
                case 16:
                case 17: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(3, 60)); break;
                case 18: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(4, 80)); break;
                case 19: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(1, 100)); break;
                case 20: treasure = CreateItem("Cloak", (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 21: treasure = CreateItem("Crowbar", (6 - (RandomHelper.GetRandomNumber(1, 4) + 1)), 55, RandomHelper.GetRandomNumber(1, 6)); break;
                case 22: treasure = CreateItem("Dagger", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 23: treasure = CreateItem("Empty Bottle", 0, 15, RandomHelper.GetRandomNumber(1, 6)); break;
                case 24: treasure = GetFineTreasure(); break;
                case 25: treasure = CreateItem("Ingredient", 0, 0, 1, GetAlchemicalTreasure("Ingredient", RandomHelper.GetRandomNumber(1, 3))); break;
                case 26: treasure = CreateItem("Javelin", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 27: treasure = CreateItem("Lantern", 0, 100, RandomHelper.GetRandomNumber(1, 3)); break;
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
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4) + 1)));
                    break;
                case 29: treasure = CreateItem("Lock Pick", 1, 0, RandomHelper.GetRandomNumber(1, 4)); break;
                case 30: treasure = CreateItem("Backpack - Medium"); break;
                case 31: treasure = CreateItem("Rope (Old)"); break;
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
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4) + 1)));
                    break;
                case 33: treasure = CreateItem("Part", 0, 0, 1, GetAlchemicalTreasure("Ingredient", RandomHelper.GetRandomNumber(1, 3))); break;
                case 34: treasure = CreateItem("Part", 0, 0, 1, GetAlchemicalTreasure("Part", 1)); break;
                case 35: treasure = CreateItem("Rapier", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 36: treasure = CreateItem("Ration", 1, 5, RandomHelper.GetRandomNumber(1, 4)); break;
                case 37: treasure = CreateItem("Ration", 1, 5, RandomHelper.GetRandomNumber(1, 6)); break;
                case 38: treasure = CreateItem("Potion Recipe - Weak"); break;
                case 39:
                case 40: treasure = CreateItem("Ring"); break;
                case 41: treasure = CreateItem("Rope"); break;
                case 42: treasure = CreateItem("Shortbow", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 43: treasure = CreateItem("Shortsword", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 44: treasure = CreateItem("Skinning Knife", 0, 100, RandomHelper.GetRandomNumber(1, 3)); break;
                case 45: treasure = CreateItem("Sling", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 46: treasure = CreateItem("Staff", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) + 1))); break;
                case 47:
                case 48:
                case 49: treasure = CreateItem("Torch"); break;
                case 50: treasure = CreateItem("Whetstone"); break;
                case 51: treasure = CreateItem("Wild Game Traps"); break;
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
            string silverDescription = "Can hurt ethereal creatures. Dmg +1 against undead.";
            string itemName = "";
            int value = 0;
            int roll = RandomHelper.GetRandomNumber(1, 54);
            // Console.WriteLine($"Treasure roll {roll}");

            Equipment treasure;

            switch (roll)
            {
                case 1: treasure = CreateItem("Alchemist Tools", (6 - (RandomHelper.GetRandomNumber(1, 4)))); break;
                case 2: treasure = CreateItem("Alchemist Belt", (6 - (RandomHelper.GetRandomNumber(1, 4)))); break;
                case 3: treasure = CreateItem("Armour Repair Kit"); break;
                case 4:
                    roll = RandomHelper.GetRandomNumber(1, 3);
                    switch (roll)
                    {
                        case 1: itemName = "Battleaxe"; break;
                        case 2: itemName = "Greataxe"; break;
                        case 3: itemName = "Halberd"; break;
                    }
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 5:
                    treasure = CreateItem("Arrow", 0, 25, 5);
                    treasure.Name = "Barbed Arrow";
                    if (treasure is Ammo ammoItem) { ammoItem.IsBarbed = true; } // Direct cast instead of GetComponent
                    break;
                case 6:
                    treasure = CreateItem("Bolt", 0, 25, 5);
                    treasure.Name = "Barbed Bolt";
                    if (treasure is Ammo ammoItem2) { ammoItem2.IsBarbed = true; }
                    break;
                case 7: treasure = CreateItem("Bedroll"); break;
                case 8:
                    roll = RandomHelper.GetRandomNumber(1, 2);
                    switch (roll)
                    {
                        case 1: itemName = "Morning Star"; break;
                        case 2: itemName = "Flail"; break;
                    }
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 9: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(1, 100) + 40); break;
                case 10: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(2, 200) + 20); break;
                case 11: treasure = CreateItem("Coin", 0, 1, RandomHelper.GetRandomNumber(3, 300)); break;
                case 12: treasure = CreateItem("Door Mirror"); break;
                case 13: treasure = CreateItem("Lock Picks - Dwarven", 1, 0, RandomHelper.GetRandomNumber(1, 6)); break;
                case 14: treasure = CreateItem("Elven Bow", (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 2)))); break;
                case 15: treasure = CreateItem("Skinning Knife - Elven"); break;
                case 16: treasure = CreateItem("Ingredient - Exquisite", 0, 0, 1, GetAlchemicalTreasure("Ingredient", 1)); break;
                case 17: treasure = CreateItem("Extended Battle Belt", (6 - (RandomHelper.GetRandomNumber(1, 4)))); break;
                case 18: treasure = CreateItem("Fishing Gear"); break;
                case 19: treasure = CreateItem("Gemstone", 0, RandomHelper.GetRandomNumber(3, 300)); break;
                case 20: treasure = CreateItem("Gemstone", 0, 100, RandomHelper.GetRandomNumber(1, 6)); break;
                case 21:
                    roll = RandomHelper.GetRandomNumber(1, 2);
                    switch (roll)
                    {
                        case 1: itemName = "Battle Hammer"; break;
                        case 2: itemName = "Warhammer"; break;
                    }
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 22: treasure = CreateItem("Heater Shield", (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4)))); break;
                case 23: treasure = CreateItem("Iron Wedge", 6, 50, (RandomHelper.GetRandomNumber(1, 3))); break;
                case 24: treasure = CreateItem("Backpack - Large"); break;
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
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4))));
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
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 4))));
                    break;
                case 30: treasure = CreateItem("Necklace", 0, RandomHelper.GetRandomNumber(3, 300)); break;
                case 31: treasure = CreateItem("Part - Exquisite", 0, 0, 1, GetAlchemicalTreasure("Part", 1)); break;
                case 32: treasure = CreateItem("Partial Map"); break;
                case 33:
                case 34: treasure = CreateItem("Potion - Standard"); break;
                case 35: treasure = CreateItem("Potion of Health"); break;
                case 36:
                    roll = RandomHelper.GetRandomNumber(1, 4);
                    switch (roll)
                    {
                        case 1: itemName = "Crossbow"; break;
                        case 2: itemName = "Longbow"; break;
                        case 3: itemName = "Crossbow Pistol"; break;
                        case 4: itemName = "Arbalest"; break;
                    }
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 37: treasure = CreateItem("Relic"); break;
                case 38: treasure = CreateItem("Ring"); break;
                case 39: treasure = CreateItem("Scroll"); break;
                case 40: treasure = CreateItem("Scroll", 0, 100, RandomHelper.GetRandomNumber(1, 3)); break;
                case 41:
                    treasure = CreateItem("Arrow", 0, 0, RandomHelper.GetRandomNumber(1, 10));
                    treasure.Name = "Silver Arrows";
                    if (treasure is Ammo silverAmmo) { silverAmmo.IsSilver = true; } // Direct cast instead of GetComponent
                    treasure.Description = silverDescription;
                    break;
                case 42:
                    treasure = CreateItem("Bolt", 0, 0, RandomHelper.GetRandomNumber(1, 10));
                    treasure.Name = "Silver Bolts";
                    if (treasure is Ammo silverBolt) { silverBolt.IsSilver = true; }
                    treasure.Description = silverDescription;
                    break;
                case 43:
                case 44:
                    roll = RandomHelper.GetRandomNumber(1, 8);
                    switch (roll)
                    {
                        case 1: itemName = "Greatsword"; value = 300; break;
                        case 2: itemName = "Greataxe"; value = 300; break;
                        case 3: itemName = "Flail"; value = 225; break;
                        case 4: itemName = "Halberd"; value = 225; break;
                        case 5: itemName = "Dagger"; value = 75; break;
                        case 6: itemName = "Shortsword"; value = 105; break;
                        case 7: itemName = "Longsword"; value = 150; break;
                        case 8: itemName = "Rapier"; value = 270; break;
                    }
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)), value);
                    if (treasure is MeleeWeapon heroWeapon) { heroWeapon.Name = "Silver " + itemName; heroWeapon.IsSilver = true; }
                    treasure.Description = silverDescription;
                    break;
                case 45: treasure = CreateItem("Superior Sling Stone", 0, 25, RandomHelper.GetRandomNumber(1, 10)); break;
                case 46: treasure = CreateItem("Potion - Supreme"); break;
                case 47:
                    roll = RandomHelper.GetRandomNumber(1, 4);
                    switch (roll)
                    {
                        case 1: itemName = "Shortsword"; break;
                        case 2: itemName = "Broadsword"; break;
                        case 3: itemName = "Longsword"; break;
                        case 4: itemName = "Greatsword"; break;
                    }
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)));
                    break;
                case 48: treasure = CreateItem("Tobacco"); break;
                case 49: treasure = CreateItem("Trap Disarming Kit"); break;
                case 50:
                case 51:
                case 52: treasure = GetWonderfulTreasure(); break;
                case 53: treasure = GetRandomWizardStaff((DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1))); break;
                case 54: treasure = CreateItem("Dwarven Ale"); break;
                default:
                    treasure = CreateItem("Unknown Fine Item");
                    break;
            }
            return treasure;
        }

        public Equipment GetWonderfulTreasure()
        {
            string silverDescription = "Can hurt ethereal creatures. Dmg +1 against undead.";
            string itemName = "";
            int value = 0;
            int roll = RandomHelper.GetRandomNumber(1, 54);
            // Console.WriteLine($"Treasure roll {roll}");

            Equipment treasure;

            switch (roll)
            {
                case 1: treasure = CreateItem("Aim Attachment", 0, 200, RandomHelper.GetRandomNumber(1, 3)); break;
                case 2: treasure = CreateItem("Talent Training Manual"); break;
                case 3: treasure = CreateItem("Combat Harness", 6 - (RandomHelper.GetRandomNumber(1, 3) - 1)); break;
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
                    treasure = CreateItem(itemName, (10 - (RandomHelper.GetRandomNumber(1, 3) - 1)));
                    break;
                case 5: treasure = CreateItem("Lock Pick - Dwarven", 0, 0, RandomHelper.GetRandomNumber(1, 6)); break;
                case 6: treasure = CreateItem("Dwarven Pickaxe", (6 - (RandomHelper.GetRandomNumber(1, 3) - 1))); break;
                case 7: treasure = CreateItem("Elven Bow", DefaultWeaponDurability); break;
                case 8: treasure = CreateItem("Elven Bowstring"); break;
                case 9: treasure = CreateItem("Potion of Restoration"); break;
                case 10: treasure = CreateItem("Relic - Epic"); break;
                case 11: treasure = CreateItem("Extended Battle Belt", 6); break;
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
                    treasure = CreateItem("Cloak", (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)), 300);
                    if (treasure is Armour magicCloak) { magicCloak.Name = "Magic Cloak of " + itemArray[0]; }
                    treasure.Description += itemArray[1];
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
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)), value);
                    treasure.Name = "Magic " + itemName + " of " + itemArray[0];
                    treasure.Description = itemArray[1];
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.Description += " Cursed: " + itemArray[2];
                    }
                    break;
                case 29:
                    itemArray = GetMagicItem("Item");
                    treasure = CreateItem("Ring", 0, 700);
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
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)), value);
                    if (treasure is MagicStaff heroWeaponMagic) { heroWeaponMagic.Name = "Magic " + itemName + " of " + itemArray[0]; }
                    treasure.Description = itemArray[1];
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.Description += " Cursed: " + itemArray[2];
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
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)));
                    break;
                case 36:
                case 37:
                case 38:
                    roll = RandomHelper.GetRandomNumber(1, 10);
                    switch (roll)
                    {
                        case 1: itemName = "Mail Coif"; value = 400; break;
                        case 2: itemName = "Mail Shirt"; value = 1200; break;
                        case 3: itemName = "Sleeved Mail Shirt"; value = 1900; break;
                        case 4: itemName = "Mail Coat"; value = 1500; break;
                        case 5: itemName = "Mail Leggings"; value = 400; break;
                        case 6: itemName = "Sleeved Mail Coat"; value = 2600; break;
                        case 7: itemName = "Plate Helmet"; value = 600; break;
                        case 8: itemName = "Breastplate"; value = 1400; break;
                        case 9: itemName = "Plate Leggings"; value = 1200; break;
                        case 10: itemName = "Plate Bracers"; value = 1400; break;
                    }
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)), value);
                    if (treasure is Armour mithrilArmour) { mithrilArmour.Name = "Mithril " + itemName; mithrilArmour.IsMithril = true; }
                    break;
                case 39:
                    treasure = CreateItem("Heater Shield", (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)), 300);
                    if (treasure is Shield mithrilShield) { mithrilShield.Name = "Mithril Heater Shield"; mithrilShield.IsMithril = true; }
                    break;
                case 40:
                case 41:
                case 42:
                    roll = RandomHelper.GetRandomNumber(1, 12);
                    switch (roll)
                    {
                        case 1: itemName = "Longsword"; value = 200; break;
                        case 2: itemName = "Warhammer"; value = 400; break;
                        case 3: itemName = "Battle Hammer"; value = 200; break;
                        case 4: itemName = "Morning Star"; value = 300; break;
                        case 5: itemName = "Battleaxe"; value = 200; break;
                        case 6: itemName = "Dagger"; value = 50; break;
                        case 7: itemName = "Shortsword"; value = 140; break;
                        case 8: itemName = "Rapier"; value = 260; break;
                        case 9: itemName = "Greatsword"; value = 400; break;
                        case 10: itemName = "Greataxe"; value = 400; break;
                        case 11: itemName = "Flail"; value = 300; break;
                        case 12: itemName = "Halberd"; value = 300; break;
                    }
                    treasure = CreateItem(itemName, (DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)), value);
                    if (treasure is MeleeWeapon mithrilWeapon) { mithrilWeapon.Name = "Mithril " + itemName; mithrilWeapon.IsMithril = true; }
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
                    treasure = CreateItem(itemName, (8 - (RandomHelper.GetRandomNumber(1, 3) - 1)));
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
                    treasure = CreateItem(itemName, (DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)));
                    break;
                case 46:
                case 47: treasure = CreateItem("Potion - Supreme"); break;
                case 48:
                case 49:
                case 50: treasure = CreateItem("Power Stone", 0, 1000, RandomHelper.GetRandomNumber(1, 3)); break;
                case 51:
                    treasure = CreateItem("Arrow", 0, 0, RandomHelper.GetRandomNumber(1, 10));
                    if (treasure is Ammo silverAmmo2) { silverAmmo2.Name = "Silver Arrows"; silverAmmo2.IsSilver = true; }
                    treasure.Description = silverDescription;
                    break;
                case 52:
                    roll = RandomHelper.GetRandomNumber(1, 4);
                    switch (roll)
                    {
                        case 1: itemName = "Greatsword"; value = 300; break;
                        case 2: itemName = "Greataxe"; value = 300; break;
                        case 3: itemName = "Flail"; value = 225; break;
                        case 4: itemName = "Halberd"; value = 225; break;
                    }
                    treasure = CreateItem(itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 3) - 1), value);
                    if (treasure is MeleeWeapon silverWeapon2) { silverWeapon2.Name = "Silver " + itemName; silverWeapon2.IsSilver = true; }
                    treasure.Description = silverDescription;
                    break;
                case 53: treasure = CreateItem("Tower Shield", DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 3) - 1)); break;
                case 54: treasure = CreateItem("Wyvern Cloak", DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 2) - 1)); break;
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
                items.AddRange(AlchemyService.GetIngredients(amount));
            }
            else if (type == "Part")
            {
                items.AddRange(AlchemyService.GetParts(amount, getOrigin));
            }

            return string.Join(", ", items);
        }

        public MagicStaff GetRandomWizardStaff(int durability)
        {
            int roll = RandomHelper.GetRandomNumber(0, _wizardStaves.Count - 1); // Adjust for 0-indexed list
            MagicStaff staffTemplate = _wizardStaves[roll];

            // Create a new instance based on the template to avoid modifying the shared template object
            // and apply the specific durability.
            MagicStaff newStaff = (MagicStaff)CreateItem(staffTemplate.Name, durability, staffTemplate.Value, 1, staffTemplate.Description);
            if (newStaff is MagicStaff meleeStaff)
            {
                meleeStaff.WeaponClass = staffTemplate.WeaponClass;
                meleeStaff.DamageRange = staffTemplate.DamageRange;
                meleeStaff.IsDefensive = staffTemplate.IsDefensive;
                meleeStaff.IsBlunt = staffTemplate.IsBlunt;
                // Copy other relevant properties from staffTemplate if necessary
            }
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
                case 1: armour = CreateItem("Padded Cap", durability); break;
                case 2: armour = CreateItem("Padded Vest", durability); break;
                case 3: armour = CreateItem("Padded Jacket", durability); break;
                case 4: armour = CreateItem("Padded Pants", durability); break;
                case 5: armour = CreateItem("Padded Coat", durability); break;
                case 6: armour = CreateItem("Leather Cap", durability); break;
                case 7: armour = CreateItem("Leather Vest", durability); break;
                case 8: armour = CreateItem("Leather Jacket", durability); break;
                case 9: armour = CreateItem("Leather Leggings", durability); break;
                case 10: armour = CreateItem("Leather Bracers", durability); break;
                case 11: armour = CreateItem("Mail Coif", durability); break;
                case 12: armour = CreateItem("Mail Shirt", durability); break;
                case 13: armour = CreateItem("Sleeved Mail Shirt", durability); break;
                case 14: armour = CreateItem("Mail Coat", durability); break;
                case 15: armour = CreateItem("Sleeved Mail Coat", durability); break;
                case 16: armour = CreateItem("Mail Leggings", durability); break;
                case 17: armour = CreateItem("Plate Helmet", durability); break;
                case 18: armour = CreateItem("Breastplate", durability); break;
                case 19: armour = CreateItem("Plate Bracers", durability); break;
                case 20: armour = CreateItem("Plate Leggings", durability); break;
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
                case 1: weapon = CreateItem("Dagger", durability); break;
                case 2: weapon = CreateItem("Shortsword", durability); break;
                case 3: weapon = CreateItem("Rapier", durability); break;
                case 4: weapon = CreateItem("Broadsword", durability); break;
                case 5: weapon = CreateItem("Longsword", durability); break;
                case 6: weapon = CreateItem("Battleaxe", durability); break;
                case 7: weapon = CreateItem("Battle Hammer", durability); break;
                case 8: weapon = CreateItem("Morning Star", durability); break;
                case 9: weapon = CreateItem("Flail", durability); break;
                case 10: weapon = CreateItem("Staff", durability); break;
                case 11: weapon = CreateItem("Javelin", durability); break;
                case 12: weapon = CreateItem("Greatsword", durability); break;
                case 13: weapon = CreateItem("Greataxe", durability); break;
                case 14: weapon = CreateItem("Warhammer", durability); break;
                case 15: weapon = CreateItem("Halberd", durability); break;
                case 16: weapon = CreateItem("Shortbow", durability); break;
                case 17: weapon = CreateItem("Longbow", durability); break;
                case 18: weapon = CreateItem("Elven Bow", durability); break;
                case 19: weapon = CreateItem("Crossbow Pistol", durability); break;
                case 20: weapon = CreateItem("Crossbow", durability); break;
                case 21: weapon = CreateItem("Arbalest", durability); break;
                case 22: weapon = CreateItem("Sling", durability); break;
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
                case "Arrow":
                    newItem = new Ammo { Name = "Arrow", Encumbrance = 0, Durability = 0, Value = 1, Quantity = 5, MaxDurability = 0 }; // Ammo typically has no durability
                    break;
                case "Bolt":
                    newItem = new Ammo { Name = "Bolt", Encumbrance = 0, Durability = 0, Value = 1, Quantity = 5, MaxDurability = 0 };
                    break;
                case "Superior Sling Stone":
                    newItem = new Ammo { Name = "Superior Sling Stone", Encumbrance = 0, Durability = 0, Value = 2, Quantity = 10, IsSupSlingstone = true, Description = "Gives RS +5 and DMG +1", MaxDurability = 0 };
                    break;
                case "Aim Attachment":
                    newItem = new Equipment { Name = "Aim Attachment", Encumbrance = 0, Durability = 0, Value = 200, Description = "Attached to a class 6 ranged weapon, providing RS +15 instead of the normal +10 when aiming", MaxDurability = 0 };
                    break;
                case "Alchemist Tools":
                    newItem = new Equipment { Name = "Alchemist Tools", Encumbrance = 5, Value = 200, Description = "used to harvest parts and ingredients", MaxDurability = 6 };
                    break;
                case "Alchemist Belt":
                    newItem = new Equipment { Name = "Alchemist Belt", Encumbrance = 0, Value = 300, Description = "grants 6 quick slots for use with potions only", MaxDurability = 6 };
                    break;
                case "Potion Recipe - Weak":
                    newItem = new Equipment { Name = $"Weak Potion Recipe: {AlchemyService.GetNonStandardPotion()}", Encumbrance = 0, Durability = 0, Value = 0, Description = "The actual components involved shall be chosen by the player", MaxDurability = 0 };
                    break;
                case "Ingredient":
                    newItem = new AlchemyItem { Name = "Ingredient", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Part":
                    newItem = new AlchemyItem { Name = "Part", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Ingredient - Exquisite":
                    newItem = new AlchemyItem { Name = "Ingredient - Exquisite", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Part - Exquisite":
                    newItem = new AlchemyItem { Name = "Part - Exquisite", Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 };
                    break;
                case "Armour Repair Kit":
                    newItem = new Equipment { Name = "Armour Repair Kit", Encumbrance = 5, Durability = 0, Value = 200, Description = "Used to repair 1 piece of armour while resting for 1d3 durability", MaxDurability = 0 }; // Durability here for 'uses' rather than item itself
                    break;
                case "Amulet":
                    newItem = new Equipment { Name = "Amulet", Encumbrance = 0, Durability = 0, Value = 100, Description = "Can be enchanted", MaxDurability = 0 };
                    break;
                case "Backpack - Medium":
                    newItem = new Equipment { Name = "Backpack - Medium", Encumbrance = 0, Durability = 0, Value = 350, Description = "Increases Enc +10, but lowers Dex -5", MaxDurability = 0 };
                    break;
                case "Backpack - Large":
                    newItem = new Equipment { Name = "Backpack - Large", Encumbrance = 1, Durability = 0, Value = 600, Description = "Increase ENC +25, lowering DEX -15", MaxDurability = 0 };
                    break;
                case "Backpack - Huge":
                    newItem = new Equipment { Name = "Backpack - Huge", Encumbrance = 1, Durability = 0, Value = 850, Description = "Increase ENC +35, while decreasing DEX -20", MaxDurability = 0 };
                    break;
                case "Bandage (Old Rag)":
                    newItem = new Equipment { Name = "Bandage (Old Rag)", Encumbrance = 1, Durability = 1, Value = 15, Description = "Heals 1d4 wounds when using the heal skill", MaxDurability = 1 }; // Disposable
                    break;
                case "Bandage (Linen)":
                    newItem = new Equipment { Name = "Bandage (Linen)", Encumbrance = 1, Durability = 1, Value = 25, Description = "Heals 1d8 wounds when using the heal skill", MaxDurability = 1 }; // Disposable
                    break;
                case "Bear Trap":
                    newItem = new Equipment { Name = "Bear Trap", Encumbrance = 5, Durability = 2, Value = 200, Description = "See rules on pg 133 of rule book", MaxDurability = 2 };
                    break;
                case "Bedroll":
                    newItem = new Equipment { Name = "Bedroll", Encumbrance = 5, Durability = 0, Value = 200, Description = "Regain all energy when used while resting", MaxDurability = 0 };
                    break;
                case "Beef Jerky":
                    newItem = new Equipment { Name = "Beef Jerky", Encumbrance = 0, Durability = 0, Description = "Eat to restore 1hp", MaxDurability = 0 };
                    break;
                case "Combat Harness":
                    newItem = new Equipment { Name = "Combat Harness", Encumbrance = 0, Durability = 6, Value = 500, Description = "Increases quick slots from 3 to 5", MaxDurability = 6 };
                    break;
                case "Crowbar":
                    newItem = new Equipment { Name = "Crowbar", Encumbrance = 10, Value = 55, Description = "inflict 1d8+8 dmg to doors and chests with a threat lvl +1", MaxDurability = 6 };
                    break;
                case "Door Mirror":
                    newItem = new Equipment { Name = "Door Mirror", Encumbrance = 0, Durability = 0, Value = 300, Description = "See item special rules on pg 133 in rule book", MaxDurability = 0 };
                    break;
                case "Dwarven Ale":
                    newItem = new Equipment { Name = "Dwarven Ale", Encumbrance = 2, Durability = 1, Value = 100, Description = "Enough for a single serving (1-4 heros). RES +20, but all other skills -10 for the dungeon/skirmish", MaxDurability = 1 };
                    break;
                case "Dwarven Pickaxe":
                    newItem = new Equipment { Name = "Dwarven Pickaxe", Encumbrance = 8, Durability = 6, Value = 225, Description = "", MaxDurability = 6 };
                    break;
                case "Elven Bowstring":
                    newItem = new Equipment { Name = "Elven Bowstring", Encumbrance = 0, Durability = 0, Value = 0, Description = "After adding to any bow during a rest, it adds RS +5 to the weapon", MaxDurability = 0 };
                    break;
                case "Extended Battle Belt":
                    newItem = new Equipment { Name = "Extended Battle Belt", Encumbrance = 0, Durability = 6, Value = 300, Description = "Increases quick access slots from 3 to 4", MaxDurability = 6 };
                    break;
                case "Fishing Gear":
                    newItem = new Equipment { Name = "Fishing Gear", Encumbrance = 3, Durability = 0, Value = 40, Description = "Grants a Foraging +10 modifier", MaxDurability = 0 };
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
                case "Iron Wedge":
                    newItem = new Equipment { Name = "Iron Wedge", Encumbrance = 4, Durability = 6, Value = 50, Description = "Takes 1AP to use and an additional 1AP if door is open. Wandering monsters will take 3 turns to open door", MaxDurability = 6 };
                    break;
                case "Lantern":
                    newItem = new Equipment { Name = "Lantern", Encumbrance = 1, Durability = 0, Value = 100, Description = "See Lantern description and special rules on pg 168 of the rule book", MaxDurability = 0 };
                    break;
                case "Lock Pick":
                    newItem = new Equipment { Name = "Lock Pick", Encumbrance = 0, Durability = 1, Value = 6, Quantity = 5, Description = "Necessary for picking locks. If attempt fails the pick is broken", MaxDurability = 1 };
                    break;
                case "Lock Picks - Dwarven":
                    newItem = new Equipment { Name = "Lock Picks - Dwarven", Encumbrance = 0, Durability = 1, Value = 0, Description = "Lock picks that add +10 to the lock picking attempt", MaxDurability = 1 };
                    break;
                case "Lute":
                    newItem = new Equipment { Name = "Lute", Encumbrance = 5, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, to recover 1 sanity for all heroes.", MaxDurability = 0 };
                    break;
                case "Necklace":
                    newItem = new Equipment { Name = "Necklace", Encumbrance = 0, Durability = 0, Value = 150, Description = "Can be enchanted", MaxDurability = 0 };
                    break;
                case "Partial Map":
                    newItem = new Equipment { Name = "Partial Map", Encumbrance = 0, Durability = 1, Value = 0, Description = "This map lets the player look at 2 unused encounter cards, afterwards placed back where they were.", MaxDurability = 1 };
                    break;
                case "Empty Bottle":
                    newItem = new AlchemyItem { Name = "Empty Bottle", Encumbrance = 0, Value = 15, Description = "These bottles can be used to brew potions", MaxDurability = 0 };
                    break;
                case "Potion - Weak":
                    newItem = new AlchemyItem { Name = "Potion - Weak", Encumbrance = 1, Durability = 1, Value = 0, Strength = PotionStrength.Weak, CreatePotion = true, MaxDurability = 1 };
                    break;
                case "Potion - Standard":
                    newItem = new AlchemyItem { Name = "Potion - Standard", Encumbrance = 1, Durability = 1, Value = 0, Strength = PotionStrength.Standard, CreatePotion = true, MaxDurability = 1 };
                    break;
                case "Potion - Supreme":
                    newItem = new AlchemyItem { Name = "Potion - Supreme", Encumbrance = 1, Durability = 1, Value = 0, Strength = PotionStrength.Supreme, CreatePotion = true, MaxDurability = 1 };
                    break;
                case "Potion of Health":
                    newItem = new AlchemyItem { Name = "Potion of Health", Encumbrance = 1, Durability = 1, Value = 100, Description = "This potion heals 1d6HP upon drinking", MaxDurability = 1 };
                    break;
                case "Potion of Restoration":
                    newItem = new AlchemyItem { Name = "Potion of Restoration", Encumbrance = 1, Durability = 1, Value = 200, Description = "Brings a hero to full health even when dead or knocked out. Additionally cures any disease and poison", MaxDurability = 1 };
                    break;
                case "Power Stone":
                    string[] stoneData = GetPowerStone();
                    newItem = new Equipment { Name = stoneData[0], Encumbrance = 0, Durability = 0, Value = 1000, Description = stoneData[1], MaxDurability = 0 };
                    break;
                case "Ration":
                    newItem = new Equipment { Name = "Ration", Encumbrance = 1, Durability = 1, Value = 5, Description = "If eaten while resting heal 1d6HP and 50% chance of regaining 1d6 energy. Also used to avoid hunger while traveling", MaxDurability = 1 };
                    break;
                case "Ring":
                    newItem = new Equipment { Name = "Ring", Encumbrance = 0, Durability = 0, Value = 150, Description = "can be enchanted", MaxDurability = 0 };
                    break;
                case "Rope (Old)":
                    newItem = new Equipment { Name = "Rope (Old)", Encumbrance = 2, Durability = 1, Value = 20, Description = "See special rules on pg 167 of the rule book", MaxDurability = 1 };
                    break;
                case "Rope":
                    newItem = new Equipment { Name = "Rope", Encumbrance = 2, Durability = 1, Value = 50, Description = "See special rules on pg 167 of the rule book", MaxDurability = 1 };
                    break;
                case "Set of Fine Clothes":
                    newItem = new Equipment { Name = "Set of Fine Clothes", Encumbrance = 0, Durability = 0, Value = 0, Description = "Increases Barter +5", MaxDurability = 0 };
                    break;
                case "Scroll":
                    newItem = new Equipment { Name = $"Scroll of {new GameDataService().GetRandomSpellName()}", Encumbrance = 0, Durability = 0, Value = 100, MaxDurability = 0 };
                    break;
                case "Skinning Knife":
                    newItem = new Equipment { Name = "Skinning Knife", Encumbrance = 1, Durability = 0, Value = 100, Description = "Allows hero to skin animals", MaxDurability = DefaultWeaponDurability }; // Assuming it has weapon durability
                    break;
                case "Skinning Knife - Elven":
                    newItem = new Equipment { Name = "Skinning Knife - Elven", Encumbrance = 1, Durability = 0, Value = 250, Description = "Allows hero to skin animals with a Foraging +10 modifier", MaxDurability = DefaultWeaponDurability };
                    break;
                case "Talent Training Manual":
                    newItem = new Equipment { Name = $"{new GameDataService().GetRandomTalent()} Training Manual", Encumbrance = 1, Durability = 0, Value = 0, Description = "Grants the talent named on the book, when read at an inn", MaxDurability = 0 };
                    break;
                case "Tobacco":
                    newItem = new Equipment { Name = "Tobacco", Encumbrance = 0, Durability = 0, Value = 50, Description = "Use to provide RES +15 for 1 dungeon", MaxDurability = 0 };
                    break;
                case "Trap Disarming Kit":
                    newItem = new Equipment { Name = "Trap Disarming Kit", Encumbrance = 5, Durability = 6, Value = 200, Description = "Pick lock +10 when disarming traps", MaxDurability = 6 };
                    break;
                case "Trap Disarming Kit - Superior":
                    newItem = new Equipment { Name = "Trap Disarming Kit - Superior", Encumbrance = 4, Value = 250, Description = "Gives Pick lock +15 when disarming traps", MaxDurability = 6 };
                    break;
                case "Torch":
                    newItem = new Equipment { Name = "Torch", Encumbrance = 1, Durability = 1, Value = 15, Description = "See Torch description and special rules on pg 168 of the rule book", MaxDurability = 1 };
                    break;
                case "Whetstone":
                    newItem = new Equipment { Name = "Whetstone", Encumbrance = 5, Durability = 0, Value = 200, Description = "Use to repair CC weapons for 1d3 durability. Can be used 3 times during resting", MaxDurability = 0 }; // Durability here refers to charges/uses
                    break;
                case "Wild Game Traps":
                    newItem = new Equipment { Name = "Wild Game Traps", Encumbrance = 3, Durability = 0, Value = 150, Description = "Foraging +10 when rolling to catch an animal", MaxDurability = 0 };
                    break;
                case "Wolf Pelt":
                    newItem = new Equipment { Name = "Wolf Pelt", Encumbrance = 2, Durability = 0, Value = 50, MaxDurability = 0 };
                    break;
                // --- Weapons ---
                case "Dagger":
                    newItem = new MeleeWeapon { Name = "Dagger", Encumbrance = 5, Value = 30, WeaponClass = 1, DamageRange = new int[2] { 1, 6 }, IsEdged = true, IsSword = true, DualWieldBonus = 1, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Shortsword":
                    newItem = new MeleeWeapon { Name = "Shortsword", Encumbrance = 7, Value = 30, WeaponClass = 2, DamageRange = new int[2] { 3, 8 }, IsEdged = true, IsSword = true, DualWieldBonus = 2, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Rapier":
                    newItem = new MeleeWeapon { Name = "Rapier", Encumbrance = 5, Value = 30, WeaponClass = 1, DamageRange = new int[2] { 2, 7 }, IsEdged = true, IsSword = true, IsFast = true, DualWieldBonus = 1, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Broadsword":
                    newItem = new MeleeWeapon { Name = "Broadsword", Encumbrance = 8, Value = 30, WeaponClass = 3, DamageRange = new int[2] { 3, 10 }, IsEdged = true, IsSword = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Longsword":
                    newItem = new MeleeWeapon { Name = "Longsword", Encumbrance = 10, Value = 30, WeaponClass = 4, DamageRange = new int[2] { 1, 12 }, IsEdged = true, IsSword = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Battleaxe":
                    newItem = new MeleeWeapon { Name = "Battleaxe", Encumbrance = 10, Value = 30, WeaponClass = 4, DamageRange = new int[2] { 2, 11 }, ArmourPiercing = 1, IsBFO = true, IsEdged = true, IsAxe = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Battle Hammer":
                    newItem = new MeleeWeapon { Name = "Battle Hammer", Encumbrance = 10, Value = 30, WeaponClass = 3, DamageRange = new int[2] { 1, 10 }, IsBFO = true, IsStun = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Morning Star":
                    newItem = new MeleeWeapon { Name = "Morning Star", Encumbrance = 10, Value = 30, WeaponClass = 4, DamageRange = new int[2] { 5, 12 }, IsUnwieldly = true, IsBFO = true, IsStun = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Flail":
                    newItem = new MeleeWeapon { Name = "Flail", Encumbrance = 20, Value = 30, WeaponClass = 5, DamageRange = new int[2] { 5, 14 }, IsUnwieldly = true, IsBFO = true, IsStun = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Staff":
                    newItem = new MeleeWeapon { Name = "Staff", Encumbrance = 5, Value = 30, WeaponClass = 2, DamageRange = new int[2] { 1, 8 }, IsDefensive = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Javelin":
                    newItem = new MeleeWeapon { Name = "Javelin", Encumbrance = 10, Value = 30, WeaponClass = 2, DamageRange = new int[2] { 1, 10 }, IsReach = true, ArmourPiercing = 1, IsBFO = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Greatsword":
                    newItem = new MeleeWeapon { Name = "Greatsword", Encumbrance = 20, Value = 30, WeaponClass = 5, DamageRange = new int[2] { 2, 12 }, IsSlow = true, IsEdged = true, IsSword = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Greataxe":
                    newItem = new MeleeWeapon { Name = "Greataxe", Encumbrance = 20, Value = 30, WeaponClass = 5, DamageRange = new int[2] { 5, 16 }, ArmourPiercing = 2, IsBFO = true, IsSlow = true, IsEdged = true, IsAxe = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Warhammer":
                    newItem = new MeleeWeapon { Name = "Warhammer", Encumbrance = 20, Value = 30, WeaponClass = 5, DamageRange = new int[2] { 2, 12 }, IsSlow = true, IsBFO = true, IsStun = true, IsBlunt = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Halberd":
                    newItem = new MeleeWeapon { Name = "Halberd", Encumbrance = 20, Value = 30, WeaponClass = 5, DamageRange = new int[2] { 1, 12 }, IsReach = true, ArmourPiercing = 1, IsEdged = true, IsAxe = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Shortbow":
                    newItem = new RangedWeapon { Name = "Shortbow", Encumbrance = 5, Value = 30, WeaponClass = 6, DamageRange = new int[2] { 1, 8 }, ReloadTime = 1, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Longbow":
                    newItem = new RangedWeapon { Name = "Longbow", Encumbrance = 10, Value = 30, WeaponClass = 6, DamageRange = new int[2] { 1, 10 }, ReloadTime = 1, ArmourPiercing = 1, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Elven Bow":
                    newItem = new RangedWeapon { Name = "Elven Bow", Encumbrance = 7, Value = 30, WeaponClass = 6, DamageRange = new int[2] { 3, 12 }, ReloadTime = 1, ArmourPiercing = 1, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Crossbow Pistol":
                    newItem = new RangedWeapon { Name = "Crossbow Pistol", Encumbrance = 5, Value = 30, WeaponClass = 6, DamageRange = new int[2] { 2, 9 }, ReloadTime = 2, IsSecondaryWeapon = true, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Crossbow":
                    newItem = new RangedWeapon { Name = "Crossbow", Encumbrance = 15, Value = 30, WeaponClass = 6, DamageRange = new int[2] { 4, 13 }, ReloadTime = 2, ArmourPiercing = 1, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Arbalest":
                    newItem = new RangedWeapon { Name = "Arbalest", Encumbrance = 20, Value = 30, WeaponClass = 5, DamageRange = new int[2] { 3, 18 }, ReloadTime = 3, ArmourPiercing = 2, MaxDurability = DefaultWeaponDurability };
                    break;
                case "Sling":
                    newItem = new RangedWeapon { Name = "Sling", Encumbrance = 1, Value = 30, WeaponClass = 6, DamageRange = new int[2] { 1, 8 }, ReloadTime = 1, MaxDurability = DefaultWeaponDurability };
                    break;
                // --- Armour ---
                case "Padded Cap":
                    newItem = new Armour { Name = "Padded Cap", Encumbrance = 1, Value = 30, ArmourClass = 1, DefValue = 2, IsStackable = true, IsHead = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Padded Vest":
                    newItem = new Armour { Name = "Padded Vest", Encumbrance = 3, Value = 60, ArmourClass = 1, DefValue = 2, IsStackable = true, IsTorso = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Padded Jacket":
                    newItem = new Armour { Name = "Padded Jacket", Encumbrance = 5, Value = 120, ArmourClass = 1, DefValue = 2, IsStackable = true, IsTorso = true, IsArms = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Padded Pants":
                    newItem = new Armour { Name = "Padded Pants", Encumbrance = 4, Value = 6, ArmourClass = 1, DefValue = 2, IsStackable = true, IsLegs = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Padded Coat":
                    newItem = new Armour { Name = "Padded Coat", Encumbrance = 6, Value = 200, ArmourClass = 1, DefValue = 2, IsLegs = true, IsTorso = true, IsArms = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Leather Cap":
                    newItem = new Armour { Name = "Leather Cap", Encumbrance = 1, Value = 50, ArmourClass = 2, DefValue = 3, IsHead = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Leather Vest":
                    newItem = new Armour { Name = "Leather Vest", Encumbrance = 3, Value = 80, ArmourClass = 2, DefValue = 3, IsTorso = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Leather Jacket":
                    newItem = new Armour { Name = "Leather Jacket", Encumbrance = 4, Value = 140, ArmourClass = 2, DefValue = 3, IsTorso = true, IsArms = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Leather Leggings":
                    newItem = new Armour { Name = "Leather Leggings", Encumbrance = 3, Value = 120, ArmourClass = 2, DefValue = 3, IsLegs = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Leather Bracers":
                    newItem = new Armour { Name = "Leather Bracers", Encumbrance = 3, Value = 120, ArmourClass = 2, DefValue = 3, IsStackable = true, IsArms = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Nightstalker Cap":
                    newItem = new Armour { Name = "Nightstalker Cap", Encumbrance = 1, Value = 230, MaxDurability = 8, ArmourClass = 2, DefValue = 4, IsHead = true };
                    break;
                case "Nightstalker Vest":
                    newItem = new Armour { Name = "Nightstalker Vest", Encumbrance = 3, Value = 650, MaxDurability = 8, ArmourClass = 2, DefValue = 4, IsTorso = true, IsDarkAsTheNight = true };
                    break;
                case "Nightstalker Jacket":
                    newItem = new Armour { Name = "Nightstalker Jacket", Encumbrance = 4, Value = 1000, MaxDurability = 8, ArmourClass = 2, DefValue = 4, IsTorso = true, IsArms = true, IsDarkAsTheNight = true };
                    break;
                case "Nightstalker Leggings":
                    newItem = new Armour { Name = "Nightstalker Leggings", Encumbrance = 3, Value = 900, MaxDurability = 8, ArmourClass = 2, DefValue = 4, IsLegs = true, IsDarkAsTheNight = true };
                    break;
                case "Nightstalker Bracers":
                    newItem = new Armour { Name = "Nightstalker Bracers", Encumbrance = 3, Value = 150, MaxDurability = 8, ArmourClass = 2, DefValue = 4, IsArms = true };
                    break;
                case "Mail Coif":
                    newItem = new Armour { Name = "Mail Coif", Encumbrance = 4, Value = 200, ArmourClass = 3, DefValue = 4, IsStackable = true, IsHead = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Mail Shirt":
                    newItem = new Armour { Name = "Mail Shirt", Encumbrance = 6, Value = 600, ArmourClass = 3, DefValue = 4, IsStackable = true, IsTorso = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Sleeved Mail Shirt":
                    newItem = new Armour { Name = "Sleeved Mail Shirt", Encumbrance = 7, Value = 950, ArmourClass = 3, DefValue = 4, IsStackable = true, IsTorso = true, IsArms = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Mail Coat":
                    newItem = new Armour { Name = "Mail Coat", Encumbrance = 8, Value = 750, ArmourClass = 3, DefValue = 4, IsStackable = true, IsLegs = true, IsTorso = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Sleeved Mail Coat":
                    newItem = new Armour { Name = "Sleeved Mail Coat", Encumbrance = 10, Value = 1300, ArmourClass = 3, DefValue = 4, IsStackable = true, IsLegs = true, IsTorso = true, IsArms = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Mail Leggings":
                    newItem = new Armour { Name = "Mail Leggings", Encumbrance = 5, Value = 200, ArmourClass = 3, DefValue = 4, IsStackable = true, IsLegs = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Plate Helmet":
                    newItem = new Armour { Name = "Plate Helmet", Encumbrance = 5, Value = 300, ArmourClass = 4, DefValue = 5, IsStackable = true, IsClunky = true, IsHead = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Breastplate":
                    newItem = new Armour { Name = "Breastplate", Encumbrance = 7, Value = 700, ArmourClass = 4, DefValue = 5, IsStackable = true, IsClunky = true, IsTorso = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Plate Bracers":
                    newItem = new Armour { Name = "Plate Bracers", Encumbrance = 4, Value = 600, ArmourClass = 4, DefValue = 5, IsStackable = true, IsArms = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Plate Leggings":
                    newItem = new Armour { Name = "Plate Leggings", Encumbrance = 6, Value = 700, ArmourClass = 4, DefValue = 5, IsLegs = true, IsStackable = true, IsClunky = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Dragon Scale Cap":
                    newItem = new Armour { Name = "Dragon Scale Cap", Encumbrance = 4, Value = 1000, Description = "Treat fire DMG as ordinary DMG, Max Durability is 10", ArmourClass = 3, DefValue = 7, IsHead = true, MaxDurability = 10 };
                    break;
                case "Dragon Scale Breastplate":
                    newItem = new Armour { Name = "Dragon Scale Breastplate", Encumbrance = 6, Value = 2300, Description = "Treat fire DMG as ordinary DMG, Max Durability is 10", ArmourClass = 3, DefValue = 7, IsTorso = true, MaxDurability = 10 };
                    break;
                case "Dragon Scale Pants":
                    newItem = new Armour { Name = "Dragon Scale Pants", Encumbrance = 5, Value = 1900, Description = "Treat fire DMG as ordinary DMG, Max Durability is 10", ArmourClass = 3, DefValue = 7, IsLegs = true, MaxDurability = 10 };
                    break;
                case "Dragon Scale Bracers":
                    newItem = new Armour { Name = "Dragon Scale Bracers", Encumbrance = 3, Value = 2000, Description = "Treat fire DMG as ordinary DMG, Max Durability is 10", ArmourClass = 3, DefValue = 7, IsArms = true, MaxDurability = 10 };
                    break;
                case "Buckler":
                    newItem = new Shield { Name = "Buckler", Encumbrance = 4, Value = 20, ArmourClass = 1, DefValue = 4, IsShield = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Heater Shield":
                    newItem = new Shield { Name = "Heater Shield", Encumbrance = 10, Value = 100, ArmourClass = 3, DefValue = 8, IsShield = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Tower Shield":
                    newItem = new Shield { Name = "Tower Shield", Encumbrance = 15, Value = 200, ArmourClass = 4, DefValue = 8, IsShield = true, IsHuge = true, MaxDurability = DefaultArmourDurability };
                    break;
                case "Cloak":
                    newItem = new Armour { Name = "Cloak", Encumbrance = 1, Value = 50, Description = "Def:1 against attacks from behind", ArmourClass = 1, DefValue = 1, MaxDurability = DefaultArmourDurability };
                    break;
                case "Wyvern Cloak":
                    newItem = new Armour { Name = "Wyvern Cloak", Encumbrance = 2, Value = 1200, Description = "Def:3 against attacks from behind", ArmourClass = 1, DefValue = 3, MaxDurability = DefaultArmourDurability };
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