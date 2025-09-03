using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

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
        public List<Equipment> EquippedRings { get; set; } = new List<Equipment>();
        public Equipment? EquippedAmulet { get; set; }
        public Equipment? Mount { get; set; }

        // Carried Items
        public List<Equipment?> Backpack { get; set; } = new List<Equipment?>();
        public List<Equipment?> QuickSlots { get; set; } = [.. new Equipment?[3]];
        public int MaxQuickSlots => QuickSlots.Count;

        private readonly Equipment _equipment = new Equipment();
        public bool CanBrewPotion => HasBrewPotionItems();

        public Inventory() 
        {
            _equipment.OnEquipmentDestroyed += HandleDestroyedEquipment;
        }

        public void Dispose()
        {
            _equipment.OnEquipmentDestroyed -= HandleDestroyedEquipment;
        }

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

        private void HandleDestroyedEquipment(Equipment equipment)
        {
            if (EquippedWeapon == equipment) EquippedWeapon = null;
            else if (OffHand is Shield shield && shield == equipment) OffHand = null;
            else if (QuickSlots.Contains(equipment)) QuickSlots.Remove(equipment);
            else if (Backpack.Contains(equipment)) BackpackHelper.TakeOneItem(Backpack, equipment);
            else if (EquippedArmour.Contains((Armour)equipment)) EquippedArmour.Remove((Armour)equipment);
        }
    }

    /// <summary>
    /// Manages hero inventory actions like equipping and rearranging gear.
    /// </summary>
    public class InventoryService
    {
        private readonly PowerActivationService _powerActivation;
        Hero heroEvent = new Hero();
        public InventoryService(PowerActivationService powerActivation)
        {
            _powerActivation = powerActivation;
            
            heroEvent.OnUnequipWeaponAsync += HandleUnequipWeaponAsync;
        }

        public void Dispose()
        {
            heroEvent.OnUnequipWeaponAsync -= HandleUnequipWeaponAsync;
        }

        private async Task HandleUnequipWeaponAsync(Hero hero, Weapon weapon)
        {
            await UnequipItemAsync(hero, weapon);
        }

        public List<Equipment> GetAllWeaponsArmour(Hero hero)
        {
            var inventory = hero.Inventory;
            List<Equipment> weaponArmourList = [.. inventory.Backpack.Where(item => item != null && (item is Weapon || item is Armour || item is Shield)).ToList(),
            .. inventory.EquippedArmour];
            if (inventory.EquippedWeapon != null) weaponArmourList.Add(inventory.EquippedWeapon);
            if (inventory.OffHand != null && (inventory.OffHand is Weapon || inventory.OffHand is Shield)) weaponArmourList.Add(inventory.OffHand);
            List<Equipment> quickSlots = [.. inventory.QuickSlots];
            if (inventory.EquippedStorage != null && inventory.EquippedStorage.Storage != null && inventory.EquippedStorage.Storage.QuickSlots != null)
                quickSlots.AddRange(inventory.EquippedStorage.Storage.QuickSlots.Cast<Equipment>());
            foreach (var item in quickSlots)
            {
                if (item is Weapon || item is Armour || item is Shield)
                {
                    weaponArmourList.Add(item);
                }
            }

            return weaponArmourList;
        }

        public List<Equipment> GetAllNonWeaponsArmour(Hero hero)
        {
            var inventory = hero.Inventory;
            List<Equipment> equipmentList = [.. inventory.Backpack.Where(item => item != null && !(item is Weapon || item is Armour || item is Shield)).ToList(),
            .. inventory.EquippedArmour];
            if (inventory.OffHand != null && !(inventory.OffHand is Weapon || inventory.OffHand is Shield)) equipmentList.Add(inventory.OffHand);
            List<Equipment> quickSlots = [.. inventory.QuickSlots];
            if (inventory.EquippedStorage != null && inventory.EquippedStorage.Storage != null && inventory.EquippedStorage.Storage.QuickSlots != null)
                quickSlots.AddRange(inventory.EquippedStorage.Storage.QuickSlots.Cast<Equipment>());
            foreach (var item in quickSlots)
            {
                if (!(item is Weapon || item is Armour || item is Shield))
                {
                    equipmentList.Add(item);
                }
            }

            return equipmentList;
        }

        /// <summary>
        /// Assigns an item from the backpack to a specific quick slot.
        /// </summary>
        /// <param name="hero">The hero whose inventory is being modified.</param>
        /// <param name="itemToSlot">The item to be moved to the quick slot.</param>
        /// <param name="slotIndex">The 0-based index of the quick slot to use.</param>
        /// <returns>True if the item was successfully slotted.</returns>
        public async Task<bool> AssignItemToQuickSlotAsync(Hero hero, Equipment itemToSlot, int slotIndex)
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
                await BackpackHelper.AddItem(hero.Inventory.Backpack, existingItem);
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
        public async Task<bool> AssignItemToEquipmentQuickSlotAsync(Hero hero, Equipment itemToSlot, Equipment container, int slotIndex)
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
                await BackpackHelper.AddItem(hero.Inventory.Backpack, existingItem);
            }

            // Place the new item in the container's slot.
            container.Storage.QuickSlots[slotIndex] = movedItem;
            return true;
        }

        public async Task<bool> EquipItemAsync(Hero hero, Equipment item)
        {
            bool success = false;
            if (item is Ammo ammo) success = await EquipAmmoAsync(hero, ammo);

            // Take a single instance of the item from the backpack stack.
            Equipment? itemToEquip = BackpackHelper.TakeOneItem(hero.Inventory.Backpack, item);
            if (itemToEquip == null) return false;

            // Route to the correct handler based on the item's type.
            if (itemToEquip is Weapon weapon) success = await EquipWeaponAsync(hero, weapon);
            else if (itemToEquip is Armour armour) success = await EquipArmourAsync(hero, armour);
            else if (itemToEquip is Shield shield) success = await EquipOffHandAsync(hero, shield);
            else if (itemToEquip.HasProperty(EquipmentProperty.Lantern)
                || itemToEquip.HasProperty(EquipmentProperty.Torch))
            {
                success = await EquipOffHandAsync(hero, itemToEquip);
            }
            else if (hero.ProfessionName == "Warrior Priest"
                && itemToEquip.Name.Contains("Relic")) success = await EquipRelicAsync(hero, itemToEquip);
            else if (item.Storage != null)
            {
                success = await EquipStorageContainerAsync(hero, itemToEquip);
            }
            else if (item.Name.Contains("Ring") || item.Name.Contains("Amulet") || item.Name.Contains("Amulet"))
            {
                success = await EquipRingAmuletAsync(hero, itemToEquip);
            }


            // If equipping failed for any reason, put the item back in the backpack.
            if (!success)
            {
                await BackpackHelper.AddItem(hero.Inventory.Backpack, itemToEquip);
                return false;
            }
            else if (itemToEquip.ActiveStatusEffects != null)
            {
                foreach (var effect in itemToEquip.ActiveStatusEffects)
                {
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, effect, _powerActivation);
                }
            }
            return true;
        }

        public async Task<bool> UnequipItemAsync(Hero hero, Equipment itemToUnequip)
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
                            await BackpackHelper.AddItem(hero.Inventory.Backpack, itemInContainer);
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
            else if (hero.Inventory.EquippedRings.Contains(itemToUnequip))
            {
                hero.Inventory.EquippedRings.Remove(itemToUnequip);
                removed = true;
            }
            else if (hero.Inventory.EquippedAmulet == itemToUnequip) { hero.Inventory.EquippedAmulet = null; removed = true; }

            if (removed)
            {
                if (itemToUnequip.ActiveStatusEffects != null)
                {
                    foreach (var effect in itemToUnequip.ActiveStatusEffects)
                    {
                        StatusEffectService.RemoveActiveStatusEffect(hero, effect);
                    }
                }

                await BackpackHelper.AddItem(hero.Inventory.Backpack, itemToUnequip);
                Console.WriteLine($"Unequipped {itemToUnequip.Name}.");
                return true;
            }
            return false;
        }

        private async Task<bool> FreeUpOffHandAsync(Hero hero)
        {
            // Remove all offhand items (dual wield, shield, secondary, lantern, torch)
            if (hero.Inventory.OffHand != null)
            {
                await UnequipItemAsync(hero, hero.Inventory.OffHand);
                Console.WriteLine($"Unequipped {hero.Inventory.OffHand.Name} from {hero.Name}'s off-hand.");
            }
            return true;
        }

        private async Task<bool> EquipRingAmuletAsync(Hero hero, Equipment item)
        {            
            if (item.Name.Contains("Ring"))
            {
                var ringBearer = hero.Talents.FirstOrDefault(t => t.Name == TalentName.RingBearer);
                if (ringBearer != null)
                {
                    if (hero.Inventory.EquippedRings.Count > 1)
                    {
                        var ringList = new List<string> { hero.Inventory.EquippedRings[0].Name, hero.Inventory.EquippedRings[1].Name };
                        var choiceResult = await new UserRequestService().RequestChoiceAsync($"Which ring would you like to swap?", ringList);
                        var ringTounequip = hero.Inventory.EquippedRings.FirstOrDefault(r => r.Name == choiceResult.SelectedOption);
                        if (ringTounequip != null) await UnequipItemAsync(hero, ringTounequip);
                    }
                }
                else
                {
                    foreach (var ring in hero.Inventory.EquippedRings)
                    {
                        await UnequipItemAsync(hero, ring);
                    }
                }
                hero.Inventory.EquippedRings.Add(item);
                return true;
            }
            else if (item.Name.Contains("Amulet") || item.Name.Contains("Amulet"))
            {
                if (hero.Inventory.EquippedAmulet != null) await UnequipItemAsync(hero, hero.Inventory.EquippedAmulet);
                else hero.Inventory.EquippedAmulet = item;
                return true;
            }

            return false;
        }

        private async Task<bool> EquipStorageContainerAsync(Hero hero, Equipment containerToEquip)
        {
            // If another container is already equipped, unequip it first.
            if (hero.Inventory.EquippedStorage != null)
            {
                await UnequipItemAsync(hero, hero.Inventory.EquippedStorage);
            }

            hero.Inventory.EquippedStorage = containerToEquip;
            return true;
        }

        private async Task<bool> EquipRelicAsync(Hero hero, Equipment relicToEquip)
        {
            if (hero.Inventory.EquippedRelic != null)
            {
                await UnequipItemAsync(hero, hero.Inventory.EquippedRelic);
                Console.WriteLine($"Unequipped {hero.Inventory.EquippedRelic.Name} from {hero.Name}'s relic slot.");
            }
            hero.Inventory.EquippedRelic = relicToEquip;
            Console.WriteLine($"Equipped {relicToEquip.Name} to {hero.Name}'s relic slot.");
            return true;
        }

        private async Task<bool> EquipAmmoAsync(Hero hero, Ammo ammoToEquip)
        {
            await EmptyQuiverAsync(hero, hero.Inventory);
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

        private async Task<bool> EmptyQuiverAsync(Hero hero, Inventory inventory)
        {
            if (inventory.EquippedQuiver != null)
            {
                await UnequipItemAsync(hero, inventory.EquippedQuiver);
            }
            return true;
        }

        private async Task<bool> EquipWeaponAsync(Hero hero, Weapon weaponToEquip)
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
                    return await EquipTwoHandedWeaponAsync(hero, melee);
                }
                else if (hero.Talents.Any(t => t.Name == TalentName.DualWield)
                    && melee.HasProperty(WeaponProperty.DualWield)
                     && hero.Inventory.EquippedWeapon != null)
                {
                    return await EquipOffHandAsync(hero, melee);
                }
                else // replace one-handed weapon
                {
                    if (equippedWeapon != null)
                    {
                        await UnequipItemAsync(hero, equippedWeapon);
                        Console.WriteLine($"Unequipped {equippedWeapon.Name} from {hero.Name}'s equipped weapon slot.");
                    }
                    hero.Inventory.EquippedWeapon = weaponToEquip;
                    Console.WriteLine($"Equipped {melee.Name} to {hero.Name}'s equipped weapon slot.");
                    return true;
                }
            }
            else if (weaponToEquip.Properties.ContainsKey(WeaponProperty.SecondaryWeapon))
            {
                return await EquipOffHandAsync(hero, (RangedWeapon)weaponToEquip);
            }
            else // equipped ranged weapon
            {
                return await EquipTwoHandedWeaponAsync(hero, weaponToEquip);
            }
        }

        private async Task<bool> EquipTwoHandedWeaponAsync(Hero hero, Weapon weaponToEquip)
        {
            Weapon? equippedWeapon = hero.Inventory.EquippedWeapon;
            if (equippedWeapon != null)
            {
                await UnequipItemAsync(hero, equippedWeapon);
            }
            await FreeUpOffHandAsync(hero);
            hero.Inventory.EquippedWeapon = weaponToEquip;
            Console.WriteLine($"Equipped {weaponToEquip.Name} with both hands, to {hero.Name}'s equipped weapon slot.");
            return true;
        }

        private async Task<bool> EquipOffHandAsync(Hero hero, Equipment itemToEquip)
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
                await UnequipItemAsync(hero, hero.Inventory.OffHand);
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

        private async Task<bool> EquipArmourAsync(Hero hero, Armour armourToEquip)
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
                    await UnequipItemAsync(hero, item); // This helper now handles its own console message.
                }
            }

            equippedArmour.Add(armourToEquip);
            await UpdateNightstalkerSetBonus(hero);
            return true;
        }

        private async Task UpdateNightstalkerSetBonus(Hero hero)
        {
            hero.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.DarkAsTheNight);

            var nightstalkerPieces = hero.Inventory.EquippedArmour
                .Where(a => a.HasProperty(ArmourProperty.DarkAsTheNight))
                .ToList();

            if (!nightstalkerPieces.Any())
            {
                return; // No pieces equipped, nothing to do
            }

            bool hasTorso = nightstalkerPieces.Any(p => p.HasProperty(ArmourProperty.Torso));
            bool hasLegs = nightstalkerPieces.Any(p => p.HasProperty(ArmourProperty.Legs));

            int modifier = 0;
            if (hasTorso && hasLegs)
            {
                // Full set bonus
                modifier = -10;
                Console.WriteLine("Full Nightstalker set bonus active: -10 to be hit.");
            }
            else
            {
                // Partial set bonus
                modifier = -5;
                Console.WriteLine("Partial Nightstalker set bonus active: -5 to be hit.");
            }

            var bonusEffect = new ActiveStatusEffect(StatusEffectType.DarkAsTheNight, -1, toHitPenalty: modifier);

            await StatusEffectService.AttemptToApplyStatusAsync(hero, bonusEffect, _powerActivation);
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
