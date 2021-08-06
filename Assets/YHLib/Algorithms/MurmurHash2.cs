using System;
using System.Text;

namespace YH.Hash
{
	public class MurmurHash2
	{
		const uint M_32 = 0x5bd1e995;
		const int R_32 = 24;

		const UInt64 A_M = 0xc6a4a7935bd1e995;
		const int A_R = 47;

		const UInt32 B_M = 0x5bd1e995;
		const int B_R = 24;


		#region Hash32
		private static unsafe uint MurmurHash32(byte* ptr, int len, uint seed)
		{
			// 'm' and 'r' are mixing constants generated offline.
			// They're not really 'magic', they just happen to work well.

			// Initialize the hash to a 'random' value
			uint h = seed ^ (uint)len;

			// Mix 4 bytes at a time into the hash

			byte* end = ptr + len;

			while (end >= ptr + 4)
			{
				uint k = *((uint*)ptr);

				k *= M_32;
				k ^= k >> R_32;
				k *= M_32;

				h *= M_32;
				h ^= k;

				ptr += 4;
			}

			// Handle the last few bytes of the input array

			switch (end - ptr)
			{
				case 3:
					h ^= (uint)(*(ptr + 2)) << 16;
					h ^= (uint)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= M_32;
					break;
				case 2:
					h ^= (uint)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= M_32;
					break;
				case 1:
					h ^= *ptr;
					h *= M_32;
					break;
			};

			// Do a few final mixes of the hash to ensure the last few
			// bytes are well-incorporated.

			h ^= h >> 13;
			h *= M_32;
			h ^= h >> 15;

			return h;
		}

		public static unsafe uint MurmurHash32(byte[] data, uint seed)
		{
			fixed (byte* ptr = &data[0])
			{
				return MurmurHash32(ptr, data.Length, seed);
			}
		}

		public static unsafe uint MurmurHash32(string str, uint seed)
		{
			if (string.IsNullOrEmpty(str))
			{
				return seed;
			}
			else
			{
				byte[] data = UTF8Encoding.UTF8.GetBytes(str);
				return MurmurHash32(data, seed);
			}
		}

		//unicode 16编码的字串
		public static unsafe uint MurmurHash32U16(string str, uint seed)
		{
			// 'm' and 'r' are mixing constants generated offline.
			// They're not really 'magic', they just happen to work well.


			// Initialize the hash to a 'random' value

			int len = str.Length * 2;
			uint h = seed ^ (uint)len;

			// Mix 4 bytes at a time into the hash

			fixed (char* pStr = str)
			{
				UInt32* pData = (UInt32*)pStr;

				while (len >= 4)
				{
					uint k = *pData++;

					k *= M_32;
					k ^= k >> R_32;
					k *= M_32;

					h *= M_32;
					h ^= k;

					len -= 4;
				}

				// Handle the last few bytes of the input array

				byte* byteData = (byte*)pData;
				switch (len)
				{
					case 3:
						h ^= (uint)*(byteData + 2) << 16;
						h ^= (uint)*(byteData + 1) << 8;
						h ^= (uint)*(byteData);
						h *= M_32;
						break;
					case 2:
						h ^= (uint)*(byteData + 1) << 8;
						h ^= (uint)*(byteData);
						h *= M_32;
						break;
					case 1:
						h ^= (uint)*(byteData);
						h *= M_32;
						break;
				};
			}
			// Do a few final mixes of the hash to ensure the last few
			// bytes are well-incorporated.

			h ^= h >> 13;
			h *= M_32;
			h ^= h >> 15;

			return h;
		}

		public static uint Mix32(uint n, uint m)
		{
			uint h = 8;
			// Mix 4 bytes at a time into the hash

			n *= M_32;
			n ^= n >> R_32;
			n *= M_32;

			h *= M_32;
			h ^= n;

			m *= M_32;
			m ^= m >> R_32;
			m *= M_32;

			h *= M_32;
			h ^= m;

			// Do a few final mixes of the hash to ensure the last few
			// bytes are well-incorporated.

			h ^= h >> 13;
			h *= M_32;
			h ^= h >> 15;

			return h;
		}

		#endregion //32

