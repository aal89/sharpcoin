using System.Collections.Generic;
using Core.Utilities;

namespace Core.Indexes
{
    public abstract class Index<T, U, V>: Dictionary<U, V>, IFileOperations
    {
        public abstract T Get(U Id);

        public abstract string FilePath();
        public abstract void Save();
        public abstract void Read();
    }

    public abstract class Index<T>: List<T>, IFileOperations
    {
        public abstract T Get(T Id);

        public abstract string FilePath();
        public abstract void Save();
        public abstract void Read();
    }
}
