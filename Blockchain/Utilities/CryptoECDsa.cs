using System;
using System.Text;
using System.Security.Cryptography;

namespace Blockchain.Utilities
{
    public static class CryptoECDsa
    {
        public static byte[] Sign(byte[] data)
        {
            using (ECDsa dsa = ECDsa.Create())
            {
                //var key = dsa.ExportParameters(false);
                //byte[] signature = dsa.SignData(data, HashAlgorithmName.SHA256);
                return dsa.SignData(data, HashAlgorithmName.SHA256);
            }
        }

        public static string Sign(string data)
        {
            return Hex.To(Sign(Encoding.UTF8.GetBytes(data)));
        }
    }
}
