using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Utilities;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.SignalR;

namespace LoDCompanion.Services.GameData
{
  public class GameDataService
  {
        public List<Spell> Spells => GetSpells();
        public List<Prayer> Prayers => GetPrayers();
        public List<Species> Species => GetSpecies();
        public List<Profession> Professions => GetProfessions();
        public List<Talent> Talents => GetTalents();
        public List<Talent> PhysicalTalents => GetTalentsByCategory(TalentCategory.Physical);
        public List<Talent> CombatTalents => GetTalentsByCategory(TalentCategory.Combat);
        public List<Talent> FaithTalents => GetTalentsByCategory(TalentCategory.Faith);
        public List<Talent> AlchemistTalents => GetTalentsByCategory(TalentCategory.Alchemist);
        public List<Talent> CommonTalents => GetTalentsByCategory(TalentCategory.Common);
        public List<Talent> MagicTalents => GetTalentsByCategory(TalentCategory.Magic);
        public List<Talent> SneakyTalents => GetTalentsByCategory(TalentCategory.Sneaky);
        public List<Talent> MentalTalents => GetTalentsByCategory(TalentCategory.Mental);
        public List<Perk> Perks => GetPerks();
        public List<Perk> LeaderPerks => GetPerksByCategory(PerkCategory.Leader);
        public List<Perk> CommonPerks => GetPerksByCategory(PerkCategory.Common);
        public List<Perk> CombatPerks => GetPerksByCategory(PerkCategory.Combat);
        public List<Perk> SneakyPerks => GetPerksByCategory(PerkCategory.Sneaky);
        public List<Perk> FaithPerks => GetPerksByCategory(PerkCategory.Faith);
        public List<Perk> ArcanePerks => GetPerksByCategory(PerkCategory.Arcane);
        public List<Perk> AlchemistPerks => GetPerksByCategory(PerkCategory.Alchemist);
        public List<ActiveStatusEffect> StatusEffects => GetStatusEffects();
        public List<MonsterSpell> MonsterSpells => GetMonsterSpells();


        public string GetRandomSpellNameByCategory(string category = "All", bool isUndead = false)
        {
          string spell = "";
          // Using the new Utilities.RandomHelper.RandomNumber method
          int roll = RandomHelper.GetRandomNumber(1, 100);

          switch (category)
          {
            case "All":
              return roll switch
              {
                <= 4 => "Fake Death",
                <= 8 => "Flare",
                <= 12 => "Gust of Wind",
                <= 16 => "Hand of Death",
                <= 22 => "Light Healing",
                <= 29 => "Protective Shield",
                <= 33 => "Slip",
                <= 35 => "Blur",
                <= 37 => "Fist of Iron",
                <= 39 => "Magic Scribbles",
                <= 41 => "Open Lock",
                <= 43 => "Seal Door",
                <= 45 => "Silence",
                <= 47 => "Strengthen Body",
                <= 49 => "Summon Lesser Demon",
                <= 51 => "Confuse",
                <= 53 => "Control Undead",
                <= 55 => "Corruption",
                <= 57 => "Enchant Item",
                <= 59 => "Healing",
                <= 61 => "Ice Pikes",
                <= 63 => "Lightning Bolt",
                <= 65 => "Magic Armour",
                <= 67 => "Magic Bolt",
                <= 69 => "Slow",
                <= 71 => "Summon Water Elemental",
                <= 73 => "Summon Wind Elemental",
                <= 75 => "Vampiric Touch",
                76 => "Banish Undead",
                77 => "Bolster Mind",
                78 => "Frost Beam",
                79 => "Hold Creature",
                80 => "Ice Tomb",
                81 => "Transpose",
                82 => "Second Sight",
                83 => "Summon Demon",
                84 => "Summon Earth Elemental",
                85 => "Summon Fire Elemental",
                86 => "Summon Souls",
                87 => "Weakness",
                88 => "Cause Animosity",
                89 => "Fire Rain",
                90 => "Fire Wall",
                91 => "Levitate",
                92 => "Mirrored Self",
                93 => "Speed",
                94 => "Time Freeze",
                95 => "Fireball",
                96 => "Into the Void",
                97 => "Life Force",
                98 => "Raise Dead",
                99 => "Summon Greater Demon",
                100 => "Teleportation",
                _ => "Invalid"
              };
            case "Ranged spell":
              roll = RandomHelper.GetRandomNumber(1, 12);
              return roll switch
              {
                <= 2 => "Blind",
                <= 4 => "Flare",
                <= 6 => "Fireball",
                <= 8 => "Frost Ray",
                <= 10 => "Gust of Wind",
                <= 12 => "Slow",
                _ => "Invalid"
              };
            case "Touch spell":
              roll = RandomHelper.GetRandomNumber(1, 12);
              return roll switch
              {
                <= 2 => "Mind Blast",
                <= 4 => "Mirrored Self",
                <= 6 => "Seduce",
                <= 8 => "Stun",
                <= 10 => "Teleportation",
                <= 12 => "Vampiric Touch",
                _ => "Invalid"
              };
            case "Support spell":
              roll = RandomHelper.GetRandomNumber(1, 16);
              return roll switch
              {
                <= 2 => "Frenzy",
                <= 4 => "Healing",
                <= 6 => "Healing Hand",
                <= 8 => "Mute",
                <= 10 => isUndead ? "Raise Dead" : GetRandomSpellNameByCategory("Support spell"),
                <= 12 => "Shield",
                <= 14 => "Summon Demon",
                <= 16 => "Summon Greater Demon",
                _ => "Invalid"
              };
          }
          return spell;
        }

        public string GetRandomSpellName()
        {
          return GetRandomSpellNameByCategory();
        }

