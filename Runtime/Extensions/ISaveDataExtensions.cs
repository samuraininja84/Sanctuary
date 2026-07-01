using System.Collections.Generic;

namespace Sanctuary.Extensions
{
    public static class ISaveDataExtensions
    {
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