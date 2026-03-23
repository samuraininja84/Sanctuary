using UnityEngine;
using UnityEditor;

namespace Sanctuary.Editor
{
    [CustomPropertyDrawer(typeof(ProfileData))]
    public class ProfileDataPropertyDrawer : PropertyDrawer
    {
        private int lineCount = 3;

        private float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height * lineCount;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start the change check
            EditorGUI.BeginChangeCheck();

            // Begin the property
            EditorGUI.BeginProperty(position, label, property);

            // Get the indent level
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Find the properties
            var scopeProperty = property.FindPropertyRelative("scope");
            var fileNameProperty = property.FindPropertyRelative("fileName");
            var idProperty = property.FindPropertyRelative("id");

            // Get the rects for the properties
            Rect scopeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect fileNameRect = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
            Rect idRect = new Rect(position.x, position.y + height * 2, position.width, EditorGUIUtility.singleLineHeight);

            // Disable editing on the scope and file name properties during play mode
            GUI.enabled = !Application.isPlaying;

            // Draw the scope property
            EditorGUI.PropertyField(scopeRect, scopeProperty, new GUIContent("Scope", "The scope of the profile data."));

            // Draw the file name property
            EditorGUI.PropertyField(fileNameRect, fileNameProperty, new GUIContent("File Name", "The name of the file for the profile data."));

            // Re-enable GUI
            GUI.enabled = true;

            // Draw the ID field based on the scope, which is only visible for `ProfileScope.Local`
            DrawIdField(scopeProperty, idProperty, idRect);

            // Reset indent level
            EditorGUI.indentLevel = indent;

            // End the change check
            if (EditorGUI.EndChangeCheck())
            {
                // Apply the modified properties
                property.serializedObject.ApplyModifiedProperties();
            }

            // End the property
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws the ID field in the editor UI based on the specified scope.
        /// </summary>
        /// <remarks>
        /// The ID field is only displayed when the scope is set to <see cref="ProfileScope.Global"/> or <see cref="ProfileScope.Scene"/>.
        /// For other scope values, the ID field is hidden, and the layout adjusts accordingly.
        /// </remarks>
        /// <param name="scopeProperty">The serialized property representing the scope. The value determines whether the ID field is displayed.</param>
        /// <param name="idProperty">The serialized property representing the ID. This is the unique identifier for the profile data.</param>
        /// <param name="idRect">The rectangular area in which the ID field is drawn.</param>
        private void DrawIdField(SerializedProperty scopeProperty, SerializedProperty idProperty, Rect idRect)
        {
            // If the scope is set to anything but `ProfileScope.Global` or `ProfileScope.Scene`, draw the ID field
            if (scopeProperty.enumValueIndex != (int)SaveScope.Global && scopeProperty.enumValueIndex != (int)SaveScope.Scene)
            {
                // Set the line count to 2 for non-global and non-scene scopes
                lineCount = 2;
            }
            else
            {
                // Set the line count to 3 for the global and scene scopes
                lineCount = 3;

                // Draw the ID property
                ProfileData.Id = EditorGUI.IntField(idRect, new GUIContent("ID", "The unique identifier for the profile data."), ProfileData.Id);

                // Add a help box for the ID field when it is less than 0
                if (ProfileData.Id < 0)
                {
                    // Increase line count for the help box
                    lineCount += 2;

                    // Clamp the ID to be at least -1
                    ProfileData.Id = Mathf.Max(ProfileData.Id, -1);

                    // Create a rect for the help box
                    Rect helpBoxRect = new Rect(idRect.x, idRect.y + height, idRect.width, height * 2);

                    // Create a help box below the ID field
                    EditorGUI.HelpBox(helpBoxRect, "If the ID is less than 0, it will save to the root of the local profile directory, instead of an indexed sub-directory.", MessageType.Warning);
                }
            }
        }
    }
}