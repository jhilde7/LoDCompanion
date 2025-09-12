using System.Linq;
using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Game;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Utilities;

namespace LoDCompanion.Code.BackEnd.Services.Dungeon
{
    public class EncounterService
    {
        private readonly WeaponFactory _weapon = new WeaponFactory();
        private readonly PassiveAbilityService _passive = new PassiveAbilityService();

        public List<Monster> Monsters => GetMonsters();

        public EncounterService()
        {
            
        }

        public List<Monster> GetRandomEncounterByType(EncounterType type, EncounterType? dungeonEncounterType = null, DungeonState? dungeon = null)
        {
            List<Monster> encounters = new List<Monster>();
            Monster monster; // Used temporarily for individual monster creation with additional properties
            int roll = RandomHelper.GetRandomNumber(1, 100);

            switch (type)
            {
                case EncounterType.Beasts:
                    switch (roll)
                    {
                        case <= 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Rat");
                            break;
                        case <= 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bat Swarm");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Leech");
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Satyr", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gnoll Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(2, "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Satyr Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Beastman Guard", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case 21:
                            encounters.Add(BuildMonster("Cave Bear"));
                            break;
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(1, "Gnoll Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 1, 1)));
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Harpy");
                            encounters.AddRange(BuildMonsters(2, "Slime"));
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 30:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Leech");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 0, true));
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Satyr", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 1, true));
                            break;
                        case <= 34:
                            encounters.Add(BuildMonster("Shambler"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Satyr", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 1, true));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Crossbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 38:
                            encounters.Add(BuildMonster("Giant Centipede"));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(1, "Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Lesser Plague Demon");
                            break;
                        case <= 44:
                            encounters = BuildMonsters(1, "River Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Satyr", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() }, 3));
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 48:
                            encounters.Add(BuildMonster("Slime"));
                            encounters.AddRange(BuildMonsters(1, "Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Ettin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Werewolf");
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Stone Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.Add(BuildMonster("Giant Centipede"));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Gargoyle");
                            break;
                        case 57:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case 58:
                            encounters.Add(BuildMonster("Griffon"));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Satyr", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            encounters.AddRange(BuildMonsters(1, "Gnoll Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 1, 1)));
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(1, "Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 0, false, null, "Cursed Weapon");
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(1, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(1, "Ettin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Plague Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 0, false, null, "Cursed Weapon");
                            break;
                        case <= 76:
                            encounters = BuildMonsters(2, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(1, "Gnoll Sergeant", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2));
                            encounters.AddRange(BuildMonsters(1, "Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(1, "Gnoll Sergeant", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            encounters.AddRange(BuildMonsters(1, "Gnoll Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 0, false, null, "Cursed Weapon");
                            break;
                        case <= 84:
                            encounters.Add(BuildMonster("Bloated Demon"));
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Bloated Demon");
                            encounters.AddRange(BuildMonsters(3, "Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case 87:
                            encounters.Add(BuildMonster("Lurker"));
                            break;
                        case 88:
                            encounters = BuildMonsters(1, "Lurker", null, 0, false, BuildSpellList(3, 2, 2));
                            encounters.AddRange(BuildMonsters(2, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 90:
                            encounters.Add(BuildMonster("Giant Spider"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Leech"));
                            break;
                        case <= 92:
                            encounters.Add(BuildMonster("Wyvern"));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, "Beastman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 96:
                            encounters.Add(BuildMonster("Gigantic Spider"));
                            break;
                        case <= 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Leech");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 0, false, null, "Cursed Weapon"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Lesser Plague Demon"));
                            break;
                        case <= 100:
                            encounters.AddRange(BuildMonsters(1, "Greater Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 3));
                            break;
                        default:
                            break;
                    }
                    break;
                case EncounterType.Undead:
                    switch (roll)
                    {
                        case <= 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Rat");
                            break;
                        case <= 4:
                            encounters = BuildMonsters(1, "Bat Swarm");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bat Swarm");
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Zombie");
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Zombie");
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Skeleton Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Zombie", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ghoul");
                            break;
                        case <= 26:
                            encounters = BuildMonsters(1, "Shambler");
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 1, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 30:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Zombie", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dire Wolf"));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 38:
                            encounters = BuildMonsters(1, "Shambler");
                            break;
                        case <= 40:
                            encounters = BuildMonsters(1, "Mummy", null, 1);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), "Ghoul");
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dire Wolf");
                            break;
                        case <= 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dire Wolf");
                            monster = BuildMonster("Necromancer", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon> { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 52:
                            encounters = BuildMonsters(2, "Mummy", null, 1);
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Mummy", null, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Zombie"));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Spider");
                            break;
                        case <= 62:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon> { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Zombie", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Minotaur Skeleton", new List<Weapon> { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Ghoul");
                            break;
                        case <= 70:
                            encounters = BuildMonsters(2, "Zombie Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 2);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 72:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 3);
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() }, 3);
                            encounters.AddRange(BuildMonsters(4, "Skeleton Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Minotaur Skeleton", new List<Weapon> { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 80:
                            encounters = BuildMonsters(1, "Vampire Fledgling", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 7), "Zombie", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(1, "Ghost");
                            break;
                        case <= 84:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 10), "Zombie", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 6), "Zombie", null, 2);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Vampire Fledgling", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Ghoul"));
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith");
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 94:
                            encounters = BuildMonsters(2, "Ghost");
                            break;
                        case <= 96:
                            encounters = BuildMonsters(1, "Banshee");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Zombie", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith");
                            monster = BuildMonster("Necromancer", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 100:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith"));
                            encounters.AddRange(BuildMonsters(1, "Vampire ", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                    }
                    break;
                case EncounterType.Bandits_Brigands:
                    switch (roll)
                    {
                        case <= 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Rat");
                            break;
                        case <= 4:
                            encounters = BuildMonsters(1, "Bat Swarm");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bat Swarm");
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Wolf"));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(2, "Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Pox Rat");
                            encounters.Add(BuildMonster("Slime"));
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 3));
                            break;
                        case <= 30:
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(2, "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            monster = BuildMonster("Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() });
                            encounters.Add(BuildMonster("Fallen Knight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true));
                            encounters.Add(BuildMonster("Fallen Knight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 4, true));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(2, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Crossbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Bandit", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            monster = BuildMonster("Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 48:
                            encounters = BuildMonsters(1, "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(2, "Shambler");
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Fallen Knight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true);
                            break;
                        case <= 54:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            monster = BuildMonster("Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(2, "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            encounters.Add(BuildMonster("Ogre Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 62:
                            encounters = BuildMonsters(2, "Giant Centipede");
                            break;
                        case <= 64:
                            encounters = BuildMonsters(2, "Shambler");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Crossbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 3));
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(2, "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Fallen Knight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true, null, "Cursed Weapon");
                            monster = BuildMonster("Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Greater Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Fallen Knight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            break;
                        case <= 78:
                            encounters = BuildMonsters(2, "Shambler");
                            monster = BuildMonster("Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 80:
                            encounters = BuildMonsters(3, "Bandit Leader", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(4, "Bandit Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Crossbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 84:
                            encounters = BuildMonsters(4, "Centaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 3, true);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            monster = BuildMonster("Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Greater Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(3, "Ogre Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 92:
                            encounters = BuildMonsters(2, "Centaur Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(2, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(3, "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 96:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Fallen Knight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true, null, "Cursed Weapon");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 98:
                            encounters.Add(BuildMonster("Ogre Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            encounters.Add(BuildMonster("Gigantic Spider"));
                            break;
                        case <= 100:
                            encounters.Add(BuildMonster("Wyvern"));
                            break;
                    }
                    break;
                case EncounterType.Orcs_Goblins:
                    switch (roll)
                    {
                        case <= 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Rat");
                            break;
                        case <= 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Bat Swarm");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Cave Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf"));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(1, "Orc Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Pox Rat");
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 30:
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Orc Brute", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1));
                            encounters.Add(BuildMonster("Orc Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Shambler"));
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 1, false, BuildSpellList(1, 1, 1)));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 42:
                            encounters = BuildMonsters(2, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 44:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Orc Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            encounters.Add(BuildMonster("Orc Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 48:
                            encounters = BuildMonsters(1, "Ettin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Cave Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Stone Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 56:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(2, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(1, "Giant Centipede");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Orc Brute", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 1));
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 1, 1)));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf"));
                            encounters.Add(BuildMonster("Ogre Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc Brute", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(1, "River Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 70:
                            encounters = BuildMonsters(2, "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Orc Brute", new List<Weapon> { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 1, 1)));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(2, "Ettin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 78:
                            encounters = BuildMonsters(2, "Shambler");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Goblin Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(3, "Orc Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(4, "Cave Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 84:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(4, "Centaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 2, true);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 1, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 90:
                            encounters = BuildMonsters(2, "Ogre Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 94:
                            encounters = BuildMonsters(4, "Giant Spider");
                            encounters.Add(BuildMonster("Lurker", null, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case <= 96:
                            encounters = BuildMonsters(2, "Gigantic Spider");
                            break;
                        case <= 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc Brute", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2));
                            encounters.AddRange(BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0));
                            break;
                        case <= 100:
                            encounters = BuildMonsters(1, "Orc Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true);
                            encounters.Add(BuildMonster("Wyvern"));
                            break;
                    }
                    break;
                case EncounterType.Reptiles:
                    switch (roll)
                    {
                        case <= 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Rat");
                            break;
                        case <= 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Bat Swarm");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 8:
                            encounters = BuildMonsters(1, "Slime");
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Snake");
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gecko", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Gecko", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gecko", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, true));
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Frogling", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, true));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            break;
                        case <= 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gecko", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1, true));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Frogling", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 1, true);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 28:
                            encounters = BuildMonsters(1, "Giant Toad");
                            break;
                        case <= 30:
                            encounters = BuildMonsters(1, "Giant Centipede");
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(2, "Gecko", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1));
                            encounters.Add(BuildMonster("Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Raptor"));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Raptor");
                            break;
                        case <= 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), "Giant Spider");
                            break;
                        case <= 42:
                            encounters = BuildMonsters(2, "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Giant Toad"));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Toad"));
                            break;
                        case <= 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() }, 3);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 1, false, BuildSpellList(1, 1, 1)));
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(2, "Slime"));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(2, "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(2, "Gecko", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 1, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gecko", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Raptor"));
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Snake");
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Frogling", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 70:
                            encounters = BuildMonsters(2, "Giant Toad");
                            encounters.AddRange(BuildMonsters(2, "Slime"));
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Snake");
                            break;
                        case <= 74:
                            encounters = BuildMonsters(1, "Salamander");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Toad");
                            encounters.AddRange(BuildMonsters(2, "Salamander"));
                            break;
                        case <= 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(3, "Saurian Warchief", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 84:
                            encounters = BuildMonsters(1, "Gigantic Snake");
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Basilisk");
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 1, false, BuildSpellList(4, 2, 2)));
                            break;
                        case <= 90:
                            encounters = BuildMonsters(1, "Saurian Warchief", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.Add(BuildMonster("Basilisk"));
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Naga", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(2, "Giant Toad"));
                            encounters.Add(BuildMonster("Basilisk"));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(3, "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 96:
                            encounters = BuildMonsters(1, "Gigantic Snake");
                            break;
                        case <= 98:
                            encounters = BuildMonsters(2, "Frogling", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian Elite", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1, true));
                            break;
                        case <= 100:
                            encounters = BuildMonsters(1, "Hydra");
                            break;
                    }
                    break;
                case EncounterType.DarkElves:
                    switch (roll)
                    {
                        case <= 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Rat");
                            break;
                        case <= 4:
                            encounters = BuildMonsters(1, "Bat Swarm");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bat Swarm");
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Dark Elf Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Dark Elf Assassin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 1, false, null, "Poisonous Weapon"));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Wolf"));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(1, "Dark Elf Captain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf Sniper", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(2, "Giant Centipede");
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Pox Rat");
                            encounters.AddRange(BuildMonsters(2, "Harpy"));
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Giant Spider");
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 30:
                            encounters = BuildMonsters(1, "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(2, "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Dark Elf Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf Sniper", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Dark Elf Captain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Flail")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(2, "Harpy"));
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(2, "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Dark Elf Assassin", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Crossbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1, false, null, "Poisonous Weapon"));
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Dark Elf Captain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            encounters.Add(BuildMonster("Dark Elf Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 1, 1)));
                            break;
                        case <= 48:
                            encounters = BuildMonsters(2, "Slime");
                            break;
                        case <= 50:
                            encounters = BuildMonsters(2, "Shambler");
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Gargoyle");
                            break;
                        case <= 54:
                            encounters = BuildMonsters(2, "Shambler");
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Plague Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Psyker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(1, "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Dark Elf Assassin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1, false, null, "Poisonous Weapon"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider"));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 62:
                            encounters = BuildMonsters(2, "Giant Centipede");
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf Assassin", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 3);
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 66:
                            encounters = BuildMonsters(1, "Dark Elf Captain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            break;
                        case <= 68:
                            encounters = BuildMonsters(2, "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true, null, "Cursed Weapon");
                            encounters.Add(BuildMonster("Dark Elf Warlock", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider"));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(2, "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            break;
                        case <= 78:
                            encounters = BuildMonsters(2, "Shambler");
                            encounters.AddRange(BuildMonsters(2, "Psyker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case <= 80:
                            encounters = BuildMonsters(2, "Dark Elf Captain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(4, "Dark Elf Sniper", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Crossbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(2, "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 84:
                            encounters = BuildMonsters(1, "Medusa", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Psyker", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Basilisk");
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dark Elf", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(2, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 92:
                            encounters = BuildMonsters(1, "Medusa", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            break;
                        case <= 94:
                            encounters = BuildMonsters(3, "Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 96:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 4, true, null, "Cursed Weapon");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf Assassin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2, false, null, "Poisonous Weapon"));
                            break;
                        case <= 98:
                            encounters.Add(BuildMonster("Dryder", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            encounters.Add(BuildMonster("Gigantic Spider"));
                            break;
                        case <= 100:
                            encounters.Add(BuildMonster("Hydra"));
                            break;
                    }
                    break;
                case EncounterType.AncientLands:
                    switch (roll)
                    {
                        case <= 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bat Swarm");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Pox Rat");
                            break;
                        case <= 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Slime");
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Leech");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.Add(BuildMonster("Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(1, "Giant Scorpion");
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gargoyle");
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Spider");
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 30:
                            encounters = BuildMonsters(2, "Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Scorpion");
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Gargoyle"));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 38:
                            encounters = BuildMonsters(1, "Sphinx");
                            break;
                        case <= 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1, true);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Snake");
                            encounters.AddRange(BuildMonsters(2, "Slime"));
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Longbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 1));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(1, "Giant Centipede");
                            break;
                        case <= 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 2);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 54:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Sphinx");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gargoyle"));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(1, "Banshee");
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Spider");
                            break;
                        case <= 62:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            monster = BuildMonster("Mummy Priest", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Slime"));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Scorpion");
                            break;
                        case <= 70:
                            encounters = BuildMonsters(2, "Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            encounters.Add(BuildMonster("Giant Scorpion"));
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(4, "Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case 77:
                            encounters = BuildMonsters(4, "Mummy", null, 1);
                            break;
                        case 78:
                            encounters = BuildMonsters(4, "Gargoyle");
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), "Mummy", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            monster = BuildMonster("Mummy Queen", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, false, BuildSpellList(3, 2, 2));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 82:
                            encounters = BuildMonsters(1, "Ghost");
                            break;
                        case <= 84:
                            encounters = BuildMonsters(1, "Gigantic Spider");
                            break;
                        case <= 86:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 2);
                            monster = BuildMonster("Mummy Queen", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, false, BuildSpellList(5, 2, 2));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith");
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Halberd")?.Clone() ?? new MeleeWeapon() }, 3);
                            break;
                        case <= 94:
                            encounters = BuildMonsters(2, "Ghost");
                            break;
                        case <= 96:
                            encounters = BuildMonsters(1, "Banshee");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Scorpion"));
                            break;
                        case <= 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith");
                            encounters.AddRange(BuildMonsters(2, "Sphinx"));
                            break;
                        case <= 100:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith"));
                            monster = BuildMonster("Mummy Queen", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, false, BuildSpellList(3, 3, 2));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                    }
                    break;
                case EncounterType.GoblinKing:
                    switch (roll)
                    {
                        case <= 3:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Rat");
                            break;
                        case <= 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bat Swarm");
                            break;
                        case <= 9:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat");
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 15:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 21:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Cave Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf"));
                            break;
                        case <= 27:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Javelin")?.Clone() ?? new Weapon() }, 1, true));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 1, false, BuildSpellList(1, 1, 0)));
                            break;
                        case <= 31:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Pox Rat");
                            break;
                        case <= 37:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 40:
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 43:
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 1);
                            break;
                        case <= 49:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 52:
                            encounters = BuildMonsters(1, "River Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat"));
                            encounters.Add(BuildMonster("Stone Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 59:
                            encounters = BuildMonsters(2, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 2));
                            break;
                        case <= 65:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 71:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Morningstar")?.Clone() ?? new MeleeWeapon() }, 2, true));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new MeleeWeapon() }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 74:
                            encounters = BuildMonsters(2, "Stone Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 77:
                            encounters = BuildMonsters(2, "Stone Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(6, "Cave Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Net")?.Clone() ?? new MeleeWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 81:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 84:
                            encounters = BuildMonsters(2, "River Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            break;
                        case <= 87:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 3, true));
                            break;
                        case <= 91:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(3, 6), "Goblin", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new MeleeWeapon() }, 2, true);
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new MeleeWeapon() }, 4));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(2, "River Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(3, 6), "Goblin Archer", new List<Weapon>() { (RangedWeapon?)EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new RangedWeapon(), (MeleeWeapon?)EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 97:
                            encounters = BuildMonsters(2, "River Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() });
                            encounters.Add(BuildMonster("Stone Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }));
                            break;
                        case <= 100:
                            encounters = BuildMonsters(1, "Gigantic Spider");
                            break;
                    }
                    break;
                case EncounterType.SpringCleaning:
                    return roll switch
                    {
                        <= 30 => BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Rat"),
                        <= 45 => BuildMonsters(1, "Johann"),
                        <= 55 => BuildMonsters(1, "Bat Swarm"),
                        <= 65 => BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat"),
                        <= 75 => BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Pox Rat"),
                        <= 85 => BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake"),
                        _ => BuildMonsters(1, "Giant Spider")
                    };
                case EncounterType.C26:
                    encounters = BuildMonsters(1, "Shambler");
                    break;
                case EncounterType.C29:
                    encounters = BuildMonsters(1, "Bat Swarm");
                    break;
                case EncounterType.R17:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case 2:
                            encounters = BuildMonsters(1, "Minotaur", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2);
                            break;
                        case 3:
                            encounters = BuildMonsters(1, "Gigantic Snake");
                            break;
                        case 4:
                        case <= 6:
                            encounters = BuildMonsters(1, "Gigantic Spider");
                            break;
                    }
                    break;
                case EncounterType.R19:
                    encounters = BuildMonsters(2, "Gargoyle");
                    break;
                case EncounterType.R20:
                    roll = RandomHelper.GetRandomNumber(1, 2);
                    switch (roll)
                    {
                        case 1:
                            return BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "FireElemental");
                        case 2:
                            if (dungeonEncounterType.HasValue)
                            {
                                return GetRandomEncounterByType(dungeonEncounterType.Value); 
                            }
                            break;
                    }
                    break;
                case EncounterType.R28:
                    return BuildMonsters(2, "Mummy", null, 0);
                case EncounterType.R30:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Snake");
                            if (dungeonEncounterType.HasValue)
                            {
                                encounters.AddRange(GetRandomEncounterByType(dungeonEncounterType.Value)); 
                            }
                            break;
                        case >= 4:
                            return BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Snake");
                        default:
                            break;
                    }
                    break;
                case EncounterType.TombGuardian:
                    return BuildMonsters(2, "Tomb Guardian", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 2);
                case EncounterType.Mimic:
                    return BuildMonsters(1, "Mimic");
                case EncounterType.TheGrievingMother:
                    roll = RandomHelper.RollDie(DiceType.D2);
                    return BuildMonsters(roll, "Giant Spider");
                case EncounterType.TheApprentice:
                    encounters = GetRandomEncounterByType(EncounterType.Undead);
                    // Now, handle the special rule for the Caretaker
                    int caretakerRoll = RandomHelper.RollDie(DiceType.D100); // Using d20 for 15-20 range
                    if (caretakerRoll >= 15 && caretakerRoll <= 20 && dungeon != null && !dungeon.DefeatedUniqueMonsters.Contains("Emil the Caretaker"))
                    {
                        var caretakerParams = new Dictionary<string, string>
                        {
                            { "Name", "Emil the Caretaker" },
                            { "BaseMonster", "Human" },
                            { "Weapons", "Greataxe" }
                        };
                        encounters.AddRange(GetEncounterByParams(caretakerParams));
                    }
                    break;
                case EncounterType.StopTheHeritics:
                    roll = RandomHelper.RollDie(DiceType.D6);
                    return roll switch
                    {
                        <= 2 => BuildMonsters(RandomHelper.RollDie(DiceType.D6), "Lesser Plague Demon"),
                        <= 5 => BuildMonsters(RandomHelper.RollDie(DiceType.D6), "Blood Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new MeleeWeapon() }, 0, false, null, "Cursed Weapon"),
                        >= 6 => BuildMonsters(1, "Greater Demon", new List<Weapon>() { (MeleeWeapon?)EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new MeleeWeapon() }, 3)
                    };
                default:
                    break;
            }

            return encounters;
        }

        /// <summary>
        /// Creates a list of new Monster instances based on a template, applying specified properties.
        /// </summary>
        /// <param name="Count">The number of monsters to create.</param>
        /// <param name="TemplateMonster">The base Monster object to copy properties from.</param>
        /// <param name="Weapons">Optional: A list of weapons to equip the monsters with.</param>
        /// <param name="armourValue">Optional: The armor value to set for the monsters.</param>
        /// <param name="HasShield">Optional: Indicates if the monsters have a shield.</param>
        /// <param name="Spells">Optional: A list of spell names the monsters can cast.</param>
        /// <param name="SpecialRule">Optional: A special rule to add to the monsters.</param>
        /// <returns>A list of new Monster objects.</returns>
        private List<Monster> BuildMonsters(
            int count,
            string templateMonster,
            List<Weapon>? weapons = null,
            int armourValue = 0,
            bool hasShield = false,
            List<MonsterSpell>? Spells = null,
            string? specialRule = null)
        {
            List<Monster> monsters = new List<Monster>();
            for (int i = 0; i < count; i++)
            {
                monsters.Add(BuildMonster(templateMonster, weapons, armourValue, hasShield, Spells, specialRule));
            }
            return monsters;
        }

        /// <summary>
        /// Creates a single new Monster instance based on a template, applying specified properties.
        /// </summary>
        /// <param name="TemplateMonster">The base Monster object to copy properties from.</param>
        /// <param name="Weapons">Optional: A list of weapons to equip the monster with.</param>
        /// <param name="armourValue">Optional: The armor value to set for the monster.</param>
        /// <param name="HasShield">Optional: Indicates if the monster has a shield.</param>
        /// <param name="Spells">Optional: A list of spell names the monster can cast.</param>
        /// <param name="SpecialRule">Optional: A special rule to add to the monster.</param>
        /// <returns>A new Monster object.</returns>
        private Monster BuildMonster(
            string templateMonster,
            List<Weapon>? weapons = null,
            int armourValue = 0,
            bool hasShield = false,
            List<MonsterSpell>? Spells = null,
            string? specialRule = null)
        {
            // Assuming Monster has a constructor that copies properties from a template Monster.
            // Or you would instantiate a new Monster and manually copy relevant properties.
            Monster newMonster = GetMonsterByName(templateMonster).Clone();

            // Assign new lists/values to the new monster instance to avoid modifying the template
            if (weapons != null)
            {
                newMonster.Weapons = new List<Weapon>(weapons);
            }
            newMonster.ArmourValue = armourValue; // Assuming Monster has an ArmourValue property
            if (Spells != null)
            {
                newMonster.Spells = new List<MonsterSpell>(Spells);
            }
            newMonster.HasShield = hasShield; // Assuming Monster has a HasShield property
            if (specialRule != null)
            {
                newMonster.SpecialRules ??= new List<string>();
                newMonster.SpecialRules.Add(specialRule);
            }

            if (newMonster.SpecialRules != null)
            {
                if (newMonster.Weapons != null)
                {
                    if (newMonster.SpecialRules.Contains("Poisonous Weapon"))
                    {
                        for (int i = 0; i < newMonster.Weapons.Count; i++)
                        {
                            var currentWeapon = newMonster.Weapons[i];
                            if (currentWeapon is MeleeWeapon meleeWeapon)
                            {
                                newMonster.Weapons[i] = _weapon.CreateModifiedMeleeWeapon(
                                    meleeWeapon.Name, $"Poisonous {meleeWeapon.Name}",
                                    weapon =>
                                    {
                                        weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                                    });
                            }
                        }
                    }

                    if (newMonster.SpecialRules.Contains("Cursed Weapon"))
                    {
                        for (int i = 0; i < newMonster.Weapons.Count; i++)
                        {
                            var currentWeapon = newMonster.Weapons[i];
                            if (currentWeapon is MeleeWeapon meleeWeapon)
                            {
                                newMonster.Weapons[i] = _weapon.CreateModifiedMeleeWeapon(
                                    meleeWeapon.Name, $"Cursed {meleeWeapon.Name}",
                                    weapon =>
                                    {
                                        weapon.Properties.TryAdd(WeaponProperty.Cursed, 0);
                                    });
                            }
                        }
                    }
                }

                if (newMonster.SpecialRules.Contains("Raise dead"))
                {
                    newMonster.Spells.Add(SpellService.GetMonsterSpellByName("Raise dead"));
                }
            }

            return newMonster;
        }

        /// <summary>
        /// Helper method to build a list of generic spell types based on counts.
        /// </summary>
        /// <param name="TouchSpell">Number of "Touch spell" entries.</param>
        /// <param name="RangedSpell">Number of "Ranged spell" entries.</param>
        /// <param name="SupportSpell">Number of "Support spell" entries.</param>
        /// <returns>A list of spell type strings.</returns>
        private List<MonsterSpell> BuildSpellList(int touchSpell, int rangedSpell, int supportSpell)
        {
            List<MonsterSpell> Spells = new List<MonsterSpell>();
            Spells.AddRange(GetRandomSpellsFromList(SpellService.GetMonsterSpellsByType(MonsterSpellType.CloseCombat), touchSpell));
            Spells.AddRange(GetRandomSpellsFromList(SpellService.GetMonsterSpellsByType(MonsterSpellType.Ranged), rangedSpell));
            Spells.AddRange(GetRandomSpellsFromList(SpellService.GetMonsterSpellsByType(MonsterSpellType.Support), supportSpell));

            return Spells;
        }

        private List<MonsterSpell> GetRandomSpellsFromList(List<MonsterSpell> spellList, int count)
        {
            // Ensure you don't try to take more spells than exist in the list.
            if (count >= spellList.Count)
            {
                return spellList;
            }

            List<MonsterSpell> shuffledList = new List<MonsterSpell>(spellList);
            shuffledList.Shuffle();

            return shuffledList.GetRange(0, count);
        }

        public List<Monster> GetMonsters()
        {
            return new List<Monster>()
            {
                new Monster()
                {
                    Name = "Bandit",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 90,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Bandit Archer",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    XP = 90,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Bandit Leader",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 16 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 },
                        { Skill.RangedSkill, 40 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 130,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Banshee",
                    Species = MonsterSpeciesName.Banshee,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 18 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 60 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToSilver, 0 },
                        { MonsterSpecialName.Silent, 0 },
                        { MonsterSpecialName.CauseTerror, 5 },
                        { MonsterSpecialName.Ethereal, 0 },
                        { MonsterSpecialName.GhostlyTouch, 0 }
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.GhostlyHowl
                    },
                    XP = 650,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Bat swarm",
                    Species = MonsterSpeciesName.GiantBat,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 10 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 55 },
                        { BasicStat.Resolve, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 100 }
                    },
                    MinDamage = 1,
                    MaxDamage = 4,
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Flyer, 0 }
                    },
                    SpecialRules = new List<string>() { "Auto hit", "Always acts first on the first turn of battle" },
                    XP = 15,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Beastman",
                    Species = MonsterSpeciesName.Beastman,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 15 },
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 35 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 20 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 100,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Beastman Chieftain",
                    Species = MonsterSpeciesName.Beastman,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 20 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 50 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 150,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Beastman Guard",
                    Species = MonsterSpeciesName.Beastman,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 18 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 20 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 110,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Berserker",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 14 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.Beast,
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    XP = 110,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Bloated Demon",
                    Species = MonsterSpeciesName.Demon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 32 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 3 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 80 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 25 }
                    },
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.DiseaseRidden, 0 },
                        { MonsterSpecialName.Floater, 0 },
                        { MonsterSpecialName.Demon, 0 },
                        { MonsterSpecialName.Large, 0 }
                    },
                    XP = 650,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Blood Demon",
                    Species = MonsterSpeciesName.Demon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 12 },
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Demon, 0 },
                    },
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    XP = 200,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Cave Bear",
                    Species = MonsterSpeciesName.CaveBear,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 20 },
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FerociousCharge, 0}
                    },
                    XP = 130,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Cave Goblin",
                    Species = MonsterSpeciesName.CaveGoblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 8 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 30 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.HateDwarves, 0}
                    },
                    XP = 70,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Cave Goblin Archer",
                    Species = MonsterSpeciesName.CaveGoblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 8 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 30 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.HateDwarves, 0}
                    },
                    XP = 70,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Centaur",
                    Species = MonsterSpeciesName.Centaur,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.HitPoints, 20 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 7 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 60 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Kick
                    },
                    XP = 150,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Centaur Archer",
                    Species = MonsterSpeciesName.Centaur,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 7 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Kick
                    },
                    XP = 150,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Common Troll",
                    Species = MonsterSpeciesName.CommonTroll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Regeneration, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Stupid, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 500,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Basilisk",
                    Species = MonsterSpeciesName.Basilisk,
                    MinDamage = 1,
                    MaxDamage = 8,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Petrify
                    },
                    XP = 325,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Dark Elf",
                    Species = MonsterSpeciesName.DarkElf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 11 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 125,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Dark Elf Archer",
                    Species = MonsterSpeciesName.DarkElf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 11 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    XP = 125,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Dark Elf Assassin",
                    Species = MonsterSpeciesName.DarkElf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 65 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 11 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Sneaky, 0}
                    },
                    XP = 135,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Dark Elf Captain",
                    Species = MonsterSpeciesName.DarkElf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 13 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 150,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Dark Elf Sniper",
                    Species = MonsterSpeciesName.DarkElf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 11 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 65 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    XP = 135,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Dark Elf Warlock",
                    Species = MonsterSpeciesName.DarkElf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 11 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 60 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.MagicUser,
                    XP = 165,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "Dire Wolf",
                    Species = MonsterSpeciesName.DireWolf,
                    MinDamage = 1,
                    MaxDamage = 10,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 8 },
                        { BasicStat.Dexterity, 15 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.FerociousCharge, 0},
                    },
                    XP = 80,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Dragon",
                    Species = MonsterSpeciesName.Dragon,
                    MinDamage = 1,
                    MaxDamage = 10,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 80 },
                        { BasicStat.DamageBonus, 5 },
                        { BasicStat.NaturalArmour, 5 },
                        { BasicStat.HitPoints, 100 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseTerror, 10 },
                        { MonsterSpecialName.XLarge, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike,
                        SpecialActiveAbility.FireBreath
                    },
                    XP = 3500,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Drider",
                    Species = MonsterSpeciesName.Drider,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 65 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.HitPoints, 28 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.WallCrawler, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 600,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Fallen Knight",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 18 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 240,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Warlock",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.MagicUser,
                    XP = 180,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "Earth Elemental",
                    Species = MonsterSpeciesName.Unknown,
                    MinDamage = 1,
                    MaxDamage = 10,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 20 },
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.MagicBeing, 0}
                    },
                    XP = 200,
                    TreasureType = TreasureType.None
                },
                new Monster()
                {
                    Name = "Fire Elemental",
                    Species = MonsterSpeciesName.Unknown,
                    MinDamage = 1,
                    MaxDamage = 10,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToFrost, 0},
                        { MonsterSpecialName.MagicBeing, 0}
                    },
                    SpecialRules = new List<string> { "Fire damage" },
                    XP = 250,
                    TreasureType = TreasureType.None
                },
                new Monster()
                {
                    Name = "Water Elemental",
                    Species = MonsterSpeciesName.Unknown,
                    MinDamage = 1,
                    MaxDamage = 10,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 15 },
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToFire, 0},
                        { MonsterSpecialName.MagicBeing, 0}
                    },
                    XP = 150,
                    TreasureType = TreasureType.None
                },
                new Monster()
                {
                    Name = "Wind Elemental",
                    Species = MonsterSpeciesName.Unknown,
                    MinDamage = 1,
                    MaxDamage = 10,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 12 },
                        { BasicStat.NaturalArmour, 1 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.MagicBeing, 0},
                        { MonsterSpecialName.Gust, 0}
                    },
                    XP = 150,
                    TreasureType = TreasureType.None
                },
                new Monster()
                {
                    Name = "Gargoyle",
                    Species = MonsterSpeciesName.Gargoyle,
                    MinDamage = 1,
                    MaxDamage = 12,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 25 },
                        { BasicStat.NaturalArmour, 4 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    XP = 400,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Ettin",
                    Species = MonsterSpeciesName.Ettin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 30 },
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Stupid, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.FreeBellow,
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 550,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Giant",
                    Species = MonsterSpeciesName.Giant,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 70 },
                        { BasicStat.DamageBonus, 5 },
                        { BasicStat.NaturalArmour, 3 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 25 }
                    },
                    Type = EncounterType.Beasts, // Type was Bandits_Brigands, likely a typo
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseTerror, 8 },
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 900,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Giant Centipede",
                    Species = MonsterSpeciesName.GiantCentipede,
                    MinDamage = 1,
                    MaxDamage = 10,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 55 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 22 },
                        { BasicStat.NaturalArmour, 4 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 300,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Frogling",
                    Species = MonsterSpeciesName.Frogling,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 35 },
                        { BasicStat.HitPoints, 8 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 40 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.PoisonSpit
                    },
                    XP = 90,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Gecko",
                    Species = MonsterSpeciesName.Gecko,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 95,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Gecko Archer",
                    Species = MonsterSpeciesName.Gecko,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    XP = 95,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Gecko Assassin",
                    Species = MonsterSpeciesName.Gecko,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 55 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Sneaky, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Camouflage
                    },
                    XP = 100,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Ghost",
                    Species = MonsterSpeciesName.Ghost,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.Ethereal, 0},
                        { MonsterSpecialName.GhostlyTouch, 0}
                    },
                    XP = 550,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Ghoul",
                    Species = MonsterSpeciesName.Ghoul,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 11 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Poisonous, 0}
                    },
                    XP = 90,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Giant Leech",
                    Species = MonsterSpeciesName.GiantLeech,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 3 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 }
                    },
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Slow, 0},
                        { MonsterSpecialName.Diseased, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Leech
                    },
                    XP = 90,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Giant Pox Rat",
                    Species = MonsterSpeciesName.GiantPoxRat,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 8 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    MinDamage = 1,
                    MaxDamage = 6,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Scurry, 0},
                        { MonsterSpecialName.Diseased, 0}
                    },
                    XP = 50,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Giant Rat",
                    Species = MonsterSpeciesName.GiantRat,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 6 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    MinDamage = 1,
                    MaxDamage = 6,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.PerfectHearing, 0},
                        { MonsterSpecialName.Scurry, 0}
                    },
                    XP = 40,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Giant Scorpion",
                    Species = MonsterSpeciesName.GiantScorpion,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 4 },
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    MinDamage = 1,
                    MaxDamage = 12,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 4 },
                        { MonsterSpecialName.WallCrawler, 0},
                        { MonsterSpecialName.Poisonous, 0}
                    },
                    XP = 220,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Giant Snake",
                    Species = MonsterSpeciesName.GiantSnake,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 60 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 8,
                    ToHitPenalty = -20,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Poisonous, 0}
                    },
                    XP = 120,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Giant Spider",
                    Species = MonsterSpeciesName.GiantSpider,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 60 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.WallCrawler, 0},
                        { MonsterSpecialName.Poisonous, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Web
                    },
                    XP = 170,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Giant Toad",
                    Species = MonsterSpeciesName.GiantToad,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 55 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Swallow,
                        SpecialActiveAbility.TongueAttack
                    },
                    XP = 400,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Giant Wolf",
                    Species = MonsterSpeciesName.GiantWolf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 9 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.PerfectHearing, 0}
                    },
                    XP = 80,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Gigantic Snake",
                    Species = MonsterSpeciesName.GiganticSnake,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 60 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.Poisonous, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 800,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Sand Worm",
                    Species = MonsterSpeciesName.GiganticSnake,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 60 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 800,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Gigantic Spider",
                    Species = MonsterSpeciesName.GiganticSpider,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseTerror, 5 },
                        { MonsterSpecialName.Poisonous, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 900,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Gnoll",
                    Species = MonsterSpeciesName.Gnoll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 80,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Gnoll Archer",
                    Species = MonsterSpeciesName.Gnoll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    XP = 80,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Gnoll Sergeant",
                    Species = MonsterSpeciesName.Gnoll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 13 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 30 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 100,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Gnoll Shaman",
                    Species = MonsterSpeciesName.Gnoll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 11 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 40 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.MagicUser,
                    XP = 150,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "Goblin",
                    Species = MonsterSpeciesName.Goblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 8 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 30 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FearElves, 0}
                    },
                    XP = 70,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Goblin Archer",
                    Species = MonsterSpeciesName.Goblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 8 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 30 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FearElves, 0}
                    },
                    XP = 70,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Goblin Shaman",
                    Species = MonsterSpeciesName.Goblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 8 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 40 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.MagicUser,
                    XP = 130,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Greater Demon",
                    Species = MonsterSpeciesName.Demon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 65 },
                        { BasicStat.HitPoints, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.MagicUser,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseTerror, 5 },
                        { MonsterSpecialName.Demon, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 1200,
                    TreasureType = TreasureType.T5
                },
                new Monster()
                {
                    Name = "Griffon",
                    Species = MonsterSpeciesName.Griffon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 65 },
                        { BasicStat.HitPoints, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FlyerOutdoors, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 1500,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Harpy",
                    Species = MonsterSpeciesName.Harpy,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 25 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -15,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FlyerOutdoors, 0}
                    },
                    XP = 130,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Hydra",
                    Species = MonsterSpeciesName.Hydra,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 70 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 7 },
                        { MonsterSpecialName.XLarge, 0},
                        { MonsterSpecialName.MultipleAttacksHydra, 5 }
                    },
                    XP = 1850,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Lesser Plague Demon",
                    Species = MonsterSpeciesName.Demon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 5 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    MinDamage = 1,
                    MaxDamage = 8,
                    ToHitPenalty = -10,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Diseased, 0},
                        { MonsterSpecialName.Demon, 0},
                        { MonsterSpecialName.Flyer, 0},
                    },
                    XP = 50,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Lurker",
                    Species = MonsterSpeciesName.Lurker,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 },
                        { Skill.RangedSkill, 60 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.MagicUser,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Floater, 0},
                        { MonsterSpecialName.Demon, 0}
                    },
                    XP = 1200,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Medusa",
                    Species = MonsterSpeciesName.Medusa,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 65 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.DarkElves,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Petrify
                    },
                    XP = 350,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Mimic",
                    Species = MonsterSpeciesName.Mimic,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 2 },
                        { BasicStat.Dexterity, 15 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Leech
                    },
                    XP = 110,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Minotaur",
                    Species = MonsterSpeciesName.Minotaur,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 26 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FerociousCharge, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 450,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Minotaur Skeleton",
                    Species = MonsterSpeciesName.MinotaurSkeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 4 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.JustBones, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Large, 0}
                    },
                    SpecialRules = new List<string>() { "Gives bonemeal as part" },
                    XP = 350,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Mummy",
                    Species = MonsterSpeciesName.Mummy,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 80 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToFire, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 300,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Mummy Priest",
                    Species = MonsterSpeciesName.Mummy,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 80 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 50 }
                    },
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.MagicUser,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToFire, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 600,
                    TreasureType = TreasureType.T5
                },
                new Monster()
                {
                    Name = "Mummy Queen",
                    Species = MonsterSpeciesName.Mummy,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 85 },
                        { BasicStat.HitPoints, 35 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 40 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.MagicUser,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToFire, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 800,
                    TreasureType = TreasureType.T5
                },
                new Monster()
                {
                    Name = "Naga",
                    Species = MonsterSpeciesName.Naga,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 65 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.MultipleAttacks, 3 }
                    },
                    XP = 650,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Necromancer",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.MagicUser,
                    XP = 180,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "Ogre",
                    Species = MonsterSpeciesName.Ogre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 24 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 20 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 400,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Ogre Berserker",
                    Species = MonsterSpeciesName.Ogre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 24 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 15 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 500,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Ogre Chieftain",
                    Species = MonsterSpeciesName.Ogre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 32 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 20 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 600,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Orc",
                    Species = MonsterSpeciesName.Orc,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 95,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Orc Brute",
                    Species = MonsterSpeciesName.Orc,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 16 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    XP = 110,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Orc Chieftain",
                    Species = MonsterSpeciesName.Orc,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 18 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 30 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    XP = 130,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Orc Shaman",
                    Species = MonsterSpeciesName.Orc,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Orcs_Goblins,
                    Behavior = MonsterBehaviorType.MagicUser,
                    XP = 180,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "Plague Demon",
                    Species = MonsterSpeciesName.Demon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Demon, 0},
                        { MonsterSpecialName.DiseaseRidden, 0},
                    },
                    XP = 200,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Psyker",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 65 },
                        { BasicStat.HitPoints, 16 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 70 }
                    },
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.MagicUser,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Psychic, 0}
                    },
                    XP = 250,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "Raptor",
                    Species = MonsterSpeciesName.Raptor,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 14 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FerociousCharge, 0},
                        { MonsterSpecialName.Rend, 0}
                    },
                    XP = 130,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "River Troll",
                    Species = MonsterSpeciesName.RiverTroll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 15 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Stench, 0},
                        { MonsterSpecialName.Stupid, 0},
                        { MonsterSpecialName.Regeneration, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 550,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Salamander",
                    Species = MonsterSpeciesName.Salamander,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 18 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Slow, 0},
                        { MonsterSpecialName.Stupid, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.FireBreath
                    },
                    XP = 430,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Satyr",
                    Species = MonsterSpeciesName.Satyr,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.PerfectHearing, 0}
                    },
                    XP = 80,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Satyr Archer",
                    Species = MonsterSpeciesName.Satyr,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.PerfectHearing, 0}
                    },
                    XP = 80,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Saurian",
                    Species = MonsterSpeciesName.Saurian,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 0 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.Beast,
                    XP = 110,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Saurian Elite",
                    Species = MonsterSpeciesName.Saurian,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 18 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.Beast,
                    XP = 140,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Saurian Priest",
                    Species = MonsterSpeciesName.Saurian,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.MagicUser,
                    XP = 200,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "Saurian Warchief",
                    Species = MonsterSpeciesName.Saurian,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Reptiles,
                    Behavior = MonsterBehaviorType.Beast,
                    XP = 160,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Shambler",
                    Species = MonsterSpeciesName.Shambler,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    MinDamage = 1,
                    MaxDamage = 12,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Entangle
                    },
                    XP = 450,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Skeleton",
                    Species = MonsterSpeciesName.Skeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 20 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.JustBones, 0},
                        { MonsterSpecialName.CauseFear, 2 }
                    },
                    SpecialRules = new List<string>() { "Gives Bone meal as part" },
                    XP = 80,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Skeleton Archer",
                    Species = MonsterSpeciesName.Skeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 20 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.JustBones, 0},
                        { MonsterSpecialName.CauseFear, 2 }
                    },
                    SpecialRules = new List<string>() { "Gives Bone meal as part" },
                    XP = 80,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Slime",
                    Species = MonsterSpeciesName.Slime,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 25 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Corrosive, 0}
                    },
                    XP = 120,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Sphinx",
                    Species = MonsterSpeciesName.Sphinx,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 38 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FlyerOutdoors, 0},
                        { MonsterSpecialName.RiddleMaster, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 1000,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Stone Golem",
                    Species = MonsterSpeciesName.StoneGolem,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 4 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 30 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.HardAsRock, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 450,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Stone Troll",
                    Species = MonsterSpeciesName.StoneTroll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 60 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 32 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 15 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Stupid, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Regeneration, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 550,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Tomb Guardian",
                    Species = MonsterSpeciesName.TombGuardian,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 550,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Vampire",
                    Species = MonsterSpeciesName.Vampire,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 70 },
                        { BasicStat.Resolve, 70 },
                        { BasicStat.HitPoints, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 75 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.CauseFear, 10 }
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.MasterOfTheDead
                    },
                    XP = 2000,
                    TreasureType = TreasureType.T5
                },
                new Monster()
                {
                    Name = "Vampire Fledgling",
                    Species = MonsterSpeciesName.Vampire,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 65 },
                        { BasicStat.Resolve, 70 },
                        { BasicStat.HitPoints, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 75 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.CauseFear, 10 }
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Seduction
                    },
                    XP = 1500,
                    TreasureType = TreasureType.T5
                },
                new Monster()
                {
                    Name = "Werewolf",
                    Species = MonsterSpeciesName.Werewolf,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 9 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FerociousCharge, 0},
                        { MonsterSpecialName.Regeneration, 0}
                    },
                    XP = 280,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Wight",
                    Species = MonsterSpeciesName.Wight,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 45 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.JustBones, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    SpecialRules = new List<string>() { "Cursed Weapon" },
                    XP = 180,
                    TreasureType = TreasureType.T2
                },
                new Monster()
                {
                    Name = "Wraiths",
                    Species = MonsterSpeciesName.Wraith,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 65 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Ethereal, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    SpecialRules = new List<string>() { "Cursed Weapon" },
                    XP = 500,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Wyvern",
                    Species = MonsterSpeciesName.Wyvern,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 4 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 60 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseTerror, 5 },
                        { MonsterSpecialName.XLarge, 0}
                    },
                    XP = 1800,
                    TreasureType = TreasureType.Part
                },
                new Monster()
                {
                    Name = "Zombie",
                    Species = MonsterSpeciesName.Zombie,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 10 },
                        { BasicStat.Resolve, 25 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 }
                    },
                    MinDamage = 1,
                    MaxDamage = 8,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Slow, 0},
                        { MonsterSpecialName.CauseFear, 2 }
                    },
                    XP = 80,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Zombie Ogre",
                    Species = MonsterSpeciesName.ZombieOgre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 15 },
                        { BasicStat.Resolve, 25 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 }
                    },
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Slow, 0},
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 450,
                    TreasureType = TreasureType.T1
                },
                new Monster()
                {
                    Name = "Demonic Conjurer",
                    Species = MonsterSpeciesName.Demon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 13 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Magic,
                    Behavior = MonsterBehaviorType.MagicUser,                    
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Demon, 0}
                    },
                    XP = 175,
                    TreasureType = TreasureType.T4
                },
                new Monster()
                {
                    Name = "The Brood Mother",
                    Species = MonsterSpeciesName.GiantRat,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 30 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Beasts,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Diseased, 0},
                        { MonsterSpecialName.CauseFear, 2 }
                    },
                    XP = 115,
                    TreasureType = TreasureType.Part,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Ulfric",
                    Species = MonsterSpeciesName.Zombie,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 35 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.CauseFear, 3 }
                    },
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    ArmourValue = 2,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    XP = 110,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Imgrahil the Apprentice",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 13 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    ArmourValue = 1,
                    Weapons = new List<Weapon>() { _weapon.CreateModifiedMeleeWeapon(
                        "Dagger", "Poisonous Dagger",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        })},
                    Spells = new List<MonsterSpell>()
                    {
                        SpellService.GetMonsterSpellByName("Raise dead"),
                        SpellService.GetMonsterSpellByName("Healing"),
                        SpellService.GetMonsterSpellByName("Vampiric touch"),
                        SpellService.GetMonsterSpellByName("Mirrored self"),
                    },
                    XP = 200,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Amburr the Ettin",
                    Species = MonsterSpeciesName.Ettin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { _weapon.CreateModifiedMeleeWeapon(
                        "Warhammer", "Diseased Warhammer",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Diseased, 0);
                        }) },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Stupid, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike,
                        SpecialActiveAbility.FreeBellow
                    },
                    XP = 550,
                    TreasureType = TreasureType.T2,
                    IsUnique = true,
                    QuestItem = QuestItem.SpiderAmulet
                },
                new Monster()
                {
                    Name = "Grop",
                    Species = MonsterSpeciesName.Orc,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 16 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Bandits_Brigands,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new Weapon() },
                    HasShield = true,
                    ArmourValue = 2,
                    XP = 110,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Digg",
                    Species = MonsterSpeciesName.Ogre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 16 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new Weapon() },
                    HasShield = true,
                    ArmourValue = 2,
                    XP = 110,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Kraghul the Mighty",
                    Species = MonsterSpeciesName.Minotaur,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 4 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 }
                    },
                    ToHitPenalty = 5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new Weapon() },
                    ArmourValue = 3,
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 450,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Belua",
                    Species = MonsterSpeciesName.GiganticSpider,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Poisonous, 0},
                        { MonsterSpecialName.CauseTerror, 5 },
                        { MonsterSpecialName.Large, 0}
                    },
                    SpecialRules = new List<string>() { "Summon children: Summons giant spider on behaviour roll of 5-6." },
                    XP = 900,
                    TreasureType = TreasureType.Part,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Briggo",
                    Species = MonsterSpeciesName.Ogre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 35 },
                        { BasicStat.HitPoints, 24 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 400,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Gorm",
                    Species = MonsterSpeciesName.Ogre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 35 },
                        { BasicStat.HitPoints, 24 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Large, 0}
                    },
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.SweepingStrike
                    },
                    XP = 500,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Goldfrid the Short",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 35 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    Weapons = new List<Weapon>() {
                        EquipmentService.GetWeaponByName("Shortbow")?.Clone() ?? new Weapon(),
                        EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new Weapon()
                    },
                    ArmourValue = 1,
                    XP = 120,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Madame Isabelle",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 14 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new Weapon() },
                    ArmourValue = 1,
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Seduction
                    },
                    XP = 140,
                    TreasureType = TreasureType.T3,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Gaul the Mauler",
                    Species = MonsterSpeciesName.Orc,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 22 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new Weapon() },
                    HasShield = true,
                    ArmourValue = 2,
                    XP = 150,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Molgor The Fiend of Summerhall",
                    Species = MonsterSpeciesName.Minotaur,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 35 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.FerociousCharge, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 450,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Beast of Turog Hall",
                    Species = MonsterSpeciesName.CommonTroll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Regeneration, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Stupid, 0},
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 700,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Turog",
                    Species = MonsterSpeciesName.Skeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.CauseFear, 10 },
                        { MonsterSpecialName.Regeneration, 0}
                    },
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    XP = 300,
                    TreasureType = TreasureType.Turog,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Ragnalf the Mad",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 85 },
                        { BasicStat.HitPoints, 35 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 65 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Spells = BuildSpellList(3, 2, 2),
                    SpecialRules = new List<string>() { "Raise dead" },
                    XP = 800,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Queen Khaba",
                    Species = MonsterSpeciesName.Mummy,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 85 },
                        { BasicStat.HitPoints, 35 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 65 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.WeakToFire, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 800,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Mapmaker",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 35 },
                        { BasicStat.HitPoints, 10 }
                    },
                    SkillStats = new Dictionary<Skill, int>(),
                    ToHitPenalty = -5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    SpecialRules = new List<string>() { "Dodge (45)" },
                    XP = 0,
                    TreasureType = TreasureType.None,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Undead Wyvern",
                    Species = MonsterSpeciesName.Skeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.JustBones, 0},
                        { MonsterSpecialName.CauseTerror, 3 },
                        { MonsterSpecialName.XLarge, 0}
                    },
                    XP = 1400,
                    TreasureType = TreasureType.Part,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Dregrir the Wyzard",
                    Species = MonsterSpeciesName.Goblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 12 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 40 },
                        { Skill.RangedSkill, 60 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    Spells = BuildSpellList(2, 2, 2),
                    XP = 150,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Klatche the Ogre",
                    Species = MonsterSpeciesName.Ogre,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() {
                        EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new Weapon(),
                        EquipmentService.GetWeaponByName("Battlehammer")?.Clone() ?? new Weapon()
                    },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.MultipleAttacks, 2 }
                    },
                    XP = 650,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Easta Rubeet",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    ArmourValue = 2,
                    SpecialRules = new List<string>() { "Frost DMG" },
                    XP = 190,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Master Locksmith",
                    Species = MonsterSpeciesName.Mummy,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 80 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = 0,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new Weapon() },
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToFire, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 600,
                    TreasureType = TreasureType.TheMasterLocksmith,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Guardian Scorpion",
                    Species = MonsterSpeciesName.GiantScorpion,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 85 },
                        { BasicStat.HitPoints, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WallCrawler, 0},
                        { MonsterSpecialName.CauseFear, 6 },
                        { MonsterSpecialName.Poisonous, 0}
                    },
                    XP = 250,
                    TreasureType = TreasureType.Part,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Queen Khezira",
                    Species = MonsterSpeciesName.Mummy,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 75 },
                        { BasicStat.HitPoints, 45 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 45 }
                    },
                    ToHitPenalty = 5,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.WeakToFire, 0},
                        { MonsterSpecialName.CauseFear, 7 },
                        { MonsterSpecialName.Regeneration, 0}
                    },
                    ArmourValue = 1,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword")?.Clone() ?? new Weapon() },
                    XP = 1000,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Chadepho",
                    Species = MonsterSpeciesName.Drider,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 4 },
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 35 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 }
                    },
                    MinDamage = 1,
                    MaxDamage = 12,
                    ToHitPenalty = 0,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WallCrawler, 0},
                        { MonsterSpecialName.CauseFear, 5 },
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 650,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Lord Brenann",
                    Species = MonsterSpeciesName.Skeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword")?.Clone() ?? new Weapon() },
                    ArmourValue = 3,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.JustBones, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 250,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Captain",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 14 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    HasShield = true,
                    XP = 280,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Novelm Slateshadow",
                    Species = MonsterSpeciesName.Skeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.Undead,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    Weapons = new List<Weapon>() { _weapon.CreateModifiedMeleeWeapon(
                        "Longsword", "Cursed Longsword",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Cursed, 0);
                        }) },
                    ArmourValue = 3,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.JustBones, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 280,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Marla the Witch",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 18 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = 5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new Weapon() },
                    ArmourValue = 1,
                    Spells = new List<MonsterSpell>()
                    {
                        SpellService.GetMonsterSpellByName("Mute"),
                        SpellService.GetMonsterSpellByName("Seduce"),
                        SpellService.GetMonsterSpellByName("Blind"),
                    },
                    XP = 200,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Adras the Witch",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 70 },
                        { BasicStat.HitPoints, 14 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Staff")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    Spells = new List<MonsterSpell>()
                    {
                        SpellService.GetMonsterSpellByName("Flare"),
                        SpellService.GetMonsterSpellByName("Healing hand"),
                        SpellService.GetMonsterSpellByName("Vampiric touch"),
                        SpellService.GetMonsterSpellByName("Summon demon"),
                    },
                    XP = 260,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Phylax the Witch",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 14 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 },
                        { Skill.RangedSkill, 50 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    Spells = new List<MonsterSpell>()
                    {
                        SpellService.GetMonsterSpellByName("Shield"),
                        SpellService.GetMonsterSpellByName("Fireball"),
                        SpellService.GetMonsterSpellByName("Mirrored self"),
                    },
                    XP = 200,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Ghammi the Witch",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 14 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    Spells = new List<MonsterSpell>()
                    {
                        SpellService.GetMonsterSpellByName("Healing hand"),
                        SpellService.GetMonsterSpellByName("Fireball"),
                        SpellService.GetMonsterSpellByName("Mirrored self"),
                    },
                    XP = 200,
                    TreasureType = TreasureType.T4,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Elder Witch",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 },
                        { Skill.RangedSkill, 70 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new Weapon() },
                    ArmourValue = 1,
                    Spells = new List<MonsterSpell>()
                    {
                        SpellService.GetMonsterSpellByName("Raise dead"),
                        SpellService.GetMonsterSpellByName("Mute"),
                        SpellService.GetMonsterSpellByName("Seduce"),
                        SpellService.GetMonsterSpellByName("Blind"),
                    },
                    XP = 350,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Goblin Chieftain",
                    Species = MonsterSpeciesName.Goblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 35 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 35 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    SpecialRules = new List<string>() { "Poisonous Weapon" },
                    XP = 130,
                    TreasureType = TreasureType.T3
                },
                new Monster()
                {
                    Name = "Ribbit",
                    Species = MonsterSpeciesName.Demon,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 75 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    Weapons = new List<Weapon>() {
                        EquipmentService.GetWeaponByName("Shortsword")?.Clone() ?? new Weapon(),
                        _weapon.CreateModifiedRangedWeapon(
                        "Sling", "Spiked Balls",
                        weapon =>
                        {
                            weapon.ReloadTime = 0;
                        }),
                    },
                    ArmourValue = 3,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    XP = 140,
                    TreasureType = TreasureType.None,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Alchemist Outlaw",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 5 },
                        { BasicStat.Dexterity, 40 },
                        { BasicStat.Resolve, 55 },
                        { BasicStat.HitPoints, 25 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 60 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidRanged,
                    Weapons = new List<Weapon>() { _weapon.CreateModifiedRangedWeapon(
                        "Crossbow Pistol", "Poisonous Crossbow Pistol",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        }),
                        _weapon.CreateModifiedMeleeWeapon(
                        "Dagger", "Poisonous Dagger",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        })
                    },
                    ArmourValue = 1,
                    SpecialRules = new List<string>() { "1 Healing Potion", "1 Vial of Black Acanthus Gas" },
                    XP = 140,
                    TreasureType = TreasureType.TheAlchemistOutlaw,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Ancient Stone Golem",
                    Species = MonsterSpeciesName.StoneGolem,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 4 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 25 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 35 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    MinDamage = 1,
                    MaxDamage = 10,
                    ToHitPenalty = 0,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.HardAsRock, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Large, 0}
                    },
                    XP = 500,
                    TreasureType = TreasureType.Part,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Trogskegg",
                    Species = MonsterSpeciesName.StoneTroll,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 20 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 40 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 50 },
                        { Skill.RangedSkill, 15 }
                    },
                    ToHitPenalty = -5,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.Beast,
                    Weapons = new List<Weapon>() { _weapon.CreateModifiedMeleeWeapon(
                        "Warhammer", "Magical Warhammer",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.ArmourPiercing, 2);
                        }) },
                    ArmourValue = 1,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Regeneration, 0},
                        { MonsterSpecialName.CauseFear, 3 },
                        { MonsterSpecialName.Large, 0}
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Bellow
                    },
                    XP = 650,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Gneeb the Manslayer",
                    Species = MonsterSpeciesName.Goblin,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    Weapons = new List<Weapon>() { _weapon.CreateModifiedMeleeWeapon(
                        "Javelin", "Poisonous Javelin",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        }) },
                    ActiveStatusEffects = new List<ActiveStatusEffect>() {
                        new ActiveStatusEffect(StatusEffectType.Frenzy, -1)
                    },
                    ArmourValue = 4,
                    HasShield = true,
                    SpecialRules = new List<string>() { "Cloak (Armour +1 from behind)" },
                    XP = 600,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Ada the Necromancer",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 3 },
                        { BasicStat.NaturalArmour, 3 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 70 },
                        { BasicStat.Resolve, 70 },
                        { BasicStat.HitPoints, 28 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 75 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger")?.Clone() ?? new Weapon() },
                    ArmourValue = 1,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.CauseFear, 5 }
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Seduction
                    },
                    Spells = new List<MonsterSpell>()
                    {
                        SpellService.GetMonsterSpellByName("Healing"),
                        SpellService.GetMonsterSpellByName("Vampiric touch"),
                        SpellService.GetMonsterSpellByName("Fireball"),
                    },
                    XP = 1500,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "Jarl Knut",
                    Species = MonsterSpeciesName.Skeleton,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 50 },
                        { BasicStat.Resolve, 50 },
                        { BasicStat.HitPoints, 30 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 70 },
                        { Skill.RangedSkill, 55 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new Weapon() },
                    HasShield = true,
                    ArmourValue = 2,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                    },
                    XP = 140,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Apostle",
                    Species = MonsterSpeciesName.Vampire,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.DamageBonus, 2 },
                        { BasicStat.NaturalArmour, 2 },
                        { BasicStat.Move, 6 },
                        { BasicStat.Dexterity, 65 },
                        { BasicStat.Resolve, 70 },
                        { BasicStat.HitPoints, 20 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 65 },
                        { Skill.RangedSkill, 40 }
                    },
                    ToHitPenalty = -15,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HigherUndead,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword")?.Clone() ?? new Weapon() },
                    ArmourValue = 2,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.Silent, 0},
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.CauseFear, 10 }
                    },
                    ActiveAbilities = new List<SpecialActiveAbility>()
                    {
                        SpecialActiveAbility.Seduction
                    },
                    XP = 1500,
                    TreasureType = TreasureType.T5,
                    IsUnique = true
                },
                new Monster()
                {
                    Name = "The Master",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.NaturalArmour, 1 },
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 40 },
                        { BasicStat.HitPoints, 14 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 45 }
                    },
                    ArmourValue = 1,
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.MagicUser,
                    Weapons = new List<Weapon>() { _weapon.CreateModifiedMeleeWeapon(
                        "Dagger", "Poisonous Dagger",
                        weapon =>
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        }) },
                    Spells = BuildSpellList(3, 4, 0),
                    SpecialRules = new List<string>() { "Raise dead" },
                    XP = 200,
                    TreasureType = TreasureType.T4,
                    IsUnique = true,
                    QuestItem = QuestItem.EngravedDagger
                },
                new Monster()
                {
                    Name = "Emil the Caretaker",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 30 },
                        { BasicStat.Resolve, 60 },
                        { BasicStat.HitPoints, 18 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 55 },
                        { Skill.RangedSkill, 70 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.LowerUndead,
                    PassiveSpecials = new Dictionary<MonsterSpecialName, int>() {
                        { MonsterSpecialName.WeakToSilver, 0},
                        { MonsterSpecialName.Silent, 0}
                    },
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe")?.Clone() ?? new Weapon() },
                    XP = 110,
                    TreasureType = TreasureType.T2,
                    IsUnique = true,
                    QuestItem = QuestItem.BronzeKey
                },
                new Monster()
                {
                    Name = "The Bandit",
                    Species = MonsterSpeciesName.Human,
                    BasicStats = new Dictionary<BasicStat, int>
                    {
                        { BasicStat.Move, 4 },
                        { BasicStat.Dexterity, 45 },
                        { BasicStat.Resolve, 35 },
                        { BasicStat.HitPoints, 15 }
                    },
                    SkillStats = new Dictionary<Skill, int>
                    {
                        { Skill.CombatSkill, 60 },
                        { Skill.RangedSkill, 0 }
                    },
                    ToHitPenalty = -10,
                    Type = EncounterType.MainQuest,
                    Behavior = MonsterBehaviorType.HumanoidMelee,
                    ArmourValue = 3,
                    HasShield = true,
                    Weapons = new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe")?.Clone() ?? new Weapon() },
                    XP = 130,
                    TreasureType = TreasureType.T2,
                    IsUnique = true
                }
            };
        }

        private Monster GetMonsterByName(string name)
        {
            return Monsters.First(m => m.Name == name);
        }

        /// <summary>
        /// Creates a list of monsters by parsing a dictionary of string-based parameters.
        /// This acts as a bridge between data-driven setup actions and the monster creation logic.
        /// </summary>
        /// <param name="parameters">A dictionary containing the monster setup instructions.</param>
        /// <returns>A list of new Monster objects, or an empty list if setup fails.</returns>
        internal List<Monster> GetEncounterByParams(Dictionary<string, string> parameters)
        {
            // 1. Get the Template Monster
            // This is the only required parameter. If it's missing, we can't proceed.
            if (!parameters.TryGetValue("Name", out var monsterName))
            {
                Console.WriteLine("Error: SpawnMonster action is missing a 'Name' parameter.");
                return new List<Monster>();
            }

            string templateMonster = monsterName;
            // Get the special rule, if any.
            string? specialRule = parameters.GetValueOrDefault("SpecialRule");

            // 2. Parse all optional parameters from the dictionary

            // Safely parse the count, defaulting to 1 if not specified.
            int count = 1;
            if (parameters.TryGetValue("Count", out var countStr))
            {
                if (countStr.Contains('d'))
                {
                    count = RandomHelper.RollDice(countStr);
                }
                else if (int.TryParse(countStr, out var parsedCount))
                {
                    count = parsedCount;
                }
            }

            // Safely parse the armour value.
            int armourValue = parameters.TryGetValue("Armour", out var armourStr) && int.TryParse(armourStr, out var parsedArmour)
                ? parsedArmour
                : 0;

            // Safely parse the shield status.
            bool hasShield = parameters.TryGetValue("Shield", out var shieldStr) && bool.TryParse(shieldStr, out var parsedShield)
                ? parsedShield
                : false;

            // Parse the comma-separated list of weapons.
            List<Weapon> weapons = new List<Weapon>();
            if (parameters.TryGetValue("Weapons", out var weaponsStr))
            {
                foreach (string weaponName in weaponsStr.Split(','))
                {
                    bool isCursed = false;
                    bool isPoisoned = false;
                    Weapon? weapon;
                    if (weaponName.Contains("Cursed"))
                    {
                        // Removed cursed from name to get the correct weapon returned
                        isCursed = true;
                        weapon = EquipmentService.GetWeaponByName(weaponName.Replace("Cursed", "").Trim());
                    }
                    else if (weaponName.Contains("Poisonous"))
                    {
                        // Removed cursed from name to get the correct weapon returned
                        isPoisoned = true;
                        weapon = EquipmentService.GetWeaponByName(weaponName.Replace("Poisonous", "").Trim());
                    }
                    else
                    {
                        weapon = EquipmentService.GetWeaponByName(weaponName.Trim());
                    }

                    if (weapon != null)
                    {
                        weapon = weapon.Clone();
                        if (isCursed)
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Cursed, 0);
                        }
                        else if (isPoisoned)
                        {
                            weapon.Properties.TryAdd(WeaponProperty.Poisoned, 0);
                        }
                        weapons.Add(weapon);
                    }
                }
            }

            // Parse the comma-separated list of spells.
            List<MonsterSpell> spells = new List<MonsterSpell>();
            if (parameters.TryGetValue("Spells:", out var spellsStr))
            {
                foreach (string spellName in spellsStr.Split(','))
                {
                    // Assumes a service exists to get spell data by name.
                    MonsterSpell? spell = SpellService.GetMonsterSpellByName(spellName.Trim());
                    if (spell != null)
                    {
                        spells.Add(spell);
                    }
                }
            }

            if (parameters.TryGetValue("BaseMonster", out var baseMonsterName))
            {
                var baseMonster = GetMonsterByName(baseMonsterName).Clone();
                // Apply modifications from parameters to baseMonster
                baseMonster.Name = monsterName;
                if (weapons.Any())
                {
                    baseMonster.Weapons = weapons;
                }
                baseMonster.ArmourValue = armourValue;
                baseMonster.HasShield = hasShield;
                if (spells.Any())
                {
                    baseMonster.Spells = spells;
                }
                if (specialRule != null)
                {
                    baseMonster.SpecialRules.Add(specialRule);
                }
                return new List<Monster> { baseMonster };
            }

            // Call the existing BuildMonsters method with the parsed data
            return BuildMonsters(
                count,
                templateMonster,
                weapons.Any() ? weapons : null,
                armourValue,
                hasShield,
                spells.Any() ? spells : null,
                specialRule
            );
        }
    }
}