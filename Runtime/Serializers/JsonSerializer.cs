using System.IO;
using System.Text;
using System.IO.Compression;
using System.Threading.Tasks;
using System;
using DirectoryUtility = Sanctuary.Utility.DirectoryUtility;

#if UNITY_NEWTONSOFT_JSON

using Formatting = Newtonsoft.Json.Formatting;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JsonTextReader = Newtonsoft.Json.JsonTextReader;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Sanctuary.Serializers
{
    /// <summary>
    /// A serializer that uses JSON format to serialize and deserialize ISaveData objects.
    /// </summary>
    public readonly struct JsonSerializer : ISerializer 
    {
        internal readonly SerializationOptions options;
        internal readonly string fileExtension;

        public static JsonSerializer Default => new(SerializationOptions.None);

        public static JsonSerializer Compressed => new(SerializationOptions.Compressed);

        public static JsonSerializer Encrypted => new(SerializationOptions.Encrypted);

        public static JsonSerializer Backup => new(SerializationOptions.Backup);

        public static JsonSerializer BackupCompressed => new(SerializationOptions.Backup | SerializationOptions.Compressed);

        public static JsonSerializer BackupEncrypted => new(SerializationOptions.Backup | SerializationOptions.Encrypted);

        public static JsonSerializer CompressionEncrypted => new(SerializationOptions.Compressed | SerializationOptions.Encrypted);

        public static JsonSerializer All => new(SerializationOptions.All);

        private JsonSerializer(SerializationOptions options = SerializationOptions.None, string fileExtension = ".json")
        {
            this.options = options;
            this.fileExtension = fileExtension;
        }

        public static JsonSerializer Create(SerializationOptions options, string fileExtension = ".json") => new(options, fileExtension);

        public static JsonSerializer CreateAsData(SerializationOptions options, string fileExtension = ".data") => new(options, fileExtension);

        public static JsonSerializer CreateAsMarkDown(SerializationOptions options, string fileExtension = ".md") => new(options, fileExtension);

        public static JsonSerializer CreateAsText(SerializationOptions options, string fileExtension = ".txt") => new(options, fileExtension);

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

                // Create a stream writer to write to the file with optional compression.
                using StreamWriter writer = useCompression ? new(new GZipStream(saveStream, CompressionMode.Compress), Encoding.UTF8, 1024, false) : new(saveStream, Encoding.UTF8, 1024, false);

                // Serialize the save data to a JSON string using Newtonsoft.Json
                writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
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

            // Create a stream reader to read from the file with optional decompression.
            using StreamReader reader = useCompression ? new(new GZipStream(loadStream, CompressionMode.Decompress), Encoding.UTF8, false) : new(loadStream, Encoding.UTF8, false);

            // Create a JSON text reader to read the JSON data from the stream.
            using var jsonReader = new JsonTextReader(reader);

            // Return the loaded save data.
            return new NewtonsoftJsonSerializer().Deserialize<SaveData>(jsonReader);
        }

        public readonly string GetFileExtension() => fileExtension;
    }
}

#endif