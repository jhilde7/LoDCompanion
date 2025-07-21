using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Utilities;
using System.Text;
using BlazorContextMenu;
using LoDCompanion.Services.GameData;
using System.Diagnostics.Metrics;

namespace LoDCompanion.Services.Player
{
    public class Inventory
    {
        // Equipped Gear
        public Weapon? EquippedWeapon { get; set; }
        public List<Armour> EquippedArmour { get; set; } = new List<Armour>();
        public Ammo? EquippedQuiver { get; set; }
        public Equipment? OffHand { get; set; }

        // Carried Items
        public List<Equipment> Backpack { get; set; } = new List<Equipment>();
        public List<Equipment> QuickSlots { get; set; } = [.. new Equipment[3]];

        public int MaxQuickSlots { get; set; } = 3;

        public Inventory() { }
    }

    /// <summary>
    /// Manages hero inventory actions like equipping and rearranging gear.
    /// </summary>
    public class InventoryService
    {
        public InventoryService() 
        { 
           
        }

        public bool AddItemToQuickSlot(Hero hero, Equipment item)
        {
            var itemInBackpack = hero.Inventory.Backpack.FirstOrDefault(i => i.Name == item.Name);
            if (itemInBackpack != null)
            {
                var itemToMove = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, itemInBackpack);
                if(itemToMove != null)
                {
                    hero.Inventory.QuickSlots.Add(itemToMove);
                    if(hero.Inventory.QuickSlots.Count > hero.Inventory.MaxQuickSlots)
                    {
                        hero.Inventory.QuickSlots.RemoveAt(0);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool EquipItem(Hero hero, Equipment item)
        {
            // Take a single instance of the item from the backpack stack.
            Equipment? itemToEquip = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, item);
            if (itemToEquip == null) return false;

            bool success = false;
            // Route to the correct handler based on the item's type.
            if (itemToEquip is Weapon weapon) success = EquipWeapon(hero, weapon);
            else if (itemToEquip is Armour armour) success = EquipArmour(hero, armour);
            else if (itemToEquip is Shield shield) success = EquipOffHand(hero, shield);
            else if (itemToEquip is Ammo ammo) success = EquipAmmo(hero, ammo);
            else if (itemToEquip.HasProperty(EquipmentProperty.Lantern) || itemToEquip.HasProperty(EquipmentProperty.Torch))
            {
                success = EquipOffHand(hero, itemToEquip);
            }

            // If equipping failed for any reason, put the item back in the backpack.
            if (!success)
            {
                BackpackHelper.AddItem(hero.Inventory.Backpack, itemToEquip);
                return false;
            }
            return true;
        }

        public bool UnequipItem(Hero hero, Equipment itemToUnequip)
        {
            bool removed = false;
            if (hero.Inventory.EquippedWeapon == itemToUnequip) { hero.Inventory.EquippedWeapon = null; removed = true; }
            else if (hero.Inventory.OffHand == itemToUnequip) { hero.Inventory.OffHand = null; removed = true; }
            else if (hero.Inventory.EquippedQuiver == itemToUnequip) { hero.Inventory.EquippedQuiver = null; removed = true; }
            else if (hero.Inventory.EquippedArmour.Contains(itemToUnequip))
            {
                hero.Inventory.EquippedArmour.Remove((Armour)itemToUnequip);
                removed = true;
            }

            if (removed)
            {
                BackpackHelper.AddItem(hero.Inventory.Backpack, itemToUnequip);
                Console.WriteLine($"Unequipped {itemToUnequip.Name}.");
                return true;
            }
            return false;
        }

        private bool FreeUpOffHand(Hero hero)
        {
            // Remove all offhand items (dual wield, shield, secondary, lantern, torch)
            if (hero.Inventory.OffHand != null)
            {
                UnequipItem(hero, hero.Inventory.OffHand);
                Console.WriteLine($"Unequipped {hero.Inventory.OffHand.Name} from {hero.Name}'s off-hand.");
            }
            return true;
        }

        private bool EquipAmmo(Hero hero, Ammo ammoToEquip)
        {
            EmptyQuiver(hero, hero.Inventory);
            int quantityToMove = Math.Min(10, ammoToEquip.Quantity);
            var quiverStack = new Ammo(ammoToEquip) { Quantity = quantityToMove };
            ammoToEquip.Quantity -= quantityToMove;

            hero.Inventory.EquippedQuiver = quiverStack;
            Console.WriteLine($"{hero.Name}'s quiver was loaded with {quantityToMove} {ammoToEquip.Name}s.");
            return false;
        }

        private bool EmptyQuiver(Hero hero, Inventory inventory)
        {
            if (inventory.EquippedQuiver != null)
            {
                UnequipItem(hero, inventory.EquippedQuiver);
            }
            return true;
        }

        private bool EquipWeapon(Hero hero, Weapon weaponToEquip)
        {
            Weapon? equippedWeapon = hero.Inventory.EquippedWeapon;
            if (equippedWeapon == null)
            {
                hero.Inventory.EquippedWeapon = weaponToEquip;
                Console.WriteLine($"Equipped {weaponToEquip.Name} to {hero.Name}'s equipped weapon slot.");
                return true;
            }

            if (weaponToEquip is MeleeWeapon melee)
            {
                if (melee.Class > hero.TwoHandedWeaponClass)
                {
                    Console.WriteLine($"Cannot equip {melee.Name}, {hero.Name} is too weak to use this weapon.");
                    return false;
                }
                else if (melee.Class > hero.OneHandedWeaponClass) // equip two-handed weapon
                {
                    return EquipTwoHandedWeapon(hero, melee);
                }
                else if (hero.Talents.Any(t => t.Name == "Dual Wield")
                    && melee.HasProperty(WeaponProperty.DualWield)
                     && hero.Inventory.EquippedWeapon != null)
                {
                    return EquipOffHand(hero, melee);
                }
                else // replace one-handed weapon
                {
                    if (equippedWeapon != null)
                    {
                        UnequipItem(hero, equippedWeapon);
                        Console.WriteLine($"Unequipped {equippedWeapon.Name} from {hero.Name}'s equipped weapon slot.");
                    }
                    hero.Inventory.EquippedWeapon = weaponToEquip;
                    Console.WriteLine($"Equipped {melee.Name} to {hero.Name}'s equipped weapon slot.");
                    return true;
                }
            }
            else if (weaponToEquip.Properties.ContainsKey(WeaponProperty.SecondaryWeapon))
            {
                return EquipOffHand(hero, (RangedWeapon)weaponToEquip);
            }
            else // equipped ranged weapon
            {
                return EquipTwoHandedWeapon(hero, weaponToEquip);
            }
        }

        private bool EquipTwoHandedWeapon(Hero hero, Weapon weaponToEquip)
        {
            Weapon? equippedWeapon = hero.Inventory.EquippedWeapon;
            if (equippedWeapon != null)
            {
                UnequipItem(hero, equippedWeapon);
            }
            FreeUpOffHand(hero);
            hero.Inventory.EquippedWeapon = weaponToEquip;
            Console.WriteLine($"Equipped {weaponToEquip.Name} with both hands, to {hero.Name}'s equipped weapon slot.");
            return true;
        }

        private bool EquipOffHand(Hero hero, Equipment itemToEquip)
        {
            // Rule: Cannot equip an off-hand item if using a two-handed weapon.
            if (hero.Inventory.EquippedWeapon != null && hero.Inventory.EquippedWeapon.Class > hero.OneHandedWeaponClass)
            {
                Console.WriteLine($"Cannot equip {itemToEquip.Name}, main hand is using a two-handed weapon.");
                return false;
            }

            // Clear the off-hand slot before equipping the new item.
            if (hero.Inventory.OffHand != null)
            {
                UnequipItem(hero, hero.Inventory.OffHand);
            }

            hero.Inventory.OffHand = itemToEquip;
            Console.WriteLine($"Equipped {itemToEquip.Name} in off-hand.");
            return true;
        }

        private readonly HashSet<ArmourProperty> _bodySlots = new()
        {
            ArmourProperty.Head,
            ArmourProperty.Torso,
            ArmourProperty.Arms,
            ArmourProperty.Legs,
            ArmourProperty.Cloak
        };

        private bool EquipArmour(Hero hero, Armour armour)
        {
            Armour? armourToEquip = (Armour?)BackpackHelper.TakeOneItem(hero.Inventory.Backpack, armour);
            if (armourToEquip == null) return false;
            var slotToOccupy = armourToEquip.Properties.Keys.FirstOrDefault(p => _bodySlots.Contains(p));
            if (slotToOccupy == default)
            {
                Console.WriteLine($"{armourToEquip.Name} does not have a valid body slot and cannot be equipped.");
                return false;
            }

            List<Armour> equippedArmour = hero.Inventory.EquippedArmour;

            var itemsInSlot = equippedArmour.Where(a => a.Properties.ContainsKey(slotToOccupy)).ToList();
            var itemsToUnequip = GetItemsToUnequip(armourToEquip, itemsInSlot);

            if (itemsToUnequip != null)
            {
                foreach (var item in itemsToUnequip)
                {
                    UnequipItem(hero, item); // This helper now handles its own console message.
                } 
            }

            equippedArmour.Add(armourToEquip);
            return true;
        }

        private List<Armour>? GetItemsToUnequip(Armour itemToEquip, List<Armour> itemsInSlot)
        {
            var unequipList = new List<Armour>();
            // If all the slots are empty, we don't need to unequip anything.
            if (!itemsInSlot.Any())
            {
                return null;
            }

            // --- Logic for NON-STACKABLE items ---
            if (!itemToEquip.HasProperty(ArmourProperty.Stackable))
            {
                var sameClassItem = itemsInSlot.FirstOrDefault(i => i.ArmourClass == itemToEquip.ArmourClass);
                var nonStackableItem = itemsInSlot.FirstOrDefault(i => !i.HasProperty(ArmourProperty.Stackable));
                if (nonStackableItem != null) unequipList.Add(nonStackableItem);
                if (sameClassItem != null) unequipList.Add(sameClassItem);
                return unequipList;
            }

            // --- Logic for STACKABLE items ---
            var conflictingItem = itemsInSlot.FirstOrDefault(i => i.ArmourClass == itemToEquip.ArmourClass);
            if (conflictingItem != null)
            {
                return new List<Armour> { conflictingItem };
            }

            const int maxStackableItems = 2;
            if (itemsInSlot.Count >= maxStackableItems)
            {
                var nonStackableItem = itemsInSlot.FirstOrDefault(i => !i.HasProperty(ArmourProperty.Stackable));
                var stackableItem = itemsInSlot.FirstOrDefault(i => i.HasProperty(ArmourProperty.Stackable));
                if (nonStackableItem != null && stackableItem != null
                    && nonStackableItem.DefValue > stackableItem.DefValue) new List<Armour> { stackableItem };
                if (nonStackableItem != null && stackableItem != null
                    && nonStackableItem.DefValue < stackableItem.DefValue) new List<Armour> { nonStackableItem };
            }

            // If no specific item needs to be replaced, equip alongside the others.
            return null;
        }

    }
}
