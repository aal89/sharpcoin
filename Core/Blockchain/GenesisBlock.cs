using System;
using Core.Crypto;
using Core.Transactions;

namespace Core
{
    public class GenesisBlock: Block
    {
        public GenesisBlock()
        {
            Index = 0;
            PreviousHash = "";
            Hash = "0000003674f6a2b8ac9e577ae1795f34c1badf5d7ac017c8d087c0c3ac1b7289";
            Timestamp = new DateTime(2019, 09, 20, 20, 46, 50);
            Nonce = 27164049;

            Transaction RTx = new Transaction(new Output[] { new Output {
                Address = "saf0219b5627144c17db605f86615978c78f3e6e2",
                Amount = 50000000000
            } }, "008034d8886af3204d82deded948715a90951af2");

            RTx.Signature = new SharpKeyPair.Signature(new byte[] {
                218, 87, 24, 66, 102, 185, 161, 182, 67, 17, 198, 32, 90, 237, 254, 176, 172, 187, 171, 200, 218, 220, 109, 45, 1, 31, 24, 141, 139, 230, 2, 73, 81, 170, 216, 34, 157, 185, 152, 249, 104, 191, 53, 17, 27, 180, 248, 39, 16, 133, 92, 220, 164, 42, 192, 146, 56, 113, 103, 224, 246, 190, 124, 11
            }, "edaa2ce5e300e6340818842f68ac0ebd5eefe10f8266e7d2b486256e33167ebbe31baa8850fcbb10adac483ca071e525343746cc3e483a500a4bea53880bdebe");

            AddTransaction(RTx);
        }
    }
}
