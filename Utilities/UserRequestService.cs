
namespace LoDCompanion.Utilities
{
    public class DiceRollRequest
    {
        public string Prompt { get; set; } = "Roll the dice";
        public string DiceNotation { get; set; } = "1d100";
        public TaskCompletionSource<int> CompletionSource { get; } = new TaskCompletionSource<int>();
    }

    public class ChooseOptionRequest
    {
        public string Prompt { get; set; } = "Choose an option";
        public List<string> Options { get; set; } = new List<string>();
        public TaskCompletionSource<string> CompletionSource { get; } = new TaskCompletionSource<string>();
    }

    public class UserRequestService
    {
        public event Action? OnRollRequested;
        public DiceRollRequest? CurrentDiceRequest { get; private set; }
        public ChooseOptionRequest? CurrentChoiceRequest { get; private set; }

        /// <summary>
        /// This is called by any part of the game that needs a dice roll.
        /// It shows the modal and waits for the user to provide a result.
        /// </summary>
        /// <returns>The result of the dice roll.</returns>
        public Task<int> RequestRollAsync(string prompt, string diceNotation = "1d100")
        {
            CurrentDiceRequest = new DiceRollRequest
            {
                Prompt = prompt,
                DiceNotation = diceNotation
            };

            OnRollRequested?.Invoke();
            return CurrentDiceRequest.CompletionSource.Task;
        }

        /// <summary>
        /// This is called by the modal when the user submits a result.
        /// </summary>
        public void CompleteRoll(int result)
        {
            if (CurrentDiceRequest != null)
            {
                CurrentDiceRequest.CompletionSource.SetResult(result);
                CurrentDiceRequest = null;
                OnRollRequested?.Invoke(); // Hides the modal
            }
        }

        internal async Task<string> RequestChoiceAsync(string prompt, List<string> list)
        {
            CurrentChoiceRequest = new ChooseOptionRequest
            {
                Prompt = prompt,
                Options = list
            };

            OnRollRequested?.Invoke();
            return await CurrentChoiceRequest.CompletionSource.Task;
        }
    }
}
