namespace Sanctuary 
{
    public readonly struct SaveLoadResult<T> where T : class
    {
        private readonly LoadStatus m_Status;
        private readonly T m_Data;
        private readonly SaveSlotInfo m_SlotInfo;
        private readonly string m_Message;

        public bool Success => Status is LoadStatus.Success
            or LoadStatus.SuccessFromBackup
            or LoadStatus.SuccessMigrated
            or LoadStatus.SuccessMigratedFromBackup;

        public LoadStatus Status => m_Status;

        public T Data => m_Data;

        public SaveSlotInfo SlotInfo => m_SlotInfo;

        public string Message => m_Message;

        public static SaveLoadResult<T> Succeed(T data, SaveSlotInfo info) => new(LoadStatus.Success, data, info);

        public static SaveLoadResult<T> FromBackup(T data, SaveSlotInfo info) => new(LoadStatus.SuccessFromBackup, data, info, "Primary save was corrupt — loaded from backup");

        public static SaveLoadResult<T> Migrated(T data, SaveSlotInfo info) => new(LoadStatus.SuccessMigrated, data, info, "Save migrated from older version");

        public static SaveLoadResult<T> MigratedFromBackup(T data, SaveSlotInfo info) => new(LoadStatus.SuccessMigratedFromBackup, data, info, "Backup loaded and migrated from older version");

        public static SaveLoadResult<T> Fail(LoadStatus status, string message) => new(status, null, null, message);

        public SaveLoadResult(LoadStatus status, T data = null, SaveSlotInfo slotInfo = null, string message = null)
        {
            m_Status = status;
            m_Data = data;
            m_SlotInfo = slotInfo;
            m_Message = message;
        }
    }

    public enum LoadStatus
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