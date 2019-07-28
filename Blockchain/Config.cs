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

        public static int MaximumBlockAge = (int)new TimeSpan(2, 0, 0, 0).TotalSeconds;

        public static ulong CalculateDifficulty(Blockchain Blockchain)
        {
            // Starting point
            ulong DefaultDifficulty = Blockchain.GetLastBlock().GetDifficulty();
            Block[] Chain = Blockchain.GetBlocks(10, Blockchain.Order.LAST);
            List<int> TimeDifferences = new List<int> { };

            // Walk backwards through the blockchain. Saves some absolute conversions (this is
            // because the blockchain implicitly is an ordered list).
            for (int i = Chain.Length - 1; i > 0; i--)
            {
                Block CurrentBlock = Chain[i];
                Block PreviousBlock = Chain[i - 1];

                TimeDifferences.Add(Convert.ToInt32(CurrentBlock.Timestamp.Subtract(PreviousBlock.Timestamp).TotalSeconds));
            }

            if (TimeDifferences.Count > 0)
            {
                // The average time diff can never be zero, one second is the minimum. This comes
                // down to the maximum percentile decrease in diff becomes -.9983333.
                int AverageTimeDifference = Math.Max(1, TimeDifferences.Reduce(R.Total, 0) / TimeDifferences.Count);

                // If the average time difference is larger than the mean time between blocks we decrease
                // difficulty. However, is the time difference smaller than the mean time then we
                // increase the difficulty. We calculate the deviation linearly to the mean and add or
                // subtract this percentage-wise from the last difficulty known (diff of last block).

                // We don't need if's, the deltapercentage is either positive or negative. We do cap
                // the maximum change to 100% (1) change. This is to cut off large pauses in between
                // blocks being mined. Normally you won't see this behaviour being coded in, but this
                // blockchain is adapted for longer periods of not mining and then suddenly mining again.
                float DeltaPercentage = Math.Min(1, ((float)AverageTimeDifference - (float)MeanTimeBetweenBlocks) / (float)MeanTimeBetweenBlocks);

                // We loose some precision with the ulong cast, but its too small to have any effect so its okay.
                return (ulong)(DefaultDifficulty + DefaultDifficulty * DeltaPercentage);
            }

            // We return the default difficulty when we have no time differences between blocks or when the
            // average time calculated is exactly the allowed mean time between blocks.
            return DefaultDifficulty;
        }
    }
}
