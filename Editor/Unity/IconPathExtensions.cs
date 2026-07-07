using UnityEditor;
using UnityEngine;

namespace Sanctuary.Editor
{
    /// <summary>
    /// Extension methods for constructing file paths to icons used in the Sanctuary editor.
    /// </summary>
    public static class IconPathExtensions
    {
        /// <summary>
        /// The path to the icons used in the Sanctuary editor.
        /// </summary>
        public const string IconsPath = "Assets/Plugins/Sanctuary/Editor/EditorResources/";

        /// <summary>
        /// Generates the full file path for an image by combining the specified icon name, file extension, and directory path.
        /// </summary>
        /// <remarks>
        /// This method is an extension method for the string type, allowing convenient construction of image file paths for icons. 
        /// The resulting path does not include validation for file existence.
        /// </remarks>
        /// <param name="iconName">The name of the icon file, without extension. Cannot be null or empty.</param>
        /// <param name="extension">The file extension to use for the image. Defaults to "png" if not specified.</param>
        /// <param name="path">The directory path where the image is located. Defaults to the value of IconsPath if not specified.</param>
        /// <returns>A string containing the full file path to the image, constructed from the provided icon name, extension, and path.</returns>
        public static string ToImagePath(this string iconName, string extension = "png", string path = IconsPath) => $"{path}{iconName}.{extension}";

        /// <summary>
        /// Creates a GUIContent object for the specified icon name, using the provided file extension and directory path to locate the image.
        /// </summary>
        /// <param name="iconName">The name of the icon file, without extension. Cannot be null or empty.</param>
        /// <param name="extension">The file extension to use for the image. Defaults to "png" if not specified.</param>
        /// <param name="path">The directory path where the image is located. Defaults to the value of IconsPath if not specified.</param>
        /// <returns>A GUIContent object containing the specified icon image.</returns>
        public static GUIContent ToGUIContent(this string iconName, string extension = "png", string path = IconsPath) => new(EditorGUIUtility.FindTexture(iconName.ToImagePath(extension, path)));
    }
}