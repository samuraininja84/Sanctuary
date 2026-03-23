using System;
using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// Represents a serializable version of a globally unique identifier (GUID).
    /// </summary>
    /// <remarks>
    /// This structure provides functionality for working with GUIDs in a format that can be serialized,
    /// making it suitable for use in scenarios such as Unity or other environments where standard GUIDs are
    /// not directly serializable. 
    /// It supports conversion to and from <see cref="Guid"/>, as well as hexadecimal string representations.
    /// </remarks>
    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        [SerializeField, HideInInspector] public uint Part1;
        [SerializeField, HideInInspector] public uint Part2;
        [SerializeField, HideInInspector] public uint Part3;
        [SerializeField, HideInInspector] public uint Part4;

        /// <summary>
        /// Gets a <see cref="SerializableGuid"/> instance that represents an empty GUID.
        /// </summary>
        public static SerializableGuid Empty => new(0, 0, 0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableGuid"/> class with the specified components.
        /// </summary>
        /// <param name="val1">The first 32-bit unsigned integer component of the GUID.</param>
        /// <param name="val2">The second 32-bit unsigned integer component of the GUID.</param>
        /// <param name="val3">The third 32-bit unsigned integer component of the GUID.</param>
        /// <param name="val4">The fourth 32-bit unsigned integer component of the GUID.</param>
        public SerializableGuid(uint val1, uint val2, uint val3, uint val4)
        {
            Part1 = val1;
            Part2 = val2;
            Part3 = val3;
            Part4 = val4;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableGuid"/> class using the specified <see cref="Guid"/>.
        /// </summary>
        /// <remarks>The provided <paramref name="guid"/> is split into four 32-bit unsigned integer parts to facilitate serialization and deserialization.</remarks>
        /// <param name="guid">The <see cref="Guid"/> to be converted into a serializable format.</param>
        public SerializableGuid(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            Part1 = BitConverter.ToUInt32(bytes, 0);
            Part2 = BitConverter.ToUInt32(bytes, 4);
            Part3 = BitConverter.ToUInt32(bytes, 8);
            Part4 = BitConverter.ToUInt32(bytes, 12);
        }

        /// <summary>
        /// Replaces the current instance with the specified <see cref="SerializableGuid"/>.
        /// </summary>
        /// <remarks>This method creates a new <see cref="SerializableGuid"/> using the components of the
        /// specified <paramref name="other"/>.</remarks>
        /// <param name="other">The <see cref="SerializableGuid"/> instance to replace the current instance with.</param>
        public void Replace(SerializableGuid other) => new SerializableGuid(other.Part1, other.Part2, other.Part3, other.Part4);

        /// <summary>
        /// Creates a new instance of <see cref="SerializableGuid"/> with a unique value.
        /// </summary>
        /// <returns>A <see cref="SerializableGuid"/> representing a newly generated unique identifier.</returns>
        public static SerializableGuid NewGuid() => Guid.NewGuid().ToSerializableGuid();

        /// <summary>
        /// Converts a 32-character hexadecimal string into a <see cref="SerializableGuid"/> instance.
        /// </summary>
        /// <remarks>The input string must consist of exactly 32 hexadecimal characters. Each group of 8
        /// characters corresponds to a part of the GUID. If the input string does not meet this requirement, the method
        /// returns <see cref="SerializableGuid.Empty"/>.</remarks>
        /// <param name="hexString">A 32-character string representing the hexadecimal value of the GUID.</param>
        /// <returns>A <see cref="SerializableGuid"/> instance created from the specified hexadecimal string. If the string is
        /// not exactly 32 characters long, returns <see cref="SerializableGuid.Empty"/>.</returns>
        public static SerializableGuid FromHexString(string hexString)
        {
            // Check if the hex string is exactly 32 characters long, which is required for a valid GUID representation, if it is not, return Empty
            if (hexString.Length != 32) return Empty;

            // Convert each 8-character segment of the hex string into a uint
            return new SerializableGuid
            (
                Convert.ToUInt32(hexString.Substring(0, 8), 16),
                Convert.ToUInt32(hexString.Substring(8, 8), 16),
                Convert.ToUInt32(hexString.Substring(16, 8), 16),
                Convert.ToUInt32(hexString.Substring(24, 8), 16)
            );
        }

        /// <summary>
        /// Converts the current object to its hexadecimal string representation.
        /// </summary>
        /// <remarks>
        /// The resulting string concatenates the hexadecimal representations of Part1, Part2, Part3, and Part4 in sequence, without separators.</remarks>
        /// <returns>
        /// A string containing the hexadecimal representation of the object's parts. 
        /// Each part is formatted as an 8-character uppercase hexadecimal value.
        /// </returns>
        public string ToHexString() => $"{Part1:X8}{Part2:X8}{Part3:X8}{Part4:X8}";

        /// <summary>
        /// Converts the current instance into a <see cref="Guid"/> representation.
        /// </summary>
        /// <remarks>The method combines the values of the instance's components into a 16-byte array and
        /// uses it to create a <see cref="Guid"/>. Ensure that the instance's components are properly initialized
        /// before calling this method.</remarks>
        /// <returns>A <see cref="Guid"/> constructed from the values of the instance.</returns>
        public Guid ToGuid()
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(Part1).CopyTo(bytes, 0);
            BitConverter.GetBytes(Part2).CopyTo(bytes, 4);
            BitConverter.GetBytes(Part3).CopyTo(bytes, 8);
            BitConverter.GetBytes(Part4).CopyTo(bytes, 12);
            return new Guid(bytes);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the specified object is a <see cref="SerializableGuid"/> and is equal to the
        /// current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is SerializableGuid guid && this.Equals(guid);

        /// <summary>
        /// Determines whether the current instance is equal to the specified <see cref="SerializableGuid"/>.
        /// </summary>
        /// <param name="other">The <see cref="SerializableGuid"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified <see cref="SerializableGuid"/>; 
        /// otherwise, <see langword="false"/>.</returns>
        public bool Equals(SerializableGuid other) => Part1 == other.Part1 && Part2 == other.Part2 && Part3 == other.Part3 && Part4 == other.Part4;

        /// <summary>
        /// Generates a hash code for the current object based on its constituent parts.
        /// </summary>
        /// <remarks>The hash code is computed using the values of <see cref="Part1"/>, <see
        /// cref="Part2"/>, <see cref="Part3"/>, and <see cref="Part4"/>. This method is suitable for use in hashing
        /// algorithms and data structures such as hash tables.</remarks>
        /// <returns>An integer representing the hash code for the current object.</returns>
        public override int GetHashCode() => HashCode.Combine(Part1, Part2, Part3, Part4);

        /// <summary>
        /// Returns a string representation of the current object in hexadecimal format.
        /// </summary>
        /// <returns>A string containing the hexadecimal representation of the object.</returns>
        public override string ToString() => ToHexString();

        // Implicit conversion operators allow for easy conversion between SerializableGuid and Guid types.
        public static implicit operator Guid(SerializableGuid serializableGuid) => serializableGuid.ToGuid();
        public static implicit operator SerializableGuid(Guid guid) => new SerializableGuid(guid);

        // Implicit conversion from string to SerializableGuid
        public static implicit operator SerializableGuid(string hexString)
        {
            if (string.IsNullOrEmpty(hexString) || hexString.Length != 32) return Empty;
            return FromHexString(hexString);
        }

        // Implicit equality operators allow for direct comparison between SerializableGuid instances.
        public static bool operator ==(SerializableGuid left, SerializableGuid right) => left.Equals(right);
        public static bool operator !=(SerializableGuid left, SerializableGuid right) => !(left == right);
        public static bool operator ==(SerializableGuid left, Guid right) => left.Equals(new SerializableGuid(right));
        public static bool operator !=(SerializableGuid left, Guid right) => !left.Equals(new SerializableGuid(right));
    }
}