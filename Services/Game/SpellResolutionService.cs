using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Game
{
    public class SpellResolutionService
    {
        private readonly DungeonState _dungeonState;
        private readonly CombatManagerService _combatManager; // To apply damage/effects
        private readonly DiceRollService _diceRoll;

        public SpellResolutionService(DungeonState dungeonState, CombatManagerService combatManager, DiceRollService diceRoll)
        {
            _dungeonState = dungeonState;
            _combatManager = combatManager;
            _diceRoll = diceRoll;
        }

        /// <summary>
        /// The main entry point to resolve any successfully cast spell.
        /// It routes the spell to the correct handler based on its properties.
        /// </summary>
        public async Task<SpellCastResult> ResolveSpellAsync(Hero caster, Spell spell, Character target, SpellCastingResult options)
        {
            var result = new SpellCastResult { IsSuccess = true };

            // Handle Touch Spells first, as they require a to-hit roll
            if (spell.Properties?.Contains(SpellProperty.Touch) ?? false)
            {
                int touchAttackRoll = await _diceRoll.RollDice("Roll to touch target", "1d100");
                if (touchAttackRoll > caster.GetSkill(Skill.CombatSkill) + 20)
                {
                    result.OutcomeMessage = $"{caster.Name}'s touch spell misses {target.Name}.";
                    return result;
                }
            }

            // Route to specific handlers based on spell type
            if (spell.IsDamageSpell)
            {
                result = HandleDamageSpell(caster, spell, target, options);
            }
            else if (spell.School == MagicSchool.Restoration)
            {
                result = HandleHealingSpell(caster, spell, target, options);
            }
            else if (spell.School == MagicSchool.Conjuration)
            {
                result = HandleSummoningSpell(caster, spell);
            }
            else
            {
                // Handle buffs, debuffs, and utility spells
                result = HandleUtilitySpell(caster, spell, target, options);
            }

            return result;
        }

        private SpellCastResult HandleDamageSpell(Hero caster, Spell spell, Character target, SpellCastingResult options)
        {
            int damage = spell.GetSpellDamage(caster.GetStat(BasicStat.Level)) + options.PowerLevels;

            // Apply damage (this would go through your combat service to handle armor, etc.)
            // For now, a direct application:
            target.TakeDamage(damage);

            return new SpellCastResult
            {
                IsSuccess = true,
                OutcomeMessage = $"{spell.Name} hits {target.Name} for {damage} {spell.DamageType} damage!"
            };
        }

        private SpellCastResult HandleHealingSpell(Hero caster, Spell spell, Character target, SpellCastingResult options)
        {
            if (target is not Hero heroTarget)
            {
                return new SpellCastResult { OutcomeMessage = "Healing spells can only target heroes." };
            }

            int healingAmount = spell.GetSpellDamage(caster.GetStat(BasicStat.Level)) + options.PowerLevels;

            heroTarget.CurrentHP = Math.Min(heroTarget.GetStat(BasicStat.HitPoints), heroTarget.CurrentHP + healingAmount);

            return new SpellCastResult
            {
                IsSuccess = true,
                OutcomeMessage = $"{heroTarget.Name} is healed for {healingAmount} HP by {spell.Name}."
            };
        }

        private SpellCastResult HandleSummoningSpell(Hero caster, Spell spell)
        {
            // Logic for summoning creatures would go here.
            // This would involve creating new Monster instances and adding them to the combat.
            return new SpellCastResult
            {
                IsSuccess = true,
                OutcomeMessage = $"{caster.Name} summons a creature with {spell.Name}!"
            };
        }

        private SpellCastResult HandleUtilitySpell(Hero caster, Spell spell, Character target, SpellCastingResult options)
        {
            // A switch for all non-damage, non-healing spells
            switch (spell.Name)
            {
                case "Protective Shield":
                    StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Shield));
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"A protective shield surrounds {target.Name}." };
                case "Slip":
                    StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Prone));
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{target.Name} slips and falls prone!" };
                // ... add cases for all other utility spells ...
                default:
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{spell.Name} is cast successfully." };
            }
        }
    }
}