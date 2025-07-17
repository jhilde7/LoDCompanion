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
        public async Task ShowTextAsync(string text, GridPosition position, string cssClass = "info-text")
        {
            var floatingText = new FloatingText
            {
                Text = text,
                Position = position,
                CssClass = cssClass
            };

            ActiveTexts.Add(floatingText);
            OnTextChanged?.Invoke();

            // Wait for the delay without blocking the thread.
            //await Task.Delay(1000);

            // This code now runs after the delay on the same context.
            //ActiveTexts.Remove(floatingText);
            //OnTextChanged?.Invoke();
            
            
            // Fire and forget a helper task to handle the removal after a delay.
            _ = RemoveTextAfterDelay(floatingText);
        }

        /// <summary>
        /// Private helper that waits for a delay and then removes the text.
        /// </summary>
        private async Task RemoveTextAfterDelay(FloatingText textToRemove)
        {
            await Task.Delay(500);

            ActiveTexts.Remove(textToRemove);
            OnTextChanged?.Invoke();
        }
    }
}
