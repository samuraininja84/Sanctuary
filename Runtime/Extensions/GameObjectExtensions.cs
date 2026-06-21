using UnityEngine;

namespace Sanctuary.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Tries to get the component of type T from the GameObject. If it doesn't exist, adds it and returns it.
        /// </summary>
        /// <typeparam name="T">The type of component to get or add.</typeparam>
        /// <param name="gameObject">A reference to the GameObject.</param>
        /// <returns>The component of type T.</returns>
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            // Try to get the component
            T component = gameObject.GetComponent<T>();

            // If the component doesn't exist, add it
            if (component == null) component = gameObject.AddComponent<T>();

            // Return the component
            return component;
        }

        /// <summary>
        /// Gets the component of type T1 from the GameObject. If it doesn't exist, adds a component of type T2 and returns it as T1.
        /// </summary>
        /// <typeparam name="T1">The type of component to get.</typeparam>
        /// <typeparam name="T2">The type of component to add if T1 doesn't exist Intended to be a subclass of T1.</typeparam>
        /// <param name="gameObject">A reference to the GameObject.</param>
        /// <returns>A component of type T1.</returns>
        public static T1 GetOrAddSubclass<T1, T2>(this GameObject gameObject) where T1 : Component where T2 : Component
        {
            // Try to get the component
            T1 component = gameObject.GetComponent<T1>();

            // If the component doesn't exist, return the GameObject's own component
            if (component == null) component = gameObject.AddComponent<T2>() as T1;

            // Return the component
            return component;
        }

        /// <summary>
        /// Returns the object itself if it exists, null otherwise.
        /// </summary>
        /// <remarks>
        /// This method helps differentiate between a null reference and a destroyed Unity object. Unity's "== null" check
        /// can incorrectly return true for destroyed objects, leading to misleading behaviour. The OrNull method use
        /// Unity's "null check", and if the object has been marked for destruction, it ensures an actual null reference is returned,
        /// aiding in correctly chaining operations and preventing NullReferenceExceptions.
        /// </remarks>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object being checked.</param>
        /// <returns>The object itself if it exists and not destroyed, null otherwise.</returns>
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
    }
}
