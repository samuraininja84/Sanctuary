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

        public SerializationOptions Options => options;

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

        public async Task Serialize(ISaveData data, Stream stream)
        {
            // Capture the options in a local variable to avoid closure issues in the async task.
            var options = this.options;

            // Run the serialization in a separate task to avoid blocking the main thread.
            await Task.Run(() =>
            {
                // Create a stream writer to write to the file with optional compression.
                using StreamWriter writer = SerializationExtensions.CreateStreamWriter(options, stream);

                // Serialize the save data to a JSON string using Newtonsoft.Json
                writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            });
        }

        public async Task<ISaveData> Deserialize(Stream stream)
        {
            // Capture the options in a local variable to avoid closure issues in the async task.
            var options = this.options;

            // Create a stream reader to read from the file with optional decompression.
            using StreamReader reader = SerializationExtensions.CreateStreamReader(options, stream);

            // Create a JSON text reader to read the JSON data from the stream.
            using var jsonReader = new JsonTextReader(reader);

            // Return the loaded save data.
            return new NewtonsoftJsonSerializer().Deserialize<SaveData>(jsonReader);
        }

        public readonly string GetFileExtension() => fileExtension;
    }
}

#endif