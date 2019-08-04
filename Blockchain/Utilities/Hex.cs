using System;

namespace Core.Utilities
{
    public static class Hex
    {
        public static string To(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
        }

        public static byte[] From(string data)
        {
            int NumberChars = data.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
            return bytes;
        }
    }
}
