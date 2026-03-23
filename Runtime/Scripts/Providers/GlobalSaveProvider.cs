using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// A Bootstrapper that configures a SaveProvider as a global save container.
    /// </summary>
    [AddComponentMenu("Safekeeper/Global Save Provider")]
    public sealed class GlobalSaveProvider : Bootstrapper
    {
        [Header("Save Provider Settings")]
        [Tooltip("The save mode to use for this SaveProvider.")]
        public SaveMode saveMode = SaveMode.Full;
        [Tooltip("The profile data to use for this SaveProvider. Controls where persistent data is stored.")]
        public ProfileData profileData = ProfileData.Global("Global");
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap() => Container.ConfigureAsGlobal(profileData, dontDestroyOnLoad);

        [ContextMenu("Save")]
        public async void Save() => await SaveProvider.Global.Save(saveMode);

        [ContextMenu("Load")]
        public async void Load() => await SaveProvider.Global.Load(saveMode);

        [ContextMenu("Delete")]
        public async void Delete() => await SaveProvider.Global.Delete();
    }
}
