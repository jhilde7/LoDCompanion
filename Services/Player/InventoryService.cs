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
        EquippedArmour
    }

    /// <summary>
    /// Manages hero inventory actions like equipping and rearranging gear.
    /// </summary>
    public class InventoryService
    {
        public InventoryService() { }

        /// <summary>
        /// Moves an item from one slot to another for a hero.
        /// </summary>
        public string RearrangeItem(Hero hero, Equipment itemToMove, ItemSlot fromSlot, ItemSlot toSlot)
        {
            // Moving to the same slot does nothing.
            if (fromSlot == toSlot) return "Cannot move an item to the same slot.";

            var sourceList = GetListFromSlot(hero, fromSlot);
            var destinationList = GetListFromSlot(hero, toSlot);

            if (sourceList == null || destinationList == null)
            {
                return "Invalid item slot specified.";
            }

            // --- Handle the four movement scenarios ---

            // 1. Moving FROM Backpack TO a Slot (Unstacking)
            if (fromSlot == ItemSlot.Backpack && toSlot != ItemSlot.Backpack)
            {
                // Check if the destination slot is already occupied.
                // A more complex implementation could check for max quick slots.
                if (destinationList.Any())
                {
                    return $"The {toSlot} is already full.";
                }

                var itemInstanceToAdd = CreateSingleInstanceOfItem(itemToMove);
                destinationList.Add(itemInstanceToAdd);
                BackpackHelper.RemoveItem(sourceList, itemToMove); // Use helper to de-stack from backpack

                return $"{hero.Name} moved a {itemToMove.Name} from their backpack to their {toSlot}.";
            }

            // 2. Moving FROM a Slot TO Backpack (Stacking)
            if (fromSlot != ItemSlot.Backpack && toSlot == ItemSlot.Backpack)
            {
                sourceList.Remove(itemToMove);
                BackpackHelper.AddItem(destinationList, itemToMove); // Use helper to stack in backpack

                return $"{hero.Name} stored their {itemToMove.Name} in their backpack.";
            }

            // 3. Moving FROM a Slot TO another Slot (Swapping)
            if (fromSlot != ItemSlot.Backpack && toSlot != ItemSlot.Backpack)
            {
                // This is a swap. We'll assume for now the destination is empty.
                if (destinationList.Any())
                {
                    return $"Cannot move {itemToMove.Name}, the {toSlot} is already occupied.";
                }
                sourceList.Remove(itemToMove);
                destinationList.Add(itemToMove);

                return $"{hero.Name} moved their {itemToMove.Name} from {fromSlot} to {toSlot}.";
            }

            return "Invalid inventory operation.";
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

        private List<Equipment>? GetListFromSlot(Hero hero, ItemSlot slot)
        {
            switch (slot)
            {
                case ItemSlot.Backpack:
                    return hero.Backpack;
                case ItemSlot.QuickSlot:
                    return hero.QuickSlots;
                case ItemSlot.EquippedWeapon:
                    return hero.Weapons.Cast<Equipment>().ToList();
                case ItemSlot.EquippedArmour:
                    return hero.Armours.Cast<Equipment>().ToList();
                default:
                    return null;
            }
        }
    }
}
