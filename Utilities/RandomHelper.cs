// File: Utilities/RandomHelper.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq; // Required for LINQ methods like ToList() in Shuffle

namespace LoDCompanion.Utilities
{
    /// <summary>
    /// Provides utility methods for random number generation and list manipulation.
    /// Replaces Unity's UnityEngine.Random with System.Random.
    /// </summary>
    public static class RandomHelper
    {
        private static readonly int[] DiceSides = { 4, 6, 8, 10, 12, 20, 100 }; // Common dice sides
        private static string[] DiceNames = { "D4", "D6", "D8", "D10", "D12", "D20", "D100" };
        private static readonly Random _random = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            // System.Random.Next(minValue, maxValue) returns a random integer
            // that is greater than or equal to minValue, and less than maxValue.
            return _random.Next(min, max + 1);
        }

        private static int GetDiceSides(string diceName)
        {
            // Find the index of the dice name in the DiceNames array
            int index = Array.IndexOf(DiceNames, diceName);
            if (index < 0 || index >= DiceSides.Length)
            {
                throw new ArgumentException($"Invalid dice name: {diceName}");
            }
            return DiceSides[index];
        }

        public static int RollDie(string die)
        {
            return GetRandomNumber(1, GetDiceSides(die));
        }

        public static T GetRandomEnumValue<T>(int min = 0, int max = 0)
        {
            var v = Enum.GetValues(typeof(T));
            if (max > 0 && max <= v.Length)
            {
                return (T)v.GetValue(GetRandomNumber(min, max));
            }
            return (T)v.GetValue(_random.Next(v.Length));
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