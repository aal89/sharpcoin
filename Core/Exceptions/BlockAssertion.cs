using System;

namespace Core.Exceptions
{
    public class BlockAssertion: Exception
    {
        public Block Block;

        public BlockAssertion(Block block, string message) : base(message)
        {
            Block = block;
        }
    }
}
