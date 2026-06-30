using System;

namespace Sanctuary
{
    /// <summary>
    /// Represents a location in the save data.
    /// </summary>
    [Serializable]
    public struct SaveLocation 
    {
        /// <summary>
        /// The ID of the chunk.
        /// </summary>
        public string ChunkId;

        /// <summary>
        /// The ID of the object. 
        /// </summary>
        public string ObjectId;

        /// <summary>
        /// Checks if the SaveLocation has been initialized.
        /// </summary>
        /// <remarks>Primarily used by the object location property drawer to determine if it needs to apply location logic.</remarks>
        public bool initialized;

        public SaveLocation(string chunkId, string objectId) 
        {
            ChunkId = chunkId;
            ObjectId = objectId;
            initialized = true;
        }
    }
}
