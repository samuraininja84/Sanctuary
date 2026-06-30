using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sanctuary
{
    public sealed class SanctuaryService : ISanctuaryService
    {
        private const string RegistryFile = "_sanctuary_index.json";

        private readonly ISaveDataProvider m_Provider;
        private readonly ISaveSerializer m_Serializer;
        private readonly ISaveIntegrityValidator m_Validator;
        private readonly ISanctuaryLogger m_Logger;
        private readonly SaveSlotRegistry m_SlotRegistry;
        private readonly SaveMigrationPipeline m_MigrationPipeline;

        private bool m_SaveInProgress;

        SanctuaryService(ISaveDataProvider provider, ISaveSerializer serializer, ISaveIntegrityValidator validator, ISanctuaryLogger logger)
        {
            m_Provider = provider;
            m_Serializer = serializer;
            m_Validator = validator;
            m_Logger = logger;
            m_SlotRegistry = new SaveSlotRegistry();
            m_MigrationPipeline = new SaveMigrationPipeline();
        }

        public static SanctuaryService Create(ISaveDataProvider provider, ISaveSerializer serializer, ISaveIntegrityValidator validator, ISanctuaryLogger logger = null) => new(provider, serializer, validator, logger);

        public async Task LoadRegistryAsync()
        {
            // Attempt to read the registry file from the provider
            var data = await m_Provider.ReadAsync(RegistryFile);

            // If the registry file exists and has data, load it into the slot registry
            if (data != null && data.Length > 0)
            {
                // Deserialize the registry data
                var loaded = SaveSlotRegistry.FromBytes(data);

                // Register all loaded slots in the current registry
                foreach (var slot in loaded.GetAllSlots())
                {
                    // Register each slot in the current registry
                    m_SlotRegistry.RegisterSlot(slot.SlotId, slot);
                }
            }
        }

        public void RegisterMigrationStep(ISaveMigrationStep step) => m_MigrationPipeline.RegisterStep(step);

        public SaveSlotInfo[] GetAvailableSlots() => m_SlotRegistry.GetAllSlots();

        public SaveSlotInfo GetSlot(string slotId) => m_SlotRegistry.GetSlot(slotId);

        public async Task<SaveResult> SaveAsync<T>(string slotId, T data) where T : class
        {
            // Check if a save operation is already in progress, and if so, return a failure result indicating that a save is already in progress
            if (m_SaveInProgress) return SaveResult.Fail(SaveFailureReason.SaveAlreadyInProgress);

            // Mark that a save operation is in progress
            m_SaveInProgress = true;

            try
            {
                // Use the internal save method to perform the actual save operation
                return await SaveInternalAsync(slotId, data);
            }
            finally
            {
                // Ensure that the save in progress flag is reset even if an exception occurs
                m_SaveInProgress = false;
            }
        }

        public async Task<LoadResult<T>> LoadAsync<T>(string slotId) where T : class
        {
            // Retrieve the slot information for the given slot ID from the slot registry
            var slotInfo = m_SlotRegistry.GetSlot(slotId);

            // If no slot information is found for the given slot ID, return a failure result indicating that no valid save slot was found
            if (slotInfo == null) return LoadResult<T>.Fail(LoadStatus.NoValidSave, $"No save slot '{slotId}' found");

            // Try current file
            var result = await TryLoadFileAsync<T>(slotInfo.CurrentFile, slotInfo);

            // If the current file is valid, return the result
            if (result.Success) return result;

            // If the current file is invalid, check if a backup file exists and attempt to load from it
            if (!string.IsNullOrEmpty(slotInfo.BackupFile))
            {
                // Log a warning indicating that the primary save is corrupt and that the system will attempt to load from the backup
                m_Logger?.Warn($"Primary save corrupt for '{slotId}', trying backup");

                // Attempt to load from the backup file
                result = await TryLoadFileAsync<T>(slotInfo.BackupFile, slotInfo);

                // If the backup load is successful, return a result indicating that the data was loaded from the backup
                if (result.Success)
                {
                    // If the backup load is successful, return a result indicating that the data was loaded from the backup
                    return result.Status == LoadStatus.SuccessMigrated
                        ? LoadResult<T>.MigratedFromBackup(result.Data, slotInfo)
                        : LoadResult<T>.FromBackup(result.Data, slotInfo);
                }
            }

            // If both current and backup are invalid, return a failure result indicating that no valid save was found
            return LoadResult<T>.Fail(LoadStatus.NoValidSave, $"Both primary and backup saves for '{slotId}' are invalid");
        }

        public async Task<bool> DeleteAsync(string slotId)
        {
            // Retrieve the slot information for the given slot ID from the slot registry
            var slotInfo = m_SlotRegistry.GetSlot(slotId);

            // If the slot does not exist, return false to indicate that there was nothing to delete
            if (slotInfo == null) return false;

            // Delete the current file if it exists
            if (!string.IsNullOrEmpty(slotInfo.CurrentFile)) await m_Provider.DeleteAsync(slotInfo.CurrentFile);

            // Delete the backup file if it exists
            if (!string.IsNullOrEmpty(slotInfo.BackupFile)) await m_Provider.DeleteAsync(slotInfo.BackupFile);

            // Remove the slot from the registry
            m_SlotRegistry.RemoveSlot(slotId);

            // Persist the updated registry to ensure that the deleted slot is no longer tracked
            await PersistRegistryAsync();

            // Return true to indicate that the slot was successfully deleted
            return true;
        }

        public async Task<bool> ExistsAsync(string slotId)
        {
            // Retrieve the slot information for the given slot ID from the slot registry
            var slotInfo = m_SlotRegistry.GetSlot(slotId);

            // If the slot does not exist, return false to indicate that there is no valid save for the given slot ID
            if (slotInfo == null) return false;

            // Check if either the current file or backup file exists in the save data provider
            var currentExists = !string.IsNullOrEmpty(slotInfo.CurrentFile) && await m_Provider.ExistsAsync(slotInfo.CurrentFile);
            var backupExists = !string.IsNullOrEmpty(slotInfo.BackupFile) && await m_Provider.ExistsAsync(slotInfo.BackupFile);

            // Return true if either the current file or backup file exists, indicating that there is a valid save for the given slot ID
            return currentExists || backupExists;
        }

        private async Task<SaveResult> SaveInternalAsync<T>(string slotId, T data) where T : class
        {
            // 1. Initialize a byte array to hold the serialized data
            byte[] serialized;

            // Try to serialize the provided data using the save serializer, and catch any exceptions that may occur during serialization
            try
            {
                // Serialize the provided data into a byte array using the save serializer
                serialized = m_Serializer.Serialize(data);
            }
            catch (Exception ex)
            {
                // If an exception occurs during serialization, log the error and return a failure result indicating that serialization failed
                m_Logger?.Error($"Serialization failed: {ex.Message}");
                return SaveResult.Fail(SaveFailureReason.SerializationFailed);
            }

            // 2. Embed a checksum into the serialized data to ensure data integrity during save and load operations
            serialized = EmbedChecksum(serialized);

            // 3. Write to temp file (unique name so a same-second save can't overwrite the slot's current/backup)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var tempFile = $"{slotId}_{timestamp}.sav";
            var attempt = 2;

            // Keep generating a new temporary file name until we find one that does not already exist in the save data provider
            while (await m_Provider.ExistsAsync(tempFile)) tempFile = $"{slotId}_{timestamp}_{attempt++}.sav";

            // Attempt to write the serialized data to the temporary file using the save data provider
            var writeSuccess = await m_Provider.WriteAsync(tempFile, serialized);

            // If the write operation fails, return a failure result indicating that the write operation failed
            if (!writeSuccess) return SaveResult.Fail(SaveFailureReason.WriteFailed);

            // 4. Read the file back and validate integrity to ensure that the data was written correctly and is not corrupted
            var readBack = await m_Provider.ReadAsync(tempFile);
            var integrity = m_Validator.Validate(readBack);

            // If the integrity check fails, delete the temporary file and return a failure result indicating that post-write validation failed
            if (!integrity.IsValid)
            {
                await m_Provider.DeleteAsync(tempFile);
                m_Logger?.Error($"Post-write validation failed: {integrity.Reason}");
                return SaveResult.Fail(SaveFailureReason.PostWriteValidationFailed);
            }

            // 5. Swap: old current → backup, new → current
            var slotInfo = m_SlotRegistry.GetSlot(slotId) ?? new SaveSlotInfo { SlotId = slotId };

            // Delete the old backup file if it exists to free up space and avoid clutter
            if (!string.IsNullOrEmpty(slotInfo.BackupFile)) await m_Provider.DeleteAsync(slotInfo.BackupFile);

            // Update the slot information to reflect the new current and backup files, as well as the last save time and schema version
            slotInfo.BackupFile = slotInfo.CurrentFile;
            slotInfo.CurrentFile = tempFile;
            slotInfo.LastSaveTime = DateTime.UtcNow;
            slotInfo.SchemaVersion = m_Serializer.CurrentSchemaVersion;

            // Register the updated slot information in the slot registry to ensure that it is tracked and can be retrieved in future sessions
            m_SlotRegistry.RegisterSlot(slotId, slotInfo);

            // 6. Persist the updated slot registry to ensure that the new save slot information is saved and can be retrieved in future sessions
            await PersistRegistryAsync();

            // 7. Return a success result indicating that the save operation was successful and providing the path to the temporary file
            return SaveResult.Succeed(tempFile);
        }

        private async Task<LoadResult<T>> TryLoadFileAsync<T>(string filePath, SaveSlotInfo slotInfo) where T : class
        {
            // If the file path is null or empty, return a failure result indicating that there is no valid save
            if (string.IsNullOrEmpty(filePath)) return LoadResult<T>.Fail(LoadStatus.NoValidSave, "File path is empty");

            // Attempt to read the raw data from the specified file path using the save data provider
            byte[] rawData;

            // Try to read the file and catch any exceptions that may occur during the read operation
            try
            {
                // Read the raw data from the specified file path using the save data provider
                rawData = await m_Provider.ReadAsync(filePath);
            }
            catch (Exception ex)
            {
                // If an exception occurs during the read operation, return a failure result indicating that there was a provider error
                return LoadResult<T>.Fail(LoadStatus.ProviderError, ex.Message);
            }

            // If the raw data is null, it means that the file was not found or could not be read, so return a failure result indicating that there is no valid save
            if (rawData == null) return LoadResult<T>.Fail(LoadStatus.NoValidSave, $"File not found: {filePath}");

            // Validate integrity
            var integrity = m_Validator.Validate(rawData);

            // If the integrity check fails, return a failure result indicating that there is no valid save due to integrity check failure
            if (!integrity.IsValid) return LoadResult<T>.Fail(LoadStatus.NoValidSave, $"Integrity check failed: {integrity.Reason}");

            // Deserialize the raw data into the target type T using the save serializer
            var deserialized = m_Serializer.Deserialize<T>(rawData);

            // If deserialization fails, attempt to migrate and deserialize the data
            if (!deserialized.Success) return await TryMigrateAndDeserializeAsync<T>(rawData, slotInfo);

            // If the schema version of the deserialized data is less than the current schema version, it means that migration is needed
            if (deserialized.SchemaVersion < m_Serializer.CurrentSchemaVersion)
            {
                // Try to migrate and deserialize the data to the current schema version
                return await TryMigrateAndDeserializeAsync<T>(rawData, slotInfo);
            }

            // If deserialization is successful and no migration is needed, return a success result with the deserialized data and slot information
            return LoadResult<T>.Succeed(deserialized.Data, slotInfo);
        }

        private Task<LoadResult<T>> TryMigrateAndDeserializeAsync<T>(byte[] rawData, SaveSlotInfo slotInfo) where T : class
        {
            try
            {
                // Convert the raw byte array into a JSON string
                var envelopeJson = Encoding.UTF8.GetString(rawData);

                // Deserialize the JSON string into a SaveEnvelope object to extract the schema version and data
                var envelope = JsonConvert.DeserializeObject<SaveEnvelope>(envelopeJson);

                // If the envelope is null, it means that the deserialization failed, and we cannot proceed with migration
                if (envelope == null)
                {
                    // If the envelope cannot be deserialized, return a failure result indicating that the migration failed
                    return Task.FromResult(LoadResult<T>.Fail(LoadStatus.MigrationFailed, "Cannot read save envelope for migration"));
                }

                // Create a migration pipeline and attempt to migrate the data from the old schema version to the current schema version
                var migration = m_MigrationPipeline.Migrate(envelope.DataJson, envelope.SchemaVersion, m_Serializer.CurrentSchemaVersion);

                // If migration fails, return a failure result indicating that the migration failed
                if (!migration.Success)
                {
                    // If migration fails, return a failure result indicating that the migration failed
                    return Task.FromResult(LoadResult<T>.Fail(LoadStatus.MigrationFailed, migration.ErrorMessage));
                }

                // Deserialize the migrated JSON into the target type T
                var data = JsonConvert.DeserializeObject<T>(migration.MigratedJson);

                // If deserialization fails after migration, return a failure result indicating that the migration failed
                if (data == null)
                {
                    // If deserialization fails after migration, return a failure result indicating that the migration failed
                    return Task.FromResult(LoadResult<T>.Fail(LoadStatus.MigrationFailed, "Deserialization failed after migration"));
                }

                // If migration and deserialization are successful, return a result indicating that the data was migrated
                return Task.FromResult(LoadResult<T>.Migrated(data, slotInfo));
            }
            catch (Exception ex)
            {
                // Return a failure result indicating that the migration process failed due to an exception
                return Task.FromResult(LoadResult<T>.Fail(LoadStatus.MigrationFailed, $"Migration error: {ex.Message}"));
            }
        }

        private byte[] EmbedChecksum(byte[] serializedData)
        {
            // Deserialize the serialized data to extract the JSON string
            var json = Encoding.UTF8.GetString(serializedData);

            // Convert the JSON string into a SaveEnvelope object
            var envelope = JsonConvert.DeserializeObject<SaveEnvelope>(json);

            // Get the bytes of the DataJson property and generate a checksum for it
            var dataBytes = Encoding.UTF8.GetBytes(envelope.DataJson);
            envelope.Checksum = m_Validator.GenerateChecksum(dataBytes);

            // Update the envelope with the new checksum and serialize it back to JSON
            var updatedJson = JsonConvert.SerializeObject(envelope);
            return Encoding.UTF8.GetBytes(updatedJson);
        }

        private async Task PersistRegistryAsync()
        {
            var registryData = m_SlotRegistry.ToBytes();
            await m_Provider.WriteAsync(RegistryFile, registryData);
        }
    }
}
