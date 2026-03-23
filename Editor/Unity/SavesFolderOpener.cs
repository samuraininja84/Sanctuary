using System.IO;
using UnityEngine;
using UnityEditor;

namespace Sanctuary.Editor
{
    public static class SavesFolderOpener
    {
        private const string MenuPath = "Tools/Sanctuary/";
        private const string ShortcutKey = "#_s";

        [MenuItem(MenuPath + "Open Saves Folder " + ShortcutKey)]
        public static void OpenSavesFolder()
        {
            // Get the path to the saves folder
            string folderPath = Path.Combine(Application.persistentDataPath, "Save Data");

            // Open the saves folder if it exists, otherwise log a warning
            if (Directory.Exists(folderPath))
            {
                // Open the folder in the file explorer
                System.Diagnostics.Process.Start(folderPath);
            }
            else
            {
                // Log a warning if the folder doesn't exist
                Debug.LogWarning($"The saves folder doesn't exist yet. Creating the folder now at: {folderPath} and opening it.");

                // Create the saves folder
                Directory.CreateDirectory(folderPath);

                // Open the newly created folder
                System.Diagnostics.Process.Start(folderPath);
            }
        }

        [MenuItem(MenuPath + "Open Saves Folder " + ShortcutKey, true)]
        public static bool CanOpenSavesFolder() => !Application.isPlaying;
    }
}
