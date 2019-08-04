using System;
using Core.Transactions;

namespace Core.Exceptions
{
    public class BuilderException : Exception
    {
        public Transaction tx;

        public BuilderException(Transaction tx, string message) : base(message)
        {
            this.tx = tx;
        }
    }
}
