﻿using System;
using System.Collections.Generic;
using Core.Utilities;
using System.Linq;
using Newtonsoft.Json;
using Core.Transactions;
using Core.Crypto;

namespace Core
{
    public class Block
    {
        public int Index;
        public string PreviousHash = "";
        public string Hash = "";
        public DateTime Timestamp = DateTime.UtcNow;
        public uint Nonce;
        public byte Version = 0x00;

        private string StringifiedTransactions = "";

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
            StringifiedTransactions = Transactions.Map(Tx => Tx.ToString()).Reduce(R.Concat, "");
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
            StringifiedTransactions = Transactions.Map(Tx => Tx.ToString()).Reduce(R.Concat, "");
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
            return Utilities.Hash.Sha256($"{Index}{PreviousHash}{Timestamp}{Nonce}{Version}{StringifiedTransactions}");
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
                    b.AddTransaction(queued[count++]);
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