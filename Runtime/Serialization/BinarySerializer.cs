using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Serialization
{
    /// <summary>
    /// A serializer that uses a binary format to serialize and deserialize ISaveData objects.
    /// </summary>
    public readonly struct BinarySerializer : ISerializer
    {
        internal readonly SerializationOptions options;
        internal readonly string fileExtension;

        public SerializationOptions Options => options;

        public static BinarySerializer Default => new(SerializationOptions.None);

        public static BinarySerializer Compressed => new(SerializationOptions.Compressed);

        public static BinarySerializer Encrypted => new(SerializationOptions.Encrypted);

        public static BinarySerializer Backup => new(SerializationOptions.Backup);

        public static BinarySerializer BackupCompressed => new(SerializationOptions.Backup | SerializationOptions.Compressed);

        public static BinarySerializer BackupEncrypted => new(SerializationOptions.Backup | SerializationOptions.Encrypted);

        public static BinarySerializer CompressionEncrypted => new(SerializationOptions.Compressed | SerializationOptions.Encrypted);

        public static BinarySerializer All => new(SerializationOptions.All);

        private BinarySerializer(SerializationOptions options = SerializationOptions.None, string fileExtension = SerializationExtensions.DefaultFileExtension)
        {
            this.options = options;
            this.fileExtension = fileExtension;
        }

        public static BinarySerializer Create(SerializationOptions options, string fileExtension = SerializationExtensions.DefaultFileExtension) => new(options, fileExtension);

        public static BinarySerializer CreateAsBinary(SerializationOptions options) => new(options, ".bin");

        public async Task Serialize(ISaveData data, Stream source)
        {
            // Capture the folderPath and filePath in local variables to avoid closure issues in the async task.
            var options = this.options;

            // Run the serialization in a separate task to avoid blocking the main thread.
            await Task.Run(() =>
            {
                // Create a binary writer to write to the file.
                using var writer = SerializationExtensions.CreateBinaryWriter(options, source);

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

        public async Task<LoadResult<ISaveData>> Deserialize(Stream stream)
        {
            // If the file could not be opened, return an empty save data object.
            if (stream == null) return LoadResult<ISaveData>.Fail(LoadStatus.ProviderError, "Stream is null.");

            // Create a binary reader to read from the file with optional decompression.
            using var reader = SerializationExtensions.CreateBinaryReader(options, stream);

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
            return LoadResult<ISaveData>.Succeed(save, null);
        }

        public readonly string GetFileExtension() => fileExtension;
    }
}
