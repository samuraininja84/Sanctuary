using System;
using System.Threading.Tasks;

namespace Sanctuary.Serializers
{
    /// <summary>
    /// Defines the interface for a serializer that can serialize and deserialize ISaveData objects to and from files.
    /// </summary>
    public interface ISerializer 
    {
        /// <summary>
        /// Serializes the given ISaveData object to a file at the specified folder and file path.
        /// </summary>
        /// <param name="data">The ISaveData object to serialize.</param>
        /// <param name="folderPath">The folder path where the file will be saved.</param>
        /// <param name="filePath">The file path where the data will be saved.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Serialize(ISaveData data, string folderPath, string filePath);

        /// <summary>
        /// Deserializes the data from the specified file path into an ISaveData object.
        /// </summary>
        /// <param name="filePath">The file path from which to deserialize the data.</param>
        /// <returns>A task representing the asynchronous operation, with the deserialized ISaveData object as the result.</returns>
        Task<ISaveData> Deserialize(string filePath);

        /// <summary>
        /// Gets the file extension used by this serializer.
        /// </summary>
        /// <returns>The file extension as a string.</returns>
        string GetFileExtension();
    }
}