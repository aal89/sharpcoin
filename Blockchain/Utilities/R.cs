using System;
namespace Blockchain.Utilities
{
    public static class R
    {
        public static int Total(int Accumulator, int Current) => Accumulator + Current;
        public static string Concat(string Accumulator, string Current) => $"{Accumulator}{Current}";
    }
}
