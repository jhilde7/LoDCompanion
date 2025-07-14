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

        /// <summary>
        /// Processes spell damage initiated by a monster against a hero.
        /// </summary>
        /// <param name="attacker">The casting monster.</param>
        /// <param name="targetHero">The hero being targeted.</param>
        /// <param name="monsterSpellDamage">The spell damage properties.</param>
        /// <returns>A tuple containing spell message and damage dealt.</returns>
        public static (string message, int damageDealt) ProcessSpellDamage(Monster attacker, Hero targetHero, Spell monsterSpellDamage)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (targetHero == null) throw new ArgumentNullException(nameof(targetHero));
            if (monsterSpellDamage == null) throw new ArgumentNullException(nameof(monsterSpellDamage));

            int baseDamage = RandomHelper.GetRandomNumber(monsterSpellDamage.MinDamage, monsterSpellDamage.MaxDamage);
            int damageAfterResist = baseDamage; // Apply any resistances/bonuses from hero or spell type later

            // Optionally, consider armour piercing and specific damage types
            if (monsterSpellDamage.IsArmourPiercing)
            {
                // Spells usually bypass physical armor, but maybe magical resistance applies here
            }

            // Apply damage to hero
            targetHero.TakeDamage(damageAfterResist);

            return ($"{attacker.Name} casts a spell on {targetHero.Name} for {damageAfterResist} damage!", damageAfterResist);
        }

        /// <summary>
        /// Determines the monster's action for the turn (e.g., attack, special ability).
        /// </summary>
        /// <param name="monster">The monster whose action is being determined.</param>
        /// <param name="adjacentHeroes">List of heroes adjacent to the monster.</param>
        /// <param name="heroesInLOS">List of heroes in line of sight of the monster.</param>
        /// <returns>A string describing the monster's action.</returns>
        public static string GetAction(Monster monster, List<Hero> adjacentHeroes, List<Hero> heroesInLOS)
        {
            if (monster == null) throw new ArgumentNullException(nameof(monster));

            // Simplified action logic:
            // 1. If adjacent heroes, prioritize melee attack.
            // 2. If no adjacent but heroes in LOS and has ranged weapon/spell, use ranged/spell.
            // 3. Otherwise, do nothing or move (movement logic not in this service).
            // 4. Consider special actions last or based on a roll.

            if (adjacentHeroes != null && adjacentHeroes.Any())
            {
                // Prefer physical attack if adjacent
                return "Physical Attack";
            }
            else if (heroesInLOS != null && heroesInLOS.Any())
            {
                // Check if monster has ranged weapon or spells
                bool hasRangedWeapon = monster.Weapons != null && monster.Weapons.Any(w => w.IsRanged);
                bool hasSpells = monster.Spells != null && monster.Spells.Any(); // Assuming monster.Spells list strings like "Fireball"

                if (hasRangedWeapon)
                {
                    return "Ranged Attack";
                }
                else if (hasSpells)
                {
                    return "Cast Spell";
                }
            }

            // If monster has special attack and conditions met (e.g., on cooldown, specific HP threshold)
            if (monster.HasSpecialAttack)
            {
                // More complex logic here, possibly calling MonsterSpecialService.CanTriggerSpecialAttack()
                if (RandomHelper.GetRandomNumber(1, 100) <= 20) // 20% chance to do a special
                {
                    return "Special Attack";
                }
            }

            return "Idle/Move"; // Monster might move if no targets or special action
        }

        /// <summary>
        /// Calculates the damage a monster deals based on its weapon and the target's armor.
        /// </summary>
        /// <param name="attacker">The monster dealing damage.</param>
        /// <param name="weapon">The weapon used by the monster (can be null for natural attacks).</param>
        /// <param name="targetArmourPieces">The collection of armor pieces on the target.</param>
        /// <returns>The total damage dealt after armor reduction.</returns>
        public static int GetDamage(Monster attacker, List<Armour> targetArmourPieces, Weapon? weapon = null)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));

            int baseDamage = 0;
            if (weapon != null)
            {
                baseDamage = weapon.RollDamage();
            }
            else
            {
                // Natural attack damage (e.g., from Monster's base stats or predefined natural attack damage)
                baseDamage = RandomHelper.GetRandomNumber(attacker.MinDamage, attacker.MaxDamage);
            }

            // Calculate total armor value from target's equipped armor
            int totalArmorValue = 0;
            if (targetArmourPieces != null)
            {
                foreach (var armour in targetArmourPieces)
                {
                    totalArmorValue += armour.ArmourClass + armour.DefValue;
                    // Consider IsMetal, IsDragonScale, etc. here for special resistances
                }
            }
            totalArmorValue += attacker.NaturalArmour; // Add target's natural armor

            // Apply armor piercing if the weapon has it
            int armourPiercing = weapon?.ArmourPiercing ?? 0;
            int effectiveArmor = Math.Max(0, totalArmorValue - armourPiercing);

            int damageAfterArmor = Math.Max(0, baseDamage - effectiveArmor);

            return damageAfterArmor;
        }
    }
}