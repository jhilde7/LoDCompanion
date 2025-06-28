using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace LoDCompanion.Models
{
    // Make this class abstract as it serves as a base for specific equipment types.
    // Removed MonoBehaviour inheritance.
    public class Equipment
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty; // Default to empty string for safety
        [JsonPropertyName("encumbrance")]
        public float Encumbrance { get; set; } = 0f; // Default value for encumbrance, can be set in constructor
        [JsonPropertyName("max_durability")]
        public int MaxDurability { get; set; } = 6; // Default value from _Equipment.cs
        [JsonPropertyName("durability")]
        public int Durability { get; set; } = 6; // Default value from _Equipment.cs, can be set in constructor or modified later
        [JsonPropertyName("value")]
        public int Value { get; set; } = 0; // Default value, can be set in constructor or modified later
        [JsonPropertyName("sell_value")]
        public int SellValue { get; private set; } // Set internally, cannot be set from outside
        [JsonPropertyName("repair_cost")]
        public int RepairCost { get; private set; } // Set internally
        [JsonPropertyName("availability")]
        public int availability { get; set; } = 4;
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("is_magic")]
        public bool IsMagic { get; set; }
        [JsonPropertyName("magic_effect")]
        public string MagicEffect { get; set; } = string.Empty;

        // Constructor

        public Equipment() { }
        // Constructor to set initial values and trigger initial calculations
        public Equipment(string name, float encumbrance, int value, int quantity, string description, bool isMagic)
        {
            Name = name;
            Encumbrance = encumbrance;
            Value = value;
            Quantity = quantity;
            Description = description;
            IsMagic = isMagic;
        }

        // Method to calculate sale price and repair cost, called when relevant properties change
        // This replaces the logic previously in Unity's Update() and Start()
        public void CalculateInitialRepairCosts()
        {
            // Initial repair cost calculation (from original Update logic)
            // Only calculate if not already set or explicitly needed.
            if (RepairCost == 0 && Value > 0)
            {
                RepairCost = (int)Math.Floor(Value * 0.2f);
            }

            CalculateSalePrice();
        }

        // Method to calculate the sell value of the equipment
        // Removed Unity's GetComponent and direct references to other classes.
        // Logic for specific item names like "Nightstalker" or "Dragon Scale" determining maxDurability
        // should ideally be handled at the point of item creation or in a more structured way,
        // but for now, it's adapted from the original `CalculateSalePrice`.
        public void CalculateSalePrice()
        {
            int currentMaxDurability = MaxDurability; // Use the property that might be set by subclasses

            // Adapt specific item name logic here if needed, or better, in specific item classes.
            // This part is derived directly from the original logic for `maxDurability` in `Equipment.cs`.
            if (Name != null) // Null check for safety
            {
                if (Name.Contains("Nightstalker"))
                {
                    currentMaxDurability = 8;
                }
                else if (Name.Contains("Dragon Scale"))
                {
                    currentMaxDurability = 10;
                }
            }


            // If quantity is 0 or less, or if durability is 0, or value is too low, it's worthless.
            if (Quantity <= 0 || Durability <= 0 || Value <= 10)
            {
                SellValue = 0;
                return;
            }

            // Calculate sale value based on durability remaining
            int durabilityDifference = currentMaxDurability - Durability;

            switch (durabilityDifference)
            {
                case 0:
                    SellValue = (int)Math.Floor(Value * 0.7f);
                    break;
                case 1:
                    SellValue = (int)Math.Floor(Value * 0.6f);
                    break;
                case 2:
                    SellValue = (int)Math.Floor(Value * 0.5f);
                    break;
                case 3:
                    SellValue = (int)Math.Floor(Value * 0.4f);
                    break;
                case 4:
                    SellValue = (int)Math.Floor(Value * 0.3f);
                    break;
                default: // For differences > 4 (e.g., 5 or more)
                    SellValue = (int)Math.Floor(Value * 0.2f);
                    break;
            }
        }
    }

    public enum AmmoType
    {
        SlingStone,
        Arrow,
        Bolt
    }

    public class Ammo : Equipment
    {
        [JsonPropertyName("type")]
        public AmmoType Type { get; set; } = AmmoType.Arrow; // Default ammo type, can be set in constructor
        [JsonPropertyName("is_silver")]
        public bool IsSilver { get; set; }
        [JsonPropertyName("is_barbed")]
        public bool IsBarbed { get; set; }
        [JsonPropertyName("is_sup_slingstone")]
        public bool IsSupSlingstone { get; set; }

        public Ammo() { } // Default constructor
        public Ammo(string name, float encumbrance, int value, int quantity, string description, AmmoType ammoType, bool isMagic = false,
                    bool isSilver = false, bool isBarbed = false, bool isSupSlingstone = false)
            : base(name, encumbrance, value, quantity, description, isMagic)
        {
            Type = ammoType;
            IsSilver = isSilver;
            IsBarbed = isBarbed;
            IsSupSlingstone = isSupSlingstone;
            MaxDurability = 1; // Ammo is usually consumed or has very low durability
            Durability = 1;
        }
    }

    public abstract class Weapon : Equipment
    {
        [JsonPropertyName("weapon_class")]
        public int WeaponClass { get; set; }
        [JsonPropertyName("damage_range")]
        public int[] DamageRange { get; set; } = new int[2]; // e.g., {1, 6} for 1d6
        [JsonPropertyName("armour_piercing")]
        public int ArmourPiercing { get; set; } // AP value

        public Weapon() { } // Default constructor
        public Weapon(string name, float encumbrance, int value, int weaponClass, int[] damageRange, bool isMagic = false)
            : base(name, encumbrance, value, 1, "", isMagic)
        {
            // Initialize Weapon-specific properties
            WeaponClass = weaponClass;
            DamageRange = damageRange;
            ArmourPiercing = 0;
        }

        // You can add common weapon methods here, e.g., for calculating damage roll
        public virtual int RollDamage()
        {
            // This would use your RandomHelper.Roll method
            // For now, a placeholder using System.Random
            if (DamageRange[0] > DamageRange[1])
            {
                // Swap if min is greater than max
                int temp = DamageRange[0];
                DamageRange[0] = DamageRange[1];
                DamageRange[1] = temp;
            }
            if (DamageRange[0] == 0 && DamageRange[1] == 0) return 0;

            // Placeholder for System.Random, would be replaced by RandomHelper
            Random rand = new Random();
            return rand.Next(DamageRange[0], DamageRange[1] + 1);
        }
    }

    public class MeleeWeapon : Weapon
    {
        [JsonPropertyName("is_melee_weapon")]
        public bool IsMeleeWeapon { get; set; } = true;
        [JsonPropertyName("is_silver")]
        public bool IsSilver { get; set; }
        [JsonPropertyName("is_mithril")]
        public bool IsMithril { get; set; }
        [JsonPropertyName("dual_wield_bonus")]
        public int DualWieldBonus { get; set; } = 0;
        [JsonPropertyName("is_bfo")]
        public bool IsBFO { get; set; } // Big Freaking Object
        [JsonPropertyName("is_slow")]
        public bool IsSlow { get; set; }
        [JsonPropertyName("is_stun")]
        public bool IsStun { get; set; }
        [JsonPropertyName("is_unwieldly")]
        public bool IsUnwieldly { get; set; }
        [JsonPropertyName("is_first_hit")]
        public bool IsFirstHit { get; set; }
        [JsonPropertyName("is_fast")]
        public bool IsFast { get; set; }
        [JsonPropertyName("is_defensive")]
        public bool IsDefensive { get; set; }
        [JsonPropertyName("is_ensnare")]
        public bool IsEnsnare { get; set; }
        [JsonPropertyName("is_reach")]
        public bool IsReach { get; set; }
        [JsonPropertyName("is_edged")]
        public bool IsEdged { get; set; }
        [JsonPropertyName("is_slayer_treated")]
        public bool IsSlayerTreated { get; set; }
        [JsonPropertyName("is_axe")]
        public bool IsAxe { get; set; }
        [JsonPropertyName("is_sword")]
        public bool IsSword { get; set; }
        [JsonPropertyName("is_blunt")]
        public bool IsBlunt { get; set; }
        [JsonPropertyName("is_metal")]
        public bool IsMetal { get; set; }

        public MeleeWeapon() { } // Default constructor
        public MeleeWeapon(string name, float encumbrance, int value, int weaponClass, int[] damageRange, bool isMithril = false, bool isMagic = false)
            : base(name, encumbrance, value, weaponClass, damageRange, isMagic)
        {
            // Default constructor
            IsMithril = isMithril;
            if (IsMithril)
            {
                Encumbrance -= 1; // Adjust encumbrance if Mithril
            }
        }
    }

    public class MagicStaff : MeleeWeapon
    {
        [JsonPropertyName("staff_type")]
        public string StaffType { get; set; } = string.Empty;
        [JsonPropertyName("arcane_arts_bonus")]
        public int ArcaneArtsSkillModifier { get; set; }
        [JsonPropertyName("contained_spell")]
        public string ContainedSpell { get; set; } = string.Empty;
        [JsonPropertyName("mana_storage")]
        public int ManaStorage { get; set; }
        [JsonPropertyName("hp_bonus")]
        public int HPBonus { get; set; }

    }

    public class RangedWeapon : Weapon
    {
        public Ammo Ammo { get; set; } = new Ammo(); // Default to an empty Ammo object
        [JsonPropertyName("elven_bowstring")]
        public bool ElvenBowstring { get; set; }
        [JsonPropertyName("aim_attachment")]
        public bool AimAttachment { get; set; }
        [JsonPropertyName("is_secondary_weapon")]
        public bool IsSecondaryWeapon { get; set; }
        [JsonPropertyName("reload_time")]
        public int ReloadTime { get; set; } = 1;
        public bool IsLoaded { get; set; } = false;

        public RangedWeapon() { } // Default constructor
        public RangedWeapon(string name, float encumbrance, int value, int weaponClass, int[] damageRange, Ammo ammo, bool isMagic = false)
            : base(name, encumbrance, value, weaponClass, damageRange, isMagic)
        {
            // Initialize RangedWeapon-specific properties
            Ammo = ammo;
            ReloadTime = 1; // Default reload time
        }

        // You might add methods specific to ranged weapons here, e.g., to consume ammo
        public bool ConsumeAmmo(int quantity = 1)
        {
            if (Ammo != null && Ammo.Quantity >= quantity)
            {
                Ammo.Quantity -= quantity;
                IsLoaded = false;
                return true;
            }
            return false;
        }

        public void reloadAmmo()
        {
            if (Ammo != null && Ammo.Quantity > 0)
            {
                IsLoaded = true;
            }
        }
    }

    public abstract class _EquipmentArmour : Equipment
    {
        [JsonPropertyName("armour_class")]
        public int ArmourClass { get; set; }
        [JsonPropertyName("def_value")]
        public int DefValue { get; set; }
        [JsonPropertyName("is_mithril")]
        public bool IsMithril { get; set; }
        [JsonPropertyName("is_metal")]
        public bool IsMetal { get; set; }

        public _EquipmentArmour() { } // Default constructor
        public _EquipmentArmour(string name, string description, float encumbrance, int value, int durability, int armourClass, int defValue, bool isMithril = false, bool isMetal = false, bool isMagic = false)
            : base(name, encumbrance, value, 1, description, isMagic)
        {
            ArmourClass = armourClass;
            DefValue = defValue;
            Durability = durability;
            IsMithril = isMithril;
            IsMetal = isMetal;
            // Calculate initial repair costs
            CalculateInitialRepairCosts();
        }
    }

    public class Armour : _EquipmentArmour
    {
        [JsonPropertyName("is_head")]
        public bool IsHead { get; set; }
        [JsonPropertyName("is_torso")]
        public bool IsTorso { get; set; }
        [JsonPropertyName("is_arms")]
        public bool IsArms { get; set; }
        [JsonPropertyName("is_legs")]
        public bool IsLegs { get; set; }
        [JsonPropertyName("is_cloak")]
        public bool IsCloak { get; set; }

        [JsonPropertyName("is_stackable")]
        public bool IsStackable { get; set; }
        [JsonPropertyName("is_clunky")]
        public bool IsClunky { get; set; }
        [JsonPropertyName("is_upgraded")]
        public bool IsUpgraded { get; set; }
        [JsonPropertyName("is_nightstalker")]
        public bool IsNightstalker { get; set; }
        [JsonPropertyName("is_dragon_scale")]
        public bool IsDragonScale { get; set; }

        public Armour() { } // Default constructor
        public Armour(string name, string description, float encumbrance, int value, bool isMagic, int armourClass, int defValue, bool isMithril = false, bool isMetal = false)
            : base(name, description, encumbrance, value, 1, armourClass, defValue, isMithril, isMetal, isMagic)
        {
            SetMithrilModifier();
        }

        public void SetMithrilModifier()
        {
            if (IsMithril)
            {
                DefValue += 1; // Increase defense value if Mithril
                Encumbrance -= 1; // Decrease encumbrance if Mithril
            }
        }
    }

    public class Shield : _EquipmentArmour
    {
        [JsonPropertyName("is_shield")]
        public bool IsShield { get; set; } = true; // Default to true for Shield class
        [JsonPropertyName("is_huge")]
        public bool IsHuge { get; set; }

        public Shield() { } // Default constructor

        public Shield(string name, string description, float encumbrance, int value, bool isMagic, int armourClass, int defValue, bool isMithril = false, bool isMetal = false)
            : base(name, description, encumbrance, value, 1, armourClass, defValue, isMithril, isMetal, isMagic)
        {

        }

    }
}