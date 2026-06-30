namespace Sanctuary 
{
    public readonly struct SaveDeserializeResult<T> where T : class
    {
        private readonly bool m_Success;
        private readonly T m_Data;
        private readonly int m_SchemaVersion;
        private readonly string m_ErrorMessage;

        public bool Success => m_Success;

        public T Data => m_Data;

        public int SchemaVersion => m_SchemaVersion;

        public string ErrorMessage => m_ErrorMessage;

        public static SaveDeserializeResult<T> Succeed(T data, int version) => new(true, data, version);

        public static SaveDeserializeResult<T> Fail(string error) => new(false, errorMessage: error);

        public SaveDeserializeResult(bool success, T data = null, int schemaVersion = 0, string errorMessage = null)
        {
            m_Success = success;
            m_Data = data;
            m_SchemaVersion = schemaVersion;
            m_ErrorMessage = errorMessage;
        }
    }
}