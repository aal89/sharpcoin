using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core.Utilities;

namespace Core.TCP
{
    public abstract class TCPServer
    {
        private readonly TcpListener server;
        private readonly List<TcpClient> clients = new List<TcpClient>();
        private readonly ILoggable log;

        protected readonly int TLVHeaderSize = 4;

        protected TCPServer(int port, ILoggable log = null)
        {
            this.log = log ?? new NullLogger();
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            new Thread(new ThreadStart(AwaitConnections)).Start();
        }

        public void AwaitConnections()
        {
            while (true)
            {
                // wait for client connection
                TcpClient newClient = server.AcceptTcpClient();
                lock(clients)
                {
                    clients.Add(newClient);
                }
                
                // client found.
                // create a thread to handle communication
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }

        public void HandleClient(object obj)
        {
            // retrieve client from parameter passed to thread
            TcpClient client = (TcpClient)obj;

            log.NewLine($"Remote {client.Client.RemoteEndPoint.ToString()} connected.");

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            // Buffer for reading the header of the tlv protocol
            byte[] bytes = new byte[4];

            // Simple TLV protocol where first byte is type and the following 3 are for length
            // so read 4 bytes and then in the while loop build data byte array
            while ((_ = stream.Read(bytes, 0, TLVHeaderSize)) != 0)
            {
                byte type = bytes[0];
                int length = bytes[1] << 16 | bytes[2] << 8 | bytes[3];
                byte[] data = new byte[length];
                stream.Read(data, 0, data.Length);
                Incoming(type, data, client);
            }

            // Shutdown and end connection
            lock(clients)
            {
                clients.Remove(client);
            }
            log.NewLine($"Remote {client.Client.RemoteEndPoint.ToString()} closing connection...");
            client.Close();
        }

        protected void Send(TcpClient client, byte type, string data)
        {
            Send(client, type, Encoding.UTF8.GetBytes(data));
        }

        protected void Send(TcpClient client, byte type, byte[] data)
        {
            byte[] tlvdata = new byte[data.Length + TLVHeaderSize];
            tlvdata[0] = type++;
            tlvdata[1] = (byte)(data.Length >> 16 & 0xff);
            tlvdata[2] = (byte)(data.Length >> 8 & 0xff);
            tlvdata[3] = (byte)(data.Length >> 0 & 0xff);

            Array.Copy(data, 0, tlvdata, TLVHeaderSize, data.Length);

            lock (client)
            {
                client.GetStream().Write(tlvdata);
            }
        }

        public abstract void Incoming(byte type, byte[] data, TcpClient client);

    }
}
