using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Utilities
{
    public static class Hash
    {
        public static string Sha1(byte[] bytes)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                // ComputeHash - returns byte array  
                byte[] computedBytes = sha1.ComputeHash(bytes);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();

                Array.ForEach(computedBytes, (Byte Byte) => builder.Append(Byte.ToString("x2")));

                return builder.ToString();
            }
        }

        public static string Sha1(string data)
        {
            return Sha1(Encoding.UTF8.GetBytes(data));
        }

        public static string Sha256(byte[] bytes)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] computedBytes = sha256Hash.ComputeHash(bytes);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();

                Array.ForEach(computedBytes, (Byte Byte) => builder.Append(Byte.ToString("x2")));

                return builder.ToString();
            }
        }

        public static string Sha256(string data)
        {
            return Sha256(Encoding.UTF8.GetBytes(data));
        }
    }
}
