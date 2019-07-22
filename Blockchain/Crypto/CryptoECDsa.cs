using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace Blockchain.Utilities
{
    public static class CryptoECDsa
    {
        public static SharpKeyPair GenerateKeyPair()
        {
            // Curve ECDSA_P256 (weaker curve, but smaller keys)
            using (ECDsa dsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
            {
                ECParameters Params = dsa.ExportParameters(true);
                return new SharpKeyPair(Params.Q.X.Concat(Params.Q.Y).ToArray(), Params.D);
            }
        }

        public static byte[] Sign(byte[] PrivateKey, byte[] data)
        {
            
            using (ECDsa dsa = ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                D = PrivateKey,
                Q = new ECPoint
                {
                    X = new byte[PrivateKey.Length],
                    Y = new byte[PrivateKey.Length]
                }
            }))
            {
                return dsa.SignData(data, HashAlgorithmName.SHA256);
            }
        }

        public static string Sign(SharpKeyPair KeyPair, string data)
        {
            if (!KeyPair.HasPrivateKey())
            {
                throw new MissingFieldException("Missing private key.");
            }
            return Hex.To(Sign(KeyPair.PrivateKey, Encoding.UTF8.GetBytes(data)));
        }

        public static bool Verify((byte[] X, byte[] Y) PublicKey, byte[] Signature, byte[] Message)
        {
            
            using (ECDsa dsa = ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                D = new byte[PublicKey.X.Length],
                Q = new ECPoint
                {
                    X = PublicKey.X,
                    Y = PublicKey.Y
                }
            }))
            {
                if (dsa.VerifyData(Message, Signature, HashAlgorithmName.SHA256))
                    return true;
                return false;
            }
        }

        public static bool Verify(SharpKeyPair KeyPair, string signatureHex, string Message)
        {
            if (!KeyPair.HasPublicKey())
            {
                throw new MissingFieldException("Missing public key.");
            }
            return Verify(KeyPair.GetPublicKey(), Hex.From(signatureHex), Encoding.UTF8.GetBytes(Message));
        }
    }
}
