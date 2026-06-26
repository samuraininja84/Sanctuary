using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Utility;
using Sanctuary.Configuration;
using Sanctuary.Serialization;
using StreamType = Sanctuary.Configuration.StreamConfiguration.StreamType;

namespace Sanctuary.Loaders
{
    /// <summary>
    /// Default implementation of <see cref="ISaveLoader"/> that saves and loads data to and from files on the local file system.
    /// </summary>
    /// <remarks>
    /// This class provides a thread-safe implementation for saving and loading data using a semaphore to ensure only one operation is performed at a time.
    /// Used as a default save loader for the <see cref="SaveProvider"/> and can be easily customized by changing the file extension, backup settings, and profile ID through method chaining.
    /// The file structure is organized based on the profile's scope and ID, allowing for easy management of multiple save files.
    /// Defaulted to when a <see cref="BaseFileSaveLoader"/> is not specified on the <see cref="SaveProvider"/>. 
    /// </remarks>
    public sealed class FileSaveLoader : ISaveLoader 
    {
        private readonly string _name;
        private readonly string _directory;

        private readonly ProfileData _profile;
        private readonly ISerializer _serializer;
        private string _filePath = string.Empty;
        private string _folderPath = string.Empty;

        /// <summary>
        /// The file name derived from the profile data.
        /// </summary>
        private string FileName => _profile.GetFileName();

        /// <summary>
        /// Semaphore used to ensure thread-safe access to save and load operations as in only one operation is performed at a time.
        /// </summary>
        private readonly SemaphoreSlim _lock = new(1);

        // To Do: Remove default folder name and file name from the constructor, as they are now derived from the profile data and serializer.

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSaveLoader"/> class with the specified profile and serializer.
        /// </summary>
        /// <remarks>Only intended to be used by the <see cref="Builder"/> class, use the <see cref="Builder.Create(ProfileData, ISerializer)"/> method to create an instance of this class.</remarks>
        /// <param name="profile">The profile data used to determine the save file's scope and ID.</param>
        /// <param name="serializer">The serializer used to serialize and deserialize the save data.</param>
        private FileSaveLoader(ProfileData profile, ISerializer serializer)
        {
            // Store the profile.
            _profile = profile;

            // Initialize the serializer, should be provided by the user, otherwise default to a binary serializer through the builder.
            _serializer = serializer;

            // Set the name to a more user-friendly format.
            _name = $"File Save \"{FileName}\"";

            // Set the directory to a "Save Data" folder in the persistent data path.
            _directory = Path.Combine(Application.persistentDataPath, SaveLoaderDefaults.DefaultFolderName);

            // Get the scoped directory based on the profile scope and ID.
            _folderPath = _profile.GetScopedPath(_directory);

            // Set the file path to the specified file name with the serializer's file extension.
            _filePath = Path.Combine(_folderPath, FileName + _serializer.GetFileExtension());
        }

        /// <summary>
        /// Builder class for constructing instances of <see cref="FileSaveLoader"/> with optional customization.
        /// </summary>
        public readonly struct Builder
        {
            private readonly ProfileData profile;
            private readonly ISerializer serializer;

            private Builder(ProfileData profile, ISerializer serializer = null)
            {
                // Store the profile and serializer.
                this.profile = profile;

                // If no serializer is provided, use the default binary serializer as a fallback.
                this.serializer = serializer ?? BinarySerializer.Default;
            }

            public static Builder Create(ProfileData profile, ISerializer serializer = null) => new(profile, serializer);

            public readonly FileSaveLoader Build() => new(profile, serializer);
        }

        // To Do: Remove the WithID method and handle the ID look up through the load method, as this will allow for a more flexible and dynamic approach to loading save data without requiring the user to specify an ID beforehand.

