using System.Collections.Generic;
using Core.P2p.Tcpn;

namespace Core.Utilities
{
    public class PeerComparer : IEqualityComparer<Peer>
    {
        public bool Equals(Peer c1, Peer c2)
        {
            if (c1 == null && c2 == null) { return true; }
            if (c1 == null | c2 == null) { return false; }
            if (c1.Ip == c2.Ip) { return true; }
            return false;
        }
        public int GetHashCode(Peer p)
        {
            return p.Ip.GetHashCode();
        }
    }
}
