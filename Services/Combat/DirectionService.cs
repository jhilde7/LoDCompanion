using LoDCompanion.Models.Character;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Combat
{
    // Defines the 8 directions relative to a character's facing.
    public enum RelativeDirection { Front, FrontRight, FrontLeft, Right, Left, BackRight, BackLeft, Back }

    public static class DirectionService
    {
        /// <summary>
        /// Determines the direction of a target relative to an observer's facing.
        /// </summary>
        /// <returns>The relative direction of the target.</returns>
        public static RelativeDirection GetRelativeDirection(FacingDirection observerFacing, GridPosition observerPosition, GridPosition targetPosition)
        {
            int dx = targetPosition.X - observerPosition.X;
            int dy = targetPosition.Y - observerPosition.Y; // Assuming Y+ is North

            // Rotate the coordinate system based on the observer's facing
            switch (observerFacing)
            {
                case FacingDirection.North:
                    break;
                case FacingDirection.South:
                    dx = -dx; dy = -dy;
                    break;
                case FacingDirection.East:
                    int tempDxE = dx;
                    dx = dy;
                    dy = -tempDxE;
                    break;
                case FacingDirection.West:
                    int tempDxW = dx;
                    dx = -dy;
                    dy = tempDxW;
                    break;
            }

            // Check adjacent squares first
            /*if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1)
            {
                if (dx == 0 && dy == -1) return RelativeDirection.Front;
                if (dx == 1 && dy == -1) return RelativeDirection.FrontRight;
                if (dx == -1 && dy == -1) return RelativeDirection.FrontLeft;
                if (dx == 1 && dy == 0) return RelativeDirection.Right;
                if (dx == -1 && dy == 0) return RelativeDirection.Left;
                if (dx == 1 && dy == 1) return RelativeDirection.BackRight;
                if (dx == -1 && dy == 1) return RelativeDirection.BackLeft;
                if (dx == 0 && dy == 1) return RelativeDirection.Back;
            }

            // Check non-adjacent squares using the sign of dx/dy
            int sdx = Math.Sign(dx);
            int sdy = Math.Sign(dy);

            if (sdx == 0 && sdy == -1) return RelativeDirection.Front;
            if (sdx == 1 && sdy == -1) return RelativeDirection.FrontRight;
            if (sdx == -1 && sdy == -1) return RelativeDirection.FrontLeft;
            if (sdx == 1 && sdy == 0) return RelativeDirection.Right;
            if (sdx == -1 && sdy == 0) return RelativeDirection.Left;
            if (sdx == 1 && sdy == 1) return RelativeDirection.BackRight;
            if (sdx == -1 && sdy == 1) return RelativeDirection.BackLeft;
            if (sdx == 0 && sdy == 1) return RelativeDirection.Back;*/
            // Use Math.Atan2 to get a precise angle, which works for any distance.
            // We adjust the angle because in a Y-down system, "Front" is at -90 degrees.
            double angle = Math.Atan2(dy, dx) * (180 / Math.PI);

            if (angle > -112.5 && angle <= -67.5) return RelativeDirection.Front;
            if (angle > -67.5 && angle <= -22.5) return RelativeDirection.FrontRight;
            if (angle > -22.5 && angle <= 22.5) return RelativeDirection.Right;
            if (angle > 22.5 && angle <= 67.5) return RelativeDirection.BackRight;
            if (angle > 67.5 && angle <= 112.5) return RelativeDirection.Back;
            if (angle > 112.5 && angle <= 157.5) return RelativeDirection.BackLeft;
            if (angle > 157.5 || angle <= -157.5) return RelativeDirection.Left;
            if (angle > -157.5 && angle <= -112.5) return RelativeDirection.FrontLeft;

            return RelativeDirection.Front; // Default case
        }

        public static FacingDirection GetOpposite(FacingDirection facing)
        {
            return facing switch
            {
                FacingDirection.North => FacingDirection.South,
                FacingDirection.South => FacingDirection.North,
                FacingDirection.East => FacingDirection.West,
                FacingDirection.West => FacingDirection.East,
                _ => facing
            };
        }

        /// <summary>
        /// Checks if an attack is coming from behind the target.
        /// </summary>
        public static bool IsAttackingFromBehind(Character attacker, Character target)
        {
            var relativeDir = GetRelativeDirection(target.Facing, target.Position, attacker.Position);
            return relativeDir is RelativeDirection.Back or RelativeDirection.BackLeft or RelativeDirection.BackRight;
        }

        /// <summary>
        /// Checks if a position is within a character's Zone of Control.
        /// A model's ZOC includes squares "directly to its side, diagonally in front, and in front of it."
        /// </summary>
        public static bool IsInZoneOfControl(GridPosition positionToCheck, Character character)
        {
            // A character's ZOC only extends to adjacent squares.
            // We check this first to ensure we are only evaluating the immediate area.
            if (GridService.GetDistance(character.Position, positionToCheck) > 1)
            {
                return false;
            }

            // If the square is adjacent, we then check its direction relative to the character's facing.
            var relativeDir = GetRelativeDirection(character.Facing, character.Position, positionToCheck);

            // The ZOC includes all directions except for the three squares to the character's back.
            return relativeDir is not (RelativeDirection.Back or RelativeDirection.BackLeft or RelativeDirection.BackRight);
        }
    }
}
