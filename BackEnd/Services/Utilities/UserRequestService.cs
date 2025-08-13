using LoDCompanion.BackEnd.Models;

namespace LoDCompanion.BackEnd.Services.Utilities
{
    public class DiceRollRequest
    {
        public string Prompt { get; set; } = "Roll the dice";
        public string DiceNotation { get; set; } = "1d100";
        public bool IsCancellable { get; set; } = false;
        public TaskCompletionSource<DiceRollResult> CompletionSource { get; } = new TaskCompletionSource<DiceRollResult>();
        public Hero? HeroAttemptingRoll { get; internal set; }
        public Skill? SkillBeingUsed { get; internal set; }
        public BasicStat? StatBeingUsed { get; internal set; }
    }

    public class DiceRollResult
    {
        public int Roll { get; set; }
        public bool WasCancelled { get; set; } = false;
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
        public Task<DiceRollResult> RequestRollAsync(string prompt, string diceNotation = "1d100", bool canCancel = false, 
            Hero? hero = null, Skill? skill = null, BasicStat? stat = null)
        {
            CurrentDiceRequest = new DiceRollRequest
            {
                Prompt = prompt,
                DiceNotation = diceNotation,
                IsCancellable = canCancel,
                HeroAttemptingRoll = hero,
                SkillBeingUsed = skill,
                StatBeingUsed = stat
            };

            OnRollRequested?.Invoke();
            return CurrentDiceRequest.CompletionSource.Task;
        }

        /// <summary>
        /// This is called by the modal when the user submits a result.
        /// </summary>
        public void CompleteRoll(DiceRollResult result)
        {
            if (CurrentDiceRequest != null)
            {
                if (CurrentDiceRequest.HeroAttemptingRoll != null)
                {
                    CurrentDiceRequest.HeroAttemptingRoll.CheckPerfectRoll(result.Roll, skill: CurrentDiceRequest.SkillBeingUsed, stat: CurrentDiceRequest.StatBeingUsed);
                }

                CurrentDiceRequest.CompletionSource.SetResult(result);
                CurrentDiceRequest = null;
                OnRollRequested?.Invoke(); // Hides the modal
            }
        }

        public void CancelRoll()
        {
            var result = new DiceRollResult { WasCancelled = true };
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
