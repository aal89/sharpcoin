using System.Linq;
using System.Collections.Generic;
using Core.Transactions;
using Core.Utilities;
using System.IO;

namespace Core.Indexes
{
    // Index<Transaction, string, int> = The index is for Transaction objects and we find them by the Id field which is a string, what it maps to is the block index which is an int.
    public class Transactions: Index<Transaction, string, int>
    {
        private readonly string DataDirectory;
        private readonly Blockchain Blockchain;
        private readonly Serializer serializer = new Serializer();

        public Transactions(Blockchain Blockchain, string DataDirectory)
        {
            this.DataDirectory = DataDirectory;
            this.Blockchain = Blockchain;
        }

        public override Transaction Get(string Id)
        {
            return Blockchain.GetBlockByIndex(this[Id]).GetTransactions().Filter(tx => tx.Id == Id).FirstOrDefault();
        }

        public override string FilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), DataDirectory, "txs.index");
        }

        public override void Save()
        {
            File.WriteAllBytes(FilePath(), serializer.Serialize(this));
        }

        public override void Read()
        {
            Transactions txIndex = serializer.Deserialize<Transactions>(File.ReadAllBytes(FilePath()));

            foreach (KeyValuePair<string, int> entry in txIndex)
            {
                Add(entry.Key, entry.Value);
            }
        }
    }
}
