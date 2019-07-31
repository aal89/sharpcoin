using System;
using System.Collections.Generic;
using Blockchain.Utilities;
using System.Linq;

namespace Blockchain
{
    [Serializable]
    public class Block
    {
        public int Index = 0;
        public string PreviousHash = "";
        public string Hash = "";
        public DateTime Timestamp = DateTime.UtcNow;
        public uint Nonce = 0;
        public List<Transaction> Transactions = new List<Transaction>();

        public Block()
        {
            Hash = ToHash();
        }

        public ulong GetDifficulty()
        {
            return Convert.ToUInt64(Hash.Substring(0, 16), 16);
        }

        public bool HasTransactions()
        {
            return Transactions.Count > 0;
        }

        public bool HasRewardTransaction()
        {
            return Transactions.Filter(Tx => Tx.Type == Transaction.TransactionType.REWARD).Count() == 1;
        }

        public Transaction GetRewardTransaction()
        {
            return Transactions.Find(Tx => Tx.Type == Transaction.TransactionType.REWARD);
        }

        public string ToHash()
        {
            string StringifiedTransactions = Transactions.Map(Tx => Tx.ToHash()).Reduce(R.Concat, "");
            return Utilities.Hash.Sha256($"{Index}{PreviousHash}{Timestamp.ToString()}{Nonce}{StringifiedTransactions}");
        }
    }
}
