using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Text;
using System.Text.Json.Serialization;

namespace LoDCompanion.BackEnd.Services.GameData
{
    public class EquipmentService
    {

        public static string SilverWeaponDescription { get; set; } = "Can hurt ethereal, and does Increased DMG+1 to Undead";

        public static List<Equipment> Equipment => GetEquipment();
        public static List<MeleeWeapon> MeleeWeapons => GetMeleeWeapons();
        public static List<RangedWeapon> RangedWeapons => GetRangedWeapons();
        public static List<Equipment> Weapons => [.. MeleeWeapons, .. RangedWeapons];
        public static List<MagicStaff> MagicStaves => GetMagicStaves();
        public static List<Ammo> Ammo => GetAmmo();
        public static List<Armour> Armour => GetArmour();
        public static List<Shield> Shields => GetShields();
        public static List<Equipment> Relics => GetRelics();

        public EquipmentService()
        {

        }

        public static Equipment? GetAnyEquipmentByName(string name)
        {
            return (Equipment?)GetMeleeWeaponByName(name)
                ?? (Equipment?)GetRangedWeaponByName(name)
                ?? (Equipment?)GetMagicStaffByName(name)
                ?? (Equipment?)GetArmourByName(name)
                ?? (Equipment?)GetShieldByName(name)
                ?? (Equipment?)GetAmmoByName(name)
                ?? GetRelicByName(name); // The last item doesn't need a cast
        }

        public static Equipment? GetEquipmentByName(string name)
        {
            var masterItem = Equipment.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }

        public static Equipment GetEquipmentByNameSetQuantity(string name, int qty)
        {
            Equipment? item = GetEquipmentByName(name);
            if (item == null) throw new NullReferenceException();
            item.Quantity = qty;
            return item;
        }

        public static Equipment GetEquipmentByNameSetDurabilitySetQuantity(string name, int durability, int qty = 1)
        {
            Equipment? item = GetEquipmentByName(name);
            if (item == null) throw new NullReferenceException();
            item.Quantity = qty;
            item.Durability = durability;
            return item;
        }

        public static List<Equipment> GetStartingEquipment()
        {
            List<Equipment> list = new List<Equipment>();
            list.AddRange(Equipment.Where(x => x.Availability > 3));
            return list;
        }

        public static Ammo? GetAmmoByName(string name)
        {
            var masterItem = Ammo.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }

        public static Ammo? GetAmmoByNameSetQuantity(string name, int qty)
        {
            Ammo? item = GetAmmoByName(name);
            if (item != null)
            {
                item.Quantity = qty;
            }
            return item;
        }

        public static List<Ammo> GetStartingAmmo()
        {
            List<Ammo> list = new List<Ammo>();
            list.AddRange(Ammo.Where(x => x.Availability > 3));
            return list;
        }

        public static MeleeWeapon? GetMeleeWeaponByName(string name)
        {
            var masterItem = MeleeWeapons.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }

        public static RangedWeapon? GetRangedWeaponByName(string name)
        {
            var masterItem = RangedWeapons.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }

        public static Weapon? GetWeaponByName(string name)
        {
            return (Weapon?)GetMeleeWeaponByName(name) ?? GetRangedWeaponByName(name);
        }

        public static Weapon? GetWeaponByNameSetDurability(string name, int durability)
        {
            Weapon? weapon = GetWeaponByName(name);
            if (weapon != null)
            {
                weapon.Durability = durability;
            }
            return weapon;
        }

        public static List<Weapon> GetStartingWeapons()
        {
            List<Weapon> list = new List<Weapon>();
            list.AddRange(MeleeWeapons.Where(x => x.Availability > 3));
            list.AddRange(RangedWeapons.Where(x => x.Availability > 3));
            return list;
        }

        public static MagicStaff? GetMagicStaffByName(string name)
        {
            var masterItem = MagicStaves.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }

        public static Armour? GetArmourByName(string name)
        {
            var masterItem = Armour.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }
        public static Armour GetArmourByNameSetDurability(string name, int durability)
        {
            Armour armour = Armour.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
            armour.Durability = durability;
            return armour;
        }

        public static List<Armour> GetStartingArmour()
        {
            List<Armour> list = new List<Armour>();
            list.AddRange(Armour.Where(x => x.Availability > 3));
            return list;
        }

        public static Shield? GetShieldByName(string name)
        {
            var masterItem = Shields.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }

        public static Shield GetShieldByNameSetDurability(string name, int durability)
        {
            Shield shield = GetShieldByName(name) ?? throw new NullReferenceException();
            shield.Durability = durability;
            return shield;
        }

        public static List<Shield> GetStartingShields()
        {
            List<Shield> list = new List<Shield>();
            list.AddRange(Shields.Where(x => x.Availability > 3));
            return list;
        }

        public static Equipment? GetRelicByName(string name)
        {
            var masterItem = Relics.FirstOrDefault(x => x.Name == name);
            return masterItem != null ? masterItem.Clone() : null;
        }

        public static List<Equipment> GetShopInventory(bool useAvailability = false)
        {
            if (useAvailability)
            {
                throw new NotImplementedException();
            }
            else
            {
                List<Equipment> list = [
                    .. Equipment.Where(x => x.Category == "Common"),
                    .. Weapons.Where(x => x.Category == "Common"),
                    .. Ammo.Where(x => x.Category == "Common"),
                    .. Armour.Where(x => x.Category == "Common"),
                    .. Shields.Where(x => x.Category == "Common"),
                    .. AlchemyService.Potions.Where(x => x.Category == "Common")
                ];
                return list;
            }
        }

