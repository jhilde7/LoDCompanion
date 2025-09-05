using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.GameData;
using System.Collections.Generic;

namespace LoDCompanion.BackEnd.Services.Utilities
{
    public class DiceRollRequest
    {
        public string Prompt { get; set; } = "Roll the dice";
        public string DiceNotation { get; set; } = "1d100";
        public bool IsCancellable { get; set; } = false;
        public TaskCompletionSource<DiceRollResult> CompletionSource { get; } = new TaskCompletionSource<DiceRollResult>();
        public (Hero, Skill)? SkillBeingUsed { get; internal set; }
        public (Hero, BasicStat)? StatBeingUsed { get; internal set; }
    }

    public class DiceRollResult
    {
        public int Roll { get; set; }
        public bool WasCancelled { get; set; } = false;
    }

    public class ChooseOptionRequest<T>
    {
        public string Prompt { get; set; } = "Choose an option";
        public List<T> Options { get; set; } = new List<T>();
        public Func<T, string> DisplaySelector { get; set; }
        public bool IsCancellable { get; set; } = false;
        public TaskCompletionSource<ChoiceOptionResult<T>> CompletionSource { get; } = new TaskCompletionSource<ChoiceOptionResult<T>>();

        public ChooseOptionRequest(string prompt, List<T> options, Func<T, string> displaySelector, bool isCancellable)
        {
            Prompt = prompt;
            Options = options;
            DisplaySelector = displaySelector;
            IsCancellable = isCancellable;
        }
    }

    public class ChoiceOptionResult<T>
    {
        public T? SelectedOption { get; set; }
        public bool WasCancelled { get; set; } = false;
    }

    public class NumberInputRequest
    {
        public string Prompt { get; set; } = "Enter a number";
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public bool IsCancellable { get; set; } = false;
        public TaskCompletionSource<NumberInputResult> CompletionSource { get; } = new TaskCompletionSource<NumberInputResult>();
    }

    public class NumberInputResult
    {
        public int Amount { get; set; }
        public bool WasCancelled { get; set; } = false;
    }

    public class RecipeCreationRequest
    {
        public string Prompt { get; set; } = "Create an Alchemical Recipe";
        public TaskCompletionSource<AlchemicalRecipe?> CompletionSource { get; } = new(); // Nullable for cancellation
    }

    public class UserRequestService
    {
        public event Action? OnRollRequested;
        public event Action? OnRequestChanged;
        private Action? _cancelChoiceAction;
        public DiceRollRequest? CurrentDiceRequest { get; private set; }
        public object? CurrentChoiceRequest { get; private set; }
        public NumberInputRequest? CurrentNumberInputRequest { get; private set; }
        public RecipeCreationRequest? CurrentRecipeRequest { get; private set; }


        /// <summary>
        /// This is called by any part of the game that needs a dice roll.
        /// It shows the modal and waits for the user to provide a result.
        /// </summary>
        /// <returns>The result of the dice roll.</returns>
        public Task<DiceRollResult> RequestRollAsync(string prompt, string diceNotation = "1d100", bool canCancel = false, 
            (Hero, Skill)? skill = null, (Hero, BasicStat)? stat = null)
        {
            CurrentDiceRequest = new DiceRollRequest
            {
                Prompt = prompt,
                DiceNotation = diceNotation,
                IsCancellable = canCancel,
                SkillBeingUsed = skill,
                StatBeingUsed = stat
            };

            OnRollRequested?.Invoke();
            return CurrentDiceRequest.CompletionSource.Task;
        }

