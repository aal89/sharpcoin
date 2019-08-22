using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core.Tcpn;
using Core.Transactions;
using Core.Utilities;

namespace Core.P2p.Tcpn
{
    public class Peer: AbstractPeer
    {
        private readonly Serializer serializer = new Serializer();
        private readonly ILoggable Log;

        public event EventHandler ClosedConn;

        private Peer(Core core, TcpClient client, ILoggable log = null): base(core, client)
        {
            Log = log ?? new NullLogger();

            Log.NewLine($"Connected successfully.");
        }

        protected override void ClosedConnection()
        {
            Log.NewLine($"Disconnected.");
            ClosedConn?.Invoke(this, EventArgs.Empty);
        }

        public static Peer Create(Core core, string ip)
        {
            return Create(core, new TcpClient(ip, Config.TcpPort));
        }

        public static Peer Create(Core core, TcpClient client)
        {
            return new Peer(core, client, new Logger($"Peer {client.Ip()}"));
        }

        public readonly object synchchain_operation = new object();
        public void SynchChain(int peerSize)
        {
            Log.NewLine($"Synching chain.");
            lock (synchchain_operation)
            {
                // When synching with a peer we try to pull some extra blocks. This is to prevent
                // orphan blocks that we may have to be overwritten with blocks from the
                // peer as its chain is longer. Bringing us into synch again.
                // Note: this does require us to remove the last x blocks from our chain first.
                int reducedSize = Math.Max(1, core.Blockchain.Size() - peerSize);

                while (core.Blockchain.Size() != reducedSize)
                    core.Blockchain.RemoveBlock(core.Blockchain.GetLastBlock());

                for (int i = reducedSize; i < peerSize; i++)
                    RequestBlock(i);
            }
        }

        // =====

        public override void RequestBlock(int index)
        {
            Log.NewLine($"Requesting block {index}.");
            Send(Operation.Codes["RequestBlock"], BitConverter.GetBytes(index).Reverse().ToArray());
        }

        protected override void ServeRequestBlock(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                data = data.Reverse().ToArray();

            int index = BitConverter.ToInt32(data, 0);

            try
            {
                byte[] compressedBlock = serializer.Serialize(core.Blockchain.GetBlockByIndex(index) ?? core.Blockchain.GetBlockByIndex(0));

                Log.NewLine($"Sending block {index}.");

                Send(Opcodes["RequestBlockResponse"], compressedBlock);
            }
            catch
            {
                Log.NewLine($"Noop'ed on block request {index}.");
                Send(Opcodes["RequestBlockResponse"], Operation.NOOP());
            }
        }

        protected override void RequestBlockResponse(byte[] data)
        {
            if (!Operation.IsNOOP(data))
            {
                Block block = serializer.Deserialize<Block>(data);
                Log.NewLine($"Got block {block.Index}.");
                core.Blockchain.AddBlock(block);
            }
        }

        // =====

        public override void AcceptBlock(Block block)
        {
            Log.NewLine($"Sending block {block.Index}.");
            Send(Operation.Codes["AcceptBlock"], serializer.Serialize(block));
        }

        protected override void ServeAcceptBlock(byte[] data)
        {
            try
            {
                Block block = serializer.Deserialize<Block>(data);
                Log.NewLine($"Accepting block {block.Index}.");
                core.Blockchain.AddBlock(block);
                Send(Opcodes["AcceptBlockResponse"], Operation.OK());
            }
            catch
            {
                Log.NewLine($"Rejecting block received.");
                Send(Opcodes["AcceptBlockResponse"], Operation.NOOP());
            }
        }

        protected override void AcceptBlockResponse(byte[] data)
        {
            string status = Operation.IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Block got {status} by peer.");
        }

        // =====

        public override void RequestPeers()
        {
            Log.NewLine($"Requesting all peers.");
            Send(Operation.Codes["RequestPeers"], Operation.NOOP());
        }

        protected override void ServeRequestPeers(byte[] data)
        {
            Log.NewLine($"Sending peers.");
            string peers = PeerManager.GetPeersAsIps().Stringified(",");
            Send(Operation.Codes["RequestPeersResponse"], peers);
        }

        protected override void RequestPeersResponse(byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",");
            Log.NewLine($"Peer responded with {peers.Length} peers.");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        // =====

        public override void AcceptPeers(string peers)
        {
            Log.NewLine($"Sending peers.");
            Send(Operation.Codes["AcceptPeers"], peers);
        }

        protected override void ServeAcceptPeers(byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",").Filter(ip => ip != IpAddr.Mine()).ToArray();
            Log.NewLine($"Accepting {peers.Length} peers.");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        protected override void AcceptPeersResponse(byte[] data)
        {
            string status = Operation.IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Peers got {status} by peer.");
        }

        // =====

        public override void RequestTransaction(string id)
        {
            Log.NewLine($"Requesting transaction {id}.");
            Send(Operation.Codes["RequestTransaction"], id);
        }

        protected override void ServeRequestTransaction(byte[] data)
        {
            string id = Encoding.UTF8.GetString(data);
            Log.NewLine($"Sending transaction {id}.");
            Transaction tx = core.Blockchain.GetQueuedTransactionById(id);
            Send(Operation.Codes["RequestTransactionResponse"], serializer.Serialize(tx));
        }

        protected override void RequestTransactionResponse(byte[] data)
        {
            Transaction tx = serializer.Deserialize<Transaction>(data);
            Log.NewLine($"Got transaction {tx.Id}.");
            core.Blockchain.QueueTransaction(tx);
        }

        // =====

        public override void AcceptTransaction(Transaction tx)
        {
            Log.NewLine($"Sending transaction {tx.Id}.");
            Send(Operation.Codes["AcceptTransactions"], serializer.Serialize(tx));
        }

        protected override void ServeAcceptTransaction(byte[] data)
        {
            Transaction tx = serializer.Deserialize<Transaction>(data);
            Log.NewLine($"Accepting transaction {tx.Id}.");
            core.Blockchain.QueueTransaction(tx);
            Send(Operation.Codes["AcceptTransactionResponse"], Operation.OK());
        }

        protected override void AcceptTransactionResponse(byte[] data)
        {
            string status = Operation.IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Transaction got {status} by peer.");
        }

        // =====

        public override void RequestBlockchainSize()
        {
            Log.NewLine($"Requesting blockchain size.");
            Send(Operation.Codes["RequestBlockchainSize"], Operation.NOOP());
        }

        protected override void ServeRequestBlockchainSize()
        {
            Log.NewLine($"Sending blockchain size.");
            Send(Operation.Codes["RequestBlockchainSizeResponse"], BitConverter.GetBytes(core.Blockchain.Size()).Reverse().ToArray());
        }

        protected override void RequestBlockchainSizeResponse(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                data = data.Reverse().ToArray();

            int size = BitConverter.ToInt32(data, 0);
            int mysize = core.Blockchain.Size();

            Log.NewLine($"Blockchain size at peer is {size}. My size {mysize}.");

            if (size > mysize && !Monitor.IsEntered(synchchain_operation))
                SynchChain(size);
        }

        // =====
    }
}
