using System.IO;
using System.Text;
using System.IO.Compression;
using System.Threading.Tasks;

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
        private readonly SerializationOptions options;

        public static JsonSerializer Default => new();

        public static JsonSerializer Compressed => new(SerializationOptions.Compressed);

        public static JsonSerializer Encrypted => new(SerializationOptions.Encrypted);

        public static JsonSerializer Backup => new(SerializationOptions.Backup);

        public static JsonSerializer All => new(SerializationOptions.All);

        internal JsonSerializer(SerializationOptions options = SerializationOptions.None) => this.options = options;

        public static JsonSerializer Create(SerializationOptions options) => new(options);

        public async Task Serialize(ISaveData data, string folderPath, string filePath)
        {
            // Capture the useCompression value in a local variable to avoid closure issues in the async task.
            bool useCompression = options.HasFlag(SerializationOptions.Compressed);

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
        }

        public async Task<ISaveData> Deserialize(string filePath)
        {
            // Capture the useCompression value in a local variable to avoid closure issues in the async task.
            bool useCompression = options.HasFlag(SerializationOptions.Compressed);

            // Create a file stream to read from the file.
            await using var loadStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create a stream reader to read from the file with optional decompression.
            using StreamReader reader = useCompression ? new(new GZipStream(loadStream, CompressionMode.Decompress), Encoding.UTF8, false) : new(loadStream, Encoding.UTF8, false);

            // Create a JSON text reader to read the JSON data from the stream.
            using var jsonReader = new JsonTextReader(reader);

            // Return the loaded save data.
            return new NewtonsoftJsonSerializer().Deserialize<SaveData>(jsonReader);
        }

        public string GetFileExtension() => ".json";
    }
}

#endif