using System;

namespace Core.Utilities
{
    public class Logger : ILoggable
    {
        private readonly string prefix;

        public Logger(string prefix = "")
        {
            this.prefix = prefix;
        }

        public void Append(string line)
        {
            Console.WriteLine($"{line}");
        }

        public void Line(string line, string prefix = null)
        {
            Console.Write($"{Date.Now().FormattedString()} [{prefix ?? this.prefix}] {line}");
        }

        public void NewLine(string line, string prefix = null)
        {
            Console.WriteLine($"{Date.Now().FormattedString()} [{prefix ?? this.prefix}] {line}");
        }
    }
}
