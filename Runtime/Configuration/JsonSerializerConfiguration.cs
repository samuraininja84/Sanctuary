using UnityEngine;
using Sanctuary.Serialization;

#if UNITY_NEWTONSOFT_JSON

namespace Sanctuary.Configuration
{
    [CreateAssetMenu(fileName = "JsonSerializerConfiguration", menuName = "Sanctuary/Configuration/Serializers/Json Serializer", order = 2)]
    public class JsonSerializerConfiguration : SerializerConfiguration
    {
        /// <summary>
        /// Gets the <see cref="JsonSerializer"/> instance based on the configuration.
        /// </summary>
        /// <returns>The configured <see cref="JsonSerializer"/> instance.</returns>
        public override ISerializer GetSerializer() => JsonSerializer.Create(serializationOptions, overrideExtension);

        /// <summary>
        /// Gets the <see cref="JsonSerializer"/> instance based on the provided serialization options and an optional override extension.
        /// </summary>
        /// <param name="options">The serialization options to use for this serializer.</param>
        /// <param name="overrideExtension">The file extension to use for serialized files. If null, the default extension will be used.</param>
        /// <returns>The configured <see cref="JsonSerializer"/> instance.</returns>
        public override ISerializer GetSerializer(SerializationOptions options, string overrideExtension = null) => overrideExtension != null ? JsonSerializer.Create(options, overrideExtension) : JsonSerializer.Create(options);
    }
}

#endif