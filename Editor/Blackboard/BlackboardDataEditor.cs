using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Sanctuary.Blackboard;

namespace Sanctuary.Editor
{
    [CustomEditor(typeof(BlackboardData))]
    public class BlackboardDataEditor : UnityEditor.Editor
    {
        ReorderableList entryList;

        void OnEnable()
        {
            // Gap between fields
            float gap = 2;

            // Create a ReorderableList for the entries property
            entryList = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(BlackboardData.entries)), true, true, true, true)
            {
                // Define how the header of the list should be drawn
                drawHeaderCallback = rect => 
                {
                    // Draw the header labels for the list
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight), "Type");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.2f + 10 + gap, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), "Key");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.5f + 5 + gap, rect.y, (rect.width * 0.5f), EditorGUIUtility.singleLineHeight), "Value");
                }
            };

            // Define how each element in the list should be drawn
            entryList.drawElementCallback = (rect, index, isActive, isFocused) => 
            {
                // Get the element at the current index
                var element = entryList.serializedProperty.GetArrayElementAtIndex(index);

                // Adjust the rect for better spacing
                rect.y += 2;

                // Get the properties of the element
                var valueType = element.FindPropertyRelative(nameof(BlackboardEntryData.valueType));
                var keyName = element.FindPropertyRelative(nameof(BlackboardEntryData.keyName));
                var value = element.FindPropertyRelative(nameof(BlackboardEntryData.value));

                // Draw the properties
                var valueTypeRect = new Rect(rect.x, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
                var keyNameRect = new Rect(rect.x + rect.width * 0.2f + gap, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(rect.x + rect.width * 0.5f + gap * 2, rect.y, (rect.width * 0.5f) - gap, EditorGUIUtility.singleLineHeight);

                // Draw the value type and key name fields
                EditorGUI.PropertyField(valueTypeRect, valueType, GUIContent.none);
                EditorGUI.PropertyField(keyNameRect, keyName, GUIContent.none);

                // Draw the value field based on the selected value type
                switch ((AnyValue.ValueType)valueType.enumValueIndex)
                {
                    case AnyValue.ValueType.Int:
                        var intValue = value.FindPropertyRelative(nameof(AnyValue.intValue));
                        EditorGUI.PropertyField(valueRect, intValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Float:
                        var floatValue = value.FindPropertyRelative(nameof(AnyValue.floatValue));
                        EditorGUI.PropertyField(valueRect, floatValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Bool:
                        var boolValue = value.FindPropertyRelative(nameof(AnyValue.boolValue));
                        EditorGUI.PropertyField(valueRect, boolValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.String:
                        var strValue = value.FindPropertyRelative(nameof(AnyValue.stringValue));
                        EditorGUI.PropertyField(valueRect, strValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Vector3:
                        var vec3Value = value.FindPropertyRelative(nameof(AnyValue.vector3Value));
                        EditorGUI.PropertyField(valueRect, vec3Value, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Object:
                        var objectValue = value.FindPropertyRelative(nameof(AnyValue.objectValue));
                        EditorGUI.PropertyField(valueRect, objectValue, GUIContent.none);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            entryList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
