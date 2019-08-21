﻿using System.Collections.Generic;
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
            Log.NewLine($"Broadcasting block {block.Index}.");
            foreach(Peer p in peers)
            {
                p.AcceptBlock(block);
            }
        }

        public static void FetchRemoteBlock(int index)
        {
            Log.NewLine($"Fetching block at remotes {index}.");
            foreach (Peer p in peers)
            {
                p.RequestBlock(index);
            }
        }

        public static void BroadcastPeers()
        {
            Log.NewLine($"Broadcasting peers.");
            foreach (Peer p in peers)
            {
                p.AcceptPeers(GetPeersAsIps().Stringified(","));
            }
        }

        public static void FetchRemotePeers()
        {
            Log.NewLine($"Fetching peers at remotes.");
            foreach (Peer p in peers)
            {
                p.RequestPeers();
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

        public static void FetchRemoteTransaction(string id)
        {
            Log.NewLine($"Fetching transaction at remotes.");
            foreach (Peer p in peers)
            {
                p.RequestTransaction(id);
            }
        }

        public static void FetchBlockchainSize()
        {
            Log.NewLine($"Fetching blockchain sizes at remotes.");
            foreach (Peer p in peers)
            {
                p.RequestBlockchainSize();
            }
        }

        // Default class operations

        private static readonly object addpeers_operation = new object();
        public static bool AddPeer(string ip, bool saveOnly = false)
        {
            lock (addpeers_operation)
            {
                try
                {
                    Peer p = Peer.Create(Core, ip);
                    p.ClosedConn += Peer_ClosedConn;

                    if (AddPeer(p, saveOnly))
                        return true;

                    return false;
                }
                catch
                {
                    Log.NewLine($"Failed to connect to {ip}, removing from peer list.");
                    return false;
                }
            }
        }

        public static bool AddPeer(Peer p, bool saveOnly = false)
        {
            lock (addpeers_operation)
            {
                SavePeers(new string[] { p.Ip });
                p.ClosedConn += Peer_ClosedConn;
                if (!saveOnly && !HasMaximumConnections() && peers.Add(p))
                {
                    BroadcastPeers();
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
