using System.Threading.Tasks;
using TimeSpan = System.TimeSpan;

namespace Sanctuary.Loaders 
{
    /// <summary>
    /// A common interface for saving and loading data in the persistent storage.
    /// </summary>
    public interface ISaveLoader 
    {
        /// <summary>
        /// Sets the file extension for this save loader.
        /// </summary>
        /// <param name="extension">The file extension to set.</param>
        /// <returns>An instance of <see cref="ISaveLoader"/> with the specified file extension set.</returns>
        virtual ISaveLoader WithExtension(string extension) => this;

        /// <summary>
        /// Sets whether to create a backup file when saving.
        /// </summary>
        /// <param name="createBackup">The flag indicating whether to create a backup file.</param>
        /// <returns>The current instance of <see cref="FileSaveLoader"/> with the updated backup setting.</returns>
        virtual ISaveLoader WithBackup(bool createBackup) => this;

        /// <summary>
        /// Sets the encryption key for the <see cref="ISaveLoader"/> instance.
        /// </summary>
        /// <param name="key">The encryption key to be used. Cannot be null or empty.</param>
        /// <returns>The current instance of <see cref="ISaveLoader"/> with the specified encryption key set.</returns>
        virtual ISaveLoader WithKey(string key) => this;

        /// <summary>
        /// Sets the profile ID for this save loader.
        /// </summary>
        /// <param name="id">The profile ID to set.</param>
        /// <returns>An instance of <see cref="ISaveLoader"/> with the specified profile ID set.</returns>
        virtual ISaveLoader WithID(int id = -1) => this;

        /// <summary>
        /// Create the save data for this save.
        /// </summary>
        /// <remarks>
        /// This method is used if the save does not exist yet.
        /// It should not save the data to the persistent storage, only create it.
        /// This data will later be passed to <see cref="Save"/>.
        /// </remarks>
        /// <returns>The newly created save data.</returns>
        Task<ISaveData> Create();

        /// <summary>
        /// Save the data to the persistent storage.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task Save(ISaveData data);

        /// <summary>
        /// Load the data from the persistent storage.
        /// </summary>
        /// <returns>The loaded data.</returns>
        Task<ISaveData> Load();

        /// <summary>
        /// Load the data from a specific file path.
        /// </summary>
        /// <param name="filePath">The file path to load the save data from.</param>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded save data.</returns>
        virtual Task<ISaveData> LoadAt(string filePath) => Task.FromResult<ISaveData>(null);

        /// <summary>
        /// Load all saves from the persistent storage.
        /// </summary>
        /// <returns>A list of all loaded saves.</returns>
        virtual Task<ISaveData[]> LoadAll() => Task.FromResult(new ISaveData[0]);

        /// <summary>
        /// Attempt to rollback from a backup save.
        /// </summary>
        /// <returns>True if the rollback was successful; otherwise, false.</returns>
        virtual Task<bool> AttemptRollback() => Task.FromResult(false);

        /// <summary>
        /// Remove this save from the persistent storage.
        /// </summary>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task Delete();

        /// <summary>
        /// Get the name of this save.
        /// </summary>
        /// <remarks>
        /// Used exclusively for debugging purposes.
        /// </remarks>
        /// <returns>The name of this save.</returns>
        Task<string> GetName();

        /// <summary>
        /// Asynchronously gets the last modified time of the save file.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last modified time.</returns>
        virtual Task<TimeSpan> GetLastModifiedTime() => Task.FromResult(TimeSpan.Zero);

        /// <summary>
        /// Check if the save has already been created.
        /// </summary>
        /// <returns>Whether the save exists.</returns>
        Task<bool> Exists();
    }
}
