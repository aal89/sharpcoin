using Core.Crypto;
using Core.Utilities;

namespace Core
{
    public static class Miner
    {
        public static Block Solve(Block Block, ulong TargetDiff, in bool ControlFlag = true)
        {
            while (Block.GetDifficulty() > TargetDiff && ControlFlag)
            {
                Block.Timestamp = Date.Now();
                Block.Nonce++;
                Block.Hash = Block.ToHash();
            }

            return Block.GetDifficulty() < TargetDiff ? Block : null;
        }

        public static Block Solve(SharpKeyPair skp, Blockchain bc, in bool ControlFlag = true)
        {
            return Solve(Block.Create(skp, bc), bc.GetDifficulty(), ControlFlag);
        }
    }
}
