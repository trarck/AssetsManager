using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManage
{
    public class AndroidAssetStream : Stream
    {
        private static jvalue[] s_InternalReadArgs = new jvalue[3];
        private static IntPtr s_ReadMethodId;

        private long _Length = 0;
        private long _Position = 0;
        private AndroidJavaObject _InternalStream;

        static AndroidAssetStream() 
        {
            IntPtr inputStreamClassPtr = AndroidJNI.FindClass("java/io/InputStream");
            s_ReadMethodId = AndroidJNIHelper.GetMethodID(inputStreamClassPtr, "read", "([BII)I");
            AndroidJNI.DeleteLocalRef(inputStreamClassPtr);
        }


        public AndroidAssetStream(AndroidJavaObject innerStream)
        {
            _InternalStream = innerStream;
            if (innerStream != null)
            {
                _Length = innerStream.Call<int>("available");
            }
        }

        public override bool CanRead
        {
            get
            {
                return _Position < _Length;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                return _Length;
            }
        }

        public override long Position
        {
            get
            {
                return _Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Close()
        {
            if (_InternalStream != null)
            {
                _InternalStream.Call("close");
                _InternalStream.Dispose();
            }
            base.Close();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readOffset = 0;
            IntPtr array = IntPtr.Zero;
            try
            {
                array = AndroidJNI.NewSByteArray(count);

                int bytesLeft = count;
                IntPtr rawObject = _InternalStream.GetRawObject();
                while (bytesLeft > 0)
                {
                    s_InternalReadArgs[0] = new jvalue() { l = array };
                    s_InternalReadArgs[1] = new jvalue() { i = readOffset };
                    s_InternalReadArgs[2] = new jvalue() { i = bytesLeft };
                    int bytesRead = AndroidJNI.CallIntMethod(rawObject, s_ReadMethodId, s_InternalReadArgs);
                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    readOffset += bytesRead;
                    bytesLeft -= bytesRead;
                }

                sbyte[] data = AndroidJNI.FromSByteArray(array);
                Buffer.BlockCopy(data, 0, buffer, offset, readOffset);
                _Position += readOffset;
            }
            finally
            {
                if (array != IntPtr.Zero)
                    AndroidJNI.DeleteLocalRef(array);
            }
            return readOffset;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
            {
                //从头开始跳转
                return Seek(Length - offset, SeekOrigin.Begin);
            }

            long skip = 0;
            if (origin == SeekOrigin.Begin)
            {
                if (offset >= _Position)
                {
                    //如果跳转位置在当前位置之后，直接跳转
                    skip = _InternalStream.Call<long>("skip", offset-Position);
                    _Position += skip;
                    return _Position;
                }
                //重置位置
                _InternalStream.Call("reset");
                _Position = 0;
            }

            while (offset > 0)
            {
                skip = _InternalStream.Call<long>("skip", offset);
                if (skip < 0)
                {
                    _Position = -1;
                    return _Position;
                }
                _Position += skip;
                offset -= skip;
            }
            return _Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
