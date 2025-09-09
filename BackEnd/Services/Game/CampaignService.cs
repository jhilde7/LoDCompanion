using LoDCompanion.BackEnd.Services.Player;

namespace LoDCompanion.BackEnd.Services.Game
{

    public class Campaign
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Quest> Quests { get; set; } = new List<Quest>();
        public (QuestColor Color, int Location)? StartingLocation { get; set; }
        public int RewardCoinsPerHero { get; set; }

        public Campaign(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    public class CampaignService
    {
        public string? ActiveCampaignName { get; private set; }
        public int CurrentQuestIndex { get; private set; }

        public void StartCampaign(Campaign campaign)
        {
            ActiveCampaignName = campaign.Name;
            CurrentQuestIndex = 0;
            Console.WriteLine($"Campaign started: {campaign.Name}");
        }

        public Quest? GetCurrentQuest(Campaign campaign)
        {
            if (campaign.Name != ActiveCampaignName || CurrentQuestIndex >= campaign.Quests.Count)
            {
                return null; // No active quest in this campaign or campaign is finished
            }
            return campaign.Quests[CurrentQuestIndex];
        }

        public void AdvanceToNextQuest()
        {
            CurrentQuestIndex++;
            Console.WriteLine("Advanced to the next quest in the campaign.");
        }

        public void ResetCampaignProgress()
        {
            ActiveCampaignName = null;
            CurrentQuestIndex = 0;
        }
    }
}
