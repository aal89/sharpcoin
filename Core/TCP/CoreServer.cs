using System;
using System.Net.Sockets;
using System.Linq;
using Core.Utilities;

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
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Send(client, Opcodes["RequestBlockResponse"], NOOP());
            }
        }

        public override void AcceptBlock(TcpClient client, byte[] data)
        {
            try
            {
                Block block = serializer.Deserialize<Block>(data);

                if (core.bc.GetBlockByHash(block.Hash) != null)
                    core.bc.AddBlock(block);

                Send(client, Opcodes["AcceptBlockResponse"], OK());
            } catch
            {
                Send(client, Opcodes["AcceptBlockResponse"], NOOP());
            }
        }
    }
}
