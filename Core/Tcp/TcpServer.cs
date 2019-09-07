using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core.Utilities;

namespace Core.Tcp
{
    public abstract class TcpServer
    {
        protected readonly TcpListener server;

        protected TcpServer(int port)
        {
            server = new TcpListener(IPAddress.Parse(IpAddr.Mine()), port);
            server.Start();

            new Thread(new ThreadStart(AwaitConnections)).Start();
        }

        public abstract void AwaitConnections();
    }
}
