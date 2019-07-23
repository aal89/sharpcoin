using System;
using System.Collections.Generic;

namespace Blockchain
{
    public class GenesisBlock: Block
    {
        public GenesisBlock()
        {
            Index = 0;
            PreviousHash = "";
            Hash = "000cfbe3bc2d82fe552bbde4e4883f262838a5dd0c7fbcef9bb106ee3dac8962";
            Timestamp = new DateTime(2019, 07, 23, 21, 07, 33);
            Nonce = 1131;
            Transactions = new List<Transaction>();
        }
    }
}
