using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;
using LoDCompanion.Utilities;
using System.Threading.Tasks;

namespace LoDCompanion.Services.Game
{
    public class SpellCastResult
    {
        public bool IsSuccess { get; set; }
        public bool IsMiscast { get; set; }
        public int ManaSpent { get; set; }
        public string OutcomeMessage { get; set; } = string.Empty;
    }

    public class SpellResolutionService
    {
        private readonly DungeonState _dungeon;
        private readonly EncounterService _encounter;
        private readonly InitiativeService _initiative;
        private readonly CombatManagerService _combatManager;
        private readonly UserRequestService _diceRoll;

        public SpellResolutionService(
            DungeonState dungeonState, 
            EncounterService encounterService,
            InitiativeService initiativeService,
            UserRequestService diceRoll,
            CombatManagerService combatManager)
        {
            _dungeon = dungeonState;
            _encounter = encounterService;
            _initiative = initiativeService;
            _diceRoll = diceRoll;
            _combatManager = combatManager;
        }

        /// <summary>
        /// The main entry point to resolve any successfully cast spell.
        /// It routes the spell to the correct handler based on its properties.
        /// </summary>
        public async Task<SpellCastResult> ResolveSpellAsync(Hero caster, Spell spell, Character initialTarget, SpellCastingResult options)
        {
            var result = new SpellCastResult { IsSuccess = true };

            // Handle Touch Spells first, as they require a to-hit roll
            if (spell.HasProperty(SpellProperty.Touch))
            {
                int touchAttackRoll = await _diceRoll.RequestRollAsync("Roll to touch target", "1d100");
                if (touchAttackRoll > caster.GetSkill(Skill.CombatSkill) + 20)
                {
                    result.OutcomeMessage = $"{caster.Name}'s touch spell misses {initialTarget.Name}.";
                    return result;
                }
            }

            // Route to specific handlers
            if (spell.School == MagicSchool.Restoration)
            {
                return await HandleHealingSpellAsync(caster, spell, initialTarget, options);
            }
            if (spell.School == MagicSchool.Destruction)
            {
                return await HandleDamageSpellAsync(caster, spell, initialTarget, options);
            }
            if (spell.School == MagicSchool.Conjuration)
            {
                return await HandleSummoningSpellAsync(caster, spell);
            }
            // All other schools fall under "Utility" which includes buffs, debuffs, etc.
            return await HandleUtilitySpellAsync(caster, spell, initialTarget, options);
        }

        private async Task<SpellCastResult> HandleDamageSpellAsync(Hero caster, Spell spell, Character target, SpellCastingResult options)
        {
            var outcome = new System.Text.StringBuilder();

            // Handle Area of Effect spells
            if (spell.HasProperty(SpellProperty.AreaOfEffectSpell))
            {
                int? radius = spell.Properties?.GetValueOrDefault(SpellProperty.Radius, 0);
                if (radius != null)
                {
                    var affectedSquares = GridService.GetAllSquaresInRadius(target.Position, (int)radius, _dungeon.DungeonGrid);
                    List<Character> allCharacters = [.. _dungeon.RevealedMonsters, .. _dungeon.HeroParty?.Heroes ?? new List<Hero>()];

                    foreach (var character in allCharacters.Where(c => affectedSquares.Contains(c.Position)))
                    {
                        bool isCenterTarget = character.Position.Equals(target.Position);
                        int damage = isCenterTarget ?
                            await GetDirectDamageAsync(caster, spell) :
                            await GetAOEDamageAsync(caster, spell);

                        damage += options.PowerLevels;

                        character.TakeDamage(damage, spell.DamageType);
                        outcome.AppendLine($"{character.Name} is hit by {spell.Name} for {damage} {spell.DamageType} damage!");
                    } 
                }
            }
            else // Single target damage
            {
                int damage = await GetDirectDamageAsync(caster, spell) + options.PowerLevels;
                target.TakeDamage(damage, spell.DamageType);
                outcome.AppendLine($"{spell.Name} hits {target.Name} for {damage} {spell.DamageType} damage!");
            }

            return new SpellCastResult { IsSuccess = true, OutcomeMessage = outcome.ToString() };
        }

        private async Task<SpellCastResult> HandleHealingSpellAsync(Hero caster, Spell spell, Character target, SpellCastingResult options)
        {
            if (target is not Hero heroTarget)
            {
                return new SpellCastResult { OutcomeMessage = "Healing spells can only target heroes." };
            }

            int healingAmount = await GetHealingAsync(caster, spell) + options.PowerLevels;

            if (spell.Name == "Life Force")
            {
                heroTarget.CurrentHP = heroTarget.GetStat(BasicStat.HitPoints); // Heals to full
            }
            else
            {
                heroTarget.CurrentHP = Math.Min(heroTarget.GetStat(BasicStat.HitPoints), heroTarget.CurrentHP + healingAmount);
            }

            return new SpellCastResult
            {
                IsSuccess = true,
                OutcomeMessage = $"{heroTarget.Name} is healed for {healingAmount} HP by {spell.Name}."
            };
        }

