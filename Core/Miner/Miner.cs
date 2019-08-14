using System;
using Core.Crypto;
using Core.Utilities;

namespace Core
{
    public static class Miner
    {
        public static Block Solve(Block Block, Blockchain bc)
        {
            ILoggable log = new Logger("Miner");

            DateTime started = DateTime.UtcNow;
            ulong TargetDiff = bc.GetDifficulty();
            int hashes = 0;

            while (Block.GetDifficulty() > TargetDiff)
            {
                hashes++;

                Block.Timestamp = DateTime.Parse(DateTime.UtcNow.ToString());

                Block.Nonce++;
                Block.Hash = Block.ToHash();
            }

            log.NewLine($"Solved block {Block.Index} with nonce {Block.Nonce} ({Block.Hash.Substring(0, 10)}...) in {(int)DateTime.UtcNow.Subtract(started).TotalMinutes} mins! Target diff was: {TargetDiff}.");

            return Block;
        }

        public static Block Solve(SharpKeyPair skp, Blockchain bc)
        {
            return Solve(Block.Create(skp, bc), bc);
        }
    }
}
