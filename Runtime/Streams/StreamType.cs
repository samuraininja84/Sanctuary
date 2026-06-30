namespace Sanctuary.Configuration
{
    /// <summary>
    /// Defines the types of streams that can be created by the StreamConfiguration.
    /// </summary>
    public enum StreamType
    {
        /// <summary>
        /// Indicates that the stream is intended for serialization purposes.
        /// </summary>
        /// <remarks>This type of stream is used to write data to a destination, such as a file or network, in a format that can be later read and reconstructed.</remarks>
        Serialization,

        /// <summary>
        /// Indicates that the stream is intended for deserialization purposes.
        /// </summary>
        /// <remarks>This type of stream is used to read data from a source, such as a file or network, and convert it into usable objects or data structures.</remarks>
        Deserialization,

        /// <summary>
        /// Indicates that the stream is intended for backup purposes.
        /// </summary>
        /// <remarks>This type of stream is used to create a backup of data, ensuring that a copy is available for recovery in case of data loss or corruption.</remarks>
        Backup
    }
}
