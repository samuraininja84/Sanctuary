namespace Sanctuary 
{
    public readonly struct SaveLoadResult<T> where T : class
    {
        private readonly SaveLoadStatus m_Status;
        private readonly T m_Data;
        private readonly SaveSlotInfo m_SlotInfo;
        private readonly string m_Message;

        public bool Success => Status is SaveLoadStatus.Success
            or SaveLoadStatus.SuccessFromBackup
            or SaveLoadStatus.SuccessMigrated
            or SaveLoadStatus.SuccessMigratedFromBackup;

        public SaveLoadStatus Status => m_Status;

        public T Data => m_Data;

        public SaveSlotInfo SlotInfo => m_SlotInfo;

        public string Message => m_Message;

        public static SaveLoadResult<T> Succeed(T data, SaveSlotInfo info) => new(SaveLoadStatus.Success, data, info);

        public static SaveLoadResult<T> FromBackup(T data, SaveSlotInfo info) => new(SaveLoadStatus.SuccessFromBackup, data, info, "Primary save was corrupt — loaded from backup");

        public static SaveLoadResult<T> Migrated(T data, SaveSlotInfo info) => new(SaveLoadStatus.SuccessMigrated, data, info, "Save migrated from older version");

        public static SaveLoadResult<T> MigratedFromBackup(T data, SaveSlotInfo info) => new(SaveLoadStatus.SuccessMigratedFromBackup, data, info, "Backup loaded and migrated from older version");

        public static SaveLoadResult<T> Fail(SaveLoadStatus status, string message) => new(status, null, null, message);

        public SaveLoadResult(SaveLoadStatus status, T data = null, SaveSlotInfo slotInfo = null, string message = null)
        {
            m_Status = status;
            m_Data = data;
            m_SlotInfo = slotInfo;
            m_Message = message;
        }
    }

    public enum SaveLoadStatus
    {
        Success,
        SuccessFromBackup,
        SuccessMigrated,
        SuccessMigratedFromBackup,
        NoValidSave,
        MigrationFailed,
        ProviderError
    }
}