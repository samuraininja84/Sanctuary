using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sanctuary.Stores;
using Sanctuary.Loaders;

namespace Sanctuary.Editor
{
    /// <summary>
    /// The Sanctuary Editor Window allows you to inspect and manage save data
    /// </summary>
    /// <remarks>
    /// Shows a window where you can inspect and manage save data.
    /// </remarks>
    public class SanctuaryEditor : EditorWindow
    {
        // Cached saves
        private static SaveControllerBase[] saves = Array.Empty<SaveControllerBase>();
        private static SaveControllerBase currentSave;

        // Current profile selection
        private SaveMode saveMode = SaveMode.Full;

        // Data caches
        private static readonly Dictionary<string, string> _formattedData = new();
        private static readonly Dictionary<string, string> _chunkNames = new();

        // Toolbar event
        private static event Action OnDrawToolbar = delegate { };

        // Search string for filtering locations
        private string searchString = string.Empty;

        // GUI styles
        private GUIStyle _sectionStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _listItemStyle;
        private GUIStyle _toolButtonStyle;
        private GUIStyle _saveSlotStyle;
        private GUIStyle _miniButtonStyle;

        // Save slot data
        private Vector2 _slotsScrollPos = Vector2.zero;
        private Dictionary<int, string> existingSaves = new();
        private bool showingSaveSlotOptions;
        private int minimumSaveSlots = 2;
        private int selectedSaveSlot = 0;

        // Current selections
        private int _currentIndex;
        private string _currentChunk;
        private string _currentLocation;

        // Scroll positions for the three main sections
        private Vector2 _chunkScrollPos;
        private Vector2 _locationScrollPos;
        private Vector2 _dataScrollPos;

        // Section resizing
        private Vector2 minSectionSplit = new Vector2(150f, 235f);
        private Vector2 sectionSplit = new Vector2(300f, 400f);
        private Rect dataSectionRect;
        private bool resizingSection;

        // Chunk / Location resizing
        private float minLocationSplit = 75f;
        private float locationSplit = 150f;
        private bool resizingLocation;

        // Data area resizing
        private Vector2 minDataSplit = new Vector2(150f, 150f);
        private Vector2 dataSplit = new Vector2(300f, 300f);
        private bool resizingData;

        // Convert the size to a human-readable format
        private static string[] sizeUnits = { "B", "KB", "MB", "GB", "TB" };

        private int HighestSaveId => GetHighestSaveId();
        private bool HorizontalLayout => Screen.width > Screen.height;
        private bool HasSaves => SaveControllerBase.ExistingSaves.Count > 0;
        private bool ShowLocation => SanctuaryEditorProcessor.showLocationWhenNamed;
        private bool FilterFiles => SanctuaryEditorProcessor.filterFiles;
        private string ExistingSavesPath => Path.Combine(Application.persistentDataPath, FileSaveLoader.DefaultFolderName);

        [MenuItem("Window/Sanctuary/Editor")]
        private static void Open()
        {
            // Get or create the Sanctuary Editor window
            SanctuaryEditor window = GetWindow<SanctuaryEditor>("Sanctuary");

            // Find the icon texture
            Texture icon = EditorGUIUtility.FindTexture("Assets/Plugins/Sanctuary/Editor/EditorResources/SanctuaryWindow.png");

            // Set the window icon
            window.titleContent = new GUIContent("Sanctuary", icon);
        }

        private void OnFocus()
        {
            // Get existing save IDs
            RefreshExistingSaveIDs();

            // Clear styles when the window gains focus
            ClearStyles();

            // Repaint the window when it gains focus
            Repaint();
        }

        private void OnEnable()
        {
            // Get all methods with the InspectorHeaderToolbarButtonAttribute
            var methods = TypeCache.GetMethodsWithAttribute<SanctuaryToolbarButtonAttribute>();

            // Iterate through the methods and create toolbar buttons
            foreach (var method in methods)
            {
                // Skip non-static methods
                if (!method.IsStatic) continue;

                // Create a delegate for the method
                var action = (Action)Delegate.CreateDelegate(typeof(Action), method);

                // Add the action to the onDrawToolbar event
                OnDrawToolbar += action;
            }

            // Repaint the window when it gains focus
            Repaint();
        }

        private void OnDisable()
        {
            // Unsubscribe all toolbar button methods
            OnDrawToolbar = delegate { };

            // Clear the data caches
            ClearCache();
        }

