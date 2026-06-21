using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Extensions;

namespace Sanctuary.Serializers
{
    public class JsonSerializer : ISerializer 
    {
        public async Task Serialize(ISaveData data, string folderPath, string filePath)
        {
            // Ensure the folder path exists.
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            // Create a file stream to write to the file.
            using var saveStream = new FileStream(filePath, FileMode.Create);

            // Create a stream writer to write to the file.
            using var writer = new StreamWriter(saveStream, Encoding.UTF8, 1024, false);

            // Write each chunk of data.
            foreach (var chunkId in data.GetChunkIDs())
            {
                // Get the chunk data.
                var chunk = data.GetChunk(chunkId);

                // Write a true boolean to indicate a chunk follows.
                writer.Write(true);

                // Add a newline after the boolean for readability.
                writer.Write(Environment.NewLine);

                // Write the chunk ID and the number of key-value pairs in the chunk.
                writer.Write(FormattingExtensions.TryFormat(chunkId 
                    //+ ": "
                    ));

                // Write the number of key-value pairs in the chunk.
                writer.Write(chunk.Count);

                // Add a newline after the boolean for readability.
                writer.Write(Environment.NewLine);

                // Write each key-value pair in the chunk.
                foreach (var (key, value) in chunk)
                {
                    // Write the key.to the file.
                    writer.Write(FormattingExtensions.TryFormat(key));

                    // Write the value to the file.
                    writer.Write(FormattingExtensions.TryFormat(value));

                    // Add a newline after the boolean for readability.
                    writer.Write(Environment.NewLine);
                }

                // Add a newline after each chunk for readability.
                writer.Write(Environment.NewLine);
            }
        }

        public async Task<ISaveData> Deserialize(string filePath)
        {
            // Create a file stream to read from the file.
            await using var loadStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create a binary reader to read from the file.
            using var reader = new StreamReader(loadStream, Encoding.UTF8, false);

            // Create a new save data object to hold the loaded data.
            var save = new SaveData();

            // Initialize a string variable to hold the current line being read from the file.
            string line;

            // Read each chunk of data.
            while ((line = reader.ReadLine()) != null)
            {
                // Read the chunk ID.
                var chunkId = reader.ReadLine();

                // Get the chunk data using the chunk ID.
                var chunk = save.GetChunk(chunkId);

                // Read the number of key-value pairs in the chunk.
                var count = int.Parse(reader.ReadLine());

                // Read each key-value pair in the chunk and add it to the chunk.
                for (var i = 0; i < count; i++) chunk.Add(reader.ReadLine(), reader.ReadLine());
            }

            // Return the loaded save data.
            return save;
        }

        public string FileExtension() => ".json";
    }
}