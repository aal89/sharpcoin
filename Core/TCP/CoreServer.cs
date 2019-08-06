using System;
using System.Net.Sockets;
using Core.Crypto;

namespace Core.TCP
{
    public class CoreServer : AbstractCoreServer
    {
        public CoreServer(Core c) : base(c) { }

        public override void CreateKeyPair(TcpClient client)
        {
            SharpKeyPair skp = SharpKeyPair.Create();
            byte[] rawkeypair = new byte[skp.PublicKey.Length + skp.PrivateKey.Length];

            Array.Copy(skp.PublicKey, 0, rawkeypair, 0, skp.PublicKey.Length);
            Array.Copy(skp.PrivateKey, 0, rawkeypair, skp.PublicKey.Length, skp.PrivateKey.Length);

            Send(client, Opcodes["CreateKeyPairResponse"], rawkeypair);
        }
    }
}
