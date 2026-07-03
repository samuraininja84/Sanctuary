using System.IO;
using System.Threading.Tasks;
using Sanctuary.Serialization;

namespace Sanctuary.Configuration
{
    /// <summary>
    /// Plain-C# default <see cref="IStreamConfiguration"/>. Used when no adapter or consumer
    /// override is registered. Resolves <c>RootPath</c> to a relative <c>"Saves"</c> directory
    /// and stamps new saves with schema version 1.
    /// </summary>
    public readonly struct DefaultStreamConfiguration : IStreamConfiguration
    {
        private readonly string m_RootPath;
        private readonly int m_CurrentSchemaVersion;
        private readonly SerializationOptions m_Options;

        public readonly string RootPath => m_RootPath;

        public readonly int CurrentSchemaVersion => m_CurrentSchemaVersion;

        public SerializationOptions Options => m_Options;

        public DefaultStreamConfiguration(string rootPath = "Save Data", SerializationOptions options = SerializationOptions.None, int currentSchemaVersion = 1)
        {
            m_RootPath = rootPath;
            m_Options = options;
            m_CurrentSchemaVersion = currentSchemaVersion;
        }

        public async Task<Stream> GetStream(StreamType streamType, string filePath = null)
        {
            return streamType switch
            {
                StreamType.Serialization => SerializationExtensions.CreateFileSerializationStream(filePath),
                StreamType.Deserialization => await SerializationExtensions.CreateFileDeserializationStream(filePath),
                StreamType.Backup => await SerializationExtensions.CreateFileBackupStream(filePath, SerializationExtensions.DefaultBackupExtension),

                // Should never reach this point because all enum values are handled above, but this is a safeguard in case of future changes to the enum.
                _ => throw new System.ArgumentOutOfRangeException(nameof(streamType), streamType, null)
            };
        }
    }
}
