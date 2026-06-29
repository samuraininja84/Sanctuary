using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Serialization;

namespace Sanctuary.Configuration
{
    [CreateAssetMenu(fileName = "MemoryStreamConfiguration", menuName = "Sanctuary/Configuration/Streams/New Memory Stream Configuration")]
    public class MemoryStreamConfiguration : StreamConfiguration
    {
        [Header("Memory Stream Settings")]
        [Tooltip("The initial capacity of the memory stream. Default is 0, which means the stream will start with no allocated memory. Adjust this value based on expected data size to optimize performance.")]
        [SerializeField] private int initialCapacity = 0;

        /// <summary>
        /// Gets a memory stream based on the specified stream type. The initial capacity of the stream can be set via the inspector.
        /// </summary>
        /// <param name="streamType">The type of stream to create.</param>
        /// <param name="filePath">This parameter is ignored for memory streams and can be left null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created memory stream.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the provided stream type is not recognized.</exception>
        public override async Task<Stream> GetStream(StreamType streamType, string filePath = null)
        { 
            return streamType switch
            {
                StreamType.Serialization => SerializationExtensions.CreateMemorySerializationStream(initialCapacity),
                StreamType.Deserialization => SerializationExtensions.CreateMemoryDeserializationStream(initialCapacity),
                StreamType.Backup => SerializationExtensions.CreateMemoryBackupStream(initialCapacity),

                // Should never reach this point because all enum values are handled above, but this is a safeguard in case of future changes to the enum.
                _ => throw new System.ArgumentOutOfRangeException(nameof(streamType), streamType, null)
            };
        }

        /// <summary>
        /// Constructs a file path for memory streams. Since memory streams do not have a physical file path, this method returns an empty string.
        /// </summary>
        /// <param name="folderPath">The folder path where the file would be located, if applicable.</param>
        /// <param name="fileName">The name of the file, if applicable.</param>
        /// <param name="extension">The file extension, if applicable.</param>
        /// <returns>An empty string, as memory streams do not have a physical file path.</returns>
        public override string ConstructPath(string folderPath, string fileName, string extension) => string.Empty;
    }
}
