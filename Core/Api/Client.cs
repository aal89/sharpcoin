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

        struct TransactionRecipient
        {
            private byte[] _RawAmount;
            public byte[] RawAmount
            {
                set {
                    if (BitConverter.IsLittleEndian)
                        _RawAmount = value.Reverse().ToArray();
                }
            }
            public byte[] RawRecipient;

            public string Recipient()
            {
                return Encoding.UTF8.GetString(RawRecipient);
            }

            public long Amount()
            {
                return BitConverter.ToInt64(_RawAmount);
            }
        }

        // todo: one big clunky method that could be split up
        public override void CreateTransaction(byte[] data)
        {
            int TotalKeySize = 96;
            int TransactionRecipientSize = 49;

            byte[] pubk = new byte[64];
            byte[] seck = new byte[32];

            Array.Copy(data, 0, pubk, 0, 64);
            Array.Copy(data, 64, seck, 0, 32);

            int TotalRecipients = (data.Length - TotalKeySize) / TransactionRecipientSize;
            TransactionRecipient[] txrs = new TransactionRecipient[TotalRecipients];

            for (int i = 0; i < TotalRecipients; i++)
            {
                byte[] rawamount = new byte[8];
                byte[] rawrecipient = new byte[41];

                Array.Copy(data, 96 + (i * TransactionRecipientSize), rawamount, 0, 8);
                Array.Copy(data, 104 + (i * TransactionRecipientSize), rawrecipient, 0, 41);

                txrs[i] = new TransactionRecipient
                {
                    RawAmount = rawamount,
                    RawRecipient = rawrecipient
                };
            }

            long TotalAmount = txrs.Map(rec => rec.Amount()).Reduce(R.Total, 0);

            SharpKeyPair skp = new SharpKeyPair(pubk, seck);
            Builder txb = new Builder(skp);

            IEnumerator<Output> utxos = ((IEnumerable<Output>)core.Blockchain.GetUnspentOutputs(skp.GetAddress())).GetEnumerator();

            while (txb.InputAmount() < TotalAmount && utxos.MoveNext())
            {
                MetaOutput output = (MetaOutput)utxos.Current;
                txb.AddInput(core.Blockchain.GetTransaction(output.Transaction), output.Index);
            }

            foreach(TransactionRecipient txr in txrs)
                txb.AddOutput(txr.Recipient(), txr.Amount());

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
