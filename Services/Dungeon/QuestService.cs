using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;

namespace LoDCompanion.Services.Dungeon
{
    public class QuestService
    {
        public Quest? ActiveQuest { get; private set; }
        public bool IsObjectiveComplete { get; private set; }

        public QuestService() { }

        /// <summary>
        /// Starts a new quest, setting it as the active one.
        /// </summary>
        /// <param name="quest">The quest to begin.</param>
        public void StartQuest(Quest quest)
        {
            ActiveQuest = quest;
            IsObjectiveComplete = false;
            Console.WriteLine($"Quest Started: {ActiveQuest.NarrativeSetup}");
        }

        /// <summary>
        /// Checks if the quest's objective has been met.
        /// This would be called after key game events (e.g., defeating a boss, finding an item).
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        public void CheckObjectiveCompletion(DungeonState dungeonState)
        {
            if (ActiveQuest == null || IsObjectiveComplete) return;

            // Example objective: Check if the party is in the objective room.
            if (dungeonState.CurrentRoom?.RoomName == ActiveQuest.ObjectiveRoom?.Name)
            {
                // A more complex quest might require a specific monster to be defeated
                // or an item to be in the party's inventory.
                Console.WriteLine("Quest objective completed!");
                IsObjectiveComplete = true;
            }
        }

        /// <summary>
        /// Grants the quest rewards to the party.
        /// </summary>
        /// <param name="party">The party to receive the rewards.</param>
        /// <returns>A string describing the rewards given.</returns>
        public string GrantRewards(Party party)
        {
            if (ActiveQuest == null || !IsObjectiveComplete)
            {
                return "Quest objective not yet complete. No rewards given.";
            }

            party.Coins += ActiveQuest.RewardCoin;
            // TODO: Add logic to grant special item rewards (ActiveQuest.RewardSpecial)

            var rewardMessage = $"Quest Complete! The party receives {ActiveQuest.RewardCoin} coins. {ActiveQuest.NarrativeAftermath}";

            // Reset the quest service for the next adventure.
            ActiveQuest = null;
            IsObjectiveComplete = false;

            return rewardMessage;
        }
    }
}
