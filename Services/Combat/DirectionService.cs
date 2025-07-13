using LoDCompanion.Models.Character;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Combat
{
    // Defines the 8 directions relative to a character's facing.
    public enum RelativeDirection { Front, FrontRight, FrontLeft, Right, Left, BackRight, BackLeft, Back }

    public class DirectionService
    {
        /// <summary>
        /// Determines the direction of a target relative to an observer's facing.
        /// </summary>
        /// <returns>The relative direction of the target.</returns>
        public RelativeDirection GetRelativeDirection(FacingDirection observerFacing, GridPosition observerPosition, GridPosition targetPosition)
        {
            int dx = targetPosition.X - observerPosition.X;
            int dy = targetPosition.Y - observerPosition.Y; // Assuming Y+ is North

            // Rotate the coordinate system based on the observer's facing
            switch (observerFacing)
            {
                case FacingDirection.North:
                    break;
                case FacingDirection.South:
                    (dx, dy) = (-dx, -dy);
                    break;
                case FacingDirection.East:
                    (dx, dy) = (dy, -dx);
                    break;
                case FacingDirection.West:
                    (dx, dy) = (-dy, dx);
                    break;
            }

            // Determine relative direction from the rotated coordinates
            if (dx == 0 && dy == 1) return RelativeDirection.Front;
            if (dx == 1 && dy == 1) return RelativeDirection.FrontRight;
            if (dx == -1 && dy == 1) return RelativeDirection.FrontLeft;
            if (dx == 1 && dy == 0) return RelativeDirection.Right;
            if (dx == -1 && dy == 0) return RelativeDirection.Left;
            if (dx == 1 && dy == -1) return RelativeDirection.BackRight;
            if (dx == -1 && dy == -1) return RelativeDirection.BackLeft;
            if (dx == 0 && dy == -1) return RelativeDirection.Back;

            // Default case if not adjacent
            return RelativeDirection.Front;
        }

        /// <summary>
        /// Checks if an attack is coming from behind the target.
        /// </summary>
        public bool IsAttackingFromBehind(Character attacker, Character target)
        {
            var relativeDir = GetRelativeDirection(target.Facing, target.Position, attacker.Position);
            return relativeDir is RelativeDirection.Back or RelativeDirection.BackLeft or RelativeDirection.BackRight;
        }

        /// <summary>
        /// Checks if a position is within a character's Zone of Control.
        /// A model's ZOC includes squares "directly to its side, diagonally in front, and in front of it."
        /// </summary>
        public bool IsInZoneOfControl(GridPosition positionToCheck, Character character)
        {
            var relativeDir = GetRelativeDirection(character.Facing, character.Position, positionToCheck);
            return relativeDir is not (RelativeDirection.Back or RelativeDirection.BackLeft or RelativeDirection.BackRight);
        }
    }
}
