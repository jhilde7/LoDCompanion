using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public enum TreasureType
    {
        T1,
        T2,
        T3,
        T4,
        T5,
        Part,
        Ingredient,
        None,
        Mundane,
        Fine,
        Wonderful,
        Turog,
        TheMasterLocksmith
    }

    public class TreasureService
    {
        private const int DefaultArmourDurability = 6;
        private const int DefaultWeaponDurability = 6;
        private readonly AlchemyService _alchemy;
        private readonly UserRequestService _diceRoll;
        private readonly WeaponFactory _weaponFactory;
        private readonly ArmourFactory _armourFactory;

        public TreasureService(
            AlchemyService alchemyService,
            UserRequestService diceRollService,
            WeaponFactory weaponFactory,
            ArmourFactory armourFactory)
        {
            _alchemy = alchemyService;
            _diceRoll = diceRollService;
            _weaponFactory = weaponFactory;
            _armourFactory = armourFactory;
        }

        public async Task<string> GetTreasureAsync(string itemName, int durability = 0, int value = 0, int amount = 1, string description = "")
        {
            Equipment item = await CreateItemAsync(itemName, durability, value, amount, description);
            return $"{item.Quantity} {item.Name}";
        }

        public async Task<List<string>> SearchCorpseAsync(TreasureType type, Hero hero, int searchRoll)
        {
            List<string> rewards = new List<string>();
            if (searchRoll == 0 || searchRoll > 10)
            {
                var resultRoll = await _diceRoll.RequestRollAsync($"Roll for treasure", "1d10"); await Task.Yield();
                searchRoll = resultRoll.Roll;
            }

            int count = 1;
            if (hero.IsThief)
            {
                count = 2;
            }

            switch (type)
            {
                case TreasureType.T1:
                    switch (searchRoll)
                    {
                        case 1:
                            Equipment? equipmentFound = await GetRandomWeaponAsync(DefaultWeaponDurability - RandomHelper.GetRandomNumber(1, 4));
                            if (equipmentFound != null) rewards.Add(await GetTreasureAsync(equipmentFound.Name, equipmentFound.Durability));
                            break;
                        case 2:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 20));
                            break;
                        case 3:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 10));
                            break;
                        case 4:
                            rewards.Add(await GetTreasureAsync("Bandage (old rags)", 1));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case TreasureType.T2:
                    switch (searchRoll)
                    {
                        case 1:
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Fine, count));
                            break;
                        case 2:
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Mundane, count));
                            break;
                        case 3:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 50));
                            break;
                        case 4:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 40));
                            break;
                        case 5:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 20));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case TreasureType.T3:
                    switch (searchRoll)
                    {
                        case 1:
                        case 2:
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Fine, count));
                            break;
                        case 3:
                        case 4:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 100));
                            break;
                        case 5:
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Mundane, count));
                            break;
                        case 6:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 80));
                            break;
                        case 7:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 60));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case TreasureType.T4:
                    switch (searchRoll)
                    {
                        case 1:
                            rewards.Add(await GetTreasureAsync("Grimoire"));
                            break;
                        case 2:
                        case 3:
                            rewards.Add(await GetTreasureAsync("Scroll"));
                            break;
                        case 4:
                            for (int i = 0; i < RandomHelper.GetRandomNumber(1, 2); i++)
                            {
                                var potions = await _alchemy.GetRandomPotions(1, RandomHelper.GetRandomEnumValue<PotionStrength>(1, 3));
                                rewards.Add(potions[0].Name);
                            }
                            break;
                        case 5:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 150));
                            break;
                        case 6:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 100));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case TreasureType.T5:
                    switch (searchRoll)
                    {
                        case 1:
                        case 2:
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Wonderful, count));
                            count *= 2;
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Fine, count));
                            break;
                        case 3:
                        case 4:
                            count *= 2;
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Fine, count));
                            rewards.Add(await GetTreasureAsync("Grimoire"));
                            break;
                        case 5:
                        case 6:
                        case 7:
                            count *= 3;
                            rewards.AddRange(await FoundTreasureAsync(TreasureType.Fine, count));
                            break;
                        case 8:
                        case 9:
                        case 10:
                            rewards.Add(await GetTreasureAsync("Coin", 0, 1, 500));
                            break;
                        default:
                            rewards.Add("You found nothing.");
                            break;
                    }
                    break;
                case TreasureType.Part:
                    if (searchRoll == 0)
                    {
                        var resultPart = await _diceRoll.RequestRollAsync($"Roll for part", "1d100"); await Task.Yield();
                        searchRoll = resultPart.Roll;
                    }
                    if (searchRoll <= hero.GetSkill(Skill.Alchemy))
                    {
                        rewards.Add(await GetTreasureAsync("Part", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Part, 1, false)));
                    }
                    else
                    {
                        rewards.Add("You found nothing.");
                    }
                    break;
                case TreasureType.Turog:
                    var resultTurog = await _diceRoll.RequestRollAsync($"Roll for coins", "2d100"); await Task.Yield();
                    rewards.Add(await GetTreasureAsync("Coin", 0, 1, resultTurog.Roll));
                    rewards.Add("The Goblins Scimitar");
                    break;
                case TreasureType.TheMasterLocksmith:
                    rewards.AddRange(await SearchCorpseAsync(TreasureType.T5, hero, searchRoll));
                    rewards.Add("The Flames of Zul");
                    break;
                default:
                    break;
            }
            return rewards;
        }

        public async Task<List<string>> FoundTreasureAsync(TreasureType type, int count)
        {
            List<string> rewards = new List<string>();
            for (int i = 0; i < count; i++)
            {
                Equipment? item;
                switch (type)
                {
                    case TreasureType.Mundane:
                        item = await GetMundaneTreasureAsync();
                        if (item != null) rewards.Add(item.Name);
                        break;
                    case TreasureType.Fine:
                        item = await GetFineTreasureAsync();
                        if (item != null) rewards.Add(item.Name);
                        break;
                    case TreasureType.Wonderful:
                        item = await GetWonderfulTreasureAsync();
                        if (item != null) rewards.Add(item.Name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }
            return rewards;
        }

        public async Task<Equipment?> GetMundaneTreasureAsync()
        {
            int roll = RandomHelper.GetRandomNumber(1, 54);
            int defaultDurabilityDamageRoll = RandomHelper.GetRandomNumber(1, 4) + 1;
            int armourDurability = DefaultArmourDurability - defaultDurabilityDamageRoll;
            int weaponDurability = DefaultWeaponDurability - defaultDurabilityDamageRoll;
            // Console.WriteLine($"Treasure roll {roll}"); // For debugging, replace with proper logging

            string itemName = "";
            Equipment? treasure;

            switch (roll)
            {
                case 1: treasure = await CreateItemAsync("Amulet"); break;
                case 2: treasure = EquipmentService.GetAmmoByNameSetQuantity("Arrow", 5); break;
                case 3: treasure = EquipmentService.GetAmmoByNameSetQuantity("Arrow", 10); break;
                case <= 5: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Bandage (old rags)", RandomHelper.GetRandomNumber(1, 3)); break;
                case 6: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Bandage (linen)", RandomHelper.GetRandomNumber(1, 2)); break;
                case 7: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Bear Trap", 2 - (RandomHelper.GetRandomNumber(1, 2) - 1), RandomHelper.GetRandomNumber(1, 3)); break;
                case <= 9: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Bedroll", RandomHelper.GetRandomNumber(1, 3)); break;
                case 10: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Beef Jerky", RandomHelper.GetRandomNumber(1, 4)); break;
                case 11: treasure = EquipmentService.GetAmmoByNameSetQuantity("Bolt", 5); break;
                case 12: treasure = EquipmentService.GetShieldByNameSetDurability("Buckler", armourDurability); break;
                case <= 14: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "1d20")).Roll); await Task.Yield(); break;
                case 15: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "2d20")).Roll); await Task.Yield(); break;
                case  <= 17: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "3d20")).Roll); await Task.Yield(); break;
                case 18: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "4d20")).Roll); await Task.Yield(); break;
                case 19: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "1d100")).Roll); await Task.Yield(); break;
                case 20: treasure = EquipmentService.GetArmourByNameSetDurability("Cloak", armourDurability); break;
                case 21: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Crowbar", 6 - defaultDurabilityDamageRoll, RandomHelper.GetRandomNumber(1, 6)); break;
                case 22: treasure = EquipmentService.GetWeaponByNameSetDurability("Dagger", weaponDurability); break;
                case 23: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Empty Bottle", RandomHelper.GetRandomNumber(1, 6)); break;
                case 24: treasure = await GetFineTreasureAsync(); break;
                case 25: treasure = await CreateItemAsync("Ingredient", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Ingredient, RandomHelper.GetRandomNumber(1, 3))); break;
                case 26: treasure = EquipmentService.GetWeaponByNameSetDurability("Javelin", weaponDurability); break;
                case 27: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Lantern", RandomHelper.GetRandomNumber(1, 3)); break;
                case 28:
                    roll = (await _diceRoll.RequestRollAsync($"You found leather armour!", "1d6")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Leather Cap"; break;
                        case <= 3: itemName = "Leather Vest"; break;
                        case 4: itemName = "Leather Jacket"; break;
                        case 5: itemName = "Leather Leggings"; break;
                        case 6: itemName = "Leather Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, armourDurability);
                    break;
                case 29: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Lock Picks", RandomHelper.GetRandomNumber(1, 4)); break;
                case 30: treasure = EquipmentService.GetEquipmentByName("Backpack - Medium"); break;
                case 31: treasure = EquipmentService.GetEquipmentByName("Rope (old)"); break;
                case 32:
                    roll = (await _diceRoll.RequestRollAsync($"You found padded armour!", "1d6")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Padded Cap"; break;
                        case <= 3: itemName = "Padded Vest"; break;
                        case 4: itemName = "Padded Jacket"; break;
                        case 5: itemName = "Padded Pants"; break;
                        case 6: itemName = "Padded Coat"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, armourDurability);
                    break;
                case 33: treasure = await CreateItemAsync("Part", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Ingredient, RandomHelper.GetRandomNumber(1, 3))); break;
                case 34: treasure = await CreateItemAsync("Part", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Part, 1)); break;
                case 35: treasure = EquipmentService.GetWeaponByNameSetDurability("Rapier", weaponDurability); break;
                case 36: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Ration", RandomHelper.GetRandomNumber(1, 4)); break;
                case 37: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Ration", RandomHelper.GetRandomNumber(1, 6)); break;
                case 38: treasure = await CreateItemAsync("Potion Recipe - Weak"); break;
                case <= 40: treasure = EquipmentService.GetEquipmentByName("Ring"); break;
                case 41: treasure = EquipmentService.GetEquipmentByName("Rope"); break;
                case 42: treasure = EquipmentService.GetWeaponByNameSetDurability("Shortbow", weaponDurability); break;
                case 43: treasure = EquipmentService.GetWeaponByNameSetDurability("Shortsword", weaponDurability); break;
                case 44: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Skinning Knife", RandomHelper.GetRandomNumber(1, 3)); break;
                case 45: treasure = EquipmentService.GetWeaponByNameSetDurability("Sling", weaponDurability); break;
                case 46: treasure = EquipmentService.GetWeaponByNameSetDurability("Staff", weaponDurability); break;
                case <= 49: treasure = EquipmentService.GetEquipmentByName("Torch"); break;
                case 50: treasure = EquipmentService.GetEquipmentByName("Whetstone"); break;
                case 51: treasure = EquipmentService.GetEquipmentByName("Wild game traps"); break;
                case 52: treasure = await CreateItemAsync("Wolf Pelt", 0, 50, RandomHelper.GetRandomNumber(1, 3)); break;
                case 53: treasure = await GetWonderfulTreasureAsync(); break;
                case 54: treasure = await CreateItemAsync("Ingredient", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Ingredient, RandomHelper.GetRandomNumber(1, 3))); break;
                default:
                    treasure = await CreateItemAsync("Unknown Mundane Item"); // Fallback for unexpected rolls
                    break;
            }

            return treasure;
        }

        public async Task<Equipment?> GetFineTreasureAsync()
        {
            string itemName = "";
            int roll = RandomHelper.GetRandomNumber(1, 54);
            // Console.WriteLine($"Treasure roll {roll}");

            Equipment? treasure = new Equipment();

            switch (roll)
            {
                case 1: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Alchemist Tool", 6 - RandomHelper.GetRandomNumber(1, 4)); break;
                case 2: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Alchemist Belt", 6 - RandomHelper.GetRandomNumber(1, 4)); break;
                case 3: treasure = EquipmentService.GetEquipmentByName("Armour Repair Kit"); break;
                case 4:
                    roll = (await _diceRoll.RequestRollAsync($"You found a weapon!", "1d6")).Roll; await Task.Yield();
                    roll = (int)Math.Ceiling(roll / 2d);
                    switch (roll)
                    {
                        case 1: itemName = "Battleaxe"; break;
                        case 2: itemName = "Greataxe"; break;
                        case 3: itemName = "Halberd"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1));
                    break;
                case 5:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity("Barbed Arrow", 5);
                    break;
                case 6:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity("Barbed Bolt", 5);
                    break;
                case 7: treasure = EquipmentService.GetEquipmentByName("Bedroll"); break;
                case 8:
                    roll = (await _diceRoll.RequestRollAsync($"You found a weapon!", "1d4")).Roll; await Task.Yield();
                    roll = (int)Math.Ceiling(roll / 2d);
                    switch (roll)
                    {
                        case 1: itemName = "Morning Star"; break;
                        case 2: itemName = "Flail"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1));
                    break;
                case 9: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "1d100")).Roll + 40); await Task.Yield(); break;
                case 10: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "2d100")).Roll + 20); await Task.Yield(); break;
                case 11: treasure = await CreateItemAsync("Coin", 0, 1,
                    (await _diceRoll.RequestRollAsync($"You found coins!", "3d100")).Roll); await Task.Yield(); break;
                case 12: treasure = EquipmentService.GetEquipmentByName("Door Mirror"); break;
                case 13: treasure = await CreateItemAsync("Lock Picks - Dwarven", 1, 0, RandomHelper.GetRandomNumber(1, 6)); break;
                case 14: treasure = EquipmentService.GetWeaponByNameSetDurability("Elven Bow", DefaultWeaponDurability - RandomHelper.GetRandomNumber(1, 2)); break;
                case 15: treasure = EquipmentService.GetEquipmentByName("Elven Skinning Knife"); break;
                case 16: treasure = await CreateItemAsync("Ingredient - Exquisite", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Ingredient, 1)); break;
                case 17: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Extended Battle Belt", 6 - RandomHelper.GetRandomNumber(1, 4)); break;
                case 18: treasure = EquipmentService.GetEquipmentByName("Fishing Gear"); break;
                case 19: treasure = await CreateItemAsync("Gemstone", 0, RandomHelper.GetRandomNumber(3, 300)); break;
                case 20: treasure = await CreateItemAsync("Gemstone", 0, 100, RandomHelper.GetRandomNumber(1, 6)); break;
                case 21:
                    roll = (await _diceRoll.RequestRollAsync($"You found a weapon!", "1d4")).Roll; await Task.Yield();
                    roll = (int)Math.Ceiling(roll / 2d);
                    switch (roll)
                    {
                        case 1: itemName = "Battlehammer"; break;
                        case 2: itemName = "Warhammer"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1));
                    break;
                case 22: treasure = EquipmentService.GetShieldByNameSetDurability("Heater Shield", DefaultArmourDurability - RandomHelper.GetRandomNumber(1, 4)); break;
                case 23: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Iron Wedge", 6, RandomHelper.GetRandomNumber(1, 3)); break;
                case 24: treasure = EquipmentService.GetEquipmentByName("Backpack - Large"); break;
                case <= 26:
                    roll = (await _diceRoll.RequestRollAsync($"You found some leather armour!", "1d6")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Leather Cap"; break;
                        case <= 3: itemName = "Leather Vest"; break;
                        case 4: itemName = "Leather Jacket"; break;
                        case 5: itemName = "Leather Leggings"; break;
                        case 6: itemName = "Leather Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, DefaultArmourDurability - RandomHelper.GetRandomNumber(1, 4));
                    break;
                case 27: treasure = await CreateItemAsync("Lute"); break;
                case <= 29:
                    roll = (await _diceRoll.RequestRollAsync($"You found some mail armour!", "1d6")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Mail Coif"; break;
                        case 2: itemName = "Mail Shirt"; break;
                        case 3: itemName = "Sleeved Mail Shirt"; break;
                        case 4: itemName = "Mail Coat"; break;
                        case 5: itemName = "Mail Leggings"; break;
                        case 6: itemName = "Sleeved Mail Coat"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, DefaultArmourDurability - RandomHelper.GetRandomNumber(1, 4));
                    break;
                case 30: treasure = EquipmentService.GetEquipmentByName("Necklace"); if(treasure != null)treasure.Value = RandomHelper.GetRandomNumber(3, 300); break;
                case 31: treasure = await CreateItemAsync("Part - Exquisite", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Part, 1)); break;
                case 32: treasure = EquipmentService.GetEquipmentByName("Partial Map"); break;
                case <= 34: treasure = await _alchemy.GetPotionByStrengthAsync(PotionStrength.Standard); break;
                case 35: treasure = AlchemyService.GetPotionByName("Potion of Health"); break;
                case 36:
                    roll = (await _diceRoll.RequestRollAsync($"You found a ranged weapon!", "1d4")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Crossbow"; break;
                        case 2: itemName = "Longbow"; break;
                        case 3: itemName = "Crossbow Pistol"; break;
                        case 4: itemName = "Arbalest"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1));
                    break;
                case 37: treasure = await CreateItemAsync("Relic"); break;
                case 38: treasure = EquipmentService.GetEquipmentByName("Ring"); break;
                case 39: treasure = await CreateItemAsync("Scroll"); break;
                case 40: treasure = await CreateItemAsync("Scroll", 0, 100, RandomHelper.GetRandomNumber(1, 3)); break;
                case 41:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity("Silver Arrow", RandomHelper.GetRandomNumber(1, 10));
                    break;
                case 42:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity("Silver Bolt", RandomHelper.GetRandomNumber(1, 10));
                    break;
                case <= 44:
                    roll = (await _diceRoll.RequestRollAsync($"You found a silver weapon!", "1d8")).Roll; await Task.Yield();
                    int value = 0;
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
                    treasure = _weaponFactory.CreateModifiedMeleeWeapon(
                        itemName, $"Silver {itemName}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Description = EquipmentService.SilverWeaponDescription;
                            weapon.Value = value;
                            weapon.Durability = DefaultWeaponDurability - RandomHelper.GetRandomNumber(1, 3) - 1;
                            weapon.Properties[WeaponProperty.Silver] = 1;
                        });
                    break;
                case 45: treasure = EquipmentService.GetAmmoByNameSetQuantity("Superior Sling Stone", RandomHelper.GetRandomNumber(1, 10)); break;
                case 46: treasure = await _alchemy.GetPotionByStrengthAsync(PotionStrength.Supreme); break;
                case 47:
                    roll = (await _diceRoll.RequestRollAsync($"You found a weapon!", "1d4")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Shortsword"; break;
                        case 2: itemName = "Broadsword"; break;
                        case 3: itemName = "Longsword"; break;
                        case 4: itemName = "Greatsword"; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(itemName, DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1));
                    break;
                case 48: treasure = EquipmentService.GetEquipmentByName("Tobacco"); break;
                case 49: treasure = EquipmentService.GetEquipmentByName("Trap Disarming Kit"); break;
                case <= 52: treasure = await GetWonderfulTreasureAsync(); break;
                case 53: treasure = GetRandomWizardStaff(DefaultWeaponDurability - (RandomHelper.GetRandomNumber(1, 4) - 1)); break;
                case 54: treasure = EquipmentService.GetEquipmentByName("Dwarven Ale"); break;
                default:
                    treasure = await CreateItemAsync("Unknown Fine Item");
                    break;
            }
            return treasure;
        }

        public async Task<Equipment?> GetWonderfulTreasureAsync()
        {
            string itemName = "";
            int roll = RandomHelper.GetRandomNumber(1, 54);
            int defaultDurabilityDamageRoll = RandomHelper.GetRandomNumber(1, 3) - 1;
            int armourDurability = DefaultArmourDurability - defaultDurabilityDamageRoll;
            int weaponDurability = DefaultWeaponDurability - defaultDurabilityDamageRoll;
            // Console.WriteLine($"Treasure roll {roll}");

            Equipment? treasure;

            switch (roll)
            {
                case 1: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Aim Attachment", RandomHelper.GetRandomNumber(1, 3)); break;
                case 2: treasure = await CreateItemAsync("Talent Training Manual"); break;
                case 3: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Combat Harness", 6 - defaultDurabilityDamageRoll); break;
                case 4:
                    roll = (await _diceRoll.RequestRollAsync($"You found dragon scale armour!", "1d6")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case <= 3: itemName = "Dragon Scale Cap"; break;
                        case 4: itemName = "Dragon Scale Breastplate"; break;
                        case 5: itemName = "Dragon Scale Pants"; break;
                        case 6: itemName = "Dragon Scale Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, 10 - defaultDurabilityDamageRoll);
                    break;
                case 5: treasure = EquipmentService.GetEquipmentByNameSetQuantity("Superior Lock Picks", RandomHelper.GetRandomNumber(1, 6)); break;
                case 6: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Dwarven Pickaxe", 6 - defaultDurabilityDamageRoll); break;
                case 7: treasure = EquipmentService.GetWeaponByNameSetDurability("Elven Bow", DefaultWeaponDurability); break;
                case 8: treasure = await CreateItemAsync("Elven Bowstring"); break;
                case 9: treasure = AlchemyService.GetPotionByName("Potion of Restoration"); break;
                case 10: treasure = await CreateItemAsync("Relic - Epic"); break;
                case 11: treasure = EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Extended Battle Belt", 6); break;
                case 12: treasure = await CreateItemAsync("Set of Fine Clothes"); break;
                case 13: treasure = await CreateItemAsync("Flute"); break;
                case <= 15: treasure = await CreateItemAsync("Gemstone", 0, 100, RandomHelper.GetRandomNumber(1, 10)); break;
                case 16: treasure = await CreateItemAsync("Grimoire"); break;
                case 17: treasure = await CreateItemAsync("Harp"); break;
                case 18: treasure = await CreateItemAsync("Huge Backpack"); break;
                case 19: treasure = await CreateItemAsync("Ingredient - Exquisite", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Ingredient, RandomHelper.GetRandomNumber(1, 3))); break;
                case <= 21: treasure = await CreateItemAsync("Legendary"); break;
                case 22:
                    string[] itemArray = await GetMagicItemAsync("Item");
                    treasure = await CreateItemAsync("Amulet", 0, 700, 1, itemArray[1]);
                    treasure.Name = "Magic amulet of " + itemArray[0];
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.Description += " Cursed: " + itemArray[2];
                    }
                    break;
                case 23:
                    itemArray = await GetMagicItemAsync("Armour");
                    treasure = EquipmentService.GetArmourByNameSetDurability("Cloak", armourDurability);
                    treasure.Name = "Magic Cloak of " + itemArray[0];
                    treasure.Value = 300;
                    treasure.MagicEffect += itemArray[1];
                    ((Armour)treasure).Properties.Add(ArmourProperty.Magic, 0);
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.Description += " Cursed: " + itemArray[2];
                    }
                    break;
                case <= 28:
                    roll = (await _diceRoll.RequestRollAsync($"You found some magic armour!", "1d100")).Roll; await Task.Yield();
                    roll = (int)Math.Ceiling(roll / 4d);
                    itemArray = await GetMagicItemAsync("Armour");
                    int value = 0;
                    switch (roll)
                    {
                        case 1: itemName = "Leather Cap"; value = 150; break;
                        case <= 3: itemName = "Leather Vest"; value = 240; break;
                        case 4: itemName = "Leather Jacket"; value = 420; break;
                        case 5: itemName = "Leather Leggings"; value = 360; break;
                        case 6: itemName = "Leather Bracers"; value = 360; break;
                        case 7: itemName = "Mail Coif"; value = 600; break;
                        case <= 9: itemName = "Mail Shirt"; value = 1800; break;
                        case 10: itemName = "Mail Coat"; value = 2250; break;
                        case 11: itemName = "Sleeved Mail Coat"; value = 3900; break;
                        case 12: itemName = "Mail Leggings"; value = 600; break;
                        case 13: itemName = "Padded Cap"; value = 90; break;
                        case <= 15: itemName = "Padded Vest"; value = 180; break;
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
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, armourDurability);
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
                    itemArray = await GetMagicItemAsync("Item");
                    treasure = EquipmentService.GetEquipmentByName("Ring");
                    if (treasure != null)
                    {
                        treasure.Value = 700;
                        treasure.Name = "Magic ring of " + itemArray[0];
                        treasure.Description = itemArray[1];
                        if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                        {
                            treasure.Description += " Cursed: " + itemArray[2];
                        }
                    }
                    break;
                case 30:
                    roll = (await _diceRoll.RequestRollAsync($"You found a magic weapon!", "1d6")).Roll; await Task.Yield();
                    roll = (int)Math.Ceiling(roll / 2d);
                    itemArray = await GetMagicItemAsync("Weapon");
                    value = 0;
                    switch (roll)
                    {
                        case 1: itemName = "Staff"; value = 120; break;
                        case 2: itemName = "Javelin"; value = 300; break;
                        case 3: itemName = "Shortbow"; value = 600; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(itemName, armourDurability) ?? new Weapon();
                    treasure.Name = "Magic " + itemName + " of " + itemArray[0];
                    treasure.MagicEffect = itemArray[1];
                    treasure.Value = value;
                    ((Weapon)treasure).Properties.Add(WeaponProperty.Magic, 0); ;
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.MagicEffect += " Cursed: " + itemArray[2];
                    }
                    break;
                case <= 33:
                    roll = (await _diceRoll.RequestRollAsync($"You found a magic weapon!", "1d12")).Roll; await Task.Yield();
                    itemArray = await GetMagicItemAsync("Weapon");
                    value = 0;
                    switch (roll)
                    {
                        case 1: itemName = "Greatsword"; value = 600; break;
                        case 2: itemName = "Greataxe"; value = 600; break;
                        case 3: itemName = "Flail"; value = 450; break;
                        case 4: itemName = "Halberd"; value = 450; break;
                        case 5: itemName = "Battleaxe"; value = 300; break;
                        case 6: itemName = "Dagger"; value = 100; break;
                        case 7: itemName = "Shortsword"; value = 210; break;
                        case 8: itemName = "Rapier"; value = 390; break;
                        case 9: itemName = "Crossbow"; value = 750; break;
                        case 10: itemName = "Crossbow Pistol"; value = 1050; break;
                        case 11: itemName = "Elven Bow"; value = 1500; break;
                        case 12: itemName = "Longbow"; value = 300; break;
                    }
                    treasure = EquipmentService.GetWeaponByNameSetDurability(itemName, armourDurability) ?? new Weapon();
                    treasure.Name = "Magic " + itemName + " of " + itemArray[0];
                    treasure.MagicEffect = itemArray[1];
                    treasure.Value = value;
                    ((Weapon)treasure).Properties.Add(WeaponProperty.Magic, 0); ;
                    if (itemArray.Length > 2 && !string.IsNullOrEmpty(itemArray[2]))
                    {
                        treasure.MagicEffect += " Cursed: " + itemArray[2];
                    }
                    break;
                case <= 35:
                    roll = (await _diceRoll.RequestRollAsync($"You found some mail armour!", "1d6")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Mail Coif"; break;
                        case 2: itemName = "Mail Shirt"; break;
                        case 3: itemName = "Sleeved Mail Shirt"; break;
                        case 4: itemName = "Mail Coat"; break;
                        case 5: itemName = "Mail Leggings"; break;
                        case 6: itemName = "Sleeved Mail Coat"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, armourDurability);
                    break;
                case <= 38:
                    roll = (await _diceRoll.RequestRollAsync($"You found some mithril armour!", "1d10")).Roll; await Task.Yield();
                    value = 0;
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
                    treasure = _armourFactory.CreateModifiedArmour(
                        itemName, $"Mithril {itemName}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Value = value;
                            weapon.Durability = armourDurability;
                            weapon.Properties[ArmourProperty.Mithril] = 1;
                        });
                    break;
                case 39:
                    itemName = "Heater Shield";
                    treasure = _armourFactory.CreateModifiedArmour(
                        itemName, $"Mithril {itemName}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Value = 300;
                            weapon.Durability = armourDurability;
                            weapon.Properties[ArmourProperty.Mithril] = 1;
                        });
                    break;
                case <= 42:
                    roll = (await _diceRoll.RequestRollAsync($"You found a mithril weapon!", "1d12")).Roll; await Task.Yield();
                    value = 0;
                    switch (roll)
                    {
                        case 1: itemName = "Longsword"; value = 200; break;
                        case 2: itemName = "Warhammer"; value = 400; break;
                        case 3: itemName = "Battlehammer"; value = 300; break;
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
                    treasure = _weaponFactory.CreateModifiedMeleeWeapon(
                        itemName, $"Mithril {itemName}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Value = value;
                            weapon.Durability = weaponDurability;
                            weapon.Properties[WeaponProperty.Mithril] = 1;
                        });
                    break;
                case 43:
                    roll = (await _diceRoll.RequestRollAsync($"You found some night stalker armour!", "1d6")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Night Stalker Cap"; break;
                        case <= 3: itemName = "Night Stalker Vest"; break;
                        case 4: itemName = "Night Stalker Jacket"; break;
                        case 5: itemName = "Night Stalker Pants"; break;
                        case 6: itemName = "Night Stalker Bracers"; break;
                        default:
                            break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, 8 - defaultDurabilityDamageRoll);
                    break;
                case 44: treasure = await CreateItemAsync("Part - Exquisite", 0, 0, 1, await GetAlchemicalTreasureAsync(TreasureType.Part, RandomHelper.GetRandomNumber(1, 3))); break;
                case 45:
                    roll = (await _diceRoll.RequestRollAsync($"You found some plate armour!", "1d4")).Roll; await Task.Yield();
                    switch (roll)
                    {
                        case 1: itemName = "Plate Helmet"; break;
                        case 2: itemName = "Breastplate"; break;
                        case 3: itemName = "Plate Leggings"; break;
                        case 4: itemName = "Plate Bracers"; break;
                    }
                    treasure = EquipmentService.GetArmourByNameSetDurability(itemName, armourDurability);
                    break;
                case <= 47: treasure = await _alchemy.GetPotionByStrengthAsync(PotionStrength.Supreme); break;
                case <= 50: treasure = await CreateItemAsync("Power Stone", 0, 1000, RandomHelper.GetRandomNumber(1, 3)); break;
                case 51:
                    treasure = EquipmentService.GetAmmoByNameSetQuantity("Silver Arrow", RandomHelper.GetRandomNumber(1, 10));
                    break;
                case 52:
                    roll = (await _diceRoll.RequestRollAsync($"You found a silver weapon!", "1d4")).Roll; await Task.Yield();
                    value = 0;
                    switch (roll)
                    {
                        case 1: itemName = "Greatsword"; value = 300; break;
                        case 2: itemName = "Greataxe"; value = 300; break;
                        case 3: itemName = "Flail"; value = 225; break;
                        case 4: itemName = "Halberd"; value = 225; break;
                    }
                    treasure = _weaponFactory.CreateModifiedMeleeWeapon(
                        itemName, $"Silver {itemName}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Description = EquipmentService.SilverWeaponDescription;
                            weapon.Value = value;
                            weapon.Durability = weaponDurability;
                            weapon.Properties[WeaponProperty.Silver] = 1;
                        });
                    break;
                case 53: treasure = EquipmentService.GetShieldByNameSetDurability("Tower Shield", armourDurability); break;
                case 54: treasure = EquipmentService.GetArmourByNameSetDurability("Wyvern Cloak", DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 2) - 1)); break;
                default:
                    treasure = await CreateItemAsync("Unknown Wonderful Item");
                    break;
            }
            return treasure;
        }

        public async Task<string> GetAlchemicalTreasureAsync(TreasureType type, int amount, bool getOrigin = true)
        {
            List<string> items = new List<string>();
            if (type == TreasureType.Ingredient)
            {
                foreach (AlchemyItem itemName in AlchemyService.GetIngredients(amount))
                {
                    items.Add(itemName.Name);
                }
            }
            else if (type == TreasureType.Part)
            {
                foreach (AlchemyItem itemName in await _alchemy.GetPartsAsync(amount))
                {
                    items.Add(itemName.Name);
                }
            }

            return string.Join(", ", items);
        }

        public static MagicStaff GetRandomWizardStaff(int durability)
        {
            int roll = RandomHelper.GetRandomNumber(0, EquipmentService.MagicStaves.Count - 1); // Adjust for 0-indexed list
            MagicStaff newStaff = EquipmentService.MagicStaves[roll];
            newStaff.Durability = durability; // Set the specific durability for this instance

            return newStaff;
        }

        public async Task<string[]> GetRelicAsync(string type = "Standard") // as it refers to fixed data
        {
            string[] relic = new string[2];
            int roll = (await _diceRoll.RequestRollAsync($"You found a religious relic!", "1d6")).Roll; await Task.Yield();
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

        public async Task<string[]> GetPowerStoneAsync() // as it refers to fixed data
        {
            string[] stone = new string[2];
            int roll = (await _diceRoll.RequestRollAsync($"You found a power stone!", "1d20")).Roll; await Task.Yield();
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

        public async Task<string[]> GetMagicItemAsync(string type)
        {
            string[] magic = new string[3];
            int roll = (await _diceRoll.RequestRollAsync($"Roll for the magical properties.", "1d10")).Roll; await Task.Yield();
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
                            magic[1] = "Through this enchantment, the wizards who created this lends some of their skill to its user.";
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

        public async Task<string> GetLegendaryAsync() // as it refers to fixed data
        {
            string item = "";
            int roll = (await _diceRoll.RequestRollAsync($"You found a legendary item!", "1d6")).Roll; await Task.Yield();
            switch (roll)
            {
                case <= 2:
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
                case <= 4:
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
                case <= 6:
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

        public async Task<List<Equipment>> GetArmourPiecesAsync(int count, int durabilityWear = 0)
        {
            List<Equipment> items = new List<Equipment>();
            if (durabilityWear == 0)
            {
                durabilityWear = RandomHelper.GetRandomNumber(1, 5);
            }
            for (int i = 0; i < count; i++)
            {
                items.Add(await GetRandomArmourAsync(DefaultArmourDurability - durabilityWear));
            }
            return items;
        }

        public async Task<Equipment> GetRandomArmourAsync(int durability = DefaultArmourDurability)
        {
            Equipment armour;
            int roll = RandomHelper.GetRandomNumber(1, 20);
            switch (roll)
            {
                case 1: armour = EquipmentService.GetArmourByNameSetDurability("Padded Cap", durability); break;
                case 2: armour = EquipmentService.GetArmourByNameSetDurability("Padded Vest", durability); break;
                case 3: armour = EquipmentService.GetArmourByNameSetDurability("Padded Jacket", durability); break;
                case 4: armour = EquipmentService.GetArmourByNameSetDurability("Padded Pants", durability); break;
                case 5: armour = EquipmentService.GetArmourByNameSetDurability("Padded Coat", durability); break;
                case 6: armour = EquipmentService.GetArmourByNameSetDurability("Leather Cap", durability); break;
                case 7: armour = EquipmentService.GetArmourByNameSetDurability("Leather Vest", durability); break;
                case 8: armour = EquipmentService.GetArmourByNameSetDurability("Leather Jacket", durability); break;
                case 9: armour = EquipmentService.GetArmourByNameSetDurability("Leather Leggings", durability); break;
                case 10: armour = EquipmentService.GetArmourByNameSetDurability("Leather Bracers", durability); break;
                case 11: armour = EquipmentService.GetArmourByNameSetDurability("Mail Coif", durability); break;
                case 12: armour = EquipmentService.GetArmourByNameSetDurability("Mail Shirt", durability); break;
                case 13: armour = EquipmentService.GetArmourByNameSetDurability("Sleeved Mail Shirt", durability); break;
                case 14: armour = EquipmentService.GetArmourByNameSetDurability("Mail Coat", durability); break;
                case 15: armour = EquipmentService.GetArmourByNameSetDurability("Sleeved Mail Coat", durability); break;
                case 16: armour = EquipmentService.GetArmourByNameSetDurability("Mail Leggings", durability); break;
                case 17: armour = EquipmentService.GetArmourByNameSetDurability("Plate Helmet", durability); break;
                case 18: armour = EquipmentService.GetArmourByNameSetDurability("Breastplate", durability); break;
                case 19: armour = EquipmentService.GetArmourByNameSetDurability("Plate Bracers", durability); break;
                case 20: armour = EquipmentService.GetArmourByNameSetDurability("Plate Leggings", durability); break;
                default:
                    armour = await CreateItemAsync("Unknown Random Armour", durability); // Should not happen
                    break;
            }
            return armour;
        }

        public async Task<Equipment?> GetRandomWeaponAsync(int durability)
        {
            Equipment? weapon;
            int roll = RandomHelper.GetRandomNumber(1, 22);
            switch (roll)
            {
                case 1: weapon = EquipmentService.GetWeaponByNameSetDurability("Dagger", durability); break;
                case 2: weapon = EquipmentService.GetWeaponByNameSetDurability("Shortsword", durability); break;
                case 3: weapon = EquipmentService.GetWeaponByNameSetDurability("Rapier", durability); break;
                case 4: weapon = EquipmentService.GetWeaponByNameSetDurability("Broadsword", durability); break;
                case 5: weapon = EquipmentService.GetWeaponByNameSetDurability("Longsword", durability); break;
                case 6: weapon = EquipmentService.GetWeaponByNameSetDurability("Battleaxe", durability); break;
                case 7: weapon = EquipmentService.GetWeaponByNameSetDurability("Battlehammer", durability); break;
                case 8: weapon = EquipmentService.GetWeaponByNameSetDurability("Morning Star", durability); break;
                case 9: weapon = EquipmentService.GetWeaponByNameSetDurability("Flail", durability); break;
                case 10: weapon = EquipmentService.GetWeaponByNameSetDurability("Staff", durability); break;
                case 11: weapon = EquipmentService.GetWeaponByNameSetDurability("Javelin", durability); break;
                case 12: weapon = EquipmentService.GetWeaponByNameSetDurability("Greatsword", durability); break;
                case 13: weapon = EquipmentService.GetWeaponByNameSetDurability("Greataxe", durability); break;
                case 14: weapon = EquipmentService.GetWeaponByNameSetDurability("Warhammer", durability); break;
                case 15: weapon = EquipmentService.GetWeaponByNameSetDurability("Halberd", durability); break;
                case 16: weapon = EquipmentService.GetWeaponByNameSetDurability("Shortbow", durability); break;
                case 17: weapon = EquipmentService.GetWeaponByNameSetDurability("Longbow", durability); break;
                case 18: weapon = EquipmentService.GetWeaponByNameSetDurability("Elven Bow", durability); break;
                case 19: weapon = EquipmentService.GetWeaponByNameSetDurability("Crossbow Pistol", durability); break;
                case 20: weapon = EquipmentService.GetWeaponByNameSetDurability("Crossbow", durability); break;
                case 21: weapon = EquipmentService.GetWeaponByNameSetDurability("Arbalest", durability); break;
                case 22: weapon = EquipmentService.GetWeaponByNameSetDurability("Sling", durability); break;
                default:
                    weapon = await CreateItemAsync("Unknown Random Weapon", durability); // Should not happen
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
        public async Task<Equipment> CreateItemAsync(string itemName, int durability = 0, int value = 0, int quantity = 0, string itemDescription = "")
        {
            Equipment newItem;

            switch (itemName)
            {
                case "Legendary":
                    newItem = new Equipment()
                    {
                        Name = "Legendary item: " + GetLegendaryAsync(),
                        Encumbrance = 0,
                        Durability = 0, // Legendary items might not have durability
                        Value = 0,
                        Description = "" // Legendary items often have their description from the name
                    };
                    break;
                case "Relic":
                case "Relic - Epic":
                    string[] relicData = itemName == "Relic" ? await GetRelicAsync() : await GetRelicAsync("Epic");
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
                    newItem = new Equipment { Name = $"Weak Potion Recipe: {await _alchemy.GetNonStandardPotionAsync()}", Encumbrance = 0, Durability = 0, Value = 0, Description = "The actual components involved shall be chosen by the player", MaxDurability = 0 };
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
                    newItem = new Equipment { Name = $"Grimoire of {SpellService.GetRandomSpellName()}", Encumbrance = 1, Durability = 0, Value = 0, Description = "This spell can be learned back at the Wizards' Guild as long as you have the proper level", MaxDurability = 0 };
                    break;
                case "Harp":
                    newItem = new Equipment { Name = "Harp", Encumbrance = 2, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, all heroes regain an extra 1d3HP.", MaxDurability = 0 };
                    break;
                case "Lute":
                    newItem = new Equipment { Name = "Lute", Encumbrance = 5, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, to recover 1 sanity for all heroes.", MaxDurability = 0 };
                    break;
                case "Power Stone":
                    string[] stoneData = await GetPowerStoneAsync();
                    newItem = new Equipment { Name = stoneData[0], Encumbrance = 0, Durability = 0, Value = 1000, Description = stoneData[1], MaxDurability = 0 };
                    break;
                case "Set of Fine Clothes":
                    newItem = new Equipment { Name = "Set of Fine Clothes", Encumbrance = 0, Durability = 0, Value = 0, Description = "Increases Barter +5", MaxDurability = 0 };
                    break;
                case "Scroll":
                    newItem = new Equipment { Name = $"Scroll of {SpellService.GetRandomSpellName()}", Encumbrance = 0, Durability = 0, Value = 100, MaxDurability = 0 };
                    break;
                case "Talent Training Manual":
                    newItem = new Equipment { Name = $"{new PassiveAbilityService().GetRandomTalent()} Training Manual", Encumbrance = 1, Durability = 0, Value = 0, Description = "Grants the talent named on the book, when read at an inn", MaxDurability = 0 };
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