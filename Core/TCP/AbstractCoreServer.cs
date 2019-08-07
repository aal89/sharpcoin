﻿using System.Collections.Generic;
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
            { "RequestBlock", 0x01 },
            { "RequestBlockResponse", 0x02 },
            { "AcceptBlock", 0x03 },
            { "AcceptBlockResponse", 0x04 },
        };

        protected AbstractCoreServer(Core core) : base(Config.TcpPort)
        {
            this.core = core;
        }

        public override void Incoming(byte type, byte[] data, TcpClient client)
        {
            switch (type)
            {
                case 0x01: RequestBlock(client, data); break;
                case 0x02: break;
                case 0x03: AcceptBlock(client, data); break;
                case 0x04: break;
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

        public abstract void RequestBlock(TcpClient client, byte[] data);
        public abstract void AcceptBlock(TcpClient client, byte[] data);
    }
}
