using System;
using System.Text;
using Newtonsoft.Json;

namespace Sanctuary.Serialization
{
    public readonly struct JsonSaveSerializer : ISaveSerializer
    {
        private readonly int m_CurrentSchemaVersion;

        public readonly int CurrentSchemaVersion => m_CurrentSchemaVersion;

        public JsonSaveSerializer(IStreamConfiguration configuration) => m_CurrentSchemaVersion = configuration.CurrentSchemaVersion;

        public byte[] Serialize<T>(T data) where T : class
        {
            // Create a SaveEnvelope object with the current schema version, timestamp, and serialized data
            var envelope = new SaveEnvelope
            {
                SchemaVersion = m_CurrentSchemaVersion,
                Timestamp = DateTime.UtcNow.ToString("o"),
                DataJson = JsonConvert.SerializeObject(data)
            };

            // Convert the envelope JSON string to a byte array using UTF-8 encoding and return it
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(envelope));
        }

        public SaveDeserializeResult<T> Deserialize<T>(byte[] data) where T : class
        {
            // Check if the input data is null or empty, and return a failure result with an appropriate message if it is
            if (data == null || data.Length == 0) return SaveDeserializeResult<T>.Fail("Data is null or empty");

            // Try to deserialize the byte array into a SaveEnvelope and then into the desired type T, handling any exceptions that may occur
            try
            {
                // Convert the byte array to a UTF-8 string
                var envelopeJson = Encoding.UTF8.GetString(data);

                // Deserialize the envelope JSON into a SaveEnvelope object
                var envelope = JsonConvert.DeserializeObject<SaveEnvelope>(envelopeJson);

                // If the envelope is null, it means that the deserialization of the envelope failed, so we return a failure result with an appropriate message
                if (envelope == null) return SaveDeserializeResult<T>.Fail("Failed to deserialize save envelope");

                // Get the result of deserializing the DataJson property of the envelope into an object of type T
                var result = JsonConvert.DeserializeObject<T>(envelope.DataJson);

                // If the result is null, it means that the deserialization failed, so we return a failure result with an appropriate message
                if (result == null) return SaveDeserializeResult<T>.Fail("Failed to deserialize save data");

                // Return a successful result with the deserialized data and the schema version from the envelope
                return SaveDeserializeResult<T>.Succeed(result, envelope.SchemaVersion);
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur during deserialization and return a failure result with the exception message
                return SaveDeserializeResult<T>.Fail($"Deserialization error: {ex.Message}");
            }
        }
    }
}
