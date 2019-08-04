using System;
using Core.Transactions;

namespace Core.TCP
{
    public class TCPServer
    {
        public event EventHandler CommandStartMining;

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
