using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sanctuary
{
    /// <summary>
    /// A Bootstrapper that configures a SaveProvider as a scene save container.
    /// </summary>
    [AddComponentMenu("Safekeeper/Scene Save Provider")]
    public sealed class SceneSaveProvider : Bootstrapper
    {
        [Header("Save Provider Settings")]
        [Tooltip("The save mode to use for this SaveProvider.")]
        public SaveMode saveMode = SaveMode.Full;
        [Tooltip("The profile data to use for this SaveProvider. Controls where persistent data is stored.")]
        public ProfileData profileData = ProfileData.Scene("Default");
        [Tooltip("If true, the SaveProvider will not be destroyed on scene load.")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        /// <summary>
        /// Gets the scene that this SaveProvider is tracking.
        /// </summary>
        public Scene TrackedScene => SceneManager.GetSceneByName(profileData.GetFileName());

        protected override void Bootstrap() => Container.ConfigureForScene(profileData, dontDestroyOnLoad);

        [ContextMenu("Save")]
        public async void Save() => await SaveProvider.ForScene(profileData.GetFileName()).Save(saveMode);

        [ContextMenu("Load")]
        public async void Load() => await SaveProvider.ForScene(profileData.GetFileName()).Load(saveMode);

        [ContextMenu("Delete")]
        public async void Delete() => await SaveProvider.ForScene(profileData.GetFileName()).Delete();

        public void SetName(string name) => profileData.SetFileName(name);
    }
}