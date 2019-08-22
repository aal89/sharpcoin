using System;
using System.Collections.Generic;
using Core.Utilities;
using System.Linq;
using Newtonsoft.Json;
using Core.Transactions;
using Core.Crypto;

namespace Core
{
    public class Block: IEquatable<Block>
    {
        public int Index;
        public string PreviousHash = "";
        public string Hash = "";
        public DateTime Timestamp = DateTime.UtcNow;
        public uint Nonce;
        public byte Version = 0x00;

        [JsonProperty]
        private readonly List<Transaction> Transactions = new List<Transaction>();

        public Block()
        {
            Hash = ToHash();
        }

        [JsonConstructor]
        public Block(List<Transaction> Transactions)
        {
            this.Transactions = Transactions;
            Hash = ToHash();
        }

        public ulong GetDifficulty()
        {
            return Convert.ToUInt64(Hash.Substring(0, 16), 16);
        }

        public bool HasTransactions()
        {
            return Transactions.Count > 0;
        }

        public void AddTransaction(Transaction Transaction)
        {
            Transactions.Add(Transaction);
        }

        public Transaction[] GetTransactions()
        {
            return Transactions.ToArray();
        }

        public bool HasRewardTransaction()
        {
            return Transactions.Filter(Tx => Tx.Type == Transaction.TransactionType.REWARD).Count() == 1;
        }

        public Transaction GetRewardTransaction()
        {
            return Transactions.Find(Tx => Tx.Type == Transaction.TransactionType.REWARD);
        }

        public string ToHash()
        {
            return Utilities.Hash.Sha256($"{Index}{PreviousHash}{Timestamp.ToString("MM/dd/yyyy HH:mm:ss")}{Nonce}{Version}{Transactions.Stringified()}");
        }

        // Creates a next block based on the chain given with a reward tx for the keypair.
        public static Block Create(SharpKeyPair skp, Blockchain bc)
        {
            Serializer s = new Serializer();
            Transaction[] queued = bc.GetQueuedTransactions();

            Block b = new Block
            {
                Index = bc.GetLastBlock().Index + 1,
                PreviousHash = bc.GetLastBlock().Hash
            };

            b.AddTransaction(Builder.MakeReward(skp, Config.BlockReward));

            int count = 0;
            do
            {
                try
                {
                    Transaction tx = queued[count++];
                    // Only add verified and valid transactions to the block
                    if (tx.Verify() && bc.IsValidTransaction(tx))
                        b.AddTransaction(tx);
                } catch
                {
                    break;
                }
            } while (s.Size(b) < Config.MaximumBlockSizeInBytes);

            b.Hash = b.ToHash();

            return b;
        }

        public bool Equals(Block other)
        {
            return other != null && Hash == other.Hash;
        }
    }
}
