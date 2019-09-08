using System.Collections.Generic;
using System.IO;
using Core.Transactions;
using Core.Utilities;
using System.Linq;

namespace Core.Indexes
{
    // Essentially just a list of Outputs, but then with file operations
    public class UnspentOutputs: Index<Output>
    {
        private readonly string DataDirectory;
        private readonly Serializer serializer = new Serializer();

        public UnspentOutputs(string DataDirectory)
        {
            this.DataDirectory = DataDirectory;

            if (!File.Exists(FilePath()))
                File.Create(FilePath()).Dispose();
        }

        public override Output Get(Output Id)
        {
            return Find(output => output.Address == Id.Address && output.Amount == Id.Amount);
        }

        public Output[] All(string Address)
        {
            return this.Filter(output => output.Address == Address).ToArray();
        }

        public override string FilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), DataDirectory, "utxo.index");
        }

        public override void Save()
        {
            File.WriteAllBytes(FilePath(), serializer.Serialize(this));
        }

        public override void Read()
        {
            // Todo: Admittedly the Index abstraction is quite leaky... The List<Output> detail shouldn't really be here.
            List<Output> utxo = serializer.Deserialize<List<Output>>(File.ReadAllBytes(FilePath()));

            if (utxo != null)
                foreach (Output output in utxo)
                    Add(output);
        }
    }
}
