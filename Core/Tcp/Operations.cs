using System.Collections.Generic;

namespace Core.Tcp
{
    public abstract class Operations
    {
        public abstract Dictionary<string, byte> Codes();

        public byte[] OK()
        {
            return new byte[] { Codes()["Ok"] };
        }

        public byte[] NOOP()
        {
            return new byte[] { Codes()["Noop"] };
        }

        public bool IsOK(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes()["Ok"];
        }

        public bool IsNOOP(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes()["Noop"];
        }
    }
}
