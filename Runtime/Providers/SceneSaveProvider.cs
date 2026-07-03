using UnityEngine;
using UnityEngine.SceneManagement;
using Sanctuary.Extensions;
using Sanctuary.Configuration;
using Sanctuary.Serialization;
using Sanctuary.Loaders;

namespace Sanctuary
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(SaveProvider))]
    [AddComponentMenu("Sanctuary/Scene Save Provider")]
    public sealed class SceneSaveProvider : SaveControllerBase
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
        public ProfileData profile = ProfileData.Scene("Scene Name");
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        /// <summary>
        /// Gets the scene that this SaveProvider is tracking.
        /// </summary>
        public Scene TrackedScene => SceneManager.GetSceneByName(profile.GetFileName());

        /// <summary>
        /// A reference to the SaveProvider instance managed by this Bootstrapper.
        /// </summary>
        private SaveProvider container;

        /// <summary>
        /// The SaveProvider instance managed by this Bootstrapper.
        /// </summary>
        internal SaveProvider Container => container != null ? container : (container = gameObject.GetOrAdd<SaveProvider>());

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

        protected override void PostInit() => Container.ConfigureForScene(this, profile, dontDestroyOnLoad);

        public void SetName(string name) => profile.SetFileName(name);
    }
}