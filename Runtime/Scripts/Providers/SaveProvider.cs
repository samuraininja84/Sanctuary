using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sanctuary.Loaders;
using Sanctuary.Extensions;
using Sanctuary.Stores;

namespace Sanctuary
{
    public class SaveProvider : MonoBehaviour
    {
        /// <summary>
        /// The controller responsible for managing save operations.
        /// </summary>
        protected SaveControllerBase controller = null;

        /// <summary>
        /// An accessor for the SaveController.
        /// </summary>
        public SaveControllerBase Controller => controller;

        /// <summary>
        /// Indicates whether the bootstrap process has completed.
        /// </summary>
        protected bool isBootstrapped = false;

        /// <summary>
        /// A flag indicating whether to automatically save before the save provider is destroyed.
        /// </summary>
        public static bool saveOnExit = false;

        /// <summary>
        /// The absolute SaveProvider instance.
        /// </summary>
        protected static SaveProvider absolute;

        /// <summary>
        /// The global SaveProvider instance.
        /// </summary>
        protected static SaveProvider global;

        /// <summary>
        /// The temporary SaveProvider instance.
        /// </summary>
        protected static SaveProvider temporary;

        /// <summary>
        /// The dictionary mapping scenes to their respective SaveProvider instances.
        /// </summary>
        public static Dictionary<string, SaveProvider> sceneContainers = new();

        /// <summary>
        /// The temporary list used for storing root GameObjects in a scene during lookup.
        /// </summary>
        private static List<GameObject> tmpSceneGameObjects = new();

        /// <summary>
        /// The name used for Absolute SaveProvider GameObjects.
        /// </summary>
        private const string k_absoluteSaveProviderName = "SaveProvider [Absolute]";

        /// <summary>
        /// The name used for Global SaveProvider GameObjects.
        /// </summary>
        private const string k_globalSaveProviderName = "SaveProvider [Global]";

        /// <summary>
        /// The name used for Temporary SaveProvider GameObjects.
        /// </summary>
        private const string k_TemporarySaveProviderName = "SaveProvider [Temporary]";

        /// <summary>
        /// The name used for Scene SaveProvider GameObjects.
        /// </summary>
        private static string SceneSaveProviderName(string sceneName) => $"SaveProvider [{sceneName}]";

