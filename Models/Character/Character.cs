
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Utilities;
using System.Diagnostics;
using System.Text;

namespace LoDCompanion.Models.Character
{
    public class Character
    {
        internal readonly string Id;
        public string Name { get; set; } = string.Empty; // Default to empty string for safety
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int Strength { get; set; }
        public int Constitution { get; set; }
        public int Dexterity { get; set; }
        public int Wisdom { get; set; }
        public int Resolve { get; set; }
        public int CombatSkill { get; set; }
        public int RangedSkill { get; set; }
        public int Dodge { get; set; }
        public int Level { get; set; }
        public int NaturalArmour { get; set; }
        public int DamageBonus { get; set; }
        public GridPosition Position { get; set; } = new GridPosition(0, 0, 0);
        public List<ActiveStatusEffect> ActiveStatusEffects { get; set; } = new List<ActiveStatusEffect>(); // e.g., "Normal", "Poisoned", "Diseased"
        public int MaxAP { get; set; } = 2;
        public int CurrentAP { get; set; } = 2;
        public bool IsLarge { get; set; }
        public bool IsVulnerableAfterPowerAttack { get; set; }

        // Constructor (optional, but good practice for initialization)
        public Character()
        {
            Id = Guid.NewGuid().ToString();
            CurrentHP = MaxHP;
        }

        // Common methods for all characters
        public virtual void TakeDamage(int damage)
        {
            CurrentHP -= damage;
            if (CurrentHP < 0)
            {
                CurrentHP = 0;
            }
        }

        public void ResetActionPoints()
        {
            this.CurrentAP = MaxAP;
        }

        public bool CanAct()
        {
            return this.CurrentAP > 0;
        }

        public void SpendActionPoints(int amount)
        {
            this.CurrentAP -= amount;
            if (this.CurrentAP < 0)
            {
                this.CurrentAP = 0;
            }
        }
    }

    /// <summary>
    /// Represents the tactical stance of a character in combat.
    /// </summary>
    public enum CombatStance
    {
        Normal,
        Parry,
        Overwatch
    }

    public class Hero : Character
    {

        // Basic Hero Information
        public string SpeciesName { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public int Experience { get; set; }
        public int Luck { get; set; }
        public int MaxEnergy { get; set; } = 1;
        public int CurrentEnergy { get; set; } = 1;
        public int? MaxMana { get; set; }
        public int? CurrentMana { get; set; }
        public int MaxSanity { get; set; } = 10;
        public int CurrentSanity { get; set; } = 10;

        // Skills (could be part of a separate Skill collection if complex, but keeping here for now)
        public int PickLocksSkill { get; set; }
        public int BarterSkill { get; set; }
        public int HealSkill { get; set; }
        public int AlchemySkill { get; set; }
        public int PerceptionSkill { get; set; }
        public int ArcaneArtsSkill { get; set; }
        public int ForagingSkill { get; set; }
        public int BattlePrayersSkill { get; set; }

        // Hero-specific States and Flags
        public int MaxArmour { get; set; }
        public bool IsThief { get; set; } // Indicates if profession is Thief
        public bool HasLantern { get; set; }
        public bool HasTorch { get; set; }
        public bool IsWeShaltNotFalter { get; set; } // Specific buff/debuff

        // Collections of Hero-specific items/abilities
        public List<Talent> Talents { get; set; } = new List<Talent>();
        public List<Perk> Perks { get; set; } = new List<Perk>();
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        public List<Armour> Armours { get; set; } = new List<Armour>();
        public Shield? Shield { get; set; }
        public List<Equipment> QuickSlots { get; set; } = new List<Equipment>();
        public List<Equipment> Backpack { get; set; } = new List<Equipment>();

        public CombatStance Stance { get; set; } = CombatStance.Normal;
        public bool HasDodgedThisBattle { get; set; } = false;
        public List<Spell> Spells { get; set; } = new List<Spell>();
        public List<Prayer> Prayers { get; set; } = new List<Prayer>();
        public int Coins { get; set; } = 150;

        // Constructor
        public Hero() : base()
        {

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"--- Hero: {Name} (Level {Level} {SpeciesName} {ProfessionName}) ---");
            sb.AppendLine($"HP: {CurrentHP}/{MaxHP}, Sanity: {CurrentSanity}/{MaxSanity}, Energy: {CurrentEnergy}/{MaxEnergy}, XP: {Experience}, Coins: {Coins}");
            if (MaxMana.HasValue) sb.AppendLine($"Mana: {CurrentMana}/{MaxMana}");

            sb.AppendLine("\n-- Stats --");
            sb.AppendLine($"STR: {Strength}, CON: {Constitution}, DEX: {Dexterity}, WIS: {Wisdom}, RES: {Resolve}");
            sb.AppendLine($"NA: {NaturalArmour}, DB: {DamageBonus}, Luck: {Luck}");

            sb.AppendLine("\n-- Skills --");
            sb.AppendLine($"Combat Skill: {CombatSkill}, Ranged Skill: {RangedSkill}, Dodge: {Dodge}");
            sb.AppendLine($"Pick Locks: {PickLocksSkill}, Barter: {BarterSkill}, Heal: {HealSkill}, Alchemy: {AlchemySkill}");
            sb.AppendLine($"Perception: {PerceptionSkill}, Arcane Arts: {ArcaneArtsSkill}, Foraging: {ForagingSkill}, Battle Prayers: {BattlePrayersSkill}");

            if (Talents.Any()) sb.AppendLine($"\n-- Talents --\n{string.Join(", ", Talents.Select(t => t.Name))}");
            if (Perks.Any()) sb.AppendLine($"\n-- Perks --\n{string.Join(", ", Perks.Select(p => p.Name))}");
            if (Spells.Any()) sb.AppendLine($"\n-- Spells --\n{string.Join(", ", Spells.Select(s => s.Name))}");
            if (Prayers.Any()) sb.AppendLine($"\n-- Prayers --\n{string.Join(", ", Prayers.Select(p => p.Name))}");
            if (Backpack.Any()) sb.AppendLine($"\n-- Backpack --\n{string.Join(", ", Backpack.Select(e => e.Name))}");

            return sb.ToString();
        }

