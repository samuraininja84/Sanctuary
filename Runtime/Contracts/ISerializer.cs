using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Serialization
{
    /// <summary>
    /// Defines the interface for a serializer that can serialize and deserialize ISaveData objects to and from files.
    /// </summary>
    [System.Obsolete("This interface is deprecated and will be removed in future versions. Please use ISaveSerializer instead.")]
    public interface ISerializer 
    {
        /// <summary>
        /// Gets the serialization options used by this serializer.
        /// </summary>
        [System.Obsolete("This property is deprecated and will be removed in future versions. Serialization options will be managed by the designated SerializationConfiguration instead.")]
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
        [System.Obsolete("This method is deprecated and will be removed in future versions. Backup functionality will be handled by the designated ISanctuaryService instead.")]
        Task CopyTo(Stream source, Stream destination = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes the data from the specified stream into an ISaveData object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which the data will be deserialized.</param>
        /// <returns>A task representing the asynchronous operation, with the deserialized <see cref="LoadResult{T}"/> object as the result.</returns>
        Task<LoadResult<ISaveData>> Deserialize(Stream stream);

        /// <summary>
        /// Gets the file extension used by this serializer.
        /// </summary>
        /// <returns>The file extension as a string.</returns>
        [System.Obsolete("This method is deprecated and will be removed in future versions. File extension handling will be managed by the designated SerializationConfiguration instead.")]
        string GetFileExtension();
    }

    /// <summary>
    /// Defines the interface for a serializer that can serialize and deserialize save data to and from byte arrays.
    /// </summary>
    public interface ISaveSerializer
    {
        byte[] Serialize<T>(T data) where T : class;

        SaveDeserializeResult<T> Deserialize<T>(byte[] data) where T : class;

        int CurrentSchemaVersion { get; }
    }
}