using System.Net.Sockets;
using Core.Transactions;
using Core.Utilities;

namespace Core.Api
{
    public class ClientManager
    {
        private static ILoggable Log;
        private static Client client;
        private static Core core;

        public ClientManager(Core core, ILoggable Log = null)
        {
            ClientManager.core = core;
            ClientManager.Log = Log ?? new NullLogger();

            _ = new ClientServer(Config.TcpPortApi);
        }

        public static void Push(Transaction data)
        {
            Log.NewLine($"Pushing transaction {data.Id} to the client.");
            if (client != null)
                client.Push(data);
        }

        public static void Push(Block data)
        {
            Log.NewLine($"Pushing block {data.Index} to the client.");
            if (client != null)
                client.Push(data);
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
