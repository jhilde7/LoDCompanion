using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.GameData;

namespace LoDCompanion.Code.BackEnd.Services.Player
{
    // This class holds the information for the spell casting request
    public class SpellCastingRequest
    {
        public required Hero Caster { get; set; }
        public required Spell Spell { get; set; }
        public string Prompt => $"Cast {Spell.Name}";
    }

    // This class holds the result from the user's input
    public class SpellCastingResult
    {
        public int FocusPoints { get; set; }
        public int PowerLevels { get; set; }
        public bool WasCancelled { get; set; } = false;
    }

    // The service to manage the interaction
    public class SpellCastingService
    {
        public event Action? OnCastingRequestChanged;
        public SpellCastingRequest? CurrentDiceRequest { get; private set; }
        private TaskCompletionSource<SpellCastingResult>? _tcs;

        public Task<SpellCastingResult> RequestCastingOptionsAsync(Hero hero, Spell spell)
        {
            CurrentDiceRequest = new SpellCastingRequest { Caster = hero, Spell = spell };
            _tcs = new TaskCompletionSource<SpellCastingResult>();
            OnCastingRequestChanged?.Invoke();
            return _tcs.Task;
        }

        public void CompleteSelection(SpellCastingResult result)
        {
            _tcs?.SetResult(result);
            CurrentDiceRequest = null;
            OnCastingRequestChanged?.Invoke();
        }
    }
}
