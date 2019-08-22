using System;
using System.Threading;
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

        private Thread MineThread;
        private bool IsMining;
        private SharpKeyPair MineRewardKeyPair;

        public Core()
        {
            Log.NewLine("sharpcoin v0.1 -- core by aal89");
            // Load blockchain
            Log.Line($"Loading blockchain...");
            Blockchain = new Blockchain();
            Log.Append($"Done. Size is {Blockchain.Size()}.");
            // Setup peer manager (server&client)
            Log.NewLine($"Setting up peer manager...");
            PeerManager = new PeerManager(this, new Logger("PeerManager"));
            Log.NewLine($"Done. Awaiting connections on 0.0.0.0:{Config.TcpPort}.");
            // Setup event listeners
            Log.Line("Setting up event listeners...");
            Blockchain.BlockAdded += Blockchain_BlockAdded;
            Blockchain.QueuedTransactionAdded += Blockchain_QueuedTransactionAdded;
            Log.Append("Done.");

            Log.NewLine("Initialized successfully!");

            //PeerManager.AddPeer("192.168.1.31");
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
            // Iff were mining; stop and start again
            if (IsMining)
            {
                StopMining();
                StartMining(MineRewardKeyPair);
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
                    Log.NewLine($"Adding mined block {b.Index} failed. Skipping.");
                }
                
            }
            Log.NewLine($"Stopped mining at {DateTime.UtcNow}.");
        }

        public void StartMining(SharpKeyPair skp)
        {
            if (!IsMining && MineRewardKeyPair != null)
            {
                IsMining = true;
                MineThread = new Thread(new ParameterizedThreadStart(Mine));
                MineThread.Start(skp);
            }
        }

        public void StopMining()
        {
            IsMining = false;
            MineThread.Abort();
        }

        static void Main() => new Core();
    }
}