        private void OnGUI()
        {
            // Ensure styles are initialized
            GetStyles();

            // Get the current mouse position
            Vector2 globalMousePosition = Event.current.mousePosition;

            // Fetch existing saves
            saves = SaveControllerBase.ExistingSaves.Select(wr => wr.TryGetTarget(out var save) ? save : null).Where(save => save != null).ToArray();

            // Start checking for changes in the GUI
            EditorGUI.BeginChangeCheck();

            // Additional spacing for the scroll rect
            float scrollRectSpacing = 5f;

            // Get the window rect
            Rect windowRect = position;

            #region Actions Menu Area

            // If using horizontal layout, draw the actions section on the left side
            if (HorizontalLayout)
            {
                // Get the rect for the right side of the window
                Rect actionMenuRect = new Rect(-3, -3, sectionSplit.x + scrollRectSpacing, position.height + 6);

                // Draw a rect for the actions section
                GUILayout.BeginArea(actionMenuRect, EditorStyles.objectFieldThumb);
            }
            else
            {
                // Get the rect for the top of the window
                Rect actionMenuRect = new Rect(-3, -3, position.width + scrollRectSpacing, sectionSplit.y);

                // Draw a rect for the actions section
                GUILayout.BeginArea(actionMenuRect, EditorStyles.objectFieldThumb);
            }

            // Add some space before the action buttons
            EditorGUILayout.Space(1f);

            // Draw the save slot selection
            DrawSaveSlots();

            // Push actions to the bottom, if using horizontal layout
            GUILayout.FlexibleSpace();

            // Draw the save slot actions if there are any existing saves
            DrawSaveSlotActions();

            // Draw the dynamic toolbar
            DrawDynamicToolbar();

            // End the area for the actions section
            GUILayout.EndArea();

            #endregion

            #region Area Seperator Resizing

            // If using horizontal layout, create a resizable splitter between the two sections
            if (HorizontalLayout)
            {
                // Add a draggable splitter
                Rect splitterRect = new Rect(sectionSplit.x - 2f, 0, 7.5f, windowRect.height);

                // Draw the splitter rect
                EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

                // Start resizing on mouse down
                if (EventInputs.MouseLeft(EventType.MouseDown) && splitterRect.Contains(globalMousePosition)) resizingSection = true;

                // Calculate the maximum split position
                float maxSplitX = windowRect.width - minSectionSplit.x - minDataSplit.x;

                // Clamp the split position
                sectionSplit.x = Mathf.Clamp(sectionSplit.x, minSectionSplit.x, maxSplitX);

                // Handle resizing
                if (resizingSection)
                {
                    // Handle mouse drag events
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        // Update the split position based on the mouse position
                        sectionSplit.x = Mathf.Clamp(globalMousePosition.x, minSectionSplit.x, maxSplitX);

                        // Repaint the window to reflect the changes
                        Repaint();
                    }

                    // Stop resizing on mouse up
                    if (Event.current.type == EventType.MouseUp) resizingSection = false;
                }
            }
            else
            {
                // Add a draggable splitter
                Rect splitterRect = new Rect(0, sectionSplit.y - 7.5f, windowRect.width, 7.5f);

                // Draw the splitter rect
                EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);

                // Start resizing on mouse down
                if (EventInputs.MouseLeft(EventType.MouseDown) && splitterRect.Contains(globalMousePosition)) resizingSection = true;

                // Calculate the maximum split position
                float maxSplitY = windowRect.height - minSectionSplit.y - minDataSplit.y;

                // Clamp the split position
                sectionSplit.y = Mathf.Clamp(sectionSplit.y, minSectionSplit.y, maxSplitY);

                // Handle resizing
                if (resizingSection)
                {
                    // Handle mouse drag events
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        // Update the split position based on the mouse position
                        sectionSplit.y = Mathf.Clamp(globalMousePosition.y, minSectionSplit.y, maxSplitY);

                        // Repaint the window to reflect the changes
                        Repaint();
                    }

