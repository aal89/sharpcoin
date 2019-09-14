using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core.Tcp;
using Core.Transactions;
using Core.Utilities;

namespace Core.P2p.Net
{
    public class Peer : AbstractClient
    {
        private readonly Core Core;
        private readonly ILoggable Log;

        private Peer(Core Core, Operations Operations, TcpClient Client, ILoggable Log = null): base(Operations, Client)
        {
            this.Log = Log ?? new NullLogger();
            this.Core = Core;

            OpenenConn += Peer_OpenenConn;
            ClosedConn += Peer_ClosedConn;
        }

        private void Peer_ClosedConn(object sender, EventArgs e)
        {
            Log.NewLine($"Disconnected.");
        }

        private void Peer_OpenenConn(object sender, EventArgs e)
        {
            Log.NewLine($"Connected successfully.");
            RequestBlockchainSize();
            RequestPeers();
        }

        public static Peer Create(Core core, string ip)
        {
            TcpClient tcp = new TcpClient();
            tcp.ConnectAsync(ip, Config.TcpPort).Wait(Config.TcpConnectTimeout);
            return Create(core, tcp);
        }

        public static Peer Create(Core core, TcpClient client)
        {
            return new Peer(core, new PeerOperations(), client, new Logger($"Peer {client.Ip()}"));
        }

        public override void Incoming(byte type, byte[] data)
        {
            switch (type)
            {
                case 0x01: ServeRequestBlock(data); break;
                case 0x02: RequestBlockResponse(data); break;
                case 0x03: ServeAcceptBlock(data); break;
                case 0x04: AcceptBlockResponse(data); break;
                case 0x05: ServeRequestPeers(data); break;
                case 0x06: RequestPeersResponse(data); break;
                case 0x07: ServeAcceptPeers(data); break;
                case 0x08: AcceptPeersResponse(data); break;
                case 0x09: ServeRequestTransaction(data); break;
                case 0x0a: RequestTransactionResponse(data); break;
                case 0x0b: ServeAcceptTransaction(data); break;
                case 0x0c: AcceptTransactionResponse(data); break;
                case 0x0d: ServeRequestBlockchainSize(); break;
                case 0x0e: RequestBlockchainSizeResponse(data); break;
            }
        }

        public readonly static object synchchain_operation = new object();
        public void SynchChain(int peerSize)
        {
            Log.NewLine($"Synching chain.");
            lock (synchchain_operation)
            {
                int BcSize = Core.Blockchain.Size();
                // When synching with a peer we try to pull in some extra blocks (see 
                // reducedSize). This is a way to solve orphanchains with fewer (or
                // no) hashing power. Eventually the chain supported by the most hashing
                // power should win and becomes the truth again.
                int reducedSize = Math.Max(1, BcSize - (peerSize - BcSize));

                // Synch for our reduced size untill we have the size of our peer,
                // delete our block before requesting the new one.
                for (int i = reducedSize; i <= peerSize; i++)
                {
                    Core.Blockchain.RemoveBlock(Core.Blockchain.GetBlockByIndex(i));
                    RequestBlock(i);
                    Thread.Sleep(100);
                }
            }
        }

        // =====

        public void RequestBlock(int index)
        {
            Log.NewLine($"Requesting block {index}.");
            Send(Opcodes["RequestBlock"], BitConverter.GetBytes(index).Reverse().ToArray());
        }

        protected void ServeRequestBlock(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                data = data.Reverse().ToArray();

            int index = BitConverter.ToInt32(data, 0);

            try
            {
                byte[] compressedBlock = Serializer.Serialize(Core.Blockchain.GetBlockByIndex(index) ?? Core.Blockchain.GetBlockByIndex(0));

                Log.NewLine($"Sending block {index}.");

                Send(Opcodes["RequestBlockResponse"], compressedBlock);
            }
            catch
            {
                Log.NewLine($"Noop'ed on block request {index}.");
                Send(Opcodes["RequestBlockResponse"], NOOP());
            }
        }

        protected void RequestBlockResponse(byte[] data)
        {
            if (!IsNOOP(data))
            {
                Block block = Serializer.Deserialize<Block>(data);
                Log.NewLine($"Got block {block.Index}.");
                Core.Blockchain.AddBlock(block, null, false);
            }
        }

        // =====

