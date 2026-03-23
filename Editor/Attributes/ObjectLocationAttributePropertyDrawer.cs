using UnityEditor;
using UnityEngine;
using Sanctuary.Attributes;

namespace Sanctuary.Editor
{
    /// <summary>
    /// Drawer for properties marked with the ObjectLocationAttribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ObjectLocationAttribute))]
    public class ObjectLocationAttributePropertyDrawer : PropertyDrawer
    {
        private float height = EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height * 2 + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Ensure the property is of the correct type
            if (attribute is not ObjectLocationAttribute objectLocation) return;

            // Ensure the property is of the correct type
            if (property.propertyType != SerializedPropertyType.Generic || property.type != nameof(SaveLocation))
            {
                // Display an error message if the property is not of type SaveLocation
                EditorGUI.HelpBox(position, $"{nameof(ObjectLocationAttribute)} can only be applied to fields of type {nameof(SaveLocation)}.", MessageType.Error);

                // Return early to avoid further processing
                return;
            }

            // Get the chunk ID property
            var chunkId = property.FindPropertyRelative(nameof(SaveLocation.ChunkId));

            // Get the object ID property
            var objectId = property.FindPropertyRelative(nameof(SaveLocation.ObjectId));

            // Apply the location logic based on the attribute settings
            ApplyLocation(objectLocation, property);

            // Start a disabled group to make the field read-only
            EditorGUI.BeginDisabledGroup(true);

            // Set the height for the position
            position.height = height;

            // Create a new label for the chunk ID field
            string chunkTooltip = objectLocation.IsPrefab ? "The location is shared among all instances of the prefab." : "The location is the GUID of the scene that this object belongs to.";
            GUIContent chunkID = new GUIContent($"{label.text}: Chunk ID", $"The ID of the chunk. {chunkTooltip}");

            // Create a new label for the object ID field
            string objectIdTooltip = objectLocation.IsPrefab ? "The location is shared among all instances of the prefab." : "The location is unique to this object.";
            GUIContent objectID = new GUIContent($"{label.text}: Object ID", $"The ID of the object. {objectIdTooltip}");

            // Get the rect for the chunk ID and object ID fields
            Rect chunkRect = new Rect(position.x, position.y, position.width, height);
            Rect objectRect = new Rect(position.x, position.y + height + EditorGUIUtility.standardVerticalSpacing, position.width, height);

            // Draw the chunk ID and object ID fields
            EditorGUI.PropertyField(chunkRect, chunkId, chunkID);
            EditorGUI.PropertyField(objectRect, objectId, objectID);

            // End the disabled group
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Applies the location logic to the given SerializedProperty based on the ObjectLocationAttribute settings.
        /// </summary>
        /// <param name="attribute">The ObjectLocationAttribute instance containing the settings.</param>
        /// <param name="property">The SerializedProperty to modify.</param>
        public static void ApplyLocation(ObjectLocationAttribute attribute, SerializedProperty property)
        {
            // Check if the property has already been initialized
            var initialized = property.FindPropertyRelative(nameof(SaveLocation.initialized));

            // If the game is running, do not modify the property
            if (Application.isPlaying || initialized.boolValue) return;

            // Get the global object ID for the target object
            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(property.serializedObject.targetObject);

            // Get the chunk ID property
            var chunkId = property.FindPropertyRelative(nameof(SaveLocation.ChunkId));

            // Get the object ID property
            var objectId = property.FindPropertyRelative(nameof(SaveLocation.ObjectId));

            // If the object is part of a prefab instance, adjust the IDs accordingly, otherwise use the default behavior
            if (attribute.IsPrefab)
            {
                // Use a constant chunk ID to group all prefab instances together
                chunkId.stringValue = "Prefabs";

                // Use only the object ID to identify prefab instances
                objectId.stringValue = globalId.targetObjectId.ToString();
            }
            else
            {
                // Use the asset GUID as the chunk ID to group objects by their source asset
                chunkId.stringValue = globalId.assetGUID.ToString();

                // Use both the object and prefab IDs to uniquely identify objects in scenes
                objectId.stringValue = $"{globalId.targetObjectId}-{globalId.targetPrefabId}";
            }

            // Mark the property as initialized
            initialized.boolValue = true;
        }
    }
}
