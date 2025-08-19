using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System;
using System.Threading.Tasks;

namespace LoDCompanion.BackEnd.Services.Player
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
        private readonly ThreatService _threat;
        private readonly DungeonManagerService _dungeonManager;
        private readonly UserRequestService _userRequest;
        private readonly PowerActivationService _powerActivation;

        public PartyRestingService(
            ThreatService threatService, 
            DungeonManagerService dungeonManagerService, 
            UserRequestService userRequestService, 
            PowerActivationService powerActivationService)
        {
            _threat = threatService;
            _dungeonManager = dungeonManagerService;
            _userRequest = userRequestService;
            _powerActivation = powerActivationService;
        }

        /// <summary>
        /// Executes the full resting sequence based on the context (Dungeon or Wilderness).
        /// </summary>
        /// <param name="party">The party that is resting.</param>
        /// <param name="context">The context in which the rest is taking place.</param>
        /// <param name="dungeonState">The current dungeon state, required if resting in a dungeon.</param>
        /// <returns>A RestResult object detailing the outcome.</returns>
        public async Task<RestResult> AttemptRest(RestingContext context, DungeonState? dungeon = null)
        {
            var result = new RestResult();
            var party = _dungeonManager.HeroParty;

            if (party == null || !party.Heroes.Any())
            {
                result.Message = "There is no party to rest.";
                return result;
            }

            // check party perks to determine if a ration is needed
            var requestResult = await party.Heroes[0].AskForPartyPerkAsync(_powerActivation, PerkName.LivingOnNothing);
            if (!requestResult.Item1)
            {
                // Check for Rations
                var ration = party.Heroes.SelectMany(h => h.Inventory.Backpack).First(i => i != null && i.Name == "Ration");
                if (ration == null || ration.Quantity <= 0)
                {
                    result.Message = "The party has no rations and cannot rest.";
                    return result;
                }
                ration.Quantity--; // Consume one ration 
            }

            if (context == RestingContext.Dungeon && dungeon != null)
            {
                _threat.UpdateThreatLevelByThreatActionType(ThreatActionType.Rest);
                var threatResult = await _dungeonManager.HandleScenarioRoll(isInBattle: false);
                result.WasInterrupted = threatResult.ThreatEventTriggered;

                if (!result.WasInterrupted)
                {
                    result.WasSuccessful = true;
                    _dungeonManager.PartyManager.UpdateMorale(changeEvent: MoraleChangeEvent.Rest);
                    result.Message = "The party rests successfully.";
                }
            }
            else if (context == RestingContext.Wilderness)
            {
                // TODO: Implement wilderness-specific interruption logic (e.g., random encounter roll)
            }

            if (!result.WasInterrupted)
            {
                // Apply healing and recovery
                foreach (var hero in party.Heroes)
                {
                    // Restore HP
                    int hpGained = RandomHelper.RollDie(DiceType.D6);
                    hero.CurrentHP = Math.Min(hero.GetStat(BasicStat.HitPoints), hero.CurrentHP + hpGained);

                    // Restore Energy
                    int energyToRestore = hero.GetStat(BasicStat.Energy) - hero.CurrentEnergy;
                    if (requestResult.Item2 == hero)
                    {
                        energyToRestore = Math.Max(0, energyToRestore - 1);
                    }

                    for (int i = 0; i < energyToRestore; i++)
                    {
                        if (RandomHelper.RollDie(DiceType.D6) <= 3) hero.CurrentEnergy++;
                    }

                    // Restore Mana for Wizards
                    if (hero.ProfessionName == "Wizard") hero.CurrentMana = hero.GetStat(BasicStat.Mana);

                    // Handle Bleeding Out and Poison
                } 
            }

            return result;
        }
    }
}
