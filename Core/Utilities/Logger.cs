using System;

namespace Core.Utilities
{
    public class Logger
    {
        private readonly string prefix;

        public Logger(string prefix = "")
        {
            this.prefix = prefix;
        }

        public void Log(string line, string prefix = null)
        {
            Console.WriteLine($"{DateTime.UtcNow} [{prefix ?? this.prefix}] {line}");
        }
    }
}
