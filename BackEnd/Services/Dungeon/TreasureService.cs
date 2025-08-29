using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using RogueSharp.DiceNotation;
using System.Collections.Generic;

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
        TheMasterLocksmith,
        TheAlchemistOutlaw,
        AlchemistTable,
        Altar,
        ArcheryTarget,
        ArmourRack,
        Backpack,
        Barrels,
        Bed,
        Bedroll,
        Bookshelf,
        BookStand,
        Boxes,
        Campfire,
        Chest,
        Coffin,
        DeadAdventurer,
        DiningTable,
        Drawer,
        Fountain,
        GrateOverHole,
        Hearth,
        ObjectiveChest,
        Pottery,
        Sarcophagus,
        Statue,
        StudyTable,
        Throne,
        TortureTools,
        TreasurePile,
        DrinkWaterBasin,
        WeaponRack,
        Well,
        Special,
        DrinkFountain
    }

    public class TreasureService
    {
        private const int DefaultArmourDurability = 6;
        private const int DefaultWeaponDurability = 6;
        private readonly AlchemyService _alchemy;
        private readonly UserRequestService _diceRoll;
        private readonly WeaponFactory _weaponFactory;
        private readonly ArmourFactory _armourFactory;
        private readonly PartyManagerService _partyManager;
        private readonly PowerActivationService _powerActivation;
        private readonly Lever _lever;

        public TreasureService(
            AlchemyService alchemyService,
            UserRequestService diceRollService,
            WeaponFactory weaponFactory,
            ArmourFactory armourFactory,
            PartyManagerService partyManagerService,
            PowerActivationService powerActivationService,
            Lever lever)
        {
            _alchemy = alchemyService;
            _diceRoll = diceRollService;
            _weaponFactory = weaponFactory;
            _armourFactory = armourFactory;
            _partyManager = partyManagerService;
            _powerActivation = powerActivationService;
            _lever = lever;

            _lever.OnFoundTreasure += HandleTreasureFound;
        }

        public void Dispose()
        {
            _lever.OnFoundTreasure -= HandleTreasureFound;
        }

        private async Task<SearchResult> HandleTreasureFound(Hero hero, LeverResult result)
        {
            var searchResult = new SearchResult();
            if (result.FoundPotions)
            {
                searchResult.FoundItems = [.. await _alchemy.GetRandomPotions(3)];
            }
            else if (result.FoundWonderfulTreasure)
            {
                searchResult.FoundItems = [.. await GetWonderfulTreasureAsync( hero.IsThief ? 2 : 1)];
            }

            return searchResult;
        }

        public async Task<Equipment?> GetCoins(string coinDice, int bonusCoins)
        {
            string[] diceParts = coinDice.ToLower().Split('d');
            int.TryParse(diceParts[0], out int numberOfDice);
            int.TryParse(diceParts[1], out int diceSides);
            int maxCoinRoll = numberOfDice * diceSides;
            return await GetTreasureAsync("Coin", value: 1, amount: RandomHelper.RollDice(coinDice) + bonusCoins, maxCoinRoll: maxCoinRoll, coinDice: coinDice, bonusCoins: bonusCoins);
        }

        public async Task<Equipment> GetTreasureAsync(string itemName, int durability = 0, int value = 0, int amount = 1, string description = "", 
            int? maxCoinRoll = null, string? coinDice = null, int? bonusCoins = null)
        {
            if (_partyManager.Party != null && maxCoinRoll != null && coinDice != null)
            {
                var lootGoblins = _partyManager.Party.Heroes.Where(h => h.Perks.Any(p => p.Name == PerkName.LootGoblin));
                if (lootGoblins.Any())
                {
                    await Task.Yield();
                    foreach (var hero in lootGoblins)
                    {
                        var lootGoblin = hero.Perks.FirstOrDefault(p => p.Name == PerkName.LootGoblin);
                        if (lootGoblin == null || hero.CurrentEnergy < 1) continue;

                        if (await _diceRoll.RequestYesNoChoiceAsync($"Current Coin roll is {amount - bonusCoins ?? 0} out of a maximum of {maxCoinRoll}. " +
                            $"Does {hero.Name} wish to activate {lootGoblin.ToString()} to roll a new coin roll? The second roll will override the current role."))
                        {
                            if (await _powerActivation.ActivatePerkAsync(hero, lootGoblin))
                            {
                                amount = (await _diceRoll.RequestRollAsync("Roll for coins.", coinDice)).Roll;
                                if (bonusCoins != null) amount += (int)bonusCoins;
                            }
                        }
                    }  
                }
            }

            return await CreateItemAsync(itemName, durability, value, amount, description);
        }

        public async Task<SearchResult> SearchCorpseAsync(TreasureType type, SearchResult result)
        {
            if (result.SearchRoll == 0 || result.SearchRoll > 10)
            {
                var resultRoll = await _diceRoll.RequestRollAsync($"Roll for treasure", "1d10"); 
                await Task.Yield();
                result.SearchRoll = resultRoll.Roll;
            }
            int count = result.HeroIsThief ? 2 : 1;

            switch (type)
            {
                case TreasureType.T1:
                    switch (result.SearchRoll)
                    {
                        case 1:
                            var equipmentFound = GetRandomWeapon(DefaultWeaponDurability - RandomHelper.GetRandomNumber(1, 4));
                            if (equipmentFound != null) result.FoundItems = [await GetTreasureAsync(equipmentFound.Name, equipmentFound.Durability)];
                            break;
                        case 2:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 20)];
                            break;
                        case 3:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 10)];
                            break;
                        case 4:
                            result.FoundItems = [await GetTreasureAsync("Bandage (old rags)", 1)];
                            break;
                        default:
                            result.Message = "You found nothing bu scrap.";
                            break;
                    }
                    break;
                case TreasureType.T2:
                    switch (result.SearchRoll)
                    {
                        case 1:
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            break;
                        case 2:
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)];
                            break;
                        case 3:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 50)];
                            break;
                        case 4:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 40)];
                            break;
                        case 5:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 20)];
                            break;
                        default:
                            result.Message = "You found nothing bu scrap.";
                            break;
                    }
                    break;
                case TreasureType.T3:
                    switch (result.SearchRoll)
                    {
                        case 1:
                        case 2:
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            break;
                        case 3:
                        case 4:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 100)];
                            break;
                        case 5:
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)];
                            break;
                        case 6:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 80)];
                            break;
                        case 7:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 60)];
                            break;
                        default:
                            result.Message = "You found nothing bu scrap.";
                            break;
                    }
                    break;
                case TreasureType.T4:
                    switch (result.SearchRoll)
                    {
                        case 1:
                            result.FoundItems = [await GetTreasureAsync("Grimoire")];
                            break;
                        case 2:
                        case 3:
                            result.FoundItems = [await GetTreasureAsync("Scroll")];
                            break;
                        case 4:
                            result.FoundItems = [.. await _alchemy.GetRandomPotions(RandomHelper.RollDie(DiceType.D2))];
                            break;
                        case 5:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 150)];
                            break;
                        case 6:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 100)];
                            break;
                        default:
                            result.Message = "You found nothing bu scrap.";
                            break;
                    }
                    break;
                case TreasureType.T5:
                    switch (result.SearchRoll)
                    {
                        case <= 2:
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Wonderful, count)];
                            count *= 2;
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            break;
                        case <= 4:
                            count *= 2;
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.FoundItems = [await GetTreasureAsync("Grimoire")];
                            break;
                        case <= 7:
                            count *= 3;
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            break;
                        case  <= 10:
                            result.FoundItems = [await GetTreasureAsync("Coin", 0, 1, 500)];
                            break;
                        default:
                            break;
                    }
                    break;
                case TreasureType.Part:
                    result.Message = "Monster does not have any loot, but can be harvested for alchemical parts.";
                    break;
                case TreasureType.Turog:
                    result.FoundItems = [await GetCoins("2d100", 0)];
                    var item = EquipmentService.GetWeaponByName("The Goblin Scimitar");
                    if (item != null) result.FoundItems = [item];
                    break;
                case TreasureType.TheMasterLocksmith:
                    var search = await SearchCorpseAsync(TreasureType.T5, result);
                    if (search.FoundItems != null)
                    {
                        foreach (var reward in search.FoundItems)
                        {
                            if (reward != null)
                            {
                                result.FoundItems = [reward];
                            }
                        } 
                    }
                    item = EquipmentService.GetWeaponByName("The Flames of Zul");
                    if (item != null) result.FoundItems = [item];
                    break;
                case TreasureType.TheAlchemistOutlaw:
                    result.FoundItems = [.. await _alchemy.GetRandomPotions(4, PotionStrength.Standard)]; 
                    result.FoundItems = [.. new List<Weapon>() {_weaponFactory.CreateModifiedRangedWeapon(
                        "Crossbow Pistol", "Poisonous Crossbow Pistol",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        }),
                        _weaponFactory.CreateModifiedMeleeWeapon(
                        "Dagger", "Poisonous Dagger",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        }) }];
                    result.FoundItems = [new AlchemicalRecipe
                        {
                            Name = $"Black Acathus gas Recipe",
                            Strength = PotionStrength.Standard,
                            Components = new List<AlchemyItem>() { new Ingredient() { Name = IngredientName.BlackAcathusLeaf } }
                        }];
                    result.FoundItems = [new Ingredient() { Name = IngredientName.BlackAcathusLeaf, Quantity = 4 }];
                    break;
                default:
                    result.Message = "Invalid search.";
                    result.WasSuccessful = false;
                    break;
            }
            result.FoundItems = result.FoundItems;
            return result;
        }

        public async Task<List<Equipment>> FoundTreasureAsync(TreasureType type, int count)
        {
            var rewards = new List<Equipment?>();
            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case TreasureType.Mundane: rewards.AddRange(await GetMundaneTreasureAsync(count)); break;
                    case TreasureType.Fine: rewards.AddRange(await GetFineTreasureAsync(count)); break;
                    case TreasureType.Wonderful: rewards.AddRange(await GetWonderfulTreasureAsync(count)); break;
                }
            }

            if (count > 1)
            {
                // Thief's Luck: Multiple items are found, but the thief must pick one to keep
                List<string> listofChoices = rewards.Where(r => r != null).Select(r => r!.Name).ToList();
                var choiceResult = await _diceRoll.RequestChoiceAsync("Choose one item to keep", listofChoices); 
                await Task.Yield();
                return new List<Equipment>() { rewards.First(r => r != null && r.Name == choiceResult.SelectedOption)!.Clone() };
            }

            return rewards.Cast<Equipment>().ToList();
        }

        public async Task<List<Equipment?>> GetMundaneTreasureAsync(int count = 1)
        {
            int roll = RandomHelper.GetRandomNumber(1, 55);
            int defaultDurabilityDamageRoll = RandomHelper.GetRandomNumber(1, 4) + 1;
            int armourDurability = DefaultArmourDurability - defaultDurabilityDamageRoll;
            int weaponDurability = DefaultWeaponDurability - defaultDurabilityDamageRoll;

            var mundaneRewards = new List<Equipment?>();

            switch (roll)
            {
                case 1: mundaneRewards.Add(await CreateItemAsync("Amulet", value: 100)); break;
                case 2: mundaneRewards.Add(EquipmentService.GetAmmoByNameSetQuantity("Arrow", 10)); break;
                case 3: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Bandage (old rags)", RandomHelper.RollDie(DiceType.D3))); break;
                case 4: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Bandage (old rags)", RandomHelper.GetRandomNumber(1, 6))); break;
                case 5: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Bandage (linen)", RandomHelper.GetRandomNumber(1, 2))); break;
                case 6: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Bear Trap", 2 - (RandomHelper.GetRandomNumber(1, 2) - 1), RandomHelper.RollDie(DiceType.D3))); break;
                case 7: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Beef Jerky", RandomHelper.GetRandomNumber(1, 4))); break;
                case 8: mundaneRewards.Add(EquipmentService.GetAmmoByNameSetQuantity("Bolt", 5)); break;
                case 9: mundaneRewards.Add(EquipmentService.GetShieldByNameSetDurability("Buckler", armourDurability)); break;
                case 10: mundaneRewards.Add(EquipmentService.GetArmourByNameSetDurability("Cloak", armourDurability)); break;
                case 11: mundaneRewards.Add(await GetCoins("2d20", 0)); break;
                case 12: mundaneRewards.Add(await GetCoins("4d20", 0)); break;
                case 13: mundaneRewards.Add(await GetCoins("1d100", 0)); await Task.Yield(); break;
                case 14: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Crowbar", 6 - defaultDurabilityDamageRoll, RandomHelper.GetRandomNumber(1, 6))); break;
                case 15: mundaneRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Dagger", weaponDurability)); break;
                case 16: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Empty Bottle", RandomHelper.GetRandomNumber(1, 6))); break;
                case 17: mundaneRewards.AddRange(await FoundTreasureAsync(TreasureType.Fine, count)); break;
                case 18: mundaneRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Javelin", weaponDurability)); break;
                case 19: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Lamp Oil")); break;
                case 20: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Lantern")); break;
                case 21: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Lock Picks", RandomHelper.GetRandomNumber(1, 4))); break;
                case 22: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Backpack - Medium")); break;
                case 23: mundaneRewards.Add((await GetAlchemicalTreasureAsync(TreasureType.Part, 1))[0]); break;
                case 24: mundaneRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Rapier", weaponDurability)); break;
                case 25: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Ration", RandomHelper.GetRandomNumber(1, 4))); break;
                case 26: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Ration", RandomHelper.GetRandomNumber(1, 6))); break;
                case 27: mundaneRewards.Add(await CreateItemAsync("Potion Recipe - Weak")); break;
                case 28: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Rope (old)")); break;
                case 29: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Rope")); break;
                case 30: mundaneRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Shortbow", weaponDurability)); break;
                case 31: mundaneRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Shortsword", weaponDurability)); break;
                case 32: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Skinning Knife", RandomHelper.RollDie(DiceType.D3))); break;
                case 33: mundaneRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Sling", weaponDurability)); break;
                case 34: mundaneRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Staff", weaponDurability)); break;
                case 35: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Whetstone")); break;
                case 36: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Wild game traps")); break;
                case 37: mundaneRewards.Add(await CreateItemAsync("Wolf Pelt", quantity: RandomHelper.RollDie(DiceType.D3))); break;
                case 38: mundaneRewards.AddRange(await FoundTreasureAsync(TreasureType.Wonderful, count)); break;
                case <= 40: mundaneRewards.Add(EquipmentService.GetAmmoByNameSetQuantity("Arrow", 5)); break;
                case <= 42: mundaneRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Bedroll", RandomHelper.RollDie(DiceType.D3))); break;
                case <= 44: mundaneRewards.Add(await CreateItemAsync("Ring", value: 150)); break;
                case <= 46: mundaneRewards.Add(await GetCoins("1d20", 0)); break;
                case <= 48: mundaneRewards.Add(await GetCoins("3d20", 0)); break;
                case <= 50: mundaneRewards.AddRange(await GetAlchemicalTreasureAsync(TreasureType.Ingredient, RandomHelper.RollDie(DiceType.D3))); break;
                case <= 53: mundaneRewards.Add(EquipmentService.GetEquipmentByName("Torch")); break;
                case 54: mundaneRewards.Add(GetPaddedArmour(armourDurability)); break;
                case 55: mundaneRewards.Add(GetLeatherArmour(armourDurability)); break;
            }
            
            mundaneRewards ??= new List<Equipment?>();
            for (int i = 0; i < mundaneRewards.Count; i++)
            {
                var currentReward = mundaneRewards[i];
                mundaneRewards[i] = currentReward != null ? currentReward.Clone() : null;
            }
            return mundaneRewards;
        }

        public async Task<List<Equipment?>> GetFineTreasureAsync(int count = 1)
        {
            _partyManager.UpdateMorale(changeEvent: MoraleChangeEvent.FineTreasure);
            int roll = RandomHelper.GetRandomNumber(1, 55);
            var equipmentDurability = 6 - RandomHelper.RollDie(DiceType.D4);
            var armourDurability = DefaultArmourDurability - RandomHelper.RollDie(DiceType.D4);
            var weaponDurability = DefaultWeaponDurability - (RandomHelper.RollDie(DiceType.D4) - 1);

            var fineRewards = new List<Equipment?>();

            switch (roll)
            {
                case 1: fineRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Alchemist Tool", equipmentDurability)); break;
                case 2: fineRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Alchemist Belt", equipmentDurability)); break;
                case 3: fineRewards.Add(EquipmentService.GetEquipmentByName("Armour Repair Kit")); break;
                case <= 5:
                    var choiceResult = await _diceRoll.RequestChoiceAsync("Choose ammo type", new List<string>() { "Barbed Arrow", "Barbed Bolt", "Superior Sling Stone" }); await Task.Yield();
                    fineRewards.Add(EquipmentService.GetAmmoByNameSetQuantity(choiceResult.SelectedOption, 5)); break;
                case 6: fineRewards.Add(EquipmentService.GetEquipmentByName("Bedroll")); break;
                case 7: fineRewards.Add(await GetCoins("1d100", 40)); break;
                case 8: fineRewards.Add(await GetCoins("2d100", 20)); break;
                case 9: fineRewards.Add(await GetCoins("3d100", 0)); break;
                case 10: fineRewards.Add(EquipmentService.GetEquipmentByName("Door Mirror")); break;
                case 11: fineRewards.Add(EquipmentService.GetEquipmentByName("Dwarven Ale")); break;
                case 12: fineRewards.Add(await CreateItemAsync("Lock Picks - Dwarven", 1, 0, RandomHelper.RollDie(DiceType.D6))); break;
                case 13: fineRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Elven Bow", DefaultWeaponDurability - RandomHelper.RollDie(DiceType.D2))); break;
                case 14: fineRewards.Add(EquipmentService.GetEquipmentByName("Elven Skinning Knife")); break;
                case 15: var ingredient = (await GetAlchemicalTreasureAsync(TreasureType.Ingredient, 1))[0]; 
                    ((Ingredient)ingredient).Exquisite = true; fineRewards.Add(ingredient); break;
                case 16: fineRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Extended Battle Belt", equipmentDurability)); break;
                case 17: fineRewards.Add(EquipmentService.GetEquipmentByName("Fishing Gear")); break;
                case 18: fineRewards.Add(await CreateItemAsync("Gemstone", value: RandomHelper.RollDice("3d100"))); break;
                case 19: fineRewards.Add(await CreateItemAsync("Gemstone", value: 100, quantity: RandomHelper.RollDie(DiceType.D6))); break;
                case 20: fineRewards.Add(EquipmentService.GetShieldByNameSetDurability("Heater Shield", armourDurability)); break;
                case 21: fineRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Iron Wedge", 1, RandomHelper.RollDie(DiceType.D3))); break;
                case 22: fineRewards.Add(EquipmentService.GetEquipmentByName("Backpack - Large")); break;
                case 23: fineRewards.Add(await CreateItemAsync("Lute")); break;
                case 24: fineRewards.Add(await CreateItemAsync("Amulet", value: RandomHelper.RollDice("3d100"))); break;
                case 25: var part = (await GetAlchemicalTreasureAsync(TreasureType.Part, 1))[0]; 
                    ((Part)part).Exquisite = true; fineRewards.Add(part); break;
                case 26: fineRewards.Add(EquipmentService.GetEquipmentByName("Partial Map")); break;
                case 27: var potion = AlchemyService.GetPotionByNameStrength("Potion of Health", PotionStrength.Standard); 
                    potion.Identified = false; fineRewards.Add(potion); break;
                case 28: fineRewards.Add(await CreateItemAsync("Relic", value: 350)); break;
                case 29: fineRewards.Add(await CreateItemAsync("Ring", value: 150)); break;
                case 30: fineRewards.Add(await CreateItemAsync("Scroll")); break;
                case 31: fineRewards.Add(await CreateItemAsync("Scroll", quantity: RandomHelper.RollDie(DiceType.D3))); break;
                case 32: potion = await _alchemy.GetPotionByStrengthAsync(PotionStrength.Supreme); 
                    potion.Identified = false; fineRewards.Add(potion);  break;
                case 33: fineRewards.Add(EquipmentService.GetEquipmentByName("Tobacco")); break;
                case 34: fineRewards.Add(EquipmentService.GetEquipmentByName("Trap Disarming Kit")); break;
                case 35: fineRewards.Add(GetRandomWizardStaff(weaponDurability)); break;
                case <= 38: choiceResult = await _diceRoll.RequestChoiceAsync("Choose ammo type", new List<string>() { "Silver Arrow", "Silver Bolt", "Superior Sling Stone" }); await Task.Yield();
                    fineRewards.Add(EquipmentService.GetAmmoByNameSetQuantity(choiceResult.SelectedOption, RandomHelper.RollDie(DiceType.D10))); break;
                case <= 41: fineRewards.AddRange(await FoundTreasureAsync(TreasureType.Wonderful, count)); break;
                case <= 43: fineRewards.AddRange(await _alchemy.GetRandomPotions(1)); break;
                case <= 49:
                    var weapon = new Weapon();
                    do
                    {
                        weapon = GetRandomWeapon(weaponDurability); 
                    } while (weapon == null || new List<string>() { "Sling", "Shortbow", "Dagger", "Rapier", "Staff", "Javelin" }.Contains(weapon.Name));
                    fineRewards.Add(weapon);
                    break;
                case <= 51: fineRewards.Add(GetLeatherArmour(armourDurability)); break;
                case <= 53: fineRewards.Add(GetMailArmour(armourDurability)); break;
                case <= 55:
                    roll = (await _diceRoll.RequestRollAsync($"You found a silver weapon!", "1d8")).Roll; await Task.Yield();
                    int value = 0;
                    string itemName = "";
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
                    fineRewards.Add(_weaponFactory.CreateModifiedMeleeWeapon(
                        itemName, $"Silver {itemName}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Description = EquipmentService.SilverWeaponDescription;
                            weapon.Value = value;
                            weapon.Durability = DefaultWeaponDurability - RandomHelper.RollDie(DiceType.D3) - 1;
                            weapon.Properties.Add(WeaponProperty.Silver, 1);
                        }));
                    break;
            }

            fineRewards ??= new List<Equipment?>();
            for (int i = 0; i < fineRewards.Count; i++)
            {
                var currentReward = fineRewards[i];
                fineRewards[i] = currentReward != null ? currentReward.Clone() : null;
            }
            return fineRewards;
        }

        public async Task<List<Equipment?>> GetWonderfulTreasureAsync(int count = 1)
        {
            _partyManager.UpdateMorale(changeEvent: MoraleChangeEvent.WonderfulTreasure);
            int roll = RandomHelper.GetRandomNumber(1, 54);
            int defaultDurabilityDamageRoll = RandomHelper.RollDie(DiceType.D3) - 1;
            int armourDurability = DefaultArmourDurability - defaultDurabilityDamageRoll;
            int weaponDurability = DefaultWeaponDurability - defaultDurabilityDamageRoll;
            // Console.WriteLine($"Treasure roll {roll}");

            var wonderfulRewards = new List<Equipment?>();

            switch (roll)
            {
                case 1: wonderfulRewards.Add(EquipmentService.GetEquipmentByName("Aim Attachment")); break;
                case 2: wonderfulRewards.Add(await CreateItemAsync("Talent Training Manual")); break;
                case 3: wonderfulRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Combat Harness", 6 - defaultDurabilityDamageRoll)); break;
                case 4: wonderfulRewards.Add(EquipmentService.GetEquipmentByNameSetQuantity("Superior Lock Picks", RandomHelper.GetRandomNumber(1, 6))); break;
                case 5: wonderfulRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Dwarven Pickaxe", 6 - defaultDurabilityDamageRoll)); break;
                case 6: wonderfulRewards.Add(await CreateItemAsync("Elven Bowstring")); break;
                case 7: wonderfulRewards.Add(await CreateItemAsync("Relic - Epic")); break;
                case 8: wonderfulRewards.Add(EquipmentService.GetWeaponByNameSetDurability("Elven Bow", DefaultWeaponDurability)); break;
                case 9: var potion = AlchemyService.GetPotionByName("Potion of Restoration"); potion.Identified = false; wonderfulRewards.Add(potion); break;
                case 10: wonderfulRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Extended Battle Belt", 6)); break;
                case 11: wonderfulRewards.Add(await CreateItemAsync("Set of Fine Clothes")); break;
                case 12: wonderfulRewards.Add(await CreateItemAsync("Flute")); break;
                case <= 14: wonderfulRewards.Add(await CreateItemAsync("Gemstone", 0, 100, RandomHelper.RollDie(DiceType.D10))); break;
                case 15: wonderfulRewards.Add(await CreateItemAsync("Grimoire")); break;
                case 16: wonderfulRewards.Add(await CreateItemAsync("Harp")); break;
                case 17: wonderfulRewards.Add(await CreateItemAsync("Backpack - Huge")); break;
                case 18: wonderfulRewards.Add(new Potion()
                    {
                    Shop = ShopCategory.Potions, Category = "Treasure", Name = "Potion of Resurrection", Strength = PotionStrength.Supreme, Value = 300,
                    EffectDescription = "If poured ona dead or knocked out hero, the hero is resurrected with full health. Any disease or poison is cured as well",
                    PotionProperties = new Dictionary<PotionProperty, int>() { { PotionProperty.CureDisease, 100 }, { PotionProperty.CurePoison, 100 }, { PotionProperty.HealHP, 999 } }}); break;
                case 19: 
                    var ingredients = await GetAlchemicalTreasureAsync(TreasureType.Ingredient, RandomHelper.RollDie(DiceType.D3)); 
                    foreach (var ingredient in ingredients)
                    {
                        ((Ingredient)ingredient).Exquisite = true;
                        wonderfulRewards.Add(ingredient);
                    }
                    break;
                case 20:
                    var parts = await GetAlchemicalTreasureAsync(TreasureType.Part, RandomHelper.RollDie(DiceType.D3));
                    foreach (var part in parts)
                    {
                        ((Part)part).Exquisite = true;
                        wonderfulRewards.Add(part);
                    }
                    break;
                case <= 21: wonderfulRewards.Add(await CreateItemAsync("Legendary")); break;
                case <= 23: potion = await _alchemy.GetPotionByStrengthAsync(PotionStrength.Supreme); potion.Identified = false; wonderfulRewards.Add(potion); break;
                case <= 26: wonderfulRewards.Add(await CreateItemAsync("Power Stone", 0, 1000, RandomHelper.RollDie(DiceType.D3))); break;
                case 27:
                    var choiceResult = await _diceRoll.RequestChoiceAsync("Choose ammo type", new List<string>() { "Silver Arrow", "Silver Bolt", "Superior Sling Stone" }); await Task.Yield();
                    wonderfulRewards.Add(EquipmentService.GetAmmoByNameSetQuantity(choiceResult.SelectedOption, RandomHelper.RollDie(DiceType.D10))); break;
                case 28: wonderfulRewards.Add(EquipmentService.GetEquipmentByNameSetDurabilitySetQuantity("Superior Trap Disarming Kit", 6 - RandomHelper.RollDie(DiceType.D3))); break;
                case 29: wonderfulRewards.Add(EquipmentService.GetShieldByNameSetDurability("Tower Shield", armourDurability)); break;
                case 30: wonderfulRewards.Add(await GetDragonScaleArmourAsync(10 - defaultDurabilityDamageRoll)); break;
                case 31: wonderfulRewards.Add(await GetNightstalkerArmourAsync(8 - defaultDurabilityDamageRoll)); break;
                case 32: wonderfulRewards.Add(EquipmentService.GetArmourByNameSetDurability("Wyvern Cloak", DefaultArmourDurability - (RandomHelper.GetRandomNumber(1, 2) - 1))); break;
                case 33:
                    var meleeWeapon = GetGreatWeapon(weaponDurability) ?? new MeleeWeapon();
                    wonderfulRewards.Add(_weaponFactory.CreateModifiedMeleeWeapon(
                        meleeWeapon.Name, $"Silver {meleeWeapon.Name}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Description = EquipmentService.SilverWeaponDescription;
                            weapon.Value = (int)Math.Ceiling(meleeWeapon.Value * 1.5);
                            weapon.Durability = weaponDurability;
                            weapon.Properties.Add(WeaponProperty.Silver, 1);
                        }));
                    break;
                case <= 35: wonderfulRewards.Add(GetMailArmour(armourDurability)); break;
                case 36: wonderfulRewards.Add(GetPlateArmour(armourDurability)); break;
                case <= 38:
                    var armour = GetMailArmour(armourDurability) ?? new Armour();
                    wonderfulRewards.Add(_armourFactory.CreateModifiedArmour(
                        armour.Name, $"Mithril {armour.Name}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Value = armour.Value * 2;
                            weapon.Durability = armourDurability;
                            weapon.Properties[ArmourProperty.Mithril] = 1;
                        }));
                    break;
                case 39:
                    armour = GetPlateArmour(armourDurability) ?? new Armour();
                    wonderfulRewards.Add(_armourFactory.CreateModifiedArmour(
                        armour.Name, $"Mithril {armour.Name}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Value = armour.Value * 2;
                            weapon.Durability = armourDurability;
                            weapon.Properties[ArmourProperty.Mithril] = 1;
                        }));
                    break;
                case <= 42:
                    var weapon = GetRandomWeapon(weaponDurability);
                    while (!(weapon is MeleeWeapon) || weapon.Name == "Staff" || weapon.Name == "Javelin")
                    {
                        weapon = GetRandomWeapon(weaponDurability);
                    }
                    meleeWeapon = weapon != null ? (MeleeWeapon)weapon : new MeleeWeapon();
                    wonderfulRewards.Add(_weaponFactory.CreateModifiedMeleeWeapon(
                        meleeWeapon.Name, $"Mithril {meleeWeapon.Name}",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Value = meleeWeapon.Value * 2;
                            weapon.Durability = weaponDurability;
                            weapon.Properties[WeaponProperty.Mithril] = 1;
                        }));
                    break;
                case 43:
                    wonderfulRewards.Add(_armourFactory.CreateModifiedArmour(
                        "Heater Shield", $"Mithril Heater Shield",
                        weapon =>
                        {
                            weapon.Category = "Treasure";
                            weapon.Value = 300;
                            weapon.Durability = armourDurability;
                            weapon.Properties[ArmourProperty.Mithril] = 1;
                        }));
                    break;
                case 44:
                    wonderfulRewards.Add(await GetMagicItemAsync(await CreateItemAsync("Amulet", 0, 700, 1))); break;
                case 45:
                    wonderfulRewards.Add(await GetMagicItemAsync(await CreateItemAsync("Ring", 0, 700, 1))); break;
                case 46: var item = await GetMagicItemAsync(EquipmentService.GetArmourByNameSetDurability("Cloak", armourDurability));break;
                case 47: var shields = EquipmentService.GetShields(); shields.Shuffle(); 
                    wonderfulRewards.Add(await GetMagicItemAsync(EquipmentService.GetShieldByNameSetDurability(shields[0].Name, armourDurability))); break;
                case <= 51: wonderfulRewards.Add(await GetMagicItemAsync(GetRandomArmour(armourDurability))); break;
                case <= 55: wonderfulRewards.Add(await GetMagicItemAsync(GetRandomWeapon(weaponDurability))); break;
            }

            wonderfulRewards ??= new List<Equipment?>();
            for (int i = 0; i < wonderfulRewards.Count; i++)
            {
                var currentReward = wonderfulRewards[i];
                wonderfulRewards[i] = currentReward != null ? currentReward.Clone() : null;
            }
            return wonderfulRewards;
        }

        public async Task<List<AlchemyItem>> GetAlchemicalTreasureAsync(TreasureType type, int amount, bool getOrigin = true)
        {
            if (type == TreasureType.Ingredient)
            {
                var list = new List<Ingredient>();
                foreach (Ingredient itemName in AlchemyService.GetIngredients(amount))
                {
                    list.Add(itemName);
                }
                return list.Cast<AlchemyItem>().ToList();
            }
            else
            {
                var items = new List<Part>();
                foreach (Part itemName in await _alchemy.GetPartsAsync(amount))
                {
                    items.Add(itemName);
                }
                return items.Cast<AlchemyItem>().ToList();
            }
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

        public async Task<Equipment> GetPowerStoneAsync()
        {
            var stone = new PowerStone();
            int roll = (await _diceRoll.RequestRollAsync($"You found a power stone!", "1d20")).Roll; await Task.Yield();
            switch (roll)
            {
                case 1: 
                    stone.Name = "Power stone of DMG +2"; stone.Description = "This stone can be used to improve any weapon.";
                    stone.WeaponProperties = new() { { WeaponProperty.DamageBonus, 2 } }; stone.ItemToEnchant = PowerStoneEffectItem.Weapon;
                    break;
                case 2: stone.Name = "Power stone of DMG +1"; stone.Description = "This stone can be used to improve any weapon.";
                    stone.WeaponProperties = new() { { WeaponProperty.DamageBonus, 1 } }; stone.ItemToEnchant = PowerStoneEffectItem.Weapon;
                    break;
                case 3: stone.Name = "Power stone of Poisonous"; stone.Description = "This stone gives any weapon a permanent poison ability.";
                    stone.WeaponProperties = new() { { WeaponProperty.Poisoned, 1 } }; stone.ItemToEnchant = PowerStoneEffectItem.Weapon;
                    break;
                case 4: stone.Name = "Power stone of Fire Damage"; stone.Description = "This stone makes the weapon cause fire damage.";
                    stone.WeaponProperties = new() { { WeaponProperty.FireDamage, 0 } }; stone.ItemToEnchant = PowerStoneEffectItem.Weapon;
                    break;
                case 5: stone.Name = "Power stone of ToHit +10"; stone.Description = "This stone will make a weapon perfectly balanced, increasing chance to hit.";
                    stone.ItemToEnchant = PowerStoneEffectItem.Weapon;
                    break;
                case 6: stone.Name = "Power stone of Strength +5"; stone.Description = "This stone can be used on rings or amulets, enhancing the bearers stat.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Strength, 5))];
                    break;
                case 7: stone.Name = "Power stone of Constitution +5"; stone.Description = "This stone can be used on rings or amulets, enhancing the bearers stat.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Constitution, 5))];
                    break;
                case 8: stone.Name = "Power stone of Wisdom +5"; stone.Description = "This stone can be used on rings or amulets, enhancing the bearers stat.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Wisdom, 5))];
                    break;
                case 9: stone.Name = "Power stone of Resolve +5"; stone.Description = "This stone can be used on rings or amulets, enhancing the bearers stat.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Resolve, 5))];
                    break;
                case 10: stone.Name = "Power stone of Dexterity +5"; stone.Description = "This stone can be used on rings or amulets, enhancing the bearers stat.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Dexterity, 5))];
                    break;
                case 11: stone.Name = "Power stone of Fast Reload"; stone.Description = "Reduces reload time by 1 AP. If reload is 0 AP then the hero may only attack twice the first action, but only once the second action.";
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.FastReload, -1)]; stone.ItemToEnchant = PowerStoneEffectItem.Weapon;
                    break;
                case 12: stone.Name = "Power stone of Def +2"; stone.Description = "This stone can be used to enhance a piece of armour or a shield.";
                    stone.DefenseBonus = 2; stone.ItemToEnchant = PowerStoneEffectItem.ArmourShield;
                    break;
                case 13: stone.Name = "Power stone of Energy +1 per quest"; stone.Description = "This stone can be used on rings or amulets.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Energy, 1))];
                    break;
                case 14: stone.Name = "Power stone of Luck +1"; stone.Description = "This stone can be used on rings or amulets.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Luck, 1))];
                    break;
                case 15: stone.Name = "Power stone of Detectection +10"; stone.Description = "This stone can be used on rings or amulets.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Perception, 10))];
                    break;
                case 16: stone.Name = "Power stone of Initiative"; stone.Description = "This stone can be used on rings or amulets. Subtract 5 from the enemies DEX.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.Initiative, -1)];
                    break;
                case 17: stone.Name = "Power stone of resist Fear/Terror"; stone.Description = "This stone can be used on rings or amulets.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ResistFearTerror, -1)];
                    break;
                case 18: stone.Name = "Power stone of HP +2"; stone.Description = "This stone can be used on rings or amulets, enhancing the bearers max HP.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.HitPoints, 2))];
                    break;
                case 19: stone.Name = "Power stone of Party Morale +2"; stone.Description = "This stone can be used on rings or amulets, enhancing the party's starting morale on each quest.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.StoneOfPartyMorale, -1)];
                    break;
                case >= 20: stone.Name = "Power stone of Sanity +2"; stone.Description = "This stone can be used on rings or amulets, enhancing the bearers stat.";
                    stone.ItemToEnchant = PowerStoneEffectItem.RingAmulet;
                    stone.ActiveStatusEffects = [new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Sanity, 2))];
                    break;
            }
            return stone;
        }

        public async Task<Equipment> GetMagicItemAsync(Equipment? item, bool isCursed = false, bool includeCurseRoll = true)
        {
            if (item == null) return new Equipment();
            string[] magic = new string[3];
            item = item.Clone();
            int roll = (await _diceRoll.RequestRollAsync($"Roll for the magical properties.", "1d10")).Roll; 
            await Task.Yield();
            if (isCursed)
            {
                item = ApplyCurseToItem(item);
                roll = RandomHelper.GetRandomNumber(1, 9); // Re-roll for positive effect if cursed, to fill magic[0] and magic[1]
            }
            item.ActiveStatusEffects ??= new List<ActiveStatusEffect>();

            if (item is Weapon weapon)
            {
                weapon.ActiveStatusEffects ??= new List<ActiveStatusEffect>();
                switch (roll)
                {
                    case 1: magic[0] = "Fire DMG"; magic[1] = "This weapon has a slight glow to it, as if radiating heat.";
                        weapon.WeaponCoating = new WeaponCoating() { DamageType = DamageType.Fire, RemoveAfterCombat = false };
                        break;
                    case 2: magic[0] = "DMG +2"; magic[1] = "When wielding this weapon, is is as if the hero can feel its taste for blood.";
                        weapon.Properties.TryGetValue(WeaponProperty.DamageBonus, out int damageBonus);
                        if (damageBonus > 0) weapon.Properties.Remove(WeaponProperty.DamageBonus);
                        damageBonus += 2;
                        weapon.Properties.TryAdd(WeaponProperty.DamageBonus, damageBonus);
                        break;
                    case 3: magic[0] = "DMG +1"; magic[1] = "A slight humming sound can be heard from this weapon, as if it powered up and ready to kill.";
                        weapon.Properties.TryGetValue(WeaponProperty.DamageBonus, out damageBonus);
                        if (damageBonus > 0) weapon.Properties.Remove(WeaponProperty.DamageBonus);
                        damageBonus += 1;
                        weapon.Properties.TryAdd(WeaponProperty.DamageBonus, damageBonus);
                        break;
                    case 4: magic[0] = "CS/RS +10"; magic[1] = "This weapon feels as if it is one with your body, making every move as simple and perfect as they can be.";
                        if (weapon is MeleeWeapon) weapon.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.CombatSkill, 10)));
                        else weapon.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.RangedSkill, 10)));
                        break;
                    case 5: magic[0] = "CS/RS +5"; magic[1] = "The magic infused into this weapon causes every strike or shot to be all but perfect.";
                        if (weapon is MeleeWeapon) weapon.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.CombatSkill, 5)));
                        else weapon.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.RangedSkill, 5)));
                        break;
                    case 6: magic[0] = "ENC -5 and Class -1 (min 1)"; magic[1] = "The low weight of this weapon goes beyond your heroes' understanding. A weapon half the size would not even weigh this little.";
                        weapon.Encumbrance -= 5;
                        if (weapon.Class > 1) weapon.Class -= 1;
                        break;
                    case 7: magic[0] = "DUR +2 (total 10)"; magic[1] = "It is as if the magic in this weapon is unusually strong, binding it together in a way that your  hero has never experienced before.";
                        weapon.MaxDurability += 2;
                        break;
                    case 8: magic[0] = "Fear/Terror test +10"; magic[1] = "One look at this weapon reassures your hero that nothing can stand between them and victory.";
                        weapon.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ResistFearTerror, -1));
                        break;
                    case 9: magic[0] = "+5 Parry for CC, +5 Dodge for ranged weapon"; magic[1] = "Imbued with powerful spells, this weapon is designed to protect its owner.";
                        if (weapon is MeleeWeapon) weapon.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Parry, 5)));
                        else weapon.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Dodge, 5)));
                        break;
                    case 10: return includeCurseRoll ? await GetMagicItemAsync(item, isCursed: true) : await GetMagicItemAsync(item, includeCurseRoll: false);
                }
                weapon.Properties.Add(WeaponProperty.Magic, 0);
            }
            else if (item is Armour armour)
            {
                armour.ActiveStatusEffects ??= new List<ActiveStatusEffect>();
                switch (roll)
                {
                    case 1: magic[0] = "DEF +2"; magic[1] = "This piece of armor simply radiates strength.";
                        armour.DefValue += 2;
                        break;
                    case 2: magic[0] = "DEF +1"; magic[1] = "This piece of armor can deflect any blow.";
                        armour.DefValue += 1;
                        break;
                    case 3: magic[0] = "-50% ENC"; magic[1] = "This armour seems lighter than even Mithril.";
                        armour.Encumbrance = (int)Math.Ceiling(armour.Encumbrance / 2d);
                        break;
                    case 4: magic[0] = "DUR +1"; magic[1] = "Enchanted by a master enchanter, it will take tremendous effort to break this armor.";
                        armour.MaxDurability += 1;
                        break;
                    case 5: magic[0] = "Fire Immunity"; magic[1] = "From time to time, itr almost looks as if this armour is on fire even though it is cool to the touch. Maybe this is the reason that it is not susceptible to fire.";
                        armour.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.CompleteFireImmunity, -1));
                        break;
                    case 6: magic[0] = "CON +10 for poison roll tests"; magic[1] = "While forging this armour, powerful enchantments were imbued into it to thwart anyone trying to poison its wearer.";
                        armour.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ResistPoison, -1));
                        break;
                    case 7: magic[0] = "CON +5"; magic[1] = "The enchantment in this armour reaches out to its wearer and strengthens their resilience.";
                        armour.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Constitution, 5)));
                        break;
                    case 8: magic[0] = "STR +5"; magic[1] = "As if the armour lends its strength, the wearer of this piece of armour becomes as strong as an ox";
                        armour.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Strength, 5)));
                        break;
                    case 9: magic[0] = "Dodge +5"; magic[1] = "Imbued with powerful spells, the armour gives a tingling sensation to its wearer when danger is near, giving them time to dodge the threat.";
                        armour.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Dodge, 5)));
                        break;
                    case 10: return includeCurseRoll ? await GetMagicItemAsync(item, isCursed: true) : await GetMagicItemAsync(item, includeCurseRoll: false);
                }
                armour.Properties.Add(ArmourProperty.Magic, 0);
            }
            else
            {
                switch (roll)
                {
                    case 1: magic[0] = "HP +1"; magic[1] = "This item has been imbued by some of the life essence of the enchanter and lends it to the one wearing it.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.HitPoints, 1)));
                        break;
                    case 2: magic[0] = "CON +5"; magic[1] = "A spell of resilience has been cast upon this item, giving the wearer the power to resist damage and illness alike.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Constitution, 5)));
                        break;
                    case 3: magic[0] = "STR +5"; magic[1] = "Using this item grants the wearer the strength of several men.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Strength, 5)));
                        break;
                    case 4: magic[0] = "RES +5"; magic[1] = "Bolstering the mind against the horrors, this item lends a bit of spine to its user.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Resolve, 5)));
                        break;
                    case 5: magic[0] = "DEX +5"; magic[1] = "This makes the user as nimble as a cat...almost.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Dexterity, 5)));
                        break;
                    case 6: magic[0] = "Energy +1"; magic[1] = "TRhis is an extraordinary item for sure. Used as a vessel for energy, it gives the user the endurance to perform that which a normal man could not.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Energy, 1)));
                        break;
                    case 7: magic[0] = "Luck +1"; magic[1] = "Clearly enchanted by a follower of Rhidnir, this item bestows a little bit of luck to its user.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Luck, 1)));
                        break;
                    case 8:
                        roll = RandomHelper.GetRandomNumber(1, 9);
                        switch (roll)
                        {
                            case 1:
                                magic[0] = "CS +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.CombatSkill, +5)));
                                break;
                            case 2:
                                magic[0] = "RS +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.RangedSkill, +5)));
                                break;
                            case 3:
                                magic[0] = "Dodge +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Dodge, +5)));
                                break;
                            case 4:
                                magic[0] = "Pick Locks +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.PickLocks, +5)));
                                break;
                            case 5:
                                magic[0] = "Barter +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Barter, +5)));
                                break;
                            case 6:
                                magic[0] = "Heal +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Heal, +5)));
                                break;
                            case 7:
                                magic[0] = "Alchemy +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Alchemy, +5)));
                                break;
                            case 8:
                                magic[0] = "Perception +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Perception, +5)));
                                break;
                            case 9:
                                magic[0] = "Foraging +5";
                                item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Foraging, +5)));
                                break;
                        }
                        magic[1] = "Through this enchantment, the wizards who created this lends some of their skill to its user.";
                        break;
                    case 9: magic[0] = "Add 1 hero initiative token"; magic[1] = "A simple attempt to copy the Rings of Awareness, this item still has its benefits.";
                        item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.Initiative, -1));
                        break;
                    case 10: return includeCurseRoll ? await GetMagicItemAsync(item, isCursed: true) : await GetMagicItemAsync(item, includeCurseRoll: false);
                }
            }


            if (item.Name != "Ring" || item.Name != "Amulet")
            {
                item.Value = item.Value * 3;
            }
            item.Description += magic[1];
            item.Identified = false;
            item.Name = $"Magic {item.Name} of " + magic[0];
            item.MagicEffect = magic[0];
            return item;
        }

        public Equipment ApplyCurseToItem(Equipment item)
        {
            string description = item.Description;
            item.ActiveStatusEffects ??= new List<ActiveStatusEffect>();
            int roll = RandomHelper.RollDie(DiceType.D10);
            switch (roll)
            {
                case 1: 
                    description = "HP -2";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.HitPoints, -2)));
                    break;
                case 2: 
                    description = "WIS -5";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Wisdom, -5)));
                    break;
                case 3: 
                    description = "CON -5";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Constitution, -5)));
                    break;
                case 4: 
                    description = "STR -5";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Strength, -5)));
                    break;
                case 5: 
                    description = "DEX -5";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Dexterity, -5)));
                    break;
                case 6: 
                    description = "HP -3";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.HitPoints, -3)));
                    break;
                case 7: 
                    description = "RES -10";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Resolve, -10)));
                    break;
                case 8:
                    roll = RandomHelper.GetRandomNumber(1, 9);
                    switch (roll)
                    {
                        case 1: 
                            description = "CS -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.CombatSkill, -5)));
                            break;
                        case 2: 
                            description = "RS -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.RangedSkill, -5)));
                            break;
                        case 3: 
                            description = "Dodge -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Dodge, -5)));
                            break;
                        case 4: 
                            description = "Pick Locks -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.PickLocks, -5)));
                            break;
                        case 5: 
                            description = "Barter -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Barter, -5)));
                            break;
                        case 6: 
                            description = "Heal -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Heal, -5)));
                            break;
                        case 7: 
                            description = "Alchemy -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Alchemy, -5)));
                            break;
                        case 8: 
                            description = "Perception -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Perception, -5)));
                            break;
                        case 9: 
                            description = "Foraging -5";
                            item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, skillBonus: (Skill.Foraging, -5)));
                            break;
                    }
                    break;
                case 9: 
                    description = "Luck -1";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Luck, -1)));
                    break;
                case 10: 
                    description = "Energy -1";
                    item.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.ItemEffect, -1, statBonus: (BasicStat.Energy, -1)));
                    break;
            }

            item.Description += $"Cursed: {description}. ";
            return item;
        }

        public async Task<string> GetLegendaryAsync() // as it refers to fixed data
        {
            string item = "";
            int roll = (await _diceRoll.RequestRollAsync($"You found a legendary item!", "1d6")).Roll; await Task.Yield();
            switch (roll)
            {
                case <= 2:
                    var list = new List<string>
                    {
                        "Horn of Alfheim",
                        "Stone of Valheir",
                        "Amulet of Haamile",
                        "The Halfling Backpack",
                        "Amulet of Flight",
                        "Amulet of Deflection",
                        "The Vampire's Brooch",
                        "Ring of Awareness",
                        "Ring of the Heirophant",
                        "Ring of Regeneration",
                        "Trap-sensing Ring",
                        "Belt of Oakenshield",
                        "Vial of Never Ending",
                        "Legendary Elixir",
                        "Priestly Dice",
                    };
                    list.Shuffle();
                    item = list[0];
                    break;
                case <= 4:
                    list = new List<string>
                    {
                        "The Helmet of Golgorosh the Ram",
                        "The Breastplate of Rannulf",
                        "Crown of Resolve",
                        "Boots of Energy",
                        "Gauntlets of Hrafneir",
                        "Boots of Stability",
                        "Cloak of Elswhyr",
                    };
                    list.Shuffle();
                    item = list[0];
                    break;
                case <= 6:
                    list = new List<string>
                    {
                        "The Goblin Scimitar",
                        "The Golden Kopesh",
                        "Bow of Divine Twilight",
                        "The Summoner's Staff",
                        "The Headman's Axe",
                        "Sword of Lightning",
                        "Dagger of Vrunoir",
                        "Ohlnir's Hammer",
                    };
                    list.Shuffle();
                    item = list[0];
                    break;
            }
            return item;
        }

        public List<Armour> GetArmourPiecesAsync(int count, int durabilityWear = 0)
        {
            var items = new List<Armour>();
            if (durabilityWear == 0)
            {
                durabilityWear = RandomHelper.GetRandomNumber(1, 5);
            }
            for (int i = 0; i < count; i++)
            {
                var armour = GetRandomArmour(DefaultArmourDurability - durabilityWear);
                if (armour != null) items.Add(armour);
            }
            return items;
        }

        public Armour? GetRandomArmour(int durability = DefaultArmourDurability)
        {
            var list = new List<Armour?>()
            {
                GetPaddedArmour(durability),
                GetLeatherArmour(durability),
                GetMailArmour(durability),
                GetPlateArmour(durability),
            };
            list.Shuffle();
            return list[0];
        }

        private Armour? GetPaddedArmour(int durability = DefaultArmourDurability)
        {
            var roll = RandomHelper.RollDie(DiceType.D6);
            return roll switch
            {
                <= 1 => EquipmentService.GetArmourByNameSetDurability("Padded Cap", durability)?.Clone(),
                <= 3 => EquipmentService.GetArmourByNameSetDurability("Padded Vest", durability)?.Clone(),
                4 => EquipmentService.GetArmourByNameSetDurability("Padded Jacket", durability)?.Clone(),
                5 => EquipmentService.GetArmourByNameSetDurability("Padded Pants", durability)?.Clone(),
                >= 6 => EquipmentService.GetArmourByNameSetDurability("Padded Coat", durability) ?.Clone(),
            };
        }

        private Armour? GetLeatherArmour(int durability = DefaultArmourDurability)
        {
            var roll = RandomHelper.RollDie(DiceType.D6);
            return roll switch
            {
                <= 1 => EquipmentService.GetArmourByNameSetDurability("Leather Cap", durability)?.Clone(),
                <= 3 => EquipmentService.GetArmourByNameSetDurability("Leather Vest", durability)?.Clone(),
                4 => EquipmentService.GetArmourByNameSetDurability("Leather Jacket", durability)?.Clone(),
                5 => EquipmentService.GetArmourByNameSetDurability("Leather Pants", durability)?.Clone(),
                >= 6 => EquipmentService.GetArmourByNameSetDurability("Leather Coat", durability) ?.Clone(),
            };
        }

        private Armour? GetMailArmour(int durability = DefaultArmourDurability)
        {
            var roll = RandomHelper.RollDie(DiceType.D6);
            return roll switch
            {
                <= 1 => EquipmentService.GetArmourByNameSetDurability("Mail Coif", durability)?.Clone(),
                <= 3 => EquipmentService.GetArmourByNameSetDurability("Mail Shirt", durability)?.Clone(),
                4 => EquipmentService.GetArmourByNameSetDurability("Sleeved Mail Shirt", durability)?.Clone(),
                5 => EquipmentService.GetArmourByNameSetDurability("Mail Leggings", durability)?.Clone(),
                >= 6 => EquipmentService.GetArmourByNameSetDurability("Mail Coat", durability) ?.Clone(),
            };
        }

        private Armour? GetPlateArmour(int durability = DefaultArmourDurability)
        {
            var roll = RandomHelper.RollDie(DiceType.D4);
            return roll switch
            {
                <= 1 => EquipmentService.GetArmourByNameSetDurability("Plate Helmet", durability)?.Clone(),
                2 => EquipmentService.GetArmourByNameSetDurability("Breastplate", durability)?.Clone(),
                3 => EquipmentService.GetArmourByNameSetDurability("Plate Bracers", durability)?.Clone(),
                >= 4 => EquipmentService.GetArmourByNameSetDurability("Plate Leggings", durability)?.Clone(),
            };
        }

        private async Task<Armour?> GetNightstalkerArmourAsync(int durability)
        {
            int roll = (await _diceRoll.RequestRollAsync($"You found some nightstalker armour!", "1d6")).Roll; await Task.Yield();
            string itemName = "";
            switch (roll)
            {
                case 1: itemName = "Nightstalker Cap"; break;
                case <= 3: itemName = "Nightstalker Vest"; break;
                case 4: itemName = "Nightstalker Jacket"; break;
                case 5: itemName = "Nightstalker Pants"; break;
                case 6: itemName = "Nightstalker Bracers"; break;
                default:
                    break;
            }
            return EquipmentService.GetArmourByNameSetDurability(itemName, durability);
        }

        private async Task<Armour?> GetDragonScaleArmourAsync(int durability)
        {
            int roll = (await _diceRoll.RequestRollAsync($"You found dragon scale armour! Roll for type", "1d6")).Roll;
            await Task.Yield();
            string itemName = "";
            switch (roll)
            {
                case <= 3: itemName = "Dragon Scale Cap"; break;
                case 4: itemName = "Dragon Scale Breastplate"; break;
                case 5: itemName = "Dragon Scale Pants"; break;
                case 6: itemName = "Dragon Scale Bracers"; break;
            }
            return EquipmentService.GetArmourByNameSetDurability(itemName, durability);
        }

        public Weapon? GetRandomWeapon(int durability)
        {
            var list = new List<Weapon?>()
            {
                GetGreatWeapon(durability),
                GetLightWeapon(durability),
                GetSword(durability),
                GetBattleWeapon(durability),
                GetRangedWeapon(durability),
                GetAdvancedRangedWeapon(durability),
            };
            list.Shuffle();
            return list[0];
        }

        private MeleeWeapon? GetGreatWeapon(int durability)
        {
            var list = new List<MeleeWeapon?>()
            {
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Greatsword", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Greataxe", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Flail", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Halberd", durability) ?.Clone(),
            };
            list.Shuffle();
            return list[0];
        }

        private MeleeWeapon? GetLightWeapon(int durability)
        {
            var list = new List<MeleeWeapon?>()
            {
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Dagger", durability)?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Staff", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Javelin", durability) ?.Clone(),
            };
            list.Shuffle();
            return list[0];
        }

        private MeleeWeapon? GetSword(int durability)
        {
            var list = new List<MeleeWeapon?>()
            {
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Shortsword", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Rapier", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Broadsword", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Longsword", durability)?.Clone(),
            };
            list.Shuffle();
            return list[0];
        }

        private MeleeWeapon? GetBattleWeapon(int durability)
        {
            var list = new List<MeleeWeapon?>()
            {
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Battleaxe", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Battlehammer", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Morning Star", durability) ?.Clone(),
                (MeleeWeapon?)EquipmentService.GetWeaponByNameSetDurability("Warhammer", durability) ?.Clone(),
            };
            list.Shuffle();
            return list[0];
        }

        private RangedWeapon? GetRangedWeapon(int durability)
        {
            var list = new List<RangedWeapon?>()
            {
                (RangedWeapon?)EquipmentService.GetWeaponByNameSetDurability("Sling", durability) ?.Clone(),
                (RangedWeapon?)EquipmentService.GetWeaponByNameSetDurability("Shortbow", durability) ?.Clone(),
                (RangedWeapon?)EquipmentService.GetWeaponByNameSetDurability("Longbow", durability) ?.Clone(),
                (RangedWeapon?)EquipmentService.GetWeaponByNameSetDurability("Crossbow", durability) ?.Clone(),
            };
            list.Shuffle();
            return list[0];
        }

        private RangedWeapon? GetAdvancedRangedWeapon(int durability)
        {
            var list = new List<RangedWeapon?>()
            {
                (RangedWeapon?)EquipmentService.GetWeaponByNameSetDurability("Elven Bow", durability) ?.Clone(),
                (RangedWeapon?)EquipmentService.GetWeaponByNameSetDurability("Crossbow Pistol", durability) ?.Clone(),
                (RangedWeapon?)EquipmentService.GetWeaponByNameSetDurability("Arbalest", durability) ?.Clone(),
            };
            list.Shuffle();
            return list[0];
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
        public async Task<Equipment> CreateItemAsync(string itemName, int durability = 0, int value = 0, int quantity = 1, string itemDescription = "")
        {
            Equipment newItem;

            switch (itemName)
            {
                case "Legendary":
                    newItem = new Equipment()
                    {
                        Name = "Legendary item: " + GetLegendaryAsync(),
                        Durability = durability,
                        Quantity = quantity,
                        Value = value,
                        Description = itemDescription
                        // TODO: build lengendary items with factories
                    };
                    break;
                case "Relic":
                case "Relic - Epic":
                    string[] relicData = itemName == "Relic" ? await GetRelicAsync() : await GetRelicAsync("Epic");
                    newItem = new Equipment
                    {
                        Name = relicData[0],
                        Durability = durability,
                        Value = itemName == "Relic" ? 350 : 450,
                        Description = relicData[1],
                        Quantity = quantity
                    };
                    break;
                case "Potion Recipe - Weak":
                    newItem = new AlchemicalRecipe { Name = $"Weak Potion Recipe: {await _alchemy.GetNonStandardPotionAsync()}", Quantity = quantity, Encumbrance = 0, Durability = 0, Value = 0, Description = "The actual components involved shall be chosen by the player", MaxDurability = 0 };
                    break;
                case "Ingredient":
                    newItem = AlchemyService.GetIngredients(quantity)[0].Clone(); newItem.Quantity = quantity;
                    break;
                case "Part":
                    newItem = (await _alchemy.GetPartsAsync(1))[0].Clone(); newItem.Quantity = quantity;
                    break;
                case "Ingredient - Exquisite":
                    newItem = AlchemyService.GetIngredients(1)[0].Clone(); ((Ingredient)newItem).Exquisite = true; newItem.Quantity = quantity;
                    break;
                case "Part - Exquisite":
                    newItem = (await _alchemy.GetPartsAsync(1))[0].Clone(); ((Part)newItem).Exquisite = true; newItem.Quantity = quantity;
                    break;
                case "Amulet":
                    newItem = new Equipment { Name = "Amulet", Quantity = quantity, Encumbrance = 0, Durability = 0, Value = value, Description = "Can be enchanted", MaxDurability = 0 };
                    break;
                case "Ring":
                    newItem = new Equipment { Name = "Ring", Quantity = quantity, Encumbrance = 0, Durability = 0, Value = value, Description = "Can be enchanted", MaxDurability = 0 };
                    break;
                case "Backpack - Huge":
                    newItem = new Equipment { Name = "Backpack - Huge", Quantity = quantity, Encumbrance = 1, Durability = 0, Value = 600, Description = "Increase ENC +35, while decreasing DEX -20", MaxDurability = 0 };
                    break;
                case "Elven Bowstring":
                    newItem = new Equipment { Name = "Elven Bowstring", Quantity = quantity, Encumbrance = 0, Durability = 0, Value = 0, Description = "After adding to any bow during a rest, it adds RS +5 to the weapon", MaxDurability = 0 };
                    break;
                case "Flute":
                    newItem = new Equipment { Name = "Flute", Quantity = quantity, Encumbrance = 1, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, to lower the threat level by 1d3.", MaxDurability = 0 };
                    break;
                case "Gemstone":
                    newItem = new Equipment { Name = "Gemstone", Quantity = quantity, Encumbrance = 0, Durability = 0, Value = 0, MaxDurability = 0 }; // Value will be set by the calling method
                    break;
                case "Grimoire":
                    newItem = new Equipment { Name = $"Grimoire of {SpellService.GetRandomSpellName()}", Quantity = quantity, Encumbrance = 1, Durability = 0, Value = 0, Description = "This spell can be learned back at the Wizards' Guild as long as you have the proper level", MaxDurability = 0 };
                    break;
                case "Harp":
                    newItem = new Equipment { Name = "Harp", Quantity = quantity, Encumbrance = 2, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, all heroes regain an extra 1d3HP.", MaxDurability = 0 };
                    break;
                case "Lute":
                    newItem = new Equipment { Name = "Lute", Quantity = quantity, Encumbrance = 5, Durability = 0, Value = 100, Description = "May be used during a short rest, with a WIS test, to recover 1 sanity for all heroes.", MaxDurability = 0 };
                    break;
                case "Power Stone":
                    newItem = await GetPowerStoneAsync();
                    break;
                case "Set of Fine Clothes":
                    newItem = new Equipment { Name = "Set of Fine Clothes", Quantity = quantity, Encumbrance = 0, Durability = 0, Value = 0, Description = "Increases Barter +5", MaxDurability = 0 };
                    break;
                case "Scroll":
                    newItem = new Equipment { Name = $"Scroll of {SpellService.GetRandomSpellName()}", Quantity = quantity, Value = value == 0 ? 100 : value };
                    break;
                case "Talent Training Manual":
                    newItem = new Equipment { Name = $"{new PassiveAbilityService().GetRandomTalent()} Training Manual", Quantity = quantity, Encumbrance = 1, Durability = 0, Value = 0, Description = "Grants the talent named on the book, when read at an inn", MaxDurability = 0 };
                    break;
                case "Wolf Pelt":
                    newItem = new Equipment { Name = "Wolf Pelt", Quantity = quantity, Encumbrance = 2, Durability = 0, Value = 50, MaxDurability = 0 };
                    break;
                default:
                    newItem = new Equipment { Name = itemName, Quantity = quantity }; // Fallback for unknown items
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

        public async Task<SearchResult> SearchFurnitureAsync(Furniture furniture, SearchResult result)
        {
            if (result.SearchRoll == 0 || result.SearchRoll > 10)
            {
                var roll = await _diceRoll.RequestRollAsync($"{result.HeroSearching.Name} is searching the {furniture.Name}. Roll 1d10.", "1d10");
                await Task.Yield();
                result.SearchRoll = roll.Roll; 
            }
            int count = result.HeroIsThief ? 2 : 1;

            switch (furniture.TreasureType)
            {
                case TreasureType.AlchemistTable:
                    switch (result.SearchRoll)
                    {
                        case <= 4: (await _alchemy.GetRandomPotions(RandomHelper.RollDie(DiceType.D3))).ForEach(i => result.FoundItems = [i]); 
                            result.Message = "You found some potions!";
                            break;
                        case <= 7: AlchemyService.GetIngredients(RandomHelper.RollDie(DiceType.D10)).ToList().ForEach(i => result.FoundItems = [i]); 
                            result.Message = "You found some alchemical ingredients!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Altar:
                    switch (result.SearchRoll)
                    {
                        case <= 3: AlchemyService.GetIngredients(RandomHelper.RollDie(DiceType.D10)).ToList().ForEach(i => result.FoundItems = [i]); 
                            result.Message = "You found some alchemical ingredients!";
                            break;
                        case <= 7: result.FoundItems = [await GetCoins("10d6", 0)];
                            result.Message = "You found some coins!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.ArcheryTarget:
                    switch (result.SearchRoll)
                    {
                        case <= 4:
                            var choiceRequest = await _diceRoll.RequestChoiceAsync("What type of ammo do you want?", new List<string> { "Arrow", "Bolt" });
                            await Task.Yield();
                            result.FoundItems = [await GetTreasureAsync(choiceRequest.SelectedOption, durability: 1, amount: RandomHelper.RollDice("1d10"))];
                            result.Message = $"You found some {choiceRequest}s!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.ArmourRack:
                    switch (result.SearchRoll)
                    {
                        case <= 4:
                            GetArmourPiecesAsync(RandomHelper.RollDie(DiceType.D3), RandomHelper.RollDie(DiceType.D4)).ForEach(a => result.FoundItems = [a]);
                            result.Message = "You found some armour!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Backpack:
                    switch (result.SearchRoll)
                    {
                        case <= 5: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)]; 
                            result.Message = "You found some mundane treasure!";
                            break;
                        case <= 7: result.FoundItems = [await GetCoins("1d100", 0)]; 
                            result.Message = "You found some coins!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Barrels:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)]; 
                            result.Message = "You found some mundane treasure!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Bed:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)];
                            result.Message = "You found some mundane treasure!";
                            break;
                        case <= 4: result.FoundItems = [await GetCoins("1d100", 0)]; 
                            result.Message = "You found some coins!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Bedroll:
                    switch (result.SearchRoll)
                    {
                        case <= 3: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)];
                            result.Message = "You found some mundane treasure!";
                            break;
                        case <= 5: result.FoundItems = [await GetCoins("1d100", 0)]; 
                            result.Message = "You found some coins!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Bookshelf:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        case <= 5: result.FoundItems = [await CreateItemAsync("Scroll")]; 
                            result.Message = "You found a scroll!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.BookStand:
                    switch (result.SearchRoll)
                    {
                        case <= 5: result.FoundItems = [await CreateItemAsync("Grimoire")]; 
                            result.Message = "You found a grimoire!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Boxes:
                    switch (result.SearchRoll)
                    {
                        case <= 4: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)]; 
                            result.Message = "You found some mundane treasure!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Chest:
                    switch (result.SearchRoll)
                    {
                        case 1: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Wonderful, count)]; 
                            result.Message = "You found some wonderful treasure!";
                            break;
                        case <= 4: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)]; result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)]; 
                            result.Message = "You found some fine treasure!";
                            break;
                        case <= 8: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Coffin:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        case <= 8: result.Message = "You found nothing of any value."; break;
                        default:
                            result.SpawnMonster = new Dictionary<string, string>()
                            {
                                { "Name", "Zombie" },
                                { "Count", "1" },
                            };
                            result.SpawnPlacement = new Dictionary<string, string>()
                            {
                                { "PlacementRule", "RelativeToTarget" },
                                { "PlacementTarget", furniture.Id }
                            };
                            result.Message = "A zombie rises from the coffin!";
                            break;
                    }
                    break;
                case TreasureType.DeadAdventurer:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        case <= 8: result.Message = "You found nothing of any value."; break;
                        default:
                            result.SpawnMonster = new Dictionary<string, string>()
                            {
                                { "Name", "Zombie" },
                                { "Count", "1" },
                                { "Armour", "1" },
                                { "Weapons", "Longsword" }
                            };
                            result.SpawnPlacement = new Dictionary<string, string>()
                            {
                                { "PlacementRule", "RelativeToTarget" },
                                { "PlacementTarget", furniture.Id }
                            };
                            result.Message = "The dead adventurer rises as a zombie!";
                            break;
                    }
                    break;
                case TreasureType.DiningTable:
                    switch (result.SearchRoll)
                    {
                        case <= 4: result.FoundItems = [EquipmentService.GetAnyEquipmentByName("Ration")]; 
                            result.Message = "You found a ration!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Drawer:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        case <= 8: result.FoundItems = [await GetCoins("2d20", 0)];
                            result.Message = "You found some coins!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Fountain:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        case <= 8: result.FoundItems = [await GetCoins("1d20", 0)];
                            result.Message = "You found some coins!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.GrateOverHole:
                    switch (result.SearchRoll)
                    {
                        case <= 4: result.FoundItems = [EquipmentService.GetEquipmentByNameSetQuantity("Lock Picks", RandomHelper.RollDie(DiceType.D6))]; 
                            result.Message = "You found some lock picks!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Hearth:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [EquipmentService.GetEquipmentByNameSetQuantity("Ration", RandomHelper.RollDie(DiceType.D4))]; 
                            result.Message = "You found some rations!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.ObjectiveChest:
                    switch (result.SearchRoll)
                    {
                        case <= 3: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count), .. await FoundTreasureAsync(TreasureType.Wonderful, count), .. await FoundTreasureAsync(TreasureType.Wonderful, count)];
                            result.Message = "You found some fine and wonerful treasure!";
                            break;
                        case <= 7: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count), .. await FoundTreasureAsync(TreasureType.Fine, count), .. await FoundTreasureAsync(TreasureType.Wonderful, count)];
                            result.Message = "You found some fine and wonerful treasure!";
                            break;
                        case <= 10: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count), .. await FoundTreasureAsync(TreasureType.Fine, count), .. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Pottery:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)];
                            result.Message = "You found some mundane treasure!";
                            break;
                        case <= 4: result.FoundItems = [await GetCoins("1d20", 0)];
                            result.Message = "You found some coins!";
                            break;
                        case <= 5: result.FoundItems = [EquipmentService.GetAnyEquipmentByName("Ration")]; 
                            result.Message = "You found a ration!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Sarcophagus:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Wonderful, count)];
                            result.Message = "You found some wonderful treasure!";
                            break;
                        case <= 5: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        case <= 8: result.Message = "You found nothing of any value."; break;
                        default:
                            result.SpawnMonster = new Dictionary<string, string>()
                            {
                                { "Name", "Mummy" },
                                { "Count", "1" },
                            };
                            result.SpawnPlacement = new Dictionary<string, string>()
                            {
                                { "PlacementRule", "RelativeToTarget" },
                                { "PlacementTarget", furniture.Id }
                            };
                            result.Message = "A mummy emerges from the sarcophagus!";
                            break;
                    }
                    break;
                case TreasureType.Statue:
                    switch (result.SearchRoll)
                    {
                        case <= 1: 
                            int roll = RandomHelper.RollDie(DiceType.D6);
                            switch (roll)
                            {
                                case <= 2:  result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Wonderful, count)];
                                    result.Message = "You found some wonderful treasure!";
                                    break;
                                case <= 6: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                                    result.Message = "You found some fine treasure!";
                                    break;
                            }
                            break;
                        case <= 3: result.FoundItems = [await GetCoins("1d20", 0)]; 
                            result.Message = "You found some coins!";
                            break;
                        case <= 9: result.Message = "You found nothing of any value."; break;
                        default:
                            result.SpawnMonster = new Dictionary<string, string>()
                            {
                                { "Name", "Gargoyle" },
                                { "Count", "1" },
                            };
                            result.SpawnPlacement = new Dictionary<string, string>()
                            {
                                { "PlacementRule", "RelativeToTarget" },
                                { "PlacementTarget", furniture.Id }
                            };
                            result.Message = "As you touch the statue, it comes to life!";
                            break;
                    }
                    break;
                case TreasureType.StudyTable:
                    switch (result.SearchRoll)
                    {
                        case <= 4: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)];
                            result.Message = "You found some mundane treasure!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Throne:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.TortureTools:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [.. await _alchemy.GetPartsAsync(RandomHelper.RollDie(DiceType.D3), SpeciesName.Human)]; 
                            result.Message = "You found some alchemical parts!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.TreasurePile:
                    switch (result.SearchRoll)
                    {
                        case <= 2: result.FoundItems = [await GetCoins("4d100", 0)];
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Wonderful, count)];
                            result.Message = "You found some wonderful treasure and some cins!";
                            break;
                        case <= 5: result.FoundItems = [await GetCoins("3d100", 0)];
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)]; result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure and some coins!";
                            break;
                        case <= 10: result.FoundItems = [await GetCoins("2d100", 0)];
                            result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Fine, count)];
                            result.Message = "You found some fine treasure and some coins!";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.WeaponRack:
                    switch (result.SearchRoll)
                    {
                        case <= 4: result.FoundItems = [GetRandomWeapon(DefaultWeaponDurability - RandomHelper.RollDie(DiceType.D4))]; 
                            result.Message = "You found a weapon.";
                            break;
                        default: result.Message = "You found nothing of any value."; break;
                    }
                    break;
                case TreasureType.Well:
                    switch (result.SearchRoll)
                    {
                        case <= 1:
                            var chest = furniture;
                            chest.TreasureType = TreasureType.Chest;
                            result.Message = "You peer down the well, and see something shiny at the bottom.";
                            return await SearchFurnitureAsync(chest, result);
                        case <= 4: result.FoundItems = [.. await FoundTreasureAsync(TreasureType.Mundane, count)]; 
                            result.Message = "You found some items at the bottom of the well.";
                            break;
                        case <= 9: result.Message = "You found nothing of any value."; break;
                        default:
                            result.Message = "As you peer into the well, a Water Elemental rises up!";
                            result.SpawnMonster = new Dictionary<string, string>()
                            {
                                { "Name", "Water Elemental" },
                                { "Count", "1" },
                            };
                            result.SpawnPlacement = new Dictionary<string, string>()
                            {
                                { "PlacementRule", "RelativeToTarget" },
                                { "PlacementTarget", furniture.Id }
                            };
                            break;
                    }
                    break;

            }

            result.FoundItems = result.FoundItems;
            return result;
        }

        internal async Task<SearchResult> DrinkFurnitureAsync(Furniture furniture, SearchResult result)
        {
            switch (furniture.DrinkTreasureType)
            {
                case TreasureType.DrinkWaterBasin:
                switch (result.SearchRoll)
                {
                    case <= 2:
                        result.HeroSearching.Heal(RandomHelper.RollDie(DiceType.D6) + 1); result.HeroSearching.CurrentEnergy = result.HeroSearching.GetStat(BasicStat.Energy);
                        result.Message = "You feel reinvigorated after drinking from the basin.";
                        break;
                    case <= 8: result.Message = "Drinking from the fountain has no effect."; break;
                    default:
                        var rollResult = await _diceRoll.RequestRollAsync("Roll alchemy test.", "1d100", skill: (result.HeroSearching, Skill.Alchemy));
                        await Task.Yield();
                        if (rollResult.Roll > result.HeroSearching.GetSkill(Skill.Alchemy))
                        {
                            await StatusEffectService.AttemptToApplyStatusAsync(result.HeroSearching, new ActiveStatusEffect(StatusEffectType.Diseased, -1), _powerActivation);
                            result.Message = "The water was contaminated! You feel ill.";
                        }
                        else
                        {
                            result.Message = "Using your alchemy skill, you judge the water as unsafe, and don't drink.";
                        }
                        break;
                }
                break;
                case TreasureType.DrinkFountain:
                    switch (result.SearchRoll)
                    {
                        case <= 2:
                            result.HeroSearching.Heal(RandomHelper.RollDie(DiceType.D4) + 1); result.HeroSearching.CurrentEnergy = Math.Min(1, result.HeroSearching.GetStat(BasicStat.Energy) - result.HeroSearching.CurrentEnergy);
                            result.Message = "You feel refreshed and regain some health and energy.";
                            break;
                        case <= 8: result.Message = "Drinking from the fountain has no effect."; break;
                        default:
                            var rollResult = await _diceRoll.RequestRollAsync("Roll alchemy test.", "1d100", skill: (result.HeroSearching, Skill.Alchemy));
                            await Task.Yield();
                            if (rollResult.Roll > result.HeroSearching.GetSkill(Skill.Alchemy))
                            {
                                await StatusEffectService.AttemptToApplyStatusAsync(result.HeroSearching, new ActiveStatusEffect(StatusEffectType.Diseased, -1), _powerActivation);
                                result.Message = "The water was contaminated! You feel ill.";
                            }
                            else
                            {
                                result.Message = "Using your alchemy skill, you judge the water as unsafe, and don't drink.";
                            }
                            break;
                    }
                    break;
            }
            return result;
        }
    }
}