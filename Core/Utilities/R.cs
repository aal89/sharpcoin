namespace Core.Utilities
{
    // All the reducers in one place
    public static class R
    {
        public static int Total(int Accumulator, int Current) => Accumulator + Current;
        public static ulong Total(ulong Accumulator, ulong Current) => Accumulator + Current;
        public static long Total(long Accumulator, long Current) => Accumulator + Current;
        public static string Concat(string Accumulator, string Current) => $"{Accumulator}{Current}";
        public static string ConcatCommaDelimited(string Accumulator, string Current) => $"{Accumulator},{Current}";
        public static ulong Lowest(ulong Accumulator, ulong Current) => Current < Accumulator ? Accumulator = Current : Accumulator;
        public static ulong Highest(ulong Accumulator, ulong Current) => Current > Accumulator ? Accumulator = Current : Accumulator;
    }
}
