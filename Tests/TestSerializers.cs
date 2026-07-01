using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Configuration;
using Sanctuary.Serialization;
using CancellationToken = System.Threading.CancellationToken;

namespace Sanctuary.Tests
{
    public class TestSerializers
    {
        public const string TestFolderName = "Save_Data_Tests";
        public const string TestChunkId = "Tests";
        public const string TestObjectId = "5561391260475779002";
        public const int BenchmarkIterations = 100;

        #region Old Serializer Tests

        #region Binary Serializer Tests

        [Test]
        public async Task TestBinaryBenchmark()
        {
            // Run the benchmark for both binary and JSON serializers for the specified number of iterations
            for (int i = 0; i < BenchmarkIterations; i++) await TestSerializer(BinarySerializer.Default, "BinaryData", false, true, false);

            // Log a message indicating that the benchmark is complete
            Debug.Log($"[Sanctuary]: Benchmark complete for {BenchmarkIterations} iterations of binary serialization.");
        }

        [Test]
        public async Task TestBinaryData() => await TestSerializer(BinarySerializer.Default, "BinaryData", false, true);

        [Test]
        public async Task TestBinaryCorruption() => await TestSerializer(BinarySerializer.Backup, "BinaryCorruption", true, true);

        [Test]
        public async Task TestBinaryCompressed() => await TestSerializer(BinarySerializer.Compressed, "BinaryCompressed", false, true);

        [Test]
        public async Task TestBinaryBackup() => await TestSerializer(BinarySerializer.Backup, "BinaryBackup", false, true);

        [Test]
        public async Task TestBinaryBackupCompressed() => await TestSerializer(BinarySerializer.BackupCompressed, "BinaryBackupCompressed", false, true);

        [Test]
        public async Task TestBinaryBackupEncrypted() => await TestSerializer(BinarySerializer.BackupEncrypted, "BinaryBackupEncrypted", false, true);

        [Test]
        public async Task TestBinaryCompressionEncrypted() => await TestSerializer(BinarySerializer.CompressionEncrypted, "BinaryCompressionEncrypted", false, true);

        [Test]
        public async Task TestBinaryAll() => await TestSerializer(BinarySerializer.All, "BinaryAll", false, true);

        #region Other Binary Serializer Tests

        [Test]
        public async Task TestBinary() => await TestSerializer(BinarySerializer.CreateAsBinary(SerializationOptions.None), "Binary", false, true);

        #endregion

        #endregion

        #region Json Serializer Tests

#if UNITY_NEWTONSOFT_JSON

        [Test]
        public async Task TestJsonBenchmark()
        {
            // Run the benchmark for both binary and JSON serializers for the specified number of iterations
            for (int i = 0; i < BenchmarkIterations; i++) await TestSerializer(JsonSerializer.Default, "JsonData", false, true, false);

            // Log a message indicating that the benchmark is complete
            Debug.Log($"[Sanctuary]: Benchmark complete for {BenchmarkIterations} iterations of JSON serialization.");
        }

        [Test]
        public async Task TestJsonData() => await TestSerializer(JsonSerializer.Default, "JsonData", false, true);

        [Test]
        public async Task TestJsonCorruption() => await TestSerializer(JsonSerializer.Backup, "JsonCorruption", true, true);

        [Test]
        public async Task TestJsonCompressed() => await TestSerializer(JsonSerializer.Compressed, "JsonCompressed", false, true);

        [Test]
        public async Task TestJsonBackup() => await TestSerializer(JsonSerializer.Backup, "JsonBackup", false, true);

        [Test]
        public async Task TestJsonBackupCompressed() => await TestSerializer(JsonSerializer.BackupCompressed, "JsonBackupCompressed", false, true);

        [Test]
        public async Task TestJsonBackupEncrypted() => await TestSerializer(JsonSerializer.BackupEncrypted, "JsonBackupEncrypted", false, true);

        [Test]
        public async Task TestJsonCompressionEncrypted() => await TestSerializer(JsonSerializer.CompressionEncrypted, "JsonCompressionEncrypted", false, true);

        [Test]
        public async Task TestJsonAll() => await TestSerializer(JsonSerializer.All, "JsonAll", false, true);

        #region Other Json Serializer Tests

        [Test]
        public async Task TestJson() => await TestSerializer(JsonSerializer.CreateAsJson(SerializationOptions.None), "Json", false, true);

        [Test]
        public async Task TestMarkdown() => await TestSerializer(JsonSerializer.CreateAsMarkDown(SerializationOptions.None), "Markdown", false, true);

        [Test]
        public async Task TestText() => await TestSerializer(JsonSerializer.CreateAsText(SerializationOptions.None), "Text", false, true);

        #endregion

#endif

        #endregion

        #region Serializer Test Helper Methods

