namespace Sanctuary 
{
    public readonly struct IntegrityResult
    {
        private readonly bool m_IsValid;
        private readonly IntegrityFailureReason m_Reason;

        public bool IsValid => m_IsValid;

        public IntegrityFailureReason Reason => m_Reason;

        public static IntegrityResult Valid() => new(true);

        public static IntegrityResult Fail(IntegrityFailureReason reason) => new(false, reason);

        public IntegrityResult(bool isValid, IntegrityFailureReason reason = IntegrityFailureReason.None)
        {
            m_IsValid = isValid;
            m_Reason = reason;
        }
    }

    public enum IntegrityFailureReason
    {
        None,
        ChecksumMismatch,
        MissingChecksum,
        IncompleteFile,
        EmptyFile,
        UnreadableFormat
    }
}