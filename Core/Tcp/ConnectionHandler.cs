using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities;

namespace Core.Tcp
{
    public abstract class ConnectionHandler
    {
        public readonly string Ip;
        protected readonly int TLVHeaderSize = 4;
        protected readonly TcpClient client;

        public event EventHandler OpenenConn;
        public event EventHandler ClosedConn;

        protected ConnectionHandler(TcpClient client)
        {
            Ip = client.Ip();
            this.client = client;

            Thread t = new Thread(new ThreadStart(AwaitCommunication));
            // Small delay before starting thread so that the control flow of the constructors
            // up above in the concretions have time finishing doing their stuff. For example
            // adding event listeners to this object first...
            Task.Delay(50).ContinueWith(task => t.Start());
        }

        public void AwaitCommunication()
        {
            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();
            // Buffer for reading the header of the tlv protocol
            byte[] bytes = new byte[4];

            try
            {
                OpenenConn?.Invoke(this, EventArgs.Empty);

                // Simple TLV protocol where first byte is type and the following 3 are for length
                // so read 4 bytes and then in the while loop build data byte array
                while ((_ = stream.Read(bytes, 0, TLVHeaderSize)) != 0)
                {
                    byte type = bytes[0];
                    int length = bytes[1] << 16 | bytes[2] << 8 | bytes[3];
                    byte[] data = new byte[length];
                    stream.Read(data, 0, data.Length);
                    Incoming(type, data);
                }
            }
            finally
            {
                client.Close();
                ClosedConn?.Invoke(this, EventArgs.Empty);
            }
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

            if (client.Connected)
            {
                client.GetStream().Write(tlvdata);
            }
        }

        public abstract void Incoming(byte type, byte[] data);
    }
}
