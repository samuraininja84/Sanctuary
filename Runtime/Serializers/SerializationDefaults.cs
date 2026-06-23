using System;
using System.IO;
using System.Threading.Tasks;
using DirectoryUtility = Sanctuary.Utility.DirectoryUtility;

namespace Sanctuary.Serializers
{
    public static class SerializationDefaults
    {
        /// <summary>
        /// Represents the file extension used for backup files.
        /// </summary>
        public const string BackupFileExtension = ".bak";

        /// <summary>
        /// Attempts to roll back a file to its backup version if the backup file exists.
        /// </summary>
        /// <remarks>
        /// This method checks for the existence of a backup file at the specified location, appending a predefined backup file extension to the original file path. 
        /// If the backup file exists, it replaces the original file with the backup. If the backup file is missing, the method logs an error and returns <see langword="false"/>. Any exceptions encountered during the rollback process are propagated to the caller.
        /// </remarks>
        /// <param name="filePath">The path of the original file to roll back to.</param>
        /// <returns><see langword="true"/> if the rollback was successful and the backup file was restored; otherwise, <see langword="false"/> if the backup file does not exist.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during the rollback process, such as a failure to copy the backup file.</exception>
        public static async Task<bool> AttemptRollback(string filePath)
        {
            // Initialize the success variable to false
            bool success = false;

            // Construct the backup file path.
            var backupFilePath = filePath + BackupFileExtension;

            // Attempt to roll back to the backup file.
            try
            {
                // If the backup file exists, copy it to the original file path, overwriting the original file.
                if (File.Exists(backupFilePath))
                {
                    // Use the DirectoryUtility to copy the backup file to the original file path.
                    await DirectoryUtility.CopyFileAsync(backupFilePath, filePath);

                    // Indicate that the rollback was successful.
                    success = true;
                }
            }
            catch (Exception e)
            {
                // Throw an exception if the rollback failed to copy the backup file to the original file path.
                throw new Exception("Error occured when trying to roll back to backup file at: " + backupFilePath + " to " + filePath + ", did not work.\n" + e);
            }

            // Indicate that the rollback was successful.
            return success;
        }
    }
}
