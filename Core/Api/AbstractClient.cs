using System.Collections.Generic;
using System.Net.Sockets;
using Core.Tcp;

namespace Core.Api
{
    public abstract class AbstractClient : ConnectionHandler
    {
        protected readonly Core core;
        protected readonly Dictionary<string, byte> Opcodes = new ApiOperations().Codes;

        protected AbstractClient(Core core, TcpClient client) : base(client)
        {
            this.core = core;
        }

        public override void Incoming(byte type, byte[] data)
        {
            switch (type)
            {
                case 0x01: RequestMining(data); break;
                case 0x03: RequestKeyPair(data); break;
                case 0x05: RequestBalance(data); break;
            }
        }

        public abstract void RequestMining(byte[] data);
        public abstract void RequestKeyPair(byte[] data);
        public abstract void RequestBalance(byte[] data);
    }
}
