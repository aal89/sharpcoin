using System.Collections.Generic;
using System.Net.Sockets;

namespace Core.TCP
{
    public abstract class AbstractCoreServer : TCPServer
    {
        protected readonly Core core;

        protected readonly Dictionary<string, byte> Opcodes = new Dictionary<string, byte>()
        {
            { "Ok", 0x98 },
            { "Noop", 0x99 },
            { "CreateKeyPair", 0x00 },
            { "CreateKeyPairResponse", 0x01 },
            { "RequestBlock", 0x02 },
            { "RequestBlockResponse", 0x03 },
            { "AcceptBlock", 0x04 },
            { "AcceptBlockResponse", 0x05 },
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
                case 0x02: RequestBlock(client, data); break;
                case 0x03: break;
                case 0x04: AcceptBlock(client, data); break;
                case 0x05: break;
            }
        }

        protected byte[] OK()
        {
            return new byte[] { Opcodes["Ok"] };
        }

        protected byte[] NOOP()
        {
            return new byte[] { Opcodes["Noop"] };
        }

        public abstract void CreateKeyPair(TcpClient client);
        public abstract void RequestBlock(TcpClient client, byte[] data);
        public abstract void AcceptBlock(TcpClient client, byte[] data);
    }
}
