using LoDCompanion.Services.Player;
using LoDCompanion.Utilities;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace LoDCompanion.Models
{
    public enum ShopCategory
    {
        General,
        Weapons,
        Armour,
        Shields,
        Potions,
    }

    public class Equipment
    {
        public string Category { get; set; } = "Common";
        public ShopCategory Shop { get; set; } = ShopCategory.General;
        public ItemSlot? ItemSlot { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Encumbrance { get; set; }
        public int MaxDurability { get; set; } = 6;
        public int Durability { get; set; } = 1;
        public double Value { get; set; } = 0; 
        public int SellValue => CalculateSalePrice();
        public int RepairCost => CalculateInitialRepairCosts();
        public int Availability { get; set; } = 4;
        public int Quantity { get; set; } = 1;
        public string Description { get; set; } = string.Empty;
        public string MagicEffect { get; set; } = string.Empty;

        public Equipment() 
        {

        }

        public Equipment(Equipment template)
        {
            // Copy values from the template
            this.Category = template.Category;
            this.Shop = template.Shop;
            this.Name = template.Name;
            this.Encumbrance = template.Encumbrance;
            this.Value = template.Value;
            this.Availability = template.Availability;
            this.MaxDurability = template.MaxDurability;
            this.Durability = template.Durability;
            this.Quantity = template.Quantity;
            this.Description = template.Description;
            this.MagicEffect = template.MagicEffect;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Category}] {Name} | ");
            sb.Append($"Value: {Value} | Dur: {Durability}/{MaxDurability}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.Append($" | Effect: {MagicEffect}");
            }
            return sb.ToString();
        }
        public string GetDisplayName()
        {
            if (Quantity > 1)
            {
                return $"{Name} (x{Quantity})";
            }

            return Name;
        }

        public int CalculateInitialRepairCosts()
        {
            if (Value > 0)
            {
                return (int)Math.Floor(Value * 0.2f);
            }
            else
            {
                return 0;
            }
        }

        public int CalculateSalePrice()
        {
            int currentMaxDurability = MaxDurability;

            if (Quantity <= 0 || Durability <= 0 || Value <= 10)
            {
                return 0;
            }

            // Calculate sale value based on durability remaining
            int durabilityDifference = currentMaxDurability - Durability;

            switch (durabilityDifference)
            {
                case 0:
                    return (int)Math.Floor(Value * 0.7f);
                case 1:
                    return (int)Math.Floor(Value * 0.6f);
                case 2:
                    return (int)Math.Floor(Value * 0.5f);
                case 3:
                    return (int)Math.Floor(Value * 0.4f);
                case 4:
                    return (int)Math.Floor(Value * 0.3f);
                default:
                    return (int)Math.Floor(Value * 0.2f);
            }
        }
    }

    public enum AmmoType
    {
        SlingStone,
        Arrow,
        Bolt
    }

    public enum AmmoProperty
    {
        Silver,
        Barbed,
        SupuriorSlingStone,
        HolyWater
    }

    public class Ammo : Equipment
    {
        public AmmoType AmmoType { get; set; } = AmmoType.Arrow; // Default ammo type, can be set in constructor
        public Dictionary<AmmoProperty, int> Properties { get; set; } = new Dictionary<AmmoProperty, int>();

        public Ammo() { } // Default constructor

        public Ammo(Ammo template)
        {
            // Copy values from the template
            this.Category = template.Category;
            this.Shop = template.Shop;
            this.Name = template.Name;
            this.Encumbrance = template.Encumbrance;
            this.Value = template.Value;
            this.Availability = template.Availability;
            this.MaxDurability = template.MaxDurability;
            this.Durability = template.Durability;
            this.Quantity = template.Quantity;
            this.Description = template.Description;
            this.MagicEffect = template.MagicEffect;
            // Create a NEW dictionary instance with the same properties
            this.Properties = new Dictionary<AmmoProperty, int>(template.Properties);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append($"[{Category}] {Name} | ");
            sb.Append($"Value: {Value} | Dur: {Durability}/{MaxDurability}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.Append($" | Effect: {MagicEffect}");
            }
            sb.Append($" | Ammo Category: {AmmoType}");
            if (HasProperty(AmmoProperty.Silver)) sb.Append(", Silver");
            if (HasProperty(AmmoProperty.Barbed)) sb.Append(", Barbed");
            if (HasProperty(AmmoProperty.SupuriorSlingStone)) sb.Append(", Superior");
            if (HasProperty(AmmoProperty.HolyWater)) sb.Append(", Holy Water added");
            return sb.ToString();
        }

        public bool HasProperty(AmmoProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(AmmoProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }
    }

    public enum WeaponProperty
    {
        Silver,
        Mithril,
        DualWield,
        BFO,
        Slow,
        Stun,
        Unwieldly,
        FirstHit,
        Fast,
        Defensive,
        Ensnare,
        Reach,
        Edged,
        SlayerTreated,
        Axe,
        Sword,
        Blunt,
        Metal,
        Magic,
        SecondaryWeapon
    }

    public class Weapon : Equipment
    {
        public bool IsRanged { get; set; }
        public bool IsMelee { get; set; }
        public new int Durability { get; set; } = 6;
        public int Class { get; set; }
        public string? DamageDice { get; set; }
        public int DamageBonus { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int ArmourPiercing { get; set; }
        public virtual Dictionary<WeaponProperty, int> Properties { get; set; } = new Dictionary<WeaponProperty, int>();

        public virtual int RollDamage()
        {
            return RandomHelper.GetRandomNumber(MinDamage, MaxDamage);
        }

    }

    public class MeleeWeapon : Weapon
    {
        private bool HasAppliedSlayerModifier { get; set; }
        private Dictionary<WeaponProperty, int> _properties = new Dictionary<WeaponProperty, int>();

        public override Dictionary<WeaponProperty, int> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                SetMithrilModifier();
            }
        }

        public MeleeWeapon() 
        {
            IsMelee = true;
        } 

        public MeleeWeapon(MeleeWeapon baseWeapon)
        {
            IsMelee = true;
            // Copy values from the base weapon
            this.Category = baseWeapon.Category;
            this.Shop = baseWeapon.Shop;
            this.Name = baseWeapon.Name;
            this.Class = baseWeapon.Class;
            this.MinDamage = baseWeapon.MinDamage;
            this.MaxDamage = baseWeapon.MaxDamage;
            this.ArmourPiercing = baseWeapon.ArmourPiercing;
            this.Value = baseWeapon.Value;
            this.Encumbrance = baseWeapon.Encumbrance;
            this.Durability = baseWeapon.Durability;
            this.DamageDice = baseWeapon.DamageDice;
            this.DamageBonus = baseWeapon.DamageBonus;
            // Create a NEW dictionary instance with the same properties
            this.Properties = new Dictionary<WeaponProperty, int>(baseWeapon.Properties);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage} | AP: {ArmourPiercing}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Key == WeaponProperty.DualWield)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"Dual Wield: +{prop.Value}");
                    }
                    else
                    {
                        // Simple properties
                        propsAsStrings.Add(prop.Key.ToString());
                    }
                }
                sb.Append(" | Properties: ").Append(string.Join(", ", propsAsStrings));
            }

            return sb.ToString();
        }

        private void SetMithrilModifier()
        {
            if(HasProperty(WeaponProperty.Mithril))
            {
                MinDamage += 1;
                MaxDamage += 1;
                DamageBonus += 1;
                Encumbrance -= 2;
            }
        }

        public void SetSlayerTreatedModifier()
        {
            if(HasProperty(WeaponProperty.SlayerTreated))
            {
                MinDamage += 1;
                MaxDamage += 1;
                DamageBonus += 1;
                HasAppliedSlayerModifier = true;
            }
        }

        public bool HasProperty(WeaponProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(WeaponProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }
    }

    public enum MagicStaffProperty
    {
        ArcaneArts,
        ManaStorage,
        HitPointsBonus,
        Illumination
    }

    public class MagicStaff : MeleeWeapon
    {
        public string StaffType { get; set; } = string.Empty;
        public string ContainedSpell { get; set; } = string.Empty;
        public Dictionary<MagicStaffProperty, int> MagicStaffProperties { get; set; } = new Dictionary<MagicStaffProperty, int>();

        public MagicStaff() { }

        public MagicStaff(MagicStaff template)
        {
            // Copy values from the template
            this.Category = template.Category;
            this.Shop = template.Shop;
            this.Name = template.Name;
            this.Class = template.Class;
            this.MinDamage = template.MinDamage;
            this.MaxDamage = template.MaxDamage;
            this.ArmourPiercing = template.ArmourPiercing;
            this.Value = template.Value;
            this.Encumbrance = template.Encumbrance;
            this.Durability = template.Durability;
            this.MagicEffect = template.MagicEffect;
            // Create a NEW dictionary instance with the same properties
            this.Properties = new Dictionary<WeaponProperty, int>(template.Properties);
            this.StaffType = template.StaffType;
            this.ContainedSpell = template.ContainedSpell;
            this.MagicStaffProperties = new Dictionary<MagicStaffProperty, int>(template.MagicStaffProperties);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage} | AP: {ArmourPiercing}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Key == WeaponProperty.DualWield)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"Dual Wield: +{prop.Value}");
                    }
                    else
                    {
                        // Simple properties
                        propsAsStrings.Add(prop.Key.ToString());
                    }
                }
                sb.Append(" | Properties: ").Append(string.Join(", ", propsAsStrings));
            }
            return sb.ToString();
        }

    }

    public class RangedWeapon : Weapon
    {
        public AmmoType AmmoType { get; set; } = AmmoType.Arrow;
        public Ammo Ammo { get; set; } = new Ammo();
        public bool ElvenBowstring { get; set; }
        public bool AimAttachment { get; set; }
        public int ReloadTime { get; set; } = 1;
        public bool IsLoaded { get; set; } = false;

        public RangedWeapon() 
        {
            IsRanged = true;
        } 

        public RangedWeapon(RangedWeapon template)
        {
            IsRanged = true;
            // --- Properties from Searchable ---
            this.Name = template.Name;
            // --- Properties from Equipment ---
            this.Description = template.Description;
            this.Class = template.Class;
            this.Durability = template.Durability;
            this.Encumbrance = template.Encumbrance;
            this.Value = template.Value;
            // --- Properties from Weapon ---
            this.MinDamage = template.MinDamage;
            this.MaxDamage = template.MaxDamage;
            this.DamageDice = template.DamageDice;
            this.DamageBonus = template.DamageBonus;
            this.ArmourPiercing = template.ArmourPiercing;
            // --- Properties from RangedWeapon ---
            this.AmmoType = template.AmmoType;
            this.Ammo = template.Ammo;
            this.ElvenBowstring = template.ElvenBowstring;
            this.AimAttachment = template.AimAttachment;
            this.IsSecondaryWeapon = template.IsSecondaryWeapon;
            this.ReloadTime = template.ReloadTime;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage} | AP: {ArmourPiercing}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");
            sb.AppendLine($" | Ammo Category: {AmmoType} | Reload Time: {ReloadTime} AP | Loaded: {IsLoaded}");
            if (ElvenBowstring) sb.Append(" | Elven Bowstring");
            if (AimAttachment) sb.Append(" | Aim Attachment");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }
            return sb.ToString();
        }

        public bool IsSlingUsingNormalAmmo()
        {
            if (AmmoType == AmmoType.SlingStone && Ammo != null && !Ammo.HasProperty(AmmoProperty.SupuriorSlingStone))
            {
                return true;
            }
            return false;
        }

        public bool ConsumeAmmo(int quantity = 1)
        {
            if (IsSlingUsingNormalAmmo())
            {
                IsLoaded = false;
                return false;
            }

            if (Ammo != null && Ammo.Quantity >= quantity)
            {
                Ammo.Quantity -= quantity;
                IsLoaded = false;
                return true;
            }
            else
            {
                IsLoaded = false;
                return true;
            }
        }

        public void reloadAmmo()
        {
            if (Ammo != null && Ammo.Quantity > 0 || IsSlingUsingNormalAmmo())
            {
                IsLoaded = true;
            }
        }
    }

    public enum ArmourProperty
    {
        Mithril,
        Metal,
        Head,
        Torso,
        Arms,
        Legs,
        Cloak,
        Stackable,
        Clunky,
        Upgraded,
        DarkAsTheNight,
        DragonScale,
        Dog,
        Magic
    }

    public class Armour : Equipment
    {
        new int Durability { get; set; } = 6;
        public int ArmourClass { get; set; }
        public int DefValue { get; set; }
        private Dictionary<ArmourProperty, int> _properties = new Dictionary<ArmourProperty, int>();

        public Dictionary<ArmourProperty, int> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                SetMithrilModifier();
            }
        }

        public Armour()
        {
            
        }

        public Armour(Armour template)
        {
            // Copy values from the template
            this.Category = template.Category;
            this.Shop = template.Shop;
            this.Name = template.Name;
            this.ArmourClass = template.ArmourClass;
            this.DefValue = template.DefValue;
            this.Encumbrance = template.Encumbrance;
            this.Value = template.Value;
            this.Availability = template.Availability;
            this.Durability = template.Durability;
            // Create a NEW dictionary instance with the same properties
            this.Properties = new Dictionary<ArmourProperty, int>(template.Properties);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Category}] {Name} | ");
            sb.AppendLine($"Class: {ArmourClass} | DEF: {DefValue}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }

            var coveredAreas = new List<string>();
            if (HasProperty(ArmourProperty.Head)) coveredAreas.Add("Head");
            if (HasProperty(ArmourProperty.Torso)) coveredAreas.Add("Torso");
            if (HasProperty(ArmourProperty.Arms)) coveredAreas.Add("Arms");
            if (HasProperty(ArmourProperty.Legs)) coveredAreas.Add("Legs");
            if (HasProperty(ArmourProperty.Cloak)) coveredAreas.Add("Back");
            if (coveredAreas.Any())
            {
                sb.AppendLine($"Covers: {string.Join(", ", coveredAreas)}");
            }
            return sb.ToString();
        }

        public void SetMithrilModifier()
        {
            if (HasProperty(ArmourProperty.Mithril))
            {
                DefValue += 1; // Increase defense value if Mithril
                Encumbrance -= 1; // Decrease encumbrance if Mithril
            }
        }

        public bool HasProperty(ArmourProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(ArmourProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }
    }

    public enum ShieldProperty
    {
        Mithril,
        Metal,
        Huge
    }

    public class Shield : Equipment
    {
        new int Durability { get; set; } = 6;
        public int WeaponClass { get; set; }
        public int DefValue { get; set; }
        private Dictionary<ShieldProperty, int> _properties = new Dictionary<ShieldProperty, int>();

        public Dictionary<ShieldProperty, int> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                SetMithrilModifier();
            }
        }

        public Shield() 
        {

        }

        public Shield(Shield template)
        {
            // Copy values from the template
            this.Category = template.Category;
            this.Shop = template.Shop;
            this.Name = template.Name;
            this.DefValue = template.DefValue;
            this.Encumbrance = template.Encumbrance;
            this.Value = template.Value;
            this.Availability = template.Availability;
            this.WeaponClass = template.WeaponClass;
            this.Durability = template.Durability;
            // Create a NEW dictionary instance with the same properties
            this.Properties = new Dictionary<ShieldProperty, int>(template.Properties);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Category}] {Name} | ");
            sb.AppendLine($"Class: {WeaponClass} | DEF: {DefValue}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }
            if (HasProperty(ShieldProperty.Huge)) sb.AppendLine("Properties: Huge");
            return sb.ToString();
        }

        public void SetMithrilModifier()
        {
            if (HasProperty(ShieldProperty.Mithril))
            {
                DefValue += 1; // Increase defense value if Mithril
                Encumbrance -= 1; // Decrease encumbrance if Mithril
            }
        }

        public bool HasProperty(ShieldProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(ShieldProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
        }

    }
}