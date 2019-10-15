using System;

namespace Core.Transactions
{
    public class MetaOutput : Output, IEquatable<MetaOutput>
    {
        public string Transaction;
        public int Index;

        public bool Equals(MetaOutput other)
        {
            return other != null
                && other.Transaction == Transaction
                && other.Index == Index
                && other.Amount == Amount
                && other.Address == Address;
        }
    }
}
