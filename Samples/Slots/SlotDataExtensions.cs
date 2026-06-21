using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sanctuary.Samples
{
    /// <summary>
    /// Provides utility methods and constants for working with slot-related data, including time formatting and conversions.
    /// </summary>
    /// <remarks>
    /// This class includes methods for converting binary timestamps to formatted strings,
    /// formatting elapsed time, and a predefined constant for a user-friendly date and time format. 
    /// It is designed to simplify common operations related to time representation in slot data.
    /// </remarks>
    public static class SlotDataExtensions
    {
        /// <summary>
        /// Represents the date and time format used to display when an operation started.
        /// </summary>
        /// <remarks>
        /// The format follows the pattern "MMM dd, yyyy - hh:mm tt", where: 
        /// <list type="bullet">
        /// <item><description><c>MMM</c>: Abbreviated month name (e.g., Jan, Feb).</description></item>
        /// <item><description><c>dd</c>: Day of the month (01-31).</description></item> 
        /// <item><description><c>yyyy</c>: Four-digit year.
        /// </description></item> <item><description><c>hh</c>: Hour in 12-hour format(01-12).</description></item> 
        /// <item><description><c>mm</c>: Minutes (00-59).</description></item>
        /// <item><description><c>tt</c>: AM/PM designator.</description></item> 
        /// </list> 
        /// This format is commonly used for user-friendly date and time displays.</remarks>
        public const string TimeStampFormat = "MMM dd, yyyy - hh:mm tt";

        /// <summary>
        /// Copies the values of all fields from a source <see cref="SlotData"/> instance to a target <see cref="SlotData"/> instance.
        /// </summary>
        /// <param name="target">The target <see cref="SlotData"/> instance to copy values to.</param>
        /// <param name="source">The source <see cref="SlotData"/> instance to copy values from.</param>
        public static void CopyFrom(this SlotData target, SlotData source)
        {
            target.name = source.name;
            target.timeStarted = source.timeStarted;
            target.timeSpent = source.timeSpent;
            target.lastOpened = source.lastOpened;
            target.completion = source.completion;
        }

        /// <summary>
        /// Gets the most recently updated slot from a list of <see cref="SlotData"/> instances based on the latest <c>timeStarted</c> value.
        /// </summary>
        /// <param name="slots">The list of slots to search through.</param>
        /// <returns>The most recently updated slot, or <c>null</c> if the list is empty.</returns>
        public static SlotData GetLastUpdated(this Dictionary<int, SlotData> slots)
        {
            // Initialize a variable to keep track of the most recently updated slot
            SlotData lastUpdated = null;

            // Iterate through the list of slots to find the one with the most recent timeStarted value
            foreach (var slot in slots) if (lastUpdated == null || slot.Value.timeStarted > lastUpdated.timeStarted) lastUpdated = slot.Value;

            // If no slots are found, return null
            return lastUpdated;
        }

        /// <summary>
        /// Gets the highest index value from a list of <see cref="SlotData"/> instances.
        /// </summary>
        /// <param name="slots">The dictionary of slots to search through.</param>
        /// <returns>The highest index value found, or -1 if the dictionary is empty.</returns>
        public static int GetHighestIndex(this Dictionary<int, SlotData> slots)
        {
            // Initialize a variable to keep track of the highest index found
            int highestIndex = -1;

            // Iterate through the list of slots to find the highest index value
            foreach (var slot in slots) if (slot.Key > highestIndex) highestIndex = slot.Key;

            // Return the highest index found, or -1 if the list is empty
            return highestIndex;
        }

        /// <summary>
        /// Checks if a slot with the specified index exists within the provided list of <see cref="SlotData"/> instances.
        /// </summary>
        /// <param name="slots">The list of slots to search through.</param>
        /// <param name="index">The index to check for.</param>
        /// <returns><c>true</c> if a slot with the specified index exists; otherwise, <c>false</c>.</returns>
        public static bool HasSlotAtIndex(this Dictionary<int, SlotData> slots, int index) => slots.ContainsKey(index);

        /// <summary>
        /// Converts a binary timestamp to a formatted string representation of the start time.
        /// </summary>
        /// <param name="time">The binary timestamp representing the start time, typically obtained using <see cref="DateTime.ToBinary"/>.</param>
        /// <returns>
        /// A string representation of the start time, formatted according to <see cref="TimeStampFormat"/>, with "AM" and "PM" replaced by "am" and "pm" for consistency.
        /// </returns>
        public static string ToTimeStamp(this long time) => DateTime.FromBinary(time).ToString(TimeStampFormat).Replace("AM", "am").Replace("PM", "pm");

        /// <summary>
        /// Converts the given time in seconds to a formatted string representing hours, minutes, and seconds.
        /// </summary>
        /// <remarks>This method is useful for displaying elapsed time in a human-readable format.</remarks>
        /// <param name="timeSpent">The time spent, in seconds. Must be a non-negative value.</param>
        /// <returns>A string formatted as "HH:mm:ss", where "HH" is the number of hours, "mm" is the number of minutes, and "ss" is the number of seconds.</returns>
        public static string GetTimeSpent(this float timeSpent)
        {
            // Convert the time spent to a TimeSpan
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeSpent);

            // Format the TimeSpan as a string
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        /// <summary>
        /// Rounds the specified floating-point value to the given number of decimal places.
        /// </summary>
        /// <remarks>
        /// This method uses a scaling factor to round the value to the specified precision. 
        /// It is particularly useful for formatting or limiting the precision of floating-point numbers.
        /// </remarks>
        /// <param name="value">The floating-point value to round.</param>
        /// <param name="decimalPlaces">The number of decimal places to round to. Must be non-negative. Defaults to 2 if not specified.</param>
        /// <returns>The rounded floating-point value with the specified number of decimal places.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="decimalPlaces"/> is negative.</exception>
        public static float SetDecimalPrecision(this float value, int decimalPlaces = 2)
        {
            // Throw an exception if decimalPlaces is negative
            if (decimalPlaces < 0) throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places must be non-negative.");

            // Calculate the scale factor and round the value to the specified number of decimal places
            float scale = Mathf.Pow(10f, decimalPlaces);

            // Round the value to the specified number of decimal places
            return Mathf.Round(value * scale) / scale;
        }
    }
}