using System.Threading.Tasks;
using Sanctuary.Configuration;
using Array = System.Array;
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
        /// This method is responsible for creating the initial save data for this save. 
        /// It should be called when the save is first created, and it should return an <see cref="ISaveData"/> object that represents the initial state of the save.
        /// </remarks>
        /// <returns>The <see cref="ISaveData"/> representing the initial state of the save.</returns>
        Task<ISaveData> Create();

        /// <summary>
        /// Save the data to the specified <see cref="StreamConfiguration"/>.
        /// </summary>
        /// <param name="config">The <see cref="StreamConfiguration"/> to use for saving the data.</param>
        /// <param name="data">The data to save as an <see cref="ISaveData"/> object.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task Save(StreamConfiguration config, ISaveData data);

        /// <summary>
        /// Load the data from the specified <see cref="StreamConfiguration"/>.
        /// </summary>
        /// <param name="config">The <see cref="StreamConfiguration"/> to load the data from.</param>
        /// <returns>The <see cref="LoadResult"/> representing the result of the load operation.</returns>
        Task<LoadResult> Load(StreamConfiguration config) => Task.FromResult(LoadResult.Failure());

        /// <summary>
        /// Load all saves from the specified <see cref="StreamConfiguration"/>.
        /// </summary>
        /// <param name="config">The <see cref="StreamConfiguration"/> to use for loading the data.</param>
        /// <returns>A list of all loaded saves as <see cref="LoadResult"/> objects.</returns>
        virtual Task<LoadResult[]> LoadAll(StreamConfiguration config) => Task.FromResult(Array.Empty<LoadResult>());

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