        public void AcceptBlock(Block block)
        {
            Log.NewLine($"Sending block {block.Index}.");
            Send(Opcodes["AcceptBlock"], Serializer.Serialize(block));
        }

        protected void ServeAcceptBlock(byte[] data)
        {
            try
            {
                Block block = Serializer.Deserialize<Block>(data);
                Log.NewLine($"Accepting block {block.Index}.");
                Core.Blockchain.AddBlock(block);
                Send(Opcodes["AcceptBlockResponse"], OK());
            }
            catch
            {
                Log.NewLine($"Rejecting block received.");
                Send(Opcodes["AcceptBlockResponse"], NOOP());
            }
        }

        protected void AcceptBlockResponse(byte[] data)
        {
            string status = IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Block got {status} by peer.");
        }

        // =====

        public void RequestPeers()
        {
            Log.NewLine($"Requesting all peers.");
            Send(Opcodes["RequestPeers"], NOOP());
        }

        protected void ServeRequestPeers(byte[] data)
        {
            Log.NewLine($"Sending peers.");
            string peers = PeerManager.GetPeersAsIps().Stringified(",");
            Send(Opcodes["RequestPeersResponse"], peers);
        }

        protected void RequestPeersResponse(byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",");
            Log.NewLine($"Peer responded with {peers.Length} peers.");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        // =====

        public void AcceptPeers(string peers)
        {
            Log.NewLine($"Sending peers.");
            Send(Opcodes["AcceptPeers"], peers);
        }

        protected void ServeAcceptPeers(byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",").Filter(ip => ip != IpAddr.Mine()).ToArray();
            Log.NewLine($"Accepting {peers.Length} peers.");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        protected void AcceptPeersResponse(byte[] data)
        {
            string status = IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Peers got {status} by peer.");
        }

        // =====

        public void RequestTransaction(string id)
        {
            Log.NewLine($"Requesting transaction {id}.");
            Send(Opcodes["RequestTransaction"], id);
        }

        protected void ServeRequestTransaction(byte[] data)
        {
            string id = Encoding.UTF8.GetString(data);
            Log.NewLine($"Sending transaction {id}.");
            Transaction tx = Core.Blockchain.GetQueuedTransactionById(id);
            Send(Opcodes["RequestTransactionResponse"], Serializer.Serialize(tx));
        }

        protected void RequestTransactionResponse(byte[] data)
        {
            Transaction tx = Serializer.Deserialize<Transaction>(data);

            if (tx.Verify() && tx.IsDefaultTransaction())
            {
                Log.NewLine($"Got transaction {tx.Id}.");
                Core.Blockchain.QueueTransaction(tx);
            }
        }

        // =====

        public void AcceptTransaction(Transaction tx)
        {
            Log.NewLine($"Sending transaction {tx.Id}.");
            Send(Opcodes["AcceptTransaction"], Serializer.Serialize(tx));
        }

        protected void ServeAcceptTransaction(byte[] data)
        {
            Transaction tx = Serializer.Deserialize<Transaction>(data);
            
            if (tx.Verify() && tx.IsDefaultTransaction() && Core.Blockchain.QueueTransaction(tx))
            {
                Send(Opcodes["AcceptTransactionResponse"], OK());
            } else
            {
                Send(Opcodes["AcceptTransactionResponse"], NOOP());
            }
        }

        protected void AcceptTransactionResponse(byte[] data)
        {
            string status = IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Transaction got {status} by peer.");
        }

        // =====

        public void RequestBlockchainSize()
        {
            Log.NewLine($"Requesting blockchain size.");
            Send(Opcodes["RequestBlockchainSize"], NOOP());
        }

        protected void ServeRequestBlockchainSize()
        {
            Log.NewLine($"Sending blockchain size.");
            Send(Opcodes["RequestBlockchainSizeResponse"], BitConverter.GetBytes(Core.Blockchain.Size()).Reverse().ToArray());
        }

        protected void RequestBlockchainSizeResponse(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                data = data.Reverse().ToArray();

            int size = BitConverter.ToInt32(data, 0);
            int mysize = Core.Blockchain.Size();

            Log.NewLine($"Blockchain size at peer is {size}. My size {mysize}.");

            if (size > mysize && !Monitor.IsEntered(synchchain_operation))
                SynchChain(size);
        }

        // =====
    }
}
