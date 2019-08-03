using System;

namespace Blockchain.TCP
{
    public class TCPServer
    {
        public void BlockAdded(object sender, EventArgs e)
        {
            Console.WriteLine($"Block {(sender as Blockchain).GetLastBlock().Index} got added to the chain.");
        }
    }
}
