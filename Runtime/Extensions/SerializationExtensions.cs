using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Encoding = System.Text.Encoding;
using DirectoryUtility = Sanctuary.Utility.DirectoryUtility;
using EncryptionUtility = Sanctuary.Utility.EncryptionUtility;

namespace Sanctuary.Serialization
{
    public static class SerializationExtensions
    {
        /// <summary>
        /// Represents the default folder name used for saving files.
        /// </summary>
        public const string DefaultFolderName = "Save Data";

        /// <summary>
        /// Represents the default file extension used for serialized data files.
        /// </summary>
        public const string DefaultFileExtension = ".data";

        /// <summary>
        /// Represents the file extension used for backup files.
        /// </summary>
        public const string DefaultBackupExtension = ".bak";

        /// <summary>
        /// Represents the default buffer size used for file operations, set to 4096 bytes.
        /// </summary>
        private const int DefaultBufferSize = 4096;

        /// <summary>
        /// Represents the default file options used for file operations, combining asynchronous and sequential scan options.
        /// </summary>
        private const FileOptions DefaultFileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

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
            var backupFilePath = filePath + DefaultBackupExtension;

            // Attempt to roll back to the backup file.
            try
            {
                // If the backup file exists, copy it to the original file path, overwriting the original file.
                if (File.Exists(backupFilePath))
                {
                    // Use the DirectoryUtility to copy the backup file to the original file path.
                    await DirectoryUtility.CopyFileAsync(backupFilePath, filePath);

                    // Log a message indicating that the rollback was successful.
                    UnityEngine.Debug.Log($"[Sanctuary]: Rollback successful: {backupFilePath} has been restored to {filePath}.");

                    // Indicate that the rollback was successful.
                    success = true;
                }
            }
            catch (Exception e)
            {
                // Throw an exception if the rollback failed to copy the backup file to the original file path.
                throw new Exception("[Sanctuary]: Error occured when trying to roll back to backup file at: " + backupFilePath + " to " + filePath + ", did not work.\n" + e);
            }

            // Indicate that the rollback was successful.
            return success;
        }

