using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Models;
using LoDCompanion.Utilities;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Services.Combat
{

    /// <summary>
    /// Represents the result of a single attack action.
    /// </summary>
    public class AttackResult
    {
        public bool IsHit { get; set; }
        public int DamageDealt { get; set; }
        public string OutcomeMessage { get; set; } = string.Empty;
        public int ToHitChance { get; set; }
        public int AttackRoll { get; set; }
    }

    public class AttackService
    {
        private readonly FloatingTextService _floatingText;
        private readonly DiceRollService _diceRoll;

        public AttackService(FloatingTextService floatingTextService, DiceRollService diceRollService)
        {
            _floatingText = floatingTextService;
            _diceRoll = diceRollService;
        }

        /// <summary>
        /// Resolves a monster's standard attack against a hero.
        /// </summary>
        public async Task<AttackResult> PerformStandardAttackAsync(Character attacker, Weapon? weapon, Character target, DungeonState dungeon, CombatContext? context = null)
        {
            if (context == null)
            {
                context = new CombatContext(); // Standard attack has a default context 
            }
            return await ResolveAttackAsync(attacker, weapon, target, context, dungeon);
        }

        /// <summary>
        /// Resolves a monster's Power Attack, which grants +20 CS.
        /// </summary>
        public async Task<AttackResult> PerformPowerAttackAsync(Character attacker, Weapon? weapon, Character target, DungeonState dungeon, CombatContext? context = null)
        {
            if (context == null)
            {
                context = new CombatContext { IsPowerAttack = true };
            }
            return await ResolveAttackAsync(attacker, weapon, target, context, dungeon);
        }

        /// <summary>
        /// Resolves a Charge Attack, which grants +10 CS.
        /// </summary>
        public async Task<AttackResult> PerformChargeAttackAsync(Character attacker, Weapon? weapon, Character target, DungeonState dungeon, CombatContext? context = null)
        {
            if (context == null)
            {
                context =  new CombatContext { IsChargeAttack = true };
            }
            return await ResolveAttackAsync(attacker, weapon, target, context, dungeon);
        }

        public async Task<AttackResult> ResolveAttackAsync(Character attacker, Weapon? weapon, Character target, CombatContext context, DungeonState dungeon)
        {
            var result = new AttackResult();

            // Calculate To-Hit Chance
            int baseSkill = (weapon?.IsRanged ?? false) ? attacker.RangedSkill : attacker.CombatSkill;
            int situationalModifier = CalculateHitChanceModifier(attacker, weapon, target, context);
            result.ToHitChance = baseSkill + situationalModifier;
            if (attacker is Hero)
            {
                if(weapon == null) 
                {
                    result.OutcomeMessage = "The hero does not have a weapon equipped!"; 
                    return result; 
                }
                result.AttackRoll = await _diceRoll.RequestRollAsync("Roll to-hit.", "1d100");
                await Task.Yield();
            }
            else
            {
                result.AttackRoll = RandomHelper.RollDie("D100");
            }

            if (result.AttackRoll > result.ToHitChance)
            {
                result.IsHit = false;
                result.OutcomeMessage = $"{attacker.Name}'s attack misses {target.Name}.";
                _floatingText.ShowText("Miss!", target.Position, "miss-toast");
                return result;
            }

            result.IsHit = true;                        

            if (target is Hero heroTarget)
            {
                int potentialDamage = (weapon != null)
                    ? CalculateWeaponPotentialDamage(weapon)
                    : (attacker is Monster m) ? CalculateMonsterPotentialDamage(m) : 0;
                result = await ResolveAttackAgainstHeroAsync(attacker, heroTarget, potentialDamage, weapon, context, dungeon);
            }
            else if (target is Monster monsterTarget && weapon != null)
            {
                result = await ResolveAttackAgainstMonsterAsync(attacker, monsterTarget, weapon, context, dungeon);
            }

            if(weapon is RangedWeapon rangedWeapon)
            {
                rangedWeapon.ConsumeAmmo();
            }
            return result;
        }

        private async Task<AttackResult> ResolveAttackAgainstHeroAsync(Character attacker, Hero target, int potentialDamage, Weapon? weapon, CombatContext context, DungeonState dungeon)
        {
            var result = new AttackResult { IsHit = true };

            DefenseResult defenseResult = await ResolveHeroDefenseAsync(target, potentialDamage);
            int damageAfterDefense = Math.Max(0, potentialDamage - defenseResult.DamageNegated);
            result.OutcomeMessage = defenseResult.OutcomeMessage;

            if (damageAfterDefense > 0)
            {
                HitLocation location = await DetermineHitLocationAsync();
                result.DamageDealt = ApplyArmorToLocation(target, location, damageAfterDefense, weapon);
                target.TakeDamage(result.DamageDealt);

                result.OutcomeMessage += $"\nThe blow hits {target.Name}'s {location} for {result.DamageDealt} damage!";
                result.OutcomeMessage += CheckForQuickSlotDamageAsync(target);
                _floatingText.ShowText($"-{result.DamageDealt}", target.Position, "damage-text");
            }
            else
            {
                _floatingText.ShowText("Blocked!", target.Position, "miss-text");
            }

            if (context.IsChargeAttack)
            {
                result.OutcomeMessage += "\n" + GridService.ShoveCharacter(attacker, target, dungeon.DungeonGrid);
            }

            return result;
        }

        private async Task<AttackResult> ResolveAttackAgainstMonsterAsync(Character attacker, Monster target, Weapon weapon, CombatContext context, DungeonState dungeon)
        {
            var result = new AttackResult { IsHit = true };
            int finalDamage = await CalculateHeroDamageAsync(attacker, target, weapon, context);
            target.TakeDamage(finalDamage);
            result.DamageDealt = finalDamage;

            result.OutcomeMessage = $"{attacker.Name}'s attack hits {target.Name} for {finalDamage} damage!";
            _floatingText.ShowText($"-{finalDamage}", target.Position, "damage-text");

            if (context.IsChargeAttack)
            {
                result.OutcomeMessage += "\n" + GridService.ShoveCharacter(attacker, target, dungeon.DungeonGrid);
            }

            return result;
        }

        public int CalculateHitChanceModifier(Character attacker, Weapon? weapon, Character target, CombatContext context)
        {
            int modifier = 0;
            Weapon? heroWeapon = null;
            Weapon? monsterWeapon = null;
            if (target is Hero hero)
            {
                heroWeapon = hero.GetEquippedWeapon(); 
            }
            if(target is Monster monster)
            {
                monsterWeapon = monster.GetMeleeWeapon();
            }


            // Ranged-specific modifiers
            if (weapon != null && weapon is RangedWeapon)
            {
                modifier -= (context.ObstaclesInLineOfSight * 10);
                if (target.IsLarge) modifier += 10;
                if (context.HasAimed) modifier += 10;
            }
            if (DirectionService.IsAttackingFromBehind(attacker, target)) modifier += 20;
            if (attacker.Position.Z > target.Position.Z) modifier += 10;

            if (context.IsChargeAttack) modifier += 10;
            if (context.IsPowerAttack) modifier += 20;
            if (target.CombatStance == CombatStance.Prone) modifier += 30;
            if (monsterWeapon != null)
            {
                if (monsterWeapon.Name == "Rapier") modifier -= 5;
                if (monsterWeapon.Properties.ContainsKey(WeaponProperty.Slow)) modifier += 5;
                if (monsterWeapon.Properties.ContainsKey(WeaponProperty.BFO)) modifier += 5;
                if (monsterWeapon.Name == "Staff") modifier -= 5;
            }
            // If the hero performed a Power Attack, they are vulnerable.
            if (target.IsVulnerableAfterPowerAttack)
            {
                modifier += 10;
            }
            else
            {
                if (!DirectionService.IsAttackingFromBehind(attacker, target))
                {
                    // If not vulnerable, their normal defensive bonuses apply.
                    if (target.HasShield)
                    {
                        modifier -= 5;
                    }
                    // A Parry Stance makes the hero harder to hit.
                    if (target.CombatStance == CombatStance.Parry)
                    {
                        modifier -= 10;
                        if (heroWeapon != null && heroWeapon is MeleeWeapon meleeWeapon)
                        {
                            if (meleeWeapon != null && meleeWeapon.HasProperty(WeaponProperty.BFO)) modifier += 5;
                            if (meleeWeapon != null && meleeWeapon.HasProperty(WeaponProperty.Defensive)) modifier -= 10;
                        }
                    }
                }
            }

            return modifier;
        }

        private int CalculateWeaponPotentialDamage(Weapon weapon)
        {
            return weapon.RollDamage();
        }

        public int CalculateMonsterPotentialDamage(Monster monster)
        {
            return RandomHelper.GetRandomNumber(monster.MinDamage, monster.MaxDamage) + monster.DamageBonus;
        }

        private async Task<DefenseResult> ResolveHeroDefenseAsync(Hero target, int incomingDamage)
        {
            if (target.Shield != null)
            {
                return await DefenseService.AttemptShieldParry(target, target.Shield, incomingDamage, _diceRoll);
            }
            if (target.CombatStance == CombatStance.Parry)
            {
                return await DefenseService.AttemptWeaponParry(target, target.Weapons.FirstOrDefault(w => w.IsMelee), _diceRoll);
            }
            return new DefenseResult { WasSuccessful = false, OutcomeMessage = $"{target.Name} is unable to defend!" };
        }

        private async Task<HitLocation> DetermineHitLocationAsync()
        {
            int roll = await _diceRoll.RequestRollAsync("Roll for the location you were hit at.", "1d6");
            await Task.Yield();
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
        private int ApplyArmorToLocation(Hero target, HitLocation location, int incomingDamage, Weapon? weapon)
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
        private bool DoesArmorCoverLocation(Armour armour, HitLocation location)
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
        private async Task<string> CheckForQuickSlotDamageAsync(Hero target)
        {
            int slotRoll = await _diceRoll.RequestRollAsync(
                $"You were hit in the torso, check for damage to the items in your quick slots ", "1d10");
            await Task.Yield();
            if (slotRoll <= target.QuickSlots.Count)
            {
                var item = target.QuickSlots[slotRoll - 1]; // -1 for 0-based index
                item.Durability--;
                return $"The blow also strikes {target.Name}'s gear! Their {item.Name} is damaged.";
            }
            return "The hero's gear was spared from the impact.";
        }

        /// <summary>
        /// Calculates the final damage dealt by a successful hit, including all bonuses and armor reduction.
        /// This method now incorporates the logic from the previous bonus calculation methods.
        /// </summary>
        private async Task<int> CalculateHeroDamageAsync(Character attacker, Character target, Weapon weapon, CombatContext context)
        {
            string? dice = weapon.DamageDice;
            int damage = 0;
            if(dice != null)
            {
                damage = await _diceRoll.RequestRollAsync($"You Hit {target.Name}, now roll for damage", dice);
                await Task.Yield();
            }
            else
            {
                damage = weapon.RollDamage();
            }
            damage += attacker.DamageBonus;

            if (weapon is MeleeWeapon meleeWeapon && attacker is Hero hero)
            {
                if (hero.Talents.Any(t => t.IsMightyBlow))
                {
                    damage += 1;
                }
                // Apply the Unwieldly bonus if the context flag is set.
                if (context.ApplyUnwieldlyBonus)
                {
                    // Get the bonus value from the weapon's properties (e.g., 4 for the Morning Star)
                    int bonus = meleeWeapon.GetPropertyValue(WeaponProperty.Unwieldly);
                    damage += bonus;
                    Console.WriteLine($"Unwieldly bonus applied: +{bonus} damage!");
                }
            }
            else if (weapon is RangedWeapon rangedWeapon)
            {
                if (rangedWeapon.Ammo != null && (rangedWeapon.Ammo.HasProperty(AmmoProperty.Barbed) || rangedWeapon.Ammo.HasProperty(AmmoProperty.SupuriorSlingStone)))
                {
                    damage += 1;
                }
            }

            int finalDamage = 0;
            int targetArmor = 0;
            int targetNaturalArmor = 0;
            if (target is Monster monster)
            {
                targetArmor = monster.ArmourValue;
                targetNaturalArmor = monster.NaturalArmour;
            }

            // Apply Armour Piercing from the context
            targetArmor = Math.Max(0, targetArmor - context.ArmourPiercingValue);

            if (context.IsFireDamage)
            {
                // Rule: "Fire Damage will ignore both NA and armour."
                finalDamage = damage;
            }
            else if (context.IsAcidicDamage)
            {
                // Rule: "Acidic Damage... will ignore NA."
                finalDamage = Math.Max(0, damage - targetArmor);
            }
            else // This includes Frost and standard Physical damage
            {
                // Rule: Standard damage calculation.
                finalDamage = Math.Max(0, damage - (targetArmor + targetNaturalArmor));
            }

            return finalDamage;
        }
    }
}
