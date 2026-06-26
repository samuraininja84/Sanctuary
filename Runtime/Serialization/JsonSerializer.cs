using System.IO;
using System.Threading;
using System.Threading.Tasks;

#if UNITY_NEWTONSOFT_JSON

using Formatting = Newtonsoft.Json.Formatting;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JsonTextReader = Newtonsoft.Json.JsonTextReader;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Sanctuary.Serialization
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

        public async Task Serialize(ISaveData data, Stream source)
        {
            // Capture the options in a local variable to avoid closure issues in the async task.
            var options = this.options;

            // Run the serialization in a separate task to avoid blocking the main thread.
            await Task.Run(() =>
            {
                // Create a stream writer to write to the file with optional compression.
                using var writer = SerializationExtensions.CreateStreamWriter(options, source);

                // Serialize the save data to a JSON string using Newtonsoft.Json
                writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            });
        }

        public async Task CopyTo(Stream source, Stream destination, CancellationToken cancellationToken = default)
        {
            // If the source or destination stream is null, return without doing anything.
            if (source == null || destination == null) return;

            // If we don't have backup enabled, we don't need to copy the data.
            if (!options.HasFlag(SerializationOptions.Backup)) return;

            // If the source stream is not readable or the destination stream is not writable, throw an exception.
            if (!source.CanRead) throw new System.InvalidOperationException("Source stream is not readable.");
            if (!destination.CanWrite) throw new System.InvalidOperationException("Destination stream is not writable.");

            // Copy the source stream to the destination stream.
            await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        }

        // To Do: Add check to see if the file is encrypted and if so, decrypt it before attempting to deserialize it, regardless of whether the options include the Encrypted flag or not.
        // This would allow files to be deserialized even in the case that the options do not include the Encrypted flag,
        // so that it doesn't break backwards compatibility with different versions of the game that may have used different serialization options.

        public async Task<LoadResult> Deserialize(Stream stream)
        {
            // Capture the options in a local variable to avoid closure issues in the async task.
            var options = this.options;

            // Create a stream reader to read from the file with optional decompression.
            using var reader = SerializationExtensions.CreateStreamReader(options, stream);

            // Create a JSON text reader to read the JSON data from the stream.
            using var jsonReader = new JsonTextReader(reader);

            // Return the loaded save data.
            var data = new NewtonsoftJsonSerializer().Deserialize<SaveData>(jsonReader);

            // Return a LoadResult indicating success or failure based on whether saveData is null.
            return data != null ? LoadResult.Success(data) : LoadResult.Failure();
        }

        public readonly string GetFileExtension() => fileExtension;
    }
}

#endif