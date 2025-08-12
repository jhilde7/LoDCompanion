using LoDCompanion.BackEnd.Models;

namespace LoDCompanion.BackEnd.Services.Combat
{
    public class FacingDirectionRequest
    {
        public required Hero Hero { get; set; }
        public string Prompt => $"Choose facing for {Hero.Name}";
    }

    public class FacingDirectionService
    {
        public event Action? OnFacingRequestChanged;
        public FacingDirectionRequest? CurrentDiceRequest { get; private set; }
        private TaskCompletionSource<FacingDirection>? _tcs;

        public Task<FacingDirection> RequestFacingDirectionAsync(Hero hero)
        {
            CurrentDiceRequest = new FacingDirectionRequest { Hero = hero };
            _tcs = new TaskCompletionSource<FacingDirection>();

            OnFacingRequestChanged?.Invoke();

            return _tcs.Task;
        }

        public void CompleteSelection(FacingDirection direction)
        {
            _tcs?.SetResult(direction);
            CurrentDiceRequest = null;
            OnFacingRequestChanged?.Invoke();
        }
    }
}
