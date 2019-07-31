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
            Hash = "00000042158c047307b29b799f7513225a557841a1cc19a504755ddb97840e2a";
            Timestamp = new DateTime(2019, 07, 31, 08, 09, 45);
            Nonce = 7831440;

            Transaction RTx = new Transaction("1e7ffd72617efb7a301625302e396ebc813ad76a");
            Output utxo = new Output
            {
                Address = "sb053596e0f3a8572833b5605bc62e4252324b9eb",
                Amount = 50000000000
            };
            RTx.Outputs = new Output[1] { utxo };
            RTx.Type = Transaction.TransactionType.REWARD;
            RTx.Signature = new SharpKeyPair.Signature(new byte[] {
                19, 180, 45, 64, 148, 63, 85, 129, 61, 251, 187, 10, 218, 221, 72, 231, 172, 219, 154, 226, 49, 217, 76, 211, 72, 86, 77, 204, 4, 102, 135, 28, 201, 48, 200, 120, 87, 118, 200, 196, 212, 198, 43, 142, 153, 23, 52, 54, 255, 251, 250, 163, 92, 20, 2, 244, 162, 110, 177, 212, 49, 53, 7, 249
            }, "2307dc492f376e8600ef5b3d92f29260e2bb8949892876acf0d3049bdd9cb3b12681be4c68ffe8b4eb5bf02511eb61b980549a2e2b7df458ff5faf13950cb99c");

            Transactions.Add(RTx);
        }
    }
}
