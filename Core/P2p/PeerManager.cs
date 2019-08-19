using System;
using System.Collections.Generic;
using Core.TCP;
using System.Linq;
using System.IO;
using Core.Utilities;
using Core.Transactions;

namespace Core.P2p
{
    public class PeerManager
    {
        private static readonly HashSet<CoreClient> peers = new HashSet<CoreClient>();
        private static readonly string peersPath = Path.Combine(Directory.GetCurrentDirectory(), "peers.txt");
        private static ILoggable log;
        private static Core core;

        public PeerManager(Core core, ILoggable log = null)
        {
            PeerManager.log = log ?? new NullLogger();
            PeerManager.core = core;

            if (!File.Exists(peersPath))
            {
                File.Create(peersPath).Dispose();
            }

            // Take some unique random ips from all the saved peers to connect to initially.
            string[] ips = File.ReadAllLines(peersPath)
                .Distinct()
                .OrderBy(x => new System.Random().Next())
                .Take(Config.MaximumOutgoingConnections)
                .ToArray();

            foreach (string ip in ips)
            {
                try
                {
                    CoreClient c = new CoreClient(core, ip, new Logger($"Peer {ip}"));
                    peers.Add(c);
                } catch
                {
                    log.NewLine($"Failed to connect to {ip}, removing from peer list.");
                }
            }

            SavePeers(GetPeersAsIps());

            // Final step: initiate the server
            _ = new CoreServer(core, new Logger("CoreServer"));
        }

        // Peer operations

        public static void BroadcastBlock(Block block)
        {
            log.NewLine($"Broadcasting block {block.Index}.");
            foreach(CoreClient c in peers)
            {
                c.AcceptBlock(block);
            }
        }

        public static void FetchRemoteBlock(int index)
        {
            log.NewLine($"Fetching block at remotes {index}.");
            foreach (CoreClient c in peers)
            {
                c.RequestBlock(index);
            }
        }

        public static void BroadcastPeers()
        {
            log.NewLine($"Broadcasting peers.");
            foreach (CoreClient c in peers)
            {
                c.AcceptPeers(GetPeersAsIps().Reduce(R.Concat(","), ""));
            }
        }

        public static void FetchRemotePeers()
        {
            log.NewLine($"Fetching peers at remotes.");
            foreach (CoreClient c in peers)
            {
                c.RequestPeers();
            }
        }

        public static void BroadcastTransaction(Transaction t)
        {
            log.NewLine($"Broadcasting transaction.");
            foreach (CoreClient c in peers)
            {
                c.AcceptTransaction(t);
            }
        }

        public static void FetchRemoteTransaction(string id)
        {
            log.NewLine($"Fetching transaction at remotes.");
            foreach (CoreClient c in peers)
            {
                c.RequestTransaction(id);
            }
        }

        // Default class operations

        public static void AddPeer(string peer, bool saveOnly = false)
        {
            try
            {
                if (!saveOnly)
                    peers.Add(new CoreClient(core, peer, new Logger($"Peer {peer}")));
                SavePeers(new string[] { peer });
            }
            catch
            {
                log.NewLine($"Failed to connect to {peer}, removing from peer list.");
            }
        }

        public static CoreClient[] GetPeers()
        {
            return peers.ToArray();
        }

        public static string[] GetPeersAsIps()
        {
            return peers.Map(client => client.Ip).ToArray();
        }

        private static readonly object savepeers_operation = new object();
        private static void SavePeers(string[] newpeers)
        {
            lock (savepeers_operation)
            {
                File.WriteAllLines(peersPath, newpeers);
            }
        }
    }
}
