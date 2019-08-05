using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core.Transactions;

namespace Core.TCP
{
    public class TCPServer
    {
        private readonly TcpListener server;
        private readonly List<TcpClient> clients = new List<TcpClient>();

        public event EventHandler CommandStartMining;

        public TCPServer(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
        }

        public void AwaitConnections()
        {
            while (true)
            {
                // wait for client connection
                TcpClient newClient = server.AcceptTcpClient();
                clients.Add(newClient);

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

            Console.WriteLine($"Peer {client.Client.RemoteEndPoint.ToString()} connected.");

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            // Simple TLV protocol
            int type = stream.ReadByte();
            int length = stream.ReadByte() + stream.ReadByte() + stream.ReadByte();

            // Buffer for reading data
            byte[] bytes = new byte[length];

            int i;

            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string data = Encoding.UTF8.GetString(bytes, 0, i);
                Console.WriteLine("Received: {0}", data);

                data = data.ToUpper();

                byte[] msg = Encoding.UTF8.GetBytes(data);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", data);
            }

            // Shutdown and end connection
            clients.Remove(client);
            Console.WriteLine($"Peer {client.Client.RemoteEndPoint.ToString()} closing connection...");
            client.Close();
        }

        public void BlockAdded(object sender, EventArgs e)
        {
            Console.WriteLine($"Block {(sender as Block).Index} got added to the chain.");
        }

        public void QueuedTransactionAdded(object sender, EventArgs e)
        {
            Console.WriteLine($"New transaction got queued {(sender as Transaction).Id}.");
        }
    }
}
