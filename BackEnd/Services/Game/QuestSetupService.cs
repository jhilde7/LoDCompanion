
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
        ModifyFurniture,
        ConstrainRoomEntry,
        ApplyStatusEffect,
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
                case QuestSetupActionType.SetDungeonRule:
                    _dungeon.DungeonRules[action.Parameters["Rule"]] = action.Parameters["Value"];
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
                    EncounterType chartType;
                    string chartName = action.Parameters["ChartName"];

                    if (chartName.Equals("DungeonDefault", StringComparison.OrdinalIgnoreCase))
                    {
                        chartType = _dungeon.EncounterType;
                    }
                    else if (!Enum.TryParse<EncounterType>(chartName, out chartType))
                    {
                        Console.WriteLine($"Error: Could not find EncounterType for chart name '{chartName}'");
                        break;
                    }

                    int rolls = action.Parameters.TryGetValue("Rolls", out var rollsStr) && int.TryParse(rollsStr, out var parsedRolls) ? parsedRolls : 1;

                    room.MonstersInRoom ??= new List<Monster>();

                    for (int i = 0; i < rolls; i++)
                    {
                        List<Monster> chartMonsters = _encounter.GetRandomEncounterByType(chartType);
                        foreach (Monster monster in chartMonsters)
                        {
                            room.MonstersInRoom.Add(monster);
                            _placement.PlaceEntity(monster, room, action.Parameters);
                        }
                    }
                    break;
                case QuestSetupActionType.ApplyStatusEffect:
                    if (action.Parameters.TryGetValue("TargetName", out var targetName) &&
                        action.Parameters.TryGetValue("StatusEffect", out var statusEffectStr) &&
                        Enum.TryParse<StatusEffectType>(statusEffectStr, out var statusEffect))
                    {
                        var targetMonster = room.MonstersInRoom?.FirstOrDefault(m => m.Name == targetName);
                        if (targetMonster != null)
                        {
                            int duration = action.Parameters.TryGetValue("Duration", out var durStr) && int.TryParse(durStr, out var dur) ? dur : -1;
                            // Assuming StatusEffectService has a method to apply effects
                            targetMonster.ActiveStatusEffects.Add(new ActiveStatusEffect(statusEffect, duration));
                        }
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
                    _dungeon.CombatRules[action.Parameters["Rule"]] = action.Parameters["Value"];
                    break;

                case QuestSetupActionType.SetPartyRule:
                    var partyRule = action.Parameters["Rule"];
                    if (partyRule == "FreeRest")
                    {
                        _partyManager.CanRestForFree = true;
                    }
                    else if (partyRule == "PreQuestRest")
                    {
                        _partyManager.CanTakePreQuestRest = true;
                    }
                    break;
                case QuestSetupActionType.ModifyFurniture:
                    if (action.Parameters.TryGetValue("TargetFurnitureName", out var targetName) && room.FurnitureList.FirstOrDefault(f => f.Name == targetName) is Furniture furnitureToModify)
                    {
                        if (action.Parameters.TryGetValue("NewName", out var newName))
                        {
                            furnitureToModify.Name = newName;
                        }
                        if (action.Parameters.TryGetValue("IsLocked", out var isLockedStr) && bool.TryParse(isLockedStr, out var isLocked) && furnitureToModify is Chest chest)
                        {
                            if (!isLocked)
                            {
                                chest.Lock.SetLockState(0, 0); // Unlocks the chest
                            }
                        }
                        if (action.Parameters.TryGetValue("IsTrapped", out var isTrappedStr) && bool.TryParse(isTrappedStr, out var isTrapped) && furnitureToModify is Chest chestWithTrap)
                        {
                            if (!isTrapped)
                            {
                                chestWithTrap.Trap.IsDisarmed = true;
                            }
                        }
                    }
                    break;
            }
        }
    }
}
