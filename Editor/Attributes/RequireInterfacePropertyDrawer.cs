using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sanctuary.Attributes;

using Object = UnityEngine.Object;

namespace Sanctuary.Editor
{
    /// <summary>
    /// Provides a custom property drawer for fields marked with the <see cref="RequireInterfaceAttribute"/>.
    /// </summary>
    /// <remarks>
    /// This property drawer ensures that object references assigned to the field implement the required interface specified by the <see cref="RequireInterfaceAttribute"/>. 
    /// It supports both single object references and arrays of objects, validating each element against the required interface type.
    /// </remarks>
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfacePropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Gets the <see cref="RequireInterfaceAttribute"/> associated with the current context.
        /// </summary>
        RequireInterfaceAttribute RequireInterfaceAttribute => (RequireInterfaceAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the required interface type from the attribute
            Type requiredInterfaceType = RequireInterfaceAttribute.InterfaceType;

            // Begin the property GUI to ensure proper handling of the serialized property
            EditorGUI.BeginProperty(position, label, property);

            // Draw the property field based on whether it is an array or a single object reference
            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
            {
                // Draw the array field with interface type validation
                DrawArrayField(position, property, label, requiredInterfaceType);
            }
            else
            {
                // Draw a single object field with interface type validation
                DrawInterfaceObjectField(position, property, label, requiredInterfaceType);
            }

            // End the property GUI to ensure proper handling of the serialized property
            EditorGUI.EndProperty();

            // Get the arguments for the interface reference utility
            var args = new InterfaceArgs(GetTypeOrElementType(fieldInfo.FieldType), requiredInterfaceType);

            // Call the utility method to handle additional GUI elements or functionality related to interface references
            InterfaceReferenceUtility.OnGUI(position, property, label, args);
        }

        /// <summary>
        /// Draws a serialized array field in the Unity Editor, allowing the user to modify the array size and edit
        /// individual elements.
        /// </summary>
        /// <remarks>This method renders an editable array field in the Unity Editor, enabling users to
        /// adjust the array size and modify individual elements. Each element is drawn using the <see
        /// cref="DrawInterfaceObjectField"/> method, ensuring compatibility with the specified <paramref
        /// name="interfaceType"/>.</remarks>
        /// <param name="position">The position and size of the field to be drawn in the editor.</param>
        /// <param name="property">The serialized property representing the array to be drawn.</param>
        /// <param name="label">The label displayed alongside the array field.</param>
        /// <param name="interfaceType">The type of interface that the array elements must implement, used for validation and display.</param>
        private void DrawArrayField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
        {
            property.arraySize = EditorGUI.IntField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                label.text + " Size", property.arraySize);

