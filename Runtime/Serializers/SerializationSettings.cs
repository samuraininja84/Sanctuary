using UnityEngine;

namespace Sanctuary.Serializers
{
    [System.Flags]
    public enum SerializationSettings
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
