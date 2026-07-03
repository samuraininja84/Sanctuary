using UnityEngine;
using Sanctuary.Extensions;
using Sanctuary.Configuration;
using Sanctuary.Serialization;
using Sanctuary.Loaders;

namespace Sanctuary
{
    /// <summary>
    /// A Bootstrapper that configures a SaveProvider as a temporary save container.
    /// </summary>
    [AddComponentMenu("Sanctuary/Temporary Save Provider")]
    public sealed class TemporarySaveProvider : SaveControllerBase
    {
        [Header("Serializer Configuration")]
        [Tooltip("The serializer configuration to use for saving and loading data.")]
        [SerializeField] private SerializerConfiguration serializer;
        [Tooltip("The stream configuration to use for saving and loading data.")]
        [SerializeField] private StreamConfiguration stream;

        [Header("Save Provider Settings")]
        [Tooltip("The save mode to use for this SaveProvider.")]
        public SaveMode saveMode = SaveMode.MemoryOnly;
        [Tooltip("The profile data to use for this SaveProvider. Controls where persistent data is stored.")]
        public ProfileData profile = ProfileData.Temporary("Temporary");
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = false;

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

        protected override void OnInit()
        {
            // Configure the serializer and stream for the SaveProvider
            Configure(FileSaveLoader.Builder.Create(profile, GetSerializer()).Build(), GetStream());

            // Configure the SaveProvider as a temporary save container
            Container.ConfigureAsTemporary(profile, dontDestroyOnLoad);
        }
    }
}
