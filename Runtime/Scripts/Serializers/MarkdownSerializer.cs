using UnityEngine;

namespace Sanctuary.Serializers
{
    public class MarkdownSerializer : ISerializer 
    {
        public string Serialize<T>(T obj) => JsonUtility.ToJson(obj, true);

        public T Deserialize<T>(string json) => JsonUtility.FromJson<T>(json);

        public string FileExtension() => ".md";
    }
}
