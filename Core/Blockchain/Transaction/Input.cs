using Core.Utilities;
using Core.Crypto;
using System;

namespace Core.Transactions
{
    public class Input : IEquatable<Input>
    {
        public string Transaction = "";
        public int Index;
        public long Amount;
        public string Address = "";
        public SharpKeyPair.Signature Signature;

        public bool Verify()
        {
            return Signature.Verify(Hash.Sha1(ToString())) && new SharpKeyPair(Signature.PublicKey).GetAddress() == Address;
        }

        public void Sign(SharpKeyPair Skp)
        {
            Signature = Skp.Sign(Hash.Sha1(ToString()));
        }

        public Output AsOutput()
        {
            return new Output
            {
                Amount = Amount,
                Address = Address
            };
        }

        public MetaOutput AsMetaOutput()
        {
            return new MetaOutput
            {
                Transaction = Transaction,
                Address = Address,
                Amount = Amount,
                Index = Index
            };
        }

        public override string ToString()
        {
            return $"{Transaction}{Index}{Amount}{Address}";
        }

        public bool Equals(Input other)
        {
            return other != null
                && other.Transaction == Transaction
                && other.Index == Index
                && other.Amount == Amount;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
