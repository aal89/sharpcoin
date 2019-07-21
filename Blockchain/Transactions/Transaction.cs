using System;
using Blockchain.Transactions;
using System.Linq;

namespace Blockchain
{
    [Serializable]
    public class Transaction
    {
        public enum TransactionType
        {
            DEFAULT, FEE, REWARD
        }

        public string Id = "000000000fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";
        public TransactionType Type = TransactionType.DEFAULT;
        public Input[] TransactionInputs = { };
        public Output[] TransactionOutputs = { };
        public string Signature = "";

        // Determines if all input and output transaction equate, with taking a variable
        // block reward into consideration.
        public bool Equates(ulong Reward)
        {
            return true;
        }

        public bool Verify()
        {
            return true;
        }

        public bool HasInputTransaction(string Transaction, int Index)
        {
            return TransactionInputs.Filter((Input Input) => Input.Transaction == Transaction && Input.Index == Index).ToArray().Length > 0;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}
