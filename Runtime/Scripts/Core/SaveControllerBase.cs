using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Sanctuary.Stores;
using Sanctuary.Loaders;

namespace Sanctuary
{
    /// <summary>
    /// The main controller for managing game saves.
    /// </summary>
    /// <remarks>
    /// This class provides methods to create, load, save, and delete game saves.
    /// Any class implementing <see cref="ISaveStore"/> can register to be notified for save and load events.
    /// This class uses an <see cref="ISaveLoader"/> to handle the actual loading and saving of data regardless of type.
    /// </remarks>
    public class SaveControllerBase
    {
        #region Instance Accessors

        /// <summary>
        /// The name of the save.
        /// </summary>
        public string Name;

        /// <summary>
        /// Boolean indicating whether the save has been initialized.
        /// </summary>
        protected bool _isInitialized = false;

        /// <summary>
        /// The scope of the save.
        /// </summary>
        protected SaveScope _scope = SaveScope.Global;

        /// <summary>
        /// The loader used to load and save the data.
        /// </summary>
        protected ISaveLoader _loader;

        /// <summary>
        /// A semaphore used to ensure that only one operation is performed at a time.
        /// </summary>
        private readonly SemaphoreSlim _lock = new(1);

        #endregion

        #region Public Accessors

        public bool IsInitialized => _isInitialized && Exists;

        /// <summary>
        /// Whether the save is currently being loaded.
        /// </summary>
        public bool IsLoading => _lock.CurrentCount == 0 || !_isInitialized;

        /// <summary>
        /// Whether the save exists.
        /// </summary>
        public bool Exists { get; private set; }

        /// <summary>
        /// Provides the scope of the save.
        /// </summary>
        public SaveScope Scope => _scope;

        /// <summary>
        /// A protected reference to the save data.
        /// </summary>
        protected ISaveData _data = new SaveData();

        /// <summary>
        /// The save data.
        /// </summary>
        /// <remarks>
        /// The data can only be accessed after the save has been loaded for the
        /// first time. Otherwise it returns an empty <see cref="SaveData"/>.
        /// </remarks>
        public ISaveData Data
        {
            get
            {
                // If the data is null, log an error and return an empty data
                if (_data == null)
                {
                    // Log a warning if the data is accessed before being loaded
                    Debug.LogWarning("[Safekeeper]: Tried to access the data before the save was loaded.\n Make sure to call `save.Load(SaveMode.Full)` before accessing the data. Returning empty data as a placeholder.");

                    // Return an empty data to avoid null reference exceptions
                    _data = new SaveData();
                }

                // Return the data
                return _data;
            }

            // Get the data from the value
            private set => _data = value;
        }

        /// <summary>
        /// An event invoked before the save is saved.
        /// </summary>
        public event Action Saving = delegate { };

        /// <summary>
        /// An event invoked after the save is saved.
        /// </summary>
        public event Action Saved = delegate { };

        #endregion

        #region Static Accessors

#if UNITY_EDITOR

        /// <summary>
        /// A list of existing saves, used for debugging purposes.
        /// </summary>
        public static readonly List<WeakReference<SaveControllerBase>> ExistingSaves = new();

#endif

        #endregion

        #region Configuration

