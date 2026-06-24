using System.IO;
using System.Threading.Tasks;

#if UNITY_NEWTONSOFT_JSON

using Formatting = Newtonsoft.Json.Formatting;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JsonTextReader = Newtonsoft.Json.JsonTextReader;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Sanctuary.Serializers
{
    public readonly struct MemorySerializer : ISerializer
    {
        internal readonly SerializationOptions options;
        internal readonly string fileExtension;

        public static MemorySerializer Default => new(SerializationOptions.None);

        public static MemorySerializer Compressed => new(SerializationOptions.Compressed);

        public static MemorySerializer Encrypted => new(SerializationOptions.Encrypted);

        public static MemorySerializer Backup => new(SerializationOptions.Backup);

        public static MemorySerializer BackupCompressed => new(SerializationOptions.Backup | SerializationOptions.Compressed);

        public static MemorySerializer BackupEncrypted => new(SerializationOptions.Backup | SerializationOptions.Encrypted);

        public static MemorySerializer CompressionEncrypted => new(SerializationOptions.Compressed | SerializationOptions.Encrypted);

        public static MemorySerializer All => new(SerializationOptions.All);

        private MemorySerializer(SerializationOptions options = SerializationOptions.None, string fileExtension = ".data")
        {
            this.options = options;
            this.fileExtension = fileExtension;
        }

        public static MemorySerializer Create(SerializationOptions options, string fileExtension = ".data") => new(options, fileExtension);

        public async Task Serialize(ISaveData data, string filePath)
        {
            // Capture the folderPath and filePath in local variables to avoid closure issues in the async task.
            var options = this.options;

            // Run the serialization in a separate task to avoid blocking the main thread.
            await Task.Run(() =>
            {                
                // Capture the memoryStream in a local variable to avoid closure issues in the async task.
                using var saveStream = new MemoryStream();

                // Create a stream writer to write to the file with optional compression.
                using StreamWriter writer = SerializationExtensions.CreateStreamWriter(options, saveStream);

                // Serialize the save data to a JSON string using Newtonsoft.Json
                writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            });
        }

        public async Task<ISaveData> Deserialize(string filePath)
        {
            // Create a memory stream to read from the file.
            await using var loadStream = new MemoryStream();

            // Create a stream reader to read from the file with optional decompression.
            using StreamReader reader = SerializationExtensions.CreateStreamReader(options, loadStream);

            // Create a JSON text reader to read the JSON data from the stream.
            using var jsonReader = new JsonTextReader(reader);

            // Return the loaded save data.
            return new NewtonsoftJsonSerializer().Deserialize<SaveData>(jsonReader);
        }

        public string GetFileExtension() => fileExtension;
    }
}

#endif