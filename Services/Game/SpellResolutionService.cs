using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;
using LoDCompanion.Utilities;
using Microsoft.Extensions.Options;
using System.Text;
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

            // Handle Touch Spells first, as they require a to-hit roll
            if (spell.HasProperty(SpellProperty.Touch))
            {
                if (singleTarget != null)
                {
                    int touchAttackRoll = await _diceRoll.RequestRollAsync("Roll to touch target", "1d100");
                    if (touchAttackRoll > caster.GetSkill(Skill.CombatSkill) + 20)
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

            // Handle Area of Effect spells
            if (spell.HasProperty(SpellProperty.AreaOfEffectSpell))
            {
                int? radius = spell.Properties?.GetValueOrDefault(SpellProperty.Radius, 0);
                if (radius != null)
                {
                    var center = centerPosition ?? singleTarget?.Position;
                    List<GridPosition> affectedSquares = new List<GridPosition>();
                    if (singleTarget != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(singleTarget.Position, (int)radius, _dungeon.DungeonGrid);
                    }
                    else if (centerPosition != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(centerPosition, (int)radius, _dungeon.DungeonGrid);
                    }
                    List<Character> allCharacters = _dungeon.AllCharactersInDungeon;

                    foreach (var character in allCharacters.Where(c => affectedSquares.Contains(c.Position)))
                    {
                        bool isCenterTarget = character.Position.Equals(center);
                        int damage = isCenterTarget ?
                            await GetDirectDamageAsync(caster, spell) :
                            await GetAOEDamageAsync(caster, spell);

                        damage += options.PowerLevels;

                        character.TakeDamage(damage, spell.DamageType);
                        outcome.AppendLine($"{character.Name} is hit by {spell.Name} for {damage} {spell.DamageType} damage!");
                    }
                }
            }
            else if (singleTarget != null) // Single target damage
            {
                int damage = await GetDirectDamageAsync(caster, spell) + options.PowerLevels;
                singleTarget.TakeDamage(damage, spell.DamageType);
                outcome.AppendLine($"{spell.Name} hits {singleTarget.Name} for {damage} {spell.DamageType} damage!");
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
                    if (target is DoorChest door && door.Properties != null)
                    {
                        // Logic to unlock a door
                        if(door.Properties.ContainsKey(DoorChestProperty.Locked))door.Properties.Remove(DoorChestProperty.Locked);
                        result.OutcomeMessage = $"{caster.Name} magically unlocks the {door.Category}!";
                    }
                    else
                    {
                        result.OutcomeMessage = "Open Lock can only target a door or chest.";
                    }
                    return result;

                case "Seal Door":
                    if (target is DoorChest doorToSeal)
                    {
                        if (doorToSeal.Properties != null)
                        {
                            doorToSeal.Properties.TryAdd(DoorChestProperty.MagicallySealed, await GetDurationAsync(caster, spell));
                        }
                        else
                        { 
                            doorToSeal.Properties = new Dictionary<DoorChestProperty, int> { { DoorChestProperty.MagicallySealed, await GetDurationAsync(caster, spell) } }; 
                        }
                        result.OutcomeMessage = $"{caster.Name} magically seals a nearby door!";
                    }
                    else
                    {
                        result.OutcomeMessage = "Seal Door can only target a door.";
                    }
                    return result;

                case "Transpose":
                    if (target is Hero otherHero)
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
                    foreach (Hero hero in _combatManager.GetActivatedHeroes())
                    {
                        if (hero.CurrentAP <= 0)
                        {
                            hero.ResetActionPoints();
                            _initiative.AddToken(ActorType.Hero);
                        }
                    }
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
                    StatusEffectService.AttemptToApplyStatus(charater, effectToApply);
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
        public SpellCastResult ResolveMonsterSpell(Monster caster, MonsterSpell spell, object target)
        {
            var result = new SpellCastResult { IsSuccess = true };
            var outcome = new StringBuilder($"{caster.Name} casts {spell.Name}!");

            // Determine Targets
            var (centerPosition, singleTarget) = GetSpellTargetingInfo(target);
            if (centerPosition == null)
            {
                return new SpellCastResult { OutcomeMessage = "Invalid target for spell." };
            }
            var affectedCharacters = GetCharactersInArea(spell.TargetType, centerPosition, spell.Properties?.GetValueOrDefault(SpellProperty.Radius) ?? 0);

            // Spend AP Cost
            caster.SpendActionPoints(spell.CostAP);

            if (spell.TargetType == SpellTargetType.Ally && singleTarget != null && singleTarget is Monster allyTarget)
            {
                return HandleHealingSpell(caster, spell, allyTarget);
            }
            else if(spell.TargetType == SpellTargetType.SingleTarget || spell.TargetType == SpellTargetType.AreaOfEffect)
            {
                return HandleDamageSpell(caster, spell, target);
            }

            if (spell.StatusEffect != null && singleTarget != null && spell.StatusEffect != StatusEffectType.RaiseDead)
            {
                result.OutcomeMessage = HandleStatusEffectingSpell(caster, spell, singleTarget);
                return result;
            }

            // Handle Special Non-Targeted Effects (Summoning, Auras, etc.)
            if (spell.TargetType == SpellTargetType.NoTarget)
            {
                switch (spell.Name)
                {
                    case "Raise dead":
                        //TODO: Implement logic to find a fallen undead ally this may be done by evaluating corpses.
                        var fallenUndead = _dungeon.RevealedMonsters.FirstOrDefault(m => m.IsUndead && m.CurrentHP <= 0);
                        if (fallenUndead != null)
                        {
                            fallenUndead.CurrentHP = fallenUndead.GetStat(BasicStat.HitPoints);
                            outcome.AppendLine(HandleStatusEffectingSpell(caster, spell, fallenUndead));
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
                                woundedUndead.CurrentHP = Math.Min(woundedUndead.GetStat(BasicStat.HitPoints), woundedUndead.CurrentHP + healing);
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

        private string HandleStatusEffectingSpell(Monster caster, MonsterSpell spell, Character target)
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

                StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect((StatusEffectType)spell.StatusEffect, duration));
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

        private SpellCastResult HandleDamageSpell(Monster caster, MonsterSpell spell, object target)
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
                    if (singleTarget != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(singleTarget.Position, (int)radius, _dungeon.DungeonGrid);
                    }
                    else if (centerPosition != null)
                    {
                        affectedSquares = GridService.GetAllSquaresInRadius(centerPosition, (int)radius, _dungeon.DungeonGrid);
                    }
                    List<Character> allCharacters = _dungeon.AllCharactersInDungeon;

                    foreach (var character in allCharacters.Where(c => affectedSquares.Contains(c.Position)))
                    {
                        bool isCenterTarget = character.Position.Equals(center);
                        int damage = isCenterTarget ?
                            GetDirectDamage(caster, spell) :
                            GetAOEDamage(caster, spell);

                        character.TakeDamage(damage, spell.DamageType != null ? spell.DamageType : null);
                        outcome.AppendLine($"{character.Name} is hit by {spell.Name} for {damage} {spell.DamageType} damage!");
                    }
                }
            }
            else if (singleTarget != null) // Single target damage
            {
                int damage = GetDirectDamage(caster, spell);
                singleTarget.TakeDamage(damage, spell.DamageType != null ? spell.DamageType : null);
                outcome.AppendLine($"{spell.Name} hits {singleTarget.Name} for {damage} {spell.DamageType} damage!");
            }

            if (spell.StatusEffect != null && singleTarget != null)
            {
                result.OutcomeMessage = HandleStatusEffectingSpell(caster, spell, singleTarget);
            }

            return result;
        }

        private SpellCastResult HandleHealingSpell(Monster caster, MonsterSpell spell, Monster target)
        {
            int healingAmount = GetHealing(caster, spell);

            target.CurrentHP = Math.Min(target.GetStat(BasicStat.HitPoints), target.CurrentHP + healingAmount);

            return new SpellCastResult
            {
                IsSuccess = true,
                OutcomeMessage = $"{target.Name} is healed for {healingAmount} HP by {spell.Name}."
            };
        }

        private GridSquare GetRandomPlacement(Character caster)
        {
            var roomGrid = caster.Room.Grid;
            roomGrid.Shuffle();

            // Find a random empty square to place the monster
            int i = 0;
            while (roomGrid[i].IsOccupied || roomGrid[i].IsWall)
            {
                i++;
            }
            var placementPosition = roomGrid[i];

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
                var characterAtCenter = _dungeon.AllCharactersInDungeon.FirstOrDefault(c => c.Position.Equals(positionTarget));
                return (positionTarget, characterAtCenter);
            }
            return (null, null);
        }

        /// <summary>
        /// Gets all characters within the area of effect of a spell.
        /// </summary>
        private List<Character> GetCharactersInArea(SpellTargetType targetType, GridPosition center, int radius)
        {
            var allCharacters = _dungeon.AllCharactersInDungeon; // Assumes a helper in DungeonState

            if (targetType == SpellTargetType.SingleTarget)
            {
                return allCharacters.Where(c => c.Position.Equals(center)).ToList();
            }

            if (targetType == SpellTargetType.AreaOfEffect)
            {
                var affectedSquares = GridService.GetAllSquaresInRadius(center, radius, _dungeon.DungeonGrid);
                return allCharacters.Where(c => c.OccupiedSquares.Any(os => affectedSquares.Contains(os))).ToList();
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
                if(spell.Properties.ContainsKey(SpellProperty.DirectDamageBonus))
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