        /// <summary>
        /// Create a new save controller with the given loader and scope, then initialize it.
        /// </summary>
        /// <param name="loader">The loader to use for loading and saving the data.</param>
        /// <param name="scope">The scope of the save. Defaults to <see cref="SaveScope.Global"/>.</param>
        /// <returns>A new instance of the save controller.</returns>
        public static SaveControllerBase New(ISaveLoader loader, SaveScope scope = SaveScope.Global) 
        {
            // Create a new save controller with the provided loader
            var save = new SaveControllerBase(loader, scope);

            // Initialize the save controller
            save.Initialize();

            // Return the newly created save controller
            return save;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveControllerBase"/> class with the specified save loader.
        /// </summary>
        /// <param name="loader">The save loader used to handle loading and saving operations. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="scope">The scope of the save. Defaults to <see cref="SaveScope.Global"/>.</param>
        public SaveControllerBase(ISaveLoader loader, SaveScope scope = SaveScope.Global)
        {
            // Set the loader
            _loader = loader;

            // Set the scope
            _scope = scope;

            // Set the lock
            _lock = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Initialize this save controller.
        /// </summary>
        /// <remarks>
        /// This process loads the basic information about the save, such as its
        /// name and whether it exists.
        /// It does not load the data itself.
        /// </remarks>
        public async void Initialize() 
        {
            // If already initialized, do nothing
            if (_isInitialized) return;

            // Intialize the save controller
            _isInitialized = true;

            #if UNITY_EDITOR

            // Remove dead references
            ExistingSaves.RemoveAll(wr => !wr.TryGetTarget(out _));

            // Add this instance to the list of existing saves
            ExistingSaves.Add(new WeakReference<SaveControllerBase>(this));

            #endif

            // Lock the semaphore to prevent other operations
            await Lock();

            // Check if the save exists
            Exists = await _loader.Exists();

            // Load the name of the save
            Name = await _loader.GetName();

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Releases all resources used by the current instance, including locks and semaphores.
        /// </summary>
        public void Dispose()
        {
            #if UNITY_EDITOR

            // Remove this instance from the list of existing saves and clean up dead references
            ExistingSaves.RemoveAll(wr =>
            {
                // Check if the target is this instance
                if (wr.TryGetTarget(out var target)) return target == this;

                // If the target is dead, return false to keep cleaning up dead references
                return false;
            });

#endif

            // Set the initialized flag to false
            _isInitialized = false;
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Create the save if it doesn't exist.
        /// </summary>
        public async Task Create()
        {
            // Lock the semaphore to prevent other operations
            await Lock();

            // If the save doesn't exist, create it
            if (!Exists)
            {
                // Mark the save as existing
                Exists = true;

                // Create a new save data to avoid null reference exceptions
                Data = await _loader.Create();

                // Invoke the OnLoad method for custom load logic
                OnLoad();
            }

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Save the game state. 
        /// </summary>
        /// <remarks>Saves based on the <see cref="SaveMode"/> provided.</remarks>
        public async Task Save(SaveMode mode = SaveMode.MemoryOnly)
        {
            // Return if we aren't initialized yet to avoid saving with an uninitialized loader
            if (!IsInitialized)
            {
                // Log a warning if trying to save before initialization
                Debug.LogWarning("[Safekeeper]: Tried to save before the controller was initialized. Make sure to call `save.Initialize()` before saving. Aborting save operation.");

                // Aborting the save operation to avoid potential issues with an uninitialized loader
                return;
            }

            // Lock the semaphore to prevent other operations
            await Lock();

            // Notify stores and invoke OnSave if needed
            if (mode != SaveMode.PersistentOnly)
            {
                // Notify all registered stores to save their data
                SaveStoreRegistry.SaveWith(this);

                // Invoke the OnSave method for custom save logic
                OnSave();
            }

            // Save to persistent storage if needed
            if (mode != SaveMode.MemoryOnly) await _loader.Save(Data);

            // Unlock the semaphore and invoke the Saved event
            Unlock();

            // Await the next frame to ensure that all operations are completed before allowing any new ones
            await Task.Yield();
        }

        /// <summary>
        /// Load the game state.
        /// </summary>
        /// <remarks>Loads based on the <see cref="SaveMode"/> provided.</remarks>
        public async Task Load(SaveMode mode = SaveMode.MemoryOnly)
        {
            // Lock the semaphore to prevent other operations
            await Lock();

            // Load from persistent storage if needed
            if (mode != SaveMode.MemoryOnly) Data = await _loader.Load();

            // Notify stores and invoke OnLoad if needed
            if (mode != SaveMode.PersistentOnly)
            {
                // Notify all registered stores to load their data
                SaveStoreRegistry.LoadWith(this);

                // Invoke the OnLoad method for custom load logic
                OnLoad();
            }

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Load all data chunks associated with this save.
        /// </summary>
        /// <remarks>Loads all chunks regardless of the <see cref="SaveMode"/>.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of all loaded <see cref="ISaveData"/> chunks.</returns>
        public async Task<bool> TryLoadAll<T>(SaveLocation location, SerializableDictionary<int, T> targets) where T : new()
        {
            // Load all data chunks from persistent storage
            ISaveData[] allData = await _loader.LoadAll();

            // If no data chunks were loaded, return false
            if (allData.Length == 0) return false;

            // Try to read each chunk into the corresponding target
            targets.ReadAllTo(allData, location);

            // Return true indicating success
            return true;
        }

        /// <summary>
        /// Load all data chunks associated with this save.
        /// </summary>
        /// <remarks>Loads all chunks regardless of the <see cref="SaveMode"/>.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of all loaded <see cref="ISaveData"/> chunks.</returns>
        public async Task<bool> TryLoadAll<T>(SaveLocation location, List<T> targets) where T : new()
        {
            // Load all data chunks from persistent storage
            ISaveData[] allData = await _loader.LoadAll();

            // If no data chunks were loaded, return false
            if (allData.Length == 0) return false;

            // Try to read each chunk into the corresponding target
            targets.ReadAllTo(allData, location);

            // Return true indicating success
            return true;
        }

        /// <summary>
        /// Delete the save.
        /// </summary>
        public async Task Delete()
        {
            // Lock the semaphore to prevent other operations
            await Lock();

            // Check if the save exists
            Exists = await _loader.Exists();

            // If the save exists, delete it
            if (Exists)
            {
                // Mark the save as not existing
                Exists = false;

                // Invoke the OnDelete method for custom delete logic
                OnDelete();

                // Clear the data to avoid stale data access
                Data = null;

                // Delete the save from persistent storage
                await _loader.Delete();
            }

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Lock the semaphore and invokes the Saving event.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task Lock()
        {
            // Check if the lock exists
            if (_lock == null) throw new InvalidOperationException("SaveProvider.Lock: The lock semaphore is null.");

            // Wait to enter the semaphore
            await _lock.WaitAsync();

            // Invoke the Saving event
            Saving?.Invoke();
        }

        /// <summary>
        /// Unlock the semaphore and invokes the Saved event.
        /// </summary>
        private void Unlock()
        {
            // Check if the lock exists and is currently locked before releasing
            _lock.Release();

            // Invoke the Saved event
            Saved?.Invoke();
        }

        /// <summary>
        /// Sets the ID of save loader.
        /// </summary>
        /// <param name="id">The ID to set.</param>
        public void SetID(int id) => _loader.WithID(id);

        #endregion

        #region Protected Save Callbacks

        /// <summary>
        /// Invoked when the save is being saved to the memory.
        /// </summary>
        /// <remarks>
        /// This method is called after <see cref="ISaveStore"/>s are notified.
        /// It can be used to save global data unrelated to scenes.
        /// </remarks>
        protected virtual void OnSave() { }

        /// <summary>
        /// Invoked when the save is loaded from the memory.
        /// </summary>
        /// <remarks>
        /// This method is guaranteed to be called before the
        /// <see cref="ISaveStore"/>s are notified.
        /// It can be used to load global data unrelated to scenes.
        /// </remarks>
        protected virtual void OnLoad() { }

        /// <summary>
        /// Invoked when the save is deleted.
        /// </summary>
        protected virtual void OnDelete() { }

        #endregion
    }
}