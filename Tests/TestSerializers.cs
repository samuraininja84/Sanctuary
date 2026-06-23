using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Sanctuary.Serializers;

namespace Sanctuary.Tests
{
    public class TestSerializers
    {
        public const string TestFolderName = "Save Data_Tests";
        public const string TestChunkId = "Tests";
        public const string TestObjectId = "5561391260475779002";

        [Test]
        public async Task TestBinary() => await TestSerializer(BinarySerializer.Default, "Binary", true);

        [Test]
        public async Task TestBinaryCompressed() => await TestSerializer(BinarySerializer.Compressed, "BinaryCompressed", true);

        [Test]
        public async Task TestBinaryBackup() => await TestSerializer(BinarySerializer.Backup, "BinaryBackup", true);

        [Test]
        public async Task TestBinaryBackupCompressed() => await TestSerializer(BinarySerializer.BackupCompressed, "BinaryBackupCompressed", true);

        [Test]
        public async Task TestBinaryBackupEncrypted() => await TestSerializer(BinarySerializer.BackupEncrypted, "BinaryBackupEncrypted", true);

        [Test]
        public async Task TestBinaryCompressionEncrypted() => await TestSerializer(BinarySerializer.CompressionEncrypted, "BinaryCompressionEncrypted", true);

        [Test]
        public async Task TestBinaryAll() => await TestSerializer(BinarySerializer.All, "BinaryAll", true);

        #region Other Binary Serializer Tests

        [Test]
        public async Task TestBinaryData() => await TestSerializer(BinarySerializer.CreateAsData(SerializationOptions.None), "BinaryData", true);

        #endregion

#if UNITY_NEWTONSOFT_JSON

        [Test]
        public async Task TestJson() => await TestSerializer(JsonSerializer.Default, "Json", true);

        [Test]
        public async Task TestJsonCompressed() => await TestSerializer(JsonSerializer.Compressed, "JsonCompressed", true);

        [Test]
        public async Task TestJsonBackup() => await TestSerializer(JsonSerializer.Backup, "JsonBackup", true);

        [Test]
        public async Task TestJsonBackupCompressed() => await TestSerializer(JsonSerializer.BackupCompressed, "JsonBackupCompressed", true);

        [Test]
        public async Task TestJsonBackupEncrypted() => await TestSerializer(JsonSerializer.BackupEncrypted, "JsonBackupEncrypted", true);

        [Test]
        public async Task TestJsonCompressionEncrypted() => await TestSerializer(JsonSerializer.CompressionEncrypted, "JsonCompressionEncrypted", true);

        [Test]
        public async Task TestJsonAll() => await TestSerializer(JsonSerializer.All, "JsonAll", true);

        #region Other Json Serializer Tests

        [Test]
        public async Task TestJsonData() => await TestSerializer(JsonSerializer.CreateAsData(SerializationOptions.None), "Data", true);

        [Test]
        public async Task TestMarkdown() => await TestSerializer(JsonSerializer.CreateAsMarkDown(SerializationOptions.None), "Markdown", true);

        [Test]
        public async Task TestText() => await TestSerializer(JsonSerializer.CreateAsText(SerializationOptions.None), "Text", true);

        #endregion

#endif

        public async Task TestSerializer(ISerializer serializer, string fileName, bool deleteAfterTest = false)
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

            // Try to serialize the save data using the binary serializer
            await serializer.Serialize(saveData, folderPath, filePath);

            // Log the file path for debugging purposes
            Debug.Log($"Serialized {fileName} save data to: {filePath}");

            // Try to deserialize the save data using the appropriate serializer
            saveData = await serializer.Deserialize(filePath);

            // Log the file path for debugging purposes
            Debug.Log($"Deserialized {fileName} save data from: {filePath}");

            // Try to read the test data from the save data
            try
            {
                if (saveData.TryRead(location, data))
                {
                    Debug.Log($"Deserialized Test Data: Name={data.Name}, Age={data.Age}, Height={data.Height}, Hobbies={string.Join(", ", data.Hobbies)}");
                }
                else
                {
                    Debug.LogError("Failed to read test data from deserialized save data.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception occurred while reading test data: {ex.Message}");
            }

            // Clean up the test data file after the test is complete
            if (deleteAfterTest)
            {
                // Check if the test data file exists before attempting to delete it
                if (File.Exists(filePath))
                {
                    // Delete the test data file
                    File.Delete(filePath);

                    // Log the file path for debugging purposes
                    Debug.Log($"Deleted test data file: {filePath}");
                }

                // Check if there is a backup file for the test data and delete it if it exists
                string backupFilePath = filePath + SerializationExtensions.BackupFileExtension;

                if (File.Exists(backupFilePath))
                {
                    // Delete the backup file
                    File.Delete(backupFilePath);

                    // Log the backup file path for debugging purposes
                    Debug.Log($"Deleted backup file: {backupFilePath}");
                }

                // If there are no more files in the test folder, delete the test folder as well
                if (Directory.Exists(folderPath) && Directory.GetFiles(folderPath).Length == 0)
                {
                    // Delete the test folder
                    Directory.Delete(folderPath);

                    // Log the folder path for debugging purposes
                    Debug.Log($"Deleted test folder: {folderPath}");
                }

                // Log a message indicating that the cleanup is complete
                Debug.Log("Cleanup complete.");
            }
        }

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
    }
}

