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
            while (Block.GetDifficulty() > TargetDiff)
            {
                if ((int)started.Subtract(DateTime.UtcNow).TotalSeconds % 60 == 0)
                {
                    Block.Timestamp = DateTime.UtcNow;
                }

                Block.Nonce++;
                Block.Hash = Block.ToHash();
            }
            Console.WriteLine($"Solved block {Block.Index} ({Block.Hash.Substring(0, 10)}...) at {DateTime.UtcNow} in {(int)DateTime.UtcNow.Subtract(started).TotalMinutes} mins! TargetDiff was: {TargetDiff}.");
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

            Transaction RTx = new Transaction();
            Output utxo = new Output
            {
                Address = skp.GetAddress(),
                Amount = Config.BlockReward
            };
            RTx.Outputs = new Output[1] { utxo };
            RTx.Type = Transaction.TransactionType.REWARD;
            RTx.Sign(skp);

            Block.Transactions.Add(RTx);

            Block.Hash = Block.ToHash();

            return Solve(Block, bc);
        }
    }
}
