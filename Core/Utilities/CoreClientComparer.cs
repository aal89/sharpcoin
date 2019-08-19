using System;
using System.Collections.Generic;
using Core.TCP;

namespace Core.Utilities
{
    public class CoreClientComparer : IEqualityComparer<CoreClient>
    {
        public bool Equals(CoreClient c1, CoreClient c2)
        {
            if (c1 == null && c2 == null) { return true; }
            if (c1 == null | c2 == null) { return false; }
            if (c1.server == c2.server) { return true; }
            return false;
        }
        public int GetHashCode(CoreClient c)
        {
            return c.server.GetHashCode();
        }
    }
}
