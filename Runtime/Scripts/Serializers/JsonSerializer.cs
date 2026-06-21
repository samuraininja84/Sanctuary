using UnityEngine;

namespace Sanctuary.Serializers
{
    public class JsonSerializer : ISerializer 
    {
        public string Serialize<T>(T obj) => JsonUtility.ToJson(obj, true);

        //public string Serialize()
        //{
        //    //// Create a stream writer to write to the file.
        //    // using var writer = new StreamWriter(saveStream, Encoding.UTF8, 1024, false);
        //
        //    // Write each chunk of data.
        //    foreach (var chunkId in data.GetChunkIDs())
        //    {
        //        // Get the chunk data.
        //        var chunk = data.GetChunk(chunkId);

        //        // Write a true boolean to indicate a chunk follows.
        //        writer.Write(true);

        //        // Add a newline after the boolean for readability.
        //        writer.Write(Environment.NewLine);

        //        // Write the chunk ID and the number of key-value pairs in the chunk.
        //        writer.Write(FormattingExtensions.TryFormat(chunkId + ": "));

        //        // Write the number of key-value pairs in the chunk.
        //        writer.Write(chunk.Count);

        //        // Add a newline after the boolean for readability.
        //        writer.Write(Environment.NewLine);

        //        // Write each key-value pair in the chunk.
        //        foreach (var (key, value) in chunk)
        //        {
        //            // Write the key.to the file.
        //            writer.Write(FormattingExtensions.TryFormat(key));

        //            // Write the value to the file.
        //            writer.Write(FormattingExtensions.TryFormat(value));

        //            // Add a newline after the boolean for readability.
        //            writer.Write(Environment.NewLine);
        //        }

        //        // Add a newline after each chunk for readability.
        //        writer.Write(Environment.NewLine);
        //    }
        //}

        public T Deserialize<T>(string json) => JsonUtility.FromJson<T>(json);

        public string FileExtension() => ".json";
    }
}