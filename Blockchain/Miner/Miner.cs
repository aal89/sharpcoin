using System;
using Blockchain.Transactions;
using Blockchain.Utilities;

namespace Blockchain
{
    public static class Miner
    {
        public static Block Solve(Block Block, Blockchain bc)
        {
            DateTime started = DateTime.UtcNow;
            ulong TargetDiff = bc.GetDifficulty();
            //int hashes = 0;
            while (Block.GetDifficulty() > TargetDiff)
            {
                //if ((int)DateTime.UtcNow.Subtract(started).TotalSeconds % 10 == 0 && hashes > 0)
                //{
                //    Console.WriteLine($"Hashrate: {hashes} in {(int)DateTime.UtcNow.Subtract(started).TotalSeconds}s");
                //}
                //hashes++;

                if ((int)DateTime.UtcNow.Subtract(started).TotalSeconds % 5 == 0)
                {
                    Block.Timestamp = DateTime.UtcNow;
                }

                Block.Nonce++;
                Block.Hash = Block.ToHash();
            }
            Console.WriteLine($"Solved block {Block.Index} with nonce {Block.Nonce} ({Block.Hash.Substring(0, 10)}...) at {DateTime.UtcNow} in {(int)DateTime.UtcNow.Subtract(started).TotalMinutes} mins! Target diff was: {TargetDiff}.");
            return Block;
        }

        public static Block Solve(SharpKeyPair skp, Blockchain bc)
        {
            Block LastBlock = bc.GetLastBlock();
            Block Block = new Block
            {
                Index = LastBlock.Index + 1,
                PreviousHash = LastBlock.Hash
            };

            Transaction RTx = new Transaction(new Output[] { new Output {
                Address = skp.GetAddress(),
                Amount = Config.BlockReward
            } });
            RTx.Sign(skp);

            Block.AddTransaction(RTx);

            Block.Hash = Block.ToHash();

            return Solve(Block, bc);
        }
    }
}
