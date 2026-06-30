namespace Sanctuary 
{
    [System.Serializable]
    public sealed class SaveEnvelope
    {
        public int SchemaVersion { get; set; }
        public string Timestamp { get; set; }
        public double TotalPlayTimeSeconds { get; set; }
        public string DataJson { get; set; }
        public string Checksum { get; set; }
    }
}