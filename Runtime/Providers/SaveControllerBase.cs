using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Stores;
using Sanctuary.Extensions;

namespace Sanctuary
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    /// <summary>
    /// The main controller for managing game saves.
    /// </summary>
    /// <remarks>
    /// This class provides methods to create, load, save, and delete game saves.
    /// Any class implementing <see cref="ISaveStore"/> can register to be notified for save and load events.
    /// This class uses an <see cref="ISaveLoader"/> to handle the actual loading and saving of data regardless of type.
    /// </remarks>
    public abstract class SaveControllerBase : MonoBehaviour, ISaveController
    {
        protected ISanctuaryService _service;
        protected ISaveData _data;

        /// <summary>
        /// Gets the save data. If the data is accessed before being loaded, a warning is logged and an empty data is returned as a placeholder.
        /// </summary>
        public ISaveData Data
        {
            get
            {
                // If the data is null, log an error and return an empty data
                if (_data == null)
                {
                    // Log a warning if the data is accessed before being loaded
                    Debug.LogWarning("[Sanctuary]: Tried to access the data before the save was loaded.\n Make sure to call `save.Load(SaveMode.Full)` before accessing the data. Returning empty data as a placeholder.");

                    // Return an empty data to avoid null reference exceptions
                    _data = new SaveData();
                }

                // Return the data
                return _data;
            }

            // Get the data from the value
            protected set => _data = value;
        }

        public abstract string Name { get; }

        public abstract string SlotID { get; protected set; }

        public bool Exists { get; protected set; }

        public event Action Saving = delegate { };

        public event Action Saved = delegate { };

        public virtual bool Initialized => _isInitialized && Exists;

        protected bool _isInitialized = false;

        protected SemaphoreSlim _lock = new(1);

        #region Configuration

        private void Awake() => Initialize();

        /// <summary>
        /// Configures the save controller with the provided save loader and stream configuration.
        /// </summary>
        /// <param name="loader">The save loader to use for loading and saving data.</param>
        /// <param name="configuration">The stream configuration to use for saving and loading data.</param>
        /// <exception cref="ArgumentNullException">Thrown if the loader or configuration is null.</exception>
        public void Configure(ISanctuaryService service)
        {
            // Check if the service is null and throw an exception if it is
            _service = service ?? throw new ArgumentNullException(nameof(service), "SaveControllerBase.Configure: The save service cannot be null.");
        }

        /// <summary>
        /// Initialize this save controller.
        /// </summary>
        /// <remarks>
        /// This process loads the basic information about the save, such as its
        /// name and whether it exists.
        /// It does not load the data itself.
        /// </remarks>
        public virtual async void Initialize() 
        {
            // If already initialized, do nothing
            if (_isInitialized) return;

            // Intialize the save controller
            _isInitialized = true;

            // Invoke the PrepareInit method for any pre-initialization logic
            PreInit();

            // Lock the semaphore to prevent other operations
            await Lock();

            // Load the registry to ensure that the save exists in the registry
            var registryLoaded = await _service.TryLoadRegistryAsync();

            // Check if the registry was loaded successfully
            var fileExists = await _service.ExistsAsync(SlotID);

            // Check if the save exists in the registry and in the file system
            Exists = registryLoaded && fileExists;

            // Unlock the semaphore and invoke the Saved event
            Unlock();

            // Invoke the OnInit method for custom initialization logic
            PostInit();
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Save the game state. 
        /// </summary>
        /// <remarks>Saves based on the <see cref="SaveMode"/> provided.</remarks>
        public virtual async Task Save(string slotID = null)
        {
            // Lock the semaphore to prevent other operations
            await Lock();

            // If the save doesn't exist, create it
            if (!Exists)
            {
                // Mark the save as existing
                Exists = true;

                // Create a new save data to avoid null reference exceptions
                Data = new SaveData();

                // Invoke the OnLoad method for custom load logic
                OnLoad();
            }

            // Notify all registered stores to save their data
            SaveStoreRegistry.SaveWith(this);

            // If a new slot ID is provided, update the SlotID
            if (!string.IsNullOrEmpty(slotID)) SlotID = slotID;

            // Save the data to persistent storage if needed
            await _service.SaveAsync(SlotID, Data);

            // Invoke the OnSave method for custom save logic
            OnSave();

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Load the game state.
        /// </summary>
        /// <remarks>Loads based on the <see cref="SaveMode"/> provided.</remarks>
        public virtual async Task Load(string slotID = null)
        {
            // Lock the semaphore to prevent other operations
            await Lock();

            // If a new slot ID is provided, update the SlotID
            if (!string.IsNullOrEmpty(slotID)) SlotID = slotID;

            // Load the save data
            var result = await _service.LoadAsync<SaveData>(SlotID);

            // Handle the result of the load operation
            switch (result.Status)
            {
                case LoadStatus.Success:
                    Data = result.Data;
                    break;
                case LoadStatus.SuccessFromBackup:
                    Data = result.Data;
                    break;
                case LoadStatus.SuccessMigrated:
                    Data = result.Data;
                    break;
                case LoadStatus.SuccessMigratedFromBackup:
                    Data = result.Data;
                    break;
                case LoadStatus.NoValidSave:
                    Debug.LogWarning($"[Sanctuary]: Failed to load save '{name}' from persistent storage. {result.Message}");
                    break;
                case LoadStatus.ProviderError:
                    Debug.LogError($"[Sanctuary]: Failed to load save '{name}' from persistent storage due to a provider error. {result.Message}");
                    break;
                case LoadStatus.MigrationFailed:
                    Debug.LogError($"[Sanctuary]: Failed to migrate save '{name}' from persistent storage. {result.Message}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Notify all registered stores to load their data
            SaveStoreRegistry.LoadWith(this);

            // Invoke the OnLoad method for custom load logic
            OnLoad();

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Delete the save.
        /// </summary>
        public virtual async Task Delete(string slotID = null)
        {
            // Lock the semaphore to prevent other operations
            await Lock();

            // If a new slot ID is provided, update the SlotID
            if (!string.IsNullOrEmpty(slotID)) SlotID = slotID;

            // Check if the save exists
            Exists = await _service.ExistsAsync(SlotID);

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
                await _service.DeleteAsync(SlotID);
            }

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Delete all saves.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual async Task DeleteAll()
        {
            // Lock the semaphore to prevent
            await Lock();

            // Delete all saves from persistent storage
            await _service.DeleteAllAsync();

            // Unlock the semaphore and invoke the Saved event
            Unlock();
        }

        /// <summary>
        /// Get a list of available save slots.
        /// </summary>
        /// <returns>An array of <see cref="SaveSlotInfo"/> representing the available save slots.</returns>
        public virtual SaveSlotInfo[] GetAvailableSlots() => _service.GetAvailableSlots();

        /// <summary>
        /// Lock the semaphore and invokes the Saving event.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task Lock()
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
        protected void Unlock()
        {
            // Check if the lock exists and is currently locked before releasing
            _lock.Release();

            // Invoke the Saved event
            Saved?.Invoke();
        }

        #endregion

        #region Protected Save Callbacks

        /// <summary>
        /// Sets the ID of the save slot. This is used to identify different save slots for the same game.
        /// </summary>
        /// <param name="slotID">The ID of the save slot.</param>
        public abstract void SetID(string slotID);

        /// <summary>
        /// Invoked before the save controller is initialized.
        /// </summary>
        protected virtual void PreInit() { }

        /// <summary>
        /// Invoked when the save controller is initialized.
        /// </summary>
        protected virtual void PostInit() { }

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

    public interface ISaveController
    {
        string Name { get; }

        bool Initialized { get; }

        bool Exists { get; }

        ISaveData Data { get; }

        void SetID(string slotID);

        Task Save(string slotID = null);

        Task Load(string slotID = null);

        Task Delete(string slotID = null);

        Task DeleteAll();

        SaveSlotInfo[] GetAvailableSlots();
    }
}