        /// <summary>
        /// Creates a FileStream for corruption testing, ensuring that the directory exists before creating the file. 
        /// </summary>
        /// <remarks>
        /// Intended for testing purposes, this method creates a FileStream that can be used to simulate file corruption scenarios.
        /// The FileStream is created with the DeleteOnClose option, which means that the file will be automatically deleted when the stream is closed. 
        /// This is useful for testing scenarios where you want to simulate file corruption without leaving behind any test files.
        /// </remarks>
        /// <param name="filePath">The path of the file to create the FileStream for.</param>
        /// <returns>A FileStream for the specified file path.</returns>
        public static FileStream CreateCorruptionStream(string filePath)
        {
            // Ensure the folder path exists.
            var folderPath = Path.GetDirectoryName(filePath);

            // Create the directory if it does not exist.
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            // Create a FileStream with the specified parameters.
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose | DefaultFileOptions);
        }

        /// <summary>
        /// Creates a FileStream for serialization, ensuring that the directory exists before creating the file.
        /// </summary>
        /// <param name="filePath">The path of the file to create the FileStream for.</param>
        /// <returns>A FileStream for the specified file path.</returns>
        public static FileStream CreateFileSerializationStream(string filePath)
        {
            // Ensure the folder path exists.
            var folderPath = Path.GetDirectoryName(filePath);

            // Create the directory if it does not exist.
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            // Create a FileStream with the specified parameters.
            return new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, DefaultBufferSize, DefaultFileOptions);
        }

        /// <summary>
        /// Creates a FileStream for deserialization, checking if the file exists and attempting to roll back to a backup if it does not.
        /// </summary>
        /// <param name="filePath">The path of the file to create the FileStream for.</param>
        /// <param name="fileStream">The output FileStream for the specified file path.</param>
        /// <returns>A boolean indicating whether the FileStream was successfully created.</returns>
        public static async Task<FileStream> CreateFileDeserializationStream(string filePath)
        {
            // Check if the file exists before attempting to deserialize it.
            if (!File.Exists(filePath))
            {
                // Attempt to roll back to the backup file, if it fails or backups are not allowed, return a new empty save data object.
                if (!await AttemptRollback(filePath))
                {
                    // Log an error if rollback failed or backups are not allowed.
                    UnityEngine.Debug.LogError("[Sanctuary]: Save file not found at " + filePath + " and rollback to backup failed, the backup file may not exist or is corrupted. Returning null to indicate that the FileStream could not be created.");

                    // Return null to indicate that the FileStream could not be created.
                    return null;
                }
            }

            // Indicate that the FileStream was successfully created.
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultFileOptions);
        }

        /// <summary>
        /// Creates a FileStream for backup purposes, ensuring that the directory exists before creating the file.
        /// </summary>
        /// <param name="filePath">The path of the file to create the FileStream for.</param>
        /// <param name="backupExtension">The extension to use for the backup file.</param>
        /// <returns>A FileStream for the specified file path.</returns>
        public static async Task<FileStream> CreateFileBackupStream(string filePath, string backupExtension)
        {
            // Ensure the folder path exists.
            var folderPath = Path.GetDirectoryName(filePath);

            // Create the directory if it does not exist.
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            // Create a FileStream with the specified parameters.
            return new FileStream(filePath + backupExtension, FileMode.Create, FileAccess.Write, FileShare.Read, DefaultBufferSize, DefaultFileOptions);
        }

        /// <summary>
        /// Creates a <see cref="MemoryStream"/> for serialization with the specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the memory stream.</param>
        /// <returns>A <see cref="MemoryStream"/> with the specified initial capacity.</returns>
        public static MemoryStream CreateMemorySerializationStream(int initialCapacity) => new(initialCapacity);

        /// <summary>
        /// Creates a <see cref="MemoryStream"/> for deserialization with the specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the memory stream.</param>
        /// <returns>A <see cref="MemoryStream"/> with the specified initial capacity.</returns>
        public static MemoryStream CreateMemoryDeserializationStream(int initialCapacity) => new(initialCapacity);

        /// <summary>
        /// Creates a <see cref="MemoryStream"/> for backup purposes with the specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the memory stream.</param>
        /// <returns>A <see cref="MemoryStream"/> with the specified initial capacity.</returns>
        public static MemoryStream CreateMemoryBackupStream(int initialCapacity) => new(initialCapacity);

        /// <summary>
        /// Creates a <see cref="BinaryWriter"/> based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to apply when creating the <see cref="BinaryWriter"/>.</param>
        /// <param name="saveStream">The <see cref="Stream"/> to write to.</param>
        /// <param name="leaveOpen">Whether to leave the stream open after the <see cref="BinaryWriter"/> is disposed.</param>
        /// <returns>A <see cref="BinaryWriter"/> configured according to the specified serialization options.</returns>
        public static BinaryWriter CreateBinaryWriter(SerializationOptions options, Stream saveStream, bool leaveOpen = false)
        {
            // Create a BinaryWriter based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a BinaryWriter directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, leaveOpen),

                // Handle the "Compressed" option by creating a GZipStream for compression and wrapping it in a BinaryWriter.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Compress), Encoding.UTF8, leaveOpen),

                // Handle the "Encrypted" option by creating a CryptoStream for encryption and wrapping it in a BinaryWriter.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Write), Encoding.UTF8, leaveOpen),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, leaveOpen),

                // Handle combined options by applying both compression and encryption, functionally equivalent to the "All" option. Compression is applied first, followed by encryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, leaveOpen),

                // Handle the "All" option by applying both compression and encryption, functionally equivalent to the combined options above. Compression is applied first, followed by encryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, leaveOpen),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, leaveOpen)
            };
        }

        /// <summary>
        /// Creates a <see cref="StreamWriter"/> based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to use.</param>
        /// <param name="saveStream">The <see cref="Stream"/> to write to.</param>
        /// <param name="leaveOpen">Whether to leave the stream open after the <see cref="StreamWriter"/> is disposed.</param>
        /// <returns>A <see cref="StreamWriter"/> configured based on the specified serialization options.</returns>
        public static StreamWriter CreateStreamWriter(SerializationOptions options, Stream saveStream, bool leaveOpen = false)
        {
            // Create a StreamWriter based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a StreamWriter directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, 1024, leaveOpen),

                // Handle the "Compressed" option by creating a GZipStream for compression and wrapping it in a StreamWriter.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Compress), Encoding.UTF8, 1024, leaveOpen),

                // Handle the "Encrypted" option by creating a CryptoStream for encryption and wrapping it in a StreamWriter.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Write), Encoding.UTF8, 1024, leaveOpen),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, 1024, leaveOpen),

                // Handle combined options by applying both compression and encryption, functionally equivalent to the "All" option. Compression is applied first, followed by encryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, 1024, leaveOpen),

                // Handle the "All" option by applying both compression and encryption, functionally equivalent to the combined options above. Compression is applied first, followed by encryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, 1024, leaveOpen),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, 1024, leaveOpen)
            };
        }

        // To Do: Add check to see if the file is encrypted and if so, decrypt it before attempting to deserialize it, regardless of whether the options include the Encrypted flag or not.
        // This would allow files to be deserialized even in the case that the options do not include the Encrypted flag,
        // so that it doesn't break backwards compatibility with different versions of the game that may have used different serialization options.

        /// <summary>
        /// Creates a BinaryReader based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to apply.</param>
        /// <param name="saveStream">The <see cref="Stream"/> to read from.</param>
        /// <param name="leaveOpen">Whether to leave the stream open after the <see cref="BinaryReader"/> is disposed.</param>
        /// <returns>A <see cref="BinaryReader"/> configured based on the specified serialization options.</returns>
        public static BinaryReader CreateBinaryReader(SerializationOptions options, Stream saveStream, bool leaveOpen = false)
        {
            // Create a BinaryReader based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a BinaryReader directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, leaveOpen),

                // Handle the "Compressed" option by creating a GZipStream for decompression and wrapping it in a BinaryReader.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Decompress), Encoding.UTF8, leaveOpen),

                // Handle the "Encrypted" option by creating a CryptoStream for decryption and wrapping it in a BinaryReader.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Read), Encoding.UTF8, leaveOpen),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, leaveOpen),

                // Handle combined options by applying both decompression and decryption, functionally equivalent to the "All" option. Decompression is applied first, followed by decryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, leaveOpen),

                // Handle the "All" option by applying both decompression and decryption, functionally equivalent to the combined options above. Decompression is applied first, followed by decryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, leaveOpen),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, leaveOpen)
            };
        }

        /// <summary>
        /// Creates a <see cref="StreamReader"/> based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to use.</param>
        /// <param name="saveStream">The <see cref="Stream"/> to read from.</param>
        /// <param name="leaveOpen">Whether to leave the stream open after the <see cref="StreamReader"/> is disposed.</param>
        /// <returns>A <see cref="StreamReader"/> configured based on the specified serialization options.</returns>
        public static StreamReader CreateStreamReader(SerializationOptions options, Stream saveStream, bool leaveOpen = false)
        {
            // Create a StreamReader based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a StreamReader directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, leaveOpen),

                // Handle the "Compressed" option by creating a GZipStream for decompression and wrapping it in a StreamReader.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Decompress), Encoding.UTF8, leaveOpen),

                // Handle the "Encrypted" option by creating a CryptoStream for decryption and wrapping it in a StreamReader.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Read), Encoding.UTF8, leaveOpen),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, leaveOpen),

                // Handle combined options by applying both decompression and decryption, functionally equivalent to the "All" option. Decompression is applied first, followed by decryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, leaveOpen),

                // Handle the "All" option by applying both decompression and decryption, functionally equivalent to the combined options above. Decompression is applied first, followed by decryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, leaveOpen),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, leaveOpen)
            };
        }
    }
}
