namespace Core.Utilities
{
    public interface IFileOperations
    {
        string FilePath();
        void Save();
        void Read();
    }
}
