using LoDCompanion.Models;

namespace LoDCompanion.Services.GameData
{
    public class EquipmentService
    {

        public static Equipment GetEquipmentByName(GameDataService _gameData, string name)
        {
            return _gameData.Equipment.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }

        public static Equipment GetEquipmentByNameSetQuantity(GameDataService _gameData, string name, int qty)
        {
            Equipment item = GetEquipmentByName(_gameData, name);
            if (item == null) throw new NullReferenceException();
            item.Quantity = qty;
            return item;
        }

        public static Equipment GetEquipmentByNameSetDurabilitySetQuantity(GameDataService _gameData, string name, int durability, int qty = 1)
        {
            Equipment item = GetEquipmentByName(_gameData, name);
            if (item == null) throw new NullReferenceException();
            item.Quantity = qty;
            item.Durability = durability;
            return item;
        }

        public static List<Equipment> GetStartingEquipment(GameDataService _gameData)
        {
            List<Equipment> list = new List<Equipment>();
            list.AddRange(_gameData.Equipment.Where(x => x.Availability > 3));
            return list;
        }

        public static Ammo GetAmmoByName(GameDataService _gameData, string name)
        {
            return _gameData.Ammo.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }

        public static Ammo GetAmmoByNameSetQuantity(GameDataService _gameData, string name, int qty)
        {
            Ammo item = GetAmmoByName(_gameData, name);
            item.Quantity = qty;
            return item;
        }

        public static List<Ammo> GetStartingAmmo(GameDataService _gameData)
        {
            List<Ammo> list = new List<Ammo>();
            list.AddRange(_gameData.Ammo.Where(x => x.Availability > 3));
            return list;
        }

        public static MeleeWeapon GetMeleeWeaponByName(GameDataService _gameData, string name)
        {
            return _gameData.MeleeWeapons.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }

        public static RangedWeapon GetRangedWeaponByName(GameDataService _gameData, string name)
        {
            return _gameData.RangedWeapons.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }

        public static Weapon GetWeaponByName(GameDataService _gameData, string name)
        {
            return (Weapon)GetMeleeWeaponByName(_gameData, name) ?? 
                (Weapon)GetRangedWeaponByName(_gameData, name) ?? throw new NullReferenceException();
        }

        public static Weapon GetWeaponByNameSetDurability(GameDataService _gameData, string name, int durability)
        {
            Weapon weapon = GetWeaponByName(_gameData, name);
            weapon.Durability = durability;
            return weapon;
        }

        public static List<Equipment> GetStartingWeapons(GameDataService _gameData)
        {
            List<Equipment> list = new List<Equipment>();
            list.AddRange(_gameData.MeleeWeapons.Where(x => x.Availability > 3));
            list.AddRange(_gameData.RangedWeapons.Where(x => x.Availability > 3));
            return list;
        }

        public static MagicStaff GetMagicStaffByName(GameDataService _gameData, string name)
        {
            return _gameData.MagicStaves.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }

        public static Armour GetArmourByName(GameDataService _gameData, string name)
        {
            return _gameData.Armour.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }
        public static Armour GetArmourByNameSetDurability(GameDataService _gameData, string name, int durability)
        {
            Armour armour = _gameData.Armour.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
            armour.Durability = durability;
            return armour;
        }

        public static List<Armour> GetStartingArmour(GameDataService _gameData)
        {
            List<Armour> list = new List<Armour>();
            list.AddRange(_gameData.Armour.Where(x => x.Availability > 3));
            return list;
        }

        public static Shield GetShieldByName(GameDataService _gameData, string name)
        {
            return _gameData.Shields.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }

        public static Shield GetShieldByNameSetDurability(GameDataService _gameData, string name, int durability)
        {
            Shield shield = GetShieldByName(_gameData, name) ?? throw new NullReferenceException();
            shield.Durability = durability;
            return shield;
        }

        public static List<Shield> GetStartingShields(GameDataService _gameData)
        {
            List<Shield> list = new List<Shield>();
            list.AddRange(_gameData.Shields.Where(x => x.Availability > 3));
            return list;
        }

        public static Equipment GetRelicByName(GameDataService _gameData, string name)
        {
            return _gameData.Relics.FirstOrDefault(x => x.Name == name) ?? throw new NullReferenceException();
        }
    }
}
