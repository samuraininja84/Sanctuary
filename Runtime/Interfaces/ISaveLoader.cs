using System.Threading.Tasks;
using Stream = System.IO.Stream;
using TimeSpan = System.TimeSpan;

namespace Sanctuary.Loaders 
{
    /// <summary>
    /// A common interface for saving and loading data in the persistent storage.
    /// </summary>
    public interface ISaveLoader 
    {
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
        /// <param name="stream">The stream to load the data from.</param>
        /// <returns>The <see cref="LoadResult"/> representing the result of the load operation.</returns>
        Task<LoadResult> Load();

        /// <summary>
        /// Load the data from the persistent storage.
        /// </summary>
        /// <param name="stream">The stream to load the data from.</param>
        /// <returns>The <see cref="LoadResult"/> representing the result of the load operation.</returns>
        virtual Task<LoadResult> Load(Stream stream) => Task.FromResult(LoadResult.Failure());

        /// <summary>
        /// Load all saves from the persistent storage.
        /// </summary>
        /// <returns>A list of all loaded saves as <see cref="LoadResult"/> objects.</returns>
        virtual Task<LoadResult[]> LoadAll() => Task.FromResult(new LoadResult[0]);

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
        /// Check if the save has already been created.
        /// </summary>
        /// <returns>Whether the save exists.</returns>
        Task<bool> Exists();

        /// <summary>
        /// Asynchronously gets the last modified time of the save file.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last modified time.</returns>
        virtual Task<TimeSpan> GetLastModifiedTime() => Task.FromResult(TimeSpan.Zero);
    }
}