		#region Hash64A				
		/// <summary>
		/// 直接读取ulong型。
		/// 在支持64位cpu下，速度较快。在32位cpu下表现较差。
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="len"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		private static unsafe UInt64 MurmurHash64A(byte* ptr, int len, UInt64 seed)
		{
			if (ptr == null)
			{
				return seed;
			}

			UInt64 h = seed ^ (((uint)len) * A_M);

			byte* end = ptr + len;

			while (ptr + 8 <= end)
			{
				UInt64 k = *((ulong*)ptr);

				k *= A_M;
				k ^= k >> A_R;
				k *= A_M;

				h ^= k;
				h *= A_M;

				ptr += 8;
			}

			//int remain = (len & 7) - 1;
			//if (remain > 0)
			//{
			//	for (; remain >= 0; --remain)
			//	{
			//		h ^= (UInt64)data[index + remain] << (remain*8);
			//	}

			//	h *= A_M;
			//}
			switch (len & 7)// == ( (len-index) & 7 ) == ((len % 8) & 7)
			{
				case 7:
					h ^= (UInt64)(*(ptr + 6)) << 48;
					h ^= (UInt64)(*(ptr + 5)) << 40;
					h ^= (UInt64)(*(ptr + 4)) << 32;
					h ^= (UInt64)(*(ptr + 3)) << 24;
					h ^= (UInt64)(*(ptr + 2)) << 16;
					h ^= (UInt64)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= A_M;
					break;
				case 6:
					h ^= (UInt64)(*(ptr + 5)) << 40;
					h ^= (UInt64)(*(ptr + 4)) << 32;
					h ^= (UInt64)(*(ptr + 3)) << 24;
					h ^= (UInt64)(*(ptr + 2)) << 16;
					h ^= (UInt64)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= A_M;
					break;
				case 5:
					h ^= (UInt64)(*(ptr + 4)) << 32;
					h ^= (UInt64)(*(ptr + 3)) << 24;
					h ^= (UInt64)(*(ptr + 2)) << 16;
					h ^= (UInt64)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= A_M;
					break;
				case 4:
					h ^= (UInt64)(*(ptr + 3)) << 24;
					h ^= (UInt64)(*(ptr + 2)) << 16;
					h ^= (UInt64)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= A_M;
					break;
				case 3:
					h ^= (UInt64)(*(ptr + 2)) << 16;
					h ^= (UInt64)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= A_M;
					break;
				case 2:
					h ^= (UInt64)(*(ptr + 1)) << 8;
					h ^= *ptr;
					h *= A_M;
					break;
				case 1:
					h ^= *ptr;
					h *= A_M;
					break;
			};

			//if ((len & 7)!=0)
			//{
			//	h *= A_M;
			//}

			h ^= h >> A_R;
			h *= A_M;
			h ^= h >> A_R;

			return h;
		}
		public static unsafe UInt64 MurmurHash64A(byte[] data, UInt64 seed)
		{
			fixed (byte* pData = &data[0])
			{
				return MurmurHash64A(pData, data.Length, seed);
			}

			//if (data == null)
			//{
			//	return seed;
			//}

			//int len = data.Length;
			//UInt64 h = seed ^ (((uint)len) * A_M);


			//int index = 0;
			//while (index+8<=len)
			//{
			//	UInt64 k = BitConverter.ToUInt64(data,index);

			//	k *= A_M;
			//	k ^= k >> A_R;
			//	k *= A_M;

			//	h ^= k;
			//	h *= A_M;

			//	index += 8;
			//}

			////int remain = (len & 7) - 1;
			////if (remain > 0)
			////{
			////	for (; remain >= 0; --remain)
			////	{
			////		h ^= (UInt64)data[index + remain] << (remain*8);
			////	}

			////	h *= A_M;
			////}
			//switch (len & 7)// == ( (len-index) & 7 ) == ((len % 8) & 7)
			//{
			//	case 7:
			//		h ^= (UInt64)data[index + 6] << 48;
			//		h ^= (UInt64)data[index + 5] << 40;
			//		h ^= (UInt64)data[index + 4] << 32;
			//		h ^= (UInt64)data[index + 3] << 24;
			//		h ^= (UInt64)data[index + 2] << 16;
			//		h ^= (UInt64)data[index + 1] << 8;
			//		h ^= (UInt64)data[index];
			//		h *= A_M;
			//		break;
			//	case 6:
			//		h ^= (UInt64)data[index + 5] << 40;
			//		h ^= (UInt64)data[index + 4] << 32;
			//		h ^= (UInt64)data[index + 3] << 24;
			//		h ^= (UInt64)data[index + 2] << 16;
			//		h ^= (UInt64)data[index + 1] << 8;
			//		h ^= (UInt64)data[index];
			//		h *= A_M;
			//		break;
			//	case 5:
			//		h ^= (UInt64)data[index + 4] << 32;
			//		h ^= (UInt64)data[index + 3] << 24;
			//		h ^= (UInt64)data[index + 2] << 16;
			//		h ^= (UInt64)data[index + 1] << 8;
			//		h ^= (UInt64)data[index];
			//		h *= A_M;
			//		break;
			//	case 4:
			//		h ^= (UInt64)data[index + 3] << 24;
			//		h ^= (UInt64)data[index + 2] << 16;
			//		h ^= (UInt64)data[index + 1] << 8;
			//		h ^= (UInt64)data[index];
			//		h *= A_M;
			//		break;
			//	case 3:
			//		h ^= (UInt64)data[index + 2] << 16;
			//		h ^= (UInt64)data[index + 1] << 8;
			//		h ^= (UInt64)data[index];
			//		h *= A_M;
			//		break;
			//	case 2:
			//		h ^= (UInt64)data[index + 1] << 8;
			//		h ^= (UInt64)data[index];
			//		h *= A_M;
			//		break;
			//	case 1:
			//		h ^= (UInt64)data[index];
			//		h *= A_M;
			//		break;
			//};

			////if ((len & 7)!=0)
			////{
			////	h *= A_M;
			////}

			//h ^= h >> A_R;
			//h *= A_M;
			//h ^= h >> A_R;

			//return h;
		}
		public static unsafe UInt64 MurmurHash64A(string str, UInt64 seed)
		{
			if (string.IsNullOrEmpty(str))
			{
				return seed;
			}
			else
			{
				byte[] data = UTF8Encoding.UTF8.GetBytes(str);
				return MurmurHash64A(data, seed);
			}
		}

