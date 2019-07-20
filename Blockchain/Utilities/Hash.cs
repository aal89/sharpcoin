using System;
using System.Security.Cryptography;
using System.Text;

namespace Blockchain.Utilities
{
    public static class Hash
    {
        public static string Sha256(string data)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();

                Array.ForEach(bytes, (Byte Byte) => builder.Append(Byte.ToString("x2")));

                return builder.ToString();
            }
        }
    }
}
