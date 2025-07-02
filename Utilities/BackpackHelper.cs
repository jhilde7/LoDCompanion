using LoDCompanion.Models;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Utilities
{
    public static class BackpackHelper
    {
        public static void AddItem(List<Equipment> backpack, Equipment itemToAdd)
        {
            var existingItem = backpack.FirstOrDefault(item => item.Name == itemToAdd.Name);

            if (existingItem != null && existingItem.Durability == itemToAdd.Durability)
            {
                // Item exists, so just increase the quantity
                existingItem.Quantity += itemToAdd.Quantity;
            }
            else
            {
                // Item is new, so add it to the list
                backpack.Add(itemToAdd);
            }
        }

        public static void RemoveItem(List<Equipment> backpack, Equipment itemToRemove)
        {
            var existingItem = backpack.FirstOrDefault(item => item == itemToRemove);

            if(existingItem.Quantity > 1)
            {
                existingItem.Quantity -= existingItem.Quantity;
            }
            else
            {
                backpack.Remove(itemToRemove);
            }
        }
    }
}
