using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace LoDCompanion.Models
{
    public class Equipment
    {
        public string? Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Encumbrance { get; set; }
        public int MaxDurability { get; set; } = 6;
        public int Durability { get; set; } = 1;
        public int Value { get; set; } = 0; 
        public int SellValue { get; private set; } 
        public int RepairCost { get; private set; } 
        public int Availability { get; set; } = 4;
        public int Quantity { get; set; } = 1;
        public string Description { get; set; } = string.Empty;
        public bool IsMagic { get; set; }
        public string MagicEffect { get; set; } = string.Empty;

        public Equipment() 
        {

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Type}] {Name} | ");
            sb.Append($"Value: {Value} | Dur: {Durability}/{MaxDurability}");
            if (IsMagic && !string.IsNullOrEmpty(MagicEffect))
            {
                sb.Append($" | Effect: {MagicEffect}");
            }
            return sb.ToString();
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
        public AmmoType AmmoType { get; set; } = AmmoType.Arrow; // Default ammo type, can be set in constructor
        public bool IsSilver { get; set; }
        public bool IsBarbed { get; set; }
        public bool IsSupSlingstone { get; set; }
        public bool IsHolyWater { get; set; }

        public Ammo() { } // Default constructor

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append($"[{Type}] {Name} | ");
            sb.Append($"Value: {Value} | Dur: {Durability}/{MaxDurability}");
            if (IsMagic && !string.IsNullOrEmpty(MagicEffect))
            {
                sb.Append($" | Effect: {MagicEffect}");
            }
            sb.Append($" | Ammo Type: {AmmoType}");
            if (IsSilver) sb.Append(", Silver");
            if (IsBarbed) sb.Append(", Barbed");
            if (IsSupSlingstone) sb.Append(", Superior");
            if (IsHolyWater) sb.Append(", Holy Water added");
            return sb.ToString();
        }
    }

    public class MeleeWeapon : Equipment
    {
        public int WeaponClass { get; set; }
        public int[] DamageRange { get; set; } = new int[2];
        public int ArmourPiercing { get; set; }
        public bool IsSilver { get; set; }
        public bool IsMithril { get; set; }
        public int? DualWieldBonus { get; set; }
        public bool IsBFO { get; set; }
        public bool IsSlow { get; set; }
        public bool IsStun { get; set; }
        public bool IsUnwieldly { get; set; }
        public int UnWieldlyBonus { get; set; }
        public bool IsFirstHit { get; set; }
        public bool IsFast { get; set; }
        public bool IsDefensive { get; set; }
        public bool IsEnsnare { get; set; }
        public bool IsReach { get; set; }
        public bool IsEdged { get; set; }
        public bool IsSlayerTreated { get; set; }
        public bool IsAxe { get; set; }
        public bool IsSword { get; set; }
        public bool IsBlunt { get; set; }
        public bool IsMetal { get; set; }

        public MeleeWeapon() { } // Default constructor

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Type}] {Name} | ");
            sb.AppendLine($"Class: {WeaponClass} | Damage: {DamageRange[0]}-{DamageRange[1]} | AP: {ArmourPiercing}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            if (IsMagic && !string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }

            var properties = new List<string>();
            if (IsSilver) properties.Add("Silver");
            if (IsMithril) properties.Add("Mithril");
            if (DualWieldBonus.HasValue) properties.Add($"Dual Wield: +{DualWieldBonus}");
            if (IsBFO) properties.Add("BFO");
            if (IsSlow) properties.Add("Slow");
            if (IsStun) properties.Add("Stun");
            if (IsUnwieldly) properties.Add($"Unwieldy (+{UnWieldlyBonus})");
            if (IsFirstHit) properties.Add("First Hit");
            if (IsFast) properties.Add("Fast");
            if (IsDefensive) properties.Add("Defensive");
            if (IsEnsnare) properties.Add("Ensnare");
            if (IsReach) properties.Add("Reach");
            if (properties.Any())
            {
                sb.AppendLine($"Properties: {string.Join(", ", properties)}");
            }

            return sb.ToString();
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

    public class MagicStaff : MeleeWeapon
    {
        public string StaffType { get; set; } = string.Empty;
        public int ArcaneArtsSkillModifier { get; set; }
        public string ContainedSpell { get; set; } = string.Empty;
        public int ManaStorage { get; set; }
        public int HPBonus { get; set; }

        public MagicStaff() { }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append($"[{Type}] {Name} | ");
            sb.AppendLine($"Class: {WeaponClass} | Damage: {DamageRange[0]}-{DamageRange[1]} | AP: {ArmourPiercing}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");

            var properties = new List<string>();
            if (IsSilver) properties.Add("Silver");
            if (IsMithril) properties.Add("Mithril");
            if (IsBFO) properties.Add("BFO");
            if (IsSlow) properties.Add("Slow");
            if (IsStun) properties.Add("Stun");
            if (IsUnwieldly) properties.Add($"Unwieldy (+{UnWieldlyBonus})");
            if (IsFirstHit) properties.Add("First Hit");
            if (IsFast) properties.Add("Fast");
            if (IsDefensive) properties.Add("Defensive");
            if (IsEnsnare) properties.Add("Ensnare");
            if (IsReach) properties.Add("Reach");
            if (properties.Any())
            {
                sb.AppendLine($"Properties: {string.Join(", ", properties)}");
            }
            sb.AppendLine($"Staff Type: {StaffType} | Arcane Arts Mod: {ArcaneArtsSkillModifier}");
            if (!string.IsNullOrEmpty(ContainedSpell)) sb.AppendLine($"Contains: {ContainedSpell}");
            if (ManaStorage > 0) sb.AppendLine($"Mana Storage: {ManaStorage}");
            if (HPBonus > 0) sb.AppendLine($"HP Bonus: {HPBonus}");
            return sb.ToString();
        }

    }

    public class RangedWeapon : Equipment
    {
        public int WeaponClass { get; set; }
        public int[] DamageRange { get; set; } = new int[2];
        public int ArmourPiercing { get; set; }
        public AmmoType AmmoType { get; set; } = AmmoType.Arrow;
        public Ammo Ammo { get; set; } = new Ammo();
        public bool ElvenBowstring { get; set; }
        public bool AimAttachment { get; set; }
        public bool IsSecondaryWeapon { get; set; }
        public int ReloadTime { get; set; } = 1;
        public bool IsLoaded { get; set; } = false;

        public RangedWeapon() { } // Default constructor

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Type}] {Name} | ");
            sb.AppendLine($"Class: {WeaponClass} | Damage: {DamageRange[0]}-{DamageRange[1]} | AP: {ArmourPiercing}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            sb.AppendLine($"Ammo Type: {AmmoType} | Reload Time: {ReloadTime} AP | Loaded: {IsLoaded}");
            if (ElvenBowstring) sb.Append(" | Elven Bowstring");
            if (AimAttachment) sb.Append(" | Aim Attachment");
            if (IsMagic && !string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }
            return sb.ToString();
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

    public class Armour : Equipment
    {
        public int ArmourClass { get; set; }
        public int DefValue { get; set; }
        public bool IsMithril { get; set; }
        public bool IsMetal { get; set; }
        public bool IsHead { get; set; }
        public bool IsTorso { get; set; }
        public bool IsArms { get; set; }
        public bool IsLegs { get; set; }
        public bool IsCloak { get; set; }
        public bool IsStackable { get; set; }
        public bool IsClunky { get; set; }
        public bool IsUpgraded { get; set; }
        public bool IsDarkAsTheNight { get; set; }
        public bool IsDragonScale { get; set; }
        public bool IsDog { get; set; }

        public Armour()
        {
            SetMithrilModifier();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Type}] {Name} | ");
            sb.AppendLine($"Class: {ArmourClass} | DEF: {DefValue}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            if (IsMagic && !string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }

            var coveredAreas = new List<string>();
            if (IsHead) coveredAreas.Add("Head");
            if (IsTorso) coveredAreas.Add("Torso");
            if (IsArms) coveredAreas.Add("Arms");
            if (IsLegs) coveredAreas.Add("Legs");
            if (IsCloak) coveredAreas.Add("Cloak");
            if (coveredAreas.Any())
            {
                sb.AppendLine($"Covers: {string.Join(", ", coveredAreas)}");
            }
            return sb.ToString();
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

    public class Shield : Equipment
    {
        public int ArmourClass { get; set; }
        public int DefValue { get; set; }
        public bool IsMithril { get; set; }
        public bool IsMetal { get; set; }
        public bool IsShield { get; set; } = true;
        public bool IsHuge { get; set; }

        public Shield() { }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Type}] {Name} | ");
            sb.AppendLine($"Class: {ArmourClass} | DEF: {DefValue}");
            sb.AppendLine($"Value: {Value} | Durability: {Durability}/{MaxDurability} | Enc: {Encumbrance}");
            if (IsMagic && !string.IsNullOrEmpty(MagicEffect))
            {
                sb.AppendLine($" | Magic Effect: {MagicEffect}");
            }
            if (IsHuge) sb.AppendLine("Properties: Huge");
            return sb.ToString();
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
}