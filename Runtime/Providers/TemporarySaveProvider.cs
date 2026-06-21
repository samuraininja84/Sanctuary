using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// A Bootstrapper that configures a SaveProvider as a temporary save container.
    /// </summary>
    [AddComponentMenu("Safekeeper/Temporary Save Provider")]
    public sealed class TemporarySaveProvider : Bootstrapper
    {
        [Header("Save Provider Settings")]
        [Tooltip("The save mode to use for this SaveProvider.")]
        public SaveMode saveMode = SaveMode.MemoryOnly;
        [Tooltip("The profile data to use for this SaveProvider. Controls where persistent data is stored.")]
        public ProfileData profileData = ProfileData.Temporary("Default");
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = false;

        protected override void Bootstrap() => Container.ConfigureAsTemporary(profileData, dontDestroyOnLoad);

        [ContextMenu("Save")]
        public async void Save() => await SaveProvider.Temporary.Save(saveMode);

        [ContextMenu("Load")]
        public async void Load() => await SaveProvider.Temporary.Load(saveMode);

        [ContextMenu("Delete")]
        public async void Delete() => await SaveProvider.Temporary.Delete();
    }
}
