using UnityEngine;
using UnityEditor;

namespace Sanctuary.Editor
{
    /// <summary>
    /// Provides utility methods for rendering interface references in custom Unity editor GUIs.
    /// </summary>
    /// <remarks>
    /// This class is designed to assist with displaying and interacting with interface references in Unity's editor environment. 
    /// It includes methods for rendering labels, handling GUI events, and customizing label styles. 
    /// Use this class in the context of custom property drawers or editor scripts.
    /// </remarks>
    public class InterfaceReferenceUtility
    {
        /// <summary>
        /// Represents the style settings for labels in the graphical user interface.
        /// </summary>
        /// <remarks>
        /// This static field can be used to customize the appearance of labels, such as font, color, alignment, and other style properties. 
        /// Ensure that the field is initialized before use to avoid runtime errors.
        /// </remarks>
        private static GUIStyle labelStyle;

        public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, InterfaceArgs args)
        {
            // Initialize the label style if it hasn't been initialized yet.
            InitializeStyleIfNeeded();

            // Get the control ID for the interface reference label.
            var controlID = GUIUtility.GetControlID(FocusType.Passive) - 1;

            // Check if the mouse is hovering over the position of the interface reference label.
            var isHovering = position.Contains(Event.current.mousePosition);

            // Display the label for the interface reference.
            var displayString = property.objectReferenceValue == null || isHovering ? $"({args.InterfaceType.Name})" : "*";

            // Draw the interface name label with the specified position, display string, and control ID.
            DrawInterfaceNameLabel(position, displayString, controlID);
        }

        /// <summary>
        /// Draws a label displaying the name of an interface within the specified rectangular area.
        /// </summary>
        /// <remarks>This method is intended to be used in the context of custom editor GUI rendering
        /// within Unity. The label is drawn only during the <see cref="EventType.Repaint"/> event.</remarks>
        /// <param name="position">The <see cref="Rect"/> defining the area where the label should be drawn.</param>
        /// <param name="displayString">The text to display in the label, typically the name of the interface.</param>
        /// <param name="controlID">The unique control ID associated with the label, used for handling drag-and-drop events.</param>
        private static void DrawInterfaceNameLabel(Rect position, string displayString, int controlID)
        {
            // Repaint event is used to draw the label, ensuring it only draws when necessary.
            if (Event.current.type == EventType.Repaint)
            {
                // Initialize the label style settings if it hasn't been initialized yet.
                const int additionalLeftWidth = 3;
                const int verticalIndent = 1;

                // Set the label style if it hasn't been initialized yet.
                var content = EditorGUIUtility.TrTextContent(displayString);
                var size = labelStyle.CalcSize(content);
                var labelPos = position;

                // Adjust the label position and size based on the calculated size and additional width.
                labelPos.width = size.x + additionalLeftWidth;
                labelPos.x += position.width - labelPos.width - 18;
                labelPos.height -= verticalIndent * 2;
                labelPos.y += verticalIndent;

                // Draw the label using the custom label style, ensuring it responds to drag-and-drop events.
                labelStyle.Draw(labelPos, EditorGUIUtility.TrTextContent(displayString), controlID, DragAndDrop.activeControlID == controlID, false);
            }
        }

        /// <summary>
        /// Initializes the label style if it has not already been initialized.
        /// </summary>
        /// <remarks>This method creates a new <see cref="GUIStyle"/> based on <see
        /// cref="EditorStyles.label"/>  and applies customizations such as font, font size, font style, text alignment,
        /// and padding. The resulting style is stored in a static field for reuse.</remarks>
        private static void InitializeStyleIfNeeded()
        {
            // If the labelStyle is already initialized, return early.
            if (labelStyle != null) return;

            // Create a new GUIStyle based on the EditorStyles.label style.
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.font = EditorStyles.objectField.font;
            style.fontSize = EditorStyles.objectField.fontSize;
            style.fontStyle = EditorStyles.objectField.fontStyle;
            style.alignment = TextAnchor.MiddleRight;
            style.padding = new RectOffset(0, 2, 0, 0);

            // Set the style to be bold and use the same text color as the object field label.
            labelStyle = style;
        }
    }
}