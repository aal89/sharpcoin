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

        public override void SendBlock(TcpClient client, byte[] data)
        {
            try
            {
                if (BitConverter.IsLittleEndian)
                    data = data.Reverse().ToArray();

                int index = BitConverter.ToInt32(data, 0);
                byte[] compressedBlock = serializer.Serialize(core.bc.GetBlockByIndex(index) ?? core.bc.GetBlockByIndex(0));

                Console.WriteLine($"[CoreServer] Sending block {index} to {client.Client.RemoteEndPoint.ToString()}.");

                Send(client, Opcodes["SendBlockResponse"], compressedBlock);
            } catch
            {
                // Return 'empty' response in case of all errors.
                Send(client, Opcodes["SendBlockResponse"]);
            }
        }
    }
}
