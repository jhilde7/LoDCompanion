using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Text;

namespace LoDCompanion.BackEnd.Services.Player
{
    public class PotionActivationService
    {
        private PowerActivationService _powerActivation;
        private UserRequestService _diceRoll;
        public PotionActivationService(PowerActivationService powerActivation, UserRequestService diceRoll)
        {
            _powerActivation = powerActivation;
            _diceRoll = diceRoll;
        }

        public async Task<string> DrinkPotionAsync(Hero hero, Potion potion)
        {
            // Consume the potion
            BackpackHelper.TakeOneItem(hero.Inventory.Backpack, potion);

            // Apply the potion's effects
            if (potion.PotionProperties != null)
            {
                foreach (var property in potion.PotionProperties)
                {
                    switch (property.Key)
                    {
                        case PotionProperty.HealHP:
                            int healing = (await _diceRoll.RequestRollAsync("Roll for heal amount.", $"1d{property.Value}")).Roll;
                            await Task.Yield();
                            potion.PotionProperties.TryGetValue(PotionProperty.HealHPBonus, out int bonus);
                            hero.Heal(healing + bonus);
                            return $"{hero.Name} heals for {healing} HP.";
                        case PotionProperty.CureDisease:
                            if (RandomHelper.RollDie(DiceType.D100) <= property.Value)
                            {
                                hero.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.Diseased);
                                return $"{hero.Name} is cured of disease.";
                            }
                            return $"{hero.Name} is not cured of disease.";
                        case PotionProperty.CurePoison:
                            if (RandomHelper.RollDie(DiceType.D100) <= property.Value)
                            {
                                hero.ActiveStatusEffects.RemoveAll(e => e.Category == StatusEffectType.Poisoned);
                                return $"{hero.Name} is cured of poison.";
                            }
                            return $"{hero.Name} is not cured of poison.";
                        case PotionProperty.Energy:
                            hero.CurrentEnergy += property.Value;
                            return $"{hero.Name} gains {property.Value} energy.";
                        case PotionProperty.Mana:
                            var rollResult = await _diceRoll.RequestRollAsync("Roll for heal amount.", $"{property.Value / 20}d20");
                            var missingMana = hero.GetStat(BasicStat.Mana) - hero.CurrentMana ?? 0;
                            var amount = Math.Min(missingMana, rollResult.Roll);
                            hero.CurrentMana += Math.Min(missingMana, rollResult.Roll);
                            return $"{hero.Name} restores {amount} mana.";
                        case PotionProperty.Experience:
                            hero.GainExperience(property.Value);
                            return $"{hero.Name} gains {property.Value} experience.";
                    }
                }
            }

            if (potion.ActiveStatusEffect != null)
            {
                await StatusEffectService.AttemptToApplyStatusAsync(hero, potion.ActiveStatusEffect, _powerActivation);
                return $"{hero.Name} feels the effects of the {potion.Name}.";
            }

            return $"{hero.Name} uses {potion.Name}, but nothing happens.";
        }

        public async Task<string> BreakPotionAsync(Hero hero, Potion potion, GridPosition targetPosition, DungeonState? dungeon = null)
        {
            DamageType? damageType = null;
            var damageRoll = string.Empty;
            if (potion.PotionProperties != null)
            {
                if (potion.PotionProperties.ContainsKey(PotionProperty.FireDamage))
                {
                    damageType = DamageType.Fire;
                    damageRoll = $"1d{potion.PotionProperties[PotionProperty.FireDamage]}";
                }
                else if (potion.PotionProperties.ContainsKey(PotionProperty.AcidDamage))
                {
                    damageType = DamageType.Acid;
                    damageRoll = $"1d{potion.PotionProperties[PotionProperty.AcidDamage]}";
                }
                else if (potion.PotionProperties.ContainsKey(PotionProperty.HolyDamage))
                {
                    damageType = DamageType.Holy;
                    damageRoll = $"1d{potion.PotionProperties[PotionProperty.HolyDamage]}";
                }
            }

            if (damageRoll == string.Empty) return "potion breaks with no effect.";


            List<GridPosition> affectedSquares = new List<GridPosition>() { targetPosition };
            var grid = dungeon != null ? dungeon.DungeonGrid : hero.Room.Grid;
            if (potion.PotionProperties != null && potion.PotionProperties.TryGetValue(PotionProperty.Throwable, out int radius))
            {
                affectedSquares = GridService.GetAllSquaresInRadius(targetPosition, radius, grid);
            }
            var characters = dungeon != null ? dungeon.AllCharactersInDungeon : hero.Room.CharactersInRoom;
            var affectedCharacters = characters.Where(c => c.Position != null && affectedSquares.Contains(c.Position)).ToList();

            var rollResult = await _diceRoll.RequestRollAsync($"Roll for {damageType} damage.", damageRoll);
            await Task.Yield();
            var damage = rollResult.Roll;

            var resultMessage = new StringBuilder($"{potion.Name} breaks at {targetPosition}!");

            foreach (var character in affectedCharacters)
            {

                if (character.Position != null && character.Position.Equals(targetPosition))
                {
                    var appliedDamage = await character.TakeDamageAsync(damage, (new FloatingTextService(), character.Position), _powerActivation, damageType: damageType);
                    resultMessage.AppendLine($"{character.Name} takes {appliedDamage} {damageType} damage.");
                }
                else
                {
                    var splashDamage = (int)Math.Ceiling(damage / 2.0);
                    splashDamage = await character.TakeDamageAsync(splashDamage, (new FloatingTextService(), character.Position), _powerActivation, damageType: damageType);
                    resultMessage.AppendLine($"{character.Name} is caught in the splash and takes {splashDamage} {damageType} damage.");
                }
            }
            return resultMessage.ToString();
        }

        public Weapon CoatWeapon(Hero hero, Potion potion, Weapon weapon)
        {
            BackpackHelper.TakeOneItem(hero.Inventory.Backpack, potion);
            StatusEffectType effectType = StatusEffectType.Poisoned;
            if (potion.PotionProperties != null && potion.PotionProperties.ContainsKey(PotionProperty.FireDamage))
            {
                effectType = StatusEffectType.FireBurning;
            }
            weapon.Properties.Add((WeaponProperty)effectType, 1);
            return weapon;
        }

        public Ammo CoatAmmo(Hero hero, Potion potion, Ammo ammo)
        {
            BackpackHelper.TakeOneItem(hero.Inventory.Backpack, potion);
            if (potion.Name == "Holy Water")
            {
                ammo.Properties.TryAdd(AmmoProperty.Silver, 1);
            }
            else
            {
                ammo.AppliedEffectOnHit = potion.ActiveStatusEffect;
            }
            return ammo;
        }
    }
}