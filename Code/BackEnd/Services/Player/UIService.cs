namespace LoDCompanion.Code.BackEnd.Services.Player
{
    public class UIService
    {
        public event Func<Task>? OnStateChanged;

        public bool IsInventoryVisible { get; private set; } = false;

        public async Task ShowInventoryAsync()
        {
            IsInventoryVisible = true;
            await NotifyStateChanged();
        }

        public async Task HideInventoryAsync()
        {
            IsInventoryVisible = false;
            await NotifyStateChanged();
        }

        private async Task NotifyStateChanged()
        {
            // If there are any subscribers to the event...
            if (OnStateChanged != null)
            {
                // Get each subscriber (delegate) in the event's invocation list.
                // Invoke them and collect all the returned Tasks.
                var tasks = OnStateChanged.GetInvocationList()
                    .Select(subscriber => ((Func<Task>)subscriber)());

                // Await for all of the tasks to complete.
                await Task.WhenAll(tasks);
            }
        }
    }
}

