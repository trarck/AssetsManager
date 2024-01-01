using System.Threading;
using System.IO;
using System.Diagnostics;

namespace YH.AssetManage.xxHash
{
    public static partial class xxHash32
    {
        private const int _StreamBufferSize = 4096;
        private static ThreadLocal<byte[]> _StreamBuffer = new ThreadLocal<byte[]>();

        /// <summary>
        /// Compute xxHash for the stream
        /// </summary>
        /// <param name="stream">The stream of data</param>
        /// <param name="seed">The seed number</param>
        /// <returns>The hash</returns>
        public static uint ComputeHash(Stream stream, uint seed = 0)
        {
            Debug.Assert(stream != null);
            if (!_StreamBuffer.IsValueCreated)
            {
                _StreamBuffer.Value = new byte[_StreamBufferSize + 32];
            }

            byte[] buffer = _StreamBuffer.Value;

            int readBytes;
            int offset = 0;
            long length = 0;

            // Prepare the seed vector
            uint v1 = seed + p1 + p2;
            uint v2 = seed + p2;
            uint v3 = seed + 0;
            uint v4 = seed - p1;

            // Read flow of bytes
            while ((readBytes = stream.Read(buffer, offset, _StreamBufferSize)) > 0)
            {
                length = length + readBytes;
                offset = offset + readBytes;

                if (offset < 16) continue;

                int r = offset % 16; // remain
                int l = offset - r;  // length

                // Process the next chunk 
                UnsafeAlign(buffer, l, ref v1, ref v2, ref v3, ref v4);

                // Put remaining bytes to buffer
                UnsafeBuffer.BlockCopy(buffer, l, buffer, 0, r);
                offset = r;
            }

            // Process the last chunk
            uint h32 = UnsafeFinal(buffer, offset, ref v1, ref v2, ref v3, ref v4, length, seed);

            return h32;
        }
    }
}