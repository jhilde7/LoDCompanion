namespace LoDCompanion.Utilities
{
    public class DiceRollRequest
    {
        public string Prompt { get; set; } = "Roll the dice";
        public string DiceNotation { get; set; } = "1d100";
        public TaskCompletionSource<int> CompletionSource { get; } = new TaskCompletionSource<int>();
    }

    public class DiceRollService
    {
        public event Action? OnRollRequested;
        public DiceRollRequest? CurrentRequest { get; private set; }

        /// <summary>
        /// This is called by any part of the game that needs a dice roll.
        /// It shows the modal and waits for the user to provide a result.
        /// </summary>
        /// <returns>The result of the dice roll.</returns>
        public Task<int> RequestRollAsync(string prompt, string diceNotation = "1d100")
        {
            CurrentRequest = new DiceRollRequest
            {
                Prompt = prompt,
                DiceNotation = diceNotation
            };

            OnRollRequested?.Invoke();
            return CurrentRequest.CompletionSource.Task;
        }

        /// <summary>
        /// This is called by the modal when the user submits a result.
        /// </summary>
        public void CompleteRoll(int result)
        {
            if (CurrentRequest != null)
            {
                CurrentRequest.CompletionSource.SetResult(result);
                CurrentRequest = null;
                OnRollRequested?.Invoke(); // Hides the modal
            }
        }
    }
}
