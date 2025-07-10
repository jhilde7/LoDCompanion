using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;

namespace LoDCompanion.Models.Dungeon
{
    public enum QuestLocation
    {
        Caelkirk,
        Coalfell,
        Freyfall,
        Irondale,
        Rochdale,
        SilverCity,
        TheOutpost,
        Windfair,
        Whiteport,
        AncientLands,
        Random,
        MainQuest,
        CurrentTown,
        OutsideRochdale,
        White22,
        White34,
        White36,
        White38,
        White39,
        White40
    }

    public enum EncounterType
    {
         Beasts,
         Undead,
         Bandits_Brigands,
         Magic,
         Orcs_Goblins,
         Reptiles,
         DarkElves,
         AncientLands,
         GoblinKing,
         SpringCleaning,
         TheTombOfTheSpiderQueen,
         StopTheHeretics,
         TheMasterAlchemist,
         SlayTheBeast,
         TheMissingBrother,
         C26,
         C29,
         R17,
         R19,
         R20,
         R28,
         R30,
         TombGuardian,
         MainQuest,
         Random
    }

    public class Quest
    {
        public bool IsSideQuest { get; set; }
        public QuestLocation Location { get; set;}
        public string Name { get; set;} = string.Empty;
        public RoomInfo? StartingRoom { get; set;}
        public string SpecialRules { get; set;} = string.Empty;
        public int CorridorCount { get; set;}
        public List<RoomInfo>? CorridorsToExclude { get; set;}
        public int RoomCount { get; set; }
        public List<RoomInfo>? RoomsToExclude { get; set; }
        public int RewardCoin { get; set;}
        public string RewardSpecial { get; set;} = string.Empty;
        public EncounterType EncounterType { get; set;}
        public RoomInfo? ObjectiveRoom { get; set;}
        public int StartThreatLevel { get; set;}
        public int MinThreatLevel { get; set;}
        public int MaxThreatLevel { get; set;}
        public string NarrativeQuest { get; set; } = string.Empty;
        public string NarrativeObjectiveRoom { get; set; } = string.Empty;
        public string NarrativeSetup { get; set;} = string.Empty;
        public string NarrativeAftermath { get; set; } = string.Empty;
        public List<QuestSetupAction> SetupActions { get; set; } = new();

        public Quest()
        {

        }
    }
}