		#endregion //Hash64A

		#region Hash64B
		/// <summary>
		/// 直接使用2个uint。
		/// 32位，64位cpu下速度相当。
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="len"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		private static unsafe UInt64 MurmurHash64B(byte* ptr, int len, UInt64 seed)
		{
			if (ptr == null)
			{
				return seed;
			}

			UInt32 h1 = ((UInt32)seed) ^ (UInt32)len;
			UInt32 h2 = (UInt32)(seed >> 32);


			byte* end = ptr + len;
			while (ptr + 8 <= end)
			{
				UInt32 k1 = *((UInt32*)ptr);
				k1 *= B_M;
				k1 ^= k1 >> B_R;
				k1 *= B_M;
				h1 *= B_M;
				h1 ^= k1;

				ptr += 4;

				UInt32 k2 = *((UInt32*)ptr);
				k2 *= B_M;
				k2 ^= k2 >> B_R;
				k2 *= B_M;
				h2 *= B_M;
				h2 ^= k2;

				ptr += 4;
			}

			if (end >= ptr + 4)
			{
				UInt32 k1 = *((UInt32*)ptr);
				k1 *= B_M;
				k1 ^= k1 >> B_R;
				k1 *= B_M;
				h1 *= B_M;
				h1 ^= k1;
				ptr += 4;
			}

			int remain = (int)(end - ptr);
			if (remain > 0)
			{
				//for (remain = remain - 1; remain >= 0; --remain)
				//{
				//	h2 ^= (UInt32)(*(data + remain)) << (remain*8);
				//}
				switch (remain)
				{
					case 3:
						h2 ^= (UInt32)(*(ptr + 2)) << 16;
						h2 ^= (UInt32)(*(ptr + 1)) << 8;
						h2 ^= *ptr;
						break;
					case 2:
						h2 ^= (UInt32)(*(ptr + 1)) << 8;
						h2 ^= *ptr;
						break;
					case 1:
						h2 ^= *ptr;
						break;
				};
				h2 *= B_M;
			}

			h1 ^= h2 >> 18;
			h1 *= B_M;
			h2 ^= h1 >> 22;
			h2 *= B_M;
			h1 ^= h2 >> 17;
			h1 *= B_M;
			h2 ^= h1 >> 19;
			h2 *= B_M;

			UInt64 h = h1;

			h = (h << 32) | h2;

			return h;
		}
		public static unsafe UInt64 MurmurHash64B(byte[] data, UInt64 seed)
		{
			if (data == null)
			{
				return seed;
			}
			fixed (byte* pData = &data[0])
			{
				return MurmurHash64B(pData, data.Length, seed);
			}
			//int len = data.Length;
			//UInt32 h1 = ((UInt32)seed) ^ (UInt32)len;
			//UInt32 h2 = (UInt32)(seed >> 32);

			//int index = 0;
			//while (len >= index + 8)
			//{
			//	UInt32 k1 = BitConverter.ToUInt32(data, index);
			//	k1 *= B_M;
			//	k1 ^= k1 >> B_R;
			//	k1 *= B_M;
			//	h1 *= B_M;
			//	h1 ^= k1;

			//	index += 4;

			//	UInt32 k2 = BitConverter.ToUInt32(data, index);
			//	k2 *= B_M;
			//	k2 ^= k2 >> B_R;
			//	k2 *= B_M;
			//	h2 *= B_M;
			//	h2 ^= k2;

			//	index += 4;
			//}

			//if (len >= index + 4)
			//{
			//	UInt32 k1 = BitConverter.ToUInt32(data, index);
			//	k1 *= B_M;
			//	k1 ^= k1 >> B_R;
			//	k1 *= B_M;
			//	h1 *= B_M;
			//	h1 ^= k1;
			//	index += 4;
			//}

			//int remain = len - index;
			//if (remain > 0)
			//{
			//	//for (remain = remain - 1; remain >= 0; --remain)
			//	//{
			//	//	h2 ^= (UInt32)data[index + remain] << (remain * 8);
			//	//}

			//	switch (remain)
			//	{
			//		case 3:
			//			h2 ^= (UInt32)data[index + 2] << 16;
			//			h2 ^= (UInt32)data[index + 1] << 8;
			//			h2 ^= (UInt32)data[index];
			//			break;
			//		case 2:
			//			h2 ^= (UInt32)data[index + 1] << 8;
			//			h2 ^= (UInt32)data[index];
			//			break;
			//		case 1:
			//			h2 ^= (UInt32)data[index];
			//			break;
			//	};
			//	h2 *= B_M;
			//}

			//h1 ^= h2 >> 18;
			//h1 *= B_M;
			//h2 ^= h1 >> 22;
			//h2 *= B_M;
			//h1 ^= h2 >> 17;
			//h1 *= B_M;
			//h2 ^= h1 >> 19;
			//h2 *= B_M;

			//UInt64 h = h1;

			//h = (h << 32) | h2;

			//return h;
		}
		public static unsafe UInt64 MurmurHash64B(string str, UInt64 seed)
		{
			if (string.IsNullOrEmpty(str))
			{
				return seed;
			}
			else
			{
				byte[] data = UTF8Encoding.UTF8.GetBytes(str);
				return MurmurHash64B(data, seed);
			}
		}
		#endregion //Hash64B

