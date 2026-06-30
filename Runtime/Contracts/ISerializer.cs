using System.IO;
using System.Threading;
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
        /// <param name="source">The <see cref="Stream"/> to which the data will be serialized.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Serialize(ISaveData data, Stream source);

        /// <summary>
        /// Copies data from the source stream to the backup stream.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> to which the data will be serialized.</param>
        /// <param name="destination">The <see cref="Stream"/> to which the backup data will be serialized, if any. This parameter is optional and can be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation, if needed. This parameter is optional and defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CopyTo(Stream source, Stream destination = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes the data from the specified stream into an ISaveData object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which the data will be deserialized.</param>
        /// <returns>A task representing the asynchronous operation, with the deserialized <see cref="SaveLoadResult{T}"/> object as the result.</returns>
        Task<SaveLoadResult<ISaveData>> Deserialize(Stream stream);

        /// <summary>
        /// Gets the file extension used by this serializer.
        /// </summary>
        /// <returns>The file extension as a string.</returns>
        string GetFileExtension();
    }
}