namespace Sanctuary
{
    [System.Serializable]
    public sealed class SaveSlotInfo
    {
        public string SlotId { get; set; }
        public string CurrentFile { get; set; }
        public string BackupFile { get; set; }
        public System.DateTime LastSaveTime { get; set; }
        public double TotalPlayTimeSeconds { get; set; }
        public int SchemaVersion { get; set; }
        public bool IsAutoSave { get; set; }
    }
}