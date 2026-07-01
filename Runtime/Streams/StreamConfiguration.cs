using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Array = System.Array;
using Sanctuary.Serialization;

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
        [Tooltip("Options for serialization, such as compression, encryption, and backup.")]
        [SerializeField] private SerializationOptions options = SerializationOptions.Compressed;

        public virtual string RootPath => Path.Combine(Application.persistentDataPath, folderName);

        public int CurrentSchemaVersion => currentSchemaVersion;

        public SerializationOptions Options => options;

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
    }
}
