using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core.Utilities;

namespace Core.P2p.Tcpn
{
    public class TcpServer
    {
        private readonly TcpListener server;

        public TcpServer(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            new Thread(new ThreadStart(AwaitConnections)).Start();
        }

        public void AwaitConnections()
        {
            while (true)
            {
                // wait for client connection
                TcpClient client = server.AcceptTcpClient();
                PeerManager.AddPeer(client.Ip());
            }
        }
    }
}