        /// <summary>
        /// Sets the profile ID for this save loader.
        /// </summary>
        /// <param name="id">The profile ID to set.</param>
        /// <returns>An instance of <see cref="ISaveLoader"/> with the specified profile ID set.</returns>
        public ISaveLoader WithID(int id = -1)
        {
            // Change the profile ID.
            _profile.SetId(id);

            // Change the folder path to the new profile.
            _folderPath = _profile.GetScopedPath(_directory);

            // Update the file path to include the new profile and the serializer's file extension.
            _filePath = Path.Combine(_folderPath, FileName + _serializer.GetFileExtension());

            // Return the current instance for method chaining.
            return this;
        }

        /// <summary>
        /// Creates a new empty save data object.
        /// </summary>
        /// <returns>A task that represents the asynchronous create operation. The task result contains the new save data object.</returns>
        public Task<ISaveData> Create() => Task.FromResult((ISaveData)SaveData.Empty);

        /// <summary>
        /// Saves the given data to the file asynchronously.
        /// </summary>
        /// <remarks>
        /// Saving is done in a thread-safe manner using a semaphore to prevent multiple operations to interfere with each other.
        /// The method writes the data to the file in chunks, where each chunk contains a set of key-value pairs before moving on to the next chunk.
        /// After all chunks have been written, a boolean value is written to indicate the end of the data.
        /// <param name="data">The data to save.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task Save(StreamConfiguration config, ISaveData data) 
        {
            // Acquire the lock.
            await _lock.WaitAsync();

            // Create a file serialization stream for the specified file path.
            using var stream = await config.GetStream(StreamType.Serialization, _filePath);

            // Write the data to the file asynchronously using the serializer.
            await _serializer.Serialize(data, stream);

            // Create a backup of the file if the setting is enabled.
            // To Do: Find a better way to handle this, as there is a potential mismatch if backups are enabled but the format is not compatible with the backup file extension.
            // This could lead to confusion or errors when attempting to restore from a backup.
            if (_serializer.Options.HasFlag(SerializationOptions.Backup)) File.Copy(_filePath, _filePath + SerializationExtensions.BackupFileExtension, true);

            // Release the lock.
            _lock.Release();
        }

        /// <summary>
        /// Asynchronously loads the save data from the specified <see cref="StreamConfiguration"/>.
        /// </summary>
        /// <param name="config">The <see cref="StreamConfiguration"/> to load the data from.</param>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the <see cref="LoadResult"/>.</returns>
        public async Task<LoadResult> Load(StreamConfiguration config)
        {
            // Acquire the lock.
            await _lock.WaitAsync();

            // Create a file deserialization stream for the specified file path.
            using var stream = await config.GetStream(StreamType.Deserialization, _filePath);

            // Try to deserialize the save data using the binary serializer. If it fails, log an error and return a new empty save data object.
            var result = await _serializer.Deserialize(stream);

            // Release the lock.
            _lock.Release();

            // Return the loaded save data.
            return result;
        }

        /// <summary>
        /// Asynchronously loads all existing save data files for the current profile's scope.
        /// </summary>
        /// <remarks>This method loads all existing save data files for the current profile's scope.</remarks>
        /// <returns>An array of loaded save data objects.</returns>
        public async Task<LoadResult[]> LoadAll(StreamConfiguration config) 
        {
            // Return early if the scope is not Global or Scene
            if (!(_profile.GetScope() == SaveScope.Global || _profile.GetScope() == SaveScope.Scene)) return Array.Empty<LoadResult>();

            // Get the existing saves for the current profile's scope using the provided stream configuration.
            var streams = await config.GetStreams();

            // If there are no existing IDs, return an empty array.
            if (streams.Length == 0) return Array.Empty<LoadResult>();

            // Create a list to hold the results of the load operations.
            var results = new System.Collections.Generic.List<LoadResult>();

            // Define a local function to load a single save data object from a stream.
            async Task<LoadResult> Load(Stream stream)
            {
                // Acquire the lock.
                await _lock.WaitAsync();

                // Try to deserialize the save data using the binary serializer. If it fails, log an error and return a new empty save data object.
                var result = await _serializer.Deserialize(stream);

                // Release the lock.
                _lock.Release();

                // Return the loaded save data.
                return result;
            }

            // Load each save data object.
            foreach (var stream in streams) 
            {
                // Load the save data using the new stream and add it to the results list.
                var result = await Load(stream);

                // Add the loaded save data to the list if the load was successful.
                results.Add(result);
            }

            // Return the array of loaded save data objects.
            return results.ToArray();
        }

