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

        public static void RemoveSingleItem(List<Equipment> backpack, Equipment itemToRemove)
        {
            var existingItem = backpack.FirstOrDefault(item => item == itemToRemove);

            if(existingItem != null && existingItem.Quantity > 1)
            {
                existingItem.Quantity -= 1;
            }
            else
            {
                backpack.Remove(itemToRemove);
            }
        }

        internal static Equipment? TakeOneItem(List<Equipment> backpack, Equipment item)
        {
            var itemInBackPack = backpack.FirstOrDefault(i => i.Name == item.Name);

            if (itemInBackPack != null)
            {
                if (itemInBackPack is MeleeWeapon melee)
                {
                    var meleeWeapon = new MeleeWeapon(melee);
                    melee.Quantity -= 1;
                    if (melee.Quantity <= 0)
                    {
                        backpack.Remove(meleeWeapon);
                    }
                    return meleeWeapon;
                }
                else if (itemInBackPack is RangedWeapon ranged)
                {
                    var rangedWeapon = new RangedWeapon(ranged);
                    ranged.Quantity -= 1;
                    if (ranged.Quantity <= 0)
                    {
                        backpack.Remove(rangedWeapon);
                    }
                    return rangedWeapon;
                }
                else if (itemInBackPack is Armour armour)
                {
                    var armourItem = new Armour(armour);
                    armour.Quantity -= 1;
                    if (armour.Quantity <= 0)
                    {
                        backpack.Remove(armourItem);
                    }
                    return armourItem;
                }
                else if (itemInBackPack is Shield shield)
                {
                    var shieldItem = new Shield(shield);
                    shield.Quantity -= 1;
                    if (shield.Quantity <= 0)
                    {
                        backpack.Remove(shieldItem);
                    }
                    return shieldItem;
                }
                else if (itemInBackPack is Ammo ammo)
                {
                    var ammoItem = new Ammo(ammo);
                    ammo.Quantity -= 1;
                    if (ammo.Quantity <= 0)
                    {
                        backpack.Remove(ammoItem);
                    }
                    return ammoItem;
                }
                else if (itemInBackPack is MagicStaff mStaff)
                {
                    var staff = new MagicStaff(mStaff);
                    mStaff.Quantity -= 1;
                    if (mStaff.Quantity <= 0)
                    {
                        backpack.Remove(staff);
                    }
                    return staff;
                }
                else
                {
                    var singleItem = new Equipment(itemInBackPack);
                    itemInBackPack.Quantity -= 1;
                    if (itemInBackPack.Quantity <= 0)
                    {
                        backpack.Remove(singleItem);
                    }
                    return singleItem;
                }
            }
            return null;
        }
    }
}
