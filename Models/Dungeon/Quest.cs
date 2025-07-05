using LoDCompanion.Services.Dungeon;

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
        Whiteport
    }

    public enum EncounterType
    {
         Beasts,
         Undead,
         Bandits,
         Orcs,
         Reptiles,
         DarkElves,
         AncientLands,
         GoblinKing,
         SpringCleaning,
         C26,
         C29,
         R17,
         R19,
         R20,
         R28,
         R30,
         TombGuardian
    }

    public class Quest
    {
        public QuestLocation Location { get; private set;}
        public RoomInfo? StartingRoom { get; private set;}
        public string SpecialRules { get; private set;} = string.Empty;
        public int CorridorCount { get; private set;}
        public int RoomCount { get; private set;}
        public int RewardCoin { get; private set;}
        public string RewardSpecial { get; private set;} = string.Empty;
        public EncounterType EncounterType { get; private set;}
        public RoomInfo? ObjectiveRoom { get; private set;}
        public int StartThreatLevel { get; private set;}
        public int MinThreatLevel { get; private set;}
        public int MaxThreatLevel { get; private set;}
        public string NarrativeObjectiveRoom { get; private set; } = string.Empty;
        public string NarrativeSetup { get; private set;} = string.Empty;
        public string NarrativeAftermath { get; private set; } = string.Empty;
    }
}
