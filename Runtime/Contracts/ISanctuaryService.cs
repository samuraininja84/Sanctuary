using System.Threading.Tasks;

namespace Sanctuary
{
    public interface ISanctuaryService
    {
        /// <summary>
        /// Registers a migration step that can be used to migrate save data from one version to another.
        /// </summary>
        /// <param name="step">The migration step to register.</param>
        void RegisterMigrationStep(ISaveMigrationStep step);

        /// <summary>
        /// Saves the specified data to the specified save slot. If the slot already exists, it will be overwritten.
        /// </summary>
        /// <typeparam name="T">The type of the data to save.</typeparam>
        /// <param name="slotId">The ID of the save slot.</param>
        /// <param name="data">The data to save.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the result of the save operation.</returns>
        Task<SaveResult> SaveAsync<T>(string slotId, T data) where T : class;

        /// <summary>
        /// Loads the data from the specified save slot. If the slot does not exist, a failed LoadResult will be returned.
        /// </summary>
        /// <typeparam name="T">The type of the data to load.</typeparam>
        /// <param name="slotId">The ID of the save slot.</param>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the result of the load operation.</returns>
        Task<LoadResult<T>> LoadAsync<T>(string slotId) where T : class;

        /// <summary>
        /// Deletes the specified save slot. If the slot does not exist, the operation will return false.
        /// </summary>
        /// <param name="slotId">The ID of the save slot to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result indicates whether the delete was successful.</returns>
        Task<bool> DeleteAsync(string slotId);

        /// <summary>
        /// Checks if the specified save slot exists.
        /// </summary>
        /// <param name="slotId">The ID of the save slot to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the save slot exists.</returns>
        Task<bool> ExistsAsync(string slotId);
        
        /// <summary>
        /// Gets information about the specified save slot.
        /// </summary>
        /// <param name="slotId">The ID of the save slot.</param>
        /// <returns>The information about the save slot.</returns>
        SaveSlotInfo GetSlot(string slotId);
        
        /// <summary>
        /// Gets information about all available save slots.
        /// </summary>
        /// <returns>An array of information about all available save slots.</returns>
        SaveSlotInfo[] GetAvailableSlots();

        /// <summary>
        /// Loads the save slot registry from the configured data provider.
        /// </summary>
        /// <returns>A task that represents the asynchronous load operation. The task result indicates whether the load was successful.</returns>
        Task<bool> TryLoadRegistryAsync();
    }
}