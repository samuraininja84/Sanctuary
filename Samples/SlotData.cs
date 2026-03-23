using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sanctuary.Samples
{
    /// <summary>
    /// Represents a data structure for tracking the state and progress of a specific slot.
    /// </summary>
    /// <remarks>
    /// The <see cref="SlotData"/> class is designed to store and manage information about a slot, including: 
    /// <list type="bullet"> 
    /// <item><description>The <see cref="name"/> of the slot.</description></item>
    /// <item><description>The <see cref="timeStarted"/>, stored as a binary timestamp.</description></item>
    /// <item><description>The <see cref="timeSpent"/> in the slot, measured in seconds.</description></item>
    /// <item><description>The <see cref="lastOpened"/> timestamp, stored as a binary value.</description></item>
    /// <item><description>The <see cref="completion"/> percentage of the slot, represented as a value between 0 and 1.</description></item>
    /// </list> 
    /// This class provides methods for updating and retrieving slot-related data, as well as resetting the slot to its initial state.
    /// </remarks>
    [System.Serializable]
    public class SlotData
    {
        public string name;
        public long timeStarted;
        public float timeSpent;
        public long lastOpened;
        [Range(0, 100)] public float completion;

        #region Constructor Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotData"/> class with default values.
        /// </summary>
        /// <remarks>
        /// The default values are: 
        /// <list type="bullet"> 
        /// <item><description>The <see cref="name"/> is set to "New Slot".</description></item> 
        /// <item><description>The <see cref="timeStarted"/> is set to the current system time in binary format.</description></item> 
        /// <item><description>The <see cref="timeSpent"/> is set to 0.</description></item>
        /// <item><description>The <see cref="completion"/> is set to 0.</description></item>
        /// </list>
        /// </remarks>
        /// <returns> A new <see cref="SlotData"/> instance initialized with default settings.</returns>
        public SlotData()
        {
            // Set the name to "New Slot"
            name = "New Slot";

            // Set the time started to the current system time
            timeStarted = System.DateTime.Now.ToBinary();

            // Set the time spent to 0
            timeSpent = 0;

            // Set the last opened time to the current system time
            lastOpened = timeStarted;

            // Set the completion to 0
            completion = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotData"/> class with default values and a specified name.
        /// </summary>
        /// <remarks>
        /// The default values are: 
        /// <list type="bullet"> 
        /// <item><description>The <see cref="name"/> is set to the provided name.</description></item> 
        /// <item><description>The <see cref="timeStarted"/> is set to the current system time in binary format.</description></item> 
        /// <item><description>The <see cref="timeSpent"/> is set to 0.</description></item>
        /// <item><description>The <see cref="completion"/> is set to 0.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name to associate with the new <see cref="SlotData"/> instance. Cannot be null or empty.</param>
        /// <returns> A new <see cref="SlotData"/> instance initialized with the specified name.</returns>
        public SlotData(string name)
        {
            // Set the name to the provided name
            this.name = name;

            // Set the time started to the current system time
            timeStarted = System.DateTime.Now.ToBinary();

            // Set the time spent to 0
            timeSpent = 0;

            // Set the last opened time to the current system time
            lastOpened = timeStarted;

            // Set the completion to 0
            completion = 0;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SlotData"/> with default values.
        /// </summary>
        /// <returns>A new <see cref="SlotData"/> instance initialized with default settings.</returns>
        public static SlotData Default() => new SlotData("New Slot");

        /// <summary>
        /// Creates a new instance of the <see cref="SlotData"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name to associate with the new <see cref="SlotData"/> instance. Cannot be null or empty.</param>
        /// <returns>A new <see cref="SlotData"/> instance initialized with the specified name.</returns>
        public static SlotData Create(string name) => new SlotData(name);

        #endregion

        #region Progress Methods

        /// <summary>
        /// Updates the time spent in the slot by adding the specified delta time.
        /// </summary>
        /// <remarks>This method is used to track the time spent within the slot, allowing for progress tracking.</remarks>
        /// <param name="deltaTime">The time increment, in seconds, to add to the accumulated time. Must be a non-negative value.</param>
        public void Update(float deltaTime) => timeSpent += deltaTime;

        /// <summary>
        /// Resets the state of the object to its initial values.
        /// </summary>
        /// <remarks>
        /// This method reinitializes the object's properties to their default values: 
        /// <list type="bullet"> 
        /// <item><description>The <see cref="name"/> is set to "New Slot". </description></item> 
        /// <item><description>The <see cref="timeStarted"/> is set to the current system time.</description></item>
        /// <item><description>The <see cref="timeSpent"/> is reset to 0.</description></item>
        /// <item><description>The <see cref="lastOpened"/> is reset to 0.</description></item>
        /// <item><description>The <see cref="completion"/> is reset to 0.</description></item> 
        /// </list> 
        /// Use this method to clear the current state and start fresh.
        /// </remarks>
        public void Reset()
        {
            // Reset the name to "New Slot"
            name = "New Slot";

            // Reset the time started to the current system time
            timeStarted = System.DateTime.Now.ToBinary();

            // Reset the time spent to 0
            timeSpent = 0;

            // Reset the last opened time to 0
            lastOpened = 0;

            // Reset the completion to 0
            completion = 0;
        }

        #endregion

        #region Set Methods

        /// <summary>
        /// Sets the name of the slot.
        /// </summary>
        /// <param name="name">The name to assign. Cannot be null or empty.</param>
        public void SetName(string name) => this.name = name;

        /// <summary>
        /// Sets the time spent in the slot.
        /// </summary>
        /// <param name="timeSpent">The amount of time, in seconds, to record. Must be a non-negative value.</param>
        public void SetTimeSpent(float timeSpent) => this.timeSpent = timeSpent;

        /// <summary>
        /// Sets the last opened time for the slot, stored as a binary timestamp.
        /// </summary>
        /// <param name="lastOpened">The binary timestamp representing the last opened time.</param>
        public void SetLastOpened(long lastOpened) => this.lastOpened = lastOpened;

        /// <summary>
        /// Sets the completion percentage, clamping the provided value between 0 and 100.
        /// </summary>
        /// <param name="completion">A value between 0 and 100 representing the completion ratio. Values outside this range will be clamped.</param>
        public void SetCompletion(float completion) => this.completion = Mathf.Clamp(completion, 0, 100);

        #endregion

        #region Get Methods

        /// <summary>
        /// Retrieves the name associated with the current instance.
        /// </summary>
        /// <returns>The name as a <see cref="string"/>. If no name is set, returns an empty string.</returns>
        public string GetName() => name;

        /// <summary>
        /// Gets the time at which the process or operation started, formatted as a string.
        /// </summary>
        /// <remarks>
        /// The returned value provides information about when the operation began. 
        /// Ensure that the <c>timeStarted</c> value is properly initialized before calling this method.
        /// </remarks>
        /// <returns>
        /// A string representing the start time. 
        /// The format and content of the string depend on the implementation of <see cref="SlotDataExtensions.ToTimeStamp"/>.
        /// </returns>
        public string GetTimeStarted() => SlotDataExtensions.ToTimeStamp(timeStarted);

        /// <summary>
        /// Converts the total time spent, in seconds, into a formatted string representation.
        /// </summary>
        /// <remarks>
        /// The returned string always uses two digits for each component, padding with leading zeros if necessary. 
        /// For example, a time span of 3661 seconds will be formatted as "01:01:01".
        /// </remarks>
        /// <returns>A string representing the time spent in the format "HH:mm:ss", where "HH" is hours, "mm" is minutes, and "ss" is seconds.</returns>
        public string GetTimeSpent() => SlotDataExtensions.GetTimeSpent(timeSpent);

        /// <summary>
        /// Gets the last opened timestamp for the current slot.
        /// </summary>
        /// <returns>A string representing the last opened timestamp. Returns an empty string if no timestamp is available.</returns>
        public string GetLastOpened() => SlotDataExtensions.ToTimeStamp(lastOpened);

        /// <summary>
        /// Calculates the completion percentage, clamped between 0 and 100, and rounds it to the specified number of decimal places.
        /// </summary>
        /// <param name="decimalPlaces">The number of decimal places to round the completion percentage to. Must be a non-negative integer. Defaults to 2.</param>
        /// <returns>The completion percentage as a float, clamped between 0 and 100, rounded to the specified number of decimal places.</returns>
        public float GetCompletion(int decimalPlaces = 2) => Mathf.Clamp(completion, 0f, 100f).SetDecimalPrecision(decimalPlaces);

        #endregion

        /// <summary>
        /// Returns a string representation of the current object, including its name, start time, time spent, and completion percentage.
        /// </summary>
        /// <returns>A string containing the object's name, the time it started, the time spent, and the completion percentage.</returns>
        public override string ToString() => $"Name: {GetName()}, Time Started: {GetTimeStarted()}, Time Spent: {GetTimeSpent()}, Last Opened: {GetLastOpened()}, Completion: {GetCompletion()}%";
    }

    #if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(SlotData))]
    public class SlotDataPropertyDrawer : PropertyDrawer
    {
        private int lineCount = 5;
        private float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height * lineCount;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start the change check
            EditorGUI.BeginChangeCheck();

            // Begin the property
            EditorGUI.BeginProperty(position, label, property);

            // Store the indentation level and set it to zero
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Get the properties
            var nameProperty = property.FindPropertyRelative("name");
            var timeStartedProperty = property.FindPropertyRelative("timeStarted");
            var timeSpentProperty = property.FindPropertyRelative("timeSpent");
            var lastOpenedProperty = property.FindPropertyRelative("lastOpened");
            var completionProperty = property.FindPropertyRelative("completion");

            // Draw the rects for the name and index properties side-by-side horizontally
            Rect nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Calculate the rects for the time started, time spent, and completion properties
            Rect timeStartedRect = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
            Rect timeSpentRect = new Rect(position.x, position.y + height * 2, position.width, EditorGUIUtility.singleLineHeight);
            Rect lastOpenedRect = new Rect(position.x, position.y + height * 3, position.width, EditorGUIUtility.singleLineHeight);
            Rect completionRect = new Rect(position.x, position.y + height * 4, position.width, EditorGUIUtility.singleLineHeight);

            // Draw the name property
            EditorGUI.PropertyField(nameRect, nameProperty, new GUIContent("Profile", "The name and id of the slot"));

            // Draw the time started property
            EditorGUI.TextField(timeStartedRect, new GUIContent("Time Started", "The time when the slot was started"), SlotDataExtensions.ToTimeStamp(timeStartedProperty.longValue));

            // Draw the time spent property
            EditorGUI.TextField(timeSpentRect, new GUIContent("Time Spent", "The time spent in the slot"), SlotDataExtensions.GetTimeSpent(timeSpentProperty.floatValue));

            // Draw the last opened property
            EditorGUI.TextField(lastOpenedRect, new GUIContent("Last Opened", "The last time the slot was opened"), SlotDataExtensions.ToTimeStamp(lastOpenedProperty.longValue));

            // Draw the completion property
            EditorGUI.PropertyField(completionRect, completionProperty, new GUIContent("Completion", "The completion status of the slot"));

            // Restore the indentation level
            EditorGUI.indentLevel = indent;

            // End the change check
            if (EditorGUI.EndChangeCheck())
            {
                // Apply the modified properties to ensure changes are saved
                property.serializedObject.ApplyModifiedProperties();
            }

            // End the property
            EditorGUI.EndProperty();
        }
    }

    #endif
}