using System;
using Blockchain.Transactions;
using System.Linq;
using Blockchain.Utilities;
using HashUtil = Blockchain.Utilities.Hash;
using RandomUtil = Blockchain.Utilities.Random;

namespace Blockchain
{
    [Serializable]
    public class Transaction
    {
        public enum TransactionType
        {
            DEFAULT, FEE, REWARD
        }

        public readonly string Id;
        public TransactionType Type = TransactionType.DEFAULT;
        public Input[] TransactionInputs = { };
        public Output[] TransactionOutputs = { };
        public SharpKeyPair.Signature Signature;

        public Transaction()
        {
            Id = HashUtil.Sha256(RandomUtil.Bytes());
        }

        // Determines if all input and output transaction equate, with taking a variable
        // block reward into consideration.
        public bool Equates(ulong Reward)
        {
            ulong TotalInputValue = TransactionInputs.Map(Tx => Tx.Amount).Reduce<ulong>(R.Total, 0);
            // Output also contains a reward tx
            ulong TotalOutputValue = TransactionOutputs.Map(Tx => Tx.Amount).Reduce<ulong>(R.Total, 0);
            return TotalInputValue - TotalOutputValue == Reward;
        }

        public bool Verify()
        {
            return Signature.Verify(ToHash());
        }

        public void Sign(SharpKeyPair Skp)
        {
            Signature = Skp.Sign(ToHash());
        }

        public string ToHash()
        {
            string InputsConcatenated = TransactionInputs.Map(Tx => Tx.ToHash()).Reduce(R.Concat, "");
            string OutputsConcatenated = TransactionOutputs.Map(Tx => Tx.ToHash()).Reduce(R.Concat, "");
            return HashUtil.Sha256($"{Id}{InputsConcatenated}{OutputsConcatenated}");
        }

        public bool ContainsInput(string Transaction, int Index)
        {
            return TransactionInputs.Filter((Input Input) => Input.Transaction == Transaction && Input.Index == Index).ToArray().Length > 0;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}
