using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Threading.Tasks;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public class Lock
    {
        public int LockModifier { get; set; }
        public int LockHP { get; set; }
        public bool IsLocked => LockHP > 0;
    }

    public class LockService
    {
        private readonly UserRequestService _diceRoll;
        private readonly ThreatService _threat;
        // Constructor for dependency injection of RandomHelper
        public LockService(UserRequestService userRequestService, ThreatService threat)
        {
            _diceRoll = userRequestService;
            _threat = threat;
        }

        /// <summary>
        /// Attempts to pick a lock.
        /// </summary>
        /// <param name="hero">The hero attempting to pick the lock.</param>
        /// <param name="lockModifier">A modifier to the lockpicking difficulty (e.g., from the lock itself).</param>
        /// <returns>True if the lock is successfully picked, false otherwise.</returns>
        public async Task<bool> PickLock(Hero hero, Lock lockToPick)
        {
            if (hero == null)
            {
                // In a real application, log this or throw a more specific exception
                Console.WriteLine("Error: Hero is null in PickLock.");
                return false;
            }

            // Check if hero has lock picks
            var lockPicks = hero.Inventory.Backpack.Find(item => item != null && item.Name == "Lock Picks");
            if (lockPicks == null || lockPicks.Quantity <= 0)
            {
                Console.WriteLine($"{hero.Name} has no Lock Picks!");
                return false;
            }

            // Original logic: "LockPicksSkill - PickLockSkillModifier" looks like a typo, assuming it means hero's PickLocksSkill
            // Assuming Hero has a PickLocksSkill property or it's part of Dexterity/Profession
            int skill = hero.GetSkill(Skill.PickLocks);

            int pickLockRoll = (await _diceRoll.RequestRollAsync("Roll for pick lock attempt.", "1d100", skill: (hero, Skill.PickLocks))).Roll;
            await Task.Yield();

            // Base roll + skill - lockModifier
            int successThreshold = skill - lockToPick.LockModifier;

            BackpackHelper.TakeOneItem(hero.Inventory.Backpack, lockPicks);
            var cleverFingers = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.CleverFingers);
            if (cleverFingers != null) hero.ActiveStatusEffects.Remove(cleverFingers);

            if (pickLockRoll <= 80 && pickLockRoll <= successThreshold)
            {
                lockToPick.LockHP = 0;
                Console.WriteLine($"{hero.Name} successfully picked the lock!");
                return true;
            }
            else
            {
                Console.WriteLine($"{hero.Name} failed to pick the lock. Lock pick broken.");
                return false;
            }
        }

        /// <summary>
        /// Attempts to bash a lock open.
        /// </summary>
        /// <param name="character">The character attempting to bash the lock.</param>
        /// <param name="lockHP">The current HP/durability of the lock.</param>
        /// <param name="weapon">The weapon used for bashing (can be null if unarmed).</param>
        /// <returns>The remaining HP of the lock after the bash attempt.</returns>
        public async Task<bool> BashLock(Character character, Lock lockToBash, MeleeWeapon weapon)
        {
            if (character == null)
            {
                Console.WriteLine("Error: Hero is null in BashLock.");
                return false;
            }

            int damageToLock = 0;
            int baseDamage = character.GetStat(BasicStat.DamageBonus); // Adjust DamageBonus source as needed

            if (weapon != null && weapon.DamageDice != null)
            {
                var rollResult = await _diceRoll.RequestRollAsync("Roll for weapon damage.", weapon.DamageDice);
                damageToLock = rollResult.Roll + baseDamage;
                Console.WriteLine($"{character.Name} bashes the lock with {weapon.Name} for {damageToLock} damage!");
            }
            else
            {
                damageToLock = baseDamage; // Unarmed bash damage example
                Console.WriteLine($"{character.Name} bashes the lock with bare hands for {damageToLock} damage!");
            }

            // Check for crowbar in backpack
            if (character is Hero hero)
            {
                var crowbar = hero.Inventory.Backpack.Find(item => item != null && item.Name == "Crowbar");
                if (crowbar != null)
                {
                    damageToLock = 8 + baseDamage;
                    _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.BashLockWithCrowbar);
                }
                else
                {
                    _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.BashLock);
                }
            }

            lockToBash.LockHP -= damageToLock;

            if (lockToBash.LockHP < 0)
            {
                Console.WriteLine("The lock is bashed open!");
                return true;
            }
            else
            {
                Console.WriteLine($"The lock has {lockToBash.LockHP} HP remaining.");
                return true;
            }
        }
    }
}