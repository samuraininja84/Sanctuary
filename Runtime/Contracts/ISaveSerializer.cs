namespace Sanctuary.Serialization
{
    /// <summary>
    /// Defines the interface for a serializer that can serialize and deserialize save data to and from byte arrays.
    /// </summary>
    public interface ISaveSerializer
    {
        byte[] Serialize<T>(T data) where T : class;

        SaveDeserializeResult<T> Deserialize<T>(byte[] data) where T : class;

        int CurrentSchemaVersion { get; }
    }
}