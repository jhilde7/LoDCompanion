using LoDCompanion.Models.Character;

namespace LoDCompanion.Services.Combat
{
    public class FacingDirectionRequest
    {
        public required Hero Hero { get; set; }
        public string Prompt => $"Choose facing for {Hero.Name}";
    }

    public class FacingDirectionService
    {
        public event Action? OnFacingRequestChanged;
        public FacingDirectionRequest? CurrentRequest { get; private set; }
        private TaskCompletionSource<FacingDirection>? _tcs;

        public Task<FacingDirection> RequestFacingDirectionAsync(Hero hero)
        {
            CurrentRequest = new FacingDirectionRequest { Hero = hero };
            _tcs = new TaskCompletionSource<FacingDirection>();

            OnFacingRequestChanged?.Invoke();

            return _tcs.Task;
        }

        public void CompleteSelection(FacingDirection direction)
        {
            _tcs?.SetResult(direction);
            CurrentRequest = null;
            OnFacingRequestChanged?.Invoke();
        }
    }
}
