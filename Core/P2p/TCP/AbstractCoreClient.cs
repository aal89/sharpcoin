using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Core.TCP
{
    public abstract class AbstractCoreClient : TCPClient
    {
        public string Ip;
        protected readonly Core core;
        protected readonly Dictionary<string, byte> Opcodes = Operation.Codes;

        protected AbstractCoreClient(Core core, string server) : base(server, Config.TcpPort)
        {
            Ip = server;
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
                case 0x07: break;
                case 0x08: AcceptPeersResponse(data); break;
                case 0x09: break;
                case 0x0a: RequestTransactionsResponse(data); break;
                case 0x0b: break;
                case 0x0c: AcceptTransactionsResponse(data); break;
            }
        }

        // Request = to request some octects at a remote.
        // Accept = to let a remote accept some octets.
        // Each type has a response.
        public abstract void RequestBlock(int index);
        protected abstract void RequestBlockResponse(byte[] data);

        public abstract void AcceptBlock(Block block);
        protected abstract void AcceptBlockResponse(byte[] data);

        public abstract void RequestPeers();
        protected abstract void RequestPeersResponse(byte[] data);

        public abstract void AcceptPeers(string peers);
        protected abstract void AcceptPeersResponse(byte[] data);

        public abstract void RequestTransactions(string txs);
        protected abstract void RequestTransactionsResponse(byte[] data);

        public abstract void AcceptTransactions(string txs);
        protected abstract void AcceptTransactionsResponse(byte[] data);
    }
}
