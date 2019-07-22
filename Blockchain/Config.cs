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
        public static ulong BlockReward = 1000000000;

        public static ulong CalculateDifficulty(Blockchain Blockchain)
        {
            // Starting point
            ulong DefaultDifficulty = Blockchain.GetLastBlock().GetDifficulty();
            Block[] Chain = Blockchain.GetBlocks(10, Blockchain.Order.LAST);
            List<int> TimeDifferences = new List<int> { };

            // Walk backwards through the blockchain. Saves some absolute conversions (this is
            // because the blockchain implicitly is an ordered list). Step size is 2 because
            // we calculate time difference between a pair of blocks. 
            for (int i = Chain.Length - 1; i > 0; i -= 2)
            {
                Block CurrentBlock = Chain[i];
                Block PreviousBlock = Chain[i - 1];

                TimeDifferences.Add(Convert.ToInt32(CurrentBlock.Timestamp.Subtract(PreviousBlock.Timestamp).TotalSeconds));
            }

            int TimeDifferenceLength = TimeDifferences.ToArray().Length;

            if (TimeDifferenceLength > 0)
            {
                int AverageTimeDifference = TimeDifferences.Reduce(R.Total, 0) / TimeDifferenceLength;

                // If the average time difference is larger than the mean time between blocks we decrease
                // difficulty. However, is the time difference smaller than the mean time then we
                // increase the difficulty. We calculate the deviation linearly to the mean and add or
                // subtract this percentage-wise from the last difficulty known (diff of last block).

                // We don't need if's, de deltapercentage is either positive or negative.
                float DeltaPercentage = ((float)AverageTimeDifference - (float)MeanTimeBetweenBlocks) / (float)MeanTimeBetweenBlocks;
                float Change = 1 - DeltaPercentage;

                // We loose some precision with the ulong cast, but its too small to have any effect so its okay.
                return (ulong)(DefaultDifficulty * Change);
            }

            // We return the default difficulty when we have no time differences between blocks or when the
            // average time calculated is exactly the allowed mean time between blocks.
            return DefaultDifficulty;
        }
    }
}
