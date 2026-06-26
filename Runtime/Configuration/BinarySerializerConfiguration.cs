using UnityEngine;
using Sanctuary.Serialization;

namespace Sanctuary.Configuration
{
    [CreateAssetMenu(fileName = "BinarySerializerConfiguration", menuName = "Sanctuary/Configuration/Serializers/Binary Serializer", order = 1)]
    public class BinarySerializerConfiguration : SerializerConfiguration
    {
        /// <summary>
        /// Gets the <see cref="BinarySerializer"/> instance based on the configuration.
        /// </summary>
        /// <returns>The configured <see cref="BinarySerializer"/> instance.</returns>
        public override ISerializer GetSerializer() => BinarySerializer.Create(serializationOptions, overrideExtension);

        /// <summary>
        /// Gets the <see cref="BinarySerializer"/> instance based on the provided serialization options and an optional override extension.
        /// </summary>
        /// <param name="options">The serialization options to use for this serializer.</param>
        /// <param name="overrideExtension">The file extension to use for serialized files. If null, the default extension will be used.</param>
        /// <returns>The configured <see cref="BinarySerializer"/> instance.</returns>
        public override ISerializer GetSerializer(SerializationOptions options, string overrideExtension = null)
        {
            // If there is an override extension, create a new serializer with the provided options and extension; otherwise, create a new serializer with the provided options and the default extension.
            return overrideExtension != null ? BinarySerializer.Create(options, overrideExtension) : BinarySerializer.Create(options);
        }
    }
}
