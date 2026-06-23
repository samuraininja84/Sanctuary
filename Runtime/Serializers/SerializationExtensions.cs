using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Encoding = System.Text.Encoding;
using DirectoryUtility = Sanctuary.Utility.DirectoryUtility;
using EncryptionUtility = Sanctuary.Utility.EncryptionUtility;

namespace Sanctuary.Serializers
{
    public static class SerializationExtensions
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
        /// Creates a BinaryWriter based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to apply when creating the BinaryWriter.</param>
        /// <param name="saveStream">The FileStream to write to.</param>
        /// <returns>A BinaryWriter configured according to the specified serialization options.</returns>
        public static BinaryWriter CreateBinaryWriter(SerializationOptions options, FileStream saveStream)
        {
            // Create a BinaryWriter based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a BinaryWriter directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, false),

                // Handle the "Compressed" option by creating a GZipStream for compression and wrapping it in a BinaryWriter.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Compress), Encoding.UTF8, false),

                // Handle the "Encrypted" option by creating a CryptoStream for encryption and wrapping it in a BinaryWriter.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Write), Encoding.UTF8, false),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, false),

                // Handle combined options by applying both compression and encryption, functionally equivalent to the "All" option. Compression is applied first, followed by encryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, false),

                // Handle the "All" option by applying both compression and encryption, functionally equivalent to the combined options above. Compression is applied first, followed by encryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, false),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, false)
            };
        }

        /// <summary>
        /// Creates a BinaryReader based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to apply.</param>
        /// <param name="saveStream">The FileStream to read from.</param>
        /// <returns>A configured BinaryReader instance.</returns>
        public static BinaryReader CreateBinaryReader(SerializationOptions options, FileStream saveStream)
        {
            // Create a BinaryReader based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a BinaryReader directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, false),

                // Handle the "Compressed" option by creating a GZipStream for decompression and wrapping it in a BinaryReader.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Decompress), Encoding.UTF8, false),

                // Handle the "Encrypted" option by creating a CryptoStream for decryption and wrapping it in a BinaryReader.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Read), Encoding.UTF8, false),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, false),

                // Handle combined options by applying both decompression and decryption, functionally equivalent to the "All" option. Decompression is applied first, followed by decryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, false),

                // Handle the "All" option by applying both decompression and decryption, functionally equivalent to the combined options above. Decompression is applied first, followed by decryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, false),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, false)
            };
        }

        /// <summary>
        /// Creates a StreamWriter based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to use.</param>
        /// <param name="saveStream">The file stream to write to.</param>
        /// <returns>A StreamWriter configured based on the specified serialization options.</returns>
        public static StreamWriter CreateStreamWriter(SerializationOptions options, FileStream saveStream)
        {
            // Create a StreamWriter based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a StreamWriter directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, 1024, false),

                // Handle the "Compressed" option by creating a GZipStream for compression and wrapping it in a StreamWriter.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Compress), Encoding.UTF8, 1024, false),

                // Handle the "Encrypted" option by creating a CryptoStream for encryption and wrapping it in a StreamWriter.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Write), Encoding.UTF8, 1024, false),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, 1024, false),

                // Handle combined options by applying both compression and encryption, functionally equivalent to the "All" option. Compression is applied first, followed by encryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, 1024, false),

                // Handle the "All" option by applying both compression and encryption, functionally equivalent to the combined options above. Compression is applied first, followed by encryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Compress), CryptoStreamMode.Write), Encoding.UTF8, 1024, false),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, 1024, false)
            };
        }

        /// <summary>
        /// Creates a StreamReader based on the specified serialization options and the provided FileStream.
        /// </summary>
        /// <param name="options">The serialization options to use.</param>
        /// <param name="saveStream">The file stream to read from.</param>
        /// <returns>A StreamReader configured based on the specified serialization options.</returns>
        public static StreamReader CreateStreamReader(SerializationOptions options, FileStream saveStream)
        {
            // Create a StreamReader based on the specified serialization options.
            return options switch
            {
                // Handle the "None" option by creating a StreamReader directly on the provided FileStream.
                SerializationOptions.None => new(saveStream, Encoding.UTF8, false),

                // Handle the "Compressed" option by creating a GZipStream for decompression and wrapping it in a StreamReader.
                SerializationOptions.Compressed => new(new GZipStream(saveStream, CompressionMode.Decompress), Encoding.UTF8, false),

                // Handle the "Encrypted" option by creating a CryptoStream for decryption and wrapping it in a StreamReader.
                SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(saveStream, CryptoStreamMode.Read), Encoding.UTF8, false),

                // Functionally equivalent to the "None" option, as it does not apply any compression or encryption.
                SerializationOptions.Backup => new(saveStream, Encoding.UTF8, false),

                // Handle combined options by applying both decompression and decryption, functionally equivalent to the "All" option. Decompression is applied first, followed by decryption.
                SerializationOptions.Compressed | SerializationOptions.Encrypted => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, false),

                // Handle the "All" option by applying both decompression and decryption, functionally equivalent to the combined options above. Decompression is applied first, followed by decryption.
                SerializationOptions.All => new(EncryptionUtility.CreateCryptoStream(new GZipStream(saveStream, CompressionMode.Decompress), CryptoStreamMode.Read), Encoding.UTF8, false),

                // Default case to handle any unexpected options, functionally equivalent to the "None" option.
                _ => new(saveStream, Encoding.UTF8, false)
            };
        }
    }
}
