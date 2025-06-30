using LoDCompanion.Utilities;
using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Services.Combat
{
    public class MonsterCombatService
    {
        private readonly MonsterSpecialService _monsterSpecialService;

        public MonsterCombatService(MonsterSpecialService monsterSpecialService)
        {
            _monsterSpecialService = monsterSpecialService;
        }

        /// <summary>
        /// Processes a physical attack initiated by a monster against a hero.
        /// </summary>
        /// <param name="attacker">The attacking monster.</param>
        /// <param name="targetHero">The hero being targeted.</param>
        /// <param name="monsterWeapon">The weapon the monster is using (can be null if natural attack).</param>
        /// <returns>A tuple containing attack message and damage dealt.</returns>
        public (string message, int damageDealt) ProcessPhysicalAttack(Monster attacker, Hero targetHero, MonsterWeapon? monsterWeapon = null)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (targetHero == null) throw new ArgumentNullException(nameof(targetHero));

            // Determine monster's base attack value (CombatSkill or RangedSkill if applicable)
            int monsterAttackSkill = attacker.CombatSkill; // Assuming most monsters use CombatSkill
            if (monsterWeapon != null && (monsterWeapon.IsRanged || attacker.Type.Contains("Archer"))) // Placeholder for ranged monsters
            {
                monsterAttackSkill = attacker.RangedSkill;
            }

            // Get hit roll
            int hitRoll = RandomHelper.GetRandomNumber(1, 100);
            int heroDodge = targetHero.Dodge;

            // Apply combat skill/bonus, weapon bonus, etc.
            int toHitValue = monsterAttackSkill; // Start with monster's skill
            if (monsterWeapon != null)
            {
                // Add any weapon-specific TO-HIT bonuses if they exist (not explicitly in original Weapon.cs)
                // For now, assuming weapon damage is primary, to-hit is from monster skill
            }

            string attackResult;
            int damageDealt = 0;

            if (hitRoll <= toHitValue && hitRoll > heroDodge) // Monster hits, hero doesn't dodge
            {
                attackResult = "Hit!";
                damageDealt = GetDamage(attacker, targetHero.Armours, monsterWeapon);
            }
            else if (hitRoll <= heroDodge) // Hero dodges
            {
                attackResult = "Dodged!";
                damageDealt = 0;
            }
            else // Monster misses
            {
                attackResult = "Miss!";
                damageDealt = 0;
            }

            targetHero.TakeDamage(damageDealt); // Apply damage to the hero

            // Check for special attacks after damage calculation
            string specialAttackMessage = string.Empty;
            if (attacker.HasSpecialAttack)
            {
                // This would trigger specific monster special abilities
                // The MonsterSpecialService would handle the effects
                specialAttackMessage = _monsterSpecialService.TriggerRandomSpecialAttack(attacker, targetHero);
            }

            return ($"{attacker.Name} attacks {targetHero.Name}! Roll: {hitRoll} (Needed {toHitValue}, Dodge {heroDodge}) -> {attackResult}. Damage: {damageDealt}. {specialAttackMessage}".Trim(), damageDealt);
        }

        /// <summary>
        /// Processes spell damage initiated by a monster against a hero.
        /// </summary>
        /// <param name="attacker">The casting monster.</param>
        /// <param name="targetHero">The hero being targeted.</param>
        /// <param name="monsterSpellDamage">The spell damage properties.</param>
        /// <returns>A tuple containing spell message and damage dealt.</returns>
        public (string message, int damageDealt) ProcessSpellDamage(Monster attacker, Hero targetHero, Spell monsterSpellDamage)
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
        public string GetAction(Monster monster, List<Hero> adjacentHeroes, List<Hero> heroesInLOS)
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
        public int GetDamage(Monster attacker, List<Armour> targetArmourPieces, MonsterWeapon? weapon = null)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));

            int baseDamage = 0;
            if (weapon != null)
            {
                baseDamage = weapon.GetDamage(attacker.DamageBonus);
            }
            else
            {
                // Natural attack damage (e.g., from Monster's base stats or predefined natural attack damage)
                baseDamage = RandomHelper.GetRandomNumber(attacker.DamageArray[0], attacker.DamageArray[1]);
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

        // The original GetHit method from MonsterCombat.cs seems to be integrated into ProcessPhysicalAttack
        // and GetAction, as it relates to the roll for hitting. It doesn't need to be a separate public method.
    }
}