        /// <summary>
        /// Sets up this SaveProvider as the absolute instance by marking as absolute and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is true.</param>
        internal void ConfigureAsAbsolute(ProfileData profile, bool dontDestroyOnLoad = true)
        {
            // Check if already configured as absolute
            if (absolute == this)
            {
                // Already configured as absolute
                Debug.LogWarning("SaveProvider.ConfigureAsAbsolute: Already configured as absolute", this);
            }
            else if (absolute != null)
            {
                // Another absolute container already exists
                Debug.LogWarning("SaveProvider.ConfigureAsAbsolute: Another SaveProvider is already configured as absolute", this);

                // Destroy this GameObject to enforce singleton pattern
                Destroy(gameObject);
            }
            else
            {
                // Configure as absolute
                absolute = this;

                // Initialize absolute save controller if needed
                if (controller == null) controller = SaveControllerBase.New(new FileSaveLoader(profile), SaveScope.Absolute);

                // Make persistent across scenes if specified and in play mode
                if (dontDestroyOnLoad && Application.isPlaying) DontDestroyOnLoad(gameObject);

                // Set the isBootstrapped flag to true to indicate that this SaveProvider has been configured
                isBootstrapped = true;
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the global instance by marking as global and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is true.</param>
        internal void ConfigureAsGlobal(ProfileData profile, bool dontDestroyOnLoad = true)
        {
            // Check if already configured as global
            if (global == this)
            {
                // Already configured as global
                Debug.LogWarning("SaveProvider.ConfigureAsGlobal: Already configured as global", this);
            }
            else if (global != null)
            {
                // Another global container already exists
                Debug.LogWarning("SaveProvider.ConfigureAsGlobal: Another SaveProvider is already configured as global", this);

                // Destroy this GameObject to enforce singleton pattern
                Destroy(gameObject);
            }
            else
            {
                // Configure as global
                global = this;

                // Initialize global save controller if needed
                if (controller == null) controller = SaveControllerBase.New(new FileSaveLoader(profile), SaveScope.Global);

                // Make persistent across scenes if specified and in play mode
                if (dontDestroyOnLoad && Application.isPlaying) DontDestroyOnLoad(gameObject);

                // Set the isBootstrapped flag to true to indicate that this SaveProvider has been configured
                isBootstrapped = true;
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the temporary instance by marking as temporary and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is false.</param>
        internal void ConfigureAsTemporary(ProfileData profile, bool dontDestroyOnLoad = false)
        {
            // Check if already configured as temporary
            if (temporary == this)
            {
                // Already configured as temporary
                Debug.LogWarning("SaveProvider.ConfigureAsTemporary: Already configured as temporary", this);
            }
            else if (temporary != null)
            {
                // Another temporary container already exists
                Debug.LogWarning("SaveProvider.ConfigureAsTemporary: Another SaveProvider is already configured as temporary", this);

                // Destroy this GameObject to enforce singleton pattern
                Destroy(gameObject);
            }
            else
            {
                // Configure as temporary
                temporary = this;

                // Initialize temporary save controller if needed
                if (controller == null) controller = SaveControllerBase.New(new FileSaveLoader(profile), SaveScope.Temporary);

                // Make persistent across scenes if specified and in play mode
                if (dontDestroyOnLoad && Application.isPlaying) DontDestroyOnLoad(gameObject);

                // Set the isBootstrapped flag to true to indicate that this SaveProvider has been configured
                isBootstrapped = true;
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the instance for its scene.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is false.</param>
        internal void ConfigureForScene(ProfileData profile, bool dontDestroyOnLoad = false)
        {
            // Get the scene this GameObject belongs to
            string scene = gameObject.scene.name;

            // Check if another container is already registered for this scene
            if (sceneContainers.ContainsKey(scene))
            {
                // Log error if another container is already registered for this scene
                Debug.LogWarning("SaveProvider.ConfigureForScene: Another SaveProvider is already configured for this scene, destroying this one", this);

                // Destroy this GameObject to enforce singleton pattern for the scene
                Destroy(gameObject);

                // Return early, since a container is already registered for this scene
                return;
            }

            // Initialize scene save controller if needed
            if (controller == null)
            {
                // Set the profile name to the scene name if not already set
                profile.SetFileName(scene);

                // Create a default FileSaveLoader for scene saves
                controller = SaveControllerBase.New(new FileSaveLoader(profile), SaveScope.Scene);
            }

            // Register this container for the scene
            sceneContainers.Add(scene, this);

            // Make persistent across scenes if specified and in play mode
            if (dontDestroyOnLoad && Application.isPlaying) DontDestroyOnLoad(gameObject);

            // Set the isBootstrapped flag to true to indicate that this SaveProvider has been configured
            isBootstrapped = true;
        }

        /// <summary>
        /// Retrieves the <see cref="AbsoluteSaveProvider"/>'s <see cref="SaveControllerBase"/> instance, creating one if it does not already exist.
        /// </summary>
        /// <remarks>
        /// This property checks for an existing <see cref="AbsoluteSaveProvider"/> instance. If none is found, it searches the scene for a <see cref="AbsoluteSaveProvider"/> component.
        /// If found, it bootstraps that instance. If no <see cref="AbsoluteSaveProvider"/> exists in the scene, a new GameObject is created with a <see cref="AbsoluteSaveProvider"/> component, and it is bootstrapped.
        /// </remarks>
        /// <returns>The <see cref="AbsoluteSaveProvider"/>'s <see cref="SaveControllerBase"/>  instance.</returns>
        public static SaveControllerBase Absolute
        {
            get
            {
                // Return existing absolute instance if available
                if (absolute != null) return absolute.Controller;

                // Try to find an existing AbsoluteSaveProvider in the scene
                if (FindFirstObjectByType<AbsoluteSaveProvider>() is { } found)
                {
                    // Bootstrap the found global instance
                    found.BootstrapOnDemand();

                    // Return the absolute instance after bootstrapping
                    return absolute.Controller;
                }

                // Create a new GameObject to hold the absolute SaveProvider
                var container = new GameObject(k_absoluteSaveProviderName, typeof(SaveProvider));

                // Bootstrap the new absolute instance
                container.AddComponent<AbsoluteSaveProvider>().BootstrapOnDemand();

                // Return the newly created absolute instance
                return absolute.Controller;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="GlobalSaveProvider"/>'s <see cref="SaveControllerBase"/> instance, creating one if it does not already exist.
        /// </summary>
        /// <remarks>
        /// This property checks for an existing <see cref="GlobalSaveProvider"/> instance. If none is found, it searches the scene for a <see cref="GlobalSaveProvider"/> component.
        /// If found, it bootstraps that instance. If no <see cref="GlobalSaveProvider"/> exists in the scene, a new GameObject is created with a <see cref="GlobalSaveProvider"/> component, and it is bootstrapped.
        /// </remarks>
        /// <returns>The <see cref="GlobalSaveProvider"/>'s <see cref="SaveControllerBase"/>  instance.</returns>
        public static SaveControllerBase Global
        {
            get
            {
                // Return existing global instance if available
                if (global != null) return global.Controller;

                // Try to find an existing GlobalSaveProvider in the scene
                if (FindFirstObjectByType<GlobalSaveProvider>() is { } found)
                {
                    // Bootstrap the found global instance
                    found.BootstrapOnDemand();

                    // Return the global instance after bootstrapping
                    return global.Controller;
                }

                // Create a new GameObject to hold the global SaveProvider
                var container = new GameObject(k_globalSaveProviderName, typeof(SaveProvider));

                // Bootstrap the new global instance
                container.AddComponent<GlobalSaveProvider>().BootstrapOnDemand();

                // Return the newly created global instance
                return global.Controller;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="TemporarySaveProvider"/>'s <see cref="SaveControllerBase"/> instance, creating one if it does not already exist.
        /// </summary>
        /// <remarks>
        /// This property checks for an existing <see cref="TemporarySaveProvider"/> instance. If none is found, it searches the scene for a <see cref="TemporarySaveProvider"/> component.
        /// If found, it bootstraps that instance. If no <see cref="TemporarySaveProvider"/> exists in the scene, a new GameObject is created with a <see cref="TemporarySaveProvider"/> component, and it is bootstrapped.
        /// </remarks>
        /// <returns>The <see cref="TemporarySaveProvider"/>'s <see cref="SaveControllerBase"/>  instance.</returns>
        public static SaveControllerBase Temporary
        {
            get
            {
                // Return existing temporary instance if available
                if (temporary != null) return temporary.Controller;

                // Try to find an existing TemporarySaveProvider in the scene
                if (FindFirstObjectByType<TemporarySaveProvider>() is { } found)
                {
                    // Bootstrap the found temporary instance
                    found.BootstrapOnDemand();

                    // Return the temporary instance after bootstrapping
                    return temporary.Controller;
                }

                // Create a new GameObject to hold the temporary SaveProvider
                var container = new GameObject(k_TemporarySaveProviderName, typeof(SaveProvider));

                // Bootstrap the new temporary instance
                container.AddComponent<TemporarySaveProvider>().BootstrapOnDemand();

                // Return the newly created temporary instance
                return temporary.Controller;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="SceneSaveProvider"/>'s <see cref="SaveControllerBase"/> instance associated with the currently active scene.
        /// </summary>
        /// <remarks>
        /// This method first checks if a <see cref="SceneSaveProvider"/> is already registered for the active scene. 
        /// If no <see cref="SceneSaveProvider"/> is found, it searches the root GameObjects of the active scene for a <see cref="SceneSaveProvider"/> component. 
        /// If one is found, it initializes the associated <see cref="SceneSaveProvider"/> and returns it. 
        /// If no <see cref="SceneSaveProvider"/> exists, a new one is created, initialized, and returned.
        /// </remarks>
        /// <returns>The <see cref="SaveControllerBase"/>'s <see cref="SaveControllerBase"/> instance associated with the active scene.</returns>
        public static SaveControllerBase ActiveScene => ForScene(SceneManager.GetActiveScene());

        /// <summary>
        /// Gets the closest <see cref="SaveControllerBase"/> instance to the provided MonoBehaviour in hierarchy, the <see cref="SaveControllerBase"/> for its scene, or the Global <see cref="SaveControllerBase"/>.
        /// </summary>
        /// <param name="behaviour">The MonoBehaviour to find the <see cref="SaveControllerBase"/> for.</param>
        /// <returns>The closest <see cref="SaveControllerBase"/> instance, or the scene/global instance if none found in hierarchy.</returns>
        public static SaveControllerBase For(MonoBehaviour behaviour) => behaviour.GetComponentInParent<SaveProvider>().OrNull().Controller ?? ForSceneOf(behaviour) ?? Global;

        /// <summary>
        /// Gets the <see cref="SaveControllerBase"/> configured for the specified scene.
        /// </summary>
        /// <param name="scene">The scene to get the <see cref="SaveControllerBase"/> for.</param>
        /// <returns>The <see cref="SaveControllerBase"/> for the specified scene.</returns>
        public static SaveControllerBase ForScene(Scene scene)
        {
            // Check if a SaveProvider is already registered for the scene
            if (sceneContainers.TryGetValue(scene.name, out SaveProvider found)) return found.Controller;

            // Clear temporary list of GameObjects
            tmpSceneGameObjects.Clear();

            // Get all root GameObjects in the scene
            scene.GetRootGameObjects(tmpSceneGameObjects);

            // Search for a SceneSaveProvider in the scene's root GameObjects
            foreach (GameObject go in tmpSceneGameObjects.Where(go => go.GetComponent<SceneSaveProvider>() != null))
            {
                // Find the SceneSaveProvider component
                if (go.TryGetComponent(out SceneSaveProvider bootstrapper))
                {
                    // Bootstrap the scene's SaveProvider
                    bootstrapper.BootstrapOnDemand();

                    // Set the scene name on the bootstrapper
                    bootstrapper.SetName(scene.name);

                    // Return the scene's SaveProvider after bootstrapping
                    return bootstrapper.Container.Controller;
                }
            }

            // Try to create a new SceneSaveProvider if none found
            GameObject obj = new GameObject(SceneSaveProviderName(scene.name), typeof(SaveProvider));

            // Add SceneSaveProvider component to the new GameObject
            SceneSaveProvider container = obj.AddComponent<SceneSaveProvider>();

            // Bootstrap the new scene's SaveProvider
            container.BootstrapOnDemand();

            // Set the scene name on the locator
            container.SetName(scene.name);

            // Return the newly created scene's SaveProvider
            return container.Container.Controller;
        }

        /// <summary>
        /// Gets the <see cref="SaveControllerBase"/> configured for the specified scene name.
        /// </summary>
        /// <param name="sceneName">The name of the scene to get the <see cref="SaveControllerBase"/> for.</param>
        /// <returns>The <see cref="SaveControllerBase"/> for the specified scene.</returns>
        public static SaveControllerBase ForScene(string sceneName) => ForScene(SceneManager.GetSceneByName(sceneName));

        /// <summary>
        /// Gets the <see cref="SaveControllerBase"/> configured for the scene of a MonoBehaviour.
        /// </summary>
        /// <remarks>
        /// Falls back to the global instance if no scene-specific SaveProvider is found.
        /// </remarks>
        /// <returns>The <see cref="SaveControllerBase"/> for the scene of the provided MonoBehaviour, or the global instance if none found.</returns>
        public static SaveControllerBase ForSceneOf(MonoBehaviour behaviour) => ForScene(behaviour.gameObject.scene);

        /// <summary>
        /// Gets the appropriate SaveController based on the provided SaveScope.
        /// </summary>
        /// <param name="scope">The scope to get the SaveController for.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if an unsupported SaveScope is provided.</exception>
        /// <returns>A SaveController corresponding to the provided scope.</returns>
        public static SaveControllerBase ByScope(SaveScope scope)
        {
            // Return the appropriate SaveController based on the provided scope
            return scope switch
            {
                // Get the SaveController for the specified scope
                SaveScope.Absolute => Absolute,
                SaveScope.Global => Global,
                SaveScope.Scene => ActiveScene,
                SaveScope.Temporary => Temporary,

                // Should never happen due to enum constraints but throw an exception if an unsupported scope is provided
                _ => throw new System.ArgumentOutOfRangeException(nameof(scope), $"Unsupported SaveScope: {scope}"),
            };
        }

        /// <summary>
        /// Prepares static fields for a fresh runtime session.
        /// </summary>
        /// <remarks>
        /// See <a href="https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html">Unity Documentation</a> for more information.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Reset absolute instance
            absolute = null;

            // Reset global instance
            global = null;

            // Reset temporary instance
            temporary = null;

            // Initialize scene containers dictionary
            sceneContainers = new Dictionary<string, SaveProvider>();

            // Initialize temporary list for scene GameObjects
            tmpSceneGameObjects = new List<GameObject>();
        }

        /// <summary>
        /// Removes this SaveProvider from the global, temporary, or scene registry upon destruction.
        /// </summary>
        private void OnDestroy()
        {
            // Only proceed if this SaveProvider has been bootstrapped
            if (!isBootstrapped) return;

            // Check if this container is registered as absolute, global, temporary, or scene-specific
            if (this == absolute)
            {
                // Clear absolute instance if this is the absolute container
                absolute = null;
            }
            else if (this == global)
            {
                // Clear global instance if this is the global container
                global = null;
            }
            if (this == temporary)
            {
                // Clear temporary instance if this is the temporary container
                temporary = null;
            }
            else if (sceneContainers.ContainsValue(this))
            {
                // Remove this container from the scene containers dictionary
                sceneContainers.Remove(GetComponent<SceneSaveProvider>().TrackedScene.name);
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Adds an Absolute SaveProvider to the scene.
        /// </summary>
        [MenuItem("GameObject/Save Provider/Add Absolute")]
        private static void AddAbsolute() => new GameObject(k_absoluteSaveProviderName, typeof(AbsoluteSaveProvider));

        /// <summary>
        /// Adds a Global SaveProvider to the scene.
        /// </summary>
        [MenuItem("GameObject/Save Provider/Add Global")]
        private static void AddGlobal() => new GameObject(k_globalSaveProviderName, typeof(GlobalSaveProvider));

        /// <summary>
        /// Adds a Temporary SaveProvider to the scene.
        /// </summary>
        [MenuItem("GameObject/Save Provider/Add Temporary")]
        private static void AddTemporary() => new GameObject(k_TemporarySaveProviderName, typeof(TemporarySaveProvider));

        /// <summary>
        /// Adds a Scene SaveProvider to the scene.
        /// </summary>
        [MenuItem("GameObject/Save Provider/Add For Scene")]
        private static void AddForScene()
        {
            // Get the active scene name
            string sceneName = SceneManager.GetActiveScene().name;

            // Create a new GameObject for the Scene SaveProvider
            var obj = new GameObject(SceneSaveProviderName(sceneName), typeof(SceneSaveProvider));

            // Set the scene name on the locator
            obj.GetComponent<SceneSaveProvider>().SetName(sceneName);
        }

#endif
    }
}