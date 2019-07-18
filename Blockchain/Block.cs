using System;

namespace Blockchain
{
    public class Block
    {
        public int Index = 0;
        public string Hash = "000000000fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";
        public DateTime Timestamp = new DateTime();

        public Block()
        {
        }

        public ulong GetDifficulty()
        {
            return Convert.ToUInt64(Hash.Substring(0, 16), 16);
        }
    }
}
