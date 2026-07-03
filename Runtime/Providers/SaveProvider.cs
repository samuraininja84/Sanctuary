using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sanctuary.Loaders;
using Sanctuary.Extensions;
using Sanctuary.Configuration;
using Sanctuary.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace Sanctuary
{
    public static class SaveProvider
    {
        private const string k_absoluteSaveProviderName = "SaveProvider [Absolute]";
        private const string k_globalSaveProviderName = "SaveProvider [Global]";
        private const string k_TemporarySaveProviderName = "SaveProvider [Temporary]";
        private static string SceneSaveProviderName(string sceneName) => $"SaveProvider [{sceneName}]";

        /// <summary>
        /// A flag indicating whether to automatically save before the save provider is destroyed.
        /// </summary>
        public static bool saveOnExit = false;

        /// <summary>
        /// The absolute SaveProvider instance.
        /// </summary>
        private static SaveControllerBase absolute;

        /// <summary>
        /// The global SaveProvider instance.
        /// </summary>
        private static SaveControllerBase global;

        /// <summary>
        /// The temporary SaveProvider instance.
        /// </summary>
        private static SaveControllerBase temporary;

        /// <summary>
        /// The dictionary mapping scenes to their respective SaveProvider instances.
        /// </summary>
        public static Dictionary<string, SaveControllerBase> sceneContainers = new();

        /// <summary>
        /// The temporary list used for storing root GameObjects in a scene during lookup.
        /// </summary>
        private static List<GameObject> tmpSceneGameObjects = new();
        
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
                if (absolute != null) return absolute;

                // Try to find an existing AbsoluteSaveProvider in the scene
                if (Object.FindFirstObjectByType<AbsoluteSaveProvider>() is { } found)
                {
                    // Bootstrap the found global instance
                    found.Initialize();

                    // Return the absolute instance after bootstrapping
                    return absolute;
                }

                // Create a new GameObject to hold the absolute SaveProvider
                var container = new GameObject(k_absoluteSaveProviderName, typeof(SaveProvider));

                // Bootstrap the new absolute instance
                container.AddComponent<AbsoluteSaveProvider>().Initialize();

                // Return the newly created absolute instance
                return absolute;
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
                if (global != null) return global;

                // Try to find an existing GlobalSaveProvider in the scene
                if (Object.FindFirstObjectByType<GlobalSaveProvider>() is { } found)
                {
                    // Bootstrap the found global instance
                    found.Initialize();

                    // Return the global instance after bootstrapping
                    return global;
                }

                // Create a new GameObject to hold the global SaveProvider
                var container = new GameObject(k_globalSaveProviderName, typeof(SaveProvider));

                // Bootstrap the new global instance
                container.AddComponent<GlobalSaveProvider>().Initialize();

                // Return the newly created global instance
                return global;
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
                if (temporary != null) return temporary;

                // Try to find an existing TemporarySaveProvider in the scene
                if (Object.FindFirstObjectByType<TemporarySaveProvider>() is { } found)
                {
                    // Bootstrap the found temporary instance
                    found.Initialize();

                    // Return the temporary instance after bootstrapping
                    return temporary;
                }

                // Create a new GameObject to hold the temporary SaveProvider
                var container = new GameObject(k_TemporarySaveProviderName, typeof(SaveProvider));

                // Bootstrap the new temporary instance
                container.AddComponent<TemporarySaveProvider>().Initialize();

                // Return the newly created temporary instance
                return temporary;
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
        /// Sets up this SaveProvider as the absolute instance by marking as absolute and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is true.</param>
        public static async void ConfigureAsAbsolute(SaveControllerBase source, ProfileData profile, bool loadOnBoot = true, bool dontDestroyOnLoad = true)
        {
            // Check if already configured as absolute
            if (absolute == source)
            {
                // Already configured as absolute
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsAbsolute: Already configured as absolute", source);
            }
            else if (absolute != null)
            {
                // Another absolute container already exists
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsAbsolute: Another SaveProvider is already configured as absolute", source);

                // Destroy this GameObject to enforce singleton pattern
                Object.Destroy(source.gameObject);
            }
            else
            {
                // Configure as absolute
                absolute = source;

                // Make persistent across scenes if specified and in play mode
                if (dontDestroyOnLoad && Application.isPlaying) Object.DontDestroyOnLoad(source.gameObject);

                // If an absolute save doesn't already exist, create one
                if (!source.Exists) await source.Save(SaveMode.Full);

                // Load the absolute save if specified
                if (loadOnBoot) await source.Load(SaveMode.Full);
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the global instance by marking as global and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is true.</param>
        public static void ConfigureAsGlobal(SaveControllerBase source, ProfileData profile, bool dontDestroyOnLoad = true)
        {
            // Check if already configured as global
            if (global == source)
            {
                // Already configured as global
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsGlobal: Already configured as global", source);
            }
            else if (global != null)
            {
                // Another global container already exists
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsGlobal: Another SaveProvider is already configured as global", source);

                // Destroy this GameObject to enforce singleton pattern
                Object.Destroy(source.gameObject);
            }
            else
            {
                // Configure as global
                global = source;

                // Make persistent across scenes if specified and in play mode
                if (dontDestroyOnLoad && Application.isPlaying) Object.DontDestroyOnLoad(source.gameObject);
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the temporary instance by marking as temporary and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is false.</param>
        public static void ConfigureAsTemporary(SaveControllerBase source, ProfileData profile, bool dontDestroyOnLoad = false)
        {
            // Check if already configured as temporary
            if (temporary == source)
            {
                // Already configured as temporary
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsTemporary: Already configured as temporary", source);
            }
            else if (temporary != null)
            {
                // Another temporary container already exists
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsTemporary: Another SaveProvider is already configured as temporary", source);

                // Destroy this GameObject to enforce singleton pattern
                Object.Destroy(source.gameObject);
            }
            else
            {
                // Configure as temporary
                temporary = source;

                // Make persistent across scenes if specified and in play mode
                if (dontDestroyOnLoad && Application.isPlaying) Object.DontDestroyOnLoad(source.gameObject);
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the instance for its scene.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is false.</param>
        public static void ConfigureForScene(SaveControllerBase source, ProfileData profile, bool dontDestroyOnLoad = false)
        {
            // Get the scene this GameObject belongs to
            string scene = source.gameObject.scene.name;

            // Check if another container is already registered for this scene
            if (sceneContainers.ContainsKey(scene))
            {
                // Log error if another container is already registered for this scene
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureForScene: Another SaveProvider is already configured for this scene, destroying this one", source);

                // Destroy this GameObject to enforce singleton pattern for the scene
                Object.Destroy(source.gameObject);

                // Return early, since a container is already registered for this scene
                return;
            }

            // Initialize scene save controller if needed
            profile.SetFileName(scene);

            // Register this container for the scene
            sceneContainers.Add(scene, source);

            // Make persistent across scenes if specified and in play mode
            if (dontDestroyOnLoad && Application.isPlaying) Object.DontDestroyOnLoad(source.gameObject);
        }

        /// <summary>
        /// Gets the closest <see cref="SaveControllerBase"/> instance to the provided MonoBehaviour in hierarchy, the <see cref="SaveControllerBase"/> for its scene, or the Global <see cref="SaveControllerBase"/>.
        /// </summary>
        /// <param name="behaviour">The MonoBehaviour to find the <see cref="SaveControllerBase"/> for.</param>
        /// <returns>The closest <see cref="SaveControllerBase"/> instance, or the scene/global instance if none found in hierarchy.</returns>
        public static SaveControllerBase For(this MonoBehaviour behaviour) => behaviour.GetComponentInParent<SaveControllerBase>().OrNull() ?? ForSceneOf(behaviour) ?? Global;

        /// <summary>
        /// Gets the <see cref="SaveControllerBase"/> configured for the specified scene.
        /// </summary>
        /// <param name="scene">The scene to get the <see cref="SaveControllerBase"/> for.</param>
        /// <returns>The <see cref="SaveControllerBase"/> for the specified scene.</returns>
        public static SaveControllerBase ForScene(Scene scene)
        {
            // Check if a SaveProvider is already registered for the scene
            if (sceneContainers.TryGetValue(scene.name, out var found)) return found;

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
                    bootstrapper.Initialize();

                    // Set the scene name on the bootstrapper
                    bootstrapper.SetName(scene.name);

                    // Return the scene's SaveProvider after bootstrapping
                    return bootstrapper;
                }
            }

            // Try to create a new SceneSaveProvider if none found
            GameObject obj = new(SceneSaveProviderName(scene.name), typeof(SaveProvider));

            // Add SceneSaveProvider component to the new GameObject
            SceneSaveProvider container = obj.AddComponent<SceneSaveProvider>();

            // Bootstrap the new scene's SaveProvider
            container.Initialize();

            // Set the scene name on the locator
            container.SetName(scene.name);

            // Return the newly created scene's SaveProvider
            return container;
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
        public static SaveControllerBase ForSceneOf(this MonoBehaviour behaviour) => ForScene(behaviour.gameObject.scene);

        /// <summary>
        /// Gets the appropriate SaveController based on the provided SaveScope.
        /// </summary>
        /// <param name="scope">The scope to get the SaveController for.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if an unsupported SaveScope is provided.</exception>
        /// <returns>A SaveController corresponding to the provided scope.</returns>
        public static SaveControllerBase ByScope(this SaveScope scope)
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
                _ => throw new System.ArgumentOutOfRangeException(nameof(scope), $"Unsupported Save Scope: {scope}")
            };
        }

        /// <summary>
        /// Clears the SaveController instance for the specified SaveScope.
        /// </summary>
        /// <param name="scope">The scope to clear the SaveController for.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if an unsupported SaveScope is provided.</exception>
        public static void ClearByScope(SaveScope scope)
        {
            switch (scope)
            {
                case SaveScope.Absolute:
                    absolute = null;
                    break;
                case SaveScope.Global:
                    global = null;
                    break;
                case SaveScope.Scene:
                    sceneContainers.Clear();
                    break;
                case SaveScope.Temporary:
                    temporary = null;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(scope), $"Unsupported Save Scope: {scope}");
            }
        }

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
            sceneContainers = new();

            // Initialize temporary list for scene GameObjects
            tmpSceneGameObjects = new List<GameObject>();
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