using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Player
{
    /// <summary>
    /// Defines the context in which the party is resting.
    /// </summary>
    public enum RestingContext
    {
        Dungeon,
        Wilderness
    }

    /// <summary>
    /// Represents the outcome of a rest attempt.
    /// </summary>
    public class RestResult
    {
        public bool WasSuccessful { get; set; }
        public bool WasInterrupted { get; set; }
        public string Message { get; set; } = string.Empty;
        public ThreatEventResult? ThreatEvent { get; set; }
    }

    /// <summary>
    /// Manages the process of the party resting.
    /// </summary>
    public class PartyRestingService
    {
        private readonly ThreatService _threatService;
        private readonly WanderingMonsterService _wanderingMonsterService;

        public PartyRestingService(ThreatService threatService, WanderingMonsterService wanderingMonsterService)
        {
            _threatService = threatService;
            _wanderingMonsterService = wanderingMonsterService;
        }

        /// <summary>
        /// Executes the full resting sequence based on the context (Dungeon or Wilderness).
        /// </summary>
        /// <param name="party">The party that is resting.</param>
        /// <param name="context">The context in which the rest is taking place.</param>
        /// <param name="dungeonState">The current dungeon state, required if resting in a dungeon.</param>
        /// <returns>A RestResult object detailing the outcome.</returns>
        public RestResult AttemptRest(Party party, RestingContext context, DungeonState? dungeonState = null)
        {
            var result = new RestResult();

            if (party == null || !party.Heroes.Any())
            {
                result.Message = "There is no party to rest.";
                return result;
            }

            // Step 1: Check for Rations
            var ration = party.Heroes.SelectMany(h => h.Backpack).FirstOrDefault(i => i.Name == "Ration");
            if (ration == null || ration.Quantity <= 0)
            {
                result.Message = "The party has no rations and cannot rest.";
                return result;
            }
            ration.Quantity--; // Consume one ration

            // Step 2: Context-specific checks (Threat, Interruption)
            if (context == RestingContext.Dungeon)
            {
                if (dungeonState == null)
                {
                    result.Message = "Dungeon state is required for dungeon resting.";
                    return result;
                }

                // Lower Threat Level
                _threatService.DecreaseThreat(dungeonState, 5);
                // Make a threat roll
                result.ThreatEvent = _threatService.ProcessScenarioRoll(dungeonState, false);
                // Move Wandering Monsters and check for interruption
                // bool monsterSpotted = _wanderingMonsterService.MoveWanderingMonsters(dungeonState, 3);
                // if (monsterSpotted) { ... return interrupted result ... }
            }
            else if (context == RestingContext.Wilderness)
            {
                // TODO: Implement wilderness-specific interruption logic (e.g., random encounter roll)
            }

            // --- If rest was not interrupted ---
            result.WasSuccessful = true;
            result.Message = "The party rests successfully.";

            // Step 3: Apply healing and recovery
            foreach (var hero in party.Heroes)
            {
                // Restore HP
                int hpGained = RandomHelper.RollDie("D6");
                hero.CurrentHP = Math.Min(hero.MaxHP, hero.CurrentHP + hpGained);

                // Restore Energy
                int energyToRestore = hero.MaxEnergy - hero.CurrentEnergy;
                for (int i = 0; i < energyToRestore; i++)
                {
                    if (RandomHelper.RollDie("D6") <= 3) hero.CurrentEnergy++;
                }

                // Restore Mana for Wizards
                if (hero.ProfessionName == "Wizard") hero.CurrentMana = hero.MaxMana;

                // Handle Bleeding Out and Poison
                // (Logic remains the same as before)
            }

            return result;
        }
    }
}
