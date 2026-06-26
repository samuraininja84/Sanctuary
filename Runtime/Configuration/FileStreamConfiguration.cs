using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Serialization;

namespace Sanctuary.Configuration
{
    [CreateAssetMenu(fileName = "FileStreamConfiguration", menuName = "Sanctuary/Configuration/File Stream Configuration")]
    public class FileStreamConfiguration : StreamConfiguration
    {
        /// <summary>
        /// Gets a stream based on the specified stream type and file path.
        /// </summary>
        /// <param name="streamType">The type of stream to create.</param>
        /// <param name="filePath">The file path for the stream, if applicable.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created stream.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the provided stream type is not recognized.</exception>
        public override async Task<Stream> GetStream(StreamType streamType, string filePath = null)
        {
            return streamType switch
            {
                StreamType.Serialization => SerializationExtensions.CreateFileSerializationStream(filePath),
                StreamType.Deserialization => await SerializationExtensions.CreateFileDeserializationStream(filePath),

                // Should never reach this point because all enum values are handled above, but this is a safeguard in case of future changes to the enum.
                _ => throw new System.ArgumentOutOfRangeException(nameof(streamType), streamType, null)
            };
        }
    }
}
