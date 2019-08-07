using System;
using System.Net.Sockets;
using System.Linq;
using Core.Crypto;
using Core.Utilities;

namespace Core.TCP
{
    public class CoreServer : AbstractCoreServer
    {
        private readonly Serializer serializer = new Serializer();

        public CoreServer(Core core) : base(core) { }

        public override void CreateKeyPair(TcpClient client)
        {
            SharpKeyPair skp = SharpKeyPair.Create();
            byte[] rawkeypair = new byte[skp.PublicKey.Length + skp.PrivateKey.Length];

            Array.Copy(skp.PublicKey, 0, rawkeypair, 0, skp.PublicKey.Length);
            Array.Copy(skp.PrivateKey, 0, rawkeypair, skp.PublicKey.Length, skp.PrivateKey.Length);

            Send(client, Opcodes["CreateKeyPairResponse"], rawkeypair);
        }

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
