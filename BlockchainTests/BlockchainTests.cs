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

            Assert.Equal(ulong.Parse("3654655253775102"), gblock.GetDifficulty());
            Assert.False(gblock.HasTransactions());
            Assert.False(gblock.GotFeeRewardTransactions());
            Assert.Equal(gblock.Hash, gblock.ToHash());
        }
    }
}
