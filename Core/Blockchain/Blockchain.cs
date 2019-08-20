using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Core.Exceptions;
using Core.Transactions;
using Core.Utilities;

namespace Core
{
    public class Blockchain
    {
        public enum Order
        {
            FIRST, LAST
        }

        private readonly List<Block> Collection = new List<Block> { new GenesisBlock() };
        private readonly HashSet<Transaction> QueuedTransactions = new HashSet<Transaction>();
        private readonly Serializer Serializer = new Serializer();

        public event EventHandler BlockAdded;
        public event EventHandler QueuedTransactionAdded;

        public Blockchain()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "blockchain")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "blockchain"));
            }

            DirectoryInfo Info = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "blockchain"));
            FileInfo[] Paths = Info.GetFiles()
                .Filter(p => p.Name.Contains(".block"))
                .OrderBy(p => p.CreationTime)
                .ToArray();

            // Load only the last section of blocks, this saves time and memory
            Paths = Paths.TakeLast(Paths.Length % Config.SectionSize + Config.SectionSize).ToArray();

            for (var i = 0; i < Paths.Length; i++)
            {
                FileInfo fi = Paths[i];
                byte[] RawBlock = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "blockchain", fi.Name));
                // we only load the last section, so the first block might get incorrectly (correctly*) invalidated
                // we skip validation for the first block loaded from disk
                AddBlock(Serializer.Deserialize<Block>(RawBlock), i > 0, false);
            }
        }

        public Block[] GetBlocks()
        {
            return Collection.ToArray();
        }

        // kind of obscure naming, but the blockchain is split up in parts of x
        // blocks this is used to recalculate the diff. In bitcoin the blockchain
        // diff is recalculated every 2016 blocks (+- 2 weeks).
        public Block[] GetLastSection(int n = 1)
        {
            if (Collection.Count >= Config.SectionSize * n)
            {
                // Collection.Count is effectively the same as the block index
                int PreviousSectionIndex = Collection.Count - (Collection.Count % Config.SectionSize) - Config.SectionSize * n;
                return Collection.GetRange(PreviousSectionIndex, Config.SectionSize).ToArray();
            }
            return null;
        }

        public Block[] GetBlocks(int n, Order take = Order.FIRST)
        {
            if (take == Order.FIRST)
            {
                return Collection.Take(n).ToArray();
            }
            return Collection.TakeLast(n).ToArray();
        }

        public Block GetBlockByIndex(int Index)
        {
            return Collection.Find(Block => Block.Index == Index);
        }

        public Block GetBlockByHash(string Hash)
        {
            return Collection.Find(Block => Block.Hash == Hash);
        }

        public Block GetLastBlock()
        {
            return Collection.Last();
        }

        public ulong GetDifficulty()
        {
            return Config.CalculateDifficulty(this);
        }

        public void QueueTransaction(Transaction tx)
        {
            if (IsValidTransaction(tx) && tx.Verify())
            {
                QueuedTransactions.Add(tx);

                // Fire the tx added event
                QueuedTransactionAdded?.Invoke(tx, EventArgs.Empty);
            }
        }

        public Transaction[] GetQueuedTransactions()
        {
            return QueuedTransactions.ToArray();
        }

        public Transaction GetQueuedTransactionById(string Id)
        {
            return QueuedTransactions.ToList().Find(Tx => Tx.Id == Id);
        }

        public Transaction GetTransactionFromChain(string Id)
        {
            return Collection.FlatMap(Block => Block.GetTransactions()).Filter(Tx => Tx.Id == Id).FirstOrDefault();
        }

        public Transaction[] GetTransactions()
        {
            return Collection.FlatMap(Block => Block.GetTransactions()).ToArray();
        }

        private readonly object addblock_operation = new object();
        public void AddBlock(Block Block, bool check = true, bool save = true)
        {
            lock (addblock_operation)
            {
                if (check)
                    IsValidBlock(Block);

                if (save)
                    File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "blockchain", $"{Block.Index}.block"), Serializer.Serialize(Block));

                Collection.Add(Block);

                // Fire the block added event
                BlockAdded?.Invoke(Block, EventArgs.Empty);
            }
        }

        public bool IsValidBlock(Block NewBlock)
        {
            Block LastBlock = GetLastBlock();

            if (NewBlock.Timestamp.Subtract(LastBlock.Timestamp).TotalSeconds < 0)
            {
                throw new BlockAssertion($"The new block is older than the last block. Timestamp last block {LastBlock.Timestamp}, timestamp new block {NewBlock.Timestamp}");
            }

            if (NewBlock.Index != LastBlock.Index + 1)
            {
                throw new BlockAssertion($"Not consecutive blocks. Expected new block index to be {LastBlock.Index + 1}, but got {NewBlock.Index}.");
            }

            if (NewBlock.PreviousHash != LastBlock.Hash)
            {
                throw new BlockAssertion($"New block points to a different block. Previous hash of new block is {NewBlock.PreviousHash}, while hash of last block is {LastBlock.Hash}.");
            }

            if (NewBlock.Hash != NewBlock.ToHash())
            {
                throw new BlockAssertion($"New blocks integrity check failed. Is {NewBlock.Hash}, should be: {NewBlock.ToHash()}");
            }

            if (NewBlock.GetDifficulty() >= GetDifficulty())
            {
                throw new BlockAssertion($"Expected the difficulty of the new block ({NewBlock.GetDifficulty()}) to be less than the current difficulty ({GetDifficulty()}).");
            }

            if (!NewBlock.HasTransactions())
            {
                throw new BlockAssertion($"New block does not have any transactions.");
            }

            if (Serializer.Size(NewBlock) > Config.MaximumBlockSizeInBytes)
            {
                throw new BlockAssertion($"New Block size (in bytes) is {Serializer.Size(NewBlock)} and the maximum is {Config.MaximumBlockSizeInBytes}.");
            }

            if (!NewBlock.HasRewardTransaction())
            {
                throw new BlockAssertion($"New block does not have a reward transaction.");
            }

            if (!NewBlock.GetRewardTransaction().IsRewardTransaction(Config.BlockReward) || !NewBlock.GetRewardTransaction().Verify())
            {
                throw new BlockAssertion($"New block does not have a valid reward transaction.");
            }

            // Somewhat more expensive operations

            if (NewBlock.GetTransactions().Any(Tx => GetTransactionFromChain(Tx.Id) != null))
            {
                throw new BlockAssertion($"New block contains duplicate transactions.");
            }

            if (!NewBlock.GetTransactions().Filter(RTx => RTx.Type != Transaction.TransactionType.REWARD).All(Tx => Tx.Equates() && Tx.Verify()))
            {
                throw new BlockAssertion($"New block contains invalid transaction (inputs do not equate with outputs or signature invalid).");
            }

            // Double spending inputs check and validate referenced outputs.

            if(!NewBlock.GetTransactions().All(IsValidTransaction))
            {
                throw new BlockAssertion($"New block tries to spend already spent inputs or the referenced outputs in the inputs are invalid.");
            }

            return true;
        }

        // Verifies the transaction does not try to double spend and all the inputs are correctly referenced
        public bool IsValidTransaction(Transaction tx)
        {
            // Double spend check
            if (GetTransactions().FlatMap(Tx => Tx.Inputs).Any(tx.ContainsInput))
            {
                return false;
            }

            // Referenced output check
            if (!tx.Inputs.All(input => GetTransactionFromChain(input.Transaction).Outputs[input.Index].Corresponds(input)))
            {
                return false;
            }

            return true;
        }

        public int Size()
        {
            return Collection.Count;
        }
    }
}
