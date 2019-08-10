using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Core.TCP;
using System.Linq;

namespace Core.P2p
{
    public class PeerManager
    {
        private static readonly HashSet<CoreClient> peers = new HashSet<CoreClient>();

        // Todo: save/update and filter list of (working) peers
        public PeerManager(Core core)
        {
            // Todo: load peers from disk
            string[] ips = { "10.90.80.10" };
            foreach(string ip in ips)
            {
                try
                {
                    CoreClient c = new CoreClient(core, ip);
                    Console.WriteLine("here");
                    peers.Add(c);
                } catch(SocketException se)
                {
                    // Todo: remove ip from list
                    Console.WriteLine("timedout");
                }
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
