using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Core.Tcp;

namespace Core.Api
{
    public abstract class AbstractClient : ConnectionHandler
    {
        protected readonly Core core;
        protected readonly Dictionary<string, byte> Opcodes = new ApiOperations().Codes;

        public AbstractClient(Core core, TcpClient client) : base(client)
        {
            this.core = core;
        }

        public override void Incoming(byte type, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
