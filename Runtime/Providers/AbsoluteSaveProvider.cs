using UnityEngine;
using Sanctuary.Configuration;
using Sanctuary.Serialization;

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
        [Tooltip("The slot ID to use for saving and loading data.")]
        [SerializeField] private string slotID = "Absolute";
        [Tooltip("If true, the SaveProvider will load data on boot.")]
        [SerializeField] private bool loadOnBoot = true;
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        public override string Name => "Absolute";

        public override string SlotID { get => slotID; protected set => slotID = value; }

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
        private IStreamConfiguration GetStream() => stream != null ? stream : new DefaultStreamConfiguration(SerializationExtensions.DefaultFolderName);

        private ISanctuaryService ConstructService()
        {
            // Create a new instance of the FileStreamConfiguration ScriptableObject to configure the file save data provider and JSON save serializer
            var config = GetStream();

            // Create a new instance of the SanctuaryService with the specified configuration and components
            return SanctuaryService.Create
            (
                new FileSaveDataProvider(config),
                new JsonSaveSerializer(config),
                new Sha256IntegrityValidator(),
                new UnityDebugLogger(),
                Name
            );
        }

        protected override async void PreInit()
        {
            // Register this SaveProvider as the absolute save provider
            SaveProvider.SetByScope(SaveScope.Absolute, this);

            // Configure the SanctuaryService with the constructed service
            Configure(ConstructService());
        }

        protected async override void PostInit()
        {
            // Load the absolute save if specified
            if (Exists && loadOnBoot) await Load();

            // Make persistent across scenes if specified and in play mode
            if (dontDestroyOnLoad && Application.isPlaying) DontDestroyOnLoad(this);
        }

        public override void SetID(string slotID) => this.slotID = slotID;

        private void OnDestroy() => SaveProvider.ClearByScope(SaveScope.Absolute);
    }
}
