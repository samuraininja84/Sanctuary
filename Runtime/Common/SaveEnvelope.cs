namespace Sanctuary 
{
    [System.Serializable]
    public sealed class SaveEnvelope
    {
        public int SchemaVersion { get; set; }
        public string Timestamp { get; set; }
        public double TotalPlayTimeSeconds { get; set; }
        public object Data { get; set; } = null;
        public string Checksum { get; set; }
    }
}