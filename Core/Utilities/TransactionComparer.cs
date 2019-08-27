using System.Collections.Generic;
using Core.Transactions;

namespace Core.Utilities
{
    public class TransactionComparer : IEqualityComparer<Transaction>
    {
        public bool Equals(Transaction c1, Transaction c2)
        {
            if (c1 == null && c2 == null) { return true; }
            if (c1 == null | c2 == null) { return false; }
            if (c1.Id == c2.Id) { return true; }
            return false;
        }
        public int GetHashCode(Transaction t)
        {
            return t.Id.GetHashCode();
        }
    }
}

