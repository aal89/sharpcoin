﻿using System;
using System.Collections.Generic;
using Blockchain.Utilities;
using System.Linq;
using Newtonsoft.Json;

namespace Blockchain
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
    }
}
