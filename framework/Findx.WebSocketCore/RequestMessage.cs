using System;

namespace Findx.WebSocketCore;

/// <summary>
///     请求消息基类
/// </summary>
public abstract class RequestMessage;

/// <summary>
///     文本请求消息
/// </summary>
public class RequestTextMessage : RequestMessage
{
    /// <summary>
    ///     文本
    /// </summary>
    public string Text { get; }

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="text"></param>
    public RequestTextMessage(string text)
    {
        Text = text;
    }
}

/// <summary>
///     二进制请求消息
/// </summary>
public class RequestBinaryMessage : RequestMessage
{
    /// <summary>
    ///     byte data
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="data"></param>
    public RequestBinaryMessage(byte[] data)
    {
        Data = data;
    }
}

/// <summary>
///     二进制段请求消息
/// </summary>
public class RequestBinarySegmentMessage : RequestMessage
{
    /// <summary>
    ///     二进制段
    /// </summary>
    public ArraySegment<byte> Data { get; }

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="data"></param>
    public RequestBinarySegmentMessage(ArraySegment<byte> data)
    {
        Data = data;
    }
}