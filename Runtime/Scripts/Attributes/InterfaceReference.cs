using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sanctuary
{
    /// <summary>
    /// A reference to an object that implements a specific interface and is of a specific object type.
    /// </summary>
    /// <remarks>
    /// Serializes the underlying object, and provides a public property to access and set the interface value.
    /// </remarks>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TObject"></typeparam>
    [Serializable]
    public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
    {
        /// <summary>
        /// Represents the underlying value stored in the object.
        /// </summary>
        /// <remarks>This field is serialized but hidden in the inspector. It is intended for internal use
        /// and should not be accessed directly.</remarks>
        [SerializeField, HideInInspector] TObject underlyingValue;

        /// <summary>
        /// The interface value of the reference.
        /// </summary>
        public TInterface Value
        {
            get => underlyingValue switch
            {
                null => null,
                TInterface @interface => @interface,
                _ => throw new InvalidOperationException($"{underlyingValue} needs to implement interface {nameof(TInterface)}.")
            };
            set => underlyingValue = value switch
            {
                null => null,
                TObject newValue => newValue,
                _ => throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.", string.Empty)
            };
        }

        /// <summary>
        /// The underlying value of the reference.
        /// </summary>
        public TObject UnderlyingValue
        {
            get => underlyingValue;
            set => underlyingValue = value;
        }

        /// <summary>
        /// Gets a value indicating whether the underlying value is non-null and implements the specified interface.
        /// </summary>
        public virtual bool HasValue => underlyingValue != null && (underlyingValue as TInterface) != null;

        /// <summary>
        /// Constructs an empty InterfaceReference.
        /// </summary>
        public InterfaceReference() { }

        /// <summary>
        /// Constructs an InterfaceReference from the underlying type.
        /// </summary>
        /// <param name="target"></param>
        public InterfaceReference(TObject target) => underlyingValue = target;

        /// <summary>
        /// Constructs an InterfaceReference from an interface.
        /// </summary>
        /// <param name="interface"></param>
        public InterfaceReference(TInterface @interface) => underlyingValue = @interface as TObject;

        /// <summary>
        /// Gets a default instance of <see cref="InterfaceReference{TInterface, TObject}"/> representing no reference.
        /// </summary>
        public static InterfaceReference<TInterface, TObject> None => new InterfaceReference<TInterface, TObject>();

        /// <summary>
        /// Creates a new <see cref="InterfaceReference{TInterface, TObject}"/> instance for the specified target
        /// object.
        /// </summary>
        /// <param name="target">The object to associate with the interface reference. Cannot be null.</param>
        /// <returns>
        /// An <see cref="InterfaceReference{TInterface, TObject}"/> that represents the association between the specified interface and target object.
        /// </returns>
        public static InterfaceReference<TInterface, TObject> Some(TObject target) => new InterfaceReference<TInterface, TObject>(target);

        /// <summary>
        /// Creates a new instance of <see cref="InterfaceReference{TInterface, TObject}"/> using the specified
        /// interface.
        /// </summary>
        /// <param name="interface">The interface instance to reference. Cannot be null.</param>
        /// <returns>
        /// An <see cref="InterfaceReference{TInterface, TObject}"/> that encapsulates the specified interface.
        /// </returns>
        public static InterfaceReference<TInterface, TObject> Some(TInterface @interface) => new InterfaceReference<TInterface, TObject>(@interface);

        // Equality operator overloads for interface reference and interface reference
        public static bool operator ==(InterfaceReference<TInterface, TObject> obj1, InterfaceReference<TInterface, TObject> obj2) => Equals(obj1, obj2);
        public static bool operator !=(InterfaceReference<TInterface, TObject> obj1, InterfaceReference<TInterface, TObject> obj2) => !Equals(obj1, obj2);

        // Equality operator overloads for interface reference and boolean
        public static bool operator ==(InterfaceReference<TInterface, TObject> obj, bool value) => obj.HasValue == value;
        public static bool operator !=(InterfaceReference<TInterface, TObject> obj, bool value) => obj.HasValue != value;

        // Helper methods for implicit conversions
        public static implicit operator TInterface(InterfaceReference<TInterface, TObject> obj) => obj.Value;
        public static implicit operator TObject(InterfaceReference<TInterface, TObject> obj) => obj.UnderlyingValue;
        public static implicit operator InterfaceReference<TInterface, TObject>(TInterface @interface) => new InterfaceReference<TInterface, TObject>(@interface);
        public static implicit operator InterfaceReference<TInterface, TObject>(TObject target) => new InterfaceReference<TInterface, TObject>(target);

        /// <summary>
        /// Sets the underlying value to the specified target object.
        /// </summary>
        /// <param name="target">The object to set as the underlying value. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is <see langword="null"/>.</exception>
        public void Set(TObject target)
        {
            // Check if the provided target object is null, if it is, we throw an ArgumentNullException.
            if (target == null) throw new ArgumentNullException(nameof(target), "Target cannot be null.");

            // Set the underlying value to the provided target object.
            underlyingValue = target;
        }

        /// <summary>
        /// Sets the underlying value to the specified interface implementation.
        /// </summary>
        /// <param name="interface">The interface implementation to set. Must not be <see langword="null"/> and must be of type <typeparamref
        /// name="TObject"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="interface"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="interface"/> is not of type <typeparamref name="TObject"/>.</exception>
        public void Set(TInterface @interface)
        {
            // First, we check if the provided interface is null. If it is, we throw an ArgumentNullException to indicate that a null value is not acceptable for this parameter.
            if (@interface == null) throw new ArgumentNullException(nameof(@interface), "Interface cannot be null.");

            // Attempt to cast the provided interface to the underlying object type. If the cast fails, it will result in null.
            underlyingValue = @interface as TObject;

            // After attempting to cast the interface to the underlying object type, we check if the result is null.
            if (underlyingValue == null) throw new ArgumentException($"{@interface} needs to be of type {typeof(TObject)}.", nameof(@interface));
        }

        /// <summary>
        /// Clears the reference by setting the underlying value to null.
        /// </summary>
        public void Clear() => underlyingValue = null;

        /// <summary>
        /// Retrieves the value of the current instance cast to the specified interface type.
        /// </summary>
        /// <typeparam name="T">The interface type to which the value should be cast. Must implement <typeparamref name="TInterface"/>.</typeparam>
        /// <returns>The value of the current instance cast to <typeparamref name="T"/>, or the default value of <typeparamref
        /// name="T"/> if the current instance has no value.</returns>
        public T GetAs<T>() where T : TInterface => HasValue ? (T)Value : (T)default;

        /// <summary>
        /// Determines whether the current instance represents a null value.
        /// </summary>
        /// <returns>true if the current instance is null; otherwise, false.</returns>
        public virtual bool IsNull() => underlyingValue == null;

        /// <summary>
        /// Determines whether two <see cref="InterfaceReference{TInterface, TObject}"/> instances are equal.
        /// </summary>
        /// <param name="obj1">The first <see cref="InterfaceReference{TInterface, TObject}"/> instance to compare. Can be <see langword="null"/>.</param>
        /// <param name="obj2">The second <see cref="InterfaceReference{TInterface, TObject}"/> instance to compare. Can be <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are <see langword="null"/> or their <c>UnderlyingValue</c> properties are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool Equals(InterfaceReference<TInterface, TObject> obj1, InterfaceReference<TInterface, TObject> obj2)
        {
            // Handle null cases first to avoid unnecessary property access. If both instances are null, they are considered equal. If only one is null, they are not equal.
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // Compare the underlying values of both instances. If either underlying value is null, the comparison will return false unless both are null (handled above).
            return obj1.UnderlyingValue == obj2.UnderlyingValue;
        }

        /// <summary>
        /// Determines whether two instances of the specified interface type are equal.
        /// </summary>
        /// <remarks>
        /// This method performs equality comparison by casting the provided interface instances to the underlying object type. 
        /// If either instance is <see langword="null"/>, the method returns <see  langword="false"/> unless both are <see langword="null"/>.
        /// </remarks>
        /// <param name="int1">The first instance to compare. Can be <see langword="null"/>.</param>
        /// <param name="int2">The second instance to compare. Can be <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are <see langword="null"/> or if they are equal when cast to the underlying object type; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool Equals(TInterface int1, TInterface int2)
        {
            // Handle null cases first to avoid unnecessary casting. If both instances are null, they are considered equal. If only one is null, they are not equal.
            if (int1 == null && int2 == null) return true;
            if (int1 == null || int2 == null) return false;

            // Attempt to cast both interface instances to the underlying object type. If either cast fails, the comparison will return false.
            return int1 as TObject == int2 as TObject;
        }

        /// <summary>
        /// Determines whether two objects of type <typeparamref name="TObject"/> are equal.
        /// </summary>
        /// <remarks>
        /// This method performs equality comparison by casting the objects to <typeparamref name="TInterface"/>. 
        /// If either object cannot be cast to <typeparamref name="TInterface"/>, the comparison will return <see langword="false"/>.
        /// </remarks>
        /// <param name="obj1">The first object to compare. Can be <see langword="null"/>.</param>
        /// <param name="obj2">The second object to compare. Can be <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if both objects are <see langword="null"/> or if their casted representations as <typeparamref name="TInterface"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool Equals(TObject obj1, TObject obj2)
        {
            // Handle null cases first to avoid unnecessary casting. If both objects are null, they are considered equal. If only one is null, they are not equal.
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // Attempt to cast both objects to the interface type. If either cast fails, the comparison will return false.
            return obj1 as TInterface == obj2 as TInterface;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is an <see cref="InterfaceReference{TInterface, TObject}"/> and is equal to the current instance; otherwise, <see langword="false"/>. 
        /// </returns>
        public override bool Equals(object obj)
        {
            // If the object is an InterfaceReference, we can compare it using the static Equals method defined for InterfaceReference.
            if (obj is InterfaceReference<TInterface, TObject> other) return Equals(this, other);

            // If the object is not an InterfaceReference, we can attempt to compare it directly to the underlying value.
            return false;
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <remarks>
        /// The hash code is derived from the underlying value of the instance. If the underlying value is <see langword="null"/>, the method returns 0.
        /// </remarks>
        /// <returns>
        /// An integer representing the hash code of the underlying value, or 0 if the underlying value is <see langword="null"/>.
        /// </returns>
        public override int GetHashCode() => underlyingValue != null ? underlyingValue.GetHashCode() : 0;
    }

    /// <summary>
    /// Represents a reference to an interface type, allowing for generic constraints and type safety.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface being referenced. Must be a class.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class
    {
        public InterfaceReference(TInterface @interface) : base(@interface) { }

        public override bool HasValue => base.HasValue && !IsNull();

        public override bool IsNull() => base.IsNull() || this is Object obj && obj == null;

        // Implicit conversion operators for convenience
        public static implicit operator Object(InterfaceReference<TInterface> obj) => obj.UnderlyingValue;
        public static implicit operator InterfaceReference<TInterface>(Object target) => new InterfaceReference<TInterface>(target as TInterface);
    }
}