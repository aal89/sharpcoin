using System.Net.Sockets;
using Core.Utilities;

namespace Core.Api
{
    public class ClientManager
    {
        private static Client client;
        private static Core core;

        public ClientManager(Core core)
        {
            ClientManager.core = core;

            _ = new ClientServer(Config.TcpPortApi);
        }

        // Only one client is allowed to be connected at any one time.
        public static bool SetClient(TcpClient conn)
        {
            if (client == null && core != null)
            {
                client = new Client(core, conn, new Logger($"Client {conn.Ip()}"));
                client.ClosedConn += Client_ClosedConn;
                return true;
            }
            return false;
        }

        private static void Client_ClosedConn(object sender, System.EventArgs e)
        {
            client = null;
        }
    }
}
