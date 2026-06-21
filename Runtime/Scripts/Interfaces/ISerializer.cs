using System.Threading.Tasks;

namespace Sanctuary 
{
    public interface ISerializer 
    {
        Task Serialize(ISaveData data, string folderPath, string filePath);

        Task<ISaveData> Deserialize(string filePath);

        string FileExtension();
    }
}