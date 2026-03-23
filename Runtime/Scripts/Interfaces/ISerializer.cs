namespace Sanctuary 
{
    public interface ISerializer 
    {
        string Serialize<T>(T obj);

        T Deserialize<T>(string file);

        string FileExtension();
    }
}