using System;
using System.Collections.Generic;
using Core.Utilities;
using System.Linq;
using Newtonsoft.Json;
using Core.Transactions;
using Core.Crypto;
using System.Numerics;
using System.Globalization;

namespace Core
{
    public class Block: IEquatable<Block>
    {
        public int Index;
        public string PreviousHash = "";
        public string TargetHash = "";
        public string Hash = "";
        public DateTime Timestamp = Date.Now();
        public uint Nonce;
        
        [JsonProperty]
        private readonly HashSet<Transaction> Transactions = new HashSet<Transaction>(new TransactionComparer());

        public Block()
        {
            Hash = ToHash();
        }

        [JsonConstructor]
        public Block(HashSet<Transaction> Transactions)
        {
            this.Transactions = Transactions;
            Hash = ToHash();
        }

        public bool IsCorrectDifficulty()
        {
            return GetDifficulty() < GetTargetDifficulty();
        }

        public BigInteger GetDifficulty()
        {
            // prepend a zero to the hash so never a negative value gets parsed...
            return BigInteger.Parse($"0{Hash}", NumberStyles.AllowHexSpecifier);
        }

        public BigInteger GetTargetDifficulty()
        {
            // prepend a zero to the hash so never a negative value gets parsed...
            return BigInteger.Parse($"0{TargetHash}", NumberStyles.AllowHexSpecifier);
        }

        public int GetPrettyDifficulty()
        {
            return GetDifficulty().Inaccurate(new GenesisBlock().GetDifficulty());
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
            return Transactions.Filter(Tx => Tx.Type == Transaction.TransactionType.REWARD).FirstOrDefault();
        }

        public string ToHash()
        {
            return Utilities.Hash.Sha256($"{Index}{PreviousHash}{TargetHash}{Timestamp.FormattedString()}{Nonce}{Transactions.Stringified()}");
        }

        public bool Equals(Block other)
        {
            return other != null && Hash == other.Hash;
        }

        // Creates a next block based on the chain given with a reward tx for the keypair.
        public static Block Create(SharpKeyPair skp, Blockchain bc)
        {
            Serializer s = new Serializer();
            Transaction[] queued = bc.GetQueuedTransactions();
            Block LastBlock = bc.GetLastBlock();

            Block b = new Block
            {
                Index = LastBlock.Index + 1,
                PreviousHash = LastBlock.Hash
            };

            b.TargetHash = b.Index % Config.SectionSize == 0 ? Config.CalculateDifficulty(bc.GetLastSection()).ToString("x") : LastBlock.TargetHash;

            b.AddTransaction(Builder.MakeReward(skp, Config.BlockReward));

            int count = 0;
            do
            {
                try
                {
                    Transaction tx = queued[count++];
                    // Only add verified, valid and non reward transactions to the block
                    if (tx.Verify() && bc.IsValidTransaction(tx) && tx.IsDefaultTransaction())
                        b.AddTransaction(tx);
                } catch
                {
                    break;
                }
            } while (s.Size(b) < Config.MaximumBlockSizeInBytes);

            b.Hash = b.ToHash();

            return b;
        }
    }
}
