using System.Runtime.InteropServices;

namespace YH.AssetManage
{
    public class UnsafeUtility
    {
        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Block16
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Block64
        {
        }
        public unsafe static void MemmoveBlk(byte* dest, byte* src, int len)
        {
            MemmoveBlk(dest, src, (uint)len);
        }

        public unsafe static void MemmoveBlk(byte* dest, byte* src, uint len)
        {
            if (len <= 2048)
            {
                if ((ulong)(dest - src) >= len && (ulong)(src - dest) >= len)
                {
                    byte* ptr = src + len;
                    byte* ptr2 = dest + len;
                    if (len > 16)
                    {
                        if (len > 64)
                        {
                            uint num = len >> 6;
                            do
                            {
                                *(Block64*)dest = *(Block64*)src;
                                dest += 64;
                                src += 64;
                                num--;
                            }
                            while (num != 0);
                            len %= 64u;
                            if (len <= 16)
                            {
                                *(Block16*)(ptr2 - 16) = *(Block16*)(ptr - 16);
                                return;
                            }
                        }

                        *(Block16*)dest = *(Block16*)src;
                        if (len > 32)
                        {
                            *(Block16*)(dest + 16) = *(Block16*)(src + 16);
                            if (len > 48)
                            {
                                *(Block16*)(dest + 32) = *(Block16*)(src + 32);
                            }
                        }

                        *(Block16*)(ptr2 - 16) = *(Block16*)(ptr - 16);
                    }
                    else if ((len & 0x18) != 0)
                    {
                        *(int*)dest = *(int*)src;
                        *(int*)(dest + 4) = *(int*)(src + 4);
                        *(int*)(ptr2 - 8) = *(int*)(ptr - 8);
                        *(int*)(ptr2 - 4) = *(int*)(ptr - 4);
                    }
                    else if ((len & 4) != 0)
                    {
                        *(int*)dest = *(int*)src;
                        *(int*)(ptr2 - 4) = *(int*)(ptr - 4);
                    }
                    else if (len != 0)
                    {
                        *dest = *src;
                        if ((len & 2) != 0)
                        {
                            *(short*)(ptr2 - 2) = *(short*)(ptr - 2);
                        }
                    }

                    return;
                }
            }
            Memmove(dest, src, len);
        }

        public static unsafe void Memmove(byte* dest, byte* src, int len)
        {
            Memmove(dest, src, (uint)len);
        }

        public static unsafe void Memmove(byte* dest, byte* src, uint len)
        {
            if (len > 8)
            {
                //dest > src
                if ((long)dest - (long)src > 8)
                {
                    uint block = len >> 3;
                    long* pDest = (long*)(dest + len);
                    long* pSrc = (long*)(src + len);

                    for (int i = 0; i < block; i++)
                    {
                        *--pDest = *--pSrc;
                    }
                    dest = (byte*)pDest;
                    src = (byte*)pSrc;
                    len = len - (block << 3);
                    dest -= len;
                    src -= len;
                }
                else if ((int)src - (int)dest > 8)
                {
                    uint block = len >> 3;
                    long* pDest = (long*)dest;
                    long* pSrc = (long*)src;

                    for (int i = 0; i < block; i++)
                    {
                        *pDest++ = *pSrc++;
                    }
                    dest = (byte*)pDest;
                    src = (byte*)pSrc;
                    len = len - (block << 3);
                }
            }

            if (len > 0)
            {
                if ((long)dest - (long)src > 0)
                {
                    byte* pDest = dest + len;
                    byte* pSrc = src + len;
                    for (int i = 0; i < len; i++)
                    {
                        *--pDest = *--pSrc;
                    }
                }
                else
                {
                    byte* pDest = dest;
                    byte* pSrc = src;
                    for (int i = 0; i < len; i++)
                    {
                        *pDest++ = *pSrc++;
                    }
                }
            }
        }

        public static unsafe void Memcopy(void* dest, void* src, int len)
        {
            Memcopy(dest, src, (uint)len);
        }

        public static unsafe void Memcopy(void* dest, void* src, uint len)
        {
            if (len > 8)
            {
                uint block = len >> 3;
                long* pDest = (long*)dest;
                long* pSrc = (long*)src;

                for (int i = 0; i < block; i++)
                {
                    *pDest++ = *pSrc++;
                }
                dest = (byte*)pDest;
                src = (byte*)pSrc;
                len = len - (block << 3);
            }

            if (len > 0)
            {
                byte* pDest = (byte*)dest;
                byte* pSrc = (byte*)src;
                for (int i = 0; i < len; i++)
                {
                    *pDest++ = *pSrc++;
                }
            }
        }
    }

}
