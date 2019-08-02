using System;
using Blockchain.Utilities;

namespace Blockchain.Transactions
{
    public class Input
    {
        public string Transaction = "";
        public int Index;
        public ulong Amount;
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

        public override string ToString()
        {
            return $"{Transaction}{Index}{Amount}{Address}";
        }
    }
}
