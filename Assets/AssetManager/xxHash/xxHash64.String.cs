using System.Threading;
using System.Text;

namespace YH.xxHash
{
    public static partial class xxHash64
    {
        private const int _StringBufferSize = 1024;
        private static ThreadLocal<byte[]> _StringBuffer=new ThreadLocal<byte[]>();
        public static ulong ComputeHash(string s)
        {
            return ComputeHash(s, Encoding.UTF8, 0);
        }

        public static ulong ComputeHash(string s, Encoding encoding, ulong seed = 0)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            if (!_StringBuffer.IsValueCreated)
            {
                _StringBuffer.Value = new byte[_StringBufferSize];
            }
            byte[] buff = _StringBuffer.Value;
            int byteLenth = encoding.GetBytes(s, 0, s.Length, buff, 0);
            return ComputeHash(buff, byteLenth, seed);
        }

        public unsafe static ulong ComputeHashStr(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            fixed (char* ps = s)
            {
                return ComputeHash((byte*)ps, s.Length * 2, 0);
            }
        }
    }
}