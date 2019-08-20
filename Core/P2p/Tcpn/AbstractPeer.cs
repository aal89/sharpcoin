using System.Collections.Generic;
using System.Net.Sockets;
using Core.Tcpn;
using Core.Transactions;

namespace Core.P2p.Tcpn
{
    public abstract class AbstractPeer: ConnectionHandler
    {
        protected readonly Core core;
        protected readonly Dictionary<string, byte> Opcodes = Operation.Codes;

        protected AbstractPeer(Core core, TcpClient client) : base(client, core.Log)
        {
            this.core = core;
        }

        public override void Incoming(byte type, byte[] data)
        {
            switch (type)
            {
                case 0x01: ServeRequestBlock(data); break;
                case 0x02: RequestBlockResponse(data); break;
                case 0x03: ServeAcceptBlock(data); break;
                case 0x04: AcceptBlockResponse(data); break;
                case 0x05: ServeRequestPeers(data); break;
                case 0x06: RequestPeersResponse(data); break;
                case 0x07: ServeAcceptPeers(data); break;
                case 0x08: AcceptPeersResponse(data); break;
                case 0x09: ServeRequestTransaction(data); break;
                case 0x0a: RequestTransactionResponse(data); break;
                case 0x0b: ServeAcceptTransaction(data); break;
                case 0x0c: AcceptTransactionResponse(data); break;
            }
        }

        // Request = to request some octects at a remote.
        // Serve = to construct a response to some request.
        // Reponse = the response constructed by another remote.
        // Accept = to let a remote accept some octets.
        // Each type has a response.
        public abstract void RequestBlock(int index);
        protected abstract void ServeRequestBlock(byte[] data);
        protected abstract void RequestBlockResponse(byte[] data);

        public abstract void AcceptBlock(Block block);
        protected abstract void ServeAcceptBlock(byte[] data);
        protected abstract void AcceptBlockResponse(byte[] data);

        public abstract void RequestPeers();
        protected abstract void ServeRequestPeers(byte[] data);
        protected abstract void RequestPeersResponse(byte[] data);

        public abstract void AcceptPeers(string peers);
        protected abstract void ServeAcceptPeers(byte[] data);
        protected abstract void AcceptPeersResponse(byte[] data);

        public abstract void RequestTransaction(string id);
        protected abstract void ServeRequestTransaction(byte[] data);
        protected abstract void RequestTransactionResponse(byte[] data);

        public abstract void AcceptTransaction(Transaction tx);
        protected abstract void ServeAcceptTransaction(byte[] data);
        protected abstract void AcceptTransactionResponse(byte[] data);
    }
}
