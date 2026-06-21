using UnityEngine;

namespace Sanctuary
{
    public class BinarySerializer : ISerializer 
    {
        public string Serialize<T>(T obj) 
        {
            // Serialize the object to a JSON string then convert it to binary
            string input = JsonUtility.ToJson(obj, true);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(bytes);
        }

        public T Deserialize<T>(string json) 
        {
            // Convert the binary to a JSON string then deserialize it
            byte[] bytes = System.Convert.FromBase64String(json);
            string output = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(output);
        }

        public string FileExtension() => ".bin";
    }
}