        public List<Spell> GetSpells()
        {
          return new List<Spell>()
                {
                    new Spell(){
                      Name = "Fake Death",
                      Level = 1,
                      CastingValue = 7,
                      ManaCost = 8,
                      TurnDuration = -1,
                      School = MagicSchool.Necromancy,
                      SpellEffect = "Causes the caster to fall to the ground, appearing dead to all around. Enemies will not target the caster for the rest of the battle. The caster may do nothing until the end of the battle."
                    },
                    new Spell(){
                      Name = "Flare",
                      Level = 1,
                      CastingValue = 8,
                      ManaCost = 15,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 8,
                      Properties = new List<SpellProperty>(){ SpellProperty.QuickSpell, SpellProperty.MagicMissile },
                      School = MagicSchool.Destruction,
                      SpellEffect = "A bright flare shoots from the caster's hand, hissing through the air to strike the target with a large bang. DMG is 1D8."
                    },
                    new Spell(){
                      Name = "Gust of Wind",
                      Level = 1,
                      CastingValue = 12,
                      ManaCost = 8,
                      UpkeepCost = 1,
                      TurnDuration = -1,
                      AddCasterLvlToDuration = true,
                      School = MagicSchool.Alteration,
                      IsAOESpell = true,
                      SpellEffect = "Suddenly a powerful wind blows through the dungeon, making arrows fly astray. All Missile Weapons now have a -15 modifier to hit if the arrows pass the room the Wizard is in. The wind lasts for Caster level turns. Upkeep is 1 point of Mana."
                    },
                    new Spell(){
                      Name = "Hand of Death",
                      Level = 1,
                      CastingValue = 7,
                      ManaCost = 8,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 10,
                      IsArmourPiercing = true,
                      Properties = new List<SpellProperty>(){ SpellProperty.QuickSpell, SpellProperty.Touch },
                      School = MagicSchool.Necromancy,
                      SpellEffect = "This is a close combat spell, where the caster touches his enemy and causes him harm through magical energy. The target loses 1d10 Hit Points which ignores armour."
                    },
                    new Spell(){
                      Name = "Healing Hand",
                      Level = 1,
                      CastingValue = 6,
                      ManaCost = 12,
                      MinDamage = 1,
                      MaxDamage = 10,
                      Properties = new List<SpellProperty>(){ SpellProperty.QuickSpell, SpellProperty.Touch },
                      School = MagicSchool.Restoration,
                      SpellEffect = "The caster lays his hand on a comrade and heals 1d10 Hit Points."
                    },
                    new Spell(){
                      Name = "Light Healing",
                      Level = 1,
                      CastingValue = 5,
                      ManaCost = 10,
                      MinDamage = 1,
                      MaxDamage = 6,
                      Properties = new List<SpellProperty>(){ SpellProperty.QuickSpell },
                      School = MagicSchool.Restoration,
                      SpellEffect = "The caster can heal one hero within 4 squares and in LOS (intervening models does not matter). It heals 1d6 Hit Points."
                    },
                    new Spell(){
                      Name = "Protective Shield",
                      Level = 1,
                      CastingValue = 10,
                      ManaCost = 10,
                      UpkeepCost = 1,
                      TurnDuration = -1,
                      School = MagicSchool.Mysticism,
                      SpellEffect = "The caster summons a translucent sphere of blue light around himself or the target (which must be in LOS), protecting it from physical harm. The shield absorbs 1 Point of Damage per Caster level to a maximum of 3. You can cast the spell twice (but not more) on each target, adding together the effect of the spell. The spell lasts the entire battle but costs 1 point of Mana in upkeep per turn."
                    },
                    new Spell(){
                      Name = "Slip",
                      Level = 1,
                      CastingValue = 10,
                      ManaCost = 10,
                      TurnDuration = 1,
                      School = MagicSchool.Hex,
                      SpellEffect = "Causes the target to slip and fall. The target will remain prone until its next action when it will spend its first turn standing up."
                    },
                    new Spell(){
                      Name = "Blur",
                      Level = 2,
                      CastingValue = 15,
                      ManaCost = 10,
                      UpkeepCost = 1,
                      TurnDuration = -1,
                      School = MagicSchool.Illusion,
                      SpellEffect = "May target self or hero in LOS. Target becomes blurry and any attacks against the target is at -15. The effect lasts for 1d4 turns."
                    },
                    new Spell(){
                      Name = "Fist of Iron",
                      Level = 2,
                      CastingValue = 8,
                      ManaCost = 14,
                      IsDamageSpell = true,
                      MinDamage = 2,
                      MaxDamage = 6,
                      IncludeCasterLevelInDamage = true,
                      School = MagicSchool.Destruction,
                      SpellEffect = "The target is struck from above by a powerful blow, causing 2d6+Caster Level points of DMG. Armour and NA protects as normal. Target must be in LOS."
                    },
                    new Spell(){
                      Name = "Magic Scribbles",
                      Level = 2,
                      CastingValue = 20,
                      Properties = new List<SpellProperty>(){ SpellProperty.Incantation },
                      School = MagicSchool.Enchantment,
                      SpellEffect = "This spell is used to create scrolls. As long as the wizard knows the spell he wants to use as the basis for the scroll, and has a good quality parchment, this is quite easy although time consuming."
                    },
                    new Spell(){
                      Name = "Open Lock",
                      Level = 2,
                      ManaCost = 8,
                      Properties = new List<SpellProperty>(){ SpellProperty.Touch },
                      School = MagicSchool.Alteration,
                      SpellEffect = "This spell can be used to magically open locked doors or chests. The caster must stand close enough to touch the lock, and the locks hit points is used as the CV of the spell."
                    },
                    new Spell(){
                      Name = "Seal Door",
                      Level = 2,
                      CastingValue = 13,
                      ManaCost = 12,
                      School = MagicSchool.Alteration,
                      SpellEffect = "The Spell Caster can magically seal a door. Any monster outside trying to pass through will take 1d3 turns in doing so. Doors that have been broken down cannot be sealed. This can be cast on any door, even if there are monsters present. It can only be cast once per door."
                    },
                    new Spell(){
                      Name = "Silence",
                      Level = 2,
                      CastingValue = 10,
                      ManaCost = 12,
                      School = MagicSchool.Hex,
                      SpellEffect = "The spell can be cast on an enemy Magic Caster. If the spell is successfully cast, the target must make a RES test when casting a spell. A failure means that the target cannot cast magic that turn, but may otherwise act as normal. Making this test does not cost an AP. If successful, the target may cast the spell as planned and the spell ceases to have any effect."
                    },
                    new Spell(){
                      Name = "Strengthen Body",
                      Level = 2,
                      CastingValue = 10,
                      ManaCost = 8,
                      UpkeepCost = 2,
                      TurnDuration = -1,
                      School = MagicSchool.Mysticism,
                      SpellEffect = "Caster may strengthen a hero in LOS with +10 in either STR or CON. The spell lasts for 1d6 turns."
                    },
                    new Spell(){
                      Name = "Summon Lesser Demon",
                      Level = 2,
                      CastingValue = 15,
                      ManaCost = 10,
                      UpkeepCost = 4,
                      School = MagicSchool.Conjuration,
                      SpellEffect = "The caster reaches into the Void and summons a Lesser Plague Demon. Place the demon in a random free square in the room. The demon may act as part of the hero's next turn. It will fight for the caster, but also try to break free at every turn. At the start of each turn, the caster must use 4 Mana as upkeep, and then pass a Resolve Test. If the caster fails, the demon breaks free and escapes back to its own dimension. Add one hero initiative token to the bag."
                    },
                    new Spell(){
                      Name = "Confuse",
                      Level = 3,
                      CastingValue = 15,
                      ManaCost = 18,
                      School = MagicSchool.Illusion,
                      SpellEffect = "If successfully cast at a target in LOS, the target must pass RES or be unable to use that action. If the target fails, it may try again for Action Point number 2. Once it succeeds, the effect of the spell is gone."
                    },
                    new Spell(){
                      Name = "Control Undead",
                      Level = 3,
                      CastingValue = 20,
                      ManaCost = 12,
                      TurnDuration = 1,
                      School = MagicSchool.Necromancy,
                      SpellEffect = "The caster may try to take control of a lower undead in LOS. If the caster succeeds with the RES+Caster Level test, the wizard may control the Undead until next turn. It still retains its monster activation token. Make Resolve test every time you activate the creature. As long as the test succeeds, the caster may control the Undead creature. There is no upkeep since the Undead has been brought back by something else than the caster's magic."
                    },
                    new Spell(){
                      Name = "Corruption",
                      Level = 3,
                      CastingValue = 18,
                      ManaCost = 16,
                      UpkeepCost = 1,
                      TurnDuration = -1,
                      Properties = new List<SpellProperty>(){ SpellProperty.MagicMissile },
                      School = MagicSchool.Necromancy,
                      SpellEffect = "A storm of flies soars from the gaping mouth of the caster, surrounding the target. The cloud of flies will make it harder for the enemy to fight by reducing its CS by 10. The spell lasts for 1d3 turns."
                    },
                    new Spell(){
                      Name = "Enchant Item",
                      Level = 3,
                      CastingValue = 25,
                      ManaCost = 16,
                      Properties = new List<SpellProperty>(){ SpellProperty.Incantation },
                      School = MagicSchool.Enchantment,
                      SpellEffect = "This spell can only be cast between quests and requires a powerstone. The power of the stone will then be fused with an object such as a weapon, an armour or a piece of jewellery. See chapter on Crafting."
                    },
                    new Spell(){
                      Name = "Healing",
                      Level = 3,
                      CastingValue = 15,
                      ManaCost = 16,
                      MinDamage = 1,
                      MaxDamage = 10,
                      School = MagicSchool.Restoration,
                      SpellEffect = "The caster may heal a hero within 4 squares and in LOS. The target regains 1d10 Hit Points."
                    },
                    new Spell(){
                      Name = "Ice Pikes",
                      Level = 3,
                      CastingValue = 10,
                      ManaCost = 16,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 12,
                      School = MagicSchool.Destruction,
                      SpellEffect = "A series of razor-sharp Ice spikes shoot from the floor, striking the target from below. It causes 1d12 Frost DMG. Target must be in LOS."
                    },
                    new Spell(){
                      Name = "Lightning Bolt",
                      Level = 3,
                      CastingValue = 16,
                      ManaCost = 18,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 10,
                      IsArmourPiercing = true,                      
                      DamageType = DamageType.Lightning,
                      Properties = new List<SpellProperty>(){ SpellProperty.MagicMissile },
                      School = MagicSchool.Destruction,
                      IsAOESpell = true,
                      AOEMinDamage = 1,
                      AOEMaxDamage = 10,
                      AOERadius = 3,
                      SpellEffect = "A crackling bolt leaps from the hand of the wizard, striking a victim within LOS, dealing 1d10 DMG, ignoring armour. The bolt will then jump to the nearest model (random if equal) and deal 1d8 DMG, ignoring armour. Finally, it will make its last jump, dealing 1d6 DMG, ignoring armour. It will always jump to the nearest model, and will never strike the same model twice. It will never jump more than 3 squares."
                    },
                    new Spell(){
                      Name = "Magic Armour",
                      Level = 3,
                      CastingValue = 15,
                      ManaCost = 15,
                      UpkeepCost = 2,
                      TurnDuration = -1,
                      School = MagicSchool.Mysticism,
                      SpellEffect = "The caster may bolster the armour of any target within LOS with +2 for all parts of the body. The spell lasts for Caster Level+2 turns."
                    },
                    new Spell(){
                      Name = "Magic Bolt",
                      Level = 3,
                      CastingValue = 10,
                      ManaCost = 14,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 10,
                      IsArmourPiercing = true,
                      Properties = new List<SpellProperty>(){ SpellProperty.MagicMissile, SpellProperty.QuickSpell },
                      School = MagicSchool.Destruction,
                      SpellEffect = "A bolt of pure energy lashes from the caster to a target within LOS. Target loses 1d10 Hit Points, ignoring any armour."
                    },
                    new Spell(){
                      Name = "Slow",
                      Level = 3,
                      CastingValue = 14,
                      ManaCost = 12,
                      UpkeepCost = 2,
                      TurnDuration = -1,
                      School = MagicSchool.Hex,
                      SpellEffect = "A target within LOS of the caster must pass a Resolve test or lose one Action Point. Test again at the start of each enemy turn. The effect will last until the enemy test succeeds."
                    },
                    new Spell(){
                      Name = "Summon Water Elemental",
                      Level = 3,
                      CastingValue = 18,
                      ManaCost = 15,
                      UpkeepCost = 5,
                      TurnDuration = -1,
                      AddCasterLvlToDuration = true,
                      School = MagicSchool.Conjuration,
                      SpellEffect = "The caster summons one of the four Elementals to aid him in the battle. The Elemental will fight for Caster Level number of turns. Immediately add one hero initiative token to the bag."
                    },
                    new Spell(){
                      Name = "Summon Wind Elemental",
                      Level = 3,
                      CastingValue = 20,
                      ManaCost = 18,
                      UpkeepCost = 5,
                      TurnDuration = -1,
                      AddCasterLvlToDuration = true,
                      School = MagicSchool.Conjuration,
                      SpellEffect = "The caster summons one of the four Elementals to aid him in the battle. The Elemental will fight for Caster Level number of turns. Immediately add one hero initiative token to the bag."
                    },
                    new Spell(){
                      Name = "Vampiric Touch",
                      Level = 3,
                      CastingValue = 15,
                      ManaCost = 14,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 6,
                      IsArmourPiercing = true,
                      School = MagicSchool.Necromancy,
                      SpellEffect = "Caster causes 1d6 DMG with no armour or NA, and the caster may heal with the same amount of HP up to maximum Hit Points."
                    },
                    new Spell(){
                      Name = "Banish Undead",
                      Level = 4,
                      CastingValue = 20,
                      ManaCost = 20,
                      IsDamageSpell = true,
                      MinDamage = 2,
                      MaxDamage = 6,
                      School = MagicSchool.Necromancy,
                      SpellEffect = "This spell only hurts Undead with the Ethereal Role. A successful spell will damage the Undead creature with 2d6."
                    },
                    new Spell(){
                      Name = "Bolstered Mind",
                      Level = 4,
                      CastingValue = 12,
                      ManaCost = 15,
                      TurnDuration = -1,
                      Properties = new List<SpellProperty>(){ SpellProperty.QuickSpell },
                      School = MagicSchool.Mysticism,
                      SpellEffect = "The caster infuses all members of the party with magical courage. Each hero gains +10 Resolve and may try to re-roll any failed fear test once. Lasts until end of turn."
                    },
                    new Spell(){
                      Name = "Frost Beam",
                      Level = 4,
                      CastingValue = 15,
                      ManaCost = 16,
                      IsDamageSpell = true,
                      MinDamage = 2,
                      MaxDamage = 8,
                      Properties = new List<SpellProperty>(){ SpellProperty.MagicMissile },
                      School = MagicSchool.Destruction,
                      SpellEffect = "A beam of frost shoots from the hands of the caster towards the target, which must be in LOS. The target takes 2d8 Frost DMG."
                    },
                    new Spell(){
                      Name = "Hold Creature",
                      Level = 4,
                      CastingValue = 20,
                      ManaCost = 20,
                      UpkeepCost = 6,
                      TurnDuration = -1,
                      Properties = new List<SpellProperty>(){ SpellProperty.QuickSpell },
                      School = MagicSchool.Hex,
                      SpellEffect = "The wizard holds an enemy in LOS in its place, making it impossible to move or fight. The enemy will make a RES Test at the start of their turn, and if successful, it will break free and act as normal. The activation token should be added to the bag as usual, and the enemy will try to act in the normal order of activation."
                    },
                    new Spell(){
                      Name = "Ice Tomb",
                      Level = 4,
                      CastingValue = 20,
                      ManaCost = 25,
                      TurnDuration = -1,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 4,
                      School = MagicSchool.Destruction,
                      SpellEffect = "Caster may trap a target in LOS in ice, forcing it to break free before being able to do anything else. The caster may roll Caster Level d10 to determine how strong the tomb is, and the target does its maximum damage (Inc) weapon) once per turn until the tomb breaks. It may act with both its actions on the turn the tomb breaks. For every turn, the target takes 1d4 points of Frost DMG."
                    },
                    new Spell(){
                      Name = "Transpose",
                      Level = 4,
                      CastingValue = 15,
                      ManaCost = 25,
                      School = MagicSchool.Alteration,
                      SpellEffect = "The caster may shift the place of two heroes that are in LOS. If the spell fails, both heroes suffer 2 Sanity Points for the ordeal. The caster may not transpose himself."
                    },
                    new Spell(){
                      Name = "Second Sight",
                      Level = 4,
                      CastingValue = 15,
                      ManaCost = 25,
                      School = MagicSchool.Divination,
                      SpellEffect = "Caster can tell what is on the other side of a door. Place the tile and roll for Encounter before opening a door. The heroes gain 2 activation tokens if there is an encounter on the other side of the door."
                    },
                    new Spell(){
                      Name = "Summon Demon",
                      Level = 4,
                      CastingValue = 25,
                      ManaCost = 15,
                      School = MagicSchool.Conjuration,
                      SpellEffect = "The caster lures a demon from its dimension over to this world. It will randomly be either a Blood Demon or a Plague Demon. The demon is placed in a random place in the same tile as the wizard and fights for the caster. Once summoned, immediately add a hero activation token to the bag and activate the demon just like a hero. However, at the start of the wizard's activation following the summoning, the caster must pass a Resolve Test. If the caster fails, the demon breaks free and escapes back to its own dimension. When it breaks free, it will make a Resolve Test of its own and if it succeeds, it takes part of the caster's mind with it. Deduct 1d3 Sanity Points from the caster. Once in our plane, the demon will relish the fighting, so no upkeep is needed."
                    },
                    new Spell(){
                      Name = "Summon Earth Elemental",
                      Level = 4,
                      CastingValue = 20,
                      ManaCost = 15,
                      UpkeepCost = 5,
                      TurnDuration = -1,
                      AddCasterLvlToDuration = true,
                      School = MagicSchool.Conjuration,
                      SpellEffect = "The caster summons one of the four Elementals to aid him in the battle. The Elemental will fight for ML number of turns. Immediately add one hero initiative token to the bag."
                    },
                    new Spell(){
                      Name = "Summon Fire Elemental",
                      Level = 4,
                      CastingValue = 25,
                      ManaCost = 15,
                      UpkeepCost = 5,
                      TurnDuration = -1,
                      AddCasterLvlToDuration = true,
                      School = MagicSchool.Conjuration,
                      SpellEffect = "The caster summons one of the four Elementals to aid him in the battle. The Elemental will fight for Caster level number of turns. Immediately add one hero initiative token to the bag."
                    },
                    new Spell(){
                      Name = "Summon Souls",
                      Level = 4,
                      CastingValue = 12,
                      ManaCost = 15,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 4,
                      IsArmourPiercing = true,
                      School = MagicSchool.Necromancy,
                      IsAOESpell = true,
                      SpellEffect = "This spell conjures a host of restless spirits to torment your enemies. Each enemy on the tile takes 1d4 points of DMG with no armour and NA. Undead enemies are immune."
                    },
                    new Spell(){
                      Name = "Weakness",
                      Level = 4,
                      CastingValue = 18,
                      ManaCost = 18,
                      TurnDuration = -1,
                      Properties = new List<SpellProperty>(){ SpellProperty.Touch },
                      School = MagicSchool.Hex,
                      SpellEffect = "The caster can choose to lower the Strength or Constitution of a chosen target if the target fails a Resolve Test. If the target fails, it loses its NA armour or DMG bonus for 1d4 turns, depending on what the wizard chooses."
                    },
                    new Spell(){
                      Name = "Cause Animosity",
                      Level = 5,
                      CastingValue = 18,
                      ManaCost = 18,
                      UpkeepCost = 10,
                      TurnDuration = 1,
                      School = MagicSchool.Illusion,
                      SpellEffect = "May target any enemy in sight. Target must pass RES or attack the closest enemy during its next activation. Once that activation is over, the effect is gone."
                    },
                    new Spell(){
                      Name = "Fire Rain",
                      Level = 5,
                      CastingValue = 23,
                      ManaCost = 25,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 8,
                      IncludeCasterLevelInDamage = true,
                      DamageType = DamageType.Fire,
                      School = MagicSchool.Destruction,
                      IsAOESpell = true,
                      AOEMinDamage = 1,
                      AOEMaxDamage = 4,
                      AOERadius = 1,
                      AOEIncludesCasterLevel = true,
                      SpellEffect = "A hail of sparks rains down over the target and any adjacent squares. The target takes 1d8+Caster Level Fire DMG and the adjacent square takes 1d4+Caster Level points of Fire DMG."
                    },
                    new Spell(){
                      Name = "Fire Wall",
                      Level = 5,
                      CastingValue = 20,
                      ManaCost = 20,
                      TurnDuration = -1,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 6,
                      DamageType = DamageType.Fire,
                      School = MagicSchool.Destruction,
                      SpellEffect = "This spell creates a Fire Wall, up to 3 squares long. It may only be placed in a straight line and not in a square that contains an enemy. All except lower Undead and Fire Elementals will avoid or try to walk around. Spell lasts for 1d4+1 turns. Any Lower Undead walking through takes 1d6 Fire DMG. Fire Elementals are immune."
                    },
                    new Spell(){
                      Name = "Levitate",
                      Level = 5,
                      CastingValue = 15,
                      ManaCost = 20,
                      TurnDuration = 1,
                      School = MagicSchool.Alteration,
                      SpellEffect = "May target self or hero in LOS. Target may levitate for the entire turn. That means the character moves above the ground, not touching any traps or similar. It may be used to leave a pit and to traverse a pit. You cannot levitate through a square which contains a model or over lava."
                    },
                    new Spell(){
                      Name = "Mirrored Self",
                      Level = 5,
                      CastingValue = 20,
                      ManaCost = 15,
                      UpkeepCost = 2,
                      TurnDuration = -1,
                      School = MagicSchool.Illusion,
                      SpellEffect = "The caster makes a copy of herself which may be placed anywhere within 4 squares of the caster. Enemies will treat this mirrored image as a target just like any other hero, even though it cannot take DMG. The mirrored self cannot move or attack. It will last for 1d4 turns."
                    },
                    new Spell(){
                      Name = "Speed",
                      Level = 5,
                      CastingValue = 15,
                      ManaCost = 15,
                      TurnDuration = -1,
                      School = MagicSchool.Mysticism,
                      SpellEffect = "May target self or any friendly character in LOS. Character gains +1M. The spell lasts until a Scenario die roll of 9-10."
                    },
                    new Spell(){
                      Name = "Time Freeze",
                      Level = 5,
                      CastingValue = 20,
                      ManaCost = 30,
                      School = MagicSchool.Divination,
                      SpellEffect = "All heroes that have acted may immediately put activation tokens back in the bag. They may act again as if it is a new turn. This spell may only be cast once during a battle."
                    },
                    new Spell(){
                      Name = "Fireball",
                      Level = 6,
                      CastingValue = 32,
                      ManaCost = 30,
                      IsDamageSpell = true,
                      MinDamage = 1,
                      MaxDamage = 20,
                      DamageType = DamageType.Fire,
                      Properties = new List<SpellProperty>(){ SpellProperty.MagicMissile },
                      School = MagicSchool.Destruction,
                      IsAOESpell = true,
                      AOEMinDamage = 1,
                      AOEMaxDamage = 10,
                      AOERadius = 1,
                      SpellEffect = "The caster shoots a fireball at a square or an enemy. The target square suffers 1d20 Fire Damage. Adjacent squares suffer 1d10 Fire Damage."
                    },
                    new Spell(){
                      Name = "Into The Void",
                      Level = 6,
                      CastingValue = 30,
                      ManaCost = 40,
                      School = MagicSchool.Mysticism,
                      IsAOESpell = true,
                      AOERadius = 2,
                      SpellEffect = "The caster conjures a large opening in the ground, swallowing any who happens to be standing there. The wizard must have LOS to at least 1 of the squares. The hole covers 4 squares and any model with their whole base inside that range must make a DEX Test or perish. That also means an X-Large creature will not be affected by this spell. The party gets the XP for any creatures that perish. Any furniture or traps in these squares also disappears. The hole then immediately closes up."
                    },
                    new Spell(){
                      Name = "Life Force",
                      Level = 6,
                      CastingValue = 20,
                      ManaCost = 30,
                      School = MagicSchool.Restoration,
                      SpellEffect = "This spell restores all of a hero's Hit Points."
                    },
                    new Spell(){
                      Name = "Raise Dead",
                      Level = 6,
                      CastingValue = 25,
                      ManaCost = 15,
                      UpkeepCost = 5,
                      TurnDuration = -1,
                      School = MagicSchool.Necromancy,
                      SpellEffect = "The caster may try to raise a defeated Lower Undead or dead human in LOS. Add one hero activation token to the bag immediately. Any Zombie or Skeleton raised will retain its stats and equipment. Any raised human will gain the stats of a zombie and retain its weapon, but armour will be 0."
                    },
                    new Spell(){
                      Name = "Summon Greater Demon",
                      Level = 6,
                      CastingValue = 30,
                      ManaCost = 25,
                      TurnDuration = -1,
                      AddCasterLvlToDuration = true,
                      School = MagicSchool.Conjuration,
                      SpellEffect = "The caster draws a demon from its dimension to do his biddings. The demon is placed in a random place on the same tile as the wizard and fights for the caster for 1d3+Caster Level turns. Once in our plane, the demon will relish fighting, so no upkeep is needed. However, making a pact with a Greater Demon comes at a price, no matter how skilled a wizard you may be. Deduct 1d6 Sanity Points from the caster."
                    },
                    new Spell(){
                      Name = "Teleportation",
                      Level = 6,
                      CastingValue = 14,
                      ManaCost = 20,
                      School = MagicSchool.Alteration,
                      SpellEffect = "The wizard may teleport one of his companions within LOS or himself up to 4 squares. This is risky business though, and a failed spell will cost the target one Sanity Point as he is partly in the void before coming back."
                    }
                };
        }

