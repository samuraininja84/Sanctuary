using System.Collections.Generic;

namespace Sanctuary
{
    /// <summary>
    /// A common interface for representing the save data.
    /// </summary>
    public interface ISaveData 
    {
        /// <summary>
        /// Public accessor for the save data.
        /// </summary>
        Dictionary<string, Dictionary<string, string>> Data { get; }

        /// <summary>
        /// Adds a new chunk to the save data.
        /// </summary>
        /// <param name="chunkId">The chunk ID.</param>
        /// <param name="objectId">The object ID.</param>
        /// <param name="value">The value to add.</param>
        void AddChunk(string chunkId, string objectId, object value);

        /// <summary>
        /// Copies all data from the given save data into this instance.
        /// </summary>
        /// <param name="data">The save data to copy to from this instance.</param>
        void CopyTo(ISaveData data);

        /// <summary>
        /// Write the given value to the given location.
        /// </summary>
        /// <param name="location">The location to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="name">The optional name to assign to the chunk.</param>
        /// <returns>The updated save data.</returns>
        ISaveData Write(SaveLocation location, object value);

        /// <summary>
        /// Read the save data from the given location.
        /// </summary>
        /// <param name="location">The location to read from.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <returns>A new instance of the data type.</returns>
        T Read<T>(SaveLocation location) where T : new();

        /// <summary>
        /// Tries to read the save data from the given location into the given target.
        /// </summary>
        /// <param name="location">The location to read from.</param>
        /// <param name="target">The target to read into.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <returns>Whether the data was read.</returns>
        bool TryRead<T>(SaveLocation location, T target);

        /// <summary>
        /// Tries to delete the chunk at the given location.
        /// </summary>
        /// <param name="location">The location of the chunk to delete.</param>
        /// <returns>Whether the chunk was successfully deleted.</returns>
        bool TryRemove(SaveLocation location);

        /// <summary>
        /// Tries to delete the object with the given chunk name at the given location.
        /// </summary>
        /// <param name="location">The location representing the chunk to delete.</param>
        /// <param name="chunkName">The name of the chunk to delete.</param>
        /// <returns>Whether the chunk was successfully deleted.</returns>
        bool TryRemove(SaveLocation location, string chunkName);

        /// <summary>
        /// Tries to remove a chunk from the save data by its name.
        /// </summary>
        /// <param name="chunkName">The name of the chunk to remove.</param>
        /// <returns>A boolean indicating whether the chunk was successfully removed.</returns>
        public bool TryRemove(string chunkName);

        /// <summary>
        /// Get the chunk with the given ID or create a new one if it doesn't exist.
        /// </summary>
        /// <param name="chunkId">The chunk ID.</param>
        /// <returns>The retrieved chunk.</returns>
        Dictionary<string, string> GetChunk(string chunkId);

        /// <summary>
        /// Get the IDs of all existing chunks.
        /// </summary>
        IEnumerable<string> GetChunkIDs();

        /// <summary>
        /// Sets the name of a chunk in the save data.
        /// </summary>
        /// <param name="location">A location representing the chunk to name.</param>
        /// <param name="name">The name to assign to the chunk. If null, the default name is used.</param>
        /// <remarks>If no name is provided, the default name is used.</remarks>
        /// <returns>A reference to the save data for chaining.</returns>
        ISaveData SetChunkName(SaveLocation location, string name);

        /// <summary>
        /// Sets the name of a chunk in the save data.
        /// </summary>
        /// <param name="objectId">The object ID representing the chunk to name.</param>
        /// <param name="name">The name to assign to the chunk. If null, the default name is used.</param>
        /// <remarks>If no name is provided, the default name is used.</remarks>
        /// <returns>A reference to the save data for chaining.</returns>
        public ISaveData SetChunkName(string objectId, string name);

        /// <summary>
        /// Gets the name of a chunk by its ID.
        /// </summary>
        /// <param name="chunkId">The ID of the chunk.</param>
        /// <returns>The name of the chunk.</returns>
        string GetChunkName(string chunkId);

        /// <summary>
        /// Checks if a chunk exists at the given location.
        /// </summary>
        /// <param name="location">The location of the chunk to check.</param>
        /// <returns>A boolean indicating whether the chunk exists.</returns>
        bool HasChunk(SaveLocation location);

        /// <summary>
        /// Checks if a chunk exists with the given ID.
        /// </summary>
        /// <param name="chunkName">The ID of the chunk to check.</param>
        /// <returns>A boolean indicating whether the chunk exists.</returns>
        bool HasChunk(string chunkName);
    }
}
