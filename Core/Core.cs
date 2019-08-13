using System;
using Core.Crypto;
using Core.P2p;
using Core.Utilities;

namespace Core
{
    public class Core
    {
        public readonly Blockchain bc;

        private readonly ILoggable Log = new Logger("Core");

        public Core()
        {
            Log.NewLine("sharpcoin v0.1 -- core by aal89");
            // Load blockchain
            Log.Line($"Loading blockchain...");
            bc = new Blockchain();
            Log.Append($"Done. Size is {bc.Size()}.");
            // Setup tcp server
            Log.Line($"Setting up peer manager...");
            _ = new PeerManager(this);
            Log.Append("Done.");

            Log.NewLine($"Ready and awaiting connections on 0.0.0.0:{Config.TcpPort}");
        }

        public void Mine()
        {
            Log.NewLine($"Started mining at {DateTime.UtcNow}");

            while (true)
            {
                Block b = Miner.Solve(SharpKeyPair.Create(), bc);
                bc.AddBlock(b);
            }
        }

        static void Main() => new Core();
    }
}
