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
        public Input[] Inputs = { };
        public Output[] Outputs = { };
        public SharpKeyPair.Signature Signature;

        public Transaction(string Id = null)
        {
            this.Id = Id ?? HashUtil.Sha1(RandomUtil.Bytes());
        }

        // Determines if all input and output transaction equate
        public bool Equates()
        {
            ulong TotalInputValue = Inputs.Map(Tx => Tx.Amount).Reduce<ulong>(R.Total, 0);
            // Output also contains a reward tx
            ulong TotalOutputValue = Outputs.Map(Tx => Tx.Amount).Reduce<ulong>(R.Total, 0);
            return TotalInputValue - TotalOutputValue == 0;
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
            string InputsConcatenated = Inputs.Map(Tx => Tx.ToHash()).Reduce(R.Concat, "");
            string OutputsConcatenated = Outputs.Map(Tx => Tx.ToHash()).Reduce(R.Concat, "");
            return HashUtil.Sha256($"{Id}{InputsConcatenated}{OutputsConcatenated}");
        }

        public bool ContainsInput(string Transaction, int Index)
        {
            return Inputs.Filter((Input Input) => Input.Transaction == Transaction && Input.Index == Index).ToArray().Length > 0;
        }

        public bool IsRewardTransaction(ulong Equates)
        {
            return Type == TransactionType.REWARD && Inputs.Length == 0 && Outputs.Length == 1 && Outputs[0].Amount == Equates;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}
