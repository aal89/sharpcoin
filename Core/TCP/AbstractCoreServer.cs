using System.Collections.Generic;
using System.Net.Sockets;

namespace Core.TCP
{
    public abstract class AbstractCoreServer : TCPServer
    {
        protected readonly Core core;

        protected readonly Dictionary<string, byte> Opcodes = new Dictionary<string, byte>()
        {
            { "Noop", 0x99 },
            { "CreateKeyPair", 0x00 },
            { "CreateKeyPairResponse", 0x01 },
            { "SendBlock", 0x02 },
            { "SendBlockResponse", 0x03 }
        };

        protected AbstractCoreServer(Core core) : base(Config.TcpPort)
        {
            this.core = core;
        }

        public override void Incoming(byte type, byte[] data, TcpClient client)
        {
            switch (type)
            {
                case 0x00: CreateKeyPair(client); break;
                case 0x01: break;
                case 0x02: SendBlock(client, data); break;
                case 0x03: break;
            }
        }

        public abstract void CreateKeyPair(TcpClient client);
        public abstract void SendBlock(TcpClient client, byte[] data);
    }
}
