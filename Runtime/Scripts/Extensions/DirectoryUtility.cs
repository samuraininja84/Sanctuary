using System;
using System.IO;
using UnityEngine;

namespace Sanctuary 
{
    /// <summary>
    /// Provides utility methods for managing and cleaning up directories, including deleting files with specific names and extensions, and determining the contents of a directory.
    /// </summary>
    /// <remarks>
    /// The <see cref="DirectoryUtility"/> class includes methods for cleaning up directories by removing specific files and optionally deleting empty directories. 
    /// It also provides methods to check for the presence of files or subdirectories within a directory. These methods are designed to simplify common directory management tasks.
    /// </remarks>
    public static class DirectoryUtility
    {
        /// <summary>
        /// Cleans up a specified directory by deleting files with specific names and extensions, and removes the directory if it becomes empty.
        /// </summary>
        /// <remarks>
        /// This method first deletes files in the specified directory that match the provided name and extensions. 
        /// If the directory becomes empty after file deletion, it is removed.</remarks>
        /// <param name="directory">The directory to clean up. Must not be <see langword="null"/>.</param>
        /// <param name="fileName">The name of the files to delete. Case-sensitive.</param>
        /// <param name="fileExtension">The primary file extension to match for deletion. Should include the leading dot (e.g., ".txt").</param>
        /// <param name="backupExtension">The backup file extension to match for deletion. Should include the leading dot (e.g., ".bak").</param>
        public static void CleanUpDirectory(this DirectoryInfo directory, string fileName, string fileExtension, string backupExtension)
        {
            // If the subdirectory has files, delete the files with the correct name and extensions
            DeleteProfileFiles(directory, fileName, fileExtension, backupExtension);

            // If the subdirectory has no files or subdirectories left, delete the directory
            if (!HasContents(directory))
            {
                // Log the deletion of the empty directory
                Debug.Log($"{directory.FullName} is empty, deleting it.");

                // Delete the empty directory
                Directory.Delete(directory.FullName, true);
            }
        }

        /// <summary>
        /// Deletes files from the specified directory that match the given name and have the specified file extension or backup extension.
        /// </summary>
        /// <remarks>
        /// This method iterates through all files in the specified directory.
        /// Then it deletes the files that match the provided file name and either the primary file extension or the backup extension. 
        /// The file name comparison is case-insensitive.
        /// </remarks>
        /// <param name="directory">The directory to search for files to delete. Must not be <see langword="null"/>.</param>
        /// <param name="fileName">The name of the files to delete, without the extension. The comparison is case-insensitive.</param>
        /// <param name="fileExtension">The primary file extension to match, including the leading period (e.g., ".txt").</param>
        /// <param name="backupExtension">An additional file extension to match, typically used for backup files, including the leading period (e.g., ".bak").</param>
        public static void DeleteProfileFiles(this DirectoryInfo directory, string fileName, string fileExtension, string backupExtension)
        {
            // Check if the directory has files, if not, return early
            if (!HasFiles(directory)) return;

            // Get all files in the directory
            foreach (FileInfo file in directory.GetFiles())
            {
                // Check if the file has the correct name and extension
                string name = Path.GetFileNameWithoutExtension(file.Name);
                bool hasCorrectName = name.Equals(fileName, StringComparison.OrdinalIgnoreCase) || name.Equals(fileName + fileExtension, StringComparison.OrdinalIgnoreCase);
                bool hasCorrectExtension = file.Extension == fileExtension || file.Extension == backupExtension;

                // If the file has the correct name and extension, delete it
                if (hasCorrectName && hasCorrectExtension)
                {
                    // Log the deletion of the file
                    Debug.Log("Deleting file: " + file.FullName);

                    // Delete the file
                    file.Delete();
                }
            }
        }

        /// <summary>
        /// Determines whether the specified directory contains any files.
        /// </summary>
        /// <remarks>This method checks only for files directly within the specified directory and does not include files in subdirectories.</remarks>
        /// <param name="directory">The directory to check for files. Must not be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the directory contains one or more files; otherwise, <see langword="false"/>.</returns>
        public static bool HasFiles(this DirectoryInfo directory) => directory.GetFiles().Length > 0;

        /// <summary>
        /// Determines whether the specified directory contains any subdirectories.
        /// </summary>
        /// <param name="directory">The directory to check for subdirectories. Must not be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the directory contains one or more subdirectories; otherwise, <see langword="false"/>.</returns>
        public static bool HasSubDirectories(this DirectoryInfo directory) => directory.GetDirectories().Length > 0;

        /// <summary>
        /// Determines whether the specified directory contains any files or subdirectories.
        /// </summary>
        /// <remarks>
        /// This method checks both files and subdirectories within the specified directory. 
        /// It does not include hidden or system files unless they are explicitly accessible.
        /// </remarks>
        /// <param name="directory">The <see cref="DirectoryInfo"/> representing the directory to check. Must not be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the directory contains at least one file or subdirectory; otherwise, <see langword="false"/>.</returns>
        public static bool HasContents(this DirectoryInfo directory) => HasFiles(directory) || HasSubDirectories(directory);
    }
}