        public static List<Equipment> GetEquipment()
        {
            return new List<Equipment>
            {
                new Equipment(){
                        Category = "Common",
                        Name = "Alchemist Belt",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "This lets you store 6 potions or vials in ready slots, on top of the ordinary ready slots. Any hit that strikes a potion in the belt will also damage the belt with 1 point.",
                        Value = 300,
                        Availability = 3,
                        Storage = new Player.Inventory(6)
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Alchemist Tool",
                        Encumbrance = 5,
                        Durability = 6,
                        Description = "Necessary to harvest parts and ingredients.",
                        Value = 200,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Armour Repair Kit",
                        Encumbrance = 5,
                        Durability = 1,
                        Description = "This kit can be used to repair armour during a short rest. It will repair 1d3 durability of each of your hero's equipped pieces of armour. Roll separately. Once done, the kit is exhausted and removed.",
                        Value = 200,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Backpack - Medium",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "This backpack increases the carrying capacity of a hero with 10 ENC points, but decreases DEX with -5.",
                        Value = 350,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Backpack - Large",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "This backpack increases the carrying capacity of a hero with 25 ENC points, but decreases DEX with -10.",
                        Value = 600,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Bandage (old rags)",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "Necessary when using the Heal Skill. Heals 1d4 Hit Points. This is a bundle with enough rags to bandage 3 times.",
                        Value = 15,
                        Availability = 5,
                        Quantity = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Bandage (linen)",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "Necessary when using the Heal Skill. Heals 1d8 Hit Points.",
                        Value = 25,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Bandage (Herbal wrap)",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "Necessary when using the Heal Skill. Gives Heal skill +15 and heals 1d10 Hit Points.",
                        Value = 50,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Bed Roll",
                        Encumbrance = 5,
                        Durability = 6,
                        Description = "The short rests you take are way more comfortable with a bed roll. You automatically regain all Energy Points.",
                        Value = 200,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Beef Jerky",
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "Eating a snack like this takes 1 AP and will heal 1 HP.",
                        Value = 10,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Cooking Gear",
                        Encumbrance = 3,
                        Durability = 6,
                        Description = "Cooking gear will help make those rations a bit tastier. Rations cooked using this will heal a further +3 HP. One set of cooking gear is enough for the entire party.",
                        Value = 100,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Crowbar",
                        Encumbrance = 10,
                        Durability = 6,
                        Description = "Inflicts 8+DB Hit Points when breaking down a door, and only increases Threat Level +1.",
                        Value = 55,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Combat Harness",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "This increases your Quick Slots from 3 to 5. Any hit that damages a piece of equipment in the harness will also damage the harness.",
                        Value = 500,
                        Availability = 2,
                        Storage = new Player.Inventory(2)
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Dwarven Ale",
                        Encumbrance = 2,
                        Durability = 1,
                        Description = "Famous liquid courage. If your hero drinks this, all stat and skill tests are at -10, except RES that are at +20 for the rest of the quest.",
                        Value = 100,
                        Availability = 2
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Dwarven Pickaxe",
                        Encumbrance = 8,
                        Durability = 6,
                        Description = "A finely crafted pickaxe. Lighter, yet stronger than ordinary pickaxes.",
                        Value = 225,
                        Availability = 2
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Empty Bottle",
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "Necessary if you want to mix new potions.",
                        Value = 25,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Extended Battle Belt",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "This increases your Quick Slots from 3 to 4. Any hit that damages a piece of equipment in the belt will also damage the belt.",
                        Value = 300,
                        Availability = 3,
                        Storage = new Player.Inventory(1)
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Fishing Gear",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "A good fishing rod always makes life better. With this, a hero's Foraging Skill increases with +5.",
                        Value = 40,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Iron Wedge",
                        Encumbrance = 4,
                        Durability = 6,
                        Description = "These can be used to block a door and takes 1 AP, plus the action to close the door. A Wandering Monster (or a monster already revealed) will stop at this door and have to roll 1d6: 4-6 next turn to pass through. Enough for 2 doors.",
                        Value = 50,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Lamp Oil",
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "Enough oil to refill a lantern once.",
                        Value = 15,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Lantern",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "The light projected by the lantern helps strengthen the resolve of your party. See separate note on lantern.",
                        Value = 100,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Lock Picks",
                        Quantity = 5,
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "Necessary to use the Pick Lock Skill, but can also be used to disarm traps. If damaged, only 1 pick will be destroyed.",
                        Value = 30,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Necklace",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "Can be enchanted.",
                        Value = 150,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Parchment",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "Necessary to make magic scrolls.",
                        Value = 50,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Partial Map",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "See separate description.",
                        Value = 75,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Pickaxe",
                        Encumbrance = 10,
                        Durability = 6,
                        Description = "Can be used to remove rubble.",
                        Value = 175,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Ration",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "Rations are used during overland travel and during short rests. 1 Ration can sustain the entire party for one day or one rest.",
                        Value = 5,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Ring",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "Can be enchanted.",
                        Value = 150,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Rope (old)",
                        Encumbrance = 2,
                        Durability = 1,
                        Description = "A piece of rope may help you out of that pit you happened to trip into. When used, roll 1d6. On a result of 5-6, the rope breaks and the hero falls down taking 1d6 wounds.",
                        Value = 20,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Rope",
                        Encumbrance = 2,
                        Durability = 1,
                        Description = "A piece of rope may help you out of that pit you happened to trip into.",
                        Value = 50,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Tobacco",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "Tobacco will help calm the nerves, but there is always the risk of becoming addicted. See separate note on tobacco.",
                        Value = 50,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Torch",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "The light projected by a torch helps strengthen the resolve of your party. See separate note on torch.",
                        Value = 15,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Trap Disarming Kit",
                        Encumbrance = 5,
                        Durability = 6,
                        Description = "+10 when disarming traps.",
                        Value = 200,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Whetstone",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "During a short rest, you will be able to touch up your weapon. Repair close-combat weapons with 1d3 Points of Durability. 3 uses per stone.",
                        Value = 100,
                        Availability = 4,
                        Quantity = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Horse",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "A horse will increase the travel speed for the party if all heroes have one. See 'Travelling and Skirmishes'.",
                        Value = 1000,
                        Availability = 4
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Camel",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "A camel provides the same advantages as a horse, but with increased speed in the desert. Can only be bought in the outpost. Price and availability listed here is not changed by the Outpost modifiers.",
                        Value = 1250,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Saddlebags",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "Saddlebags will let you store 10 Encumbrance Points on your horse or camel. This equipment may be left on the mount during quests, but cannot be accessed until after the quest.",
                        Value = 250,
                        Availability = 4,
                        Storage = new Player.Inventory()
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Mule",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "A mule will let you store 100 Encumbrance Points. This equipment can be left on the mule during quests, but cannot be accessed until after the dungeon.",
                        Value = 800,
                        Availability = 4,
                        Storage = new Player.Inventory()
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Wagon",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "Requires a horse. It lets you store 500 Encumbrance Points. This equipment can be left in the wagon during quests, but cannot be accessed until after the dungeon.",
                        Value = 1500,
                        Availability = 3,
                        Storage = new Player.Inventory()
                    },
                    new Equipment(){
                        Category = "Common",
                        Name = "Holy Water",
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "This can be thrown in the same way as throwing a potion, but you can also dip 5 arrows into it. If thrown, it causes 1d3 Hit Points to any undead, but only in the square it hits. Arrows dipped add +1 DMG to all Undead (treated as non-mundane weapons as well).",
                        Value = 25,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Dark Guild",
                        Name = "Bear Trap",
                        Encumbrance = 5,
                        Durability = 2,
                        Description = "See Special Note (Implies a detailed rule beyond the table).",
                        Value = 200,
                        Availability = -1
                    },
                    new Equipment(){
                        Category = "Dark Guild",
                        Name = "Caltrops Trap",
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "These small metal pyramids can be thrown on the floor and the unsuspecting enemy will walk on them. They cause immense pain and will disrupt the movement for anyone walking over them. (Almost like Lego). The Caltrops can be thrown in a square up to 2 squares away from the hero. Throwing them takes 1 AP and this can be done as long as there is no enemy adjacent to the thrower. Any enemy walking over the square with no armour will suffer 1d4 Points of Damage and will stop its turn immediately.",
                        Value = 50,
                        Availability = -1
                    },
                    new Equipment(){
                        Category = "Dark Guild",
                        Name = "Door Mirror",
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "By spending 1 turn before opening a door, you can slide the mirror underneath the door and get a grip of the room on the other side. You may draw the Exploration Card and roll for an Encounter before opening the door. The party may add 2 hero initiative tokens to the bag if there is an encounter behind the door.",
                        Value = 300,
                        Availability = -1
                    },
                    new Equipment(){
                        Category = "Dark Guild",
                        Name = "Superior Lock Picks",
                        Quantity = 5,
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "These lock picks are Dwarven-made with extreme precision. They give +5 Lock picking skill.",
                        Value = 75,
                        Availability = -1
                    },
                    new Equipment(){
                        Category = "Dark Guild",
                        Name = "Superior Trap Disarming Kit",
                        Encumbrance = 4,
                        Durability = 6,
                        Description = "A Dwarven-made kit with perfect tolerances and smooth surfaces. Gives a +15 modifier to any attempt to disarm a trap.",
                        Value = 250,
                        Availability = -1
                    },
                    new Equipment(){
                        Category = "Dark Guild",
                        Name = "Tripwire with Darts Trap",
                        Encumbrance = 2,
                        Durability = 1,
                        Description = "See Special Note (Implies a detailed rule beyond the table, specifically for Wandering Monsters).",
                        Value = 150,
                        Availability = -1
                    },
                    new Equipment(){
                        Category = "Rangers Guild",
                        Name = "Aim Attachment",
                        Encumbrance = 0,
                        Durability = 6,
                        Description = "This can be added to a shortbow, bow, or any form of crossbow to make it easier to aim. When using an Aim Action, you get +15 instead of +10.",
                        Value = 200,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Rangers Guild",
                        Name = "Compass",
                        Encumbrance = 0,
                        Durability = 1,
                        Description = "This rather unique item will allow the party to reroll one Travel Event per travel. It must be carried in a Quick Slot.",
                        Value = 300,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Rangers Guild",
                        Name = "Elven Skinning Knife",
                        Encumbrance = 1,
                        Durability = 6,
                        Description = "This slender blade is razor sharp and will make the skinning process much easier. It grants a +10 modifier to Foraging whilst skinning only. Can be kept in the backpack all the time.",
                        Value = 250,
                        Availability = 3
                    },
                    new Equipment(){
                        Category = "Rangers Guild",
                        Name = "Skinning Knife",
                        Encumbrance = 1,
                        Durability = 6,
                        Description = "This will allow a Ranger to skin animals. Can be kept in the backpack all the time.",
                        Value = 100,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "Rangers Guild",
                        Name = "Wild game traps",
                        Encumbrance = 3,
                        Durability = 6,
                        Description = "These traps will make catching animals much easier and confers a +10 modifier when rolling a Foraging roll to catch animals. Can be kept in the backpack all the time.",
                        Value = 150,
                        Availability = 5
                    },
                    new Equipment(){
                        Category = "The Inner Sanctum",
                        Name = "Incense",
                        Encumbrance = 1,
                        Durability = 1,
                        Description = "Increases the Prayer skill with +5. Enough for 1 skirmish or 1 dungeon. May be lit in a quick slot during skirmish setup or before entering a dungeon.",
                        Value = 40,
                        Availability = 4
                    },
                    new Equipment()
                    {
                        Category = "Fighters Guild",
                        Name = "Pain Killer",
                        Durability=1,
                        Value = 50,
                        Description = "This is a fine powder, made form a mix of herbs. By snorting the powder a wounded individual will receive considerable pain relief. Once snorted, the hero may remove any wound status until the end of battle, and regain lost AP. The wound effect will still be applicable once the battle is over and the powder has worn off."
                    },
                    new Equipment()
                    {
                        Category = "Fighters Guild",
                        Name = "Slayer Weapon Treatment",
                        Value = 100,
                        Description = "The weapon smith at the guild has a special way of heat-treating weapons, giving them a razor-sharp edge which is able to cut through even the thickest armour. The weapon gets +1 DMG that is cumulative with any other modifier. The wepon must be at full durability, and the treatment lasts until the weapon breaks."
                    }
            };

        }

