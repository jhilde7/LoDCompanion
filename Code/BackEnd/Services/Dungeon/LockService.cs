using System.Threading.Tasks;
using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Game;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Player;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Dungeon
{
    public class Lock
    {
        public int LockModifier { get; private set; }
        public int LockHP { get; private set; }
        public bool IsLocked => LockHP > 0;
        public QuestItem requiredItemToOpen { get; private set; }

        public Lock()
        {
            int lockRoll = RandomHelper.RollDie(DiceType.D10);
            if (lockRoll > 6)
            {
                switch (lockRoll)
                {
                    case 7: SetLockState(0, 10); break;
                    case 8: SetLockState(-10, 15); break;
                    case 9: SetLockState(-15, 20); break;
                    case 10: SetLockState(-20, 25); break;
                }
            }
        }

        public Lock(QuestItem item)
        {
            SetLockState(999, 999);
            requiredItemToOpen = item;
        }

        public void SetLockState(int lockModifier, int lockHP)
        {
            LockModifier = lockModifier;
            LockHP = lockHP;
        }

        public bool PickLock(int attemptRoll, int skill)
        {
            if (attemptRoll < skill + LockModifier)
            {
                LockHP = 0;
                return true; 
            }
            else
            {
                return false;
            }
        }

        public int BashLock(int damage)
        {
            LockHP -= damage;
            if (LockHP < 0) LockHP = 0;
            return LockHP;
        }
    }

    public class LockService
    {
        private readonly UserRequestService _diceRoll = new UserRequestService();

        public event Action<ThreatActionType>? OnUpdateThreatLevelByThreatActionType;


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
        public async Task<ActionResult> PickLock(Hero hero, Lock lockToPick)
        {
            var result = new ActionResult();
            if (hero == null)
            {
                // In a real application, log this or throw a more specific exception
                result.Message = "Error: Hero is null in PickLock.";
                result.WasSuccessful = false;
                return result;
            }

            // Check if hero has lock picks
            var lockPicks = hero.Inventory.Backpack.Find(item => item != null && item.Name == "Lock Picks");
            if (lockPicks == null || lockPicks.Quantity <= 0)
            {
                result.Message = $"{hero.Name} has no Lock Picks!";
                result.WasSuccessful = false;
                return result;
            }

            int skill = hero.GetSkill(Skill.PickLocks);

            BackpackHelper.TakeOneItem(hero.Inventory.Backpack, lockPicks);
            var cleverFingers = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.CleverFingers);
            if (cleverFingers != null) hero.ActiveStatusEffects.Remove(cleverFingers);

            int pickLockRoll = (await _diceRoll.RequestRollAsync("Roll for pick lock attempt.", "1d100", skill: (hero, Skill.PickLocks))).Roll;
            await Task.Yield();

            if (pickLockRoll <= 80 && lockToPick.PickLock(pickLockRoll, skill))
            {
                result.Message = $"{hero.Name} successfully picked the lock!";
                result.WasSuccessful = true;
                return result;
            }
            else
            {
                result.Message = $"{hero.Name} failed to pick the lock. Lock pick broken.";
                result.WasSuccessful = false;
                return result;
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
            if (character is Hero hero && OnUpdateThreatLevelByThreatActionType != null)
            {
                var crowbar = hero.Inventory.Backpack.Find(item => item != null && item.Name == "Crowbar");
                if (crowbar != null)
                {
                    damageToLock = 8 + baseDamage;
                    OnUpdateThreatLevelByThreatActionType.Invoke(ThreatActionType.BashLockWithCrowbar);
                }
                else
                {
                    OnUpdateThreatLevelByThreatActionType.Invoke(ThreatActionType.BashLock);
                }
            }

            lockToBash.BashLock(damageToLock);

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