using System;
using System.Linq;
using System.Text;
using Core.P2p;
using Core.Transactions;
using Core.Utilities;

namespace Core.TCP
{
    public class CoreClient : AbstractCoreClient
    {
        private readonly Serializer serializer = new Serializer();
        private readonly string server;
        private readonly ILoggable Log;

        public CoreClient(Core core, string server, ILoggable log = null) : base(core, server)
        {
            this.Log = log ?? new NullLogger();
            this.server = server;

            this.Log.NewLine($"Connected successfully.");
        }

        public override void AcceptBlock(Block block)
        {
            Log.NewLine($"Sending block {block.Index} to peer {server}.");
            Send(Operation.Codes["AcceptBlock"], serializer.Serialize(block));
        }

        protected override void AcceptBlockResponse(byte[] data)
        {
            string status = Operation.IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Block got {status} by peer {server}.");
        }

        public override void RequestBlock(int index)
        {
            Log.NewLine($"Requesting block {index} from peer {server}.");
            Send(Operation.Codes["RequestBlock"], BitConverter.GetBytes(index).Reverse().ToArray());
        }

        protected override void RequestBlockResponse(byte[] data)
        {
            if (!Operation.IsNOOP(data))
            {
                Block block = serializer.Deserialize<Block>(data);
                Log.NewLine($"Got block {block.Index} from peer {server}.");
                core.Blockchain.AddBlock(block);
            }
        }

        public override void RequestPeers()
        {
            Log.NewLine($"Requesting all peers from peer {server}.");
            Send(Operation.Codes["RequestPeers"], Operation.NOOP());
        }

        protected override void RequestPeersResponse(byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",");
            Log.NewLine($"Peer {server} responded with {peers.Length} new or existing peers.");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        public override void AcceptPeers(string peers)
        {
            Log.NewLine($"Sending peers to peer {server}.");
            Send(Operation.Codes["AcceptPeers"], peers);
        }

        protected override void AcceptPeersResponse(byte[] data)
        {
            string status = Operation.IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Peers got {status} by peer {server}.");
        }

        public override void RequestTransaction(string id)
        {
            Log.NewLine($"Requesting transaction {id} from peer {server}.");
            Send(Operation.Codes["RequestTransaction"], id);
        }

        protected override void RequestTransactionResponse(byte[] data)
        {
            Transaction tx = serializer.Deserialize<Transaction>(data);
            Log.NewLine($"Got transaction {tx.Id} from peer {server}.");
            core.Blockchain.QueueTransaction(tx);
        }

        public override void AcceptTransaction(Transaction tx)
        {
            Log.NewLine($"Sending transaction {tx.Id} to peer {server}.");
            Send(Operation.Codes["AcceptTransactions"], serializer.Serialize(tx));
        }

        protected override void AcceptTransactionResponse(byte[] data)
        {
            string status = Operation.IsOK(data) ? "accepted" : "rejected";
            Log.NewLine($"Transaction got {status} by peer {server}.");
        }
    }
}
