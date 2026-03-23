using System;
using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// Represents a serializable wrapper for a <see cref="Type"/> object, enabling type resolution and serialization.
    /// </summary>
    /// <remarks>This class is designed to facilitate the serialization and deserialization of <see
    /// cref="Type"/> objects by storing their assembly-qualified names. It implements <see
    /// cref="ISerializationCallbackReceiver"/> to handle pre-serialization and post-deserialization processing.
    /// Additionally, it provides utility methods for type resolution and supports implicit conversions between <see
    /// cref="SerializableType"/> and <see cref="Type"/>.</remarks>
    [Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        /// <summary>
        /// The assembly-qualified name of a type.
        /// </summary>
        /// <remarks>This string represents the fully qualified name of a type, including its namespace
        /// and assembly information. It is typically used for type resolution or serialization purposes.</remarks>
        [SerializeField] string assemblyQualifiedName = string.Empty;

        /// <summary>
        /// Gets the type of the object represented by this instance.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Prepares the object for serialization by storing the assembly-qualified name of the associated <see
        /// cref="Type"/>.
        /// </summary>
        /// <remarks>This method is called automatically during the serialization process. It ensures that
        /// the assembly-qualified name of the <see cref="Type"/> is preserved, allowing the type to be correctly
        /// resolved during deserialization.</remarks>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Before serialization, we store the assembly-qualified name of the Type.
            assemblyQualifiedName = Type?.AssemblyQualifiedName ?? assemblyQualifiedName;
        }

        /// <summary>
        /// Invoked after the object has been deserialized to perform post-deserialization processing.
        /// </summary>
        /// <remarks>This method ensures that the <see cref="Type"/> property is correctly set based on
        /// the deserialized <c>assemblyQualifiedName</c>. If the type cannot be resolved, the
        /// <c>assemblyQualifiedName</c> defaults to <c>System.Object</c>, and an error message is logged.</remarks>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // If the assemblyQualifiedName is empty, we default to System.Object.
            if (!TryGetType(assemblyQualifiedName, out var type))
            {
                if (assemblyQualifiedName == string.Empty) assemblyQualifiedName = "System.Object";
                Debug.LogError($"Type {assemblyQualifiedName} not found");
                return;
            }

            // If the type is found, set the Type property to the retrieved type.
            Type = type;
        }

        /// <summary>
        /// Attempts to retrieve a <see cref="Type"/> object based on the specified type name.
        /// </summary>
        /// <remarks>This method does not throw an exception if the type cannot be found. Instead, it
        /// returns <see langword="false"/> and sets <paramref name="type"/> to <see langword="null"/>.</remarks>
        /// <param name="typeString">The fully qualified name of the type to retrieve. This cannot be null or empty.</param>
        /// <param name="type">When this method returns, contains the <see cref="Type"/> object corresponding to <paramref
        /// name="typeString"/>, if the type was found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the type was successfully retrieved or <paramref name="typeString"/> is not empty;
        /// otherwise, <see langword="false"/>.</returns>
        static bool TryGetType(string typeString, out Type type)
        {
            type = Type.GetType(typeString);
            return type != null || !string.IsNullOrEmpty(typeString);
        }

        /// <summary>
        /// Creates a <see cref="SerializableType"/> instance that represents the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to be wrapped in a <see cref="SerializableType"/> instance.</param>
        /// <returns>A <see cref="SerializableType"/> instance that encapsulates the provided <see cref="Type"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <see langword="null"/>.</exception>
        public static SerializableType FromType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Type cannot be null.");
            return new SerializableType(type);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableType"/> class with the specified type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to be associated with this instance. Cannot be <see langword="null"/>.</param>
        public SerializableType(Type type) => Type = type;

        // Implicit conversion from SerializableType to Type
        public static implicit operator Type(SerializableType sType) => sType.Type;

        // Implicit conversion from Type to SerializableType
        public static implicit operator SerializableType(Type type) => FromType(type);
    }
}
