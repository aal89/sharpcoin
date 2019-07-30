using System;
using Blockchain.Transactions;
using Blockchain.Utilities;

namespace Blockchain
{
    [Serializable]
    public class GenesisBlock: Block
    {
        public GenesisBlock()
        {
            Index = 0;
            PreviousHash = "";
            Hash = "0000000182e519d235da18b539cd315a9d61a2ee2d7bbafd17883bfa18228d0a";
            Timestamp = new DateTime(2019, 07, 30, 22, 40, 16);
            Nonce = 32399984;

            Transaction RTx = new Transaction("64fbef255842974e39affa7f05bdc2133c567306");
            Output utxo = new Output
            {
                Address = "se34a76a22c22f7577486f2030c5aba036b5db416",
                Amount = 50000000000
            };
            RTx.Outputs = new Output[1] { utxo };
            RTx.Type = Transaction.TransactionType.REWARD;
            RTx.Signature = new SharpKeyPair.Signature(new byte[] {
                225, 127, 192, 208, 183, 193, 118, 61, 2, 7, 87, 51, 103, 250, 144, 58, 25, 251, 53, 222, 42, 203, 110, 178, 91, 181, 202, 72, 95, 111, 7, 138, 213, 46, 28, 178, 39, 145, 227, 173, 228, 186, 10, 195, 152, 43, 134, 166, 3, 159, 133, 215, 190, 158, 41, 240, 213, 91, 90, 26, 12, 38, 171, 181
            }, "200e6c654b4a7e939013b480072cf5b7e59712286b366f909ffff0607a4d797eca79a8f0d175c9c2b05409528d65e44dba4db62b89c7c02e854061d3fff9307d");

            Transactions.Add(RTx);
        }
    }
}
