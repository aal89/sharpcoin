using System;
using System.Numerics;

namespace Core.Utilities
{
    // All the reducers in one place
    public static class R
    {
        public static int Total(int Accumulator, int Current) => Accumulator + Current;
        public static ulong Total(ulong Accumulator, ulong Current) => Accumulator + Current;
        public static long Total(long Accumulator, long Current) => Accumulator + Current;
        public static BigInteger Total(BigInteger Accumulator, BigInteger Current) => Accumulator + Current;
        public static Func<string, string, string> Concat(string Delimiter = "") => (string Accumulator, string Current) => $"{Accumulator}{Delimiter}{Current}";
        public static ulong Lowest(ulong Accumulator, ulong Current) => Current < Accumulator ? Current : Accumulator;
        public static ulong Highest(ulong Accumulator, ulong Current) => Current > Accumulator ? Current : Accumulator;
    }
}
