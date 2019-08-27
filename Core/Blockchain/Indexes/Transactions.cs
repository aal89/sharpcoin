using System.Linq;
using System.Collections.Generic;
using Core.Transactions;
using Core.Utilities;
using System.IO;

namespace Core.Blockchain.Indexes
{
    public class Transactions: Dictionary<string, int>
    {
        private readonly string BlockchainDirectory;
        private readonly Blockchain Blockchain;
        private readonly Serializer serializer = new Serializer();

        public Transactions(Blockchain Blockchain, string BlockchainDirectory)
        {
            this.BlockchainDirectory = BlockchainDirectory;
            this.Blockchain = Blockchain;
        }

        public Transaction GetTransactionById(string Id)
        {
            return Blockchain.GetBlockByIndex(this[Id]).GetTransactions().Filter(tx => tx.Id == Id).FirstOrDefault();
        }

        public new void Add(string key, int value)
        {
            base.Add(key, value);
            Save();
        }

        private void Save()
        {
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), BlockchainDirectory), serializer.Serialize(this));
        }
    }
}
