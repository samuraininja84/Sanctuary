using System.Collections.Generic;

namespace Sanctuary.Stores 
{
    /// <summary>
    /// A static registry for <see cref="ISaveStore"/>s.
    /// </summary>
    public static class SaveStoreRegistry
    {
        /// <summary>
        /// A linked list of registered stores.
        /// </summary>
        private static readonly LinkedList<ISaveStore> _stores = new();

        /// <summary>
        /// A lookup dictionary for stores and their associated save controllers.
        /// </summary>
        private static readonly Dictionary<ISaveStore, ISaveController> _storeLookup = new();

        /// <summary>
        /// An event that is invoked when a store is registered. It takes the registered store and its associated save controller as parameters.
        /// </summary>
        public static event System.Action<ISaveStore, ISaveController> OnStoreRegistered = delegate { };

        /// <summary>
        /// An event that is invoked when a store is unregistered. It takes the unregistered store as a parameter.
        /// </summary>
        public static event System.Action<ISaveStore> OnStoreUnregistered = delegate { };

        #region Registration & Unregistration

        /// <summary>
        /// Gets a copy of the currently registered stores and their associated save controllers.
        /// </summary>
        /// <returns>A dictionary containing the registered stores and their associated save controllers.</returns>
        public static Dictionary<ISaveStore, ISaveController> GetRegisteredStores() => new(_storeLookup);

        /// <summary>
        /// Register a store.
        /// </summary>
        /// <param name="store">The store to register.</param>
        /// <param name="save">The associated save controller.</param>
        public static void Register(this ISaveStore store, ISaveController save = null)
        {
            // Register the store in the linked list.
            _stores.AddFirst(store);

            // Add the store and its associated save controller to the lookup dictionary, if provided.
            if (save != null) _storeLookup.Add(store, save);

            // Invoke the store registered event.
            OnStoreRegistered?.Invoke(store, save);
        }

        /// <summary>
        /// Unregisters the specified save store, removing it from the internal collection.
        /// </summary>
        /// <remarks>This method removes the specified save store from the internal linked list and lookup dictionary.
        /// If the save store is not registered, this method has no effect.
        /// </remarks>
        /// <param name="store">The save store to unregister. Must not be <see langword="null"/>.</param>
        public static void Unregister(this ISaveStore store)
        {
            // Remove the store from the linked list.
            _stores.Remove(store);

            // Remove the store from the lookup dictionary.
            _storeLookup.Remove(store);

            // Invoke the store unregistered event.
            OnStoreUnregistered?.Invoke(store);
        }

        /// <summary>
        /// Removes all stores from the collection and clears the associated lookup dictionary.
        /// </summary>
        /// <returns><see langword="true"/> if any stores were removed; otherwise, <see langword="false"/>.</returns>
        public static bool Clear()
        {
            // Invoke the store unregistered event for each store before clearing.
            foreach (var store in _stores) OnStoreUnregistered?.Invoke(store);

            // Clear the linked list.
            _stores.Clear();

            // Clear the lookup dictionary.
            _storeLookup.Clear();

            // Return whether any stores were removed.
            return _stores.Count > 0 || _storeLookup.Count > 0;
        }

        #endregion

        #region Save/Load/Delete For Save Controller

        /// <summary>
        /// Find all stores associated with the given save controller and invoke their save operation.
        /// </summary>
        /// <param name="save">The save controller to match.</param>
        internal static void SaveWith(this ISaveController save)
        {
            // Find all stores associated with the given save controller.
            foreach (var kvp in _storeLookup)
            {
                // If the store's associated save controller matches the given one, call OnSave on it.
                if (kvp.Value == save) kvp.Key.OnSave(kvp.Value);
            }
        }

        /// <summary>
        /// Find all stores associated with the given save controller and invoke their load operation.
        /// </summary>
        /// <param name="save">The save controller to match.</param>
        internal static void LoadWith(this ISaveController save)
        {
            // Find all stores associated with the given save controller.
            foreach (var kvp in _storeLookup)
            {
                // If the store's associated save controller matches the given one, call OnLoad on it.
                if (kvp.Value == save) kvp.Key.OnLoad(kvp.Value);
            }
        }

        #endregion

        #region Save/Load For All Stores Under A Save Controller

        /// <summary>
        /// Invoke <see cref="ISaveStore.OnSave"/> on all registered stores.
        /// </summary>
        public static void SaveAllWith(this ISaveController save)
        {
            // Get the first store in the linked list.
            var store = _stores.First;

            // Iterate through the linked list and call OnSave on each store.
            while (store != null)
            {
                // Call OnSave on the current store.
                store.Value.OnSave(save);

                // Move to the next store in the linked list.
                store = store.Next;
            }
        }

        /// <summary>
        /// Invoke <see cref="ISaveStore.OnLoad"/> on all registered stores.
        /// </summary>
        /// <param name="save">The current save controller.</param>
        public static void LoadAllWith(this ISaveController save)
        {
            // Get the first store in the linked list.
            var store = _stores.First;

            // Iterate through the linked list and call OnLoad on each store.
            while (store != null)
            {
                // Call OnLoad on the current store.
                store.Value.OnLoad(save);

                // Move to the next store in the linked list.
                store = store.Next;
            }
        }

        #endregion

        #region Create/Save/Load/Delete By Scope

        /// <summary>
        /// Invokes the save operation on all stores that match the specified scope.
        /// </summary>
        /// <remarks>
        /// This method iterates through all registered stores and invokes their save operation if the store's associated save controller matches the specified name.
        /// If no stores match the given name, no action is performed.
        /// </remarks>
        /// <param name="scope">The scope of the stores to save. This parameter cannot be <see langword="null"/> or empty.</param>
        /// <param name="mode">The mode in which to save the stores. Defaults to <see cref="SaveMode.Full"/>.</param>
        public static async void SaveByScope(this SaveScope scope) => await SaveProvider.ByScope(scope).Save();

        /// <summary>
        /// Loads and initializes all stores associated with the specified scope.
        /// </summary>
        /// <remarks>This method iterates through all registered stores and invokes their load operation if their name matches the specified value.</remarks>
        /// <param name="scope">The scope of the stores to load. This parameter cannot be <see langword="null"/> or empty.</param>
        /// <param name="mode">The mode in which to load the stores. Defaults to <see cref="SaveMode.Full"/>.</param>
        public static async void LoadByScope(this SaveScope scope) => await SaveProvider.ByScope(scope).Load();

        /// <summary>
        /// Deletes all saved data associated with the specified scope.
        /// </summary>
        /// <param name="scope">The scope of the stores to delete. This parameter cannot be <see langword="null"/> or empty.</param>
        public static async void DeleteByScope(this SaveScope scope) => await SaveProvider.ByScope(scope).Delete();

        #endregion
    }
}