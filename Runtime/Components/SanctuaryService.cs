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
        private readonly SaveSlotRegistry m_SlotRegistry;
        private readonly SaveMigrationPipeline m_MigrationPipeline;

        private bool m_SaveInProgress;

        public SanctuaryService(ISaveDataProvider provider, ISaveSerializer serializer, ISaveIntegrityValidator validator)
        {
            m_Provider = provider;
            m_Serializer = serializer;
            m_Validator = validator;
            m_SlotRegistry = new SaveSlotRegistry();
            m_MigrationPipeline = new SaveMigrationPipeline();
        }

        public async Task LoadRegistryAsync()
        {
            var data = await m_Provider.ReadAsync(RegistryFile);

            if (data != null && data.Length > 0)
            {
                var loaded = SaveSlotRegistry.FromBytes(data);
                foreach (var slot in loaded.GetAllSlots())
                {
                    m_SlotRegistry.RegisterSlot(slot.SlotId, slot);
                }
            }
        }

        public void RegisterMigrationStep(ISaveMigrationStep step) => m_MigrationPipeline.RegisterStep(step);

        public SaveSlotInfo[] GetAvailableSlots() => m_SlotRegistry.GetAllSlots();

        public SaveSlotInfo GetSlot(string slotId) => m_SlotRegistry.GetSlot(slotId);

        public async Task<SaveResult> SaveAsync<T>(string slotId, T data) where T : class
        {
            if (m_SaveInProgress) return SaveResult.Fail(SaveFailureReason.SaveAlreadyInProgress);

            m_SaveInProgress = true;

            try
            {
                return await SaveInternalAsync(slotId, data);
            }
            finally
            {
                m_SaveInProgress = false;
            }
        }

        public async Task<SaveLoadResult<T>> LoadAsync<T>(string slotId) where T : class
        {
            var slotInfo = m_SlotRegistry.GetSlot(slotId);

            if (slotInfo == null)
            {
                return SaveLoadResult<T>.Fail(SaveLoadStatus.NoValidSave, $"No save slot '{slotId}' found");
            }

            // Try current file
            var result = await TryLoadFileAsync<T>(slotInfo.CurrentFile, slotInfo);

            if (result.Success) return result;

            // Try backup
            if (!string.IsNullOrEmpty(slotInfo.BackupFile))
            {
                //m_Logger.Warn($"Primary save corrupt for '{slotId}', trying backup");
                result = await TryLoadFileAsync<T>(slotInfo.BackupFile, slotInfo);

                if (result.Success)
                {
                    return result.Status == SaveLoadStatus.SuccessMigrated
                        ? SaveLoadResult<T>.MigratedFromBackup(result.Data, slotInfo)
                        : SaveLoadResult<T>.FromBackup(result.Data, slotInfo);
                }
            }

            return SaveLoadResult<T>.Fail(SaveLoadStatus.NoValidSave, $"Both primary and backup saves for '{slotId}' are invalid");
        }

        public async Task<bool> DeleteSlotAsync(string slotId)
        {
            var slotInfo = m_SlotRegistry.GetSlot(slotId);

            if (slotInfo == null) return false;

            if (!string.IsNullOrEmpty(slotInfo.CurrentFile))
            {
                await m_Provider.DeleteAsync(slotInfo.CurrentFile);
            }

            if (!string.IsNullOrEmpty(slotInfo.BackupFile))
            {
                await m_Provider.DeleteAsync(slotInfo.BackupFile);
            }

            m_SlotRegistry.RemoveSlot(slotId);
            await PersistRegistryAsync();

            return true;
        }

        private async Task<SaveResult> SaveInternalAsync<T>(string slotId, T data) where T : class
        {
            // 1. Serialize
            byte[] serialized;

            try
            {
                serialized = m_Serializer.Serialize(data);
            }
            catch (Exception ex)
            {
                //m_Logger.Error($"Serialization failed: {ex.Message}");
                return SaveResult.Fail(SaveFailureReason.SerializationFailed);
            }

            // 2. Embed checksum
            serialized = EmbedChecksum(serialized);

            // 3. Write to temp file (unique name so a same-second save can't overwrite the slot's current/backup)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var tempFile = $"{slotId}_{timestamp}.sav";
            var attempt = 2;
            while (await m_Provider.ExistsAsync(tempFile))
                tempFile = $"{slotId}_{timestamp}_{attempt++}.sav";

            var writeSuccess = await m_Provider.WriteAsync(tempFile, serialized);

            if (!writeSuccess)
            {
                return SaveResult.Fail(SaveFailureReason.WriteFailed);
            }

            // 4. Read back and validate
            var readBack = await m_Provider.ReadAsync(tempFile);
            var integrity = m_Validator.Validate(readBack);

            if (!integrity.IsValid)
            {
                await m_Provider.DeleteAsync(tempFile);
                //m_Logger.Error($"Post-write validation failed: {integrity.Reason}");
                return SaveResult.Fail(SaveFailureReason.PostWriteValidationFailed);
            }

            // 5. Swap: old current → backup, new → current
            var slotInfo = m_SlotRegistry.GetSlot(slotId) ?? new SaveSlotInfo { SlotId = slotId };

            if (!string.IsNullOrEmpty(slotInfo.BackupFile))
            {
                await m_Provider.DeleteAsync(slotInfo.BackupFile);
            }

            slotInfo.BackupFile = slotInfo.CurrentFile;
            slotInfo.CurrentFile = tempFile;
            slotInfo.LastSaveTime = DateTime.UtcNow;
            slotInfo.SchemaVersion = m_Serializer.CurrentSchemaVersion;

            m_SlotRegistry.RegisterSlot(slotId, slotInfo);
            await PersistRegistryAsync();

            return SaveResult.Succeed(tempFile);
        }

        private async Task<SaveLoadResult<T>> TryLoadFileAsync<T>(string filePath, SaveSlotInfo slotInfo) where T : class
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return SaveLoadResult<T>.Fail(SaveLoadStatus.NoValidSave, "File path is empty");
            }

            byte[] rawData;

            try
            {
                rawData = await m_Provider.ReadAsync(filePath);
            }
            catch (Exception ex)
            {
                return SaveLoadResult<T>.Fail(SaveLoadStatus.ProviderError, ex.Message);
            }

            if (rawData == null)
            {
                return SaveLoadResult<T>.Fail(SaveLoadStatus.NoValidSave, $"File not found: {filePath}");
            }

            // Validate integrity
            var integrity = m_Validator.Validate(rawData);

            if (!integrity.IsValid)
            {
                return SaveLoadResult<T>.Fail(SaveLoadStatus.NoValidSave,
                    $"Integrity check failed: {integrity.Reason}");
            }

            // Deserialize
            var deserialized = m_Serializer.Deserialize<T>(rawData);

            if (!deserialized.Success)
            {
                // Try migration if version mismatch
                return await TryMigrateAndDeserializeAsync<T>(rawData, slotInfo);
            }

            // Check if migration needed
            if (deserialized.SchemaVersion < m_Serializer.CurrentSchemaVersion)
            {
                return await TryMigrateAndDeserializeAsync<T>(rawData, slotInfo);
            }

            return SaveLoadResult<T>.Succeed(deserialized.Data, slotInfo);
        }

        private Task<SaveLoadResult<T>> TryMigrateAndDeserializeAsync<T>(byte[] rawData, SaveSlotInfo slotInfo) where T : class
        {
            try
            {
                var envelopeJson = Encoding.UTF8.GetString(rawData);
                var envelope = JsonConvert.DeserializeObject<SaveEnvelope>(envelopeJson);

                if (envelope == null)
                {
                    return Task.FromResult(
                        SaveLoadResult<T>.Fail(SaveLoadStatus.MigrationFailed, "Cannot read save envelope for migration"));
                }

                var migration = m_MigrationPipeline.Migrate(
                    envelope.DataJson,
                    envelope.SchemaVersion,
                    m_Serializer.CurrentSchemaVersion
                );

                if (!migration.Success)
                {
                    return Task.FromResult(
                        SaveLoadResult<T>.Fail(SaveLoadStatus.MigrationFailed, migration.ErrorMessage));
                }

                var data = JsonConvert.DeserializeObject<T>(migration.MigratedJson);

                if (data == null)
                {
                    return Task.FromResult(
                        SaveLoadResult<T>.Fail(SaveLoadStatus.MigrationFailed, "Deserialization failed after migration"));
                }

                return Task.FromResult(SaveLoadResult<T>.Migrated(data, slotInfo));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    SaveLoadResult<T>.Fail(SaveLoadStatus.MigrationFailed, $"Migration error: {ex.Message}"));
            }
        }

        private byte[] EmbedChecksum(byte[] serializedData)
        {
            var json = Encoding.UTF8.GetString(serializedData);
            var envelope = JsonConvert.DeserializeObject<SaveEnvelope>(json);

            var dataBytes = Encoding.UTF8.GetBytes(envelope.DataJson);
            envelope.Checksum = m_Validator.GenerateChecksum(dataBytes);

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
