namespace MicroBatching.Interfaces
{
    /// <summary>
    /// Simple logging interface to implement for logging in the micro batcher
    /// </summary>
    public interface ILogger
    {
        void Log(LogLevel level, string message);
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
}
