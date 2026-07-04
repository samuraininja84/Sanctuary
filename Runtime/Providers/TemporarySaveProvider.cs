using Sanctuary.Configuration;
using Sanctuary.Loaders;
using Sanctuary.Serialization;
using Sanctuary.Stores;
using System.Threading.Tasks;
using UnityEngine;

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
        [Tooltip("The profile data to use for this SaveProvider. Controls where persistent data is stored.")]
        public ProfileData profile = ProfileData.Temporary("Temporary");
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = false;

        /// <summary>
        /// Retrieves the serializer to be used for saving and loading data.
        /// </summary>
        /// <remarks>If a custom serializer is provided, it will be used; otherwise, the default binary serializer will be returned.</remarks>
        /// <returns>The serializer to be used for saving and loading data.</returns>
        private ISerializer GetSerializer() => serializer != null ? serializer.GetSerializer(GetStream().Options) : BinarySerializer.Default;

        /// <summary>
        /// Retrieves the stream configuration to be used for saving and loading data.
        /// </summary>
        /// <remarks>If a custom stream configuration is provided, it will be used; otherwise, a new instance of FileStreamConfiguration will be created.</remarks>
        /// <returns>The stream configuration to be used for saving and loading data.</returns>
        private StreamConfiguration GetStream() => stream != null ? stream : stream = ScriptableObject.CreateInstance<FileStreamConfiguration>();

        protected override void PreInit() => Configure(FileSaveLoader.Builder.Create(profile, GetSerializer()).Build(), GetStream());

        protected override void PostInit()
        {
            // Register this SaveProvider with the SaveProvider static class for temporary scope
            SaveProvider.SetByScope(SaveScope.Temporary, this, profile);

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
                Data = await _loader.Create();

                // Invoke the OnLoad method for custom load logic
                OnLoad();
            }

            // Notify all registered stores to save their data
            SaveStoreRegistry.SaveWith(this);

            // Save the data to persistent storage if needed
            await _loader.Save(_configuration, Data);

            // Invoke the OnSave method for custom save logic
            OnSave();

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        private void OnDestroy() => SaveProvider.ClearByScope(SaveScope.Temporary);
    }
}
