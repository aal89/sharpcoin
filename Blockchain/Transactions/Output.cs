using System;
using Blockchain.Utilities;

namespace Blockchain.Transactions
{
    [Serializable]
    public class Output
    {
        public ulong Amount = 0;
        public string Address = "";

        public string ToHash()
        {
            return Hash.Sha256($"{Amount}{Address}");
        }
    }
}
