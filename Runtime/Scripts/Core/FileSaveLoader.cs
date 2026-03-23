using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Sanctuary.Loaders
{
    /// <summary>
    /// Stores saves in the persistent data path. 
    /// </summary>
    /// <remarks>
    /// The saves are stored locally in a `Save Data` directory located in the persistent data path.
    /// </remarks>
    public class FileSaveLoader : ISaveLoader 
    {
        private readonly string _name;
        private readonly string _directory;

        private ProfileData _profile;
        public readonly ISerializer serializer;

        private string _filePath = string.Empty;
        private string _folderPath = string.Empty;
        private string _fileExtension = ".data";
        private bool _backupAllowed = true;

        /// <summary>
        /// The file name derived from the profile data.
        /// </summary>
        private string fileName => _profile.GetFileName();

        /// <summary>
        /// Semaphore used to ensure thread-safe access to save and load operations as in only one operation is performed at a time.
        /// </summary>
        private readonly SemaphoreSlim _lock = new(1);

        /// <summary>
        /// Represents the file extension used for backup files.
        /// </summary>
        public const string BackupFileExtension = ".bak";

        /// <summary>
        /// Represents the default folder name used for saving files.
        /// </summary>
        public const string DefaultFolderName = "Save Data";

        public FileSaveLoader(ProfileData profile)
        {
            // Store the profile.
            _profile = profile;

            // Set the directory to a "Save Data" folder in the persistent data path.
            _directory = Path.Combine(Application.persistentDataPath, DefaultFolderName);

            // Get the scoped directory based on the profile scope and ID.
            _folderPath = _profile.GetScopedPath(_directory);

            // Set the file path to the specified file name with a .data extension.
            _filePath = Path.Combine(_folderPath, fileName + _fileExtension);

            // Set the name to a more user-friendly format.
            _name = $"File Save \"{fileName}\"";
        }

        /// <summary>
        /// Sets the file extension for this save loader.
        /// </summary>
        /// <param name="extension">The file extension to set.</param>
        /// <returns>An instance of <see cref="ISaveLoader"/> with the specified file extension set.</returns>
        public ISaveLoader WithExtension(string extension) 
        {
            // Update the file extension.
            _fileExtension = extension;

            // Update the file path to include the new extension.
            _filePath = Path.ChangeExtension(_filePath, _fileExtension);

            // Return the current instance for method chaining.
            return this;
        }

        /// <summary>
        /// Sets whether to create a backup file when saving.
        /// </summary>
        /// <param name="createBackup">The flag indicating whether to create a backup file.</param>
        /// <returns>The current instance of <see cref="FileSaveLoader"/> with the updated backup setting.</returns>
        public ISaveLoader WithBackup(bool createBackup)
        {
            // Update the create backup setting.
            _backupAllowed = createBackup;

            // Return the current instance for method chaining.
            return this;
        }

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

            // Update the file path to include the new profile.
            _filePath = Path.Combine(_folderPath, fileName + _fileExtension);

            // Return the current instance for method chaining.
            return this;
        }

        /// <summary>
        /// Creates a new empty save data object.
        /// </summary>
        /// <returns>A task that represents the asynchronous create operation. The task result contains the new save data object.</returns>
        public Task<ISaveData> Create() => Task.FromResult((ISaveData)new SaveData());

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

            // Write the data to the file asynchronously.
            await Task.Run
            (
                () => 
                {
                    // Ensure the folder path exists.
                    if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);

                    // Create a file stream to write to the file.
                    using var saveStream = new FileStream(_filePath, FileMode.Create);

                    // Create a binary writer to write to the file.
                    using var writer = new BinaryWriter(saveStream, Encoding.UTF8, false);

                    // Write each chunk of data.
                    foreach (var chunkId in data.GetChunkIDs()) 
                    {
                        // Get the chunk data.
                        var chunk = data.GetChunk(chunkId);

                        // Write a true boolean to indicate a chunk follows.
                        writer.Write(true);

                        // Write the chunk ID and the number of key-value pairs in the chunk.
                        writer.Write(chunkId);

                        // Write the number of key-value pairs in the chunk.
                        writer.Write(chunk.Count);

                        // Write each key-value pair in the chunk.
                        foreach (var (key, value) in chunk) 
                        {
                            // Write the key.to the file.
                            writer.Write(key);

                            // Write the value to the file.
                            writer.Write(value);
                        }
                    }

                    // Write a false boolean to indicate the end of chunks.
                    writer.Write(false);
                }
            );

            // Create a backup of the file if the setting is enabled.
            if (_backupAllowed) File.Copy(_filePath, GetBackupFilePath(), true);

            // Release the lock.
            _lock.Release();
        }

        /// <summary>
        /// Asynchronously loads the save data from the file.
        /// </summary>
        /// <remarks>
        /// Loading is done in a thread-safe manner using a semaphore to prevent multiple operations to interfere with each other.
        /// The method reads the file in chunks, where each chunk contains a set of key-value pairs before moving on to the next chunk.
        /// After all chunks have been read, the method returns the fully constructed save data object.
        /// If the file does not exist, a new empty save data object is returned.
        /// </remarks>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded save data.</returns>
        public async Task<ISaveData> Load() => await LoadAt(_filePath);

        /// <summary>
        /// Asynchronously loads the save data from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to load the save data from.</param>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded save data.</returns>
        public virtual async Task<ISaveData> LoadAt(string filePath) 
        {
            // Acquire the lock.
            await _lock.WaitAsync();

            // If the file doesn't exist, try to roll back to a backup file.
            if (!File.Exists(filePath)) 
            {
                // Attempt to roll back to the backup file, if it fails or backups are not allowed, return a new empty save data object.
                if (!await AttemptRollback())
                {
                    // Release the lock.
                    _lock.Release();

                    // Determine the appropriate error message based on whether backups are allowed.
                    string errorMessage = _backupAllowed ? "rollback to backup failed" : "rollback to backup failed because backup likely doesn't exist";

                    // Log an error if rollback failed or backups are not allowed.
                    Debug.LogError($"Save file not found at {filePath} and {errorMessage}, Creating new empty save data.");

                    // Return a new empty save data object.
                    return new SaveData();
                }
            }

            // Create a file stream to read from the file.
            await using var loadStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create a binary reader to read from the file.
            using var reader = new BinaryReader(loadStream, Encoding.UTF8, false);

            // Create a new save data object to hold the loaded data.
            var save = new SaveData();

            // Read each chunk of data.
            while (reader.ReadBoolean()) 
            {
                // Read the chunk ID.
                var chunkId = reader.ReadString();

                // Get the chunk data using the chunk ID.
                var chunk = save.GetChunk(chunkId);

                // Read the number of key-value pairs in the chunk.
                var count = reader.ReadInt32();

                // Read each key-value pair in the chunk and add it to the chunk.
                for (var i = 0; i < count; i++) chunk.Add(reader.ReadString(), reader.ReadString());
            }

            // Release the lock.
            _lock.Release();

            // Return the loaded save data.
            return save;
        }

        /// <summary>
        /// Asynchronously loads all existing save data files for the current profile's scope.
        /// </summary>
        /// <remarks>This method loads all existing save data files for the current profile's scope.</remarks>
        /// <returns>An array of loaded save data objects.</returns>
        public async Task<ISaveData[]> LoadAll() 
        {
            // Return early if the scope is not Global or Scene
            if (!(_profile.GetScope() == SaveScope.Global || _profile.GetScope() == SaveScope.Scene)) return Array.Empty<ISaveData>();

            // Get the existing save IDs.
            int[] existingIds = await ExistingSaveIDs();

            // If there are no existing IDs, return an empty array.
            if (existingIds.Length == 0) 
            {
                // Release the lock.
                _lock.Release();

                // Return an empty array if there are no existing IDs.
                return Array.Empty<ISaveData>();
            }

            // Create a list to hold the loaded save data objects.
            var saves = new System.Collections.Generic.List<ISaveData>();

            // Load each save data object.
            foreach (int id in existingIds) 
            {
                // Set the file path for the current ID.
                string folderPath = Path.Combine(_directory, $"{id}");

                // Update the file path to include the new profile.
                string filePath = Path.Combine(folderPath, fileName + _fileExtension);

                // Check if the file exists
                if (!File.Exists(filePath))
                {
                    // Log a warning if the file doesn't exist
                    Debug.LogWarning($"Save file not found at {filePath}, skipping.");

                    // Skip this ID if the file doesn't exist.
                    continue;
                }

                // Load the save data using the new file path.
                ISaveData saveData = await LoadAt(filePath);

                // Add the loaded save data to the list.
                saves.Add(saveData);
            }

            // Return the array of loaded save data objects.
            return saves.ToArray();
        }

        /// <summary>
        /// Attempts to roll back a file to its backup version if the backup file exists.
        /// </summary>
        /// <remarks>
        /// This method checks for the existence of a backup file at the specified location, appending a predefined backup file extension to the original file path. 
        /// If the backup file exists, it replaces the original file with the backup. If the backup file is missing, the method logs an error and returns <see langword="false"/>. Any exceptions encountered during the rollback process are propagated to the caller.
        /// </remarks>
        /// <returns> <see langword="true"/> if the rollback was successful and the backup file was restored; otherwise, <see langword="false"/> if the backup file does not exist.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during the rollback process, such as a failure to copy the backup file.</exception>
        public async Task<bool> AttemptRollback()
        {
            // Acquire the lock.
            await _lock.WaitAsync();

            // Initialize the success variable to false
            bool success = false;

            // Construct the backup file path.
            var backupFilePath = GetBackupFilePath();

            // Attempt to roll back to the backup file.
            try
            {
                // Check if the backup file exists.
                if (!File.Exists(backupFilePath)) 
                {
                    // Release the lock.
                    _lock.Release();

                    // Indicate that the rollback was not successful.
                    return success;
                }

                // Copy the backup file to the original file path, overwriting it.
                File.Copy(backupFilePath, _filePath, true);

                // Indicate that the rollback was successful.
                success = true;
            }
            catch (Exception e)
            {
                // Release the lock.
                _lock.Release();

                // Throw an exception if the rollback failed
                throw new Exception("Error occured when trying to roll back to backup file at: " + backupFilePath + ", did not work.\n" + e);
            }

            // Release the lock.
            _lock.Release();

            // Indicate that the rollback was successful.
            return success;
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
                Debug.Log($"{scopedDirectory.FullName} is empty, deleting it.");

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
                        Debug.Log($"{parentDirectory.FullName} is empty, deleting it.");

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
        /// Asynchronously gets the last modified time of the save file.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last modified time.</returns>
        public Task<TimeSpan> GetLastModifiedTime() => Task.FromResult(File.GetLastWriteTimeUtc(_filePath).TimeOfDay);

        /// <summary>
        /// Asynchronously checks if the save file exists.
        /// </summary>
        /// <returns>A task that represents the asynchronous existence check operation. The task result contains true if the file exists, false otherwise.</returns>
        public Task<bool> Exists() => Task.FromResult(File.Exists(_filePath));

        /// <summary>
        /// Gets the existing save IDs by scanning the save directory for folders named with integer IDs.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of existing save IDs.</returns>
        private Task<int[]> ExistingSaveIDs()
        {
            // Construct the path to the existing saves directory.
            string existingSavesPath = Path.Combine(Application.persistentDataPath, DefaultFolderName);

            // Ensure the directory exists.
            if (!Directory.Exists(existingSavesPath)) return Task.FromResult(Array.Empty<int>());

            // Create a list to hold the existing save IDs.
            var ids = new System.Collections.Generic.List<int>();

            // Get all folders in the saves directory
            DirectoryInfo savesDirectory = new DirectoryInfo(existingSavesPath);

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
        private string GetBackupFilePath() => _filePath + BackupFileExtension;
    }
}