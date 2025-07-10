using LoDCompanion.Models.Character;
using LoDCompanion.Models;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;

namespace LoDCompanion.Services.Game
{
    public enum QuestSetupActionType
    {
        SetQuestRule,
        SetRoom,
        PlaceHeroes,
        SpawnMonster,
        SpawnFromChart,
        SetTurnOrder,
        DefineInteraction,
        ModifyInitiative,
        SetCombatRule
    }

    public class QuestSetupAction
    {
        public QuestSetupActionType ActionType { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    public class QuestSetupService
    {
        private readonly EncounterService _encounter;
        private readonly RoomService _room;
        private readonly GameDataService _gameData;
        private readonly PartyManagerService _party;

        public QuestSetupService(
            EncounterService encounter, 
            RoomService room, 
            GameDataService gameData,
            PartyManagerService partyManagerService)
        {
            _encounter = encounter;
            _room = room;
            _gameData = gameData;
            _party = partyManagerService;
        }

        public void ExecuteObjectiveRoomSetup(Quest quest)
        {
            foreach (var action in quest.SetupActions)
            {
                ExecuteAction(action);
            }
        }

        private void ExecuteAction(QuestSetupAction action)
        {
            switch (action.ActionType)
            {
                case QuestSetupActionType.SetQuestRule:
                    // Example: _gameState.SetRule(action.Parameters["Rule"], action.Parameters["Value"]);
                    break;
                case QuestSetupActionType.SetRoom:
                    _room.GetRoomByName(action.Parameters["RoomName"]);
                    break;
                case QuestSetupActionType.PlaceHeroes:
                    _encounter.PlaceHeroes(
                        _party.GetCurrentParty().Heroes,
                        _room.InitializeRoomData(_room.GetRoomByName(action.Parameters["RoomName"]), new Room()),
                        action.Parameters
                    );
                    break;
                case QuestSetupActionType.SpawnMonster:
                    Monster baseMonster = _encounter.GetMonsterByName(action.Parameters["Name"]);
                    int count = int.Parse(action.Parameters["Count"]);
                    List<Weapon> weapons = new List<Weapon>();
                    foreach (string weapon in action.Parameters["Weapons"].Split(","))
                    {
                        weapons.Add(EquipmentService.GetWeaponByName(weapon.Trim()));
                    }
                    bool hasShield = bool.Parse(action.Parameters["Shield"]);
                    int armour = int.Parse(action.Parameters["Armour"]);
                    List<MonsterSpell> spells = new List<MonsterSpell>();
                    foreach (string spell in action.Parameters["Spells"].Split(","))
                    {
                        spells.Add(_gameData.GetMonsterSpellByName(spell));
                    }
                    string specialRule = action.Parameters["SpecialRule"];

                    List<Monster> spawnMonsters = _encounter.BuildMonsters(count, baseMonster, weapons, armour, hasShield, spells, specialRule);

                    // The CombatManager places the created monster.
                    foreach (Monster monster in spawnMonsters)
                    {
                        _encounter.PlaceMonster(
                                        monster,
                                        _room.InitializeRoomData(_room.GetRoomByName(action.Parameters["RoomName"]), new Room()),
                                        action.Parameters
                                    ); 
                    }
                    break;
                case QuestSetupActionType.SpawnFromChart:
                    EncounterType type;
                    switch(action.Parameters["ChartName"])
                    {
                        case "BanditsAndBrigands":
                            type = EncounterType.Bandits_Brigands;
                            break;
                        case "OrcsAndGoblins":
                            type = EncounterType.Orcs_Goblins;
                            break;

                        case "Undead":
                            type = EncounterType.Undead;
                            break;

                        case "Reptiles":
                            type = EncounterType.Reptiles;
                            break;

                        case "Dark Elves":
                            type = EncounterType.DarkElves;
                            break;

                        case "AncientLands":
                            type = EncounterType.AncientLands;
                            break;
                        default: type = EncounterType.Beasts; 
                            break;
                    }
                    List<Monster> chartMonsters = _encounter.GetRandomEncounterByType(type);

                    foreach (Monster monster in chartMonsters)
                    {
                        _encounter.PlaceMonster(
                                        monster,
                                        _room.InitializeRoomData(_room.GetRoomByName(action.Parameters["RoomName"]), new Room()),
                                        action.Parameters
                                    );
                    }
                    break;
                case QuestSetupActionType.SetTurnOrder:
                    break;
                case QuestSetupActionType.ModifyInitiative:
                    break;
                case QuestSetupActionType.SetCombatRule:
                    break;
            }
        }
    }
}