            float yOffset = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                var elementRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                DrawInterfaceObjectField(elementRect, element, new GUIContent($"Element {i}"), interfaceType);
                yOffset += EditorGUIUtility.singleLineHeight;
            }
        }

        /// <summary>
        /// Draws a custom object field in the Unity Editor that restricts the selectable objects to those  implementing
        /// a specified interface type.
        /// </summary>
        /// <remarks>This method ensures that the object field only allows selection of objects that are
        /// assignable  to the specified <paramref name="interfaceType"/>. If the selected object does not meet the 
        /// criteria, it will not be assigned to the property.</remarks>
        /// <param name="position">The position and size of the object field in the editor UI.</param>
        /// <param name="property">The serialized property representing the object reference to be edited.</param>
        /// <param name="label">The label displayed next to the object field.</param>
        /// <param name="interfaceType">The interface type that the selectable objects must implement.</param>
        private void DrawInterfaceObjectField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
        {
            // Ensure the property is of type ObjectReference
            var oldReference = property.objectReferenceValue;

            // Check if the field type is assignable to the interface type
            Type baseType = GetAssignableBaseType(fieldInfo.FieldType, interfaceType);

            // Get the type of the object field, which should be assignable to the interface type
            var newReference = EditorGUI.ObjectField(position, label, oldReference, baseType, true);

            // If the new reference is not null and is different from the old reference, validate and assign it
            if (newReference != null && newReference != oldReference)
            {
                ValidateAndAssignObject(property, newReference, interfaceType);
            }
            else if (newReference == null)
            {
                property.objectReferenceValue = null;
            }
        }

        /// <summary>
        /// Determines the most appropriate base type that is assignable to a specified interface type from the given
        /// field type.
        /// </summary>
        /// <remarks>This method is particularly useful in scenarios where the field type may represent
        /// collections or Unity-specific types, and a compatible base type needs to be determined for further
        /// processing.</remarks>
        /// <param name="fieldType">The type of the field to evaluate. This can be an array type, a generic type, or a concrete type.</param>
        /// <param name="interfaceType">The interface type to check for assignability.</param>
        /// <returns>The base type that is assignable to <paramref name="interfaceType"/>. 
        /// If the field type is an array or generic type, the element type or generic argument is evaluated. 
        /// If no assignable type is found, a fallback type such as  <see cref="ScriptableObject"/>, <see cref="MonoBehaviour"/>, or <see cref="Object"/> is returned.
        /// </returns>
        private Type GetAssignableBaseType(Type fieldType, Type interfaceType)
        {
            // Check if the field type is null or not assignable to the interface type
            Type elementType = fieldType.IsArray ? fieldType.GetElementType() :
                fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)
                    ? fieldType.GetGenericArguments()[0]
                    : fieldType;

            // If the field type is already the interface type, return it directly
            if (interfaceType.IsAssignableFrom(elementType)) return elementType;

            // Check for common Unity types that might be used in the context of interfaces
            if (typeof(ScriptableObject).IsAssignableFrom(elementType)) return typeof(ScriptableObject);
            if (typeof(MonoBehaviour).IsAssignableFrom(elementType)) return typeof(MonoBehaviour);

            // If the type is not assignable to the interface, return Object as a fallback
            return typeof(Object);
        }

        /// <summary>
        /// Validates whether the provided object implements the specified interface type and assigns it to the given
        /// serialized property.
        /// </summary>
        /// <remarks>If the provided object is a <see cref="GameObject"/>, the method attempts to retrieve
        /// a component from the GameObject that implements the specified interface type. If the object directly
        /// implements the interface type, it is assigned to the serialized property. If the object does not implement
        /// the interface type, a warning is logged, and the serialized property is set to <see
        /// langword="null"/>.</remarks>
        /// <param name="property">The serialized property to which the validated object reference will be assigned.</param>
        /// <param name="newReference">The object reference to validate and assign.</param>
        /// <param name="interfaceType">The interface type that the object must implement.</param>
        private void ValidateAndAssignObject(SerializedProperty property, Object newReference, Type interfaceType)
        {
            // Check if the new reference is a null or if it already matches the property value
            if (newReference is GameObject gameObject)
            {
                var component = gameObject.GetComponent(interfaceType);
                if (component != null)
                {
                    property.objectReferenceValue = component;
                    return;
                }
            }
            else if (interfaceType.IsAssignableFrom(newReference.GetType()))
            {
                property.objectReferenceValue = newReference;
                return;
            }

            // If the object does not implement the required interface, log a warning
            Debug.LogWarning($"The assigned object does not implement '{interfaceType.Name}'.");

            // If the object does not implement the required interface, set the property to null
            property.objectReferenceValue = null;
        }

        /// <summary>
        /// Retrieves the type itself or the type of its element if the provided type represents an array or a generic
        /// type.
        /// </summary>
        /// <param name="type">The type to evaluate. Must not be <see langword="null"/>.</param>
        /// <returns>The element type if <paramref name="type"/> is an array, the first generic argument if <paramref
        /// name="type"/> is a generic type,  or the type itself if neither condition applies.</returns>
        private Type GetTypeOrElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return type;
        }
    }
}