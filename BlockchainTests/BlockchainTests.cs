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

            Assert.Equal(ulong.Parse("233890554552"), gblock.GetDifficulty());
            Assert.True(gblock.HasTransactions());
            Assert.True(gblock.HasRewardTransaction());
            Assert.Equal("0000003674f6a2b8ac9e577ae1795f34c1badf5d7ac017c8d087c0c3ac1b7289", gblock.ToHash());
            Assert.True(gblock.GetRewardTransaction().Verify());
        }

        //[Fact]
        //public void Block_IsValidBlock_Test()
        //{
        //    BlockchainObject bc = new BlockchainObject();

        //    Block nextblock = new Block();
        //    // This one is here to fake previous hash because the genesis block
        //    // does not have a hash and defaults out to "".
        //    nextblock.PreviousHash = "PREVHASH";

        //    Exception ex1 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal("Not consecutive blocks. Expected new block index to be 1, but got 0.", ex1.Message);

        //    nextblock.Index = 1;

        //    Exception ex2 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal("New block points to a different block. Previous hash of new block is PREVHASH, while hash of last block is 000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962.", ex2.Message);

        //    nextblock.PreviousHash = "000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962";

        //    Exception ex3 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal("New blocks integrity check failed.", ex3.Message);

        //    nextblock.Hash = nextblock.ToHash();

        //    Exception ex4 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal($"Expected the difficulty of the new block ({nextblock.GetDifficulty()}) to be less than the current difficulty ({bc.GetDifficulty()}).", ex4.Message);

        //    // mine the block, genesis block is real low diff so its ok
        //    while (nextblock.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock.Nonce++;
        //        nextblock.Hash = nextblock.ToHash();
        //    }

        //    Exception ex5 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal("New block does not have any transactions.", ex5.Message);

        //    // create a keypair then add a transaction and then mine the block again
        //    SharpKeyPair skp = SharpKeyPair.Create();
        //    Transaction Tx = new Transaction();
        //    Input input = new Input
        //    {
        //        Transaction = "aba9dae211ad3df108d8eb914200f633",
        //        Index = 0,
        //        Amount = 40000000,
        //        Address = skp.GetAddress()
        //    };
        //    input.Sign(skp);
        //    Tx.Inputs = new Input[1] { input };

        //    Output output = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000
        //    };
        //    Tx.Outputs = new Output[1] { output };

        //    Tx.Sign(skp);

        //    nextblock.Transactions.Add(Tx);

        //    // Reset the hash, this most probably is not a correct one, so mine:
        //    nextblock.Hash = nextblock.ToHash();
        //    while (nextblock.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock.Nonce++;
        //        nextblock.Hash = nextblock.ToHash();
        //    }

        //    Exception ex6 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal("New block does not have a reward transaction.", ex6.Message);

        //    nextblock.Transactions[0].Type = Transaction.TransactionType.REWARD;

        //    Exception ex7 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal("New block does not have a valid reward transaction.", ex7.Message);

        //    nextblock.Transactions[0].Type = Transaction.TransactionType.DEFAULT;

        //    Transaction Tx2 = new Transaction();
        //    Output outputreward = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000
        //    };
        //    Tx2.Outputs = new Output[1] { outputreward };
        //    Tx2.Type = Transaction.TransactionType.REWARD;
        //    Tx2.Sign(skp);

        //    nextblock.Transactions.Add(Tx2);

        //    // Reset the hash, this most probably is not a correct one, so mine:
        //    nextblock.Hash = nextblock.ToHash();
        //    while (nextblock.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock.Nonce++;
        //        nextblock.Hash = nextblock.ToHash();
        //    }

        //    Exception ex8 = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock));
        //    Assert.Equal("New block contains invalid transaction (inputs do not equate with outputs or signature invalid).", ex8.Message);

        //    nextblock.Transactions[0].Inputs[0].Amount = 50000000;
        //    nextblock.Transactions[0].Inputs[0].Sign(skp);
        //    nextblock.Transactions[0].Sign(skp);

        //    nextblock.Hash = nextblock.ToHash();
        //    while (nextblock.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock.Nonce++;
        //        nextblock.Hash = nextblock.ToHash();
        //    }

        //    Assert.True(bc.IsValidBlock(nextblock));
        //}

        //[Fact]
        //public void Block_IsValidBlock_DuplicateTransactions()
        //{
        //    BlockchainObject bc = new BlockchainObject();
        //    SharpKeyPair skp = SharpKeyPair.Create();

        //    Block nextblock = new Block
        //    {
        //        Index = 1,
        //        PreviousHash = "000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962"
        //    };

        //    Transaction RTx = new Transaction();
        //    Output outputreward = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000
        //    };
        //    RTx.Outputs = new Output[1] { outputreward };
        //    RTx.Type = Transaction.TransactionType.REWARD;
        //    RTx.Sign(skp);

        //    nextblock.Transactions.Add(RTx);

        //    nextblock.Hash = nextblock.ToHash();
        //    while (nextblock.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock.Nonce++;
        //        nextblock.Hash = nextblock.ToHash();
        //    }

        //    bc.AddBlock(nextblock);

        //    Block nextblock2 = new Block
        //    {
        //        Index = 2,
        //        PreviousHash = nextblock.Hash
        //    };

        //    nextblock2.Transactions.Add(RTx);

        //    nextblock2.Hash = nextblock2.ToHash();
        //    while (nextblock2.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock2.Nonce++;
        //        nextblock2.Hash = nextblock2.ToHash();
        //    }

        //    Exception ex = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock2));
        //    Assert.Equal("New block contains duplicate transactions.", ex.Message);
        //}

        //[Fact]
        //public void Block_IsValidBlock_DoubleSpend()
        //{
        //    BlockchainObject bc = new BlockchainObject();
        //    SharpKeyPair skp = SharpKeyPair.Create();

        //    // Block 1

        //    Block nextblock = new Block
        //    {
        //        Index = 1,
        //        PreviousHash = "000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962"
        //    };

        //    Transaction RTx = new Transaction();
        //    Output outputreward = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000
        //    };
        //    RTx.Outputs = new Output[1] { outputreward };
        //    RTx.Type = Transaction.TransactionType.REWARD;
        //    RTx.Sign(skp);

        //    nextblock.Transactions.Add(RTx);

        //    nextblock.Hash = nextblock.ToHash();
        //    while (nextblock.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock.Nonce++;
        //        nextblock.Hash = nextblock.ToHash();
        //    }

        //    bc.AddBlock(nextblock);

        //    // Block 2

        //    Block nextblock2 = new Block
        //    {
        //        Index = 2,
        //        PreviousHash = nextblock.Hash
        //    };

        //    Transaction RTx2 = new Transaction();
        //    Output outputreward2 = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000
        //    };
        //    RTx2.Outputs = new Output[1] { outputreward2 };
        //    RTx2.Type = Transaction.TransactionType.REWARD;
        //    RTx2.Sign(skp);

        //    Transaction Tx2 = new Transaction();
        //    Input input2 = new Input
        //    {
        //        Transaction = nextblock.Transactions[0].Id,
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000,
        //        Index = 0
        //    };
        //    input2.Sign(skp);
        //    Tx2.Inputs = new Input[1] { input2 };
        //    Output output2 = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000
        //    };
        //    Tx2.Outputs = new Output[1] { output2 };
        //    Tx2.Sign(skp);

        //    nextblock2.Transactions.Add(RTx2);
        //    nextblock2.Transactions.Add(Tx2);

        //    nextblock2.Hash = nextblock2.ToHash();
        //    while (nextblock2.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock2.Nonce++;
        //        nextblock2.Hash = nextblock2.ToHash();
        //    }

        //    bc.AddBlock(nextblock2);

        //    // Block 3

        //    Block nextblock3 = new Block
        //    {
        //        Index = 3,
        //        PreviousHash = nextblock2.Hash
        //    };

        //    Transaction RTx3 = new Transaction();
        //    Output outputreward3 = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000
        //    };
        //    RTx3.Outputs = new Output[1] { outputreward3 };
        //    RTx3.Type = Transaction.TransactionType.REWARD;
        //    RTx3.Sign(skp);

        //    Transaction Tx3 = new Transaction();
        //    // This input has been used in block 2 already
        //    Input input3 = new Input
        //    {
        //        Transaction = nextblock.Transactions[0].Id,
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000,
        //        Index = 0
        //    };
        //    input3.Sign(skp);
        //    Tx3.Inputs = new Input[1] { input3 };
        //    Output output3 = new Output
        //    {
        //        Address = skp.GetAddress(),
        //        Amount = 50000000000
        //    };
        //    Tx3.Outputs = new Output[1] { output3 };
        //    Tx3.Sign(skp);

        //    nextblock3.Transactions.Add(RTx3);
        //    nextblock3.Transactions.Add(Tx3);

        //    nextblock3.Hash = nextblock3.ToHash();
        //    while (nextblock3.GetDifficulty() > bc.GetDifficulty())
        //    {
        //        nextblock3.Nonce++;
        //        nextblock3.Hash = nextblock3.ToHash();
        //    }

        //    Exception ex = Assert.Throws<BlockAssertion>(() => bc.IsValidBlock(nextblock3));
        //    Assert.Equal("New block tries to spend already spent transaction inputs.", ex.Message);
        //}
    }
}
