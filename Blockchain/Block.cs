using System;
using System.Linq;
using Blockchain.Utilities;

namespace Blockchain
{
    [Serializable]
    public class Block
    {
        public int Index = 0;
        public string PreviousHash = "aaaaaaaaafb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";
        public string Hash = "000000000fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";
        public DateTime Timestamp = new DateTime();
        public int Nonce = 0;
        public Transaction[] Transactions = new Transaction[0];

        public Block()
        {
            Transactions = new Transaction[] { new Transaction() };
        }

        public ulong GetDifficulty()
        {
            return Convert.ToUInt64(Hash.Substring(0, 16), 16);
        }

        public bool HasTransactions()
        {
            return Transactions.Length > 0;
        }

        public bool GotFeeRewardTransactions()
        {
            return Transactions
                .Filter((Transaction Transaction) => Transaction.Type == Transaction.TransactionType.FEE || Transaction.Type == Transaction.TransactionType.REWARD)
                .ToArray()
                .Length == 2;
        }

        public string ToHash()
        {
            string StringifiedTransactions = Transactions.Map((Transaction Transaction) => Transaction.ToString()).Reduce(R.Concat, "");
            return Utilities.Hash.Sha256($"{Index}{PreviousHash}{Timestamp.ToString()}{Nonce}{StringifiedTransactions}");
        }
    }
}