        private async Task<SpellCastResult> HandleSummoningSpellAsync(Hero caster, Spell spell)
        {
            Dictionary<string, string> summoningParams = new Dictionary<string, string>();
            if (spell.Name == "Summon Greater Demon")
            {
                summoningParams.TryAdd("Name", "Greater Demon");
                summoningParams.TryAdd("Weapons", "Greataxe");
            }
            else if (spell.Name == "Summon Lesser Demon")
            {
                summoningParams.TryAdd("Name", "Lesser Plague Demon");
            }
            else if (spell.Name == "Summon Demon")
            {
                int random = RandomHelper.GetRandomNumber(1, 2);
                if (random == 1)
                {
                    summoningParams.TryAdd("Name", "Blood Demon");
                }
                else
                {
                    summoningParams.TryAdd("Name", "Plague Demon");
                }
                summoningParams.TryAdd("Weapons", "Cursed Longsword");
            }
            else if (spell.Name == "Summon Fire Elemental")
            {
                summoningParams.TryAdd("Name", "Fire Elemental");
            }
            else if (spell.Name == "Summon Water Elemental")
            {
                summoningParams.TryAdd("Name", "Water Elemental");
            }
            else if (spell.Name == "Summon Earth Elemental")
            {
                summoningParams.TryAdd("Name", "Earth Elemental");
            }
            else if (spell.Name == "Summon Wind Elemental")
            {
                summoningParams.TryAdd("Name", "Wind Elemental");
            }

            if (summoningParams.Count > 0)
            {
                summoningParams.TryAdd("Count", "1");
            }

            if (summoningParams.Count <= 0)
            {
                return new SpellCastResult { IsSuccess = false, OutcomeMessage = "Spell failed to summon creature" };
            }

            // This is a placeholder. A full implementation would require a MonsterFactory
            // and logic to add the summoned monster to the combat.
            var summonedMonster = _encounter.GetEncounterByParams(summoningParams);
            if (summonedMonster != null)
            {
                if (spell.HasProperty(SpellProperty.RandomPlacement))
                {
                    var roomGrid = caster.Room.Grid;
                    roomGrid.Shuffle();

                    int i = 0;
                    while (roomGrid[i].IsOccupied || roomGrid[i].IsWall)
                    {
                        i++;
                    }

                    // Find a random empty square to place the monster
                    var placementPosition = roomGrid[i];
                    if (placementPosition != null)
                    {
                        summonedMonster[0].Room = caster.Room;
                        summonedMonster[0].Position = placementPosition.Position;
                        if (summonedMonster[0].Name != "Lesser Plague Demon")
                        {
                            _initiative.AddToken(ActorType.Hero); 
                        }
                        return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{caster.Name} summons a {summonedMonster[0].Name}!" };
                    }
                    return new SpellCastResult { IsSuccess = false, OutcomeMessage = "No space to summon the creature." };

                }
                
            }
            return new SpellCastResult { IsSuccess = false, OutcomeMessage = "Could not find the creature to summon." };
        }

        private async Task<SpellCastResult> HandleUtilitySpellAsync(Hero caster, Spell spell, Character target, SpellCastingResult options)
        {
            // Apply status effects or other unique spell rules
            if (spell.StatusEffect.HasValue)
            {
                var effectToApply = new ActiveStatusEffect(spell.StatusEffect.Value, 1);
                effectToApply.Duration = await GetDurationAsync(caster, spell);
                StatusEffectService.AttemptToApplyStatus(target, effectToApply); // Bypasses resistance for direct spell effects
                return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{target.Name} is affected by {spell.Name}!" };
            }

            switch (spell.Name)
            {
                case "Strengthen Body":
                    // This requires a mechanism to apply temporary stat bonuses.
                    // For now, we apply a status effect that other services can check.
                    var statToBoost = await _diceRoll.RequestChoiceAsync("Choose Stat", new List<string> { "Strength", "Constitution" });
                    var effect = statToBoost == "Strength" ? StatusEffectType.StrengthenBodyStrength : StatusEffectType.StrengthenBodyConstitution;
                    var statusEffect = new ActiveStatusEffect(effect, 1);
                    statusEffect.Duration = await GetDurationAsync(caster, spell);
                    StatusEffectService.AttemptToApplyStatus(target, statusEffect);
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{target.Name}'s {statToBoost} is bolstered!" };

                case "Time Freeze":
                    foreach(Hero hero in _combatManager.GetActivatedHeroes())
                    {
                        if (hero.CurrentAP <= 0)
                        {
                            hero.ResetActionPoints();
                            _initiative.AddToken(ActorType.Hero); // Add a new token for the hero to act again
                        }
                    }
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = "Time freezes! The heroes feel a surge of energy and can act again." };

                default:
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{spell.Name} is cast, its ancient magic weaving through the air." };
            }
        }

