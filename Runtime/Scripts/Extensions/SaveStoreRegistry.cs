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
        private static readonly Dictionary<ISaveStore, SaveControllerBase> _storeLookup = new();

        #region Registration & Unregistration

        /// <summary>
        /// Register a store.
        /// </summary>
        /// <param name="store">The store to register.</param>
        /// <param name="save">The associated save controller.</param>
        public static void Register(this ISaveStore store, SaveControllerBase save = null)
        {
            // Register the store in the linked list.
            _stores.AddFirst(store);

            // Add the store and its associated save controller to the lookup dictionary, if provided.
            if (save != null) _storeLookup.Add(store, save);
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
        }

        /// <summary>
        /// Removes all stores from the collection and clears the associated lookup dictionary.
        /// </summary>
        /// <returns><see langword="true"/> if any stores were removed; otherwise, <see langword="false"/>.</returns>
        public static bool Clear()
        {
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
        internal static void SaveWith(this SaveControllerBase save)
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
        internal static void LoadWith(this SaveControllerBase save)
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
        public static void SaveAllWith(this SaveControllerBase save)
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
        public static void LoadAllWith(this SaveControllerBase save)
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

        #region Create/Save/Load/Delete All

        /// <summary>
        /// Creates all registered data stores for each supported save scope.
        /// </summary>
        /// <remarks>
        /// This method initializes data stores associated with the Absolute, Global, Scene, and Temporary save scopes. 
        /// It should be called before attempting to access or manipulate data in any of these stores to ensure they are properly set up.<
        /// /remarks>
        public static void CreateAll()
        {
            // Create all of the registered stores with their associated save controllers.
            CreateByScope(SaveScope.Absolute);
            CreateByScope(SaveScope.Global);
            CreateByScope(SaveScope.Scene);
            CreateByScope(SaveScope.Temporary);
        }

        /// <summary>
        /// Simultaneously saves all registered stores.
        /// </summary>
        public static void SaveAll(SaveMode mode = SaveMode.Full)
        {
            // Save all of the registered stores with their associated save controllers.
            SaveByScope(SaveScope.Absolute, mode);
            SaveByScope(SaveScope.Global, mode);
            SaveByScope(SaveScope.Scene, mode);
            SaveByScope(SaveScope.Temporary, SaveMode.MemoryOnly);
        }

        /// <summary>
        /// Simultaneously loads all registered stores.
        /// </summary>
        public static void LoadAll(SaveMode mode = SaveMode.Full)
        {
            // Load all of the registered stores with their associated save controllers.
            LoadByScope(SaveScope.Absolute, mode);
            LoadByScope(SaveScope.Global, mode);
            LoadByScope(SaveScope.Scene, mode);
            LoadByScope(SaveScope.Temporary, SaveMode.MemoryOnly);
        }

        /// <summary>
        /// Simultaneously deletes all saved data for all registered stores.
        /// </summary>
        public static void DeleteAll()
        {
            // Delete all of the registered stores with their associated save controllers.
            DeleteByScope(SaveScope.Absolute);
            DeleteByScope(SaveScope.Global);
            DeleteByScope(SaveScope.Scene);
            DeleteByScope(SaveScope.Temporary);
        }

        #endregion

        #region Create/Save/Load/Delete Indexed

        /// <summary>
        /// Initializes all indexed save data for both global and scene scopes.
        /// </summary>
        /// <remarks>
        /// Call this method to ensure that all required indexed save data structures are created and ready for use. 
        /// This is typically necessary during application startup or when resetting save data.
        /// </remarks>
        public static void CreateIndexed()
        {
            // Create all of the indexed saves (Global and Scene)
            CreateByScope(SaveScope.Global);
            CreateByScope(SaveScope.Scene);
        }

        /// <summary>
        /// Simultaneously saves all registered stores that are associated with a save controller of scope <see cref="SaveScope.Global"/> or <see cref="SaveScope.Scene"/>.
        /// </summary>
        /// <param name="mode">The mode in which to save the stores. Defaults to <see cref="SaveMode.Full"/>.</param>
        public static void SaveIndexed(SaveMode mode = SaveMode.Full)
        {
            // Save all of the indexed saves (Global and Scene)
            SaveByScope(SaveScope.Global, mode);
            SaveByScope(SaveScope.Scene, mode);
        }

        /// <summary>
        /// Loads all indexed saves from both global and scene scopes using the specified save mode.
        /// </summary>
        /// <remarks>This method retrieves saves from both global and scene contexts, allowing for flexible save management based on the provided save mode.</remarks>
        /// <param name="mode">Specifies the save mode to use when loading indexed saves. Determines the extent of the loading operation.
        /// The default is <see cref="SaveMode.Full"/>.</param>
        public static void LoadIndexed(SaveMode mode = SaveMode.Full)
        {
            // Load all of the indexed saves (Global and Scene)
            LoadByScope(SaveScope.Global, mode);
            LoadByScope(SaveScope.Scene, mode);
        }

        /// <summary>
        /// Deletes all indexed saves, including both global and scene-specific data.
        /// </summary>
        /// <remarks>
        /// This method removes all saves that have been indexed, regardless of their scope. 
        /// Use with caution, as this operation cannot be undone and will result in the loss of all indexed save data.
        /// </remarks>
        public static void DeleteIndexed()
        {
            // Delete all of the indexed saves (Global and Scene)
            DeleteByScope(SaveScope.Global);
            DeleteByScope(SaveScope.Scene);
        }

        #endregion

        #region Create/Save/Load/Delete By Scope

        /// <summary>
        /// Creates all stores that are associated with the specified save scope.
        /// </summary>
        /// <remarks>This method initiates the creation process for all stores matching the provided scope. 
        /// The operation is asynchronous but returns immediately; any exceptions thrown during store creation will be unobserved unless handled elsewhere. 
        /// Consider using an asynchronous return type to await completion and handle errors appropriately.
        /// </remarks>
        /// <param name="scope">The save scope used to identify which stores to create.</param>
        public static async void CreateByScope(this SaveScope scope) => await SaveProvider.ByScope(scope).Create();

        /// <summary>
        /// Invokes the save operation on all stores that match the specified scope.
        /// </summary>
        /// <remarks>
        /// This method iterates through all registered stores and invokes their save operation if the store's associated save controller matches the specified name.
        /// If no stores match the given name, no action is performed.
        /// </remarks>
        /// <param name="scope">The scope of the stores to save. This parameter cannot be <see langword="null"/> or empty.</param>
        /// <param name="mode">The mode in which to save the stores. Defaults to <see cref="SaveMode.Full"/>.</param>
        public static async void SaveByScope(this SaveScope scope, SaveMode mode = SaveMode.Full) => await SaveProvider.ByScope(scope).Save(mode);

        /// <summary>
        /// Loads and initializes all stores associated with the specified scope.
        /// </summary>
        /// <remarks>This method iterates through all registered stores and invokes their load operation if their name matches the specified value.</remarks>
        /// <param name="scope">The scope of the stores to load. This parameter cannot be <see langword="null"/> or empty.</param>
        /// <param name="mode">The mode in which to load the stores. Defaults to <see cref="SaveMode.Full"/>.</param>
        public static async void LoadByScope(this SaveScope scope, SaveMode mode = SaveMode.Full) => await SaveProvider.ByScope(scope).Load(mode);

        /// <summary>
        /// Deletes all saved data associated with the specified scope.
        /// </summary>
        /// <param name="scope">The scope of the stores to delete. This parameter cannot be <see langword="null"/> or empty.</param>
        public static async void DeleteByScope(this SaveScope scope) => await SaveProvider.ByScope(scope).Delete();

        #endregion
    }
}