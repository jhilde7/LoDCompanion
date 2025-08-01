﻿
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;
using LoDCompanion.Services.Combat;
using LoDCompanion.Utilities;
using System.Text;

namespace LoDCompanion.Models.Character
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
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public bool HasShield { get; set; } // Indicates if the monster has a shield
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
        public GridPosition Position { get; set; } = new GridPosition(0, 0, 0);
        public List<GridPosition> OccupiedSquares { get; set; } = new List<GridPosition>();
        public List<ActiveStatusEffect> ActiveStatusEffects { get; set; } = new List<ActiveStatusEffect>(); // e.g., "Normal", "Poisoned", "Diseased"
        public int CurrentAP { get; set; } = 2;
        public int CurrentMovePoints { get; set; }
        public bool IsVulnerableAfterPowerAttack { get; set; }
        public bool HasMadeFirstMoveAction { get; set; }
        public FacingDirection Facing { get; set; } = FacingDirection.North;
        public event Action<Character>? OnDeath;
        public bool CanAct() => this.CurrentAP > 0;


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
        /// <returns>The value of the stat, or 0 if not found.</returns>
        public int GetStat(BasicStat stat)
        {
            return BasicStats.TryGetValue(stat, out int value) ? value : 0;
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

            if(stat == BasicStat.HitPoints)
            {
                CurrentHP = value; // Ensure CurrentHP is set to the new HitPoints value
            }
        }

        /// <summary>
        /// Gets the value of a specific skill.
        /// </summary>
        /// <param name="skill">The skill to retrieve.</param>
        /// <returns>The value of the skill, or 0 if not found.</returns>
        public int GetSkill(Skill skill)
        {
            return SkillStats.TryGetValue(skill, out int value) ? value : 0;
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
        public virtual void TakeDamage(int damage, DamageType? damageType = null)
        {
            CurrentHP -= damage;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Die();
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
            CurrentAP += GetStat(BasicStat.ActionPoints);
            if(CurrentAP > GetStat(BasicStat.ActionPoints)) CurrentAP = GetStat(BasicStat.ActionPoints);
        }

        public void SpendActionPoints(int amount)
        {
            this.CurrentAP -= amount;
            if (this.CurrentAP < 0)
            {
                this.CurrentAP = 0;
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
        public string SpeciesName { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public int CurrentEnergy { get; set; } = 1;
        public int? CurrentMana { get; set; }
        public int CurrentSanity { get; set; } = 10;

        // Hero-specific States and Flags
        public int MaxArmourType => new GameDataService().GetProfessionMaxArmourType(ProfessionName);
        public bool IsThief => ProfessionName == "Thief";
        public int OneHandedWeaponClass => Get1HWeaponClass(GetStat(BasicStat.Strength));
        public int TwoHandedWeaponClass => Get2HWeaponClass(GetStat(BasicStat.Strength));

        // Collections of Hero-specific items/abilities
        public List<Talent> Talents { get; set; } = new List<Talent>();
        public List<Perk> Perks { get; set; } = new List<Perk>();
        public Inventory Inventory { get; set; } = new Inventory();
        public int DualWieldBonus => GetDualWieldBonus();
        public Dictionary<ArmourProperty, int> ArmourDefValues => GetDefValues();

        public List<Spell>? Spells { get; set; }
        public ChanneledSpell? ChanneledSpell { get; set; }
        public List<Prayer>? Prayers { get; set; }
        public int Coins { get; set; } = 150;

        public bool HasBeenTargetedThisTurn { get; set; }
        public bool HasDodgedThisBattle { get; set; } = false;
        public Levelup Levelup { get; set; } = new Levelup();

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

            if (this.ProfessionName == "Wizard" && stat == BasicStat.Wisdom)
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

        public bool ResistDisease(int? roll = null)
        {
            // This method would use a RandomHelper service or static method now
            if (roll == null)
            {
                roll = RandomHelper.RollDie(DiceType.D100);
            }
            int con = GetStat(BasicStat.Constitution);

            // Apply talent bonuses
            foreach (Talent talent in Talents)
            {
                if (talent.Name == TalentName.ResistDisease)
                {
                    con += 10;
                }
            }

            return (roll <= con);
        }

        public bool ResistPoison(int? roll = null)
        {
            if (roll == null)
            {
                roll = RandomHelper.RollDie(DiceType.D100);
            }
            int con = GetStat(BasicStat.Constitution);

            foreach (var talent in Talents)
            {
                if (talent.Name == TalentName.ResistPoison)
                {
                    con += 10;
                }
            }

            return (roll <= con);
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
            var equippedWeapons = new List<Weapon>(this.Weapons);

            foreach (var weapon in equippedWeapons)
            {
                if (GetWieldStatus(GetStat(BasicStat.Strength), weapon) == "(Too weak to wield)")
                {
                    new InventoryService().UnequipItem(this, weapon);

                    Console.WriteLine($"{this.Name} is no longer strong enough to wield the {weapon.Name} and has unequipped it.");
                }
            }
        }

        private int GetDualWieldBonus()
        {
            MeleeWeapon? dualWieldWeapon = (MeleeWeapon?)Inventory.OffHand;
            if (dualWieldWeapon == null) return 0;
            
            if (dualWieldWeapon.HasProperty(WeaponProperty.DualWield))
                return dualWieldWeapon.GetPropertyValue(WeaponProperty.DualWield);
            return 0; // No dual wield bonus if the weapon doesn't have the property
        }

        private Dictionary<ArmourProperty, int> GetDefValues()
        {
            int head = 0;
            int torso = 0;
            int arms = 0;
            int legs = 0;
            foreach(var armour in Inventory.EquippedArmour)
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

    public class Monster : Character // Inherit from the new Character base class
    {
        public EncounterType? Type { get; set; }
        public int ArmourValue { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public bool HasSpecialAttack => ActiveAbilities != null;
        public List<SpecialActiveAbility>? ActiveAbilities { get; set; }
        public int ToHitPenalty { get; set; } 
        public int XP { get; set; }
        public Dictionary<MonsterPassiveSpecial, int> PassiveSpecials { get; set; } = new Dictionary<MonsterPassiveSpecial, int>();
        public List<string> SpecialRules { get; set; } = new List<string>(); // List of raw rule names
        public bool IsUndead => Type == EncounterType.Undead || Behavior == MonsterBehaviorType.LowerUndead || Behavior == MonsterBehaviorType.HigherUndead;
        public List<MonsterSpell> Spells { get; set; } = new List<MonsterSpell>(); // List of actual spell names
        public Corpse Body { get; set; } = new Corpse("Corpse", TreasureType.None);
        private TreasureType _treasureType = TreasureType.None;
        public TreasureType TreasureType
        {
            get => _treasureType;
            set
            {
                _treasureType = value;
                Body = new Corpse($"{Name} corpse", _treasureType);
            }
        }
        public List<string> Treasures { get; set; } = new List<string>();
        public MonsterBehaviorType Behavior { get; set; } = MonsterBehaviorType.HumanoidMelee;

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

        private void SetPassive(MonsterPassiveSpecial key, int value)
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
            OccupiedSquares.Clear();
            int SizeX = 1;
            int SizeY = 1;
            int SizeZ = 1;

            if (PassiveSpecials.Any(n => n.Key.Name == MonsterSpecialName.Large))
            {
                SizeX = 2;
                SizeY = 2;
                SizeZ = 1;
            }
            if (PassiveSpecials.Any(n => n.Key.Name == MonsterSpecialName.XLarge))
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