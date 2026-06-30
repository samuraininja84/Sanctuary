namespace Sanctuary.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Computes the FNV-1a hash for the input string. 
        /// The FNV-1a hash is a non-cryptographic hash function known for its speed and good distribution properties.
        /// Useful for creating Dictionary keys instead of using strings.
        /// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="str">The input string to hash.</param>
        /// <returns>An integer representing the FNV-1a hash of the input string.</returns>
        public static int ComputeFNV1aHash(this string str)
        {
            // Initialize the FNV-1a hash value with the FNV offset basis
            uint hash = 2166136261;

            // Iterate over each character in the string, updating the hash value
            foreach (char c in str) hash = (hash ^ c) * 16777619;

            // Return the hash as an unchecked integer to avoid overflow exceptions
            return unchecked((int)hash);
        }

        /// <summary>
        /// Converts a byte array to its hexadecimal string representation.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A string representing the hexadecimal values of the byte array.</returns>
        public static string ToHexString(this byte[] bytes)
        {
            // Create a StringBuilder with an initial capacity of twice the length of the byte array
            var sb = new System.Text.StringBuilder(bytes.Length * 2);

            // Convert each byte to its hexadecimal representation and append it to the StringBuilder
            foreach (var b in bytes) sb.Append(b.ToString("x2"));

            // Return the hexadecimal string representation of the byte array
            return sb.ToString();
        }
    }
}