using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Core.Exceptions;
using Core.Transactions;
using Core.Utilities;
using Core.Crypto;

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

            Log.NewLine($"Loading tx index.");
            Transactions = new Indexes.Transactions(this, Config.BlockchainDirectory);
            Transactions.Read();

            Log.NewLine($"Loading utxo index.");
            UnspentOutputs = new Indexes.UnspentOutputs(Config.BlockchainDirectory);
            UnspentOutputs.Read();

            Log.NewLine($"Validating blockchain.");
            Validate();
            Log.NewLine($"Valid!");
        }

        public long Balance(SharpKeyPair skp)
        {
            return UnspentOutputs.Filter(output => output.Address == skp.GetAddress()).Map(output => output.Amount).Reduce(R.Total, 0);
        }

        public void Validate()
        {
            // In order to validate we have to continuously build up the index as it was written at the time
            // of the block mined was added to the chain. So we clear the indexes if we have any loaded.
            // As an addition we also save this index when the last block got processed.
            ClearIndexes();

            // Keep track of the blockchain size.
            int BcSize = Size();

            for (var i = 1; i < BcSize; i++)
            {
                // Get the block to be checked
                Block CurrentBlock = ReadBlock(i);
                // Is it valid?
                IsValidBlock(CurrentBlock, i == 1 ? Genesis : ReadBlock(i - 1));
                // If so create the indexes for that block, but save them only at the last iteration processed.
                CreateIndexes(CurrentBlock, i == BcSize - 1);
                Log.NewLine($"Block {i}/{BcSize - 1} is valid!");
            }
        }

        // kind of obscure naming, but the blockchain is split up in parts of x
        // blocks this is used to recalculate the diff.
        public Block[] GetLastSection(Block Perspective = null)
        {
            int BlockchainSize = Perspective != null ? Perspective.Index : Size();

            if (BlockchainSize < Config.SectionSize)
                return null;

            int PreviousSectionIndex = Math.Max(0, BlockchainSize - (BlockchainSize % Config.SectionSize) - Config.SectionSize);
            Block[] BlockSection = new Block[Config.SectionSize];

            for (int i = PreviousSectionIndex; i < PreviousSectionIndex + Config.SectionSize; i++)
                BlockSection[i - PreviousSectionIndex] = GetBlockByIndex(i);

            return BlockSection;
        }

        public Block GetBlockByIndex(int Index)
        {
            return ReadBlock(Index) ?? Genesis;
        }

        public Block GetLastBlock()
        {
            return ReadBlock(Size() - 1) ?? Genesis;
        }

        public bool QueueTransaction(Transaction tx)
        {
            if (IsValidTransaction(tx) && tx.Verify() && QueuedTransactions.Add(tx))
            {
                // Fire the tx added event
                QueuedTransactionAdded?.Invoke(tx, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public Transaction[] GetQueuedTransactions()
        {
            return QueuedTransactions.ToArray();
        }

        public Transaction GetQueuedTransactionById(string Id)
        {
            return QueuedTransactions.ToList().Find(Tx => Tx.Id == Id);
        }

        private readonly static object removeblock_operation = new object();
        public void RemoveBlock(Block Block)
        {
            lock (removeblock_operation)
            {
                RemoveIndexes(Block);
                DeleteBlock(Block);
            }
        }

        private readonly static object addblock_operation = new object();
        public void AddBlock(Block Block, Block PreviousBlock = null, bool TriggerEvent = true)
        {
            lock (addblock_operation)
            {
                // Check block
                IsValidBlock(Block, PreviousBlock);

                // Write block out to disk
                WriteBlock(Block);

                // Remove all queued transactions that got included in the valid block.
                QueuedTransactions.RemoveWhere(tx => Block.GetTransactions().Any(btx => btx.Id == tx.Id));

                // Creates indexes for this newly added block.
                CreateIndexes(Block);

                if (TriggerEvent)
                    BlockAdded?.Invoke(Block, EventArgs.Empty);
            }
        }

        private void ClearIndexes()
        {
            if (Transactions != null && UnspentOutputs != null)
            {
                Transactions.Clear();
                UnspentOutputs.Clear();
            }
        }

        private void CreateIndexes(Block Block, bool Save = true)
        {
            // 1st part is easy, just keep track of all the new txs this block generates

            // Create indexes for all the txs on the given block.
            foreach (Transaction tx in Block.GetTransactions())
                Transactions.Add(tx.Id, Block.Index);

            // 2nd part requires some understanding; first we remove all the (unspent) outputs on this block
            // that were used as inputs and remove them from the utxo index.

            // Get all the inputs on this block and remove them as being an output.
            foreach (Input input in Block.GetTransactions().FlatMap(tx => tx.Inputs))
                UnspentOutputs.Remove(input.AsOutput());

            // Then, we keep track of the newly generated outputs that those inputs created.
            foreach(Transaction tx in Block.GetTransactions())
                foreach ((Output output, int index) in tx.Outputs.Track())
                    UnspentOutputs.Add(new MetaOutput {
                        Address = output.Address,
                        Amount = output.Amount,
                        Transaction = tx.Id,
                        Index = index
                    });

            if (Save)
            {
                Transactions.Save();
                UnspentOutputs.Save();
            }
        }

        public void RemoveIndexes(Block Block)
        {
            foreach (Transaction tx in Block.GetTransactions())
                Transactions.Remove(tx.Id);

            foreach (Output output in Block.GetTransactions().FlatMap(tx => tx.Outputs))
                UnspentOutputs.Remove(output);

            Transactions.Save();
            UnspentOutputs.Save();
        }

        public bool IsValidBlock(Block NewBlock, Block PreviousBlock = null)
        {
            if (PreviousBlock == null)
                PreviousBlock = GetLastBlock();

            if (NewBlock == null)
                throw new BlockAssertion(NewBlock, $"New or previous block is null.");

            if (NewBlock.Timestamp.Subtract(PreviousBlock.Timestamp).TotalSeconds < 0)
            {
                throw new BlockAssertion(NewBlock, $"The new block is older than the last block. Timestamp last block {PreviousBlock.Timestamp}, timestamp new block {NewBlock.Timestamp}.");
            }

            if (NewBlock.Index != PreviousBlock.Index + 1)
            {
                throw new BlockAssertion(NewBlock, $"Not consecutive blocks. Expected new block index to be {PreviousBlock.Index + 1}, but got {NewBlock.Index}.");
            }

            if (NewBlock.PreviousHash != PreviousBlock.Hash)
            {
                throw new BlockAssertion(NewBlock, $"New block points to a different block. Previous hash of new block is {NewBlock.PreviousHash}, while hash of last block is {PreviousBlock.Hash}.");
            }

            if (NewBlock.Hash != NewBlock.ToHash())
            {
                throw new BlockAssertion(NewBlock, $"New blocks integrity check failed. Is {NewBlock.Hash}, should be: {NewBlock.ToHash()}");
            }

            string CorrectTarget = NewBlock.Index % Config.SectionSize == 0 ? Config.CalculateDifficulty(GetLastSection(NewBlock)).ToString("x") : PreviousBlock.TargetHash;
            if (NewBlock.TargetHash != CorrectTarget)
            {
                throw new BlockAssertion(NewBlock, $"New blocks target difficulty is wrong. Target hash: {NewBlock.TargetHash}, should be {CorrectTarget}.");
            }

            if (!NewBlock.IsCorrectDifficulty())
            {
                throw new BlockAssertion(NewBlock, $"Expected the difficulty of the new block ({NewBlock.GetPrettyDifficulty()}) to be less than the target difficulty ({NewBlock.GetPrettyTargetDifficulty()}).");
            }

            if (!NewBlock.HasTransactions())
            {
                throw new BlockAssertion(NewBlock, $"New block does not have any transactions.");
            }

            if (Serializer.Size(NewBlock) > Config.MaximumBlockSizeInBytes)
            {
                throw new BlockAssertion(NewBlock, $"New Block size (in bytes) is {Serializer.Size(NewBlock)} and the maximum is {Config.MaximumBlockSizeInBytes}.");
            }

            if (!NewBlock.HasRewardTransaction())
            {
                throw new BlockAssertion(NewBlock, $"New block does not have a reward transaction.");
            }

            if (!NewBlock.GetRewardTransaction().IsRewardTransaction(Config.BlockReward) || !NewBlock.GetRewardTransaction().Verify())
            {
                throw new BlockAssertion(NewBlock, $"New block does not have a valid reward transaction.");
            }

            // Somewhat more expensive operations

            if (NewBlock.GetTransactions().Any(Tx => Transactions.Get(Tx.Id) != null))
            {
                throw new BlockAssertion(NewBlock, $"New block contains duplicate transactions.");
            }

            if (NewBlock.GetTransactions().FlatMap(tx => tx.Inputs).ContainsDuplicates())
            {
                throw new BlockAssertion(NewBlock, $"New block contains duplicate inputs.");
            }

            if (!NewBlock.GetTransactions().Filter(RTx => RTx.Type != Transaction.TransactionType.REWARD).All(Tx => Tx.Equates() && Tx.Verify()))
            {
                throw new BlockAssertion(NewBlock, $"New block contains invalid transaction (inputs do not equate with outputs or signature invalid).");
            }

            // Double spending inputs check and validate referenced outputs.

            if(!NewBlock.GetTransactions().All(IsValidTransaction))
            {
                throw new BlockAssertion(NewBlock, $"New block tries to spend already spent inputs or the referenced outputs in the inputs are invalid.");
            }

            return true;
        }

        // Verifies the transaction does not try to double spend and all the inputs are correctly referenced
        public bool IsValidTransaction(Transaction tx)
        {
            // Double spend check
            if (tx.Inputs.Any(input => UnspentOutputs.Get(input.AsOutput()) == null))
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

        public Output[] GetUnspentOutputs(string Address)
        {
            return UnspentOutputs.All(Address);
        }

        public Transaction GetTransaction(string Id)
        {
            return Transactions.Get(Id);
        }

        public int Size()
        {
            // +1 for the genesisblock that is hardcoded instead of saved to disk
            return DirectoryInfo().GetFiles().Filter(i => i.Name.Contains(".block")).Count() + 1;
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
            if (Block != null)
                File.Delete(Path.Combine(BlockchainDirectory, $"{Block.Index}.block"));
        }
    }
}
