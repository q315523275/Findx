using System.Security.Cryptography;
using Findx.Data;

namespace Findx.Utilities;

/// <summary>
///     有序Guid
///     <para>Abp对应有序Guid,非连续递增,如需连续递增请使用Findx.Guid.NewId组件包</para>
/// </summary>
public static class SequentialGuidUtility
{
    private static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();

    /// <summary>
    ///     生成连续 Guid
    /// </summary>
    /// <param name="sequentialGuidType"></param>
    /// <param name="databaseType"></param>
    /// <returns></returns>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static Guid Next(SequentialGuidType sequentialGuidType, DatabaseType databaseType = DatabaseType.MySql)
    {
        switch (databaseType)
        {
            case DatabaseType.SqlServer:
                return Next(SequentialGuidType.AtEnd);
            case DatabaseType.Oracle:
                return Next(SequentialGuidType.AsBinary);
            case DatabaseType.Sqlite:
            case DatabaseType.MySql:
            case DatabaseType.PostgreSql:
            default:
                return Next(SequentialGuidType.AsString);
        }
    }

    /// <summary>
    ///     生成连续 Guid
    /// </summary>
    /// <param name="guidType"></param>
    /// <returns></returns>
    public static Guid Next(SequentialGuidType guidType)
    {
        // We start with 16 bytes of cryptographically strong random data.
        var randomBytes = new byte[10];
        RandomNumberGenerator.GetBytes(randomBytes);

        // An alternate method: use a normally-created GUID to get our initial
        // random data:
        // byte[] randomBytes = Guid.NewGuid().ToByteArray();
        // This is faster than using RNGCryptoServiceProvider, but I don't
        // recommend it because the .NET Framework makes no guarantee of the
        // randomness of GUID data, and future versions (or different
        // implementations like Mono) might use a different method.

        // Now we have the random basis for our GUID.  Next, we need to
        // create the six-byte block which will be our timestamp.

        // We start with the number of milliseconds that have elapsed since
        // DateTime.MinValue.  This will form the timestamp.  There's no use
        // being more specific than milliseconds, since DateTime.Now has
        // limited resolution.

        // Using millisecond resolution for our 48-bit timestamp gives us
        // about 5900 years before the timestamp overflows and cycles.
        // Hopefully this should be sufficient for most purposes. :)
        long timestamp = DateTime.UtcNow.Ticks / 10000L;

        // Then get the bytes
        byte[] timestampBytes = BitConverter.GetBytes(timestamp);

        // Since we're converting from an Int64, we have to reverse on
        // little-endian systems.
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }

        byte[] guidBytes = new byte[16];

        switch (guidType)
        {
            case SequentialGuidType.AsString:
            case SequentialGuidType.AsBinary:

                // For string and byte-array version, we copy the timestamp first, followed
                // by the random data.
                Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                // If formatting as a string, we have to compensate for the fact
                // that .NET regards the Data1 and Data2 block as an Int32 and an Int16,
                // respectively.  That means that it switches the order on little-endian
                // systems.  So again, we have to reverse.
                if (guidType == SequentialGuidType.AsString && BitConverter.IsLittleEndian)
                {
                    Array.Reverse(guidBytes, 0, 4);
                    Array.Reverse(guidBytes, 4, 2);
                }

                break;

            case SequentialGuidType.AtEnd:

                // For sequential-at-the-end versions, we copy the random data first,
                // followed by the timestamp.
                Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                break;
        }

        return new Guid(guidBytes);
    }
}




/// <summary>
///     Describes the type of a sequential GUID value.
/// </summary>
public enum SequentialGuidType
{
    /// <summary>
    /// <para>dddddddd-dddd-Mddd-Ndrr-rrrrrrrrrrrr</para>
    /// <para>用于MySql和PostgreSql</para>
    /// <para>当使用<see cref="Guid.ToString()"/>方法进行格式化时连续</para>
    /// <para>顺序体现在第8个字节</para>
    /// </summary>
    AsString,

    /// <summary>
    /// <para>dddddddd-dddd-Mddd-Ndrr-rrrrrrrrrrrr</para>
    /// <para>用于Oracle</para>
    /// <para>当使用<see cref="Guid.ToByteArray()"/>方法进行格式化时连续</para>
    /// <para>顺序体现在第8个字节，连续递增</para>
    /// </summary>
    AsBinary,

    /// <summary>
    /// <para>rrrrrrrr-rrrr-Mxdr-Nddd-dddddddddddd</para>
    /// <para>用于SqlServer</para>
    /// <para>连续性体现于GUID的第4块（Data4）</para>
    /// <para>顺序比较Block5 > Block4 > Block3 > Block2 > Block1</para>
    /// </summary>
    AtEnd
}