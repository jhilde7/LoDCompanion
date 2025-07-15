using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Game
{
    public class ToastMessage
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? Text { get; set; }
        public GridPosition? Position { get; set; }
        public string? CssClass { get; set; } // e.g., "damage-toast", "miss-toast"
    }

    public class ToastService
    {
        public event Action? OnToastsChanged;
        public List<ToastMessage> ActiveToasts { get; } = new List<ToastMessage>();

        /// <summary>
        /// Displays a toast message at a specific grid position for a short duration.
        /// </summary>
        public void ShowToast(string text, GridPosition position, string cssClass = "info-toast")
        {
            var toast = new ToastMessage
            {
                Text = text,
                Position = position,
                CssClass = cssClass
            };

            ActiveToasts.Add(toast);
            OnToastsChanged?.Invoke();

            // Automatically remove the toast after a delay (e.g., 2 seconds)
            _ = Task.Delay(2000).ContinueWith(t =>
            {
                ActiveToasts.Remove(toast);
                OnToastsChanged?.Invoke();
            });
        }
    }
}
