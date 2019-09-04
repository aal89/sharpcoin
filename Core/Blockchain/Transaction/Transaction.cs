using System.Linq;
using Core.Utilities;
using Newtonsoft.Json;
using Core.Crypto;

namespace Core.Transactions
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
            this.Id = Id ?? Hash.Sha1(Random.Bytes());
            this.Inputs = Inputs;
            this.Outputs = Outputs;
            InputsConcatenated = Inputs.Stringified();
            OutputsConcatenated = Outputs.Stringified();
        }

        public Transaction(Output[] Outputs, string Id = null)
        {
            this.Id = Id ?? Hash.Sha1(Random.Bytes());
            this.Outputs = Outputs;
            Type = TransactionType.REWARD;
            InputsConcatenated = Inputs.Stringified();
            OutputsConcatenated = Outputs.Stringified();
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
            return Signature.Verify(Hash.Sha1(ToString())) && Inputs.All(In => In.Verify());
        }

        public void Sign(SharpKeyPair Skp)
        {
            Signature = Skp.Sign(Hash.Sha1(ToString()));
        }

        public bool ContainsInput(Input input)
        {
            return Inputs.Any(Input => Input.Transaction == input.Transaction && Input.Index == input.Index);
        }

        public bool IsRewardTransaction(long Equates)
        {
            return Type == TransactionType.REWARD && Inputs.Length == 0 && Outputs.Length == 1 && Outputs[0].Amount == Equates;
        }

        public bool IsDefaultTransaction()
        {
            return Type == TransactionType.DEFAULT;
        }

        public override string ToString()
        {
            return $"{Id}{InputsConcatenated}{OutputsConcatenated}";
        }
    }
}
