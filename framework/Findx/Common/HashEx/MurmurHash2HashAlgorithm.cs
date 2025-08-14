namespace Findx.Common;

/// <summary>
///     MurmurHash-Hash算法
/// </summary>
public class MurmurHash2HashAlgorithm : IHashAlgorithm
{
    private const uint M = 0x5bd1e995;
    private const int R = 24;

    /// <summary>
    ///     计算hash值
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int Hash(string item)
    {
        var hash = Hash(Encoding.UTF8.GetBytes(item));
        return (int)hash;
    }


    /// <summary>
    ///     计算hash值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static uint Hash(byte[] data)
    {
        return Hash(data, 0xc58f1a7a);
    }

    /// <summary>
    ///     计算hash值
    /// </summary>
    /// <param name="data"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    private static uint Hash(IReadOnlyList<byte> data, uint seed)
    {
        var length = data.Count;
        if (length == 0)
            return 0;

        var h = seed ^ (uint)length;
        var currentIndex = 0; // current index
        while (length >= 4)
        {
            var k = (uint)(data[currentIndex++] | (data[currentIndex++] << 8) | (data[currentIndex++] << 16) |
                           (data[currentIndex++] << 24));
            k *= M;
            k ^= k >> R;
            k *= M;

            h *= M;
            h ^= k;
            length -= 4;
        }

        switch (length)
        {
            case 3:
                h ^= (ushort)(data[currentIndex++] | (data[currentIndex++] << 8));
                h ^= (uint)(data[currentIndex] << 16);
                h *= M;
                break;
            case 2:
                h ^= (ushort)(data[currentIndex++] | (data[currentIndex] << 8));
                h *= M;
                break;
            case 1:
                h ^= data[currentIndex];
                h *= M;
                break;
        }

        h ^= h >> 13;
        h *= M;
        h ^= h >> 15;

        return h;
    }
}