        private async Task<int> GetDurationAsync(Hero caster, Spell spell)
        {
            if (spell.HasProperty(SpellProperty.TurnDuration))
            {
                int duration = spell.Properties?[SpellProperty.TurnDuration] ?? 0;
                if (spell.HasProperty(SpellProperty.DiceCount))
                {
                    duration += await _diceRoll.RequestRollAsync("Roll for duration", $"{spell.Properties?[SpellProperty.DiceCount]}d{spell.Properties?[SpellProperty.DiceMaxValue]}");
                }
                if (spell.HasProperty(SpellProperty.AddCasterLvlToDuration))
                {
                    duration += caster.GetStat(BasicStat.Level);
                }
                return duration;
            }
            else return 0;
        }

        private async Task<int> GetHealingAsync(Hero caster, Spell spell)
        {
            int healing = 0;

            if (spell.HasProperty(SpellProperty.DiceCount))
            {
                healing += await _diceRoll.RequestRollAsync("Roll for healing amount", $"{spell.Properties?[SpellProperty.DiceCount]}d{spell.Properties?[SpellProperty.DiceMaxValue]}");
            }
            if (spell.HasProperty(SpellProperty.IncludeCasterLevelInDamage))
            {
                healing += caster.GetStat(BasicStat.Level);
            }
            return healing;
        }

        private async Task<int> GetDirectDamageAsync(Hero caster, Spell spell)
        {
            int damage = 0;
            if (spell.HasProperty(SpellProperty.TurnDuration))
            {
                if (spell.HasProperty(SpellProperty.DiceCount2))
                {
                    damage += await _diceRoll.RequestRollAsync("Roll for direct dmage", $"{spell.Properties?[SpellProperty.DiceCount2]}d{spell.Properties?[SpellProperty.DiceMaxValue2]}");
                }
                if (spell.HasProperty(SpellProperty.IncludeCasterLevelInDamage))
                {
                    damage += caster.GetStat(BasicStat.Level);
                }
                return damage;
            }
            else if (!spell.HasProperty(SpellProperty.TurnDuration))
            {
                if (spell.HasProperty(SpellProperty.DiceCount))
                {
                    damage += await _diceRoll.RequestRollAsync("Roll for direct dmage", $"{spell.Properties?[SpellProperty.DiceCount]}d{spell.Properties?[SpellProperty.DiceMaxValue]}");
                }
                if (spell.HasProperty(SpellProperty.IncludeCasterLevelInDamage))
                {
                    damage += caster.GetStat(BasicStat.Level);
                }
                return damage;
            }
            else return 0;
        }

        private async Task<int> GetAOEDamageAsync(Hero caster, Spell spell)
        {
            int damage = 0;
            if (spell.HasProperty(SpellProperty.AreaOfEffectSpell))
            {
                if (spell.HasProperty(SpellProperty.AOEDiceCount))
                {
                    damage += await _diceRoll.RequestRollAsync("Roll for area of effect dmage", $"{spell.Properties?[SpellProperty.AOEDiceCount]}d{spell.Properties?[SpellProperty.AOEDiceMaxValue]}");
                }
                if (spell.HasProperty(SpellProperty.IncludeCasterLevelInDamage))
                {
                    damage += caster.GetStat(BasicStat.Level);
                }
                return damage;
            }
            else return 0;
        }

        private async Task<int> GetAOESecondaryDamageAsync(Hero caster, Spell spell)
        {
            int damage = 0;
            if (spell.HasProperty(SpellProperty.AreaOfEffectSpell))
            {
                if (spell.HasProperty(SpellProperty.AOEDiceCount2))
                {
                    damage += await _diceRoll.RequestRollAsync("Roll for area of effect dmage", $"{spell.Properties?[SpellProperty.AOEDiceCount2]}d{spell.Properties?[SpellProperty.AOEDiceMaxValue2]}");
                }
                if (spell.HasProperty(SpellProperty.IncludeCasterLevelInDamage))
                {
                    damage += caster.GetStat(BasicStat.Level);
                }
                return damage;
            }
            else return 0;
        }
    }
}