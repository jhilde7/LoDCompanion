// File: Services/Dungeon/QuestEncounterService.cs

using System.Collections.Generic;
using LoDCompanion.Models.Character; // For Monster
using LoDCompanion.Services.GameData;
using LoDCompanion.Utilities; // For RandomHelper

namespace LoDCompanion.Services.Dungeon
{
    public class QuestEncounterService
    {
        private readonly GameDataService _gameData;
        private readonly EncounterService _encounterService; // Dependency for generating supporting encounters

        public QuestEncounterService(GameDataService gameData, EncounterService enclunterService)
        {
            _gameData = gameData;
            _encounterService = enclunterService;
        }

        /// <summary>
        /// Generates the monsters and encounters for a specific quest.
        /// </summary>
        /// <param name="questMonsterTemplate">The template for the primary quest monster (e.g., a boss).</param>
        /// <param name="supportingMonsterTemplate">The template for supporting minion monsters.</param>
        /// <param name="monsterCountRange">An array [min, max] for the number of supporting minions.</param>
        /// <param name="supportingEncounterType">The type of supporting encounter to generate (e.g., "GoblinHorde").</param>
        /// <param name="numberOfSupportingEncounters">The number of times to generate the supporting encounter.</param>
        /// <returns>A list of all monsters generated for this quest encounter.</returns>
        public List<Monster> GenerateQuestEncounter(
            Monster questMonsterTemplate,
            Monster supportingMonsterTemplate,
            int[] monsterCountRange,
            string supportingEncounterType,
            int numberOfSupportingEncounters)
        {
            List<Monster> spawnedMonsters = new List<Monster>();

            // Spawn the main quest monster if provided
            if (questMonsterTemplate != null)
            {
                // Create a new instance of the quest monster template
                spawnedMonsters.Add(CopyMonster(questMonsterTemplate));
            }

            // Spawn supporting minions if provided
            if (supportingMonsterTemplate != null && monsterCountRange != null && monsterCountRange.Length == 2)
            {
                int minionCount = RandomHelper.GetRandomNumber(monsterCountRange[0], monsterCountRange[1]);
                for (int i = 0; i < minionCount; i++)
                {
                    spawnedMonsters.Add(CopyMonster(supportingMonsterTemplate));
                }
            }

            /*
            // Generate supporting encounters if a type is provided
            if (!string.IsNullOrEmpty(supportingEncounterType) && numberOfSupportingEncounters > 0)
            {
                for (int i = 0; i < numberOfSupportingEncounters; i++)
                {
                    // Assuming EncounterService has a method to get an encounter by type
                    // and that method returns a list of monsters
                    List<Monster> minionsFromEncounter = _encounterService.GetEncounter(supportingEncounterType);
                    if (minionsFromEncounter != null)
                    {
                        spawnedMonsters.AddRange(minionsFromEncounter);
                    }
                }
            }
            */
            return spawnedMonsters;
        }

        // Helper method to create a new instance from a template, similar to Unity's Instantiate
        private Monster CopyMonster(Monster original)
        {
            // This method should perform a deep copy if the Monster object contains complex reference types
            // For simplicity, this is a shallow copy for primitive types and lists of primitives.
            // You might need a more robust cloning mechanism (e.g., ICloneable, a dedicated copier service, or serialization)
            // if Monster has complex object references that need to be unique for each instance.
            return new Monster(_gameData)
            {                
                Name = original.Name,
                CurrentHP = original.CurrentHP,
                MaxHP = original.MaxHP,
                Strength = original.Strength,
                Constitution = original.Constitution,
                Dexterity = original.Dexterity,
                Wisdom = original.Wisdom,
                Resolve = original.Resolve,
                CombatSkill = original.CombatSkill,
                RangedSkill = original.RangedSkill,
                Dodge = original.Dodge,
                Level = original.Level,
                NaturalArmour = original.NaturalArmour,
                Type = original.Type,
                DamageBonus = original.DamageBonus,
                DamageArray = original.DamageArray != null ? (int[])original.DamageArray.Clone() : null,
                HasSpecialAttack = original.HasSpecialAttack,
                IsGhost = original.IsGhost,
                ToHit = original.ToHit,
                Move = original.Move,
                XP = original.XP,
                IsLarge = original.IsLarge,
                IsUndead = original.IsUndead,
                SpecialRules = original.SpecialRules != null ? new List<string>(original.SpecialRules) : null,
                Spells = original.Spells != null ? new List<string>(original.Spells) : null,
                Weapons = original.Weapons != null ? new List<MonsterWeapon>(original.Weapons) : null, // Consider deep copying Weapon objects if they are mutable
                };
        }
    }
}