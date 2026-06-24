using System.IO;
using System.Threading.Tasks;

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

                // Create a binary writer to write to the memory stream.
                using var writer = SerializationExtensions.CreateBinaryWriter(options, saveStream);

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
                        // Write the key to the file.
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
            // Create a memory stream to read from the file.
            await using var loadStream = new MemoryStream();

            // Reset the position of the memory stream to the beginning before reading.
            loadStream.Position = 0;

            // Create a binary reader to read from the memory stream with optional decompression.
            using var reader = SerializationExtensions.CreateBinaryReader(options, loadStream);

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

            // Clear the memory stream after reading to free up resources.
            loadStream.SetLength(0);
            loadStream.Position = 0;

            // Return the loaded save data.
            return save;
        }

        public string GetFileExtension() => fileExtension;
    }
}