using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DirectoryUtility = Sanctuary.Utility.DirectoryUtility;

namespace Sanctuary.Serializers
{
    /// <summary>
    /// A serializer that uses a binary format to serialize and deserialize ISaveData objects.
    /// </summary>
    public readonly struct BinarySerializer : ISerializer
    {
        internal readonly SerializationOptions options;
        internal readonly string fileExtension;

        public static BinarySerializer Default => new();

        public static BinarySerializer Compressed => new(SerializationOptions.Compressed);

        public static BinarySerializer Encrypted => new(SerializationOptions.Encrypted);

        public static BinarySerializer Backup => new(SerializationOptions.Backup);

        public static BinarySerializer All => new(SerializationOptions.All);

        internal BinarySerializer(SerializationOptions options = SerializationOptions.None, string fileExtension = ".bin")
        {
            this.options = options;
            this.fileExtension = fileExtension;
        }

        public static BinarySerializer Create(SerializationOptions options, string fileExtension = ".bin") => new(options, fileExtension);

        public static BinarySerializer Create(bool useCompression, bool useEncryption, bool allowBackup, string fileExtension = ".bin")
        {
            SerializationOptions options = SerializationOptions.None;
            if (useCompression) options |= SerializationOptions.Compressed;
            if (useEncryption) options |= SerializationOptions.Encrypted;
            if (allowBackup) options |= SerializationOptions.Backup;
            return new BinarySerializer(options, fileExtension);
        }

        public async Task Serialize(ISaveData data, string folderPath, string filePath)
        {
            // Capture the useCompression value in a local variable to avoid closure issues in the async task.
            bool useCompression = options.HasFlag(SerializationOptions.Compressed);

            // Capture the backupAllowed value in a local variable to avoid closure issues in the async task.
            bool backupAllowed = options.HasFlag(SerializationOptions.Backup);

            // Run the serialization in a separate task to avoid blocking the main thread.
            await Task.Run(() =>
            {
                // Ensure the folder path exists.
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Create a file stream to write to the file.
                using var saveStream = new FileStream(filePath, FileMode.Create);

                // Create a binary writer to write to the file.
                using BinaryWriter writer = useCompression ? new(new GZipStream(saveStream, CompressionMode.Compress), Encoding.UTF8, false) : new(saveStream, Encoding.UTF8, false);

                // Write each chunk of data.
                foreach (var chunkId in data.GetChunkIDs())
                {
                    // Get the chunk data.
                    var chunk = data.GetChunk(chunkId);

                    // Write a true boolean to indicate a chunk follows.
                    writer.Write(true);

                    // Write the chunk ID and the number of key-value pairs in the chunk.
                    writer.Write(chunkId);

                    // Write the number of key-value pairs in the chunk.
                    writer.Write(chunk.Count);

                    // Write each key-value pair in the chunk.
                    foreach (var (key, value) in chunk)
                    {
                        // Write the key.to the file.
                        writer.Write(key);

                        // Write the value to the file.
                        writer.Write(value);
                    }
                }

                // Write a false boolean to indicate the end of chunks.
                writer.Write(false);
            });

            // Create a backup of the file if the setting is enabled.
            if (backupAllowed) await DirectoryUtility.CopyFileAsync(filePath, filePath + SerializationDefaults.BackupFileExtension);
        }

        public async Task<ISaveData> Deserialize(string filePath)
        {
            // Capture the useCompression value in a local variable to avoid closure issues in the async task.
            bool useCompression = options.HasFlag(SerializationOptions.Compressed);

            // Check if the file exists before attempting to deserialize it.
            if (!File.Exists(filePath))
            {
                // Attempt to roll back to the backup file, if it fails or backups are not allowed, return a new empty save data object.
                if (!await SerializationDefaults.AttemptRollback(filePath))
                {
                    // Determine the appropriate error message based on whether backups are allowed.
                    string errorMessage = "rollback to backup failed, the backup file may not exist or is corrupted.";

                    // Log an error if rollback failed or backups are not allowed.
                    UnityEngine.Debug.LogError($"Save file not found at {filePath} and {errorMessage}.");

                    // Return a new empty save data object.
                    return new SaveData();
                }
            }

            // Create a file stream to read from the file.
            await using var loadStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create a binary reader to read from the file with optional decompression.
            using BinaryReader reader = useCompression ? new(new GZipStream(loadStream, CompressionMode.Decompress), Encoding.UTF8, false) : new(loadStream, Encoding.UTF8, false);

            // Create a new save data object to hold the loaded data.
            var save = new SaveData();

            // Read each chunk of data.
            while (reader.ReadBoolean())
            {
                // Read the chunk ID.
                var chunkId = reader.ReadString();

                // Get the chunk data using the chunk ID.
                var chunk = save.GetChunk(chunkId);

                // Read the number of key-value pairs in the chunk.
                var count = reader.ReadInt32();

                // Read each key-value pair in the chunk and add it to the chunk.
                for (var i = 0; i < count; i++) chunk.Add(reader.ReadString(), reader.ReadString());
            }

            // Return the loaded save data.
            return save;
        }

        public readonly string GetFileExtension() => fileExtension;
    }
}
