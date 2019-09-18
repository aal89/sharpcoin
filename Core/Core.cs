using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.Api;
using Core.P2p;
using Core.Transactions;
using Core.Utilities;
using Open.Nat;

namespace Core
{
    public class Core
    {
        public readonly Blockchain Blockchain;
        public readonly PeerManager PeerManager;

        private readonly Operator Operator;

        private readonly ILoggable Log = new Logger("Core");

        public Core(string[] args)
        {
            Log.NewLine("sharpcoin (core) v0.1 -- by aal89");

            // Configure the ip address to bind on
            if (args.Length == 1 && IPAddress.Parse(args[0]) != null)
                IpAddr.Set(args[0]);
            Log.NewLine($"Attempting to bind tcp servers to address: {IpAddr.Mine()}.");

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

            // Creating upnp mapping to optimize connections
            Log.NewLine($"Creating UPnP port mapping.");
            _ = CreateUPnPMapping();

            PeerManager = new PeerManager(this, new Logger("PeerManager"));
        }

        public async Task CreateUPnPMapping()
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(3000);
                NatDevice device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                // max value so that a Session object is created internally in the lib, only session objects
                // are properly renewed (10min intervals)...
                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 18910, 18910, int.MaxValue, "sc.Nat"));

                Log.NewLine("Successfully created UPnP port mapping.");
            }
            catch
            {
                Log.NewLine("Could not create UPnP port mapping. Decreased network connectivity.");
            }
            
        }

        public Operator GetOperator()
        {
            return Operator;
        }

        private void Blockchain_QueuedTransactionAdded(object sender, EventArgs e)
        {
            Transaction t = (Transaction)sender;
            PeerManager.BroadcastTransaction(t);
            ClientManager.Push(t);
        }

        private void Blockchain_BlockAdded(object sender, EventArgs e)
        {
            Block b = (Block)sender;
            // Broadcast block to all peers
            PeerManager.BroadcastBlock(b);
            ClientManager.Push(b);

            // Iff were mining, stop and start again.
            if (Operator.Busy())
            {
                Operator.Stop();
                // Give the operator some time to stop before starting again.
                Task.Delay(100).ContinueWith(task => Operator.Start());
            }
        }

        static void Main(string[] args) => new Core(args);
    }
}
