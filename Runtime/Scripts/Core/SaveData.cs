using System.Collections.Generic;
using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// A basic implementation of <see cref="ISaveData"/> that uses JSON
    /// serialization.
    /// </summary>
    public class SaveData : ISaveData
    {
        /// <summary>
        /// The default chunk ID used when no chunk ID is provided.
        /// </summary>
        private const string _defaultChunkId = "Default";

        /// <summary>
        /// The actual save data, organized by chunk ID and object ID.
        /// </summary>
        protected readonly Dictionary<string, Dictionary<string, string>> _data = new();

        /// <summary>
        /// The information about each chunk in the save data, organized by chunk ID to output a name for each chunk.
        /// </summary>
        protected readonly Dictionary<string, string> _chunkInformation = new();

        /// <summary>
        /// Public accessor for the save data.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Data { get => _data; }

        /// <summary>
        /// Adds a new chunk to the save data.
        /// </summary>
        /// <param name="chunkId">The chunk ID.</param>
        /// <param name="objectId">The object ID.</param>
        /// <param name="value">The value to add.</param>
        public void AddChunk(string chunkId, string objectId, object value)
        {
            // Use the default chunk if no chunk ID is provided
            var realChunkId = chunkId ?? _defaultChunkId;

            // If the chunk doesn't exist, create an empty one
            if (!_data.ContainsKey(realChunkId)) _data[realChunkId] = new Dictionary<string, string>();

            // Write the data to the chunk
            _data[realChunkId][objectId] = JsonUtility.ToJson(value);
        }

        /// <summary>
        /// Copies all data from the given save data into this instance.
        /// </summary>
        /// <param name="data">The save data to copy to from this instance.</param>
        public void CopyTo(ISaveData data)
        {
            // Combine the data from the provided ISaveData instance
            foreach (var chunk in _data)
            {
                // If the chunk doesn't exist in the target data, create it
                if (!data.Data.ContainsKey(chunk.Key)) data.Data[chunk.Key] = new Dictionary<string, string>();

                // Copy each object in the chunk
                foreach (var obj in chunk.Value) data.Data[chunk.Key].Add(obj.Key, obj.Value);
            }

            // Copy the chunk information from the provided ISaveData instance
            foreach (var chunkInfo in _chunkInformation) data.SetChunkName(chunkInfo.Key, chunkInfo.Value);
        }

        /// <summary>
        /// Writes an object to the save data.
        /// </summary>
        /// <param name="location">The location to write the object to.</param>
        /// <param name="value">The object to write to the save data.</param>
        /// <param name="name">The optional name to assign to the chunk.</param>
        /// <returns>The updated save data.</returns>
        public ISaveData Write(SaveLocation location, object value)
        {
#if UNITY_EDITOR
            // Set a breakpoint here to catch unexpected prefabs being saved, which can lead to data corruption
            if (location.ChunkId == "00000000000000000000000000000000" || location.ChunkId == "0")
            {
                // Log a warning to the console
                Debug.LogFormat("[Safekeeper]: Unexpected prefab ({0})", location.ObjectId);

                // Return early to avoid saving
                return this;
            }
#endif
            // Add the chunk to the save data
            AddChunk(location.ChunkId, location.ObjectId, value);

            // Return the save data for chaining
            return this;
        }

        /// <summary>
        /// Reads an object from the save data.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="location">The location of the object to read.</param>
        /// <returns>An instance of the object read from the save data, or a new instance of T if the object does not exist.</returns>
        public T Read<T>(SaveLocation location) where T : new()
        {
            // Use the default chunk if no chunk ID is provided
            var realChunkId = location.ChunkId ?? _defaultChunkId;

            // If the chunk or object doesn't exist, return a new instance of T
            return _data.ContainsKey(realChunkId) && _data[realChunkId].ContainsKey(location.ObjectId) ? JsonUtility.FromJson<T>(_data[realChunkId][location.ObjectId]) : new T();
        }

        /// <summary>
        /// Tries to read an object from the save data and overwrites the provided target object with the data.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="location">The location of the object to read.</param>
        /// <param name="target">The object to overwrite with the data read from the save data.</param>
        /// <returns>A boolean indicating whether the read was successful.</returns>
        public bool TryRead<T>(SaveLocation location, T target)
        {
            // Use the default chunk if no chunk ID is provided
            var realChunkId = location.ChunkId ?? _defaultChunkId;

            // If the chunk or object doesn't exist, return false
            if (location.ObjectId == null || !_data.ContainsKey(realChunkId) || !_data[realChunkId].ContainsKey(location.ObjectId)) return false;

            // Overwrite the target object with the data from the save
            JsonUtility.FromJsonOverwrite(_data[realChunkId][location.ObjectId], target);

            // Indicate that the read was successful
            return true;
        }

        /// <summary>
        /// Tries to remove an object from the save data.
        /// </summary>
        /// <param name="location">The location of the object to remove.</param>
        /// <returns>Whether the object was successfully removed.</returns>
        public bool TryRemove(SaveLocation location)
        {
            // Use the default chunk if no chunk ID is provided
            var realChunkId = location.ChunkId ?? _defaultChunkId;

            // If the chunk exists, try to remove the object from it
            if (_data.ContainsKey(realChunkId)) return _data[realChunkId].Remove(location.ObjectId);

            // Indicate that the removal was unsuccessful
            return false;
        }

        /// <summary>
        /// Tries to remove a chunk from the save data by its name.
        /// </summary>
        /// <param name="location">The location representing the chunk to remove.</param>
        /// <param name="chunkName">The name of the chunk to remove.</param>
        /// <returns>Whether the chunk was successfully removed.</returns>
        public bool TryRemove(SaveLocation location, string chunkName)
        {
            // If there is no chunk by the given ID, indicate that the removal was unsuccessful
            if (!HasChunk(chunkName)) return false;

            // Indicate that the removal was unsuccessful
            return TryRemove(location);
        }

        /// <summary>
        /// Tries to remove a chunk from the save data by its name.
        /// </summary>
        /// <param name="chunkName">The name of the chunk to remove.</param>
        /// <returns>A boolean indicating whether the chunk was successfully removed.</returns>
        public bool TryRemove(string chunkName)
        {
            // If there is no chunk by the given name, indicate that the removal was unsuccessful
            if (!HasChunk(chunkName)) return false;

            // Find the chunk ID associated with the given name
            foreach (var chunkInfo in _chunkInformation)
            {
                // If the chunk name matches, remove it
                if (chunkInfo.Value == chunkName)
                {
                    // Remove the chunk from the data and chunk information
                    var removedFromData = _data.Remove(chunkInfo.Key);
                    var removedFromInfo = _chunkInformation.Remove(chunkInfo.Key);

                    // Indicate whether both removals were successful
                    return removedFromData && removedFromInfo;
                }
            }

            // Indicate that the removal was unsuccessful
            return false;
        }

        /// <summary>
        /// Gets a chunk of data by its ID.
        /// </summary>
        /// <param name="chunkId">The ID of the chunk to get. If null, the default chunk is used.</param>
        /// <returns>A dictionary representing the chunk of data.</returns>
        public Dictionary<string, string> GetChunk(string chunkId = null)
        {
            // Use the default chunk if no chunk ID is provided
            var realChunkId = chunkId ?? _defaultChunkId;

            // If the chunk doesn't exist, create an empty one
            if (!_data.ContainsKey(realChunkId)) _data[realChunkId] = new Dictionary<string, string>();

            // Return the chunk
            return _data[realChunkId];
        }

        /// <summary>
        /// Gets the IDs of all existing chunks.
        /// </summary>
        /// <returns>The IDs of the existing chunks.</returns>
        public IEnumerable<string> GetChunkIDs() => _data.Keys;

        /// <summary>
        /// Sets the name of a chunk in the save data.
        /// </summary>
        /// <param name="location">A location representing the chunk to name.</param>
        /// <param name="name">The name to assign to the chunk. If null, the default name is used.</param>
        /// <remarks>If no name is provided, the default name is used.</remarks>
        /// <returns>A reference to the save data for chaining.</returns>
        public ISaveData SetChunkName(SaveLocation location, string name = _defaultChunkId) => SetChunkName(location.ObjectId, name);

        /// <summary>
        /// Sets the name of a chunk in the save data.
        /// </summary>
        /// <param name="objectId">The object ID representing the chunk to name.</param>
        /// <param name="name">The name to assign to the chunk. If null, the default name is used.</param>
        /// <remarks>If no name is provided, the default name is used.</remarks>
        /// <returns>A reference to the save data for chaining.</returns>
        public ISaveData SetChunkName(string objectId, string name = _defaultChunkId)
        {
            // Set the chunk name information with the provided name or default if null
            if (!_chunkInformation.ContainsKey(objectId))
            {
                // Add new chunk information
                _chunkInformation.Add(objectId, name);
            }
            else
            {
                // Update existing chunk information
                _chunkInformation[objectId] = name;
            }

            // Return the save data for chaining
            return this;
        }

        /// <summary>
        /// Gets the name of a chunk by its ID.
        /// </summary>
        /// <remarks>If no name is set for the chunk, a default name is returned.</remarks>
        /// <param name="chunkId">The ID of the chunk.</param>
        /// <returns>The name of the chunk.</returns>
        public string GetChunkName(string chunkId)
        {
            // Try to get the name from the chunk information
            if (_chunkInformation.TryGetValue(chunkId, out var name)) return name;

            // Fallback to a default name if no name is set
            return _defaultChunkId;
        }

        /// <summary>
        /// Checks if a chunk exists in the save data.
        /// </summary>
        /// <param name="location">The location of the chunk to check.</param>
        /// <returns>A boolean indicating whether the chunk exists.</returns>
        public bool HasChunk(SaveLocation location)
        {
            // Use the default chunk if no chunk ID is provided
            var realChunkId = location.ChunkId ?? _defaultChunkId;

            // Check if the chunk and object exist
            return _data.ContainsKey(realChunkId) && _data[realChunkId].ContainsKey(location.ObjectId);
        }

        /// <summary>
        /// Checks if a chunk exists in the save data by its name.
        /// </summary>
        /// <param name="chunkName">The name of the chunk to check.</param>
        /// <returns>A boolean indicating whether the chunk exists.</returns>
        public bool HasChunk(string chunkName) => _chunkInformation.ContainsValue(chunkName);
    }
}
