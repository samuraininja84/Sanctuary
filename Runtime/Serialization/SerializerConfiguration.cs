using UnityEngine;
using Sanctuary.Serialization;

namespace Sanctuary.Configuration
{
    /// <summary>
    /// Represents a configuration for a serializer, allowing customization of serialization options and file extensions.
    /// </summary>
    public abstract class SerializerConfiguration : ScriptableObject
    {
        /// <summary>
        /// Gets the serializer instance based on the provided serialization options and optional override extension.
        /// </summary>
        /// <param name="options">The serialization options to use for this serializer.</param>
        /// <param name="overrideExtension">The file extension to use for serialized files. If null, the default extension will be used.</param>
        /// <returns>The configured serializer instance.</returns>
        public abstract ISerializer GetSerializer(SerializationOptions options, string overrideExtension = null);
    }
}
