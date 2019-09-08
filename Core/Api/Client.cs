using System;
using System.Net.Sockets;
using Core.Crypto;
using Core.Tcp;
using Core.Utilities;
using System.Linq;

namespace Core.Api
{
    public class Client : AbstractClient
    {
        private readonly Operations Operation = new ApiOperations();
        private readonly ILoggable Log;

        public event EventHandler ClosedConn;

        public Client(Core core, TcpClient client, ILoggable log = null) : base(core, client)
        {
            Log = log ?? new NullLogger();

            Log.NewLine($"Connected successfully.");
        }

        public void Push(byte[] data)
        {
            Send(Operation.Codes["Push"], data);
        }

        protected override void ClosedConnection()
        {
            Log.NewLine($"Disconnected.");
            ClosedConn?.Invoke(this, EventArgs.Empty);
        }

        public override void RequestMining(byte[] data)
        {
            byte[] pubk = new byte[64];
            byte[] seck = new byte[32];

            Array.Copy(data, pubk, 64);
            Array.Copy(data, 64, seck, 0, 32);

            core.StartMining(new SharpKeyPair(pubk, seck));

            Send(Operation.Codes["RequestMiningResponse"], Operation.OK());
        }

        public override void RequestKeyPair(byte[] data)
        {
            Log.NewLine($"Sending keypair.");
            Send(Operation.Codes["RequestKeyPairResponse"], SharpKeyPair.Create().AsData());
        }

        public override void RequestBalance(byte[] data)
        {
            SharpKeyPair skp = new SharpKeyPair(data);
            long balance = core.Blockchain.Balance(skp);

            Log.NewLine($"Sending balance ({balance}) for address {skp.GetAddress()}.");

            Send(Operation.Codes["RequestBalanceResponse"], BitConverter.GetBytes(balance).Reverse().ToArray());
        }
    }
}
