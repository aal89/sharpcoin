using System;
using System.Collections.Generic;
using Core.Tcp;

namespace Core.Api
{
    public class ApiOperations : Operations
    {
        public new readonly Dictionary<string, byte> Codes = new Dictionary<string, byte>
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
        };

        public override byte[] OK()
        {
            return new byte[] { Codes["Ok"] };
        }

        public override byte[] NOOP()
        {
            return new byte[] { Codes["Noop"] };
        }

        public override bool IsOK(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes["Ok"];
        }

        public override bool IsNOOP(byte[] data)
        {
            return data.Length > 0 && data[0] == Codes["Noop"];
        }
    }
}
