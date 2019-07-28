using System;
using Blockchain;
using Blockchain.Utilities;
using Xunit;
using BlockchainObject = Blockchain.Blockchain;
using Blockchain.Transactions;

namespace BlockchainTests
{
    public class ConfigTests
    {
        [Fact]
        public void Config_CalculateDifficulty_1Block()
        {
            BlockchainObject bc = new BlockchainObject();

            Assert.Equal<ulong>(3654655253775102, Config.CalculateDifficulty(bc));
        }

        [Fact]
        public void Config_CalculateDifficulty_2Blocks()
        {
            SharpKeyPair skp = SharpKeyPair.Create();
            BlockchainObject bc = new BlockchainObject();

            Block nextblock = new Block
            {
                Index = 1,
                PreviousHash = "000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962",
                Timestamp = new DateTime(2019, 07, 27, 21, 25, 50),
                Nonce = 10683,
                Hash = "00044ad42c31de15a312892a19d9aad4e3b14e6ff28e37024882f659abc43a15"
            };

            Transaction RTx = new Transaction("6bb4b1a7d7e0ef62f55fec7b57a58cacad038400");
            Output outputreward = new Output
            {
                Address = skp.GetAddress(),
                Amount = 50000000000
            };
            RTx.Outputs = new Output[1] { outputreward };
            RTx.Type = Transaction.TransactionType.REWARD;
            RTx.Sign(skp);

            nextblock.Transactions.Add(RTx);

            bc.AddBlock(nextblock);

            Assert.Equal<ulong>(2416350211342336, Config.CalculateDifficulty(bc));
        }

        [Fact]
        public void Config_CalculateDifficulty_3Blocks()
        {
            SharpKeyPair skp = SharpKeyPair.Create();
            BlockchainObject bc = new BlockchainObject();

            Block nextblock = new Block
            {
                Index = 1,
                PreviousHash = "000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962",
                Timestamp = new DateTime(2019, 07, 27, 21, 25, 50),
                Nonce = 10683,
                Hash = "00044ad42c31de15a312892a19d9aad4e3b14e6ff28e37024882f659abc43a15"
            };

            Transaction RTx = new Transaction("6bb4b1a7d7e0ef62f55fec7b57a58cacad038400");
            Output outputreward = new Output
            {
                Address = skp.GetAddress(),
                Amount = 50000000000
            };
            RTx.Outputs = new Output[1] { outputreward };
            RTx.Type = Transaction.TransactionType.REWARD;
            RTx.Sign(skp);

            nextblock.Transactions.Add(RTx);

            bc.AddBlock(nextblock);

            Block nextblock2 = new Block
            {
                Index = 2,
                PreviousHash = nextblock.Hash,
                Timestamp = new DateTime(2019, 07, 27, 21, 29, 50),
                Nonce = 16707,
                Hash = "0005013e4348bc0caa9c51c1b780dbc025ba2aed91a083ca3b7c1c59cf4e5055"
            };

            Transaction RTx2 = new Transaction("a5cdd20e70cc57bbea3666c878b375bd9897aabb");
            Output outputreward2 = new Output
            {
                Address = skp.GetAddress(),
                Amount = 50000000000
            };
            RTx2.Outputs = new Output[1] { outputreward2 };
            RTx2.Type = Transaction.TransactionType.REWARD;
            RTx2.Sign(skp);

            nextblock2.Transactions.Add(RTx2);

            bc.AddBlock(nextblock2);

            Assert.Equal<ulong>(2817483513790464, Config.CalculateDifficulty(bc));
        }
    }
}
