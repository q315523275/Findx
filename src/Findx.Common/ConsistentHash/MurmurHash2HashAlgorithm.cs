using System;
using System.Text;

namespace Findx.ConsistentHash
{
    public class MurmurHash2HashAlgorithm : IHashAlgorithm
    {
        public int Hash(string item)
        {
            uint hash = Hash(Encoding.UTF8.GetBytes(item));
            return (int)hash;
        }

        private const uint m = 0x5bd1e995;
        private const int r = 24;

        public static uint Hash(byte[] data)
        {
            return Hash(data, 0xc58f1a7b);
        }

        public static uint Hash(byte[] data, uint seed)
        {
            int length = data.Length;
            if (length == 0)
                return 0;

            uint h = seed ^ (uint)length;
            int c = 0; // current index
            while (length >= 4)
            {
                uint k = (uint)(
                    data[c++]
                    | data[c++] << 8
                    | data[c++] << 16
                    | data[c++] << 24);
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                length -= 4;
            }

            switch (length)
            {
                case 3:
                    h ^= (ushort)(data[c++] | data[c++] << 8);
                    h ^= (uint)(data[c] << 16);
                    h *= m;
                    break;
                case 2:
                    h ^= (ushort)(data[c++] | data[c] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= data[c];
                    h *= m;
                    break;
                default:
                    break;
            }

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}

