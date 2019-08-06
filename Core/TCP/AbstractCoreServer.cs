using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Core.TCP
{
    public abstract class AbstractCoreServer : TCPServer
    {
        private readonly Core c;

        protected readonly Dictionary<string, byte> Opcodes = new Dictionary<string, byte>()
        {
            { "CreateKeyPair", 0x00 },
            { "CreateKeyPairResponse", 0x01 }
        };

        protected AbstractCoreServer(Core c) : base(Config.TcpPort)
        {
            this.c = c;
        }

        public override void Incoming(byte type, byte[] data, TcpClient client)
        {
            switch (type)
            {
                case 0x00: CreateKeyPair(client); break;
                case 0x01: break;
            }
        }

        public abstract void CreateKeyPair(TcpClient client);
    }
}
