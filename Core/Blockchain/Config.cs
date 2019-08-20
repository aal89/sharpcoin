using System;
using System.Collections.Generic;
using Core.Utilities;

namespace Core
{
    public static class Config
    {
        private static readonly float MeanTimeBetweenBlocks = 10 * 60; // in seconds

        public static readonly long BlockReward = 50000000000;

        public static readonly int MaximumBlockSizeInBytes = 2 * 1024 * 1000;

        public static readonly int MaximumConnections = 20;

        public static readonly int SectionSize = 144;

        public static readonly int TcpPort = 18910;

        public static ulong CalculateDifficulty(Blockchain Blockchain)
        {
            ulong GenesisDifficulty = Blockchain.GetBlockByIndex(0).GetDifficulty();
            Block[] Section = Blockchain.GetLastSection();

            if (Section != null)
            {
                List<int> TimeDifferences = new List<int> { };

                for (int i = 0; i < Section.Length - 1; i++)
                {
                    Block NextBlock = Section[i + 1];
                    Block PreviousBlock = Section[i];

                    TimeDifferences.Add(Convert.ToInt32(NextBlock.Timestamp.Subtract(PreviousBlock.Timestamp).TotalSeconds));
                }

                // Cap decline at max 80% (120 secs out of 600 secs).
                int AverageTimeDifference = Math.Max(120, TimeDifferences.Reduce(R.Total, 0) / TimeDifferences.Count);
                ulong AverageDifficulty = Section.Map(b => b.GetDifficulty()).Reduce(R.Total, GenesisDifficulty) / (ulong)Section.Length;
                // Cap growth at max 80% (0.8).
                float DeltaChange = Math.Min((float)0.8, (AverageTimeDifference - MeanTimeBetweenBlocks) / MeanTimeBetweenBlocks);

                return (ulong)(AverageDifficulty + AverageDifficulty * DeltaChange);
            }

            return GenesisDifficulty;
        }
    }
}
