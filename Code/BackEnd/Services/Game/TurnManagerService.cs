using LoDCompanion.Code.BackEnd.Models;

namespace LoDCompanion.Code.BackEnd.Services.Game
{
    public class TurnManagerService
    {
        public int TurnNumber { get; private set; }
        public Character? CurrentActor { get; private set; }
        private readonly List<Hero> Heroes;
        private readonly List<Monster> Monsters;
        private readonly List<Character> ActedCharacters = new List<Character>();

        /// <summary>
        /// Initializes a new instance of the TurnManagerService.
        /// </summary>
        /// <param name="heroes">The list of heroes in the combat.</param>
        /// <param name="enemies">The list of enemies in the combat.</param>
        public TurnManagerService(List<Hero> heroes, List<Monster> enemies)
        {
            Heroes = heroes;
            Monsters = enemies;
            StartNewTurn();
        }

        /// <summary>
        /// Starts a new turn, resetting the acted lists.
        /// </summary>
        public void StartNewTurn()
        {
            TurnNumber = TurnNumber == 0 ? 1 : TurnNumber + 1;
            ActedCharacters.Clear();

            // Reset Action Points for all characters
            Heroes.ForEach(h => h.ResetActionPoints());
            Monsters.ForEach(e => e.ResetActionPoints());

            // Heroes act first in a new round
            CurrentActor = Heroes.FirstOrDefault();
        }

        /// <summary>
        /// Ends the turn for the current actor and determines the next actor.
        /// If all characters have acted, a new round begins.
        /// </summary>
        public void EndTurnForCurrentActor()
        {
            if (CurrentActor == null) return;

            ActedCharacters.Add(CurrentActor);
            CurrentActor.CurrentAP = 0; // Ensure AP is 0 when turn is explicitly ended

            // Find the next hero who hasn't acted
            Character? nextActor = Heroes.FirstOrDefault(h => !ActedCharacters.Contains(h));

            // If no heroes are left, find the next enemy
            if (nextActor == null)
            {
                // This is where you would implement the logic for enemy turn order to avoid blocking.
                // For now, we'll just pick the first un-acted enemy.
                nextActor = Monsters.First(e => !ActedCharacters.Contains(e));
            }

            // If everyone has acted, start a new round. Otherwise, set the next actor.
            if (nextActor == null)
            {
                StartNewTurn();
            }
            else
            {
                CurrentActor = nextActor;
            }
        }

        /// <summary>
        /// Handles an enemy interruption. The heroes' turn phase ends immediately,
        /// and the enemies begin their turns.
        /// </summary>
        public void HandleEnemyInterrupt()
        {
            // Mark all heroes as having acted to end their turn phase.
            foreach (var hero in Heroes)
            {
                if (!ActedCharacters.Contains(hero))
                {
                    ActedCharacters.Add(hero);
                }
            }

            // The next actor will be the first enemy in the queue who hasn't acted.
            Character nextEnemy = Monsters.First(e => !ActedCharacters.Contains(e));
            if (nextEnemy != null)
            {
                CurrentActor = nextEnemy;
            }
            else
            {
                // If all enemies have also acted (unlikely in an interrupt), start a new round.
                StartNewTurn();
            }
        }
    }
}
