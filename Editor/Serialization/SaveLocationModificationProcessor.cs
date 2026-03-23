using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Sanctuary.Attributes;

using Object = UnityEngine.Object;

namespace Sanctuary.Editor 
{
    /// <summary>
    /// This class processes asset modifications to apply location attributes before saving.
    /// </summary>
    public class SaveLocationModificationProcessor : AssetModificationProcessor 
    {
        /// <summary>
        /// Static constructor to register the OnWillSaveAssets callback.
        /// </summary>
        /// <param name="paths">The paths of the assets being saved.</param>
        /// <returns>Array of paths to continue the save operation.</returns>
        private static string[] OnWillSaveAssets(string[] paths) 
        {
            // Process each path being saved
            foreach (var path in paths) 
            {
                // Determine action based on file extension
                switch (Path.GetExtension(path)) 
                {
                    // Switch based on the file extension
                    case ".prefab":
                        // Get the current prefab stage
                        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                        // Process the prefab stage if a prefab is being saved
                        if (prefabStage != null && prefabStage.assetPath == path) ProcessStage(prefabStage);
                        break;
                    case ".unity":
                        // Process the main stage if a scene is being saved
                        ProcessStage(StageUtility.GetMainStage());
                        break;
                    case ".asset":
                        // Load all assets at the given path
                        var assets = AssetDatabase.LoadAllAssetsAtPath(path);

                        // Process each asset iof the .asset file extension
                        foreach (var asset in assets) 
                        {
                            // If the asset is not null, process it
                            if (asset != null) ProcessObject(asset);
                        }
                        break;
                }
            }

            // Return the original paths to continue the save operation
            return paths;
        }

        /// <summary>
        /// Process all MonoBehaviour components in the given stage.
        /// </summary>
        /// <param name="stage">The stage to process.</param>
        private static void ProcessStage(Stage stage) 
        {
            // Find all MonoBehaviour components in the stage
            var components = stage.FindComponentsOfType<MonoBehaviour>();

            // Process all components in the stage
            foreach (var component in components) ProcessObject(component);
        }

        /// <summary>
        /// Process the given object, applying any location attributes found on its fields.
        /// </summary>
        /// <param name="component">The object to process.</param>
        private static void ProcessObject(Object component) 
        {
            // Get all non-public instance fields of the component
            var fields = component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            // Process each field
            foreach (var fieldInfo in fields) 
            {
                // Skip static, readonly, and const fields
                if (fieldInfo.IsStatic || fieldInfo.IsInitOnly || fieldInfo.IsLiteral) continue;

                // Check for ObjectLocationAttribute and apply location if found
                if (Attribute.GetCustomAttribute(fieldInfo, typeof(ObjectLocationAttribute), false) is ObjectLocationAttribute objectLocation) 
                {
                    // Create a SerializedObject for the component
                    var serializedObject = new SerializedObject(component);

                    // Apply the location using the property drawer
                    ObjectLocationAttributePropertyDrawer.ApplyLocation(objectLocation, serializedObject.FindProperty(fieldInfo.Name));

                    // Apply the modified properties without undo
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                // Check for AssetLocationAttribute and apply location if found
                if (Attribute.GetCustomAttribute(fieldInfo, typeof(AssetLocationAttribute), false) is AssetLocationAttribute assetLocation)
                {
                    // Create a SerializedObject for the component
                    var serializedObject = new SerializedObject(component);

                    // Apply the location using the property drawer
                    AssetLocationAttributePropertyDrawer.ApplyLocation(assetLocation, serializedObject.FindProperty(fieldInfo.Name));

                    // Apply the modified properties without undo
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }
}
