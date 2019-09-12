using System;
using System.Collections.Generic;
using Core.Tcp;

namespace Core.Api
{
    public class ApiOperations : Operations
    {
        public override Dictionary<string, byte> Codes()
        {
            return new Dictionary<string, byte>
            {
                { "Ok", 0x98 },
                { "Noop", 0x99 },
                { "Push", 0xff },
                { "RequestMining", 0x01 },
                { "RequestMiningResponse", 0x02 },
                { "RequestKeyPair", 0x03 },
                { "RequestKeyPairResponse", 0x04 },
                { "RequestBalance", 0x05 },
                { "RequestBalanceResponse", 0x06 },
                { "CreateTransaction", 0x07 },
                { "CreateTransactionResponse", 0x08 },
            };
        }
    }
}
