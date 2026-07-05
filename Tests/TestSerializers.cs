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
        public const string TestRegistryFile = "test";
        public const string TestChunkId = "Tests";
        public const string TestObjectId = "5561391260475779002";
        public const int BenchmarkIterations = 100;

        #region New Serializer Tests

        [Test]
        public async Task TestSerialization()
        {
            // Create a new instance of the FileStreamConfiguration ScriptableObject to configure the file save data provider and JSON save serializer
            var config = new DefaultStreamConfiguration(TestFolderName);

            // Create a new instance of the SanctuaryService with the specified configuration and components
            var service = SanctuaryService.Create
            (
                new FileSaveDataProvider(config),
                new JsonSaveSerializer(config),
                new Sha256IntegrityValidator(),
                new UnityDebugLogger(),
                TestRegistryFile
            );

            // Load the save slot registry from the specified registry file
            var registryLoaded = await service.TryLoadRegistryAsync();

            // Define a slot ID for the test save data
            string slotId = "TestSlot";

            // Log the result of the registry load operation
            if (registryLoaded) Debug.Log($"[Sanctuary]: Loaded save slot registry from {TestRegistryFile}.");
            else Debug.Log($"[Sanctuary]: A new registry will be created upon the first save operation.");

            // Save the test data to the specified save slot
            await Save(service, slotId);

            // Load the test data from the specified save slot
            await Load(service, slotId);

            // Delete the test data from the specified save slot
            await Delete(service, slotId);
        }

        [Test]
        public async Task TestBulkSerialization()
        {
            // Create a new instance of the FileStreamConfiguration ScriptableObject to configure the file save data provider and JSON save serializer
            var config = new DefaultStreamConfiguration(TestFolderName);

            // Create a new instance of the SanctuaryService with the specified configuration and components
            var service = SanctuaryService.Create
            (
                new FileSaveDataProvider(config),
                new JsonSaveSerializer(config),
                new Sha256IntegrityValidator(),
                new UnityDebugLogger(),
                TestRegistryFile
            );

            // Load the save slot registry from the specified registry file
            await service.TryLoadRegistryAsync();

            // Benchmark the save, load, and delete operations for the specified number of iterations
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                // Define a slot ID for the test save data
                string slotId = "TestSlot_" + i;

                // Save the test data to the specified save slot
                await Save(service, slotId);

                // Load the test data from the specified save slot
                await Load(service, slotId);

                // Delete the test data from the specified save slot
                await Delete(service, slotId);
            }
        }

        [Test]
        public async Task TestEscapeSlashes()
        {
            // Create a new instance of the FileStreamConfiguration ScriptableObject to configure the file save data provider and JSON save serializer
            var config = new DefaultStreamConfiguration(TestFolderName);

            // Create a new instance of the SanctuaryService with the specified configuration and components
            var service = SanctuaryService.Create
            (
                new FileSaveDataProvider(config),
                new JsonSaveSerializer(config),
                new Sha256IntegrityValidator(),
                new UnityDebugLogger(),
                TestRegistryFile
            );

            // Load the save slot registry from the specified registry file
            var registryLoaded = await service.TryLoadRegistryAsync();

            // Define a slot ID for the test save data
            string slotId = "TestSlot";

            // Log the result of the registry load operation
            if (registryLoaded) Debug.Log($"[Sanctuary]: Loaded save slot registry from {TestRegistryFile}.");
            else Debug.Log($"[Sanctuary]: A new registry will be created upon the first save operation.");

            // Save the test data to the specified save slot
            await Save(service, slotId);
        }

        public async Task Save(ISanctuaryService service, string slotId)
        {
            TestSaveDataClass save = new("John Doe", 30, 5.9f, new string[] { "Reading", "Gaming", "Hiking" });
            var result = await service.SaveAsync(slotId, save);
            Debug.Log(result.Success ? $"[Sanctuary]: Saved {save.Name} → {slotId} ({result.FilePath})." : $"[Sanctuary]: Save failed: {result.Reason}`.");
        }

        public async Task Load(ISanctuaryService service, string slotId)
        {
            // Attempt to load the save slot with the specified slotId
            var result = await service.LoadAsync<TestSaveDataClass>(slotId);

            // Log the result of the load operation
            if (result.Success)
            {
                Debug.Log($"[Sanctuary]: Loaded {result.Data.Name} · age {result.Data.Age} · height {result.Data.Height} · hobbies {string.Join(", ", result.Data.Hobbies)} (status: {result.Status}).");
            }
            else
            {
                Debug.Log($"[Sanctuary]: Load failed: {result.Status} — {result.Message}.");
            }
        }

        public async Task Delete(ISanctuaryService service, string slotId)
        {
            // Attempt to delete the save slot with the specified slotId
            var deleted = await service.DeleteAsync(slotId);

            // Log the result of the delete operation
            Debug.Log($"[Sanctuary]: Delete {slotId}: {deleted}.");
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