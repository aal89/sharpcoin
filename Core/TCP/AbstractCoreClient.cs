using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Core.TCP
{
    public abstract class AbstractCoreClient : TcpClient
    {
        protected readonly Core core;
        protected readonly Dictionary<string, byte> Opcodes = Operation.Codes;

        protected readonly int TLVHeaderSize = 4;

        protected AbstractCoreClient(Core core, string server) : base(server, Config.TcpPort)
        {
            this.core = core;
        }

        protected void Send(byte type, string data)
        {
            Send(type, Encoding.UTF8.GetBytes(data));
        }

        protected void Send(byte type, byte[] data)
        {
            byte[] tlvdata = new byte[data.Length + TLVHeaderSize];
            tlvdata[0] = type;
            tlvdata[1] = (byte)(data.Length >> 16 & 0xff);
            tlvdata[2] = (byte)(data.Length >> 8 & 0xff);
            tlvdata[3] = (byte)(data.Length >> 0 & 0xff);

            Array.Copy(data, 0, tlvdata, TLVHeaderSize, data.Length);

            // fix this lock this
            lock (this)
            {
                GetStream().Write(tlvdata);
            }
        }

        public abstract void RequestBlock(int index);
        public abstract void AcceptBlock(TcpClient client, byte[] data);
    }
}
