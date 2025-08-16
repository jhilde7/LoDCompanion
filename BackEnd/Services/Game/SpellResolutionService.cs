using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Text;

namespace LoDCompanion.BackEnd.Services.Game
{
    public class SpellCastResult
    {
        public bool IsSuccess { get; set; }
        public bool IsMiscast { get; set; }
        public int ManaSpent { get; set; }
        public string OutcomeMessage { get; set; } = string.Empty;

    }

    /// <summary>
    /// Represents a spell that is being cast over multiple actions (due to Focus).
    /// </summary>
    public class ChanneledSpell
    {
        public Hero Caster { get; }
        public Spell Spell { get; }
        public object Target { get; } // Can be a Character or GridPosition
        public SpellCastingResult CastingOptions { get; }
        public int FocusActionsRemaining { get; set; }

        public ChanneledSpell(Hero caster, Spell spell, object target, SpellCastingResult options)
        {
            Caster = caster;
            Spell = spell;
            Target = target;
            CastingOptions = options;
            FocusActionsRemaining = options.FocusPoints;
        }
    }

    public class SpellResolutionService
    {
        private readonly DungeonState _dungeon;
        private readonly EncounterService _encounter;
        private readonly InitiativeService _initiative;
        private readonly UserRequestService _diceRoll;
        private readonly FloatingTextService _floatingText;
        private readonly PowerActivationService _powerActivation;

        public event Action? OnTimeFreezeCast;
        public event Func<Hero, Monster, Task<AttackResult>>? OnTouchAttack;

        public SpellResolutionService(
            DungeonState dungeonState,
            EncounterService encounterService,
            InitiativeService initiativeService,
            UserRequestService diceRoll,
            FloatingTextService floatingText,
            PowerActivationService powerActivation)
        {
            _dungeon = dungeonState;
            _encounter = encounterService;
            _initiative = initiativeService;
            _diceRoll = diceRoll;
            _floatingText = floatingText;
            _powerActivation = powerActivation;
        }

        /// <summary>
        /// The main entry point to resolve any successfully cast spell.
        /// It routes the spell to the correct handler based on its properties.
        /// </summary>
        public async Task<SpellCastResult> ResolveSpellAsync(Hero caster, Spell spell, object initialTarget, SpellCastingResult options)
        {
            var (centerPosition, singleTarget) = GetSpellTargetingInfo(initialTarget);
            if (centerPosition == null)
            {
                return new SpellCastResult
                {
                    IsSuccess = false,
                    OutcomeMessage = "Invalid target for spell."
                };
            }

            // Route to specific handlers
            if (spell.School == MagicSchool.Restoration && singleTarget != null)
            {
                return await HandleHealingSpellAsync(caster, spell, singleTarget, options);
            }
            if (spell.School == MagicSchool.Destruction)
            {
                return await HandleDamageSpellAsync(caster, spell, initialTarget, options);
            }
            if (spell.School == MagicSchool.Conjuration)
            {
                return HandleSummoningSpell(caster, centerPosition, spell);
            }
            // All other schools fall under "Utility" which includes buffs, debuffs, etc.
            if (singleTarget != null)
            {
                return await HandleUtilitySpellAsync(caster, spell, singleTarget, options);
            }

            // Handle Touch Spells first, as they require a to-hit roll
            if (spell.HasProperty(SpellProperty.Touch))
            {
                if (singleTarget != null && OnTouchAttack != null && singleTarget is Monster monster)
                {
                    var result = await OnTouchAttack.Invoke(caster, monster);
                    if (!result.IsHit)
                    {
                        return new SpellCastResult
                        {
                            IsSuccess = true,
                            OutcomeMessage = $"{caster.Name}'s touch spell misses {singleTarget.Name}."
                        };
                    }
                }
                else
                {
                    return new SpellCastResult { IsSuccess = false, OutcomeMessage = "Invalid target for spell." };
                }
            }

            return new SpellCastResult
            {
                IsSuccess = false,
                OutcomeMessage = "Spell casting failed due to invalid target or spell properties."
            };
        }

        private async Task<SpellCastResult> HandleDamageSpellAsync(Hero caster, Spell spell, object target, SpellCastingResult options)
        {
            var outcome = new StringBuilder();
            var (centerPosition, singleTarget) = GetSpellTargetingInfo(target);
            var innerPower = caster.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.InnerPower);
            int addDamage = 0;

