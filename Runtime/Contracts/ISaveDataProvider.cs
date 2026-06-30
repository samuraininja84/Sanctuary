using System.Threading.Tasks;

namespace Sanctuary
{
    public interface ISaveDataProvider
    {
        string RootPath { get; }

        Task<bool> WriteAsync(string relativePath, byte[] data);

        Task<byte[]> ReadAsync(string relativePath);

        Task<bool> DeleteAsync(string relativePath);

        Task<bool> ExistsAsync(string relativePath);
    }
}