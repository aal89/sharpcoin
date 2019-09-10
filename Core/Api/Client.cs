using System;
using System.Net.Sockets;
using Core.Crypto;
using Core.Utilities;
using System.Linq;
using Core.Transactions;
using System.Collections.Generic;
using System.Text;

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

        public override void CreateTransaction(byte[] data)
        {
            byte[] pubk = new byte[64];
            byte[] seck = new byte[32];
            byte[] rawamount = new byte[8];
            byte[] rawrecipient = new byte[41];

            Array.Copy(data, 0, pubk, 0, 64);
            Array.Copy(data, 64, seck, 0, 32);
            Array.Copy(data, 96, rawamount, 0, 8);
            Array.Copy(data, 104, rawrecipient, 0, 41);

            if (BitConverter.IsLittleEndian)
                rawamount = rawamount.Reverse().ToArray();

            SharpKeyPair skp = new SharpKeyPair(pubk, seck);
            Builder txb = new Builder(skp);
            long amount = BitConverter.ToInt64(rawamount);
            string recipient = Encoding.UTF8.GetString(rawrecipient);

            IEnumerator<Output> utxos = ((IEnumerable<Output>)core.Blockchain.GetUnspentOutputs(skp.GetAddress())).GetEnumerator();

            while (txb.InputAmount() < amount && utxos.MoveNext())
            {
                MetaOutput output = (MetaOutput)utxos.Current;
                txb.AddInput(core.Blockchain.GetTransaction(output.Transaction), output.Index);
            }

            txb.AddOutput(recipient, amount);

            try
            {
                Transaction newtx = txb.Make();

                Console.WriteLine(newtx);

                Send(Opcodes["CreateTransactionResponse"], OK());
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Send(Opcodes["CreateTransactionResponse"], NOOP());
            }
        }
    }
}
