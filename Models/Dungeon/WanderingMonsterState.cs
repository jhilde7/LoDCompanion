using LoDCompanion.Models.Character;

namespace LoDCompanion.Models.Dungeon
{
    public class WanderingMonsterState
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public string CurrentRoomId { get; set; }
        public Monster? RevealedMonster { get; set; }
        public bool IsRevealed => RevealedMonster != null;

        public WanderingMonsterState(string startingRoomId)
        {
            CurrentRoomId = startingRoomId;
            RevealedMonster = null;
        }
    }
}
