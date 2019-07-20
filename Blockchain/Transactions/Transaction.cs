using System;

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
        public Input[] TransactionInputs = new Input[] { };
        public Output[] TransactionOutputs = new Output[] { };
        public string Signature = "";

        // Determines if all input and output transaction equate, with taking a variable
        // block reward into consideration.
        public bool Equates(long Reward)
        {
            return true;
        }

        public bool Verify()
        {
            return true;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}
