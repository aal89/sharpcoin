using System;
using System.Collections.Generic;
using Core.TCP;
using System.Linq;
using System.IO;

namespace Core.P2p
{
    public class PeerManager
    {
        private static readonly HashSet<CoreClient> peers = new HashSet<CoreClient>();

        private static readonly string peersPath = Path.Combine(Directory.GetCurrentDirectory(), "peers.txt");

        public PeerManager(Core core)
        {
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
                    Console.WriteLine($"Failed to connect to {ip}, removing from peer list.");
                }
            }

            SavePeers(GetPeersAsIps(), true);

            // Final step: initiate the server
            _ = new CoreServer(core);
        }

        public static void SavePeers(string[] peers, bool overwrite = false)
        {
            if (overwrite)
            {
                File.WriteAllLines(peersPath, peers);
            } else
            {
                File.AppendAllLines(peersPath, peers);
            }
        }

        public static void BroadcastBlock(Block block)
        {
            foreach(CoreClient c in peers)
            {
                c.AcceptBlock(block);
            }
        }

        public static void FetchRemoteBlock(int index)
        {
            foreach (CoreClient c in peers)
            {
                c.RequestBlock(index);
            }
        }

        public static void FetchRemotePeers()
        {
            foreach (CoreClient c in peers)
            {
                c.RequestPeers();
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
