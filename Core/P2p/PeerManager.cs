using System.Collections.Generic;
using System.Linq;
using System.IO;
using Core.Utilities;
using Core.Transactions;
using Core.P2p.Net;
using System.Net.Sockets;
using System.Timers;
using System;

namespace Core.P2p
{
    public class PeerManager
    {
        private static readonly HashSet<Peer> peers = new HashSet<Peer>(new PeerComparer());
        private static readonly string peersPath = Path.Combine(Directory.GetCurrentDirectory(), "peers.txt");

        private static ILoggable Log;
        private static Core Core;

        private static Timer PeerInterval;
        private static Timer KeepAliveInterval;

        public PeerManager(Core core, ILoggable log = null)
        {
            Log = log ?? new NullLogger();
            Core = core;

            if (!File.Exists(peersPath))
            {
                File.Create(peersPath).Dispose();
            }

            Log.NewLine("Setting up server and accepting connections...");

            // Take some unique random ips from all the saved peers to connect to initially.
            string[] ips = File.ReadAllLines(peersPath)
                .Distinct()
                .ToArray()
                .Shuffle()
                .Take(Config.MaximumConnections)
                .ToArray();

            foreach (string ip in ips)
                AddPeer(ip);

            PeerInterval = new Timer(Config.PeerInterval);
            PeerInterval.Elapsed += PeerInterval_Elapsed;
            PeerInterval.AutoReset = true;
            PeerInterval.Enabled = true;

            KeepAliveInterval = new Timer(Config.PeerKeepAliveInterval);
            KeepAliveInterval.Elapsed += KeepAliveInterval_Elapsed;
            KeepAliveInterval.AutoReset = true;
            KeepAliveInterval.Enabled = true;

            // Final step: initiate the server
            _ = new PeerServer(Config.TcpPort);
        }

        private void KeepAliveInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Because the keepalive is happening quite often these operations are silent as in no log lines.
            foreach (Peer p in peers)
                p.Signal();
        }

        // Peer operations

        private static void PeerInterval_Elapsed(object source, ElapsedEventArgs e)
        {
            // On each timed event we share our list of peers and check if the blockchain
            // is in synch.
            BroadcastBlockchainSize();
            BroadcastPeers();
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

        public static void BroadcastPeers()
        {
            Log.NewLine("Broadcasting peers.");
            foreach (Peer p in peers)
            {
                p.AcceptPeers(GetPeersAsIps().Stringified(","));
            }
        }

        public static void BroadcastBlockchainSize()
        {
            Log.NewLine($"Broadcasting blockchain size.");
            foreach (Peer p in peers)
            {
                p.AcceptBlockchainSize();
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
                    return ip != IpAddr.Mine() && !HasPeer(ip) && !HasMaximumConnections() && AddPeer(Peer.Create(Core, ip));
                }
                catch
                {
                    Log.NewLine($"Failed to connect to {ip}.");
                    return false;
                }
            }
        }

        public static bool AddPeer(TcpClient tcpc)
        {
            lock (addpeers_operation)
            {
                try
                {
                    return tcpc.Ip() != IpAddr.Mine() && !HasPeer(tcpc.Ip()) && !HasMaximumConnections() && AddPeer(Peer.Create(Core, tcpc));
                }
                catch
                {
                    Log.NewLine($"Failed to connect to {tcpc.Ip()}.");
                    return false;
                }
            }
        }

        private static bool AddPeer(Peer p)
        {
            lock (addpeers_operation)
            {
                p.ClosedConn += Peer_ClosedConn;
                p.OpenenConn += Peer_OpenenConn;

                if (peers.Add(p))
                    return true;
                return false;
            }
        }

        private static void Peer_OpenenConn(object sender, EventArgs e)
        {
            // Save all peers to a file
            SavePeers(GetPeersAsIps().ToArray());
        }

        private static void Peer_ClosedConn(object sender, System.EventArgs e)
        {
            peers.Remove((Peer)sender);
        }

        public static bool HasPeer(string ip)
        {
            return peers.Any(peer => peer.Ip == ip);
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
        
        private static void SavePeers(string[] newips)
        {
            List<string> ips = File.ReadAllLines(peersPath).ToList();

            foreach (string ip in newips)
                ips.Add(ip);

            File.WriteAllLines(peersPath, ips.Distinct().ToArray());
        }
    }
}
