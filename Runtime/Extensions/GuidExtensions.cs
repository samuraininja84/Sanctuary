using System;

namespace Sanctuary.Extensions
{
    /// <summary>
    /// Provides extension methods for converting between <see cref="Guid"/> and <c>SerializableGuid</c>.
    /// </summary>
    /// <remarks>
    /// These methods enable seamless conversion between the standard .NET <see cref="Guid"/> structure and a
    /// custom <c>SerializableGuid</c> type, which may be used in scenarios where a different representation of GUIDs is
    /// required, such as serialization or interoperability with systems that do not support the standard <see cref="Guid"/> format.
    /// </remarks>
    public static class GuidExtensions
    {
        /// <summary>
        /// Converts a <see cref="Guid"/> to a <see cref="SerializableGuid"/>.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> to convert.</param>
        /// <returns>
        /// A <see cref="SerializableGuid"/> representation of the specified <see cref="Guid"/>.
        /// </returns>
        public static SerializableGuid ToSerializableGuid(this Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            return new SerializableGuid(
                BitConverter.ToUInt32(bytes, 0),
                BitConverter.ToUInt32(bytes, 4),
                BitConverter.ToUInt32(bytes, 8),
                BitConverter.ToUInt32(bytes, 12)
            );
        }

        /// <summary>
        /// Converts a <see cref="SerializableGuid"/> instance to a <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">The <see cref="SerializableGuid"/> to convert.</param>
        /// <returns>
        /// A <see cref="Guid"/> that represents the same value as the specified <see cref="SerializableGuid"/>.
        /// </returns>
        public static Guid ToSystemGuid(this SerializableGuid guid)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(guid.Part1).CopyTo(bytes, 0);
            BitConverter.GetBytes(guid.Part2).CopyTo(bytes, 4);
            BitConverter.GetBytes(guid.Part3).CopyTo(bytes, 8);
            BitConverter.GetBytes(guid.Part4).CopyTo(bytes, 12);
            return new Guid(bytes);
        }
    }
}