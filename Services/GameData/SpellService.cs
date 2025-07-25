﻿using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Utilities;
using LoDCompanion.Services.Combat;
using System.Text;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Services.GameData
{
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

    public enum SpellTargetType
    {
        SingleTarget, // Affects one character
        AreaOfEffect, // Affects a grid square and its surroundings
        Self,
        Ally,
        NoTarget      // A global effect like Gust of Wind
    }

    public static class SpellService
    {
        public static List<Spell> Spells => GetSpells();
        public static List<MonsterSpell> MonsterSpells => GetMonsterSpells();


        public static string GetRandomSpellNameByCategory(string category = "All", bool isUndead = false)
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

        public static string GetRandomSpellName()
        {
            return GetRandomSpellNameByCategory();
        }

        public static List<Spell> GetSpells()
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
                      TargetType = SpellTargetType.Self,
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
                      TargetType = SpellTargetType.Self,
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
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.Ally,
                      SpellEffect = "Caster may strengthen a hero in LOS with +10 in either STR or CON. The spell lasts for 1d6 turns."
                    },
                    new Spell(){
                      Name = "Summon Lesser Demon",
                      Level = 2,
                      CastingValue = 15,
                      ManaCost = 10,
                      UpkeepCost = 4,
                      School = MagicSchool.Conjuration,
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.Self,
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
                      TargetType = SpellTargetType.Ally,
                      SpellEffect = "The caster may shift the place of two heroes that are in LOS. If the spell fails, both heroes suffer 2 Sanity Points for the ordeal. The caster may not transpose himself."
                    },
                    new Spell(){
                      Name = "Second Sight",
                      Level = 4,
                      CastingValue = 15,
                      ManaCost = 25,
                      School = MagicSchool.Divination,
                      TargetType = SpellTargetType.NoTarget,
                      SpellEffect = "Caster can tell what is on the other side of a door. Place the tile and roll for Encounter before opening a door. The heroes gain 2 activation tokens if there is an encounter on the other side of the door."
                    },
                    new Spell(){
                      Name = "Summon Demon",
                      Level = 4,
                      CastingValue = 25,
                      ManaCost = 15,
                      School = MagicSchool.Conjuration,
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.NoTarget,
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
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.Self,
                      SpellEffect = "The caster makes a copy of herself which may be placed anywhere within 4 squares of the caster. Enemies will treat this mirrored image as a target just like any other hero, even though it cannot take DMG. The mirrored self cannot move or attack. It will last for 1d4 turns."
                    },
                    new Spell(){
                      Name = "Speed",
                      Level = 5,
                      CastingValue = 15,
                      ManaCost = 15,
                      TurnDuration = -1,
                      School = MagicSchool.Mysticism,
                      TargetType = SpellTargetType.Ally,
                      SpellEffect = "May target self or any friendly character in LOS. Character gains +1M. The spell lasts until a Scenario die roll of 9-10."
                    },
                    new Spell(){
                      Name = "Time Freeze",
                      Level = 5,
                      CastingValue = 20,
                      ManaCost = 30,
                      School = MagicSchool.Divination,
                      TargetType = SpellTargetType.Self,
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
                      TargetType = SpellTargetType.AreaOfEffect,
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
                      TargetType = SpellTargetType.AreaOfEffect,
                      SpellEffect = "The caster conjures a large opening in the ground, swallowing any who happens to be standing there. The wizard must have LOS to at least 1 of the squares. The hole covers 4 squares and any model with their whole base inside that range must make a DEX Test or perish. That also means an X-Large creature will not be affected by this spell. The party gets the XP for any creatures that perish. Any furniture or traps in these squares also disappears. The hole then immediately closes up."
                    },
                    new Spell(){
                      Name = "Life Force",
                      Level = 6,
                      CastingValue = 20,
                      ManaCost = 30,
                      School = MagicSchool.Restoration,
                      TargetType = SpellTargetType.Ally,
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
                      TargetType = SpellTargetType.NoTarget,
                      SpellEffect = "The caster draws a demon from its dimension to do his biddings. The demon is placed in a random place on the same tile as the wizard and fights for the caster for 1d3+Caster Level turns. Once in our plane, the demon will relish fighting, so no upkeep is needed. However, making a pact with a Greater Demon comes at a price, no matter how skilled a wizard you may be. Deduct 1d6 Sanity Points from the caster."
                    },
                    new Spell(){
                      Name = "Teleportation",
                      Level = 6,
                      CastingValue = 14,
                      ManaCost = 20,
                      School = MagicSchool.Alteration,
                      TargetType = SpellTargetType.Ally,
                      SpellEffect = "The wizard may teleport one of his companions within LOS or himself up to 4 squares. This is risky business though, and a failed spell will cost the target one Sanity Point as he is partly in the void before coming back."
                    }
                };
        }

        public static List<Spell> GetSpellsByLevel(int level)
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

        public static MonsterSpell GetMonsterSpellByName(string name)
        {
            return MonsterSpells.First(s => s.Name == name);
        }

        public static List<MonsterSpell> GetMonsterSpellsByType(MonsterSpellType type)
        {
            return (List<MonsterSpell>)MonsterSpells.Where(s => s.Type == type);
        }

        public static List<MonsterSpell> GetMonsterSpells()
        {
            return new List<MonsterSpell>()
            {
                // Ranged Spells
                new MonsterSpell()
                {
                    Name = "Blind",
                    Type = MonsterSpellType.Ranged,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.TargetHighestCombatSkillHero,
                    Effect = "Through his incantation, the wizard manages to blind a target in LOS. It does not matter if there are intervening models or furniture, as long as the wizard can reasonably see its target. The target is blinded during its next turn and cannot fight. Any character striking at the target gets a +20 CS bonus. The target will walk two squares in random directions (randomize each step)."
                },
                new MonsterSpell()
                {
                    Name = "Flare",
                    Type = MonsterSpellType.Ranged,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = true,
                    AITargetingHint = AiTargetHints.TargetRandomHero,
                    Effect = "The wizard targets one random target with a magic missile. The target must be in LOS of the caster and takes 1d10 DMG. Armour and NA works as normal."
                },
                new MonsterSpell()
                {
                    Name = "Fireball",
                    Type = MonsterSpellType.Ranged,
                    TargetType = SpellTargetType.AreaOfEffect,
                    AreaOfEffectRadius = 1, // Target square and all adjacent squares
                    DoesDamage = true,
                    AITargetingHint = AiTargetHints.MaximizeHeroTargets,
                    Effect = "The wizard casts a fireball spell in the square that will hit the most non-allied targets. The spell effects the target square and all adjacent squares. The caster will ignore if this spell hurts any allies. The target square suffers 1d10+2 fire DMG, and the adjacent squares suffers 1d6+1 fire DMG. The spell requires LOS to the target square."
                },
                new MonsterSpell()
                {
                    Name = "Frost ray",
                    Type = MonsterSpellType.Ranged,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = true,
                    AITargetingHint = AiTargetHints.TargetRandomHero,
                    Effect = "The wizard shoots a ray of frost at a target in LOS. The target suffers 1d8 DMG and is stunned (loses one AP next turn)."
                },
                new MonsterSpell()
                {
                    Name = "Gust of wind",
                    Type = MonsterSpellType.Ranged,
                    TargetType = SpellTargetType.NoTarget, // Global effect within range
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.DebuffHeroRanged,
                    Effect = "The wizard summons a gust of wind making the use of projectiles harder. For any ranged weapon where the projectile would pass within 10 squares of the wizard, heroes or enemies suffer a 10 to hit modifier. The spell lasts the entire battle or until the caster is dead."
                },
                new MonsterSpell()
                {
                    Name = "Slow",
                    Type = MonsterSpellType.Ranged,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.TargetRandomHero,
                    Effect = "The wizard targets one random target that will be slowed. It does not matter if there are intervening models or furniture, as long as the wizard can reasonably see its target. The target may still move, but each square now counts as 2, effectively halving the target's movement (RDD). The spell lasts until the end of the battle, but can be overcome by a successful RES test that may be done at the start of every turn."
                },

                // Close Combat Spells
                new MonsterSpell()
                {
                    Name = "Mind blast",
                    Type = MonsterSpellType.CloseCombat,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = true, // Damages sanity or HP
                    AITargetingHint = AiTargetHints.TargetAdjacentHero,
                    Effect = "The wizard touches the target and searing pain shoots through their head. The target loses 1d3 points of sanity. If the target is not a hero, the target instead loses 1d6 HP with no armour or natural armour."
                },
                new MonsterSpell()
                {
                    Name = "Mirrored self",
                    Type = MonsterSpellType.CloseCombat,
                    TargetType = SpellTargetType.Self,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.SelfPreservation,
                    Effect = "The wizard creates a duplicate of himself in any random empty square on the same tile. This copy is in the same condition as the wizard (i.e. same amount of wounds). Once placed, there is no telling which is the copy and which is the real wizard. Both will fight as normal and cast spells. Once one of them is killed there is a 50% chance that this was the real wizard. In that case, the copy vanishes as well. Otherwise the wizard will continue to fight."
                },
                new MonsterSpell()
                {
                    Name = "Seduce",
                    Type = MonsterSpellType.CloseCombat,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.TargetHighestCombatSkillHero,
                    Effect = "The wizard uses a seducing spell on an adjacent target. The target must pas a RES test or become seduced. A seduced target will fight for the enemy until having passed a RES test at the start of the turn or the caster dies. During the seduction, the target will act as a beast, or humanoid with a hand weapon/ranged weapon depending on the weapon equipped. The target will never use magic or any perks. Does not work on undead."
                },
                new MonsterSpell()
                {
                    Name = "Stun",
                    Type = MonsterSpellType.CloseCombat,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.TargetAdjacentHero,
                    Effect = "The wizard reaches out and touches its target, sending an electric bolt through her. The target is stunned and loses 1 AP during their next turn."
                },
                new MonsterSpell()
                {
                    Name = "Teleportation",
                    Type = MonsterSpellType.CloseCombat, // Often used for movement in combat
                    TargetType = SpellTargetType.Self,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.SelfPreservation,
                    Effect = "The wizard teleports herself to a random location on the same tile that is not adjacent to a non-allied character. If no such square is available, the wizard will instead end up in a random square in any random adjacent tile."
                },
                new MonsterSpell()
                {
                    Name = "Vampiric touch",
                    Type = MonsterSpellType.CloseCombat,
                    TargetType = SpellTargetType.SingleTarget,
                    DoesDamage = true,
                    AITargetingHint = AiTargetHints.TargetAdjacentHero,
                    Effect = "The wizard reaches out and touches the adjacent target. Through this touch, the wizard absorbs the target's life, adding it to his own life force. The target loses 1d10 HP and neither armour nor natural armour will help. The same amount of HP will be added to the casters HP, although it cannot go beyond his starting HP."
                },

                // Support Spells
                new MonsterSpell()
                {
                    Name = "Frenzy",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.Ally,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.BuffHighestCombatSkillAlly,
                    Effect = "The wizard targets one ally that gets the frenzy special rule. This lasts until the end of the battle. No LOS required."
                },
                new MonsterSpell()
                {
                    Name = "Healing",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.Ally, // Can heal self or ally
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.HealLowestHealthAlly,
                    Effect = "The wizard heals himself or the most wounded ally with 1d10 HP."
                },
                new MonsterSpell()
                {
                    Name = "Healing hand",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.Ally, // Can heal self or adjacent ally
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.HealLowestHealthAdjacentAlly,
                    Effect = "The wizard heals himself or the most wounded adjacent ally with 1d10 HP."
                },
                new MonsterSpell()
                {
                    Name = "Mute",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.NoTarget, // Affects all spells/prayers in the tile
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.DebuffEnemyCaster,
                    Effect = "The wizard casts a mute spell, making the use of an enemies spell or prayer more difficult. Any prayer or spell in the same tile as the wizard now requires the enemy warrior priest or wizard to add +15 to the skill check. This spell is in effect until the wizard who cast the mute spell casts another spell."
                },
                new MonsterSpell()
                {
                    Name = "Raise dead",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.NoTarget, // Affects fallen enemies or random undead
                    OnlyUndead = true,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.ResurrectOrHealUndeadAllies,
                    Effect = "The wizard raises one random, fallen and undead enemy. It will regain all its HP and equipment. If there are no fallen enemies, this spell will instead heal one random undead with 1d6 HP. No LOS required. If the caster is slain, the raised dead will once again fall to the ground and stay dead."
                },
                new MonsterSpell()
                {
                    Name = "Shield",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.Ally, // Can shield self or ally
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.BuffLowestArmourAlly,
                    Effect = "The wizard conjures a shield around one random ally, including the caster himself. The target receives +2 armour. The spell lasts until the end of the battle or until the caster dies. There is no need for LOS to the target. A character can only get this bonus once."
                },
                new MonsterSpell()
                {
                    Name = "Summon demon",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.NoTarget, // Summons a new entity
                    CostAP = 2,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.SummonReinforcements,
                    Effect = "The wizard summons a random demon. This is placed in a random empty square in the same tile as the caster. If there is no empty square, it is placed in an adjacent tile. The summoned demon is determined by a d10 roll: 1-6 for a Lesser Plague Demon, 7-8 for a Blood Demon, and 9-10 for a Plague Demon."
                },
                new MonsterSpell()
                {
                    Name = "Summon greater demon",
                    Type = MonsterSpellType.Support,
                    TargetType = SpellTargetType.NoTarget, // Summons a new entity
                    CostAP = 2,
                    DoesDamage = false,
                    AITargetingHint = AiTargetHints.SummonReinforcements,
                    Effect = "The wizard summons a random demon. This is placed in a random empty square in the same tile as the caster. If there is no empty square, it is placed in an adjacent tile. This spell can summon 1 demon per battle. The summoned demon is determined by a d10 roll: 1-6 for a Bloated Demon, 7-9 for a Lurker, and 10 for a Greater Demon."
                }
            };
        }
    }

    public enum AiTargetHints
    {
        TargetClosest,
        SummonReinforcements,
        BuffLowestArmourAlly,
        ResurrectOrHealUndeadAllies,
        DebuffEnemyCaster,
        HealLowestHealthAdjacentAlly,
        HealLowestHealthAlly,
        BuffHighestCombatSkillAlly,
        TargetAdjacentHero,
        SelfPreservation,
        TargetHighestCombatSkillHero,
        TargetRandomHero,
        MaximizeHeroTargets,
        DebuffHeroRanged
    }

    /// <summary>
    /// A helper class to store a potential spell action and its calculated value.
    /// </summary>
    public class SpellChoice
    {
        public MonsterSpell? Spell { get; set; }
        public GridPosition? Target { get; set; } // The character/entity being targeted.
        public double Score { get; set; }
    }

    public class MonsterSpell
    {
        public string Name { get; set; } = string.Empty;
        public MonsterSpellType Type { get; set; }
        public SpellTargetType TargetType { get; set; } = SpellTargetType.SingleTarget;
        public int AreaOfEffectRadius { get; set; } = 0; // 0 for single target, 1 for target and adjacent squares, etc.
        public bool DoesDamage { get; set; } = true;

        // A simple way to define the AI's goal for this spell
        // e.g., "MaximizeTargets", "TargetLowestHealth", "TargetClosest"
        public AiTargetHints AITargetingHint { get; set; } = AiTargetHints.TargetClosest;

        public string Effect { get; set; } = string.Empty;
        public bool OnlyUndead { get; set; }
        public bool FullTurn { get; set; }
        public int CostAP { get; set; } = 1;

        internal string CastSpell(Monster caster, GridPosition targetPosition, DungeonState dungeon)
        {
            List<Character> characterList = [.. dungeon.RevealedMonsters];
            if (dungeon.HeroParty != null)
            {
                characterList.AddRange(dungeon.HeroParty.Heroes);
            }
            Character? target = characterList.FirstOrDefault(c => c.Position == targetPosition);

            int damage = 0;
            caster.CurrentAP--;
            switch (this.Name)
            {
                case "Blind":
                    if (target == null) break;
                    StatusEffectService.AttemptToApplyStatus(
                        target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Blind)); // Blinded for its next turn
                    return $"{caster.Name} blinds {target.Name}! They are disoriented and cannot fight effectively.";
                case "Flare":
                    if (target == null) break;
                    damage = RandomHelper.RollDie("D10");
                    //TODO: ProcessSpellDamage(damage);
                    return $"{caster.Name} hits {target.Name} with a Flare for {damage} damage.";
                case "Fireball":
                    // The initial target can be null if the center of the blast is an empty square.
                    string outcome = $"{caster.Name} launches a fireball!";

                    // Damage the center square (if a character is there).
                    if (target != null)
                    {
                        int centerDamage = RandomHelper.RollDie("D10") + 2;
                        //TODO: ProcessSpellDamage(centerDamage);
                        outcome += $"\n{target.Name} is at the center, taking {centerDamage} fire damage.";
                    }

                    // Get all adjacent squares and find any characters within them for splash damage.
                    var adjacentSquares = GridService.GetNeighbors(targetPosition, dungeon.DungeonGrid);
                    var charactersInSplash = characterList.Where(c => adjacentSquares.Contains(c.Position)).ToList();

                    foreach (Character splashTarget in charactersInSplash)
                    {
                        int splashDamage = RandomHelper.RollDie("D6") + 1;
                        //TODO: ProcessSpellDamage(splashDamage);
                        outcome += $"\n{splashTarget.Name} is caught in the blast for {splashDamage} damage.";
                    }
                    return outcome;
                case "Frost ray":
                    if (target == null) break;
                    damage = RandomHelper.RollDie("D8");
                    //TODO: ProcessSpellDamage(damage);
                    StatusEffectService.AttemptToApplyStatus(
                        target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Stunned));
                    return $"{caster.Name}'s Frost Ray hits {target.Name} for {damage} damage, stunning them.";
                case "Gust of wind":
                    // This is a global debuff originating from the caster.
                    StatusEffectService.AttemptToApplyStatus(caster, StatusEffectService.GetStatusEffectByType(StatusEffectType.GustOfWindAura));
                    return $"{caster.Name} summons a howling gust of wind, making ranged attacks difficult!";
                case "Slow":
                    if (target == null) break;
                    StatusEffectService.AttemptToApplyStatus(
                        target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Slow));
                    return $"{caster.Name} slows {target.Name}, halving their movement.";

                // --- CLOSE COMBAT SPELLS ---
                case "Mind blast":
                    if (target == null) break;
                    if (target is Hero heroTarget)
                    {
                        int sanityDamage = RandomHelper.RollDie("D3");
                        heroTarget.CurrentSanity -= sanityDamage;
                        return $"{caster.Name} blasts {heroTarget.Name}'s mind, causing {sanityDamage} sanity damage!";
                    }
                    else
                    {
                        damage = RandomHelper.RollDie("D6");
                        //TODO: ProcessSpellDamage(damage); // No armour save
                        return $"{caster.Name} blasts {target.Name}'s mind for {damage} unblockable damage!";
                    }

                case "Mirrored self":
                    // Logic to find an empty square and place a copy of the caster.
                    //TODO: figure out the duplicate monster and the death effect
                    /*
                    var emptySquare = GridService.FindRandomEmptySquareInRoom(caster.Position, caster.Room);
                    if (emptySquare != null)
                    {
                        var mirrorImage = MonsterFactory.CreateCopy(caster);
                        GridService.MoveCharacter(mirrorImage, emptySquare.Position, dungeon.DungeonGrid);
                        dungeon.RevealedMonsters.Add(mirrorImage);
                        return $"{caster.Name} creates a perfect mirror image of itself!";
                    }
                    else
                    {
                        return $"{caster.Name} tries to create a mirror image, but there is no room!";
                    }
                    */
                    break;
                case "Seduce":
                    if (target == null) break;
                    StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Seduce));
                    return $"{caster.Name} seduces {target.Name}, turning them against their allies!";

                case "Stun":
                    if (target == null) break;
                    StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Stunned));
                    return $"{caster.Name} touches {target.Name}, stunning them with a jolt of energy.";

                case "Teleportation":
                    //TODO add logic to GridService is that is the best place for it
                    /*
                    var newPos = GridService.FindRandomTeleportSpot(caster.Position, dungeon.DungeonGrid, characterList);
                    if (newPos != null)
                    {
                        GridService.MoveCharacter(caster, newPos, dungeon.DungeonGrid);
                        return $"{caster.Name} vanishes and reappears in a new location!";
                    }*/
                    break;

                case "Vampiric touch":
                    if (target == null) break;
                    damage = RandomHelper.RollDie("D10");
                    //TODO: ProcessSpellDamage(damage); // No armour save
                    caster.CurrentHP = Math.Min(caster.GetStat(BasicStat.HitPoints), caster.CurrentHP + damage);
                    return $"{caster.Name} drains {damage} life from {target.Name}, healing itself.";

                // --- SUPPORT SPELLS ---
                case "Frenzy":
                    if (target == null) break;
                    StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Frenzy));
                    return $"{caster.Name} enchants {target.Name}, who flies into a frenzy!";

                case "Healing":
                    if (target == null) break;
                    int healAmount = RandomHelper.RollDie("D10");
                    target.CurrentHP = Math.Min(target.GetStat(BasicStat.HitPoints), target.CurrentHP + healAmount);
                    return $"{caster.Name} casts a healing spell on {target.Name}, recovering {healAmount} HP.";

                case "Healing hand":
                    if (target == null) break;
                    healAmount = RandomHelper.RollDie("D10");
                    target.CurrentHP = Math.Min(target.GetStat(BasicStat.HitPoints), target.CurrentHP + healAmount);
                    return $"{caster.Name} lays a healing hand on {target.Name}, recovering {healAmount} HP.";

                case "Mute":
                    StatusEffectService.AttemptToApplyStatus(caster, StatusEffectService.GetStatusEffectByType(StatusEffectType.MuteAura));
                    return $"{caster.Name} casts a field of silence, making other spells harder to cast.";

                case "Raise dead":
                    var fallenUndead = dungeon.RevealedMonsters.FirstOrDefault(m => m.IsUndead && m.CurrentHP <= 0);
                    if (fallenUndead != null)
                    {
                        fallenUndead.CurrentHP = fallenUndead.GetStat(BasicStat.HitPoints);
                        return $"{caster.Name} raises {fallenUndead.Name} from the dead!";
                    }
                    else
                    {
                        var woundedUndead = dungeon.RevealedMonsters.Where(m => m.IsUndead && m.CurrentHP < m.GetStat(BasicStat.HitPoints)).FirstOrDefault();
                        if (woundedUndead != null)
                        {
                            healAmount = RandomHelper.RollDie("D6");
                            woundedUndead.CurrentHP = Math.Min(woundedUndead.GetStat(BasicStat.HitPoints), woundedUndead.CurrentHP + healAmount);
                            return $"{caster.Name} channels dark energy, healing {woundedUndead.Name} for {healAmount} HP.";
                        }
                    }
                    break;

                case "Shield":
                    if (target == null) break;
                    StatusEffectService.AttemptToApplyStatus(target, StatusEffectService.GetStatusEffectByType(StatusEffectType.Shield));
                    return $"{caster.Name} conjures a magical shield around {target.Name}, granting +2 Armour.";

                case "Summon demon":
                case "Summon greater demon":
                    // TODO: update after added logic for random square is done
                    /*
                    var summonPos = GridService.FindRandomEmptySquareIRoom(caster.Position, caster.Room);
                    if (summonPos != null)
                    {
                        // This assumes a MonsterFactory that can create monsters by name
                        var demonToSummon = GetDemonToSummon(this.Name);
                        var newDemon = MonsterFactory.Create(demonToSummon);
                        GridService.MoveCharacter(newDemon, summonPos.Position, dungeon.DungeonGrid);
                        dungeon.RevealedMonsters.Add(newDemon);                    
                        caster.CurrentAP--;
                        return $"{caster.Name} summons a terrifying {newDemon.Name}!";
                    }
                    else
                    {
                        return $"{caster.Name} attempts a summoning, but there's no space for the creature!";
                    }*/
                    break;
                default: return $"{caster.Name} forgets the incantation.";
            }
            return $"{caster.Name} forgets the incantation.";
        }
    }

    public class Spell
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public SpellTargetType TargetType { get; set; } = SpellTargetType.SingleTarget; // Default to single target
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
}
