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
        private bool _isRunning;

        public event EventHandler CommandStartMining;

        public TCPServer(int port)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();

            _isRunning = true;
        }

        public void AwaitConnections()
        {
            while (_isRunning)
            {
                // wait for client connection
                TcpClient newClient = _server.AcceptTcpClient();

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

            // sets two streams
            //StreamWriter sWriter = new StreamWriter(client.GetStream(), Encoding.UTF8);
            StreamReader sReader = new StreamReader(client.GetStream(), Encoding.UTF8);
            // you could use the NetworkStream to read and write, 
            // but there is no forcing flush, even when requested

            bool bClientConnected = true;
            String sData = null;

            while (bClientConnected)
            {
                // reads from stream
                sData = sReader.ReadLine();

                // shows content on the console.
                Console.WriteLine("Client > " + sData);

                // to write something back.
                // sWriter.WriteLine("Meaningfull things here");
                // sWriter.Flush();
            }
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
