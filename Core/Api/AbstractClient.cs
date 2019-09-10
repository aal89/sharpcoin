using System.Collections.Generic;
using System.Net.Sockets;
using Core.Tcp;

namespace Core.Api
{
    public abstract class AbstractClient : ConnectionHandler
    {
        private readonly ApiOperations Operations = new ApiOperations();

        protected readonly Core core;
        protected readonly Dictionary<string, byte> Opcodes;

        protected AbstractClient(Core core, TcpClient client) : base(client)
        {
            this.core = core;
            Opcodes = Operations.Codes;
        }

        public override void Incoming(byte type, byte[] data)
        {
            switch (type)
            {
                case 0x01: RequestMining(data); break;
                case 0x03: RequestKeyPair(data); break;
                case 0x05: RequestBalance(data); break;
                case 0x07: CreateTransaction(data); break;
            }
        }

        public abstract void RequestMining(byte[] data);
        public abstract void RequestKeyPair(byte[] data);
        public abstract void RequestBalance(byte[] data);
        public abstract void CreateTransaction(byte[] data);

        // delegates
        public byte[] OK()
        {
            return Operations.OK();
        }

        public byte[] NOOP()
        {
            return Operations.NOOP();
        }

        public bool IsOK(byte[] data)
        {
            return Operations.IsOK(data);
        }

        public bool IsNOOP(byte[] data)
        {
            return Operations.IsNOOP(data);
        }
    }
}
