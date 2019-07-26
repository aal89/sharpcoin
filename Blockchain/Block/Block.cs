using System;
using System.Collections.Generic;
using System.Linq;
using Blockchain.Utilities;

namespace Blockchain
{
    [Serializable]
    public class Block
    {
        public int Index = 0;
        public string PreviousHash = "";
        public string Hash = "";
        public DateTime Timestamp = new DateTime();
        public int Nonce = 0;
        public List<Transaction> Transactions = new List<Transaction>();

        public ulong GetDifficulty()
        {
            return Convert.ToUInt64(Hash.Substring(0, 16), 16);
        }

        public bool HasTransactions()
        {
            return Transactions.ToArray().Length > 0;
        }

        public bool GotRewardTransaction()
        {
            return Transactions.Find(Tx => Tx.Type == Transaction.TransactionType.REWARD) != null;
        }

        public string ToHash()
        {
            string StringifiedTransactions = Transactions.Map(Tx => Tx.ToString()).Reduce(R.Concat, "");
            return Utilities.Hash.Sha256($"{Index}{PreviousHash}{Timestamp.ToString()}{Nonce}{StringifiedTransactions}");
        }
    }
}