        public async Task TestSerializer(ISerializer serializer, string fileName, bool testBackups = false, bool deleteAfterTest = false, bool debug = true)
        {
            // Create a new instance of the save data
            ISaveData saveData = SaveData.Empty;

            // Create a save location for the test data
            SaveLocation location = new(TestChunkId, TestObjectId);

            // Create a new instance of the test data
            TestSaveData data = new("John Doe", 30, 5.9f, new string[] { "Reading", "Gaming", "Hiking" });

            // Write the test data to the save data
            saveData.Write(location, data);

            // Set the folder path for the save data to be serialized to the tests folder in the persistent data path
            string folderPath = Path.Combine(Application.persistentDataPath, TestFolderName);

            // Append the appropriate file extension to the file name based on the serializer being used
            fileName += serializer.GetFileExtension();

            // Set the file path for the save data
            string filePath = Path.Combine(folderPath, fileName);

            // Create a source stream for file serialization using the appropriate serializer and file path
            using var source = SerializationExtensions.CreateFileSerializationStream(filePath);

            // Try to serialize the save data using the binary serializer
            await serializer.Serialize(saveData, source);

            // Log the file path for debugging purposes
            if (debug) Debug.Log($"[Sanctuary]: Serialized {fileName} save data to: {filePath}");

            // If testBackups is true, check if a backup file was created and log the result
            if (testBackups) await TestBackups(serializer, filePath, debug);

            // Try to deserialize the save data using the appropriate serializer
            await TestDeserialization(serializer, location, filePath, debug);

            // Clean up the test data file after the test is complete
            if (deleteAfterTest) await Cleanup(folderPath, filePath, debug);
        }

        private static async Task TestBackups(ISerializer serializer, string filePath, bool debug)
        {
            // Create a source stream for file serialization using the appropriate serializer and file path.
            using (var source = SerializationExtensions.CreateCorruptionStream(filePath))
            {
                // Create the backup stream if the serializer has the backup option enabled
                using var backup = await SerializationExtensions.CreateFileBackupStream(filePath, SerializationExtensions.DefaultBackupExtension);

                // If the serializer has the backup option enabled, try to serialize the save data to the backup stream as well
                await serializer.CopyTo(source, backup, CancellationToken.None);
            }

            // Run the backup file existence check in a separate task to avoid blocking the main thread.
            await Task.Run(() =>
            {
                // Set the backup file path for the save data
                string backupFilePath = filePath + SerializationExtensions.DefaultBackupExtension;

                // Try to check if the backup file exists and log the result
                try
                {
                    // Check if the backup file exists
                    if (File.Exists(backupFilePath))
                    {
                        // Log the backup file path for debugging purposes
                        Debug.Log($"[Sanctuary]: Backup file created: {backupFilePath}");

                        // Delete the original file to simulate a failure and test the rollback functionality
                        File.Delete(filePath);

                        // Log the deletion of the original file for debugging purposes
                        if (debug) Debug.Log($"[Sanctuary]: Deleted original file to test rollback.");
                    }
                }
                catch (System.Exception ex)
                {
                    // Log the exception message for debugging purposes
                    if (debug) Debug.LogError($"[Sanctuary]: Exception occurred while checking for backup file: {ex.Message}");
                }
            });
        }

        private static async Task TestDeserialization(ISerializer serializer, SaveLocation location, string filePath, bool debug)
        {
            // Await the creation of a file deserialization stream for the save data file
            using var stream = await SerializationExtensions.CreateFileDeserializationStream(filePath);

            // Try to deserialize the save data using the appropriate serializer
            var result = await serializer.Deserialize(stream);

            // Log the file path for debugging purposes
            if (debug) Debug.Log($"[Sanctuary]: Deserialized save data from: {filePath}");

            // Try to read the test data from the save data
            try
            {
                // Try to read the test data from the save data using the specified location
                var data = result.Data.Read<TestSaveData>(location);

                // Log the result status for debugging purposes
                switch (result.Status)
                {
                    case LoadStatus.Success:
                        if (debug) Debug.Log("[Sanctuary]: Deserialized Test Data");
                        break;
                    case LoadStatus.SuccessFromBackup:
                        if (debug) Debug.Log("[Sanctuary]: Deserialized Test Data From Backup");
                        break;
                    case LoadStatus.SuccessMigrated:
                        if (debug) Debug.Log("[Sanctuary]: Deserialized Test Data From Migrated Save");
                        break;
                    case LoadStatus.SuccessMigratedFromBackup:
                        if (debug) Debug.Log("[Sanctuary]: Deserialized Test Data From Migrated Backup");
                        break;
                    case LoadStatus.NoValidSave:
                        if (debug) Debug.LogWarning($"[Sanctuary]: Failed to load save from persistent storage. {result.Message}");
                        break;
                    case LoadStatus.ProviderError:
                        if (debug) Debug.LogWarning($"[Sanctuary]: Failed to load save from persistent storage due to a provider error. {result.Message}");
                        break;
                    case LoadStatus.MigrationFailed:
                        if (debug) Debug.LogWarning($"[Sanctuary]: Failed to migrate save from persistent storage. {result.Message}");
                        break;
                }

                // Log the test data for debugging purposes
                if (debug) Debug.Log($"[Sanctuary]: Name={data.Name}, Age={data.Age}, Height={data.Height}, Hobbies={string.Join(", ", data.Hobbies)}");
            }
            catch (System.Exception ex)
            {
                if (debug) Debug.LogError($"[Sanctuary]: Exception occurred while reading test data: {ex.Message}");
            }
        }

