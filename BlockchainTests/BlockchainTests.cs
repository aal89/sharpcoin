using System;
using Xunit;
using Blockchain;
using BlockchainObject = Blockchain.Blockchain;
using Blockchain.Exceptions;
using Blockchain.Transactions;
using Blockchain.Utilities;

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
            Assert.False(gblock.HasRewardTransaction());
            Assert.Equal(gblock.Hash, gblock.ToHash());
        }

        [Fact]
        public void Block_IsValidBlock_Test()
        {
            BlockchainObject bc = new BlockchainObject();

            Block nextblock = new Block();
            // This one is here to fake previous hash because the genesis block
            // does not have a hash and defaults out to "".
            nextblock.PreviousHash = "PREVHASH";

            Exception ex1 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock, bc.GetLastBlock()));
            Assert.Equal("Not consecutive blocks. Expected new block index to be 1, but got 0.", ex1.Message);

            nextblock.Index = 1;

            Exception ex2 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock, bc.GetLastBlock()));
            Assert.Equal("New block points to a different block. Previous hash of new block is PREVHASH, while hash of last block is 000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962.", ex2.Message);

            nextblock.PreviousHash = "000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962";

            Exception ex3 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock, bc.GetLastBlock()));
            Assert.Equal("New blocks integrity check failed.", ex3.Message);

            nextblock.Hash = nextblock.ToHash();

            Exception ex4 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock, bc.GetLastBlock()));
            Assert.Equal("Expected the difficulty of the new block (3483650137023183582) to be less than the current difficulty (3654655253775102).", ex4.Message);

            // mine the block, genesis block is real low diff so its ok
            while(nextblock.GetDifficulty() > 3654655253775102)
            {
                nextblock.Nonce++;
                nextblock.Hash = nextblock.ToHash();
            }

            Exception ex5 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock, bc.GetLastBlock()));
            Assert.Equal("New block does not have any transactions.", ex5.Message);

            // create a keypair then add a transaction and then mine the block again
            SharpKeyPair skp = SharpKeyPair.Create();
            Transaction Tx = new Transaction();
            Input input = new Input
            {
                Transaction = "aba9dae211ad3df108d8eb914200f633",
                Index = 0,
                Amount = 50000000,
                Address = skp.GetAddress()
            };
            input.Sign(skp);
            Tx.Inputs = new Input[1] { input };

            Output output = new Output
            {
                Address = skp.GetAddress(),
                Amount = 50000000
            };
            Tx.Outputs = new Output[1] { output };

            Tx.Sign(skp);

            nextblock.Transactions.Add(Tx);

            // Reset the hash, this most probably is not a correct one, so mine:
            nextblock.Hash = nextblock.ToHash();
            while (nextblock.GetDifficulty() > 3654655253775102)
            {
                nextblock.Nonce++;
                nextblock.Hash = nextblock.ToHash();
            }

            Console.WriteLine(nextblock.Hash);

            Exception ex6 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock, bc.GetLastBlock()));
            Assert.Equal("New block does not have a reward transaction.", ex6.Message);

            nextblock.Transactions[0].Type = Transaction.TransactionType.REWARD;

        }
    }
}
