﻿using System;
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
                if (core.bc.GetBlockByHash(block.Hash) == null)
                    core.bc.AddBlock(block);
            }
        }

        public override void RequestPeers()
        {
            Send(Operation.Codes["RequestPeers"], Operation.NOOP());
        }

        protected override void RequestPeersResponse(byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",");
            PeerManager.SavePeers(peers);
        }

        public override void AcceptPeers()
        {
            string peers = PeerManager.GetPeersAsIps().Reduce(R.Concat(","), "");
            Send(Operation.Codes["AcceptPeers"], peers);
        }

        protected override void AcceptPeersResponse(byte[] data) { }
    }
}
