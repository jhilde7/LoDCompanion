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
        public string Name { get; set; } = string.Empty;
        public int Encumbrance { get; set; }
        public int MaxDurability { get; set; } = 6;
        public int Durability { get; set; } = 1;
        public double Value { get; set; } = 0; 
        public int SellValue { get; private set; } 
        public int RepairCost { get; private set; } 
        public int Availability { get; set; } = 4;
        public int Quantity { get; set; } = 1;
        public string Description { get; set; } = string.Empty;
        public string MagicEffect { get; set; } = string.Empty;

        public Equipment() 
        {

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
                default:
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
        Magic
    }

    public class Weapon : Equipment
    {
        public bool IsRanged { get; set; }
        public bool IsMelee { get; set; }
        public new int Durability { get; set; } = 6;
        public int Class { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int ArmourPiercing { get; set; }
        public Dictionary<WeaponProperty, int> Properties { get; set; } = new Dictionary<WeaponProperty, int>();

        public virtual int RollDamage()
        {
            return RandomHelper.GetRandomNumber(MinDamage, MaxDamage);
        }

    }

    public class MeleeWeapon : Weapon
    {
        private bool HasAppliedSlayerModifier { get; set; }
        
        public MeleeWeapon() 
        {
            IsMelee = true;
            SetMithrilModifier();
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

        public void SetMithrilModifier()
        {
            if(HasProperty(WeaponProperty.Mithril))
            {
                MinDamage += 1;
                MaxDamage += 1;
                Encumbrance -= 2;
            }
        }

        public void SetSlayerTreatedModifier()
        {
            if(HasProperty(WeaponProperty.SlayerTreated))
            {
                MinDamage += 1;
                MaxDamage += 1;
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
        public bool IsSecondaryWeapon { get; set; }
        public int ReloadTime { get; set; } = 1;
        public bool IsLoaded { get; set; } = false;

        public RangedWeapon() 
        {
            IsRanged = true;
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

        // You might add methods specific to ranged weapons here, e.g., to consume ammo
        public bool ConsumeAmmo(int quantity = 1)
        {
            if (IsSlingUsingNormalAmmo())
            {
                return false;
            }

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
        public Dictionary<ArmourProperty, int> Properties { get; set; } = new Dictionary<ArmourProperty, int>();

        public Armour()
        {
            SetMithrilModifier();
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
        public int ArmourClass { get; set; }
        public int DefValue { get; set; }
        public Dictionary<ShieldProperty, int> Properties { get; set; } = new Dictionary<ShieldProperty, int>();

        public Shield() 
        { 
        
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