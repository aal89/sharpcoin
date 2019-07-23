using System;
using Xunit;
using Blockchain;

namespace BlockchainTests
{
    public class BlockchainTests
    {
        [Fact]
        public void GenesisBlock_Verify_Test()
        {
            GenesisBlock gblock = new GenesisBlock();

            Assert.NotEqual(UInt64.Parse("0"), gblock.GetDifficulty());
            Assert.False(gblock.HasTransactions());
            Assert.False(gblock.GotFeeRewardTransactions());
            Assert.Equal(gblock.Hash, gblock.ToHash());
        }
    }
}
