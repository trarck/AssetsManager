using System.Text;

namespace YH.Hash.xxHash
{
    public static partial class xxHash32
    {
        public static uint ComputeHash(string s)
        {
            return ComputeHash(s, Encoding.UTF8, 0);
        }

        public static uint ComputeHash(string s, Encoding encoding, uint seed = 0)
        {
            byte[] buff = encoding.GetBytes(s);
            return ComputeHash(buff, buff.Length, seed);
        }
    }
}