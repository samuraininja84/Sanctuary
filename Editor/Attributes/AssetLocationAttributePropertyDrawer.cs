using UnityEngine;
using UnityEditor;
using Sanctuary.Attributes;

namespace Sanctuary.Editor 
{
    /// <summary>
    /// Drawer for properties marked with the AssetLocationAttribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(AssetLocationAttribute))]
    public class AssetLocationAttributePropertyDrawer : PropertyDrawer
    {
        private float height = EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height * 2 + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Ensure the attribute is of the correct type
            if (attribute is not AssetLocationAttribute assetLocation) return;

            // Ensure the property is of the correct type
            if (property.propertyType != SerializedPropertyType.Generic || property.type != nameof(SaveLocation))
            {
                // Display an error message if the property is not of type SaveLocation
                EditorGUI.HelpBox(position, $"{nameof(AssetLocationAttribute)} can only be applied to fields of type {nameof(SaveLocation)}.", MessageType.Error);

                // Return early to avoid further processing
                return;
            }

            // Get the chunk ID property
            var chunkId = property.FindPropertyRelative(nameof(SaveLocation.ChunkId));

            // Get the object ID property
            var objectId = property.FindPropertyRelative(nameof(SaveLocation.ObjectId));

            // Apply the location based on the attribute
            ApplyLocation(assetLocation, property);

            // Begin a disabled group to make the fields read-only
            EditorGUI.BeginDisabledGroup(true);

            // Set the height for the position
            position.height = height;

            // Create a new label for the chunk ID field
            string chunkTooltip = "The location is shared among all instances of the asset.";
            GUIContent chunkID = new GUIContent($"{label.text}: Chunk ID", $"The ID of the chunk. {chunkTooltip}");

            // Create a new label for the object ID field
            string objectIdTooltip = "The location is shared among all instances of the asset.";
            GUIContent objectID = new GUIContent($"{label.text}: Asset ID", $"The ID of the Asset. {objectIdTooltip}");

            // Get the rect for the chunk ID and object ID fields
            Rect chunkRect = new Rect(position.x, position.y, position.width, height);
            Rect objectRect = new Rect(position.x, position.y + height + EditorGUIUtility.standardVerticalSpacing, position.width, height);

            // Draw the chunk ID and object ID fields
            EditorGUI.PropertyField(chunkRect, chunkId, chunkID);
            EditorGUI.PropertyField(objectRect, objectId, objectID);

            // Disable editing of the ChunkId and ObjectId fields
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Applies the location information to the SerializedProperty based on the AssetLocationAttribute.
        /// </summary>
        /// <param name="attribute">The AssetLocationAttribute containing location info.</param>
        /// <param name="property">The SerializedProperty to modify.</param>
        public static void ApplyLocation(AssetLocationAttribute attribute, SerializedProperty property)
        {
            // Get the GlobalObjectId of the target object
            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(property.serializedObject.targetObject);

            // Get the ChunkId property
            var chunkId = property.FindPropertyRelative(nameof(SaveLocation.ChunkId));

            // Get the ObjectId property
            var objectId = property.FindPropertyRelative(nameof(SaveLocation.ObjectId));

            // Set the ChunkId from the attribute
            chunkId.stringValue = attribute.ChunkId;

            // Set the ObjectId based on whether it's a prefab or not
            objectId.stringValue = globalId.assetGUID.ToString();
        }
    }
}