        public static List<Ammo> GetAmmo()
        {
            return new List<Ammo>()
            {
                new Ammo(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Arrow",
                    MaxDurability = 1,
                    Quantity = 5,
                    Value = 5,
                    Availability = 4,
                    AmmoType = AmmoType.Arrow
                  },
                  new Ammo(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Bolt",
                    MaxDurability = 1,
                    Quantity = 5,
                    Value = 5,
                    Availability = 4,
                    AmmoType = AmmoType.Bolt
                  },
                  new Ammo(){
                    Category = "Common",
                    Name = "Barbed Arrow",
                    Shop = ShopCategory.Weapons,
                    MaxDurability = 1,
                    Quantity = 5,
                    Description = "Increased DMG+1",
                    Value = 25,
                    Availability = 4,
                    Properties = new Dictionary<AmmoProperty, int>
                    {
                        { AmmoProperty.Barbed, 1 }
                    },
                    AmmoType = AmmoType.Arrow
                  },
                  new Ammo(){
                    Category = "Common",
                    Name = "Barbed Bolt",
                    Shop = ShopCategory.Weapons,
                    MaxDurability = 1,
                    Quantity = 5,
                    Description = "Increased DMG+1",
                    Value = 25,
                    Availability = 4,
                    Properties = new Dictionary<AmmoProperty, int>
                    {
                        { AmmoProperty.Barbed, 1 }
                    },
                    AmmoType = AmmoType.Bolt
                  },
                  new Ammo(){
                    Category = "Dark Guild",
                    Shop = ShopCategory.Weapons,
                    Name = "Superior Sling Stone",
                    MaxDurability = 1,
                    Quantity = 10,
                    Description = "This ammo is cast metal bullets rather than the normal stones. Gives +5 RS with slings and +1 DMG.",
                    Value = 25,
                    Availability = -1,
                    AmmoType = AmmoType.SlingStone
                  },
                  new Ammo()
                  {
                      Category = "Treasure",
                      Name = "Silver Arrow",
                      MaxDurability = 1,
                      Description = SilverWeaponDescription,
                      Value = 0,
                      Availability = 0,
                      Properties = new Dictionary<AmmoProperty, int>
                      {
                          { AmmoProperty.Silver, 1 }
                      },
                      AmmoType = AmmoType.Arrow
                  },
                  new Ammo()
                  {
                      Category = "Treasure",
                      Name = "Silver Bolt",
                      MaxDurability = 1,
                      Description = SilverWeaponDescription,
                      Value = 0,
                      Availability = 0,
                      Properties = new Dictionary<AmmoProperty, int>
                      {
                          { AmmoProperty.Silver, 1 }
                      },
                      AmmoType = AmmoType.Bolt
                  }
            };
        }

