using System;
using System.Threading;
using Core.Crypto;
using Core.Utilities;

namespace Core
{
    public class Operator
    {
        private static bool IsMining;
        private Thread MineThread;
        private SharpKeyPair MiningKeyPair;

        private readonly ILoggable Log;
        private readonly Blockchain Blockchain;

        public Operator(Blockchain Blockchain, ILoggable Log)
        {
            this.Log = Log;
            this.Blockchain = Blockchain;
        }

        public void Mine(object skp)
        {
            while (IsMining)
            {
                DateTime started = Date.Now();
                Log.NewLine($"Started at {started}. Attempting to solve block {Blockchain.GetLastBlock().Index + 1}.");

                Block b = Miner.Solve((SharpKeyPair)skp, Blockchain, IsMining);

                if (b != null)
                    Log.NewLine($"Solved block {b.Index} with nonce {b.Nonce} ({b.Hash.Substring(0, 10)}) in {(int)Date.Now().Subtract(started).TotalMinutes} mins! Target diff was: {b.GetPrettyDifficulty(true)}.");

                try
                {
                    Blockchain.AddBlock(b);
                }
                catch
                {
                    Log.NewLine($"Adding mined block failed. Skipping.");
                }

            }
            Log.NewLine($"Stopped at {Date.Now()}.");
        }

        public void Start(SharpKeyPair skp = null)
        {
            SharpKeyPair resolvedSkp = skp ?? MiningKeyPair;

            if (!IsMining && resolvedSkp != null)
            {
                IsMining = true;
                MiningKeyPair = resolvedSkp;

                MineThread = new Thread(new ParameterizedThreadStart(Mine));
                MineThread.Start(resolvedSkp);
            }
        }

        public void Stop()
        {
            IsMining = false;
        }

        public bool Busy()
        {
            return IsMining;
        }
    }
}
