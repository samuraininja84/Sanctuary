using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Sanctuary.Attributes;

namespace Sanctuary.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypeDrawer : PropertyDrawer 
    {
        TypeFilterAttribute typeFilter;
        // SerializableTypeDropdown dropdown;
        string[] typeNames, typeFullNames;

        private void Initialize() 
        {
            // If the type names are already initialized, return
            if (typeFullNames != null) return;

            // Get the field info of the property this attribute is attached to
            typeFilter = (TypeFilterAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(TypeFilterAttribute));

            // Get all the types in the current domain
            var filteredTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeFilter == null ? DefaultFilter(t) : typeFilter.Filter(t))
                .ToArray();

            // Get the names and full names of the filtered types
            typeNames = filteredTypes.Select(t => t.ReflectedType == null ? t.Name : $"t.ReflectedType.Name + t.Name").ToArray();
            typeFullNames = filteredTypes.Select(t => t.AssemblyQualifiedName).ToArray();
        }
        
        private static bool DefaultFilter(Type type) 
        {
            // Check if the type is not abstract, not an interface and not a generic type
            return !type.IsAbstract && !type.IsInterface && !type.IsGenericType;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            // Get the changes made to the property
            EditorGUI.BeginChangeCheck();

            // Initialize the type names and full names
            Initialize();

            // Get the assemblyQualifiedName property of the property
            var typeIdProperty = property.FindPropertyRelative("assemblyQualifiedName");

            // If the type id property is empty, set it to the first type full name
            if (string.IsNullOrEmpty(typeIdProperty.stringValue)) 
            {
                typeIdProperty.stringValue = typeFullNames.First();
                property.serializedObject.ApplyModifiedProperties();
            }

            // Get the index of the current type in the type full names array
            var currentIndex = Array.IndexOf(typeFullNames, typeIdProperty.stringValue);
            var selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames);

            // If the selected index is valid and different from the current index, set the type id property to the selected type full name
            if (selectedIndex >= 0 && selectedIndex != currentIndex) 
            {
                typeIdProperty.stringValue = typeFullNames[selectedIndex];
                property.serializedObject.ApplyModifiedProperties();
            }

            // If the user made changes to the property, apply the modified properties
            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();
        }
    }

    public class SerializableTypeDropdown : AdvancedDropdown
    {
        private string[] names;

        public SerializableTypeDropdown(AdvancedDropdownState state, string[] names) : base(state) { this.names = names; }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Types");
            foreach (var name in names)
            {
                root.AddChild(new AdvancedDropdownItem(name));
            }

            return root;
        }
    }
}
