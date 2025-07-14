using LoDCompanion.Utilities;
using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;
using LoDCompanion.Models.Combat;
using System;
using System.Threading;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Services.Combat
{
    public static class MonsterCombatService
    {

        /// <summary>
        /// Resolves a monster's standard attack against a hero.
        /// </summary>
        public static AttackResult PerformStandardAttack(Monster attacker, Hero target)
        {
            var context = new CombatContext(); // Standard attack has a default context
            return ResolveAttack(attacker, target, context);
        }

        /// <summary>
        /// Resolves a monster's Power Attack, which grants +20 CS.
        /// </summary>
        public static AttackResult PerformPowerAttack(Monster attacker, Hero target)
        {
            var context = new CombatContext { IsPowerAttack = true };
            Console.WriteLine($"{attacker.Name} uses a Power Attack!");
            return ResolveAttack(attacker, target, context);
        }

        /// <summary>
        /// Resolves a monster's Charge Attack, which grants +10 CS.
        /// </summary>
        public static AttackResult PerformChargeAttack(Monster attacker, Hero target)
        {
            var context = new CombatContext { IsChargeAttack = true };
            Console.WriteLine($"{attacker.Name} charges {target.Name}!");
            return ResolveAttack(attacker, target, context);
        }

        /// <summary>
        /// Resolves a monster's physical attack against a hero, including the hero's defense attempt.
        /// </summary>
        /// <param name="attacker">The attacking monster.</param>
        /// <param name="target">The hero being attacked.</param>
        /// <returns>An AttackResult object detailing the outcome.</returns>
        public static AttackResult ResolveAttack(Monster attacker, Hero target, CombatContext context)
        {
            var result = new AttackResult();
            var monsterWeapon = attacker.Weapons.First();

            // Calculate To-Hit Chance
            int baseSkill = (monsterWeapon?.IsRanged ?? false) ? attacker.RangedSkill : attacker.CombatSkill;
            int situationalModifier = CalculateHitChanceModifier(attacker, target);
            result.ToHitChance = baseSkill + situationalModifier;

            result.AttackRoll = RandomHelper.RollDie("D100");

            if (result.AttackRoll > result.ToHitChance)
            {
                result.IsHit = false;
                result.OutcomeMessage = $"{attacker.Name}'s attack misses {target.Name}.";
                return result;
            }

            int potentialDamage = CalculatePotentialDamage(attacker, monsterWeapon);

            DefenseResult defenseResult = ResolveHeroDefense(target, potentialDamage);
            result.OutcomeMessage = defenseResult.OutcomeMessage;

            int damageAfterDefense = Math.Max(0, potentialDamage - defenseResult.DamageNegated);

            if (damageAfterDefense > 0)
            {
                // If damage remains, determine hit location and apply armor.
                HitLocation location = DetermineHitLocation();
                int finalDamage = ApplyArmorToLocation(target, location, damageAfterDefense, monsterWeapon);

                target.TakeDamage(finalDamage);
                result.DamageDealt = finalDamage;
                result.OutcomeMessage += $"\nThe blow hits {target.Name}'s {location} for {finalDamage} damage!";

                // Handle damaging quick slot items on a torso hit.
                if (location == HitLocation.Torso)
                {
                    result.OutcomeMessage += "\n" + CheckForQuickSlotDamage(target);
                }
            }
            else
            {
                result.DamageDealt = 0;
                result.OutcomeMessage += $"\n{target.Name} takes no damage!";
            }

            return result;
        }

        /// <summary>
        /// Calculates the total "to-hit" modifier for a monster attacking a hero.
        /// This method is now public static to be used by other services like MonsterAIService.
        /// </summary>
        public static int CalculateHitChanceModifier(Monster attacker, Hero target)
        {
            int modifier = 0;
            var heroWeapon = target.GetEquippedWeapon();


            if (target.Stance == CombatStance.Prone) modifier += 30;

            // Height advantage also provides a bonus.
            if (attacker.Position.Z > target.Position.Z) modifier += 10;

            // Attacking from behind gives a significant advantage.
            if (DirectionService.IsAttackingFromBehind(attacker, target)) modifier += 20;
            // If the hero performed a Power Attack, they are vulnerable.
            if (target.IsVulnerableAfterPowerAttack)
            {
                modifier += 10;
            }
            else
            {
                if(!DirectionService.IsAttackingFromBehind(attacker, target))
                {
                    // If not vulnerable, their normal defensive bonuses apply.
                    if (target.Shield != null)
                    {
                        modifier -= 5;
                    }
                    // A Parry Stance makes the hero harder to hit.
                    if (target.Stance == CombatStance.Parry)
                    {
                        modifier -= 10;
                        if (target.GetEquippedWeapon() is MeleeWeapon meleeWeapon)
                        {
                            if (meleeWeapon != null && meleeWeapon.HasProperty(WeaponProperty.BFO)) modifier += 5;
                            if (meleeWeapon != null && meleeWeapon.HasProperty(WeaponProperty.Defensive)) modifier -= 10;
                        }
                    }
                }
            }

            return modifier;
        }

        private static DefenseResult ResolveHeroDefense(Hero target, int incomingDamage)
        {
            // In a real UI, the player would choose. We'll prioritize dodge.
            if (!target.HasDodgedThisBattle)
            {
                return DefenseService.AttemptDodge(target);
            }
            if (target.Shield != null)
            {
                return DefenseService.AttemptShieldParry(target, target.Shield, incomingDamage);
            }
            return new DefenseResult { WasSuccessful = false, OutcomeMessage = $"{target.Name} is unable to defend!" };
        }

        private static HitLocation DetermineHitLocation()
        {
            int roll = RandomHelper.RollDie("D6");
            return roll switch
            {
                1 => HitLocation.Head,
                2 => HitLocation.Arms,
                6 => HitLocation.Legs,
                _ => HitLocation.Torso
            };
        }

        /// <summary>
        /// Applies armor reduction based on the specific hit location.
        /// </summary>
        private static int ApplyArmorToLocation(Hero target, HitLocation location, int incomingDamage, Weapon? weapon)
        {
            var relevantArmor = target.Armours.Where(a => DoesArmorCoverLocation(a, location)).ToList();
            int totalArmorValue = relevantArmor.Sum(a => a.DefValue);

            int armourPiercing = weapon?.ArmourPiercing ?? 0;
            int effectiveArmor = Math.Max(0, totalArmorValue - armourPiercing);

            return Math.Max(0, incomingDamage - effectiveArmor);
        }

        /// <summary>
        /// Checks if a piece of armor covers a given hit location.
        /// </summary>
        private static bool DoesArmorCoverLocation(Armour armour, HitLocation location)
        {
            return location switch
            {
                HitLocation.Head => armour.HasProperty(ArmourProperty.Head),
                HitLocation.Arms => armour.HasProperty(ArmourProperty.Arms),
                HitLocation.Torso => armour.HasProperty(ArmourProperty.Torso),
                HitLocation.Legs => armour.HasProperty(ArmourProperty.Legs),
                _ => false,
            };
        }

        /// <summary>
        /// On a torso hit, rolls to see if an item in a quick slot is damaged.
        /// </summary>
        private static string CheckForQuickSlotDamage(Hero target)
        {
            int slotRoll = RandomHelper.RollDie("D10");
            if (slotRoll <= target.QuickSlots.Count)
            {
                var item = target.QuickSlots[slotRoll - 1]; // -1 for 0-based index
                item.Durability--;
                return $"The blow also strikes {target.Name}'s gear! Their {item.Name} is damaged.";
            }
            return "The hero's gear was spared from the impact.";
        }

        /// <summary>
        /// Calculates the raw damage of a monster's attack before armor and defense.
        /// </summary>
        private static int CalculatePotentialDamage(Monster attacker, Weapon? weapon)
        {
            if (weapon != null)
            {
                return weapon.RollDamage();
            }
            return RandomHelper.GetRandomNumber(attacker.MinDamage, attacker.MaxDamage) + attacker.DamageBonus;
        }
    }
}