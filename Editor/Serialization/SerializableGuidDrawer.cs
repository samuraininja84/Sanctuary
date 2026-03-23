using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Sanctuary.Editor
{
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidDrawer : PropertyDrawer
    {
        private static readonly string[] GuidParts = { "Part1", "Part2", "Part3", "Part4" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            // Begin the property GUI with the label
            EditorGUI.BeginProperty(position, label, property);

            // Draw the label for the GUID
            EditorGUI.PropertyField(position, property, new GUIContent(label.text + " (GUID)"), true);

            // Width for the button
            float buttonWidth = 20f;
            float gap = 3f;

            // Move position to the right of the label
            position.x += EditorGUIUtility.labelWidth;

            // Adjust width to account for the label with space for a button
            position.width -= EditorGUIUtility.labelWidth + buttonWidth + gap;

            // Ensure the property has the expected structure
            if (GetGuidParts(property).All(x => x != null)) 
            {
                // Get the GUID parts as a string
                string guidString = BuildGuidString(GetGuidParts(property));

                // Disable the GUI to prevent editing
                EditorGUI.BeginDisabledGroup(true);

                // Draw the GUID string in a selectable label
                EditorGUI.SelectableLabel(position, guidString, EditorStyles.textField);

                // Enable the GUI again
                EditorGUI.EndDisabledGroup();
            }
            else 
            {
                // If the GUID is not initialized, display a warning message
                EditorGUI.SelectableLabel(position, "GUID Not Initialized");
            }

            // Check for right-click
            bool hasClicked = Event.current.type == EventType.MouseUp  && Event.current.button == 1;

            // Check if the click was on the menu button
            if (hasClicked && position.Contains(Event.current.mousePosition)) 
            {
                // Show the context menu
                ShowContextMenu(property);

                // Consume the event
                Event.current.Use();
            }

            // Reset position for the next GUI element
            position.x -= EditorGUIUtility.labelWidth;
            position.width += EditorGUIUtility.labelWidth + buttonWidth + gap;

            // Add a button to show the context menu
            position.x += position.width - buttonWidth;
            position.width = buttonWidth;

            // Create an button style for the context menu
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.padding = new RectOffset(1, 1, 1, 1);
            buttonStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
            buttonStyle.fixedWidth = buttonWidth;

            // Create an context menu icon for the button
            GUIContent contextMenu = EditorGUIUtility.IconContent("d_Settings");

            // Draw the button for the context menu
            if (GUI.Button(position, contextMenu, buttonStyle)) ShowContextMenu(property);

            // End the property GUI
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Displays a context menu with options for managing the GUID associated with the specified serialized
        /// property.
        /// </summary>
        /// <remarks>The context menu provides options to copy, reset, or regenerate the GUID associated
        /// with the given property. Selecting an option will invoke the corresponding action.</remarks>
        /// <param name="property">The serialized property for which the context menu is displayed. This property must represent a valid GUID.</param>
        private void ShowContextMenu(SerializedProperty property)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy GUID"), false, () => CopyGuid(property));
            menu.AddItem(new GUIContent("Paste GUID"), false, () => PasteGuid(property));
            menu.AddItem(new GUIContent("Reset GUID"), false, () => ResetGuid(property));
            menu.AddItem(new GUIContent("Regenerate GUID"), false, () => RegenerateGuid(property));
            menu.AddItem(new GUIContent("GUID From Object"), false, () => GUIDFromObject(property));
            menu.ShowAsContext();
        }

        /// <summary>
        /// Copies the GUID associated with the specified serialized property to the system clipboard.
        /// </summary>
        /// <remarks>This method constructs a GUID string from the parts of the specified serialized
        /// property and copies it to the system clipboard. If any of the GUID parts are null, the method does not
        /// perform any operation. Additionally, the copied GUID is logged to the console for reference.</remarks>
        /// <param name="property">The serialized property containing the GUID parts to be copied. The property must contain valid GUID parts.</param>
        private void CopyGuid(SerializedProperty property)
        {
            // Ensure the GUID parts are not null before copying
            if (GetGuidParts(property).Any(x => x == null)) return;

            // Build the GUID string from the parts and copy it to the clipboard
            string guid = BuildGuidString(GetGuidParts(property));

            // Copy the GUID to the clipboard
            EditorGUIUtility.systemCopyBuffer = guid;

            // Log the copied GUID to the console
            Debug.Log($"GUID copied to clipboard: {guid}");
        }

        /// <summary>
        /// Pastes a GUID from the clipboard into the specified serialized property.
        /// </summary>
        /// <remarks>This method retrieves a GUID string from the system clipboard, validates its format,
        /// and splits it into parts to assign to the serialized property. The GUID must be a 32-character hexadecimal
        /// string (containing digits and uppercase letters A-F). If the clipboard does not contain a valid GUID, the
        /// method logs a warning and does not modify the property.  Ensure that the serialized property is properly
        /// structured to store GUID values before calling this method. The method applies changes to the serialized
        /// object after successfully pasting the GUID.</remarks>
        /// <param name="property">The serialized property representing the GUID. The property must consist of multiple parts that can store
        /// the GUID values.</param>
        private void PasteGuid(SerializedProperty property)
        {
            // Ensure the GUID parts are not null before pasting
            if (GetGuidParts(property).Any(x => x == null)) return;

            // Get the GUID string from the clipboard
            string guid = EditorGUIUtility.systemCopyBuffer;

            // Validate the GUID format
            if (guid.Length != 32 || !guid.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F')))
            {
                // Log as warning for the GUID format
                Debug.LogWarning("Invalid GUID format in clipboard. Please copy a valid GUID.");

                // Return early as this GUID cannot be used
                return;
            }

            // Split the GUID into parts and assign them to the serialized property
            SerializedProperty[] guidParts = GetGuidParts(property);

            // Iterate over all of the GUID parts
            for (int i = 0; i < GuidParts.Length; i++)
            {
                // Copy each section of the GUID onto each part
                guidParts[i].uintValue = Convert.ToUInt32(guid.Substring(i * 8, 8), 16);
            }

            // Apply the changes to the serialized object
            property.serializedObject.ApplyModifiedProperties();

            // Log the pasted GUID to the console
            Debug.Log($"GUID pasted from clipboard: {guid}");
        }

        /// <summary>
        /// Resets the GUID associated with the specified serialized property.
        /// </summary>
        /// <remarks>This method displays a confirmation dialog to the user before resetting the GUID. If
        /// the user confirms, all parts of the GUID are set to zero, and the changes are applied to the serialized
        /// object. A log message is written to the console upon successful reset.</remarks>
        /// <param name="property">The <see cref="SerializedProperty"/> representing the GUID to reset.  This property must contain valid GUID
        /// parts for the operation to succeed.</param>
        private void ResetGuid(SerializedProperty property)
        {
            // Create a confirmation dialog before resetting the GUID
            const string warning = "Are you sure you want to reset the GUID?";

            // Ask the user if they want to erase the GUID
            if (!EditorUtility.DisplayDialog("Reset GUID", warning, "Yes", "No")) return;

            // Reset each part of the GUID to zero
            foreach (var part in GetGuidParts(property)) part.uintValue = 0;

            // Apply the changes to the serialized object
            property.serializedObject.ApplyModifiedProperties();

            // Log the reset action to the console
            Debug.Log("GUID has been reset.");
        }

        /// <summary>
        /// Regenerates the GUID associated with the specified serialized property.
        /// </summary>
        /// <remarks>This method prompts the user with a confirmation dialog before regenerating the GUID.
        /// If the user confirms, a new GUID is generated, and its parts are assigned to the corresponding fields of the
        /// serialized property. Changes are applied to the serialized object, and a log message is written to the
        /// console indicating the successful regeneration.</remarks>
        /// <param name="property">The serialized property whose GUID will be regenerated. This property must represent a GUID split into
        /// parts.</param>
        private void RegenerateGuid(SerializedProperty property)
        {
            // Create a confirmation dialog before regenerating the GUID
            const string warning = "Are you sure you want to regenerate the GUID?";
            if (!EditorUtility.DisplayDialog("Reset GUID", warning, "Yes", "No")) return;

            // Generate a new GUID and update the serialized property parts
            byte[] bytes = Guid.NewGuid().ToByteArray();

            // Get the GUID parts
            SerializedProperty[] guidParts = GetGuidParts(property);

            // Iterate over all of the GUID parts
            for (int i = 0; i < GuidParts.Length; i++)
            {
                // Convert the bytes into UInts for the guid parts
                guidParts[i].uintValue = BitConverter.ToUInt32(bytes, i * 4);
            }

            // Apply the changes to the serialized object
            property.serializedObject.ApplyModifiedProperties();

            // Log the regeneration action to the console
            Debug.Log("GUID has been regenerated.");
        }

        /// <summary>
        /// Applies the ObjectGUID to the given SerializedProperty.
        /// </summary>
        /// <param name="property">The SerializedProperty to modify.</param>
        public static void GUIDFromObject(SerializedProperty property)
        {
            // Get the global object ID for the target object
            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(property.serializedObject.targetObject);

            // Check if the object is a prefab
            bool isPrefab = property.isInstantiatedPrefab;

            // Intialize the guid as empty
            string guid = globalId.targetObjectId.ToString();

            // If the object is not a prefab append the target prefab IDs to uniquely identify objects in scenes
            if (!isPrefab) guid += $"{globalId.targetPrefabId}";

            // Add a zero for spot up to 32 to buffer the rest of the GUID
            for (int i = guid.Length; i < 32; i++) guid += "0";

            // Validate the GUID format
            if (guid.Length != 32 || !guid.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F')))
            {
                // Log as warning for the GUID
                Debug.LogWarning("Invalid GUID format.");

                // Return early as this GUID cannot be used
                return;
            }

            // Get the GUID parts
            SerializedProperty[] guidParts = GetGuidParts(property);

            // Iterate over all of the GUID parts and copy each section of the GUID onto each part
            for (int i = 0; i < GuidParts.Length; i++) guidParts[i].uintValue = Convert.ToUInt32(guid.Substring(i * 8, 8), 16);

            // Apply the changes to the serialized object
            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Constructs a GUID string by combining the hexadecimal representations of the provided serialized property
        /// values.
        /// </summary>
        /// <param name="guidParts">An array of serialized properties representing the parts of the GUID. The array must contain exactly four
        /// elements, each corresponding to a 32-bit unsigned integer value.</param>
        /// <returns>A string representation of the GUID, formatted as a sequence of four 8-character hexadecimal segments.</returns>
        private static string BuildGuidString(SerializedProperty[] guidParts)
        {
            return new StringBuilder()
                .AppendFormat("{0:X8}", guidParts[0].uintValue)
                .AppendFormat("{0:X8}", guidParts[1].uintValue)
                .AppendFormat("{0:X8}", guidParts[2].uintValue)
                .AppendFormat("{0:X8}", guidParts[3].uintValue)
                .ToString();
        }

        /// <summary>
        /// Retrieves an array of serialized properties corresponding to the individual parts of a GUID.
        /// </summary>
        /// <remarks>This method assumes that the provided <paramref name="property"/> contains child
        /// properties named according to the predefined GUID parts. If any part is missing, the corresponding element
        /// in the returned array will be null.</remarks>
        /// <param name="property">The serialized property containing the GUID structure. This property must have child properties matching the
        /// expected GUID parts.</param>
        /// <returns>An array of <see cref="SerializedProperty"/> objects representing the individual parts of the GUID. The
        /// array will contain one element for each part of the GUID.</returns>
        private static SerializedProperty[] GetGuidParts(SerializedProperty property)
        {
            // Intialize am array to hold the GUID parts
            var values = new SerializedProperty[GuidParts.Length];

            // Iterate over the property to find the GUID parts
            for (int i = 0; i < GuidParts.Length; i++)
            {
                // Add the GUID parts to the array
                values[i] = property.FindPropertyRelative(GuidParts[i]);
            }

            // Return the GUID parts
            return values;
        }
    }
}
