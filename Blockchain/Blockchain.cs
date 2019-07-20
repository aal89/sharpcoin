using System;
using System.Linq;
using Blockchain.Exceptions;
using Blockchain.Utilities;

namespace Blockchain
{
    public class Blockchain
    {
        public enum Order
        {
            FIRST, LAST
        }

        private int MaximumBlockSizeInBytes = 2 * 1024;

        private Block[] Collection = null;
        private Transaction[] QueuedTransactions = null;

        public Blockchain(string pathToData)
        {
            // TODO: resolve data file.
            Block test = new Block();
            test.Index = 0;
            test.Timestamp = new DateTime(2019, 07, 01, 10, 9, 0);
            Block test2 = new Block();
            test2.Index = 1;
            test2.Timestamp = new DateTime(2019, 07, 01, 10, 10, 0);
            Block test3 = new Block();
            test3.Index = 2;
            test3.Timestamp = new DateTime(2019, 07, 01, 10, 12, 0);
            Block test4 = new Block();
            test4.Index = 3;
            test4.Timestamp = new DateTime(2019, 07, 01, 10, 19, 0);
            Block test5 = new Block();
            test5.Index = 4;
            test5.Timestamp = new DateTime(2019, 07, 01, 10, 40, 0);
            //test5.Transactions = new Transaction[] { new Transaction() };
            Collection = new Block[] { test, test2, test3, test4, test5 };
        }

        public Block[] GetBlocks()
        {
            return Collection;
        }

        public Block[] GetBlocks(int n, Order take = Order.FIRST)
        {
            if (take == Order.FIRST)
            {
                return Collection.Take(n).ToArray();
            }
            return Enumerable.Reverse(Collection).Take(n).Reverse().ToArray();
        }

        public Block GetBlockByIndex(int Index)
        {
            return Array.Find(Collection, (Block Block) => Block.Index == Index);
        }

        public Block GetBlockByHash(string Hash)
        {
            return Array.Find(Collection, (Block Block) => Block.Hash == Hash);
        }

        public Block GetLastBlock()
        {
            return Collection[Collection.Length - 1];
        }

        public ulong GetDifficulty()
        {
            return Config.CalculateDifficulty(this);
        }

        public Transaction[] GetQueuedTransactions()
        {
            return QueuedTransactions;
        }

        public Transaction GetQueuedTransactionById(string Id)
        {
            return Array.Find(QueuedTransactions, (Transaction Transaction) => Transaction.Id == Id);
        }

        public Transaction GetTransactionFromChain(string Id)
        {
            return Collection.Map((Block Block) => Block.Transactions)
                .SelectMany(x => x)
                .Filter((Transaction Transaction) => Transaction.Id == Id).FirstOrDefault();
        }

        public bool IsValidBlock(Block NewBlock, Block LastBlock)
        {
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
                throw new BlockAssertion($"Not consecutive blocks. Expected new block index to be  {LastBlock.Index + 1}, but got {NewBlock.Index}.");
            }

            if (NewBlock.PreviousHash == LastBlock.Hash)
            {
                throw new BlockAssertion($"New block points to a different block. Previous hash of new block is {NewBlock.PreviousHash}, while hash of last block is {LastBlock.Hash}.");
            }

            if (NewBlock.Hash != NewBlock.ToHash())
            {
                throw new BlockAssertion($"New blocks integrity check failed.");
            }

            if (NewBlock.GetDifficulty() >= GetDifficulty())
            {
                throw new BlockAssertion($"Expected the difficulty of the new block ({NewBlock.GetDifficulty()}) to be less than the current difficulty ({GetDifficulty()}).");
            }

            if (!NewBlock.HasTransactions())
            {
                throw new BlockAssertion($"New block does not have any transactions.");
            }

            if (Serializer.GetSerializedSize(NewBlock) > MaximumBlockSizeInBytes)
            {
                throw new BlockAssertion($"New Block size (in bytes) is {Serializer.GetSerializedSize(NewBlock)} and the maximum is {MaximumBlockSizeInBytes}.");
            }

            if (!NewBlock.GotFeeRewardTransactions())
            {
                throw new BlockAssertion($"New block does not have a fee and reward transaction.");
            }

            // Somewhat more expensive operations

            if (NewBlock.Transactions.Any((Transaction Transaction) => GetTransactionFromChain(Transaction.Id) != null))
            {
                throw new BlockAssertion($"New block contains duplicate transactions.");
            }

            if (!NewBlock.Transactions.All((Transaction Transaction) => Transaction.IsValid()))
            {
                throw new BlockAssertion($"New block contains invalid transaction.");
            }


            return true;
        }

        static void Main(string[] args)
        {
            Blockchain Bc = new Blockchain("");
            //Console.WriteLine(Serializer.GetSerializedSize(Bc.GetLastBlock()));
            Serializer.Write(Bc.GetLastBlock(), "");
        }
    }
}
