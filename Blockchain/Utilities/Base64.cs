using System;
using System.Text;

namespace Blockchain.Utilities
{
    public static class Base64
    {
        public static string Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        public static string Decode(string base64EncodedData)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }

    }
}
