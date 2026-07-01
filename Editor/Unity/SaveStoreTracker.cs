using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sanctuary.Stores;

namespace Sanctuary.Editor
{
    public class SaveStoreTracker : EditorWindow
    {
        private Dictionary<SaveControllerBase, List<ISaveStore>> registeredStores = new();

        private Vector2 scrollPosition;

        [MenuItem("Window/Sanctuary/Store Tracker")]
        public static void Open()
        {
            // Get the Save Store Tracker window or create a new one if it doesn't exist
            var window = GetWindow<SaveStoreTracker>("Store Tracker");

            // Find the icon texture
            Texture icon = EditorGUIUtility.FindTexture("Assets/Plugins/Sanctuary/Editor/EditorResources/TrackerWindow.png");

            // Set the title content of the window with the icon
            window.titleContent = new GUIContent("Store Tracker", icon);

            // Show the window
            window.Show();
        }

        private void OnEnable()
        {
            // Register the event handlers for when a save store is registered or unregistered
            SaveStoreRegistry.OnStoreRegistered += Register;
            SaveStoreRegistry.OnStoreUnregistered += Unregister;
        }

        private void OnDisable()
        {
            // Unregister the event handlers for when a save store is registered or unregistered
            SaveStoreRegistry.OnStoreRegistered -= Register;
            SaveStoreRegistry.OnStoreUnregistered -= Unregister;
        }

        private void OnGUI()
        {
            // Add a space before the title and description of the window
            EditorGUILayout.Separator();

            // Display the title and description of the window
            EditorGUILayout.LabelField("Store Tracker", HeaderStyle(Color.gold));
            EditorGUILayout.LabelField("This window is used to track the active save stores in the project.", CenteredMiniLabelStyle(Color.gray));

            // Add a space between the label and the list of save stores
            EditorGUILayout.Separator();

            // Define the area for the list of save stores with a box style
            var areaRect = new Rect(5, 60, position.width - 10, position.height - 85);

            // Begin an area for the list of save stores with a box style
            GUILayout.BeginArea(areaRect);

            // Start a scroll view to display the list of save stores
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // If the application is in play mode, display the list of save stores, otherwise display a message indicating that the window is only available in play mode
            if (Application.isPlaying)
            {
                // Display the list of save stores in play mode
                PlayModeDisplay();
            }
            else
            {
                // Define a style for the empty state message, which is centered and stretches to fill the available height and width of the window
                var emptyStateStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    stretchHeight = true
                };

                // Display a message indicating that the window is only available in play mode
                GUILayout.Box("The Save Store Tracker window is only available in play mode.", emptyStateStyle);
            }

            // End the scroll view
            EditorGUILayout.EndScrollView();

            // End the area for the list of save stores
            GUILayout.EndArea();

            // Push the refresh button to the bottom of the window
            GUILayout.FlexibleSpace();

            // Push the refresh button to the right side of the window
            GUILayout.FlexibleSpace();

            // Add a button to refresh the list of save stores
            if (Application.isPlaying && GUILayout.Button("Refresh")) registeredStores = Convert(SaveStoreRegistry.GetRegisteredStores());
        }

        private void Register(ISaveStore store, SaveControllerBase controller)
        {
            // Check if the controller is already registered in the dictionary, if not, add it with an empty list of stores
            if (!registeredStores.ContainsKey(controller)) registeredStores[controller] = new List<ISaveStore>();

            // Add the store to the list of stores for the specified controller
            registeredStores[controller].Add(store);
        }

        private void Unregister(ISaveStore store)
        {
            // Iterate through the registered stores dictionary to find the controller associated with the store to be unregistered
            foreach (var kvp in registeredStores)
            {
                // Check if the list of stores for this controller contains the store to be unregistered
                if (kvp.Value.Contains(store))
                {
                    // Remove the store from the list of stores for this controller
                    kvp.Value.Remove(store);

                    // If the list of stores for this controller is now empty, remove the controller from the dictionary
                    if (kvp.Value.Count == 0) registeredStores.Remove(kvp.Key);

                    // Exit the loop after removing the store to avoid modifying the collection while iterating
                    break;
                }
            }
        }

        private void PlayModeDisplay()
        {
            // If the registered stores dictionary is empty, display a message indicating that there are no save stores registered
            if (registeredStores.Count == 0)
            {
                // Display a message indicating that there are no save stores registered
                EditorGUILayout.LabelField("No save stores registered.");

                // Return early to avoid displaying an empty list
                return;
            }

            // Display the list of save stores here
            foreach (var kvp in registeredStores)
            {
                // Get the save store and its associated controller from the dictionary
                var controller = kvp.Key;
                var stores = kvp.Value;

                // Display the name of the controller associated with the save store
                if (controller != null) EditorGUILayout.LabelField($"{controller.Name}");

                // Iterate through the list of save stores for the current controller
                for (var i = 0; i < stores.Count; i++)
                {
                    // Get the current save store from the list of stores
                    var store = stores[i];

                    // Display the source object of the save store if it exists, otherwise display "None" and the type of the save store
                    if (store.Source != null) EditorGUILayout.ObjectField(store.Source, typeof(UnityEngine.Object), true);
                    else EditorGUILayout.LabelField("Source: None | Type: " + store.GetType().Name);
                }

                // Draw a horizontal line to separate the save stores for each controller
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // Add a space between the list of save stores for each controller
                EditorGUILayout.Separator();
            }
        }

        private static GUIStyle HeaderStyle(Color textColor)
        {
            // Create a new GUIStyle for the header
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(-5, -5, -5, -5),
                normal = { textColor = textColor }
            };
        }

        private static GUIStyle CenteredMiniLabelStyle(Color textColor)
        {
            // Create a new GUIStyle for the header
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(-5, -5, -5, -5),
                normal = { textColor = textColor }
            };
        }

        private static Dictionary<SaveControllerBase, List<ISaveStore>> Convert(Dictionary<ISaveStore, SaveControllerBase> source)
        {
            // Create a new dictionary to hold the converted data
            var target = new Dictionary<SaveControllerBase, List<ISaveStore>>();

            // Iterate through the source dictionary to convert it into the target dictionary
            foreach (var kvp in source)
            {
                // Get the save store and its associated controller from the source dictionary
                var store = kvp.Key;
                var controller = kvp.Value;

                // Check if the controller is already registered in the target dictionary, if not, add it with an empty list of stores
                if (!target.ContainsKey(controller)) target[controller] = new List<ISaveStore>();

                // Add the store to the list of stores for the specified controller in the target dictionary
                target[controller].Add(store);
            }

            // Return the converted dictionary that maps SaveControllerBase to a list of ISaveStore
            return target;
        }
    }
}
