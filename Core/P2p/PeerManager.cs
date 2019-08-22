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
        private static ILoggable Log;
        private static Core Core;

        public PeerManager(Core core, ILoggable log = null)
        {
            Log = log ?? new NullLogger();
            Core = core;

            if (!File.Exists(peersPath))
            {
                File.Create(peersPath).Dispose();
            }

            // Take some unique random ips from all the saved peers to connect to initially.
            string[] ips = File.ReadAllLines(peersPath)
                .Distinct()
                .Take(Config.MaximumConnections)
                .ToArray()
                .Shuffle();

            foreach (string ip in ips)
                AddPeer(ip);

            // Final step: initiate the server
            _ = new TcpServer(core, Config.TcpPort);
        }

        // Peer operations

        public static void PeerConnected(Peer p)
        {
            Log.NewLine("Savind new peer list and initiating contact.");
            SavePeers(GetPeersAsIps().Distinct().ToArray());
            p.AcceptPeers(GetPeersAsIps().Stringified(","));
            p.RequestBlockchainSize();
        }

        public static void BroadcastBlock(Block block)
        {
            Log.NewLine($"Broadcasting block {block.Index}.");
            foreach(Peer p in peers)
            {
                p.AcceptBlock(block);
            }
        }

        public static void BroadcastTransaction(Transaction t)
        {
            Log.NewLine($"Broadcasting transaction.");
            foreach (Peer p in peers)
            {
                p.AcceptTransaction(t);
            }
        }

        // Default class operations

        private static readonly object addpeers_operation = new object();
        public static bool AddPeer(string ip)
        {
            lock (addpeers_operation)
            {
                try
                {
                    if (AddPeer(Peer.Create(Core, ip)))
                        return true;

                    return false;
                }
                catch
                {
                    Log.NewLine($"Failed to connect to {ip}.");
                    return false;
                }
            }
        }

        public static bool AddPeer(Peer p)
        {
            lock (addpeers_operation)
            {
                p.ClosedConn += Peer_ClosedConn;
                if (!HasMaximumConnections() && peers.Add(p))
                {
                    PeerConnected(p);
                    return true;
                }
                return false;
            }
        }

        private static void Peer_ClosedConn(object sender, System.EventArgs e)
        {
            peers.Remove((Peer)sender);
        }

        public static Peer[] GetPeers()
        {
            return peers.ToArray();
        }

        public static string[] GetPeersAsIps()
        {
            return peers.Map(p => p.Ip).ToArray();
        }

        public static bool HasMaximumConnections()
        {
            return peers.Count >= Config.MaximumConnections;
        }
        
        private static void SavePeers(string[] newpeers)
        {
            File.WriteAllLines(peersPath, newpeers);
        }
    }
}
