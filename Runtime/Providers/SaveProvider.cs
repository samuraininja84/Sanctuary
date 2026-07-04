using UnityEngine;

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

        /// <summary>
        /// A flag indicating whether to automatically save before the save provider is destroyed.
        /// </summary>
        public static bool saveOnExit = false;

        /// <summary>
        /// The absolute SaveProvider instance.
        /// </summary>
        private static ISaveController absolute;

        /// <summary>
        /// The global SaveProvider instance.
        /// </summary>
        private static ISaveController global;

        /// <summary>
        /// The temporary SaveProvider instance.
        /// </summary>
        private static ISaveController temporary;
        
        /// <summary>
        /// Retrieves the <see cref="AbsoluteSaveProvider"/>'s <see cref="SaveControllerBase"/> instance, creating one if it does not already exist.
        /// </summary>
        /// <remarks>
        /// This property checks for an existing <see cref="AbsoluteSaveProvider"/> instance. If none is found, it searches the scene for a <see cref="AbsoluteSaveProvider"/> component.
        /// If found, it bootstraps that instance. If no <see cref="AbsoluteSaveProvider"/> exists in the scene, a new GameObject is created with a <see cref="AbsoluteSaveProvider"/> component, and it is bootstrapped.
        /// </remarks>
        /// <returns>The <see cref="AbsoluteSaveProvider"/>'s <see cref="SaveControllerBase"/>  instance.</returns>
        public static ISaveController Absolute
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
        public static ISaveController Global
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
        public static ISaveController Temporary
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
        /// Gets the appropriate SaveController based on the provided SaveScope.
        /// </summary>
        /// <param name="scope">The scope to get the SaveController for.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if an unsupported SaveScope is provided.</exception>
        /// <returns>A SaveController corresponding to the provided scope.</returns>
        public static ISaveController ByScope(this SaveScope scope)
        {
            // Return the appropriate SaveController based on the provided scope
            return scope switch
            {
                // Get the SaveController for the specified scope
                SaveScope.Absolute => Absolute,
                SaveScope.Global => Global,
                SaveScope.Temporary => Temporary,

                // Should never happen due to enum constraints but throw an exception if an unsupported scope is provided
                _ => throw new System.ArgumentOutOfRangeException(nameof(scope), $"Unsupported Save Scope: {scope}")
            };
        }

        /// <summary>
        /// Sets up the SaveProvider based on the provided SaveScope, configuring it as Absolute, Global, Scene, or Temporary as appropriate.
        /// </summary>
        /// <param name="scope">The scope to configure the SaveProvider for.</param>
        /// <param name="source">The source SaveController to configure.</param>
        /// <param name="profile">The profile data to use for the configuration.</param>
        /// <returns>True if the configuration was successful; otherwise, false.</returns>
        public static bool SetByScope(SaveScope scope, ISaveController source)
        {
            switch (scope)
            {
                case SaveScope.Absolute:
                    return ConfigureAsAbsolute(source);
                case SaveScope.Global:
                    ConfigureAsGlobal(source);
                    return true;
                case SaveScope.Temporary:
                    ConfigureAsTemporary(source);
                    return true;
                default:
                    Debug.LogWarning($"[Sanctuary]: SaveProvider.SetByScope: Unsupported Save Scope: {scope}");
                    return false;
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the absolute instance by marking as absolute. Only one absolute instance can exist at a time.
        /// </summary>
        /// <param name="source">The source SaveController to configure as absolute.</param>
        /// <param name="profile">The profile data to use for the configuration.</param>
        /// <returns>True if the configuration was successful; otherwise, false.</returns>
        private static bool ConfigureAsAbsolute(ISaveController source)
        {
            // Check if already configured as absolute
            if (absolute == source)
            {
                // Already configured as absolute
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsAbsolute: Already configured as absolute");

                // Return true to indicate that the configuration was successful
                return true;
            }
            else if (absolute != null)
            {
                // Another absolute container already exists
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsAbsolute: Another SaveProvider is already configured as absolute");

                // Return false to indicate that the configuration was not successful
                return false;
            }
            else
            {
                // Configure as absolute
                absolute = source;

                // Return true to indicate that the configuration was successful
                return true;
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the global instance by marking as global and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is true.</param>
        private static bool ConfigureAsGlobal(ISaveController source)
        {
            // Check if already configured as global
            if (global == source)
            {
                // Already configured as global
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsGlobal: Already configured as global");

                // Return false to indicate that the configuration was not successful
                return false;
            }
            else if (global != null)
            {
                // Another global container already exists
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsGlobal: Another SaveProvider is already configured as global");

                // Return false to indicate that the configuration was not successful
                return false;
            }
            else
            {
                // Configure as global
                global = source;

                // Return true to indicate that the configuration was successful
                return true;
            }
        }

        /// <summary>
        /// Sets up this SaveProvider as the temporary instance by marking as temporary and optionally making persistent across scene loads.
        /// </summary>
        /// <param name="dontDestroyOnLoad">The GameObject will persist across scene loads if true. Default is false.</param>
        private static bool ConfigureAsTemporary(ISaveController source)
        {
            // Check if already configured as temporary
            if (temporary == source)
            {
                // Already configured as temporary
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsTemporary: Already configured as temporary");

                // Return false to indicate that the configuration was not successful
                return false;
            }
            else if (temporary != null)
            {
                // Another temporary container already exists
                Debug.LogWarning("[Sanctuary]: SaveProvider.ConfigureAsTemporary: Another SaveProvider is already configured as temporary");

                // Return false to indicate that the configuration was not successful
                return false;
            }
            else
            {
                // Configure as temporary
                temporary = source;

                // Return true to indicate that the configuration was successful
                return true;
            }
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

#endif
    }
}