using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public class LockService
    {
        // Constructor for dependency injection of RandomHelper
        public LockService()
        {
        }

        /// <summary>
        /// Attempts to pick a lock.
        /// </summary>
        /// <param name="hero">The hero attempting to pick the lock.</param>
        /// <param name="lockModifier">A modifier to the lockpicking difficulty (e.g., from the lock itself).</param>
        /// <returns>True if the lock is successfully picked, false otherwise.</returns>
        public bool PickLock(Hero hero, int lockModifier)
        {
            if (hero == null)
            {
                // In a real application, log this or throw a more specific exception
                Console.WriteLine("Error: Hero is null in PickLock.");
                return false;
            }

            // Check if hero has lock picks
            var lockPicks = hero.Inventory.Backpack.Find(item => item.Name == "Lock Picks");
            if (lockPicks == null || lockPicks.Quantity <= 0)
            {
                Console.WriteLine($"{hero.Name} has no Lock Picks!");
                return false;
            }

            // Original logic: "LockPicksSkill - PickLockSkillModifier" looks like a typo, assuming it means hero's PickLocksSkill
            // Assuming Hero has a PickLocksSkill property or it's part of Dexterity/Profession
            int skill = hero.GetSkill(Skill.PickLocks); // Example: sum of Dex and profession/talent bonus

            int pickLockRoll = RandomHelper.GetRandomNumber(1, 100); // Roll a d100

            // Base roll + skill - lockModifier
            int successThreshold = skill - lockModifier;

            if (pickLockRoll <= 80 && pickLockRoll <= successThreshold)
            {
                lockPicks.Quantity--; // Consume one lock pick on success (or on attempt, depending on rules)
                Console.WriteLine($"{hero.Name} successfully picked the lock!");
                return true;
            }
            else
            {
                lockPicks.Quantity--; // Consume one lock pick on failure
                Console.WriteLine($"{hero.Name} failed to pick the lock. Lock pick broken.");
                return false;
            }
        }

        /// <summary>
        /// Attempts to bash a lock open.
        /// </summary>
        /// <param name="hero">The hero attempting to bash the lock.</param>
        /// <param name="lockHP">The current HP/durability of the lock.</param>
        /// <param name="weapon">The weapon used for bashing (can be null if unarmed).</param>
        /// <returns>The remaining HP of the lock after the bash attempt.</returns>
        public int BashLock(Hero hero, int lockHP, MeleeWeapon weapon)
        {
            if (hero == null)
            {
                Console.WriteLine("Error: Hero is null in BashLock.");
                return lockHP;
            }

            int damageToLock = 0;
            int baseDamage = hero.GetStat(BasicStat.Strength) + hero.GetStat(BasicStat.DamageBonus); // Adjust DamageBonus source as needed

            if (weapon != null)
            {
                // Assuming weapon.GetDamage() returns an int array like {min, max}
                // We'll just take the min for simplicity or roll dice if needed.
                // For a more robust combat system, this would involve a proper damage roll
                damageToLock = RandomHelper.GetRandomNumber(weapon.MinDamage, weapon.MaxDamage) + baseDamage;
                Console.WriteLine($"{hero.Name} bashes the lock with {weapon.Name} for {damageToLock} damage!");
            }
            else
            {
                damageToLock = RandomHelper.GetRandomNumber(1, 4) + baseDamage; // Unarmed bash damage example
                Console.WriteLine($"{hero.Name} bashes the lock with bare hands for {damageToLock} damage!");
            }

            // Check for crowbar in backpack
            var crowbar = hero.Inventory.Backpack.Find(item => item.Name == "Crowbar");
            if (crowbar != null)
            {
                damageToLock = (int)(damageToLock * 1.5); // 50% bonus from crowbar
                Console.WriteLine("Crowbar provides a bonus to the bash!");
            }

            lockHP -= damageToLock;
            if (lockHP < 0)
            {
                lockHP = 0;
            }

            if (lockHP == 0)
            {
                Console.WriteLine("The lock is bashed open!");
            }
            else
            {
                Console.WriteLine($"The lock has {lockHP} HP remaining.");
            }
            return lockHP;
        }
    }
}