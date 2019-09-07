using System;
using Core.Tcp;

namespace Core.Api
{
    public class ApiOperations : Operations
    {
        public ApiOperations()
        {
        }

        public override bool IsNOOP(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override bool IsOK(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] NOOP()
        {
            throw new NotImplementedException();
        }

        public override byte[] OK()
        {
            throw new NotImplementedException();
        }
    }
}
