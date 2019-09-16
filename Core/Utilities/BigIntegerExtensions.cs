using System;
using System.Numerics;

namespace Core.Utilities
{
    public static class BigIntegerExtensions
    {
        public static BigInteger Percentage(this BigInteger self, float num)
        {
            return self + ((self / 100) * (int)(num * 100));
        }
    }
}
