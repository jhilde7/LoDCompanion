﻿
using LoDCompanion.Utilities;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Models;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Services.Dungeon
{
    public class EncounterService
    {
        private readonly GameDataService _gameData;

        public List<Monster> Monsters => GetMonsters();

        public EncounterService(GameDataService gameDataService)
        {
            _gameData = gameDataService;
        }

        public List<Monster> GetRandomEncounterByType(EncounterType type)
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Satyr", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 0, true);
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1, true);
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gnoll Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(2, "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Satyr Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Beastman Guard", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true));
                            break;
                        case 21:
                            encounters.Add(BuildMonster("Cave Bear"));
                            break;
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(1, "Gnoll Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(1, 1, 1)));
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
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 0, true));
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Satyr", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 1, true));
                            break;
                        case <= 34:
                            encounters.Add(BuildMonster("Shambler"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Satyr", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 1, true));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Crossbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2));
                            break;
                        case <= 38:
                            encounters.Add(BuildMonster("Giant Centipede"));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(1, "Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 3);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Lesser Plague Demon");
                            break;
                        case <= 44:
                            encounters = BuildMonsters(1, "River Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 2);
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Satyr", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() }, 3));
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 48:
                            encounters.Add(BuildMonster("Slime"));
                            encounters.AddRange(BuildMonsters(1, "Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2));
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Ettin", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 2);
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Werewolf");
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Stone Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 2);
                            encounters.Add(BuildMonster("Giant Centipede"));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Gargoyle");
                            break;
                        case 57:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gnoll Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case 58:
                            encounters.Add(BuildMonster("Griffon"));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Satyr", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true));
                            encounters.AddRange(BuildMonsters(1, "Gnoll Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(1, 1, 1)));
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(1, "Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true));
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Blood Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 0, false, null, "Cursed weapon");
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(1, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(1, "Ettin", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1));
                            break;
                        case <= 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Plague Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 0, false, null, "Cursed weapon");
                            break;
                        case <= 76:
                            encounters = BuildMonsters(2, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            break;
                        case <= 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(1, "Gnoll Sergeant", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2));
                            encounters.AddRange(BuildMonsters(1, "Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2));
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gnoll", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(1, "Gnoll Sergeant", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2));
                            encounters.AddRange(BuildMonsters(1, "Gnoll Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Blood Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 0, false, null, "Cursed weapon");
                            break;
                        case <= 84:
                            encounters.Add(BuildMonster("Bloated Demon"));
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Bloated Demon");
                            encounters.AddRange(BuildMonsters(3, "Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2));
                            break;
                        case 87:
                            encounters.Add(BuildMonster("Lurker"));
                            break;
                        case 88:
                            encounters = BuildMonsters(1, "Lurker", null, 0, false, BuildSpellList(3, 2, 2));
                            encounters.AddRange(BuildMonsters(2, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1));
                            break;
                        case <= 90:
                            encounters.Add(BuildMonster("Giant Spider"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Leech"));
                            break;
                        case <= 92:
                            encounters.Add(BuildMonster("Wyvern"));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, "Beastman", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2));
                            break;
                        case <= 96:
                            encounters.Add(BuildMonster("Gigantic Spider"));
                            break;
                        case <= 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Leech");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, "Blood Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 0, false, null, "Cursed weapon"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Lesser Plague Demon"));
                            break;
                        case <= 100:
                            encounters.AddRange(BuildMonsters(1, "Greater Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 3));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 1, true);
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Skeleton Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2);
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Zombie", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ghoul");
                            break;
                        case <= 26:
                            encounters = BuildMonsters(1, "Shambler");
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(1, 1, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 30:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Zombie", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 2, true);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1, true);
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dire Wolf");
                            break;
                        case <= 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dire Wolf");
                            monster = BuildMonster("Necromancer", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(1, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon> { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            break;
                        case <= 52:
                            encounters = BuildMonsters(2, "Mummy", null, 1);
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Mummy", null, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Zombie"));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 3);
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Spider");
                            break;
                        case <= 62:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon> { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Zombie", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Minotaur Skeleton", new List<Weapon> { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() });
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Ghoul");
                            break;
                        case <= 70:
                            encounters = BuildMonsters(2, "Zombie Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 2);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 72:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 3);
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() }, 3);
                            encounters.AddRange(BuildMonsters(4, "Skeleton Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2));
                            break;
                        case <= 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Minotaur Skeleton", new List<Weapon> { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 1);
                            break;
                        case <= 80:
                            encounters = BuildMonsters(1, "Vampire Fledgling", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 7), "Zombie", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(1, "Ghost");
                            break;
                        case <= 84:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 10), "Zombie", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 6), "Zombie", null, 2);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Vampire Fledgling", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Ghoul"));
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith");
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 3);
                            break;
                        case <= 94:
                            encounters = BuildMonsters(2, "Ghost");
                            break;
                        case <= 96:
                            encounters = BuildMonsters(1, "Banshee");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Zombie", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith");
                            monster = BuildMonster("Necromancer", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 100:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith"));
                            encounters.AddRange(BuildMonsters(1, "Vampire ", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Wolf"));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(2, "Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() });
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Pox Rat");
                            encounters.Add(BuildMonster("Slime"));
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 3));
                            break;
                        case <= 30:
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(2, "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            monster = BuildMonster("Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(1, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() });
                            encounters.Add(BuildMonster("Fallen Knight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true));
                            encounters.Add(BuildMonster("Fallen Knight", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 4, true));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(2, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Crossbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() }, 2));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 0, true);
                            monster = BuildMonster("Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 48:
                            encounters = BuildMonsters(1, "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() }, 1);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(2, "Shambler");
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Fallen Knight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true);
                            break;
                        case <= 54:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1);
                            break;
                        case <= 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            monster = BuildMonster("Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(2, "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            encounters.Add(BuildMonster("Ogre Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 2);
                            break;
                        case <= 62:
                            encounters = BuildMonsters(2, "Giant Centipede");
                            break;
                        case <= 64:
                            encounters = BuildMonsters(2, "Shambler");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Crossbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 3));
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2);
                            encounters.Add(BuildMonster("Bandit Leader", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(2, "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Fallen Knight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true, null, "Cursed weapon");
                            monster = BuildMonster("Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Greater Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Fallen Knight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            break;
                        case <= 78:
                            encounters = BuildMonsters(2, "Shambler");
                            monster = BuildMonster("Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 80:
                            encounters = BuildMonsters(3, "Bandit Leader", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(4, "Bandit Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Crossbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 84:
                            encounters = BuildMonsters(4, "Centaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 3, true);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            monster = BuildMonster("Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Summon Greater Demon"));
                            encounters.Add(monster);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(3, "Ogre Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 92:
                            encounters = BuildMonsters(2, "Centaur Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(2, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(3, "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 3);
                            break;
                        case <= 96:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Fallen Knight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true, null, "Cursed weapon");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Bandit", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2));
                            break;
                        case <= 98:
                            encounters.Add(BuildMonster("Ogre Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() });
                            break;
                        case <= 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1, true);
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Cave Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf"));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(1, "Orc Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 2, true));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, false, BuildSpellList(1, 1, 0)));
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
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Orc Brute", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1));
                            encounters.Add(BuildMonster("Orc Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 3, true));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Shambler"));
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 1, false, BuildSpellList(1, 1, 1)));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 42:
                            encounters = BuildMonsters(2, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 44:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Orc Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 2, true));
                            encounters.Add(BuildMonster("Orc Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2));
                            break;
                        case <= 48:
                            encounters = BuildMonsters(1, "Ettin", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() });
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Cave Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Stone Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 2);
                            break;
                        case <= 56:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(2, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(1, "Giant Centipede");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Orc Brute", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 1));
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 1, 1)));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf"));
                            encounters.Add(BuildMonster("Ogre Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true));
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() }, 2);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 2, true);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc Brute", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(1, "River Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2));
                            break;
                        case <= 70:
                            encounters = BuildMonsters(2, "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() });
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Orc Brute", new List<Weapon> { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 0, false, BuildSpellList(3, 1, 1)));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.AddRange(BuildMonsters(2, "Ettin", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }));
                            break;
                        case <= 78:
                            encounters = BuildMonsters(2, "Shambler");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Goblin Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(3, "Orc Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(4, "Cave Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 84:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(4, "Centaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 2, true);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Orc Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 1, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 90:
                            encounters = BuildMonsters(2, "Ogre Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Ogre Berserker", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 94:
                            encounters = BuildMonsters(4, "Giant Spider");
                            encounters.Add(BuildMonster("Lurker", null, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case <= 96:
                            encounters = BuildMonsters(2, "Gigantic Spider");
                            break;
                        case <= 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc Brute", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2));
                            encounters.AddRange(BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0));
                            break;
                        case <= 100:
                            encounters = BuildMonsters(1, "Orc Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true);
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gecko", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Gecko", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gecko", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, true));
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Frogling", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, true));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1, true);
                            break;
                        case <= 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Gecko", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1, true));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Frogling", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 0, true);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 1, true);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 28:
                            encounters = BuildMonsters(1, "Giant Toad");
                            break;
                        case <= 30:
                            encounters = BuildMonsters(1, "Giant Centipede");
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true);
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(2, "Gecko", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1));
                            encounters.Add(BuildMonster("Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Raptor"));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Raptor");
                            break;
                        case <= 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), "Giant Spider");
                            break;
                        case <= 42:
                            encounters = BuildMonsters(2, "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2);
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Giant Toad"));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Toad"));
                            break;
                        case <= 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() }, 3);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 1, false, BuildSpellList(1, 1, 1)));
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 54:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(2, "Slime"));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(2, "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(2, "Gecko", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 1, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Gecko", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Raptor"));
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Snake");
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Frogling", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
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
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true));
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Toad");
                            encounters.AddRange(BuildMonsters(2, "Salamander"));
                            break;
                        case <= 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2);
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Saurian", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(3, "Saurian Warchief", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 84:
                            encounters = BuildMonsters(1, "Gigantic Snake");
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Basilisk");
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            encounters.Add(BuildMonster("Saurian Priest", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 1, false, BuildSpellList(4, 2, 2)));
                            break;
                        case <= 90:
                            encounters = BuildMonsters(1, "Saurian Warchief", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.Add(BuildMonster("Basilisk"));
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Naga", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(2, "Giant Toad"));
                            encounters.Add(BuildMonster("Basilisk"));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(3, "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 3);
                            break;
                        case <= 96:
                            encounters = BuildMonsters(1, "Gigantic Snake");
                            break;
                        case <= 98:
                            encounters = BuildMonsters(2, "Frogling", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gecko Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Saurian Elite", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1, true));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Dark Elf Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1));
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Dark Elf Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 1, false, null, "Poisonous 1"));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Wolf"));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(1, "Dark Elf Captain", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dark Elf Sniper", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
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
                            encounters = BuildMonsters(1, "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(2, "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Dark Elf Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf Sniper", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Dark Elf Captain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true));
                            break;
                        case <= 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Blood Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Flail") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(2, "Harpy"));
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 40:
                            encounters = BuildMonsters(2, "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Dark Elf Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Crossbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1, false, null, "Poisonous 1"));
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Dark Elf Captain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true));
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 0, true);
                            encounters.Add(BuildMonster("Dark Elf Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, false, BuildSpellList(2, 1, 1)));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Plague Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Psyker", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case <= 58:
                            encounters = BuildMonsters(1, "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Dark Elf Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1, false, null, "Poisonous 1"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider"));
                            break;
                        case <= 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 2);
                            break;
                        case <= 62:
                            encounters = BuildMonsters(2, "Giant Centipede");
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 3);
                            encounters.AddRange(BuildMonsters(2, "Shambler"));
                            break;
                        case <= 66:
                            encounters = BuildMonsters(1, "Dark Elf Captain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true);
                            break;
                        case <= 68:
                            encounters = BuildMonsters(2, "Blood Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() });
                            break;
                        case <= 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Blood Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true, null, "Cursed weapon");
                            encounters.Add(BuildMonster("Dark Elf Warlock", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider"));
                            break;
                        case <= 76:
                            encounters = BuildMonsters(2, "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            break;
                        case <= 78:
                            encounters = BuildMonsters(2, "Shambler");
                            encounters.AddRange(BuildMonsters(2, "Psyker", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case <= 80:
                            encounters = BuildMonsters(2, "Dark Elf Captain", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(4, "Dark Elf Sniper", new List<Weapon>() { EquipmentService.GetWeaponByName("Crossbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2));
                            break;
                        case <= 82:
                            encounters = BuildMonsters(2, "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2);
                            break;
                        case <= 84:
                            encounters = BuildMonsters(1, "Medusa", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() }, 0, true);
                            break;
                        case <= 86:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Psyker", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case <= 88:
                            encounters = BuildMonsters(1, "Basilisk");
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Dark Elf", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(2, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 2));
                            break;
                        case <= 92:
                            encounters = BuildMonsters(1, "Medusa", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            break;
                        case <= 94:
                            encounters = BuildMonsters(3, "Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 3);
                            break;
                        case <= 96:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Blood Demon", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 4, true, null, "Cursed weapon");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Dark Elf Assassin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2, false, null, "Poisonous 1"));
                            break;
                        case <= 98:
                            encounters.Add(BuildMonster("Dryder", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 1, true);
                            break;
                        case <= 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Leech");
                            break;
                        case <= 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 0, true);
                            break;
                        case <= 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1, true);
                            encounters.Add(BuildMonster("Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 1));
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2));
                            break;
                        case <= 20:
                            encounters = BuildMonsters(1, "Giant Scorpion");
                            break;
                        case <= 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Broadsword") ?? new Weapon() });
                            break;
                        case <= 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Gargoyle");
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Spider");
                            break;
                        case <= 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 30:
                            encounters = BuildMonsters(2, "Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 1);
                            break;
                        case <= 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Scorpion");
                            break;
                        case <= 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Skeleton", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Gargoyle"));
                            break;
                        case <= 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Spider");
                            break;
                        case <= 38:
                            encounters = BuildMonsters(1, "Sphinx");
                            break;
                        case <= 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1, true);
                            break;
                        case <= 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Snake");
                            encounters.AddRange(BuildMonsters(2, "Slime"));
                            break;
                        case <= 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Skeleton Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Longbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 1));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(1, "Giant Centipede");
                            break;
                        case <= 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 2);
                            monster = BuildMonster("Necromancer", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 50:
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            break;
                        case <= 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Battlehammer") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Shambler"));
                            break;
                        case <= 54:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 2));
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
                            encounters = BuildMonsters(1, "Zombie Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            break;
                        case <= 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            monster = BuildMonster("Mummy Priest", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Slime"));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Giant Scorpion");
                            break;
                        case <= 70:
                            encounters = BuildMonsters(2, "Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            encounters.Add(BuildMonster("Giant Scorpion"));
                            break;
                        case <= 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            break;
                        case <= 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 3);
                            break;
                        case <= 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(4, "Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Greatsword") ?? new Weapon() }, 2));
                            break;
                        case 77:
                            encounters = BuildMonsters(4, "Mummy", null, 1);
                            break;
                        case 78:
                            encounters = BuildMonsters(4, "Gargoyle");
                            break;
                        case <= 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), "Mummy", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            monster = BuildMonster("Mummy Queen", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, false, BuildSpellList(3, 2, 2));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2);
                            break;
                        case <= 88:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 2);
                            monster = BuildMonster("Mummy Queen", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, false, BuildSpellList(5, 2, 2));
                            monster.Spells.Add(SpellService.GetMonsterSpellByName("Raise Dead"));
                            encounters.Add(monster);
                            break;
                        case <= 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith");
                            break;
                        case <= 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Halberd") ?? new Weapon() }, 3);
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Wight", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "Wraith"));
                            monster = BuildMonster("Mummy Queen", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, false, BuildSpellList(3, 3, 2));
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() });
                            break;
                        case <= 15:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            break;
                        case <= 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), "Giant Snake");
                            break;
                        case <= 21:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Cave Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 24:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf"));
                            break;
                        case <= 27:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Orc", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Javelin") ?? new Weapon() }, 1, true));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 1, false, BuildSpellList(1, 1, 0)));
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
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 1);
                            break;
                        case <= 43:
                            encounters = BuildMonsters(1, "Ogre", new List<Weapon>() { EquipmentService.GetWeaponByName("Longsword") ?? new Weapon() }, 2, true);
                            encounters.AddRange(BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }));
                            break;
                        case <= 46:
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 1);
                            break;
                        case <= 49:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 2, true));
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2));
                            break;
                        case <= 52:
                            encounters = BuildMonsters(1, "River Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 56:
                            encounters = BuildMonsters(2, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Rat"));
                            encounters.Add(BuildMonster("Stone Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }));
                            break;
                        case <= 59:
                            encounters = BuildMonsters(2, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            break;
                        case <= 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 2));
                            break;
                        case <= 65:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 71:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Wolf");
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Morningstar") ?? new Weapon() }, 2, true));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Staff") ?? new Weapon() }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 74:
                            encounters = BuildMonsters(2, "Stone Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            break;
                        case <= 77:
                            encounters = BuildMonsters(2, "Stone Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(6, "Cave Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Net") ?? new Weapon(), EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }));
                            encounters.Add(BuildMonster("Goblin Shaman", new List<Weapon>() { EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case <= 81:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Spider");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 84:
                            encounters = BuildMonsters(2, "River Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            break;
                        case <= 87:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), "Giant Wolf");
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 3, true));
                            break;
                        case <= 91:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(3, 6), "Goblin", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortsword") ?? new Weapon() }, 2, true);
                            encounters.Add(BuildMonster("Goblin Chieftain", new List<Weapon>() { EquipmentService.GetWeaponByName("Battleaxe") ?? new Weapon() }, 4));
                            break;
                        case <= 94:
                            encounters = BuildMonsters(2, "River Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(3, 6), "Goblin Archer", new List<Weapon>() { EquipmentService.GetWeaponByName("Shortbow") ?? new Weapon(), EquipmentService.GetWeaponByName("Dagger") ?? new Weapon() }));
                            break;
                        case <= 97:
                            encounters = BuildMonsters(2, "River Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() });
                            encounters.Add(BuildMonster("Stone Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }));
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
                            encounters = BuildMonsters(1, "Common Troll", new List<Weapon>() { EquipmentService.GetWeaponByName("Warhammer") ?? new Weapon() }, 2);
                            break;
                        case 2:
                            encounters = BuildMonsters(1, "Minotaur", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2);
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
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), "FireElemental");
                            break;
                        case 2:
                            // This originally called dungeonManager.encounter.encounterType.
                            // In a service-oriented approach, the calling service (e.g., DungeonManagerService)
                            // would pass the appropriate current encounter type.
                            encounters = GetRandomEncounterByType(type);
                            break;
                    }
                    break;
                case EncounterType.R28:
                    encounters = BuildMonsters(2, "Mummy", null, 0);
                    break;
                case EncounterType.R30:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Snake");
                            encounters.AddRange(GetRandomEncounterByType(type));
                            break;
                        case 4:
                        case 5:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), "Giant Snake");
                            break;
                        default:
                            break;
                    }
                    break;
                case EncounterType.TombGuardian:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 4:
                        case <= 6:
                            encounters = BuildMonsters(2, "Tomb Guardian", new List<Weapon>() { EquipmentService.GetWeaponByName("Greataxe") ?? new Weapon() }, 2);
                            break;
                        default:
                            break;
                    }
                    break;
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
                if (newMonster.SpecialRules == null)
                {
                    newMonster.SpecialRules = new List<string>();
                }
                newMonster.SpecialRules.Add(specialRule);
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
                SpecialRules = new List<string>() { "Ghostly Howl", "Terror 5", "Ethereal", "Ghostly touch" },
                XP = 650,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Bat swarm",
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
                SpecialRules = new List<string>() { "Auto hit", "Flyer", "Always acts first on the first turn of battle" },
                XP = 15,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Beastman",
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
                SpecialRules = new List<string>() { "Frenzy" },
                XP = 110,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Bloated Demon",
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
                SpecialRules = new List<string>() { "Demon", "Floater", "Disease ridden", "Fear 5", "Large" },
                XP = 650,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Blood Demon",
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
                SpecialRules = new List<string>() { "Demon", "Frenzy" },
                XP = 200,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Cave Bear",
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
                SpecialRules = new List<string>() { "Ferocious charge" },
                XP = 130,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Cave Goblin",
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
                SpecialRules = new List<string>() { "Hate Dwarves" },
                XP = 70,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Cave Goblin Archer",
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
                SpecialRules = new List<string>() { "Hate Dwarves" },
                XP = 70,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Centaur",
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
                SpecialRules = new List<string>() { "Kick" },
                XP = 150,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Centaur Archer",
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
                SpecialRules = new List<string>(){ "Kick" },
                XP = 150,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Common Troll",
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
                SpecialRules = new List<string>(){ "Regenerate", "Stupid", "Simple weapons", "Large", "Bellow", "Fear 3" },
                XP = 500,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Basilisk",
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
                SpecialRules = new List<string>(){ "Petrify", "Large" },
                XP = 325,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Dark Elf",
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
                SpecialRules = new List<string>(){ "Sneaky" },
                XP = 135,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Dark Elf Captain",
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
                SpecialRules = new List<string>(){ "Magic User" },
                XP = 165,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Dire Wolf",
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
                SpecialRules = new List<string>(){ "Fear 3", "Ferocious charge" },
                XP = 80,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Dragon",
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
                SpecialRules = new List<string>(){ "Fire breath", "Terror 10", "X-Large", "Sweeping strike" },
                XP = 3500,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Drider",
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
                SpecialRules = new List<string>(){ "Fear 5", "Wall crawler", "Large" },
                XP = 600,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Fallen Knight",
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
                SpecialRules = new List<string> { "Magic User" },
                XP = 180,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Earth Elemental",
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
                SpecialRules = new List<string> { "Magic being" },
                XP = 200,
                TreasureType = TreasureType.None
            },
            new Monster()
            {
                Name = "Fire Elemental",
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
                SpecialRules = new List<string> { "Magic being", "Fire damage", "Extra damage from Water" },
                XP = 250,
                TreasureType = TreasureType.None
            },
            new Monster()
            {
                Name = "Water Elemental",
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
                SpecialRules = new List<string> { "Extra damage from Fire", "Magic being" },
                XP = 150,
                TreasureType = TreasureType.None
            },
            new Monster()
            {
                Name = "Wind Elemental",
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
                SpecialRules = new List<string> { "Gust, Magic being" },
                XP = 150,
                TreasureType = TreasureType.None
            },
            new Monster()
            {
                Name = "Gargoyle",
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
                SpecialRules = new List<string> { "Stupid", "Simple weapons", "Large", "Free Bellow", "Sweeping strike" },
                XP = 550,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Giant",
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
                SpecialRules = new List<string> { "Terror 8", "Large", "Sweeping strike", "Simple weapons" },
                XP = 900,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Giant Centipede",
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
                SpecialRules = new List<string> { "Fear 5" },
                XP = 300,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Frogling",
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
                SpecialRules = new List<string>() { "Poisonous spit", "Silent" },
                XP = 90,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Gecko",
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
                SpecialRules = new List<string>() { "Camouflage", "sneaky" },
                XP = 100,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Ghost",
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
                SpecialRules = new List<string>() { "Ethereal", "Fear 5", "Ghostly touch" },
                XP = 550,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Ghoul",
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
                SpecialRules = new List<string>() { "Fear 3", "Poisonous" },
                XP = 90,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Giant Leech",
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
                SpecialRules = new List<string>() { "Leech", "Slow", "Disease" },
                XP = 90,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Giant Pox Rat",
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
                SpecialRules = new List<string>() { "Disease", "Scurry" },
                XP = 50,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Giant Rat",
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
                SpecialRules = new List<string>() { "Perfect hearing", "Scurry" },
                XP = 40,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Giant Scorpion",
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
                SpecialRules = new List<string>() { "Fear 4", "Poisonous", "Wall crawler" },
                XP = 220,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Giant Snake",
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
                SpecialRules = new List<string>() { "Poisonous", "Fear 3" },
                XP = 120,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Giant Spider",
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
                SpecialRules = new List<string>() { "Fear 5", "Poisonous", "Wall crawler", "Web" },
                XP = 170,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Giant Toad",
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
                SpecialRules = new List<string>() { "Large", "Swallow", "Tongue attack" },
                XP = 400,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Giant Wolf",
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
                SpecialRules = new List<string>() { "Perfect hearing" },
                XP = 80,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Gigantic Snake",
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
                SpecialRules = new List<string>() { "Fear 5", "Poisonous", "Large", "Sweeping strike" },
                XP = 800,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Sand Worm",
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
                SpecialRules = new List<string>() { "Fear 5", "Large", "Sweeping strike" },
                XP = 800,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Gigantic Spider",
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
                SpecialRules = new List<string>() { "Terror 5, Poisonous, Large" },
                XP = 900,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Gnoll",
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
                SpecialRules = new List<string>() { "Magic User" },
                XP = 150,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Goblin",
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
                SpecialRules = new List<string>() { "Fear elves" },
                XP = 70,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Goblin Archer",
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
                SpecialRules = new List<string>() { "Fear elves" },
                XP = 70,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Goblin Shaman",
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
                SpecialRules = new List<string>() { "Magic User" },
                XP = 130,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Greater Demon",
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
                SpecialRules = new List<string>() { "Demon", "Terror 5", "Large" },
                XP = 1200,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Griffon",
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
                SpecialRules = new List<string>() { "Fear 5", "Flyer (0)" },
                XP = 1500,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Harpy",
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
                SpecialRules = new List<string>() { "Flyer (O)" },
                XP = 130,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Hydra",
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
                SpecialRules = new List<string>() { "Multiple attacks Hydra", "X-Large", "Fear 7" },
                XP = 1850,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Lesser Plague Demon",
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
                SpecialRules = new List<string>() { "Demon", "Disease", "Flyer" },
                XP = 50,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Lurker",
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
                SpecialRules = new List<string>() { "Demon", "Magic User", "Floater" },
                XP = 1200,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Medusa",
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
                SpecialRules = new List<string>() { "Petrify" },
                XP = 350,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Mimic",
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
                SpecialRules = new List<string>() { "Leech" },
                XP = 110,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Minotaur",
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
                SpecialRules = new List<string>() { "Ferocious charge", "Large", "Bellow", "Fear 3" },
                XP = 450,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Minotaur Skeleton",
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
                SpecialRules = new List<string>() { "Large", "Just bones", "Fear 3", "Gives bonemeal as part" },
                XP = 350,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Mummy",
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
                SpecialRules = new List<string>() { "Extra damage from Fire", "Fear 5" },
                XP = 300,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Mummy Priest",
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
                SpecialRules = new List<string>() { "Fear 5", "Extra damage from fire", "Magic User" },
                XP = 600,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Mummy Queen",
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
                SpecialRules = new List<string>() { "Fear 5", "Extra damage from fire", "Magic User" },
                XP = 800,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Naga",
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
                SpecialRules = new List<string>() { "Multiple attacks 3" },
                XP = 650,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Necromancer",
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
                SpecialRules = new List<string>() { "Magic User" },
                XP = 180,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Ogre",
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
                SpecialRules = new List<string>() { "Large", "Sweeping strike" },
                XP = 400,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Ogre Berserker",
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
                SpecialRules = new List<string>() { "Large", "Frenzy", "Sweeping strike" },
                XP = 500,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Ogre Chieftain",
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
                SpecialRules = new List<string>() { "Large", "Sweeping strike" },
                XP = 600,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Orc",
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
                SpecialRules = new List<string>() { "Frenzy" },
                XP = 130,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Orc Shaman",
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
                SpecialRules = new List<string>() { "Magic User" },
                XP = 180,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Plague Demon",
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
                SpecialRules = new List<string>() { "Demon", "Disease ridden" },
                XP = 200,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Psyker",
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
                SpecialRules = new List<string>() { "Magic User", "Psychic" },
                XP = 250,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Raptor",
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
                SpecialRules = new List<string>() { "Ferocious charge", "Rend" },
                XP = 130,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "River Troll",
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
                SpecialRules = new List<string>() { "Regeneration", "Stupid", "Stench", "Simple weapons", "Large", "Bellow", "Fear 3" },
                XP = 550,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Salamander",
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
                SpecialRules = new List<string>() { "Fire Breath", "Slow", "Stupid", "Large" },
                XP = 430,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Satyr",
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
                SpecialRules = new List<string>() { "Perfect hearing" },
                XP = 80,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Satyr Archer",
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
                SpecialRules = new List<string>() { "Perfect hearing" },
                XP = 80,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Saurian",
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
                SpecialRules = new List<string>() { "Magic User" },
                XP = 200,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Saurian Warchief",
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
                SpecialRules = new List<string>() { "Large", "Entangle" },
                XP = 450,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Skeleton",
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
                SpecialRules = new List<string>() { "Gives Bone meal as part", "Just bones", "Fear 2" },
                XP = 80,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Skeleton Archer",
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
                SpecialRules = new List<string>() { "Gives Bone meal as part", "Just bones", "Fear 2" },
                XP = 80,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Slime",
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
                SpecialRules = new List<string>() { "Corrosive" },
                XP = 120,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Sphinx",
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
                SpecialRules = new List<string>() { "Flyer(O)", "Large", "Riddle master" },
                XP = 1000,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Stone Golem",
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
                SpecialRules = new List<string>() { "Hard as rock", "Fear 3", "Large" },
                XP = 450,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Stone Troll",
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
                SpecialRules = new List<string>() { "Regenerate", "Stupid", "Simple weapons", "Large", "Bellow", "Fear 3" },
                XP = 550,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Tomb Guardian",
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
                SpecialRules = new List<string>() { "Fear 5", "Large" },
                XP = 550,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Vampire",
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
                SpecialRules = new List<string>() { "Fear 10", "Master of the Dead", "Extra DMG from silver" },
                XP = 2000,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Vampire Fledgling",
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
                SpecialRules = new List<string>() { "Fear 10", "Seduction", "Extra DMG from silver" },
                XP = 1500,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Werewolf",
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
                SpecialRules = new List<string>() { "Ferocious charge", "Regeneration" },
                XP = 280,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Wight",
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
                SpecialRules = new List<string>() { "Fear 5", "Cursed weapons", "Just bones" },
                XP = 180,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Wraiths",
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
                SpecialRules = new List<string>() { "Ethereal", "Cursed weapons", "Fear 5" },
                XP = 500,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Wyvern",
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
                SpecialRules = new List<string>() { "Terror 5, X-Large" },
                XP = 1800,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Zombie",
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
                SpecialRules = new List<string>() { "Slow", "Fear 2" },
                XP = 80,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "Zombie Ogre",
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
                SpecialRules = new List<string>() { "Fear 5", "Simple weapons", "Large, Slow" },
                XP = 450,
                TreasureType = TreasureType.T1
            },
            new Monster()
            {
                Name = "The Brood Mother",
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
                SpecialRules = new List<string>() { "Disease", "Fear 2" },
                XP = 115,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Grop",
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
                SpecialRules = new List<string>() { "Battleaxe", "Shield", "Armour 2" },
                XP = 110,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Ulfric",
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
                SpecialRules = new List<string>() { "Fear 3", "Frenzy" },
                XP = 110,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Imgrahil the Apprentice",
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
                SpecialRules = new List<string>() { "Magic User", "Spells: Raise dead, Healing, Vampiric Touch, Mirrored Self", "Poisonous dagger", "Armour 1" },
                XP = 200,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Ambar the Ettin",
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
                SpecialRules = new List<string>() { "Stupid: Inactive on 1 (1d6). Large: Re-roll DMG. +10 to hit when shot at. Warhammer. Can cause disease just like poison. Free Bellow: All heroes must past RES or be stunned. Whenever you roll this action, the other head may still direct a standard attack during the same action. Sweeping strike: See rules." },
                XP = 550,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Digg",
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
                SpecialRules = new List<string>() { "Battleaxe", "Shield", "Armour 2" },
                XP = 110,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Kraghul the Mighty",
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
                SpecialRules = new List<string>() { "Large", "Bellow", "Greataxe", "Armour 3" },
                XP = 450,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Belua",
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
                SpecialRules = new List<string>() { "Terror 5", "Poisonous", "Large", "Summon children: Summons giant spider on behaviour roll of 5-6." },
                XP = 900,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Briggo",
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
                SpecialRules = new List<string>() { "Large", "Sweeping strike", "Warhammer", "Armour 2" },
                XP = 400,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Gorm",
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
                SpecialRules = new List<string>() { "Large", "Sweeping strike", "Frenzy", "Warhammer", "Armour 2" },
                XP = 500,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Goldfrid the Short",
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
                SpecialRules = new List<string>() { "Special rule: Will always try to move out of close combat", "Shortbow", "Shortsword", "Armour 1" },
                XP = 120,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Madame Isabelle",
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
                SpecialRules = new List<string>() { "Seduction", "Longsword", "Armour 1" },
                XP = 140,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Gaul the Mauler",
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
                SpecialRules = new List<string>() { "Longsword, Shield, Armour 2" },
                XP = 150,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Molgor The Fiend of Summerhall",
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
                SpecialRules = new List<string>() { "Large", "Bellow", "Ferocious charge", "Frenzy", "Fear 3", "Greataxe", "Armour 2" },
                XP = 450,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Turog",
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
                SpecialRules = new List<string>() { "Regenerate", "Frenzy", "Broadsword", "Armour 2", "Fear 7" },
                XP = 300,
                TreasureType = TreasureType.Turog
            },
            new Monster()
            {
                Name = "Ragnalf the Mad",
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
                SpecialRules = new List<string>() { "Magic User", "Knows Raise dead, 2 support, 2 ranged and 3 close combat spells" },
                XP = 800,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Queen Khaba",
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
                SpecialRules = new List<string>() { "Extra damage from fire", "Fear 5", "Magic User", "Dagger", "Armour 0" },
                XP = 800,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "The Mapmaker",
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
                TreasureType = TreasureType.None
            },
            new Monster()
            {
                Name = "Beast of Turog Hall",
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
                SpecialRules = new List<string>() { "Regenerate", "Stupid", "Large", "Warhammer", "Armour 2", "Bellow", "Fear 3" },
                XP = 700,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Undead Wyvern",
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
                SpecialRules = new List<string>() { "Terror 3", "XLarge", "Just bones" },
                XP = 1400,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Dregrir the Wyzard",
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
                SpecialRules = new List<string>() { "Magic User", "Knows 2 ranged, 2 support and 2 close combat spells", "Shortsword", "Armour 2" },
                XP = 150,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Klatche the Ogre",
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
                SpecialRules = new List<string>() { "Special: May make 2 attacks for every attack, even power attacks. Roll to hit for each attack", "2 Dead goblins (battlehammers)" },
                XP = 650,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Easta Rubeet",
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
                SpecialRules = new List<string>() { "Frost DMG", "Armour 2" },
                XP = 190,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "The Master Locksmith",
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
                SpecialRules = new List<string>() { "Extra damage from fire", "Fear 5", "Magic User", "Dagger" },
                XP = 600,
                TreasureType = TreasureType.TheMasterLocksmith
            },
            new Monster()
            {
                Name = "The Guardian Scorpion",
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
                SpecialRules = new List<string>() { "Fear 6", "Poisonous", "Wall crawler" },
                XP = 250,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Queen Khezira",
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
                SpecialRules = new List<string>() { "Fear 7", "Regenerate", "Extra damage from fire" },
                XP = 1000,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Chadepho",
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
                SpecialRules = new List<string>() { "Fear 5", "Wall crawler", "Large", "Greatsword", "Armour 2" },
                XP = 650,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Marla the Witch",
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
                SpecialRules = new List<string>() { "Magic User", "Knows Mute, Seduce and Blind", "Dagger", "Armour 1" },
                XP = 200,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Lord Brenann",
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
                SpecialRules = new List<string>() { "Just bones", "Fear 5", "Greatsword", "Armour 3" },
                XP = 250,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "The Captain",
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
                SpecialRules = new List<string>() { "Longsword", "Shield", "Armour 2" },
                XP = 280,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Novelm Slateshadow",
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
                SpecialRules = new List<string>() { "Just bones", "Fear 5", "Cursed Longsword", "Armour 3" },
                XP = 280,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Adras the Witch",
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
                SpecialRules = new List<string>() { "Magic User", "Knows Healing Hand, Summon Demon, Vampiric Touch and Flare", "Staff", "Armour 2" },
                XP = 260,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Phylax the Witch",
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
                SpecialRules = new List<string>() { "Magic User", "Knows Shield, Mirrored Self and Fireball", "Dagger", "Armour 2" },
                XP = 200,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "The Elder Witch",
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
                SpecialRules = new List<string>() { "Magic User", "Knows Mute, Seduce, Raise Dead and Blind", "Dagger", "Armour 1" },
                XP = 350,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Ghammi the Witch",
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
                SpecialRules = new List<string>() { "Magic User", "Knows Healing hand, Mirrored Self and Fireball", "Dagger", "Armour 2" },
                XP = 200,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Goblin Chieftain",
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
                SpecialRules = new List<string>() { "Poisonous weapon" },
                XP = 130,
                TreasureType = TreasureType.T3
            },
            new Monster()
            {
                Name = "Ribbit",
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
                SpecialRules = new List<string>() { "Throws spiked balls (1d8}, no reload time", "Fear 5", "Shortsword", "Armour 3" },
                XP = 140,
                TreasureType = TreasureType.None
            },
            new Monster()
            {
                Name = "The Alchemist Outlaw",
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
                SpecialRules = new List<string>() { "Special: See rules", "Crossbow Pistol (Poisonous}", "Dagger (Poisonous}", "Armour 1" },
                XP = 140,
                TreasureType = TreasureType.None
            },
            new Monster()
            {
                Name = "Ancient Stone Golem",
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
                SpecialRules = new List<string>() { "Hard as rock", "Cause Fear 3", "Large" },
                XP = 500,
                TreasureType = TreasureType.Part
            },
            new Monster()
            {
                Name = "Trogskegg",
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
                SpecialRules = new List<string>() { "Regenerate", "Large", "Bellow", "Fear 3", "Magical Warhammer (AP2)", "Armour 1" },
                XP = 650,
                TreasureType = TreasureType.T2
            },
            new Monster()
            {
                Name = "Gneeb the Manslayer",
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
                SpecialRules = new List<string>() { "Poisonous weapon", "Frenzy", "Javelin", "Shield", "Armour 4", "Cloak (Armour +1 from behind)" },
                XP = 600,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "Ada the Necromancer",
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
                SpecialRules = new List<string>() { "Fear 5", "Seduction", "Extra DMG from Silver", "Magic User", "Knows Vampiric Touch, Fireball, Heal", "Dagger", "Armour 1" },
                XP = 1500,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "The Apostle",
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
                SpecialRules = new List<string>() { "Fear 10", "Seduction", "Extra DMG from Silver", "Longsword", "Armour 2" },
                XP = 1500,
                TreasureType = TreasureType.T5
            },
            new Monster()
            {
                Name = "The Master",
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
                ToHitPenalty = -10,
                Type = EncounterType.MainQuest,
                Behavior = MonsterBehaviorType.MagicUser,
                SpecialRules = new List<string>() { "Poisonous dagger", "Armour 1", "Spells: Raise dead, 3 close combat, 4 ranged." },
                XP = 200,
                TreasureType = TreasureType.T4
            },
            new Monster()
            {
                Name = "Emil the Caretaker",
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
                SpecialRules = new List<string>() { "Sharpened shovel (greataxe}", "Armour 0" },
                XP = 110,
                TreasureType = TreasureType.T2
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

            // 2. Parse all optional parameters from the dictionary

            // Safely parse the count, defaulting to 1 if not specified.
            int count = parameters.TryGetValue("Count", out var countStr) && int.TryParse(countStr, out var parsedCount)
                ? parsedCount
                : 1;

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
                    // Assumes a service exists to get weapon data by name.
                    Weapon? weapon = EquipmentService.GetWeaponByName(weaponName.Trim());
                    if (weapon != null)
                    {
                        weapons.Add(weapon);
                    }
                }
            }

            // Parse the comma-separated list of spells.
            List<MonsterSpell> spells = new List<MonsterSpell>();
            if (parameters.TryGetValue("Spells", out var spellsStr))
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

            // Get the special rule, if any.
            string? specialRule = parameters.GetValueOrDefault("SpecialRule");

            // 3. Call the existing BuildMonsters method with the parsed data
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