namespace Core.Utilities
{
    public class NullLogger : ILoggable
    {
        public void Append(string line) { }

        public void Line(string line, string prefix = null) { }

        public void NewLine(string line, string prefix = null) { }
    }
}
