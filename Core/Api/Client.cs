using System;
using System.Net.Sockets;
using Core.Tcp;
using Core.Utilities;

namespace Core.Api
{
    public class Client : AbstractClient
    {
        private readonly Serializer serializer = new Serializer();
        private readonly Operations Operation = new ApiOperations();
        private readonly ILoggable Log;

        public event EventHandler ClosedConn;

        public Client(Core core, TcpClient client, ILoggable log = null) : base(core, client)
        {
            Log = log ?? new NullLogger();

            Log.NewLine($"Connected successfully.");
        }

        protected override void ClosedConnection()
        {
            Log.NewLine($"Disconnected.");
            ClosedConn?.Invoke(this, EventArgs.Empty);
        }
    }
}
