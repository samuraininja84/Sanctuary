using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Serialization;

namespace Sanctuary.Configuration
{
    [CreateAssetMenu(fileName = "MemoryStreamConfiguration", menuName = "Sanctuary/Configuration/Streams/New Memory Stream Configuration")]
    public sealed class MemoryStreamConfiguration : StreamConfiguration
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
    }
}
