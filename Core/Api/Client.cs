﻿using System;
using System.Net.Sockets;
using Core.Crypto;
using Core.Utilities;
using System.Linq;
using Core.Transactions;
using System.Collections.Generic;
using System.Text;
using Core.Tcp;

namespace Core.Api
{
    public class Client : AbstractClient
    {
        private readonly Serializer Serializer = new Serializer();
        private readonly Core Core;
        private readonly ILoggable Log;

        public event EventHandler ClosedConn;

        private Client(Core Core, Operations Operations, TcpClient Client, ILoggable Log = null) : base(Operations, Client)
        {
            this.Log = Log ?? new NullLogger();
            this.Core = Core;

            Log.NewLine($"Connected successfully.");
        }

        public static Client Create(Core Core, TcpClient Client)
        {
            return new Client(Core, new ApiOperations(), Client, new Logger($"Client {Client.Ip()}"));
        }

        public override void Incoming(byte type, byte[] data)
        {
            switch (type)
            {
                case 0x01: RequestMining(data); break;
                case 0x03: RequestKeyPair(data); break;
                case 0x05: RequestBalance(data); break;
                case 0x07: CreateTransaction(data); break;
            }
        }

        public void Push(Block b)
        {
            Push(Serializer.Serialize(b));
        }

        public void Push(Transaction tx)
        {
            Push(Serializer.Serialize(tx));
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

        public void RequestMining(byte[] data)
        {
            bool start = data[0] == 0x00 && data.Length == 97;

            Log.NewLine($"Mine command received ({(start ? "start" : "stop")}).");

            if (start)
            {
                byte[] pubk = new byte[64];
                byte[] seck = new byte[32];

                Array.Copy(data, 1, pubk, 0, 64);
                Array.Copy(data, 65, seck, 0, 32);

                Core.StartMining(new SharpKeyPair(pubk, seck));
            }
            else
            {
                Core.StopMining();
            }

            Send(Opcodes["RequestMiningResponse"], OK());
        }

        public void RequestKeyPair(byte[] data)
        {
            Log.NewLine($"Sending keypair.");
            Send(Opcodes["RequestKeyPairResponse"], SharpKeyPair.Create().AsData());
        }

        public void RequestBalance(byte[] data)
        {
            SharpKeyPair skp = new SharpKeyPair(data);
            long balance = Core.Blockchain.Balance(skp);

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
        public void CreateTransaction(byte[] data)
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

            IEnumerator<Output> utxos = ((IEnumerable<Output>)Core.Blockchain.GetUnspentOutputs(skp.GetAddress())).GetEnumerator();

            while (txb.InputAmount() < TotalAmount && utxos.MoveNext())
            {
                MetaOutput output = (MetaOutput)utxos.Current;
                txb.AddInput(Core.Blockchain.GetTransaction(output.Transaction), output.Index);
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
