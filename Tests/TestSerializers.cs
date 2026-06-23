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
        public async Task TestBinary() => await TestSerializer(BinarySerializer.Default, "Binary");

        [Test]
        public async Task TestBinaryCompressed() => await TestSerializer(BinarySerializer.Compressed, "BinaryCompressed");

#if UNITY_NEWTONSOFT_JSON

        [Test]
        public async Task TestJson() => await TestSerializer(JsonSerializer.Default, "Json");

        [Test]
        public async Task TestJsonCompressed() => await TestSerializer(JsonSerializer.Compressed, "JsonCompressed");

        [Test]
        public async Task TestMarkdown() => await TestSerializer(MarkdownSerializer.Default, "Markdown");

        [Test]
        public async Task TestMarkdownCompressed() => await TestSerializer(MarkdownSerializer.Compressed, "MarkdownCompressed");

        [Test]
        public async Task TestText() => await TestSerializer(TextSerializer.Default, "Text");

        [Test]
        public async Task TestTextCompressed() => await TestSerializer(TextSerializer.Compressed, "TextCompressed");

#endif

        public async Task TestSerializer(ISerializer serializer, string fileName)
        {
            // Create a new instance of the save data
            ISaveData saveData = new SaveData();

            // Create a save location for the test data
            SaveLocation location = new(TestChunkId, TestObjectId);

            // Create a new instance of the test data
            TestData data = new("John Doe", 30, 5.9f, new string[] { "Reading", "Gaming", "Hiking" });

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
        }

        [System.Serializable]
        public struct TestData
        {
            public string Name;
            public int Age;
            public float Height;
            public string[] Hobbies;

            public TestData(string name, int age, float height, string[] hobbies)
            {
                Name = name;
                Age = age;
                Height = height;
                Hobbies = hobbies;
            }
        }
    }
}

