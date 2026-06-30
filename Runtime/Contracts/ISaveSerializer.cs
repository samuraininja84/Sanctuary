namespace Sanctuary
{
    public interface ISaveSerializer
    {
        byte[] Serialize<T>(T data) where T : class;

        SaveDeserializeResult<T> Deserialize<T>(byte[] data) where T : class;

        int CurrentSchemaVersion { get; }
    }
}