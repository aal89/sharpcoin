using System.Collections.Generic;

namespace Core.Tcp
{
    public abstract class Operations
    {
        public readonly Dictionary<string, byte> Codes;

        public abstract byte[] OK();
        public abstract byte[] NOOP();

        public abstract bool IsOK(byte[] data);
        public abstract bool IsNOOP(byte[] data);
    }
}
