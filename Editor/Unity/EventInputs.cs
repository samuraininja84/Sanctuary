using UnityEngine;

namespace Sanctuary.Editor
{
    public static class EventInputs
    {
        public static bool Copy(bool use = false) => KeyPress(EventType.KeyDown, KeyCode.C, use);
        public static bool Paste(bool use = false) => KeyPress(EventType.KeyDown, KeyCode.V, use);
        public static bool SpaceDown(bool use = false) => KeyPress(EventType.KeyDown, KeyCode.Space, use);
        public static bool DeleteDown(bool use = false) => KeyPress(EventType.KeyDown, KeyCode.Delete, use);
        public static bool EscapeDown(bool use = false) => KeyPress(EventType.KeyDown, KeyCode.Escape, use);
        public static bool EscapeUp(bool use = false) => KeyPress(EventType.KeyUp, KeyCode.Escape, use);
        public static bool LeftShift() => Event.current.keyCode == KeyCode.LeftShift;
        public static bool RightShift() => Event.current.keyCode == KeyCode.RightShift;

        public static bool KeyPress(EventType type, KeyCode keyCode, bool use = false)
        {
            // Get the current event
            Event key = Event.current;

            // Check if the event type and key code match
            if (key.type == type && key.keyCode == keyCode)
            {
                // Use the event to prevent further processing
                if (use) key.Use();

                // Return true if the key press matches
                return true;
            }

            // Return false if the key press does not match
            return false;
        }

        public static bool MouseLeft(this EventType eventType, bool use = false) => eventType.MousePress(0, use);
        public static bool MouseRight(this EventType eventType, bool use = false) => eventType.MousePress(1, use);
        public static bool MouseMiddle(this EventType eventType, bool use = false) => eventType.MousePress(2, use);

        private static bool MousePress(this EventType type, int button, bool use = false)
        {
            // Get the current event
            Event key = Event.current;

            // Check if the event type and mouse button match
            if (key.rawType == type && key.button == button)
            {
                // Use the event to prevent further processing
                if (use) key.Use();

                // Return true if the mouse press matches
                return true;
            }

            // Return false if the mouse press does not match
            return false;
        }

        public static bool ScrollUp(bool use = false)
        {
            // Get the current event
            Event key = Event.current;

            // Check if the event is a scroll wheel event
            if (key.type == EventType.ScrollWheel && key.delta.y < 0)
            {
                // Use the event to prevent further processing
                if (use) key.Use();

                // Return true if the scroll up event matches
                return true;
            }

            // Return false if the scroll up event does not match
            return false;
        }
        public static bool ScrollDown(bool use = false)
        {
            // Get the current event
            Event key = Event.current;

            // Check if the event is a scroll wheel event
            if (key.type == EventType.ScrollWheel && key.delta.y > 0)
            {
                // Use the event to prevent further processing
                if (use) key.Use();

                // Return true if the scroll down event matches
                return true;
            }
            // Return false if the scroll down event does not match
            return false;
        }
    }
}