        public bool ResistDisease(int? roll = null)
        {
            // This method would use a RandomHelper service or static method now
            if (roll == null)
            {
                roll = RandomHelper.RollDie("D100");
            }
            int con = Constitution;

            // Apply talent bonuses
            foreach (Talent talent in Talents)
            {
                if (talent.IsResistDisease)
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
            int con = Constitution;

            foreach (var talent in Talents)
            {
                if (talent.IsResistPoison)
                {
                    con += 10;
                }
            }

            return (roll <= con);
        }

        /// <summary>
        /// Gets the current total armour class from equipped armours and shields.
        /// </summary>
        /// <returns>The total armour class.</returns>
        public int GetTotalArmourClass()
        {
            int totalAC = 0;
            foreach (var armour in Armours)
            {
                totalAC += armour.ArmourClass;
            }
            if (Shield != null)
            {
                totalAC += Shield.ArmourClass;
            }
            return totalAC;
        }

        // Method to get current weapon for combat. HeroWeapon.cs had complex logic
        // This simplified approach assumes the first weapon in the list is the "active" one
        // or a dedicated 'EquippedWeapon' property would be better
        public Equipment? GetEquippedWeapon()
        {
            if (Weapons.Count > 0)
            {
                return Weapons[0]; // Simple, assumes the primary equipped weapon is at index 0
            }
            return null; // No weapon equipped
        }
    }

    public enum MonsterBehaviorType
    {
        HumanoidMelee,
        Ranged,
        MagicUser,
        Beast,
        LowerUndead,
        HigherUndead
    }

    public class Monster : Character // Inherit from the new Character base class
    {
        private readonly GameDataService _gameData;
        public string Type { get; set; } = string.Empty;
        public int ArmourValue { get; set; }
        public bool HasShield { get; set; } // Indicates if the monster has a shield
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public bool HasSpecialAttack { get; set; }
        public bool IsGhost { get; set; }
        public int ToHitPenalty { get; set; } 
        public int Move { get; set; }
        public int XP { get; set; }
        public List<string> SpecialRules { get; set; } = new List<string>(); // List of raw rule names
        public List<string> SpecialRuleDescriptions { get; private set; } = new List<string>(); // List of formatted descriptions
        public bool IsUndead { get; set; }
        public List<string> Spells { get; set; } = new List<string>(); // List of actual spell names
        public List<Weapon> Weapons { get; set; } = new List<Weapon>(); // List of Monster Weapon objects
        public Corpse Body { get; set; }
        public string TreasureType { get; set; } = "-"; // Default value indicating no treasure type assigned
        public List<string> Treasures { get; set; } = new List<string>();
        public MonsterBehaviorType Behavior { get; set; } = MonsterBehaviorType.HumanoidMelee;

        public Monster(GameDataService gameData) : base()
        {
            _gameData = gameData;
            Body = new Corpse(_gameData, TreasureType); // Initialize the Body with the default TreasureType
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"--- Monster: {Name} [{Type}] ---");
            sb.AppendLine($"HP: {CurrentHP}/{MaxHP}, Armour: {ArmourValue}, Move: {Move}, XP: {XP}");

            sb.AppendLine("\n-- Combat Stats --");
            sb.AppendLine($"CS: {CombatSkill}, RS: {RangedSkill}, Dodge: {Dodge}, To Hit Bonus: {ToHitPenalty}");
            sb.AppendLine($"Damage: {MinDamage}-{MaxDamage}, DB: {DamageBonus}");

            if (Weapons.Any())
            {
                sb.AppendLine("\n-- Weapons --");
                foreach (var weapon in Weapons)
                {
                    sb.AppendLine($"- {weapon.Name} (Damage: {weapon.MinDamage} - {weapon.MaxDamage})");
                }
            }

            if (SpecialRules.Any()) sb.AppendLine($"\n-- Special Rules --\n{string.Join(", ", SpecialRules)}");
            if (Spells.Any()) sb.AppendLine($"\n-- Spells --\n{string.Join(", ", Spells)}");

            return sb.ToString();
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

        /// <summary>
        /// Populates the Spells list by looking up actual spell names based on initial spell types.
        /// Requires an instance of SpellLookupService.
        /// </summary>
        /// <param name="spellLookupService">The service used to lookup spell names.</param>
        public void DesignateSpells()
        {

            List<string> returnedSpells = new List<string>();
            foreach (string spellType in Spells) // Assuming 'Spells' initially holds spell types/categories
            {
                string returnedSpell = "";
                int attempts = 0; // Prevent infinite loops
                const int maxAttempts = 10;
                do
                {
                    returnedSpell = _gameData.GetRandomSpellNameByCategory(spellType, IsUndead);
                    attempts++;
                }
                while (returnedSpells.Contains(returnedSpell) && attempts < maxAttempts);

                if (!string.IsNullOrEmpty(returnedSpell) && !returnedSpells.Contains(returnedSpell))
                {
                    returnedSpells.Add(returnedSpell);
                }
            }
            Spells = returnedSpells; // Update the actual Spells list with designated spells
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