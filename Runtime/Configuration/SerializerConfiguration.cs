using UnityEngine;
using Sanctuary.Serialization;

namespace Sanctuary.Configuration
{
    public abstract class SerializerConfiguration : ScriptableObject
    {
        [Header("Serializer Settings")]
        [Tooltip("The serialization options to use for this Serializer. Default is None.")]
        [SerializeField] protected SerializationOptions serializationOptions = SerializationOptions.None;
        [Tooltip("The file extension to use for serialized files. If left empty, the default extension will be used.")]
        [SerializeField] protected string overrideExtension = string.Empty;

        /// <summary>
        /// Gets the serializer instance based on the configuration.
        /// </summary>
        /// <remarks>Use this method to obtain a serializer that is configured according to the settings defined in this ScriptableObject.</remarks>
        /// <returns>The configured serializer instance.</returns>
        public abstract ISerializer GetSerializer();

        /// <summary>
        /// Gets the serializer instance based on the provided serialization options and optional override extension.
        /// </summary>
        /// <param name="options">The serialization options to use for this serializer.</param>
        /// <param name="overrideExtension">The file extension to use for serialized files. If null, the default extension will be used.</param>
        /// <returns>The configured serializer instance.</returns>
        public abstract ISerializer GetSerializer(SerializationOptions options, string overrideExtension = null);
    }
}
