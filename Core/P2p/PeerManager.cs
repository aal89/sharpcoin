using System;
using System.Collections.Generic;
using Core.TCP;
using System.Linq;
using System.IO;
using Core.Utilities;

namespace Core.P2p
{
    public class PeerManager
    {
        private static readonly HashSet<CoreClient> peers = new HashSet<CoreClient>();
        private static readonly string peersPath = Path.Combine(Directory.GetCurrentDirectory(), "peers.txt");
        private static ILoggable log;

        public PeerManager(Core core, ILoggable log = null)
        {
            PeerManager.log = log ?? new NullLogger();

            if (!File.Exists(peersPath))
            {
                File.Create(peersPath).Dispose();
            }

            string[] ips = File.ReadAllLines(peersPath);

            foreach (string ip in ips)
            {
                try
                {
                    CoreClient c = new CoreClient(core, ip);
                    peers.Add(c);
                } catch
                {
                    log.NewLine($"Failed to connect to {ip}, removing from peer list.");
                }
            }

            SavePeers(GetPeersAsIps(), true);

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

        // Default class operations

        public static void SavePeers(string peer)
        {
            SavePeers(new string[] { peer });
        }

        private static readonly object savepeers_operation = new object();
        public static void SavePeers(string[] peers, bool overwrite = false)
        {
            lock (savepeers_operation)
            {
                if (overwrite)
                {
                    File.WriteAllLines(peersPath, peers);
                }
                else
                {
                    File.AppendAllLines(peersPath, peers);
                }
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
    }
}
