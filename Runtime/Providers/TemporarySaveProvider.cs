using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Stores;
using Sanctuary.Configuration;
using Sanctuary.Serialization;

namespace Sanctuary
{
    [AddComponentMenu("Sanctuary/Temporary Save Provider")]
    public sealed class TemporarySaveProvider : SaveControllerBase
    {
        [Header("Serializer Configuration")]
        [Tooltip("The serializer configuration to use for saving and loading data.")]
        [SerializeField] private SerializerConfiguration serializer;
        [Tooltip("The stream configuration to use for saving and loading data.")]
        [SerializeField] private StreamConfiguration stream;

        [Header("Save Provider Settings")]
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = false;

        public override string Name => "Temporary";

        /// <summary>
        /// Retrieves the serializer to be used for saving and loading data.
        /// </summary>
        /// <remarks>If a custom serializer is provided, it will be used; otherwise, the default binary serializer will be returned.</remarks>
        /// <returns>The serializer to be used for saving and loading data.</returns>
        private ISaveSerializer GetSerializer() => null;

        /// <summary>
        /// Retrieves the stream configuration to be used for saving and loading data.
        /// </summary>
        /// <remarks>If a custom stream configuration is provided, it will be used; otherwise, a new instance of FileStreamConfiguration will be created.</remarks>
        /// <returns>The stream configuration to be used for saving and loading data.</returns>
        private StreamConfiguration GetStream() => stream != null ? stream : stream = ScriptableObject.CreateInstance<FileStreamConfiguration>();

        private ISanctuaryService ConstructService()
        {
            // Create a new instance of the FileStreamConfiguration ScriptableObject to configure the file save data provider and JSON save serializer
            var config = GetStream();

            // Create a new instance of the SanctuaryService with the specified configuration and components
            return SanctuaryService.Create
            (
                new StreamSaveDataProvider(config),
                new JsonSaveSerializer(config),
                new Sha256IntegrityValidator(),
                new UnityDebugLogger()
            );
        }

        protected override void PreInit() => Configure(ConstructService());

        protected override void PostInit()
        {
            // Register this SaveProvider with the SaveProvider static class for temporary scope
            SaveProvider.SetByScope(SaveScope.Temporary, this);

            // Make persistent across scenes if specified and in play mode
            if (dontDestroyOnLoad && Application.isPlaying) DontDestroyOnLoad(this);
        }

        public override async Task Save()
        {
            // Lock the semaphore to prevent other operations
            await Lock();

            // If the save doesn't exist, create it
            if (!Exists)
            {
                // Mark the save as existing
                Exists = true;

                // Create a new save data to avoid null reference exceptions
                Data = new SaveData();

                // Invoke the OnLoad method for custom load logic
                OnLoad();
            }

            // Notify all registered stores to save their data
            SaveStoreRegistry.SaveWith(this);

            //// Save the data to persistent storage if needed | Disabled for temporary saves as they are not meant to be persisted
            // await _service.SaveAsync(Name, Data);

            // Invoke the OnSave method for custom save logic
            OnSave();

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        private void OnDestroy() => SaveProvider.ClearByScope(SaveScope.Temporary);
    }
}
