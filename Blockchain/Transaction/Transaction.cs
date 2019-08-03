using System;
using Blockchain.Transactions;
using System.Linq;
using Blockchain.Utilities;
using HashUtil = Blockchain.Utilities.Hash;
using RandomUtil = Blockchain.Utilities.Random;
using Newtonsoft.Json;

namespace Blockchain
{
    public class Transaction
    {
        public enum TransactionType
        {
            DEFAULT, FEE, REWARD
        }

        public readonly string Id;
        public TransactionType Type = TransactionType.DEFAULT;
        public SharpKeyPair.Signature Signature;

        public readonly Input[] Inputs = new Input[0];
        public readonly Output[] Outputs = new Output[0];

        private readonly string InputsConcatenated;
        private readonly string OutputsConcatenated;

        [JsonConstructor]
        public Transaction(Input[] Inputs, Output[] Outputs, string Id = null)
        {
            this.Id = Id ?? HashUtil.Sha1(RandomUtil.Bytes());
            this.Inputs = Inputs;
            this.Outputs = Outputs;
            InputsConcatenated = Inputs.Map(In => In.ToString()).Reduce(R.Concat, "");
            OutputsConcatenated = Outputs.Map(Out => Out.ToString()).Reduce(R.Concat, "");
        }

        public Transaction(Output[] Outputs, string Id = null)
        {
            this.Id = Id ?? HashUtil.Sha1(RandomUtil.Bytes());
            this.Outputs = Outputs;
            Type = TransactionType.REWARD;
            InputsConcatenated = Inputs.Map(In => In.ToString()).Reduce(R.Concat, "");
            OutputsConcatenated = Outputs.Map(Out => Out.ToString()).Reduce(R.Concat, "");
        }

        public Output GetOutputByIndex(int Index)
        {
            return Outputs[Index];
        }

        // Determines if all input and output transaction equate
        public bool Equates()
        {
            return Balance() == 0;
        }

        public long Balance()
        {
            return Inputs.Map(Tx => Tx.Amount).Reduce(R.Total, 0) - Outputs.Map(Tx => Tx.Amount).Reduce(R.Total, 0);
        }

        public bool Verify()
        {
            return Signature.Verify(HashUtil.Sha1(ToString())) && Inputs.All(In => In.Verify());
        }

        public void Sign(SharpKeyPair Skp)
        {
            Signature = Skp.Sign(HashUtil.Sha1(ToString()));
        }

        public bool ContainsInput(string Transaction, int Index)
        {
            return Inputs.Filter((Input Input) => Input.Transaction == Transaction && Input.Index == Index).ToArray().Length > 0;
        }

        public bool IsRewardTransaction(long Equates)
        {
            return Type == TransactionType.REWARD && Inputs.Length == 0 && Outputs.Length == 1 && Outputs[0].Amount == Equates;
        }

        public override string ToString()
        {
            return $"{Id}{InputsConcatenated}{OutputsConcatenated}";
        }
    }
}
