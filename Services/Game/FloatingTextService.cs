using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Game
{
    public class FloatingText
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
        public GridPosition Position { get; set; } = new GridPosition(0, 0, 0);
        public string CssClass { get; set; } = "info-text"; // e.g., "damage-text", "miss-text"
    }

    public class FloatingTextService
    {
        public event Action? OnTextChanged;
        public List<FloatingText> ActiveTexts { get; } = new List<FloatingText>();

        /// <summary>
        /// Shows a text message at a specific grid position that fades out after a delay.
        /// </summary>
        public void ShowText(string text, GridPosition position, string cssClass = "info-text")
        {
            var floatingText = new FloatingText
            {
                Text = text,
                Position = position,
                CssClass = cssClass
            };

            ActiveTexts.Add(floatingText);
            OnTextChanged?.Invoke();

            // Automatically remove the text after a couple of seconds.
            _ = Task.Delay(2000).ContinueWith(t =>
            {
                ActiveTexts.Remove(floatingText);
                OnTextChanged?.Invoke();
            });
        }
    }
}
