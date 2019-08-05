using System;
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
        private TcpListener _server;

        public event EventHandler CommandStartMining;

        public TCPServer(int port)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
        }

        public void AwaitConnections()
        {
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                // wait for client connection
                TcpClient newClient = _server.AcceptTcpClient();
                Console.WriteLine("Connected!");

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

            // Buffer for reading data
            byte[] bytes = new byte[256];

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i;

            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                string data = Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received: {0}", data);

                // Process the data sent by the client.
                data = data.ToUpper();

                byte[] msg = Encoding.ASCII.GetBytes(data);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", data);
            }

            // Shutdown and end connection
            Console.WriteLine("Closing connection...");
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
