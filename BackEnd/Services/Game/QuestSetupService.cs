
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Player;

namespace LoDCompanion.BackEnd.Services.Game
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
        SetCombatRule,
        SetDungeonRule,
        SetPartyRule,
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
        private readonly PartyManagerService _partyManager;
        private readonly DungeonState _dungeon;
        private readonly InitiativeService _initiative;


        public QuestSetupService(
            EncounterService encounter, 
            RoomService room, 
            PartyManagerService partyManagerService,
            PlacementService placementService,
            DungeonState dungeonState,
            InitiativeService initiativeService)
        {
            _encounter = encounter;
            _room = room;
            _partyManager = partyManagerService;
            _placement = placementService;
            _dungeon = dungeonState;
            _initiative = initiativeService;
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
                    var roomInfo = _room.GetRoomByName(action.Parameters["RoomName"]);
                    _room.InitializeRoomData(roomInfo, room);
                    GridService.GenerateGridForRoom(room);
                    GridService.PlaceRoomOnGrid(room, room.GridOffset, _dungeon.DungeonGrid);
                    break;
                case QuestSetupActionType.PlaceHeroes:
                    if (_partyManager.Party != null)
                    {
                        room.HeroesInRoom ??= new List<Hero>();
                        foreach (Hero hero in _partyManager.Party.Heroes)
                        {
                            _placement.PlaceEntity(hero, room, action.Parameters);
                        } 
                    }
                    break;
                case QuestSetupActionType.SpawnMonster:

                    List<Monster> spawnMonsters = _encounter.GetEncounterByParams(action.Parameters);

                    room.MonstersInRoom ??= new List<Monster>();
                    foreach (Monster monster in spawnMonsters)
                    {
                        _placement.PlaceEntity(monster, room, action.Parameters); 
                    }
                    break;
                case QuestSetupActionType.SpawnFromChart:
                    if (Enum.TryParse<EncounterType>(action.Parameters["ChartName"], out var type))
                    {
                        List<Monster> chartMonsters = _encounter.GetRandomEncounterByType(type);
                        room.MonstersInRoom ??= new List<Monster>();
                        foreach (Monster monster in chartMonsters)
                        {
                            _placement.PlaceEntity(monster, room, action.Parameters);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: Could not find EncounterType for chart name '{action.Parameters["ChartName"]}'");
                    }
                    break;
                case QuestSetupActionType.SetTurnOrder:
                    if (Enum.TryParse<ActorType>(action.Parameters["First"], out var actorType))
                    {
                        _initiative.ForcedFirstActor = actorType;
                    }
                    break;
                case QuestSetupActionType.ModifyInitiative:
                    if (Enum.TryParse<ActorType>(action.Parameters["Target"], out var initiativeTarget) && int.TryParse(action.Parameters["Amount"], out var amount))
                    {
                        if (initiativeTarget == ActorType.Hero) _initiative.HeroInitiativeModifier = amount;
                        else _initiative.MonsterInitiativeModifier = amount;
                    }
                    break;
                case QuestSetupActionType.SetCombatRule:
                    if(!_dungeon.CombatRules.TryAdd(action.Parameters["Rule"], action.Parameters["Value"]))
                        _dungeon.CombatRules[action.Parameters["Rule"]] = action.Parameters["Value"];
                    break;
                case QuestSetupActionType.SetDungeonRule:
                    var rule = action.Parameters["Rule"];
                    var value = action.Parameters["Value"];
                    if (rule == "WanderingMonsterAtThreat")
                    {
                        _dungeon.WanderingMonsterAtThreat = int.Parse(value);
                    }
                    break;

                case QuestSetupActionType.SetPartyRule:
                    var partyRule = action.Parameters["Rule"];
                    if (partyRule == "FreeRest")
                    {
                        _partyManager.CanRestForFree = true;
                    }
                    break;
            }
        }
    }
}
