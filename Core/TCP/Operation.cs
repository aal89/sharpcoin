using System.Collections.Generic;

namespace Core.TCP
{
    public static class Operation
    {
        public static readonly Dictionary<string, byte> Codes = new Dictionary<string, byte>
        {
            { "Ok", 0x98 },
            { "Noop", 0x99 },
            { "RequestBlock", 0x01 },
            { "RequestBlockResponse", 0x02 },
            { "AcceptBlock", 0x03 },
            { "AcceptBlockResponse", 0x04 },
            { "RequestPeers", 0x05 },
            { "RequestPeersResponse", 0x06 },
        };

        public static byte[] OK()
        {
            return new byte[] { Codes["Ok"] };
        }

        public static byte[] NOOP()
        {
            return new byte[] { Codes["Noop"] };
        }

        public static bool IsOK(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes["Ok"];
        }

        public static bool IsNOOP(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes["Noop"];
        }
    }
}
