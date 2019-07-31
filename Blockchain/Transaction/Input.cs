using System;
using Blockchain.Utilities;

namespace Blockchain.Transactions
{
    [Serializable]
    public class Input
    {
        public string Transaction = "";
        public int Index;
        public ulong Amount;
        public string Address = "";
        public SharpKeyPair.Signature Signature;

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
            return Hash.Sha1($"{Transaction}{Index}{Amount}{Address}");
        }
    }
}
