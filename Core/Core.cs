﻿using System;
using System.Threading;
using Core.Crypto;
using Core.TCP;

namespace Core
{
    public class Core
    {
        private readonly Blockchain bc;
        private readonly TCPServer tcp;

        public Core()
        {
            Console.WriteLine("sharpcoin v0.1 -- core by aal89");
            // Load blockchain
            //Console.Write($"Loading blockchain...");
            //bc = new Blockchain();
            //Console.WriteLine($"Done. Size is {bc.Size()}.");
            // Setup tcp server
            Console.Write($"Setting up TCP server...");
            tcp = new TCPServer(Config.TcpPort);
            //bc.BlockAdded += tcp.BlockAdded;
            //bc.QueuedTransactionAdded += tcp.BlockAdded;

            new Thread(new ThreadStart(tcp.AwaitConnections)).Start();
            
            Console.WriteLine("Done.");

            Console.WriteLine($"Ready and awaiting connections on 0.0.0.0:{Config.TcpPort}");
        }

        public void Mine()
        {
            Console.WriteLine($"Started mining at {DateTime.UtcNow}");

            while (true)
            {
                Block b = Miner.Solve(SharpKeyPair.Create(), bc);
                bc.AddBlock(b);
            }
        }

        static void Main() => new Core();
    }
}