        internal List<Spell> GetSpellsByLevel(int level)
        {
          List<Spell> list = new List<Spell>();
          foreach (Spell spell in Spells)
          {
            if (spell.Level == level)
            {
              list.Add(spell);
            }
          }
          return list;
        }

        public List<Prayer> GetPrayers()
    {
      return new List<Prayer>()
                {
                new Prayer(){
                  Name = "Bringer of Light",
                  Level = 1,
                  PrayerEffect = "The light of the gods shines through the priest, causing the Undead to waver. Any Undead trying to attack the Warrior Priest suffers -10 CS.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "The Power of Iphy",
                  Level = 1,
                  PrayerEffect = "This empowering psalm strengthens your resolve. The party gets +10 RES on any Fear or Terror Test during the battle. If they already have failed these tests, they may retake them with this bonus.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Charas, Walk with Us",
                  Level = 1,
                  PrayerEffect = "This prayer goes to Charus and as long as he listens, all heroes regain an Energy Point on any skill roll of 01-10, instead of the normal 01-05. Note that this only affects energy, not the other options you have if you roll 01-05. However, the priest will be too busy with the prayer to benefit from this.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Metheia's Ward",
                  Level = 1,
                  PrayerEffect = "Under the protection of Metheia, the priest regains 1 lost HP at the start of his activation, for the rest of the battle.",
                  Duration = "For the rest of the battle."
                },
                new Prayer(){
                  Name = "Power of the Gods",
                  Level = 1,
                  PrayerEffect = "By channelling the power of the gods and diverting it to a wizard, the priest can help conjure a spell. As long as the prayer is active, any hero wizard gains +10 Arcane Arts.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Litany of Metheia",
                  Level = 2,
                  PrayerEffect = "Metheia watches over the heroes and grants them her power of life. Every hero that passes a RES test at the start of their activation regains 1 HP.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Power of Faith",
                  Level = 2,
                  PrayerEffect = "The gods grant your party inner strength, making them immune to fear, and treating terror as fear. Heroes already scared will regain their courage if this prayer succeeds.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Smite The Heretics!",
                  Level = 2,
                  PrayerEffect = "The wrath of the gods renders the flesh of your enemies. At the start of each turn, the enemies within 4 squares of the priest must pass a RES test or lose 1 HP.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Verse of The Sane",
                  Level = 2,
                  PrayerEffect = "As long as this verse is read, the heroes are less prone to mental scars. Each event that would trigger a loss of a Sanity Point is negated by a RES test. If the test succeeds, the Sanity Point is not lost.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Shield of the Gods",
                  Level = 2,
                  PrayerEffect = "The gods will protect the pious, and as long as this prayer is active, any wizard will be protected from miscast. Any Miscast roll can be ignored, although the priest will instead have to pass a RES test, or suffer 1d4 DMG with no armour and NA.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Strengths of Ohlnir",
                  Level = 3,
                  PrayerEffect = "The party feels invigorated, and their weapons feel like feathers in their hands. All members of the party gain +10 Strength.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Warriors of Ramos",
                  Level = 3,
                  PrayerEffect = "As if the gods guide the weapons of the heroes, all seem to fight with renewed power. All members of the party fight with +5 CS.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Stay Thy Hand!",
                  Level = 3,
                  PrayerEffect = "The enemies seem to slow down, as if questioning whether to fight or not. All enemies within 4 squares of the priest must pass a Resolve Test and will lose 1 Action Point during that turn. Test at the start of every turn. This effect is not cumulative with any other effect causing an enemy to lose an action. For instance, a wounded enemy will not be affected by this prayer.",
                  Duration = "Until enemy test succeeds (tested at start of every turn)."
                },
                new Prayer(){
                  Name = "Be Gone!",
                  Level = 3,
                  PrayerEffect = "The enemies seem to slow down, as if questioning whether to fight or not. All enemies within 4 squares of the priest must pass a Resolve Test and will lose 1 Action Point during that turn. Test at the start of every turn. This effect is not cumulative with any other effect causing an enemy to lose an action. For instance, a wounded enemy will not be affected by this prayer.",
                  Duration = "Until enemy test succeeds (tested at start of every turn)."
                },
                new Prayer(){
                  Name = "Providence of Metheia",
                  Level = 3,
                  PrayerEffect = "Metheia will shield its children and protect them from harm. While this prayer is active, all heroes get +10 CON when rolling to resist disease or poison.",
                  Duration = "While prayer is active (until end of next battle, or 4 turns if used between battles)."
                },
                new Prayer(){
                  Name = "We Shall Not Falter",
                  Level = 4,
                  PrayerEffect = "The power of the gods strengthens the party, making them more resilient than ever. All members of the party gain +5 HP that can temporarily give a hero more HP than its current max. After the battle, this goes back to the normal max HP.",
                  Duration = "Until end of battle."
                },
                new Prayer(){
                  Name = "God's Champion",
                  Level = 4,
                  PrayerEffect = "The priest fights like a dervish, imbued by the power of his or her god. Combat Skill is increased by +15 but after the battle, the priest loses an additional Point of Energy. If there are not enough points, the Constitution of the priest is halved (RDD) until the next short rest or until the heroes leave the dungeon.",
                  Duration = "Until end of battle (effect on energy/constitution is after battle)."
                }
            };
    }

        internal List<Prayer> GetPrayersByLevel(int level)
        {
          List<Prayer> list = new List<Prayer>();
          foreach (Prayer prayer in Prayers)
          {
            if (prayer.Level == level)
            {
              list.Add(prayer);
            }
          }
          return list;
        }

