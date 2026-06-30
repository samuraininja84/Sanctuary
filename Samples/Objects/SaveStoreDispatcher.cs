using UnityEngine;
using Sanctuary.Stores;

namespace Sanctuary.Samples 
{
    /// <summary>
    /// A helper component that registers and unregisters all
    /// <see cref="ISaveStore"/>s on the same game object.
    /// </summary>
    public class SaveStoreDispatcher : MonoBehaviour 
    {
        [Tooltip("The scope to register the stores with.")]
        [SerializeField] private SaveScope Scope = SaveScope.Scene;
        private ISaveStore[] _stores;

        private void Awake() => _stores = GetComponents<ISaveStore>();

        private void OnEnable() 
        {
            // Register all stores on this game object
            for (var i = 0; i < _stores.Length; i++) 
            {
                // Cache the store in a local variable to avoid multiple array accesses
                var store = _stores[i];

                // Register the store
                SaveStoreRegistry.Register(store, SaveProvider.ByScope(Scope));
            }
        }

        private void OnDisable() 
        {
            // Unregister all stores on this game object
            for (var i = 0; i < _stores.Length; i++) 
            {
                // Cache the store in a local variable to avoid multiple array accesses
                var store = _stores[i];

                // Unregister the store
                SaveStoreRegistry.Unregister(store);
            }
        }
    }
}
