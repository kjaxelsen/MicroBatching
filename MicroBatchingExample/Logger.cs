using MicroBatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
