using System.IO;
using System.Threading.Tasks;
using Sanctuary.Serialization;
using Newtonsoft.Json;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Sanctuary
{
    public readonly struct StreamSaveDataProvider : ISaveDataProvider
    {
        private readonly IStreamConfiguration m_Configuration;

        public readonly string RootPath => m_Configuration.RootPath;

        public readonly SerializationOptions Options => m_Configuration.Options;

        public StreamSaveDataProvider(IStreamConfiguration configuration) => m_Configuration = configuration;

        public async Task<bool> WriteAsync(string relativePath, byte[] data)
        {
            // Create a file serialization stream to write to the file with optional compression.
            using var source = await m_Configuration.GetStream(Configuration.StreamType.Serialization, GetFullPath(relativePath));

            // Create a stream writer to write to the file with optional compression.
            using var writer = SerializationExtensions.CreateStreamWriter(Options, source);

            // Serialize the save data to a JSON string using Newtonsoft.Json
            writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));

            // Return true to indicate that the write operation was successful
            return true;
        }

        public async Task<byte[]> ReadAsync(string relativePath)
        {
            // Create a file deserialization stream to read from the file with optional decompression.
            using var source = await m_Configuration.GetStream(Configuration.StreamType.Deserialization, GetFullPath(relativePath));

            // Create a stream reader to read from the file with optional decompression.
            using var reader = SerializationExtensions.CreateStreamReader(Options, source);

            // Create a JSON text reader to read the JSON data from the stream.
            using var jsonReader = new JsonTextReader(reader);

            // Read the JSON data from the stream and deserialize it into a byte array using Newtonsoft.Json
            var serializer = new NewtonsoftJsonSerializer();

            // Run the deserialization in a separate task to avoid blocking the main thread.
            return await Task.Run(() => serializer.Deserialize<byte[]>(jsonReader));
        }

        public Task<bool> DeleteAsync(string relativePath)
        {
            // Get the full path of the file to delete
            var fullPath = GetFullPath(relativePath);

            // Delete the file if it exists. If it doesn't exist, we still return true because the end result is that the file is not present.
            if (File.Exists(fullPath)) File.Delete(fullPath);

            // Return true even if the file didn't exist, as the end result is that the file is not present.
            return Task.FromResult(true);
        }

        public Task<bool> ExistsAsync(string relativePath) => Task.FromResult(File.Exists(GetFullPath(relativePath)));

        private string GetFullPath(string relativePath) => Path.Combine(RootPath, relativePath);
    }
}
