#nullable enable
using System.Net.NetworkInformation;

namespace Findx.Utilities;

/// <summary>
///     雪花算法生成ID
/// </summary>
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class SnowflakeIdUtility
{
    /// <summary>
    ///     Start time 2010-11-04 09:42:54
    /// </summary>
    private const long StartTimestamp = 1288834974657L;

    private const int WorkerIdBitLength = 10;
    private const int SequenceBitLength = 12;
    private const int MaxWorkerId = ~(-1 << WorkerIdBitLength);
    
    private const int WorkerIdShift = SequenceBitLength;
    private const int TimestampLeftShift = SequenceBitLength + WorkerIdBitLength;
    private const long SequenceMask = -1L ^ (-1L << SequenceBitLength);

    private static SnowflakeIdUtility? _snowflakeId;

    private static readonly object SLock = new();
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="workerId">工作id</param>
    /// <exception cref="ArgumentException"></exception>
    public SnowflakeIdUtility(long workerId)
    {
        // sanity check for workerId
        if (workerId is > MaxWorkerId or < 0)
            throw new ArgumentException($"worker Id can't be greater than {MaxWorkerId} or less than 0");

        WorkerId = workerId << WorkerIdShift;
    }
    
    private readonly object _lock = new();
    
    private long WorkerId { get; }
    
    private long _lastTimeTick = -1L;
    
    private int _sequence = -1;

    /// <summary>
    ///     默认雪花算法服务
    /// </summary>
    /// <returns></returns>
    public static SnowflakeIdUtility Default()
    {
        if (_snowflakeId != null) 
            return _snowflakeId;

        lock (SLock)
        {
            if (_snowflakeId != null) 
                return _snowflakeId;

            if (!long.TryParse(Environment.GetEnvironmentVariable("WORKERID"), out var workerId))
            {
                var nodeId = Util.GenerateWorkerId(MaxWorkerId);
                var pid = Environment.ProcessId;
                var tid = Environment.CurrentManagedThreadId;
                workerId = ((nodeId & 0x1F) << 5) | (uint)((pid ^ tid) & 0x1F);
            }
            
            // ReSharper disable once PossibleMultipleWriteAccessInDoubleCheckLocking
            return _snowflakeId = new SnowflakeIdUtility(workerId);
        }
    }

    /// <summary>
    ///     雪花Id
    /// </summary>
    /// <returns></returns>
    public virtual long NextId()
    {
        lock (_lock)
        {
            var currentTimeTick = GetCurrentTimeTick();
            if (currentTimeTick < _lastTimeTick)
            {
                var t = _lastTimeTick - currentTimeTick;
                
                // 在夏令时地区，时间可能回拨1个小时
                if (t > 3600_000 + 10_000) 
                    throw new InvalidOperationException($"time reversal too large ({t}ms). To ensure uniqueness, Snowflake refuses to generate a new Id");
            }
            
            if (currentTimeTick > _lastTimeTick)
            {
                _lastTimeTick = currentTimeTick;
                _sequence = 0;
                return GenerateId(_lastTimeTick);
            }
            
            _sequence += 1;

            if (_sequence > SequenceMask)
            {
                _lastTimeTick++;
                _sequence = 0;
            }
            
            return GenerateId(_lastTimeTick);
        }
    }

    private long GenerateId(long useTimeTick)
    {
        return (useTimeTick << TimestampLeftShift) | WorkerId | (uint)_sequence;
    }
    
    private long GetCurrentTimeTick()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - StartTimestamp;
    }
    
    /// <summary>
    ///     时间转为Id，不带节点和序列号。可用于构建时间片段查询</summary>
    /// <remarks>
    ///     基于指定时间，转为utc时间后，生成不带WorkerId和序列号的Id。
    ///     一般用于构建时间片段查询，例如查询某个时间段内的数据，把时间片段转为雪花Id片段。
    /// </remarks>
    /// <param name="time">时间</param>
    /// <returns></returns>
    public static long GetId(DateTime time)
    {
        var dateTimeOffset = new DateTimeOffset(time);
        var t = dateTimeOffset.ToUniversalTime().ToUnixTimeMilliseconds() - StartTimestamp;
        return t << TimestampLeftShift;
    }

    /// <summary>
    ///     解析雪花Id，得到时间、WorkerId和序列号
    /// </summary>
    /// <param name="id"></param>
    /// <param name="time">返回本地时间(DateTimeOffset.LocalDateTime)</param>
    /// <param name="workerId">节点</param>
    /// <param name="sequence">序列号</param>
    /// <returns></returns>
    public static bool TryParse(long id, out DateTime time, out int workerId, out int sequence)
    {
        time = DateTimeOffset.FromUnixTimeMilliseconds((id >> TimestampLeftShift) + StartTimestamp).LocalDateTime;
        workerId = (int)((id >> WorkerIdShift) & MaxWorkerId);
        sequence = (int)(id & SequenceMask);
        return true;
    }
}

internal static class Util
{
    /// <summary>
    ///     auto generate workerId, try using mac first, if failed, then randomly generate one
    /// </summary>
    /// <returns>workerId</returns>
    public static long GenerateWorkerId(int maxWorkerId)
    {
        try
        {
            return GenerateWorkerIdBaseOnMac();
        }
        catch
        {
            return GenerateRandomWorkerId(maxWorkerId);
        }
    }

    /// <summary>
    ///     use lowest 10 bit of available MAC as workerId
    /// </summary>
    /// <returns>workerId</returns>
    private static long GenerateWorkerIdBaseOnMac()
    {
        var nice = NetworkInterface.GetAllNetworkInterfaces();
        // exclude virtual and Loopback
        var firstUpInterface = nice.OrderByDescending(x => x.Speed).FirstOrDefault(x => !x.Description.Contains("Virtual") && x.NetworkInterfaceType != NetworkInterfaceType.Loopback && x.OperationalStatus == OperationalStatus.Up);
        if (firstUpInterface == null) throw new Exception("no available mac found");
        var address = firstUpInterface.GetPhysicalAddress();
        var mac = address.GetAddressBytes();

        return ((mac[4] & 0B11) << 8) | (mac[5] & 0xFF);
    }

    /// <summary>
    ///     randomly generate one as workerId
    /// </summary>
    /// <returns></returns>
    private static long GenerateRandomWorkerId(int maxWorkerId)
    {
        return new Random().Next(maxWorkerId + 1);
    }
}