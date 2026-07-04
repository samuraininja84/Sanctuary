using UnityEngine;
using Sanctuary.Configuration;
using Sanctuary.Serialization;
using Sanctuary.Loaders;

namespace Sanctuary
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Sanctuary/Absolute Save Provider")]
    public sealed class AbsoluteSaveProvider : SaveControllerBase
    {
        [Header("Serializer Configuration")]
        [Tooltip("The serializer configuration to use for saving and loading data.")]
        [SerializeField] private SerializerConfiguration serializer;
        [Tooltip("The stream configuration to use for saving and loading data.")]
        [SerializeField] private StreamConfiguration stream;

        [Header("Save Provider Settings")]
        [Tooltip("The save mode to use for this SaveProvider.")]
        public SaveMode saveMode = SaveMode.Full;
        [Tooltip("The profile data to use for this SaveProvider. Controls where persistent data is stored.")]
        public ProfileData profile = ProfileData.Absolute("Absolute");
        [Tooltip("If true, the SaveProvider will load data on boot.")]
        [SerializeField] private bool loadOnBoot = true;
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = true;

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

        protected async override void PostInit()
        {
            // Register this SaveProvider as the absolute save provider
            SaveProvider.SetByScope(SaveScope.Absolute, this, profile);

            // If an absolute save doesn't already exist, create one
            if (!Exists) await Save(SaveMode.Full);

            // Load the absolute save if specified
            if (loadOnBoot) await Load(SaveMode.Full);

            // Make persistent across scenes if specified and in play mode
            if (dontDestroyOnLoad && Application.isPlaying) DontDestroyOnLoad(this);
        }

        private void OnDestroy() => SaveProvider.ClearByScope(SaveScope.Absolute);
    }
}
