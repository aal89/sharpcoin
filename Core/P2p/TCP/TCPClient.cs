﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Core.TCP
{
    public abstract class TCPClient: TcpClient
    {
        protected readonly int TLVHeaderSize = 4;

        protected TCPClient(string server, int port) : base(server, port)
        {
            new Thread(new ThreadStart(AwaitCommunication)).Start();
        }

        public void AwaitCommunication()
        {
            // Get a stream object for reading and writing
            NetworkStream stream = GetStream();

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
                Incoming(type, data);
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

            GetStream().Write(tlvdata);
        }

        public abstract void Incoming(byte type, byte[] data);
    }
}