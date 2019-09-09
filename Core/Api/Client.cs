using System;
using System.Net.Sockets;
using Core.Crypto;
using Core.Tcp;
using Core.Utilities;
using System.Linq;
using Core.Transactions;

namespace Core.Api
{
    public class Client : AbstractClient
    {
        private readonly Serializer serializer = new Serializer();
        private readonly ILoggable Log;

        public event EventHandler ClosedConn;

        public Client(Core core, TcpClient client, ILoggable log = null) : base(core, client)
        {
            Log = log ?? new NullLogger();

            Log.NewLine($"Connected successfully.");
        }

        public void Push(Block b)
        {
            Push(serializer.Serialize(b));
        }

        public void Push(Transaction tx)
        {
            Push(serializer.Serialize(tx));
        }

        private void Push(byte[] data)
        {
            Send(Opcodes["Push"], data);
        }

        protected override void ClosedConnection()
        {
            Log.NewLine($"Disconnected.");
            ClosedConn?.Invoke(this, EventArgs.Empty);
        }

        public override void RequestMining(byte[] data)
        {
            bool start = data[0] == 0x00 && data.Length == 97;

            Log.NewLine($"Mine command received ({(start ? "start" : "stop")}).");

            if (start)
            {
                byte[] pubk = new byte[64];
                byte[] seck = new byte[32];

                Array.Copy(data, 1, pubk, 0, 64);
                Array.Copy(data, 65, seck, 0, 32);

                core.StartMining(new SharpKeyPair(pubk, seck));
            }
            else
            {
                core.StopMining();
            }

            Send(Opcodes["RequestMiningResponse"], OK());
        }

        public override void RequestKeyPair(byte[] data)
        {
            Log.NewLine($"Sending keypair.");
            Send(Opcodes["RequestKeyPairResponse"], SharpKeyPair.Create().AsData());
        }

        public override void RequestBalance(byte[] data)
        {
            SharpKeyPair skp = new SharpKeyPair(data);
            long balance = core.Blockchain.Balance(skp);

            Log.NewLine($"Sending balance ({balance}) for address {skp.GetAddress()}.");

            Send(Opcodes["RequestBalanceResponse"], BitConverter.GetBytes(balance).Reverse().ToArray());
        }
    }
}
