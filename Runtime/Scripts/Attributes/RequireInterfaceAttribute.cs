using System;
using UnityEngine;

namespace Sanctuary.Attributes
{
    /// <summary>
    /// Specifies that a field must reference an object implementing a specific interface.
    /// </summary>
    /// <remarks>This attribute is applied to fields to enforce that the assigned object implements the
    /// specified interface type. It is commonly used in scenarios where interface-based programming is required, such
    /// as dependency injection or  ensuring compatibility with specific APIs.</remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the type of the interface associated with this instance.
        /// </summary>
        public readonly Type InterfaceType;

        public RequireInterfaceAttribute(Type interfaceType)
        {
            // Ensure the provided type is not null and is an interface
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");
            InterfaceType = interfaceType;
        }
    }
}