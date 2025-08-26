using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class Inventory
    {
        // Equipped Gear
        public Weapon? EquippedWeapon { get; set; }
        public List<Armour> EquippedArmour { get; set; } = new List<Armour>();
        public Ammo? EquippedQuiver { get; set; }
        public Equipment? OffHand { get; set; }
        public Equipment? EquippedRelic { get; set; }
        public Equipment? EquippedStorage { get; set; }

        // Carried Items
        public List<Equipment?> Backpack { get; set; } = new List<Equipment?>();
        public List<Equipment?> QuickSlots { get; set; } = [.. new Equipment?[3]];
        public int MaxQuickSlots => QuickSlots.Count;

        public bool CanBrewPotion => HasBrewPotionItems();

        public Inventory() { }

        public Inventory(int slots)
        {
            QuickSlots = [.. new Equipment?[slots]];
        }

        private bool HasBrewPotionItems()
        {
            return 
                Backpack.Where(i => i is Part).Count() >= 1 
                && Backpack.Where(i => i is Ingredient).Count() >= 1 
                && Backpack.Where(i => i != null && i.Name == "Empty Bottle").Count() >= 1
                && Backpack.FirstOrDefault(i => i != null && i.Name == "Alchemist Tool") != null;
        }
    }

    /// <summary>
    /// Manages hero inventory actions like equipping and rearranging gear.
    /// </summary>
    public class InventoryService
    {
        public InventoryService()
        {

        }

        /// <summary>
        /// Assigns an item from the backpack to a specific quick slot.
        /// </summary>
        /// <param name="hero">The hero whose inventory is being modified.</param>
        /// <param name="itemToSlot">The item to be moved to the quick slot.</param>
        /// <param name="slotIndex">The 0-based index of the quick slot to use.</param>
        /// <returns>True if the item was successfully slotted.</returns>
        public bool AssignItemToQuickSlot(Hero hero, Equipment itemToSlot, int slotIndex)
        {
            // Validate the slot index
            if (slotIndex < 0 || slotIndex >= hero.Inventory.MaxQuickSlots)
            {
                Console.WriteLine("Invalid quick slot index.");
                return false;
            }

            // Ensure the item exists in the backpack
            var itemInBackpack = hero.Inventory.Backpack.FirstOrDefault(i => i == itemToSlot);
            if (itemInBackpack == null)
            {
                Console.WriteLine("Item not found in backpack.");
                return false;
            }

            // Take one item from the backpack stack
            var movedItem = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, itemInBackpack);
            if (movedItem == null) return false;

            // If an item is already in the target slot, move it back to the backpack
            var existingItem = hero.Inventory.QuickSlots[slotIndex];
            if (existingItem != null)
            {
                BackpackHelper.AddItem(hero.Inventory.Backpack, existingItem);
                Console.WriteLine($"Moved {existingItem.Name} from quick slot back to backpack.");
            }

            // Place the new item in the specified quick slot
            hero.Inventory.QuickSlots[slotIndex] = movedItem;
            Console.WriteLine($"Assigned {movedItem.Name} to quick slot {slotIndex + 1}.");
            return true;
        }

        /// <summary>
        /// Assigns an item from the backpack to a specific slot within a container.
        /// </summary>
        public bool AssignItemToEquipmentQuickSlot(Hero hero, Equipment itemToSlot, Equipment container, int slotIndex)
        {
            // Ensure the target container actually has storage.
            if (container != hero.Inventory.EquippedStorage ||
                container.Storage == null || slotIndex < 0 || slotIndex >= container.Storage.MaxQuickSlots)
            {
                return false; // Invalid container or slot index.
            }

            var movedItem = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, itemToSlot);
            if (movedItem == null) return false;

            // If a different item is in the target slot, move it back to the backpack.
            var existingItem = container.Storage.QuickSlots[slotIndex];
            if (existingItem != null)
            {
                BackpackHelper.AddItem(hero.Inventory.Backpack, existingItem);
            }

            // Place the new item in the container's slot.
            container.Storage.QuickSlots[slotIndex] = movedItem;
            return true;
        }

        public bool EquipItem(Hero hero, Equipment item)
        {
            bool success = false;
            if (item is Ammo ammo) success = EquipAmmo(hero, ammo);

            // Take a single instance of the item from the backpack stack.
            Equipment? itemToEquip = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, item);
            if (itemToEquip == null) return false;

            // Route to the correct handler based on the item's type.
            if (itemToEquip is Weapon weapon) success = EquipWeapon(hero, weapon);
            else if (itemToEquip is Armour armour) success = EquipArmour(hero, armour);
            else if (itemToEquip is Shield shield) success = EquipOffHand(hero, shield);
            else if (itemToEquip.HasProperty(EquipmentProperty.Lantern)
                || itemToEquip.HasProperty(EquipmentProperty.Torch))
            {
                success = EquipOffHand(hero, itemToEquip);
            }
            else if (hero.ProfessionName == "Warrior Priest"
                && itemToEquip.Name.Contains("Relic")) success = EquipRelic(hero, itemToEquip);
            else if (item.Storage != null)
            {
                success = EquipStorageContainer(hero, item);
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

            if (hero.Inventory.EquippedStorage == itemToUnequip)
            {
                // Empty the container's contents back into the backpack FIRST.
                if (itemToUnequip.Storage != null)
                {
                    foreach (var itemInContainer in itemToUnequip.Storage.QuickSlots)
                    {
                        if (itemInContainer != null)
                        {
                            BackpackHelper.AddItem(hero.Inventory.Backpack, itemInContainer);
                        }
                    }
                    // Clear the container's slots by filling it with nulls
                    for (int i = 0; i < itemToUnequip.Storage.QuickSlots.Count; i++)
                    {
                        itemToUnequip.Storage.QuickSlots[i] = null;
                    }
                }

                hero.Inventory.EquippedStorage = null;
                removed = true;
            }
            else if (hero.Inventory.EquippedWeapon == itemToUnequip) { hero.Inventory.EquippedWeapon = null; removed = true; }
            else if (hero.Inventory.OffHand == itemToUnequip) { hero.Inventory.OffHand = null; removed = true; }
            else if (hero.Inventory.EquippedQuiver == itemToUnequip) { hero.Inventory.EquippedQuiver = null; removed = true; }
            else if (hero.Inventory.EquippedArmour.Contains(itemToUnequip))
            {
                hero.Inventory.EquippedArmour.Remove((Armour)itemToUnequip);
                removed = true;
            }
            else if (hero.Inventory.QuickSlots.Contains(itemToUnequip))
            {
                int index = hero.Inventory.QuickSlots.IndexOf(itemToUnequip);
                if (index != -1)
                {
                    hero.Inventory.QuickSlots[index] = null;
                    removed = true;
                }
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

        private bool EquipStorageContainer(Hero hero, Equipment containerToEquip)
        {
            // If another container is already equipped, unequip it first.
            if (hero.Inventory.EquippedStorage != null)
            {
                UnequipItem(hero, hero.Inventory.EquippedStorage);
            }

            hero.Inventory.EquippedStorage = containerToEquip;
            return true;
        }

        private bool EquipRelic(Hero hero, Equipment relicToEquip)
        {
            if (hero.Inventory.EquippedRelic != null)
            {
                UnequipItem(hero, hero.Inventory.EquippedRelic);
                Console.WriteLine($"Unequipped {hero.Inventory.EquippedRelic.Name} from {hero.Name}'s relic slot.");
            }
            hero.Inventory.EquippedRelic = relicToEquip;
            Console.WriteLine($"Equipped {relicToEquip.Name} to {hero.Name}'s relic slot.");
            return true;
        }

        private bool EquipAmmo(Hero hero, Ammo ammoToEquip)
        {
            EmptyQuiver(hero, hero.Inventory);
            var backpackStack = (Ammo?)hero.Inventory.Backpack.FirstOrDefault(a => a != null && a.Name == ammoToEquip.Name);
            if (backpackStack != null)
            {
                int quantityToMove = Math.Min(10, backpackStack.Quantity);
                var quiverStack = backpackStack.Clone();
                quiverStack.Quantity = quantityToMove;
                backpackStack.Quantity -= quantityToMove;
                hero.Inventory.EquippedQuiver = quiverStack;
                Console.WriteLine($"{hero.Name}'s quiver was loaded with {quantityToMove} {ammoToEquip.Name}s.");
                return true;
            }
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
                else if (hero.Talents.Any(t => t.Name == TalentName.DualWield)
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

        private bool EquipArmour(Hero hero, Armour armourToEquip)
        {
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
