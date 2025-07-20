namespace LoDCompanion.Services.Player
{
    public class UIService
    {
        public event Action? OnStateChanged;

        public bool IsInventoryVisible { get; private set; } = false;

        public void ShowInventory()
        {
            IsInventoryVisible = true;
            OnStateChanged?.Invoke();
        }

        public void HideInventory()
        {
            IsInventoryVisible = false;
            OnStateChanged?.Invoke();
        }
    }
}
