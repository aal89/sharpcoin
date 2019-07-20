using System;
namespace Blockchain.Transactions
{
    public class Input
    {
        public string Transaction = "";
        public int Index = 0;
        public ulong Amount = 0;
        public string Address = "";
        public string Signature = "";

        public void Sign(string Key)
        {
            Signature = "SIGNED";
        }

        public bool Verify()
        {
            return true;
        }
    }
}
