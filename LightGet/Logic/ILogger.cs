namespace LightGet.Logic {
    public interface ILogger {
        void LogDebug(string format, params object[] args);
        void LogMessage(string format, params object[] args);
        void LogWarning(string format, params object[] args);
        void LogError(string format, params object[] args);
    }
}