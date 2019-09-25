using System;
using System.Collections.Generic;
using System.Numerics;
using Core.Utilities;

namespace Core
{
    public static class Config
    {
        private static readonly float MeanTimeBetweenBlocks = 10 * 60; // in seconds

        public static readonly long BlockReward = 50000000000;

        public static readonly int MaximumBlockSizeInBytes = 2 * 1024 * 1000;

        public static readonly int MaximumConnections = 20;

        public static readonly int SectionSize = 10;

        public static readonly int TcpPort = 18910;

        public static readonly int TcpPortApi = 28910;

        public static readonly int TcpConnectTimeout = 5000;

        public static readonly int PeerInterval = 15 * 60 * 1000; // in ms

        public static readonly int PeerKeepAliveInterval = 25 * 1000; // in ms

        // quite arbitrarily chosen number, we assume that an orphan chain will never be any longer than this...
        public static readonly int MaximumBlockTruncation = 50;

        public static readonly string BlockchainDirectory = "blockchain";

        public static BigInteger CalculateDifficulty(Block[] Section)
        {
            BigInteger GenesisDifficulty = new GenesisBlock().GetTargetDifficulty();

            if (Section == null)
                return GenesisDifficulty;

            List<int> TimeDifferences = new List<int> { };

            for (int i = 0; i < Section.Length - 1; i++)
            {
                Block NextBlock = Section[i + 1];
                Block PreviousBlock = Section[i];

                TimeDifferences.Add(Convert.ToInt32(NextBlock.Timestamp.Subtract(PreviousBlock.Timestamp).TotalSeconds));
            }

            // Cap growth and decline at max 80% (120 secs out of 600 secs), this prevents impossible target diffs.
            int AverageTimeDifference = Math.Max(120, TimeDifferences.Reduce(R.Total, 0) / TimeDifferences.Count);
            float DeltaChange = Math.Min((float)0.8, (AverageTimeDifference - MeanTimeBetweenBlocks) / MeanTimeBetweenBlocks);

            Console.WriteLine(DeltaChange.ToString("0.00"));

            return Section[0].GetTargetDifficulty() + Section[0].GetTargetDifficulty().Percentage(DeltaChange);
        }
    }
}
