using System.Collections.Generic;
using System.Net.Sockets;
using Core.Utilities;

namespace Core.Tcp
{
    public abstract class AbstractClient : ConnectionHandler
    {
        private readonly Operations Operations;
        protected readonly Dictionary<string, byte> Opcodes;
        protected readonly Serializer Serializer = new Serializer();

        protected AbstractClient(Operations Operations, TcpClient client) : base(client)
        {
            this.Operations = Operations;
            Opcodes = Operations.Codes();
        }

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
