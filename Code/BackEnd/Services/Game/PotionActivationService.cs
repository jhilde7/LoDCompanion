using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Dungeon;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Player;
using LoDCompanion.Code.BackEnd.Services.Utilities;
using System.Text;

namespace LoDCompanion.Code.BackEnd.Services.Game
{
    public class PotionActivationService
    {
        private PowerActivationService _powerActivation;
        private UserRequestService _diceRoll;
        public PotionActivationService(PowerActivationService powerActivation, UserRequestService diceRoll)
        {
            _powerActivation = powerActivation;
            _diceRoll = diceRoll;

            StatusEffectService.OnUseHealingPotion += HandleUseHealingPotion;
        }

        public void Dispose()
        {
            StatusEffectService.OnUseHealingPotion -= HandleUseHealingPotion;
        }

        private async Task HandleUseHealingPotion(Hero hero, Potion potion)
        {
            await DrinkPotionAsync(hero, potion);
        }

        public async Task<string> DrinkPotionAsync(Hero hero, Potion potion)
        {
            // Apply the potion's effects
            if (potion.PotionProperties != null)
            {
                foreach (var property in potion.PotionProperties)
                {
                    var diceCount = potion.PotionProperties.GetValueOrDefault(PotionProperty.DiceCount, 1);
                    switch (property.Key)
                    {
                        case PotionProperty.HealHP:
                            int healing = 0;
                            if (property.Value <= 100)
                            {
                                healing = (await _diceRoll.RequestRollAsync("Roll for heal amount.", $"{diceCount}d{property.Value}")).Roll;
                                await Task.Yield();
                                potion.PotionProperties.TryGetValue(PotionProperty.HealHPBonus, out int bonus);
                                hero.Heal(healing + bonus);
                            }
                            else healing = property.Value;
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
                            var rollResult = await _diceRoll.RequestRollAsync("Roll for heal amount.", $"{diceCount}d{property.Value}");
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

            if (potion.ActiveStatusEffects != null)
            {
                foreach (var effect in potion.ActiveStatusEffects)
                { 
                    await StatusEffectService.AttemptToApplyStatusAsync(hero, effect, _powerActivation); 
                }
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
                    var appliedDamage = await character.TakeDamageAsync(damage, (new FloatingTextService(), character.Position), _powerActivation, damageType: (damageType, 0));
                    resultMessage.AppendLine($"{character.Name} takes {appliedDamage} {damageType} damage.");
                }
                else
                {
                    var splashDamage = (int)Math.Ceiling(damage / 2.0);
                    splashDamage = await character.TakeDamageAsync(splashDamage, (new FloatingTextService(), character.Position), _powerActivation, damageType: (damageType, 0));
                    resultMessage.AppendLine($"{character.Name} is caught in the splash and takes {splashDamage} {damageType} damage.");
                }
            }
            return resultMessage.ToString();
        }

        public Weapon CoatWeapon(Hero hero, Potion potion, Weapon weapon)
        {
            if (potion.PotionProperties != null && potion.PotionProperties.ContainsKey(PotionProperty.FireDamage))
            {
                weapon.WeaponCoating = new WeaponCoating(DamageType.Fire) { RemoveAfterCombat = true };
            }
            if (potion.PotionProperties != null && potion.PotionProperties.ContainsKey(PotionProperty.Poison))
            {
                weapon.WeaponCoating = new WeaponCoating(DamageType.Poison) { RemoveAfterCombat = true };
            }
            return weapon;
        }

        public Ammo CoatAmmo(Hero hero, Potion potion, Ammo ammo)
        {

            if (potion.PotionProperties != null && potion.PotionProperties.ContainsKey(PotionProperty.Poison))
            {
                ammo.AmmoCoating = new AmmoCoating(DamageType.Poison, Math.Min(ammo.Quantity, 5));
            }
            if (potion.PotionProperties != null && potion.PotionProperties.ContainsKey(PotionProperty.HolyDamage))
            {
                ammo.AmmoCoating = new AmmoCoating(DamageType.Holy, Math.Min(ammo.Quantity, 5)) { DamageBonus = 1 };
            }
            return ammo;
        }
    }
}