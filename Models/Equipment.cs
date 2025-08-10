using LoDCompanion.Services.GameData;
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

    public enum EquipmentProperty
    {
        Lantern,
        Torch,
        Backpack,
        Consumable,
        Restorative,
        Lockpick,
    }

    [JsonDerivedType(typeof(Weapon), typeDiscriminator: "Weapon")]
    [JsonDerivedType(typeof(MeleeWeapon), typeDiscriminator: "MeleeWeapon")]
    [JsonDerivedType(typeof(RangedWeapon), typeDiscriminator: "RangedWeapon")]
    [JsonDerivedType(typeof(Armour), typeDiscriminator: "Armour")]
    [JsonDerivedType(typeof(Shield), typeDiscriminator: "Shield")]
    [JsonDerivedType(typeof(Ammo), typeDiscriminator: "Ammo")]
    [JsonDerivedType(typeof(AlchemyItem), typeDiscriminator: "AlchemyItem")]
    [JsonDerivedType(typeof(Ingredient), typeDiscriminator: "Ingredient")]
    [JsonDerivedType(typeof(Part), typeDiscriminator: "Part")]
    [JsonDerivedType(typeof(Potion), typeDiscriminator: "Potion")]
    [JsonDerivedType(typeof(AlchemicalRecipe), typeDiscriminator: "AlchemicalRecipe")]
    public class Equipment
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = "Common";
        public ShopCategory Shop { get; set; } = ShopCategory.General;
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
        public Inventory? Storage { get; set; }
        public Dictionary<EquipmentProperty, int> Properties { get; set; } = new Dictionary<EquipmentProperty, int>();

        public Equipment()
        {

        }

        public virtual Equipment Clone()
        {
            Equipment newEquipment = new Equipment();
            // Copy values from the this
            newEquipment.Category = this.Category;
            newEquipment.Shop = this.Shop;
            newEquipment.Name = this.Name;
            newEquipment.Encumbrance = this.Encumbrance;
            newEquipment.Value = this.Value;
            newEquipment.Availability = this.Availability;
            newEquipment.MaxDurability = this.MaxDurability;
            newEquipment.Durability = this.Durability;
            newEquipment.Quantity = this.Quantity;
            newEquipment.Description = this.Description;
            newEquipment.MagicEffect = this.MagicEffect;
            newEquipment.Storage = this.Storage;
            // Create a NEW dictionary instance with the same properties
            newEquipment.Properties = new Dictionary<EquipmentProperty, int>(this.Properties);

            return newEquipment;
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

        public bool HasProperty(EquipmentProperty property)
        {
            return Properties.ContainsKey(property);
        }

        public int GetPropertyValue(EquipmentProperty property)
        {
            return Properties.GetValueOrDefault(property, 0);
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
        public new Dictionary<AmmoProperty, int> Properties { get; set; } = new Dictionary<AmmoProperty, int>();

        public Ammo() { } // Default constructor

        public override Ammo Clone()
        {
            Ammo newAmmo = new Ammo();
            // Copy values from the template
            newAmmo.Category = Category;
            newAmmo.Shop = Shop;
            newAmmo.Name = Name;
            newAmmo.Encumbrance = Encumbrance;
            newAmmo.Value = Value;
            newAmmo.Availability = Availability;
            newAmmo.MaxDurability = MaxDurability;
            newAmmo.Durability = Durability;
            newAmmo.Quantity = Quantity;
            newAmmo.Description = Description;
            newAmmo.MagicEffect = MagicEffect;
            // Create a NEW dictionary instance with the same properties
            newAmmo.Properties = new Dictionary<AmmoProperty, int>(Properties);
            return newAmmo;
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
        SecondaryWeapon,
        ArmourPiercing,
        Cursed,
        Poisoned,
        Diseased
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
        public new virtual Dictionary<WeaponProperty, int> Properties { get; set; } = new Dictionary<WeaponProperty, int>();

        public virtual int RollDamage()
        {
            return RandomHelper.GetRandomNumber(MinDamage, MaxDamage);
        }

        public override Weapon Clone()
        {
            if (this is MeleeWeapon meleeWeapon)
            {
                return meleeWeapon.Clone();
            }
            else if (this is RangedWeapon rangedWeapon)
            {
                return rangedWeapon.Clone();
            }

            return (Weapon)this.Clone();
        }

        public virtual bool HasProperty(WeaponProperty property)
        {
            return Properties.ContainsKey(property);
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

        public override MeleeWeapon Clone()
        {
            MeleeWeapon newMeleeWeapon = new MeleeWeapon();

            IsMelee = true;
            // Copy values from the base weapon
            newMeleeWeapon.Category = Category;
            newMeleeWeapon.Shop = Shop;
            newMeleeWeapon.Name = Name;
            newMeleeWeapon.Class = Class;
            newMeleeWeapon.MinDamage = MinDamage;
            newMeleeWeapon.MaxDamage = MaxDamage;
            newMeleeWeapon.Value = Value;
            newMeleeWeapon.Encumbrance = Encumbrance;
            newMeleeWeapon.Durability = Durability;
            newMeleeWeapon.DamageDice = DamageDice;
            newMeleeWeapon.DamageBonus = DamageBonus;
            // Create a NEW dictionary instance with the same properties
            newMeleeWeapon.Properties = new Dictionary<WeaponProperty, int>(Properties);
            return newMeleeWeapon;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Value > 0)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"{prop.Key}: +{prop.Value}");
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
            if (HasProperty(WeaponProperty.Mithril))
            {
                MinDamage += 1;
                MaxDamage += 1;
                DamageBonus += 1;
                Encumbrance -= 2;
            }
        }

        public void SetSlayerTreatedModifier()
        {
            if (HasProperty(WeaponProperty.SlayerTreated))
            {
                MinDamage += 1;
                MaxDamage += 1;
                DamageBonus += 1;
                HasAppliedSlayerModifier = true;
            }
        }

        public override bool HasProperty(WeaponProperty property)
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

        public override MagicStaff Clone()
        {
            MagicStaff newMagicStaff = new MagicStaff();
            // Copy values from the template
            newMagicStaff.Category = Category;
            newMagicStaff.Shop = Shop;
            newMagicStaff.Name = Name;
            newMagicStaff.Class = Class;
            newMagicStaff.MinDamage = MinDamage;
            newMagicStaff.MaxDamage = MaxDamage;
            newMagicStaff.Value = Value;
            newMagicStaff.Encumbrance = Encumbrance;
            newMagicStaff.Durability = Durability;
            newMagicStaff.MagicEffect = MagicEffect;
            // Create a NEW dictionary instance with the same properties
            newMagicStaff.Properties = new Dictionary<WeaponProperty, int>(Properties);
            newMagicStaff.StaffType = StaffType;
            newMagicStaff.ContainedSpell = ContainedSpell;
            newMagicStaff.MagicStaffProperties = new Dictionary<MagicStaffProperty, int>(MagicStaffProperties);
            return newMagicStaff;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Value > 0)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"{prop.Key}: +{prop.Value}");
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

        public override RangedWeapon Clone()
        {
            RangedWeapon newRangedWeapon = new RangedWeapon();

            IsRanged = true;
            // --- Properties from Searchable ---
            newRangedWeapon.Name = Name;
            // --- Properties from Equipment ---
            newRangedWeapon.Description = Description;
            newRangedWeapon.Class = Class;
            newRangedWeapon.Durability = Durability;
            newRangedWeapon.Encumbrance = Encumbrance;
            newRangedWeapon.Value = Value;
            // --- Properties from Weapon ---
            newRangedWeapon.MinDamage = MinDamage;
            newRangedWeapon.MaxDamage = MaxDamage;
            newRangedWeapon.DamageDice = DamageDice;
            newRangedWeapon.DamageBonus = DamageBonus;
            // --- Properties from RangedWeapon ---
            newRangedWeapon.AmmoType = AmmoType;
            newRangedWeapon.Ammo = Ammo;
            newRangedWeapon.ElvenBowstring = ElvenBowstring;
            newRangedWeapon.AimAttachment = AimAttachment;
            newRangedWeapon.ReloadTime = ReloadTime;
            return newRangedWeapon;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Name}] Class: {Class} | Dmg: {MinDamage}-{MaxDamage}");
            sb.Append($" | Val: {Value} | Dur: {Durability}/{Durability} | Enc: {Encumbrance}");
            sb.AppendLine($" | Ammo Category: {AmmoType} | Reload Time: {ReloadTime} AP | Loaded: {IsLoaded}");
            if (ElvenBowstring) sb.Append(" | Elven Bowstring");
            if (AimAttachment) sb.Append(" | Aim Attachment");
            if (!string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }
            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    if (prop.Value > 0)
                    {
                        // Special formatting for properties with a value
                        propsAsStrings.Add($"{prop.Key}: +{prop.Value}");
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

        public new Dictionary<ArmourProperty, int> Properties
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

        public override Armour Clone()
        {
            Armour newArmour = new Armour();
            // Copy values from the template
            newArmour.Category = Category;
            newArmour.Shop = Shop;
            newArmour.Name = Name;
            newArmour.ArmourClass = ArmourClass;
            newArmour.DefValue = DefValue;
            newArmour.Encumbrance = Encumbrance;
            newArmour.Value = Value;
            newArmour.Availability = Availability;
            newArmour.Durability = Durability;
            // Create a NEW dictionary instance with the same properties
            newArmour.Properties = new Dictionary<ArmourProperty, int>(Properties);
            return newArmour;
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

            if (Properties.Any())
            {
                var propsAsStrings = new List<string>();
                foreach (var prop in Properties)
                {
                    propsAsStrings.Add(prop.Key.ToString());
                }
                sb.Append(" | Properties: ").Append(string.Join(", ", propsAsStrings));
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

        public new Dictionary<ShieldProperty, int> Properties
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

        public override Shield Clone()
        {
            Shield newShield = new Shield();
            // Copy values from the template
            newShield.Category = Category;
            newShield.Shop = Shop;
            newShield.Name = Name;
            newShield.DefValue = DefValue;
            newShield.Encumbrance = Encumbrance;
            newShield.Value = Value;
            newShield.Availability = Availability;
            newShield.WeaponClass = WeaponClass;
            newShield.Durability = Durability;
            // Create a NEW dictionary instance with the same properties
            newShield.Properties = new Dictionary<ShieldProperty, int>(Properties);
            return newShield;
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