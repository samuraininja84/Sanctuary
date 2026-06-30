namespace Sanctuary.Configuration
{
    /// <summary>
    /// Plain-C# default <see cref="IStreamConfiguration"/>. Used when no adapter or consumer
    /// override is registered. Resolves <c>RootPath</c> to a relative <c>"Saves"</c> directory
    /// and stamps new saves with schema version 1.
    /// </summary>
    public sealed class DefaultStreamConfiguration : IStreamConfiguration
    {
        public string RootPath { get; }

        public int CurrentSchemaVersion { get; }

        public DefaultStreamConfiguration(string rootPath = "Saves", int currentSchemaVersion = 1)
        {
            RootPath = rootPath;
            CurrentSchemaVersion = currentSchemaVersion;
        }
    }
}
