using System;
using System.Linq;
using System.Text;
using Core.P2p;
using Core.Utilities;

namespace Core.TCP
{
    public class CoreClient : AbstractCoreClient
    {
        private readonly Serializer serializer = new Serializer();

        public CoreClient(Core core, string server) : base(core, server) { }

        public override void AcceptBlock(Block block)
        {
            Send(Operation.Codes["AcceptBlock"], serializer.Serialize(block));
        }

        protected override void AcceptBlockResponse(byte[] data) { }

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

        public override void RequestTransactions(string txs)
        {
            throw new NotImplementedException();
        }

        protected override void RequestTransactionsResponse(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void AcceptTransactions(string txs)
        {
            throw new NotImplementedException();
        }

        protected override void AcceptTransactionsResponse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
