namespace LoDCompanion.Code.BackEnd.Services.Utilities
{
    public enum DiceType
    {
        D2,
        D3,
        D4,
        D6,
        D8,
        D10,
        D12,
        D20,
        D100
    }
    /// <summary>
    /// Provides utility methods for random number generation and list manipulation.
    /// Replaces Unity's UnityEngine.Random with System.Random.
    /// </summary>
    public static class RandomHelper
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Gets the integer number of sides for a given DiceType enum.
        /// </summary>
        /// <param name="diceType">The enum value of the die.</param>
        /// <returns>The number of sides as an integer.</returns>
        public static int GetSides(DiceType diceType)
        {
            return diceType switch
            {
                DiceType.D4 => 4,
                DiceType.D6 => 6,
                DiceType.D8 => 8,
                DiceType.D10 => 10,
                DiceType.D12 => 12,
                DiceType.D20 => 20,
                DiceType.D100 => 100,
                _ => 0, // Default case, should not be reached
            };
        }

        public static int GetRandomNumber(int min, int max)
        {
            // System.Random.Next(minValue, maxValue) returns a random integer
            // that is greater than or equal to minValue, and less than maxValue.
            return _random.Next(min, max + 1);
        }

        public static int RollDie(DiceType die)
        {
            if (die == DiceType.D2)
            {
                // D2 is not a standard die, but we can simulate it as a 1-2 range.
                return GetRandomNumber(1, 2);
            }
            else if (die == DiceType.D3)
            {
                // D3 is not a standard die, but we can simulate it as a 1-3 range.
                return GetRandomNumber(1, 3);
            }
            return GetRandomNumber(1, GetSides(die));
        }

        public static int RollDice(string dice)
        {
            string[] diceParts = dice.ToLower().Split('d');
            if (diceParts.Length != 2 || !int.TryParse(diceParts[0], out int numberOfDice) || !int.TryParse(diceParts[1], out int diceSides))
            {
                return 0;
            }

            DiceType diceType;
            try
            {
                diceType = (DiceType)Enum.Parse(typeof(DiceType), "D" + diceSides);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Invalid dice type specified: d{diceSides}");
                return 0;
            }

            int totalRoll = 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                totalRoll += RollDie(diceType);
            }

            return totalRoll;
        }

        public static T GetRandomEnumValue<T>(int min = 0, int max = 0)
        {
            var v = Enum.GetValues(typeof(T));
            if (v == null || v.Length == 0)
            {
                throw new InvalidOperationException($"Enum type {typeof(T)} has no values.");
            }

            if (max > 0 && max <= v.Length)
            {
                int index = GetRandomNumber(min, max);
                object? value = v.GetValue(index);
                if (value == null)
                {
                    throw new InvalidOperationException($"Enum value at index {index} is null.");
                }
                return (T)value;
            }

            object? randomValue = v.GetValue(_random.Next(v.Length));
            if (randomValue == null)
            {
                throw new InvalidOperationException("Randomly selected enum value is null.");
            }
            return (T)randomValue;
        }
    }

    /// <summary>
    /// Provides extension methods for IList<T>.
    /// </summary>
    public static class IListExtensions
    {
        private static readonly Random _shuffleRandom = new Random(); // Separate Random instance if Shuffle needs different seeding/behavior

        /// <summary>
        /// Shuffles the elements of an IList<T> randomly.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _shuffleRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Inserts elements from a source list into a target list at random positions within a specified range.
        /// </summary>
        /// <typeparam name="T">The type of elements in the lists.</typeparam>
        /// <param name="targetList">The list to insert elements into.</param>
        /// <param name="sourceList">The list of elements to insert.</param>
        /// <param name="startIndex">The starting index (inclusive) of the range where elements can be inserted.</param>
        /// <param name="endIndex">The ending index (inclusive) of the range where elements can be inserted.</param>
        public static void InsertRandomRange<T>(this IList<T> targetList, IList<T> sourceList, int startIndex, int endIndex)
        {
            // Ensure the source list is not empty
            if (sourceList == null || sourceList.Count == 0)
            {
                return;
            }

            // Ensure valid indices for the target list
            if (startIndex < 0) startIndex = 0;
            if (endIndex >= targetList.Count) endIndex = targetList.Count - 1;
            if (startIndex > endIndex) startIndex = endIndex; // Handle case where range is invalid

            // Create a copy of the source list to avoid issues if it's the same as targetList or if it's modified during iteration
            List<T> sourceCopy = sourceList.ToList();
            sourceCopy.Shuffle(); // Optional: Shuffle the source elements before inserting

            foreach (T item in sourceCopy)
            {
                if (targetList.Count == 0)
                {
                    targetList.Add(item);
                }
                else
                {
                    // Calculate a random insertion point within the specified range
                    // If the range is empty (startIndex == endIndex), it will insert at that point.
                    // If targetList is empty initially and then gets items, startIndex/endIndex might need adjustment.
                    // For simplicity, let's ensure insertion is within current bounds of targetList.
                    int actualStartIndex = Math.Max(0, startIndex);
                    int actualEndIndex = Math.Min(targetList.Count, endIndex + 1); // +1 because Insert expects exclusive end

                    if (actualStartIndex > actualEndIndex) actualStartIndex = actualEndIndex; // Adjust if bounds cross

                    int insertIndex = _shuffleRandom.Next(actualStartIndex, actualEndIndex);
                    targetList.Insert(insertIndex, item);
                }
            }
        }
    }
}