            if (innerPower != null && spell.HasProperty(SpellProperty.MagicMissile))
            {
                var resultRoll = await _diceRoll.RequestRollAsync("Roll for your inner power", $"{innerPower.DiceToRoll}"); 
                await Task.Yield();
                addDamage = resultRoll.Roll;
            }

            if (spell.Name == "Lightning Bolt")
            {
                var hitTargets = new List<Character>();
                var result = new SpellCastResult { IsSuccess = true };

                if (singleTarget != null)
                {
                    var resultRoll = await _diceRoll.RequestRollAsync("Roll for Lightning Bolt primary damage",
                        $"{spell.Properties?[SpellProperty.DiceCount]}d{spell.Properties?[SpellProperty.DiceMaxValue]}"); 
                    await Task.Yield();
                    int primaryDamage = resultRoll.Roll;
                    primaryDamage += options.PowerLevels;
                    if (spell.HasProperty(SpellProperty.MagicMissile)) primaryDamage += addDamage;
                    await singleTarget.TakeDamageAsync(primaryDamage, (_floatingText, singleTarget.Position), _powerActivation, damageType: spell.DamageType);
                    outcome.AppendLine($"{spell.Name} strikes {singleTarget.Name} for {primaryDamage} {spell.DamageType} damage!");
                    hitTargets.Add(singleTarget);

                    // First Jump
                    Character? secondTarget = FindNextChainTarget(singleTarget, hitTargets, 3);
                    if (secondTarget != null)
                    {
                        resultRoll = await _diceRoll.RequestRollAsync("Roll for Lightning Bolt secondary damage",
                        $"{spell.Properties?[SpellProperty.AOEDiceCount]}d{spell.Properties?[SpellProperty.AOEDiceMaxValue]}"); 
                        await Task.Yield();
                        int secondDamage = resultRoll.Roll;
                        secondDamage += options.PowerLevels;
                        if (spell.HasProperty(SpellProperty.MagicMissile)) secondDamage += addDamage;
                        await secondTarget.TakeDamageAsync(secondDamage, (_floatingText, secondTarget.Position), _powerActivation, damageType: spell.DamageType);
                        outcome.AppendLine($"The bolt chains to {secondTarget.Name} for {secondDamage} {spell.DamageType} damage!");
                        hitTargets.Add(secondTarget);

                        // Second Jump
                        Character? thirdTarget = FindNextChainTarget(secondTarget, hitTargets, 3);
                        if (thirdTarget != null)
                        {
                            resultRoll = await _diceRoll.RequestRollAsync("Roll for Lightning Bolt tertiary damage",
                        $"{spell.Properties?[SpellProperty.AOEDiceCount2]}d{spell.Properties?[SpellProperty.AOEDiceMaxValue2]}"); 
                            await Task.Yield();
                            int thirdDamage = resultRoll.Roll;
                            thirdDamage += options.PowerLevels;
                            if (spell.HasProperty(SpellProperty.MagicMissile)) thirdDamage += addDamage;
                            await thirdTarget.TakeDamageAsync(thirdDamage, (_floatingText, thirdTarget.Position), _powerActivation, damageType: spell.DamageType);
                            outcome.AppendLine($"It chains again to {thirdTarget.Name} for {thirdDamage} {spell.DamageType} damage!");
                        }
                    }
                }
                result.OutcomeMessage = outcome.ToString();
                return result;
            }
            // Handle Area of Effect spells
            if (spell.HasProperty(SpellProperty.AreaOfEffectSpell))
            {
                int? radius = spell.Properties?.GetValueOrDefault(SpellProperty.Radius, 0);
                if (radius != null)
                {
                    var center = centerPosition ?? singleTarget?.Position;
                    List<GridPosition> affectedSquares = new List<GridPosition>();
                    if (singleTarget != null && singleTarget.Position != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(singleTarget.Position, (int)radius, _dungeon.DungeonGrid);
                    }
                    else if (centerPosition != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(centerPosition, (int)radius, _dungeon.DungeonGrid);
                    }
                    List<Character> allCharacters = _dungeon.AllCharactersInDungeon;

                    foreach (var character in allCharacters.Where(c => c.Position != null && affectedSquares.Contains(c.Position)))
                    {
                        if (character.Position == null) continue;
                        bool isCenterTarget = character.Position.Equals(center);
                        int damage = isCenterTarget ?
                            await GetDirectDamageAsync(caster, spell) :
                            await GetAOEDamageAsync(caster, spell);

                        damage += options.PowerLevels;
                        if (spell.HasProperty(SpellProperty.MagicMissile)) damage += addDamage;

                        await character.TakeDamageAsync(damage, (_floatingText, character.Position), _powerActivation, damageType: spell.DamageType);
                        outcome.AppendLine($"{character.Name} is hit by {spell.Name} for {damage} {spell.DamageType} damage!");
                    }
                }
            }
            else if (singleTarget != null) // Single target damage
            {
                int damage = await GetDirectDamageAsync(caster, spell) + options.PowerLevels;
                if (spell.HasProperty(SpellProperty.MagicMissile)) damage += addDamage;
                await singleTarget.TakeDamageAsync(damage, (_floatingText, singleTarget.Position), _powerActivation, damageType: spell.DamageType);
                outcome.AppendLine($"{spell.Name} hits {singleTarget.Name} for {damage} {spell.DamageType} damage!");
            }