        public List<Talent> GetTalents()
    {
      return new List<Talent>()
                    {
                        new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Catlike",
                            Description = "Your hero moves with grace and has almost supernatural balance. Your hero gains +5 DEX.",
                            IsCatLike = true
                          },
                          new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Fast",
                            Description = "Your hero moves unusually fast and gains a permanent +1 bonus to their Movement stat.",
                            IsFast = true
                          },
                          new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Resilient",
                            Description = "Your hero's brawny physique grants a +5 bonus to the Constitution stat.",
                            IsResilient = true
                          },
                          new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Resistance to Disease",
                            Description = "Your hero seems to have a natural ability to resist diseases. Your hero gets a +10 bonus on Constitution Tests to resist disease.",
                            IsResistDisease = true
                          },
                          new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Resistance to Poison",
                            Description = "Your hero seems to have a natural ability to resist poison. Your hero gets a +10 bonus on Constitution Tests to resist poison.",
                            IsResistPoison = true
                          },
                          new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Strong",
                            Description = "Your hero's exercises have paid off and your hero gains a +5 bonus to her Strength stat.",
                            IsStrong = true
                          },
                          new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Strong Build",
                            Description = "Your hero gains a +2 bonus to her Hit Points stat.",
                            IsStrongBuild = true
                          },
                          new Talent(){
                            Category = TalentCategory.Physical,
                            Name = "Tank",
                            Description = "Wearing heavy armour has little effect on your hero's ability to move. The hero ignores the Clunky Special Rule.",
                            IsTank = true
                          },
                new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Axeman",
                            Description = "Preferring the balance of a good axe, this hero has become a master of using this weapon. He gains +5 CS when using all kinds of axes.",
                            IsAxeman = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Bruiser",
                            Description = "The hero excels at fighting with blunt weapons and gains +5 CS with all hammers, flails, staffs, and morning stars.",
                            IsBruiser = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Death Lament",
                            Description = "When others fall, this hero still stands, refusing to give in. Each time your hero is reduced to 0 Hit Points, roll 1d6: on a result of 1-3, the hero regains 1 Hit Point.",
                            IsDeathLament = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Disarm",
                            Description = "This is a special attack, using the target's DEX as a negative modifier to the attack. If the attack succeeds it inflicts no damage, but causes the enemy to drop his weapon. The enemy must spend his next action trying to pick it up. In order to do so, the enemy will have to succeed with an DEX Test. The enemy will continue until successful. If the hero's attack fails, nothing happens. This can only be used on enemies that are carrying weapons.",
                            IsDisarm = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Dual Wield",
                            Description = "This talent requires a DEX of 60. Any hero with this talent may use a weapon with the Dual Wield Special Rule in its offhand. The attacks are still done as usual with the main weapon, but any hit will add +X DMG to the target. The X is defined in the Weapon Table. Parrying with two weapons is also easier, and any parry while using two weapons has a 1+5 modifier.",
                            IsDualWield = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Fast Reload",
                            Description = "Years of practice makes your hero faster than most and she can reload in the blink of an eye. She may reload bows and sling in the same action as she shoots once per turn. Crossbows may be reloaded in 1 action and fired in the next. An Arbalest can be reloaded in 2 actions, and fired in the next turn.",
                            IsFastReload = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Marksman",
                            Description = "Fighting from afar comes naturally to your hero. The hero gains +5 RS.",
                            IsMarksman = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Mighty Blow",
                            Description = "Your hero is an expert at finding the weak spots of the enemy. Your hero gets a +1 bonus on Damage Rolls with melee weapons.",
                            IsMightyBlow = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Parry Master",
                            Description = "Your hero is adept at protecting himself with a weapon. If the hero has taken the Parry Stance, he may parry twice with a weapon during one turn.",
                            IsParryMaster = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Perfect Shot",
                            Description = "Identifying the weak spots in enemy armour can sometimes make the difference when firing an arrow or bolt from afar. If the Damage Roll is odd, your hero ignores armour (But not NA).",
                            IsPerfectShot = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Riposte Master",
                            Description = "When successfully parrying a strike with her weapon, the hero may automatically cause 2 Points of Damage to that Enemy. May only be done with weapons of class 3 or lower.",
                            IsRiposteMaster = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Sniper",
                            Description = "With practiced ease, your hero cannot seem to miss when taking careful aim. The aim action gives your hero a +15 modifier instead of +10.",
                            IsSniper = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Swordsman",
                            Description = "This hero is very skilled with a blade and gains +5 CS with all types of swords.",
                            IsSwordsman = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Tight Grip",
                            Description = "With unusually strong hands, the hero may add +5 STR when calculating what weapon class he or she can use.",
                            IsTightGrip = true
                          },
                          new Talent(){
                            Category = TalentCategory.Combat,
                            Name = "Tunnel Fighter",
                            Description = "Your hero is accustomed to fighting in tight spaces. +10 CS when fighting in a corridor.",
                            IsTunnelFighter = true
                          },
                new Talent(){
                            Category = TalentCategory.Faith,
                            Name = "Devoted",
                            Description = "The hero gains an extra Energy Point that can only be used for praying.",
                            IsDevoted = true
                          },
                          new Talent(){
                            Category = TalentCategory.Faith,
                            Name = "God's Chosen",
                            Description = "As if by the will of the gods, nothing seems to hurt this priest. +1 Luck.",
                            IsGodsChosen = true
                          },
                          new Talent(){
                            Category = TalentCategory.Faith,
                            Name = "Healer",
                            Description = "This priest has tended many wounds and applies bandages with practiced hands. A bandage applied by this priest will heal +1 HP",
                            IsHealer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Faith,
                            Name = "Messiah",
                            Description = "With a confidence that radiates through the room, no one can help but be inspired. All heroes within LOS of the priest gain +5 Resolve.",
                            IsMessiah = true
                          },
                          new Talent(){
                            Category = TalentCategory.Faith,
                            Name = "Pure",
                            Description = "The radiance of this priest hurts the eyes of all demons. Any demon trying to attack the priest does so at -10 CS.",
                            IsPure = true
                          },
                          new Talent(){
                            Category = TalentCategory.Faith,
                            Name = "Reliquary",
                            Description = "So strong is their faith in the holy relics, that this priest can channel the power of 3 relics, rather than the standard two.",
                            IsReliquary = true
                          },
                    new Talent(){
                            Category = TalentCategory.Alchemist,
                            Name = "Gatherer",
                            Description = "Finding good ingredients in the wild comes naturally to the hero. +10 Alchemy when searching for ingredients in the wild.",
                            IsGatherer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Alchemist,
                            Name = "Harvester",
                            Description = "With precise incisions, the hero can harvest good quality components from fallen enemies. +10 Alchemy when harvesting parts.",
                            IsHarvester = true
                          },
                          new Talent(){
                            Category = TalentCategory.Alchemist,
                            Name = "Keen Eye",
                            Description = "The Alchemist has a keen eye when it comes to finding ingredients. The hero may reroll the result when rolling to see what has been gathered. The second result stands.",
                            IsKeenEye = true
                          },
                          new Talent(){
                            Category = TalentCategory.Alchemist,
                            Name = "Master Healer",
                            Description = "This hero has perfected the art of making Healing Potions. All potions brewed heal +2 Hit Points more than normal.",
                            IsMasterHealer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Alchemist,
                            Name = "Perfect Toss",
                            Description = "The hero has a knack for lobbying bottles in a perfect arc over friends and foes alike. +10 RS when lobbying a potion over the heads of others.",
                            IsPerfectToss = true
                          },
                          new Talent(){
                            Category = TalentCategory.Alchemist,
                            Name = "Poisoner",
                            Description = "The hero is very adept at making all sorts of poisons. Poisons created by this hero always inflict 1 additional Hit Point per turn.",
                            IsPoisoner = true
                          },
                          new Talent(){
                            Category = TalentCategory.Alchemist,
                            Name = "Powerful Potions",
                            Description = "The strength of these heroes' potions is remarkable. All basic stat (Not M) enhancing potions grants an additional +5 bonus.",
                            IsPowerfulPotions = true
                          },
                        new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Charming",
                            Description = "This hero seems to get along with everyone and always draws a smile from those to whom he talks. Well aware of the party lets this hero negotiate all rewards and gains +5% Reward Bonus on all quests.",
                            IsCharming = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Disciplined",
                            Description = "Thanks to a military background, this hero has an increased degree of calmness under pressure. This also spreads to the rest of the party. The hero gains +10 RES and the other members of the party gain +5 RES as long as the hero is not knocked out. The effect on the party is not cumulative if other heroes have the same talent. Furthermore, a hero with this talent will not benefit from the effect of this talent from another hero.",
                            IsDisciplined = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Hunter",
                            Description = "The hero has a knack for finding wild game and knows how best to hunt them. The hero gains +10 to Foraging.",
                            IsHunter = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Lucky",
                            Description = "Some are just luckier than others. Everything seems to go your way. You gain +1 Luck Point.",
                            IsLucky = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Master Cook",
                            Description = "During a rest, the party members will regain +2 extra HP if they have rations, due to your hero's expert cooking skills. This is not cumulative if more than one hero has this Talent.",
                            IsMasterCook = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Natural Leader",
                            Description = "The hero's natural ability to lead will add +2 to the Party Moral permanently. This is not cumulative if more than one hero has this talent.",
                            IsNaturalLeader = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Ring Bearer",
                            Description = "Somehow, this hero has managed to tame the effect of magic imbued items. Instead of being limited to one ring, your hero can now use two rings simultaneously.",
                            IsRingBearer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Survivalist",
                            Description = "This talent lets your hero forage one ration from any monster in the Beast category (in a dungeon or after a skirmish), as long as the Forage roll is successful.",
                            IsSurvivalist = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Swift Leader",
                            Description = "The party may always add one initiative token to the bag. This is only used to increase chance of activation and all heroes may still only act once per turn. Only one hero per party can have this talent.",
                            IsSwiftLeader = true
                          },
                          new Talent(){
                            Category = TalentCategory.Common,
                            Name = "Veteran",
                            Description = "You have your gear in perfect order, making changes in equipment very easy. You can use equipment from a Quick Slot without spending an Action Point (once per turn).",
                            IsVeteran = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Blood Magic",
                            Description = "The wizard can spend his own life blood to create Mana. For every 2 HP spent, the wizard gains 5 Mana. This transformation can be done for free during the wizard's turn.",
                            IsBloodMagic = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Conjurer",
                            Description = "The wizard is an expert conjurer and gains +5 Arcane Arts whenever casting a Conjuration Spell. Furthermore, the Mana cost for such a spell is reduced with 5.",
                            IsConjurer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Divinator",
                            Description = "The wizard gets +5 Arcane Arts whenever casting a Divination Spell. Furthermore, the Mana cost for such a spell is reduced with 5.",
                            IsDivinator = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Fast Reflexes",
                            Description = "With lightning-fast reflexes, your hero can reach out and touch your enemies when casting spells. Your hero gains a +15 Combat Skill Bonus when casting Touch Spells.",
                            IsFastReflexes = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Focused",
                            Description = "Well attuned to the void, your hero is adept at tapping into it to gain maximum power. Your hero gets +15 Arcane Arts when focusing.",
                            IsFocused = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Restorer",
                            Description = "Restoration spells are the favourite spells of your hero, and this results in all Healing Spells healing +2 Hit Points in addition to the spell's normal result. You cannot have this talent at the same time as you have the Necromancer Talent.",
                            IsRestorer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Mystic",
                            Description = "The wizard is truly skilled with Mysticism Spells and gets +5 Arcane Arts whenever casting a Mysticism Spell. Furthermore, the Mana cost for such a spell is reduced with 5.",
                            IsMystic = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Necromancer",
                            Description = "The hero gets +5 Arcane Arts whenever casting a Necromantic Spell. Furthermore, the Mana cost for such a spell is reduced with 5. You cannot have this Talent at the same time as you have the Restorer Talent.",
                            IsNecromancer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Powerful Missiles",
                            Description = "Your hero has perfected the use of Magic Missiles, knowing where to aim for maximum effect. Magic Missile Spells do +1 Damage.",
                            IsPowerfulMissiles = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Summoner",
                            Description = "Reaching into other realms and bringing other beings to his aid has become easier with years of practice. Your hero gets +5 on all Summoning Spells.",
                            IsSummoner = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Sustainer",
                            Description = "Upkeep for the wizard's spells is reduced by 1.",
                            IsSustainer = true
                          },
                          new Talent(){
                            Category = TalentCategory.Magic,
                            Name = "Thrifty",
                            Description = "The wizard requires 2 Mana less on every spell cast.",
                            IsThrifty = true
                          },
                        new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Assassin",
                            Description = "With uncanny precision, the hero will automatically hit any target from behind with a class 1 or 2 weapon.",
                            IsAssassin = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Backstabber",
                            Description = "Accustomed to optimizing the odds, your hero ignores enemy armour and NA when attacking from behind.",
                            IsBackstabber = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Cutpurse",
                            Description = "Once per visit in a settlement, your hero may try to steal the purse from some unsuspecting victim. This must be done as the first thing when entering a settlement. Roll 1d6. On a result of 1-2 the hero gains 1d100 coins. On a result of 6, the attempt is detected, and the hero is immediately chased out of the settlement. The hero may do nothing until the rest of the party decides to leave the settlement. Rations must be used as normal, and if rations are lacking, the hero becomes hungry. Foraging is allowed while waiting.",
                            IsCutpurse = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Evaluate",
                            Description = "Your hero has a good sense for the value of things. A successful Barter Roll will give your hero +15% instead of the usual +10%.",
                            IsEvaluate = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Lock Picker",
                            Description = "No lock seems to hinder this hero from beating them. +5 Pick Locks skill.",
                            IsLockPicker = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Mechanical Genius",
                            Description = "Your hero is a master at understanding mechanical contraptions and gain +10 when disarming traps.",
                            IsMechanicalGenius = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Nimble",
                            Description = "The hero may dodge twice per battle instead of only once.",
                            IsNimble = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Quick Fingers",
                            Description = "Accustomed to working under pressure, your hero has mastered the skill of reading a lock and picking it. Picking a lock now takes 1AP instead of 2.",
                            IsQuickFingers = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Sharp-eyed",
                            Description = "Your hero has an extreme sense for details and can easily notice anything out of the ordinary. Your hero gains a +10 bonus on Perception Tests.",
                            IsSharpEyed = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Sense for Gold",
                            Description = "It seems this hero can almost smell his way to treasures. When rolling on the Furniture Table for treasures, the hero may subtract -1 to the roll.",
                            IsSenseForGold = true
                          },
                          new Talent(){
                            Category = TalentCategory.Sneaky,
                            Name = "Trap Finder",
                            Description = "Your hero is an expert at dealing with traps. Your hero gains a +10 PER bonus when avoiding traps. This is cumulative with Sharp-eyed.",
                            IsTrapFinder = true
                          },
                            new Talent(){
                            Category = TalentCategory.Mental,
                            Name = "Braveheart",
                            Description = "Your hero is braver than most. +10 bonus on Fear and Terror Tests.",
                            IsBraveheart = true
                        },
                        new Talent(){
                            Category = TalentCategory.Mental,
                            Name = "Confident",
                            Description = "No enemy or task is too difficult. Your hero gains a +5 bonus to the Resolve stat.",
                            IsConfident = true
                        },
                        new Talent(){
                            Category = TalentCategory.Mental,
                            Name = "Fearless",
                            Description = "Your hero is completely immune to the effects of fear and treats terror as fear. This talent requires that the hero already has the Braveheart Mental Talent.",
                            IsFearless = true
                        },
                        new Talent()
                        {
                            Category = TalentCategory.Mental,
                            Name = "Hate",
                            Description = "This hate fuels their fighting, granting a +5 bonus to CS when attacking these enemies. However, so blind is their hatred that their focus on parrying and dodging diminishes (-5 penalty) when struck by them.",
                            IsHate = true
                        },
                        new Talent(){
                            Category = TalentCategory.Mental,
                            Name = "Strong-Minded",
                            Description = "Your hero is less affected by the horrors he faces in the dungeons than his comrades. He gains +1 Sanity Point.",
                            IsStrongMinded = true
                        },
                        new Talent(){
                            Category = TalentCategory.Mental,
                            Name = "Wise",
                            Description = "Your hero gains a permanent +5 bonus to the Wisdom stat.",
                            IsWise = true
                        }
            };
    }

        public List<Talent> GetTalentCategoryAtLevelup(Profession profession, int level)
    {

      switch (profession.Name)
      {
        case "Alchemist":
          return level switch
          {
            3 => MentalTalents,
            4 => CommonTalents,
            6 => CombatTalents,
            7 => MentalTalents,
            8 => CommonTalents,
            _ => AlchemistTalents
          };
        case "Barbarian":
          return level switch
          {
            2 => PhysicalTalents,
            4 => MentalTalents,
            5 => CommonTalents,
            7 => PhysicalTalents,
            9 => CommonTalents,
            _ => CombatTalents
          };
        case "Ranger":
          return level switch
          {
            2 => PhysicalTalents,
            4 => CommonTalents,
            5 => MentalTalents,
            7 => PhysicalTalents,
            8 => CommonTalents,
            9 => MentalTalents,
            _ => CombatTalents
          };
        case "Rogue":
          return level switch
          {
            2 => PhysicalTalents,
            3 => SneakyTalents,
            5 => MentalTalents,
            6 => PhysicalTalents,
            8 => SneakyTalents,
            9 => CommonTalents,
            _ => CombatTalents
          };
        case "Thief":
          return level switch
          {
            3 => CommonTalents,
            5 => CombatTalents,
            6 => MentalTalents,
            7 => PhysicalTalents,
            8 => CommonTalents,
            9 => CombatTalents,
            _ => SneakyTalents
          };
        case "Warrior":
          return level switch
          {
            2 => MentalTalents,
            4 => PhysicalTalents,
            6 => CommonTalents,
            7 => MentalTalents,
            9 => CommonTalents,
            _ => CombatTalents
          };
        case "Warrior Priest":
          return level switch
          {
            2 => MentalTalents,
            4 => CombatTalents,
            5 => PhysicalTalents,
            7 => CombatTalents,
            8 => MentalTalents,
            10 => CombatTalents,
            _ => FaithTalents
          };
        case "Wizard":
          return level switch
          {
            3 => CommonTalents,
            4 => MentalTalents,
            6 => MentalTalents,
            7 => PhysicalTalents,
            9 => CommonTalents,
            10 => MentalTalents,
            _ => MagicTalents
          };
        default: return CommonTalents;
      }
    }

        public Talent GetRandomTalent()
        {
          return GetRandomTalentByCategory();
        }

        public Talent GetRandomTalentByCategory(TalentCategory? category = null)
    {
      switch (category)
      {
        case TalentCategory.Physical:
          return PhysicalTalents[RandomHelper.GetRandomNumber(0, PhysicalTalents.Count - 1)];
        case TalentCategory.Combat:
          return CombatTalents[RandomHelper.GetRandomNumber(0, CombatTalents.Count - 1)];
        case TalentCategory.Faith:
          return FaithTalents[RandomHelper.GetRandomNumber(0, FaithTalents.Count - 1)];
        case TalentCategory.Alchemist:
          return AlchemistTalents[RandomHelper.GetRandomNumber(0, AlchemistTalents.Count - 1)];
        case TalentCategory.Common:
          return CommonTalents[RandomHelper.GetRandomNumber(0, CommonTalents.Count - 1)];
        case TalentCategory.Magic:
          return MagicTalents[RandomHelper.GetRandomNumber(0, MagicTalents.Count - 1)];
        case TalentCategory.Sneaky:
          return SneakyTalents[RandomHelper.GetRandomNumber(0, SneakyTalents.Count - 1)];
        case TalentCategory.Mental:
          return MentalTalents[RandomHelper.GetRandomNumber(0, MentalTalents.Count - 1)];
        default:
          return Talents[RandomHelper.GetRandomNumber(0, Talents.Count - 1)];
      }
    }

        private List<Talent> GetTalentsByCategory(TalentCategory category)
        {
          return Talents.Where(t => t.Category == category).ToList();
        }

        public Talent GetTalentByName(string name)
        {
          return Talents.FirstOrDefault(t => t.Name == name) ?? new Talent();
        }

        public enum HateCategory
    {
      Bandits,
      Bats,
      Beastmen,
      Centipedes,
      DarkElves,
      Demons,
      Dragons,
      Elementals,
      Froglings,
      Geckos,
      Ghosts,
      Ghouls,
      Giants,
      Gnolls,
      Goblins,
      Golems,
      Minotaurs,
      Mummies,
      Ogres,
      Orcs,
      Rats,
      Saurians,
      Scorpions,
      Skeletons,
      Snakes,
      Spiders,
      Toads,
      Trolls,
      Vampires,
      Werewolves,
      Wights,
      Wolves,
      Zombies
    }

        public Talent GetHateByName(HateCategory? hateCategory)
    {
      string baseDescription = "This hate fuels their fighting, granting a +5 bonus to CS when attacking these enemies. However, so blind is their hatred that their focus on parrying and dodging diminishes (-5 penalty) when struck by them.";
      int roll = 0;
      if (!hateCategory.HasValue)
      {
        roll = RandomHelper.GetRandomNumber(1, 33);
      }
      else
      {
        roll = (int)hateCategory + 1;
      }

      return roll switch
      {
        1 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Bandits",
          Description = $"This talent applies to any enemy with 'Bandit' in its name. {baseDescription}",
          IsHate = true
        },
        2 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Bats",
          Description = $"This talent applies to any enemy with 'Bat' in its name. {baseDescription}",
          IsHate = true
        },
        3 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Beastmen",
          Description = $"This talent applies to any enemy with 'Beastman' in its name. {baseDescription}",
          IsHate = true
        },
        4 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Centipedes",
          Description = $"This talent applies to any enemy with 'Centipede' in its name. {baseDescription}",
          IsHate = true
        },
        5 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Dark Elves",
          Description = $"This talent applies to any enemy with 'Dark Elf' in its name. {baseDescription}",
          IsHate = true
        },
        6 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Demons",
          Description = $"This talent applies to any enemy with 'Demon' in its name. {baseDescription}",
          IsHate = true
        },
        7 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Dragons",
          Description = $"This talent applies to any enemy with 'Dragon' in its name. {baseDescription}",
          IsHate = true
        },
        8 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Elementals",
          Description = $"This talent applies to any enemy with 'Elemental' in its name. {baseDescription}",
          IsHate = true
        },
        9 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Froglings",
          Description = $"This talent applies to any enemy with 'Frogling' in its name. {baseDescription}",
          IsHate = true
        },
        10 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Geckos",
          Description = $"This talent applies to any enemy with 'Gecko' in its name. {baseDescription}",
          IsHate = true
        },
        11 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Ghosts",
          Description = $"This talent applies to ethereal undead like Ghosts, Banshees, and Wraiths. {baseDescription}",
          IsHate = true
        },
        12 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Ghouls",
          Description = $"This talent applies to any enemy with 'Ghoul' in its name. {baseDescription}",
          IsHate = true
        },
        13 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Giants",
          Description = $"This talent applies to any enemy with 'Giant' in its name. {baseDescription}",
          IsHate = true
        },
        14 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Gnolls",
          Description = $"This talent applies to any enemy with 'Gnoll' in its name. {baseDescription}",
          IsHate = true
        },
        15 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Goblins",
          Description = $"This talent applies to any enemy with 'Goblin' in its name. {baseDescription}",
          IsHate = true
        },
        16 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Golems",
          Description = $"This talent applies to any enemy with 'Golem' in its name. {baseDescription}",
          IsHate = true
        },
        17 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Minotaurs",
          Description = $"This talent applies to any enemy with 'Minotaur' in its name. {baseDescription}",
          IsHate = true
        },
        18 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Mummies",
          Description = $"This talent applies to any enemy with 'Mummy' in its name. {baseDescription}",
          IsHate = true
        },
        19 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Ogres",
          Description = $"This talent applies to any enemy with 'Ogre' in its name. {baseDescription}",
          IsHate = true
        },
        20 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Orcs",
          Description = $"This talent applies to any enemy with 'Orc' in its name. {baseDescription}",
          IsHate = true
        },
        21 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Rats",
          Description = $"This talent applies to any enemy with 'Rat' in its name. {baseDescription}",
          IsHate = true
        },
        22 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Saurians",
          Description = $"This talent applies to any enemy with 'Saurian' in its name. {baseDescription}",
          IsHate = true
        },
        23 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Scorpions",
          Description = $"This talent applies to any enemy with 'Scorpion' in its name. {baseDescription}",
          IsHate = true
        },
        24 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Skeletons",
          Description = $"This talent applies to any enemy with 'Skeleton' in its name. {baseDescription}",
          IsHate = true
        },
        25 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Snakes",
          Description = $"This talent applies to any enemy with 'Snake' in its name. {baseDescription}",
          IsHate = true
        },
        26 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Spiders",
          Description = $"This talent applies to any enemy with 'Spider' in its name. {baseDescription}",
          IsHate = true
        },
        27 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Toads",
          Description = $"This talent applies to any enemy with 'Toad' in its name. {baseDescription}",
          IsHate = true
        },
        28 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Trolls",
          Description = $"This talent applies to any enemy with 'Troll' in its name. {baseDescription}",
          IsHate = true
        },
        29 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Vampires",
          Description = $"This talent applies to any enemy with 'Vampire' in its name. {baseDescription}",
          IsHate = true
        },
        30 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Werewolves",
          Description = $"This talent applies to any enemy with 'Werewolf' in its name. {baseDescription}",
          IsHate = true
        },
        31 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Wights",
          Description = $"This talent applies to any enemy with 'Wight' in its name. {baseDescription}",
          IsHate = true
        },
        32 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Wolves",
          Description = $"This talent applies to any enemy with 'Wolf' in its name. {baseDescription}",
          IsHate = true
        },
        33 => new Talent()
        {
          Category = TalentCategory.Mental,
          Name = "Hate Zombies",
          Description = $"This talent applies to any enemy with 'Zombie' in its name. {baseDescription}",
          IsHate = true
        },
        _ => new Talent()
      };
    }

        public List<Perk> GetPerks()
    {
      return new List<Perk>()
                       {
                            new Perk(){
                                Category = PerkCategory.Leader,
                                Name = "Call to Action",
                                Effect = "Your hero lets out a battle shout that gives another hero a chance to spring into immediate action. You may use this Perk when activating the hero. Once the 2 AP has been spent, you may take a hero token from the bag and activate another hero within LOS of the hero using this Perk.",
                                Comment = ""
                              },
                              new Perk(){
                                Category = PerkCategory.Leader,
                                Name = "Encouragement",
                                Effect = "Your hero's encouragement strengthens the hearts of his comrades, giving them +10 on an upcoming Fear or Terror Test.",
                                Comment = "May be used outside of the ordinary acting order whenever a fear or test is necessary."
                              },
                              new Perk(){
                                Category = PerkCategory.Leader,
                                Name = "Keep Calm and Carry On!",
                                Effect = "Your hero's Resolve keeps the party together. You may increase Party Morale with +2.",
                                Comment = "This may not increase morale above starting morale."
                              },
                              new Perk(){
                                Category = PerkCategory.Leader,
                                Name = "Rally",
                                Effect = "The hero tries to encourage a comrade to action! A hero that has failed a Fear or Terror Test may immediately retake that test.",
                                Comment = "May be used outside of the ordinary acting order whenever a fear or test is necessary."
                        },
                               new Perk(){
                                Category = PerkCategory.Common,
                                Name = "Ignore Wounds",
                                Effect = "Hero gains Natural Armour 2",
                                Comment = "Lasts for one battle."
                              },
                              new Perk(){
                                Category = PerkCategory.Common,
                                Name = "Sixth Sense",
                                Effect = "May add +20 to a dodge result when attacked or +20 to a Perception Test when avoiding a triggered trap.",
                                Comment = ""
                              },
                              new Perk(){
                                Category = PerkCategory.Common,
                                Name = "Sprint",
                                Effect = "Your hero may use one Point of Energy to move up to 6 squares with the first movement. A second movement is still allowed but with the standard half movement.",
                                Comment = ""
                              },
                              new Perk(){
                                Category = PerkCategory.Common,
                                Name = "Taunt",
                                Effect = "Your hero knows exactly how to trigger the enemy. Your hero can force an enemy, that is not locked in close combat, to attack him ignoring normal targeting procedure.",
                                Comment = "Chose which enemy to taunt before rolling."
                              },
                              new Perk(){
                                Category = PerkCategory.Common,
                                Name = "Taste for Blood",
                                Effect = "Character evokes blood lust on a To Hit' roll of 01-10 instead of 01-05 for an entire battle.",
                                Comment = "Must be used before Damage Roll but lasts the entire battle."
                        },
                              new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Battle Fury",
                            Effect = "Using his inner energy, your hero may perform 2 Power Attacks in one turn as if they only cost 1 AP.",
                            Comment = ""
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Deadly Strike",
                            Effect = "Adds +25 CS to your next attack.",
                            Comment = ""
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Frenzy",
                            Effect = "Working herself into a frenzy, your hero flails wildly at her enemies. For every attack that damages the enemy, she may attack again. This attack does not have to be at the same target. While frenzied, the hero may only move or attack and may do nothing else, including parrying or dodging.",
                            Comment = "Barbarians only. Takes 1 AP to activate. Lasts for one battle."
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Hunter's Eye",
                            Effect = "Your hero may shoot two arrows when performing a Ranged Attack with a bow. Both arrows must target the same enemy. Roll for each attack separately.",
                            Comment = "Can only be activated when using a bow or a sling."
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Perfect Aim",
                            Effect = "Your hero's aim is spot on. Add +25 to RS to your next Ranged Attack.",
                            Comment = ""
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Powerful Blow",
                            Effect = "Your hero attacks with all her strength, causing 1d6 extra damage.",
                            Comment = "Must be decided before the attack is made."
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Shield Bash",
                            Effect = "Pushes one enemy of the same size or smaller one square. Target must make a Dexterity Test or fall over, spending its next action to stand again. Hero may occupy target's square afterwards.",
                            Comment = "Flyers are immune. Requires a Heater or Tower shield."
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Shield Wall",
                            Effect = "Years of training lets your hero handle that shield like a pro. You may parry twice during one turn while in Parry Stance.",
                            Comment = "May be used when attacked."
                          },
                          new Perk(){
                                Category = PerkCategory.Combat,
                            Name = "Stunning Strike",
                            Effect = "Your hero may choose to stun the enemy instead of inflicting wounds. Your hero performs a Standard Attack with a -10 CS penalty and if the attack is successful, the enemy must pass a RES test or it may perform NO actions during its next turn.",
                            Comment = "Only melee weapons. Does not work on X-Large creatures and Large creatures only lose 1 AP."
                          },
                          new Perk(){
                                Category = PerkCategory.Sneaky,
                            Name = "Clever Fingers",
                            Effect = "Relying on her experience, her fingers dance across the mechanism. Add +25 bonus to a single pick lock or disarming trap attempt.",
                            Comment = "Use before rolling."
                          },
                          new Perk(){
                                Category = PerkCategory.Sneaky,
                            Name = "Hide in the Shadows",
                            Effect = "Your hero finds that perfect spot to avoid drawing attention. No enemy will target him if they are more than 2 squares away when they start their turn. If your model is adjacent to an enemy, that enemy will always attack another adjacent model if there is one. If not, the Perk will not work.",
                            Comment = ""
                          },
                          new Perk(){
                                Category = PerkCategory.Sneaky,
                            Name = "Living on Nothing",
                            Effect = "Accustomed to hardship, your hero can sustain themselves on almost nothing. Spending an Energy Point to activate this Perk is considered the same as consuming a ration.",
                            Comment = "The Energy Point cannot be regained in the same rest in which it was spent."
                          },
                          new Perk(){
                                Category = PerkCategory.Sneaky,
                            Name = "Loot Goblin",
                            Effect = "Whenever told to roll for an amount of gold, your hero may decide to re-roll the result once.",
                            Comment = "The decision to use this Perk is done after the first dice roll. Second result will stand."
                          },
                          new Perk(){
                                Category = PerkCategory.Sneaky,
                            Name = "Lucky Git",
                            Effect = "As always, lady fortune is smiling at your hero. Reduce the Threat Level by 2.",
                            Comment = ""
                          },
                          new Perk(){
                                Category = PerkCategory.Sneaky,
                            Name = "Quick Dodge",
                            Effect = "Quick reflexes may make all the difference. Hero may dodge, even if the normal dodge is expended.",
                            Comment = "May be used when attacked."
                          },
                          new Perk(){
                                Category = PerkCategory.Sneaky,
                            Name = "Strike to Injure",
                            Effect = "Your hero targets the enemy with extreme precision, striking the most vulnerable area. Ignore the enemy's armour for all your attacks this turn.",
                            Comment = "Must be declared before attacking"
                          },
                          new Perk(){
                                Category = PerkCategory.Faith,
                            Name = "God's Favorite",
                            Effect = "Your hero is well attuned to the gods, and they always seem to listen to him. Once again, his prayer is heard, and all problems seem smaller. Decrease the Threat Level by 1d6.",
                            Comment = ""
                          },
                          new Perk(){
                                Category = PerkCategory.Faith,
                            Name = "Fate Forger",
                            Effect = "By spending an Energy Point, the priest can force a reroll of the Scenario die.",
                            Comment = "Used as soon as the Scenario die has been rolled."
                          },
                          new Perk(){
                                Category = PerkCategory.Faith,
                            Name = "Healer",
                            Effect = "Putting that extra effort into tending a wound can make such a difference. When applying a bandage, this Perk adds +3 HP to the result.",
                            Comment = "Used at the same time as the Healing Skill."
                          },
                          new Perk(){
                                Category = PerkCategory.Faith,
                            Name = "My Will Be Done",
                            Effect = "Using his inner strength, the priest manifests tremendous Resolve. Add +10 RES.",
                            Comment = "Lasts until end of next battle"
                          },
                          new Perk(){
                                Category = PerkCategory.Arcane,
                            Name = "Dispel Master",
                            Effect = "The wizard is very skilled in the art of countering enemy magic.",
                            Comment = "The wizard gets +10 when rolling to dispel when this Perk is used."
                          },
                          new Perk(){
                                Category = PerkCategory.Arcane,
                            Name = "Energy to Mana",
                            Effect = "The wizard has the ability to turn energy into Mana. For each Energy Point spent, the wizard gains 5 Mana.",
                            Comment = "The wizard may spend any number of Energy Points in one go."
                          },
                          new Perk(){
                                Category = PerkCategory.Arcane,
                            Name = "Inner Power",
                            Effect = "The wizard increases the power of his magic missiles, causing an extra 1d6 Damage.",
                            Comment = "Must be declared before the spell is cast."
                          },
                          new Perk(){
                                Category = PerkCategory.Arcane,
                            Name = "In Tune with the Magic",
                            Effect = "Caster may use Focus before trying to identify a Magic Item. However, when attuning herself to the magic that way, she opens the mind enough to risk her Sanity.",
                            Comment = "Works just as if casting a spell but introduces miscast to the roll as well. 1 Action of Focus will give a miscast on 95-00. Increase the risk with 5 for each action."
                          },
                          new Perk(){
                                Category = PerkCategory.Arcane,
                            Name = "Quick Focus",
                            Effect = "The wizard has the ability of extreme Focus, increasing the chance to succeed with a spell. Add +10 Arcane Arts Skill without spending an action on focus. Risk for miscast is still increased.",
                            Comment = "Used at the same time as casting a spell. Only lasts for that spell."
                          },
                          new Perk(){
                                Category = PerkCategory.Alchemist,
                            Name = "Careful Touch",
                            Effect = "By taking a little extra time, that specific specimen can be perfect. Chance of getting an Exquisite Ingredient or Part is increased to 20.",
                            Comment = "Declare before harvesting or gathering."
                          },
                          new Perk(){
                                Category = PerkCategory.Alchemist,
                            Name = "Connoisseur",
                            Effect = "The alchemist has a knack for identifying potions brewed by others. Grants a +10 bonus to the Alchemy roll on your next attempt to identify a potion (only one per Energy Point).",
                            Comment = "Energy Point is spent at the same time as you try to identify the potion."
                          },
                          new Perk(){
                                Category = PerkCategory.Alchemist,
                            Name = "Perfect Healer",
                            Effect = "Your hero's perfect mixing increases the potency of her potions. The Healing Potion heals +3 HP.",
                            Comment = "Used at the same time as the potion is mixed."
                          },
                          new Perk(){
                                Category = PerkCategory.Alchemist,
                            Name = "Pitcher",
                            Effect = "With that extra second to aim, your alchemist can throw a bottle with a perfect arc. Grants a +10 RS bonus to your next attempt to throw a potion.",
                            Comment = "Only lasts for one potion and must be declared before throwing."
                          },
                          new Perk(){
                                Category = PerkCategory.Alchemist,
                            Name = "Precise Mixing",
                            Effect = "Good mixing skills make all the difference. The alchemist may choose to reroll the result when rolling to see what potions have been created.",
                            Comment = "Energy may be spent after the first dice roll. The second result stands."
                          },
                          new Perk(){
                                Category = PerkCategory.Alchemist,
                            Name = "Surgeon",
                            Effect = "Taking a deep breath to calm the nerves, the alchemist can remove most parts with precision. The alchemist may choose what part to harvest.",
                            Comment = "Only works on one enemy per Energy Point spent."
                          }
            };
    }

        public List<Perk>? GetPerkCategoryAtLevelup(Profession profession, int level)
    {
      switch (profession.Name)
      {
        case "Alchemist":
          return level switch
          {
            2 => AlchemistPerks,
            4 => LeaderPerks,
            6 => CombatPerks,
            8 => AlchemistPerks,
            10 => CommonPerks,
            _ => null,
          };
        case "Barbarian":
          return level switch
          {
            2 => CombatPerks,
            4 => CommonPerks,
            6 => CombatPerks,
            8 => CommonPerks,
            10 => CombatPerks,
            _ => null,
          };
        case "Ranger":
          return level switch
          {
            2 => CombatPerks,
            4 => CommonPerks,
            6 => CombatPerks,
            8 => CommonPerks,
            10 => CommonPerks,
            _ => null,
          };
        case "Rogue":
          return level switch
          {
            2 => CombatPerks,
            4 => SneakyPerks,
            6 => CommonPerks,
            8 => CombatPerks,
            10 => SneakyPerks,
            _ => null,
          };
        case "Thief":
          return level switch
          {
            2 => SneakyPerks,
            4 => CommonPerks,
            6 => SneakyPerks,
            8 => CombatPerks,
            10 => SneakyPerks,
            _ => null,
          };
        case "Warrior":
          return level switch
          {
            2 => LeaderPerks,
            4 => CombatPerks,
            6 => CombatPerks,
            8 => CommonPerks,
            10 => CombatPerks,
            _ => null,
          };
        case "Warrior Priest":
          return level switch
          {
            2 => FaithPerks,
            4 => LeaderPerks,
            6 => CombatPerks,
            8 => FaithPerks,
            10 => CommonPerks,
            _ => null,
          };
        case "Wizard":
          return level switch
          {
            2 => ArcanePerks,
            4 => LeaderPerks,
            6 => ArcanePerks,
            8 => CommonPerks,
            10 => ArcanePerks,
            _ => null,
          };
        default: return CommonPerks;
      }
    }

        public List<Perk> GetPerksByCategory(PerkCategory category)
        {
          return Perks.Where(p => p.Category == category).ToList();
        }

        public Perk GetPerkByName(string name)
        {
          return Perks.FirstOrDefault(t => t.Name == name) ?? new Perk();
        }

        public List<Species> GetSpecies()
    {
      return new List<Species>()
            {
                new Species() {
                    Name = "Dwarf",
                    Description = "Dwarves are short, but broad and often muscular after their mandatory service in the mines under the mountains of the world. Their beards grow thick and long, and they serve as a symbol of their status. The longer the beard, the more respect they earn amongst their kindred. Female dwarves are every bit as sturdy as their male counterparts, and are seen just as often on the battlefield wielding an axe or warhammer. There have been numerous conflicts between Dwarf and Goblin clans, and with the Dwarves inability to forget a misdeed against them, this has led to a full-blown hatred.",
                    BaseStrength = 40,
                    BaseConstitution =  30,
                    BaseDexterity =  25,
                    BaseWisdom =  25,
                    BaseResolve =  30,
                    BaseHitPoints =  8
                },
                new Species() {
                    Name = "Elf",
                    Description = "Elves are fair skinned, graceful, and often beautiful beings. They move gracefully, but can be exceptionally fast. Whilst Dwarves like to live far underground, Elves prefer to live in the forests of the world. Elven fighters are renowned for their skill with bows, but they often make good Wizards as well.",
                    BaseStrength = 25,
                    BaseConstitution =  20,
                    BaseDexterity =  40,
                    BaseWisdom =  35,
                    BaseResolve =  30,
                    BaseHitPoints =  6
                },
                new Species() {
                    Name = "Human",
                    Description = "Humans are the most versatile of all species, with a wide range of physical characteristics and abilities. They are known for their adaptability and resourcefulness, as well as their ability to form complex societies. With their versatility, they could be anything from Wizards to lowly Thieves",
                    BaseStrength = 30,
                    BaseConstitution =  30,
                    BaseDexterity =  30,
                    BaseWisdom =  30,
                    BaseResolve =  30,
                    BaseHitPoints =  7
                },
                new Species() {
                    Name = "Halfling",
                    Description = "Halfling's are small and nimble, with a love for adventure and exploration. They are known for their quick reflexes and stealthy nature, as well as their ability to blend in with their surroundings. They often gravity towards stealthier professions, such as thieves or rougues. There are not many known Halfling Wizards or alchemists, but this is not unheard of. Their preferred weapons are daggers, shorswords, or ranged weapons, such as shortbows and slings.",
                    BaseStrength = 20,
                    BaseConstitution =  20,
                    BaseDexterity =  40,
                    BaseWisdom =  30,
                    BaseResolve =  40,
                    BaseHitPoints =  5
                }
            };
    }

        public int GetDamageBonusFromSTR(int strength)
        {
          return strength switch
          {
            < 60 => 0,
            < 70 => 1,
            < 80 => 2,
            _ => 3,
          };
        }

        public int GetNaturalArmourFromCON(int constitution)
    {
      return constitution switch
      {
        < 50 => 0,
        < 55 => 1,
        < 60 => 2,
        < 65 => 3,
        < 70 => 4,
        _ => 5,
      };
    }

        public int Get1HWeaponClass(int strength)
        {
            return strength switch
            {
                < 40 => 2,
                < 50 => 3,
                >= 50 => 4
            };
        }

        public int Get2HWeaponClass(int strength)
        {
            return strength switch
            {
                < 30 => 2,
                < 40 => 3,
                < 55 => 4,
                >= 55 => 5
            };
        }

        public string GetWieldStatus(int strength, Weapon weapon)
        {
            if (weapon.Class != 6 && Get2HWeaponClass(strength) < weapon.Class)
            {
                return "(Too weak to wield)";
            }
            if (Get1HWeaponClass(strength) >= weapon.Class && !weapon.Properties.ContainsKey(WeaponProperty.BFO))
            {
                return "(1-Handed)";
            }
            return "(2-Handed)";
        }

        public List<Profession> GetProfessions()
    {
      return new List<Profession>()
            {
                new Profession {
                    Name = "Wizard",
                    Description = "Wizards have studied the magical arts at one of the sorcery colleges. These colleges admit only a select few, and the power that they wield can be quite remarkable in the eyes of commoners. Consequently, most wizards are regarded with suspicion. Their spells range from petty Hedge Magic, which is little more than conjuring tricks, to powerful Battle Magic or Spells which allow the wizard to bend the will of demons. However, dealing with magic is dangerous and any mistake can ravage your mind.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = -10,
                    DodgeSkillModifier = -10,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = 5,
                    HealSkillModifier = -5,
                    AlchemySkillModifier = -20,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = 10,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "RangedSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 2,
                    MaxMeleeWeaponType = 5,
                    TalentChoices = [GetTalentByName("Wise"), GetTalentByName("Charming")],
                    StartingBackpackList = new List<Equipment>(){EquipmentService.GetMeleeWeaponByName("Staff")},
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 5 },
                      {"DEX", 4 },
                      {"CON", 4 },
                      {"WIS", 2 },
                      {"RES", 3 },
                      {"CS", 5 },
                      {"RS", 4 },
                      {"Dodge", 3 },
                      {"PickLocks", 4 },
                      {"Perception", 2 },
                      {"Heal", 2 },
                      {"ArcaneArts", 1 },
                      {"Foraging", 5 },
                      {"Barter", 1 },
                      {"Alchemy", 3 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Rogue",
                    Description = "The Rogue walks a thin line between what is legal and what is illegal. Some overstep the line completely to become highwaymen, robbing unfortunate passers-by. Others take day-to-day work drifting from one town to another. Accustomed to a harsh life and surviving on their own, they are Jacks of all trades, ready to do whatever it takes to make a living.",
                    CombatSkillModifier = 0,
                    RangedSkillModifier = 0,
                    DodgeSkillModifier = 0,
                    PickLocksSkillModifier = -5,
                    BarterSkillModifier = 5,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -25,
                    PerceptionSkillModifier = 0,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = 0,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "PickLocksSkill",
                      "HealSkill",
                      "AlchemySkill"
                    ],
                    HPModifier = 1,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Shortsword/Rapier"],
                    StartingBackpackList = [
                        EquipmentService.GetArmourByName("Padded Jacket"),
                        EquipmentService.GetEquipmentByNameSetQuantity("Lock Picks", 10),
                        EquipmentService.GetEquipmentByName("Backpack - Medium") ],
                    StartingTalentList = [ GetTalentByName("Backstabber"),
                        new Talent() {
                            Category = TalentCategory.Sneaky,
                            Name = "Streetwise",
                            Description = "Your hero knows who to turn to in order to acquire the gear he is searching for. Every roll this hero makes for availability may be modified with -1.",
                            IsStreetwise = true
                        } ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 3},
                      {"DEX", 2},
                      {"CON", 3},
                      {"WIS", 4},
                      {"RES", 3 },
                      {"CS", 3},
                      {"RS", 3 },
                      {"Dodge", 3 },
                      {"PickLocks", 3 },
                      {"Perception", 3 },
                      {"Heal", 3 },
                      {"Foraging", 3 },
                      {"Barter", 3 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Ranger",
                    Description = "The Ranger spends his or her days in the wild. They make their living by tracking animals and selling their meat and pelts. Rangers earn a meagre income, but with time they will acquire unrivalled knowledge in how to survive in the wild, and they will seldom go hungry. Constant exposure to the weather and wandering the forests day after day also makes them quite tough and resilient. Their favourite weapon is, of course, the bow. However, some prefer the heavier crossbow for its sheer stopping power.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = 15,
                    DodgeSkillModifier = -5,
                    PickLocksSkillModifier = -25,
                    BarterSkillModifier = -20,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -20,
                    PerceptionSkillModifier = 0,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = 15,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "BarterSkill",
                      "HealSkill",
                      "AlchemySkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    TalentChoices = [GetTalentByName("Marksman"), GetTalentByName("Hunter")],
                    StartingBackpackList = [
                        EquipmentService.GetRangedWeaponByName("Longbow"),
                        EquipmentService.GetAmmoByNameSetQuantity("Arrow", 10) ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 3 },
                      {"DEX", 2},
                      {"CON", 1},
                      {"WIS", 4},
                      {"RES", 3},
                      {"CS", 3 },
                      {"RS", 1 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 2 },
                      {"Heal", 2 },
                      {"Barter", 3 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Barbarian",
                    Description = "Barbarians live for the thrill of battle. Unlike most sane people, Barbarians eagerly await the next possibility for a good fight, and they often work themselves up into a frenzy once the battle starts. This frenzy causes them to wield their weapons like dervishes, striking out left and right, which makes them formidable. On the other hand, being in the grips of such a frenzy makes it easy to abandon caution and to forget to properly protect yourself.",
                    CombatSkillModifier = 15,
                    RangedSkillModifier = -10,
                    DodgeSkillModifier = 5,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = -15,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -25,
                    PerceptionSkillModifier = -5,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -15,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "RangedSkill",
                      "PickLocksSkill",
                      "BarterSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 2,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = [ "Weapon of Choice" ],
                    StartingPerkList = [GetPerkByName("Frenzy")],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 2 },
                      {"DEX", 2},
                      {"CON", 2},
                      {"WIS", 5},
                      {"RES", 3},
                      {"CS", 1 },
                      {"RS", 3 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 4 },
                      {"Heal", 4 },
                      {"Foraging", 4 },
                      {"Barter", 5 },
                      {"Alchemy", 5 },
                      {"HitPoints", 5 }
                    }
                },
                new Profession {
                    Name = "Warrior Priest",
                    Description = "Warrior Priests have taken it upon themselves to act as soldiers of their god, preaching to those who will listen, and smiting those that they regard as heretics. Luckily, their codex will normally limit those deemed as heretics to the monsters of the world, or to those who choose to dabble with dark magic or evil gods. Their faith makes them unwavering in front of the most fearsome foes. The more experienced priests have learned to perfect the art of battle prayers, bestowing some blessings from their god to their comrades in arms.",
                    CombatSkillModifier = 5,
                    RangedSkillModifier = -5,
                    DodgeSkillModifier = -5,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = -10,
                    HealSkillModifier = 5,
                    AlchemySkillModifier = -15,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = 15,
                    FreeSkills = [
                      "RangedSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "BarterSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 1,
                    MaxArmourType = 4,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Weapon of choice", "Religious Relic of choice" ],
                    TalentChoices = [ GetTalentByName("Braveheart"), GetTalentByName("Confident") ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 3 },
                      {"DEX", 3},
                      {"CON", 3},
                      {"WIS", 3},
                      {"RES", 2},
                      {"CS", 2 },
                      {"RS", 2 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 4 },
                      {"Heal", 2 },
                      {"BattlePrayers", 1 },
                      {"Foraging", 4 },
                      {"Barter", 3 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Warrior",
                    Description = "The Warrior is a veteran of the King's army and has more often than not seen countless battles. In contrast with the Berserker, the Warrior does not relish the fight and is more than happy to skip the risk it brings with it. However, when battle is unavoidable, the Warrior is effective in combat with a good all-round set of skills.",
                    CombatSkillModifier = 10,
                    RangedSkillModifier = 5,
                    DodgeSkillModifier = 0,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = -15,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -25,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -15,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "PickLocksSkill",
                      "BarterSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 3,
                    MaxArmourType = 4,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Weapon of Choice"],
                    TalentChoices = [ GetTalentByName("Mighty Blow"), GetTalentByName("Braveheart")],
                    StartingBackpackList = [EquipmentService.GetArmourByName("Leather Jacket") ],
                    StartingTalentList = [GetTalentByName("Disciplined") ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 2 },
                      {"DEX", 2},
                      {"CON", 2},
                      {"WIS", 5},
                      {"RES", 3},
                      {"CS", 1 },
                      {"RS", 2 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 4 },
                      {"Heal", 4 },
                      {"Foraging", 4 },
                      {"Barter", 4 },
                      {"Alchemy", 5 },
                      {"HitPoints", 5 }
                    }
                },
                new Profession {
                    Name = "Alchemist",
                    Description = "The Alchemist has spent years studying the properties of materials and solutions, learning the effect they have on each other and on the human body. Through both study and experience, they have learned how to mix ingredients to obtain beneficial effects that have become highly sought after. Their ability to create powerful concoctions have made them popular amongst adventurers. Many alchemists can hold their ground pretty well, spreading fire and destruction all around them.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = -5,
                    DodgeSkillModifier = -10,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = 0,
                    HealSkillModifier = 5,
                    AlchemySkillModifier = 10,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "RangedSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Potions x 3 of choice", "Parts x 3 of choice", "Recipe of choice"],
                    StartingBackpackList = [
                        EquipmentService.GetEquipmentByName("Alchemist Tool"),
                        EquipmentService.GetEquipmentByName("Alchemist Belt"),
                        EquipmentService.GetMeleeWeaponByName("Shortsword"),
                        AlchemyService.GetIngredients(3)[0],
                        AlchemyService.GetIngredients(3)[0],
                        AlchemyService.GetIngredients(3)[0]
                        ],
                    StartingTalentList = [ GetTalentByName("Resistance To Poison") ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 5 },
                      {"DEX", 4},
                      {"CON", 4},
                      {"WIS", 2},
                      {"RES", 3},
                      {"CS", 3 },
                      {"RS", 3 },
                      {"Dodge", 4 },
                      {"PickLocks", 4 },
                      {"Perception", 2 },
                      {"Heal", 3 },
                      {"Foraging", 4 },
                      {"Barter", 3 },
                      {"Alchemy", 1 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Thief",
                    Description = "The Thief prefers to work in the shadows, avoiding all attention if possible. The use of arms is not alien to them, but it is seen as a last resort. Better to take what you want undetected, and thereby minimise the risk of getting injured whilst doing it. As a consequence, daggers are their preferred weapons. Lock picks and crowbars are the tools of the trade. Special: Whenever it is time to get treasure, a thief may always get two choices and choose which one to keep. This ability may be combined with the sense of gold talent.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = 5,
                    DodgeSkillModifier = 5,
                    PickLocksSkillModifier = 10,
                    BarterSkillModifier = 0,
                    HealSkillModifier = -20,
                    AlchemySkillModifier = -30,
                    PerceptionSkillModifier = 10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 2,
                    StartingBackpackList = [
                        EquipmentService.GetMeleeWeaponByName("Dagger"),
                        EquipmentService.GetEquipmentByName("Rope"),
                        EquipmentService.GetEquipmentByNameSetQuantity("Lock Picks", 10) ],
                    StartingTalentList = [ GetTalentByName("Evaluate") ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 5},
                      {"DEX", 2},
                      {"CON", 4},
                      {"WIS", 3},
                      {"RES", 3 },
                      {"CS", 5},
                      {"RS", 2 },
                      {"Dodge", 1 },
                      {"PickLocks", 1 },
                      {"Perception", 1 },
                      {"Heal", 4 },
                      {"Foraging", 4 },
                      {"Barter", 2 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                }
            };
    }

        public List<ActiveStatusEffect> GetStatusEffects()
        {
            return new List<ActiveStatusEffect>()
            {
                new ActiveStatusEffect(StatusEffectType.FireBurning, 1),
                new ActiveStatusEffect(StatusEffectType.AcidBurning, 1),
                new ActiveStatusEffect(StatusEffectType.Poisoned, RandomHelper.GetRandomNumber(1, RandomHelper.RollDie("D10"))),
                new ActiveStatusEffect(StatusEffectType.Diseased, 0),
                new ActiveStatusEffect(StatusEffectType.Stunned, 1),
                new ActiveStatusEffect(StatusEffectType.Prone, 1),
                new ActiveStatusEffect(StatusEffectType.Pit, 0),
                // TODO: Update duration of below effects
                new ActiveStatusEffect(StatusEffectType.BleedingOut, 0),
                new ActiveStatusEffect(StatusEffectType.Fear, 0),
                new ActiveStatusEffect(StatusEffectType.Terror, 0),
                new ActiveStatusEffect(StatusEffectType.Entangled, 0),
                new ActiveStatusEffect(StatusEffectType.Petrified, 0),
                new ActiveStatusEffect(StatusEffectType.Incapacitated, 0),
                new ActiveStatusEffect(StatusEffectType.BeingSwallowed, 0),
                new ActiveStatusEffect(StatusEffectType.Swallowed, 0),
                new ActiveStatusEffect(StatusEffectType.Ensnared, 0),
                new ActiveStatusEffect(StatusEffectType.IgnoreWounds, 0),
                new ActiveStatusEffect(StatusEffectType.Sprint, 0),
                new ActiveStatusEffect(StatusEffectType.BattleFury, 0),
                new ActiveStatusEffect(StatusEffectType.Frenzy, 0),
                new ActiveStatusEffect(StatusEffectType.HideInShadows, 0),
                new ActiveStatusEffect(StatusEffectType.MyWillBeDone, 0),
                new ActiveStatusEffect(StatusEffectType.NightVision, 0),
                new ActiveStatusEffect(StatusEffectType.PerfectHearing, 0),
                new ActiveStatusEffect(StatusEffectType.ResistanceToDisease, 0),
                new ActiveStatusEffect(StatusEffectType.ResistanceToPoison, 0),
                new ActiveStatusEffect(StatusEffectType.Tank, 0),
                new ActiveStatusEffect(StatusEffectType.Axeman, 0),
                new ActiveStatusEffect(StatusEffectType.Bruiser, 0),
                new ActiveStatusEffect(StatusEffectType.DeathLament, 0),
                new ActiveStatusEffect(StatusEffectType.DualWield, 0),
                new ActiveStatusEffect(StatusEffectType.FastReload, 0),
                new ActiveStatusEffect(StatusEffectType.Marksman, 0),
                new ActiveStatusEffect(StatusEffectType.MightyBlow, 0),
                new ActiveStatusEffect(StatusEffectType.ParryMaster, 0),
                new ActiveStatusEffect(StatusEffectType.PerfectShot, 0),
                new ActiveStatusEffect(StatusEffectType.RiposteMaster, 0),
                new ActiveStatusEffect(StatusEffectType.Sniper, 0),
                new ActiveStatusEffect(StatusEffectType.TunnelFighter, 0),
                new ActiveStatusEffect(StatusEffectType.Healer, 0),
                new ActiveStatusEffect(StatusEffectType.Messiah, 0),
                new ActiveStatusEffect(StatusEffectType.Pure, 0),
                new ActiveStatusEffect(StatusEffectType.Gatherer, 0),
                new ActiveStatusEffect(StatusEffectType.Harvester, 0),
                new ActiveStatusEffect(StatusEffectType.PerfectToss, 0),
                new ActiveStatusEffect(StatusEffectType.Disciplined, 0),
                new ActiveStatusEffect(StatusEffectType.Conjurer, 0),
                new ActiveStatusEffect(StatusEffectType.Divinator, 0),
                new ActiveStatusEffect(StatusEffectType.FastReflexes, 0),
                new ActiveStatusEffect(StatusEffectType.Focused, 0),
                new ActiveStatusEffect(StatusEffectType.Restorer, 0),
                new ActiveStatusEffect(StatusEffectType.Mystic, 0),
                new ActiveStatusEffect(StatusEffectType.PowerfulMissiles, 0),
                new ActiveStatusEffect(StatusEffectType.Summoner, 0),
                new ActiveStatusEffect(StatusEffectType.Sustainer, 0),
                new ActiveStatusEffect(StatusEffectType.Thrifty, 0),
                new ActiveStatusEffect(StatusEffectType.Assassin, 0),
                new ActiveStatusEffect(StatusEffectType.Backstabber, 0),
                new ActiveStatusEffect(StatusEffectType.LockPicker, 0),
                new ActiveStatusEffect(StatusEffectType.MechanicalGenius, 0),
                new ActiveStatusEffect(StatusEffectType.QuickFingers, 0),
                new ActiveStatusEffect(StatusEffectType.SharpEyed, 0),
                new ActiveStatusEffect(StatusEffectType.SenseForGold, 0),
                new ActiveStatusEffect(StatusEffectType.TrapFinder, 0),
                new ActiveStatusEffect(StatusEffectType.BraveHeart, 0),
                new ActiveStatusEffect(StatusEffectType.Fearless, 0),
                new ActiveStatusEffect(StatusEffectType.HateBandits, 0),
                new ActiveStatusEffect(StatusEffectType.HateBats, 0),
                new ActiveStatusEffect(StatusEffectType.HateBeastmen, 0),
                new ActiveStatusEffect(StatusEffectType.HateCentipedes, 0),
                new ActiveStatusEffect(StatusEffectType.HateDarkElves, 0),
                new ActiveStatusEffect(StatusEffectType.HateDemons, 0),
                new ActiveStatusEffect(StatusEffectType.HateDragons, 0),
                new ActiveStatusEffect(StatusEffectType.HateElementals, 0),
                new ActiveStatusEffect(StatusEffectType.HateFroglings, 0),
                new ActiveStatusEffect(StatusEffectType.HateGeckos, 0),
                new ActiveStatusEffect(StatusEffectType.HateGhosts, 0),
                new ActiveStatusEffect(StatusEffectType.HateGhouls, 0),
                new ActiveStatusEffect(StatusEffectType.HateGiants, 0),
                new ActiveStatusEffect(StatusEffectType.HateGnolls, 0),
                new ActiveStatusEffect(StatusEffectType.HateGoblins, 0),
                new ActiveStatusEffect(StatusEffectType.HateGolems, 0),
                new ActiveStatusEffect(StatusEffectType.HateMinotaurs, 0),
                new ActiveStatusEffect(StatusEffectType.HateMummies, 0),
                new ActiveStatusEffect(StatusEffectType.HateOgres, 0),
                new ActiveStatusEffect(StatusEffectType.HateOrcs, 0),
                new ActiveStatusEffect(StatusEffectType.HateRats, 0),
                new ActiveStatusEffect(StatusEffectType.HateSaurians, 0),
                new ActiveStatusEffect(StatusEffectType.HateScorpions, 0),
                new ActiveStatusEffect(StatusEffectType.HateSkeletons, 0),
                new ActiveStatusEffect(StatusEffectType.HateSnakes, 0),
                new ActiveStatusEffect(StatusEffectType.HateSpiders, 0),
                new ActiveStatusEffect(StatusEffectType.HateToads, 0),
                new ActiveStatusEffect(StatusEffectType.HateTrolls, 0),
                new ActiveStatusEffect(StatusEffectType.HateVampires, 0),
                new ActiveStatusEffect(StatusEffectType.HateWerewolves, 0),
                new ActiveStatusEffect(StatusEffectType.HateWights, 0),
                new ActiveStatusEffect(StatusEffectType.HateWolves, 0),
                new ActiveStatusEffect(StatusEffectType.HateZombies, 0),
                new ActiveStatusEffect(StatusEffectType.Claustrophobia, 0),
                new ActiveStatusEffect(StatusEffectType.Arachnophobia, 0),
                new ActiveStatusEffect(StatusEffectType.BadTempered, 0),
                new ActiveStatusEffect(StatusEffectType.Poverty, 0),
                new ActiveStatusEffect(StatusEffectType.TheFraud, 0),
                new ActiveStatusEffect(StatusEffectType.TheRealDeal, 0),
                new ActiveStatusEffect(StatusEffectType.TheNoble, 0),
                new ActiveStatusEffect(StatusEffectType.TheApprentice, 0),
                new ActiveStatusEffect(StatusEffectType.BringerOfLight, 0),
                new ActiveStatusEffect(StatusEffectType.PowerOfTheGods, 0),
                new ActiveStatusEffect(StatusEffectType.ThePowerOfIphy, 0),
                new ActiveStatusEffect(StatusEffectType.MetheiasWard, 0),
                new ActiveStatusEffect(StatusEffectType.LitanyOfMetheia, 0),
                new ActiveStatusEffect(StatusEffectType.SmiteTheHeretics, 0),
                new ActiveStatusEffect(StatusEffectType.ShieldOfTheGods, 0),
                new ActiveStatusEffect(StatusEffectType.PowerOfFaith, 0),
                new ActiveStatusEffect(StatusEffectType.VerseOfTheSane, 0),
                new ActiveStatusEffect(StatusEffectType.StrengthOfOhlnir, 0),
                new ActiveStatusEffect(StatusEffectType.StayThyHand, 0),
                new ActiveStatusEffect(StatusEffectType.ProvidenceOfMetheia, 0),
                new ActiveStatusEffect(StatusEffectType.WarriorOfRamos, 0),
                new ActiveStatusEffect(StatusEffectType.BeGone, 0),
                new ActiveStatusEffect(StatusEffectType.WeShallNotFalter, 0),
                new ActiveStatusEffect(StatusEffectType.GodsChampion, 0),
                new ActiveStatusEffect(StatusEffectType.ProtectiveShield, 0),
                new ActiveStatusEffect(StatusEffectType.FakeDeath, 0),
                new ActiveStatusEffect(StatusEffectType.GustOfWind, 0),
                new ActiveStatusEffect(StatusEffectType.StrengthenBody, 0),
                new ActiveStatusEffect(StatusEffectType.Silence, 0),
                new ActiveStatusEffect(StatusEffectType.Blur, 0),
                new ActiveStatusEffect(StatusEffectType.MagicArmour, 0),
                new ActiveStatusEffect(StatusEffectType.Slow, 0),
                new ActiveStatusEffect(StatusEffectType.Corruption, 0),
                new ActiveStatusEffect(StatusEffectType.ControlUndead, 0),
                new ActiveStatusEffect(StatusEffectType.Confuse, 0),
                new ActiveStatusEffect(StatusEffectType.HoldCreature, 0),
                new ActiveStatusEffect(StatusEffectType.IceTomb, 0),
                new ActiveStatusEffect(StatusEffectType.Weakness, 0),
                new ActiveStatusEffect(StatusEffectType.BolsteredMind, 0),
                new ActiveStatusEffect(StatusEffectType.CauseAnimosity, 0),
                new ActiveStatusEffect(StatusEffectType.Levitate, 0),
                new ActiveStatusEffect(StatusEffectType.Speed, 0),
                new ActiveStatusEffect(StatusEffectType.CauseFear, 0),
                new ActiveStatusEffect(StatusEffectType.CauseTerror, 0),
                new ActiveStatusEffect(StatusEffectType.Corrosive, 0),
                new ActiveStatusEffect(StatusEffectType.CursedWeapon, 0),
                new ActiveStatusEffect(StatusEffectType.Demon, 0),
                new ActiveStatusEffect(StatusEffectType.DiseaseRidden, 0),
                new ActiveStatusEffect(StatusEffectType.Ethereal, 0),
                new ActiveStatusEffect(StatusEffectType.WeakToFire, 0),
                new ActiveStatusEffect(StatusEffectType.WeakToWater, 0),
                new ActiveStatusEffect(StatusEffectType.WeakToSilver, 0),
                new ActiveStatusEffect(StatusEffectType.FearElves, 0),
                new ActiveStatusEffect(StatusEffectType.FerociousCharge, 0),
                new ActiveStatusEffect(StatusEffectType.FireDamage, 0),
                new ActiveStatusEffect(StatusEffectType.FrostDamage, 0),
                new ActiveStatusEffect(StatusEffectType.Floater, 0),
                new ActiveStatusEffect(StatusEffectType.Flyer, 0),
                new ActiveStatusEffect(StatusEffectType.GhostlyTouch, 0),
                new ActiveStatusEffect(StatusEffectType.Gust, 0),
                new ActiveStatusEffect(StatusEffectType.HardAsRock, 0),
                new ActiveStatusEffect(StatusEffectType.HateHero, 0),
                new ActiveStatusEffect(StatusEffectType.JustBones, 0),
                new ActiveStatusEffect(StatusEffectType.Large, 0),
                new ActiveStatusEffect(StatusEffectType.XLarge, 0),
                new ActiveStatusEffect(StatusEffectType.Leech, 0),
                new ActiveStatusEffect(StatusEffectType.MagicBeing, 0),
                new ActiveStatusEffect(StatusEffectType.MagicUser, 0),
                new ActiveStatusEffect(StatusEffectType.MultipleAttacksX, 0),
                new ActiveStatusEffect(StatusEffectType.MultipleAttacksHydra, 0),
                new ActiveStatusEffect(StatusEffectType.Poisonous, 0),
                new ActiveStatusEffect(StatusEffectType.Psychic, 0),
                new ActiveStatusEffect(StatusEffectType.Regeneration, 0),
                new ActiveStatusEffect(StatusEffectType.Rend, 0),
                new ActiveStatusEffect(StatusEffectType.RiddleMaster, 0),
                new ActiveStatusEffect(StatusEffectType.Scurry, 0),
                new ActiveStatusEffect(StatusEffectType.Silent, 0),
                new ActiveStatusEffect(StatusEffectType.SimpleWeapons, 0),
                new ActiveStatusEffect(StatusEffectType.Sneaky, 0),
                new ActiveStatusEffect(StatusEffectType.Stench, 0),
                new ActiveStatusEffect(StatusEffectType.Stupid, 0),
                new ActiveStatusEffect(StatusEffectType.WallCrawler, 0)
            };
        }

        public ActiveStatusEffect GetStatusEffectByType(StatusEffectType type)
        {
            return StatusEffects.First(x => x.Category == type);
        }

        public List<MonsterSpell> GetMonsterSpells()
        {
            return new List<MonsterSpell>()
            {
                new MonsterSpell()
                {
                    Name = "Blind",
                    Type = MonsterSpellType.Ranged,
                    Effect = "Through his incantation, the wizard manages to blind a target in LOS. It does not matter if there are intervening models or furniture, as long as the wizard can reasonably see its target. The target is blinded during its next turn and cannot fight. Any character striking at the target gets a +20 CS bonus. The target will walk two squares in random directions (randomize each step)."
                },
                new MonsterSpell()
                {
                    Name = "Flare",
                    Type = MonsterSpellType.Ranged,
                    Effect = "The wizard targets one random target with a magic missile. The target must be in LOS of the caster and takes 1d10 DMG. Armour and NA works as normal."
                },
                new MonsterSpell()
                {
                    Name = "Fireball",
                    Type = MonsterSpellType.Ranged,
                    Effect = "The wizard casts a fireball spell in the square that will hit the most non-allied targets. The spell effects the target square and all adjacent squares. The caster will ignore if this spell hurts any allies. The target square suffers 1d10+2 fire DMG, and the adjacent squares suffers 1d6+1 fire DMG. The spell requires LOS to the target square."
                },
                new MonsterSpell()
                {
                    Name = "Frost ray",
                    Type = MonsterSpellType.Ranged,
                    Effect = "The wizard shoots a ray of frost at a target in LOS. The target suffers 1d8 DMG and is stunned (loses one AP next turn)."
                },
                new MonsterSpell()
                {
                    Name = "Gust of wind",
                    Type = MonsterSpellType.Ranged,
                    Effect = "The wizard summons a gust of wind making the use of projectiles harder. For any ranged weapon where the projectile would pass within 10 squares of the wizard, heroes or enemies suffer a 10 to hit modifier. The spell lasts the entire battle or until the caster is dead."
                },
                new MonsterSpell()
                {
                    Name = "Slow",
                    Type = MonsterSpellType.Ranged,
                    Effect = "The wizard targets one random target that will be slowed. It does not matter if there are intervening models or furniture, as long as the wizard can reasonably see its target. The target may still move, but each square now counts as 2, effectively halving the target's movement (RDD). The spell lasts until the end of the battle, but can be overcome by a successful RES test that may be done at the start of every turn."
                },
                new MonsterSpell()
                {
                    Name = "Mind blast",
                    Type = MonsterSpellType.CloseCombat,
                    Effect = "The wizard touches the target and searing pain shoots through her head. The target loses 1d3 points of sanity. If the target is not a hero, the target instead loses 1d6 HP with no armour or natural armour."
                },
                new MonsterSpell()
                {
                    Name = "Mirrored self",
                    Type = MonsterSpellType.CloseCombat,
                    Effect = "The wizard creates a duplicate of himself in any random empty square on the same tile. This copy is in the same condition as the wizard (i.e. same amount of wounds). Once placed, there is no telling which is the copy and which is the real wizard. Both will fight as normal and cast spells. Once one of them is killed there is a 50% chance that this was the real wizard. In that case, the copy vanishes as well. Otherwise the wizard will continue to fight."
                },
                new MonsterSpell()
                {
                    Name = "Seduce",
                    Type = MonsterSpellType.CloseCombat,
                    Effect = "The wizard uses a seducing spell on an adjacent target. The target must pas a RES test or become seduced. A seduced target will fight for the enemy until having passed a RES test at the start of the turn or the caster dies. During the seduction, the target will act as a beast, or humanoid with a hand weapon/ranged weapon depending on the weapon equipped. The target will never use magic or any perks. Does not work on undead."
                },
                new MonsterSpell()
                {
                    Name = "Stun",
                    Type = MonsterSpellType.CloseCombat,
                    Effect = "The wizard reaches out and touches its target, sending an electric bolt through her. The target is stunned and loses 1 AP during their next turn."
                },
                new MonsterSpell()
                {
                    Name = "Teleportation",
                    Type = MonsterSpellType.CloseCombat,
                    Effect = "The wizard teleports herself to a random location on the same tile that is not adjacent to a non-allied character. If no such square is available, the wizard will instead end up in a random square in any random adjacent tile."
                },
                new MonsterSpell()
                {
                    Name = "Vampiric touch",
                    Type = MonsterSpellType.CloseCombat,
                    Effect = "The wizard reaches out and touches the adjacent target. Through this touch, the wizard absorbs the target's life, adding it to his own life force. The target loses 1d10 HP and neither armour nor natural armour will help. The same amount of HP will be added to the casters HP, although it cannot go beyond his starting HP."
                },
                new MonsterSpell()
                {
                    Name = "Frenzy",
                    Type = MonsterSpellType.Support,
                    Effect = "The wizard targets one ally that gets the frenzy special rule. This lasts until the end of the battle. No LOS required."
                },
                new MonsterSpell()
                {
                    Name = "Healing",
                    Type = MonsterSpellType.Support,
                    Effect = "The wizard heals himself or the most wounded ally with 1d10 HP."
                },
                new MonsterSpell()
                {
                    Name = "Healing hand",
                    Type = MonsterSpellType.Support,
                    Effect = "The wizard heals himself or the most wounded adjacent ally with 1d10 HP."
                },
                new MonsterSpell()
                {
                    Name = "Mute",
                    Type = MonsterSpellType.Support,
                    Effect = "The wizard casts a mute spell, making the use of an enemies spell or prayer more difficult. Any prayer or spell in the same tile as the wizard now requires the enemy warrior priest or wizard to add +15 to the skill check. This spell is in effect until the wizard who cast the mute spell casts another spell."
                },
                new MonsterSpell()
                {
                    Name = "Raise dead",
                    Type = MonsterSpellType.Support,
                    OnlyUndead = true,
                    Effect = "The wizard raises one random, fallen and undead enemy. It will regain all its HP and equipment. If there are no fallen enemies, this spell will instead heal one random undead with 1d6 HP. No LOS required. If the caster is slain, the raised dead will once again fall to the ground and stay dead."
                },
                new MonsterSpell()
                {
                    Name = "Shield",
                    Type = MonsterSpellType.Support,
                    Effect = "The wizard conjures a shield around one random ally, including the caster himself. The target receives +2 armour. The spell lasts until the end of the battle or until the caster dies. There is no need for LOS to the target. A character can only get this bonus once."
                },
                new MonsterSpell()
                {
                    Name = "Summon demon",
                    Type = MonsterSpellType.Support,
                    FullTurn = true,
                    Effect = "The wizard summons a random demon. This is placed in a random empty square in the same tile as the caster. If there is no empty square, it is placed in an adjacent tile. The summoned demon is determined by a d10 roll: 1-6 for a Lesser Plague Demon, 7-8 for a Blood Demon, and 9-10 for a Plague Demon."
                },
                new MonsterSpell()
                {
                    Name = "Summon greater demon",
                    Type = MonsterSpellType.Support,
                    FullTurn = true,
                    Effect = "The wizard summons a random demon. This is placed in a random empty square in the same tile as the caster. If there is no empty square, it is placed in an adjacent tile. This spell can summon 1 demon per battle. The summoned demon is determined by a d10 roll: 1-6 for a Bloated Demon, 7-9 for a Lurker, and 10 for a Greater Demon."
                }
            };
        }

        public MonsterSpell GetMonsterSpellByName(string name)
        {
            return MonsterSpells.First(s => s.Name == name);
        }

        public List<MonsterSpell> GetMonsterSpellsByType(MonsterSpellType type)
        {
            return (List<MonsterSpell>)MonsterSpells.Where(s => s.Type == type);
        }
    }

    public enum DamageType
    {
        Mundane,
        Silver,
        Fire,
        Water,
        Lightning,
        Acid,
        Poison,
        Magic
    }

    public enum MagicSchool
    {
        Necromancy,
        Destruction,
        Alteration,
        Restoration,
        Mysticism,
        Hex,
        Illusion,
        Enchantment,
        Conjuration,
        Divination
    }

    public enum SpellProperty
    {
        QuickSpell,
        Incantation,
        MagicMissile,
        Touch
    }

    public enum MonsterSpellType
    {
        Ranged,
        CloseCombat,
        Support
    }

    public class MonsterSpell
    {
        public string Name { get; set; } = string.Empty;
        public MonsterSpellType Type { get; set; }
        public string Effect { get; set; } = string.Empty;
        public bool OnlyUndead { get; set; }
        public bool FullTurn { get; set; }
    }

    public class Spell
      {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string SpellEffect { get; set; } = string.Empty;
        public int CastingValue { get; set; } // The base difficulty or power of the spell
        public int ManaCost { get; set; }
        public int UpkeepCost { get; set; } // Per turn cost for sustained spells
        public int TurnDuration { get; set; } // Duration in turns
        public bool AddCasterLvlToDuration { get; set; }

        // Damage properties for direct damage spells
        public bool IsDamageSpell { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public bool IncludeCasterLevelInDamage { get; set; }
        public bool IsArmourPiercing { get; set; }
        public DamageType DamageType { get; set; } = DamageType.Magic;

        // Spell Category Flags (can be used for filtering or specific effects)
        public List<SpellProperty>? Properties { get; set; }
        public MagicSchool School { get; set; }

        // AOE properties
        public bool IsAOESpell { get; set; }
        public int AOEMinDamage { get; set; }
        public int AOEMaxDamage { get; set; }
        public int AOERadius { get; set; } // Or target count for EnemiesAOE
        public bool AOEIncludesCasterLevel { get; set; }

        public Spell()
        {
        }

        public override string ToString()
        {
          var sb = new StringBuilder();
          sb.AppendLine($"--- Spell: {Name} {School} (Lvl {Level}) ---");
          sb.AppendLine($"Cost: {ManaCost} Mana | Upkeep: {UpkeepCost} | CV: {CastingValue}");
          if (TurnDuration > 0)
          {
            sb.Append($"Duration: {TurnDuration}" + (AddCasterLvlToDuration ? " + Caster Lvl" : "") + " turns. ");
          }
          sb.AppendLine($"Effect: {SpellEffect}");

          if (IsDamageSpell)
          {
            sb.Append($"Damage: {MinDamage}-{MaxDamage}" + (IncludeCasterLevelInDamage ? " + Caster Lvl" : ""));
            if (IsArmourPiercing) sb.Append(" (AP)");
            sb.AppendLine();
          }
          if (IsAOESpell)
          {
            sb.Append($"AOE Damage: {AOEMinDamage}-{AOEMaxDamage}" + (AOEIncludesCasterLevel ? " + Caster Lvl" : ""));
            sb.AppendLine($" | Radius: {AOERadius}");
          }

          if (Properties != null && Properties.Any())
          {
            sb.AppendLine($"Category: {string.Join(", ", Properties)}");
          }

          return sb.ToString();
        }

        public int GetSpellDamage(int casterLevel)
        {
          if (!IsDamageSpell) return 0;

          int calculatedDamage = RandomHelper.GetRandomNumber(MinDamage, MaxDamage);
          if (IncludeCasterLevelInDamage)
          {
            calculatedDamage += casterLevel;
          }
          return calculatedDamage;
        }

        public int GetSpellDamageAOE(int casterLevel)
        {
          if (!IsAOESpell) return 0;

          int calculatedDamage = RandomHelper.GetRandomNumber(AOEMinDamage, AOEMaxDamage);
          if (AOEIncludesCasterLevel)
          {
            calculatedDamage += casterLevel;
          }
          return calculatedDamage;
        }

        /// <summary>
        /// Represents the casting attempt for the spell.
        /// Actual mana deduction, success/failure handling, and effects on game state
        /// would be managed by a higher-level SpellCastingService or Hero class.
        /// </summary>
        /// <param name="hero">The hero attempting to cast the spell.</param>
        /// <param name="skillRoll">The result of the hero's ArcaneArts skill roll.</param>
        /// <returns>True if the spell was successfully cast, false otherwise.</returns>
        public bool CastSpell(Hero hero, int skillRoll)
        {
          // Simplified logic: Check if hero has enough mana
          if (hero.CurrentEnergy < ManaCost)
          {
            // Optionally log: Console.WriteLine($"{hero.Name} does not have enough mana to cast {SpellName}.");
            return false;
          }

          // Check if skill roll meets or exceeds casting value
          if (skillRoll >= CastingValue)
          {
            hero.CurrentEnergy -= ManaCost; // Deduct mana
                                            // Spell effect would be handled by a SpellCastingService
                                            // Console.WriteLine($"{hero.Name} successfully cast {SpellName}!");
            return true;
          }
          else
          {
            // Optionally log: Console.WriteLine($"{hero.Name} failed to cast {SpellName}.");
            return false;
          }
        }
      }

      public class Prayer
      {
            public string Name { get; set; } = string.Empty;
            public int Level { get; set; }
            public int EnergyCost { get; set; } = 1;
            public bool IsActive { get; set; }
            public string Duration { get; set; } = string.Empty;
            public string PrayerEffect { get; set; } = string.Empty;// This could be an enum or a more complex object if effects become varied.

        // Constructor
        public Prayer()
        {

        }

    public override string ToString()
    {
      return $"[{Name} (Lvl {Level})] Cost: {EnergyCost} Energy | Duration: {Duration} | Effect: {PrayerEffect}";
    }

    // Method to get the prayer effect description
    // This method can be expanded to apply the effect in a game logic service.
    public string GetPrayerEffectDescription()
    {
      return PrayerEffect;
    }

    // Example method for applying the prayer effect (logic would typically be in a service)
    public void ApplyEffect(Hero hero)
    {
      // This is a placeholder. Real logic would depend on the 'PrayerEffect' string
      // or if 'PrayerEffect' was an enum/interface.
      Console.WriteLine($"{hero.Name} is affected by {Name}: {PrayerEffect}");
      // Example: if PrayerName == "Heal" -> hero.HP += amount;
    }
  }

  public enum TalentCategory
  {
    Magic,
    Physical,
    Combat,
    Faith,
    Alchemist,
    Common,
    Sneaky,
    Mental
  }

  public class Talent
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TalentCategory Category { get; set; }
    // Physical Talents
    public bool IsCatLike { get; set; }
    public bool IsFast { get; set; }
    public bool IsNightVision { get; set; }
    public bool IsPerfectHearing { get; set; }
    public bool IsResilient { get; set; }
    public bool IsResistDisease { get; set; }
    public bool IsResistPoison { get; set; }
    public bool IsStrong { get; set; }
    public bool IsStrongBuild { get; set; }
    public bool IsTank { get; set; }

    // Combat Talents
    public bool IsAxeman { get; set; }
    public bool IsBruiser { get; set; }
    public bool IsDeathLament { get; set; }
    public bool IsDisarm { get; set; }
    public bool IsDualWield { get; set; }
    public bool IsFastReload { get; set; }
    public bool IsMarksman { get; set; }
    public bool IsMightyBlow { get; set; }
    public bool IsParryMaster { get; set; }
    public bool IsPerfectShot { get; set; }
    public bool IsRiposteMaster { get; set; }
    public bool IsSniper { get; set; }
    public bool IsSwordsman { get; set; }
    public bool IsTightGrip { get; set; }
    public bool IsTunnelFighter { get; set; }

    // Faith Talents
    public bool IsDevoted { get; set; }
    public bool IsGodsChosen { get; set; }
    public bool IsHealer { get; set; }
    public bool IsMessiah { get; set; }
    public bool IsPure { get; set; }
    public bool IsReliquary { get; set; }

    // Alchemist Talents
    public bool IsGatherer { get; set; }
    public bool IsHarvester { get; set; }
    public bool IsKeenEye { get; set; }
    public bool IsMasterHealer { get; set; }
    public bool IsPerfectToss { get; set; }
    public bool IsPoisoner { get; set; }
    public bool IsPowerfulPotions { get; set; }

    // Common Talents
    public bool IsCharming { get; set; }
    public bool IsDisciplined { get; set; }
    public bool IsHunter { get; set; }
    public bool IsLucky { get; set; }
    public bool IsMasterCook { get; set; }
    public bool IsNaturalLeader { get; set; }
    public bool IsRingBearer { get; set; }
    public bool IsSurvivalist { get; set; }
    public bool IsSwiftLeader { get; set; }
    public bool IsVeteran { get; set; }

    // Magic Talents
    public bool IsBloodMagic { get; set; }
    public bool IsConjurer { get; set; }
    public bool IsDivinator { get; set; }
    public bool IsFastReflexes { get; set; }
    public bool IsFocused { get; set; }
    public bool IsRestorer { get; set; }
    public bool IsMystic { get; set; }
    public bool IsNecromancer { get; set; }
    public bool IsPowerfulMissiles { get; set; }
    public bool IsSummoner { get; set; }
    public bool IsSustainer { get; set; }
    public bool IsThrifty { get; set; }

    // Sneaky Talents
    public bool IsAssassin { get; set; }
    public bool IsBackstabber { get; set; }
    public bool IsCutpurse { get; set; }
    public bool IsEvaluate { get; set; }
    public bool IsLockPicker { get; set; }
    public bool IsMechanicalGenius { get; set; }
    public bool IsNimble { get; set; }
    public bool IsQuickFingers { get; set; }
    public bool IsSharpEyed { get; set; }
    public bool IsSenseForGold { get; set; }
    public bool IsStreetwise { get; set; }
    public bool IsTrapFinder { get; set; }

    // Mental Talents
    public bool IsBraveheart { get; set; }
    public bool IsConfident { get; set; }
    public bool IsFearless { get; set; }
    public bool IsHate { get; set; }
    public bool IsStrongMinded { get; set; }
    public bool IsWise { get; set; }

    public Dictionary<string, int> GetInitialTalentBonus()
    {
      Dictionary<string, int> bonus = new Dictionary<string, int>();
      if (IsCatLike)
      {
        bonus.Add("DEX", 5);
      }
      if (IsResilient)
      {
        bonus.Add("CON", 5);
      }
      if (IsNightVision)
      {
        bonus.Add("PS", 10);
      }
      if (IsStrong)
      {
        bonus.Add("STR", 5);
      }
      if (IsStrongBuild)
      {
        bonus.Add("HP", 2);
      }
      if (IsMarksman)
      {
        bonus.Add("RS", 5);
      }
      if (IsSniper)
      {
        bonus.Add("RS", 10);
      }
      if (IsGodsChosen || IsLucky)
      {
        bonus.Add("Luck", 1);
      }
      if (IsHunter)
      {
        bonus.Add("FS", 10);
      }
      if (IsLockPicker)
      {
        bonus.Add("PLS", 5);
      }
      if (IsConfident)
      {
        bonus.Add("RES", 5);
      }
      if (IsStrongMinded)
      {
        bonus.Add("Sanity", 1);
      }
      if (IsWise)
      {
        bonus.Add("WIS", 5);
      }
      if (IsHate)
      {
        new GameDataService().GetHateByName(null);
      }

      return bonus;
    }

    public override string ToString()
    {
      return $"{Name}: {Description}";
    }

  }

  public enum PerkCategory
  {
    Leader,
    Common,
    Combat,
    Sneaky,
    Faith,
    Arcane,
    Alchemist
  }

  public class Perk
  {
    public string Name { get; set; } = string.Empty;
    public PerkCategory Category { get; set; }
    public string Effect { get; set; } = string.Empty;
    public string? Comment { get; set; }

    public Perk() { }

    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append($"{Name}: {Effect}");
      if (!string.IsNullOrEmpty(Comment))
      {
        sb.Append($" ({Comment})");
      }
      return sb.ToString();
    }

  }
}
