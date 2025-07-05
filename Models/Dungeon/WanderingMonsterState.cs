using LoDCompanion.Models.Character;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Models.Dungeon
{
    public class WanderingMonsterState
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public RoomService? CurrentRoom { get; set; }
        public Monster? RevealedMonster { get; set; }
        public bool IsRevealed => RevealedMonster != null;
        public GridPosition CurrentPosition { get; set; } = new GridPosition(0, 0);

        public WanderingMonsterState()
        {

        }
    }
}
