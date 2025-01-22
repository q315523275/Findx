using System;
using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Serialization;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
/// 
/// </summary>
[Route("api/json")]
[Description("Json服务"), Tags("Json服务")]
public class JsonController: ApiControllerBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="jsonSerializer"></param>
    public JsonController(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    ///     序列化
    /// </summary>
    /// <returns></returns>
    [HttpGet("serialize")]
    public string Serialize()
    {
        return _jsonSerializer.Serialize(new TestNewsInfo { Content = "测试使用", Author = "Findx", Id = 1, Status = 1, Title = "test", CreatedTime = DateTime.Now });
    }
    
    /// <summary>
    ///     反序列化-泛型
    /// </summary>
    /// <returns></returns>
    [HttpPost("deserialize")]
    public CommonResult Deserialize(string json)
    {
        return CommonResult.Success(_jsonSerializer.Deserialize<TestNewsInfo>(json));
    }
    
    /// <summary>
    ///     反序列化-类型
    /// </summary>
    /// <returns></returns>
    [HttpPost("deserializeType")]
    public CommonResult DeserializeType(string json)
    {
        return CommonResult.Success(_jsonSerializer.Deserialize(json, typeof(TestNewsInfo)));
    }
}