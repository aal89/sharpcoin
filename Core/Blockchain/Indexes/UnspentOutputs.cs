using System.IO;
using Core.Transactions;
using Core.Utilities;

namespace Core.Indexes
{
    public class UnspentOutputs: Index<Output>
    {
        private readonly string DataDirectory;
        private readonly Serializer serializer = new Serializer();

        public UnspentOutputs(string DataDirectory)
        {
            this.DataDirectory = DataDirectory;
        }

        public override Output Get(Output Id)
        {
            return Find(output => output.Address == Id.Address && output.Amount == Id.Amount);
        }

        public Output Shift(Output Id)
        {
            if (Get(Id) != null)
            {
                int index = FindIndex(output => output.Address == Id.Address && output.Amount == Id.Amount);
                Output o = this[index];
                RemoveAt(index);
                return o;
            }
            return null;
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
            UnspentOutputs utxo = serializer.Deserialize<UnspentOutputs>(File.ReadAllBytes(FilePath()));

            foreach (Output output in utxo)
            {
                Add(output);
            }
        }
    }
}