        private static async Task Cleanup(string folderPath, string filePath, bool debug)
        {
            await Task.Run(() =>
            {
                // Check if the test data file exists before attempting to delete it
                if (File.Exists(filePath))
                {
                    // Delete the test data file
                    File.Delete(filePath);

                    // Log the file path for debugging purposes
                    if (debug) Debug.Log($"[Sanctuary]: Deleted test data file: {filePath}");
                }

                // Check if there is a backup file for the test data and delete it if it exists
                string backupFilePath = filePath + SerializationExtensions.DefaultBackupExtension;

                // Check if the backup file exists before attempting to delete it
                if (File.Exists(backupFilePath))
                {
                    // Delete the backup file
                    File.Delete(backupFilePath);

                    // Log the backup file path for debugging purposes
                    if (debug) Debug.Log($"[Sanctuary]: Deleted backup file: {backupFilePath}");
                }

                // If there are no more files in the test folder, delete the test folder as well
                if (Directory.Exists(folderPath) && Directory.GetFiles(folderPath).Length == 0)
                {
                    // Delete the test folder
                    Directory.Delete(folderPath);

                    // Log the folder path for debugging purposes
                    if (debug) Debug.Log($"[Sanctuary]: Deleted test folder: {folderPath}");
                }

                // Log a message indicating that the cleanup is complete
                if (debug) Debug.Log("[Sanctuary]: Test file cleanup complete.");
            });
        }

        #endregion

        #endregion

        #region New Serializer Tests

        [Test]
        public async Task TestNewSerializer()
        {
            // Create a new instance of the FileStreamConfiguration ScriptableObject to configure the file save data provider and JSON save serializer
            var config = new DefaultStreamConfiguration(Application.persistentDataPath + "/TestFolder");

            // Create a new instance of the SanctuaryService with the specified configuration and components
            var service = SanctuaryService.Create
            (
                new FileSaveDataProvider(config),
                new JsonSaveSerializer(config),
                new Sha256IntegrityValidator(),
                new UnityDebugLogger()
            );

            // Define a slot ID for the test save data
            string slotId = "TestSlot";

            // Save the test data to the specified save slot
            await Save(service, slotId);

            // Load the test data from the specified save slot
            await Load(service, slotId);

            // Delete the test data from the specified save slot
            await Delete(service, slotId);
        }

        public async Task Save(ISanctuaryService service, string slotId)
        {
            TestSaveDataClass save = new("John Doe", 30, 5.9f, new string[] { "Reading", "Gaming", "Hiking" });
            var result = await service.SaveAsync(slotId, save);
            Debug.Log(result.Success ? $"[Three Saves] Saved {save.Name} → {slotId} ({result.FilePath})." : $"[Three Saves] Save failed: {result.Reason}`.");
        }

        public async Task Load(ISanctuaryService service, string slotId)
        {
            // Attempt to load the save slot with the specified slotId
            var result = await service.LoadAsync<TestSaveDataClass>(slotId);

            // Log the result of the load operation
            if (result.Success)
            {
                Debug.Log($"[Three Saves] Loaded {result.Data.Name} · age {result.Data.Age} · height {result.Data.Height} · hobbies {string.Join(", ", result.Data.Hobbies)} (status: {result.Status}).");
            }
            else
            {
                Debug.Log($"[Three Saves] Load failed: {result.Status} — {result.Message}.");
            }
        }

        public async Task Delete(ISanctuaryService service, string slotId)
        {
            // Attempt to delete the save slot with the specified slotId
            var deleted = await service.DeleteAsync(slotId);

            // Log the result of the delete operation
            Debug.Log($"[Three Forks] Delete {slotId}: {deleted}.");
        }

        #endregion

        [System.Serializable]
        public struct TestSaveData
        {
            public string Name;
            public int Age;
            public float Height;
            public string[] Hobbies;

            public TestSaveData(string name, int age, float height, string[] hobbies)
            {
                Name = name;
                Age = age;
                Height = height;
                Hobbies = hobbies;
            }
        }

        [System.Serializable]
        public class TestSaveDataClass
        {
            public string Name;
            public int Age;
            public float Height;
            public string[] Hobbies;

            public TestSaveDataClass(string name, int age, float height, string[] hobbies)
            {
                Name = name;
                Age = age;
                Height = height;
                Hobbies = hobbies;
            }
        }
    }
}