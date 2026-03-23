using System.Collections.Generic;

namespace Sanctuary
{
    public static class ISaveDataExtensions
    {
        /// <summary>
        /// Reads all save data chunks into a dictionary of target objects, using an index as the key.
        /// </summary>
        /// <typeparam name="T">The type of target objects to read into.</typeparam>
        /// <param name="targets">The dictionary to populate with loaded data.</param>
        /// <param name="saveData">The collection of save data chunks to read from.</param>
        /// <param name="location">The location to read from.</param>
        /// <returns></returns>
        public static SerializableDictionary<int, T> ReadAllTo<T>(this SerializableDictionary<int, T> targets, IEnumerable<ISaveData> saveData, SaveLocation location) where T : new()
        {
            // Clear existing data
            targets.Clear();

            // Update the location to include the chunk ID placeholder
            int index = -1;

            // Iterate through each loaded chunk
            foreach (ISaveData chunk in saveData)
            {
                // Create a new SlotData instance to hold the chunk data
                T slotData = new();

                // Increment the index
                index++;

                // Try to read each chunk into a SlotData instance, and add it to the dictionary if successful
                if (chunk.TryRead(location, slotData)) targets.Add(index, slotData);
            }

            // Return the dictionary of all loaded data
            return targets;
        }

        /// <summary>
        /// Reads all save data chunks into a list of target objects.
        /// </summary>
        /// <typeparam name="T">The type of target objects to read into.</typeparam>
        /// <param name="targets">The list to populate with loaded data.</param>
        /// <param name="saveData">The collection of save data chunks to read from.</param>
        /// <param name="location">The location to read from.</param>
        /// <returns>A collection of all loaded target objects.</returns>
        public static IEnumerable<T> ReadAllTo<T>(this List<T> targets, IEnumerable<ISaveData> saveData, SaveLocation location) where T : new()
        {
            // Clear existing data
            targets.Clear();

            // Iterate through each loaded chunk
            foreach (ISaveData chunk in saveData)
            {
                // Create a new SlotData instance to hold the chunk data
                T slotData = new();

                // Try to read each chunk into a SlotData instance, and add it to the list if successful
                if (chunk.TryRead(location, slotData)) targets.Add(slotData);
            }

            // Return the list of all loaded data
            return targets;
        }

        /// <summary>
        /// Combines multiple ISaveData instances into a single composite ISaveData.
        /// </summary>
        /// <param name="composite">The composite ISaveData to combine into.</param>
        /// <param name="saveData">The collection of ISa<T1, T1>veData instances to combine.</param>
        /// <returns>A composite ISaveData containing all combined data.</returns>
        public static ISaveData Combine(this ISaveData composite, IEnumerable<ISaveData> saveData)
        {
            // Iterate through each chunk and add it to the composite save data
            foreach (ISaveData data in saveData) data.CopyTo(composite);

            // Return the composite save data
            return composite;
        }
    }
}
