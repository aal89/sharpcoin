using System;
using Core.Crypto;

namespace Core
{
    public static class Miner
    {
        public static Block Solve(Block Block, ulong TargetDiff, bool ControlFlag = true)
        {
            while (Block.GetDifficulty() > TargetDiff && ControlFlag)
            {
                Block.Timestamp = DateTime.Parse(DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"));
                Block.Nonce++;
                Block.Hash = Block.ToHash();
            }

            return Block.GetDifficulty() < TargetDiff ? Block : null;
        }

        public static Block Solve(SharpKeyPair skp, Blockchain bc, bool ControlFlag = true)
        {
            return Solve(Block.Create(skp, bc), bc.GetDifficulty(), ControlFlag);
        }
    }
}