        // To Do: Remove the parameter-less Delete method to handle streams that are not file-based, as this method currently only works for file-based saves.

        /// <summary>
        /// Deletes the save file.
        /// </summary>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task Delete()
        {
            // Acquire the lock.
            await _lock.WaitAsync();

            // Delete the file if it exists.
            if (File.Exists(_filePath)) File.Delete(_filePath);

            // Get the backup file path.
            var backupFilePath = GetBackupFilePath();

            // Delete the backup file if it exists.
            if (File.Exists(backupFilePath)) File.Delete(backupFilePath);

            // Check if the scoped path exists
            if (!Directory.Exists(_folderPath)) 
            {
                // Release the lock.
                _lock.Release();

                // Indicate that the delete operation was semi-successful.
                return; 
            }

            // Create a DirectoryInfo object for the scoped path
            var scopedDirectory = new DirectoryInfo(_folderPath);

            // If there are no files left in the directory, delete the directory
            if (!scopedDirectory.HasFiles())
            {
                // Log the deletion of the empty directory
                Debug.Log($"[Sanctuary]: {scopedDirectory.FullName} is empty, deleting it.");

                // Delete the empty directory
                Directory.Delete(scopedDirectory.FullName, true);

                // If the profile's scope is Global or Scene, check if there are any files or directories left in the parent directory, if not, delete it as well
                if (_profile.GetScope() == SaveScope.Global || _profile.GetScope() == SaveScope.Scene)
                {
                    // Get the parent directory of the scoped path
                    DirectoryInfo parentDirectory = Directory.GetParent(_folderPath);

                    // If the parent directory is empty, delete it
                    if (!parentDirectory.HasContents())
                    {
                        // Log the deletion of the empty directory
                        Debug.Log($"[Sanctuary]: {parentDirectory.FullName} is empty, deleting it.");

                        // Delete the empty parent directory
                        Directory.Delete(parentDirectory.FullName, true);
                    }
                }
            }

            // Release the lock.
            _lock.Release();
        }

        /// <summary>
        /// Asynchronously gets the name of the save.
        /// </summary>
        /// <returns>Gets the name of the save.</returns>
        public Task<string> GetName() => Task.FromResult(_name);

        /// <summary>
        /// Gets the file extension for this save.
        /// </summary>
        /// <returns>The file extension for this save.</returns>
        public string GetExtension() => _serializer.GetFileExtension();

        // To Do: Update Exists, GetLastModifiedTime, & ExistingSaveIDs to handle streams that are not file-based, as these methods currently only work for file-based saves.
        // This may require a different approach or additional parameters to handle non-file-based streams.

        /// <summary>
        /// Asynchronously checks if the save file exists.
        /// </summary>
        /// <returns>A task that represents the asynchronous existence check operation. The task result contains true if the file exists, false otherwise.</returns>
        public Task<bool> Exists() => Task.FromResult(File.Exists(_filePath));

        /// <summary>
        /// Asynchronously gets the last modified time of the save file.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last modified time.</returns>
        public Task<TimeSpan> GetLastModifiedTime() => Task.FromResult(File.GetLastWriteTimeUtc(_filePath).TimeOfDay);

        /// <summary>
        /// Gets the backup file path by appending the backup file extension to the original file path.
        /// </summary>
        /// <returns>The backup file path.</returns>
        private string GetBackupFilePath() => _filePath + SerializationExtensions.BackupFileExtension;
    }
}