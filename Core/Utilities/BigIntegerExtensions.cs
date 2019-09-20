using System;
using System.Numerics;

namespace Core.Utilities
{
    public static class BigIntegerExtensions
    {
        public static BigInteger Percentage(this BigInteger self, float num)
        {
            return (self / 100) * (int)(num * 100);
        }

        public static int Inaccurate(this BigInteger self, BigInteger divider)
        {
            return (int)(divider / self);
        }
    }
}
