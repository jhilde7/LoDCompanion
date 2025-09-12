using LoDCompanion.Code.BackEnd.Services.GameData;
using System.Threading.Tasks;

namespace LoDCompanion.Code.BackEnd.Services.Utilities
{
    public static class BackpackHelper
    {
        public static event Func<Equipment, Task>? OnIdentifyItemAsync;
        public static async Task AddItem(List<Equipment?> backpack, Equipment itemToAdd)
        {
            if (!itemToAdd.Identified && OnIdentifyItemAsync != null) await OnIdentifyItemAsync.Invoke(itemToAdd);
            var existingItem = backpack.FirstOrDefault(item => item != null && item.Name == itemToAdd.Name);

            if (existingItem != null && existingItem.Durability == itemToAdd.Durability && existingItem.Identified == itemToAdd.Identified)
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

        public static void RemoveSingleItem(List<Equipment?> backpack, Equipment itemToRemove)
        {
            var existingItem = backpack.FirstOrDefault(item => item == itemToRemove);

            if (existingItem != null && existingItem.Quantity > 1)
            {
                existingItem.Quantity -= 1;
            }
            else
            {
                backpack.Remove(itemToRemove);
            }
        }

        internal static Equipment? TakeOneItem(List<Equipment?> backpack, Equipment item)
        {
            var itemInBackPack = backpack.FirstOrDefault(i => i != null && i.Name == item.Name);

            if (itemInBackPack != null)
            {
                if (itemInBackPack is MeleeWeapon melee)
                {
                    var meleeWeapon = melee.Clone();
                    meleeWeapon.Quantity = 1; // Ensure we return a single item
                    melee.Quantity -= 1;
                    if (melee.Quantity <= 0)
                    {
                        backpack.Remove(melee);
                    }
                    return meleeWeapon;
                }
                else if (itemInBackPack is RangedWeapon ranged)
                {
                    var rangedWeapon = ranged.Clone();
                    rangedWeapon.Quantity = 1; // Ensure we return a single item
                    ranged.Quantity -= 1;
                    if (ranged.Quantity <= 0)
                    {
                        backpack.Remove(ranged);
                    }
                    return rangedWeapon;
                }
                else if (itemInBackPack is Armour armour)
                {
                    var armourItem = armour.Clone();
                    armourItem.Quantity = 1; // Ensure we return a single item
                    armour.Quantity -= 1;
                    if (armour.Quantity <= 0)
                    {
                        backpack.Remove(armour);
                    }
                    return armourItem;
                }
                else if (itemInBackPack is Shield shield)
                {
                    var shieldItem = shield.Clone();
                    shieldItem.Quantity = 1; // Ensure we return a single item
                    shield.Quantity -= 1;
                    if (shield.Quantity <= 0)
                    {
                        backpack.Remove(shield);
                    }
                    return shieldItem;
                }
                else if (itemInBackPack is Ammo ammo)
                {
                    var ammoItem = ammo.Clone();
                    ammoItem.Quantity = 1; // Ensure we return a single item
                    ammo.Quantity -= 1;
                    if (ammo.Quantity <= 0)
                    {
                        backpack.Remove(ammo);
                    }
                    return ammoItem;
                }
                else if (itemInBackPack is MagicStaff mStaff)
                {
                    var staff = mStaff.Clone();
                    staff.Quantity = 1; // Ensure we return a single item
                    mStaff.Quantity -= 1;
                    if (mStaff.Quantity <= 0)
                    {
                        backpack.Remove(mStaff);
                    }
                    return staff;
                }
                else
                {
                    var singleItem = itemInBackPack.Clone();
                    singleItem.Quantity = 1; // Ensure we return a single item
                    itemInBackPack.Quantity -= 1;
                    if (itemInBackPack.Quantity <= 0)
                    {
                        backpack.Remove(itemInBackPack);
                    }
                    return singleItem;
                }
            }
            return null;
        }
    }
}
