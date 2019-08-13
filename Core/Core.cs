using System;
using Core.Crypto;
using Core.P2p;
using Core.Utilities;

namespace Core
{
    public class Core
    {
        public readonly Blockchain Blockchain;
        public readonly PeerManager PeerManager;

        private readonly ILoggable Log = new Logger("Core");

        public Core()
        {
            Log.NewLine("sharpcoin v0.1 -- core by aal89");
            // Load blockchain
            Log.Line($"Loading blockchain...");
            Blockchain = new Blockchain();
            Log.Append($"Done. Size is {Blockchain.Size()}.");
            // Setup peer manager (server&client)
            Log.Line($"Setting up peer manager...");
            PeerManager = new PeerManager(this, new Logger("PeerManager"));
            Log.Append("Done.");

            Log.NewLine($"Ready and awaiting connections on 0.0.0.0:{Config.TcpPort}");
        }

        public void Mine()
        {
            Log.NewLine($"Started mining at {DateTime.UtcNow}");

            while (true)
            {
                Block b = Miner.Solve(SharpKeyPair.Create(), Blockchain);
                Blockchain.AddBlock(b);
            }
        }

        static void Main() => new Core();
    }
}
