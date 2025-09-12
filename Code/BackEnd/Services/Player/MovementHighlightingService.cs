using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Dungeon;

namespace LoDCompanion.Code.BackEnd.Services.Player
{
    public class MovementHighlightingService
    {
        public event Action? OnHighlightChanged;
        public HashSet<GridPosition> HighlightedSquares { get; private set; } = new HashSet<GridPosition>();

        public void HighlightWalkableSquares(Hero hero, Room room)
        {
            var enemiesList = room.MonstersInRoom ?? new List<Monster>();
            if (hero?.Position == null || room.Grid == null)
            {
                ClearHighlights();
                return;
            }

            if(enemiesList.Count <= 0)
            {
                enemiesList = hero.Room?.MonstersInRoom ?? new List<Monster>();
            }

            // Get the pre-calculated costs for all squares reachable within a full move.
            var walkableSquaresWithCosts = GridService.GetAllWalkableSquares(hero, room.Grid, enemiesList.Cast<Character>().ToList());

            // Now, filter this dictionary based on the hero's CURRENT movement points.
            HighlightedSquares = walkableSquaresWithCosts
                .Where(kvp => kvp.Value <= hero.CurrentMovePoints)
                .Select(kvp => kvp.Key)
                .ToHashSet();

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