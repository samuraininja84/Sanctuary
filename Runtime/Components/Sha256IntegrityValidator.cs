using System;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Sanctuary.Extensions;

namespace Sanctuary
{
    /// <summary>
    /// Represents a validator that checks the integrity of save files using SHA-256 checksums.
    /// </summary>
    public readonly struct Sha256IntegrityValidator : ISaveIntegrityValidator 
    {
        /// <summary>
        /// Generates a SHA-256 checksum for the given byte array.
        /// </summary>
        /// <param name="data">The byte array for which to generate the checksum.</param>
        /// <returns>A string representing the SHA-256 checksum of the input data.</returns>
        public string GenerateChecksum(byte[] data) 
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return hash.ToHexString();
        }

        /// <summary>
        /// Validates the integrity of a raw file by checking its checksum and structure.
        /// </summary>
        /// <param name="rawFile">The raw file data to validate.</param>
        /// <returns>An <see cref="IntegrityResult"/> indicating the result of the validation.</returns>
        public IntegrityResult Validate(byte[] rawFile) 
        {
            // If the file is null or empty, return an integrity failure indicating that the file is empty.
            if (rawFile == null || rawFile.Length == 0) return IntegrityResult.Fail(IntegrityFailureReason.EmptyFile);

            // Initialize a SaveEnvelope variable to hold the deserialized data.
            SaveEnvelope envelope;

            // Attempt to deserialize the raw file into a SaveEnvelope object.
            try
            {
                // Get the JSON string from the raw byte array using UTF-8 encoding.
                var json = Encoding.UTF8.GetString(rawFile);

                // Deserialize the JSON string into a SaveEnvelope object.
                envelope = JsonConvert.DeserializeObject<SaveEnvelope>(json);
            }
            catch 
            {
                // If deserialization fails, return an integrity failure indicating that the format is unreadable.
                return IntegrityResult.Fail(IntegrityFailureReason.UnreadableFormat);
            }

            // If the envelope is null after deserialization, return an integrity failure indicating that the file is incomplete.
            if (envelope == null) return IntegrityResult.Fail(IntegrityFailureReason.IncompleteFile);

            // If the checksum is null or empty, return an integrity failure indicating that the checksum is missing.
            if (string.IsNullOrEmpty(envelope.Checksum)) return IntegrityResult.Fail(IntegrityFailureReason.MissingChecksum);
            
            // If the data JSON is null or empty, return an integrity failure indicating that the file is incomplete.
            if (string.IsNullOrEmpty(envelope.DataJson)) return IntegrityResult.Fail(IntegrityFailureReason.IncompleteFile);

            // Convert the DataJson string to a byte array using UTF-8 encoding.
            var dataBytes = Encoding.UTF8.GetBytes(envelope.DataJson);

            // Generate the checksum of the data bytes and compare it with the checksum in the envelope.
            var computed = GenerateChecksum(dataBytes);

            // If the computed checksum does not match the checksum in the envelope, return an integrity failure indicating a checksum mismatch.
            if (!string.Equals(computed, envelope.Checksum, StringComparison.OrdinalIgnoreCase)) 
            {
                // If the checksums do not match, return an integrity failure indicating a checksum mismatch.
                return IntegrityResult.Fail(IntegrityFailureReason.ChecksumMismatch);
            }

            // If all checks pass, return a valid integrity result.
            return IntegrityResult.Valid();
        }
    }
}
