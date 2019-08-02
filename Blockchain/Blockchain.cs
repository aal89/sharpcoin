using System;
using System.Collections.Generic;
using System.Linq;
using Blockchain.Exceptions;
using Blockchain.Transactions;
using Blockchain.Utilities;

namespace Blockchain
{
    public class Blockchain
    {
        public enum Order
        {
            FIRST, LAST
        }

        private List<Block> Collection = new List<Block> { new GenesisBlock() };
        private List<Transaction> QueuedTransactions = new List<Transaction>();

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

        public void QueueTransaction(Transaction Transaction)
        {
            QueuedTransactions.Add(Transaction);
        }

        public Transaction[] GetQueuedTransactions()
        {
            return QueuedTransactions.ToArray();
        }

        public Transaction GetQueuedTransactionById(string Id)
        {
            return QueuedTransactions.Find(Tx => Tx.Id == Id);
        }

        public Transaction GetTransactionFromChain(string Id)
        {
            return Collection.FlatMap(Block => Block.GetTransactions()).Filter(Tx => Tx.Id == Id).FirstOrDefault();
        }

        public Transaction[] GetTransactions()
        {
            return Collection.FlatMap(Block => Block.GetTransactions()).ToArray();
        }

        public void AddBlock(Block Block)
        {
            IsValidBlock(Block);
            Collection.Add(Block);
            // Todo: save blockchain
        }

        public bool IsValidBlock(Block NewBlock)
        {
            Block LastBlock = GetLastBlock();
            // Before marking a block as valid we have to go through a serie of checks. Some can be quite
            // hard to graps initially but they all make perfect sense once you get them. We do the cheap
            // operations first since failing on those checks after a heavy operation is a waste of the
            // cpu cycles.
            // 1st) Check if the index is correct.
            // 2nd) Check if the previous hash is my last block hash.
            // 3rd) Check if the hash of the block is correct.
            // 4th) Check if difficulty of new block is gte to the expected difficulty.
            // 5th) Check if the block has at least one transaction (to prevent empty blocks from being
            // mined).
            // 6th) Check is block size does not exceed max block size.
            // 7th) Check if there are only one reward and one fee transaction.
            // More expensive operations:
            // 8th) The transaction is not already in the blockchain.
            // 9th) Check if all included transactions are valid.
            // 10th) The transaction has only unspent inputs.
            // 11th) The signature of the transaction is correct.
            // 12th) Check if the sum of all input transactions equal the sum of all output transactions
            // + block reward.
            // 13th) Check if there are no double spent input transactions on the block.
            // Some of these checks you can find under the transaction validation.

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

            Serializer Serializer = new Serializer();

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

            // Double spending inputs check. Get one list of all transaction inputs of the new block and check
            // if each one Input does not occur in transactions on the chain

            if (GetTransactions()
                .FlatMap(Tx => Tx.Inputs)
                .Any(Input => NewBlock.GetTransactions().Any(Tx => Tx.ContainsInput(Input.Transaction, Input.Index))))
            {
                throw new BlockAssertion($"New block tries to spend already spent transaction inputs.");
            }

            // Todo: check if all inputs actually exist for each transaction
            // Todo: check if block is not too old

            return true;
        }

        static void Main(string[] args)
        {
            Blockchain bc = new Blockchain();
            SharpKeyPair skp = SharpKeyPair.Create();

            Console.WriteLine($"Started mining at {DateTime.UtcNow}");
            while (true)
            {
                Block b = Miner.Solve(skp, bc);
                bc.AddBlock(b);
            }
        }
    }
}
