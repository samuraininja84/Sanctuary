using System.Collections.Generic;

namespace Sanctuary
{
    public sealed class SaveMigrationPipeline
    {
        private readonly SortedDictionary<int, ISaveMigrationStep> m_Steps = new();

        public void RegisterStep(ISaveMigrationStep step) => m_Steps[step.FromVersion] = step;

        public MigrationResult Migrate(string rawJson, int fromVersion, int toVersion)
        {
            // If the fromVersion is greater than or equal to the toVersion, no migration is needed.
            if (fromVersion >= toVersion) return MigrationResult.Succeed(rawJson, fromVersion);

            // If there are no registered migration steps, we cannot perform any migrations.
            var currentJson = rawJson;

            // Initialize the current version to the starting version.
            var currentVersion = fromVersion;

            // Loop through the versions, applying each migration step in sequence until we reach the target version.
            while (currentVersion < toVersion)
            {
                // Attempt to retrieve the migration step for the current version.
                if (!m_Steps.TryGetValue(currentVersion, out var step))
                {
                    // If there is no migration step for the current version, return a failure result.
                    return MigrationResult.Fail($"Missing migration step from v{currentVersion} to v{currentVersion + 1}");
                }

                // Ensure that the migration step is valid and matches the expected version transition.
                if (step.ToVersion != currentVersion + 1)
                {
                    // If the migration step does not match the expected version transition, return a failure result.
                    return MigrationResult.Fail(
                        $"Migration step mismatch: expected v{currentVersion}→v{currentVersion + 1}, " +
                        $"got v{step.FromVersion}→v{step.ToVersion}"
                    );
                }

                // Perform the migration step.
                currentJson = step.Migrate(currentJson);

                // If the migration step returns null, it indicates a failure in the migration process.
                if (currentJson == null) return MigrationResult.Fail($"Migration step v{currentVersion}→v{step.ToVersion} returned null");

                // Update the current version to the next version after successful migration.
                currentVersion = step.ToVersion;
            }

            // If we reach here, it means we have successfully migrated to the target version.
            return MigrationResult.Succeed(currentJson, currentVersion);
        }
    }
}
