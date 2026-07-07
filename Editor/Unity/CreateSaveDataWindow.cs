using UnityEngine;
using UnityEditor;
using Sanctuary.Extensions;

namespace Sanctuary.Editor
{
    public class CreateSaveDataWindow : ScriptableWizard
    {
        private string saveName = "New Save";
        private int selectedIndex = 0;
        private ISaveController selectedController;
        private ISaveController[] saveControllers;

        public static void Show(ISaveController[] controllers)
        {
            // Create Instance without using GetWindow to avoid immediate rendering
            var window = GetWindow<CreateSaveDataWindow>("Create Save Data");
            window.titleContent = new GUIContent("Create Save Data");

            // Store the save controllers in the window instance for later use
            window.saveControllers = controllers;

            // Sets window size limits
            window.minSize = new Vector2(300, 150);
            window.maxSize = new Vector2(300, 200);

            // Shows window as a blocking modal
            window.Show();
        }

        private void OnGUI()
        {
            // Set the other button label to "Cancel"
            otherButtonName = "Cancel";

            // Draw the input field for the new item name
            saveName = EditorGUILayout.TextField(new GUIContent("New Save Name"), saveName);

            // Draw an enum popup for selecting the save controller
            if (saveControllers != null && saveControllers.Length > 0)
            {
                // Create an array of strings to hold the names of the save controllers
                string[] options = new string[saveControllers.Length];

                // Populate the options array with the names of the save controllers
                for (int i = 0; i < saveControllers.Length; i++) options[i] = saveControllers[i].Name;

                // Draw the popup for selecting the save controller
                selectedIndex = EditorGUILayout.Popup("Save Controller", selectedIndex, options);

                // Update the selected controller based on the selected index
                selectedController = saveControllers[selectedIndex];
            }

            // Push the button to the bottom of the window
            GUILayout.FlexibleSpace();

            // Check if the save name is not empty
            var isNotEmpty = !string.IsNullOrEmpty(saveName);

            // Check if the save name is unique by checking if the selected controller is not null and if it does not already have a slot with the same name
            var isUnique = selectedController != null && !selectedController.HasSlot(saveName);

            // If the save name is empty, or matches an existing save name, display a warning message to the user
            if (!isNotEmpty || !isUnique) EditorGUILayout.HelpBox("Please enter a unique save name.", MessageType.Warning);

            // Begin a horizontal layout for the buttons at the bottom of the window
            EditorGUILayout.BeginHorizontal();

            // Draw a create button that is only enabled if a save controller is selected
            EditorGUI.BeginDisabledGroup(selectedController == null || !isNotEmpty || !isUnique);

            // Draw the create button and call Create when clicked
            if (GUILayout.Button("Create")) Create();

            // End the disabled group for the create button
            EditorGUI.EndDisabledGroup();

            // Draw a cancel button that closes the window when clicked
            if (GUILayout.Button("Cancel")) Close();

            // End the horizontal layout for the buttons
            EditorGUILayout.EndHorizontal();
        }

        public async void Create()
        {
            // If the save name is empty, display an error message and return early
            if (string.IsNullOrEmpty(saveName))
            {
                // Display an error dialog to inform the user that the save name cannot be empty
                var retry = EditorUtility.DisplayDialog("Error", "Save name cannot be empty. Please enter a valid save name.", "Retry", "Cancel");

                // If the user clicks "Retry", return early to avoid proceeding with the creation process
                if (retry) Show(saveControllers);

                // Return early to avoid proceeding with the creation process
                return;
            }

            // If no save controllers are available, display an error message and return early
            if (saveControllers == null || saveControllers.Length == 0)
            {
                // Display an error dialog to inform the user that no save controllers are available
                var retry = EditorUtility.DisplayDialog("Error", "No save controllers available. Please ensure that at least one save controller is registered.", "Retry", "Cancel");

                // If the user clicks "Retry", return early to avoid proceeding with the creation process
                if (retry) Show(saveControllers);

                // Return early to avoid proceeding with the creation process
                return;
            }

            // Get the selected save controller based on the selected index
            selectedController = saveControllers[selectedIndex];

            // Save the new save data using the selected controller
            await selectedController.Save(saveName);

            // Close the window when the "Create" button is pressed
            Close();
        }
    }
}