        public void CompleteRoll(DiceRollResult result)
        {
            if (CurrentDiceRequest != null)
            {
                if (CurrentDiceRequest.SkillBeingUsed.HasValue)
                {
                    Hero hero = CurrentDiceRequest.SkillBeingUsed.Value.Item1;
                    hero.CheckPerfectRoll(result.Roll, skill: CurrentDiceRequest.SkillBeingUsed.Value.Item2);
                }
                else if (CurrentDiceRequest.StatBeingUsed.HasValue)
                {
                    Hero hero = CurrentDiceRequest.StatBeingUsed.Value.Item1;
                    hero.CheckPerfectRoll(result.Roll, stat: CurrentDiceRequest.StatBeingUsed.Value.Item2);
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

        /// <summary>
        /// Prompts the user to choose an item from a generic list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="prompt">The message to display to the user.</param>
        /// <param name="options">The list of items to choose from.</param>
        /// <param name="displaySelector">A function to get the display string for each item.</param>
        /// <param name="canCancel">Whether the user can cancel the choice.</param>
        /// <returns>The result of the user's choice.</returns>
        public Task<ChoiceOptionResult<T>> RequestChoiceAsync<T>(string prompt, List<T> options, Func<T, string> displaySelector, bool canCancel = false)
        {
            var request = new ChooseOptionRequest<T>(prompt, options, displaySelector, canCancel);
            CurrentChoiceRequest = request;

            _cancelChoiceAction = () =>
            {
                request.CompletionSource.SetResult(new ChoiceOptionResult<T> { WasCancelled = true });
                CurrentChoiceRequest = null;
                _cancelChoiceAction = null;
                OnRequestChanged?.Invoke();
            };

            OnRequestChanged?.Invoke();
            return request.CompletionSource.Task;
        }

        public void CompleteChoice<T>(T selectedOption)
        {
            if (CurrentChoiceRequest is ChooseOptionRequest<T> request)
            {
                request.CompletionSource.SetResult(new ChoiceOptionResult<T> { SelectedOption = selectedOption });
                CurrentChoiceRequest = null;
                _cancelChoiceAction = null;
                OnRequestChanged?.Invoke();
            }
        }

        public void CancelChoice()
        {
            _cancelChoiceAction?.Invoke();
        }

        public async Task<bool> RequestYesNoChoiceAsync(string prompt)
        {
            var options = new List<string> { "Yes", "No" };
            // We use the generic method, specifying <string> and a simple selector.
            var result = await RequestChoiceAsync(prompt, options, s => s);
            return !result.WasCancelled && result.SelectedOption == "Yes";
        }

        public Task<NumberInputResult> RequestNumberInputAsync(string prompt, int? min = null, int? max = null, bool canCancel = false)
        {
            CurrentNumberInputRequest = new NumberInputRequest
            {
                Prompt = prompt,
                MinValue = min,
                MaxValue = max,
                IsCancellable = canCancel,
            };

            OnRequestChanged?.Invoke();
            return CurrentNumberInputRequest.CompletionSource.Task;
        }

        public void CompleteNumberInput(int amount)
        {
            if (CurrentNumberInputRequest != null)
            {
                CurrentNumberInputRequest.CompletionSource.SetResult(new NumberInputResult { Amount = amount });
                CurrentNumberInputRequest = null;
                OnRequestChanged?.Invoke();
            }
        }

        public void CancelNumberInput()
        {
            if (CurrentNumberInputRequest != null)
            {
                CurrentNumberInputRequest.CompletionSource.SetResult(new NumberInputResult { WasCancelled = true });
                CurrentNumberInputRequest = null;
                OnRequestChanged?.Invoke();
            }
        }

        public Task<AlchemicalRecipe?> RequestRecipeCreationAsync()
        {
            CurrentRecipeRequest = new RecipeCreationRequest();
            OnRequestChanged?.Invoke(); // Notify UI to show the modal
            return CurrentRecipeRequest.CompletionSource.Task;
        }

        public void CompleteRecipeCreation(AlchemicalRecipe recipe)
        {
            CurrentRecipeRequest?.CompletionSource.SetResult(recipe);
            CurrentRecipeRequest = null;
            OnRequestChanged?.Invoke(); // Notify UI to hide the modal
        }

        public void CancelRecipeCreation()
        {
            CurrentRecipeRequest?.CompletionSource.SetResult(null);
            CurrentRecipeRequest = null;
            OnRequestChanged?.Invoke(); // Notify UI to hide the modal
        }
    }
}
