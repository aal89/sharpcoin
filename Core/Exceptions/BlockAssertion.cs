using System;

namespace Core.Exceptions
{
    public class BlockAssertion: Exception
    {
        public BlockAssertion(string message) : base(message) { }
    }
}
