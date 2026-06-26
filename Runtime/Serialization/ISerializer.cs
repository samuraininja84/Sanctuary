using System.IO;
using System.Threading.Tasks;

namespace Sanctuary.Serialization
{
    /// <summary>
    /// Defines the interface for a serializer that can serialize and deserialize ISaveData objects to and from files.
    /// </summary>
    public interface ISerializer 
    {
        /// <summary>
        /// Gets the serialization options used by this serializer.
        /// </summary>
        SerializationOptions Options { get; }

        /// <summary>
        /// Serializes the given ISaveData object to a stream.
        /// </summary>
        /// <param name="data">The ISaveData object to serialize.</param>
        /// <param name="stream">The <see cref="Stream"/> to which the data will be serialized.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Serialize(ISaveData data, Stream stream);

        /// <summary>
        /// Deserializes the data from the specified stream into an ISaveData object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which the data will be deserialized.</param>
        /// <returns>A task representing the asynchronous operation, with the deserialized <see cref="LoadResult"/> object as the result.</returns>
        Task<LoadResult> Deserialize(Stream stream);

        /// <summary>
        /// Gets the file extension used by this serializer.
        /// </summary>
        /// <returns>The file extension as a string.</returns>
        string GetFileExtension();
    }
}