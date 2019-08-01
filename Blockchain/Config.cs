using System;
using System.Collections.Generic;
using Blockchain.Utilities;

namespace Blockchain
{
    public static class Config
    {
        private static int MeanTimeBetweenBlocks = 10 * 60; // in seconds
        // block reward is the 'half' of ulong number in length (20 digits)
        // 'one' coin is now dividable into a billion pieces and there are
        // still 18446744073 blocks to be mined with this reward.
        public static ulong BlockReward = 50000000000;

        public static int MaximumBlockSizeInBytes = 2 * 1024;

        public static int SectionSize = 6;

        public static int MaximumBlockAge = (int)new TimeSpan(2, 0, 0, 0).TotalSeconds;

        public static ulong CalculateDifficulty(Blockchain Blockchain)
        {
            Block[] Chain = Blockchain.GetLastSection();
            Block[] SecondLastSection = Blockchain.GetSecondLastSection();
            List<int> TimeDifferences = new List<int> { };

            // Walk backwards through the blockchain. Saves some absolute conversions (this is
            // because the blockchain implicitly is an ordered list).
            for (int i = Chain.Length - 1; i > 0; i--)
            {
                Block CurrentBlock = Chain[i];
                Block PreviousBlock = Chain[i - 1];

                TimeDifferences.Add(Convert.ToInt32(CurrentBlock.Timestamp.Subtract(PreviousBlock.Timestamp).TotalSeconds));
            }

            // if we have a full section of blocks (which is always except when the chain is shorter
            // than 6 blocks) continue calculating the averages, otherwise return
            // the diff of the genesis block
            if (TimeDifferences.Count == SectionSize - 1)
            {
                // The average time diff can never be zero, sixty seconds is the minimum (20%). This comes
                // down to the maximum percentile decrease in diff (lowerbound) is 80%.
                int AverageTimeDifference = Math.Max(1, TimeDifferences.Reduce(R.Total, 0) / TimeDifferences.Count);
                //ulong AverageDifficulty = Chain.Map(block => block.GetDifficulty()).Reduce<ulong>(R.Total, 0) / (ulong)Chain.Length;
                ulong AverageSecondLastDiff = SecondLastSection.Map(block => block.GetDifficulty()).Reduce<ulong>(R.Total, 0) / (ulong)SecondLastSection.Length;

                // If the average time difference is larger than the mean time between blocks we decrease
                // difficulty. However, is the time difference smaller than the mean time then we
                // increase the difficulty.

                // We don't need if's, the deltapercentage is either positive or negative. We do cap
                // the maximum change to 0.8 (or 80%) (upperbound) change. This is to cut off large pauses in between
                // blocks being mined. Normally you won't see this behaviour being coded in, but this
                // blockchain is adapted for longer periods of not mining and then suddenly mining again.
                // Without giving strange large swings in diff. Keep in mind that 'stabilizing' the chain
                // will take some time after pauses.
                float DeltaPercentage = Math.Min((float)500.8, ((float)AverageTimeDifference - (float)MeanTimeBetweenBlocks) / (float)MeanTimeBetweenBlocks);
                Console.WriteLine(DeltaPercentage);
                // We loose some precision with the ulong cast, but its too small to have any effect so its okay.
                ulong TargetDiff = (ulong)(AverageSecondLastDiff + AverageSecondLastDiff * DeltaPercentage);
                return TargetDiff != 0 ? TargetDiff : UInt64.MaxValue;
            }

            return Chain[0].GetDifficulty();
        }
    }
}
