namespace Findx.Machine.Network;

/// <summary>
/// 速率
/// </summary>
public struct Rate
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="receivedLength"></param>
    /// <param name="sendLength"></param>
    public Rate(DateTime startTime, long receivedLength, long sendLength)
    {
        StartTime = startTime;
        ReceivedLength = receivedLength;
        SendLength = sendLength;
    }

    /// <summary>
    /// 记录时间
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// 此网卡总接收网络流量字节数
    /// </summary>
    public long ReceivedLength { get; }

    /// <summary>
    /// 此网卡总发送网络流量字节数
    /// </summary>
    public long SendLength { get; }
}