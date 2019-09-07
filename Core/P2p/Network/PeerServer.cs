using Core.Tcp;
using System.Net.Sockets;

namespace Core.P2p.Tcpn
{
    public class PeerServer : TcpServer
    {
        public PeerServer(int port) : base(port) { }

        public override void AwaitConnections()
        {
            while (true)
            {
                // wait for client connection
                TcpClient client = server.AcceptTcpClient();
                if (!PeerManager.AddPeer(client))
                    client.Close();
            }
        }
    }
}
