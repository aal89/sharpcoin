using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Blockchain.Utilities
{
    public class Serializer
    {
        public Serializer()
        {
        }

        public static void Write(object o, string path)
        {
            using (Stream s = new FileStream("./temp.dat", FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
            }

        }

        public static long GetSerializedSize(object o)
        {
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
                return s.Length;
            }

        }
    }
}
