
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;
using LoDCompanion.Utilities;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

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
        public bool IsLarge { get; set; }
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

        public void UpdateOccupiedSquares()
        {
            OccupiedSquares.Clear();
            int SizeX = 1;
            int SizeY = 1;
            int SizeZ = 1;

            if(IsLarge)
            {
                SizeX = 2;
                SizeY = 2;
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

        public List<Spell> Spells { get; set; } = new List<Spell>();
        public int FocusActionRemaining { get; set; }
        public List<Prayer> Prayers { get; set; } = new List<Prayer>();
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
            if (Spells.Any()) sb.AppendLine($"\n-- Spells --\n{string.Join(", ", Spells.Select(s => s.Name))}");
            if (Prayers.Any()) sb.AppendLine($"\n-- Prayers --\n{string.Join(", ", Prayers.Select(p => p.Name))}");
            if (Inventory.Backpack.Any()) sb.AppendLine($"\n-- Backpack --\n{string.Join(", ", Inventory.Backpack.Select(e => e.Name))}");

            return sb.ToString();
        }

        public bool ResistDisease(int? roll = null)
        {
            // This method would use a RandomHelper service or static method now
            if (roll == null)
            {
                roll = RandomHelper.RollDie("D100");
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
                roll = RandomHelper.RollDie("D100");
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
        public bool HasSpecialAttack { get; set; }
        public bool IsGhost { get; set; }
        public int ToHitPenalty { get; set; } 
        public int XP { get; set; }
        public List<string> SpecialRules { get; set; } = new List<string>(); // List of raw rule names
        public List<string> SpecialRuleDescriptions { get; private set; } = new List<string>(); // List of formatted descriptions
        public bool IsUndead { get; set; }
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
            newMonster.Name = this.Name;
            newMonster.IsLarge = this.IsLarge;
            newMonster.HasShield = this.HasShield;

            // --- Copy Monster-Specific Properties ---
            newMonster.Type = this.Type;
            newMonster.ArmourValue = this.ArmourValue;
            newMonster.MinDamage = this.MinDamage;
            newMonster.MaxDamage = this.MaxDamage;
            newMonster.HasSpecialAttack = this.HasSpecialAttack;
            newMonster.IsGhost = this.IsGhost;
            newMonster.ToHitPenalty = this.ToHitPenalty;
            newMonster.XP = this.XP;
            newMonster.IsUndead = this.IsUndead;
            newMonster.Behavior = this.Behavior;
            newMonster.TreasureType = this.TreasureType;

            // --- Copy Stats and Skills using SetStat/SetSkill ---
            foreach (var statEntry in this.BasicStats)
            {
                newMonster.SetStat(statEntry.Key, statEntry.Value);
            }

            foreach (var skillEntry in this.SkillStats)
            {
                newMonster.SetSkill(skillEntry.Key, skillEntry.Value);
            }

            // --- Copy Collections ---
            newMonster.Weapons = new List<Weapon>(this.Weapons);
            newMonster.SpecialRules = new List<string>(this.SpecialRules);
            newMonster.Spells = new List<MonsterSpell>(this.Spells);
            newMonster.Treasures = new List<string>(this.Treasures);

            // --- Final Setup ---
            newMonster.BuildSpecialRuleDescriptions();

            return newMonster;
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

        /// <summary>
        /// Populates the SpecialRuleDescriptions list based on the raw SpecialRules.
        /// This method should be called after a Monster object is fully initialized.
        /// </summary>
        public void BuildSpecialRuleDescriptions()
        {
            SpecialRuleDescriptions.Clear(); // Clear existing descriptions before rebuilding
            foreach (string rule in SpecialRules)
            {
                string newRule = rule;
                string description = GetRuleDescription(newRule);
                if (!string.IsNullOrEmpty(description)) // Only add if a description exists
                {
                    // Handle cases like "Fear 2", "Regenerate 3"
                    // Original code removed last 2 chars if it parsed as int, which is for number in rule name.
                    // This logic might need adjustment based on how the "rules" list is populated,
                    // but for now, we'll just format the rule and description.
                    SpecialRuleDescriptions.Add($"{newRule}: {description}");
                }
                else
                {
                    // If no specific description found, just add the rule name itself
                    SpecialRuleDescriptions.Add(newRule);
                }
            }
        }


        // Private helper method for rule descriptions
        private string GetRuleDescription(string rule)
        {
            // Note: In the original Unity script, this method also set flags on MonsterSpecial.
            // That coupling is removed here. This method ONLY returns the description string.
            // The logic for applying special effects or setting flags on MonsterSpecial
            // should be handled by the MonsterSpecialService when monster actions are processed.
            switch (rule)
            {
                case "Acts first": return "During the first turn this creature always goes first regardless of token drawn.";
                case "Bellow": return "All heroes must pass RES test or be stunned.";
                case "Camouflage": return "If triggered remove model. On next turn place on a random tile adjacent to a hero. if no heros on tile, place with LOS to most heroes as possible.";
                case "Corrosive": return "When hero is hit, if the armour in the area hit is metal then it suffers -1 durability. When hit, if the weapon is metal it suffers -1 durability on an odd DMG roll. Silver and Mithril cannot be corroded";
                case "Cursed weapon": return "A wound caused by a cursed weapon also removes -1 sanity from th target.";
                case "Demon": return "Magic damage of 10 forces the demon to make a RES test. if failed, it's forced back into the Void. Remove enemy, no loot.";
                case "Disease": return "Wounded hero, CON test or diseased.";
                case "Disease ridden": return "Adjacent heroes, CON +10 test every turn or diseased.";
                case "Entangle": return "If this attack is not parried or dodged and it is successful, the hero will be trapped and the only action available is to break free. While entangled, each turn starting at 1hp the hero will take 1HP of increased damage each turn; turn 1 1HP, turn 2 2HP, etc. The needs to make a STR test and 1AP to break free, but after the first turn each turn after there is a -10 modifier added each turn; turn 1 -0, turn 2 -10, turn 3 -20, etc. Other hero's can help for 2AP and STR +10, or if the creature dies the entangled hero is set free.";
                case "Ethereal": return "Ignores ZOC. Immune to mundane weapons.";
                case "Extra damage from fire": return "1d6 extra damage from fire based attacks.";
                case "Extra damage from water": return "1d6 extra damage from water based attacks.";
                case "Extra damage from silver": return "1d6 extra damage from silver weapons.";
                case "Fear 2": return "Hero lvl <= 2, RES test if attacking, fail: -10 CS/RS/AA.";
                case "Fear 3": return "Hero lvl <= 3, RES test if attacking, fail: -10 CS/RS/AA.";
                case "Fear 4": return "Hero lvl <= 4, RES test if attacking, fail: -10 CS/RS/AA.";
                case "Fear 5": return "Hero lvl <= 5, RES test if attacking, fail: -10 CS/RS/AA.";
                case "Fear 6": return "Hero lvl <= 6, RES test if attacking, fail: -10 CS/RS/AA.";
                case "Fear 7": return "Hero lvl <= 7, RES test if attacking, fail: -10 CS/RS/AA.";
                case "Fear 10": return "Hero lvl <= 10, RES test if attacking, fail: -10 CS/RS/AA.";
                case "Fear of Elves": return "Must make RES test when attacking, fail: -10 CS/RS/AA.";
                case "Ferocious charge": return "DMG +1d4 on charge.";
                case "Fire breath": return "Does 1d10 fire damage in a straight or diagonal line for 4 squares, this attack is an automatic hit.";
                case "Fire damage": return "All attacks cause fire damage.";
                case "Floater": return "Immune to traps and pits.";
                case "Flyer": return "Move through squares with other models, ignore ZOC.";
                case "Free Bellow": return "All heroes must pass RES test or be stunned. The other head can still make a standard attack in the same action.";
                case "Frenzy": return "If it wounds a hero, it may make a standard attack immediately again for free.";
                case "Frost DMG": return "50% chance of stun on hit";
                case "Gust": return "All models or any models attacking a model in the same tile suffer from RS -15.";
                case "Ghostly touch": return "Cannot be parried, but can be dodged. No NA or armour. Make a RES test, on fail lose 1d8HP and -1 sanity.";
                case "Ghostly howl": return "All heros make a RES test, on fail lose 1d8HP and -1 sanity.";
                case "Hard as rock": return "Immune to ranged weapons, 50% damage from bladed weapons unless magic or Mithril.";
                case "Hate Dwarves": return "CS +5 against Dwarves. Targets Dwarves if possible";
                case "Just bones": return "Weapon projectiles DMG -2.";
                case "Kick": return "May make 1 attack on any hero standing behind the creature as a free attack, 1d10+2 DMG.";
                case "Large": return "Re-roll DMG. +10 to hit when shot at.";
                case "Leech": return "A successful attack will immobilize the target, and will only be able to attack this creature. The leech will automatically drain 1d4HP from the target at he beginning of each turn, and the target must make a CON tes or be diseased. There must be a CON roll for each leech attached. Any other hero attacking the leech deal 50% DMG, since they are being careful.";
                case "Magic being": return "Vanished when destroyed, no loot.";
                case "Magic User": return "May cast spells using RS as skill value.";
                case "Master of the Dead": return "If there are any undead minions on the table, even with 0HP, 1 of them gains full health. If there are no undead minions then the Vampire regains 1d6HP, if at full health instead treat as standard attack.";
                case "Multiple attacks 3": return "Every standard attack is replaced by 3 attacks. All attacks are on the same target with different CS rolls for each.";
                case "Perfect Hearing": return "Add +1 enemy initiative tokens.";
                case "Petrify": return "Random adjacent hero must pass RES test or be petrified for 1d6 turns. While petrified, the hero will not be targeted unless the last living. The hero cannot do anything for these turns.";
                case "Poisonous 1": return "Wounded hero, CON test or poisoned. If poisoned, for 1d10 turns CON test, if failed lose 1HP.";
                case "Poisonous 2": return "Wounded hero, CON test or poisoned. If poisoned, for 1d10 turns CON test, if failed lose 2HP.";
                case "Poisonous spit": return "RS attack for adjacent or 1 square away. Act like a normal ranged attack in regard to parry and dodge rules. Follow normal rules for poison if hit.";
                case "Psychic": return "All heroes automatically suffer RES -20 while the monster is alive. Not cumulative with another psychics.";
                case "Regenerate 2": return "1d2HP each turn.";
                case "Regenerate 3": return "1d3HP each turn.";
                case "Regenerate 6": return "1d6HP each turn.";
                case "Rend": return "If attack is successful, hero must make a STR test or receive an additional 1d6 DMG.";
                case "Riddle master": return "If the only models on the tile are Sphinx, then the party can make a WIS test to answer a riddle. If successful the sphinx will leave granting 150xp, if fail then battle will ensue.";
                case "Scurry": return "May move through all models and ZOC for free. Cannot stop in a square occupied by another model.";
                case "Seduction": return "RES test or be seduced. On fail lose all AP. Every turn after make a RES test to remove status.While seduced, the hero will not be targeted unless there are no other non-seduced heroes.";
                case "Silent": return "Perfect hearing has no effect when there is only silent enemies in the room.";
                case "Slow": return "Cannot move more than once per turn.";
                case "Sneaky": return "Add +1 enemy initiative token.";
                case "Stench": return "Adjacent hero's suffer CS -10.";
                case "Stupid": return "Roll 1d6, if 1 then inactive.";
                case "Summon children": return "Summons a giant spider on behavior roll of 5-6.";
                case "Swallow": return "A successful attack cannot be parried, but can be dodged. In the next turn the hero can attempt to break free with a STR test, if failed the hero can do nothing. The next turn the hero can make a final attempt to free themselves with 50% STR test, iof failed the hero is removed form the table. The creature will be inactive during this turn. The only way to free the hero is by killing the creature. After the hero is saved, the will be prone when put back on the board.";
                case "Sweeping strike": return "On a successful CS, all heroes in ZOC are pushed back 1 square along with any heros they run into.All heroes in ZOC also suffer 50% DMG unless pushed up against wall, then full DMG is received.In addition to be pushed back the heros must pass a DEX test or be knocked down unless falling into a pit then DEX test for the pit.";
                case "Terror 3": return "Hero lvl <= 3, RES test -20 if attacking or adjacent, fail: -10 CS/RS/AA and stunned for 1AP.";
                case "Terror 5": return "Hero lvl <= 5, RES test -20 if attacking or adjacent, fail: -10 CS/RS/AA and stunned for 1AP.";
                case "Terror 8": return "Hero lvl <= 8, RES test -20 if attacking or adjacent, fail: -10 CS/RS/AA and stunned for 1AP.";
                case "Terror 10": return "Hero lvl <= 10, RES test -20 if attacking or adjacent, fail: -10 CS/RS/AA and stunned for 1AP.";
                case "Tongue attack": return "RS attack against targets 1 square away. Fail to parry or dodge moves the target adjacent to the creature, changing place with any model in that square. If already adjacent then Swallow.";
                case "Wall crawler": return "May move on walls and ignore ZOC if near a wall.";
                case "Web": return "Target is immobilized, and must make a STR test and 1AP to free themselves.";
                case "XLarge": return "Re-roll DMG. +10 to hit when shot at.";
                default: return "";
            }
        }
    }
}