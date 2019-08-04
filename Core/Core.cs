using System;
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
            // Load blockchain
            Console.WriteLine($"Loading blockchain...");
            bc = new Blockchain();
            Console.WriteLine($"Loaded blockchain of size {bc.Size()}.");
            // Setup tcp server
            Console.WriteLine($"Setting up TCP server...");
            tcp = new TCPServer();
            bc.BlockAdded += tcp.BlockAdded;
            bc.QueuedTransactionAdded += tcp.BlockAdded;
            Console.WriteLine("Done.");
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
