using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Player
{
    /// <summary>
    /// Defines the possible inventory locations for an item.
    /// </summary>
    public enum ItemSlot
    {
        Backpack,
        QuickSlot,
        EquippedWeapon,
        EquippedArmour,
        DualWield,
        Shield,
        Torch,
        Lantern,
        Quiver
    }

    /// <summary>
    /// Manages hero inventory actions like equipping and rearranging gear.
    /// </summary>
    public class InventoryService
    {
        public InventoryService() { }

        /// <summary>
        /// Equips an item from a hero's backpack.
        /// </summary>
        public static void EquipItem(Hero hero, Equipment item)
        {
            if (!hero.Backpack.Contains(item)) return;

            if (item is Weapon weapon)
            {
                // Simple logic: unequip the current weapon and equip the new one.
                // A more complex system could handle dual-wielding.
                if (hero.Weapons.Any())
                {
                    UnequipItem(hero, hero.Weapons.First());
                }
                hero.Weapons.Add(weapon);
                BackpackHelper.RemoveItem(hero.Backpack, item);
            }
            else if (item is Armour armour)
            {
                // Handle equipping armor, potentially swapping with an existing piece.
                // This would involve checking the armor's slot (Head, Torso, etc.).
            }
            // Add logic for other equippable item types here...
        }

        /// <summary>
        /// Unequips an item and moves it back to the hero's backpack.
        /// </summary>
        public static void UnequipItem(Hero hero, Equipment item)
        {
            if (item is Weapon weapon && hero.Weapons.Contains(weapon))
            {
                hero.Weapons.Remove(weapon);
                BackpackHelper.AddItem(hero.Backpack, item);
            }
            else if (item is Armour armour && hero.Armours.Contains(armour))
            {
                hero.Armours.Remove(armour);
                BackpackHelper.AddItem(hero.Backpack, item);
            }
            // Add logic for other equippable item types here...
        }


        public List<Equipment>? GetListFromSlot(Hero hero, ItemSlot slot)
        {
            return hero.Backpack.Where(i => i.ItemSlot == slot).ToList();
        }

        private bool AddItemToSlot(Hero hero, Equipment item, ItemSlot slot)
        {
            var sourceList = GetListFromSlot(hero, slot);
            if (sourceList == null) return false;
            // Check if the item already exists in the slot
            var existingItem = sourceList.FirstOrDefault(i => i.Name == item.Name);
            if (existingItem != null)
            {
                // If it exists, increase the quantity
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                // If it doesn't exist, create a new instance with quantity 1
                var newItem = CreateSingleInstanceOfItem(item);
                sourceList.Add(newItem);
            }
            return true;
        }

        private bool RemoveItemFromSlot(Hero hero, Equipment item, ItemSlot slot)
        {
            var sourceList = GetListFromSlot(hero, slot);
            if (sourceList == null) return false;

            var itemInList = sourceList.FirstOrDefault(i => i.Name == item.Name);
            if (itemInList != null)
            {
                if (itemInList.Quantity > 1)
                {
                    itemInList.Quantity--;
                }
                else
                {
                    sourceList.Remove(itemInList);
                }
                return true;
            }
            return false;
        }

        private Equipment CreateSingleInstanceOfItem(Equipment originalItem)
        {
            var newItem = new Equipment
            {
                Name = originalItem.Name,
                Description = originalItem.Description,
                Value = originalItem.Value,
                Encumbrance = originalItem.Encumbrance,
                MaxDurability = originalItem.MaxDurability,
                Durability = originalItem.Durability,
                MagicEffect = originalItem.MagicEffect,
                Quantity = 1 // Ensure the new instance has a quantity of 1 for the slot
            };
            // This would need to be expanded to handle specific weapon/armour properties
            // if you were moving those types.
            return newItem;
        }
    }
}
