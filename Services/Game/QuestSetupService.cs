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
        private readonly PlacementService _placement;
        private readonly RoomService _room;
        private readonly PartyManagerService _party;

        public QuestSetupService(
            EncounterService encounter, 
            RoomService room, 
            PartyManagerService partyManagerService,
            PlacementService placementService)
        {
            _encounter = encounter;
            _room = room;
            _party = partyManagerService;
            _placement = placementService;
        }

        public void ExecuteRoomSetup(Quest quest, Room room)
        {
            foreach (var action in quest.SetupActions)
            {
                ExecuteAction(room, action);
            }
        }

        private void ExecuteAction(Room room, QuestSetupAction action)
        {
            switch (action.ActionType)
            {
                case QuestSetupActionType.SetQuestRule:
                    // Example: _gameState.SetRule(action.Parameters["Rule"], action.Parameters["Value"]);
                    break;
                case QuestSetupActionType.SetRoom:
                    _room.InitializeRoomData(_room.GetRoomByName(action.Parameters["RoomName"]), room);
                    break;
                case QuestSetupActionType.PlaceHeroes:
                    if (_party.Party != null)
                    {
                        foreach (Hero hero in _party.Party.Heroes)
                        {
                            _placement.PlaceEntity(hero, room, action.Parameters);
                        } 
                    }
                    break;
                case QuestSetupActionType.SpawnMonster:

                    List<Monster> spawnMonsters = _encounter.GetEncounterByParams(action.Parameters);

                    // The CombatManager places the created monster.
                    foreach (Monster monster in spawnMonsters)
                    {
                        _placement.PlaceEntity(monster, room, action.Parameters); 
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
                        _placement.PlaceEntity(monster, room, action.Parameters);
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
