using System;
using Core.Api;
using Core.Crypto;
using Core.P2p;
using Core.Transactions;
using Core.Utilities;

namespace Core
{
    public class Core
    {
        public readonly Blockchain Blockchain;
        public readonly PeerManager PeerManager;

        private Operator Operator;

        private readonly ILoggable Log = new Logger("Core");

        public Core()
        {
            Log.NewLine("sharpcoin (core) v0.1 -- by aal89");

            // Load blockchain
            Log.NewLine($"Initializing blockchain.");
            Blockchain = new Blockchain(new Logger("Blockchain"));

            // Setup event listeners
            Log.Line("Setting up event listeners...");
            Blockchain.BlockAdded += Blockchain_BlockAdded;
            Blockchain.QueuedTransactionAdded += Blockchain_QueuedTransactionAdded;
            Log.Append("Done.");

            // Setup api
            Log.Line($"Setting up client management...");
            _ = new ClientManager(this, new Logger("ClientManager"));
            Log.Append("Done.");

            // Setup mine operator
            Log.Line($"Setting up mine operator...");
            Operator = new Operator(Blockchain, new Logger("Miner"));
            Log.Append("Done.");

            // Setup peer manager (server&client)
            Log.NewLine($"Setting up peer manager.");
            PeerManager = new PeerManager(this, new Logger("PeerManager"));

            Operator.Start(SharpKeyPair.Create());
        }

        public Operator GetOperator()
        {
            return Operator;
        }

        private void Blockchain_QueuedTransactionAdded(object sender, EventArgs e)
        {
            Transaction t = (Transaction)sender;
            PeerManager.BroadcastTransaction(t);
        }

        private void Blockchain_BlockAdded(object sender, EventArgs e)
        {
            Block b = (Block)sender;
            // Broadcast block to all peers
            PeerManager.BroadcastBlock(b);

            // Iff were mining, stop and start again.
            if (Operator.Busy())
            {
                Operator.Stop();
                Operator.Start();
            }
        }

        static void Main() => new Core();
    }
}
