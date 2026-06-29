using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Serialization;
using Array = System.Array;

namespace Sanctuary.Configuration
{
    [CreateAssetMenu(fileName = "FileStreamConfiguration", menuName = "Sanctuary/Configuration/Streams/New File Stream Configuration")]
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
                StreamType.Backup => await SerializationExtensions.CreateFileBackupStream(filePath, backupExtension),

                // Should never reach this point because all enum values are handled above, but this is a safeguard in case of future changes to the enum.
                _ => throw new System.ArgumentOutOfRangeException(nameof(streamType), streamType, null)
            };
        }

        /// <summary>
        /// Gets all streams from the specified folder name. If no folder name is provided, it defaults to the configured save folder.
        /// </summary>
        /// <param name="folderName">The name of the folder to get streams from. If null or empty, the default save folder is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of streams from the specified folder.</returns>
        public override async Task<Stream[]> GetStreams(string folderName = null)
        {
            // If a specific directory path is provided, use it instead of the default save directory.
            string folder = string.IsNullOrEmpty(folderName) ? saveFolder : folderName;

            // Construct the path to the existing saves directory.
            string path = Path.Combine(Application.persistentDataPath, folder);

            // Ensure the directory exists.
            if (!Directory.Exists(path))
            {
                // Log a warning if the directory does not exist and return an empty array of streams.
                Debug.LogWarning($"Directory '{path}' does not exist. Returning an empty array of streams.");

                // Return an empty array of streams if the directory does not exist.
                return Array.Empty<Stream>();
            }

            // Create a list to hold the streams corresponding to existing saves.
            var streams = new System.Collections.Generic.List<Stream>();

            // Get all folders in the saves directory
            var savesDirectory = new DirectoryInfo(path);

            // Iterate through each directory in the saves directory
            foreach (var dir in savesDirectory.GetDirectories())
            {
                // Try to parse the directory name as an integer ID
                if (int.TryParse(dir.Name, out int id))
                {
                    // If successful, create a file stream for the save data file and add it to the list
                    string saveFilePath = Path.Combine(dir.FullName, id.ToString());

                    // Create a deserialization stream for the save file
                    var stream = await SerializationExtensions.CreateFileDeserializationStream(saveFilePath);

                    // Add the stream to the list of streams
                    streams.Add(stream);
                }
            }

            // Return the array of existing save IDs.
            return streams.ToArray();
        }
    }
}