        public static List<MeleeWeapon> GetMeleeWeapons()
        {
            return new List<MeleeWeapon> {
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Dagger",
                    MinDamage = 1,
                    MaxDamage = 6,
                    DamageDice = "1d6",
                    Encumbrance = 5,
                    Class = 1,
                    Value = 10,
                    Availability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.DualWield, 1 },
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Shortsword",
                    MinDamage = 3,
                    MaxDamage = 8,
                    DamageDice = "1d6",
                    DamageBonus = 2,
                    Encumbrance = 7,
                    Class = 2,
                    Value = 70,
                    Availability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.DualWield, 2 },
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Sword, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Rapier",
                    MinDamage = 2,
                    MaxDamage = 7,
                    DamageDice = "1d6",
                    DamageBonus = 1,
                    Encumbrance = 5,
                    Class = 1,
                    Value = 130,
                    Availability = 3,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Fast, 0 },
                        { WeaponProperty.DualWield, 2 },
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Sword, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Broadsword",
                    MinDamage = 3,
                    MaxDamage = 10,
                    DamageDice = "1d8",
                    DamageBonus = 2,
                    Encumbrance = 8,
                    Class = 3,
                    Value = 90,
                    Availability = 5,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Sword, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Treasure",
                    Shop = ShopCategory.Weapons,
                    Name = "The Goblins Scimitar",
                    Description = "This scimitar once belonged to the Goblin King, Teezmeald. He wielded it in their famous last fight against the great Elf warrior, Aelynthi Bihorn. As Elves despise all things of Goblin making, Bihorn left the blade where the king fell. Whoever, or whatever, snatched the blade from their withered corpse will never be known, but this artifact continues to crave blood, no matter who wields it.",
                    //TODO: The wielder of this weapon gains the perk Frenzy
                    MinDamage = 3,
                    MaxDamage = 10,
                    DamageDice = "1d8",
                    DamageBonus = 2,
                    Encumbrance = 8,
                    Class = 3,
                    Value = 0,
                    Availability = 0,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Sword, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Treasure",
                    Shop = ShopCategory.Weapons,
                    Name = "The Flames of Zul",
                    Description = "When examining the fallen Mummy, the heroes find a sword strapped to its back. The sword is unnaturally cold and the metal is almost black. When given a test swing, a flame suddenly bursts out from it. As soon as the blade is held still, the flame vanishes.",
                    //TODO: All DMG is causes is regarded as fir DMG.
                    MinDamage = 3,
                    MaxDamage = 10,
                    DamageDice = "1d8",
                    DamageBonus = 2,
                    Encumbrance = 8,
                    Class = 3,
                    Value = 0,
                    Availability = 0,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Sword, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Longsword",
                    MinDamage = 1,
                    MaxDamage = 12,
                    DamageDice = "1d12",
                    Encumbrance = 10,
                    Class = 4,
                    Value = 100,
                    Availability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Sword, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Battleaxe",
                    MinDamage = 2,
                    MaxDamage = 11,
                    DamageDice = "1d10",
                    DamageBonus = 1,
                    Encumbrance = 10,
                    Class = 4,
                    Value = 100,
                    Availability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.BFO, 0 },
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Axe, 0 },
                        { WeaponProperty.Metal, 0 },
                        { WeaponProperty.ArmourPiercing, 1 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Battlehammer",
                    MinDamage = 1,
                    MaxDamage = 10,
                    DamageDice = "1d10",
                    Encumbrance = 10,
                    Class = 3,
                    Value = 100,
                    Availability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Stun, 0 },
                        { WeaponProperty.BFO, 0 },
                        { WeaponProperty.Blunt, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Morning Star",
                    MinDamage = 1,
                    MaxDamage = 8,
                    DamageDice = "1d8",
                    Encumbrance = 10,
                    Class = 4,
                    Value = 150,
                    Availability = 2,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Unwieldly, 4 },
                        { WeaponProperty.BFO, 0 },
                        { WeaponProperty.Stun, 0 },
                        { WeaponProperty.Blunt, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Flail",
                    MinDamage = 1,
                    MaxDamage = 10,
                    DamageDice = "1d10",
                    Encumbrance = 20,
                    Class = 5,
                    Value = 150,
                    Availability = 2,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Unwieldly, 4 },
                        { WeaponProperty.BFO, 0 },
                        { WeaponProperty.Stun, 0 },
                        { WeaponProperty.Blunt, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Staff",
                    MinDamage = 1,
                    MaxDamage = 8,
                    DamageDice = "1d8",
                    Encumbrance = 5,
                    Class = 2,
                    Value = 5,
                    Availability = 5,
                    MaxDurability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Defensive, 0 },
                        { WeaponProperty.Blunt, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Javelin",
                    MinDamage = 1,
                    MaxDamage = 10,
                    DamageDice = "1d10",
                    Encumbrance = 10,
                    Class = 2,
                    Value = 100,
                    Availability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Reach, 0 },
                        { WeaponProperty.BFO, 0 },
                        { WeaponProperty.Metal, 0 },
                        { WeaponProperty.ArmourPiercing, 1 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Greatsword",
                    MinDamage = 2,
                    MaxDamage = 12,
                    DamageDice = "2d6",
                    Encumbrance = 20,
                    Class = 5,
                    Value = 200,
                    Availability = 3,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Slow, 0 },
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Sword, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Greataxe",
                    MinDamage = 3,
                    MaxDamage = 14,
                    DamageDice = "1d12",
                    DamageBonus = 2,
                    Encumbrance = 20,
                    Class = 5,
                    Value = 200,
                    Availability = 3,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Slow, 0 },
                        { WeaponProperty.BFO, 0 },
                        { WeaponProperty.Edged, 0 },
                        { WeaponProperty.Axe, 0 },
                        { WeaponProperty.Metal, 0 },
                        { WeaponProperty.ArmourPiercing, 2 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Warhammer",
                    MinDamage = 2,
                    MaxDamage = 12,
                    DamageDice = "2d6",
                    Encumbrance = 20,
                    Class = 5,
                    Value = 200,
                    Availability = 3,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Slow, 0 },
                        { WeaponProperty.BFO, 0 },
                        { WeaponProperty.Stun, 0 },
                        { WeaponProperty.Blunt, 0 },
                        { WeaponProperty.Metal, 0 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Halberd",
                    MinDamage = 1,
                    MaxDamage = 12,
                    DamageDice = "1d12",
                    Encumbrance = 20,
                    Class = 5,
                    Value = 150,
                    Availability = 4,
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Reach, 0 },
                        { WeaponProperty.Metal, 0 },
                        { WeaponProperty.ArmourPiercing, 1 }
                    }
                },
                new MeleeWeapon()
                {
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Net",
                    MinDamage = 0,
                    MaxDamage = 0,
                    Encumbrance = 2,
                    Class = 2,
                    Value = 100,
                    Availability = 3,
                    Description = "If this weapon hits, the target is trapped in the net and must spend 1 action to get free with a successful STR Test. Until freed, this is the only action it can do. A net can be used once per battle and is retrieved automatically after the battle",
                    Properties = new Dictionary<WeaponProperty, int>
                    {
                        { WeaponProperty.Ensnare, 0 }
                    }
                },
            };
        }

        public static List<RangedWeapon> GetRangedWeapons()
        {
            return new List<RangedWeapon>
            {
                new RangedWeapon(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Shortbow",
                    MinDamage = 1,
                    MaxDamage = 8,
                    DamageDice = "1d8",
                    Encumbrance = 5,
                    Class = 6,
                    Value = 100,
                    Availability = 4,
                    ReloadTime = 1,
                    Description = "",
                    AmmoType = AmmoType.Arrow
                  },
                  new RangedWeapon(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Longbow",
                    MinDamage = 1,
                    MaxDamage = 10,
                    DamageDice = "1d10",
                    Encumbrance = 10,
                    Class = 6,
                    Value = 100,
                    Availability = 4,
                    ReloadTime = 1,
                    Description = "",
                    AmmoType = AmmoType.Arrow,
                    Properties = new Dictionary<WeaponProperty, int>() {
                        { WeaponProperty.ArmourPiercing, 1 }
                    }
                  },
                  new RangedWeapon(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Elven bow",
                    MinDamage = 3,
                    MaxDamage = 12,
                    DamageDice = "1d10",
                    DamageBonus = 2,
                    Encumbrance = 7,
                    Class = 6,
                    Value = 700,
                    Availability = 2,
                    ReloadTime = 1,
                    Description = "",
                    ElvenBowstring = true,
                    AmmoType = AmmoType.Arrow,
                    Properties = new Dictionary<WeaponProperty, int>() {
                        { WeaponProperty.ArmourPiercing, 1 }
                    }
                  },
                  new RangedWeapon(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Crossbow Pistol",
                    MinDamage = 2,
                    MaxDamage = 9,
                    DamageDice = "1d8",
                    DamageBonus = 1,
                    Encumbrance = 5,
                    Class = 2,
                    Value = 350,
                    Availability = 2,
                    ReloadTime = 2,
                    Description = "",
                    AmmoType = AmmoType.Bolt,
                    Properties = new Dictionary<WeaponProperty, int>() { { WeaponProperty.SecondaryWeapon, 0 } }
                  },
                  new RangedWeapon(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Crossbow",
                    MinDamage = 4,
                    MaxDamage = 13,
                    DamageDice = "1d10",
                    DamageBonus = 3,
                    Encumbrance = 15,
                    Class = 6,
                    Value = 250,
                    Availability = 3,
                    ReloadTime = 2,
                    Description = "",
                    AmmoType = AmmoType.Bolt,
                    Properties = new Dictionary<WeaponProperty, int>() {
                        { WeaponProperty.ArmourPiercing, 1 }
                    }
                  },
                  new RangedWeapon(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Arbalest",
                    MinDamage = 3,
                    MaxDamage = 18,
                    DamageDice = "3d6",
                    Encumbrance = 20,
                    Class = 6,
                    Value = 400,
                    Availability = 2,
                    ReloadTime = 3,
                    Description = "Requires STR 55",
                    AmmoType = AmmoType.Bolt,
                    Properties = new Dictionary<WeaponProperty, int>() {
                        { WeaponProperty.ArmourPiercing, 2 }
                    }
                  },
                  new RangedWeapon(){
                    Category = "Common",
                    Shop = ShopCategory.Weapons,
                    Name = "Sling",
                    MinDamage = 1,
                    MaxDamage = 8,
                    DamageDice = "1d8",
                    Encumbrance = 1,
                    Class = 6,
                    Value = 40,
                    Availability = 4,
                    ReloadTime = 1,
                    Description = "",
                    AmmoType = AmmoType.SlingStone
                  }
            };
        }

        public static List<MagicStaff> GetMagicStaves()
        {
            WeaponFactory weaponFactory = new WeaponFactory();
            return new List<MagicStaff>()
            {
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Arcane Staff",
                    Description = "This staff will lend some of its power to the wizard. It will give a +5 modifier to the Arcane Arts Skill. When leaving a dungeon, roll 1d10. On a result of 9-10, the magic has dissipated and must be recharged. Until recharged, it is considered a normal staff.",
                    Value = 400,
                    Availability = 4,
                    StaffType = "StatBonus",
                    MagicStaffProperties = new Dictionary<MagicStaffProperty, int>
                    {
                        { MagicStaffProperty.ArcaneArts, 5 }
                    }
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Major Mana Staff",
                    Description = "This staff may be used to store Mana between quests. Storing Mana does not take any time and can be done in a settlement while resting at the inn. This stores 30 points of Mana.",
                    Value = 800,
                    Availability = 2,
                    StaffType = "StatBonus",
                    MagicStaffProperties = new Dictionary<MagicStaffProperty, int>
                    {
                        { MagicStaffProperty.ManaStorage, 30 }
                    }
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Mana Staff",
                    Description = "This staff may be used to store Mana between quests. Storing Mana does not take any time and can be done in a settlement while resting at the inn. This stores 20 points of Mana.",
                    Value = 500,
                    Availability = 3,
                    StaffType = "StatBonus",
                    MagicStaffProperties = new Dictionary<MagicStaffProperty, int>
                    {
                        { MagicStaffProperty.ManaStorage, 20 }
                    }
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Minor Mana Staff",
                    Description = "This staff may be used to store Mana between quests. Storing Mana does not take any time and can be done in a settlement while resting at the inn. This stores 10 points of Mana.",
                    Value = 300,
                    Availability = 4,
                    StaffType = "StatBonus",
                    MagicStaffProperties = new Dictionary<MagicStaffProperty, int>
                    {
                        { MagicStaffProperty.ManaStorage, 10 }
                    }
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Staff of the Heart",
                    Description = "This staff strengthens the body of the wizard, giving +3 HP while the staff is carried. Between each quest, roll 1d10. On a result of 10, the magic has dissipated and must be recharged. Until recharged, it is considered a normal staff.",
                    Value = 350,
                    Availability = 4,
                    StaffType = "StatBonus",
                    MagicStaffProperties = new Dictionary<MagicStaffProperty, int>
                    {
                        { MagicStaffProperty.HitPointsBonus, 3 }
                    }
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Staff of Illumination",
                    Description = "This staff works just like a lantern. If the wizard makes a miscast, the flame goes out. It will not go out due to any other reason. To rekindle the flame, the wizard must simply spend one Action.",
                    Value = 300,
                    Availability = 4,
                    StaffType = "Illumination",
                    MagicStaffProperties = new Dictionary<MagicStaffProperty, int>
                    {
                        { MagicStaffProperty.Illumination, 1 }
                    }
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Fire Staff",
                    Description = "This staff contains the magic spell: Flare.",
                    Value = 400,
                    Availability = 3,
                    StaffType = "Spell",
                    ContainedSpell = "Flare",
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Staff of Slow",
                    Description = "This staff contains the magic spell: Slow.",
                    Value = 400,
                    Availability = 3,
                    StaffType = "Spell",
                    ContainedSpell = "Slow",
                }),
                weaponFactory.CreateMagicStaff(new MagicStaff()
                {
                    Category = "Wizards Guild",
                    Name = "Staff of the Bolt",
                    Description = "This staff contains the spell: Magic Bolt.",
                    Value = 500,
                    Availability = 3,
                    StaffType = "Spell",
                    ContainedSpell = "Magic Bolt",
                })
            };
        }

        public static List<Armour> GetArmour()
        {
            string upgradesText = "Can be added to padded, leather, or mail armours that already have a DEF value in the indicated area. Permanent bonus and cannot be separated from the armour. If the attached armour is destroyed, so is this item.";
            return new List<Armour>()
            {
                // --- FIGHTERS GUILD UPGRADES ---
                new Armour()
                {
                    Category = "Fighters Guild",
                    Shop = ShopCategory.General,
                    Name = "Gauntlets",
                    Encumbrance = 1,
                    DefValue = 1,
                    Value = 50,
                    Description = upgradesText,
                },
                new Armour()
                {
                    Category = "Fighters Guild",
                    Shop = ShopCategory.General,
                    Name = "Gorget",
                    Encumbrance = 1,
                    DefValue = 1,
                    Value = 50,
                    Description = upgradesText,
                },
                new Armour()
                {
                    Category = "Fighters Guild",
                    Shop = ShopCategory.General,
                    Name = "Poleyns",
                    Encumbrance = 1,
                    DefValue = 1,
                    Value = 50,
                    Description = upgradesText,
                },
                new Armour()
                {
                    Category = "Fighters Guild",
                    Shop = ShopCategory.General,
                    Name = "Shoulder Pads",
                    Encumbrance = 1,
                    DefValue = 1,
                    Value = 50,
                    Description = upgradesText,
                },
                // --- PADDED ARMOUR ---
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Padded Cap", DefValue = 2, Encumbrance = 1, Value = 30, Availability = 4, ArmourClass = 1, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Head, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Padded Vest", DefValue = 2, Encumbrance = 3, Value = 60, Availability = 4, ArmourClass = 1, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 }, { ArmourProperty.Stackable, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Padded Jacket", DefValue = 2, Encumbrance = 5, Value = 120, Availability = 4, ArmourClass = 1, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.Torso, 0 }, { ArmourProperty.Stackable, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Padded Pants", DefValue = 2, Encumbrance = 4, Value = 100, Availability = 4, ArmourClass = 1, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Legs, 0 }, { ArmourProperty.Stackable, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Padded Coat", DefValue = 2, Encumbrance = 6, Value = 200, Availability = 3, ArmourClass = 1, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.Torso, 0 }, { ArmourProperty.Legs, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Cloak", DefValue = 1, Encumbrance = 1, Value = 50, Availability = 4, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Cloak, 0 }, { ArmourProperty.Stackable, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Padded Dog Armour", DefValue = 2, Encumbrance = 1, Value = 60, Availability = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Dog, 0 } }
                },
                // --- LEATHER ARMOUR ---
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Leather Cap", DefValue = 3, Encumbrance = 1, Value = 50, Availability = 4, ArmourClass = 2, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Head, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Leather Vest", DefValue = 3, Encumbrance = 3, Value = 80, Availability = 4, ArmourClass = 2, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Leather Jacket", DefValue = 3, Encumbrance = 4, Value = 140, Availability = 4, ArmourClass = 2, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 }, { ArmourProperty.Arms, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Leather Leggings", DefValue = 3, Encumbrance = 3, Value = 120, Availability = 4, ArmourClass = 2, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Legs, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Leather Bracers", DefValue = 3, Encumbrance = 3, Value = 120, Availability = 3, ArmourClass = 2, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.Stackable, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Leather Dog Armour", DefValue = 3, Encumbrance = 3, Value = 120, Availability = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Dog, 0 } }
                },
                // --- MAIL ARMOUR ---
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Mail Coif", DefValue = 4, Encumbrance = 4, Value = 200, Availability = 3, ArmourClass = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Head, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Mail Shirt", DefValue = 4, Encumbrance = 6, Value = 600, Availability = 3, ArmourClass = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Sleeved Mail Shirt", DefValue = 4, Encumbrance = 7, Value = 950, Availability = 3, ArmourClass = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.Torso, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Mail Coat", DefValue = 4, Encumbrance = 8, Value = 750, Availability = 3, ArmourClass = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 }, { ArmourProperty.Legs, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Sleeved Mail Coat", DefValue = 4, Encumbrance = 10, Value = 1300, Availability = 3, ArmourClass = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.Torso, 0 }, { ArmourProperty.Legs, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Mail Leggings", DefValue = 4, Encumbrance = 5, Value = 200, Availability = 2, ArmourClass = 3, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Legs, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                // --- PLATE ARMOUR ---
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Helmet", DefValue = 5, Encumbrance = 5, Value = 300, Availability = 3, ArmourClass = 4, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Head, 0 }, { ArmourProperty.Clunky, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Breastplate", DefValue = 5, Encumbrance = 7, Value = 700, Availability = 3, ArmourClass = 4, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 }, { ArmourProperty.Clunky, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Plate Bracers", DefValue = 5, Encumbrance = 4, Value = 600, Availability = 3, ArmourClass = 4, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                new Armour()
                {
                    Category = "Common",
                    Shop = ShopCategory.Armour, Name = "Plate Leggings", DefValue = 5, Encumbrance = 6, Value = 700, Availability = 3, ArmourClass = 4, Durability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Legs, 0 }, { ArmourProperty.Clunky, 0 }, { ArmourProperty.Stackable, 0 }, { ArmourProperty.Metal, 0 } }
                },
                // --- DARK GUILD ARMOUR ---
                new Armour()
                {
                    Category = "Dark Guild",
                    Shop = ShopCategory.Armour, Name = "Nightstalker Cap", DefValue = 4, Encumbrance = 1, Value = 230, Availability = 3, ArmourClass = 2, Durability = 8, MaxDurability = 8,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Head, 0 } }
                },
                new Armour()
                {
                    Category = "Dark Guild",
                    Shop = ShopCategory.Armour, Name = "Nightstalker Vest", DefValue = 4, Encumbrance = 3, Value = 650, Availability = 3, ArmourClass = 2, Durability = 8, MaxDurability = 8,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 }, { ArmourProperty.DarkAsTheNight, 0 } }
                },
                new Armour()
                {
                    Category = "Dark Guild",
                    Shop = ShopCategory.Armour, Name = "Nightstalker Jacket", DefValue = 4, Encumbrance = 4, Value = 1000, Availability = 3, ArmourClass = 2, Durability = 8, MaxDurability = 8,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.Torso, 0 }, { ArmourProperty.DarkAsTheNight, 0 } }
                },
                new Armour()
                {
                    Category = "Dark Guild",
                    Shop = ShopCategory.Armour, Name = "Nightstalker Pants", DefValue = 4, Encumbrance = 3, Value = 900, Availability = 3, ArmourClass = 2, Durability = 8, MaxDurability = 8,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Legs, 0 }, { ArmourProperty.DarkAsTheNight, 0 } }
                },
                new Armour()
                {
                    Category = "Dark Guild",
                    Shop = ShopCategory.Armour, Name = "Nightstalker Bracers", DefValue = 4, Encumbrance = 3, Value = 150, Availability = 3, ArmourClass = 2, Durability = 8, MaxDurability = 8,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 } }
                },
                new Armour
                {
                    Category = "Treasure", Name = "Dragon Scale Cap", Encumbrance = 4, Value = 1000, Description = "Treat fire DMG as ordinary DMG", Availability = 0, ArmourClass = 3, DefValue = 7,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Head, 0 }, { ArmourProperty.DragonScale, 0 } }, MaxDurability = 10
                },
                new Armour
                {
                    Category = "Treasure", Name = "Dragon Scale Breastplate", Encumbrance = 6, Value = 2300, Description = "Treat fire DMG as ordinary DMG", Availability = 0, ArmourClass = 3, DefValue = 7,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Torso, 0 }, { ArmourProperty.DragonScale, 0 } }, MaxDurability = 10
                },
                new Armour
                {
                    Category = "Treasure", Name = "Dragon Scale Pants", Encumbrance = 5, Value = 1900, Description = "Treat fire DMG as ordinary DMG", Availability = 0, ArmourClass = 3, DefValue = 7,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Legs, 0 }, { ArmourProperty.DragonScale, 0 } }, MaxDurability = 10
                },
                new Armour
                {
                    Category = "Treasure", Name = "Dragon Scale Bracers", Encumbrance = 3, Value = 2000, Description = "Treat fire DMG as ordinary DMG", Availability = 0, ArmourClass = 3, DefValue = 7,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Arms, 0 }, { ArmourProperty.DragonScale, 0 } }, MaxDurability = 10
                },
                new Armour
                {
                    Category = "Treasure", Name = "Wyvern Cloak", Encumbrance = 2, Value = 1200, Description = "Def:3 against attacks from behind", Availability = 0, ArmourClass = 1, DefValue = 3, MaxDurability = 6,
                    Properties = new Dictionary<ArmourProperty, int> { { ArmourProperty.Cloak, 0 },  { ArmourProperty.Stackable, 0 } }
                }
            }
            ;
        }

        public static List<Shield> GetShields()
        {
            return new List<Shield>()
            {
                  new Shield(){
                    Category = "Common",
                    Shop = ShopCategory.Shields,
                    Name = "Buckler",
                    DefValue = 4,
                    Encumbrance = 4,
                    Value = 20,
                    Availability = 4,
                    WeaponClass = 1,
                    Durability = 6,
                    Properties = new Dictionary<ShieldProperty, int> { { ShieldProperty.Metal, 0 } }
                  },
                  new Shield(){
                    Category = "Common",
                    Shop = ShopCategory.Shields,
                    Name = "Heater Shield",
                    DefValue = 6,
                    Encumbrance = 10,
                    Value = 100,
                    Availability = 3,
                    WeaponClass = 3,
                    Durability = 6,
                    Properties = new Dictionary<ShieldProperty, int> { { ShieldProperty.Metal, 0 } }
                  },
                  new Shield(){
                    Category = "Common",
                    Shop = ShopCategory.Shields,
                    Name = "Tower Shield",
                    DefValue = 8,
                    Encumbrance = 15,
                    Value = 200,
                    Availability = 2,
                    WeaponClass = 5,
                    Durability = 6,
                    Properties = new Dictionary<ShieldProperty, int> { { ShieldProperty.Metal, 0 }, { ShieldProperty.Huge, 0 } }
                  },
            };
        }

        public static List<Equipment> GetRelics()
        {
            return new List<Equipment>()
            {
                new Equipment(){
                    Category = "The Inner Sanctum",
                    Shop = ShopCategory.General,
                    Name = "Relic of Charus",
                    Encumbrance = 1,
                    Durability = 1,
                    Description = "Can be used by a Warrior Priest. Can be a ring or a necklace.",
                    Value = 500,
                    Availability = 4,
                    MagicEffect = "Gain 1 max energy point"
                  },
                  new Equipment(){
                    Category = "The Inner Sanctum",
                    Shop = ShopCategory.General,
                    Name = "Relic of Metheia",
                    Encumbrance = 1,
                    Durability = 1,
                    Description = "Can be used by a Warrior Priest. Can be a ring or a necklace.",
                    Value = 500,
                    Availability = 4,
                    MagicEffect = "Healing bonus 1d3 done by the Warrior Priest"
                  },
                  new Equipment(){
                    Category = "The Inner Sanctum",
                    Shop = ShopCategory.General,
                    Name = "Relic of Iphy",
                    Encumbrance = 1,
                    Durability = 1,
                    Description = "Can be used by a Warrior Priest. Can be a ring or a necklace.",
                    Value = 500,
                    Availability = 4,
                    MagicEffect = "Adds 5 to RES"
                  },
                  new Equipment(){
                    Category = "The Inner Sanctum",
                    Shop = ShopCategory.General,
                    Name = "Relic of Rhidnir",
                    Encumbrance = 1,
                    Durability = 1,
                    Description = "Can be used by a Warrior Priest. Can be a ring or a necklace.",
                    Value = 500,
                    Availability = 4,
                    MagicEffect = "Luck +1"
                  },
                  new Equipment(){
                    Category = "The Inner Sanctum",
                    Shop = ShopCategory.General,
                    Name = "Relic of Ohinir",
                    Encumbrance = 1,
                    Durability = 1,
                    Description = "Can be used by a Warrior Priest. Can be a ring or a necklace.",
                    Value = 500,
                    Availability = 4,
                    MagicEffect = "Adds 5 to STR"
                  },
                  new Equipment(){
                    Category = "The Inner Sanctum",
                    Shop = ShopCategory.General,
                    Name = "Relic of Ramos",
                    Encumbrance = 1,
                    Durability = 1,
                    Description = "Can be used by a Warrior Priest. Can be a ring or a necklace.",
                    Value = 500,
                    Availability = 4,
                    MagicEffect = "Adds 5 to CS"
                  }
            };
        }
    }

    public enum ShopCategory
    {
        General,
        Weapons,
        Armour,
        Shields,
        Potions,
    }

    public enum EquipmentProperty
    {
        Lantern,
        Torch,
        Backpack,
        Consumable,
        Restorative,
        Lockpick,
    }

    [JsonDerivedType(typeof(Weapon), typeDiscriminator: "Weapon")]
    [JsonDerivedType(typeof(MeleeWeapon), typeDiscriminator: "MeleeWeapon")]
    [JsonDerivedType(typeof(RangedWeapon), typeDiscriminator: "RangedWeapon")]
    [JsonDerivedType(typeof(Armour), typeDiscriminator: "Armour")]
    [JsonDerivedType(typeof(Shield), typeDiscriminator: "Shield")]
    [JsonDerivedType(typeof(Ammo), typeDiscriminator: "Ammo")]
    [JsonDerivedType(typeof(AlchemyItem), typeDiscriminator: "AlchemyItem")]
    [JsonDerivedType(typeof(Ingredient), typeDiscriminator: "Ingredient")]
    [JsonDerivedType(typeof(Part), typeDiscriminator: "Part")]
    [JsonDerivedType(typeof(Potion), typeDiscriminator: "Potion")]
    [JsonDerivedType(typeof(AlchemicalRecipe), typeDiscriminator: "AlchemicalRecipe")]
    public class Equipment
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = "Common";
        public ShopCategory Shop { get; set; } = ShopCategory.General;
        public string Name { get; set; } = string.Empty;
        public int Encumbrance { get; set; }
        public int MaxDurability { get; set; } = 6;
        public int Durability { get; set; } = 1;
        public double Value { get; set; } = 0;
        public int SellValue => CalculateSalePrice();
        public int RepairCost => CalculateInitialRepairCosts();
        public int Availability { get; set; } = 4;
        public int Quantity { get; set; } = 1;
        public string Description { get; set; } = string.Empty;
        public string MagicEffect { get; set; } = string.Empty;
        public Inventory? Storage { get; set; }
        public Dictionary<EquipmentProperty, int> Properties { get; set; } = new Dictionary<EquipmentProperty, int>();

        public Equipment()
        {

        }

        public virtual Equipment Clone()
        {
            Equipment newEquipment = new Equipment();
            newEquipment.Category = Category;
            newEquipment.Shop = Shop;
            newEquipment.Name = Name;
            newEquipment.Encumbrance = Encumbrance;
            newEquipment.Value = Value;
            newEquipment.Availability = Availability;
            newEquipment.MaxDurability = MaxDurability;
            newEquipment.Durability = Durability;
            newEquipment.Quantity = Quantity;
            newEquipment.Description = Description;
            newEquipment.MagicEffect = MagicEffect;
            newEquipment.Storage = Storage;
            newEquipment.Properties = new Dictionary<EquipmentProperty, int>(Properties);

            return newEquipment;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Category}] {Name} | ");
            sb.Append($"Value: {Value} | Dur: {Durability}/{MaxDurability}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.Append($" | Effect: {MagicEffect}");
            }
            return sb.ToString();
        }
        public string GetDisplayName()
        {
            if (Quantity > 1)
            {
                return $"{Name} (x{Quantity})";
            }

            return Name;
        }

        public int CalculateInitialRepairCosts()
        {
            if (Value > 0)
            {
                return (int)Math.Floor(Value * 0.2f);
            }
            else
            {
                return 0;
            }
        }

        public int CalculateSalePrice()
        {
            int currentMaxDurability = MaxDurability;

            if (Quantity <= 0 || Durability <= 0 || Value <= 10)
            {
                return 0;
            }

            // Calculate sale value based on durability remaining
            int durabilityDifference = currentMaxDurability - Durability;

            switch (durabilityDifference)
            {
                case 0:
                    return (int)Math.Floor(Value * 0.7f);
                case 1:
                    return (int)Math.Floor(Value * 0.6f);
                case 2:
                    return (int)Math.Floor(Value * 0.5f);
                case 3:
                    return (int)Math.Floor(Value * 0.4f);
                case 4:
                    return (int)Math.Floor(Value * 0.3f);
                default:
                    return (int)Math.Floor(Value * 0.2f);
            }
        }

        public bool HasProperty(EquipmentProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(EquipmentProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }
    }

    public enum AmmoType
    {
        SlingStone,
        Arrow,
        Bolt
    }

    public enum AmmoProperty
    {
        Silver,
        Barbed,
        SuperiorSlingStone,
        HolyWater
    }

    public class Ammo : Equipment
    {
        public AmmoType AmmoType { get; set; } = AmmoType.Arrow; // Default ammo type, can be set in constructor
        public new Dictionary<AmmoProperty, int> Properties { get; set; } = new Dictionary<AmmoProperty, int>();

        public Ammo() { } // Default constructor

        public override Ammo Clone()
        {
            Ammo newAmmo = new Ammo();
            newAmmo.Category = Category;
            newAmmo.Shop = Shop;
            newAmmo.Name = Name;
            newAmmo.Encumbrance = Encumbrance;
            newAmmo.Value = Value;
            newAmmo.Availability = Availability;
            newAmmo.MaxDurability = MaxDurability;
            newAmmo.Durability = Durability;
            newAmmo.Quantity = Quantity;
            newAmmo.Description = Description;
            newAmmo.MagicEffect = MagicEffect;
            newAmmo.Properties = new Dictionary<AmmoProperty, int>(Properties);
            return newAmmo;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append($"[{Category}] {Name} | ");
            sb.Append($"Value: {Value} | Dur: {Durability}/{MaxDurability}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.Append($" | Effect: {MagicEffect}");
            }
            sb.Append($" | Ammo Category: {AmmoType}");
            if (HasProperty(AmmoProperty.Silver)) sb.Append(", Silver");
            if (HasProperty(AmmoProperty.Barbed)) sb.Append(", Barbed");
            if (HasProperty(AmmoProperty.SuperiorSlingStone)) sb.Append(", Superior");
            if (HasProperty(AmmoProperty.HolyWater)) sb.Append(", Holy Water added");
            return sb.ToString();
        }

        public bool HasProperty(AmmoProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(AmmoProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }
    }

    public enum WeaponProperty
    {
        Silver,
        Mithril,
        DualWield,
        BFO,
        Slow,
        Stun,
        Unwieldly,
        FirstHit,
        Fast,
        Defensive,
        Ensnare,
        Reach,
        Edged,
        SlayerTreated,
        Axe,
        Sword,
        Blunt,
        Metal,
        Magic,
        SecondaryWeapon,
        ArmourPiercing,
        Cursed,
        Poisoned,
        Diseased
    }

    public class Weapon : Equipment
    {
        public bool IsRanged { get; set; }
        public bool IsMelee { get; set; }
        public new int Durability { get; set; } = 6;
        public int Class { get; set; }
        public string? DamageDice { get; set; }
        public int DamageBonus { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public new virtual Dictionary<WeaponProperty, int> Properties { get; set; } = new Dictionary<WeaponProperty, int>();

        public virtual int RollDamage()
        {
            return RandomHelper.GetRandomNumber(MinDamage, MaxDamage);
        }

        public override Weapon Clone()
        {
            if (this is MeleeWeapon meleeWeapon)
            {
                return meleeWeapon.Clone();
            }
            else if (this is RangedWeapon rangedWeapon)
            {
                return rangedWeapon.Clone();
            }

            return Clone();
        }

        public virtual bool HasProperty(WeaponProperty property)
        {
            return Properties.ContainsKey(property);
        }

    }

    public class MeleeWeapon : Weapon
    {
        private bool HasAppliedSlayerModifier { get; set; }
        private Dictionary<WeaponProperty, int> _properties = new Dictionary<WeaponProperty, int>();

        public override Dictionary<WeaponProperty, int> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                SetMithrilModifier();
            }
        }

        public MeleeWeapon()
        {
            IsMelee = true;
        }

        public override MeleeWeapon Clone()
        {
            MeleeWeapon newMeleeWeapon = new MeleeWeapon();

            IsMelee = true;
            newMeleeWeapon.Category = Category;
            newMeleeWeapon.Shop = Shop;
            newMeleeWeapon.Name = Name;
            newMeleeWeapon.Class = Class;
            newMeleeWeapon.MinDamage = MinDamage;
            newMeleeWeapon.MaxDamage = MaxDamage;
            newMeleeWeapon.Value = Value;
            newMeleeWeapon.Encumbrance = Encumbrance;
            newMeleeWeapon.Durability = Durability;
            newMeleeWeapon.DamageDice = DamageDice;
            newMeleeWeapon.DamageBonus = DamageBonus;
            newMeleeWeapon.Properties = new Dictionary<WeaponProperty, int>(Properties);
            return newMeleeWeapon;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Value > 0)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"{prop.Key}: +{prop.Value}");
                    }
                    else
                    {
                        // Simple properties
                        propsAsStrings.Add(prop.Key.ToString());
                    }
                }
                sb.Append(" | Properties: ").Append(string.Join(", ", propsAsStrings));
            }

            return sb.ToString();
        }

        private void SetMithrilModifier()
        {
            if (HasProperty(WeaponProperty.Mithril))
            {
                MinDamage += 1;
                MaxDamage += 1;
                DamageBonus += 1;
                Encumbrance -= 2;
            }
        }

        public void SetSlayerTreatedModifier()
        {
            if (HasProperty(WeaponProperty.SlayerTreated))
            {
                MinDamage += 1;
                MaxDamage += 1;
                DamageBonus += 1;
                HasAppliedSlayerModifier = true;
            }
        }

        public override bool HasProperty(WeaponProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(WeaponProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }
    }

    public enum MagicStaffProperty
    {
        ArcaneArts,
        ManaStorage,
        HitPointsBonus,
        Illumination
    }

    public class MagicStaff : MeleeWeapon
    {
        public string StaffType { get; set; } = string.Empty;
        public string ContainedSpell { get; set; } = string.Empty;
        public Dictionary<MagicStaffProperty, int> MagicStaffProperties { get; set; } = new Dictionary<MagicStaffProperty, int>();

        public MagicStaff() { }

        public override MagicStaff Clone()
        {
            MagicStaff newMagicStaff = new MagicStaff();
            newMagicStaff.Category = Category;
            newMagicStaff.Shop = Shop;
            newMagicStaff.Name = Name;
            newMagicStaff.Class = Class;
            newMagicStaff.MinDamage = MinDamage;
            newMagicStaff.MaxDamage = MaxDamage;
            newMagicStaff.Value = Value;
            newMagicStaff.Encumbrance = Encumbrance;
            newMagicStaff.Durability = Durability;
            newMagicStaff.MagicEffect = MagicEffect;
            newMagicStaff.Properties = new Dictionary<WeaponProperty, int>(Properties);
            newMagicStaff.StaffType = StaffType;
            newMagicStaff.ContainedSpell = ContainedSpell;
            newMagicStaff.MagicStaffProperties = new Dictionary<MagicStaffProperty, int>(MagicStaffProperties);
            return newMagicStaff;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Value > 0)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"{prop.Key}: +{prop.Value}");
                    }
                    else
                    {
                        // Simple properties
                        propsAsStrings.Add(prop.Key.ToString());
                    }
                }
                sb.Append(" | Properties: ").Append(string.Join(", ", propsAsStrings));
            }
            return sb.ToString();
        }

    }

    public class RangedWeapon : Weapon
    {
        public AmmoType AmmoType { get; set; } = AmmoType.Arrow;
        public Ammo Ammo { get; set; } = new Ammo();
        public bool ElvenBowstring { get; set; }
        public bool AimAttachment { get; set; }
        public int ReloadTime { get; set; } = 1;
        public bool IsLoaded { get; set; } = false;

        public RangedWeapon()
        {
            IsRanged = true;
        }

        public override RangedWeapon Clone()
        {
            RangedWeapon newRangedWeapon = new RangedWeapon();

            IsRanged = true;
            newRangedWeapon.Name = Name;
            newRangedWeapon.Description = Description;
            newRangedWeapon.Class = Class;
            newRangedWeapon.Durability = Durability;
            newRangedWeapon.Encumbrance = Encumbrance;
            newRangedWeapon.Value = Value;
            newRangedWeapon.MinDamage = MinDamage;
            newRangedWeapon.MaxDamage = MaxDamage;
            newRangedWeapon.DamageDice = DamageDice;
            newRangedWeapon.DamageBonus = DamageBonus;
            newRangedWeapon.AmmoType = AmmoType;
            newRangedWeapon.Ammo = Ammo;
            newRangedWeapon.ElvenBowstring = ElvenBowstring;
            newRangedWeapon.AimAttachment = AimAttachment;
            newRangedWeapon.ReloadTime = ReloadTime;
            return newRangedWeapon;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");
            sb.AppendLine($" | Ammo Category: {AmmoType} | Reload Time: {ReloadTime} AP | Loaded: {IsLoaded}");
            if (ElvenBowstring) sb.Append(" | Elven Bowstring");
            if (AimAttachment) sb.Append(" | Aim Attachment");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }
            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Value > 0)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"{prop.Key}: +{prop.Value}");
                    }
                    else
                    {
                        // Simple properties
                        propsAsStrings.Add(prop.Key.ToString());
                    }
                }
                sb.Append(" | Properties: ").Append(string.Join(", ", propsAsStrings));
            }
            return sb.ToString();
        }

        public bool IsSlingUsingNormalAmmo()
        {
            if (AmmoType == AmmoType.SlingStone && Ammo != null && !Ammo.HasProperty(AmmoProperty.SuperiorSlingStone))
            {
                return true;
            }
            return false;
        }

        public bool ConsumeAmmo(int quantity = 1)
        {
            if (IsSlingUsingNormalAmmo())
            {
                IsLoaded = false;
                return false;
            }

            if (Ammo != null && Ammo.Quantity >= quantity)
            {
                Ammo.Quantity -= quantity;
                IsLoaded = false;
                return true;
            }
            else
            {
                IsLoaded = false;
                return true;
            }
        }

        public void reloadAmmo()
        {
            if (Ammo != null && Ammo.Quantity > 0 || IsSlingUsingNormalAmmo())
            {
                IsLoaded = true;
            }
        }
    }

    public enum ArmourProperty
    {
        Mithril,
        Metal,
        Head,
        Torso,
        Arms,
        Legs,
        Cloak,
        Stackable,
        Clunky,
        Upgraded,
        DarkAsTheNight,
        DragonScale,
        Dog,
        Magic
    }

    public class Armour : Equipment
    {
        new int Durability { get; set; } = 6;
        public int ArmourClass { get; set; }
        public int DefValue { get; set; }
        private Dictionary<ArmourProperty, int> _properties = new Dictionary<ArmourProperty, int>();

        public new Dictionary<ArmourProperty, int> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                SetMithrilModifier();
            }
        }

        public Armour()
        {

        }

        public override Armour Clone()
        {
            Armour newArmour = new Armour();
            newArmour.Category = Category;
            newArmour.Shop = Shop;
            newArmour.Name = Name;
            newArmour.ArmourClass = ArmourClass;
            newArmour.DefValue = DefValue;
            newArmour.Encumbrance = Encumbrance;
            newArmour.Value = Value;
            newArmour.Availability = Availability;
            newArmour.Durability = Durability;
            newArmour.Properties = new Dictionary<ArmourProperty, int>(Properties);
            return newArmour;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Category}] {Name} | ");
            sb.AppendLine($"Class: {ArmourClass} | DEF: {DefValue}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    propsAsStrings.Add(prop.Key.ToString());
                }
                sb.Append(" | Properties: ").Append(string.Join(", ", propsAsStrings));
            }
            return sb.ToString();
        }

        public void SetMithrilModifier()
        {
            if (HasProperty(ArmourProperty.Mithril))
            {
                DefValue += 1; // Increase defense value if Mithril
                Encumbrance -= 1; // Decrease encumbrance if Mithril
            }
        }

        public bool HasProperty(ArmourProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(ArmourProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }
    }

    public enum ShieldProperty
    {
        Mithril,
        Metal,
        Huge
    }

    public class Shield : Equipment
    {
        new int Durability { get; set; } = 6;
        public int WeaponClass { get; set; }
        public int DefValue { get; set; }
        private Dictionary<ShieldProperty, int> _properties = new Dictionary<ShieldProperty, int>();

        public new Dictionary<ShieldProperty, int> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                SetMithrilModifier();
            }
        }

        public Shield()
        {

        }

        public override Shield Clone()
        {
            Shield newShield = new Shield();
            newShield.Category = Category;
            newShield.Shop = Shop;
            newShield.Name = Name;
            newShield.DefValue = DefValue;
            newShield.Encumbrance = Encumbrance;
            newShield.Value = Value;
            newShield.Availability = Availability;
            newShield.WeaponClass = WeaponClass;
            newShield.Durability = Durability;
            newShield.Properties = new Dictionary<ShieldProperty, int>(Properties);
            return newShield;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Category}] {Name} | ");
            sb.AppendLine($"Class: {WeaponClass} | DEF: {DefValue}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }
            if (HasProperty(ShieldProperty.Huge)) sb.AppendLine("Properties: Huge");
            return sb.ToString();
        }

        public void SetMithrilModifier()
        {
            if (HasProperty(ShieldProperty.Mithril))
            {
                DefValue += 1; // Increase defense value if Mithril
                Encumbrance -= 1; // Decrease encumbrance if Mithril
            }
        }

        public bool HasProperty(ShieldProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(ShieldProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }

    }

    public class WeaponFactory
    {
        /// <summary>
        /// Creates a new weapon instance from a base template and applies modifications.
        /// </summary>
        /// <param name="baseWeaponName">The name of the weapon to use as a template.</param>
        /// <param name="newName">The new name for the modified weapon.</param>
        /// <param name="modifications">An action to apply custom changes to the new weapon.</param>
        /// <returns>A new, modified MeleeWeapon instance, or null if the base weapon doesn't exist.</returns>
        public MeleeWeapon CreateModifiedMeleeWeapon(
            string baseWeaponName,
            string newName,
            Action<MeleeWeapon> modifications)
        {
            var template = EquipmentService.GetWeaponByName(baseWeaponName) as MeleeWeapon ?? new MeleeWeapon();

            var newWeapon = template.Clone();

            newWeapon.Name = newName;

            modifications(newWeapon);

            return newWeapon;
        }

        /// <summary>
        /// Creates a new weapon instance from a base template and applies modifications.
        /// </summary>
        /// <param name="baseWeaponName">The name of the weapon to use as a template.</param>
        /// <param name="newName">The new name for the modified weapon.</param>
        /// <param name="modifications">An action to apply custom changes to the new weapon.</param>
        /// <returns>A new, modified RangedWeapon instance, or null if the base weapon doesn't exist.</returns>
        public RangedWeapon CreateModifiedRangedWeapon(
            string baseWeaponName,
            string newName,
            Action<RangedWeapon> modifications)
        {
            var template = EquipmentService.GetWeaponByName(baseWeaponName) as RangedWeapon ?? new RangedWeapon();

            var newWeapon = template.Clone();

            newWeapon.Name = newName;

            modifications(newWeapon);

            return newWeapon;
        }

        /// <summary>
        /// Creates a new, randomized Magic Staff by combining a base staff with magical properties.
        /// </summary>
        /// <returns>A fully constructed MagicStaff object, or null if the base staff is not found.</returns>
        public MagicStaff CreateMagicStaff(MagicStaff magicStaffTemplate)
        {
            Weapon baseStaff = EquipmentService.GetWeaponByName("Staff") ?? new MeleeWeapon();

            MagicStaff newMagicStaff = new MagicStaff
            {
                // --- Properties from the base Staff ---
                MinDamage = baseStaff.MinDamage,
                MaxDamage = baseStaff.MaxDamage,
                DamageDice = baseStaff.DamageDice,
                Encumbrance = baseStaff.Encumbrance,
                Class = baseStaff.Class,
                Properties = new Dictionary<WeaponProperty, int>(baseStaff.Properties), // Copy the dictionary

                // --- Properties from the selected MagicStaff template ---
                Category = magicStaffTemplate.Category,
                Name = magicStaffTemplate.Name,
                Description = magicStaffTemplate.Description,
                Value = magicStaffTemplate.Value,
                Availability = magicStaffTemplate.Availability,
                StaffType = magicStaffTemplate.StaffType,
                ContainedSpell = magicStaffTemplate.ContainedSpell,
                MagicStaffProperties = new Dictionary<MagicStaffProperty, int>(magicStaffTemplate.MagicStaffProperties ?? new Dictionary<MagicStaffProperty, int>())
            };

            return newMagicStaff;
        }
    }

    public class ArmourFactory
    {
        public Armour CreateModifiedArmour(
            string baseArmourName,
            string newName,
            Action<Armour> modifications)
        {
            // 1. Find the base template.
            Armour template = EquipmentService.GetArmourByName(baseArmourName) ?? new Armour();

            Armour newArmour = template.Clone();
            newArmour.Name = newName;

            modifications(newArmour);

            return newArmour;
        }

        public Shield CreateModifiedShield(
            string baseShieldName,
            string newName,
            Action<Shield> modifications)
        {
            // 1. Find the base template.
            Shield template = EquipmentService.GetShieldByName(baseShieldName) ?? new Shield();

            Shield newShield = template.Clone();
            newShield.Name = newName;

            modifications(newShield);

            return newShield;
        }
    }
}
