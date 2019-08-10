using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Core.TCP;
using System.Linq;
using System.IO;

namespace Core.P2p
{
    public class PeerManager
    {
        private static readonly HashSet<CoreClient> peers = new HashSet<CoreClient>();

        private static readonly string peersPath = Path.Combine(Directory.GetCurrentDirectory(), "peers.txt");

        // Todo: save/update and filter list of (working) peers
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

            SavePeers(true);
        }

        public static void SavePeers(bool overwrite = false)
        {
            if (overwrite)
            {
                File.WriteAllLines(peersPath, GetPeers());
            } else
            {
                // Todo:
                File.AppendAllLines(peersPath, GetPeers())
            }
        }

        public static void BroadcastBlock(Block block)
        {
            foreach(CoreClient c in peers)
            {
                c.AcceptBlock(block);
            }
        }

        public static void FetchBlock(int index)
        {
            foreach (CoreClient c in peers)
            {
                c.RequestBlock(index);
            }
        }

        public static void FetchPeers()
        {
            foreach (CoreClient c in peers)
            {
                c.RequestPeers();
            }
        }

        public static string[] GetPeers()
        {
            return peers.Map(client => client.Ip).ToArray();
        }
    }
}
