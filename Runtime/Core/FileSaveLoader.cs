using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Sanctuary.Utility;
using Sanctuary.Serializers;

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

            // Set the directory to a "Save Data" folder in the persistent data path.
            _directory = Path.Combine(Application.persistentDataPath, SaveLoaderDefaults.DefaultFolderName);

            // Get the scoped directory based on the profile scope and ID.
            _folderPath = _profile.GetScopedPath(_directory);

            // Set the file path to the specified file name with the serializer's file extension.
            _filePath = Path.Combine(_folderPath, FileName + _serializer.GetFileExtension());

            // Set the name to a more user-friendly format.
            _name = $"File Save \"{FileName}\"";
        }

        /// <summary>
        /// Builder class for constructing instances of <see cref="FileSaveLoader"/> with optional customization.
        /// </summary>
        public readonly struct Builder
        {
            private readonly ProfileData _profile;
            private readonly ISerializer _serializer;

            private Builder(ProfileData profile, ISerializer serializer = null)
            {
                // Store the profile and serializer.
                _profile = profile;

                // If no serializer is provided, use the default binary serializer as a fallback.
                _serializer = serializer ?? BinarySerializer.Default;
            }

            public static Builder Create(ProfileData profile, ISerializer serializer = null) => new(profile, serializer);

            public readonly FileSaveLoader Build() => new(_profile, _serializer);
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
        public async Task Save(ISaveData data) 
        {
            // Acquire the lock.
            await _lock.WaitAsync();

            // Create a file serialization stream for the specified file path.
            using var stream = SerializationExtensions.CreateFileSerializationStream(_filePath);

            // Write the data to the file asynchronously using the serializer.
            await _serializer.Serialize(data, stream);

            // Create a backup of the file if the setting is enabled.
            // To Do: Find a better way to handle this, as there is a potential mismatch if backups are enabled but the format is not compatible with the backup file extension.
            // This could lead to confusion or errors when attempting to restore from a backup.
            if (_serializer.Options.HasFlag(SerializationOptions.Backup)) File.Copy(_filePath, _filePath + SerializationExtensions.BackupFileExtension, true);

            // Release the lock.
            _lock.Release();
        }

        // To Do: Remove the parameter-less Load method to handle streams that are not file-based, as this method currently only works for file-based saves.
        // This may require a different approach or additional parameters to handle non-file-based streams.

        public async Task<LoadResult> Load()
        {
            // Create a file deserialization stream for the specified file path.
            using var stream = await SerializationExtensions.CreateFileDeserializationStream(_filePath);

            // Load the save data using the new stream and return the result.
            return await Load(stream);
        }

        /// <summary>
        /// Asynchronously gets the <see cref="LoadResult"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load the data from.</param>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the <see cref="LoadResult"/>.</returns>
        public async Task<LoadResult> Load(Stream stream)
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

        /// <summary>
        /// Asynchronously loads all existing save data files for the current profile's scope.
        /// </summary>
        /// <remarks>This method loads all existing save data files for the current profile's scope.</remarks>
        /// <returns>An array of loaded save data objects.</returns>
        public async Task<LoadResult[]> LoadAll() 
        {
            // Return early if the scope is not Global or Scene
            if (!(_profile.GetScope() == SaveScope.Global || _profile.GetScope() == SaveScope.Scene)) return Array.Empty<LoadResult>();

            // Get the existing save IDs.
            int[] existingIds = await ExistingSaveIDs();

            // If there are no existing IDs, return an empty array.
            if (existingIds.Length == 0) return Array.Empty<LoadResult>();

            // Create a list to hold the results of the load operations.
            var results = new System.Collections.Generic.List<LoadResult>();

            // Load each save data object.
            foreach (int id in existingIds) 
            {
                // Set the file path for the current ID.
                string folderPath = Path.Combine(_directory, $"{id}");

                // Update the file path to include the new profile and the serializer's file extension.
                string filePath = Path.Combine(folderPath, FileName + _serializer.GetFileExtension());

                // Create a file deserialization stream for the specified file path.
                using var stream = await SerializationExtensions.CreateFileDeserializationStream(filePath);

                // Load the save data using the new stream and add it to the results list.
                var result = await Load(stream);

                // Add the loaded save data to the list if the load was successful.
                results.Add(result);
            }

            // Return the array of loaded save data objects.
            return results.ToArray();
        }

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
            DirectoryInfo scopedDirectory = new DirectoryInfo(_folderPath);

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
        /// Gets the existing save IDs by scanning the save directory for folders named with integer IDs.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of existing save IDs.</returns>
        private Task<int[]> ExistingSaveIDs()
        {
            // Construct the path to the existing saves directory.
            string existingSavesPath = Path.Combine(Application.persistentDataPath, SaveLoaderDefaults.DefaultFolderName);

            // Ensure the directory exists.
            if (!Directory.Exists(existingSavesPath)) return Task.FromResult(Array.Empty<int>());

            // Create a list to hold the existing save IDs.
            var ids = new System.Collections.Generic.List<int>();

            // Get all folders in the saves directory
            var savesDirectory = new DirectoryInfo(existingSavesPath);

            // Iterate through each directory in the saves directory
            foreach (var dir in savesDirectory.GetDirectories())
            {
                // Try to parse the directory name as an integer ID
                if (int.TryParse(dir.Name, out int id))
                {
                    // If successful, add the ID to the list
                    ids.Add(id);
                }
            }

            // Return the array of existing save IDs.
            return Task.FromResult(ids.ToArray());
        }

        /// <summary>
        /// Gets the backup file path by appending the backup file extension to the original file path.
        /// </summary>
        /// <returns>The backup file path.</returns>
        private string GetBackupFilePath() => _filePath + SerializationExtensions.BackupFileExtension;
    }
}