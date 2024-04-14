using MicroBatching.Interfaces;

namespace MicroBatchingExample
{
    public class Logger : ILogger
    {
        public void Log(LogLevel level, string message)
        {
            Console.WriteLine($"{level.ToString()} - {message}");
        }
    }
}
