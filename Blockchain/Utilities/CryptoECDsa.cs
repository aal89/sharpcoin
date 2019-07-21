using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace Blockchain.Utilities
{
    public static class CryptoECDsa
    {
        public struct SharpKeyPair
        {
            public readonly byte[] PublicKey;
            public readonly byte[] PrivateKey;

            public SharpKeyPair(byte[] PublicKey, byte[] PrivateKey)
            {
                this.PublicKey = PublicKey;
                this.PrivateKey = PrivateKey;
            }
        }

        public static SharpKeyPair GenerateKeyPair()
        {
            // Curve ECDSA_P521
            using (ECDsa dsa = ECDsa.Create())
            {
                ECParameters Params = dsa.ExportParameters(true);
                return new SharpKeyPair(Params.Q.X.Concat(Params.Q.Y).ToArray(), Params.D);
            }
        }

        public static string GenerateAddress(SharpKeyPair KeyPair)
        {
            return "s" + Hash.Sha1(KeyPair.PublicKey);
        }

        public static byte[] Sign(SharpKeyPair KeyPair, byte[] data)
        {
            using (ECDsa dsa = ECDsa.Create(new ECParameters
            {
                D = KeyPair.PrivateKey,
                Q = new ECPoint
                {
                    X = KeyPair.PublicKey.Take(66).ToArray(),
                    Y = KeyPair.PublicKey.TakeLast(66).ToArray()
                }
            }))
            {
                return dsa.SignData(data, HashAlgorithmName.SHA256);
            }
        }

        public static string Sign(SharpKeyPair KeyPair, string data)
        {
            return Hex.To(Sign(KeyPair, Encoding.UTF8.GetBytes(data)));
        }

        //public static bool Verify(ECParameters PublicParams, byte[] Signature, byte[] Message)
        //{
        //    using (ECDsa dsa = ECDsa.Create(PublicParams))
        //    {
        //        if (dsa.VerifyData(Message, Signature, HashAlgorithmName.SHA256))
        //            return true;
        //        return false;
        //    }
        //}

        //public static bool Verify(ECParameters PublicParams, string signatureHex, string Hash)
        //{
        //    return Verify(PublicParams, Hex.From(signatureHex), Encoding.UTF8.GetBytes(Hash));
        //}
    }
}
