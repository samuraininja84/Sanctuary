using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Sanctuary
{
    /// <summary>
    /// Provides utility methods for encryption and decryption of text, as well as key generation and encryption state detection.
    /// </summary>
    /// <remarks>
    /// This class includes methods for generating encryption keys, encrypting and decrypting text using AES encryption, and determining whether a string is encrypted. 
    /// It is designed to simplify common encryption tasks while ensuring  secure handling of cryptographic operations. 
    /// The caller is responsible for securely managing encryption keys.
    /// </remarks>
    public static class EncryptionUtility
    {
        /// <summary>
        /// A marker string used to identify encrypted data processed by the encryption utility.
        /// </summary>
        /// <remarks>This marker is prefixed to encrypted data to distinguish it from unencrypted content.
        /// It is intended for internal use within the encryption utility.</remarks>
        private static string EncryptionMarker = "EncryptionUtility:";

        /// <summary>
        /// Generates a random encryption key using a cryptographic random number generator.
        /// </summary>
        /// <remarks>
        /// The generated key is 256 bits (32 bytes) in length and is returned as a Base64-encoded string. 
        /// This format is suitable for storage or transmission in text-based systems.
        /// </remarks>
        /// <returns>A Base64-encoded string representing a 256-bit random encryption key.</returns>
        public static string RandomEncryptionKey()
        {
            // Generate a random encryption key using a cryptographic random number generator
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Create a byte array to hold the random key with a length of 32 bytes (256 bits)
                byte[] keyBytes = new byte[32];

                // Fill the byte array with random bytes
                rng.GetBytes(keyBytes);

                // Convert the byte array to a Base64 string for easy storage and transmission
                return Convert.ToBase64String(keyBytes);
            }
        }

        /// <summary>
        /// Encrypts the specified plain text using the provided encryption key.
        /// </summary>
        /// <remarks>This method uses AES encryption with a key and initialization vector derived from the
        /// provided encryption key. The encryption process includes converting the plain text to Unicode bytes,
        /// encrypting the bytes, and encoding the result as a base64 string. The caller is responsible for ensuring the
        /// encryption key is securely managed.</remarks>
        /// <param name="plainText">The text to be encrypted. Cannot be null or empty.</param>
        /// <param name="encryptionKey">The encryption key used to derive the cryptographic key and initialization vector. Must be a non-empty
        /// string. The strength of the encryption depends on the quality of this key.</param>
        /// <returns>A base64-encoded string representing the encrypted text, prefixed with a marker indicating encryption.</returns>
        public static string Encrypt(string plainText, string encryptionKey)
        {
            // Convert the plain text to a byte array using Unicode encoding.
            byte[] clearBytes = Encoding.Unicode.GetBytes(plainText);

            // Create an Aes encryptor to perform the encryption.
            using (Aes encryptor = Aes.Create())
            {
                // Derive the cryptographic key and initialization vector from the provided encryption key.
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                // Create a MemoryStream to hold the encrypted bytes.
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create a CryptoStream to perform the encryption.
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Write the clear bytes to the CryptoStream for encryption.
                        cs.Write(clearBytes, 0, clearBytes.Length);

                        // Close the CryptoStream to ensure all data is flushed and encrypted properly.
                        cs.Close();
                    }

                    // Convert the encrypted byte array to a Base64 string
                    plainText = Convert.ToBase64String(ms.ToArray());
                }
            }

            // Prefix the encrypted text with the EncryptionUtility marker
            return EncryptionMarker + plainText;
        }

        /// <summary>
        /// Decrypts the specified cipher text using the provided encryption key.
        /// </summary>
        /// <remarks>The method expects the cipher text to be Base64-encoded and may include a specific
        /// prefix that is removed during decryption. Ensure that the encryption key provided matches the key used
        /// during encryption; otherwise, decryption will fail.</remarks>
        /// <param name="cipherText">The encrypted text to be decrypted. Must be a Base64-encoded string.</param>
        /// <param name="encryptionKey">The key used to decrypt the cipher text. Must match the key used during encryption.</param>
        /// <returns>The decrypted plain text as a string.</returns>
        public static string Decrypt(string cipherText, string encryptionKey)
        {
            // Remove the EncryptionUtility prefix from the cipherText
            cipherText = cipherText.Replace(EncryptionMarker, "");

            // Replace spaces with plus signs
            cipherText = cipherText.Replace(" ", "+");

            // Convert the Base64-encoded cipher text back to a byte array
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Create an Aes encryptor to perform the decryption.
            using (Aes encryptor = Aes.Create())
            {
                // Derive the cryptographic key and initialization vector from the provided encryption key.
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                // Create a MemoryStream to hold the decrypted bytes.
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create a CryptoStream to perform the decryption.
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        // Write the cipher bytes to the CryptoStream for decryption.
                        cs.Write(cipherBytes, 0, cipherBytes.Length);

                        // Close the CryptoStream to ensure all data is flushed and decrypted properly.
                        cs.Close();
                    }

                    // Convert the decrypted byte array back to a string using Unicode encoding.
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            // Return the decrypted plain text.
            return cipherText;
        }

        /// <summary>
        /// Determines whether the specified string is encrypted.
        /// </summary>
        /// <param name="data">The string to check for encryption. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the string begins with the encryption marker; otherwise, <see langword="false"/>.</returns>
        public static bool Encrypted(string data) => data.StartsWith(EncryptionMarker);
    }
}
