using Sanctuary.Attributes;
using Sanctuary.Stores;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sanctuary.Editor
{
    /// <summary>
    /// The SanctuaryEditorProcessor class handles loading and saving save data when entering and exiting Play Mode in the Unity Editor.
    /// </summary>
    [InitializeOnLoad]
    public static class SanctuaryEditorProcessor
    {
        // Load and save options
        public static bool loadOnEnter = false;

        // Display options
        public static bool showLocationWhenNamed = false;

        // File filtering
        public static bool filterFiles = false;

        /// <summary>
        /// The EditorPrefs key for saving the <see cref="loadOnEnter"/> preference.
        /// </summary>
        public const string loadOnEnterKey = "Sanctuary_LoadOnEnterPlayMode";

        /// <summary>
        /// The EditorPrefs key for saving the <see cref="saveOnExit"/> preference.
        /// </summary>
        public const string saveOnExitKey = "Sanctuary_SaveOnExitPlayMode";

        /// <summary>
        /// The EditorPrefs key for saving the show location when named preference.
        /// </summary>
        public const string showLocationKey = "Sanctuary_ShowLocationWhenNamed";

        /// <summary>
        /// The EditorPrefs key for saving the filter files preference.
        /// </summary>
        public const string filterFilesKey = "Sanctuary_FilterFiles";

        /// <summary>
        /// The menu path for accessing Sanctuary's preferences in Unity's Preferences window.
        /// </summary>
        public const string preferencesMenuPath = "Preferences/Sanctuary";

        static SanctuaryEditorProcessor()
        {
            // Subscribe to play mode state changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // If there is no key for loadOnEnter, set it to false
            if (!EditorPrefs.HasKey(loadOnEnterKey)) EditorPrefs.SetBool(loadOnEnterKey, false);

            // Load the loadOnEnter preference from EditorPrefs
            loadOnEnter = EditorPrefs.GetBool(loadOnEnterKey);

            // If there is no key for saveOnExit, set it to true
            if (!EditorPrefs.HasKey(saveOnExitKey)) EditorPrefs.SetBool(saveOnExitKey, true);

            // Load the saveOnExit preference from EditorPrefs
            SaveProvider.saveOnExit = EditorPrefs.GetBool(saveOnExitKey);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Handle different play mode state changes
            switch (state)
            {
                // When entering play mode
                case PlayModeStateChange.EnteredPlayMode:
                    // Check if loading on enter is enabled
                    if (loadOnEnter)
                    {
                        // Log a message indicating that save data is being loaded
                        Debug.Log("Loading all existing save data from disk on entering Play Mode...");

                        // Load all existing save data from disk
                        SaveStoreRegistry.LoadAll();
                    }
                    break;
                // When exiting play mode clean up the cache
                case PlayModeStateChange.ExitingPlayMode:
                    SanctuaryEditor.ClearCache();
                    break;
            }
        }

        [SanctuaryToolbarButton]
        private static void LoadButton()
        {
            // Create Open Saves Path Content
            GUIContent loadContent = EditorGUIUtility.IconContent("Import");
            loadContent.tooltip = loadOnEnter ? "Disable Load on Enter Play Mode" : "Enable Load on Enter Play Mode";

            // Change GUI color based on loadOnEnter state
            GUI.contentColor = loadOnEnter ? Color.green : Color.red;

            // Draw a button to load all existing save data from disk
            if (GUILayout.Button(loadContent, EditorStyles.toolbarButton))
            {
                // Switch the loadOnEnter state
                loadOnEnter = !loadOnEnter;

                // Set the EditorPrefs value for loadOnEnter
                EditorPrefs.SetBool(loadOnEnterKey, loadOnEnter);
            }

            // Reset GUI color
            GUI.contentColor = Color.white;
        }

        [SanctuaryToolbarButton]
        private static void SaveButton()
        {
            // Create Open Saves Path Content
            GUIContent saveContent = EditorGUIUtility.IconContent("d_SaveAs");
            saveContent.tooltip = SaveProvider.saveOnExit ? "Disable Save on Exit Play Mode" : "Enable Save on Exit Play Mode";

            // Change GUI color based on saveOnExit state
            GUI.contentColor = SaveProvider.saveOnExit ? Color.green : Color.red;

            // Draw a button to save all save data to disk
            if (GUILayout.Button(saveContent, EditorStyles.toolbarButton))
            {
                // Switch the saveOnExit state
                SaveProvider.saveOnExit = !SaveProvider.saveOnExit;

                // Set the EditorPrefs value for saveOnExit
                EditorPrefs.SetBool(saveOnExitKey, SaveProvider.saveOnExit);
            }

            // Reset GUI color
            GUI.contentColor = Color.white;
        }

        [SanctuaryToolbarButton]
        private static void ToggleShowLocation()
        {
            // Create toggle button content
            GUIContent toggleContent = EditorGUIUtility.IconContent("d_FilterByType");
            toggleContent.tooltip = showLocationWhenNamed ? "Showing location names. Click to hide." : "Hiding location names. Click to show.";

            // Set content color based on toggle state
            GUI.contentColor = showLocationWhenNamed ? Color.green : Color.red;

            // Draw the toggle button
            if (GUILayout.Button(toggleContent, EditorStyles.toolbarButton))
            {
                // Toggle the showLocationWhenNamed state
                showLocationWhenNamed = !showLocationWhenNamed;

                // Save the new value to EditorPrefs
                EditorPrefs.SetBool(showLocationKey, showLocationWhenNamed);
            }

            // Reset content color
            GUI.contentColor = Color.white;
        }

        [SanctuaryToolbarButton]
        private static void ToggleFilterFiles()
        {
            // Create toggle button content
            GUIContent toggleContent = EditorGUIUtility.IconContent("d_Animation.FilterBySelection");
            toggleContent.tooltip = filterFiles ? "Filtering files enabled. Click to disable." : "Filtering files disabled. Click to enable.";

            // Set content color based on toggle state
            GUI.contentColor = filterFiles ? Color.green : Color.red;

            // Draw the toggle button
            if (GUILayout.Button(toggleContent, EditorStyles.toolbarButton))
            {
                // Toggle the filterFiles state
                filterFiles = !filterFiles;

                // Save the new value to EditorPrefs
                EditorPrefs.SetBool(filterFilesKey, filterFiles);
            }

            // Reset content color
            GUI.contentColor = Color.white;
        }

        [SanctuaryToolbarButton]
        private static void OpenPreferences()
        {
            // Create Open Preferences Content
            GUIContent preferencesContent = EditorGUIUtility.IconContent("d_SettingsIcon");
            preferencesContent.tooltip = "Open Sanctuary Preferences";

            // Draw a button to open the Sanctuary preferences
            if (GUILayout.Button(preferencesContent, EditorStyles.toolbarButton))
            {
                // Open the Sanctuary preferences in Unity's Preferences window
                SettingsService.OpenUserPreferences(preferencesMenuPath);
            }
        }
    }

    /// <summary>
    /// A settings provider for Sanctuary preferences, allowing users to configure load and save options in the Unity Editor.
    /// </summary>
    public class SanctuaryPreferencesProvider : SettingsProvider
    {
        private static List<string> evaluatedAssemblies = new List<string>();
        private static bool foldoutOpen = true;

        public SanctuaryPreferencesProvider(string path, SettingsScope scopes = SettingsScope.User) : base(path, scopes) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // Call the base method
            base.OnActivate(searchContext, rootElement);

            // Initialize the list of evaluated assemblies
            evaluatedAssemblies = CompletionExtensions.GetEvaluatedAssemblies().Select(assembly => assembly.GetName().Name).ToList();
        }

        public override void OnGUI(string searchContext)
        {
            // Draw the default settings provider GUI
            base.OnGUI(searchContext);

            // Draw a label for the preferences description
            GUILayout.Label("Configure preferences for loading and saving save data in Play Mode.", EditorStyles.wordWrappedLabel);

            // Add a space for better layout
            GUILayout.Space(10);

            // Display the title and description for the Sanctuary Preferences
            GUILayout.Label("Sanctuary Preferences", EditorStyles.boldLabel);

            // Load on Enter Play Mode toggle
            bool newLoadOnEnter = EditorGUILayout.Toggle("Load on Enter Play Mode", SanctuaryEditorProcessor.loadOnEnter, GUILayout.ExpandWidth(true));

            // Check if the value has changed
            if (newLoadOnEnter != SanctuaryEditorProcessor.loadOnEnter)
            {
                // Update the static field in SanctuaryEditorProcessor
                SanctuaryEditorProcessor.loadOnEnter = newLoadOnEnter;

                // Save the new value to EditorPrefs
                EditorPrefs.SetBool(SanctuaryEditorProcessor.loadOnEnterKey, newLoadOnEnter);
            }

            // Save on Exit Play Mode toggle
            bool newSaveOnExit = EditorGUILayout.Toggle("Save on Exit Play Mode", SaveProvider.saveOnExit, GUILayout.ExpandWidth(true));

            // Check if the value has changed
            if (newSaveOnExit != SaveProvider.saveOnExit)
            {
                // Update the static field in SanctuaryEditorProcessor
                SaveProvider.saveOnExit = newSaveOnExit;

                // Save the new value to EditorPrefs
                EditorPrefs.SetBool(SanctuaryEditorProcessor.saveOnExitKey, newSaveOnExit);
            }

            // Show Location When Named toggle
            bool newShowLocation = EditorGUILayout.Toggle("Show Location When Named", SanctuaryEditorProcessor.showLocationWhenNamed, GUILayout.ExpandWidth(true));

            // Check if the value has changed
            if (newShowLocation != SanctuaryEditorProcessor.showLocationWhenNamed)
            {
                // Update the static field in SanctuaryEditor
                SanctuaryEditorProcessor.showLocationWhenNamed = newShowLocation;

                // Save the new value to EditorPrefs
                EditorPrefs.SetBool(SanctuaryEditorProcessor.showLocationKey, newShowLocation);
            }

            // Filter Files toggle
            bool newFilterFiles = EditorGUILayout.Toggle("Filter Files", SanctuaryEditorProcessor.filterFiles, GUILayout.ExpandWidth(true));

            // Check if the value has changed
            if (newFilterFiles != SanctuaryEditorProcessor.filterFiles)
            {
                // Update the static field in SanctuaryEditorProcessor
                SanctuaryEditorProcessor.filterFiles = newFilterFiles;

                // Save the new value to EditorPrefs
                EditorPrefs.SetBool(SanctuaryEditorProcessor.filterFilesKey, newFilterFiles);
            }

            // Create a foldout for the evaluated assemblies list
            if (foldoutOpen = EditorGUILayout.Foldout(foldoutOpen, "Evaluated Assemblies"))
            {
                // Indent the following GUI elements
                EditorGUI.indentLevel++;

                // Disable the GUI
                GUI.enabled = false;

                // Display each evaluated assembly
                foreach (string assembly in evaluatedAssemblies) EditorGUILayout.TextField(assembly);

                // Re-enable the GUI
                GUI.enabled = true;

                // Decrease the indent level
                EditorGUI.indentLevel--;
            }

            // Check if there are no evaluated assemblies
            if (evaluatedAssemblies.Count == 0) EditorGUILayout.HelpBox("No assemblies have been evaluated yet. Enter Play Mode to evaluate assemblies with CompletionEvaluationAttribute.", MessageType.Info);

            // Add some space before the refresh button
            GUILayout.Space(5);

            // Draw a label for the refresh button
            GUILayout.Label("Evaluated Assemblies Actions", EditorStyles.boldLabel);

            // Draw a button to refresh the list of evaluated assemblies
            if (GUILayout.Button("Refresh Evaluated Assemblies", EditorStyles.miniButton)) evaluatedAssemblies = CompletionExtensions.GetEvaluatedAssemblies().Select(assembly => assembly.GetName().Name).ToList();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SanctuaryPreferencesProvider"/> for user settings.
        /// </summary>
        /// <returns>A <see cref="SettingsProvider"/> configured for the user scope, allowing access to the <see cref="SanctuaryEditorProcessor"/> preferences.</returns>
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => new SanctuaryPreferencesProvider(SanctuaryEditorProcessor.preferencesMenuPath, SettingsScope.User);
    }
}