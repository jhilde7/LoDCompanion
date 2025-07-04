
using LoDCompanion.Utilities;
using LoDCompanion.Models.Character; // For Monster
using LoDCompanion.Models.Dungeon;

namespace LoDCompanion.Services.Dungeon
{
    public class EncounterService
    {
        // In a full implementation, you would inject services to provide base monster and weapon definitions,
        // e.g., IMonsterDefinitionService and IWeaponDefinitionService.
        // For the purpose of this single file generation, the 'GetEncounters' method
        // will now accept dictionaries of monster and weapon templates.

        public EncounterService()
        {

        }

        /// <summary>
        /// Generates a list of monsters based on the specified encounter type.
        /// </summary>
        /// <param name="type">The type of encounter (e.g., "Beasts", "Undead", "Bandits").</param>
        /// <param name="monsterTemplates">A dictionary of pre-defined monster templates keyed by name.</param>
        /// <param name="weaponTemplates">A dictionary of pre-defined weapon templates keyed by name.</param>
        /// <param name="currentDungeonEncounterType">Optional: The current dungeon's encounter type, used for recursive calls in certain cases (e.g., R20, R30).</param>
        /// <returns>A list of instantiated Monster objects for the encounter.</returns>
        public List<Monster> GetEncounters(EncounterType type, Dictionary<string, Monster> monsterTemplates, Dictionary<string, MonsterWeapon> weaponTemplates, EncounterType currentDungeonEncounterType = EncounterType.Beasts)
        {
            List<Monster> encounters = new List<Monster>();
            Monster monster; // Used temporarily for individual monster creation with additional properties
            int roll = RandomHelper.GetRandomNumber(1, 100);

            switch (type)
            {
                case EncounterType.Beasts:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantRat"]);
                            break;
                        case 3:
                        case 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["batSwarm"]);
                            break;
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 7:
                        case 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantLeech"]);
                            break;
                        case 9:
                        case 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["satyr"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 0, true);
                            break;
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantSnake"]);
                            break;
                        case 13:
                        case 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1, true);
                            break;
                        case 15:
                        case 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["gnoll"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["gnollArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 17:
                        case 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }));
                            break;
                        case 19:
                        case 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["satyrArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"] }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["beastmanGuard"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true));
                            break;
                        case 21:
                            encounters.Add(BuildMonster(monsterTemplates["caveBear"]));
                            break;
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["gnoll"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["gnollArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 23:
                        case 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["gnoll"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1, true);
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["gnollShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(1, 1, 1)));
                            break;
                        case 25:
                        case 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["harpy"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["slime"]));
                            break;
                        case 27:
                        case 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantSpider"]);
                            break;
                        case 29:
                        case 30:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantLeech"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["gnoll"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 0, true));
                            break;
                        case 31:
                        case 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["satyr"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 1, true));
                            break;
                        case 33:
                        case 34:
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["satyr"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 1, true));
                            break;
                        case 35:
                        case 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["gnoll"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["gnollArcher"], new List<MonsterWeapon>() { weaponTemplates["crossbow"], weaponTemplates["dagger"] }, 2));
                            break;
                        case 37:
                        case 38:
                            encounters.Add(BuildMonster(monsterTemplates["giantCentipede"]));
                            break;
                        case 39:
                        case 40:
                            encounters = BuildMonsters(1, monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 3);
                            break;
                        case 41:
                        case 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["lesserPlagueDemon"]);
                            break;
                        case 43:
                        case 44:
                            encounters = BuildMonsters(1, monsterTemplates["riverTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 2);
                            break;
                        case 45:
                        case 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["satyr"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["flail"] }, 3));
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 47:
                        case 48:
                            encounters.Add(BuildMonster(monsterTemplates["slime"]));
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2));
                            break;
                        case 49:
                        case 50:
                            encounters = BuildMonsters(1, monsterTemplates["ettin"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 2);
                            break;
                        case 51:
                        case 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["wereWolf"]);
                            break;
                        case 53:
                        case 54:
                            encounters = BuildMonsters(1, monsterTemplates["stoneTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 2);
                            encounters.Add(BuildMonster(monsterTemplates["giantCentipede"]));
                            break;
                        case 55:
                        case 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["gargoyle"]);
                            break;
                        case 57:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["gnollArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 58:
                            encounters.Add(BuildMonster(monsterTemplates["griffon"]));
                            break;
                        case 59:
                        case 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["satyr"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true));
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["gnollShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(1, 1, 1)));
                            break;
                        case 61:
                        case 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true));
                            break;
                        case 63:
                        case 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["bloodDemon"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 0, false, null, "Cursed weapon");
                            break;
                        case 65:
                        case 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 1, true);
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1));
                            break;
                        case 67:
                        case 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["ettin"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1));
                            break;
                        case 69:
                        case 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 71:
                        case 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]);
                            break;
                        case 73:
                        case 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["plagueDemon"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 0, false, null, "Cursed weapon");
                            break;
                        case 75:
                        case 76:
                            encounters = BuildMonsters(2, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            break;
                        case 77:
                        case 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["gnoll"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["gnollSergeant"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2));
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2));
                            break;
                        case 79:
                        case 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["gnoll"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1, true);
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["gnollSergeant"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2));
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["gnollShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case 81:
                        case 82:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["bloodDemon"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 0, false, null, "Cursed weapon");
                            break;
                        case 83:
                        case 84:
                            encounters.Add(BuildMonster(monsterTemplates["bloatedDemon"]));
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["shambler"]));
                            break;
                        case 85:
                        case 86:
                            encounters = BuildMonsters(1, monsterTemplates["bloatedDemon"]);
                            encounters.AddRange(BuildMonsters(3, monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2));
                            break;
                        case 87:
                            encounters.Add(BuildMonster(monsterTemplates["lurker"]));
                            break;
                        case 88:
                            encounters = BuildMonsters(1, monsterTemplates["lurker"], null, 0, false, BuildSpellList(3, 2, 2));
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1));
                            break;
                        case 89:
                        case 90:
                            encounters.Add(BuildMonster(monsterTemplates["giantSpider"]));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantLeech"]));
                            break;
                        case 91:
                        case 92:
                            encounters.Add(BuildMonster(monsterTemplates["wyvern"]));
                            break;
                        case 93:
                        case 94:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, monsterTemplates["beastman"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2));
                            break;
                        case 95:
                        case 96:
                            encounters.Add(BuildMonster(monsterTemplates["giganticSpider"]));
                            break;
                        case 97:
                        case 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantLeech"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6) + 2, monsterTemplates["bloodDemon"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 0, false, null, "Cursed weapon"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["lesserPlagueDemon"]));
                            break;
                        case 99:
                        case 100:
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["greaterDemon"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 3));
                            break;
                        default:
                            break;
                    }
                    break;
                case EncounterType.Undead:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantRat"]);
                            break;
                        case 3:
                        case 4:
                            encounters = BuildMonsters(1, monsterTemplates["batSwarm"]);
                            break;
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 7:
                        case 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["batSwarm"]);
                            break;
                        case 9:
                        case 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["zombie"]);
                            break;
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantSnake"]);
                            break;
                        case 13:
                        case 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["zombie"]);
                            break;
                        case 15:
                        case 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 1, true);
                            break;
                        case 17:
                        case 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["skeletonArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 19:
                        case 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2);
                            break;
                        case 21:
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["zombie"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            break;
                        case 23:
                        case 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["ghoul"]);
                            break;
                        case 25:
                        case 26:
                            encounters = BuildMonsters(1, monsterTemplates["shambler"]);
                            break;
                        case 27:
                        case 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            monster = BuildMonster(monsterTemplates["necromancer"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(1, 1, 0));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 29:
                        case 30:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["zombie"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 2, true);
                            break;
                        case 31:
                        case 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3);
                            break;
                        case 33:
                        case 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["direWolf"]));
                            break;
                        case 35:
                        case 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantSpider"]);
                            break;
                        case 37:
                        case 38:
                            encounters = BuildMonsters(1, monsterTemplates["shambler"]);
                            break;
                        case 39:
                        case 40:
                            encounters = BuildMonsters(1, monsterTemplates["mummy"], null, 1);
                            break;
                        case 41:
                        case 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), monsterTemplates["ghoul"]);
                            break;
                        case 43:
                        case 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["skeletonArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 45:
                        case 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["direWolf"]);
                            break;
                        case 47:
                        case 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["direWolf"]);
                            monster = BuildMonster(monsterTemplates["necromancer"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(1, 2, 0));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 49:
                        case 50:
                            encounters = BuildMonsters(1, monsterTemplates["zombieOgre"], new List<MonsterWeapon> { weaponTemplates["warhammer"] });
                            break;
                        case 51:
                        case 52:
                            encounters = BuildMonsters(2, monsterTemplates["mummy"], null, 1);
                            break;
                        case 53:
                        case 54:
                            encounters = BuildMonsters(1, monsterTemplates["mummy"], null, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["zombie"]));
                            break;
                        case 55:
                        case 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["skeletonArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }));
                            break;
                        case 57:
                        case 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 3);
                            break;
                        case 59:
                        case 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantSpider"]);
                            break;
                        case 61:
                        case 62:
                            encounters = BuildMonsters(1, monsterTemplates["zombieOgre"], new List<MonsterWeapon> { weaponTemplates["warhammer"] }, 1);
                            break;
                        case 63:
                        case 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["zombie"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            monster = BuildMonster(monsterTemplates["necromancer"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 65:
                        case 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["minotaurSkeleton"], new List<MonsterWeapon> { weaponTemplates["greataxe"] });
                            break;
                        case 67:
                        case 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["ghoul"]);
                            break;
                        case 69:
                        case 70:
                            encounters = BuildMonsters(2, monsterTemplates["zombieOgre"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 2);
                            monster = BuildMonster(monsterTemplates["necromancer"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 71:
                        case 72:
                            encounters = BuildMonsters(1, monsterTemplates["zombieOgre"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            break;
                        case 73:
                        case 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 3);
                            break;
                        case 75:
                        case 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["flail"] }, 3);
                            encounters.AddRange(BuildMonsters(4, monsterTemplates["skeletonArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 2));
                            break;
                        case 77:
                        case 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["minotaurSkeleton"], new List<MonsterWeapon> { weaponTemplates["greataxe"] }, 1);
                            break;
                        case 79:
                        case 80:
                            encounters = BuildMonsters(1, monsterTemplates["vampireFledgling"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 7), monsterTemplates["zombie"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 81:
                        case 82:
                            encounters = BuildMonsters(1, monsterTemplates["ghost"]);
                            break;
                        case 83:
                        case 84:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 10), monsterTemplates["zombie"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            break;
                        case 85:
                        case 86:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 6), monsterTemplates["zombie"], null, 2);
                            break;
                        case 87:
                        case 88:
                            encounters = BuildMonsters(1, monsterTemplates["vampireFledgling"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["ghoul"]));
                            break;
                        case 89:
                        case 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["wraith"]);
                            break;
                        case 91:
                        case 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 3);
                            break;
                        case 93:
                        case 94:
                            encounters = BuildMonsters(2, monsterTemplates["ghost"]);
                            break;
                        case 95:
                        case 96:
                            encounters = BuildMonsters(1, monsterTemplates["banshee"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["zombie"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 97:
                        case 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["wraith"]);
                            monster = BuildMonster(monsterTemplates["necromancer"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 99:
                        case 100:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["wraith"]));
                            encounters.AddRange(BuildMonsters(1, monsterTemplates["vampire"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2));
                            break;
                    }
                    break;
                case EncounterType.Bandits:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantRat"]);
                            break;
                        case 3:
                        case 4:
                            encounters = BuildMonsters(1, monsterTemplates["batSwarm"]);
                            break;
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 7:
                        case 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["batSwarm"]);
                            break;
                        case 9:
                        case 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] });
                            break;
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantSnake"]);
                            break;
                        case 13:
                        case 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 15:
                        case 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["shortsword"] }, 1));
                            break;
                        case 17:
                        case 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantWolf"]));
                            break;
                        case 19:
                        case 20:
                            encounters = BuildMonsters(2, monsterTemplates["berserker"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] });
                            break;
                        case 21:
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 23:
                        case 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantWolf"]);
                            break;
                        case 25:
                        case 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantPoxRat"]);
                            encounters.Add(BuildMonster(monsterTemplates["slime"]));
                            break;
                        case 27:
                        case 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["banditLeader"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 3));
                            break;
                        case 29:
                        case 30:
                            encounters = BuildMonsters(1, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            break;
                        case 31:
                        case 32:
                            encounters = BuildMonsters(2, monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            monster = BuildMonster(monsterTemplates["warlock"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(1, 2, 0));
                            monster.Spells.Add("Summon Demon");
                            encounters.Add(monster);
                            break;
                        case 33:
                        case 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 35:
                        case 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["banditLeader"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true));
                            break;
                        case 37:
                        case 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["berserker"], new List<MonsterWeapon>() { weaponTemplates["flail"] });
                            encounters.Add(BuildMonster(monsterTemplates["fallenKnight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true));
                            encounters.Add(BuildMonster(monsterTemplates["fallenKnight"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 4, true));
                            break;
                        case 39:
                        case 40:
                            encounters = BuildMonsters(2, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 41:
                        case 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["crossbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 43:
                        case 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.Add(BuildMonster(monsterTemplates["banditLeader"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            encounters.Add(BuildMonster(monsterTemplates["banditLeader"], new List<MonsterWeapon>() { weaponTemplates["flail"] }, 2));
                            break;
                        case 45:
                        case 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 0, true);
                            monster = BuildMonster(monsterTemplates["warlock"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add("Summon Demon");
                            encounters.Add(monster);
                            break;
                        case 47:
                        case 48:
                            encounters = BuildMonsters(1, monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["flail"] }, 1);
                            break;
                        case 49:
                        case 50:
                            encounters = BuildMonsters(2, monsterTemplates["shambler"]);
                            break;
                        case 51:
                        case 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["fallenKnight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true);
                            break;
                        case 53:
                        case 54:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["berserker"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1);
                            break;
                        case 55:
                        case 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["berserker"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            monster = BuildMonster(monsterTemplates["warlock"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add("Summon Demon");
                            encounters.Add(monster);
                            break;
                        case 57:
                        case 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            encounters.Add(BuildMonster(monsterTemplates["ogreChieftain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 59:
                        case 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 2);
                            break;
                        case 61:
                        case 62:
                            encounters = BuildMonsters(2, monsterTemplates["giantCentipede"]);
                            break;
                        case 63:
                        case 64:
                            encounters = BuildMonsters(2, monsterTemplates["shambler"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["crossbow"], weaponTemplates["dagger"] }, 3));
                            break;
                        case 65:
                        case 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["berserker"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2);
                            encounters.Add(BuildMonster(monsterTemplates["banditLeader"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true));
                            break;
                        case 67:
                        case 68:
                            encounters = BuildMonsters(2, monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 69:
                        case 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]);
                            break;
                        case 71:
                        case 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["fallenKnight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true, null, "Cursed weapon");
                            monster = BuildMonster(monsterTemplates["warlock"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add("Summon Greater Demon");
                            encounters.Add(monster);
                            break;
                        case 73:
                        case 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["fallenKnight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true));
                            break;
                        case 75:
                        case 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            break;
                        case 77:
                        case 78:
                            encounters = BuildMonsters(2, monsterTemplates["shambler"]);
                            monster = BuildMonster(monsterTemplates["warlock"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 2, 1));
                            monster.Spells.Add("Summon Demon");
                            encounters.Add(monster);
                            break;
                        case 79:
                        case 80:
                            encounters = BuildMonsters(3, monsterTemplates["banditLeader"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(4, monsterTemplates["banditArcher"], new List<MonsterWeapon>() { weaponTemplates["crossbow"], weaponTemplates["dagger"] }, 2));
                            break;
                        case 81:
                        case 82:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 83:
                        case 84:
                            encounters = BuildMonsters(4, monsterTemplates["centaur"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 3, true);
                            break;
                        case 85:
                        case 86:
                            encounters = BuildMonsters(1, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            monster = BuildMonster(monsterTemplates["warlock"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add("Summon Greater Demon");
                            encounters.Add(monster);
                            break;
                        case 87:
                        case 88:
                            encounters = BuildMonsters(3, monsterTemplates["ogreChieftain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            break;
                        case 89:
                        case 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 91:
                        case 92:
                            encounters = BuildMonsters(2, monsterTemplates["centaurArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["shortsword"] }, 1);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 93:
                        case 94:
                            encounters = BuildMonsters(3, monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 3);
                            break;
                        case 95:
                        case 96:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["fallenKnight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true, null, "Cursed weapon");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["bandit"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["shortsword"] }, 2));
                            break;
                        case 97:
                        case 98:
                            encounters.Add(BuildMonster(monsterTemplates["ogreChieftain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true));
                            encounters.Add(BuildMonster(monsterTemplates["giganticSpider"]));
                            break;
                        case 99:
                        case 100:
                            encounters.Add(BuildMonster(monsterTemplates["wyvern"]));
                            break;
                    }
                    break;
                case EncounterType.Orcs:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantRat"]);
                            break;
                        case 3:
                        case 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["batSwarm"]);
                            break;
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 7:
                        case 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["dagger"] });
                            break;
                        case 9:
                        case 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] });
                            break;
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantSnake"]);
                            break;
                        case 13:
                        case 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1, true);
                            break;
                        case 15:
                        case 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["caveGoblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 17:
                        case 18:
                            encounters = BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]));
                            break;
                        case 19:
                        case 20:
                            encounters = BuildMonsters(1, monsterTemplates["orcChieftain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 21:
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 2, true));
                            encounters.Add(BuildMonster(monsterTemplates["goblinShaman"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case 23:
                        case 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantWolf"]);
                            break;
                        case 25:
                        case 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantPoxRat"]);
                            break;
                        case 27:
                        case 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantSpider"]);
                            break;
                        case 29:
                        case 30:
                            encounters = BuildMonsters(1, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            break;
                        case 31:
                        case 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["orcBrute"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true);
                            break;
                        case 33:
                        case 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1));
                            encounters.Add(BuildMonster(monsterTemplates["orcChieftain"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 3, true));
                            break;
                        case 35:
                        case 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            encounters.Add(BuildMonster(monsterTemplates["orcShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 1, false, BuildSpellList(1, 1, 1)));
                            break;
                        case 37:
                        case 38:
                            encounters = BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 39:
                        case 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantSpider"]);
                            break;
                        case 41:
                        case 42:
                            encounters = BuildMonsters(2, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 43:
                        case 44:
                            encounters = BuildMonsters(1, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            break;
                        case 45:
                        case 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.Add(BuildMonster(monsterTemplates["orcChieftain"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 2, true));
                            encounters.Add(BuildMonster(monsterTemplates["orcChieftain"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2));
                            break;
                        case 47:
                        case 48:
                            encounters = BuildMonsters(1, monsterTemplates["ettin"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            break;
                        case 49:
                        case 50:
                            encounters = BuildMonsters(1, monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["longsword"] });
                            break;
                        case 51:
                        case 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["caveGoblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 53:
                        case 54:
                            encounters = BuildMonsters(1, monsterTemplates["stoneTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 2);
                            break;
                        case 55:
                        case 56:
                            encounters = BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 57:
                        case 58:
                            encounters = BuildMonsters(1, monsterTemplates["giantCentipede"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["orcBrute"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 1));
                            encounters.Add(BuildMonster(monsterTemplates["orcShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 1, 1)));
                            break;
                        case 59:
                        case 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]));
                            encounters.Add(BuildMonster(monsterTemplates["ogreChieftain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true));
                            break;
                        case 61:
                        case 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["flail"] }, 2);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 63:
                        case 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 2, true);
                            break;
                        case 65:
                        case 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orcBrute"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 2));
                            break;
                        case 67:
                        case 68:
                            encounters = BuildMonsters(1, monsterTemplates["riverTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 2));
                            break;
                        case 69:
                        case 70:
                            encounters = BuildMonsters(2, monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] });
                            break;
                        case 71:
                        case 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]);
                            break;
                        case 73:
                        case 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["orcBrute"], new List<MonsterWeapon> { weaponTemplates["longsword"] }, 3, true);
                            encounters.Add(BuildMonster(monsterTemplates["orcShaman"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 0, false, BuildSpellList(3, 1, 1)));
                            break;
                        case 75:
                        case 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["ettin"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }));
                            break;
                        case 77:
                        case 78:
                            encounters = BuildMonsters(2, monsterTemplates["shambler"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            break;
                        case 79:
                        case 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["goblinShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case 81:
                        case 82:
                            encounters = BuildMonsters(3, monsterTemplates["orcChieftain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(4, monsterTemplates["caveGoblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 83:
                        case 84:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 85:
                        case 86:
                            encounters = BuildMonsters(4, monsterTemplates["centaur"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 2, true);
                            break;
                        case 87:
                        case 88:
                            encounters = BuildMonsters(1, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["orcShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 1, false, BuildSpellList(2, 2, 2)));
                            break;
                        case 89:
                        case 90:
                            encounters = BuildMonsters(2, monsterTemplates["ogreChieftain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 91:
                        case 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["ogreBerserker"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 93:
                        case 94:
                            encounters = BuildMonsters(4, monsterTemplates["giantSpider"]);
                            encounters.Add(BuildMonster(monsterTemplates["lurker"], null, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case 95:
                        case 96:
                            encounters = BuildMonsters(2, monsterTemplates["giganticSpider"]);
                            break;
                        case 97:
                        case 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orcBrute"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["shortsword"] }, 2));
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 0));
                            break;
                        case 99:
                        case 100:
                            encounters = BuildMonsters(1, monsterTemplates["orcChieftain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true);
                            encounters.Add(BuildMonster(monsterTemplates["wyvern"]));
                            break;
                    }
                    break;
                case EncounterType.Reptiles:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantRat"]);
                            break;
                        case 3:
                        case 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["batSwarm"]);
                            break;
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 7:
                        case 8:
                            encounters = BuildMonsters(1, monsterTemplates["slime"]);
                            break;
                        case 9:
                        case 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantSnake"]);
                            break;
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["gecko"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] });
                            break;
                        case 13:
                        case 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["gecko"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["gecko"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, true));
                            break;
                        case 15:
                        case 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["geckoAssassin"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["frogling"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, true));
                            break;
                        case 17:
                        case 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1, true);
                            break;
                        case 19:
                        case 20:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["geckoAssassin"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 21:
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["gecko"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1, true));
                            break;
                        case 23:
                        case 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["frogling"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 0, true);
                            encounters.Add(BuildMonster(monsterTemplates["saurianPriest"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case 25:
                        case 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 1, true);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 27:
                        case 28:
                            encounters = BuildMonsters(1, monsterTemplates["giantToad"]);
                            break;
                        case 29:
                        case 30:
                            encounters = BuildMonsters(1, monsterTemplates["giantCentipede"]);
                            break;
                        case 31:
                        case 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true);
                            break;
                        case 33:
                        case 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 1, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["gecko"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1));
                            encounters.Add(BuildMonster(monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true));
                            break;
                        case 35:
                        case 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["raptor"]));
                            break;
                        case 37:
                        case 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["raptor"]);
                            break;
                        case 39:
                        case 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), monsterTemplates["giantSpider"]);
                            break;
                        case 41:
                        case 42:
                            encounters = BuildMonsters(2, monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 2);
                            break;
                        case 43:
                        case 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["giantToad"]));
                            break;
                        case 45:
                        case 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantToad"]));
                            break;
                        case 47:
                        case 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2);
                            break;
                        case 49:
                        case 50:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["flail"] }, 3);
                            encounters.Add(BuildMonster(monsterTemplates["saurianPriest"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 1, false, BuildSpellList(1, 1, 1)));
                            break;
                        case 51:
                        case 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["shambler"]));
                            break;
                        case 53:
                        case 54:
                            encounters = BuildMonsters(1, monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 2);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["slime"]));
                            break;
                        case 55:
                        case 56:
                            encounters = BuildMonsters(2, monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 2);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["gecko"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1));
                            break;
                        case 57:
                        case 58:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["saurianPriest"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 1, false, BuildSpellList(2, 2, 1)));
                            break;
                        case 59:
                        case 60:
                            encounters = BuildMonsters(1, monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 3);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["gecko"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }, 1));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["raptor"]));
                            break;
                        case 61:
                        case 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantSnake"]);
                            break;
                        case 63:
                        case 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            break;
                        case 65:
                        case 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["geckoArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 2));
                            break;
                        case 67:
                        case 68:
                            encounters = BuildMonsters(1, monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["frogling"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["geckoAssassin"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 69:
                        case 70:
                            encounters = BuildMonsters(2, monsterTemplates["giantToad"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["slime"]));
                            break;
                        case 71:
                        case 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSnake"]);
                            break;
                        case 73:
                        case 74:
                            encounters = BuildMonsters(1, monsterTemplates["salamander"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true));
                            encounters.Add(BuildMonster(monsterTemplates["saurianPriest"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case 75:
                        case 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantToad"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["salamander"]));
                            break;
                        case 77:
                        case 78:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 2);
                            break;
                        case 79:
                        case 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["saurian"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["saurianPriest"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case 81:
                        case 82:
                            encounters = BuildMonsters(3, monsterTemplates["saurianWarchief"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 2, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["shambler"]));
                            break;
                        case 83:
                        case 84:
                            encounters = BuildMonsters(1, monsterTemplates["giganticSnake"]);
                            break;
                        case 85:
                        case 86:
                            encounters = BuildMonsters(1, monsterTemplates["basilisk"]);
                            break;
                        case 87:
                        case 88:
                            encounters = BuildMonsters(1, monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["geckoAssassin"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 1));
                            encounters.Add(BuildMonster(monsterTemplates["saurianPriest"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 1, false, BuildSpellList(4, 2, 2)));
                            break;
                        case 89:
                        case 90:
                            encounters = BuildMonsters(1, monsterTemplates["saurianWarchief"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.Add(BuildMonster(monsterTemplates["basilisk"]));
                            break;
                        case 91:
                        case 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["naga"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 2);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["giantToad"]));
                            encounters.Add(BuildMonster(monsterTemplates["basilisk"]));
                            break;
                        case 93:
                        case 94:
                            encounters = BuildMonsters(3, monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 3);
                            break;
                        case 95:
                        case 96:
                            encounters = BuildMonsters(1, monsterTemplates["giganticSnake"]);
                            break;
                        case 97:
                        case 98:
                            encounters = BuildMonsters(2, monsterTemplates["frogling"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["geckoArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["shortsword"] }, 2));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["saurianElite"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1, true));
                            break;
                        case 99:
                        case 100:
                            encounters = BuildMonsters(1, monsterTemplates["hydra"]);
                            break;
                    }
                    break;
                case EncounterType.DarkElves:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantRat"]);
                            break;
                        case 3:
                        case 4:
                            encounters = BuildMonsters(1, monsterTemplates["batSwarm"]);
                            break;
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 7:
                        case 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["batSwarm"]);
                            break;
                        case 9:
                        case 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] });
                            break;
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantSnake"]);
                            break;
                        case 13:
                        case 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["darkElfArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 1));
                            break;
                        case 15:
                        case 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["darkElfAssassin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 1, false, null, "Poisonous 1"));
                            break;
                        case 17:
                        case 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantWolf"]));
                            break;
                        case 19:
                        case 20:
                            encounters = BuildMonsters(1, monsterTemplates["darkElfCaptain"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 21:
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 4), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["darkElfSniper"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 23:
                        case 24:
                            encounters = BuildMonsters(2, monsterTemplates["giantCentipede"]);
                            break;
                        case 25:
                        case 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantPoxRat"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["harpy"]));
                            break;
                        case 27:
                        case 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["giantSpider"]);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 29:
                        case 30:
                            encounters = BuildMonsters(1, monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            break;
                        case 31:
                        case 32:
                            encounters = BuildMonsters(2, monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["darkElfWarlock"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(1, 1, 0)));
                            break;
                        case 33:
                        case 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["darkElfSniper"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 35:
                        case 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["darkElfCaptain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true));
                            break;
                        case 37:
                        case 38:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["bloodDemon"], new List<MonsterWeapon>() { weaponTemplates["flail"] });
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["harpy"]));
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 39:
                        case 40:
                            encounters = BuildMonsters(2, monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 41:
                        case 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["darkElfAssassin"], new List<MonsterWeapon>() { weaponTemplates["crossbow"], weaponTemplates["dagger"] }, 1, false, null, "Poisonous 1"));
                            break;
                        case 43:
                        case 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.Add(BuildMonster(monsterTemplates["darkElfCaptain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true));
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 45:
                        case 46:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 0, true);
                            encounters.Add(BuildMonster(monsterTemplates["darkElfWarlock"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, false, BuildSpellList(2, 1, 1)));
                            break;
                        case 47:
                        case 48:
                            encounters = BuildMonsters(2, monsterTemplates["slime"]);
                            break;
                        case 49:
                        case 50:
                            encounters = BuildMonsters(2, monsterTemplates["shambler"]);
                            break;
                        case 51:
                        case 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["gargoyle"]);
                            break;
                        case 53:
                        case 54:
                            encounters = BuildMonsters(2, monsterTemplates["shambler"]);
                            break;
                        case 55:
                        case 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["plagueDemon"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["psyker"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 0, false, BuildSpellList(2, 2, 1)));
                            break;
                        case 57:
                        case 58:
                            encounters = BuildMonsters(1, monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["darkElfAssassin"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 1, false, null, "Poisonous 1"));
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]));
                            break;
                        case 59:
                        case 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 2);
                            break;
                        case 61:
                        case 62:
                            encounters = BuildMonsters(2, monsterTemplates["giantCentipede"]);
                            break;
                        case 63:
                        case 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["darkElfAssassin"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }, 3);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["shambler"]));
                            break;
                        case 65:
                        case 66:
                            encounters = BuildMonsters(1, monsterTemplates["darkElfCaptain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true);
                            break;
                        case 67:
                        case 68:
                            encounters = BuildMonsters(2, monsterTemplates["bloodDemon"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] });
                            break;
                        case 69:
                        case 70:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]);
                            break;
                        case 71:
                        case 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["bloodDemon"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true, null, "Cursed weapon");
                            encounters.Add(BuildMonster(monsterTemplates["darkElfWarlock"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case 73:
                        case 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]));
                            break;
                        case 75:
                        case 76:
                            encounters = BuildMonsters(2, monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            break;
                        case 77:
                        case 78:
                            encounters = BuildMonsters(2, monsterTemplates["shambler"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["psyker"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(3, 2, 1)));
                            break;
                        case 79:
                        case 80:
                            encounters = BuildMonsters(2, monsterTemplates["darkElfCaptain"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(4, monsterTemplates["darkElfSniper"], new List<MonsterWeapon>() { weaponTemplates["crossbow"], weaponTemplates["dagger"] }, 2));
                            break;
                        case 81:
                        case 82:
                            encounters = BuildMonsters(2, monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2);
                            break;
                        case 83:
                        case 84:
                            encounters = BuildMonsters(1, monsterTemplates["medusa"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] }, 0, true);
                            break;
                        case 85:
                        case 86:
                            encounters = BuildMonsters(1, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["psyker"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(3, 2, 2)));
                            break;
                        case 87:
                        case 88:
                            encounters = BuildMonsters(1, monsterTemplates["basilisk"]);
                            break;
                        case 89:
                        case 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["darkElf"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 1, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 2));
                            break;
                        case 91:
                        case 92:
                            encounters = BuildMonsters(1, monsterTemplates["medusa"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            break;
                        case 93:
                        case 94:
                            encounters = BuildMonsters(3, monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 3);
                            break;
                        case 95:
                        case 96:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["bloodDemon"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 4, true, null, "Cursed weapon");
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["darkElfAssassin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 2, false, null, "Poisonous 1"));
                            break;
                        case 97:
                        case 98:
                            encounters.Add(BuildMonster(monsterTemplates["dryder"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true));
                            encounters.Add(BuildMonster(monsterTemplates["giganticSpider"]));
                            break;
                        case 99:
                        case 100:
                            encounters.Add(BuildMonster(monsterTemplates["hydra"]));
                            break;
                    }
                    break;
                case EncounterType.AncientLands:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 3:
                        case 4:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["batSwarm"]);
                            break;
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantPoxRat"]);
                            break;
                        case 7:
                        case 8:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["slime"]);
                            break;
                        case 9:
                        case 10:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 1, true);
                            break;
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantLeech"]);
                            break;
                        case 13:
                        case 14:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 0, true);
                            break;
                        case 15:
                        case 16:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1, true);
                            encounters.Add(BuildMonster(monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 1));
                            break;
                        case 17:
                        case 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2));
                            break;
                        case 19:
                        case 20:
                            encounters = BuildMonsters(1, monsterTemplates["giantScorpion"]);
                            break;
                        case 21:
                        case 22:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["broadsword"] });
                            break;
                        case 23:
                        case 24:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["gargoyle"]);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 25:
                        case 26:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantSpider"]);
                            break;
                        case 27:
                        case 28:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            monster = BuildMonster(monsterTemplates["necromancer"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 29:
                        case 30:
                            encounters = BuildMonsters(2, monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 1);
                            break;
                        case 31:
                        case 32:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantScorpion"]);
                            break;
                        case 33:
                        case 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["skeleton"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["gargoyle"]));
                            break;
                        case 35:
                        case 36:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantSpider"]);
                            break;
                        case 37:
                        case 38:
                            encounters = BuildMonsters(1, monsterTemplates["sphinx"]);
                            break;
                        case 39:
                        case 40:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1, true);
                            break;
                        case 41:
                        case 42:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantSnake"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["slime"]));
                            break;
                        case 43:
                        case 44:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["skeletonArcher"], new List<MonsterWeapon>() { weaponTemplates["longbow"], weaponTemplates["dagger"] }, 1));
                            break;
                        case 45:
                        case 46:
                            encounters = BuildMonsters(1, monsterTemplates["giantCentipede"]);
                            break;
                        case 47:
                        case 48:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 2);
                            monster = BuildMonster(monsterTemplates["necromancer"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(2, 2, 0));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 49:
                        case 50:
                            encounters = BuildMonsters(1, monsterTemplates["zombieOgre"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            break;
                        case 51:
                        case 52:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["battlehammer"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["shambler"]));
                            break;
                        case 53:
                        case 54:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 2));
                            break;
                        case 55:
                        case 56:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["sphinx"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["gargoyle"]));
                            break;
                        case 57:
                        case 58:
                            encounters = BuildMonsters(1, monsterTemplates["banshee"]);
                            break;
                        case 59:
                        case 60:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantSpider"]);
                            break;
                        case 61:
                        case 62:
                            encounters = BuildMonsters(1, monsterTemplates["zombieOgre"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            break;
                        case 63:
                        case 64:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            monster = BuildMonster(monsterTemplates["mummyPriest"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 0, false, BuildSpellList(3, 2, 1));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 65:
                        case 66:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["slime"]));
                            break;
                        case 67:
                        case 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantScorpion"]);
                            break;
                        case 69:
                        case 70:
                            encounters = BuildMonsters(2, monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            encounters.Add(BuildMonster(monsterTemplates["giantScorpion"]));
                            break;
                        case 71:
                        case 72:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            break;
                        case 73:
                        case 74:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 3);
                            break;
                        case 75:
                        case 76:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 5), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true);
                            encounters.AddRange(BuildMonsters(4, monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["greatsword"] }, 2));
                            break;
                        case 77:
                            encounters = BuildMonsters(4, monsterTemplates["mummy"], null, 1);
                            break;
                        case 78:
                            encounters = BuildMonsters(4, monsterTemplates["gargoyle"]);
                            break;
                        case 79:
                        case 80:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(2, 7), monsterTemplates["mummy"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            monster = BuildMonster(monsterTemplates["mummyQueen"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, false, BuildSpellList(3, 2, 2));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 81:
                        case 82:
                            encounters = BuildMonsters(1, monsterTemplates["ghost"]);
                            break;
                        case 83:
                        case 84:
                            encounters = BuildMonsters(1, monsterTemplates["giganticSpider"]);
                            break;
                        case 85:
                        case 86:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2);
                            break;
                        case 87:
                        case 88:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 2);
                            monster = BuildMonster(monsterTemplates["mummyQueen"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, false, BuildSpellList(5, 2, 2));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                        case 89:
                        case 90:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["wraith"]);
                            break;
                        case 91:
                        case 92:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["halberd"] }, 3);
                            break;
                        case 93:
                        case 94:
                            encounters = BuildMonsters(2, monsterTemplates["ghost"]);
                            break;
                        case 95:
                        case 96:
                            encounters = BuildMonsters(1, monsterTemplates["banshee"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantScorpion"]));
                            break;
                        case 97:
                        case 98:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["wraith"]);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["sphinx"]));
                            break;
                        case 99:
                        case 100:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["wight"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 3, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["wraith"]));
                            monster = BuildMonster(monsterTemplates["mummyQueen"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, false, BuildSpellList(3, 3, 2));
                            monster.Spells.Add("Raise Dead");
                            encounters.Add(monster);
                            break;
                    }
                    break;
                case EncounterType.GoblinKing:
                    switch (roll)
                    {
                        case 1:
                        case 2:
                        case 3:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantRat"]);
                            break;
                        case 4:
                        case 5:
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["batSwarm"]);
                            break;
                        case 7:
                        case 8:
                        case 9:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]);
                            break;
                        case 10:
                        case 11:
                        case 12:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["dagger"] });
                            break;
                        case 13:
                        case 14:
                        case 15:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] });
                            break;
                        case 16:
                        case 17:
                        case 18:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantSnake"]);
                            break;
                        case 19:
                        case 20:
                        case 21:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["caveGoblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 22:
                        case 23:
                        case 24:
                            encounters = BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]));
                            break;
                        case 25:
                        case 26:
                        case 27:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["orc"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["javelin"] }, 1, true));
                            encounters.Add(BuildMonster(monsterTemplates["goblinShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 1, false, BuildSpellList(1, 1, 0)));
                            break;
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantWolf"]);
                            break;
                        case 32:
                        case 33:
                        case 34:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantPoxRat"]);
                            break;
                        case 35:
                        case 36:
                        case 37:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantSpider"]);
                            break;
                        case 38:
                        case 39:
                        case 40:
                            encounters = BuildMonsters(1, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 1);
                            break;
                        case 41:
                        case 42:
                        case 43:
                            encounters = BuildMonsters(1, monsterTemplates["ogre"], new List<MonsterWeapon>() { weaponTemplates["longsword"] }, 2, true);
                            encounters.AddRange(BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }));
                            break;
                        case 44:
                        case 45:
                        case 46:
                            encounters = BuildMonsters(1, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 1);
                            break;
                        case 47:
                        case 48:
                        case 49:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantWolf"]);
                            encounters.Add(BuildMonster(monsterTemplates["goblinChieftain"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 2, true));
                            encounters.Add(BuildMonster(monsterTemplates["goblinChieftain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2));
                            break;
                        case 50:
                        case 51:
                        case 52:
                            encounters = BuildMonsters(1, monsterTemplates["riverTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 53:
                        case 54:
                        case 55:
                        case 56:
                            encounters = BuildMonsters(2, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]));
                            encounters.Add(BuildMonster(monsterTemplates["stoneTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }));
                            break;
                        case 57:
                        case 58:
                        case 59:
                            encounters = BuildMonsters(2, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            break;
                        case 60:
                        case 61:
                        case 62:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            encounters.Add(BuildMonster(monsterTemplates["goblinChieftain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 2));
                            break;
                        case 63:
                        case 64:
                        case 65:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 0, true);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            encounters.Add(BuildMonster(monsterTemplates["goblinShaman"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case 66:
                        case 67:
                        case 68:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 69:
                        case 70:
                        case 71:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantWolf"]);
                            encounters.Add(BuildMonster(monsterTemplates["goblinChieftain"], new List<MonsterWeapon>() { weaponTemplates["morningstar"] }, 2, true));
                            encounters.Add(BuildMonster(monsterTemplates["goblinShaman"], new List<MonsterWeapon>() { weaponTemplates["staff"] }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case 72:
                        case 73:
                        case 74:
                            encounters = BuildMonsters(2, monsterTemplates["stoneTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            break;
                        case 75:
                        case 76:
                        case 77:
                            encounters = BuildMonsters(2, monsterTemplates["stoneTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            encounters.AddRange(BuildMonsters(6, monsterTemplates["caveGoblin"], new List<MonsterWeapon>() { weaponTemplates["net"], weaponTemplates["shortsword"] }));
                            encounters.Add(BuildMonster(monsterTemplates["goblinShaman"], new List<MonsterWeapon>() { weaponTemplates["dagger"] }, 2, false, BuildSpellList(2, 2, 2)));
                            break;
                        case 78:
                        case 79:
                        case 80:
                        case 81:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSpider"]);
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 82:
                        case 83:
                        case 84:
                            encounters = BuildMonsters(2, monsterTemplates["riverTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            break;
                        case 85:
                        case 86:
                        case 87:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantWolf"]);
                            encounters.Add(BuildMonster(monsterTemplates["goblinChieftain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 3, true));
                            break;
                        case 88:
                        case 89:
                        case 90:
                        case 91:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(3, 6), monsterTemplates["goblin"], new List<MonsterWeapon>() { weaponTemplates["shortsword"] }, 2, true);
                            encounters.Add(BuildMonster(monsterTemplates["goblinChieftain"], new List<MonsterWeapon>() { weaponTemplates["battleaxe"] }, 4));
                            break;
                        case 92:
                        case 93:
                        case 94:
                            encounters = BuildMonsters(2, monsterTemplates["riverTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            encounters.AddRange(BuildMonsters(RandomHelper.GetRandomNumber(3, 6), monsterTemplates["goblinArcher"], new List<MonsterWeapon>() { weaponTemplates["shortbow"], weaponTemplates["dagger"] }));
                            break;
                        case 95:
                        case 96:
                        case 97:
                            encounters = BuildMonsters(2, monsterTemplates["riverTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] });
                            encounters.Add(BuildMonster(monsterTemplates["stoneTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }));
                            break;
                        case 98:
                        case 99:
                        case 100:
                            encounters = BuildMonsters(1, monsterTemplates["giganticSpider"]);
                            break;
                    }
                    break;
                case EncounterType.SpringCleaning:
                    return roll switch
                    {
                        <= 30 => BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantRat"]),
                        <= 45 => BuildMonsters(1, monsterTemplates["johann"]),
                        <= 55 => BuildMonsters(1, monsterTemplates["batSwarm"]),
                        <= 65 => BuildMonsters(RandomHelper.GetRandomNumber(1, 6), monsterTemplates["giantRat"]),
                        <= 75 => BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["giantPoxRat"]),
                        <= 85 => BuildMonsters(RandomHelper.GetRandomNumber(1, 2), monsterTemplates["giantSnake"]),
                        _ => BuildMonsters(1, monsterTemplates["giantSpider"])
                    };
                case EncounterType.C26:
                    encounters = BuildMonsters(1, monsterTemplates["shambler"]);
                    break;
                case EncounterType.C29:
                    encounters = BuildMonsters(1, monsterTemplates["batSwarm"]);
                    break;
                case EncounterType.R17:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 1:
                            encounters = BuildMonsters(1, monsterTemplates["commonTroll"], new List<MonsterWeapon>() { weaponTemplates["warhammer"] }, 2);
                            break;
                        case 2:
                            encounters = BuildMonsters(1, monsterTemplates["minotaur"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2);
                            break;
                        case 3:
                            encounters = BuildMonsters(1, monsterTemplates["giganticSnake"]);
                            break;
                        case 4:
                        case 5:
                        case 6:
                            encounters = BuildMonsters(1, monsterTemplates["giganticSpider"]);
                            break;
                    }
                    break;
                case EncounterType.R19:
                    encounters = BuildMonsters(2, monsterTemplates["gargoyle"]);
                    break;
                case EncounterType.R20:
                    roll = RandomHelper.GetRandomNumber(1, 2);
                    switch (roll)
                    {
                        case 1:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 3), monsterTemplates["fireElemental"]);
                            break;
                        case 2:
                            // This originally called dungeonManager.encounter.encounterType.
                            // In a service-oriented approach, the calling service (e.g., DungeonManagerService)
                            // would pass the appropriate current encounter type.
                            encounters = GetEncounters(currentDungeonEncounterType, monsterTemplates, weaponTemplates);
                            break;
                    }
                    break;
                case EncounterType.R28:
                    encounters = BuildMonsters(2, monsterTemplates["mummy"], null, 0);
                    break;
                case EncounterType.R30:
                    roll = RandomHelper.GetRandomNumber(1, 6);
                    switch (roll)
                    {
                        case 6:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSnake"]);
                            encounters.AddRange(GetEncounters(currentDungeonEncounterType, monsterTemplates, weaponTemplates));
                            break;
                        case 4:
                        case 5:
                            encounters = BuildMonsters(RandomHelper.GetRandomNumber(1, 4), monsterTemplates["giantSnake"]);
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
                        case 5:
                        case 6:
                            encounters = BuildMonsters(2, monsterTemplates["tombGuardian"], new List<MonsterWeapon>() { weaponTemplates["greataxe"] }, 2);
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
        /// <param name="count">The number of monsters to create.</param>
        /// <param name="templateMonster">The base Monster object to copy properties from.</param>
        /// <param name="weapons">Optional: A list of weapons to equip the monsters with.</param>
        /// <param name="armourValue">Optional: The armor value to set for the monsters.</param>
        /// <param name="hasShield">Optional: Indicates if the monsters have a shield.</param>
        /// <param name="Spells">Optional: A list of spell names the monsters can cast.</param>
        /// <param name="specialRule">Optional: A special rule to add to the monsters.</param>
        /// <returns>A list of new Monster objects.</returns>
        private List<Monster> BuildMonsters(
            int count,
            Monster templateMonster,
            List<MonsterWeapon>? weapons = null,
            int armourValue = 0,
            bool hasShield = false,
            List<string>? Spells = null,
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
        /// <param name="templateMonster">The base Monster object to copy properties from.</param>
        /// <param name="weapons">Optional: A list of weapons to equip the monster with.</param>
        /// <param name="armourValue">Optional: The armor value to set for the monster.</param>
        /// <param name="hasShield">Optional: Indicates if the monster has a shield.</param>
        /// <param name="Spells">Optional: A list of spell names the monster can cast.</param>
        /// <param name="specialRule">Optional: A special rule to add to the monster.</param>
        /// <returns>A new Monster object.</returns>
        private Monster BuildMonster(
            Monster templateMonster,
            List<MonsterWeapon>? weapons = null,
            int armourValue = 0,
            bool hasShield = false,
            List<string>? Spells = null,
            string? specialRule = null)
        {
            // Assuming Monster has a constructor that copies properties from a template Monster.
            // Or you would instantiate a new Monster and manually copy relevant properties.
            Monster newMonster = templateMonster;

            // Assign new lists/values to the new monster instance to avoid modifying the template
            if (weapons != null)
            {
                newMonster.Weapons = new List<MonsterWeapon>(weapons);
            }
            newMonster.ArmourValue = armourValue; // Assuming Monster has an ArmourValue property
            if (Spells != null)
            {
                newMonster.Spells = new List<string>(Spells);
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
        /// <param name="touchSpell">Number of "Touch spell" entries.</param>
        /// <param name="rangedSpell">Number of "Ranged spell" entries.</param>
        /// <param name="supportSpell">Number of "Support spell" entries.</param>
        /// <returns>A list of spell type strings.</returns>
        private List<string> BuildSpellList(int touchSpell, int rangedSpell, int supportSpell)
        {
            List<string> Spells = new List<string>();
            for (int i = 0; i < touchSpell; i++)
            {
                Spells.Add("Touch spell");
            }
            for (int i = 0; i < rangedSpell; i++)
            {
                Spells.Add("Ranged spell");
            }
            for (int i = 0; i < supportSpell; i++)
            {
                Spells.Add("Support spell");
            }
            return Spells;
        }
    }
}