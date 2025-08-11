
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;

namespace LoDCompanion.Services.Player
{
    public class MovementHighlightingService
    {
        public event Action? OnHighlightChanged;
        public HashSet<GridPosition> HighlightedSquares { get; private set; } = new HashSet<GridPosition>();

        public void HighlightWalkableSquares(Hero hero, DungeonState dungeonState)
        {
            if (hero?.Position == null || dungeonState?.DungeonGrid == null)
            {
                ClearHighlights();
                return;
            }

            var path = GridService.GetAllWalkableSquares(hero, dungeonState.DungeonGrid, dungeonState.RevealedMonsters.Cast<Character>().ToList());
            HighlightedSquares = new HashSet<GridPosition>(path);
            NotifyStateChanged();
        }

        public void ClearHighlights()
        {
            if (HighlightedSquares.Any())
            {
                HighlightedSquares.Clear();
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnHighlightChanged?.Invoke();
    }
}