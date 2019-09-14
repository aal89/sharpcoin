using System.Collections.Generic;
using Core.Tcp;

namespace Core.P2p.Net
{
    public class PeerOperations : Operations
    {
        public override Dictionary<string, byte> Codes()
        {
            return new Dictionary<string, byte>
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
                { "RequestBlockchainSizeResponse", 0x0e },
                { "AcceptBlockchainSize", 0x0f },
                { "AcceptBlockchainSizeResponse", 0x10 }
            };
        }
    }
}
