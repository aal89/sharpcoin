using System;
using System.Net.Sockets;
using System.Linq;
using Core.Utilities;
using Core.P2p;

namespace Core.TCP
{
    public class CoreServer : AbstractCoreServer
    {
        private readonly Serializer serializer = new Serializer();

        public CoreServer(Core core) : base(core) { }

        public override void RequestBlock(TcpClient client, byte[] data)
        {
            try
            {
                if (BitConverter.IsLittleEndian)
                    data = data.Reverse().ToArray();

                int index = BitConverter.ToInt32(data, 0);
                byte[] compressedBlock = serializer.Serialize(core.bc.GetBlockByIndex(index) ?? core.bc.GetBlockByIndex(0));

                Console.WriteLine($"[CoreServer] Sending block {index} to {client.Client.RemoteEndPoint.ToString()}.");

                Send(client, Opcodes["RequestBlockResponse"], compressedBlock);
            } catch
            {
                Send(client, Opcodes["RequestBlockResponse"], Operation.NOOP());
            }
        }

        public override void AcceptBlock(TcpClient client, byte[] data)
        {
            try
            {
                Block block = serializer.Deserialize<Block>(data);

                if (core.bc.GetBlockByHash(block.Hash) == null)
                {
                    core.bc.AddBlock(block);
                    Send(client, Opcodes["AcceptBlockResponse"], Operation.OK());
                } else
                {
                    Send(client, Opcodes["AcceptBlockResponse"], Operation.NOOP());
                }
            } catch
            {
                Send(client, Opcodes["AcceptBlockResponse"], Operation.NOOP());
            }
        }

        public override void RequestPeers(TcpClient client, byte[] data)
        {
            string peers = PeerManager.GetPeersAsIps().Reduce(R.Concat(","), "");
            Send(client, Operation.Codes["RequestPeersResponse"], peers);
        }
    }
}
