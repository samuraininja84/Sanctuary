namespace Sanctuary
{
    public readonly struct UnityDebugLogger : ISanctuaryLogger
    {
        public readonly void Info(string message) => UnityEngine.Debug.Log(message);

        public readonly void Warn(string message) => UnityEngine.Debug.LogWarning(message);

        public readonly void Error(string message) => UnityEngine.Debug.LogError(message);
    }
}
