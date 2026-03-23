using System;
using System.Linq;

namespace Sanctuary
{
    /// <summary>
    /// Provides extension methods for working with <see cref="Type"/> objects,  including methods to check inheritance or implementation relationships.
    /// </summary>
    /// <remarks>
    /// This class contains static methods that extend the functionality of the <see cref="Type"/> class.
    /// Enabling developers to perform common type-related operations, such as determining whether a type inherits or implements a specified base type or interface.
    /// </remarks>
    public static class TypeExtensions 
    {
        /// <summary>
        /// Checks if a given type inherits or implements a specified base type.
        /// </summary>
        /// <param name="type">The type which needs to be checked.</param>
        /// <param name="baseType">The base type/interface which is expected to be inherited or implemented by the 'type'</param>
        /// <returns>Return true if 'type' inherits or implements 'baseType'. False otherwise</returns>        
        public static bool InheritsOrImplements(this Type type, Type baseType) 
        {
            // Resolve the generic type definitions for the given type and the base type
            type = ResolveGenericType(type);
            baseType = ResolveGenericType(baseType);

            // While the type is not object, check if it is the same as the base type or if it implements any interfaces that match the base type
            while (type != typeof(object)) 
            {
                // Check if the current type is the same as the base type or if it has any interfaces that match the base type, if so, return true
                if (baseType == type || HasAnyInterfaces(type, baseType)) return true;

                // If the type is a generic type, resolve its generic type definition
                type = ResolveGenericType(type.BaseType);

                // If the type is null, it means we have reached the end of the inheritance chain, return false
                if (type == null) return false;
            }

            // If we reach here, it means the type does not inherit or implement the base type, return false
            return false;
        }
        
        /// <summary>
        /// Resolves the generic type definition of the specified type.
        /// </summary>
        /// <remarks>This method checks whether the provided type is a generic type and, if so,  returns
        /// its generic type definition. If the type is not generic or is already  its generic type definition, the
        /// original type is returned.</remarks>
        /// <param name="type">The type to resolve. Must not be <see langword="null"/>.</param>
        /// <returns>The generic type definition if the specified type is a generic type;  otherwise, the original type.</returns>
        private static Type ResolveGenericType(Type type) 
        {
            // If the type is already a generic type definition, return it
            if (type is not { IsGenericType: true }) return type;

            // Cast the type to a generic type by getting its generic type definition
            var genericType = type.GetGenericTypeDefinition();

            // If the generic type is the same as the original type, return the original type, otherwise return the generic type
            return genericType != type ? genericType : type;
        }

        /// <summary>
        /// Determines whether the specified type implements the given interface type.
        /// </summary>
        /// <param name="type">The type to inspect for implemented interfaces. Cannot be <see langword="null"/>.</param>
        /// <param name="interfaceType">The interface type to check against. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the specified type implements the given interface type; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool HasAnyInterfaces(Type type, Type interfaceType) => type.GetInterfaces().Any(i => ResolveGenericType(i) == interfaceType);
    }
}
