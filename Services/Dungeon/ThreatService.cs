﻿using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;
using System;

namespace LoDCompanion.Services.Dungeon
{
    /// <summary>
    /// Represents the result of a triggered threat event.
    /// </summary>
    public class ThreatEventResult
    {
        public string Description { get; set; } = "Nothing happens.";
        public int ThreatDecrease { get; set; } = 0;
        public bool SpawnWanderingMonster { get; set; } = false;
        public bool SpawnTrap { get; set; } = false;
        // Add other properties here for more complex events, e.g., reinforcements
    }

    /// <summary>
    /// Manages the dungeon's threat level and related events.
    /// </summary>
    public class ThreatService
    {
        public ThreatService()
        {
            // Constructor can be used for dependency injection if needed later.
        }

        /// <summary>
        /// Increases the threat level in the dungeon state.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        /// <param name="amount">The amount to increase the threat by.</param>
        public void IncreaseThreat(DungeonState dungeonState, int amount)
        {
            dungeonState.ThreatLevel += amount;
            Console.WriteLine($"Threat increased by {amount}. New Threat Level: {dungeonState.ThreatLevel}");
        }

        /// <summary>
        /// Decreases the threat level in the dungeon state.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        /// <param name="amount">The amount to decrease the threat by.</param>
        public void DecreaseThreat(DungeonState dungeonState, int amount)
        {
            dungeonState.ThreatLevel -= amount;
            if (dungeonState.ThreatLevel < dungeonState.MinThreatLevel)
            {
                dungeonState.ThreatLevel = dungeonState.MinThreatLevel;
            }
            Console.WriteLine($"Threat decreased by {amount}. New Threat Level: {dungeonState.ThreatLevel}");
        }

        /// <summary>
        /// Processes the scenario roll at the start of a turn and triggers a threat event if necessary.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        /// <param name="isInBattle">Whether the party is currently in combat.</param>
        /// <returns>A ThreatEventResult object if an event was triggered, otherwise null.</returns>
        public ThreatEventResult? ProcessScenarioRoll(DungeonState dungeonState, bool isInBattle)
        {
            // As per the PDF, roll a d10 Scenario Die.
            int scenarioRoll = RandomHelper.RollDie(DiceType.D10);
            Console.WriteLine($"Scenario Roll: {scenarioRoll}");

            // A roll of 9 or 10 triggers a Threat Level roll.
            if (scenarioRoll < 9)
            {
                return null; // Nothing happens on a 1-8.
            }

            // Perform the Threat Level roll (d20).
            int threatRoll = RandomHelper.RollDie(DiceType.D20);
            Console.WriteLine($"Threat Roll: {threatRoll} (Current Threat: {dungeonState.ThreatLevel})");

            if (threatRoll == 20)
            {
                DecreaseThreat(dungeonState, 5);
                return new ThreatEventResult { Description = "A moment of calm. Threat decreases by 5." };
            }

            if (threatRoll <= dungeonState.ThreatLevel)
            {
                // A threat event is triggered!
                return ResolveThreatEvent(dungeonState, isInBattle);
            }
            else
            {
                // The roll was above the threat level, so the threat increases.
                IncreaseThreat(dungeonState, 1);
                return new ThreatEventResult { Description = "The heroes feel a growing sense of dread... (Threat increased by 1)" };
            }
        }

        /// <summary>
        /// Resolves a triggered threat event based on whether the party is in combat.
        /// </summary>
        private ThreatEventResult ResolveThreatEvent(DungeonState dungeonState, bool isInBattle)
        {
            ThreatEventResult result;
            if (isInBattle)
            {
                result = ResolveInBattleEvent();
            }
            else
            {
                result = ResolveOutOfBattleEvent();
            }

            // Decrease the threat level after the event is resolved.
            DecreaseThreat(dungeonState, result.ThreatDecrease);
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
                case int n when n >= 1 && n <= 12:
                    result.Description = "A Wandering Monster has appeared!";
                    result.ThreatDecrease = 5;
                    result.SpawnWanderingMonster = true;
                    break;
                case int n when n >= 13 && n <= 15:
                    result.Description = "The dungeon shifts... Add one extra Exploration Card on top of each pile.";
                    result.ThreatDecrease = 5;
                    // Note: The calling service (DungeonManagerService) will need to handle this logic.
                    break;
                case int n when n >= 16 && n <= 17:
                    result.Description = "The air grows heavy. The risk of encounters has gone up by 10% for the rest of the quest.";
                    result.ThreatDecrease = 6;
                    // Note: A property in DungeonState should track this modifier.
                    break;
                case int n when n >= 18 && n <= 19:
                    result.Description = "A hero has sprung a trap!";
                    result.ThreatDecrease = 7;
                    result.SpawnTrap = true;
                    break;
                case 20:
                    result.Description = "A strange energy fills the dungeon. Add +1 on all Scenario die rolls for the remainder of the dungeon.";
                    result.ThreatDecrease = 10;
                    // Note: A property in DungeonState should track this modifier.
                    break;
            }
            return result;
        }

        /// <summary>
        /// Handles the "If the party is in battle" event table from the rulebook.
        /// </summary>
        private ThreatEventResult ResolveInBattleEvent()
        {
            int roll = RandomHelper.RollDie(DiceType.D10);
            var result = new ThreatEventResult();

            switch (roll)
            {
                case 1:
                    result.Description = "A disturbance in the Void! Spell Casters may do nothing during the coming turn.";
                    result.ThreatDecrease = 2;
                    break;
                case 2:
                    result.Description = "A greenish tint appears on the enemies' weapons! They gain the Poisonous Special Rule.";
                    result.ThreatDecrease = 2;
                    break;
                case 3:
                    result.Description = "Forged under pressure! One enemy gains +15 CS until dead.";
                    result.ThreatDecrease = 3;
                    break;
                case int n when n >= 4 && n <= 5:
                    result.Description = "Divine intervention? One wounded enemy heals 1d10 Hit Points.";
                    result.ThreatDecrease = 3;
                    break;
                case 6:
                    result.Description = "Frenzy! One enemy gains the Frenzy Special Rule until dead.";
                    result.ThreatDecrease = 3;
                    break;
                case 7:
                    result.Description = "Disarmed! A random hero drops their weapon and must spend an action to retrieve it.";
                    result.ThreatDecrease = 3;
                    break;
                case 8:
                    result.Description = "Fearsome! One enemy gains the Fear Special Rule.";
                    result.ThreatDecrease = 4;
                    break;
                case 9:
                    result.Description = "Reinforcements! A new encounter appears at a random door.";
                    result.ThreatDecrease = 4;
                    // Note: This would trigger logic in DungeonManagerService to spawn more monsters.
                    break;
                case 10:
                    result.Description = "Onwards! All enemies gain +10 CS until the end of the battle.";
                    result.ThreatDecrease = 6;
                    break;
            }
            return result;
        }
    }
}
