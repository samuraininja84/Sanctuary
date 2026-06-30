namespace Sanctuary.Configuration
{
    /// <summary>
    /// Plain-C# default <see cref="ISanctuaryConfiguration"/>. Used when no adapter or consumer
    /// override is registered. Resolves <c>RootPath</c> to a relative <c>"Saves"</c> directory
    /// and stamps new saves with schema version 1.
    /// </summary>
    public sealed class DefaultSanctuaryConfiguration : ISanctuaryConfiguration
    {
        public string RootPath { get; }

        public int CurrentSchemaVersion { get; }

        public DefaultSanctuaryConfiguration(string rootPath = "Saves", int currentSchemaVersion = 1)
        {
            RootPath = rootPath;
            CurrentSchemaVersion = currentSchemaVersion;
        }
    }
}
