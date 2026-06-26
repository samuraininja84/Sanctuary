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

            entryList = new ReorderableList(serializedObject, serializedObject.FindProperty("entries"), true, true, true, true)
            {
                drawHeaderCallback = rect => 
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight), "Type");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.2f + 10 + gap, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), "Key");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.5f + 5 + gap, rect.y, (rect.width * 0.5f), EditorGUIUtility.singleLineHeight), "Value");
                }
            };

            entryList.drawElementCallback = (rect, index, isActive, isFocused) => 
            {
                // Get the element at the current index
                var element = entryList.serializedProperty.GetArrayElementAtIndex(index);

                // Adjust the rect for better spacing
                rect.y += 2;

                // Get the properties of the element
                var valueType = element.FindPropertyRelative("valueType");
                var keyName = element.FindPropertyRelative("keyName");
                var value = element.FindPropertyRelative("value");

                // Draw the properties
                var valueTypeRect = new Rect(rect.x, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
                var keyNameRect = new Rect(rect.x + rect.width * 0.2f + gap, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(rect.x + rect.width * 0.5f + gap * 2, rect.y, (rect.width * 0.5f) - gap, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(valueTypeRect, valueType, GUIContent.none);
                EditorGUI.PropertyField(keyNameRect, keyName, GUIContent.none);

                switch ((AnyValue.ValueType)valueType.enumValueIndex)
                {
                    case AnyValue.ValueType.Int:
                        var intValue = value.FindPropertyRelative("intValue");
                        EditorGUI.PropertyField(valueRect, intValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Float:
                        var floatValue = value.FindPropertyRelative("floatValue");
                        EditorGUI.PropertyField(valueRect, floatValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Bool:
                        var boolValue = value.FindPropertyRelative("boolValue");
                        EditorGUI.PropertyField(valueRect, boolValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.String:
                        var strValue = value.FindPropertyRelative("stringValue");
                        EditorGUI.PropertyField(valueRect, strValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Vector3:
                        var vec3Value = value.FindPropertyRelative("vector3Value");
                        EditorGUI.PropertyField(valueRect, vec3Value, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Object:
                        var objectValue = value.FindPropertyRelative("objectValue");
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
