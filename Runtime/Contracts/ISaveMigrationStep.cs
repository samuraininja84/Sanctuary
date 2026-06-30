namespace Sanctuary
{
    public interface ISaveMigrationStep
    {
        int FromVersion { get; }

        int ToVersion { get; }

        string Migrate(string rawJson);
    }
}