namespace Sanctuary
{
    public readonly struct UnityDebugLogger : ISanctuaryLogger
    {
        public void Info(string message) => UnityEngine.Debug.Log(message);
        public void Warn(string message) => UnityEngine.Debug.LogWarning(message);
        public void Error(string message) => UnityEngine.Debug.LogError(message);
    }
}
