using System;
using System.Linq;

namespace Blockchain
{
    public class Blockchain
    {
        public enum Order
        {
            FIRST, LAST
        }

        private Block[] collection = null;

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
            collection = new Block[] { test, test2, test3, test4, test5 };
        }

        public Block[] GetBlocks()
        {
            return collection;
        }

        public Block[] GetBlocks(int n, Order take = Order.FIRST)
        {
            if (take == Order.FIRST)
            {
                return collection.Take(n).ToArray();
            }
            return Enumerable.Reverse(collection).Take(n).Reverse().ToArray();
        }

        public Block GetBlockByIndex(int Index)
        {
            return Array.Find(collection, (Block block) => block.Index == Index);
        }

        public Block GetBlockByHash(string Hash)
        {
            return Array.Find(collection, (Block block) => block.Hash == Hash);
        }

        public Block GetLastBlock()
        {
            return collection[collection.Length - 1];
        }

        public ulong GetDifficulty()
        {
            return Config.CalculateDifficulty(this);
        }

        static void Main(string[] args)
        {
            Blockchain bc = new Blockchain("");
            Console.WriteLine(bc.GetDifficulty());
        }
    }
}
