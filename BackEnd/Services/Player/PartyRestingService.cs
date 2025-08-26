using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using LoDCompanion.BackEnd.Services.Combat;
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
        Wilderness,
        Dessert
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
        private readonly PowerActivationService _powerActivation;
        private readonly PartyManagerService _partyManager;
        private readonly UserRequestService _userRequest;

        public event Func<PartyManagerService, Task<RestResult>>? OnDungeonRestAsync;
        public event Action? OnBrewPotion;

        public PartyRestingService(
            PowerActivationService powerActivationService,
            PartyManagerService partyManager,
            UserRequestService userRequestService)
        {
            _powerActivation = powerActivationService;
            _partyManager = partyManager;
            _userRequest = userRequestService;
        }

        /// <summary>
        /// Executes the full resting sequence based on the context (Dungeon or Wilderness).
        /// </summary>
        /// <param name="party">The party that is resting.</param>
        /// <param name="context">The context in which the rest is taking place.</param>
        /// <param name="dungeonState">The current dungeon state, required if resting in a dungeon.</param>
        /// <returns>A RestResult object detailing the outcome.</returns>
        public async Task<RestResult> AttemptRest(RestingContext context)
        {
            var result = new RestResult();
            var party = _partyManager.Party;
            var rationUsed = false;

            if (party == null || !party.Heroes.Any())
            {
                result.Message = "There is no party to rest.";
                return result;
            }

            // check party perks to determine if a ration is needed
            if (context == RestingContext.Wilderness)
            {
                var rollResult = await _userRequest.RequestRollAsync("Roll for foraging skill check", "1d100");
                var roll = rollResult.Roll;
                var heroWithHighestSkill = party.Heroes
                    .OrderBy(h => h.GetSkill(Skill.Foraging))
                    .First();
                if (heroWithHighestSkill != null && roll <= heroWithHighestSkill.GetSkill(Skill.Foraging))
                {
                    rationUsed = true;
                }
            }
            (bool, Hero?) requestResult = (false, null);

            if (!rationUsed)
            {
                requestResult = await party.Heroes[0].AskForPartyPerkAsync(_powerActivation, PerkName.LivingOnNothing);
                if (!requestResult.Item1)
                {
                    // Check for Rations
                    var ration = party.Heroes.SelectMany(h => h.Inventory.Backpack).First(i => i != null && i.Name == "Ration");
                    if (context == RestingContext.Dungeon && ration == null)
                    {
                        result.Message = "The party has no rations and cannot rest.";
                        return result;
                    }
                    if (ration != null)
                    {
                        if (context == RestingContext.Dessert) 
                        {
                            if (ration.Quantity < 2)
                            {
                                rationUsed = false; 
                            }
                            else
                            {
                                ration.Quantity -= 2;
                                rationUsed = true;
                            }
                        }
                        else
                        {
                            ration.Quantity--;
                            rationUsed = true;
                        }
                    }
                }
            }

            if (rationUsed && context == RestingContext.Dungeon && OnDungeonRestAsync != null)
            {
                result = await OnDungeonRestAsync.Invoke(_partyManager);                
            }
            else
            {
                if (!rationUsed)
                {
                    foreach (var hero in party.Heroes)
                    {
                        var constitution = hero.GetStat(BasicStat.Constitution);
                        var negativeBonus = (int)Math.Floor(constitution / 2d);
                        await StatusEffectService.AttemptToApplyStatusAsync(
                            hero, 
                            new ActiveStatusEffect(StatusEffectType.Hungry, -1, statBonus: (BasicStat.Constitution, -negativeBonus)), 
                            _powerActivation);
                    }
                    _partyManager.UpdateMorale(-4);
                }
                else
                {
                    foreach (var hero in party.Heroes)
                    {
                        var hungry = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.Hungry);
                        if (hungry != null)
                        {
                            StatusEffectService.RemoveActiveStatusEffect(hero, hungry);

                        }
                    }
                }
            }

            if (result.WasSuccessful)
            {
                // Apply healing and recovery
                foreach (var hero in party.Heroes)
                {
                    // Handle Bleeding Out and Poison
                    var bleedingOut = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.BleedingOut);
                    var poison = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == StatusEffectType.Poisoned);

                    if (bleedingOut != null)
                    {
                        var rollRequest = await _userRequest.RequestRollAsync($"{hero.Name}, roll constitution test.", "1d100");
                        if (hero.TestConstitution(rollRequest.Roll, 10))
                        {
                            hero.Heal(RandomHelper.RollDie(DiceType.D4));
                        }
                        else
                        {
                            result.Message += $"{hero.Name} bleeds to death.";
                        }
                        StatusEffectService.RemoveActiveStatusEffect(hero, bleedingOut);
                    }

                    if (poison != null)
                    {
                        for (int i = 0; i < poison.Duration; i++)
                        {
                            await StatusEffectService.ProcessActiveStatusEffectsAsync(hero, _powerActivation);
                        }

                        result.Message += $"{hero.Name} recovers from their poison effect.";
                    }

                    // Restore HP
                    int hpGained = RandomHelper.RollDie(DiceType.D6);
                    if (party.Heroes.Where(h => h.Inventory.Backpack.FirstOrDefault(i => i != null && i.Name == "Cooking Gear") != null).Any() && rationUsed) hpGained += 3;
                    hero.Heal(hpGained);

                    // Restore Energy
                    if (hero.Inventory.Backpack.FirstOrDefault(i => i != null && i.Name == "Bed Roll") != null) hero.CurrentEnergy = hero.GetStat(BasicStat.Energy);
                    int energyToRestore = hero.GetStat(BasicStat.Energy) - hero.CurrentEnergy;
                    if (requestResult.Item2 == hero)
                    {
                        energyToRestore = Math.Max(0, energyToRestore - 1);
                    }

                    for (int i = 0; i < energyToRestore; i++)
                    {
                        if (RandomHelper.RollDie(DiceType.D100) <= 50) hero.CurrentEnergy++;
                    }

                    // Restore Mana for Wizards
                    if (hero.ProfessionName == "Wizard") hero.CurrentMana = hero.GetStat(BasicStat.Mana);

                    //Brew potions
                    if (hero.Inventory.CanBrewPotion)
                    {
                        if (OnBrewPotion != null && await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to brew a potion?"))
                        {
                            //TODO tie in UI to handle brewing potions
                            OnBrewPotion.Invoke();
                        }
                    }

                    // Use equipment
                    var armourRepairKit = hero.Inventory.Backpack.FirstOrDefault(i => i != null && i.Name == "Armour Repair Kit");
                    var whetstone = hero.Inventory.Backpack.FirstOrDefault(i => i != null && i.Name == "Whetstone");
                    if (armourRepairKit != null)
                    {
                        if(await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to use their {armourRepairKit.Name}?"))
                        {
                            await Task.Yield();
                            foreach (var armour in hero.Inventory.EquippedArmour)
                            {
                                var repairAmount = RandomHelper.RollDie(DiceType.D3);
                                var amountRepaired = armour.Repair(repairAmount);
                                result.Message += $"{hero.Name} repairs their {armour.Name} for {amountRepaired} durability";
                            }
                        }
                    }
                    if (whetstone != null && hero.Inventory.EquippedWeapon is MeleeWeapon meleeWeapon)
                    {
                        if (await _userRequest.RequestYesNoChoiceAsync($"Does {hero.Name} wish to use their {whetstone.Name}?"))
                        {
                            await Task.Yield();
                            var repairAmount = RandomHelper.RollDie(DiceType.D3);
                            var amountRepaired = meleeWeapon.Repair(repairAmount);
                            whetstone.TakeDamage(1);
                            result.Message += $"{hero.Name} repairs their {meleeWeapon.Name} for {amountRepaired} durability";
                        }
                    }
                }
            }

            return result;
        }
    }
}
