using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using Microsoft.AspNetCore.Rewrite;
using System;
using System.Collections;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LoDCompanion.BackEnd.Models
{
    public enum FacingDirection
    {
        North,
        South,
        East,
        West
    }

    public enum BasicStat
    {
        Strength,
        Constitution,
        Dexterity,
        Wisdom,
        Resolve,
        DamageBonus,
        NaturalArmour,
        ActionPoints,
        Energy,
        Mana,
        Sanity,
        Luck,
        Move,
        HitPoints,
        Experience,
        Level
    }

    public enum Skill
    {
        CombatSkill,
        RangedSkill,
        Dodge,
        PickLocks,
        Barter,
        Heal,
        Alchemy,
        Perception,
        ArcaneArts,
        Foraging,
        BattlePrayers
    }

    public class Character : IGameEntity
    {
        public string Id { get; }
        public string Name { get; set; } = string.Empty; // Default to empty string for safety
        public int CurrentHP { get; set; }
        public Dictionary<BasicStat, int> BasicStats { get; set; }
        public Dictionary<Skill, int> SkillStats { get; set; }
        public CombatStance CombatStance { get; set; } = CombatStance.Normal;
        public virtual bool HasShield { get; set; } // Indicates if the character has a shield
        private Room? _room;
        public Room Room
        {
            get => _room ??= new Room();
            set
            {

                // If the character was in a room before, remove it from that room's list.
                if (_room != null)
                {
                    if (this is Hero hero) _room.HeroesInRoom?.Remove(hero);
                    else if (this is Monster monster) _room.MonstersInRoom?.Remove(monster);
                }

                // Assign the new room.
                _room = value;

                // If the new room isn't null, add the character to the new room.
                if (_room != null)
                {
                    if (this is Hero hero)
                    {
                        _room.HeroesInRoom ??= new List<Hero>();
                        if (!_room.HeroesInRoom.Contains(hero))
                        {
                            _room.HeroesInRoom.Add(hero);
                        }
                    }
                    else if (this is Monster monster)
                    {
                        _room.MonstersInRoom ??= new List<Monster>();
                        if (!_room.MonstersInRoom.Contains(monster))
                        {
                            _room.MonstersInRoom.Add(monster);
                        }
                    }
                }
            }
        }
        public GridPosition? Position { get; set; }
        public List<GridPosition> OccupiedSquares { get; set; } = new List<GridPosition>();
        public List<ActiveStatusEffect> ActiveStatusEffects { get; set; } = new List<ActiveStatusEffect>(); // e.g., "Normal", "Poisoned", "Diseased"
        public int CurrentAP { get; set; } = 2;
        public int CurrentMovePoints { get; set; }
        public bool IsVulnerableAfterPowerAttack { get; set; }
        public bool HasMadeFirstMoveAction { get; set; }
        public FacingDirection Facing { get; set; } = FacingDirection.North;
        public event Action<Character>? OnDeath;
        public bool CanAct => CurrentAP > 0;
        public bool Wounded => CurrentHP <= GetStat(BasicStat.HitPoints) / 2;

        public bool TestResolve(int roll) => roll <= GetStat(BasicStat.Resolve);
        public bool TestConstitution(int roll) => roll <= GetStat(BasicStat.Constitution);
        public bool TestStrength(int roll) => roll <= GetStat(BasicStat.Strength);
        public bool TestDexterity(int roll) => roll <= GetStat(BasicStat.Dexterity);


        // Constructor (optional, but good practice for initialization)
        public Character()
        {
            Id = Guid.NewGuid().ToString();

            // Initialize dictionaries
            BasicStats = new Dictionary<BasicStat, int>();
            SkillStats = new Dictionary<Skill, int>();

            // Populate stats with default values to avoid KeyNotFoundException
            foreach (BasicStat stat in Enum.GetValues(typeof(BasicStat)))
            {
                BasicStats[stat] = 0;
            }
            // Set a default move value
            BasicStats[BasicStat.Move] = 4;

            foreach (Skill skill in Enum.GetValues(typeof(Skill)))
            {
                SkillStats[skill] = 0;
            }
        }

        /// <summary>
        /// Gets the value of a specific basic stat.
        /// </summary>
        /// <param name="stat">The basic stat to retrieve.</param>
        /// <returns>The value of the stat plus any active status effect modifiers, or 0 if not found.</returns>
        public int GetStat(BasicStat stat)
        {
            if (!BasicStats.TryGetValue(stat, out int value)) return 0;

            // Filter for effects that have a stat bonus for the specific stat being requested.
            value += ActiveStatusEffects
                .Where(e => e.StatBonus.HasValue && e.StatBonus.Value.Item1 == stat)
                .Sum(e => e.StatBonus != null ? e.StatBonus.Value.Item2 : 0);

            if (this is Hero hero)
            {
                value += hero.Talents
                    .Where(e => e.StatBonus.HasValue && e.StatBonus.Value.Item1 == stat)
                    .Sum(e => e.StatBonus != null ? e.StatBonus.Value.Item2 : 0);
            }

            return value;
        }

        /// <summary>
        /// Sets the value of a specific basic stat.
        /// This method is virtual to allow derived classes (like Hero) to add specific logic.
        /// </summary>
        /// <param name="stat">The basic stat to set.</param>
        /// <param name="value">The new value for the stat.</param>
        public virtual void SetStat(BasicStat stat, int value)
        {
            BasicStats[stat] = value;

            if (stat == BasicStat.HitPoints)
            {
                CurrentHP = value; // Ensure CurrentHP is set to the new HitPoints value
            }
        }

        /// <summary>
        /// Gets the value of a specific skill.
        /// </summary>
        /// <param name="skill">The skill to retrieve.</param>
        /// <returns>The value of the skill plus any active status effect modifiers, or 0 if not found.</returns>
        public int GetSkill(Skill skill)
        {
            if (!SkillStats.TryGetValue(skill, out int value)) return 0;

            value += ActiveStatusEffects
                .Where(e => e.SkillBonus.HasValue && e.SkillBonus.Value.Item1 == skill)
                .Sum(e => e.SkillBonus != null ? e.SkillBonus.Value.Item2 : 0);

            if (this is Hero hero)
            {
                value += hero.Talents
                    .Where(e => e.SkillBonus.HasValue && e.SkillBonus.Value.Item1 == skill)
                    .Sum(e => e.SkillBonus != null ? e.SkillBonus.Value.Item2 : 0);

                foreach (var talent in hero.Talents)
                {
                    if (skill == Skill.CombatSkill)
                    {
                        switch (talent.Name)
                        {
                            case TalentName.Axeman
                            when hero.Inventory.EquippedWeapon?.HasProperty(WeaponProperty.Axe) == true:
                                value += 5;
                                break;
                            case TalentName.Bruiser
                            when hero.Inventory.EquippedWeapon?.HasProperty(WeaponProperty.Blunt) == true:
                                value += 5;
                                break;
                            case TalentName.Swordsman
                            when hero.Inventory.EquippedWeapon?.HasProperty(WeaponProperty.Sword) == true:
                                value += 5;
                                break;
                            case TalentName.TunnelFighter
                            when hero.Room.Name.StartsWith('C') && hero.Room.Name.Length <= 4:
                                value += 10;
                                break;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Sets the value of a specific skill.
        /// </summary>
        /// <param name="skill">The skill to set.</param>
        /// <param name="value">The new value for the skill.</param>
        public void SetSkill(Skill skill, int value)
        {
            SkillStats[skill] = value;
        }

        public override bool Equals(object? obj)
        {
            // Check if the object is a Character and then use the type-safe Equals method.
            return Equals(obj as Character);
        }

        public bool Equals(Character? other)
        {
            // If the other object is null, they are not equal.
            if (other is null) return false;

            // Two characters are the same if they have the same ID.
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            // The hash code should be based on the unique identifier.
            return Id.GetHashCode();
        }

        public static bool operator ==(Character? left, Character? right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        public static bool operator !=(Character? left, Character? right)
        {
            return !(left == right);
        }

        public virtual void UpdateOccupiedSquares()
        {
            if (Position == null) return;
            OccupiedSquares.Clear();
            int SizeX = 1;
            int SizeY = 1;
            int SizeZ = 1;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    for (int z = 0; z < SizeZ; z++)
                    {
                        OccupiedSquares.Add(new GridPosition(Position.X + x, Position.Y + y, Position.Z + z));
                    }
                }
            }
        }

        // Common methods for all characters
        public virtual async Task<int> TakeDamageAsync(int damage, (FloatingTextService, GridPosition?) floatingText, PowerActivationService activation, CombatContext? combatContext = null, DamageType? damageType = null, bool ignoreAllArmour = false)
        {
            int naturalArmour = GetStat(BasicStat.NaturalArmour);
            bool fireDamage = combatContext != null && combatContext.IsFireDamage || damageType == DamageType.Fire;
            bool acidDamage = combatContext != null && combatContext.IsAcidicDamage || damageType == DamageType.Acid;
            bool frostDamage = combatContext != null && combatContext.IsFrostDamage || damageType == DamageType.Frost;
            bool poisonDamage = combatContext != null && combatContext.IsPoisonousAttack || damageType == DamageType.Poison;
            bool holyDamage = damageType == DamageType.Holy;
            bool diseaseDamage = combatContext != null && combatContext.CausesDisease;

            if (!fireDamage || !acidDamage || !ignoreAllArmour)
            {
                damage -= naturalArmour; //natural armour is not affected by armour piercing 
            }
            if (!fireDamage || !ignoreAllArmour)
            {
                damage -= combatContext?.ArmourValue ?? 0; // Apply any armour value from the combat context 
            }

            var isUndead = this is Monster monster && monster.IsUndead;
            if (holyDamage && !isUndead && combatContext != null && combatContext.IsThrown)
            {
                return 0;
            }

            CurrentHP -= damage;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Die();
            }

            if (floatingText.Item2 != null)
            {
                floatingText.Item1.ShowText($"-{damage}", floatingText.Item2, "damage-text"); 
            }

            if (fireDamage)
            {
                await ApllyFireEffectAsync(damage, activation);
            }
            if (acidDamage)
            {
                await ApllyAcidEffectAsync(damage, activation);
            }
            if (frostDamage)
            {
                await ApplyFrostEffectAsync(activation);
            }
            if (poisonDamage)
            {
                await ApplyPoisonEffectAsync(activation);
            }
            if (diseaseDamage)
            {
                await ApplyDiseaseEffectAsync(activation);
            }

            return damage; // Return the amount of damage taken
        }

        private async Task ApplyDiseaseEffectAsync(PowerActivationService activation)
        {
            await StatusEffectService.AttemptToApplyStatusAsync(this, new ActiveStatusEffect(StatusEffectType.Diseased, -1), activation);
        }

        private async Task ApplyPoisonEffectAsync(PowerActivationService activation)
        {
            if (this is Hero)
            {
                await StatusEffectService.AttemptToApplyStatusAsync(this, new ActiveStatusEffect(StatusEffectType.Poisoned, RandomHelper.RollDie(DiceType.D10), damage: 1), activation);
            }
            else
            {
                await StatusEffectService.AttemptToApplyStatusAsync(this, new ActiveStatusEffect(StatusEffectType.Poisoned, -1, damage: 1), activation);
            }
        }

        private async Task ApplyFrostEffectAsync(PowerActivationService activation)
        {
            int roll = RandomHelper.RollDie(DiceType.D100);
            if (roll <= 50) // 50% chance to apply frost effect
            {
                await StatusEffectService.AttemptToApplyStatusAsync(this, new ActiveStatusEffect(StatusEffectType.Stunned, 1), activation);
            }
        }

        private async Task ApllyAcidEffectAsync(int damage, PowerActivationService activation)
        {
            int roll = RandomHelper.RollDie(DiceType.D6);
            if (roll >= 4)
            {
                await StatusEffectService.AttemptToApplyStatusAsync(this, new ActiveStatusEffect(StatusEffectType.AcidBurning, 1, damage: (int)Math.Ceiling(damage / 2d)), activation); 
            }
        }

        private async Task ApllyFireEffectAsync(int damage, PowerActivationService activation)
        {
            int roll = RandomHelper.RollDie(DiceType.D6);
            if (roll >= 4)
            {
                await StatusEffectService.AttemptToApplyStatusAsync(this, new ActiveStatusEffect(StatusEffectType.FireBurning, 1, damage: (int)Math.Ceiling(damage / 2d)), activation);
            }
        }

        public void Heal(int amount)
        {
            CurrentHP += amount;
            if (CurrentHP > GetStat(BasicStat.HitPoints))
            {
                CurrentHP = GetStat(BasicStat.HitPoints);
            }
        }

        /// <summary>
        /// Handles the character's death, raising the OnDeath event.
        /// </summary>
        protected virtual void Die()
        {
            Console.WriteLine($"{Name} has been defeated!");
            OnDeath?.Invoke(this);
        }

        public void ResetActionPoints()
        {
            CurrentAP += !Wounded ? GetStat(BasicStat.ActionPoints) : (int)Math.Ceiling(GetStat(BasicStat.ActionPoints) / 2d);
            if (CurrentAP > GetStat(BasicStat.ActionPoints)) CurrentAP = GetStat(BasicStat.ActionPoints);
        }

        public void SpendActionPoints(int amount)
        {
            CurrentAP -= amount;
            if (CurrentAP < 0)
            {
                CurrentAP = 0;
            }
        }

        public void ResetMovementPoints()
        {
            // Reset movement points to the base value defined by the Move stat
            CurrentMovePoints = GetStat(BasicStat.Move);
        }

        internal void SpendMovementPoints(int movementPointsSpent)
        {
            CurrentMovePoints -= movementPointsSpent;
        }
    }

    /// <summary>
    /// Represents the tactical stance of a character in combat.
    /// </summary>
    public enum CombatStance
    {
        Normal,
        Parry,
        Overwatch,
        Aiming,
        Prone
    }

    public class Hero : Character
    {
        // Basic Hero Information
        public Party? Party { get; set; }
        public string SpeciesName { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public int CurrentEnergy { get; set; } = 1;
        public int? CurrentMana { get; set; }
        public int CurrentSanity { get; set; } = 10;
        public int Level { get; set; } = 1;
        private int MaxLevel { get; set; } = 10;
        public int Experience { get; set; }
        public int XPtoLVL => RequiredXPbyLVL(Level);

        // Hero-specific States and Flags
        public int MaxArmourType => GetProfessionMaxArmourType(ProfessionName);
        public bool IsThief => ProfessionName == "Thief";
        public int OneHandedWeaponClass => Get1HWeaponClass(GetStat(BasicStat.Strength));
        public int TwoHandedWeaponClass => Get2HWeaponClass(GetStat(BasicStat.Strength));

        // Collections of Hero-specific items/abilities
        public List<Talent> Talents { get; set; } = new List<Talent>();
        public List<Perk> Perks { get; set; } = new List<Perk>();
        public Inventory Inventory { get; set; } = new Inventory();
        public int DualWieldBonus => GetDualWieldBonus();
        public Dictionary<ArmourProperty, int> ArmourDefValues => GetDefValues();
        public override bool HasShield => Inventory.OffHand != null && Inventory.OffHand is Shield;

        public List<Spell>? Spells { get; set; }
        public ChanneledSpell? ChanneledSpell { get; set; }
        public List<Prayer>? Prayers { get; set; }
        public int Coins { get; set; } = 150;

        public bool HasBeenTargetedThisTurn { get; set; }
        public bool HasDodgedThisBattle { get; set; }
        public bool HasParriedThisTurn { get; set; }
        public List<Monster> AfraidOfTheseMonsters { get; set; } = new List<Monster>();
        public Levelup Levelup { get; set; } = new Levelup();
        public bool ReceivedPerfectRollSkill { get; set; }
        public bool ReceivedPerfectRollStat { get; set; }
        public Monster MonsterLastFought { get; set; } = new Monster();
        public bool CanCastSpell { get; set; } = false;

        // Constructor
        public Hero() : base() { }

        /// <summary>
        /// Overriding SetStat to include special logic for when a Hero's Strength changes.
        /// </summary>
        public override void SetStat(BasicStat stat, int value)
        {
            base.SetStat(stat, value); // Set the stat using the base class method

            // If the strength stat was changed, we need to check weapon requirements.
            if (stat == BasicStat.Strength)
            {
                CheckWeaponRequirements();
                SetStat(BasicStat.DamageBonus, GetDamageBonusFromSTR());
            }

            if (stat == BasicStat.Constitution)
            {
                SetStat(BasicStat.NaturalArmour, GetNaturalArmourFromCON());
            }

            if (ProfessionName == "Wizard" && stat == BasicStat.Wisdom)
            {
                SetStat(BasicStat.Mana, GetStat(BasicStat.Wisdom));
            }

            if (stat == BasicStat.Sanity)
            {
                CurrentSanity = value; // Ensure CurrentSanity is set to the new Sanity value
            }

            if (stat == BasicStat.HitPoints)
            {
                CurrentHP = value; // Ensure CurrentHP is set to the new HitPoints value
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"--- Hero: {Name} (Level {GetStat(BasicStat.Level)} {SpeciesName} {ProfessionName}) ---");
            sb.AppendLine($"HP: {CurrentHP}/{GetStat(BasicStat.HitPoints)}, Sanity: {CurrentSanity}/{GetStat(BasicStat.Sanity)}, Energy: {CurrentEnergy}/{GetStat(BasicStat.Energy)}, XP: {GetStat(BasicStat.Experience)}, Coins: {Coins}");
            if (GetStat(BasicStat.Mana) > 0) sb.AppendLine($"Mana: {CurrentMana}/{GetStat(BasicStat.Mana)}");

            sb.AppendLine("\n-- Stats --");
            sb.AppendLine($"STR: {GetStat(BasicStat.Strength)}, CON: {GetStat(BasicStat.Constitution)}, DEX: {GetStat(BasicStat.Dexterity)}, WIS: {GetStat(BasicStat.Wisdom)}, RES: {GetStat(BasicStat.Resolve)}");
            sb.AppendLine($"NA: {GetStat(BasicStat.NaturalArmour)}, DB: {GetStat(BasicStat.DamageBonus)}, Luck: {GetStat(BasicStat.Luck)}");

            sb.AppendLine("\n-- Skills --");
            sb.AppendLine($"Combat Skill: {GetSkill(Skill.CombatSkill)}, Ranged Skill: {GetSkill(Skill.RangedSkill)}, Dodge: {GetSkill(Skill.Dodge)}");
            sb.AppendLine($"Pick Locks: {GetSkill(Skill.PickLocks)}, Barter: {GetSkill(Skill.Barter)}, Heal: {GetSkill(Skill.Heal)}, Alchemy: {GetSkill(Skill.Alchemy)}");
            sb.AppendLine($"Perception: {GetSkill(Skill.Perception)}, Arcane Arts: {GetSkill(Skill.ArcaneArts)}, Foraging: {GetSkill(Skill.Foraging)}, Battle Prayers: {GetSkill(Skill.BattlePrayers)}");

            if (Talents.Any()) sb.AppendLine($"\n-- Talents --\n{string.Join(", ", Talents.Select(t => t.Name))}");
            if (Perks.Any()) sb.AppendLine($"\n-- Perks --\n{string.Join(", ", Perks.Select(p => p.Name))}");
            if (Spells != null && Spells.Any()) sb.AppendLine($"\n-- Spells --\n{string.Join(", ", Spells.Select(s => s.Name))}");
            if (Prayers != null && Prayers.Any()) sb.AppendLine($"\n-- Prayers --\n{string.Join(", ", Prayers.Select(p => p.Name))}");
            if (Inventory.Backpack.Any()) sb.AppendLine($"\n-- Backpack --\n{string.Join(", ", Inventory.Backpack.Select(e => e.Name))}");

            return sb.ToString();
        }

        private int GetProfessionMaxArmourType(string professionName)
        {
            return professionName switch
            {
                "Wizard" => 2,
                "Thief" => 2,
                "Rogue" => 3,
                "Ranger" => 3,
                "Barbarian" => 3,
                "Alchemist" => 3,
                "Warrior Priest" => 4,
                "Warrior" => 4,
                _ => 2
            };
        }

        public bool ResistDisease(int? roll = null)
        {
            if (roll == null)
            {
                roll = RandomHelper.RollDie(DiceType.D100);
            }
            CheckPerfectRoll((int)roll, stat: BasicStat.Constitution);
            int con = GetStat(BasicStat.Constitution);

            if (Talents.Any(t => t.Name == TalentName.ResistDisease)) roll -= 10;
            if (ActiveStatusEffects.Any(e => e.Category == StatusEffectType.ProvidenceOfMetheia)) roll -= 10;

            return TestConstitution((int)roll);
        }

        public bool ResistPoison(int? roll = null)
        {
            if (roll == null)
            {
                roll = RandomHelper.RollDie(DiceType.D100);
            }
            CheckPerfectRoll((int)roll, stat: BasicStat.Constitution);
            int con = GetStat(BasicStat.Constitution);

            if (Talents.Any(t => t.Name == TalentName.ResistPoison)) roll -= 10;
            if (ActiveStatusEffects.Any(e => e.Category == StatusEffectType.ProvidenceOfMetheia)) roll -= 10;

            return TestConstitution((int)roll);
        }

        internal async Task<bool> ResistFearAsync(Monster fearCauser, PowerActivationService activation, int? roll = null, bool wasTerror = false)
        {
            await Task.Yield();
            fearCauser.PassiveSpecials.TryGetValue(MonsterSpecialName.CauseFear, out int level);
            if (this.Level > level) return true;

            if (roll == null)
            {
                roll = RandomHelper.RollDie(DiceType.D100);
            }

            if (!ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Encouragement))
            {
                await AskForPartyPerkAsync(activation, PerkName.Encouragement);
            }

            roll -= GetResistFearModifiations(wasTerror);

            if (!TestResolve((int)roll))
            {
                if (await AskForPartyPerkAsync(activation, PerkName.Rally))
                    return await ResistFearAsync(fearCauser, activation, roll: (await new UserRequestService().RequestRollAsync("Roll resolve test", "1d100")).Roll);
                AfraidOfTheseMonsters.Add(fearCauser);
                return false;
            }

            return true;
        }

        internal async Task<bool> ResistTerrorAsync(Monster fearCauser, PowerActivationService activation, int? roll = null)
        {
            await Task.Yield();
            fearCauser.PassiveSpecials.TryGetValue(MonsterSpecialName.CauseTerror, out int level);
            if (this.Level > level) return true;
            
            if (roll == null)
            {
                roll = RandomHelper.RollDie(DiceType.D100);
            }

            if (!ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Encouragement))
            {
                await AskForPartyPerkAsync(activation, PerkName.Encouragement);
            }

            if(ActiveStatusEffects.Any(e => e.Category == StatusEffectType.PowerOfFaith))
            {
                return await ResistFearAsync(fearCauser, activation, roll, wasTerror: true); // Treats terror as fear
            }
            else
            {
                roll += GetResistFearModifiations();
            }

            if (!TestResolve((int)roll + 20))
            {
                if (await AskForPartyPerkAsync(activation, PerkName.Rally))
                    {return await ResistTerrorAsync(fearCauser, activation, roll: (await new UserRequestService().RequestRollAsync("Roll resolve test", "1d100")).Roll);}
                
                AfraidOfTheseMonsters.Add(fearCauser);
                await StatusEffectService.AttemptToApplyStatusAsync(this, new ActiveStatusEffect(StatusEffectType.Stunned, 1), activation);
                return false;
            }

            return true;
        }

        public async Task<bool> AskForPartyPerkAsync(PowerActivationService activation, PerkName perkName)
        {
            if (Party != null && Party.Heroes.Any(h => h.Perks.Any(p => p.Name == perkName)))
            {
                var heroesWithPerk = Party.Heroes.Where(h => h.Perks.Any(p => p.Name == perkName));

                foreach (var hero in heroesWithPerk)
                {
                    var perk = hero.Perks.FirstOrDefault(p => p.Name == perkName);
                    if (perk != null && perk.ActiveStatusEffect != null && hero.CurrentEnergy >= 1)
                    {
                        await Task.Yield();
                        if (await new UserRequestService().RequestYesNoChoiceAsync($"Does {hero.Name} wish to use their perk {perk.Name.ToString()}"))
                        {
                            return await activation.ActivatePerkAsync(hero, perk, target: this);
                        }
                        else continue;
                    }
                    else continue;
                }
            }

            return false;
        }

        public int GetResistFearModifiations(bool wasTerror = false)
        {
            int modification = 0;
            foreach (var effect in ActiveStatusEffects)
            {
                switch (effect.Category)
                {
                    case StatusEffectType.ThePowerOfIphy: modification -= 10; break;
                    case StatusEffectType.PowerOfFaith: if (!wasTerror) modification -= 100; break; // resist fear completely, but if was terror initially treat as fear without the complete resist.
                    case StatusEffectType.Encouragement: modification -= 10; ActiveStatusEffects.Remove(effect); break;
                }
            }
            return modification;
        }

        public int GetDamageBonusFromSTR()
        {
            return GetStat(BasicStat.Strength) switch
            {
                < 60 => 0,
                < 70 => 1,
                < 80 => 2,
                _ => 3,
            };
        }

        public int GetNaturalArmourFromCON()
        {
            return GetStat(BasicStat.Constitution) switch
            {
                < 50 => 0,
                < 55 => 1,
                < 60 => 2,
                < 65 => 3,
                < 70 => 4,
                _ => 5,
            };
        }

        public static int Get1HWeaponClass(int str)
        {
            return str switch
            {
                < 30 => 1,
                < 40 => 2,
                < 50 => 3,
                >= 50 => 4
            };
        }

        public static int Get2HWeaponClass(int str)
        {
            return str switch
            {
                < 30 => 2,
                < 40 => 3,
                < 55 => 4,
                >= 55 => 5
            };
        }

        public static string GetWieldStatus(int str, Weapon weapon)
        {
            if (weapon.Class != 6 && Get2HWeaponClass(str) < weapon.Class)
            {
                return "(Too weak to wield)";
            }
            if (Get1HWeaponClass(str) >= weapon.Class)
            {
                return "(1-Handed)";
            }
            return "(2-Handed)";
        }

        /// <summary>
        /// Checks equipped weapons against the hero's current strength and unequips any
        /// that they are too weak to wield.
        /// </summary>
        private void CheckWeaponRequirements()
        {
            var equippedWeapon = this.Inventory.EquippedWeapon;

            if (equippedWeapon != null && GetWieldStatus(GetStat(BasicStat.Strength), equippedWeapon) == "(Too weak to wield)")
            {
                new InventoryService().UnequipItem(this, equippedWeapon);

                Console.WriteLine($"{Name} is no longer strong enough to wield the {equippedWeapon.Name} and has unequipped it.");
            }
        }

        private int GetDualWieldBonus()
        {
            if (Inventory.OffHand is MeleeWeapon dualWieldWeapon)
            {
                if (dualWieldWeapon.HasProperty(WeaponProperty.DualWield))
                    return dualWieldWeapon.GetPropertyValue(WeaponProperty.DualWield);
            }
            return 0; // No dual wield bonus if the weapon doesn't have the property
        }

        private Dictionary<ArmourProperty, int> GetDefValues()
        {
            int head = 0;
            int torso = 0;
            int arms = 0;
            int legs = 0;
            foreach (var armour in Inventory.EquippedArmour)
            {
                foreach (var property in armour.Properties)
                {
                    switch (property.Key)
                    {
                        case ArmourProperty.Head:
                            head += armour.DefValue;
                            break;
                        case ArmourProperty.Torso:
                            torso += armour.DefValue;
                            break;
                        case ArmourProperty.Arms:
                            arms += armour.DefValue;
                            break;
                        case ArmourProperty.Legs:
                            legs += armour.DefValue;
                            break;
                    }
                }
            }

            return new Dictionary<ArmourProperty, int>
            {
                { ArmourProperty.Head, head },
                { ArmourProperty.Torso, torso },
                { ArmourProperty.Arms, arms },
                { ArmourProperty.Legs, legs }
            };
        }

        public void ActivateDiseasedEffect()
        {
            // Check for the base "Diseased" status effect, which acts as the trigger.
            if (ActiveStatusEffects.Any(e => e.Category == StatusEffectType.Diseased))
            {
                // --- Handle Constitution Penalty ---
                var diseasedConstitution = ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.Diseased && a.StatBonus.HasValue && a.StatBonus.Value.Item1 == BasicStat.Constitution);

                int currentCon = GetStat(BasicStat.Constitution);
                int additionalConPenalty = currentCon / 2;
                int existingConPenalty = 0;

                if (diseasedConstitution != null)
                {
                    existingConPenalty = diseasedConstitution.StatBonus?.Item2 ?? 0;
                    ActiveStatusEffects.Remove(diseasedConstitution);
                }

                ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.Diseased, -1, statBonus: (BasicStat.Constitution, existingConPenalty - additionalConPenalty)));

                // --- Handle Strength Penalty ---
                var diseasedStrength = ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.Diseased && a.StatBonus.HasValue && a.StatBonus.Value.Item1 == BasicStat.Strength);

                int currentStr = GetStat(BasicStat.Strength);
                int additionalStrPenalty = currentStr / 2;
                int existingStrPenalty = 0;

                if (diseasedStrength != null)
                {
                    existingStrPenalty = diseasedStrength.StatBonus?.Item2 ?? 0;
                    ActiveStatusEffects.Remove(diseasedStrength);
                }

                ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.Diseased, -1, statBonus: (BasicStat.Strength, existingStrPenalty - additionalStrPenalty)));
            }
        }

        private int RequiredXPbyLVL(int level)
        {
            return level switch
            {
                1 => 1000,
                2 => 2500,
                3 => 6000,
                4 => 10500,
                5 => 16000,
                6 => 25000,
                7 => 35000,
                8 => 50000,
                9 => 70000,
                _ => 0,
            };
        }

        public void GainExperience(int experience)
        {
            SetStat(BasicStat.Experience, GetStat(BasicStat.Experience) + experience);
        }

        internal void CheckPerfectRoll(int roll, Skill? skill = null, BasicStat? stat = null)
        {
            if(ActiveStatusEffects.Any(e => e.Category == StatusEffectType.CharusWalkWithUs) && roll <= 10 && roll > 5) CurrentEnergy += 1;
            else if (roll <= 5)
            {
                CurrentEnergy += 1;
                if (skill.HasValue && !ReceivedPerfectRollSkill) SetSkill(skill.Value, GetSkill(skill.Value) + 1);
                if (stat.HasValue && !ReceivedPerfectRollStat) SetStat(stat.Value, GetStat(stat.Value) + 1);
            }

            if (CurrentEnergy > GetStat(BasicStat.Energy)) CurrentEnergy = GetStat(BasicStat.Energy);
        }

        public async Task TakeSanityDamage(int damage, (FloatingTextService, GridPosition?) floatingText, PowerActivationService activation)
        {
            bool resisted = false;
            if(ActiveStatusEffects.Any(e => e.Category == StatusEffectType.VerseOfTheSane))
            {
                var result = await new UserRequestService().RequestRollAsync("Roll for resolve test", "1d100"); await Task.Yield();
                resisted = TestResolve(result.Roll);
            }
            
            if(!resisted) CurrentSanity -= damage;
            if (floatingText.Item2 != null)
            {
                floatingText.Item1.ShowText($"-{damage}", floatingText.Item2, "damage-text");
            }

            if (CurrentSanity <= 0) await GetConditionAsync(activation);
        }

        public async Task GetConditionAsync(PowerActivationService activation)
        {
            bool conditionApplied = false;
            while (!conditionApplied)
            {
                ActiveStatusEffect? conditionToApply = null;
                var result = await new UserRequestService().RequestRollAsync("Roll for condition", "1d10"); await Task.Yield();
                switch (result.Roll)
                {
                    case 1:
                        string statusEffectName = "Hate" + MonsterLastFought.HateCategory.ToString();
                        if (Enum.TryParse<StatusEffectType>(statusEffectName, out var statusEffect))
                        {
                            conditionToApply = new ActiveStatusEffect(statusEffect, -1);
                        }
                        break;
                    case <= 3: conditionToApply = new ActiveStatusEffect(StatusEffectType.AcuteStessSyndrome, -1); break;
                    case 4: conditionToApply = new ActiveStatusEffect(StatusEffectType.PostTraumaticStressDisorder, -1); break;
                    case 5: conditionToApply = new ActiveStatusEffect(StatusEffectType.FearDark, -1); break;
                    case 6: conditionToApply = new ActiveStatusEffect(StatusEffectType.Arachnophobia, -1); break;
                    case 7: conditionToApply = new ActiveStatusEffect(StatusEffectType.Jumpy, -1); break;
                    case 8: conditionToApply = new ActiveStatusEffect(StatusEffectType.IrrationalFear, -1); break;
                    case 9: conditionToApply = new ActiveStatusEffect(StatusEffectType.Claustrophobia, -1); break;
                    case 10: conditionToApply = new ActiveStatusEffect(StatusEffectType.Depression, -1); break;
                }
                if (conditionToApply != null)
                {
                    string outcome = await StatusEffectService.AttemptToApplyStatusAsync(this, conditionToApply, activation);
                    if (outcome != "Already affected") conditionApplied = true; 
                }
            }

        }
    }

    public enum MonsterBehaviorType
    {
        HumanoidMelee,
        HumanoidRanged,
        MagicUser,
        Beast,
        LowerUndead,
        HigherUndead
    }

    public enum SpeciesName
    {
        Banshee,
        Basilisk,
        Beastman,
        CaveBear,
        CaveGoblin,
        Centaur,
        CommonTroll,
        DireWolf,
        DarkElf,
        Demon,
        Dragon,
        Drider,
        Ettin,
        Frogling,
        Gargoyle,
        Gecko,
        Ghost,
        Ghoul,
        Giant,
        GiantBat,
        GiantCentipede,
        GiantLeech,
        GiantPoxRat,
        GiantRat,
        GiantScorpion,
        GiantSnake,
        GiantSpider,
        GiantToad,
        GiantWolf,
        GiganticSnake,
        GiganticSpider,
        Gnoll,
        Goblin,
        Griffon,
        Harpy,
        Human,
        Hydra,
        Lurker,
        Medusa,
        Mimic,
        Minotaur,
        MinotaurSkeleton,
        Mummy,
        Naga,
        Ogre,
        Orc,
        Raptor,
        RiverTroll,
        Salamander,
        Satyr,
        Saurian,
        Shambler,
        Skeleton,
        Slime,
        Sphinx,
        StoneGolem,
        StoneTroll,
        TombGuardian,
        Vampire,
        Werewolf,
        Wight,
        Wraith,
        Wyvern,
        Zombie,
        ZombieOgre,
        Unknown
    }

    public class Monster : Character // Inherit from the new Character base class
    {
        public SpeciesName Species {  get; set; }
        public EncounterType? Type { get; set; }
        public int ArmourValue { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public bool HasSpecialAttack => ActiveAbilities != null;
        public List<SpecialActiveAbility>? ActiveAbilities { get; set; }
        public int ToHitPenalty { get; set; }
        public int XP { get; set; }
        public Dictionary<MonsterSpecialName, int> PassiveSpecials { get; set; } = new Dictionary<MonsterSpecialName, int>();
        public List<string> SpecialRules { get; set; } = new List<string>(); // List of raw rule names
        public bool IsUndead => Type == EncounterType.Undead || Behavior == MonsterBehaviorType.LowerUndead || Behavior == MonsterBehaviorType.HigherUndead;
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public Weapon? ActiveWeapon { get; set; }
        public List<MonsterSpell> Spells { get; set; } = new List<MonsterSpell>(); // List of actual spell names
        public Corpse? Body { get; set; }
        private TreasureType _treasureType = TreasureType.None;
        public TreasureType TreasureType
        {
            get => _treasureType;
            set
            {
                _treasureType = value;
                Body = new Corpse(this);
            }
        }
        public List<string> Treasures { get; set; } = new List<string>();
        public MonsterBehaviorType Behavior { get; set; } = MonsterBehaviorType.HumanoidMelee;
        public HateCategory HateCategory { get; set; } = HateCategory.Bandits;

        public Monster()
        {

        }

        public Monster Clone()
        {
            // Create a new instance that the DI container doesn't know about.
            var newMonster = new Monster();

            // --- Copy Base Character Properties ---
            newMonster.Name = Name;
            newMonster.HasShield = HasShield;

            // --- Copy Monster-Specific Properties ---
            newMonster.Type = Type;
            newMonster.ArmourValue = ArmourValue;
            newMonster.MinDamage = MinDamage;
            newMonster.MaxDamage = MaxDamage;
            newMonster.ToHitPenalty = ToHitPenalty;
            newMonster.XP = XP;
            newMonster.Behavior = Behavior;
            newMonster.TreasureType = TreasureType;

            // --- Copy Stats and Skills using SetStat/SetSkill ---
            foreach (var statEntry in BasicStats)
            {
                newMonster.SetStat(statEntry.Key, statEntry.Value);
            }

            foreach (var skillEntry in SkillStats)
            {
                newMonster.SetSkill(skillEntry.Key, skillEntry.Value);
            }

            foreach (var passive in PassiveSpecials)
            {
                newMonster.SetPassive(passive.Key, passive.Value);
            }

            // --- Copy Collections ---
            newMonster.Weapons = [.. Weapons];
            newMonster.SpecialRules = [.. SpecialRules];
            newMonster.Spells = [.. Spells];
            newMonster.Treasures = [.. Treasures];

            return newMonster;
        }

        private void SetPassive(MonsterSpecialName key, int value)
        {
            PassiveSpecials.TryAdd(key, value);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"--- Monster: {Name} [{Type}] ---");
            sb.AppendLine($"HP: {CurrentHP}/{GetStat(BasicStat.HitPoints)}, Armour: {ArmourValue}, Move: {GetStat(BasicStat.Move)}, XP: {XP}");

            sb.AppendLine("\n-- Combat Stats --");
            sb.AppendLine($"CS: {GetSkill(Skill.CombatSkill)}, RS: {GetSkill(Skill.RangedSkill)}, Dodge: {GetSkill(Skill.Dodge)}, To Hit Bonus: {ToHitPenalty}");
            sb.AppendLine($"Damage: {MinDamage}-{MaxDamage}, DB: {GetStat(BasicStat.DamageBonus)}");

            if (Weapons.Any())
            {
                sb.AppendLine("\n-- Weapons --");
                foreach (var weapon in Weapons)
                {
                    sb.AppendLine($"- {weapon.Name} (Damage: {weapon.MinDamage} - {weapon.MaxDamage})");
                }
            }

            if (SpecialRules.Any()) sb.AppendLine($"\n-- Special Rules --\n{string.Join(", ", SpecialRules)}");
            if (Spells.Any()) sb.AppendLine($"\n-- Spells --\n{string.Join(", ", Spells.Select(s => s.Name))}");

            return sb.ToString();
        }

        public Weapon? GetMeleeWeapon()
        {
            return Weapons.FirstOrDefault(w => w.IsMelee);
        }

        public Weapon? GetRangedWeapon()
        {
            return Weapons.FirstOrDefault(w => w.IsRanged);
        }

        public override void UpdateOccupiedSquares()
        {
            if (Position == null) return;
            OccupiedSquares.Clear();
            int SizeX = 1;
            int SizeY = 1;
            int SizeZ = 1;

            if (PassiveSpecials.Any(n => n.Key == MonsterSpecialName.Large))
            {
                SizeX = 2;
                SizeY = 2;
                SizeZ = 1;
            }
            if (PassiveSpecials.Any(n => n.Key == MonsterSpecialName.XLarge))
            {
                SizeX = 2;
                SizeY = 3;
                SizeZ = 1;
            }

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    for (int z = 0; z < SizeZ; z++)
                    {
                        OccupiedSquares.Add(new GridPosition(Position.X + x, Position.Y + y, Position.Z + z));
                    }
                }
            }
        }
    }
}