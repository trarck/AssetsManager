using System.Collections.Generic;
using System.Text;
using System;

namespace YH.AssetManage
{
    public class FixedStringPool
    {
        struct HitInfo
        {
            public int hit;
            public int miss;
        }

        private static FixedStringPool _Instance;
        private static object _LockInstanceObj = new object();

        public  int PoolMaxSize = 100; 
        private Dictionary<int,Stack<string>> _Pool;
        private Dictionary<int, HitInfo> _PoolHits;
        private object _PoolLockObj;

        private List<int> _NeedRemoveKeys;

        public void Init(int maxPoolSize=100)
        {
            _Pool = new Dictionary<int, Stack<string>>();
            _PoolHits = new Dictionary<int, HitInfo>();
            _PoolLockObj = new object();
            _NeedRemoveKeys = new List<int>();
            PoolMaxSize = maxPoolSize;
        }

        public void Dispose()
        {
            _Pool.Clear();
            _PoolHits.Clear();

            _Pool = null;
            _PoolHits = null;
        }

        public static FixedStringPool Instance
        {
            get 
            { 
                if(_Instance == null)
                {
                    lock (_LockInstanceObj)
                    {
                        //再次检查，防止同时多个线程同时引用
                        if (_Instance == null)
                        {
                            FixedStringPool instance = new FixedStringPool();
                            instance.Init();
                            _Instance= instance;
                        }
                    }
                }
                return _Instance; 
            } 
        }

        public static void DestryInstance()
        {
            if(_Instance != null)
            {
                _Instance.Dispose();
                _Instance = null;
            }
        }


        public string Get(int len)
        {
            if (len <= 0)
            {
                return string.Empty;
            }

            string str;
            Stack<string> sameLengthStringStack = null;
              
            if (_Pool.TryGetValue(len, out sameLengthStringStack) && sameLengthStringStack != null && sameLengthStringStack.Count > 0)
            {
                str = sameLengthStringStack.Pop();
                AddHit(len);
            }
            else
            {
                str = new string((char)0, len);
                AddMiss(len);
            }
            return str;
        }

        public string SafeGet(int len)
        {
            if (len <= 0)
            {
                return string.Empty;
            }

            lock (_PoolLockObj)
            {
                string str;
                Stack<string> sameLengthStringStack = null;

                if (_Pool.TryGetValue(len, out sameLengthStringStack) && sameLengthStringStack != null && sameLengthStringStack.Count > 0)
                {
                    str = sameLengthStringStack.Pop();
                    AddHit(len);
                }
                else
                {
                    str = new string((char)0, len);
                    AddMiss(len);
                }
                return str;
            }
        }

        public void Release(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            Stack<string> sameLengthStringStack = null;
            if (_Pool.TryGetValue(str.Length, out sameLengthStringStack))
            {
                if (sameLengthStringStack.Count < PoolMaxSize)
                {
                    sameLengthStringStack.Push(str);
                }
            }
            else
            {
                sameLengthStringStack = new Stack<string>();
                sameLengthStringStack.Push(str);
                _Pool.Add(str.Length,sameLengthStringStack);
            }
            
        }

        public void SafeRelease(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            lock (_PoolLockObj)
            {
                Stack<string> sameLengthStringStack = null;
                if (_Pool.TryGetValue(str.Length, out sameLengthStringStack))
                {
                    if (sameLengthStringStack.Count < PoolMaxSize)
                    {
                        sameLengthStringStack.Push(str);
                    }
                }
                else
                {
                    sameLengthStringStack = new Stack<string>();
                    sameLengthStringStack.Push(str);
                    _Pool.Add(str.Length, sameLengthStringStack);
                }
            }
        }

        public void Clean()
        {
            lock (_PoolLockObj)
            {
                if (_Pool != null)
                {
                    _Pool.Clear();
                    _Pool = null;
                }

                if (_PoolHits != null)
                {
                    _PoolHits.Clear();
                    _PoolHits = null;
                }

                if (_NeedRemoveKeys != null)
                {
                    _NeedRemoveKeys.Clear();
                    _NeedRemoveKeys = null;
                }
            }
        }

        public void Clear()
        {
            lock (_PoolLockObj)
            {
                _Pool.Clear();
                _PoolHits.Clear();
            }
        }

        public void Clear(int len)
        {
            lock (_PoolLockObj)
            {
                _Pool.Remove(len);
                _PoolHits.Remove(len);
            }
        }

        public void ClearRange(int minLen)
        {
            lock (_PoolLockObj)
            {
                foreach (var len in _Pool.Keys)
                {
                    if (minLen <= len)
                    {
                        _NeedRemoveKeys.Add(len);
                    }
                }
                foreach (var len in _NeedRemoveKeys)
                {
                    _Pool.Remove(len);
                    _PoolHits.Remove(len);
                }
                _NeedRemoveKeys.Clear();
            }
        }

