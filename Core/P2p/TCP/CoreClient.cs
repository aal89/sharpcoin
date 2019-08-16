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
        private readonly ILoggable log;

        public CoreClient(Core core, string server, ILoggable log = null) : base(core, server)
        {
            this.log = log ?? new NullLogger();
            this.server = server;

            this.log.NewLine($"Connected to {server} succesfully.");
        }

        public override void AcceptBlock(Block block)
        {
            log.NewLine($"Sending block {block.Index} to peer {server}");
            Send(Operation.Codes["AcceptBlock"], serializer.Serialize(block));
        }

        protected override void AcceptBlockResponse(byte[] data)
        {
            string status = Operation.IsOK(data) ? "accepted" : "rejected";
            log.NewLine($"Block got {status} by peer {server}");
        }

        public override void RequestBlock(int index)
        {
            Send(Operation.Codes["RequestBlock"], BitConverter.GetBytes(index).Reverse().ToArray());
        }

        protected override void RequestBlockResponse(byte[] data)
        {
            if (!Operation.IsNOOP(data))
            {
                Block block = serializer.Deserialize<Block>(data);
                core.Blockchain.AddBlock(block);
            }
        }

        public override void RequestPeers()
        {
            Send(Operation.Codes["RequestPeers"], Operation.NOOP());
        }

        protected override void RequestPeersResponse(byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        public override void AcceptPeers(string peers)
        {
            Send(Operation.Codes["AcceptPeers"], peers);
        }

        protected override void AcceptPeersResponse(byte[] data) { }

        public override void RequestTransaction(string id)
        {
            Send(Operation.Codes["RequestTransaction"], id);
        }

        protected override void RequestTransactionResponse(byte[] data)
        {
            core.Blockchain.QueueTransaction(serializer.Deserialize<Transaction>(data));
        }

        public override void AcceptTransaction(Transaction tx)
        {
            Send(Operation.Codes["AcceptTransactions"], serializer.Serialize(tx));
        }

        protected override void AcceptTransactionResponse(byte[] data) { }
    }
}
