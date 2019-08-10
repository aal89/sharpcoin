using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Core.TCP
{
    public abstract class AbstractCoreClient : TCPClient
    {
        protected readonly Core core;
        protected readonly Dictionary<string, byte> Opcodes = Operation.Codes;

        protected AbstractCoreClient(Core core, string server) : base(server, Config.TcpPort)
        {
            this.core = core;
        }

        public override void Incoming(byte type, byte[] data)
        {
            switch (type)
            {
                case 0x01: break;
                case 0x02: RequestBlockResponse(data); break;
                case 0x03: break;
                case 0x04: AcceptBlockResponse(data); break;
                case 0x05: break;
                case 0x06: RequestPeersResponse(data); break;
            }
        }

        public abstract void RequestBlock(int index);
        protected abstract void RequestBlockResponse(byte[] data);
        public abstract void AcceptBlock(Block block);
        protected abstract void AcceptBlockResponse(byte[] data);
        public abstract void RequestPeers();
        protected abstract void RequestPeersResponse(byte[] data);
    }
}