        public void ClearRange(int minLen, int maxLen)
        {
            lock (_PoolLockObj)
            {
                foreach (var len in _Pool.Keys)
                {
                    if (minLen <= len && len <= maxLen)
                    {
                        _NeedRemoveKeys.Add(len);
                    }
                }
                foreach(var len in _NeedRemoveKeys)
                {
                    _Pool.Remove(len);
                    _PoolHits.Remove(len);
                }
                _NeedRemoveKeys.Clear();
            }
        }

        private void AddHit(int len)
        {
            if(_PoolHits.TryGetValue(len,out HitInfo hitInfo))
            {
                ++hitInfo.hit;
                _PoolHits[len] = hitInfo;
            }
            else
            {
                _PoolHits[len] = new HitInfo();
            }
        }

        private void AddMiss(int len)
        {
            if (_PoolHits.TryGetValue(len, out HitInfo hitInfo))
            {
                ++hitInfo.miss;
                _PoolHits[len] = hitInfo;
            }
            else
            {
                _PoolHits[len] = new HitInfo();
            }
        }

        public string Display()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("***************************************************\n");
            sb.Append("Item Count :").Append(_Pool.Count).Append("\n");
            int n = 0;
            foreach (var iter in _Pool)
            {
                sb.Append("    ")
                    .Append(iter.Key).Append(" string Count :").Append(iter.Value.Count)
                    .Append(" hit :").Append(_PoolHits[iter.Key].hit)
                    .Append(" miss :").Append(_PoolHits[iter.Key].miss)
                    .Append("\n");
                n += iter.Key * iter.Value.Count;
            }
            sb.Append("Total string leng :").Append(n).Append("\n");
            sb.Append("***************************************************");
            return sb.ToString();
        }

        public static void FullFill(string dest, string str)
        {
            AMDebug.Assert(dest.Length == str.Length, "string length not match expect  {0} but {1}", dest.Length, str.Length);
            FillStringChecked(dest,0,str);
        }

        public static void FullFill(string dest, string str1,string str2)
        {
            AMDebug.Assert(dest.Length == str1.Length + str2.Length, "string length not match expect  {0} but {1}", dest.Length, str1.Length + str2.Length);
            FillStringChecked(dest, 0, str1);
            FillStringChecked(dest, str1.Length, str2);
        }

        public static void FullFill(string dest, string str1, string str2,string str3)
        {
            AMDebug.Assert(dest.Length == str1.Length + str2.Length+str3.Length, "string length not match expect  {0} but {1}", dest.Length, str1.Length + str2.Length + str3.Length);
            FillStringChecked(dest, 0, str1);
            FillStringChecked(dest, str1.Length, str2);
            FillStringChecked(dest, str1.Length+str2.Length, str3);
        }

        public static void FullFill(string dest, string str1, string str2, string str3,string str4)
        {
            AMDebug.Assert(dest.Length == str1.Length + str2.Length + str3.Length+str4.Length, "string length not match expect  {0} but {1}",dest.Length, str1.Length + str2.Length + str3.Length + str4.Length);
            int pos = 0;
            FillStringChecked(dest, 0, str1);
            pos+=str1.Length;
            FillStringChecked(dest, pos, str2);
            pos += str2.Length;
            FillStringChecked(dest, pos, str3);
            pos += str3.Length;
            FillStringChecked(dest, pos, str4);
        }

        public static void FullFill(string dest, string str1, string str2, string str3, string str4, string str5)
        {
            AMDebug.Assert(dest.Length == str1.Length + str2.Length + str3.Length + str4.Length+str5.Length, "string length not match expect  {0} but {1}",dest.Length, str1.Length + str2.Length + str3.Length + str4.Length + str5.Length);
            int pos = 0;
            FillStringChecked(dest, 0, str1);
            pos += str1.Length;
            FillStringChecked(dest, pos, str2);
            pos += str2.Length;
            FillStringChecked(dest, pos, str3);
            pos += str3.Length;
            FillStringChecked(dest, pos, str4);
            pos += str4.Length;
            FillStringChecked(dest, pos, str5);
        }

        public static void FullFill(string dest, params string[] values)
        {
            int position = 0;
            for (int i = 0; i < values.Length; i++)
            {
                FillStringChecked(dest, position, values[i]);
                position += values[i].Length;
            }

            AMDebug.Assert(dest.Length == position, "string length not match", "expect  {0} but {1}", position);
        }

        public static unsafe void FillStringChecked(string dest, int destPos, string src)
        {
            if (src.Length > (dest.Length - destPos))
            {
                throw new IndexOutOfRangeException();
            }
            fixed (char* pDest = dest)
            {
                fixed (char* pSrc = src)
                {
                    UnsafeUtility.Memcopy(pDest + destPos, pSrc, src.Length * 2);
                }
            }
        }


        public static unsafe void FillHexString(string dest, int destPos, byte[] data, Casing casing = Casing.Upper)
        {
            fixed (char* pDest = dest)
            {
                for(int i=0;i<data.Length;++i)
                {
                    ToCharsBuffer(data[i], pDest + destPos, casing);
                }
            }
        }

