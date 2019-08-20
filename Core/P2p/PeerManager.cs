using System.Collections.Generic;
using System.Linq;
using System.IO;
using Core.Utilities;
using Core.Transactions;
using Core.P2p.Tcpn;

namespace Core.P2p
{
    public class PeerManager
    {
        private static readonly HashSet<Peer> peers = new HashSet<Peer>(new PeerComparer());
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
                .Take(Config.MaximumOutgoingConnections)
                .ToArray()
                .Shuffle();

            foreach (string ip in ips)
            {
                try
                {
                    Peer p = Peer.Create(core, ip);
                    peers.Add(p);
                } catch
                {
                    log.NewLine($"Failed to connect to {ip}, removing from peer list.");
                }
            }

            SavePeers(GetPeersAsIps());

            // Final step: initiate the server
            _ = new TcpServer(core, Config.TcpPort);
        }

        // Peer operations

        public static void BroadcastBlock(Block block)
        {
            log.NewLine($"Broadcasting block {block.Index}.");
            foreach(Peer p in peers)
            {
                p.AcceptBlock(block);
            }
        }

        public static void FetchRemoteBlock(int index)
        {
            log.NewLine($"Fetching block at remotes {index}.");
            foreach (Peer p in peers)
            {
                p.RequestBlock(index);
            }
        }

        public static void BroadcastPeers()
        {
            log.NewLine($"Broadcasting peers.");
            foreach (Peer p in peers)
            {
                p.AcceptPeers(GetPeersAsIps().Reduce(R.Concat(","), ""));
            }
        }

        public static void FetchRemotePeers()
        {
            log.NewLine($"Fetching peers at remotes.");
            foreach (Peer p in peers)
            {
                p.RequestPeers();
            }
        }

        public static void BroadcastTransaction(Transaction t)
        {
            log.NewLine($"Broadcasting transaction.");
            foreach (Peer p in peers)
            {
                p.AcceptTransaction(t);
            }
        }

        public static void FetchRemoteTransaction(string id)
        {
            log.NewLine($"Fetching transaction at remotes.");
            foreach (Peer p in peers)
            {
                p.RequestTransaction(id);
            }
        }

        // Default class operations

        private static readonly object addpeers_operation = new object();
        public static void AddPeer(string ip, bool saveOnly = false)
        {
            lock (addpeers_operation)
            {
                try
                {
                    if (!saveOnly && peers.Count < Config.MaximumOutgoingConnections)
                        peers.Add(Peer.Create(core, ip));
                    SavePeers(new string[] { ip });
                }
                catch
                {
                    log.NewLine($"Failed to connect to {ip}, removing from peer list.");
                }
            }
        }

        public static void AddPeer(Peer p, bool saveOnly = false)
        {
            lock (addpeers_operation)
            {
                if (!saveOnly && peers.Count < Config.MaximumOutgoingConnections)
                    peers.Add(p);
                SavePeers(new string[] { p.Ip });
            }
        }

        public static Peer[] GetPeers()
        {
            return peers.ToArray();
        }

        public static string[] GetPeersAsIps()
        {
            return peers.Map(p => p.Ip).ToArray();
        }

        
        private static void SavePeers(string[] newpeers)
        {
            File.WriteAllLines(peersPath, newpeers);
        }
    }
}