            if (innerPower != null) caster.ActiveStatusEffects.Remove(innerPower);
            return new SpellCastResult { IsSuccess = true, OutcomeMessage = outcome.ToString() };
        }

        private Character? FindNextChainTarget(Character currentTarget, List<Character> hitTargets, int maxJumpDistance)
        {
            if (currentTarget.Position == null) return null;
            return _dungeon.AllCharactersInDungeon
                .Where(c => !hitTargets.Contains(c) && c.CurrentHP > 0 && c.Position != null && currentTarget.Position != null)
                .Where(c => c.Position != null && GridService.GetDistance(currentTarget.Position, c.Position) <= maxJumpDistance)
                .OrderBy(c => GridService.GetDistance(currentTarget.Position, c.Position ?? currentTarget.Position))
                .FirstOrDefault();
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
                heroTarget.Heal(heroTarget.GetStat(BasicStat.HitPoints)); // Heals to full
            }
            else
            {
                heroTarget.Heal(healingAmount);
            }

            return new SpellCastResult
            {
                IsSuccess = true,
                OutcomeMessage = $"{heroTarget.Name} is healed for {healingAmount} HP by {spell.Name}."
            };
        }

        private SpellCastResult HandleSummoningSpell(Hero caster, GridPosition position, Spell spell)
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
                    var placementPosition = GetRandomPlacement(caster);
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
                else
                {
                    // Place the summoned monster at the specified position
                    summonedMonster[0].Room = caster.Room;
                    summonedMonster[0].Position = position;
                    _initiative.AddToken(ActorType.Hero);
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{caster.Name} summons a {summonedMonster[0].Name} at {position}!" };
                }

            }
            return new SpellCastResult { IsSuccess = false, OutcomeMessage = "Could not find the creature to summon." };
        }

        /// <summary>
        /// Handles all non-damaging, non-healing spells cast by a hero.
        /// </summary>
        private async Task<SpellCastResult> HandleUtilitySpellAsync(Hero caster, Spell spell, object target, SpellCastingResult options)
        {
            var result = new SpellCastResult { IsSuccess = true };
            var effectToApply = spell.StatusEffect.HasValue ? new ActiveStatusEffect(spell.StatusEffect.Value, -1) : null;

            switch (spell.Name)
            {
                // --- ALTERATION SPELLS ---
                case "Gust of Wind":
                case "Levitate":
                case "Speed":
                    // These are self/ally buffs with durations, handled by the default case.
                    break;
                case "Open Lock":
                    if (target is Door door && door.Properties != null)
                    {
                        // Logic to unlock a door
                        if (door.Properties.ContainsKey(DoorProperty.Locked)) door.Properties.Remove(DoorProperty.Locked);
                        result.OutcomeMessage = $"{caster.Name} magically unlocks the door!";
                    }
                    else if (target is Chest chest && chest.Properties != null)
                    {
                        // Logic to unlock a door
                        if (chest.Properties.ContainsKey(ChestProperty.Locked)) chest.Properties.Remove(ChestProperty.Locked);
                        result.OutcomeMessage = $"{caster.Name} magically unlocks the chest!";
                    }
                    else
                    {
                        result.OutcomeMessage = "Open Lock can only target a door or chest.";
                    }
                    return result;

                case "Seal Door":
                    if (target is Door doorToSeal)
                    {
                        if (doorToSeal.Properties != null)
                        {
                            doorToSeal.Properties.TryAdd(DoorProperty.MagicallySealed, await GetDurationAsync(caster, spell));
                        }
                        else
                        {
                            doorToSeal.Properties = new Dictionary<DoorProperty, int> { { DoorProperty.MagicallySealed, await GetDurationAsync(caster, spell) } };
                        }
                        result.OutcomeMessage = $"{caster.Name} magically seals a nearby door!";
                    }
                    else
                    {
                        result.OutcomeMessage = "Seal Door can only target a door.";
                    }
                    return result;

                case "Transpose":
                    if (target is Hero otherHero && caster.Position != null && otherHero.Position != null)
                    {
                        var casterPos = caster.Position;
                        var targetPos = otherHero.Position;
                        GridService.MoveCharacterToPosition(caster, targetPos, _dungeon.DungeonGrid);
                        GridService.MoveCharacterToPosition(otherHero, casterPos, _dungeon.DungeonGrid);
                        result.OutcomeMessage = $"{caster.Name} and {otherHero.Name} swap positions!";
                    }
                    else
                    {
                        result.OutcomeMessage = "Transpose can only target another hero.";
                    }
                    return result;

                // --- DIVINATION SPELLS ---
                case "Second Sight":
                    // This logic would need to be implemented in your DungeonManagerService
                    result.OutcomeMessage = $"{caster.Name} peers through the next door, gaining a tactical advantage.";
                    return result;

                case "Time Freeze":

                    OnTimeFreezeCast?.Invoke();
                    return new SpellCastResult { IsSuccess = true, OutcomeMessage = "Time freezes! The heroes can act again." };
                // --- HEX SPELLS ---
                case "Hold Creature":
                case "Silence":
                case "Slow":
                case "Weakness":
                    // These are all debuffs with durations, handled by the default case.
                    break;

                // --- MYSTICISM SPELLS ---
                case "Bolstered Mind":
                case "Protective Shield":
                case "Strengthen Body":
                    // These are all buffs with durations, handled by the default case.
                    break;
            }

            // --- DEFAULT HANDLER FOR MOST STATUS EFFECT SPELLS ---
            if (effectToApply != null)
            {
                effectToApply.Duration = await GetDurationAsync(caster, spell);

                // For effects requiring a Resolve Test to apply
                if (target is Character charater && spell.HasProperty(SpellProperty.ResolveTest))
                {
                    int resolveRoll = RandomHelper.RollDie(DiceType.D100);
                    if (resolveRoll <= charater.GetStat(BasicStat.Resolve))
                    {
                        return new SpellCastResult { IsSuccess = true, OutcomeMessage = $"{charater.Name} resisted the effects of {spell.Name}!" };
                    }
                    await StatusEffectService.AttemptToApplyStatusAsync(charater, effectToApply, _powerActivation);
                    result.OutcomeMessage = $"{charater.Name} is affected by {spell.Name}!";
                }
            }
            else
            {
                result.OutcomeMessage = $"{spell.Name} is cast, its ancient magic weaving through the air.";
            }

            return result;
        }

        /// <summary>
        /// Resolves the effect of a spell cast by a monster.
        /// </summary>
        public async Task<SpellCastResult> ResolveMonsterSpellAsync(Monster caster, MonsterSpell spell, object target)
        {
            var result = new SpellCastResult { IsSuccess = true };
            var outcome = new StringBuilder($"{caster.Name} casts {spell.Name}!");

            // Determine Targets
            var (centerPosition, singleTarget) = GetSpellTargetingInfo(target);
            if (centerPosition == null)
            {
                return new SpellCastResult { IsSuccess = false, OutcomeMessage = "Invalid target for spell." };
            }
            var affectedCharacters = GetCharactersInArea(spell.TargetType, centerPosition, spell.Properties?.GetValueOrDefault(SpellProperty.Radius) ?? 0);

            // Spend AP Cost
            caster.SpendActionPoints(spell.CostAP);

            // Attempt to cast the spell
            if (RandomHelper.RollDie(DiceType.D100) > caster.GetSkill(Skill.RangedSkill))
            {
                return new SpellCastResult { IsSuccess = false, OutcomeMessage = $"{caster.Name} fails to cast spell {spell.Name}." };
            }

            // Dispel spell logic
            if (target is Hero hero && hero.Party != null && hero.Party.Heroes.Any(h => h.ProfessionName == "Wizard"))
            {
                foreach (var wizard in hero.Party.Heroes.Where(h => h.ProfessionName == "Wizard"))
                {
                    var attemptDispel = await _diceRoll.RequestYesNoChoiceAsync($"Does {wizard.Name} want to try and attempt to dispel {spell.Name}, " +
                        $"this attempt will prevent {wizard.Name} from casting spells onm there next activation.");
                    await Task.Yield();
                    if (attemptDispel)
                    {
                        var dispelMaster = wizard.Perks.FirstOrDefault(p => p.Name == PerkName.DispelMaster);
                        if(dispelMaster != null)
                        {
                            var activateDispelMaster = await _diceRoll.RequestYesNoChoiceAsync($"Does {wizard.Name} wish to activate {dispelMaster.Name.ToString()}");
                            await Task.Yield();
                            if (activateDispelMaster)
                            {
                                await _powerActivation.ActivatePerkAsync(wizard, dispelMaster);
                            }
                        }
                        wizard.CanCastSpell = false;
                        var rollResult = await _diceRoll.RequestRollAsync($"Roll {Skill.ArcaneArts.ToString()} test", "1d100", skill: Skill.ArcaneArts);
                        await Task.Yield();
                        var skillTarget = wizard.GetSkill(Skill.ArcaneArts);

                        var activeDispelMaster = wizard.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.DispelMaster);
                        if (activeDispelMaster != null) wizard.ActiveStatusEffects.Remove(activeDispelMaster);

                        if (rollResult.Roll <= skillTarget)
                        {
                            return new SpellCastResult { IsSuccess = false, OutcomeMessage = $"{wizard.Name} foils the casting of spell {spell.Name}." };
                        }
                    }
                }
            }

            if (spell.TargetType == TargetType.Ally && singleTarget != null && singleTarget is Monster allyTarget)
            {
                return HandleHealingSpell(caster, spell, allyTarget);
            }
            else if (spell.TargetType == TargetType.SingleTarget || spell.TargetType == TargetType.AreaOfEffect)
            {
                return await HandleDamageSpellAsync(caster, spell, target);
            }

            if (spell.StatusEffect != null && singleTarget != null && spell.StatusEffect != StatusEffectType.RaiseDead)
            {
                result.OutcomeMessage = await HandleStatusEffectingSpellAsync(caster, spell, singleTarget);
                return result;
            }

            // Handle Special Non-Targeted Effects (Summoning, Auras, etc.)
            if (spell.TargetType == TargetType.NoTarget)
            {
                switch (spell.Name)
                {
                    case "Raise dead":
                        //TODO: Implement logic to find a fallen undead ally this may be done by evaluating corpses.
                        var fallenUndead = _dungeon.RevealedMonsters.FirstOrDefault(m => m.IsUndead && m.CurrentHP <= 0);
                        if (fallenUndead != null)
                        {
                            fallenUndead.Heal(fallenUndead.GetStat(BasicStat.HitPoints));
                            outcome.AppendLine(await HandleStatusEffectingSpellAsync(caster, spell, fallenUndead));
                        }
                        else
                        {
                            var woundedUndead = _dungeon.RevealedMonsters
                                .Where(m => m.IsUndead && m.CurrentHP < m.GetStat(BasicStat.HitPoints))
                                .OrderBy(m => m.CurrentHP)
                                .FirstOrDefault();

                            if (woundedUndead != null)
                            {
                                int healing = RandomHelper.RollDie(DiceType.D6);
                                woundedUndead.Heal(healing);
                                outcome.AppendLine($" Dark energy knits the wounds of {woundedUndead.Name}, healing {healing} HP.");
                            }
                            else
                            {
                                outcome.AppendLine(" The spell fizzles, finding no suitable target.");
                            }
                        }
                        break;
                    case "Summon demon":
                    case "Summon greater demon":
                        var summonedMonster = GetSummonedDemon(spell);
                        var placementPos = GetRandomPlacement(caster);
                        if (summonedMonster != null && placementPos != null)
                        {
                            summonedMonster.Position = placementPos.Position;
                            summonedMonster.Room = caster.Room;
                            _dungeon.RevealedMonsters.Add(summonedMonster);
                            _initiative.AddToken(ActorType.Monster);
                            outcome.AppendLine($" A terrifying {summonedMonster.Name} appears!");
                        }
                        else
                        {
                            outcome.AppendLine(" The summoning fizzles, there is no space!");
                        }
                        break;
                }
            }

            result.OutcomeMessage = outcome.ToString();
            return result;
        }

        private async Task<string> HandleStatusEffectingSpellAsync(Monster caster, MonsterSpell spell, Character target)
        {
            var outcome = new StringBuilder($"{caster.Name} casts {spell.Name}!");

            if (spell.StatusEffect != null)
            {
                // Add specific flavor text for certain spells
                switch (spell.Name)
                {
                    case "Blind":
                        outcome.Append(" A flash of light erupts!");
                        break;
                    case "Gust of wind":
                        outcome.Append(" A howling wind fills the area!");
                        break;
                    case "Mute":
                        outcome.Append(" An arcane silence descends!");
                        break;
                    case "Mirrored self":
                        outcome.Append(" An identical copy of the caster appears!");
                        break;
                    case "Shield":
                        outcome.Append(" A shimmering shield surrounds the target!");
                        break;
                    case "Seduce":
                        outcome.Append(" The target's will is bent to the caster's!");
                        break;
                    case "Slow":
                        outcome.Append(" The target suddenly feels sluggish and heavy.");
                        break;
                    case "Stun":
                        outcome.Append(" A jolt of energy leaves the target reeling!");
                        break;
                    case "Frenzy":
                        outcome.Append(" The target is filled with an uncontrollable rage!");
                        break;
                    case "Raise dead":
                        outcome.Append(" A fallen ally begins to stir...");
                        break;
                    case "Frost ray":
                        outcome.Append(" The target is frozen and has restricted actions");
                        break;
                }

                int duration = spell.Properties?.GetValueOrDefault(SpellProperty.TurnDuration, -1) ?? -1;

                await StatusEffectService.AttemptToApplyStatusAsync(target, new ActiveStatusEffect((StatusEffectType)spell.StatusEffect, duration), _powerActivation);
                outcome.Append($" {target.Name} is now affected by {spell.StatusEffect.ToString()}.");
            }

            return outcome.ToString();
        }

        private Monster? GetSummonedDemon(MonsterSpell spell)
        {
            if (spell.Properties == null) return null;
            int roll = RandomHelper.GetRandomNumber(spell.Properties.GetValueOrDefault(SpellProperty.DiceCount, 1),
                                            spell.Properties.GetValueOrDefault(SpellProperty.DiceMaxValue, 10));

            string monsterToSummonName = "";
            if (spell.Name == "Summon demon")
            {
                if (roll <= 6) monsterToSummonName = "Lesser Plague Demon";
                else if (roll <= 8) monsterToSummonName = "Blood Demon";
                else monsterToSummonName = "Plague Demon";
            }
            else if (spell.Name == "Summon greater demon")
            {
                if (roll <= 6) monsterToSummonName = "Bloated Demon";
                else if (roll <= 9) monsterToSummonName = "Lurker";
                else monsterToSummonName = "Greater Demon";
            }

            Dictionary<string, string> summoningParams = new Dictionary<string, string>();
            summoningParams.TryAdd("Name", monsterToSummonName);
            summoningParams.TryAdd("Count", "1");

            if (monsterToSummonName == "Greater Demon" || monsterToSummonName == "Bloated Demon")
            {
                summoningParams.TryAdd("Weapons", "Greataxe");
            }
            else if (monsterToSummonName == "Blood Demon" || monsterToSummonName == "Plague Demon")
            {
                summoningParams.TryAdd("Weapons", "Cursed Longsword");
            }

            return !string.IsNullOrEmpty(monsterToSummonName) ? _encounter.GetEncounterByParams(summoningParams)[0] : null;
        }

        private async Task<SpellCastResult> HandleDamageSpellAsync(Monster caster, MonsterSpell spell, object target)
        {
            var result = new SpellCastResult { IsSuccess = true };
            var outcome = new StringBuilder();
            var (centerPosition, singleTarget) = GetSpellTargetingInfo(target);

            // Handle Area of Effect spells
            if (spell.Properties != null && spell.Properties.ContainsKey(SpellProperty.AreaOfEffectSpell))
            {
                int? radius = spell.Properties?.GetValueOrDefault(SpellProperty.Radius, 0);
                if (radius != null)
                {
                    var center = centerPosition ?? singleTarget?.Position;
                    List<GridPosition> affectedSquares = new List<GridPosition>();
                    if (singleTarget != null && singleTarget.Position != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(singleTarget.Position, (int)radius, _dungeon.DungeonGrid);
                    }
                    else if (centerPosition != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(centerPosition, (int)radius, _dungeon.DungeonGrid);
                    }
                    List<Character> allCharacters = _dungeon.AllCharactersInDungeon;

                    foreach (var character in allCharacters.Where(c => c.Position != null && affectedSquares.Contains(c.Position)))
                    {
                        if (character.Position == null) continue;
                        bool isCenterTarget = character.Position.Equals(center);
                        int damage = isCenterTarget ?
                            GetDirectDamage(caster, spell) :
                            GetAOEDamage(caster, spell);

                        await character.TakeDamageAsync(damage, (_floatingText, character.Position), _powerActivation, damageType: spell.DamageType != null ? spell.DamageType : null);
                        outcome.AppendLine($"{character.Name} is hit by {spell.Name} for {damage} {spell.DamageType} damage!");
                    }
                }
            }
            else if (singleTarget != null) // Single target damage
            {
                int damage = GetDirectDamage(caster, spell);
                await singleTarget.TakeDamageAsync(damage, (_floatingText, singleTarget.Position), _powerActivation, damageType: spell.DamageType != null ? spell.DamageType : null);
                outcome.AppendLine($"{spell.Name} hits {singleTarget.Name} for {damage} {spell.DamageType} damage!");
            }

            if (spell.StatusEffect != null && singleTarget != null)
            {
                result.OutcomeMessage = await HandleStatusEffectingSpellAsync(caster, spell, singleTarget);
            }

            return result;
        }

        private SpellCastResult HandleHealingSpell(Monster caster, MonsterSpell spell, Monster target)
        {
            int healingAmount = GetHealing(caster, spell);

            target.Heal(healingAmount);

            return new SpellCastResult
            {
                IsSuccess = true,
                OutcomeMessage = $"{target.Name} is healed for {healingAmount} HP by {spell.Name}."
            };
        }

        private GridSquare GetRandomPlacement(Character caster)
        {
            var roomGrid = caster.Room.Grid.Keys.ToList();
            roomGrid.Shuffle();

            // Find a random empty square to place the monster
            int i = 0;
            while (caster.Room.Grid[roomGrid[i]].IsOccupied || caster.Room.Grid[roomGrid[i]].IsWall)
            {
                i++;
            }
            var placementPosition = caster.Room.Grid[roomGrid[i]];

            return placementPosition;
        }

        /// <summary>
        /// Determines the center of a spell's effect and the primary single target.
        /// </summary>
        private (GridPosition?, Character?) GetSpellTargetingInfo(object target)
        {
            if (target is Character characterTarget)
            {
                return (characterTarget.Position, characterTarget);
            }
            if (target is GridPosition positionTarget)
            {
                // For AOE spells targeting a square, find if a character is at that center.
                var characterAtCenter = _dungeon.AllCharactersInDungeon.FirstOrDefault(c => c.Position != null && c.Position.Equals(positionTarget));
                return (positionTarget, characterAtCenter);
            }
            return (null, null);
        }

        /// <summary>
        /// Gets all characters within the area of effect of a spell.
        /// </summary>
        private List<Character> GetCharactersInArea(TargetType targetType, GridPosition center, int radius)
        {
            var allCharacters = _dungeon.AllCharactersInDungeon; // Assumes a helper in DungeonState

            if (targetType == TargetType.SingleTarget)
            {
                return allCharacters.Where(c => c.Position != null && c.Position.Equals(center)).ToList();
            }

            if (targetType == TargetType.AreaOfEffect)
            {
                var affectedSquares = GridService.GetAllSquaresInRadius(center, radius, _dungeon.DungeonGrid);
                return allCharacters.Where(c => c.Position != null && c.OccupiedSquares.Any(os => affectedSquares.Contains(os))).ToList();
            }

            return new List<Character>();
        }

        private async Task<int> GetDurationAsync(Hero caster, Spell spell)
        {
            if (spell.HasProperty(SpellProperty.TurnDuration))
            {
                int duration = spell.Properties?[SpellProperty.TurnDuration] ?? 0;
                if (spell.HasProperty(SpellProperty.DiceCount))
                {
                    var resultRoll = await _diceRoll.RequestRollAsync("Roll for duration",
                        $"{spell.Properties?[SpellProperty.DiceCount]}d{spell.Properties?[SpellProperty.DiceMaxValue]}"); await Task.Yield();
                    duration += resultRoll.Roll;
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
                var resultRoll = await _diceRoll.RequestRollAsync("Roll for healing amount",
                    $"{spell.Properties?[SpellProperty.DiceCount]}d{spell.Properties?[SpellProperty.DiceMaxValue]}"); await Task.Yield();
                healing += resultRoll.Roll;
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
                    var resultRoll = await _diceRoll.RequestRollAsync("Roll for direct damage",
                        $"{spell.Properties?[SpellProperty.DiceCount2]}d{spell.Properties?[SpellProperty.DiceMaxValue2]}"); await Task.Yield();
                    damage += resultRoll.Roll;
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
                    var resultRoll = await _diceRoll.RequestRollAsync("Roll for direct damage",
                        $"{spell.Properties?[SpellProperty.DiceCount]}d{spell.Properties?[SpellProperty.DiceMaxValue]}"); await Task.Yield();
                    damage += resultRoll.Roll;
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
                    var resultRoll = await _diceRoll.RequestRollAsync("Roll for area of effect damage",
                        $"{spell.Properties?[SpellProperty.AOEDiceCount]}d{spell.Properties?[SpellProperty.AOEDiceMaxValue]}"); await Task.Yield();
                    damage += resultRoll.Roll;
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
                    var resultRoll = await _diceRoll.RequestRollAsync("Roll for area of effect damage",
                        $"{spell.Properties?[SpellProperty.AOEDiceCount2]}d{spell.Properties?[SpellProperty.AOEDiceMaxValue2]}"); await Task.Yield();
                    damage += resultRoll.Roll;
                }
                if (spell.HasProperty(SpellProperty.IncludeCasterLevelInDamage))
                {
                    damage += caster.GetStat(BasicStat.Level);
                }
                return damage;
            }
            else return 0;
        }

        private int GetHealing(Monster caster, MonsterSpell spell)
        {
            int healing = 0;

            if (spell.Properties != null && spell.Properties.ContainsKey(SpellProperty.DiceCount))
            {
                healing += RandomHelper.GetRandomNumber(spell.Properties[SpellProperty.DiceCount], spell.Properties[SpellProperty.DiceMaxValue]);
            }
            return healing;
        }

        private int GetDirectDamage(Monster caster, MonsterSpell spell)
        {
            int damage = 0;
            if (spell.Properties != null && spell.Properties.ContainsKey(SpellProperty.TurnDuration))
            {
                if (spell.Properties.ContainsKey(SpellProperty.DiceCount2))
                {
                    damage += RandomHelper.GetRandomNumber(spell.Properties[SpellProperty.DiceCount2], spell.Properties[SpellProperty.DiceMaxValue2]);
                }
                return damage;
            }
            else if (spell.Properties != null && !spell.Properties.ContainsKey(SpellProperty.TurnDuration))
            {
                if (spell.Properties.ContainsKey(SpellProperty.DiceCount))
                {
                    damage += RandomHelper.GetRandomNumber(spell.Properties[SpellProperty.DiceCount], spell.Properties[SpellProperty.DiceMaxValue]);
                }
                if (spell.Properties.ContainsKey(SpellProperty.DirectDamageBonus))
                {
                    damage += spell.Properties[SpellProperty.DirectDamageBonus];
                }
                return damage;
            }
            else return 0;
        }

        private int GetAOEDamage(Monster caster, MonsterSpell spell)
        {
            int damage = 0;
            if (spell.Properties != null && spell.Properties.ContainsKey(SpellProperty.AreaOfEffectSpell))
            {
                if (spell.Properties.ContainsKey(SpellProperty.AOEDiceCount))
                {
                    damage += RandomHelper.GetRandomNumber(spell.Properties[SpellProperty.AOEDiceCount], spell.Properties[SpellProperty.AOEDiceMaxValue]);
                }
                if (spell.Properties.ContainsKey(SpellProperty.AOEDamageBonus))
                {
                    damage += spell.Properties[SpellProperty.AOEDamageBonus];
                }
                return damage;
            }
            else return 0;
        }
    }
}