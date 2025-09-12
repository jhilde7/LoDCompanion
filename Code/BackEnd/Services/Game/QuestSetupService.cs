using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Dungeon;
using LoDCompanion.Code.BackEnd.Services.Player;

namespace LoDCompanion.Code.BackEnd.Services.Game
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
        private readonly EncounterService _encounter = new EncounterService();
        private readonly PlacementService _placement = new PlacementService();
        private readonly RoomService _room = new RoomService();

        public event Action<ActorType>? OnForcedFirstActor;
        public event Action<ActorType, int>? OnInitiativeModifier;
        public event Action<bool>? OnCanRestForFree;
        public event Action<bool>? OnCanTakePreQuestRest;
        public event Func<Task<List<Hero>>>? OnGetParty;
        public event Action<string, string>? OnSetDungeonRule;
        public event Func<Task<EncounterType>>? OnGetEncounterType;
        public event Action<CombatRule>? OnAddQuestCombatRules;

        public QuestSetupService()
        {
            
        }

        public async Task ExecuteRoomSetupAsync(Quest quest, Room room)
        {
            foreach (var action in quest.SetupActions)
            {
                await ExecuteActionAsync(room, action);
            }
        }

        private async Task ExecuteActionAsync(Room room, QuestSetupAction action)
        {
            switch (action.ActionType)
            {
                case QuestSetupActionType.SetDungeonRule:
                    if (OnSetDungeonRule != null) OnSetDungeonRule.Invoke(action.Parameters["Rule"], action.Parameters["Value"]);
                    break;
                case QuestSetupActionType.SetRoom:
                    var roomInfo = _room.GetRoomByName(action.Parameters["RoomName"]);
                    _room.InitializeRoomData(roomInfo, room);
                    GridService.GenerateGridForRoom(room);
                    GridService.PlaceRoomOnGrid(room, room.GridOffset, room.Grid);
                    break;
                case QuestSetupActionType.PlaceHeroes:
                    if (OnGetParty != null)
                    {
                        foreach (Hero hero in await OnGetParty.Invoke())
                        {
                            _placement.PlaceEntity(hero, room, action.Parameters);
                        } 
                    }
                    break;
                case QuestSetupActionType.SpawnMonster:

                    List<Monster> spawnMonsters = _encounter.GetEncounterByParams(action.Parameters);

                    foreach (Monster monster in spawnMonsters)
                    {
                        _placement.PlaceEntity(monster, room, action.Parameters); 
                    }
                    break;
                case QuestSetupActionType.SpawnFromChart:
                    EncounterType chartType;
                    string chartName = action.Parameters["ChartName"];

                    if (chartName.Equals("DungeonDefault", StringComparison.OrdinalIgnoreCase) && OnGetEncounterType != null)
                    {
                        chartType = await OnGetEncounterType.Invoke();
                    }
                    else if (!Enum.TryParse(chartName, out chartType))
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
                            targetMonster.ActiveStatusEffects.Add(new ActiveStatusEffect(statusEffect, duration));
                        }
                    }
                    break;
                case QuestSetupActionType.SetTurnOrder:
                    if (Enum.TryParse<ActorType>(action.Parameters["First"], out var actorType)
                        && OnForcedFirstActor != null)
                    {
                        OnForcedFirstActor.Invoke(actorType);
                    }
                    break;
                case QuestSetupActionType.ModifyInitiative:
                    if (Enum.TryParse<ActorType>(action.Parameters["Target"], out var initiativeTarget) && int.TryParse(action.Parameters["Amount"], out var amount) 
                        && OnInitiativeModifier != null)
                    {
                        if (initiativeTarget == ActorType.Hero) OnInitiativeModifier.Invoke(ActorType.Hero, amount);
                        else OnInitiativeModifier.Invoke(ActorType.Monster, amount);
                    }
                    break;
                case QuestSetupActionType.SetCombatRule:
                    var newRule = new CombatRule();

                    if (action.Parameters.TryGetValue("Rule", out var ruleTypeStr) && Enum.TryParse<CombatRuleType>(ruleTypeStr, out var ruleType))
                    {
                        newRule.RuleType = ruleType;
                    }

                    if (action.Parameters.TryGetValue("Value", out var valStr) && int.TryParse(valStr, out var intVal))
                    {
                        newRule.IntValue = intVal;
                    }
                    else
                    {
                        newRule.StringValue = valStr;
                    }

                    if (action.Parameters.TryGetValue("Target", out var target))
                    {
                        newRule.TargetName = target;
                    }

                    if (action.Parameters.TryGetValue("OnFail", out var onFailStr))
                    {
                        newRule.OnFailTrigger = ParseTrigger(onFailStr);
                    }

                    if (OnAddQuestCombatRules != null) OnAddQuestCombatRules.Invoke(newRule);
                    break;

                case QuestSetupActionType.SetPartyRule:
                    var partyRule = action.Parameters["Rule"];
                    if (partyRule == "FreeRest" && OnCanRestForFree != null)
                    {
                        OnCanRestForFree.Invoke(true);
                    }
                    else if (partyRule == "PreQuestRest" && OnCanTakePreQuestRest != null)
                    {
                        OnCanTakePreQuestRest.Invoke(true);
                    }
                    break;
            }
        }

        private Trigger? ParseTrigger(string triggerString)
        {
            // Expected format: "TriggerType:Param1=Value1,Param2=Value2"
            // Example: "SummonMonster:Name=Demon,Count=1d3,PlacementRule=RandomSquare"
            var parts = triggerString.Split(':', 2);
            if (parts.Length < 2) return null;

            if (Enum.TryParse<TriggerType>(parts[0], out var type))
            {
                var trigger = new Trigger { Type = type };
                var parameters = parts[1].Split(',');
                foreach (var param in parameters)
                {
                    var keyValue = param.Split('=', 2);
                    if (keyValue.Length < 2) continue;
                    trigger.Parameters[keyValue[0].Trim()] = keyValue[1].Trim();
                }
                return trigger;
            }
            return null;
        }
    }
}
