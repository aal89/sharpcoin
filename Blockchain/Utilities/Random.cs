using System;
namespace Blockchain.Utilities
{
    public static class Random
    {
        public static byte[] Bytes()
        {
            System.Random rand = new System.Random();

            Byte[] b = new Byte[32];

            rand.NextBytes(b);

            return b;
        }
    }
}
