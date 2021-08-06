using System.Diagnostics;
using System.Text;

namespace YH.Hash.xxHash
{
    public static partial class xxHash64
    {
        public static ulong ComputeHash(string s)
        {
            return ComputeHash(s, Encoding.UTF8, 0);
        }

        public static ulong ComputeHash(string s , Encoding encoding, ulong seed = 0)
        {

            byte[] buff = encoding.GetBytes(s);
            return ComputeHash(buff, buff.Length,seed);
        }
    }
}