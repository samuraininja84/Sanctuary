using UnityEngine;
using Sanctuary.Attributes;

namespace Sanctuary
{
    /// <summary>
    /// A ScriptableObject that holds a reference to a SaveLocation, allowing it to be shared across multiple objects and scenes.
    /// </summary>
    /// <remarks>For use cases where multiple objects need to reference the same SaveLocation, such as a shared inventory or a common save point.</remarks>
    [CreateAssetMenu(fileName = "New Shared Save Location", menuName = "Sanctuary/New Shared Save Location")]
    public class SharedSaveLocation : ScriptableObject
    {
        [ObjectLocation]
        public SaveLocation Location;

        // Implicit conversion operator to allow using SharedSaveLocation directly as a SaveLocation.
        public static implicit operator SaveLocation(SharedSaveLocation sharedLocation) => sharedLocation.Location;
    }
}
