using UnityEngine;

namespace Sanctuary.Serializers
{
    /// <summary>
    /// Defines the settings that can be applied during serialization.
    /// </summary>
    /// <remarks>
    /// Select from the following options:
    /// <list type="bullet">
    /// <item><see cref="None"/>: No special settings are applied.</item>
    /// <item><see cref="Compressed"/>: Indicates that the serializer should compress the data during serialization.</item>
    /// <item><see cref="Encrypted"/>: Indicates that the serializer should encrypt the data during serialization.</item>
    /// <item><see cref="Backup"/>: Enables backup functionality for the serializer.</item>
    /// <item><see cref="All"/>: Indicates that the serializer should use all available settings (compressed, encrypted, and backup).</item>
    /// </list>
    /// </remarks>
    [System.Flags]
    [System.Serializable]
    public enum SerializationOptions
    {
        /// <summary>
        /// No special settings are applied.
        /// </summary>  
        [Tooltip("No special settings are applied.")]
        None = 0,

        /// <summary>
        /// Indicates that the serializer should compress the data during serialization.
        /// </summary>
        [Tooltip("Indicates that the serializer should compress the data during serialization.")]
        Compressed = 1 << 0,

        /// <summary>
        /// Indicates that the serializer should encrypt the data during serialization.
        /// </summary>
        [Tooltip("Indicates that the serializer should encrypt the data during serialization.")]
        Encrypted = 1 << 1,

        /// <summary>
        /// Enables backup functionality for the serializer.
        /// When this flag is set, the serializer will create a backup of the existing data before overwriting it during serialization. 
        /// This can help prevent data loss in case of errors or interruptions during the serialization process.
        /// </summary>
        [Tooltip("Enables backup functionality for the serializer.")]
        Backup = 1 << 2,

        /// <summary>
        /// Indicates that the serializer should use all available settings (compressed, encrypted, and backup).
        /// </summary>
        [Tooltip("Indicates that the serializer should use all available settings (compressed, encrypted, and backup).")]
        All = Compressed | Encrypted | Backup
    }
}
