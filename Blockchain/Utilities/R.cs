namespace Blockchain.Utilities
{
    // All the reducers in one place
    public static class R
    {
        public static int Total(int Accumulator, int Current) => Accumulator + Current;
        public static ulong Total(ulong Accumulator, ulong Current) => Accumulator + Current;
        public static string Concat(string Accumulator, string Current) => $"{Accumulator}{Current}";
    }
}
