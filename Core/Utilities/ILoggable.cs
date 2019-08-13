namespace Core.Utilities
{
    public interface ILoggable
    {
        void Append(string line);
        void Line(string line, string prefix = null);
        void NewLine(string line, string prefix = null);
    }
}
