using System.IO;
using System.Threading.Tasks;

namespace Sanctuary
{
    public sealed class FileSaveDataProvider : ISaveDataProvider
    {
        private readonly string m_RootPath;

        public string RootPath => m_RootPath;

        public FileSaveDataProvider(ISanctuaryConfiguration configuration) => m_RootPath = configuration.RootPath;

        public async Task<bool> WriteAsync(string relativePath, byte[] data)
        {
            // Get the full path of the file to write
            var fullPath = GetFullPath(relativePath);

            // Get the directory of the file to ensure it exists before writing
            var directory = Path.GetDirectoryName(fullPath);

            // Ensure the directory exists. If it doesn't, create it.
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Write the data to the file asynchronously
            await File.WriteAllBytesAsync(fullPath, data);

            // Return true to indicate that the write operation was successful
            return true;
        }

        public Task<byte[]> ReadAsync(string relativePath)
        {
            // Get the full path of the file to read
            var fullPath = GetFullPath(relativePath);

            // Check if the file exists. If it doesn't, return null to indicate that the file could not be found.
            if (!File.Exists(fullPath)) return null;

            // Read the file asynchronously and return the byte array
            return File.ReadAllBytesAsync(fullPath);
        }

        public Task<bool> DeleteAsync(string relativePath)
        {
            // Get the full path of the file to delete
            var fullPath = GetFullPath(relativePath);

            // Delete the file if it exists. If it doesn't exist, we still return true because the end result is that the file is not present.
            if (File.Exists(fullPath)) File.Delete(fullPath);

            // Return true even if the file didn't exist, as the end result is that the file is not present.
            return Task.FromResult(true);
        }

        public Task<bool> ExistsAsync(string relativePath) => Task.FromResult(File.Exists(GetFullPath(relativePath)));

        private string GetFullPath(string relativePath) => Path.Combine(m_RootPath, relativePath);
    }
}
