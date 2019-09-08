using System;
using System.Threading;
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

        private readonly ILoggable Log = new Logger("Core");

        private bool IsMining;
        private Thread MineThread;
        private SharpKeyPair MiningKeyPair;

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

            // Setup peer manager (server&client)
            Log.NewLine($"Setting up peer manager.");
            PeerManager = new PeerManager(this, new Logger("PeerManager"));
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
            // Iff were mining, stop.
            if (IsMining)
            {
                StopMining();
                StartMining(MiningKeyPair);
            }
        }

        public void Mine(object skp)
        {
            while (IsMining)
            {
                DateTime started = DateTime.UtcNow;
                Log.NewLine($"Started mining at {started}. Attempting to solve block {Blockchain.GetLastBlock().Index + 1}.");

                Block b = Miner.Solve((SharpKeyPair)skp, Blockchain, IsMining);

                if (b != null)
                    Log.NewLine($"Solved block {b.Index} with nonce {b.Nonce} ({b.Hash.Substring(0, 10)}) in {(int)DateTime.UtcNow.Subtract(started).TotalMinutes} mins! Target diff was: {Blockchain.GetDifficulty()}.");

                try
                {
                    Blockchain.AddBlock(b);
                } catch
                {
                    Log.NewLine($"Adding mined block failed. Skipping.");
                }
                
            }
            Log.NewLine($"Stopped mining at {DateTime.UtcNow}.");
        }

        public void StartMining(SharpKeyPair skp)
        {
            if (!IsMining && skp != null)
            {
                IsMining = true;
                MiningKeyPair = skp;
                MineThread = new Thread(new ParameterizedThreadStart(Mine));
                MineThread.Start(skp);
            }
        }

        public void StopMining()
        {
            IsMining = false;
        }

        static void Main() => new Core();
    }
}
