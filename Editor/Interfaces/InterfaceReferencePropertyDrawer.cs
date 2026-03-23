using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Sanctuary.Editor
{
    /// <summary>
    /// Provides a custom property drawer for fields of type <see cref="InterfaceReference{T}"/> and <see
    /// cref="InterfaceReference{T1, T2}"/>.
    /// </summary>
    /// <remarks>This property drawer enables Unity's Inspector to display and edit fields that reference
    /// objects implementing specific interfaces. It ensures that assigned objects meet the required interface
    /// constraints and provides validation feedback.</remarks>
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferencePropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Represents the name of the field used to store the underlying value in a data structure or object.
        /// </summary>
        const string UnderlyingValueFieldName = "underlyingValue";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Retrieve the underlying property that holds the actual object reference from the serialized property.
            var underlyingProperty = property.FindPropertyRelative(UnderlyingValueFieldName);

            // Get the arguments for the interface reference, including the object type and interface type.
            var args = GetArguments(fieldInfo);

            // Set the height of the position rectangle to accommodate a single line height for the property.
            position.height = EditorGUIUtility.singleLineHeight;

            // Begin a property GUI block to handle the drawing of the property in the editor.
            EditorGUI.BeginProperty(position, label, property);

            // Draw the object field for the interface reference, allowing assignment of an object that implements the specified interface.
            var assignedObject = EditorGUI.ObjectField(position, label, underlyingProperty.objectReferenceValue, args.ObjectType, true);

            // If an object is assigned, check if it implements the required interface and assign it accordingly.
            if (assignedObject != null)
            {
                // Initialize a variable to hold the component that implements the interface.
                Object component = null;

                // Check if the assigned object is a GameObject or if it directly implements the interface.
                if (assignedObject is GameObject gameObject)
                {
                    component = gameObject.GetComponent(args.InterfaceType);
                }
                else if (args.InterfaceType.IsAssignableFrom(assignedObject.GetType()))
                {
                    component = assignedObject;
                }

                // If a component implementing the interface is found, validate and assign it to the underlying property.
                if (component != null)
                {
                    ValidateAndAssignObject(underlyingProperty, component, component.name, args.InterfaceType.Name);
                }
                else
                {
                    Debug.LogWarning($"Assigned object does not implement required interface '{args.InterfaceType.Name}'.");
                    underlyingProperty.objectReferenceValue = null;
                }
            }
            else
            {
                underlyingProperty.objectReferenceValue = null;
            }

            // End the property GUI block to ensure proper rendering and interaction handling.
            EditorGUI.EndProperty();

            // Call the utility method to handle additional GUI rendering for the interface reference.
            InterfaceReferenceUtility.OnGUI(position, underlyingProperty, label, args);
        }

        /// <summary>
        /// Extracts object and interface types from a given field's type.
        /// </summary>
        /// <remarks>This method identifies object and interface types based on specific patterns in the
        /// field's type: <list type="bullet"> <item> If the field's type is a generic type matching <see
        /// cref="InterfaceReference{T}"/> or <see cref="InterfaceReference{T1, T2}"/>, the object and interface types
        /// are extracted from the generic arguments. </item> <item> If the field's type implements <see
        /// cref="IList{T}"/>, the method attempts to extract the object and interface types from the element type of
        /// the list. </item> </list> If neither pattern is matched, the returned <see cref="InterfaceArgs"/> will
        /// contain <see langword="null"/> values.</remarks>
        /// <param name="fieldInfo">The metadata information of the field whose type is analyzed.</param>
        /// <returns>
        /// An <see cref="InterfaceArgs"/> instance containing the object type and interface type derived from the field's type. 
        /// If the field's type does not match the expected patterns, both types in the returned <see cref="InterfaceArgs"/> will be <see langword="null"/>.
        /// </returns>
        private static InterfaceArgs GetArguments(FieldInfo fieldInfo)
        {
            // Initialize the object and interface types to null.
            Type objectType = null, interfaceType = null;
            Type fieldType = fieldInfo.FieldType;

            // Helper method to extract types from InterfaceReference or InterfaceReference<T1, T2>.
            bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;

                if (type?.IsGenericType != true) return false;

                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;

                if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    var types = type.GetGenericArguments();
                    intfType = types[0];
                    objType = types[1];
                    return true;
                }

                return false;
            }

            // Helper method to extract types from IList<T> interface.
            void GetTypesFromList(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;

                var listInterface = type.GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));

                if (listInterface != null)
                {
                    var elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
                }
            }

            // Try to get types from InterfaceReference or InterfaceReference<T1, T2>.
            if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }

            // If we still don't have types, return nulls.
            return new InterfaceArgs(objectType, interfaceType);
        }

        /// <summary>
        /// Validates the specified target object and assigns it to the given serialized property.
        /// </summary>
        /// <remarks>If <paramref name="targetObject"/> is not null, it is assigned to <paramref
        /// name="property"/>. If <paramref name="targetObject"/> is null, a warning is logged indicating that the
        /// object does not implement the specified interface (if provided), and <paramref name="property"/> is set to
        /// null.</remarks>
        /// <param name="property">The serialized property to which the object reference will be assigned.</param>
        /// <param name="targetObject">The object to validate and assign. If null, a warning will be logged and the property will be set to null.</param>
        /// <param name="componentNameOrType">The name or type of the component associated with the target object, used for logging purposes.</param>
        /// <param name="interfaceName">The name of the interface that the target object is expected to implement, used for logging purposes. Can be null.</param>
        private static void ValidateAndAssignObject(SerializedProperty property, Object targetObject, string componentNameOrType, string interfaceName = null)
        {
            if (targetObject != null)
            {
                property.objectReferenceValue = targetObject;
            }
            else
            {
                var message = interfaceName != null
                    ? $"GameObject '{componentNameOrType}'"
                    : "assigned object";

                Debug.LogWarning(
                    $"The {message} does not have a component that implements '{interfaceName}'."
                );
                property.objectReferenceValue = null;
            }
        }
    }

    /// <summary>
    /// Represents the association between an object type and an interface type.
    /// </summary>
    /// <remarks>This struct is used to define a relationship between a specific object type and an interface
    /// type. The <see cref="ObjectType"/> must be a type that derives from <see cref="Object"/>, and the  <see
    /// cref="InterfaceType"/> must be an interface.</remarks>
    public struct InterfaceArgs
    {
        /// <summary>
        /// Gets the <see cref="Type"/> of the object represented by this instance.
        /// </summary>
        public readonly Type ObjectType;

        /// <summary>
        /// Gets the type of the interface represented by this instance.
        /// </summary>
        public readonly Type InterfaceType;

        public InterfaceArgs(Type objectType, Type interfaceType)
        {
            // Debug assertions to ensure the types are valid.
            Debug.Assert(typeof(Object).IsAssignableFrom(objectType), $"{nameof(objectType)} needs to be of Type {typeof(Object)}.");
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");

            // Assign the types to the properties.
            ObjectType = objectType;
            InterfaceType = interfaceType;
        }
    }
}