                    // Stop resizing on mouse up
                    if (Event.current.type == EventType.MouseUp) resizingSection = false;
                }
            }

            #endregion

            #region Save Data Area

            // Begin the area for the right section, if using horizontal layout
            if (HorizontalLayout)
            {
                // Calculate the starting position for the right section
                float startPos = Mathf.Max(sectionSplit.x, minSectionSplit.x) + scrollRectSpacing;

                // Define the rect for the right section
                dataSectionRect = new Rect(startPos + 1, 3, (position.width - startPos - 1), position.height - 3);

                // Begin the area for the right section
                GUILayout.BeginArea(dataSectionRect);
            }
            else
            {
                // Calculate the starting position for the bottom section
                float startPos = Mathf.Max(sectionSplit.y, minSectionSplit.y);

                // Define the rect for the bottom section
                dataSectionRect = new Rect(3, startPos, position.width - 6, position.height - startPos - 5);

                // Begin the area for the bottom section
                GUILayout.BeginArea(dataSectionRect);
            }

            // Draw the information header for the save data section
            DrawInformationHeader();

            // Handle the empty state when no saves are found
            if (saves.Length == 0)
            {
                // Define a style for the empty state message
                GUIStyle emptyStateStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    stretchHeight = true
                };

                // Display a message when no saves are found
                GUILayout.Box("No existing save controllers found.\nOnce created, they'll appear in this window.", emptyStateStyle);

                // End the area for the right section, if using horizontal layout
                GUILayout.EndArea();

                // Return early if no saves are found
                return;
            }

            // Get the currently selected save
            currentSave = saves[Mathf.Clamp(_currentIndex, 0, saves.Length - 1)];

            // Reset the current chunk and location if the selected save changes
            if (EditorGUI.EndChangeCheck())
            {
                // Clear cached formatted data when switching saves
                _currentChunk = null;

                // Reset the current location when the chunk changes
                _currentLocation = null;
            }

            // Get the composite save data
            ISaveData composite = ISaveDataExtensions.Combine(new SaveData(), saves.Select(s => s.Data));

            // Determine the save data to display based on filtering
            ISaveData data = FilterFiles ? currentSave.Data : composite;

            // Check if the current save data is null
            if (data == null)
            {
                // Show the help box indicating no save data found
                EditorGUILayout.HelpBox("No save data found for the selected controller. Please create a snapshot or load existing data.", MessageType.Warning);
            }
            else
            {
                // Show the main layout GUI for the current save data
                LayoutGUI(data);
            }

            // End the area for the right section, if using horizontal layout
            GUILayout.EndArea();

            #endregion
        }

        #region Save Slot Methods

        private void DrawSaveSlots()
        {
            // Start a scroll view for the save slots if there are more than the minimum save slots
            _slotsScrollPos = GUILayout.BeginScrollView(_slotsScrollPos);

            // Draw a save slot button for the current save slot
            GUILayout.Box("Save Slots", _sectionStyle);

            // Disable GUI if there are no existing saves
            GUI.enabled = HasSaves;

            // Draw the 'Absolute' save slot if it exists, because it uses the id of -1
            if (ValidSave(-1)) DrawActiveSaveSlot(-1);
            else DrawEmptySaveSlot(-1);

            // Draw the save slots
            for (int i = 0; i < HighestSaveId; i++)
            {
                // If the save is valid, draw the active save slot, otherwise draw the empty save slot
                if (ValidSave(i)) DrawActiveSaveSlot(i);
                else DrawEmptySaveSlot(i);
            }

            // End the scroll view if there are more than the minimum save slots
            GUILayout.EndScrollView();

            // Re-enable GUI
            GUI.enabled = true;
        }

        private void DrawActiveSaveSlot(int index)
        {
            // Store the slot name
            string slotName = index == -1 ? "Absolute Save Slot" : $"Save Slot {index}";

            // Get the save slot information
            string[] info = existingSaves[index].Split('-');

            // Store the started at and last modified
            string startedAt = info[0].Trim();
            string lastModified = info[1].Trim();
            string fileSize = info[2].Trim();

            // Combine the info into a string
            string combinedInfo = $"{slotName}\n{startedAt}\n{lastModified}\n{fileSize}";

            // Draw the button
            if (GUILayout.Button(combinedInfo, _saveSlotStyle))
            {
                // Toggle the save slot options
                if (selectedSaveSlot == index)
                {
                    // Toggle the save slot options if selecting the same slot
                    showingSaveSlotOptions = !showingSaveSlotOptions;
                }
                else
                {
                    // Set the current index to this index
                    selectedSaveSlot = index;

                    // Set the profile ID
                    SetProfileID(index);

                    // Show the save slot options when selecting a new slot
                    showingSaveSlotOptions = true;
                }
            }

            // If this save slot is selected, draw the save slot options
            if (selectedSaveSlot == index) DrawSaveSlotOptions(index);
        }

        private void DrawEmptySaveSlot(int index)
        {
            // Create a copy of the save slot style to modify the height
            GUIStyle buttonStyle = new GUIStyle(_saveSlotStyle) 
            { 
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = (EditorGUIUtility.singleLineHeight + 3) * 3
            };

            // Change the color of the button to a darker gray
            GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);

            // Change the color of the text to yellow
            GUI.contentColor = Color.yellowNice;

            // Create New Game Content
            GUIContent newGameContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
            newGameContent.tooltip = $"Create a New Save with Default Settings in Slot {index}";

            // Draw the button for creating a new save in this slot
            if (GUILayout.Button(newGameContent, buttonStyle))
            {
                // Set the selected save slot to this index
                selectedSaveSlot = index;

                // Set the profile ID
                SetProfileID(index);

                // Create a save in this slot
                if (index >= 0) SaveIndexed();
                else SaveAbsolute();

                // Refresh the save IDs
                RefreshExistingSaveIDs();
                RefreshExistingSaveIDs();

                // Repaint the window
                Repaint();
            }

            // Reset the color of the button to the default color
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
        }

        private void DrawSaveSlotOptions(int index)
        {
            // If has no save hide the save slot options
            if (!HasSaves) showingSaveSlotOptions = false;

            // If not showing the save slot options, return
            if (!showingSaveSlotOptions) return;

            // Create Content for the overwrite button
            GUIContent overwriteContent = EditorGUIUtility.IconContent("d_SaveAs");
            overwriteContent.tooltip = "Overwrite this Save File";

            // Create Content for the load button
            GUIContent loadContent = EditorGUIUtility.IconContent("Import");
            loadContent.tooltip = "Load this Save File";

            // Create Content for the delete button
            GUIContent deleteContent = EditorGUIUtility.IconContent("d_Grid.EraserTool");
            deleteContent.tooltip = "Delete this Save File";

            // Begin a horizontal layout 
            EditorGUILayout.BeginHorizontal();

            // Draw the save mode selection
            saveMode = (SaveMode)EditorGUILayout.EnumPopup(saveMode);

            // Draw a mini button to overwrite this save
            if (GUILayout.Button(overwriteContent, _miniButtonStyle))
            {
                // Set the current index to this index
                selectedSaveSlot = index;

                // Set the profile ID
                SetProfileID(index);

                // Save the data
                if (index >= 0) SaveIndexed();
                else SaveAbsolute();

                // Update the existing save IDs
                RefreshExistingSaveIDs();
                RefreshExistingSaveIDs();
            }

            // Draw a mini button to load this save
            if (GUILayout.Button(loadContent, _miniButtonStyle))
            {
                // Set the current index to this index
                selectedSaveSlot = index;

                // Set the profile ID
                SetProfileID(index);

                // Load the save
                if (index >= 0) LoadIndexed();
                else LoadAbsolute();
            }

            // Draw a mini button to delete this save
            if (GUILayout.Button(deleteContent, _miniButtonStyle))
            {
                // Set the current index to this index
                selectedSaveSlot = index;

                // Set the profile ID
                SetProfileID(index);

                // Delete the save
                if (index >= 0) DeleteIndexed();
                else DeleteAbsolute();

                // Update the existing save IDs
                RefreshExistingSaveIDs();
            }

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSaveSlotActions()
        {
            // Disable GUI if there are no existing saves
            GUI.enabled = HasSaves;

            // Check if there are any existing saves with an id higher than the highest save id
            if (HasMinimumSaveSlots())
            {
                // Draw a horizontal line to separate the save slots from the buttons
                HorizontalLine();

                // Change the color of the button to a darker gray
                GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);

                // Change the color of the text to yellow
                GUI.contentColor = Color.yellowNice;

                // Begin a horizontal layout for the buttons
                EditorGUILayout.BeginHorizontal();

                // Create Delete Last Save Content
                GUIContent deleteLastSaveContent = EditorGUIUtility.IconContent("d_Toolbar Minus");
                deleteLastSaveContent.tooltip = "Delete the Last Save File";

                // Create New Game Content
                GUIContent newGameContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
                newGameContent.tooltip = $"Create a New Save with Default Settings in Slot {HighestSaveId}";

                // Draw the button for creating a new save in this slot
                if (GUILayout.Button(newGameContent, _miniButtonStyle))
                {
                    // Set the selected save slot to this index
                    selectedSaveSlot = HighestSaveId;

                    // Set the profile ID
                    SetProfileID(selectedSaveSlot);

                    // Create a save in this slot
                    if (selectedSaveSlot >= 0) SaveIndexed();
                    else SaveAbsolute();

                    // Refresh the save IDs
                    RefreshExistingSaveIDs();
                    RefreshExistingSaveIDs();

                    // Repaint the window
                    Repaint();

                    // Scroll to the bottom of the save slots
                    _slotsScrollPos.y = float.MaxValue;
                }

                // Draw a button to delete the last save
                if (GUILayout.Button(deleteLastSaveContent, _miniButtonStyle))
                {
                    // Set the save data id to the highest save id - 1 and delete the game
                    selectedSaveSlot = HighestSaveId - 1;

                    // Set the profile ID
                    SetProfileID(selectedSaveSlot);

                    // Delete the save
                    if (selectedSaveSlot >= 0) DeleteIndexed();
                    else DeleteAbsolute();

                    // Refresh the save IDs
                    RefreshExistingSaveIDs();

                    // Repaint the window
                    Repaint();
                }

                // Create Delete All Saves Content
                GUIContent deleteAllSavesContent = EditorGUIUtility.IconContent("d_Grid.EraserTool");
                deleteAllSavesContent.tooltip = "Delete All Save Files";

                // Draw a button to delete all saves
                if (GUILayout.Button(deleteAllSavesContent, _miniButtonStyle))
                {
                    // Confirm deletion
                    if (EditorUtility.DisplayDialog("Delete All Saves", "Are you sure you want to delete all save files? This action cannot be undone.", "Yes", "No"))
                    {
                        // Delete all saves
                        DeleteAll();

                        // Refresh the save IDs
                        RefreshExistingSaveIDs();
                    }
                }

                // End the horizontal layout for the buttons
                EditorGUILayout.EndHorizontal();

                // Reset the colors to the default
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
            }

            // Re-enable GUI
            GUI.enabled = true;
        }

        #endregion

        #region ID Methods

        private void SetProfileID(int currentId)
        {
            // Clamp the current id to 0 minimum
            currentId = Mathf.Max(-1, currentId);

            // Update the existing save IDs for the current save
            foreach (var save in saves)
            {
                // Update the static profile id
                ProfileData.Id = currentId;

                // Update the existing save IDs
                save.SetID(ProfileData.Id);
            }
        }

        private void RefreshExistingSaveIDs()
        {
            // Clear the existing save IDs list
            existingSaves.Clear();

            // Check if the saves directory exists
            if (!Directory.Exists(ExistingSavesPath)) return;

            // Get all folders in the saves directory
            DirectoryInfo savesDirectory = new DirectoryInfo(ExistingSavesPath);

            // Loop through all folders in the saves directory
            foreach (var dir in savesDirectory.GetDirectories())
            {
                // Try to parse the folder name as an integer
                if (int.TryParse(dir.Name, out int id))
                {
                    // Get the save file data
                    string startedAt = GetSaveSlotStartedAt(id.ToString());
                    string lastModified = GetSaveSlotLastModified(id.ToString());
                    string fileSize = GetSaveSlotFolderSize(id.ToString());
                    string combinedData = startedAt + " - "  +  lastModified + " - " + fileSize;

                    // Add the parsed id to the existing save IDs list
                    existingSaves.Add(id, combinedData);
                }
                else
                {
                    // This is likely to be the 'Absolute' save folder, so add it with an id of -1
                    string startedAt = GetSaveSlotStartedAt(dir.Name);
                    string lastModified = GetSaveSlotLastModified(dir.Name);
                    string fileSize = GetSaveSlotFolderSize(dir.Name);
                    string combinedData = startedAt + " - "  +  lastModified + " - " + fileSize;

                    // Add the parsed id to the existing save IDs list
                    existingSaves.Add(-1, combinedData);
                }
            }
        }

        private string GetSaveSlotStartedAt(string subDirectory)
        {
            // Get the path to the save slot directory
            string saveSlotPath = Path.Combine(ExistingSavesPath, subDirectory);

            // If the directory doesn't exist, return "Started At: N/A"
            if (!Directory.Exists(saveSlotPath)) return "Started At: N/A";

            // Create a DirectoryInfo object for the save slot directory
            DirectoryInfo dirInfo = new DirectoryInfo(saveSlotPath);

            // Get all files in the directory and its subdirectories
            FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);

            // If there are no files, return DateTime.MinValue
            if (files.Length == 0) return "Started At: N/A";

            // Find the earliest creation time among all files
            DateTime startedAt = files.Min(file => file.CreationTime);

            // Return the earliest creation time
            return "Started At: " + startedAt.ToString("g");
        }

        private string GetSaveSlotLastModified(string subDirectory)
        {
            // Get the path to the save slot directory
            string saveSlotPath = Path.Combine(ExistingSavesPath, subDirectory);

            // If the directory doesn't exist, return "Last Modified: N/A"
            if (!Directory.Exists(saveSlotPath)) return "Last Modified: N/A";

            // Create a DirectoryInfo object for the save slot directory
            DirectoryInfo dirInfo = new DirectoryInfo(saveSlotPath);

            // Get all files in the directory and its subdirectories
            FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);

            // If there are no files, return DateTime.MinValue
            if (files.Length == 0) return "Last Modified: N/A";

            // Find the most recent last write time among all files
            DateTime lastModified = files.Max(file => file.LastWriteTime);

            // Return the most recent last write time
            return "Last Modified: " + lastModified.ToString("g");
        }

        private string GetSaveSlotFolderSize(string subDirectory)
        {
            // Get the path to the save slot directory
            string saveSlotPath = Path.Combine(ExistingSavesPath, subDirectory);

            // If the directory doesn't exist, return "N/A"
            if (!Directory.Exists(saveSlotPath)) return "File Size: N/A";

            // Create a DirectoryInfo object for the save slot directory
            DirectoryInfo dirInfo = new DirectoryInfo(saveSlotPath);

            // Get all files in the directory and its subdirectories
            FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);

            // If there are no files, return "N/A"
            if (files.Length == 0) return "File Size: N/A";

            // Calculate the total size of all files in bytes
            long totalSize = files.Sum(file => file.Length);

            // Convert the size to a double for easier calculations
            double size = totalSize;

            // Initialize the unit index
            int unitIndex = 0;

            // Convert the size to the appropriate unit
            while (size >= 1024 && unitIndex < sizeUnits.Length - 1)
            {
                // Divide the size by 1024 to convert to the next unit
                size /= 1024;

                // Increment the unit index
                unitIndex++;
            }

            // Return the formatted size string
            return $"File Size: {size:F2} {sizeUnits[unitIndex]}";
        }

        private int GetHighestSaveId()
        {
            // Initialize the highest as -1
            int highestId = -1;

            // If the list is empty, return -1
            if (existingSaves != null && existingSaves.Count > 0)
            {
                // Loop through the list and find the greatest value
                foreach (var value in existingSaves)
                {
                    // If the value is greater than the highest value, set the highest value to the value
                    if (value.Key > highestId) highestId = value.Key;
                }
            }

            // If the highest id is less than the minimum save slots, set it to always show the minimum save slots (-1, 0, 1), accounting for negative indexing
            if (highestId < minimumSaveSlots) highestId = minimumSaveSlots - 1;

            // Increment the highest id by 1 to account for negative indexing
            highestId += 1;

            // Return the highest id found
            return highestId;
        }

        private bool HasMinimumSaveSlots() => ValidSave(HighestSaveId - 1);

        private bool ValidSave(int id) => existingSaves != null && existingSaves.ContainsKey(id);

        #endregion

        #region Save Data Section Methods

        private void DrawInformationHeader()
        {
            // Start a horizontal layout for save file features
            EditorGUILayout.BeginHorizontal();

            // Create Open Saves Path Content
            GUIContent openSavesFolderContent = EditorGUIUtility.IconContent("d_FolderFavorite Icon");
            openSavesFolderContent.tooltip = "Open the Saves Folder in File Explorer";

            // Draw a button to open the saves folder
            if (GUILayout.Button(openSavesFolderContent, _toolButtonStyle)) SavesFolderOpener.OpenSavesFolder();

            // Disable GUI if there are no saves
            GUI.enabled = HasSaves;

            // If filtering files, draw the save controller dropdown
            if (FilterFiles)
            {
                // Dropdown to select the save controller
                _currentIndex = EditorGUILayout.Popup(_currentIndex, saves.Select(save => save.Name).ToArray());
            }
            else
            {
                // Draw disabled popup when not filtering files
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Popup(0, new string[] { "Composite Save Data (All Controllers)" });
                EditorGUI.EndDisabledGroup();
            }

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();

            // Start a horizontal layout for the action buttons
            EditorGUILayout.BeginHorizontal();

            // Add some space between the buttons and the search field
            GUILayout.Space(3f);

            // Create a style for the search field
            GUIStyle searchField = new GUIStyle(EditorStyles.toolbarSearchField)
            {
                // Set fixed height to match other toolbar elements
                fixedHeight = EditorGUIUtility.singleLineHeight
            };

            // Draw a search field with a toolbar style
            searchString = EditorGUILayout.TextField(searchString, searchField);

            // Show the profile ID field only if a valid save slot is selected
            if (selectedSaveSlot >= 0)
            {
                // Store the current profile id
                int currentId = EditorGUILayout.IntField(ProfileData.Id, GUILayout.Width(50));

                // Clamp the current id to 0 minimum
                currentId = Mathf.Max(0, currentId);

                // On Value Changed, update the existing save IDs
                if (GUI.changed && currentId != ProfileData.Id) SetProfileID(currentId);

                // Add some space between the buttons and the search field
                GUILayout.Space(5f);
            }
            else
            {
                // Add some space between the edge and the search field
                GUILayout.Space(3f);
            }

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();

            // Re-enable GUI
            GUI.enabled = true;
        }

        private void LayoutGUI(ISaveData data)
        {
            // If the screen is taller than it is wide, use a vertical layout, otherwise use a horizontal layout
            if (HorizontalLayout)
            {
                // Draw the horizontal layout for the data
                DrawHorizontalDataLayout(data);
            }
            else
            {
                // Draw the vertical layout for the data
                DrawVerticalDataLayout(data);
            }
        }

        private void DrawHorizontalDataLayout(ISaveData data)
        {
            // Start a horizontal layout that expands to the height of the window
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

            // Start a vertical layout for the chunks / locations section, taking up 20% of the screen width
            EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.2f));

            // Spacing between elements
            float spacing = 3f;

            #region Chunks GUI

            // Get the rect for the chunks section
            Rect chunkRect = new Rect(0, dataSectionRect.y + 40, dataSplit.x, locationSplit);

            // Create an area for the chunks section
            GUILayout.BeginArea(chunkRect, EditorStyles.objectFieldThumb);

            // Render the chunks GUI
            ChunksGUI(data);

            // End the area for the chunks section
            GUILayout.EndArea();

            #endregion

            #region Chunks and Locations Splitter

            // Add a draggable splitter at the gap between the chunks and locations section
            Rect locationSplitterRect = new Rect(0, chunkRect.y + chunkRect.height + spacing, chunkRect.width, 7.5f);

            // Draw a rect at the splitter position
            EditorGUIUtility.AddCursorRect(locationSplitterRect, MouseCursor.ResizeVertical);

            // Start resizing on mouse down in the splitter rect
            if (EventInputs.MouseLeft(EventType.MouseDown) && locationSplitterRect.Contains(Event.current.mousePosition)) resizingLocation = true;

            // Calculate the clamp for the split position 
            float locationClamp = dataSectionRect.height - (minLocationSplit * 1.5f);

            // Clamp the split position
            locationSplit = Mathf.Clamp(locationSplit, minLocationSplit, locationClamp);

            // Handle resizing
            if (resizingLocation)
            {
                // Handle mouse drag events
                if (Event.current.type == EventType.MouseDrag)
                {
                    // Update the split position based on the mouse position
                    locationSplit = Mathf.Clamp(Event.current.mousePosition.y - dataSectionRect.y - 40, minLocationSplit, locationClamp);

                    // Repaint the window to reflect the changes
                    Repaint();
                }

                // Stop resizing on mouse up
                if (Event.current.type == EventType.MouseUp) resizingLocation = false;
            }

            #endregion

            #region Locations GUI

            // Get the rect for the locations section
            Rect locationRect = new Rect(0, chunkRect.y + chunkRect.height + spacing, chunkRect.width, (dataSectionRect.height - locationSplit) - (48 + spacing));

            // Create an area for the locations section
            GUILayout.BeginArea(locationRect, EditorStyles.objectFieldThumb);

            // Render the locations GUI
            LocationsGUI(data);

            // End the area for the locations section
            GUILayout.EndArea();

            #endregion

            // End the vertical layout for the chunks and locations section
            EditorGUILayout.EndVertical();

            #region Data Section Split Resizer

            // Add a draggable splitter at the right edge of the chunks and locations section for the data section
            Rect dataSplitterRect = new Rect(chunkRect.width - 2f, chunkRect.y, 50f, dataSectionRect.height - 50f);

            // Draw the splitter rect
            EditorGUIUtility.AddCursorRect(dataSplitterRect, MouseCursor.ResizeHorizontal);

            // Start resizing on mouse down in the splitter rect
            if (EventInputs.MouseLeft(EventType.MouseDown) && dataSplitterRect.Contains(Event.current.mousePosition)) resizingData = true;

            // Calculate the clamp for the split position
            float dataClamp = dataSectionRect.width < minDataSplit.x * 2f ? dataSectionRect.width - spacing : dataSectionRect.width - minDataSplit.x;

            // Clamp the split position
            dataSplit.x = Mathf.Clamp(dataSplit.x, minDataSplit.x, dataClamp);

            // Handle resizing
            if (resizingData)
            {
                // Handle mouse drag events
                if (Event.current.type == EventType.MouseDrag)
                {
                    // Update the split position based on the mouse position
                    dataSplit.x = Mathf.Clamp(Event.current.mousePosition.x, minDataSplit.x, dataClamp);

                    // Repaint the window to reflect the changes
                    Repaint();
                }

                // Stop resizing on mouse up
                if (Event.current.type == EventType.MouseUp) resizingData = false;
            }

            #endregion

            #region Data GUI

            // Start a vertical layout for the data section that expands to fill the remaining width
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            // Get the rect for the data section
            Rect dataRect = new Rect(dataSplit.x + spacing, dataSectionRect.y + 40, dataSectionRect.width - chunkRect.width - (5 + spacing), dataSectionRect.height - (45 + spacing));

            // Create an area for the data section
            GUILayout.BeginArea(dataRect, EditorStyles.objectFieldThumb);

            // Render the data GUI
            DataGUI(data);

            // End the area for the data section
            GUILayout.EndArea();

            // End the vertical layout for the data section
            EditorGUILayout.EndVertical();

            #endregion

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVerticalDataLayout(ISaveData data)
        {
            // Start a horizontal layout that expands to the height of the window
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

            // Start a vertical layout for the chunks section, taking up half the screen width
            EditorGUILayout.BeginVertical(GUILayout.Height(Screen.height * 0.2f));

            // Spacing between elements
            float spacing = 3f;

            #region Locations GUI

            // Get the rect for the chunks section
            Rect chunkRect = new Rect(0, dataSectionRect.y - sectionSplit.y + 40, dataSplit.y, locationSplit);

            // Create an area for the chunks section
            GUILayout.BeginArea(chunkRect, EditorStyles.objectFieldThumb);

            // Render the chunks GUI
            ChunksGUI(data);

            // End the area for the chunks section
            GUILayout.EndArea();

            #endregion

            #region Chunks and Locations Splitter

            // Add a draggable splitter at the gap between the chunks and locations section
            Rect locationSplitterRect = new Rect(0, chunkRect.y + chunkRect.height + spacing, chunkRect.width, 7.5f);

            // Draw a rect at the splitter position
            EditorGUIUtility.AddCursorRect(locationSplitterRect, MouseCursor.ResizeVertical);

            // Start resizing on mouse down in the splitter rect
            if (EventInputs.MouseLeft(EventType.MouseDown) && locationSplitterRect.Contains(Event.current.mousePosition)) resizingLocation = true;

            // Calculate the clamp for the split position 
            float locationClamp = dataSectionRect.height - (minLocationSplit * 1.5f);

            // Clamp the split position
            locationSplit = Mathf.Clamp(locationSplit, minLocationSplit, locationClamp);

            // Handle resizing
            if (resizingLocation)
            {
                // Handle mouse drag events
                if (Event.current.type == EventType.MouseDrag)
                {
                    // Update the split position based on the mouse position
                    locationSplit = Mathf.Clamp(Event.current.mousePosition.y - 40, minLocationSplit, locationClamp);

                    // Repaint the window to reflect the changes
                    Repaint();
                }

                // Stop resizing on mouse up
                if (Event.current.type == EventType.MouseUp) resizingLocation = false;
            }

            #endregion

            #region Locations GUI

            // Get the rect for the locations section
            Rect locationRect = new Rect(0, chunkRect.y + chunkRect.height + spacing, chunkRect.width, (dataSectionRect.height - locationSplit) - (48 + spacing));

            // Create an area for the locations section
            GUILayout.BeginArea(locationRect, EditorStyles.objectFieldThumb);

            // Render the locations GUI
            LocationsGUI(data);

            // End the area for the locations section
            GUILayout.EndArea();

            // End the vertical layout for the locations section
            EditorGUILayout.EndVertical();

            #endregion

            #region Section Split Resizer

            // Add a draggable splitter at the right edge of the chunks and locations section for the data section
            Rect dataSplitterRect = new Rect(chunkRect.width - 2f, chunkRect.y, 50f, dataSectionRect.height - 50f);

            // Draw the splitter rect
            EditorGUIUtility.AddCursorRect(dataSplitterRect, MouseCursor.ResizeHorizontal);

            // Start resizing on mouse down in the splitter rect
            if (EventInputs.MouseLeft(EventType.MouseDown) && dataSplitterRect.Contains(Event.current.mousePosition)) resizingData = true;

            // Calculate the clamp for the split position
            float dataClamp = dataSectionRect.width < minDataSplit.y * 2f ? dataSectionRect.width - spacing : dataSectionRect.width - minDataSplit.y;

            // Clamp the split position
            dataSplit.y = Mathf.Clamp(dataSplit.y, minDataSplit.y, dataClamp);

            // Handle resizing
            if (resizingData)
            {
                // Handle mouse drag events
                if (Event.current.type == EventType.MouseDrag)
                {
                    // Update the split position based on the mouse position
                    dataSplit.y = Mathf.Clamp(Event.current.mousePosition.x, minDataSplit.y, dataClamp);

                    // Repaint the window to reflect the changes
                    Repaint();
                }

                // Stop resizing on mouse up
                if (Event.current.type == EventType.MouseUp) resizingData = false;
            }

            #endregion

            #region Data GUI

            // Start a vertical layout for the data section, taking up half the screen height
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            // Get the rect for the data section
            Rect dataRect = new Rect(dataSplit.y + spacing, dataSectionRect.y - sectionSplit.y + 40, dataSectionRect.width - chunkRect.width - (5 + spacing), dataSectionRect.height - (40 + spacing));

            // Create an area for the data section
            GUILayout.BeginArea(dataRect, EditorStyles.objectFieldThumb);

            // Render the data GUI
            DataGUI(data);

            // End the area for the data section
            GUILayout.EndArea();

            // End the vertical layout
            EditorGUILayout.EndVertical();

            #endregion

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();
        }

        private void ChunksGUI(ISaveData data)
        {
            // Display the header for the chunks section
            Header("Chunks");

            // Set up a scroll view for the chunks list
            _chunkScrollPos = EditorGUILayout.BeginScrollView(_chunkScrollPos);

            // Iterate through each chunk ID in the save data
            foreach (var chunkId in data.GetChunkIDs())
            {
                // Set the first chunk as the current chunk if none is selected
                _currentChunk ??= chunkId;

                // Try to get the chunk name from the cache
                if (!_chunkNames.TryGetValue(chunkId, out var chunkName))
                {
                    // Try to get the asset path from the chunk ID (GUID)
                    chunkName = AssetDatabase.GUIDToAssetPath(chunkId);

                    // If the path is empty, it means the asset was deleted or moved, so just use the chunk ID
                    if (string.IsNullOrEmpty(chunkName)) chunkName = chunkId;

                    // Cache the chunk name for future use
                    _chunkNames.Add(chunkId, chunkName);
                }

                // Create a list item for each chunk, if clicked, set it as the current chunk
                if (ListItem(chunkId, chunkName, _currentChunk))
                {
                    // Set the clicked chunk as the current chunk
                    _currentChunk = chunkId;

                    // Reset the current location when the chunk changes
                    _currentLocation = null;
                }
            }

            // End the scroll view
            EditorGUILayout.EndScrollView();
        }

        private void LocationsGUI(ISaveData data)
        {
            // Display the header for the locations section
            Header("Locations");

            // Set up a scroll view for the locations list
            _locationScrollPos = EditorGUILayout.BeginScrollView(_locationScrollPos);

            // Only display locations if a chunk is selected
            if (!string.IsNullOrEmpty(_currentChunk))
            {
                // Get the current chunk of data
                var chunk = data.GetChunk(_currentChunk);

                // Iterate through each location in the current chunk
                foreach (var location in chunk.Keys)
                {
                    // Set the first location as the current location if none is selected
                    _currentLocation ??= location;

                    // Get a readable name for the location
                    var name = ShowLocation ? $"{data.GetChunkName(location)}: {location}" : data.GetChunkName(location);

                    // Filter locations based on the search string
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(searchString) && !name.ToLower().Contains(searchString.ToLower())) continue;

                    // Create a list item for each location in the chunk, if clicked, set it as the current location
                    if (ListItem(location, name, _currentLocation)) _currentLocation = location;
                }
            }

            // End the scroll view
            EditorGUILayout.EndScrollView();
        }

        private void DataGUI(ISaveData data)
        {
            // Display the header for the data section
            Header("Data: " + _currentLocation?.ToString());

            // Set up a scroll view for the data display areas
            _dataScrollPos = EditorGUILayout.BeginScrollView(_dataScrollPos);

            // Only display data if a chunk and location are selected
            if (!string.IsNullOrEmpty(_currentChunk) && _currentLocation != null)
            {
                // Get the current chunk of data
                var chunk = data.GetChunk(_currentChunk);

                // Try to get the data for the current location
                if (chunk.TryGetValue(_currentLocation, out var value))
                {
                    // Format the data as pretty-printed JSON if possible
                    if (!_formattedData.TryGetValue(value, out var formatted))
                    {
#if UNITY_NEWTONSOFT_JSON
                        try
                        {
                            // Pretty-print the JSON data
                            formatted = Newtonsoft.Json.Linq.JToken.Parse(value).ToString(Newtonsoft.Json.Formatting.Indented);

                            // Cache the formatted data for future use
                            _formattedData.Add(value, formatted);
                        }
                        catch
                        {
                            // If parsing fails, just use the raw value
                            formatted = value;
                        }
#else
                        // If Newtonsoft.Json is not available, just use the raw value
                        formatted = value;
#endif
                    }

                    // Display the formatted data in a text area
                    EditorGUILayout.TextArea(formatted, GUILayout.ExpandHeight(true));
                }
            }

            // End the scroll view
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region GUI Helper Methods

        private void GetStyles()
        {
            // Get the section style
            _sectionStyle ??= new GUIStyle(EditorStyles.objectFieldThumb)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 5, 5),
            };

            // Get the header style
            _headerStyle ??= new GUIStyle(GUI.skin.label)
            {
                fixedHeight = 32
            };

            // Get the list item style
            _listItemStyle ??= new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
            };

            // Get the button style for the text buttons
            _toolButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(1, 1, 1, 1),
                fixedHeight = EditorGUIUtility.singleLineHeight,
                fixedWidth = 20
            };

            // Get the content style for the save slot button
            _saveSlotStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(10, 10, 10, 10),
                fixedHeight = (EditorGUIUtility.singleLineHeight + 3) * 4,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            // Create mini button style
            _miniButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(1, 1, 1, 1),
                fixedHeight = EditorGUIUtility.singleLineHeight
            };
        }

        private void ClearStyles()
        {
            // Clear all cached styles
            _sectionStyle = null;
            _headerStyle = null;
            _listItemStyle = null;
            _toolButtonStyle = null;
            _saveSlotStyle = null;
            _miniButtonStyle = null;
        }

        private void Header(string label) => GUILayout.Box(label, _headerStyle);

        private void HorizontalLine() => EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        private bool ListItem<T>(T id, string label, T activeId)
        {
            // Create a button for the list item
            var clicked = GUILayout.Button(label, _listItemStyle);

            // Highlight the active item
            if (id.Equals(activeId)) EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(1, 1, 1, 0.2f));

            // Return true if the item was clicked and is not already active
            return clicked;
        }

        private static void DrawDynamicToolbar()
        {
            // Begin the horizontal toolbar layout
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Add some space at the start of the toolbar
            GUILayout.FlexibleSpace();

            // Invoke the registered toolbar button method
            OnDrawToolbar.Invoke();

            // Add some space at the end of the toolbar
            GUILayout.Space(4);

            // End the horizontal toolbar layout
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Static Helper Methods

        public void CreateAbsolute() => SaveStoreRegistry.CreateByScope(SaveScope.Absolute);

        public void SaveAbsolute() => SaveStoreRegistry.SaveByScope(SaveScope.Absolute, saveMode);

        public void LoadAbsolute() => SaveStoreRegistry.LoadByScope(SaveScope.Absolute, saveMode);

        public void DeleteAbsolute() => SaveStoreRegistry.DeleteByScope(SaveScope.Absolute);

        public void CreateIndexed()
        {
            // Create all of the indexed saves (Global and Scene)
            SaveStoreRegistry.CreateByScope(SaveScope.Global);
            SaveStoreRegistry.CreateByScope(SaveScope.Scene);

            // Include Temporary saves as well, for simplicity
            SaveStoreRegistry.CreateByScope(SaveScope.Temporary);
        }

        public void SaveIndexed()
        {
            // Save all of the indexed saves (Global and Scene)
            SaveStoreRegistry.SaveIndexed(saveMode);

            // Include Temporary saves as well, for simplicity
            SaveStoreRegistry.SaveByScope(SaveScope.Temporary, SaveMode.MemoryOnly);
        }

        public void LoadIndexed()
        {
            // Load all of the indexed saves (Global and Scene)
            SaveStoreRegistry.LoadIndexed(saveMode);

            // Include Temporary saves as well, for simplicity
            SaveStoreRegistry.LoadByScope(SaveScope.Temporary, SaveMode.MemoryOnly);
        }

        public void DeleteIndexed()
        {
            // Delete all indexed saves (Global and Scene)
            SaveStoreRegistry.DeleteByScope(SaveScope.Global);
            SaveStoreRegistry.DeleteByScope(SaveScope.Scene);

            // Include Temporary saves as well, for simplicity
            SaveStoreRegistry.DeleteByScope(SaveScope.Temporary);
        }

        public void DeleteAll()
        {
            // Get the existing save IDs before deletion
            RefreshExistingSaveIDs();

            // Loop through all existing save IDs
            foreach (var id in existingSaves)
            {
                // Set the profile ID, if it isn't a negative index
                SetProfileID(id.Key);

                // Delete all saves for this profile ID
                SaveStoreRegistry.DeleteByScope(SaveScope.Scene);
                SaveStoreRegistry.DeleteByScope(SaveScope.Global);
            }

            // Finally, delete Temporary and Absolute saves
            SaveStoreRegistry.DeleteByScope(SaveScope.Temporary);
            SaveStoreRegistry.DeleteByScope(SaveScope.Absolute);

            // Clear the existing save IDs list
            existingSaves.Clear();
        }

        [MenuItem("Window/Sanctuary/Clear Cache")]
        public static void ClearCache()
        {
            // Clear existing saves in all save controllers
            SaveControllerBase.ExistingSaves.Clear();

            // Clear the formatted data cache
            _formattedData.Clear();

            // Clear the chunk names cache
            _chunkNames.Clear();

            // Clear the current save reference
            currentSave = null;
        }

        #endregion
    }
}