using System.IO;
using System.Threading.Tasks;
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

        private JsonSerializer(SerializationOptions options = SerializationOptions.None, string fileExtension = ".data")
        {
            this.options = options;
            this.fileExtension = fileExtension;
        }

        public static JsonSerializer Create(SerializationOptions options, string fileExtension = ".data") => new(options, fileExtension);

        public static JsonSerializer CreateAsJson(SerializationOptions options, string fileExtension = ".json") => new(options, fileExtension);

        public static JsonSerializer CreateAsMarkDown(SerializationOptions options, string fileExtension = ".md") => new(options, fileExtension);

        public static JsonSerializer CreateAsText(SerializationOptions options, string fileExtension = ".txt") => new(options, fileExtension);

        public async Task Serialize(ISaveData data, string filePath)
        {
            // Capture the options in a local variable to avoid closure issues in the async task.
            var options = this.options;

            // Run the serialization in a separate task to avoid blocking the main thread.
            await Task.Run(() =>
            {
                // Ensure the folder path exists.
                var folderPath = Path.GetDirectoryName(filePath);

                // Create the directory if it does not exist.
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Create a file stream to write to the file.
                using var saveStream = new FileStream(filePath, FileMode.Create);

                // Create a stream writer to write to the file with optional compression.
                using StreamWriter writer = SerializationExtensions.CreateStreamWriter(options, saveStream);

                // Serialize the save data to a JSON string using Newtonsoft.Json
                writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            });

            // Create a backup of the file if the setting is enabled.
            if (options.HasFlag(SerializationOptions.Backup)) await DirectoryUtility.CopyFileAsync(filePath, filePath + SerializationExtensions.BackupFileExtension);
        }

        // To Do: Add check to see if the file is encrypted and if so, decrypt it before attempting to deserialize it, regardless of whether the options include the Encrypted flag or not.
        // This would allow files to be deserialized even in the case that the options do not include the Encrypted flag,
        // so that it doesn't break backwards compatibility with different versions of the game that may have used different serialization options.

        public async Task<ISaveData> Deserialize(string filePath)
        {
            // Capture the options in a local variable to avoid closure issues in the async task.
            var options = this.options;

            // Check if the file exists before attempting to deserialize it.
            if (!File.Exists(filePath))
            {
                // Attempt to roll back to the backup file, if it fails or backups are not allowed, return a new empty save data object.
                if (!await SerializationExtensions.AttemptRollback(filePath))
                {
                    // Log an error if rollback failed or backups are not allowed.
                    UnityEngine.Debug.LogError("[Sanctuary]: Save file not found at " + filePath + " and rollback to backup failed, the backup file may not exist or is corrupted. Returning a new empty save data object.");

                    // Return a new empty save data object.
                    return SaveData.Empty;
                }
            }

            // Create a file stream to read from the file.
            await using var loadStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create a stream reader to read from the file with optional decompression.
            using StreamReader reader = SerializationExtensions.CreateStreamReader(options, loadStream);

            // Create a JSON text reader to read the JSON data from the stream.
            using var jsonReader = new JsonTextReader(reader);

            // Return the loaded save data.
            return new NewtonsoftJsonSerializer().Deserialize<SaveData>(jsonReader);
        }

        public readonly string GetFileExtension() => fileExtension;
    }
}

#endif