using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Sanctuary.Configuration
{
    /// <summary>
    /// Represents a configuration for creating streams used in serialization and deserialization processes.
    /// </summary>
    public abstract class StreamConfiguration : ScriptableObject
    {
        /// <summary>
        /// Gets a stream based on the specified stream type and optional file path.
        /// </summary>
        /// <param name="streamType">The type of stream to create.</param>
        /// <param name="filePath">The file path for the stream, if applicable.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created stream.</returns>
        public abstract Task<Stream> GetStream(StreamType streamType, string filePath = null);

        /// <summary>
        /// Defines the types of streams that can be created by the StreamConfiguration.
        /// </summary>
        public enum StreamType
        {
            /// <summary>
            /// Indicates that the stream is intended for serialization purposes.
            /// </summary>
            /// <remarks>This type of stream is used to write data to a destination, such as a file or network, in a format that can be later read and reconstructed.</remarks>
            Serialization,

            /// <summary>
            /// Indicates that the stream is intended for deserialization purposes.
            /// </summary>
            /// <remarks>This type of stream is used to read data from a source, such as a file or network, and convert it into usable objects or data structures.</remarks>
            Deserialization
        }
    }
}