		public static UInt64 Mix64(UInt64 n, UInt64 m)
		{
			uint len = 16;
			UInt64 h = len * A_M;

			UInt64 k = n;

			k *= A_M;
			k ^= k >> A_R;
			k *= A_M;
			h ^= k;
			h *= A_M;

			k = m;

			k *= A_M;
			k ^= k >> A_R;
			k *= A_M;
			h ^= k;
			h *= A_M;


			h ^= h >> A_R;
			h *= A_M;
			h ^= h >> A_R;

			return h;
		}

		public static UInt64 Mix64(UInt64 n, uint m)
		{
			uint len = 12;
			UInt32 h1 = len;
			UInt32 h2 = 0;

			UInt32 k1 = (UInt32)n;
			k1 *= B_M;
			k1 ^= k1 >> B_R;
			k1 *= B_M;
			h1 *= B_M;
			h1 ^= k1;


			UInt32 k2 = (UInt32)(n >> 32);
			k2 *= B_M;
			k2 ^= k2 >> B_R;
			k2 *= B_M;
			h2 *= B_M;
			h2 ^= k2;

			k1 = m;

			k1 *= B_M;
			k1 ^= k1 >> B_R;
			k1 *= B_M;
			h1 *= B_M;
			h1 ^= k1;

			h1 ^= h2 >> 18;
			h1 *= B_M;
			h2 ^= h1 >> 22;
			h2 *= B_M;
			h1 ^= h2 >> 17;
			h1 *= B_M;
			h2 ^= h1 >> 19;
			h2 *= B_M;

			UInt64 h = h1;

			h = (h << 32) | h2;

			return h;
		}

