using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Array = System.Array;
using SerializationExtensions = Sanctuary.Serialization.SerializationExtensions;

namespace Sanctuary.Configuration
{
    /// <summary>
    /// Represents a configuration for creating streams used in serialization and deserialization processes.
    /// </summary>
    public abstract class StreamConfiguration : ScriptableObject, IStreamConfiguration
    {
        [Header("Stream Configuration")]
        [Tooltip("Subdirectory under Application.persistentDataPath where saves are stored. Not applicable for all stream types.")]
        [SerializeField] protected string folderName = SerializationExtensions.DefaultFolderName;
        [Tooltip("The file extension used for serialization streams.")]
        [SerializeField] protected string extension = SerializationExtensions.DefaultFileExtension;
        [Tooltip("The file extension used for backup streams.")]
        [SerializeField] protected string backupExtension = SerializationExtensions.DefaultBackupExtension;
        [Tooltip("Schema version stamped onto new save envelopes. Bump when your save format changes; pair with an ISaveMigrationStep for the upgrade.")]
        [SerializeField] private int currentSchemaVersion = 1;

        public virtual string RootPath => Path.Combine(Application.persistentDataPath, folderName);

        public int CurrentSchemaVersion => currentSchemaVersion;

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
            Deserialization,

            /// <summary>
            /// Indicates that the stream is intended for backup purposes.
            /// </summary>
            /// <remarks>This type of stream is used to create a backup of data, ensuring that a copy is available for recovery in case of data loss or corruption.</remarks>
            Backup
        }
    }
}
