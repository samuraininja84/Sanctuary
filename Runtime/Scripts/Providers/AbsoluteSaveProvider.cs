using UnityEngine;

namespace Sanctuary
{
    [AddComponentMenu("Safekeeper/Absolute Save Provider")]
    public sealed class AbsoluteSaveProvider : Bootstrapper
    {
        [Header("Save Provider Settings")]
        [Tooltip("The save mode to use for this SaveProvider.")]
        public SaveMode saveMode = SaveMode.Full;
        [Tooltip("The profile data to use for this SaveProvider. Controls where persistent data is stored.")]
        public ProfileData profileData = ProfileData.Absolute("Absolute");
        [Tooltip("If true, the SaveProvider will load data on boot.")]
        [SerializeField] private bool loadOnBoot = true;
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap()
        {
            // Configure the SaveProvider as Absolute with the specified profile data
            Container.ConfigureAsAbsolute(profileData, dontDestroyOnLoad);

            // Load data on boot if specified
            if (loadOnBoot) Load();
        }

        [ContextMenu("Save")]
        public async void Save() => await SaveProvider.Absolute.Save(saveMode);

        [ContextMenu("Load")]
        public async void Load() => await SaveProvider.Absolute.Load(saveMode);

        [ContextMenu("Delete")]
        public async void Delete() => await SaveProvider.Absolute.Delete();
    }
}
