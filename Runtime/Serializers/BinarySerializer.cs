using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Sanctuary.Serializers
{
    /// <summary>
    /// A serializer that uses a binary format to serialize and deserialize ISaveData objects.
    /// </summary>
    public readonly struct BinarySerializer : ISerializer
    {
        private readonly SerializationOptions options;

        public static BinarySerializer Default => new();

        public static BinarySerializer Compressed => new(SerializationOptions.Compressed);

        public static BinarySerializer Encrypted => new(SerializationOptions.Encrypted);

        public static BinarySerializer Backup => new(SerializationOptions.Backup);

        public static BinarySerializer All => new(SerializationOptions.All);

        internal BinarySerializer(SerializationOptions options) => this.options = options;

        public static BinarySerializer Create(SerializationOptions options) => new(options);

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
        }

        public async Task<ISaveData> Deserialize(string filePath)
        {
            // Capture the useCompression value in a local variable to avoid closure issues in the async task.
            bool useCompression = options.HasFlag(SerializationOptions.Compressed);

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

        public readonly string GetFileExtension() => ".bin";
    }
}
