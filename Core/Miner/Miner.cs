using System;
using Core.Crypto;

namespace Core
{
    public static class Miner
    {
        public static Block Solve(Block Block, Blockchain bc)
        {
            ulong TargetDiff = bc.GetDifficulty();

            while (Block.GetDifficulty() > TargetDiff)
            {
                Block.Timestamp = DateTime.Parse(DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"));
                Block.Nonce++;
                Block.Hash = Block.ToHash();
            }

            return Block;
        }

        public static Block Solve(SharpKeyPair skp, Blockchain bc)
        {
            return Solve(Block.Create(skp, bc), bc);
        }
    }
}
