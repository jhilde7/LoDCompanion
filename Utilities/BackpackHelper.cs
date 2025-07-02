using LoDCompanion.Models;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Utilities
{
    public static class BackpackHelper
    {
        public static void AddItem(List<Equipment> backpack, Equipment itemToAdd)
        {
            // We only stack Potions and Parts. Other items like weapons or recipes are unique.
            bool isStackable = itemToAdd is Potion || itemToAdd is Part;

            if (isStackable)
            {
                var existingItem = backpack.FirstOrDefault(item => item.Name == itemToAdd.Name);

                if (existingItem != null)
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
            else
            {
                // If the item is not stackable (e.g., a weapon, armor, or recipe), add it as a new entry.
                backpack.Add(itemToAdd);
            }
        }
    }
}
