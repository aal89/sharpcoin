using System;
using System.Linq;
using Newtonsoft.Json;
using Core.Utilities;

namespace Core.Crypto
{
    public class SharpKeyPair
    {
        public struct Signature
        {
            public readonly byte[] PublicKey;
            public readonly string Value;

            [JsonConstructor]
            public Signature(byte[] PublicKey, string Value)
            {
                this.PublicKey = PublicKey;
                this.Value = Value;
            }

            public bool Verify(string Message)
            {
                return new SharpKeyPair(PublicKey).Verify(this, Message);
            }
        }

        public readonly byte[] PublicKey;
        public readonly byte[] PrivateKey;

        public SharpKeyPair(byte[] PublicKey = null, byte[] PrivateKey = null)
        {
            this.PublicKey = PublicKey;
            this.PrivateKey = PrivateKey;
        }

        // Delegate/Helper method
        public static SharpKeyPair Create()
        {
            return CryptoECDsa.GenerateKeyPair();
        }

        public bool HasPublicKey()
        {
            return PublicKey != null;
        }

        public (byte[] X, byte[] Y) GetPublicKey()
        {
            return (PublicKey.Take(PublicKey.Length / 2).ToArray(), PublicKey.TakeLast(PublicKey.Length / 2).ToArray());
        }

        public bool HasPrivateKey()
        {
            return PrivateKey != null;
        }

        public string GetAddress()
        {
            if (!HasPublicKey())
            {
                throw new MissingFieldException("Missing public key.");
            }
            return "s" + Hash.Sha1(PublicKey);
        }

        public bool Verify(Signature Signature, string Message)
        {
            return CryptoECDsa.Verify(this, Signature.Value, Message);
        }

        public Signature Sign(string Message)
        {
            return new Signature(PublicKey, CryptoECDsa.Sign(this, Message));
        }
    }
}
