using Core.Utilities;
using Core.Crypto;

namespace Core.Transactions
{
    public class Input
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

        public override string ToString()
        {
            return $"{Transaction}{Index}{Amount}{Address}";
        }
    }
}
