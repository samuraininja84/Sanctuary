using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Loaders;
using Array = System.Array;

namespace Sanctuary.Configuration
{
    /// <summary>
    /// Represents a configuration for creating streams used in serialization and deserialization processes.
    /// </summary>
    public abstract class StreamConfiguration : ScriptableObject
    {
        [Header("Stream Configuration")]
        [Tooltip("The folder where saves are stored. Not applicable for all stream types.")]
        [SerializeField] protected string saveFolder = SaveLoaderDefaults.DefaultFolderName;

        /// <summary>
        /// Gets a stream based on the specified stream type and optional file path.
        /// </summary>
        /// <param name="streamType">The type of stream to create.</param>
        /// <param name="filePath">The file path for the stream, if applicable.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created stream.</returns>
        public abstract Task<Stream> GetStream(StreamType streamType, string filePath = null);

        /// <summary>
        /// Gets all streams from the specified folder name. If no folder name is provided, it defaults to the configured save folder.
        /// </summary>
        /// <param name="folderName">The name of the folder to get streams from. If null or empty, the default save folder is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of streams from the specified folder.</returns>
        public virtual Task<Stream[]> GetStreams(string folderName = null) => Task.FromResult(Array.Empty<Stream>());

        /// <summary>
        /// Constructs a file path by combining the persistent data path, specified folder path, file name, and extension.
        /// </summary>
        /// <param name="folderPath">The folder path where the file is located.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="extension">The file extension.</param>
        /// <returns>The constructed file path.</returns>
        public virtual string ConstructPath(string folderPath, string fileName, string extension) => Path.Combine(Application.persistentDataPath, folderPath, fileName + extension);

        /// <summary>
        /// Gets the name of the folder where saves are stored.
        /// </summary>
        /// <returns>The name of the save folder.</returns>
        public string GetSaveFolderName() => saveFolder;

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
