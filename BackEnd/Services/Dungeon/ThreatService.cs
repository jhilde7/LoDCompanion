using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Models;
using System.Threading.Tasks;
using LoDCompanion.BackEnd.Services.Combat;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    /// <summary>
    /// Represents the result of a triggered threat event.
    /// </summary>
    public class ThreatEventResult
    {
        public string Description { get; set; } = "Nothing happens.";
        public bool SpawnWanderingMonster { get; set; } = false;
        public bool SpawnTrap { get; set; } = false;
        public bool ShouldAddExplorationCards { get; internal set; }
        public Room? SpawnRandomEncounter { get; internal set; }
        public bool ThreatEventTriggered => SpawnWanderingMonster || SpawnTrap || ShouldAddExplorationCards || SpawnRandomEncounter != null;
    }

    public enum ThreatActionType
    {
        // Increases
        WinBattle,
        OpenDoorOrChest,
        ClearCobweb,
        ThreatRollExceeded,
        BashLock,
        BashLockWithCrowbar,

        // Decreases from Threat Events
        WanderingMonsterSpawned,
        ExtraExplorationCards,
        EncounterRiskIncreased,
        TrapSprung,
        ScenarioDieModified,
        Rest,
        PerfectRoll,

        // Decreases from In-Battle Events
        DisturbanceInTheVoid,
        GreenishTint,
        ForgedUnderPressure,
        EnemyHealing,
        EnemyFrenzy,
        HeroDisarmed,
        EnemyBecomesFearsome,
        Reinforcements,
        Onwards,
        Lever,
    }

    /// <summary>
    /// Manages the dungeon's threat level and related events.
    /// </summary>
    public class ThreatService
    {
        private readonly PowerActivationService _powerActivation;
        private readonly DungeonState _dungeon;
        private readonly EncounterService _encounter;

        public ThreatService(
            PowerActivationService powerActivationService, 
            DungeonState dungeon,
            EncounterService encounterService)
        {
            _powerActivation = powerActivationService;
            _dungeon = dungeon;
            _encounter = encounterService;
        }

        /// <summary>
        /// Increases the threat level in the dungeon state.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        /// <param name="amount">The amount to increase the threat by.</param>
        private void IncreaseThreat(int amount)
        {
            if(amount < 0)
            {
                amount = Math.Abs(amount);
                DecreaseThreat(amount);
            }
            _dungeon.ThreatLevel += amount;
            Console.WriteLine($"Threat increased by {amount}. New Threat Level: {_dungeon.ThreatLevel}");
        }

        /// <summary>
        /// Decreases the threat level in the dungeon state.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        /// <param name="amount">The amount to decrease the threat by.</param>
        private void DecreaseThreat(int amount)
        {
            var missingThreat = _dungeon.ThreatLevel - _dungeon.MinThreatLevel;
            amount = Math.Min(amount, missingThreat);
            _dungeon.ThreatLevel -= amount;
            Console.WriteLine($"Threat decreased by {amount}. New Threat Level: {_dungeon.ThreatLevel}");
        }

        /// <summary>
        /// Processes the scenario roll at the start of a turn and triggers a threat event if necessary.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        /// <param name="isInBattle">Whether the party is currently in combat.</param>
        /// <returns>A ThreatEventResult object if an event was triggered, otherwise null.</returns>
        public async Task<ThreatEventResult> ProcessScenarioRoll(bool isInBattle, Party? heroParty)
        {
            int scenarioRoll = RandomHelper.RollDie(DiceType.D10) + _dungeon.ScenarioRollModifier;
            Console.WriteLine($"Scenario Roll: {scenarioRoll}");

            if (heroParty != null && scenarioRoll >= 9)
            {
                var requestResult = await heroParty.Heroes[0].AskForPartyPerkAsync(_powerActivation, PerkName.FateForger);
                if (requestResult.Item1)
                {
                    scenarioRoll = RandomHelper.RollDie(DiceType.D10) + _dungeon.ScenarioRollModifier;
                }
            }

            // A roll of 9 or 10 triggers a Threat Level roll.
            if (scenarioRoll < 9)
            {
                return new ThreatEventResult(); // Nothing happens on a 1-8.
            }

            // Perform the Threat Level roll (d20).
            int threatRoll = RandomHelper.RollDie(DiceType.D20);
            Console.WriteLine($"Threat Roll: {threatRoll} (Current Threat: {_dungeon.ThreatLevel})");

            if (threatRoll == 20)
            {
                UpdateThreatLevelByThreatActionType(ThreatActionType.PerfectRoll);
                return new ThreatEventResult { Description = "A moment of calm. Threat decreases by 5." };
            }

            if (threatRoll <= _dungeon.ThreatLevel)
            {
                // A threat event is triggered!
                return await ResolveThreatEventAsync(isInBattle);
            }

            return new ThreatEventResult { Description = "Nothing happens" };
        }

        /// <summary>
        /// Resolves a triggered threat event based on whether the party is in combat.
        /// </summary>
        private async Task<ThreatEventResult> ResolveThreatEventAsync(bool isInBattle)
        {
            ThreatEventResult result;
            if (isInBattle)
            {
                result = await ResolveInBattleEvent();
            }
            else
            {
                result = ResolveOutOfBattleEvent();
            }

            return result;
        }

        /// <summary>
        /// Handles the "If the party is not in battle" event table from the rulebook.
        /// </summary>
        private ThreatEventResult ResolveOutOfBattleEvent()
        {
            int roll = RandomHelper.RollDie(DiceType.D20);
            var result = new ThreatEventResult();

            switch (roll)
            {
                case <= 12:
                    result.Description = "A Wandering Monster has appeared!";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.WanderingMonsterSpawned);
                    result.SpawnWanderingMonster = true;
                    break;
                case <= 15:
                    result.Description = "The dungeon shifts... Add one extra Exploration Card on top of each pile.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.ExtraExplorationCards);
                    result.ShouldAddExplorationCards = true;
                    break;
                case <= 17:
                    result.Description = "The air grows heavy. The risk of encounters has gone up by 10% for the rest of the quest.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.EncounterRiskIncreased);
                    _dungeon.EncounterChanceModifier += 10;
                    break;
                case <= 19:
                    result.Description = "A hero has sprung a trap!";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.TrapSprung);
                    result.SpawnTrap = true;
                    break;
                case 20:
                    result.Description = "A strange energy fills the dungeon. Add +1 on all Scenario die rolls for the remainder of the dungeon.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.ScenarioDieModified);
                    _dungeon.ScenarioRollModifier += 1;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Handles the "If the party is in battle" event table from the rulebook.
        /// </summary>
        private async Task<ThreatEventResult> ResolveInBattleEvent()
        {
            int roll = RandomHelper.RollDie(DiceType.D10);
            var result = new ThreatEventResult();
            var monsters = _dungeon.RevealedMonsters.ToList();
            var heroes = _dungeon.HeroParty?.Heroes.ToList();

            switch (roll)
            {
                case 1:
                    result.Description = "A disturbance in the Void! Spell Casters may do nothing during the coming turn.";
                    var spellCasters = _dungeon.HeroParty?.Heroes.Where(h => h.CanCastSpell = true);
                    foreach (var caster in spellCasters ?? Enumerable.Empty<Hero>())
                    {
                        await StatusEffectService.AttemptToApplyStatusAsync(caster, 
                            new ActiveStatusEffect(StatusEffectType.Incapacitated, 1), _powerActivation);
                    }
                    UpdateThreatLevelByThreatActionType(ThreatActionType.DisturbanceInTheVoid);
                    break;
                case 2:
                    monsters.Shuffle();
                    var monsterToModify = monsters.FirstOrDefault(m => m.ActiveWeapon != null);
                    monsterToModify?.ActiveWeapon?.Properties.Add(WeaponProperty.Poisoned, 1);
                    result.Description = $"A greenish tint appears on an enemies' weapons! {monsterToModify?.Name} gains the Poisonous.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.GreenishTint);
                    break;
                case 3:
                    monsters.Shuffle();
                    await StatusEffectService.AttemptToApplyStatusAsync(monsters[0], 
                        new ActiveStatusEffect(StatusEffectType.ForgedUnderPressure, -1, skillBonus: (Skill.CombatSkill, 15)), _powerActivation);
                    result.Description = $"Forged under pressure! {monsters[0].Name} gains +15 CS until dead.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.ForgedUnderPressure);
                    break;
                case int n when n >= 4 && n <= 5:
                    var WoundedMonsters = monsters.Where(m => m.CurrentHP < m.GetStat(BasicStat.HitPoints)).OrderBy(m => m.CurrentHP < m.GetStat(BasicStat.HitPoints));
                    int amountToHeal = RandomHelper.RollDie(DiceType.D10);
                    if (WoundedMonsters.Any()) WoundedMonsters.First().Heal(amountToHeal);
                    result.Description = $"Divine intervention? {WoundedMonsters.First()} heals {amountToHeal} Hit Points.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.EnemyHealing);
                    break;
                case 6:
                    monsters.Shuffle();
                    await StatusEffectService.AttemptToApplyStatusAsync(monsters[0], 
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1), _powerActivation);
                    result.Description = $"Frenzy! {monsters[0].Name} gains Frenzy until dead.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.EnemyFrenzy);
                    break;
                case 7:
                    heroes?.Shuffle();
                    var disarmedHero = heroes?[0];
                    if (disarmedHero != null && disarmedHero.Inventory.EquippedWeapon != null)
                    {
                        disarmedHero.DroppedWeapon = disarmedHero.Inventory.EquippedWeapon;
                        disarmedHero.Inventory.EquippedWeapon = null;
                    }
                    result.Description = $"Disarmed! A {disarmedHero?.Name} drops their weapon and must spend an action to retrieve it.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.HeroDisarmed);
                    break;
                case 8:
                    monsters.Shuffle();
                    monsters[0].PassiveSpecials.Add(Game.MonsterSpecialName.CauseFear, 10);
                    result.Description = $"Fearsome! {monsters[0].Name} gains the Fear passive.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.EnemyBecomesFearsome);
                    break;
                case 9:
                    var doors = _dungeon.CurrentRoom?.Doors;
                    doors?.Shuffle();
                    var roomForEncounter = doors?[0].ConnectedRooms?.FirstOrDefault(r => r != _dungeon.CurrentRoom);
                    result.SpawnRandomEncounter = roomForEncounter;
                    result.Description = "Reinforcements! A new encounter appears at a random door.";
                    UpdateThreatLevelByThreatActionType(ThreatActionType.Reinforcements);
                    break;
                case 10:
                    result.Description = "Onwards! All enemies gain +10 CS until the end of the battle.";
                    foreach (var monster in monsters)
                    {
                        await StatusEffectService.AttemptToApplyStatusAsync(monster, 
                            new ActiveStatusEffect(StatusEffectType.ForgedUnderPressure, -1, skillBonus: (Skill.CombatSkill, 10)), _powerActivation);
                    }
                    UpdateThreatLevelByThreatActionType(ThreatActionType.Onwards);
                    break;
            }
            return result;
        }

        public void UpdateThreatLevelByThreatActionType (ThreatActionType threatAction, int changeAmount = 0)
        {
            switch (threatAction)
            {
                case ThreatActionType.WinBattle:
                case ThreatActionType.OpenDoorOrChest:
                case ThreatActionType.ClearCobweb:
                case ThreatActionType.ThreatRollExceeded:
                case ThreatActionType.BashLockWithCrowbar:
                    changeAmount = 1;
                    break;
                case ThreatActionType.BashLock:
                    changeAmount = 2;
                    break;
                case ThreatActionType.ExtraExplorationCards:
                case ThreatActionType.WanderingMonsterSpawned:
                case ThreatActionType.Rest:
                case ThreatActionType.PerfectRoll:
                    changeAmount = -5;
                    break;
                case ThreatActionType.EncounterRiskIncreased:
                    changeAmount = -6;
                    break;
                case ThreatActionType.TrapSprung:
                    changeAmount = -7;
                    break;
                case ThreatActionType.ScenarioDieModified:
                    changeAmount = -10;
                    break;
                case ThreatActionType.DisturbanceInTheVoid:
                case ThreatActionType.GreenishTint:
                    changeAmount = -2;
                    break;
                case ThreatActionType.ForgedUnderPressure:
                case ThreatActionType.EnemyHealing:
                case ThreatActionType.EnemyFrenzy:
                case ThreatActionType.HeroDisarmed:
                    changeAmount = -3;
                    break;
                case ThreatActionType.EnemyBecomesFearsome:
                case ThreatActionType.Reinforcements:
                    changeAmount = -4;
                    break;
                case ThreatActionType.Onwards:
                    changeAmount = -6;
                    break;
                case ThreatActionType.Lever:
                    break;
            }

            IncreaseThreat(changeAmount);
        }
    }
}