        public static unsafe void FillHexString(string dest, int destPos, ulong data, Casing casing = Casing.Upper)
        {
            fixed (char* pDest = dest)
            {
                ToCharsBuffer((byte)(data >> 56), pDest + destPos, casing);
                ToCharsBuffer((byte)(data >> 48), pDest + destPos+2, casing);
                ToCharsBuffer((byte)(data >> 40), pDest + destPos+4, casing);
                ToCharsBuffer((byte)(data >> 32), pDest + destPos+6, casing);
                ToCharsBuffer((byte)(data >> 24), pDest + destPos+8, casing);
                ToCharsBuffer((byte)(data >> 16), pDest + destPos+10, casing);
                ToCharsBuffer((byte)(data >> 8), pDest + destPos+12, casing);
                ToCharsBuffer((byte)(data & 0xFF), pDest + destPos+14, casing);
            }
        }

        public static unsafe void FillHexString(string dest, int destPos, uint data, Casing casing = Casing.Upper)
        {
            fixed (char* pDest = dest)
            {
                ToCharsBuffer((byte)(data >> 24), pDest + destPos, casing);
                ToCharsBuffer((byte)(data >> 16), pDest + destPos+2, casing);
                ToCharsBuffer((byte)(data >> 8), pDest + destPos+4, casing);
                ToCharsBuffer((byte)(data&0xFF), pDest + destPos+6, casing);
            }
        }

        public static unsafe void FillHexString(string dest, int destPos, ushort data, Casing casing = Casing.Upper)
        {
            fixed (char* pDest = dest)
            {
                ToCharsBuffer((byte)(data >> 8), pDest + destPos, casing);
                ToCharsBuffer((byte)(data & 0xFF), pDest + destPos+2, casing);
            }
        }

        public static unsafe void FillHexString(string dest, int destPos, byte data, Casing casing = Casing.Upper)
        {
            fixed (char* pDest = dest)
            {
                ToCharsBuffer(data, pDest + destPos, casing);
            }
        }

        public enum Casing : uint
        {
            Upper = 0,
            Lower = 0x2020
        }

        // We want to pack the incoming byte into a single integer [ 0000 HHHH 0000 LLLL ],
        // where HHHH and LLLL are the high and low nibbles of the incoming byte. Then
        // subtract this integer from a constant minuend as shown below.
        //
        //   [ 1000 1001 1000 1001 ]
        // - [ 0000 HHHH 0000 LLLL ]
        // =========================
        //   [ *YYY **** *ZZZ **** ]
        //
        // The end result of this is that YYY is 0b000 if HHHH <= 9, and YYY is 0b111 if HHHH >= 10.
        // Similarly, ZZZ is 0b000 if LLLL <= 9, and ZZZ is 0b111 if LLLL >= 10.
        // (We don't care about the value of asterisked bits.)
        //
        // To turn a nibble in the range [ 0 .. 9 ] into hex, we calculate hex := nibble + 48 (ascii '0').
        // To turn a nibble in the range [ 10 .. 15 ] into hex, we calculate hex := nibble - 10 + 65 (ascii 'A').
        //                                                                => hex := nibble + 55.
        // The difference in the starting ASCII offset is (55 - 48) = 7, depending on whether the nibble is <= 9 or >= 10.
        // Since 7 is 0b111, this conveniently matches the YYY or ZZZ value computed during the earlier subtraction.

        // The commented out code below is code that directly implements the logic described above.

        // uint packedOriginalValues = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU);
        // uint difference = 0x8989U - packedOriginalValues;
        // uint add7Mask = (difference & 0x7070U) >> 4; // line YYY and ZZZ back up with the packed values
        // uint packedResult = packedOriginalValues + add7Mask + 0x3030U /* ascii '0' */;

        // The code below is equivalent to the commented out code above but has been tweaked
        // to allow codegen to make some extra optimizations.

        // The low byte of the packed result contains the hex representation of the incoming byte's low nibble.
        // The adjacent byte of the packed result contains the hex representation of the incoming byte's high nibble.

        // Finally, write to the output buffer starting with the *highest* index so that codegen can
        // elide all but the first bounds check. (This only works if 'startingIndex' is a compile-time constant.)

        // The JIT can elide bounds checks if 'startingIndex' is constant and if the caller is
        // writing to a span of known length (or the caller has already checked the bounds of the
        // furthest access).
        private static void ToCharsBuffer(byte value, char[] buffer, int startingIndex = 0, Casing casing = Casing.Upper)
        {
            uint difference = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU) - 0x8989U;
            uint packedResult = ((((uint)(-(int)difference) & 0x7070U) >> 4) + difference + 0xB9B9U) | (uint)casing;

            buffer[startingIndex + 1] = (char)(packedResult & 0xFF);
            buffer[startingIndex] = (char)(packedResult >> 8);
        }

        private static unsafe void ToCharsBuffer(byte value, char* buffer, Casing casing = Casing.Upper)
        {
            uint difference = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU) - 0x8989U;
            uint packedResult = ((((uint)(-(int)difference) & 0x7070U) >> 4) + difference + 0xB9B9U) | (uint)casing;

            *buffer = (char)(packedResult >> 8);
            *(buffer+1) = (char)(packedResult & 0xFF);
        }
    }
}
