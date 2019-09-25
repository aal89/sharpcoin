using System.Numerics;
using Core.Crypto;
using Core.Utilities;

namespace Core
{
    public static class Miner
    {
        public static Block Solve(Block Block, in bool ControlFlag = true)
        {
            while (!Block.IsCorrectDifficulty() && ControlFlag)
            {
                Block.Timestamp = Date.Now();
                Block.Nonce++;
                Block.Hash = Block.ToHash();
            }

            return Block.IsCorrectDifficulty() ? Block : null;
        }

        public static Block Solve(SharpKeyPair skp, Blockchain bc, in bool ControlFlag = true)
        {
            return Solve(Block.Create(skp, bc), ControlFlag);
        }
    }
}