		#region Hash U16
		//直接使用unicode16。要注意不同平台、不同语言之间的字符串编码。
		public static unsafe UInt64 MurmurHash64AU16(string str, UInt64 seed)
		{
			//c#字符串在内存中是unicod16。占二个字节。
			int len = str.Length * 2;
			fixed (char* ptr = str)
			{
				return MurmurHash64A((byte*)ptr, len, seed);
			}
		}

		public static unsafe UInt64 MurmurHash64AU16LE(string key, UInt64 seed)
		{
			if (string.IsNullOrEmpty(key))
			{
				return seed;
			}

			int len = key.Length;
			UInt64 h = seed ^ (((uint)len) * A_M);

			fixed (char* pStr = key)
			{
				byte* pData = (byte*)pStr;
				byte* end = pData + len * 2;

				while (pData + 16 <= end)
				{
					UInt64 k = ((UInt64)(*pData)) | ((UInt64)(*(pData + 2))) << 8 | ((UInt64)(*(pData + 4))) << 16 | ((UInt64)(*(pData + 6))) << 24
										| ((UInt64)(*(pData + 8))) << 32 | ((UInt64)(*(pData + 10))) << 40 | ((UInt64)(*(pData + 12))) << 48 | ((UInt64)(*(pData + 14))) << 56;

					k *= A_M;
					k ^= k >> A_R;
					k *= A_M;

					h ^= k;
					h *= A_M;

					pData += 16;
				}

				int remain = (len & 7) - 1;
				if (remain >= 0)
				{
					for (; remain >= 0; --remain)
					{
						h ^= ((UInt64)(*(pData + 2 * remain))) << (remain * 8);
					}

					h *= A_M;
				}

				h ^= h >> A_R;
				h *= A_M;
				h ^= h >> A_R;

				return h;
			}
		}


		/// <summary>
		/// 获取字符串的hash值
		/// 不是大小端安全的，依赖运行平台。
		/// 主要是因为在把字符串buff转成uint时，直接使用指针转换。
		/// </summary>
		/// <param name="str"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		public static unsafe UInt64 MurmurHash64BU16(string str, UInt64 seed)
		{
			if (string.IsNullOrEmpty(str))
			{
				return seed;
			}

			int len = str.Length * 2;
			UInt32 h1 = ((UInt32)seed) ^ (UInt32)len;
			UInt32 h2 = (UInt32)(seed >> 32);

			fixed (char* pStr = str)
			{
				UInt32* pData = (UInt32*)pStr;

				while (len >= 8)
				{
					UInt32 k1 = *pData++;
					k1 *= B_M;
					k1 ^= k1 >> B_R;
					k1 *= B_M;
					h1 *= B_M;
					h1 ^= k1;

					UInt32 k2 = *pData++;
					k2 *= B_M;
					k2 ^= k2 >> B_R;
					k2 *= B_M;
					h2 *= B_M;
					h2 ^= k2;

					len -= 8;
				}

				if (len >= 4)
				{
					UInt32 k1 = *pData++;
					k1 *= B_M;
					k1 ^= k1 >> B_R;
					k1 *= B_M;
					h1 *= B_M;
					h1 ^= k1;

					len -= 4;
				}

				if (len > 0)
				{
					byte* pRemain = (byte*)pData;
					//for (len = len - 1; len >= 0; --len)
					//{
					//	h2 ^= (UInt32)(*(pRemain + len)) << (len * 8);
					//}

					switch (len)
					{
						case 3:
							h2 ^= (UInt32)(*(pRemain + 2)) << 16;
							h2 ^= (UInt32)(*(pRemain + 1)) << 8;
							h2 ^= (UInt32)(*pRemain);
							h2 *= B_M;
							break;
						case 2:
							h2 ^= (UInt32)(*(pRemain + 1)) << 8;
							h2 ^= (UInt32)(*pRemain);
							h2 *= B_M;
							break;
						case 1:
							h2 ^= (UInt32)(*pRemain);
							h2 *= B_M;
							break;
					};
				}
			}

			h1 ^= h2 >> 18;
			h1 *= B_M;
			h2 ^= h1 >> 22;
			h2 *= B_M;
			h1 ^= h2 >> 17;
			h1 *= B_M;
			h2 ^= h1 >> 19;
			h2 *= B_M;

			UInt64 h = h1;

			h = (h << 32) | h2;

			return h;
		}

