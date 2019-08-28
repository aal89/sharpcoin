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
        private readonly Block Genesis = new GenesisBlock();

        private readonly HashSet<Transaction> QueuedTransactions = new HashSet<Transaction>(new TransactionComparer());
        private readonly Serializer Serializer = new Serializer();

        private readonly string BlockchainDirectory = Path.Combine(Directory.GetCurrentDirectory(), Config.BlockchainDirectory);

        public event EventHandler BlockAdded;
        public event EventHandler QueuedTransactionAdded;

        public Blockchain()
        {
            if (!File.Exists(BlockchainDirectory))
                Directory.CreateDirectory(BlockchainDirectory);

            Validate();
        }

        public bool Validate()
        {
            for (var i = 1; i < Size(); i++)
                IsValidBlock(ReadBlock(i), i == 1 ? Genesis : ReadBlock(i - 1));

            return true;
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

        public Transaction GetTransactionFromChain(string Id)
        {
            return null;
            //return Collection.FlatMap(Block => Block.GetTransactions()).Filter(Tx => Tx.Id == Id).FirstOrDefault();
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
                IsValidBlock(Block, PreviousBlock);
                WriteBlock(Block);

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
            // todo: keep index of all unspent inputs
            // if(!tx.Inputs.All(in => utxos.Contains(in)))
            //      do-shit
            //if (GetTransactions().FlatMap(Tx => Tx.Inputs).Any(tx.ContainsInput))
            //{
            //    return false;
            //}

            // Referenced output check
            // todo: keep index of all txs
            if (!tx.Inputs.All(input => GetTransactionFromChain(input.Transaction).Outputs[input.Index].Corresponds(input)))
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
