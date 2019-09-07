using System.Collections.Generic;
using Core.Tcp;

namespace Core.P2p.Tcpn
{
    public class PeerOperations : Operations
    {
        public new readonly Dictionary<string, byte> Codes = new Dictionary<string, byte>
        {
            { "Ok", 0x98 },
            { "Noop", 0x99 },
            { "RequestBlock", 0x01 },
            { "RequestBlockResponse", 0x02 },
            { "AcceptBlock", 0x03 },
            { "AcceptBlockResponse", 0x04 },
            { "RequestPeers", 0x05 },
            { "RequestPeersResponse", 0x06 },
            { "AcceptPeers", 0x07 },
            { "AcceptPeersResponse", 0x08 },
            { "RequestTransaction", 0x09 },
            { "RequestTransactionResponse", 0x0a },
            { "AcceptTransaction", 0x0b },
            { "AcceptTransactionResponse", 0x0c },
            { "RequestBlockchainSize", 0x0d },
            { "RequestBlockchainSizeResponse", 0x0e }
        };

        public override byte[] OK()
        {
            return new byte[] { Codes["Ok"] };
        }

        public override byte[] NOOP()
        {
            return new byte[] { Codes["Noop"] };
        }

        public override bool IsOK(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes["Ok"];
        }

        public override bool IsNOOP(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes["Noop"];
        }
    }
}
