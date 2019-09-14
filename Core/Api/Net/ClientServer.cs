using System.Net.Sockets;
using Core.Tcp;

namespace Core.Api.Net
{
    public class ClientServer : TcpServer
    {
        public ClientServer(int port) : base(port) { }

        public override void AwaitConnections()
        {
            while (true)
            {
                // wait for client connection
                TcpClient client = server.AcceptTcpClient();
                if (!ClientManager.SetClient(client))
                    client.Close();
            }
        }
    }
}
