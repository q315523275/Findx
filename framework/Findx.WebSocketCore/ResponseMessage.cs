#nullable enable
using System;
using System.IO;

namespace Findx.WebSocketCore;

/// <summary>
///     响应消息
/// </summary>
public class ResponseMessage
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="stream"></param>
    /// <param name="responseMessageType"></param>
    public ResponseMessage(ReadOnlyMemory<byte> bytes, MemoryStream? stream, ResponseMessageType responseMessageType)
    {
        Memory = bytes;
        Stream = stream;
        ResponseMessageType = responseMessageType;
    }

    /// <summary>
    ///     只读二进制数据
    /// </summary>
    public ReadOnlyMemory<byte> Memory { get; set; }

    /// <summary>
    ///     数据流
    /// </summary>
    public MemoryStream? Stream { get; set; }
    
    /// <summary>
    ///     响应消息类型
    /// </summary>
    public ResponseMessageType ResponseMessageType { get; }
    
    /// <summary>
    ///     创建二进制响应消息
    /// </summary>
    public static ResponseMessage BinaryMessage(ReadOnlyMemory<byte> data)
    {
        return new ResponseMessage(data, null, ResponseMessageType.Memory);
    }
    
    /// <summary>
    ///     创建内存流响应消息
    /// </summary>
    public static ResponseMessage BinaryStreamMessage(MemoryStream? memoryStream)
    {
        return new ResponseMessage(null, memoryStream, ResponseMessageType.Stream);
    }
}