using System;
using Core.Crypto;
using Core.TCP;

namespace Core
{
    public class Program
    {
        static void Main(string[] args)
        {
            Blockchain bc = new Blockchain();
            bc.BlockAdded += new TCPServer().BlockAdded;
            Console.WriteLine($"Loaded blockchain of size {bc.Size()}");

            Console.WriteLine($"Started mining at {DateTime.UtcNow}");

            while (true)
            {
                Block b = Miner.Solve(SharpKeyPair.Create(), bc);
                bc.AddBlock(b);
            }
        }
    }
}
