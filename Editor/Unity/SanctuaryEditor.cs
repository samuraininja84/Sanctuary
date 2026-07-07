using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sanctuary.Stores;
using Sanctuary.Extensions;

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
        private static ISaveController currentController;
        private static ISaveController[] saveControllers = Array.Empty<ISaveController>();

        // Registered stores
        private static Dictionary<ISaveController, List<ISaveStore>> registeredStores = new();
        private static Dictionary<ISaveController, bool> controllerFoldouts = new();

        // Data caches
        private static readonly Dictionary<string, string> _formattedData = new();
        private static readonly Dictionary<string, string> _chunkNames = new();

        // Tabs for the two main sections
        private static readonly string[] sectionNames = new string[] { "Data Viewer", "Store Tracker" };

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
        private bool showingSaveSlotOptions;
        private int selectedSaveSlot;

        // Current selections
        private int _currentIndex;
        private string _currentChunk;
        private string _currentLocation;
        private int _selectedSectionIndex;

        // Scroll positions for the two main data viewing sections
        private Vector2 _chunkScrollPos;
        private Vector2 _dataScrollPos;

        // Scroll position for the store tracker section
        private Vector2 _storeScrollPos;

        // Section resizing
        private Vector2 minSectionSplit = new(150f, 235f);
        private Vector2 sectionSplit = new(300f, 400f);
        private Rect dataSectionRect;
        private bool resizingSection;

        // Chunk / Location settings
        private bool showNestedLocations = false;

        // Data area resizing
        private Vector2 minDataSplit = new(150f, 150f);
        private Vector2 dataSplit = new(300f, 300f);
        private bool resizingData;

        // Convert the size to a human-readable format
        private static readonly string[] sizeUnits = { "B", "KB", "MB", "GB", "TB" };

        private bool HorizontalLayout => Screen.width > Screen.height;
        private bool HasSaves => SaveProvider.ExistingControllers.Count > 0;
        private bool ShowLocation => SanctuaryEditorProcessor.showLocationWhenNamed;
        private bool FilterFiles => SanctuaryEditorProcessor.filterFiles;
        public static bool SaveToGlobal => SanctuaryEditorProcessor.saveToGlobal;
        public static bool SaveToTemporary => SanctuaryEditorProcessor.saveToTemporary;
        public static bool SaveToAll => SanctuaryEditorProcessor.saveToAll;

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

        [MenuItem("Window/Sanctuary/Clear Cache")]
        public static void ClearCache()
        {
            // Clear existing saves in all save controllers
            SaveProvider.ClearAllControllers();

            // Clear the existing saves in the editor
            saveControllers = null;

            // Clear the current save reference
            currentController = null;

            // Clear the registered stores dictionary
            registeredStores.Clear();

            // Clear the controller foldouts dictionary
            controllerFoldouts.Clear();

            // Clear the formatted data cache
            _formattedData.Clear();

            // Clear the chunk names cache
            _chunkNames.Clear();
        }

        private void OnFocus()
        {
            // Clear styles when the window gains focus
            ClearStyles();

            // Repaint the window when it gains focus
            Repaint();
        }

        private void OnEnable()
        {
            // Register the event handlers for when a save store is registered or unregistered
            SaveStoreRegistry.OnStoreRegistered += Register;
            SaveStoreRegistry.OnStoreUnregistered += Unregister;

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
            // Unregister the event handlers for when a save store is registered or unregistered
            SaveStoreRegistry.OnStoreRegistered -= Register;
            SaveStoreRegistry.OnStoreUnregistered -= Unregister;

            // Unsubscribe all toolbar button methods
            OnDrawToolbar = delegate { };

            // Clear the data caches
            ClearCache();
        }

        private void OnInspectorUpdate()
        {
            // If the application is not running, clear the data caches to avoid stale data
            if (!Application.isPlaying && saveControllers != null) ClearCache();
        }

        private void OnGUI()
        {
            // Ensure styles are initialized
            GetStyles();

            // Get the current mouse position
            Vector2 globalMousePosition = Event.current.mousePosition;

            // Update the static saveControllers reference
            saveControllers = SaveProvider.ExistingControllers.Select(wr => wr.TryGetTarget(out var save) ? save : null).Where(save => save != null).ToArray();

            // Start checking for changes in the GUI
            EditorGUI.BeginChangeCheck();

            // Additional spacing for the scroll rect
            float scrollRectSpacing = 3f;

            // Get the window rect
            Rect windowRect = position;

            // Draw the action menu section
            DrawActionMenu(scrollRectSpacing);

            // Adjust the layout based on the current mouse position and window rect
            LayoutAdjustments(globalMousePosition, windowRect);

            // Draw the save data section
            DrawSection(_selectedSectionIndex, scrollRectSpacing);
        }

        #region Draw Methods

        private void DrawActionMenu(float scrollRectSpacing)
        {
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

            // Add some space before the save slot selection
            EditorGUILayout.Space(1f);

            // Draw the save slot selection
            DrawSaveSlots();

            // Push actions to the bottom, if using horizontal layout
            GUILayout.FlexibleSpace();

            //// Draw the save slot actions if there are any existing saves
            // DrawSaveSlotActions();

            // Draw the dynamic toolbar
            DrawDynamicToolbar();

            // End the area for the actions section
            GUILayout.EndArea();
        }

        private void LayoutAdjustments(Vector2 globalMousePosition, Rect windowRect)
        {
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
        }

        private void DrawSectionToolbar(float width = -1) => _selectedSectionIndex = Tabs(sectionNames, _selectedSectionIndex, ref searchString, width);

        private void DrawSection(int sectionIndex, float scrollRectSpacing)
        {
            // Draw the section based on the selected index
            switch (sectionIndex)
            {
                case 0:
                    // Draw the save data section
                    DrawSaveData(scrollRectSpacing);
                    break;
                case 1:
                    // Draw the store tracker section
                    DrawStoreTracker(scrollRectSpacing);
                    break;
            }
        }

        private void DrawSaveData(float scrollRectSpacing)
        {
            // Begin the area for the right section, if using horizontal layout
            if (HorizontalLayout)
            {
                // Calculate the starting position for the right section
                float startPos = Mathf.Max(sectionSplit.x, minSectionSplit.x) + scrollRectSpacing;

                // Define the rect for the right section
                dataSectionRect = new Rect(startPos, 3, (position.width - startPos - 1), position.height - 3);

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
            if (!HasSaves)
            {
                // Define a style for the empty state message
                var emptyStateStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    stretchHeight = true
                };

                // Display a message when no saves are found
                GUILayout.Box("No existing save controllers found.\nOnce created, they'll appear in this window.", emptyStateStyle);

                // Draw the section toolbar
                DrawSectionToolbar(dataSectionRect.width / 2);

                // End the area for the right section, if using horizontal layout
                GUILayout.EndArea();

                // Return early since there are no saves to display
                return;
            }

            // Get the currently selected save
            currentController = saveControllers[Mathf.Clamp(_currentIndex, 0, saveControllers.Length - 1)];

            // Reset the current chunk and location if the selected save changes
            if (EditorGUI.EndChangeCheck())
            {
                // Clear cached formatted data when switching saves
                _currentChunk = null;

                // Reset the current location when the chunk changes
                _currentLocation = null;
            }

            // Get the composite save data
            ISaveData composite = ISaveDataExtensions.Combine(new SaveData(), saveControllers.Select(s => s.Data));

            // Determine the save data to display based on filtering
            ISaveData data = FilterFiles ? currentController.Data : composite;

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

            // Draw the section toolbar
            DrawSectionToolbar(dataSectionRect.width / 2);

            // End the area for the right section, if using horizontal layout
            GUILayout.EndArea();
        }

        private void DrawStoreTracker(float scrollRectSpacing)
        {
            // Begin the area for the right section, if using horizontal layout
            if (HorizontalLayout)
            {
                // Calculate the starting position for the right section
                float startPos = Mathf.Max(sectionSplit.x, minSectionSplit.x) + scrollRectSpacing;

                // Define the rect for the right section
                dataSectionRect = new Rect(startPos + 2, 3, (position.width - startPos - 1), position.height - 3);

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

            // Draw the tracking header for the store tracker section
            DrawTrackingHeader();

            // If the application is in play mode, display the list of save stores, otherwise display a message indicating that the window is only available in play mode
            if (!Application.isPlaying)
            {
                // Define a style for the empty state message, which is centered and stretches to fill the available height and width of the window
                var emptyStateStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    stretchHeight = true
                };

                // Display a message indicating that the window is only available in play mode
                GUILayout.Box("The Save Store Tracker window is only available in play mode.", emptyStateStyle);

                // Draw the section toolbar
                DrawSectionToolbar(dataSectionRect.width / 2);

                // End the area for the right section, if using horizontal layout
                GUILayout.EndArea();

                // Return early
                return;
            }
            else
            {
                // Start a scroll view to display the list of save stores
                _storeScrollPos = EditorGUILayout.BeginScrollView(_storeScrollPos);

                // Add a space between the label and the list of save stores
                EditorGUILayout.Separator();

                // Draw the store tracker GUI
                StoreTrackerGUI();

                // End the scroll view
                EditorGUILayout.EndScrollView();
            }

            // Begin a horizontal layout for the section toolbar and refresh button
            EditorGUILayout.BeginHorizontal();

            // Reduce the space between the section toolbar and the refresh button
            EditorGUILayout.Space(-3f);

            // Draw the section toolbar
            DrawSectionToolbar(dataSectionRect.width / 2);

            // Push the refresh button to the right side of the toolbar
            GUILayout.FlexibleSpace();

            // Get the GuiContent for the refresh button
            var refreshContent = EditorGUIUtility.IconContent("d_Refresh");
            refreshContent.tooltip = "Refresh the list of save stores";

            // Add a button to refresh the list of save stores
            if (GUILayout.Button(refreshContent, GUILayout.Width(30))) registeredStores = Convert(SaveStoreRegistry.GetRegisteredStores());

            // End the horizontal layout for the section toolbar and refresh button
            EditorGUILayout.EndHorizontal();

            // End the area for the right section, if using horizontal layout
            GUILayout.EndArea();
        }

        #endregion

        #region Save Slot Methods

        private void DrawSaveSlots()
        {
            // Start a scroll view for the save slots if there are more than the minimum save slots
            _slotsScrollPos = GUILayout.BeginScrollView(_slotsScrollPos);

            // Draw a save slot button for the current save slot
            GUILayout.Box("Save Slots", _sectionStyle);

            // Disable GUI if there are no existing saves
            GUI.enabled = HasSaves;

            // Get all the save slots from the slot registry
            var slots = GetAllAvailableSlots();

            // If there are no save slots, display a help box indicating that no save slots were found
            if (slots.Count == 0)
            {
                // Show the help box indicating no save slots found
                EditorGUILayout.HelpBox("No save slots found. Please create a new save slot or load existing data.", MessageType.Info);

                // End the scroll view if there are more than the minimum save slots
                GUILayout.EndScrollView();

                // Re-enable GUI
                GUI.enabled = true;

                // Return early since there are no save slots to display
                return;
            }

            // Draw the save slots
            for (int i = 0; i < slots.Count; i++) DrawSaveSlot(slots[i], i);

            // End the scroll view if there are more than the minimum save slots
            GUILayout.EndScrollView();

            // Re-enable GUI
            GUI.enabled = true;
        }

        private void DrawSaveSlot(SaveSlotInfo slot, int index)
        {
            // Extract the relevant information from the save slot
            string slotName = "Slot ID: " + slot.SlotId;
            string startedAt = "Started At: " + slot.FileCreationTime.ToString("g");
            string lastModified = "Last Modified: " + slot.LastSaveTime.ToString("g");
            string totalPlayTime = "Total Play Time: " + TimeSpan.FromSeconds(slot.TotalPlayTimeSeconds).ToString(@"hh\:mm\:ss");
            string fileSize = "File Size: " + GetReadableFileSize(slot.FileSize);
            string schemaVersion = "Schema Version: " + slot.SchemaVersion;

            // Combine the info into a string
            string combinedInfo = $"{slotName}\n{startedAt}\n{lastModified}\n{totalPlayTime}\n{fileSize}\n{schemaVersion}";

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

                    // Show the save slot options when selecting a new slot
                    showingSaveSlotOptions = true;
                }
            }

            // If this save slot is selected, draw the save slot options
            if (selectedSaveSlot == index) DrawSaveSlotOptions(index);
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

            // Draw a mini button to overwrite this save
            if (GUILayout.Button(overwriteContent, _miniButtonStyle))
            {
                // Set the current index to this index
                selectedSaveSlot = index;

                // Save the data
                Save(index);
            }

            // Draw a mini button to load this save
            if (GUILayout.Button(loadContent, _miniButtonStyle))
            {
                // Set the current index to this index
                selectedSaveSlot = index;

                // Load the save
                Load(index);
            }

            // Draw a mini button to delete this save
            if (GUILayout.Button(deleteContent, _miniButtonStyle))
            {
                // Set the current index to this index
                selectedSaveSlot = index;

                // Delete the save
                Delete(index);
            }

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSaveSlotActions()
        {
            // Disable GUI if there are no existing saves
            GUI.enabled = HasSaves;

            // Check if there are any existing saves with an id higher than the highest save id
            //if (HasMinimumSaveSlots())
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
                newGameContent.tooltip = $"Create a New Save with Default Settings";

                // Draw the button for creating a new save in this slot
                if (GUILayout.Button(newGameContent, _miniButtonStyle))
                {
                    //// Set the save data id to -1
                    //selectedSaveSlot = -1;

                    //// Create a save in this slot
                    //Save(selectedSaveSlot);

                    // Repaint the window
                    Repaint();

                    // Scroll to the bottom of the save slots
                    _slotsScrollPos.y = float.MaxValue;
                }

                // Draw a button to delete the last save
                if (GUILayout.Button(deleteLastSaveContent, _miniButtonStyle))
                {
                    //// Set the save data id to -1
                    //selectedSaveSlot = -1;

                    //// Delete the save
                    //Delete(selectedSaveSlot);

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

        #region Save Helper Methods

        public async void Save(int index) => await saveControllers[index].Save();

        public async void Load(int index) => await saveControllers[index].Load();

        public async void Delete(int index) => await saveControllers[index].Delete();

        public async void DeleteAll()
        {
            // Finally, delete Temporary and Absolute saves
            await SaveProvider.ByScope(SaveScope.Absolute).DeleteAll();
            await SaveProvider.ByScope(SaveScope.Global).DeleteAll();
            await SaveProvider.ByScope(SaveScope.Temporary).DeleteAll();
        }

        private string GetReadableFileSize(long fileSize)
        {
            // Convert the file size to a human-readable format
            int unitIndex;

            // If the file size is greater than 0, calculate the unit index and readable size
            if (fileSize > 0)
            {
                // Calculate the unit index based on the file size
                unitIndex = (int)Mathf.Floor(Mathf.Log10(fileSize) / Mathf.Log10(1024));

                // Clamp the unit index to the available size units
                unitIndex = Mathf.Clamp(unitIndex, 0, sizeUnits.Length - 1);

                // Calculate the readable file size
                float readableSize = fileSize / Mathf.Pow(1024, unitIndex);

                // Return the formatted string with two decimal places
                return $"{readableSize:F2} {sizeUnits[unitIndex]}";
            }
            else
            {
                // Return "0 B" for a file size of 0
                return "0 B";
            }
        }

        private List<SaveSlotInfo> GetAllAvailableSlots()
        {
            // Initialize a list to hold the save slot information
            var slots = new List<SaveSlotInfo>();

            // Iterate through each save controller to gather save slot information
            foreach (var save in saveControllers)
            {
                // Get the save slot information for the current save controller
                var slotInfo = save.GetAvailableSlots();

                // Add the save slot information to the list
                slots.AddRange(slotInfo);
            }

            // Return the list of save slot information
            return slots;
        }

        private void Register(ISaveStore store, ISaveController controller)
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

        private static Dictionary<ISaveController, List<ISaveStore>> Convert(Dictionary<ISaveStore, ISaveController> source)
        {
            // Create a new dictionary to hold the converted data
            var target = new Dictionary<ISaveController, List<ISaveStore>>();

            // Sort the source dictionary by the controller's name and the list of stores for each controller by the store's name
            var sortedSource = source.OrderBy(kvp => kvp.Value.Name).ThenBy(kvp => kvp.Key.Source.name).ToList();

            // Iterate through the sorted source dictionary to convert it into the target dictionary
            foreach (var kvp in sortedSource)
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

        #endregion

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

            // Create toggle button content
            GUIContent toggleContent = (SanctuaryEditorProcessor.filterFiles ? "ToggleOn" : "ToggleOff").ToGUIContent();
            toggleContent.tooltip = SanctuaryEditorProcessor.filterFiles ? "Filtering files enabled. Click to disable." : "Filtering files disabled. Click to enable.";

            // Draw the toggle button
            if (GUILayout.Button(toggleContent, _toolButtonStyle))
            {
                // Toggle the filterFiles state
                SanctuaryEditorProcessor.filterFiles = !SanctuaryEditorProcessor.filterFiles;

                // Save the new value to EditorPrefs
                EditorPrefs.SetBool(SanctuaryEditorProcessor.filterFilesKey, SanctuaryEditorProcessor.filterFiles);
            }

            // Disable GUI if there are no saves
            GUI.enabled = HasSaves;

            // If there are saves, draw the save controller dropdown, otherwise show a disabled popup indicating no saves are available
            if (HasSaves)
            {
                // If filtering files, draw the save controller dropdown
                if (FilterFiles)
                {
                    // Dropdown to select the save controller
                    _currentIndex = EditorGUILayout.Popup(_currentIndex, saveControllers.Select(save => save.Name).ToArray());
                }
                else
                {
                    // Draw disabled popup when not filtering files
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Popup(0, new string[] { "Composite Save Data (All Controllers)" });
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                // Draw a disabled popup with a message indicating no save files found
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Popup(0, new string[] { "No Save Files Available" });
                EditorGUI.EndDisabledGroup();
            }

            // Add some space between the edge and the search field
            GUILayout.Space(3f);

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();

            // Start a horizontal layout for the action buttons
            EditorGUILayout.BeginHorizontal();

            // Add some space between the buttons and the search field
            GUILayout.Space(3f);

            // Create a style for the search field
            var searchField = new GUIStyle(EditorStyles.toolbarSearchField)
            {
                // Set fixed height to match other toolbar elements
                fixedHeight = EditorGUIUtility.singleLineHeight
            };

            // Draw a search field with a toolbar style
            searchString = EditorGUILayout.TextField(searchString, searchField);

            // Add some space between the edge and the search field
            GUILayout.Space(3f);

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

            // Spacing between elements
            float spacing = 3f;

            // Calculate the height for the data section, accounting for the header and spacing
            float height = dataSectionRect.height - (65 + spacing);

            // Calculate the y position for the data section, accounting for the header and spacing
            float yPosition = dataSectionRect.y + 40;

            #region Chunks GUI

            // Get the rect for the chunks section
            Rect chunkRect = new Rect(0, yPosition, dataSplit.x, height);

            // Create an area for the chunks section
            GUILayout.BeginArea(chunkRect, EditorStyles.objectFieldThumb);

            // Render the chunks GUI
            ChunksGUI(data);

            // End the area for the chunks section
            GUILayout.EndArea();

            #endregion

            #region Data Section Split Resizer

            // Add a draggable splitter at the right edge of the chunks and locations section for the data section
            Rect dataSplitterRect = new Rect(chunkRect.width - 2f, chunkRect.y, 50f, height);

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
            Rect dataRect = new Rect(dataSplit.x + spacing, yPosition, dataSectionRect.width - chunkRect.width - (5 + spacing), height);

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

            // Spacing between elements
            float spacing = 3f;

            // Calculate the height for the data section, accounting for the header and spacing
            float height = dataSectionRect.height - (65 + spacing);

            // Calculate the y position for the data section, accounting for the header and spacing
            float yPosition = 40 + spacing;

            #region Locations GUI

            // Get the rect for the chunks section
            Rect chunkRect = new Rect(0, yPosition, dataSplit.y, height);

            // Create an area for the chunks section
            GUILayout.BeginArea(chunkRect, EditorStyles.objectFieldThumb);

            // Render the chunks GUI
            ChunksGUI(data);

            // End the area for the chunks section
            GUILayout.EndArea();

            #endregion

            #region Section Split Resizer

            // Add a draggable splitter at the right edge of the chunks and locations section for the data section
            Rect dataSplitterRect = new Rect(chunkRect.width - 2f, chunkRect.y, 50f, height);

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
            Rect dataRect = new Rect(dataSplit.y + spacing, yPosition, dataSectionRect.width - chunkRect.width - (5 + spacing), height);

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
            Header("Chunks / Locations");

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

                // Add an arrow to indicate whether the chunk is expanded or collapsed
                string showingLocations = chunkId == _currentChunk && showNestedLocations ? "▼ " : "▶ ";

                // Create a list item for each chunk, if clicked, set it as the current chunk
                if (ListItem(chunkId, showingLocations + chunkName, _currentChunk))
                {
                    // Set the clicked chunk as the current chunk
                    _currentChunk = chunkId;

                    // Reset the current location when the chunk changes
                    _currentLocation = null;

                    // Toggle the display of nested locations when a chunk is clicked
                    showNestedLocations = !showNestedLocations;
                }

                // If the current chunk is selected, display its locations in a nested list
                if (chunkId == _currentChunk && showNestedLocations) LocationsGUI(data, chunkId);
            }

            // End the scroll view
            EditorGUILayout.EndScrollView();
        }

        private void LocationsGUI(ISaveData data, string chunkId)
        {
            // Get the current chunk of data
            var chunk = data.GetChunk(chunkId);

            // Iterate through each location in the current chunk
            foreach (var location in chunk.Keys)
            {
                // Set the first location as the current location if none is selected
                _currentLocation ??= location;

                // Get a readable name for the location
                var name = ShowLocation ? $"{data.GetChunkName(location)}: {location}" : data.GetChunkName(location);

                // Filter locations based on the search string
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(searchString) && !name.ToLower().Contains(searchString.ToLower())) continue;

                // Start a horizontal layout for the location list item
                EditorGUILayout.BeginHorizontal();

                // Add an arrow to indicate the current location
                string addDot = location == _currentLocation ? "  ● " : "  ";

                // Create a list item for each location in the chunk, if clicked, set it as the current location
                if (ListItem(location, addDot + name, _currentLocation)) _currentLocation = location;

                // End the horizontal layout for the location list item
                EditorGUILayout.EndHorizontal();
            }
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

                    // Disable editing of the text area to prevent focusing the text area and modifying the data
                    EditorGUI.BeginDisabledGroup(true);

                    // Display the formatted data in a text area
                    EditorGUILayout.TextArea(formatted, GUILayout.ExpandHeight(true));

                    // Re-enable GUI after displaying the text area
                    EditorGUI.EndDisabledGroup();
                }
            }

            // End the scroll view
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Save Store Tracker

        private void DrawTrackingHeader()
        {
            // Start a horizontal layout for save file features
            EditorGUILayout.BeginHorizontal();

            // Create Open Saves Path Content
            GUIContent openSavesFolderContent = EditorGUIUtility.IconContent("d_FolderFavorite Icon");
            openSavesFolderContent.tooltip = "Open the Saves Folder in File Explorer";

            // Draw a button to open the saves folder
            if (GUILayout.Button(openSavesFolderContent, _toolButtonStyle)) SavesFolderOpener.OpenSavesFolder();

            // Add some space between the buttons and the search field
            GUILayout.Space(3f);

            // Disable GUI if there are no saves
            GUI.enabled = HasSaves;

            // Create a style for the search field
            var searchField = new GUIStyle(EditorStyles.toolbarSearchField)
            {
                // Set fixed height to match other toolbar elements
                fixedHeight = EditorGUIUtility.singleLineHeight
            };

            // Draw a search field with a toolbar style
            searchString = EditorGUILayout.TextField(searchString, searchField);

            // Add some space between the edge and the search field
            GUILayout.Space(3f);

            // End the horizontal layout
            EditorGUILayout.EndHorizontal();

            // Re-enable GUI
            GUI.enabled = true;
        }

        private void StoreTrackerGUI()
        {
            // If the registered stores dictionary is empty, display a message indicating that there are no save stores registered
            if (registeredStores.Count == 0)
            {
                // Display a message indicating that there are no save stores registered
                EditorGUILayout.LabelField("No save stores registered.");

                // Return early to avoid displaying an empty list
                return;
            }

            // If the controller foldouts dictionary is empty, initialize it with the registered controllers
            if (controllerFoldouts.Count != registeredStores.Count)
            {
                // Clear the controller foldouts dictionary to avoid stale data
                controllerFoldouts.Clear();

                // Iterate through the registered stores to initialize the controller foldouts
                foreach (var kvp in registeredStores)
                {
                    // Get the save controller from the registered stores
                    var controller = kvp.Key;

                    // Initialize the foldout state for the controller to false (collapsed)
                    controllerFoldouts[controller] = false;
                }
            }

            // Display the list of save stores here
            foreach (var kvp in registeredStores)
            {
                // Get the save store and its associated controller from the dictionary
                var controller = kvp.Key;
                var stores = kvp.Value;

                // Display the name of the controller associated with the save store
                if (controller != null)
                {
                    // Create a string that combines the controller's name and its type for display
                    string controllerName = controller.Name + " (" + controller.GetType().Name + ")";

                    // Draw a foldout for the list of save stores associated with the current controller
                    controllerFoldouts[controller] = EditorGUILayout.Foldout(controllerFoldouts[controller], controllerName, true);

                    // Indent the foldout for better readability
                    EditorGUI.indentLevel++;

                    // If the foldout is expanded and there are save stores associated with the controller, display the list of save stores
                    if (controllerFoldouts[controller] && stores != null)
                    {
                        // Iterate through the list of save stores for the current controller
                        for (var i = 0; i < stores.Count; i++)
                        {
                            // Get the current save store from the list of stores
                            var store = stores[i];

                            // Skip null stores to avoid displaying them
                            if (store == null) continue;

                            // Create a string that combines the source object's name (if it exists) and the type of the save store for display
                            string storeName = store.Source != null ? store.Source.name : store.GetType().Name;

                            // If the store name doesn't match the search string, skip displaying it
                            if (!string.IsNullOrEmpty(searchString) && !storeName.ToLower().Contains(searchString.ToLower())) continue;

                            // Display the source object of the save store if it exists, otherwise display "N/A" and the type of the save store
                            if (store.Source != null) EditorGUILayout.ObjectField(store.Source, typeof(UnityEngine.Object), true);
                            else EditorGUILayout.LabelField("Source: N/A | Type: " + store.GetType().Name);
                        }
                    }

                    // Reset the indent level after displaying the list of save stores
                    EditorGUI.indentLevel--;
                }

                // Add a space between the list of save stores for each controller
                EditorGUILayout.Separator();
            }
        }

        #endregion

        #region GUI Helper Methods

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

        public static int Tabs(string[] options, int selected, ref string searchString, float width = -1)
        {
            // Define the colors for the tabs
            const float DarkGray = 0.5f;
            const float LightGray = 1f;

            // Store the current background color to restore it later
            var storeColor = GUI.backgroundColor;

            // Define the highlight and background colors for the tabs
            var highlightCol = new Color(LightGray, LightGray, LightGray);
            var bgCol = new Color(DarkGray, DarkGray, DarkGray);

            // Create a button style based on the default button style, but with modified padding
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding.bottom = 3;

            // Start a horizontal layout for the tabs, overriding the width if specified
            if (width > 0) GUILayout.BeginHorizontal(GUILayout.Width(width));
            else GUILayout.BeginHorizontal();

            // Interate through the options and create a button for each one
            for (int i = 0; i < options.Length; ++i)
            {
                // Set the background color for the tab based on whether it's selected or not
                GUI.backgroundColor = i == selected ? bgCol : highlightCol;

                // Create a button for the tab, and if it's clicked, set it as the selected tab
                if (GUILayout.Button(options[i], buttonStyle))
                {
                    // Set the selected tab index to the clicked tab
                    selected = i;

                    // Clear the search string when a new tab is selected to avoid filtering the list based on the previous tab's search
                    searchString = string.Empty;
                }
            }

            // End the horizontal layout for the tabs
            GUILayout.EndHorizontal();

            // Restore the background color
            GUI.backgroundColor = storeColor;

            // Return the selected tab index
            return selected;
        }

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
                fixedHeight = (EditorGUIUtility.singleLineHeight + 3) * 5,
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

        #endregion
    }
}