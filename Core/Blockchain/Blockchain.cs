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
        private readonly ILoggable Log;

        private readonly Block Genesis = new GenesisBlock();
        private readonly HashSet<Transaction> QueuedTransactions = new HashSet<Transaction>(new TransactionComparer());
        private readonly Serializer Serializer = new Serializer();

        private readonly string BlockchainDirectory = Path.Combine(Directory.GetCurrentDirectory(), Config.BlockchainDirectory);

        private readonly Indexes.Transactions Transactions;
        private readonly Indexes.UnspentOutputs UnspentOutputs;

        public event EventHandler BlockAdded;
        public event EventHandler QueuedTransactionAdded;

        public Blockchain(ILoggable Log = null)
        {
            this.Log = Log ?? new NullLogger();

            if (!File.Exists(BlockchainDirectory))
                Directory.CreateDirectory(BlockchainDirectory);

            
            Validate();
            
            Transactions = new Indexes.Transactions(this, Config.BlockchainDirectory);
            Log.NewLine($"Loading txs index.");
            Transactions.Read();
            UnspentOutputs = new Indexes.UnspentOutputs(Config.BlockchainDirectory);
            Log.NewLine($"Loading utxo index.");
            UnspentOutputs.Read();
        }

        public void Validate()
        {
            Log.NewLine("Validating blockchain.");
            for (var i = 1; i < Size(); i++)
            {
                IsValidBlock(ReadBlock(i), i == 1 ? Genesis : ReadBlock(i - 1));
                Log.NewLine($"Block {i} is valid!");
            }
            Log.NewLine($"Valid! Size is {Size()}.");
        }

        // kind of obscure naming, but the blockchain is split up in parts of x
        // blocks this is used to recalculate the diff.
        public Block[] GetLastSection(int n = 1)
        {
            int BlockchainSize = Size();
            if (BlockchainSize >= Config.SectionSize * n)
            {
                // BlockchainSize is effectively the same as the block index
                int PreviousSectionIndex = BlockchainSize - (BlockchainSize % Config.SectionSize) - Config.SectionSize * n;
                Block[] BlockSection = new Block[Config.SectionSize];

                for (int i = PreviousSectionIndex; i < PreviousSectionIndex + Config.SectionSize; i++)
                    BlockSection[i - PreviousSectionIndex] = ReadBlock(i);

                return BlockSection;
            }
            return null;
        }

        public Block GetBlockByIndex(int Index)
        {
            return ReadBlock(Index) ?? Genesis;
        }

        public Block GetLastBlock()
        {
            return ReadBlock(Size());
        }

        public ulong GetDifficulty()
        {
            return Config.CalculateDifficulty(this);
        }

        public void QueueTransaction(Transaction tx)
        {
            if (IsValidTransaction(tx) && tx.Verify() && QueuedTransactions.Add(tx))
            {
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

        private readonly object removeblock_operation = new object();
        public void RemoveBlock(Block Block)
        {
            lock (removeblock_operation)
            {
                DeleteBlock(Block);
            }
        }

        private readonly object addblock_operation = new object();
        public void AddBlock(Block Block, Block PreviousBlock = null, bool TriggerEvent = true)
        {
            lock (addblock_operation)
            {
                // Check block
                IsValidBlock(Block, PreviousBlock);

                // Write block out to disk
                WriteBlock(Block);

                // Remove all queued transactions that got included in the valid block
                QueuedTransactions.RemoveWhere(tx => Block.GetTransactions().Any(btx => btx.Id == tx.Id));

                // Remove all unspent outputs from the index that got used for this blocks transactions.
                // todo

                // Create indexes for all the new data this block adds
                foreach (Transaction tx in Block.GetTransactions())
                    Transactions.Add(tx.Id, Block.Index);

                foreach (Output output in Block.GetTransactions().FlatMap(tx => tx.Outputs))
                    UnspentOutputs.Add(output);

                if (TriggerEvent)
                    BlockAdded?.Invoke(Block, EventArgs.Empty);
            }
        }

        public bool IsValidBlock(Block NewBlock, Block PreviousBlock = null)
        {
            if (PreviousBlock == null)
                PreviousBlock = GetLastBlock();

            if (NewBlock == null)
                throw new BlockAssertion($"New or previous block is null.");

            if (NewBlock.Timestamp.Subtract(PreviousBlock.Timestamp).TotalSeconds < 0)
            {
                throw new BlockAssertion($"The new block is older than the last block. Timestamp last block {PreviousBlock.Timestamp}, timestamp new block {NewBlock.Timestamp}.");
            }

            if (NewBlock.Index != PreviousBlock.Index + 1)
            {
                throw new BlockAssertion($"Not consecutive blocks. Expected new block index to be {PreviousBlock.Index + 1}, but got {NewBlock.Index}.");
            }

            if (NewBlock.PreviousHash != PreviousBlock.Hash)
            {
                throw new BlockAssertion($"New block points to a different block. Previous hash of new block is {NewBlock.PreviousHash}, while hash of last block is {PreviousBlock.Hash}.");
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

            if (NewBlock.GetTransactions().Any(Tx => Transactions.Get(Tx.Id) != null))
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
            if (UnspentOutputs.Shift())
            if (GetTransactions().FlatMap(Tx => Tx.Inputs).Any(tx.ContainsInput))
            {
                return false;
            }

            // Referenced output check
            if (!tx.Inputs.All(input => Transactions.Get(input.Transaction).Outputs[input.Index].Corresponds(input)))
            {
                return false;
            }

            return true;
        }

        private DirectoryInfo DirectoryInfo()
        {
            return new DirectoryInfo(BlockchainDirectory);
        }

        private void WriteBlock(Block Block)
        {
            File.WriteAllBytes(Path.Combine(BlockchainDirectory, $"{Block.Index}.block"), Serializer.Serialize(Block));
        }

        private Block ReadBlock(int index)
        {
            try
            {
                return Serializer.Deserialize<Block>(File.ReadAllBytes(Path.Combine(BlockchainDirectory, $"{index}.block")));
            }
            catch
            {
                return null;
            }
        }

        private void DeleteBlock(Block Block)
        {
            File.Delete(Path.Combine(BlockchainDirectory, $"{Block.Index}.block"));
        }

        public int Size()
        {
            return DirectoryInfo().GetFiles().Filter(i => i.Name.Contains(".block")).Count();
        }
    }
}