		/// <summary>
		/// 计算字符串hash。
		/// 使用小端方式转化字符串buff到uint。
		/// 跳过utf16中的0字符。目前只对ascii码的字符串有效。理论速度快一倍，谨慎使用！！！
		/// </summary>
		/// <param name="key"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		public static unsafe UInt64 MurmurHash64BU16LE(string key, UInt64 seed)
		{
			if (string.IsNullOrEmpty(key))
			{
				return seed;
			}

			int len = key.Length;
			UInt32 h1 = ((UInt32)seed) ^ (UInt32)len;
			UInt32 h2 = (UInt32)(seed >> 32);

			fixed (char* pStr = key)
			{
				byte* pData = (byte*)pStr;

				while (len >= 8)
				{

					UInt32 k1 = ((UInt32)(*pData)) | ((UInt32)(*(pData + 2))) << 8 | ((UInt32)(*(pData + 4))) << 16 | ((UInt32)(*(pData + 6))) << 24;
					pData += 8;

					k1 *= B_M;
					k1 ^= k1 >> B_R;
					k1 *= B_M;
					h1 *= B_M;
					h1 ^= k1;


					UInt32 k2 = ((UInt32)(*pData)) | ((UInt32)(*(pData + 2))) << 8 | ((UInt32)(*(pData + 4))) << 16 | ((UInt32)(*(pData + 6))) << 24;
					pData += 8;

					k2 *= B_M;
					k2 ^= k2 >> B_R;
					k2 *= B_M;
					h2 *= B_M;
					h2 ^= k2;

					len -= 8;
				}

				if (len >= 4)
				{
					UInt32 k1 = ((UInt32)(*pData)) | ((UInt32)(*(pData + 2))) << 8 | ((UInt32)(*(pData + 4))) << 16 | ((UInt32)(*(pData + 6))) << 24;
					pData += 8;

					k1 *= B_M;
					k1 ^= k1 >> B_R;
					k1 *= B_M;
					h1 *= B_M;
					h1 ^= k1;

					len -= 4;
				}

				if (len > 0)
				{
					//for (len = len - 1; len >= 0; --len)
					//{
					//	h2 ^= (UInt32)(*(pData + len)) << (len * 8);
					//}

					switch (len)
					{
						case 3:
							h2 ^= (UInt32)(*(pData + 4)) << 16;
							h2 ^= (UInt32)(*(pData + 2)) << 8;
							h2 ^= (UInt32)(*pData);
							break;
						case 2:
							h2 ^= (UInt32)(*(pData + 2)) << 8;
							h2 ^= (UInt32)(*pData);
							break;
						case 1:
							h2 ^= (UInt32)(*pData);
							break;
					};
					h2 *= B_M;
				}
			}

			h1 ^= h2 >> 18;
			h1 *= B_M;
			h2 ^= h1 >> 22;
			h2 *= B_M;
			h1 ^= h2 >> 17;
			h1 *= B_M;
			h2 ^= h1 >> 19;
			h2 *= B_M;

			UInt64 h = h1;

			h = (h << 32) | h2;

			return h;
		}

		/// <summary>
		/// c#字符串是按utf16存储的，对于只包含英文的字符串，可以跳过0。相当于计算减半。
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		private static unsafe UInt32 ReadUInt32LE(byte* p)
		{
			UInt32 a = *p;
			a |= (UInt32)(*(p + 2)) << 8;
			a |= (UInt32)(*(p + 4)) << 16;
			a |= (UInt32)(*(p + 6)) << 24;
			return a;
		}

		#endregion //Hash U16
	}
}
