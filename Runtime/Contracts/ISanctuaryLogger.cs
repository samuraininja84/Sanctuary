namespace Sanctuary
{
    public interface ISanctuaryLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }
}